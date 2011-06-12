using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MvcPhotos.Services;
using MvcPhotos.Services.Mvc;
using MvcPhotos.Web.Models;

namespace MvcPhotos.Web.Controllers
{
    public class PhotosController : AsyncController
    {
        //private readonly PhotosService _service = new PhotosService();
        public static string CreateMessage = "photoscontroller.created";

        [Jsonable]
        public ActionResult Index(int last = 0, int size = 100)
        {
            using (var service = new PhotosService())
            {
                var model = new Paging<Photo>
                                {
                                    Items = service.RecentPhotos(last, size).Select(m => new Photo(m)),
                                    Total = -1
                                };
                return View(model);
            }
        }

        [Jsonable]
        public ActionResult Tags(string id, int last = 0, int size = 100)
        {
            using (var service = new PhotosService())
            {
                var model = new Paging<Photo>
                                {
                                    Items = service.TagPhotos(id, last, size).Select(m => new Photo(m)),
                                    Total = -1
                                };

                return View("Index", model);
            }
        }

        public ActionResult Create()
        {
            ViewBag.Message = TempData[CreateMessage];
            return View();
        }

        [HttpPost]
        public ActionResult Create(Photo photo)
        {
            using (var service = new PhotosService())
            {
                var model = photo.ToModel();
                if (ModelState.IsValid)
                {
                    var path = StorageSettings.BasePath("Storage.Uploading");
                    model.ImportTags(MvcPhotos.Models.Tag.ToTags(photo.InputTags.Split(' ')));
                    service.Append(model, path, photo.File);

                    // 保存
                    TempData[CreateMessage] = "登録しました";
                    return Redirect(Url.UaRouteUrl("Default", new {action = "Create"}));
                }
            }
            return View();
        }

        public void ImageAsync(int id, ResizeMode? type, int? size)
        {
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
            {
                using (var service = new PhotosService())
                {
                    var photo = service.GetPhoto(id);
                    if (photo == null)
                        return;

                    // 此処から始まる処理が概ねファイルアクセスで競合エラー起きる
                    // originalとoptimizeへのアクセスがサムネイルとプレビューで同時に発生するから
                    // ファイルアクセスが追いついてないみたい。
                    // なので、この2ファイルに関してはちょっと早めに処理するか、排他制御出来るように
                    // 制御しましょう。
                    Stream image;
                    //try
                    {
                        image = service.GetImage(photo, type ?? ResizeMode.Optimize, size ?? 0);
                    }
                    //catch{}
                    AsyncManager.Parameters["image"] = image;
                    AsyncManager.Parameters["contentType"] = photo.ContentType;
                    AsyncManager.OutstandingOperations.Decrement();
                }
            });
        }

        public ActionResult ImageCompleted(Stream image, string contentType)
        {
            if (image == null)
                return File("~/Content/waiting.png", "image/png");

            return File(image, contentType);
        }
    }
}
