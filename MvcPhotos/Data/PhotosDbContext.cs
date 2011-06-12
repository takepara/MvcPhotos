using System.Configuration;
using System.Data.Entity;
using MvcPhotos.Models;

namespace MvcPhotos.Data
{
    public class PhotosDbContext : DbContext
    {
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public PhotosDbContext() : base("mvcPhotos")
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Debug" && 
                ConfigurationManager.ConnectionStrings["mvcPhotos"].ProviderName.Contains("SqlServerCe"))
                Database.SetInitializer(new PhotosDbContextInitializer());
        }
    }

    public class PhotosDbContextInitializer : DropCreateDatabaseIfModelChanges<PhotosDbContext>
    {
    }

}
