using System;
using System.Configuration;
using System.IO;

namespace MvcPhotos
{
    public class StorageSettings
    {
        public static string BasePath(string key)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                ConfigurationManager.AppSettings[key]);
        }
    }
}
