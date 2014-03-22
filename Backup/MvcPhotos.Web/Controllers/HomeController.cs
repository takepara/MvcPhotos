using System.Web;
using System.Web.Mvc;

namespace MvcPhotos.Web.Controllers
{
    public class HomeController : Controller
    {
        public string InjectionTitle { get; set; }
        public ActionResult Index()
        {
            ViewBag.Title = InjectionTitle ?? "mvcPhotos - ASP.NET MVC photoshot collection";
            
            return View();
        }

        public ActionResult Recycle()
        {
            HttpRuntime.UnloadAppDomain();
            return Content("recycling...");
        }
    }
}
