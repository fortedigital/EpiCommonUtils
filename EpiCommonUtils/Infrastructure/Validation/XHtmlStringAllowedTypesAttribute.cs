using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Forte.EpiCommonUtils.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class XHtmlStringAllowedTypesAttribute : ValidationAttribute
    {
        private readonly Type[] _allowedTypes;
        private readonly IContentLoader _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        private readonly IPermanentLinkMapper _linkMapper = ServiceLocator.Current.GetInstance<IPermanentLinkMapper>();

        public XHtmlStringAllowedTypesAttribute(params Type[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (context.ObjectInstance is IContentData contentData &&
                contentData.Property[context.MemberName].Value is XhtmlString)
            {
                var richTextProperty = (XhtmlString) contentData.Property[context.MemberName].Value;

                foreach (var stringFragment in richTextProperty.Fragments.Where(x =>
                    x is ContentFragment || x is UrlFragment))
                {
                    var references = Enumerable.Empty<ContentReference>();
                    var baseType = typeof(object);

                    switch (stringFragment)
                    {
                        case ContentFragment contentFragment:
                            references = new[] {contentFragment.ContentLink};
                            baseType = typeof(ContentData);
                            break;
                        case UrlFragment fragment when fragment.ReferencedPermanentLinkIds?.Any() == true:
                            references = fragment.ReferencedPermanentLinkIds
                                .Select(x => _linkMapper.Find(x))
                                .Select(x => x.ContentReference);
                            baseType = typeof(MediaData);
                            break;
                    }

                    var content = _contentLoader.GetItems(references, CultureInfo.InvariantCulture);

                    var invalidContent = content
                        .Where(x => baseType.IsAssignableFrom(x.GetOriginalType()))
                        .Where(x => _allowedTypes.All(t => t.IsInstanceOfType(x) == false))
                        .ToArray();

                    if (invalidContent.Any())
                    {
                        var invalidContentTypeNames = invalidContent.Select(x => x.GetOriginalType().Name).Distinct();
                        return new ValidationResult(
                            $"You cannot add {string.Join(",", invalidContentTypeNames)} to {context.MemberName}");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}