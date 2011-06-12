using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Mail;

namespace MvcPhotos.Tests.Mail
{
    [TestClass]
    public class RealPop3Test
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

        private readonly bool _secure;
        private readonly string _server;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        private const int TestMailMaxNo = 10;

        public RealPop3Test()
        {
            _secure = bool.Parse(ConfigurationManager.AppSettings["Pop3.Secure"] ?? "false");
            _server = ConfigurationManager.AppSettings["Pop3.Server"];
            _port = int.Parse(ConfigurationManager.AppSettings["Pop3.Port"] ?? "110");
            _userName = ConfigurationManager.AppSettings["Pop3.UserName"];
            _password = ConfigurationManager.AppSettings["Pop3.Password"];
        }

        [TestInitialize]
        public void 初期化()
        {
            // リセットしとく
            Pop3.TransportFactory = () => new Transport();
        }

        [TestCleanup]
        public void 全メール削除()
        {
            Response response;
            using (var pop3 = new Pop3())
            {
                pop3.Connect(_server, _port, _secure);
                pop3.Login(_userName, _password);
                response = pop3.GetMessageItems();
                // 全部消す
                foreach (var item in response.Items.Where(i => i.No > TestMailMaxNo))
                {
                    pop3.CommandDele(item.No);
                }
                pop3.CommandQuit();
            }
        }

        public void SendTestMail(string to)
        {
            var mailClient = new SmtpClient();
            mailClient.Send("takepara@gmail.com", to, "テストメール件名", "テストメールの本文\r\n" + DateTime.Now);
        }

        [TestMethod]
        public void サーバーに接続()
        {
            Response response;
            using (var pop3 = new Pop3())
            {
                response = pop3.Connect(_server, _port, _secure);
                Console.WriteLine(response.Trace);
            }
            Assert.IsTrue(response.Successed);
        }

        [TestMethod]
        public void ログイン()
        {
            Response response;
            using (var pop3 = new Pop3())
            {
                response = pop3.Connect(_server, _port, _secure);
                Console.WriteLine(response.Trace);

                response = pop3.Login(_userName, _password);
                Console.WriteLine(response.Trace);
            }
            Assert.IsTrue(response.Successed);
        }

        [TestMethod]
        public void メッセージ一覧を取得()
        {
            SendTestMail(_userName);

            Response response;
            using (var pop3 = new Pop3())
            {
                response = pop3.Connect(_server, _port, _secure);

                Console.WriteLine(response.Trace);

                response = pop3.Login(_userName, _password);
                Console.WriteLine(response.Trace);

                response = pop3.GetMessageItems();
                Console.WriteLine(response.Trace);
                Console.WriteLine(response.Items.Count + " messages");
            }

            Assert.IsTrue(response.Successed);
            Assert.IsTrue(response.Items.Any());
        }

        [TestMethod]
        public void メッセージを取得()
        {
            SendTestMail(_userName);

            Response response;
            using (var pop3 = new Pop3())
            {
                response = pop3.Connect(_server, _port, _secure);

                Console.WriteLine(response.Trace);

                response = pop3.Login(_userName, _password);
                Console.WriteLine(response.Trace);

                response = pop3.GetMessageItems();
                Console.WriteLine(response.Trace);

                if (response.Items.Any())
                {
                    var messageNo = response.Items.First().No;
                    response = pop3.CommandRetr(messageNo);
                    Console.WriteLine(response.Trace);
                    Console.WriteLine(response.Message.Source);
                }
            }

            Assert.IsTrue(response.Successed);
        }

        private Response InternalMailRequest(int mailNo)
        {
            using (var pop3 = new Pop3())
            {
                var response = pop3.Connect(_server, _port, _secure);

                Console.WriteLine(response.Trace);

                response = pop3.Login(_userName, _password);
                Console.WriteLine(response.Trace);

                response = pop3.CommandRetr(mailNo);
                Assert.IsTrue(response.Successed);

                return response;
            }
        }

        private string InternalSaveMultipart(Multipart multipart, string name)
        {
            var baseDir = TestContext.TestDir;
            var path = Path.Combine(baseDir, name);

            if (File.Exists(path))
                File.Delete(path);

            multipart.SaveToFile(path);
            return path;
        }

        [TestMethod]
        public void iPhone1画像添付のメッセージを取得()
        {
            var response = InternalMailRequest(1);
            Assert.IsTrue(response.Message.IsMultipart);

            var text =
                response.Message.FirstTextPlainMultipart();
            var images = response.Message.Multiparts
                .Where(m => m.Body.ContentType.StartsWith("image/"));

            Assert.IsTrue(response.Message.Header("From").Contains("@i.softbank.jp"));
            Assert.AreEqual(images.Count(), 1);

            var multipart = images.First();
            var path = InternalSaveMultipart(multipart, multipart.Body.FileName);
            Console.WriteLine("File Name:" + path);
        }

        [TestMethod]
        public void Gmail1画像添付のメッセージを取得()
        {
            var response = InternalMailRequest(2);
            Assert.IsTrue(response.Message.IsMultipart);

            var text =
                response.Message.FirstTextPlainMultipart();
            var images = response.Message.Multiparts
                .Where(m => m.Body.ContentType.StartsWith("image/"));

            Assert.IsTrue(response.Message.Header("From").Contains("@gmail.com"));
            Assert.AreEqual(images.Count(), 1);

            var multipart = images.First();
            var path = InternalSaveMultipart(multipart, multipart.Body.FileName);
            Console.WriteLine("File Name:" + path);
        }

        [TestMethod]
        public void DoCoMo1画像添付のメッセージを取得()
        {
            var response = InternalMailRequest(3);
            Assert.IsTrue(response.Message.IsMultipart);

            var text =
                response.Message.FirstTextPlainMultipart();
            var images = response.Message.Multiparts
                .Where(m => m.Body.ContentType.StartsWith("image/"));

            Assert.IsTrue(response.Message.Header("From").Contains("@docomo.ne.jp"));
            Assert.AreEqual(images.Count(), 1);

            var multipart = images.First();
            var path = InternalSaveMultipart(multipart, multipart.Body.FileName);
            Console.WriteLine("File Name:" + path);
        }

        [TestMethod]
        public void DoCoMo2画像添付のメッセージを取得()
        {
            var response = InternalMailRequest(4);
            Assert.IsTrue(response.Message.IsMultipart);

            var text =
                response.Message.FirstTextPlainMultipart();
            var images = response.Message.Multiparts
                .Where(m => m.Body.ContentType.StartsWith("image/"));

            Assert.IsTrue(response.Message.Header("From").Contains("@docomo.ne.jp"));
            Assert.AreEqual(images.Count(), 2);

            var multipart = images.First();
            var path = InternalSaveMultipart(multipart, multipart.Body.FileName);
            Console.WriteLine("File Name:" + path);
            multipart = images.Skip(1).First();
            path = InternalSaveMultipart(multipart, multipart.Body.FileName);
            Console.WriteLine("File Name:" + path);
        }

        [TestMethod]
        public void いろいろ画像添付のメッセージを取得()
        {
            for (int i = 1; i <= TestMailMaxNo; i++)
            {
                var response = InternalMailRequest(i);
                Assert.IsTrue(response.Message.IsMultipart);

                var text =
                    response.Message.FirstTextPlainMultipart();
                var images = response.Message.Multiparts
                    .Where(m => m.Body.ContentType.StartsWith("image/")).ToArray();

                Assert.IsTrue(images.Count() > 0);

                for(var j =0;j<images.Count();j++)
                {
                    var multipart = images[j];
                    var path = InternalSaveMultipart(multipart, i + "-" + j + "-" + multipart.Body.FileName);

                    Assert.IsTrue(File.Exists(path));
                    Console.WriteLine("File Name:" + path);
                }
            }
        }

        [TestMethod]
        public void メッセージを削除()
        {
            SendTestMail(_userName);

            Response response;
            using (var pop3 = new Pop3())
            {
                response = pop3.Connect(_server, _port, _secure);

                Console.WriteLine(response.Trace);

                response = pop3.Login(_userName, _password);
                Console.WriteLine(response.Trace);

                response = pop3.GetMessageItems();
                Console.WriteLine(response.Trace);
                Console.WriteLine(response.Items.Count + " messages");

                // 全部消す
                foreach (var item in response.Items.Where(i => i.No > TestMailMaxNo))
                {
                    response = pop3.CommandDele(item.No);
                    Assert.IsTrue(response.Successed);
                    Console.WriteLine(response.Trace);
                }
                response = pop3.CommandQuit();
                Console.WriteLine(response.Trace);
            }

            Assert.IsTrue(response.Successed);
        }

    }
}
