using System.Collections.Generic;

namespace MvcPhotos.Web.Models
{
    public class Paging<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Total { get; set; }
    }
}