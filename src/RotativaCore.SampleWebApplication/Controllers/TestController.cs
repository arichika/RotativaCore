using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using RotativaCore.SampleWebApplication.Models;

namespace RotativaCore.SampleWebApplication.Controllers
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
