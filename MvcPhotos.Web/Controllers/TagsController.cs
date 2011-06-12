using System.Linq;
using System.Web.Mvc;
using MvcPhotos.Services;
using MvcPhotos.Services.Mvc;
using MvcPhotos.Web.Models;

namespace MvcPhotos.Web.Controllers
{
    public class TagsController : Controller
    {
        [Jsonable]
        public ActionResult Index(string id, int size = 100)
        {
            using (var service = new PhotosService())
            {
                var model = new Paging<Tag>
                                {
                                    Items =
                                        service.RecentTags(size)
                                        .Select(t => new Tag(t)),
                                    Total = -1
                                };

                return View(model);
            }
        }

    }
}
