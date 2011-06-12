using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Models;
using MvcPhotos.Storage.Providers;

namespace MvcPhotos.Tests.Storage
{
    /// <summary>
    /// LocalStorageProviderTest の概要の説明
    /// </summary>
    [TestClass]
    public class LocalStorageProviderTest
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private string StoragePath
        {
            get { return Path.Combine( TestContext.TestDir,"Storage"); }
        }

        private string CachePath
        {
            get { return Path.Combine( TestContext.TestDir,"Cache"); }
        }

        [TestInitialize]
        public void Startup()
        {
            Directory.CreateDirectory(StoragePath);
            Directory.CreateDirectory(CachePath);
        }

        private IStorageProvider GetProvider()
        {
            return new LocalStorageProvider(StoragePath);
        }

        [TestMethod]
        public void インスタンス作る()
        {
            var provider = GetProvider();

            Assert.IsNotNull(provider);
        }

        private StorageEntry TestStorageEntry(string name)
        {
            return new StorageEntry
            {
                Name = name,
                ContentType = "text/plain",
                Contents = new byte[] { 0x41, 0x42, 0x43 }
            };
        }

        [TestMethod]
        public void ファイルを作成()
        {
            var provider = GetProvider();
            var entry = TestStorageEntry("text.txt");
            var result = provider.AddEntry(entry);

            Assert.AreEqual(entry.Name, result.Name);
            Assert.IsTrue(entry.Contents.SequenceEqual(result.Contents));
        }

        [TestMethod]
        public void サブフォルダにファイルを作成()
        {
            var provider = GetProvider();
            var entry = TestStorageEntry("x/text.txt");
            var result = provider.AddEntry(entry);

            Assert.AreEqual(entry.Name, result.Name);
            Assert.IsTrue(entry.Contents.SequenceEqual(result.Contents));
        }

        [TestMethod]
        public void ファイルを削除()
        {
            var provider = GetProvider();
            var entry = TestStorageEntry("delete.txt");

            provider.AddEntry(entry);
            provider.RemoveEntry("delete.txt");

            Assert.IsFalse(File.Exists(Path.Combine(StoragePath, "delete.txt")));
        }

        [TestMethod]
        public void ファイル一覧()
        {
            var provider = GetProvider();
            for (int i = 0; i < 3; i++)
            {
                var entry = TestStorageEntry("list" + i + ".txt");
                provider.AddEntry(entry);
            }

            for (int i = 0; i < 3; i++)
            {
                var entry = provider.GetEntry("list" + i + ".txt");
                Assert.IsNotNull(entry);
                provider.RemoveEntry(entry.Name);
            }

            Assert.IsTrue(Directory.EnumerateFiles(StoragePath, "list*.*").Count() == 0);
        }

        [TestMethod]
        public void サブフォルダ内のファイル一覧()
        {
            var provider = GetProvider();
            for (var i = 0; i < 3; i++)
            {
                var entry = TestStorageEntry("xlist" + i + "/" + i + ".txt");
                provider.AddEntry(entry);
            }

            for (var i = 0; i < 3; i++)
            {
                var list = provider.GetEntries("xlist" + i + "/");
                Assert.AreEqual(list.Count(), 1);

                foreach (var entry in list)
                {
                    provider.RemoveEntry(entry.Name);
                }
                Assert.IsFalse(Directory.Exists(StoragePath + "\\xlist" + i));
            }
        }
    }
}
