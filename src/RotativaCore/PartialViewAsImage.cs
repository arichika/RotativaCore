using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;

namespace RotativaCore
{
    public class PartialViewAsImage : ViewAsImage
    {
        public PartialViewAsImage()
        {
        }

        public PartialViewAsImage(string partialViewName)
            : base(partialViewName)
        {
        }

        public PartialViewAsImage(object model)
            : base(model)
        {
        }

        public PartialViewAsImage(string partialViewName, object model)
            : base(partialViewName, model)
        {
        }

        protected override ViewEngineResult GetView(ActionContext  context, string viewName, string masterName)
        {
            var compositeViewEngine = context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
            return compositeViewEngine.FindView(context, viewName, false);
        }
    }
}
