using System.Collections.Generic;
using MvcPhotos.Models;

namespace MvcPhotos
{
    public interface IStorageProvider
    {
        string LocalStoragePath { get; }
        StorageEntry GetEntry(string path);
        IEnumerable<StorageEntry> GetEntries();
        IEnumerable<StorageEntry> GetEntries(string path);
        StorageEntry AddEntry(StorageEntry entry);
        void RemoveEntry(string entryName);
    }
}