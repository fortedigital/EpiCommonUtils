using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EPiServer.Web;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public class WebpackManifest
    {
        private readonly EpiCommonUtilsOptions _options;
        private readonly IWebHostingEnvironment _webHostingEnvironment;
        private readonly Lazy<IReadOnlyDictionary<string, Entry>> _instance;
        public WebpackManifest(EpiCommonUtilsOptions options, IWebHostingEnvironment webHostingEnvironment)
        {
            _options = options;
            _webHostingEnvironment = webHostingEnvironment;
            _instance = new Lazy<IReadOnlyDictionary<string, Entry>>(Load);
        }

        public string ManifestPath => _options.WebpackManifestPath;

        public Entry GetEntry(string key)
        {
            if (_instance.Value.TryGetValue(key, out var value) == false)
            {
                throw new InvalidOperationException($"Key {key} not found in manifest file.");
            }

            return value;
        }

        private IReadOnlyDictionary<string, Entry> Load()
        {
            var manifestAbsolutePath = Path.Join(_webHostingEnvironment.WebRootPath, ManifestPath);

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
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            return JsonConvert.DeserializeObject<Dictionary<string, Entry>>(File.ReadAllText(manifestAbsolutePath), serializerSettings);
        }

        public class Entry : Dictionary<string, EntryValue>
        {
            public Entry() : base(StringComparer.InvariantCultureIgnoreCase)
            {
            }

            public string Css => this.GetSingleValue("css");

            public string Js => this.GetSingleValue("js");

            public string Svg => GetSingleValue("svg");

            private string GetSingleValue(string key)
            {
                if (this.TryGetValue(key, out var value) == false)
                {
                    return null;
                }

                return value.FirstOrDefault();
            }
        }

        [JsonConverter(typeof(EntryValueConverter))]
        public class EntryValue : List<string>
        {
            public EntryValue()
            {
            }

            public EntryValue([NotNull] IEnumerable<string> collection) : base(collection)
            {
            }
        }
    }
}
