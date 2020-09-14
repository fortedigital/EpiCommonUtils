using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Shell.ObjectEditing;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    public class NullableEnumSelectionFactory<TEnum> : EnumSelectionFactory<TEnum> where TEnum : struct, IConvertible
    {
        public override IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new[]{new SelectItem
            {
                Text = "",
                Value = null,
            }}.Concat(base.GetSelections(metadata));
        }
    }
}