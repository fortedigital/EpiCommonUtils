using System;
using System.Globalization;
using System.Linq;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public static class ContentReferenceExtensions
    {
        public static string GetContentExternalUrl(this ContentReference contentLink, CultureInfo contentLanguage, bool absoluteUrl = true)
        {
            var result = ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(
                contentLink,
                contentLanguage.Name,
                new VirtualPathArguments
                {
                    ContextMode = ContextMode.Default,
                    ForceCanonical = absoluteUrl
                });

            // HACK: Temprorary fix until GetUrl and ForceCanonical works as expected,
            // i.e returning an absolute URL even if there is a HTTP context that matches the content's site definition and host.
            if (absoluteUrl)
            {
                Uri relativeUri;

                if (Uri.TryCreate(result, UriKind.RelativeOrAbsolute, out relativeUri))
                {
                    if (!relativeUri.IsAbsoluteUri)
                    {
                        var siteDefinitionResolver = ServiceLocator.Current.GetInstance<ISiteDefinitionResolver>();
                        var siteDefinition = siteDefinitionResolver.GetByContent(contentLink, true, true);
                        var hosts = siteDefinition.GetHosts(contentLanguage, true).ToList();

                        var host = hosts.FirstOrDefault(h => h.Type == HostDefinitionType.Primary) 
                                   ?? hosts.FirstOrDefault(h => h.Type == HostDefinitionType.Undefined);

                        var basetUri = siteDefinition.SiteUrl;

                        if (host != null && host.Name.Equals("*") == false)
                        {
                            // Try to create a new base URI from the host with the site's URI scheme. Name should be a valid
                            // authority, i.e. have a port number if it differs from the URI scheme's default port number.
                            Uri.TryCreate(siteDefinition.SiteUrl.Scheme + "://" + host.Name, UriKind.Absolute, out basetUri);
                        }

                        var absoluteUri = new Uri(basetUri, relativeUri);

                        return absoluteUri.AbsoluteUri;
                    }
                }
            }

            return result;
        }
    }
}