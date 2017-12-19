using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RotativaCore.Options;
using SharpTestsEx;
using Xunit;

namespace RotativaCore.UnitTests
{
    public class WkhtmltoimageCommandLineStringTests
    {
        public class TestAsImageResult : AsImageResultBase
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
                var imageResult = new TestAsImageResult();
                var post = new Dictionary<string, string>
                {
                    {"param1", "value1"},
                    {"param2", "value2"}
                };

                imageResult.Post = post;
                imageResult.Format = ImageFormat.png;

                var commandlineOptions = imageResult.GetConvertOptionsValue();
                commandlineOptions.Should().Contain("--post param1 value1 --post param2 value2");
                commandlineOptions.Should().Contain("-f png");
            }
        }
    }
}
