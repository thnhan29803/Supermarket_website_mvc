using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class DeleteFileCommand : ICommand
    {
        private readonly string _fileName; //Lưu trữ tên tập tin cần xóa.
        private string _resultMessage; // Lưu trữ thông điệp kết quả sau khi thực hiện xóa tập tin.

        public DeleteFileCommand(string fileName)
        {
            _fileName = fileName;
        }

        public void Execute()
        {
            try
            {
                //Lấy đường dẫn đầy đủ của tập tin cần xóa 
                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Resource/Pdf/") + _fileName;

                if (System.IO.File.Exists(path))
                {
                    //Nếu tập tin tồn tại, thực hiện xóa tập tin bằng
                    System.IO.File.Delete(path);
                    _resultMessage = "File deleted successfully";
                }
                else
                {
                    _resultMessage = "File not found";
                }
            }
            catch (Exception ex)
            {
                _resultMessage = "Error deleting file: " + ex.Message;
                // Có thể ghi log chi tiết ngoại lệ để gỡ lỗi tại đây
            }

        }

        //Cung cấp phương thức GetResultMessage() để lấy thông điệp kết quả
        public string GetResultMessage()
        {
            return _resultMessage;
        }
    }

}