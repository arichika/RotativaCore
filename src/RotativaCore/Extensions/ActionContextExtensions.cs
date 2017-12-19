using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;


namespace RotativaCore.Extensions
{
    public static class ActionContextExtensions
    {
        public static string GetHtmlFromView(this ActionContext  context, ViewEngineResult viewResult, string viewName, object model)
        {
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };

            var tempDataProvider = context.HttpContext.RequestServices.GetRequiredService<ITempDataProvider>();
            var tempData = new TempDataDictionary(context.HttpContext, tempDataProvider);

            using (var sw = new StringWriter())
            {
                // view not found, throw an exception with searched locations
                if (viewResult.View == null)
                {
                    var locations = new StringBuilder();
                    locations.AppendLine();

                    foreach (var location in viewResult.SearchedLocations)
                    {
                        locations.AppendLine(location);
                    }

                    throw new InvalidOperationException(
                        string.Format(
                            "The view '{0}' or its master was not found, searched locations: {1}", viewName, locations));
                }

                var viewContext = new ViewContext(context, viewResult.View, viewData, tempData, sw, new HtmlHelperOptions());
                var task = viewResult.View.RenderAsync(viewContext);
                task.Wait();

                var html = sw.ToString();
                var currentUri = new Uri(context.HttpContext.Request.GetDisplayUrl());
                var authority = currentUri.GetComponents(UriComponents.StrongAuthority, UriFormat.Unescaped);
                var baseUrl = string.Format("{0}://{1}", context.HttpContext.Request.Scheme, authority);
                html = Regex.Replace(html, "<head>", string.Format("<head><base href=\"{0}\" />", baseUrl), RegexOptions.IgnoreCase);
                return html;
            }
        }
    }
}
