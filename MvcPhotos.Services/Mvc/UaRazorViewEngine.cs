using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace MvcPhotos.Services.Mvc
{
    public class UaRazorViewEngine : RazorViewEngine
    {
        public const string UaDataTokenKey = "ua";
        public static string[] UaRoutes = new[]{"mobile",""};

        private Dictionary<string, string[]> TokenizeFormats = new Dictionary<string, string[]>();

        public UaRazorViewEngine() : this(null){}
        public UaRazorViewEngine(IViewPageActivator viewPageActivator) : base(viewPageActivator)
        {
            foreach (var token in UaRoutes)
            {
                Func<string[], string[]> tokenizer = formats => formats;
                if (!string.IsNullOrEmpty(token))
                    tokenizer = formats => formats.Select(format => Regex.Replace(format, ".(vb|cs)html$", "." + token + ".$1html")).ToArray();

                EntryTokenizeFormats(token, tokenizer, () => AreaViewLocationFormats);
                EntryTokenizeFormats(token, tokenizer, () => AreaMasterLocationFormats);
                EntryTokenizeFormats(token, tokenizer, () => AreaPartialViewLocationFormats);
                EntryTokenizeFormats(token, tokenizer, () => ViewLocationFormats);
                EntryTokenizeFormats(token, tokenizer, () => MasterLocationFormats);
                EntryTokenizeFormats(token, tokenizer, () => PartialViewLocationFormats);
            }
        }

        private void EntryTokenizeFormats(string token, Func<string[],string[]> tokenizer, Expression<Func<string[]>> expression)
        {
            var key = ExpressionHelper.GetExpressionText(expression);
            var formats = tokenizer(expression.Compile().Invoke().ToArray());

            TokenizeFormats[token + ":" + key] = formats;
        }

        private string[] GetTokenizeFormats(string token, Expression<Func<string[]>> expression)
        {
            var key = ExpressionHelper.GetExpressionText(expression);
            return TokenizeFormats[token + ":" + key];
        }

        private void SetTokenizeFormats(string token)
        {
            AreaViewLocationFormats = GetTokenizeFormats(token, () => AreaViewLocationFormats);
            AreaMasterLocationFormats = GetTokenizeFormats(token, () => AreaMasterLocationFormats);
            AreaPartialViewLocationFormats = GetTokenizeFormats(token, () => AreaPartialViewLocationFormats);
            ViewLocationFormats = GetTokenizeFormats(token, () => ViewLocationFormats);
            MasterLocationFormats = GetTokenizeFormats(token, () => MasterLocationFormats);
            PartialViewLocationFormats = GetTokenizeFormats(token, () => PartialViewLocationFormats);
        }

        private static string GetUaToken(ControllerContext controllerContext)
        {
            var dataToken = controllerContext.IsChildAction
                                ? controllerContext.ParentActionViewContext.RouteData.DataTokens
                                : controllerContext.RouteData.DataTokens;

            return (string)dataToken[UaDataTokenKey];
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var token = GetUaToken(controllerContext);
            SetTokenizeFormats(token);

            return base.FindView(controllerContext, viewName, masterName, false/*useCache*/);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var token = GetUaToken(controllerContext);
            SetTokenizeFormats(token);

            return base.FindPartialView(controllerContext, partialViewName, false/*useCache*/);
        }
    }
}
