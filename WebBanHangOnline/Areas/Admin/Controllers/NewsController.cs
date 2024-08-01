using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/News
        public ActionResult Index(string Searchtext, int? page)// 2 đối số dùng dể tìm kiếm, phân trang
        {
            var pageSize = 10;// đánh dấu số trang quy định là 10
            if (page == null)// kiểm tra nếu số trang rổng
            {
                page = 1;// gán 1 cho số trang
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id);// gọi dữ danh sách sắp xếp theo id
            if (!string.IsNullOrEmpty(Searchtext))// kiểm tra nếu chứa đoạn văn bản cần tìm
            {
                items= items.Where(x=>x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext)); // gán kết quả tìm được
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1; // chuyển số trang thành kiểu int
             items = items.ToPagedList(pageIndex, pageSize); // truyền giá trị trang và kích thước trang cho item
            ViewBag.PageSize = pageSize;// gán kích thước trang cho viewbag để sử dụng cho phần view
            ViewBag.Page = page;// gán giá trị trang cho viewbag để sử dụng cho phần view
            return View(items);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(News model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.CategoryId = 2;
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.News.Attach(model);// thêm mới 
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.News.Find(id); // lấy id cần tìm 
            return View(item); // truyền id sang view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News model)
        {
            if (ModelState.IsValid)// kiểm tra validdate
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.News.Attach(model);// thêm cái mới vào cái củ
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            // tìm id đối tượng cần xóa
            var item = db.News.Find(id);
            if (item != null) // kiểm tra nếu không rổng thì xóa
            {
                db.News.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            // tìm đối tượng cần bật hoạt động
            var item = db.News.Find(id);
            if (item != null) // kiểm tra nếu chưa bật thì bật
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids)) // lấy chuổi id cần xóa
            {
                var items = ids.Split(','); // cắt chuổi 
                if (items != null && items.Any())// kiểm tra nếu chuổi tồn tại 
                {
                    foreach (var item in items)
                    {
                        var obj = db.News.Find(Convert.ToInt32(item));// chuyển đổi chuổi thành số
                        db.News.Remove(obj); // xóa hàng loạt
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}