using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer.Core;
using EPiServer.SpecializedProperties;

namespace Forte.EpiCommonUtils.Infrastructure.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AllowedItemsCount : ValidationAttribute
    {
        private readonly int _max;
        private readonly int _min;

        public AllowedItemsCount(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public override bool IsValid(object item)
        {
            if (item is ContentArea contentArea)
            {
                if (contentArea.Items != null)
                    return contentArea.Items.Count <= _max && contentArea.Items.Count >= _min;
                return true;
            }

            if (item is LinkItemCollection linkItemCollection)
                return linkItemCollection.Count <= _max && linkItemCollection.Count >= _min;
            if (item is IEnumerable<object> source)
            {
                var count = source.Count();
                return count <= _max && count >= _min;
            }

            return true;
        }

        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            var validationResult = base.IsValid(value, validationContext);
            if (!string.IsNullOrWhiteSpace(validationResult?.ErrorMessage))
                validationResult.ErrorMessage = string.Format("{0} must have between {1} and {2} items",
                    validationContext.DisplayName, _min, _max);
            return validationResult;
        }
    }
}