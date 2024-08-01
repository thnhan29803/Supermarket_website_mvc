using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: News
        public ActionResult Index(string searchPhone)
        {
            IQueryable<Order> orders = db.Orders.OrderByDescending(x => x.Id);

            if (!string.IsNullOrEmpty(searchPhone))
            {
                orders = orders.Where(x => x.Phone.Contains(searchPhone)); // Lọc theo số điện thoại
            }

            ViewBag.SearchPhone = searchPhone; // Truyền lại số điện thoại vào View

            return View(orders.ToList()); // Trả về View với danh sách đơn hàng không phân trang
        }
        public ActionResult Detail(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }
        public ActionResult Partial_News_Home()
        {
            var items = db.News.Take(3).ToList();
            return PartialView(items);
        }
    }
}