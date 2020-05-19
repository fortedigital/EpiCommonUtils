using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.ServiceLocation;

namespace Forte.EpiCommonUtils.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class XHtmlStringAllowedBlockTypes : ValidationAttribute
    {
#pragma warning disable 649
        private readonly Injected<IContentLoader> _contentLoader;
#pragma warning restore 649
        private readonly Type[] _allowedTypes;
        
        public XHtmlStringAllowedBlockTypes(params Type[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
        }
 
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (context.ObjectInstance is IContentData contentData && contentData.Property[context.MemberName].Value is XhtmlString)
            {
                var richTextProperty = (XhtmlString)contentData.Property[context.MemberName].Value;
 
                foreach (var stringFragment in richTextProperty.Fragments.Where(x => x is ContentFragment))
                {
                    var fragment = (ContentFragment) stringFragment;
                    var content = _contentLoader.Service.Get<IContentData>(fragment.ContentLink);
 
                    foreach (var allowedType in _allowedTypes)
                    {
                        if (allowedType.IsInstanceOfType(content) == false)
                        {
                            return new ValidationResult($"You cannot add {content.GetOriginalType().Name} to {context.MemberName}");
                        }
                    }
                }
            }
 
            return ValidationResult.Success;
        }
    }
}