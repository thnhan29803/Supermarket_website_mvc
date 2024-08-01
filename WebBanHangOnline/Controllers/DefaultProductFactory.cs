using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class DefaultProductFactory:ProductFactory
    {
        private readonly ApplicationDbContext _context;

        public DefaultProductFactory(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /*
         Lớp DefaultProductFactory: Một factory cụ thể, mở rộng từ ProductFactory. Lớp này cung cấp một phương thức
         GetProducts để trả về danh sách tất cả các sản phẩm có số lượng lớn hơn 0 mà không cần quan tâm đến trạng thái giảm giá.
         */
        public List<Product> GetProducts()
        {
            return _context.Products.Where(p => p.Quantity > 0).ToList();
        }

    }
}
