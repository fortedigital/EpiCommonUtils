using System;
using Forte.EpiCommonUtils.Infrastructure;
using Forte.EpiCommonUtils.Infrastructure.Policies;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Forte.EpiCommonUtils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEpiCommonUtils(this IServiceCollection services,
            Action<EpiCommonUtilsOptions> configurationAction = null, bool indexAsDefaultAction = true)
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

            if (indexAsDefaultAction)
            {
                services.AddSingleton<MatcherPolicy, ActionMatcherPolicy>();
            }

            return services;
        }
    }
}