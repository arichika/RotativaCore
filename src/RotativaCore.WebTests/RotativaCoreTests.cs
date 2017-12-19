using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SharpTestsEx;
using Xunit.Abstractions;

namespace RotativaCore.WebTests
{
    public class RotativaCoreTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private readonly IWebDriver _selenium;

        public RotativaCoreTests(ITestOutputHelper output)
        {
            _output = output;
            _selenium = new ChromeDriver(AppContext.BaseDirectory);
            _selenium.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 10);
            _selenium.Navigate().GoToUrl(TestConst.RotativaDemoUrl);
        }

        [Fact]
        public void Is_the_site_reachable()
        {
            Assert.Equal("Home Page", _selenium.Title);
        }
        
        [Fact]
        public void Can_print_the_test_pdf()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains("RotativaCore.SampleWebApplication").Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_the_test_image()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test Image"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }

        [Fact]
        public void Can_print_the_test_image_png()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test Image Png"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Png);
            }
        }

        [Fact]
        public void Can_print_the_pdf_from_a_view()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains("My MVC Application").Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_the_image_from_a_view()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View Image"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }
        
        [Fact]
        public void Can_print_the_pdf_from_a_view_with_non_ascii_chars()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains("àéù").Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_the_pdf_from_a_view_with_jp_chars()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains("日本語").Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_the_image_from_a_view_with_non_ascii_chars()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View Image"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }

        [Fact]
        public void Can_print_the_pdf_from_a_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test ViewAsPdf with a model"));
            var pdfHref = testLink.GetAttribute("href");
            const string title = "This is a test";
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains(title).Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_the_image_from_a_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test ViewAsImage with a model"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }

        [Fact]
        public void Can_print_the_pdf_from_a_partial_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test PartialViewAsPdf with a model"));
            var pdfHref = testLink.GetAttribute("href");
            const string content = "This is a test with a partial view";
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains(content).Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_the_image_from_a_partial_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test PartialViewAsImage with a model"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }

        [Fact]
        public void Can_print_pdf_from_page_with_content_from_ajax_request()
        {
            var testLink = _selenium.FindElement(By.LinkText("Ajax Test"));
            var pdfHref = testLink.GetAttribute("href");
            const string content = "Hi there, this is content from a Ajax call.";
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains(content).Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_image_from_page_with_content_from_ajax_request()
        {
            var testLink = _selenium.FindElement(By.LinkText("Ajax Image Test"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }

        [Fact]
        public void Can_print_pdf_from_page_with_external_css_file()
        {
            var testLink = _selenium.FindElement(By.LinkText("External CSS Test"));
            var pdfHref = testLink.GetAttribute("href");
            const string content = "Hi guys, this content shows up thanks to css file.";
            using (var wc = new WebClient())
            {
                var pdfResult = wc.DownloadData(new Uri(pdfHref));
                var pdfTester = new PdfTester(_output);
                pdfTester.LoadPdf(pdfResult);
                pdfTester.PdfIsValid.Should().Be.True();
                pdfTester.PdfContains(content).Should().Be.True();
            }
        }

        [Fact]
        public void Can_print_image_from_page_with_external_css_file()
        {
            var testLink = _selenium.FindElement(By.LinkText("External CSS Test Image"));
            var pdfHref = testLink.GetAttribute("href");
            using (var wc = new WebClient())
            {
                var imageResult = wc.DownloadData(new Uri(pdfHref));
                var image = Image.FromStream(new MemoryStream(imageResult));
                image.Should().Not.Be.Null();
                image.RawFormat.Should().Be.EqualTo(ImageFormat.Jpeg);
            }
        }

        public void Dispose()
        {
            _selenium?.Dispose();
        }
    }
}
