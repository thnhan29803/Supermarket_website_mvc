    using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ProductSaleController : Controller
    {
        // GET: ProductSale
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProductFactory _productFactory;
        public ProductSaleController()
        {
            // Khởi tạo một instance mới của ProductFactory
            _productFactory = new ProductFactory(new ApplicationDbContext());
        }


        public ActionResult Index(bool isSaleOnly = true)
        {
            var items = _productFactory.GetFilteredProducts(isSaleOnly);
            return View("Index", items);
        }
    }
}