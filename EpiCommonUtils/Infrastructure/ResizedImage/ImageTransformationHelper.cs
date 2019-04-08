using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Forte.EpiCommonUtils.Infrastructure.Model;
using JetBrains.Annotations;

namespace Forte.EpiCommonUtils.Infrastructure.ResizedImage
{
    public static class ImageTransformationHelper
    {
        public static UrlBuilder ResizedImageUrl(this HtmlHelper helper, ContentReference image, int width, int? height = null, [CanBeNull] ExtendedPictureProfile profile = null)
        {
            var baseUrl = ResolveImageUrl(image);
            
            return BuildResizedImageUrl(baseUrl, width, height, profile);
        }
        
        private static string ResolveImageUrl(ContentReference image)
        {
            return UrlResolver.Current.GetUrl(image);
        }
        
        private static UrlBuilder BuildResizedImageUrl(string imageUrl, int? width, int? height, ExtendedPictureProfile profile)
        {
            var target = new UrlBuilder(imageUrl);

            if (width.HasValue)
                target.Width(width.Value);

            if (height.HasValue)
                target.Height(height.Value);

            if (profile == null) return target;
            
            if (profile.MaxHeight.HasValue)
                target.MaxHeight(profile.MaxHeight.Value);

            if (profile.Mode != ScaleMode.Default)
                target.Mode(profile.Mode);

            if (profile.Quality.HasValue)
                target.Quality(profile.Quality.Value);

            return target;
        }
        
        public static MvcHtmlString ResizedPictureExtended(
            this HtmlHelper helper,
            ContentReference image,
            ExtendedPictureProfile profile,
            string fallbackUrl = null,
            string additionalCssClass = null)
        {
            var isEmpty = ContentReference.IsNullOrEmpty(image);
            var baseUrl = isEmpty ? fallbackUrl : ResolveImageUrl(image);

            var alternateText =
                ServiceLocator.Current.GetInstance<IContentLoader>().TryGet<ImageBase>(image, out var content)
                    ? content.Description
                    : string.Empty;

            return GenerateResizedPicture(baseUrl, profile, alternateText, additionalCssClass);
        }
        
        private static MvcHtmlString GenerateResizedPicture(string imageBaseUrl,
            ExtendedPictureProfile profile, string alternateText, string additionalCssClass)
        {
            var urlBuilder = BuildResizedImageUrl(imageBaseUrl, profile.DefaultWidth, null, profile);
            var sourceSets = profile.SrcSetWidths.Select(w => FormatSourceSet(imageBaseUrl, w, profile)).ToArray();

            return GeneratePictureElement(profile, urlBuilder.ToString(), sourceSets, alternateText,
                additionalCssClass);
        }
        
        private static MvcHtmlString GeneratePictureElement(ExtendedPictureProfile profile,
            string imgUrl,
            string[] sourceSets,
            string alternateText,
            string additionalCssClass)
        {
            var sourceElement = new TagBuilder("source")
            {
                Attributes = {{"srcset", string.Join(", ", sourceSets)}}
            };

            if (profile.SrcSetSizes != null && profile.SrcSetSizes.Length != 0)
                sourceElement.Attributes.Add("sizes", string.Join(", ", profile.SrcSetSizes));

            var pictureElement = new TagBuilder("picture")
            {
                InnerHtml = sourceElement.ToString(TagRenderMode.SelfClosing) +
                            CreateImgElement(imgUrl, alternateText, additionalCssClass)
                                .ToString(TagRenderMode.SelfClosing)
            };

            return new MvcHtmlString(pictureElement.ToString());
        }
        
        private static TagBuilder CreateImgElement(string imgUrl, string alternateText, string additionalCssClass)
        {
            var tagBuilder = new TagBuilder("img")
            {
                Attributes =
                {
                    {"src", imgUrl},
                    {"alt", alternateText},
                    {"data-object-fit", "cover"},
                    {"data-object-position", "center"}
                }
            };
            if (!string.IsNullOrEmpty(additionalCssClass)) tagBuilder.Attributes.Add("class", additionalCssClass);

            return tagBuilder;
        }
        
        private static string FormatSourceSet(string imageUrl, int width, ExtendedPictureProfile profile)
        {
            var url = BuildResizedImageUrl(imageUrl, width, null, profile);
            return $"{url} {width}w";
        }
    }
}
