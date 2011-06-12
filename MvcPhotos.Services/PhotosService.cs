using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using MvcPhotos.Models;

namespace MvcPhotos.Services
{
    public class PhotosService : IDisposable
    {
        private readonly IPhotosRepository _repository;
        private readonly IStorageProvider _storage;

        public PhotosService()
        {
            _repository = ObjectResolver.Resolve<IPhotosRepository>();
            _storage = ObjectResolver.Resolve<IStorageProvider>();
        }

        public static string GetStringHash(string value)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var hashProvider = new SHA1CryptoServiceProvider();
            var hash = hashProvider.ComputeHash(bytes);

            return BitConverter.ToString(hash).ToLower().Replace("-", "");
        }

        private static string CachePath
        {
            get { return StorageSettings.BasePath("Storage.Cache"); }
        }

        private string CacheFullPath(string fileName)
        {
            return Path.Combine(CachePath, fileName);
        }

        private string RetrieveCache(string name)
        {
            var entry = _storage.GetEntry(name);
            if (entry == null)
                return null;

            var cacheName = CacheFullPath(name);
            if (!File.Exists(cacheName))
            {
                var dir = Path.GetDirectoryName(cacheName);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var stream = new FileStream(cacheName, FileMode.Create))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(entry.Contents, 0, entry.Contents.Length);
                }
            }

            return cacheName;
        }

        private void GenerateResize(string cacheName, string resizeName, ResizeMode type, int size)
        {
            var resizer = new ImageResizer();
            var resizePath = CacheFullPath(resizeName);
            if (type != ResizeMode.Original && !File.Exists(resizePath))
            {
                resizer.Resize(cacheName, type, size, resizePath);
            }
        }

        public Photo GetPhoto(int id)
        {
            return _repository.GetPhoto(id);
        }

        public static string GenerateFileName(string name, ResizeMode type)
        {
            return GenerateFileName(name, type, 0);
        }

        public static string GenerateFileName(string name, ResizeMode type, int size)
        {
            var names = new Dictionary<ResizeMode, string>
                            {
                                {ResizeMode.Original, "original{1}"},
                                {ResizeMode.Optimize, "optimize{1}"},
                                {ResizeMode.Clip, "clip.{0}{1}"},
                                {ResizeMode.Fit, "fit.{0}{1}"},
                            };
            var dir = Path.GetDirectoryName(name);
            var ext = Path.GetExtension(name) ?? ".unknown";

            var fileName = string.Format(names[type], size, ext);
            return string.IsNullOrEmpty(dir)
                ? fileName
                : string.Format("{0}/{1}", dir, fileName);
        }

        public Stream GetImage(Photo photo, ResizeMode type, int size)
        {
            // クラウドに送り込んだ？
            if (!photo.IsUploaded)
                return null;

            // 処理中？
            //if (!_processing.Lock(photo.Id))
            //{
            //    var dump = string.Format("[id = {0}, type = {1}, size = {2}]", photo.Id, type, size);
            //    Trace.WriteLine(" can't get lock - " + dump + " : " + DateTime.Now);
            //    return null;
            //}
                
            // ファイルがあるなら即返す
            var loadName = GenerateFileName(photo.FileName, type, size);
            var loadPath = CacheFullPath(loadName);
            var entry = new StorageEntry(photo);
            var resizer = new ImageResizer();
            if (!File.Exists(loadPath))
            {
                // キャッシュになければStorageから取り出す。
                var cacheName = RetrieveCache(photo.FileName);
                if (string.IsNullOrEmpty(cacheName))
                    return null;

                // 最適化して回転する
                var optName = GenerateFileName(photo.FileName, ResizeMode.Optimize);
                var optPath = CacheFullPath(optName);
                resizer.Optimize(cacheName, optPath);

                // ローカルキャッシュにリサイズしたものを取得
                GenerateResize(optPath, loadName, type, size);
            }
            //_processing.Unlock(photo.Id);

            var bytes = entry.Load(CachePath, loadName);


            return bytes == null ? null : new MemoryStream(bytes);
        }

        public IEnumerable<Photo> TagPhotos(string tagWord, int last, int count)
        {
            return _repository.TagPhotos(tagWord, last, count);
        }

        public IEnumerable<Photo> RecentPhotos(int last, int count)
        {
            return _repository.RecentPhotos(last, count);
        }

        public IEnumerable<dynamic> RecentTags(int count)
        {
            return _repository.RecentTags(count);
        }

        public static string InitializeSavePath(string uploadingPath, string fileName)
        {
            var name = GenerateFileName(Path.GetFileName(fileName), ResizeMode.Original);
            name = string.Format("{0}/{1}", Guid.NewGuid(), name);

            var path = Path.Combine(uploadingPath, name);

            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return name;
        }

        public bool Append(Photo photo, string uploadingPath, HttpPostedFileBase file)
        {
            if (photo == null)
                throw new ArgumentNullException("photo");
            if (file == null)
                throw new ArgumentNullException("file");

            photo.FileName = InitializeSavePath(uploadingPath, Path.GetFileName(file.FileName));

            var path = Path.Combine(uploadingPath, photo.FileName);

            // ファイルをローカルに保存
            // ※非同期でクラウドに転送するよ
            file.SaveAs(path);

            photo.TakenAt = ImageResizer.GetExifTakenAt(path);
            photo.EntryAt = DateTime.Now;
            photo.ContentType = file.ContentType;
            photo.Length = file.ContentLength;
            photo.Description = (photo.Description ?? "").Trim();
            photo.DeleteCode = GetStringHash(photo.DeleteCode);

            _repository.Append(photo);
            return _repository.SaveChanges() != 0;
        }

        public bool Delete(int id, string deleteCode)
        {
            var photo = _repository.GetPhoto(id);
            if (photo == null)
                return false;

            if (photo.DeleteCode != deleteCode)
                return false;

            _repository.DeletePhoto(id);
            return _repository.SaveChanges() != 0;
        }

        // 同時リクエスト時の排他制御
        // プロセスが1つでもリクエストは同時にくるからね！Worker Processが複数なら違う方法も考えましょう。

        public void Upload(string uploadingPath)
        {
            var uploading = _repository.UploadingPhotos();

            // ここは非同期にしてもしなくてもどっちでも～。
            // 同時にたくさんのファイルアップロードがあって、パフォーマンス問題が起きるなら
            // 非同期にするのを考えればいいんじゃないかな。コンカレントだし。

            //var tasks = new List<Task>();
            foreach (var photo in uploading)
            {
                // 処理
                //var task = Task.Factory.StartNew(photoId =>
                //{
                Trace.WriteLine(" uploading - " + photo.Id + " : " + DateTime.Now);
                var entry = new StorageEntry(photo);
                entry.Load(uploadingPath);

                // Provider使ってファイル保存
                _storage.AddEntry(entry);

                // 保存済み
                _repository.UploadedPhoto(photo.Id);
                _repository.SaveChanges();

                // 一時ファイルの削除
                entry.Delete(uploadingPath);

                Trace.WriteLine(" uploaded - " + photo.Id + " : " + DateTime.Now);

                //}, up.Id);
                //tasks.Add(task);
            }
            //Task.WaitAll(tasks.ToArray());
        }

        public void Dispose()
        {
            var repository = _repository as IDisposable;

            if (repository != null)
                repository.Dispose();
        }
    }
}
