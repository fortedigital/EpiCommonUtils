using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer.Core;
using EPiServer.SpecializedProperties;

namespace Forte.EpiCommonUtils.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class MaxItemsAllowedAttribute: ValidationAttribute
    {
        private readonly int _maximumItemsAllowed;

        public MaxItemsAllowedAttribute(int maximumItemsAllowed)
        {
            this._maximumItemsAllowed = maximumItemsAllowed;
        }

        public override bool IsValid(object @object)
        {
            if (@object is ContentArea contentArea)
            {
                return contentArea.Items == null || contentArea.Items.Count <= this._maximumItemsAllowed;
            }

            if (@object is LinkItemCollection lic)
            {
                return lic.Count <= this._maximumItemsAllowed;
            }

            if (@object is IEnumerable<object> enumerable)
            {
                return enumerable.Count() <= this._maximumItemsAllowed;
            }

            return true;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var validationResult = base.IsValid(value, validationContext);

            if (string.IsNullOrWhiteSpace(validationResult?.ErrorMessage) == false)
            {
                validationResult.ErrorMessage = $"{validationContext.DisplayName} cannot have more items than {this._maximumItemsAllowed}";
            }    

            return validationResult;
        }
    }
}
