using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using MvcPhotos.Models;

namespace MvcPhotos
{
    public static class StorageProviderExtensions
    {
        public static string ContentTypeFromExtension(this IStorageProvider provider, string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            using (var registry = Registry.ClassesRoot.OpenSubKey(extension))
            {
                return registry == null ? null : registry.GetValue("Content Type").ToString();
            }
        }

        public static void DeleteFile(this IStorageProvider provider, string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || dir.Equals(provider.LocalStoragePath,StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!Directory.Exists(dir))
                return;
            
            if (Directory.EnumerateFiles(dir).Any())
                return;

            Directory.Delete(dir, true);
        }

        public static StorageEntry ToStorageEntry(this IStorageProvider provider, string path)
        {
            if (!File.Exists(path))
                return null;

            var fileInfo = new FileInfo(path);
            var fileName = fileInfo.FullName.Replace(provider.LocalStoragePath, "").Replace("\\", "/").TrimStart('/');
            var contentType = provider.ContentTypeFromExtension(Path.GetExtension(fileName));
            var entry = new StorageEntry
                            {
                                Name = fileName,
                                ContentType = contentType,
                                Length = fileInfo.Length,
                                Timestamp = fileInfo.LastWriteTime,
                                Contents = new byte[fileInfo.Length],
                            };

            using (var stream = fileInfo.Open(FileMode.Open,FileAccess.Read, FileShare.None))
            {
                stream.Read(entry.Contents, 0, entry.Contents.Length);
            }

            return entry;
        }

    }
}