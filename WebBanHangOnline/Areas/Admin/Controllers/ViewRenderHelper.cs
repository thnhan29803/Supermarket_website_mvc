using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public static class ViewRenderHelper
    {
        public static string RenderPartialToString(ControllerContext controllerContext, string partialViewName, object model)
        {
            if (string.IsNullOrEmpty(partialViewName))
                partialViewName = controllerContext.RouteData.GetRequiredString("action");
            ViewDataDictionary ViewData = new ViewDataDictionary(model);
            TempDataDictionary TempData = new TempDataDictionary();
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controllerContext, partialViewName);
                ViewContext viewContext = new ViewContext(controllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }

}