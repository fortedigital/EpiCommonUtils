using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Forte.EpiCommonUtils.Infrastructure.HtmlTransformations
{
    public interface IHtmlTransformation
    {
        void Apply(IHtmlElement htmlElement, IHtmlHelper html);
    }
}
