using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public class Webpack
    {
        private readonly WebpackManifest _webpackManifest;
        private readonly EpiCommonUtilsOptions _epiCommonUtilsOptions;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Webpack(WebpackManifest webpackManifest, EpiCommonUtilsOptions epiCommonUtilsOptions, IWebHostEnvironment webHostEnvironment)
        {
            _webpackManifest = webpackManifest;
            _epiCommonUtilsOptions = epiCommonUtilsOptions;
            _webHostEnvironment = webHostEnvironment;
        }

        public string WebpackDevServerUrl => _epiCommonUtilsOptions.WebpackDevServerUrl;

        public string GetStylesheetUrl(string name)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                return WebpackDevServerUrl + name + ".css";
            }

            var entry = _webpackManifest.GetEntry(name);

            return entry.Css;
        }

        public string GetScriptUrl(string name)
        {
            if (_webHostEnvironment.IsDevelopment())
            {
                return WebpackDevServerUrl + name + ".js";
            }

            var entry = _webpackManifest.GetEntry(name);

            return entry.Js;
        }
    }
}
