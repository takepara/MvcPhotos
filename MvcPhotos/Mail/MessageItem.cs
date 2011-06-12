namespace MvcPhotos.Mail
{
    public class MessageItem
    {
        public int No { get; set; }
        public string Text { get; set; }
        public int Value { get; set; }

        public MessageItem()
        {
            No = Value = - 1;
            Text = "";
        }
    }
}
