using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RotativaCore
{
    public class RouteAsPdf: AsPdfResultBase
    {
        private RouteValueDictionary _routeValuesDict;
        private object _routeValues;
        private string _routeName;

        public RouteAsPdf(string routeName)
        {
            _routeName = routeName;
        }

        public RouteAsPdf(string routeName, RouteValueDictionary routeValues)
            : this(routeName)
        {
            _routeValuesDict = routeValues;
        }

        public RouteAsPdf(string routeName, object routeValues)
            : this(routeName)
        {
            _routeValues = routeValues;
        }

        protected override string GetUrl(ActionContext  context)
        {
            var urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(context);

            string actionUrl;
            if (_routeValues == null)
                actionUrl = urlHelper.RouteUrl(_routeName, _routeValuesDict);
            else if (_routeValues != null)
                actionUrl = urlHelper.RouteUrl(_routeName, _routeValues);
            else
                actionUrl = urlHelper.RouteUrl(_routeName);

            var currentUri = new Uri(context.HttpContext.Request.GetDisplayUrl());
            var authority = currentUri.GetComponents(UriComponents.StrongAuthority, UriFormat.Unescaped);

            var url = string.Format("{0}://{1}{2}", context.HttpContext.Request.Scheme, authority, actionUrl);
            return url;
        }
    }
}
