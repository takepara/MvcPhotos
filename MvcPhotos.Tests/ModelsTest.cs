using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Models;

namespace MvcPhotos.Tests
{
    /// <summary>
    /// ModelsTest の概要の説明
    /// </summary>
    [TestClass]
    public class ModelsTest
    {
        [TestMethod]
        public void URLに使えない文字を含むTag()
        {
            var tag = new Tag {Text = "<#>"};

            Assert.AreEqual(tag.Text, "___");
            Assert.AreEqual(tag.Hash, "___".GetHashCode());
        }

        [TestMethod]
        public void URLに使えない文字を含むTagその２()
        {
            var tag = new Tag { Text = "@tags" };

            Assert.AreEqual(tag.Text, "_tags");
            Assert.AreEqual(tag.Hash, "_tags".GetHashCode());
        }

        [TestMethod]
        public void URLに使えない文字を含むTagその３()
        {
            var tag = new Tag { Text = "ta/gs" };

            Assert.AreEqual(tag.Text, "ta_gs");
            Assert.AreEqual(tag.Hash, "ta_gs".GetHashCode());
        }

        [TestMethod]
        public void URLに使えない文字を含まない()
        {
            var tag = new Tag { Text = "tag" };

            Assert.AreEqual(tag.Text, "tag");
            Assert.AreEqual(tag.Hash, "tag".GetHashCode());
        }
        
        [TestMethod]
        public void URLに使えない文字を含まない日本語()
        {
            var tag = new Tag { Text = "タグ" };

            Assert.AreEqual(tag.Text, "タグ");
            Assert.AreEqual(tag.Hash, "タグ".GetHashCode());
        }

        [TestMethod]
        public void Photoにタグ指定()
        {
            var photo = new Web.Models.Photo { Tags = new[]{"タグ"}}.ToModel();

            Assert.AreEqual(photo.Tags.Count, 1);
            Assert.AreEqual(photo.Tags.First().Text, "タグ");
        }

        [TestMethod]
        public void Photoに複数タグ指定()
        {
            var photo = new Web.Models.Photo { Tags = new[]{"タグ1","tag2"} }.ToModel();

            Assert.AreEqual(photo.Tags.Count, 2);
            Assert.AreEqual(photo.Tags.First().Text, "タグ1");
            Assert.AreEqual(photo.Tags.Skip(1).First().Text, "tag2");
        }

        [TestMethod]
        public void Photoに複数タグ指定URL使用不可文字あり()
        {
            var photo = new Web.Models.Photo { Tags = new []{"タグ1","tag2","</>"} }.ToModel();

            Assert.AreEqual(photo.Tags.Count, 3);
            Assert.AreEqual(photo.Tags.First().Text, "タグ1");
            Assert.AreEqual(photo.Tags.Skip(1).First().Text, "tag2");
            Assert.AreEqual(photo.Tags.Skip(2).First().Text, "___");
        }

        [TestMethod]
        public void PhotoにNULLタグ指定()
        {
            var photo = new Web.Models.Photo { Tags = null }.ToModel();

            Assert.AreEqual(photo.Tags.Count, 0);
        }

        [TestMethod]
        public void Photoに空タグ指定()
        {
            var photo = new Web.Models.Photo { Tags = new[]{""} }.ToModel();

            Assert.AreEqual(photo.Tags.Count, 0);
        }
    }
}
