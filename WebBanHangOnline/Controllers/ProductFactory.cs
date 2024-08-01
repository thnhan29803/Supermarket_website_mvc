using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ProductFactory
    {
        //Dùng Lấy danh sách sản phẩm sale và sản phẩm hiển thị bình thường

        /*
         Lớp ProductFactory: Đây là một factory chung, có thể được sử dụng để 
         tạo ra các đối tượng Product dựa trên các tiêu chí nhất định, như chỉ 
         lấy các sản phẩm đang giảm giá (IsSale) và có số lượng lớn hơn 0.

         Thuộc nhóm creation
         */

        private ApplicationDbContext db = new ApplicationDbContext();

        public ProductFactory(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public List<Product> GetFilteredProducts(bool isSaleOnly = true)
        {
            IQueryable<Product> query = db.Products;

            if (isSaleOnly)
            {
                query = query.Where(x => x.IsSale && x.IsActive && x.Quantity > 0);
            }
            else
            {
                query = query.Where(x => x.IsActive && x.Quantity > 0);
            }

            return query.ToList();
        }
    }
}