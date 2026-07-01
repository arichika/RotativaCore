using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace RotativaCore.SampleWebApp.Controllers
{
    public class AjaxTestsController : Controller
    {
        //
        // GET: /AjaxTests/
        public ActionResult Index()
        {
            return new UrlAsPdf(Url.Action("Index2", "AjaxTests"));
        }

        public ActionResult IndexImage()
        {
            return new UrlAsImage(Url.Action("Index2", "AjaxTests"));
        }

        public ActionResult Index2()
        {
            return View("Index");
        }

        public ActionResult AjaxContent()
        {
            Thread.Sleep(100);
            return PartialView();
        }

    }
}
