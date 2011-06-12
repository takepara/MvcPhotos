using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace MvcPhotos.Services.Mvc
{
    public class UaRoute : Route
    {
        public bool IsMobile { get; set; }
        
        public UaRoute(string url, IRouteHandler routeHandler) : base(url, routeHandler){}
        public UaRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler) : base(url, defaults, routeHandler){}
        public UaRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler) : base(url, defaults, constraints, routeHandler){}
        public UaRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler) : base(url, defaults, constraints, dataTokens, routeHandler){}
    }
}
