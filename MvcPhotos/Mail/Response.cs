using System.Collections.Generic;

namespace MvcPhotos.Mail
{
    public class Response
    {
        public bool Successed { get; set; }
        public string Trace { get; set; }
        public string Body { get; set; }

        public Message Message { get; set; }
        public IList<MessageItem> Items { get; set; }
        
        public Response()
        {
            Successed = false;
            Trace = "";
            Body = "";
            
            Message = new Message();
            Items = new List<MessageItem>();
        }
    }
}
