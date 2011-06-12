using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using MvcPhotos.Models;

namespace MvcPhotos
{
    public interface IPhotosRepository : IDisposable
    {
        Photo GetPhoto(int id);
        Photo GetPhoto(string  messageId);
        DbEntityValidationResult Validation<T>(T model) where T : class;
        void Append(Photo photo);
        void UploadedPhoto(int id);
        void DeletePhoto(int id);
        int PhotoTotal();
        IEnumerable<Photo> UploadingPhotos();
        IEnumerable<Photo> TagPhotos(string tagWord, int last, int count);
        IEnumerable<Photo> RecentPhotos(int last, int count);
        IEnumerable<dynamic> RecentTags(int count);
        int SaveChanges();
    }
}