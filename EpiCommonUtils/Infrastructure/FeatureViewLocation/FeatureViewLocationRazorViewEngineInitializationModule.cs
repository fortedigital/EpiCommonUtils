using System.Configuration;
using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace Forte.EpiCommonUtils.Infrastructure.FeatureViewLocation
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class FeatureViewLocationRazorViewEngineInitializationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var disableFeatureViewLocation = ConfigurationManager.AppSettings["epiCommonUtils:disableFeatureViewLocationRazor"];
            if (disableFeatureViewLocation == null || bool.Parse(disableFeatureViewLocation) == false)
            {
                ViewEngines.Engines.Add(new FeatureViewLocationRazorViewEngine());                
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
