using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MvcPhotos.Models
{
    public class Tag
    {
        public int Id { get; set; }
        
        public int Hash { get; set; }

        private string _text;
        [Required]
        public string Text
        {
            get { return _text; }
            set
            {
                _text = Sanitize(value);
                Hash = _text.ToLower().GetHashCode();
            }
        }

        public ICollection<Photo> Photos { get; set; }

        public Tag()
        {
            Photos = new List<Photo>();
        }

        public static IEnumerable<Tag> ToTags(string[] tags)
        {
            if (tags == null)
                return Enumerable.Empty<Tag>();

            return tags.Where(t => !string.IsNullOrEmpty(t))
                .Select(t => t.ToLower())
                .Distinct()
                .Select(tag => new Tag {Text = tag}).ToList();
        }

        public string Sanitize(string tag)
        {
            // 自動文字変換
            // ※・ファイルシステムで使えるない(「\/:*?"<>|」)
            //   ・URLで意味のある文字(「;/?:@&=+$,」)
            //   ・URLで禁止文字(制御文字, SP,「<>#%"{}|\^[]`」,日本語等8bit文字)
            //   これらは全てアンダーバーに変換！
            // まとめると → \/:;+*?"|@&=$,#%^`<>{}[]
            var valid = Regex.Replace(tag, "[\\\\/:;\\+\\*\\?\"\\|@&=\\$,#%\\^`<>\\{\\}\\[\\]]", "_");

            // autodiscoveryに対処するために(HTMLコメント内に2つのハイフンが入るとおかしくなる！！)
            // ハイフンは置き換える？
            //valid = valid.Replace('-', '_');

            return valid;
        }
    }
}
