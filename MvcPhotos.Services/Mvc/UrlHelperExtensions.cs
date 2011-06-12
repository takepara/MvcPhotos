using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MvcPhotos.Services.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string UaRouteUrl(this UrlHelper helper,string routeName, object routeValues)
        {
            var token = (string)helper.RequestContext.RouteData.DataTokens[UaRazorViewEngine.UaDataTokenKey];
            return helper.RouteUrl(token + routeName, routeValues);
        }
    }
}
