using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace RotativaCore.SampleWebApp._3._0.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index(string name)
        {
            ViewBag.Message = string.Format("Hello {0} to the test route!", name);
            return View();
        }

    }
}
