using System;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    public class NullableEnumEditorDescriptor<TEnum> : EnumEditorDescriptor<TEnum, NullableEnumSelectionFactory<TEnum>> where TEnum : struct, IConvertible
    {
    }
}
