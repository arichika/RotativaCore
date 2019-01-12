using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RotativaCore;
using RotativaCore.Options;

namespace RotativaCore.SampleWebApplication.Controllers
{
    public class CssTestsController : Controller
    {
        public ActionResult InSiteCSSIndex()
        {
            return View("InSiteCSSIndex");
        }

        public ActionResult InSiteCSSIndexAsPdf()
        {
            return new ActionAsPdf("InSiteCSSIndex")
            {
                FileName = "InSiteCSSIndexAsPdf.pdf"
            };
        }

        public ActionResult InSiteCSSIndexAsImage()
        {
            return new ActionAsImage("InSiteCSSIndex")
            {
                FileName = "InSiteCSSIndexAsImage.png",
                Format = ImageFormat.png
            };
        }

        public ActionResult ExtSiteCSSIndex()
        {
            return View("ExtSiteCSSIndex");
        }

        public ActionResult ExtSiteCSSIndexAsPdf()
        {
            return new ActionAsPdf("ExtSiteCSSIndex")
            {
                FileName = "InSiteCSSIndexAsPdf.pdf",
            };
        }


        public ActionResult ExtSiteCSSIndexAsImage()
        {
            return new ActionAsImage("ExtSiteCSSIndex")
            {
                FileName = "InSiteCSSIndexAsImage.png",
                Format = ImageFormat.png
            };
        }

    }
}
