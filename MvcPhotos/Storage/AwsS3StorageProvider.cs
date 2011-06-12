using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using MvcPhotos.Models;

namespace MvcPhotos.Storage.Providers
{
    public class AwsS3StorageProvider : IStorageProvider
    {
        private static LockList<string> _lock = new LockList<string>();
        private readonly string _storagePath;
        public AwsS3StorageProvider(string storagePath)
        {
            _storagePath = storagePath;
        }

        public string LocalStoragePath
        {
            get { return _storagePath; }
        }

        public string BucketName
        {
            get { return ConfigurationManager.AppSettings["AWS.BucketName"]; }
        }

        public string AccessKey
        {
            get { return ConfigurationManager.AppSettings["AWS.AccessKey"]; }
        }

        public string SecretKey
        {
            get { return ConfigurationManager.AppSettings["AWS.SecretKey"]; }
        }

        private AmazonS3 GetAmazonS3()
        {
            return Amazon.AWSClientFactory.CreateAmazonS3Client(AccessKey, SecretKey);
        }

        public StorageEntry GetEntry(string path)
        {
            var cachePath = Path.Combine(_storagePath, path);
            if (!_lock.Lock(cachePath))
                return null;

            using (var s3 = GetAmazonS3())
            {
                var request = new GetObjectRequest().WithBucketName(BucketName).WithKey(path);
                using (var response = s3.GetObject(request))
                {
                    if (!File.Exists(cachePath))
                    {
                        response.WriteResponseStreamToFile(cachePath);
                    }
                }
            }
            _lock.Unlock(cachePath);
            return this.ToStorageEntry(cachePath);
        }

        public IEnumerable<StorageEntry> GetEntries()
        {
            return GetEntries(BucketName, "");
        }

        public IEnumerable<StorageEntry> GetEntries(string filter)
        {
            return GetEntries(BucketName, filter);
        }

        public IEnumerable<StorageEntry> GetEntries(string path, string filter)
        {
            var list = new List<StorageEntry>();
            using (var s3 = GetAmazonS3())
            {
                var listRequest = new ListObjectsRequest { BucketName = path };
                listRequest.WithPrefix(filter);
                using (var listResponse = s3.ListObjects(listRequest))
                {
                    foreach (var entry in listResponse.S3Objects)
                    {
                        var cachePath = Path.Combine(_storagePath, entry.Key);

                        var request = new GetObjectRequest().WithBucketName(BucketName).WithKey(entry.Key);
                        using (var response = s3.GetObject(request))
                        {
                            if (!File.Exists(cachePath))
                            {
                                response.WriteResponseStreamToFile(cachePath);
                            }
                        }

                        list.Add(this.ToStorageEntry(cachePath));
                    }
                }
            }

            return list;
        }

        public StorageEntry AddEntry(StorageEntry entry)
        {
            // 本体作成
            using (var stream = new MemoryStream(entry.Contents))
            {
                var request = new PutObjectRequest { InputStream = stream };
                request.WithBucketName(BucketName)
                       .WithKey(entry.Name)
                       .WithContentType(entry.ContentType);

                using (var s3 = GetAmazonS3())
                {
                    var response = s3.PutObject(request);
                    response.Dispose();
                }
            }

            return entry;
        }

        public void RemoveEntry(string entryName)
        {
            var request = new DeleteObjectRequest();
            request.WithBucketName(BucketName)
                   .WithKey(entryName);

            using (var s3 = GetAmazonS3())
            {
                var response = s3.DeleteObject(request);
                response.Dispose();
            }

            this.DeleteFile(Path.Combine(_storagePath, entryName));
        }
    }
}
