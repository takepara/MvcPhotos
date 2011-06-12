using System;
using System.Collections.Generic;
using System.Configuration;
using MvcPhotos.Models;

namespace MvcPhotos.Storage.Providers
{
    public class GoogleStorageProvider : IStorageProvider
    {
        private string _authKey;
        private string _authSecret;

        public GoogleStorageProvider()
        {
            _authKey = ConfigurationManager.AppSettings["GoogleStorage.AuthKey"];
            _authSecret = ConfigurationManager.AppSettings["GoogleStorage.AuthSecret"];
        }

        public string LocalStoragePath
        {
            get { throw new NotImplementedException(); }
        }

        public StorageEntry GetEntry(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StorageEntry> GetEntries()
        {

            throw new NotImplementedException();
        }

        public IEnumerable<StorageEntry> GetEntries(string filter)
        {
            throw new NotImplementedException();
        }

        public StorageEntry AddEntry(StorageEntry entry)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void RemoveEntry(string entryName)
        {
            throw new NotImplementedException();
        }
    }
}
