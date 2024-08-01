using SelectPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    //chuyển đổi một HTML thành tệp PDF và lưu trữ nó trên máy chủ.
    public class ConvertHtmlToPdfCommand : ICommand
    {
        private readonly List<Order> _items;
        //Môi trường controller context cần thiết để render trang HTML từ một view (partial view).
        private readonly ControllerContext _controllerContext;

        public ConvertHtmlToPdfCommand(List<Order> items, ControllerContext controllerContext)
        {
            _items = items;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            HtmlToPdf converter = new HtmlToPdf(); //Khởi tạo một đối tượng HtmlToPdf từ thư viện
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.MarginLeft = 10;
            converter.Options.MarginRight = 10;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;

            //render trang HTML từ một partial view (PartialViewPdfResult.cshtml) thành một chuỗi ký tự.
            var htmlPdf = ViewRenderHelper.RenderPartialToString(_controllerContext, "~/Areas/Admin/Views/Order/PartialViewPdfResult.cshtml", _items);


            //Tiến hành chuyển đổi chuỗi HTML thành tệp PDF bằng cách sử dụng phương thức 
            PdfDocument doc = converter.ConvertHtmlString(htmlPdf);
            string fileName = string.Format("{0}.pdf", DateTime.Now.Ticks);
            //Xác định đường dẫn đầy đủ cho tệp PDF trên máy chủ.
            string pathFile = string.Format("{0}/{1}", _controllerContext.HttpContext.Server.MapPath("~/Resource/Pdf"), fileName);

            doc.Save(pathFile);
            doc.Close();
        }

    }

}