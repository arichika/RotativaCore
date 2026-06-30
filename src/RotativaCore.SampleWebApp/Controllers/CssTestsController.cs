using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace RotativaCore.SampleWebApp.Controllers
{
    public class CssTestsController : Controller
    {
        //
        // GET: /CssTests/

        public ActionResult Index()
        {
            return new ViewAsPdf("Index");
        }

        public ActionResult IndexImage()
        {
            return new ViewAsImage("Index");
        }
    }
}
