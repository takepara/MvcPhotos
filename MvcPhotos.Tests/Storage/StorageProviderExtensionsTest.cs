using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Storage;

namespace MvcPhotos.Tests.Storage
{
    [TestClass]
    public class StorageProviderExtensionsTest
    {
        [TestMethod]
        public void ドットのないGif()
        {
            var contentType = StorageProviderExtensions.ContentTypeFromExtension(null, "gif");

            Assert.AreEqual(contentType, "image/gif");
        }
        
        [TestMethod]
        public void ドットあるGif()
        {
            var contentType = StorageProviderExtensions.ContentTypeFromExtension(null, ".gif");

            Assert.AreEqual(contentType, "image/gif");
        }

        [TestMethod]
        public void メジャーな画像()
        {
            var jpeg = StorageProviderExtensions.ContentTypeFromExtension(null, "jpg");
            var png = StorageProviderExtensions.ContentTypeFromExtension(null, "png");
            var bmp = StorageProviderExtensions.ContentTypeFromExtension(null, "bmp");

            Assert.AreEqual(jpeg, "image/jpeg");
            Assert.AreEqual(png, "image/png");
            Assert.AreEqual(bmp, "image/bmp");
        }
    }
}
