using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    public class EnumEditorDescriptor<TEnum> : EditorDescriptor where TEnum: struct, IConvertible
    {
        public override void ModifyMetadata(
            ExtendedMetadata metadata, 
            IEnumerable<Attribute> attributes)
        {
            SelectionFactoryType = typeof(EnumSelectionFactory);

            ClientEditingClass = 
                "epi-cms/contentediting/editors/SelectionEditor";

            base.ModifyMetadata(metadata, attributes);
        }

        public class EnumSelectionFactory : ISelectionFactory
        {
            public IEnumerable<ISelectItem> GetSelections(
                ExtendedMetadata metadata)
            {
                var values = Enum.GetValues(typeof(TEnum));
                foreach (var value in values)
                {
                    var enumType = typeof(TEnum);
                    var memberInfos = enumType.GetMember(value.ToString());
                    if (memberInfos[0].CustomAttributes.Any(x => x.AttributeType == typeof(EpiUnselectableAttribute)) ==
                        false)
                    {
                        yield return new SelectItem
                        {
                            Text = ((TEnum) value).GetLocalizedName(),
                            Value = value
                        };
                    }
                }
            }
        }
    }
}