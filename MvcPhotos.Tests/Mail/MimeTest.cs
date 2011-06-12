using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Mail;

namespace MvcPhotos.Tests.Mail
{
    /// <summary>
    /// MimeTest の概要の説明
    /// </summary>
    [TestClass]
    public class MimeTest
    {
        [TestMethod]
        public void 日本語7bitエンコード()
        {
            var source = @"$BK\J8$b$""$k$h!#(B";
            var text = Mime.DecodeString("7bit", "ISO-2022-JP", source);

            Console.WriteLine(text);
            Assert.AreEqual(text, "本文もあるよ。");
        }

        [TestMethod]
        public void UTF8エンコード()
        {
            var source = @"44OG44K544OI44Oh44O844Or44Gu5pys5paHDQoyMDExLzA0LzI3IDE3OjQ4OjQw";
            var text = Mime.DecodeString("base64", "utf-8", source);

            Console.WriteLine(text);
            Assert.IsTrue(text.StartsWith("テストメールの本文"));
        }
    }
}
