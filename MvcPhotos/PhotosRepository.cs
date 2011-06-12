using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Dynamic;
using System.Linq;
using MvcPhotos.Data;
using MvcPhotos.Models;

namespace MvcPhotos
{
    public class PhotosRepository : IPhotosRepository
    {
        private PhotosDbContext _db;
        public PhotosRepository()
        {
            _db = new PhotosDbContext();
        }

        private Photo EditablePhoto(int id)
        {
            return (from p in _db.Photos.Include(m => m.Tags)
                         where p.Id == id
                         select p).First();
        }
        private IQueryable<Photo> VisiblePhotos()
        {
            return from photo in _db.Photos.Include(p => p.Tags)
                   where photo.IsUploaded && !photo.IsDeleted
                   select photo;
        }

        public Photo GetPhoto(int id)
        {
            return (from photo in VisiblePhotos()
                    where photo.Id == id
                    select photo)
                    .FirstOrDefault();
        }

        public Photo GetPhoto(string messageId)
        {
            return (from photo in VisiblePhotos()
                    where photo.MessageId == messageId
                    select photo)
                    .FirstOrDefault();
        }

        public int PhotoTotal()
        {
            return VisiblePhotos().Count();
        }

        public IEnumerable<Photo> UploadingPhotos()
        {
            return (from photo in _db.Photos
                    where !photo.IsUploaded && !photo.IsDeleted
                    select photo).ToList();
        }

        public IEnumerable<Photo> TagPhotos(string tagWord, int last, int count)
        {
            var hash = tagWord.ToLower().GetHashCode();

            return (from photo in VisiblePhotos()
                    from tag in photo.Tags
                    where photo.Id > last && tag.Hash == hash
                    select photo)
                    .OrderByDescending(p => p.Id)
                    .Take(count)
                    .ToList();
        }

        public IEnumerable<Photo> RecentPhotos(int last, int count)
        {
            return (from photo in VisiblePhotos()
                    where photo.Id > last
                    select photo)
                .OrderByDescending(p => p.Id)
                .Take(count).ToList();
        }

        public IEnumerable<dynamic> RecentTags(int count)
        {
            var hashs = from t in _db.Tags
                        group t by t.Hash
                            into tg
                            select new { Hash = tg.Key, Count = tg.Count(), Id = tg.Min(t => t.Id) };

            return (from tag in _db.Tags
                    join hash in hashs on tag.Id equals hash.Id
                    orderby hash.Count descending
                    select new { hash.Hash, hash.Count, tag.Text })
                .Take(count)
                .ToList()
                .Select(model => new { model.Hash, model.Count, model.Text }.ToDynamic());
        }

        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        public DbEntityValidationResult Validation<T>(T model) where T : class
        {
            return _db.Entry(model).GetValidationResult();
        }

        public void Append(Photo photo)
        {
            if (!Validation(photo).IsValid)
                return;

            _db.Photos.Add(photo);
        }

        public void UploadedPhoto(int id)
        {
            var photo = EditablePhoto(id);
            if (photo == null || photo.IsUploaded)
                return;

            photo.IsUploaded = true;
        }

        public void DeletePhoto(int id)
        {
            var photo = EditablePhoto(id);
            if (photo == null || photo.IsDeleted)
                return;

            photo.IsDeleted = true;
        }

        public dynamic Dynamic(Action<dynamic> init)
        {
            dynamic model = new ExpandoObject();
            init(model);
            return model;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
