using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace RotativaCore.SampleWebApp._3._0.Controllers
{
    public class AjaxTestsController : Controller
    {
        //
        // GET: /AjaxTests/
        public ActionResult Index()
        {
            return new ViewAsPdf("Index")
            {
                //CustomSwitches = "--enable-javascript --window-status jsdone"
            };
        }

        public ActionResult IndexImage()
        {
            return new ViewAsImage("Index")
            {
                //CustomSwitches = "--enable-javascript --window-status jsdone"
            };
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
