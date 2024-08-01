using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using PagedList;
using System.Globalization;
using System.Data.Entity;
using WebBanHangOnline.Models.ViewModels;
using SelectPdf;
using System.IO;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]

    public class OrderController : BaseController
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(int? page)
        {
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList();

            if (page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;
            var pageSize = 10;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            return View(items.ToPagedList(pageNumber, pageSize));
        }



        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Attach(item);
                item.TypePayment = trangthai;
                db.Entry(item).Property(x => x.TypePayment).IsModified = true;
                db.SaveChanges();
                return Json(new { message = "Success", Success = true });
            }
            return Json(new { message = "Unsuccess", Success = false });
        }

        public void ThongKe(string fromDate, string toDate)
        {
            var query = from o in db.Orders
                        join od in db.OrderDetails on o.Id equals od.OrderId
                        join p in db.Products
                        on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.Price
                        };
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.CreatedDate >= start);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.CreatedDate < endDate);
            }
            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(r => new
            {
                Date = r.Key.Value,
                TotalBuy = r.Sum(x => x.OriginalPrice * x.Quantity), // tổng giá bán
                TotalSell = r.Sum(x => x.Price * x.Quantity) // tổng giá mua
            }).Select(x => new RevenueStatisticViewModel
            {
                Date = x.Date,
                Benefit = x.TotalSell - x.TotalBuy,
                Revenues = x.TotalSell
            });
        }
        public ActionResult ExportPdt()
        {
            // instantiate a html to pdf converter object
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList();
            items.ToPagedList(1, 1000000);

            //Khoi tao ICommand pattern
            ICommand convertToPdfCommand = new ConvertHtmlToPdfCommand(items, ControllerContext);
            convertToPdfCommand.Execute();

            return Json(1, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListFile()
        {
            string[] filepaths = Directory.GetFiles(Server.MapPath("~/Resource/Pdf"));
            string message = TempData["FileHelperMessage"]?.ToString() ?? string.Empty;
            List<FileModel> files = new List<FileModel>();
            foreach (string filepath in filepaths)
            {
                files.Add(new FileModel { FileName = Path.GetFileName(filepath) });
            }
            return View(files);
        }
        public FileResult DownloadFile(string fileName)
        {
            string path = Server.MapPath("~/Resource/Pdf/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public ActionResult DeleteFile(string fileName)
        {
            ICommand deleteFileCommand = new DeleteFileCommand(fileName);
            deleteFileCommand.Execute();

            string fileHelper = (deleteFileCommand as DeleteFileCommand)?.GetResultMessage() ?? "Invalid operation";

            TempData["FileHelperMessage"] = fileHelper;
            return RedirectToAction("ListFile");
        }
        public ActionResult FilteredOrders(int? day, int? month, int? year)
        {
            var query = db.Orders.AsQueryable();

            if (day.HasValue && month.HasValue && year.HasValue)
            {
                query = query.Where(x => x.CreatedDate.Day == day && x.CreatedDate.Month == month && x.CreatedDate.Year == year);
            }
            else if (month.HasValue && year.HasValue)
            {
                query = query.Where(x => x.CreatedDate.Month == month && x.CreatedDate.Year == year);
            }
            else if (year.HasValue)
            {
                query = query.Where(x => x.CreatedDate.Year == year);
            }

            var items = query.OrderByDescending(x => x.CreatedDate).ToList();
            var totalProfit = CalculateTotalProfit(items);
            decimal totalRevenue = items.Sum(x => x.TotalAmount);

            ViewBag.TotalRevenue = totalRevenue.ToString("C");
            ViewBag.TotalProfit = totalProfit.ToString("C");
            ViewBag.Items = items;

            return View(ViewBag.Items);
        }

        private decimal CalculateTotalProfit(List<Order> orders)
        {
            var totalProfit = 0m;
            var combinedData = orders
         .Join(
             db.OrderDetails,
             order => order.Id,
             orderDetail => orderDetail.OrderId,
             (order, orderDetail) => new { Order = order, OrderDetail = orderDetail }
         )
         .Join(
             db.Products,
             combined => combined.OrderDetail.ProductId,
             product => product.Id,
             (combined, product) => new { Combined = combined, Product = product }
         )
         .ToList();

            foreach (var data in combinedData)
            {
                decimal cost = data.Product.OriginalPrice;
                decimal revenue = data.Combined.Order.TotalAmount;
                decimal profit = revenue - cost;
                totalProfit += profit;
            }

            return totalProfit;
        }





    }
}