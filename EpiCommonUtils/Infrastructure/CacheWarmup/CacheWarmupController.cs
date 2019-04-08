using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace Forte.EpiCommonUtils.Infrastructure.CacheWarmup
{
    public class CacheWarmupController : Controller
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IContentModelUsage _contentModelUsage;
        private readonly IContentRepository _contentRepository;
        private static bool _cacheWarmedUp;

        public CacheWarmupController(IContentTypeRepository contentTypeRepository, IContentModelUsage contentModelUsage, IContentRepository contentRepository)
        {
            _contentTypeRepository = contentTypeRepository;
            _contentModelUsage = contentModelUsage;
            _contentRepository = contentRepository;
        }

        public async Task<ActionResult> Warmup()
        {
            if (_cacheWarmedUp) return new EmptyResult();

            var uniquePageTypeUsages = GetUniquePageTypeUsages();

            using (var httpClient = new HttpClient())
            {
                foreach (var pageTypeUsage in uniquePageTypeUsages)
                {
                    var url = pageTypeUsage.ContentLink.GetContentExternalUrl(pageTypeUsage.Language);
                    await httpClient.GetAsync(url);
                }                
            }

            _cacheWarmedUp = true;
            
            return new EmptyResult();
        }

        private IEnumerable<PageData> GetUniquePageTypeUsages()
        {
            var types = _contentTypeRepository.List().OfType<PageType>();

            foreach (var type in types)
            {
                var contentType = _contentTypeRepository.Load(type.ModelType);
                if (contentType == null) continue;

                var usages = _contentModelUsage.ListContentOfContentType(contentType)
                    .Select(i => _contentRepository.Get<PageData>(i.ContentLink))
                    .ToList();

                var usageFiltered = FilterForVisitor.Filter(usages).FirstOrDefault();
                if (usageFiltered == null) continue;

                var pageData = usages.First(u => u.ContentLink == usageFiltered.ContentLink);

                yield return pageData;
            }
        }
    }
    
    [InitializableModule]
    public class CacheWarmupRouteInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            RouteTable.Routes.MapRoute(
                null,
                "cache-warmup/warmup",
                new { controller = "CacheWarmup", action = "Warmup" });
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    } 
}
