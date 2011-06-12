using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcPhotos.Web.Controllers;

namespace MvcPhotos.Tests.Controller
{
    /// <summary>
    /// HomeControllerTest の概要の説明
    /// </summary>
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void CreateInstance()
        {
            var controller = new HomeController();

            Assert.IsNotNull(controller);
        }
    }
}
