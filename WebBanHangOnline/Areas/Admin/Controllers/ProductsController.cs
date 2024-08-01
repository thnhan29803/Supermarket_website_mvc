using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly IUnitOfWork _unitOfWork;

        //Khởi tạo biến _productStateContext
        private readonly ProductStateContext _productStateContext;
        public ProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductsController()
        {
            // Khởi tạo các chiến lược cụ thể 
            _productStateContext = new ProductStateContext();
        }
        
           
          

            // GET: Admin/Products
            public ActionResult Index(int? page)
            {
                IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
                var pageSize = 10;
                if (page == null)
                {
                    page = 1;
                }
                var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                items = items.ToPagedList(pageIndex, pageSize);
                ViewBag.PageSize = pageSize;
                ViewBag.Page = page;
                return View(items);
            }

            public ActionResult Add()
            {
                ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
            {
                if (ModelState.IsValid)
                {
                    if (Images != null && Images.Count > 0)
                    {
                        for (int i = 0; i < Images.Count; i++)
                        {
                            if (i + 1 == rDefault[0])
                            {
                                model.Image = Images[i];
                                model.ProductImage.Add(new ProductImage
                                {
                                    ProductId = model.Id,
                                    Image = Images[i],
                                    IsDefault = true
                                });
                            }
                            else
                            {
                                model.ProductImage.Add(new ProductImage
                                {
                                    ProductId = model.Id,
                                    Image = Images[i],
                                    IsDefault = false
                                });
                            }
                        }
                    }
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    if (string.IsNullOrEmpty(model.SeoTitle))
                    {
                        model.SeoTitle = model.Title;
                    }
                    if (string.IsNullOrEmpty(model.Alias))
                        model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                    db.Products.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                return View(model);
            }


            public ActionResult Edit(int id)
            {
                ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
                var item = db.Products.Find(id);
                return View(item);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit(Product model)
            {
                if (ModelState.IsValid)
                {
                    model.ModifiedDate = DateTime.Now;
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                    db.Products.Attach(model);
                    db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(model);
            }

            [HttpPost]
            public ActionResult Delete(int id)
            {
                var item = db.Products.Find(id);
                if (item != null)
                {
                    var checkImg = item.ProductImage.Where(x => x.ProductId == item.Id);
                    if (checkImg != null)
                    {
                        foreach (var img in checkImg)
                        {
                            db.ProductImages.Remove(img);
                            db.SaveChanges();
                        }
                    }
                    db.Products.Remove(item);
                    db.SaveChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false });
            }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    var productsToDelete = new List<Product>();

                    foreach (var item in items)
                    {
                        var productId = Convert.ToInt32(item);
                        var product = _unitOfWork.Products.GetById(productId); // Lấy sản phẩm từ repository

                        if (product != null)
                        {
                            productsToDelete.Add(product);
                        }
                    }

                    _unitOfWork.Products.DeleteRange(productsToDelete); // Xóa hàng loạt sản phẩm

                    _unitOfWork.Save(); // Lưu thay đổi vào cơ sở dữ liệu

                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        //Repository Pattern là lớp trung gian giữa tầng Business Logic và Data Access thuộc nhóm structual pattern
        /*Công dụng : Giúp cho việc truy cập dữ liệu trở nên dễ dàng hơn
          cung cấp 1 cách tiếp cận tốt hơn cho việc thao tác với dữ liệu
         */

        //Lớp ProductRepository là nơi triển khai các phương thức để thực hiện các thao tác liên quan đến sản phẩm trong cơ sở dữ liệu.
        public class ProductRepository : IProductRepository
        {
            private readonly ApplicationDbContext _context;

            //Các phương thức trong ProductRepository thực hiện các thao tác như xóa một sản phẩm,
            //xóa một tập hợp sản phẩm và lấy một sản phẩm theo ID.
            public ProductRepository(ApplicationDbContext context)
            {
                _context = context;
            }

            public void Delete(Product product)
            {
                _context.Products.Remove(product);
            }

            // Thêm một phương thức để xóa hàng loạt sản phẩm
            public void DeleteRange(IEnumerable<Product> products)
            {
                _context.Products.RemoveRange(products);
            }
            public Product GetById(int productId)
            {
                return _context.Products.Find(productId); // Triển khai phương thức GetById lấy Id sản phẩm
            }
        }


        //Interface ProductRepository
        public interface IProductRepository
        {
            void Delete(Product product);
            void DeleteRange(IEnumerable<Product> products);
            Product GetById(int productId);
        }


        //Kết hợp Repository pattern với UnitOfWork pattern để lưu lại trên database

        /*
         Unit Of Work được sử dụng để đảm bảo nhiều hành động như insert, update, delete...
         được thực thi trong cùng một transaction thống nhất để đảm bảo tính toàn vẹn dữ liệu.
         Đây là một đối tượng quản lý một hoặc nhiều repository và cung cấp phương thức Save để lưu các thay đổi vào cơ sở dữ liệu
         
         UOW pattern thuộc loại structual 
         */

        public class UnitOfWork : IUnitOfWork
        {
            private readonly ApplicationDbContext _context;

            public UnitOfWork(ApplicationDbContext context)
            {
                _context = context;
                Products = new ProductRepository(_context);
            }

            public IProductRepository Products { get; private set; }

            public void Save()
            {
                _context.SaveChanges();
            }
        }


        //interface IUnitOfWork
        public interface IUnitOfWork
        {
            IProductRepository Products { get; }
            void Save();
        }


        //Áp dung mãu State Design Pattern cho trạng thái sản phẩm
        //Để sử dụng mẫu này khai báo 2 đối số id dùng cho id product còn stateName dùng để xem nó là IsActive, IsHome, IsSale
        [HttpPost]
        public ActionResult ChangeProductStatus(int id, string stateName)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                // Sử dụng ProductStateContext để áp dụng trạng thái cụ thể cho sản phẩm
                switch (stateName)
                {
                    case "IsActive":
                        _productStateContext.SetState(new ActiveState());
                        break;
                    case "IsHome":
                        _productStateContext.SetState(new HomeState());
                        break;
                    case "IsSale":
                        _productStateContext.SetState(new SaleState());
                        break;
                    default:
                        return Json(new { success = false });
                }

                _productStateContext.ChangeStatus(item); // Áp dụng trạng thái cụ thể cho sản phẩm

                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();

                // Trả về JSON thông báo thành công và trạng thái mới của thuộc tính
                return Json(new { success = true, IsActive = item.IsActive, IsHome = item.IsHome, IsSale = item.IsSale });
            }

            return Json(new { success = false });
        }

    }
    }