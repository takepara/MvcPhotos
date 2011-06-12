using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Mail;

namespace MvcPhotos.Tests.Mail
{
    /// <summary>
    /// FakePop3Test の概要の説明
    /// </summary>
    [TestClass]
    public class FakePop3Test
    {
        [TestInitialize]
        public void 初期化()
        {
        }

        [TestMethod]
        public void テキスト本文のみ()
        {
            Pop3.TransportFactory = () => new FakeTransport("TextOnly");
            Response response;
            string mailBody;
            using(var pop3 = new Pop3())
            {
                response = pop3.CommandRetr(0);
                var message = response.Message;
                mailBody = Mime.DecodeString(message.Encoding, message.Charset, message.BodySource);
                Console.WriteLine(mailBody);
            }

            Assert.IsTrue(response.Successed);
            Assert.IsTrue(mailBody.Contains("テストメールの本文"));
        }

        [TestMethod]
        public void テキスト添付のみ()
        {
            Pop3.TransportFactory = () => new FakeTransport("TextOnlyAttatch");
            Response response;
            string mailBody;
            using (var pop3 = new Pop3())
            {
                response = pop3.CommandRetr(0);
                var message = response.Message.Multiparts.First().Body;
                mailBody = Mime.DecodeString(message.Encoding, message.Charset, message.BodySource);
                Console.WriteLine(mailBody);
            }

            Assert.IsTrue(response.Successed);
            Assert.IsTrue(mailBody.Contains("添付テキストのテスト"));
        }

        [TestMethod]
        public void テキストとテキスト添付()
        {
            Pop3.TransportFactory = () => new FakeTransport("TextAndTextAttatch");
            Response response;
            string mailBody;
            string text;
            using (var pop3 = new Pop3())
            {
                response = pop3.CommandRetr(0);
                var message = response.Message.FirstTextPlainMultipart();
                text = Mime.DecodeString(message.Encoding, message.Charset, message.BodySource);
                message = response.Message.Multiparts.First(m=>m.IsAttachment).Body;
                mailBody = Mime.DecodeString(message.Encoding, message.Charset, message.BodySource);
                Console.WriteLine(mailBody);
            }

            Assert.IsTrue(response.Successed);
            Assert.IsTrue(text.Contains("本文もあるよ。"));
            Assert.IsTrue(mailBody.Contains("添付テキストのテスト"));
        }

        [TestMethod]
        public void 画像添付()
        {
            Pop3.TransportFactory = () => new FakeTransport("ImageAttatch");
            Response response;
            Message message;
            using (var pop3 = new Pop3())
            {
                response = pop3.CommandRetr(0);
                message = response.Message.Multiparts.First(m => m.IsAttachment && !m.IsTextPlain).Body;
                Console.WriteLine(message.Source);
            }

            Assert.IsTrue(message.ContentType.StartsWith("image/jpeg"));
            Assert.IsTrue(response.Successed);
        }
    }
}
