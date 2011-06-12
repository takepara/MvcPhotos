using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MvcPhotos.Models
{


    public class Photo
    {
        public int Id { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public string Description { get; set; }

        [NotMapped, Range(1, int.MaxValue)]
        public int TagCount { get { return Tags.Count; } }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string ContentType { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int Length { get; set; }

        public string MessageId { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime EntryAt { get; set; }
        public bool IsUploaded { get; set; }
        public bool IsDeleted { get; set; }

        public string Sender { get; set; }
        public string DeleteCode { get; set; }

        public Photo()
        {
            Tags = new List<Tag>();
        }
        
        public void ImportTags(IEnumerable<Tag> tags)
        {
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }
    }
}
