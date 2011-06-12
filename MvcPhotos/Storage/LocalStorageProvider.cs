using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MvcPhotos.Models;

namespace MvcPhotos.Storage.Providers
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _storagePath;

        public LocalStorageProvider(string storagePath)
        {
            _storagePath = storagePath;
        }

        public string LocalStoragePath
        {
            get { return _storagePath; }
        }

        private void AssertContainerExist()
        {
            if (!Directory.Exists(_storagePath))
                throw new ApplicationException("ディレクトリがありません");
        }

        public StorageEntry GetEntry(string path)
        {
            var entryPath = Path.Combine(LocalStoragePath, path);
            
            return this.ToStorageEntry(entryPath);
        }

        public IEnumerable<StorageEntry> GetEntries()
        {
            return GetEntries("*.*");
        }

        public IEnumerable<StorageEntry> GetEntries(string filter)
        {
            AssertContainerExist();

            return Directory.EnumerateFiles(_storagePath, filter, SearchOption.AllDirectories)
                            .Select(filePath => this.ToStorageEntry(filePath))
                            .Where(etnry => etnry != null && etnry.Name.StartsWith(filter));
        }

        private void CreateFile(string path, byte[] contents)
        {
            this.DeleteFile(path);
            
            // ディレクトリ作成
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            // 本体作成
            using (var stream = File.Create(path))
            {
                stream.Write(contents, 0, contents.Length);
            }
        }

        public StorageEntry AddEntry(StorageEntry entry)
        {
            AssertContainerExist();
            var path = Path.Combine(_storagePath, entry.Name);

            // 本体作成
            CreateFile(path, entry.Contents);

            return this.ToStorageEntry(path);
        }

        public void RemoveEntry(string entryName)
        {
            AssertContainerExist();

            var path = Path.Combine(_storagePath, entryName);
            if (File.Exists(path))
                this.DeleteFile(path);
        }
    }
}
