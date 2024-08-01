using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{

    /*
        Mẫu State Design Pattern là mẫu  chuyên dùng để set trạng thái
        
     
     */
    public interface IProductState
    {
        //Phương thức ChangeStatus được sử dụng để thay đổi trạng thái của một đối tượng Product.
        void ChangeStatus(Product product);
    }

    public class ActiveState : IProductState
    {
        public void ChangeStatus(Product product)
        {
            product.IsActive = !product.IsActive;
        }
    }

    public class HomeState : IProductState
    {
        public void ChangeStatus(Product product)
        {
            product.IsHome = !product.IsHome;
        }
    }

    public class SaleState : IProductState
    {
        public void ChangeStatus(Product product)
        {
            product.IsSale = !product.IsSale;
        }
    }

    //Lớp ProductStateContext dùng để chuyển đổi trạng thái sản phẩm khác nhau
    public class ProductStateContext
    {
        //Biến currentState tham chiếu đến IProductState 
        private IProductState currentState;

        public ProductStateContext()
        {
            // Khởi tạo trạng thái mặc định ở đây (ActiveState, HomeState, hoặc SaleState)
            currentState = new ActiveState();
        }

        //Phương thức SetState dùng để gán trạng thái mình thay đổi cho sản phẩm
        public void SetState(IProductState newState)
        {
            currentState = newState;
        }

        //Phương thức ChangeStatus dùng để thay đổi trạng thái mà mình muốn gán cho nó
        public void ChangeStatus(Product product)
        {
            currentState.ChangeStatus(product);
        }
    }
}
