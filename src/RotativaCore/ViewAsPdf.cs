using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using RotativaCore.Extensions;

namespace RotativaCore
{
    public class ViewAsPdf : AsPdfResultBase
    {
        private string _viewName;

        public string ViewName
        {
            get => _viewName ?? string.Empty;
            set => _viewName = value;
        }

        private string _masterName;

        public string MasterName
        {
            get => _masterName ?? string.Empty;
            set => _masterName = value;
        }

        public object Model { get; set; }

        public ViewAsPdf()
        {
            WkhtmlPath = string.Empty;
            MasterName = string.Empty;
            ViewName = string.Empty;
            Model = null;
        }

        public ViewAsPdf(string viewName)
            : this()
        {
            ViewName = viewName;
        }

        public ViewAsPdf(object model)
            : this()
        {
            Model = model;
        }

        public ViewAsPdf(string viewName, object model)
            : this()
        {
            ViewName = viewName;
            Model = model;
        }

        public ViewAsPdf(string viewName, string masterName, object model)
            : this(viewName, model)
        {
            MasterName = masterName;
        }

        protected override string GetUrl(ActionContext  context)
        {
            return string.Empty;
        }

        protected virtual ViewEngineResult GetView(ActionContext  context, string viewName, string masterName)
        {
            var compositeViewEngine = context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
            return compositeViewEngine.FindView(context, viewName, false);
        }

        protected override byte[] CallTheDriver(ActionContext  context)
        {
            // use action name if the view name was not provided
            var viewName = ViewName;
            if (string.IsNullOrEmpty(viewName))
                viewName = context.ActionDescriptor.DisplayName;

            var viewResult = GetView(context, viewName, MasterName);
            var html = context.GetHtmlFromView(viewResult, viewName, Model);
            var fileContent = WkhtmltopdfDriver.ConvertHtml(WkhtmlPath, GetConvertOptions(), html);
            return fileContent;
        }
    }
}