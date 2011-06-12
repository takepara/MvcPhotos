using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Services;

namespace MvcPhotos.Tests.Services
{
    /// <summary>
    /// MailServiceTest の概要の説明
    /// </summary>
    [TestClass]
    public class MailServiceTest
    {
        [TestMethod]
        public void メールアドレスじゃない()
        {
            var invalids = new List<string>
                               {
                                   "aaa@ddd",
                                   "うそんこ",
                                   "",
                                   null,
                                   "それっぽい@a.com"
                               };

            Assert.IsTrue(invalids.All(email=>MailService.GetMailAddress(email) == null));
        }

        [TestMethod]
        public void 普通のメールアドレス()
        {
            var valids = new List<string>
                               {
                                   "aaa@ddd.com",
                                   "aaa@ddd.co.jp",
                                   "aaa@gmail.com",
                                   "aaa..aa@gmail.com",
                                   "a..a..aaa@gmail.com",
                                   "username+whatever@gmail.com"
                               };

            Assert.IsTrue(valids.All(email => MailService.GetMailAddress(email) != null));
        }

        [TestMethod]
        public void 名前の入ってるメールアドレス()
        {
            var emails = new []
                               {
                                   "あああ<aaa@ddd.com>",
                                   "あああ<aaa@ddd.co.jp>",
                                   "<aaa@gmail.com>",
                               };

            Assert.AreEqual(MailService.GetMailAddress(emails[0]), "aaa@ddd.com");
            Assert.AreEqual(MailService.GetMailAddress(emails[1]), "aaa@ddd.co.jp");
            Assert.AreEqual(MailService.GetMailAddress(emails[2]), "aaa@gmail.com");
        }
    }
}
