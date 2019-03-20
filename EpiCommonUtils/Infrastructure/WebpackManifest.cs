using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public static class WebpackManifest
    {

        private static string _manifestPath;

        public static string ManifestPath
        {
            get
            {
                if (_manifestPath != null)
                {
                    return _manifestPath;
                }

                _manifestPath = ConfigurationManager.AppSettings["webpack:manifestPath"];

                return _manifestPath ?? "/dist/webpack-assets.json";
            }
            
            set => _manifestPath = value;
        }

        private static readonly Lazy<IReadOnlyDictionary<string, Entry>> instance = new Lazy<IReadOnlyDictionary<string, Entry>>(Load);

        public static IReadOnlyDictionary<string, Entry> Instance => instance.Value;

        private static IReadOnlyDictionary<string, Entry> Load()
        {
            var manifestAbsolutePath = System.Web.Hosting.HostingEnvironment.MapPath(ManifestPath);

            if (string.IsNullOrEmpty(manifestAbsolutePath))
            {
                throw new InvalidOperationException("Unable to get webpack manifest file path");
            }

            if (File.Exists(manifestAbsolutePath) == false)
            {
                throw new InvalidOperationException("Webpack manifest file does not exist");

            }

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.DeserializeObject<Dictionary<string, Entry>>(File.ReadAllText(manifestAbsolutePath), serializerSettings);
        }

        public class Entry
        {
            public string Css { get; set; }
            public string Js { get; set; }
        }
    }
}
