using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using MvcPhotos.Services;
using MvcPhotos.Services.Mvc;
using MvcPhotos.Storage.Providers;

namespace MvcPhotos.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void UaMapRoute(RouteCollection routes, string name, string url, object parameters)
        {
            foreach (var ua in UaRazorViewEngine.UaRoutes)
            {
                var uaUrl = ua + (string.IsNullOrEmpty(ua) ? "" : "/") + url;
                routes.MapRoute(
                    ua + name, // Route name
                    uaUrl, // URL with parameters
                    parameters
                ).DataTokens[UaRazorViewEngine.UaDataTokenKey] = ua;
            }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            UaMapRoute(routes,
                "Tags", // Route name
                "Tags/{id}", // URL with parameters
                new { controller = "Tags", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            UaMapRoute(routes,
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            ObjectResolver.Entry<IPhotosRepository>(() => new PhotosRepository());
            var useCloud = bool.Parse(ConfigurationManager.AppSettings["Storage.UseCloud"] ?? "false");
            var localPath = StorageSettings.BasePath("Storage.Local");
            if (!useCloud)
            {
                ObjectResolver.Entry<IStorageProvider>(() => new LocalStorageProvider(localPath));
            }
            else
            {
                ObjectResolver.Entry<IStorageProvider>(() => new AwsS3StorageProvider(localPath));
            }
            //DependencyResolver.SetResolver(new SimpleResolver());

            // create folder
            var dirs = new[]{
                StorageSettings.BasePath("Storage.Cache"),
                StorageSettings.BasePath("Storage.Uploading"),
                StorageSettings.BasePath("Storage.Local")};
            foreach (var dir in dirs.Where(dir => !Directory.Exists(dir)))
            {
                Directory.CreateDirectory(dir);
            }

            // Worker起動(Task便利～)
            Task.Factory.StartNew(() =>
            {
                MailWorker();
                StorageWorker();

                WorkerSchedule();
            });

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new UaRazorViewEngine());
        }

        private void OutputStackTrace()
        {
            // スタックトレースを出して確認してみる
            Trace.WriteLine("stack trace");
            var trace = new StackTrace();
            foreach (var stackFrame in trace.GetFrames())
            {
                Trace.WriteLine(" " + stackFrame.GetMethod().Name);
            }
        }

        public void WorkerSchedule()
        {
            // CacheExpiresの実装をこっそり覗くと２０秒が最短です。
            // 他にもTimerつかったりThreadPool使う方法もあります。
            HttpRuntime.Cache.Add(
                "_workerCacheTrigger", 0, null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromSeconds(20),
                CacheItemPriority.NotRemovable,
                (key, value, reason) =>
                {

                    MailWorker();
                    StorageWorker();

                    WorkerSchedule();
                });
        }

        public static void MailWorker()
        {
            var enable = bool.Parse(ConfigurationManager.AppSettings["Worker.Mail"] ?? "false");
            var service = new MailService();
            var uploading = StorageSettings.BasePath("Storage.Uploading");

            Trace.WriteLine("Mail working : " + DateTime.Now);
            if (enable)
            {
                service.Receive(uploading);
            }
            Trace.WriteLine("Mail worked : " + DateTime.Now);
        }

        public static void StorageWorker()
        {
            var enable = bool.Parse(ConfigurationManager.AppSettings["Worker.Storage"] ?? "false");
            using (var service = new PhotosService())
            {
                var uploading = StorageSettings.BasePath("Storage.Uploading");

                Trace.WriteLine("Storage working : " + DateTime.Now);
                if (enable)
                {
                    service.Upload(uploading);
                }
                Trace.WriteLine("Storage worked : " + DateTime.Now);
            }
        }
    }

    #region IDependencyResolver
    public class SimpleResolver : IDependencyResolver
    {
        public SimpleResolver()
        {
            ObjectResolver.Entry<IControllerFactory>(() => new DefaultControllerFactory());
            ObjectResolver.Entry<IControllerActivator>(() => null);
            ObjectResolver.Entry<IFilterProvider>(() => GlobalFilters.Filters);
            ObjectResolver.Entry<IModelBinderProvider>(() => null);
            ObjectResolver.Entry<ValueProviderFactory>(() => null);
            ObjectResolver.Entry<ModelMetadataProvider>(() => new DataAnnotationsModelMetadataProvider());
            ObjectResolver.Entry<ModelValidatorProvider>(() => new DataAnnotationsModelValidatorProvider());
            ObjectResolver.Entry<IViewEngine>(() => new RazorViewEngine());
            ObjectResolver.Entry<IViewPageActivator>(() => null);
        }

        public object GetService(Type serviceType)
        {
            return ObjectResolver.Contains(serviceType)
                        ? ObjectResolver.Resolve<object>(serviceType)
                        : Activator.CreateInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ObjectResolver.Resolves<object>(serviceType);
        }
    }
    #endregion
}
