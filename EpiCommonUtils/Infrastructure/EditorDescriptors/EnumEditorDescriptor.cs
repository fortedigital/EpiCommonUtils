using System;
using System.Collections.Generic;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    public class EnumEditorDescriptor<TEnum> : EnumEditorDescriptor<TEnum, EnumSelectionFactory<TEnum>> where TEnum: struct, IConvertible
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
}
