using System;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    /// <summary>
    /// Use it on enum property to force EnumSelectionFactory to send its value to Episerver UI using underlying type.
    /// For example, it might be useful to bypass custom JsonConverter applied to the enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UseUnderlyingTypeAttribute : Attribute
    {
    }
}
