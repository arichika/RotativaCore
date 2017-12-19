using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;

namespace RotativaCore
{
    public class PartialViewAsPdf : ViewAsPdf
    {
        public PartialViewAsPdf()
        {
        }

        public PartialViewAsPdf(string partialViewName)
            : base(partialViewName)
        {
        }

        public PartialViewAsPdf(object model)
            : base(model)
        {
        }

        public PartialViewAsPdf(string partialViewName, object model)
            : base(partialViewName, model)
        {
        }
        
        protected override ViewEngineResult GetView(ActionContext context, string viewName, string masterName)
        {
            var compositeViewEngine = context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
            return compositeViewEngine.FindView(context, viewName, false);
        }
    }
}