using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using RotativaCore.Options;
using RotativaCore;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.WindowsAzure.Storage;
using RotativaCore.SampleWebApplication.Models;

namespace RotativaCore.SampleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string name = "World")
        {
            ViewBag.Message = $"Hello {name}, Welcome to ASP.NET Core!";

            return View();
        }


        public ActionResult TestInlie()
        {
            return new ActionAsPdf("Index", new { name = "Friends" })
            {
                //FileName = "Test.pdf",
                ContentDisposition = ContentDisposition.Inline,
                OnBuildFileSuccess = async (bytes, context, fileName) =>
                {
                    // some code done.
                    return true;

                    // example.
                    if (string.IsNullOrEmpty(fileName))
                        fileName = $"{Guid.NewGuid()}.pdf";

                    var container = CloudStorageAccount
                        .Parse(connectionString:null) // Please set your value.If it's null, it will result in an ArgumentNullException().
                        .CreateCloudBlobClient()
                        .GetContainerReference(containerName:null); // Please set your value.If it's null, it will result in an ArgumentNullException().

                    try
                    {
                        var blockBlob = container.GetBlockBlobReference(fileName);
                        blockBlob.Properties.ContentType = "application/pdf";
                        await blockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                    }
                    catch (Exception e)
                    {
                        // logging.
                        return false;  // fire InvalidOperationException()
                    }

                    return true;
                },
            };
        }

        public ActionResult TestAttachmentWithA4LandscapeDisableSmartShrinkingViewPortSize1024()
        {
            return new ActionAsPdf("Index", new { name = "Friends" })
            {
                FileName = "TestWithA4LandscapeDisableSmartShrinkingViewPortSize1024.pdf",
                ContentDisposition = ContentDisposition.Attachment,
                PageSize = Size.A4,
                PageOrientation = Orientation.Landscape,
                DisableSmartShrinking = true,
                ViewportSize = 1024,
            };
        }

        public ActionResult TestImage()
        {
            return new ActionAsImage("Index", new { name = "Friends" })
            {
                FileName = "TestImage.jpg"
            };
        }

        public ActionResult TestImagePng()
        {
            return new ActionAsImage("Index", new { name = "Friends" })
            {
                FileName = "TestImagePng.png",
                Format = ImageFormat.png
            };
        }

        public ActionResult TestUrl()
        {
            // Now I realize that this isn't very expressive example of why this can be useful.
            // However imagine that you have your own UrlHelper extensions like UrlHelper.User(...)
            // where you create correct URL according to passed conditions, prepare some complex model, etc.

            var urlHelper = new UrlHelper(ControllerContext);
            var url = urlHelper.Action("Index", new { name = "Great Friends" });

            return new UrlAsPdf(url)
            {
                FileName = "TestUrl.pdf"
            };
        }

        public ActionResult TestExternalUrl()
        {
            // In some cases you might want to pull completely different URL that is not related to your application.
            // You can do that by specifying full URL.

            return new UrlAsPdf("http://www.github.com")
            {
                FileName = "TestExternalUrl.pdf",
                PageMargins = new Margins(0, 0, 0, 0)
            };
        }

        public ActionResult TestView()
        {
            // The more usual way of using this would be to have a Model object that you would pass into ViewAsPdf
            // and work with that Model inside your View.
            // Good example could be an Order Summary page on some fictional E-shop.

            // Probably the biggest advantage of this approach is that you have Session object available.

            ViewBag.Message = "Hello Super Great Friends to ASP.NET Core!";
            return new ViewAsPdf("Index")
            {
                FileName = "TestView.pdf",
                PageSize = Size.A3,
                PageOrientation = Orientation.Landscape,
                PageMargins = { Left = 0, Right = 0 },
                ContentDisposition =ContentDisposition.Inline
            };
        }

        public ActionResult TestViewImage()
        {
            // The more usual way of using this would be to have a Model object that you would pass into ViewAsImage
            // and work with that Model inside your View.
            // Good example could be an Order Summary page on some fictional E-shop.

            // Probably the biggest advantage of this approach is that you have Session object available.

            ViewBag.Message = "Hello Super Great Friends to ASP.NET Core!";
            return new ViewAsImage("Index")
            {
                FileName = "TestViewImage.png",
            };
        }

        public ActionResult TestViewWithModel(string id)
        {
            var model = new TestViewModel
            {
                DocTitle = id, DocContent = "This is a test"
            };

            return new ViewAsPdf("TestViewWithModel",model)
            {
                FileName = "TestViewWithModel.pdf"
            };
        }

        public ActionResult TestImageViewWithModel(string id)
        {
            var model = new TestViewModel
            {
                DocTitle = id, DocContent = "This is a test"
            };

            return new ViewAsImage("TestViewWithModel", model)
            {
                FileName = "TestImageViewWithModel.pdf"
            };
        }

        public ActionResult TestPartialViewWithModel(string id)
        {
            var model = new TestViewModel
            {
                DocTitle = id, DocContent = "This is a test with a partial view"
            };

            return new PartialViewAsPdf("TestPartialViewWithModel", model)
            {
                FileName = "TestPartialViewWithModel.pdf"
            };
        }

        public ActionResult TestImagePartialViewWithModel(string id)
        {
            var model = new TestViewModel
            {
                DocTitle = id, DocContent = "This is a test with a partial view"
            };

            return new PartialViewAsImage("TestPartialViewWithModel", model);
        }

        public ActionResult ErrorTest()
        {
            return new ActionAsPdf("SomethingBad")
            {
                FileName = "ErrorTest.pdf"
            };
        }

        public ActionResult SomethingBad()
        {
            return Redirect("http://thisdoesntexists");
        }

        public ActionResult RouteTest()
        {
            return new RouteAsPdf("TestRoute", new {name = "Friends"})
            {
                FileName = "RouteTest.pdf"
            };
        }
        
        public ActionResult BinaryTest()
        {
            var pdfResult = new ActionAsPdf("Index", new { name = "Friends" })
            {
                FileName = "BinaryTest.pdf"
            };

            var binary = pdfResult.BuildPdf(this.ControllerContext);

            return File(binary, "application/pdf");
        }
    }
}
