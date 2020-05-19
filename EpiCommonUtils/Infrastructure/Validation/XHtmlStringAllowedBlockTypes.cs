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
        private readonly IContentLoader contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        
        private readonly Type[] allowedTypes;
        
        public XHtmlStringAllowedBlockTypes(Type[] allowedTypes)
        {
            this.allowedTypes = allowedTypes;
        }
 
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (context.ObjectInstance is IContentData contentData && contentData.Property[context.MemberName].Value is XhtmlString)
            {
                var richTextProperty = (XhtmlString)contentData.Property[context.MemberName].Value;
 
                foreach (var stringFragment in richTextProperty.Fragments.Where(x => x is ContentFragment))
                {
                    var fragment = (ContentFragment) stringFragment;
                    var content = this.contentLoader.Get<IContentData>(fragment.ContentLink);
 
                    foreach (var allowedType in this.allowedTypes)
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