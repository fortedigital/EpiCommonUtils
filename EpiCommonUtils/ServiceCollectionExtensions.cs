using System;
using Forte.EpiCommonUtils.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Forte.EpiCommonUtils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEpiCommonUtils(this IServiceCollection services, Action<EpiCommonUtilsOptions> configurationAction = null)
        {
            var epiCommonUtilsOptions = new EpiCommonUtilsOptions
            {
                WebpackManifestPath = "/dist/webpack-assets.json",
                WebpackDevServerUrl = "http://localhost:8080/dist/",
            };

            configurationAction?.Invoke(epiCommonUtilsOptions);

            services.AddSingleton(epiCommonUtilsOptions);
            services.AddSingleton<WebpackManifest>();
            services.AddSingleton<Webpack>();

            return services;
        }
    }
}
