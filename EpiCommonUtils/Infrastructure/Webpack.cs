using System;
using System.Configuration;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public class Webpack
    {
        private static string _webpackDevServerUrl;

        public static string WebpackDevServerUrl
        {
            get
            {
                if (_webpackDevServerUrl != null)
                {
                    return _webpackDevServerUrl;
                }

                _webpackDevServerUrl = ConfigurationManager.AppSettings["webpack:devServerUrl"];

                return _webpackDevServerUrl ?? "http://localhost:8080/dist/";
            }
            
            set => _webpackDevServerUrl = value;
        }
        public static string GetStylesheetUrl(string name)
        {
            if (HostingEnvironment.IsLocalDev)
            {
                return WebpackDevServerUrl + name + ".css";
            }

            if (WebpackManifest.Instance.TryGetValue(name, out var entry) == false)
            {
                throw new InvalidOperationException($"WebpackManifestEntry does not exist for '{name}'");
            }

            return entry.Css;
        }

        public static string GetScriptUrl(string name)
        {
            if (HostingEnvironment.IsLocalDev)
            {
                return WebpackDevServerUrl + name + ".js";
            }

            if (WebpackManifest.Instance.TryGetValue(name, out var entry) == false)
            {
                throw new InvalidOperationException($"WebpackManifestEntry does not exist for '{name}'");
            }

            return entry.Js;
        }
    }
}
