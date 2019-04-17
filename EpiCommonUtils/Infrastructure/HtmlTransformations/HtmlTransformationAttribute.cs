using System;

namespace Forte.EpiCommonUtils.Infrastructure.HtmlTransformations
{
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HtmlTransformationAttribute : Attribute
    {
        public int Order { get; set; } = 100;
    }
}
