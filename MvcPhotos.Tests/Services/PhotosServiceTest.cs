using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Services;

namespace MvcPhotos.Tests.Services
{
    /// <summary>
    /// PhotoServiceTest の概要の説明
    /// </summary>
    [TestClass]
    public class PhotosServiceTest
    {
        [TestMethod]
        public void ハッシュを生成()
        {
            var hash = PhotosService.GetStringHash("takehara");
            Console.WriteLine(hash);

            Assert.AreNotEqual(hash,"takehara");
            Assert.AreEqual(hash, "efaaca8b1fd2ae56aa92c28f5dfe6c7991003236");
        }
    }
}
