using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    //render một phần tử HTML thành chuỗi ký tự từ lớp này mới
    public class RenderPartialToStringCommand : ICommand
    {
        private string _viewName; //Tên của view (partial view) cần được render.
        private object _model;
        private ControllerContext _controllerContext; // Môi trường controller context cần thiết để render view.
        public string RenderedHtml { get; private set; } // Chuỗi kết quả sau khi view được render thành HTML.

        public RenderPartialToStringCommand(string viewName, object model, ControllerContext controllerContext)
        {
            _viewName = viewName;
            _model = model;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            //Kiểm tra nếu tên view là null hoặc rỗng
            if (string.IsNullOrEmpty(_viewName))
                _viewName = _controllerContext.RouteData.GetRequiredString("action");
            ViewDataDictionary ViewData = new ViewDataDictionary();
            TempDataDictionary TempData = new TempDataDictionary();
            //Thiết lập dữ liệu mô hình cho ViewData.Model.
            ViewData.Model = _model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(_controllerContext, _viewName);
                ViewContext viewContext = new ViewContext(_controllerContext, viewResult.View, ViewData, TempData, sw);
                //render nội dung view vào StringWriter.
                viewResult.View.Render(viewContext, sw);

                //Gán nội dung đã render thành chuỗi vào biến RenderedHtml.
                RenderedHtml = sw.ToString();
            }
        }
    }

}