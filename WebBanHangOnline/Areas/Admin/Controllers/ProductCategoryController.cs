using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class ProductCategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductCategory
        public sealed class ProductCategorySingleton
        {
            private static readonly ApplicationDbContext context = new ApplicationDbContext();
            private static ProductCategorySingleton instance = null;
            private static readonly object padlock = new object();

            ProductCategorySingleton() { }

            public static ProductCategorySingleton Instance
            {
                get
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new ProductCategorySingleton();
                        }
                        return instance;
                    }
                }
            }


            public IEnumerable<ProductCategory> GetAllCategories()
            {
                return context.ProductCategories.ToList();
            }

            // Thêm các phương thức khác nếu cần
        }

        public ActionResult Index()
        {
            var singleton = ProductCategorySingleton.Instance;
            var categories = singleton.GetAllCategories();
            return View(categories);
        }

        /*
         Observer pattern là một mẫu thiết kế phần mềm mà một đối tượng, gọi là subject, duy trì một danh sách các thành phần phụ thuộc nó, 
         gọi là observer, và thông báo tới chúng một cách tự động về bất cứ thay đổi nào, thường thì bằng cách gọi 1 hàm của chúng.
         Nhóm : Behavior    
         */

        //Lớp ProductObserver: Triển khai interface IProductObserver.
        //Trong phương thức ProductAdded, nó thực hiện các
        //hành động cụ thể mà observer muốn thực hiện khi có sản phẩm mới được thêm vào
        

        public interface IProductObserver
        {
            void ProductAdded(ProductCategory product);
        }

        //Lớp này giữ danh sách các Observer, cũng như phương thức thêm hay hủy thông báo cho các Observer về các sự kiện
        //Trong trường hợp này là khi 1 sản phẩm mới được thêm vào
        public class ProductCategoryRepository2
        {
            private readonly List<IProductObserver> _observers = new List<IProductObserver>();
            private readonly ApplicationDbContext _context = new ApplicationDbContext();

            public void Attach(IProductObserver observer)
            {
                _observers.Add(observer);
            }

            public void Detach(IProductObserver observer)
            {
                _observers.Remove(observer);
            }

            public void Notify(ProductCategory product)
            {
                foreach (var observer in _observers)
                {
                    observer.ProductAdded(product);
                }
            }


            public void AddProduct(ProductCategory model)
            {
                if (model != null)
                {
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);

                    _context.ProductCategories.Add(model);
                    _context.SaveChanges();
                }
            }

            // Method to send notification after product addition
            public void NotifyObserversAboutAddedProduct(ProductCategory model)
            {
                Notify(model);
            }
        }

        public class ProductObserver : IProductObserver
        {
            private readonly ModelStateDictionary _modelState;

            //contructer có đối số là một thông báo
            public ProductObserver(ModelStateDictionary modelState)
            {
                _modelState = modelState;
            }


            public void ProductAdded(ProductCategory product)
            {
                // Thực hiện các hành động khi có sản phẩm mới được thêm vào
                // Ví dụ: Gửi thông báo, cập nhật giao diện, logging, v.v.
                //Thêm thông báo vào ModelState
                _modelState.AddModelError(string.Empty, $"Sản phẩm mới đã được thêm vào: {product.Title}");
            }
        }

  
        private readonly ProductCategoryRepository2 _productCategoryRepository = new ProductCategoryRepository2();
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                //Nếu điều kiện hợp lý khai náo productObserver khởi tạo từ ProductObserver với biến ModelState
                var productObserver = new ProductObserver(ModelState);
                _productCategoryRepository.AddProduct(model);
                //phương thức NotifyObserversAboutAddedProduct() để thông báo cho tất cả các observers về sự kiện đã thêm một danh mục sản phẩm mới.
                //Cuối cùng, nó chuyển hướng người dùng đến action "Index" để hiển thị danh sách các danh mục sản phẩm sau khi đã thêm mới thành công.
                _productCategoryRepository.NotifyObserversAboutAddedProduct(model);

                //Thêm thành công thì dùng
                ModelState.AddModelError(string.Empty, "Thêm sản phẩm thành công");
                return Json(new { success = true }); // hien thi ra alert
            }
            return View(model); // chuyen qua view su dung ajax de xu ly thon tin
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
                        var obj = db.ProductCategories.Find(Convert.ToInt32(item));// chuyển đổi chuổi thành số
                        db.ProductCategories.Remove(obj); // xóa hàng loạt
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}