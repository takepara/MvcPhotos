using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.Web.Mvc;

namespace MvcPhotos.Web.Models
{
    /// <summary>
    /// POCOをそのまま使えばいいじゃないかというのもあり。
    /// DbContext用のPOCOにメソッド付けて実装しても問題ないし、プロパティもNonMappedにしとけば
    /// DBには反映されないからね。
    /// 今回は入力のバリデーションと、DBのバリデーションが違うルールがあるので分けてます。
    /// もちろんUIに関連する要素がある場合も分けるべし、でしょう。
    /// </summary>
    public class Photo
    {
        public int Id { get; set; }

        public string[] Tags { get; set; }

        [ScriptIgnore]
        [Required, Display(Name = "Tags")]
        public string InputTags { get; set; }

        [ScriptIgnore]
        [Required, Display(Name = "Photo file")]
        public HttpPostedFileBase File { get; set; }

        [DataType(DataType.MultilineText), Display(Name = "Description")]
        public string Description { get; set; }

        [ScriptIgnore]
        [Required, Display(Name = "Delete Code")]
        public string DeleteCode { get; set; }

        public static string EmailAddress
        {
            get { return ConfigurationManager.AppSettings["EmailAddress"]; }
        }

        public Photo(){}
        public Photo(MvcPhotos.Models.Photo model)
        {
            ModelCopier.CopyModel(model, this);

            Tags = model.Tags.Select(t=>t.Text.ToLower()).ToArray();
        }

        public MvcPhotos.Models.Photo ToModel()
        {
            var model = new MvcPhotos.Models.Photo();
            ModelCopier.CopyModel(this, model);

            model.Tags.Clear();
            model.ImportTags(MvcPhotos.Models.Tag.ToTags(Tags));

            return model;
        }
    }
}