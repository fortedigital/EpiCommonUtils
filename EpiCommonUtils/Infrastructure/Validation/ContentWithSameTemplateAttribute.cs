using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Web;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Forte.EpiCommonUtils.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ContentWithSameTemplateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;
            
            var contentArea = (ContentArea) value;

            var isValid = contentArea.Items.Select(this.GetTemplate).Where(x => x != null).Select(x => x.TemplateType)
                .Distinct().Count() <= 1;

            return isValid
                ? ValidationResult.Success
                : new ValidationResult("Content on the list must be of the same type");
        }

        private TemplateModel GetTemplate(ContentAreaItem contentAreaItem)
        {
            return _contentLoader.Service.TryGet<IContent>(contentAreaItem.ContentLink, out var content)
                ? _templateResolver.Service.Resolve(content, TemplateTypeCategories.MvcPartial, "")
                : null;
        }
        
#pragma warning disable 649
        private Injected<ITemplateResolver> _templateResolver;
        private Injected<IContentLoader> _contentLoader;
#pragma warning restore 649
    }
}