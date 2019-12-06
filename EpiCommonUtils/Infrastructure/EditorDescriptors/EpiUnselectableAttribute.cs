using System;

namespace Forte.EpiCommonUtils.Infrastructure.EditorDescriptors
{
    /// <summary>
    /// Use it on enum field to make it ignored and unselectable in Episerver edit mode 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EpiUnselectableAttribute: Attribute
    {
    }
}