namespace MvcPhotos.Web.Models
{
    /// <summary>
    /// dynamicならそのまま使えばいいじゃないかと思うところだけど、
    /// dynamicだとJavaScriptSerializerが思ったように動いてくれなくて。
    /// Key/Valueなのはそうなんだけどさ～。
    /// </summary>
    public class Tag
    {
        public int Hash { get; set; }
        public string Text { get; set; }
        public int Count { get; set; }

        public Tag()
        {
            
        }

        public Tag(dynamic model)
        {
            Hash = model.Hash;
            Text = model.Text.ToLower();
            Count = model.Count;
        }
    }
}