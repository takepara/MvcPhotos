using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvcPhotos.Tests
{
    /// <summary>
    /// ImageResizerTest の概要の説明
    /// </summary>
    [TestClass]
    public class ImageResizerTest
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

        private bool IsLocal
        {
            get { return ConfigurationManager.AppSettings["Environment"] == "Debug"; }
        }

        [TestMethod]
        public void 正常なDoCoMo写真の向き()
        {
            if(!IsLocal)
                return;

            var path = Path.Combine(TestContext.DeploymentDirectory, "3-0-image.jpg");
            var orientation = ImageResizer.GetExifOrientation(path);

            Assert.AreEqual(orientation, ExifOrientation.TopLeftSide);
        }

        [TestMethod]
        public void 左に90度回転してるiPhone写真の向き()
        {
            if (!IsLocal)
                return;

            var path = Path.Combine(TestContext.DeploymentDirectory, "1-0-__.jpg");
            var orientation = ImageResizer.GetExifOrientation(path);

            Assert.AreEqual(orientation, ExifOrientation.RightSideTop);
        }

        [TestMethod]
        public void 写真の撮影日時()
        {
            if (!IsLocal)
                return;

            var path = Path.Combine(TestContext.DeploymentDirectory, "1-0-__.jpg");
            var takenAt = ImageResizer.GetExifTakenAt(path);

            Assert.IsNotNull(takenAt);
            Assert.AreEqual(takenAt, DateTime.Parse("2011/05/17 12:15:26"));
        }

        [TestMethod]
        public void 写真の撮影日時が含まれてない()
        {
            if (!IsLocal)
                return;

            var path = Path.Combine(TestContext.DeploymentDirectory, "2-0-metalking.jpg");
            var takenAt = ImageResizer.GetExifTakenAt(path);

            Assert.IsNull(takenAt);
        }

        [TestMethod]
        public void 画像を自動回転()
        {
            if (!IsLocal)
                return;

            var files = Directory.GetFiles(TestContext.DeploymentDirectory, "*.jpg");
            var resizer = new ImageResizer();

            foreach (var file in files)
            {
                var baseDir = TestContext.TestDir;
                var path = Path.Combine(baseDir, "orientation-" + Path.GetFileName(file));

                // 実行できたなら結果は目視でいいんじゃん？
                resizer.Optimize(file, path);

                Assert.IsTrue(File.Exists(path));
            }
        }

        [TestMethod]
        public void 画像を１００ピクセルにリサイズ()
        {
            if (!IsLocal)
                return;

            var files = Directory.GetFiles(TestContext.DeploymentDirectory, "*.jpg");
            var resizer = new ImageResizer();

            var baseDir = TestContext.TestDir;
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var path = Path.Combine(baseDir, "opt-" + name);

                // 実行できたなら結果は目視でいいんじゃん？
                resizer.Optimize(file, path);
                Assert.IsTrue(resizer.Resize(path, ResizeMode.Fit, 100, Path.Combine(baseDir, "fit-" + name)));
                Assert.IsTrue(resizer.Resize(path, ResizeMode.Clip, 100, Path.Combine(baseDir, "clip-" + name)));
            }
        }

        [TestMethod]
        public void 小さすぎる画像を１００ピクセルにリサイズ()
        {
            if (!IsLocal)
                return;

            var resizer = new ImageResizer();
            var baseDir = TestContext.TestDir;
            var name = "little.png";
            var path = Path.Combine(TestContext.DeploymentDirectory, name);
            Assert.IsTrue(resizer.Resize(path, ResizeMode.Fit, 100, Path.Combine(baseDir, "fit-" + name)));
            Assert.IsTrue(resizer.Resize(path, ResizeMode.Clip, 100, Path.Combine(baseDir, "clip-" + name)));
        }

        [TestMethod]
        public void 短い長さに合わせるリサイズ計算()
        {
            var resize = ImageResizer.ClipSize(new Rectangle(0, 0, 200, 300), 100);

            Assert.AreEqual(resize.Width, 100);
            Assert.AreEqual(resize.Height, 150);
        }

        [TestMethod]
        public void 小さいものを短い長さに合わせるリサイズ計算()
        {
            var resize = ImageResizer.ClipSize(new Rectangle(0, 0, 50, 50), 100);

            Assert.AreEqual(resize.Width, 50);
            Assert.AreEqual(resize.Height, 50);
        }

        [TestMethod]
        public void 長い長さを合わせるリサイズ計算()
        {
            var resize = ImageResizer.FitSize(new Rectangle(0, 0, 200, 400), 100);

            Assert.AreEqual(resize.Width, 50);
            Assert.AreEqual(resize.Height, 100);
        }

        [TestMethod]
        public void 小さいものを長い長さに合わせるリサイズ計算()
        {
            var resize = ImageResizer.FitSize(new Rectangle(0, 0, 30, 40), 100);

            Assert.AreEqual(resize.Width, 30);
            Assert.AreEqual(resize.Height, 40);
        }

        [TestMethod]
        public void 大きい画像をクリップするときの元画像位置()
        {
            var resize = ImageResizer.ClipRePositionSource(new Rectangle(0, 0, 100, 150), 100);

            Assert.AreEqual(resize.X, 0);
            Assert.AreEqual(resize.Y, 25);
        }

        [TestMethod]
        public void 大きい画像をクリップするときの生成画像位置()
        {
            var resize = ImageResizer.ClipRePositionDestination(new Rectangle(0, 0, 100, 150), 100);

            Assert.AreEqual(resize.X, 0);
            Assert.AreEqual(resize.Y, 0);
        }


        [TestMethod]
        public void 小さい画像をクリップするときの元画像位置()
        {
            var resize = ImageResizer.ClipRePositionSource(new Rectangle(0, 0, 50, 50), 100);

            Assert.AreEqual(resize.X, 0);
            Assert.AreEqual(resize.Y, 0);
        }

        [TestMethod]
        public void 小さい画像をクリップするときの生成画像位置()
        {
            var resize = ImageResizer.ClipRePositionDestination(new Rectangle(0, 0, 50, 50), 100);

            Assert.AreEqual(resize.X, 25);
            Assert.AreEqual(resize.Y, 25);
        }

    }
}
