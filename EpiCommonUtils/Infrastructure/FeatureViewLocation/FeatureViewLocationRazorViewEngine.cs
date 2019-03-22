using System.Web.Mvc;
using JetBrains.Annotations;

[assembly: AspMvcViewLocationFormat("~/Features/Pages/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/Pages/{1}/{1}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/{1}/{0}.cshtml")]
namespace Forte.EpiCommonUtils.Infrastructure.FeatureViewLocation
{
    public class FeatureViewLocationRazorViewEngine : RazorViewEngine
    {
        public FeatureViewLocationRazorViewEngine()
        {
            this.ViewLocationFormats = new[]
            {
                "~/Features/Pages/{1}/{0}.cshtml",
                "~/Features/Pages/{1}/{1}.cshtml",
                "~/Features/{1}/{0}.cshtml"
            };

            this.MasterLocationFormats = new[]
            {
                "~/Features/Layouts/{0}.cshtml",
            };

            this.PartialViewLocationFormats = new[]
            {
                "~/Features/{0}.cshtml", // {0} -> DisplayTemplates\Image
                "~/Features/{1}/{0}.cshtml", // {1} -> 
                "~/Features/Blocks/{1}/{0}.cshtml",
                "~/Features/Blocks/{0}/Index.cshtml",
                "~/Features/Partials/{0}.cshtml",
                "~/Features/Partials/{0}/{0}.cshtml",
                "~/Features/Partials/{1}/{0}.cshtml",
            };
        }
    }
}
