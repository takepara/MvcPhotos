using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MvcPhotos.Services.Mvc
{
    public class JsonableAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (filterContext.HttpContext.Request.AcceptTypes != null && 
                filterContext.HttpContext.Request.AcceptTypes.Contains("application/json") && 
                viewResult != null)
            {
                filterContext.Result = new JsonResult { Data = viewResult.Model, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            }
            base.OnActionExecuted(filterContext);
        }
    }
}
