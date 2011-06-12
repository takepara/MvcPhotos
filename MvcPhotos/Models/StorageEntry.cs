using System;
using System.IO;

namespace MvcPhotos.Models
{
    public class StorageEntry
    {
        public string Name { get; set; }
        public long Length { get; set; }
        public string ContentType { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[] Contents { get; set; }

        public StorageEntry() { }
        public StorageEntry(Photo photo)
        {
            Name = photo.FileName;
            ContentType = photo.ContentType;
        }

        public byte[] Load(string localPath)
        {
            return Load(localPath, Name);
        }

        public byte[] Load(string localPath, string fileName)
        {
            var path = Path.Combine(localPath, fileName);
            if (!File.Exists(path))
                return null;

            var contents = File.ReadAllBytes(path);

            Length = contents.Length;
            Contents = contents;

            return Contents;
        }

        public void Delete(string localPath)
        {
            var path = Path.Combine(localPath, Name);
            var dir = Path.GetDirectoryName(path);
            
            if(Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }
}