using System.Web;
using System.Web.Mvc;

namespace MvcPhotos.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "mvcPhotos - ASP.NET MVC photoshot collection";
            
            return View();
        }

        public ActionResult Recycle()
        {
            HttpRuntime.UnloadAppDomain();
            return Content("recycling...");
        }
    }
}
