using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
   public interface ICommand
    {
        void Execute();
    }
}
