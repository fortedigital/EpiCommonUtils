using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    public class EnumEditorDescriptor<TEnum> : EnumEditorDescriptor<TEnum, EnumSelectionFactory<TEnum>> where TEnum: struct, IConvertible
    {
    }

    public class NullableEnumEditorDescriptor<TEnum> : EnumEditorDescriptor<TEnum, NullableEnumSelectionFactory<TEnum>> where TEnum : struct, IConvertible
    {
    }

    public class EnumEditorDescriptor<TEnum, TSelectionFactory> : EditorDescriptor where TEnum : struct, IConvertible
    {
        public override void ModifyMetadata(
            ExtendedMetadata metadata, 
            IEnumerable<Attribute> attributes)
        {
            SelectionFactoryType = typeof(TSelectionFactory);

            ClientEditingClass = 
                "epi-cms/contentediting/editors/SelectionEditor";

            base.ModifyMetadata(metadata, attributes);
        }
    }
    
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
    
    public class EnumSelectionFactory<TEnum> : ISelectionFactory where TEnum: struct, IConvertible
    {
        public virtual IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var values = Enum.GetValues(typeof(TEnum));
            foreach (var value in values)
            {
                var enumType = typeof(TEnum);
                var memberInfos = enumType.GetMember(value.ToString());
                if (memberInfos[0].CustomAttributes.Any(x => x.AttributeType == typeof(EpiUnselectableAttribute)) == false)
                {
                    yield return new SelectItem
                    {
                        Text = GetEnumText((TEnum) value),
                        Value = value
                    };
                }
            }
        }

        private static string GetEnumText(TEnum value)
        {
            var localizedEnumText = value.GetLocalizedName();

            if (string.IsNullOrEmpty(localizedEnumText) == false)
            {
                return localizedEnumText;
            }

            var displayAttributeText = GetDisplayFromAttribute(value);

            if (string.IsNullOrEmpty(displayAttributeText) == false)
            {
                return displayAttributeText;
            }

            return Enum.GetName(typeof(TEnum), value);
        }
        
        private static string GetDisplayFromAttribute(object value)
        {
            var type = value.GetType();
            var memberInfos = type.GetMember(value.ToString());
            var displayAttribute = memberInfos[0].GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name;
        }
    }
}
