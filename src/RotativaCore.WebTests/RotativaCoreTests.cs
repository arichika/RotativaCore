using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SharpTestsEx;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace RotativaCore.WebTests
{
    [SupportedOSPlatform("windows")]
    public class RotativaCoreTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private readonly HttpClient _httpClient;
        private readonly IWebDriver _selenium;

        public RotativaCoreTests(ITestOutputHelper output)
        {
            _output = output;
            var selenium = CreateChromeDriver();

            try
            {
                selenium.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 10);
                selenium.Navigate().GoToUrl(TestConst.RotativaDemoUrl);

                _selenium = selenium;
                _httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
            }
            catch
            {
                selenium.Dispose();
                throw;
            }
        }

        private IWebDriver CreateChromeDriver()
        {
            var staleDriverPath = Path.Combine(AppContext.BaseDirectory, "chromedriver.exe");
            if (File.Exists(staleDriverPath))
            {
                File.Delete(staleDriverPath);
                _output.WriteLine($"Removed stale ChromeDriver: {staleDriverPath}");
            }

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            return new ChromeDriver(service, new ChromeOptions());
        }

        private byte[] DownloadBytes(Uri uri) =>
            _httpClient.GetByteArrayAsync(uri).GetAwaiter().GetResult();

        private void AssertImageFormat(Uri uri, ImageFormat expectedFormat)
        {
            using var stream = new MemoryStream(DownloadBytes(uri));
            using var image = Image.FromStream(stream);

            image.Should().Not.Be.Null();
            image.RawFormat.Should().Be.EqualTo(expectedFormat);
        }

        [Fact]
        public void Is_the_site_reachable()
        {
            Assert.StartsWith("Home Page", _selenium.Title);
        }
        
        [Fact]
        public void Can_print_the_test_pdf()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test PDF Inline"));
            var pdfHref = testLink.GetAttribute("href");
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains("RotativaCore.SampleWebApp").Should().Be.True();
        }

        [Fact]
        public void Can_print_the_test_image()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test Image"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }

        [Fact]
        public void Can_print_the_test_image_png()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test Image Png"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Png);
        }

        [Fact]
        public void Can_print_the_pdf_from_a_view()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View"));
            var pdfHref = testLink.GetAttribute("href");
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains("Some text with non ascii char").Should().Be.True();
        }

        [Fact]
        public void Can_print_the_image_from_a_view()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View Image"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }
        
        [Fact]
        public void Can_print_the_pdf_from_a_view_with_non_ascii_chars()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View"));
            var pdfHref = testLink.GetAttribute("href");
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains("àéù").Should().Be.True();
        }

        [Fact]
        public void Can_print_the_pdf_from_a_view_with_jp_chars()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View"));
            var pdfHref = testLink.GetAttribute("href");
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains("日本語").Should().Be.True();
        }

        [Fact]
        public void Can_print_the_image_from_a_view_with_non_ascii_chars()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test View Image"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }

        [Fact]
        public void Can_print_the_pdf_from_a_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test ViewAsPdf with a model"));
            var pdfHref = testLink.GetAttribute("href");
            const string title = "This is a test";
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains(title).Should().Be.True();
        }

        [Fact]
        public void Can_print_the_image_from_a_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test ViewAsImage with a model"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }

        [Fact]
        public void Can_print_the_pdf_from_a_partial_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test PartialViewAsPdf with a model"));
            var pdfHref = testLink.GetAttribute("href");
            const string content = "This is a test with a partial view";
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains(content).Should().Be.True();
        }

        [Fact]
        public void Can_print_the_image_from_a_partial_view_with_a_model()
        {
            var testLink = _selenium.FindElement(By.LinkText("Test PartialViewAsImage with a model"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }

        [Fact]
        public void Can_print_pdf_from_page_with_content_from_ajax_request()
        {
            var testLink = _selenium.FindElement(By.LinkText("Ajax Test"));
            var pdfHref = testLink.GetAttribute("href");
            const string content = "Hi there, this is content from a Ajax call.";

            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains(content).Should().Be.True();
        }

        [Fact]
        public void Can_print_image_from_page_with_content_from_ajax_request()
        {
            var testLink = _selenium.FindElement(By.LinkText("Ajax Image Test"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }

        [Fact]
        public void Can_print_pdf_from_page_with_external_css_file()
        {
            var testLink = _selenium.FindElement(By.LinkText("External CSS Test"));
            var pdfHref = testLink.GetAttribute("href");
            const string content = "Hi guys, this content shows up thanks to css file.";
            var pdfResult = DownloadBytes(new Uri(pdfHref));
            var pdfTester = new PdfTester(_output);
            pdfTester.LoadPdf(pdfResult);
            pdfTester.PdfIsValid.Should().Be.True();
            pdfTester.PdfContains(content).Should().Be.True();
        }

        [Fact]
        public void Can_print_image_from_page_with_external_css_file()
        {
            var testLink = _selenium.FindElement(By.LinkText("External CSS Test Image"));
            var pdfHref = testLink.GetAttribute("href");
            AssertImageFormat(new Uri(pdfHref), ImageFormat.Jpeg);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _selenium?.Dispose();
        }
    }
}
