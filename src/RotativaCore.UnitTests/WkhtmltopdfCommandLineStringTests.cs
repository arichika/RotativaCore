using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using RotativaCore.Options;
using SharpTestsEx;

namespace RotativaCore.UnitTests
{
    public class TestAsPdfResult: AsPdfResultBase
    {
        public string GetConvertOptionsValue()
        {
            return base.GetConvertOptions();
        }
        protected override string GetUrl(ActionContext context)
        {
            return string.Empty;
        }
    }

    public class WkhtmltopdfCommandLineStringTests
    {
        [Fact]
        public void WhenAPdfActioResultHasSetOptionsAsProperties_TheResultingCommandLineHasTheCorrectOptions()
        {
            var pdfResult = new TestAsPdfResult();
            var post = new Dictionary<string, string>
            {
                { "param1", "value1" },
                { "param2", "value2" }
            };

            pdfResult.Post = post;
            pdfResult.PageOrientation = Orientation.Landscape;

            var commandlineOptions = pdfResult.GetConvertOptionsValue();
            commandlineOptions.Should().Contain("--post param1 value1 --post param2 value2");
            commandlineOptions.Should().Contain("-O Landscape");
        }
    }
}
