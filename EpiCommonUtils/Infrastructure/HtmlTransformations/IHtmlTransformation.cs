using System.Web.Mvc;
using AngleSharp.Html.Dom;

namespace Forte.EpiCommonUtils.Infrastructure.HtmlTransformations
{
    public interface IHtmlTransformation
    {
        void Apply(IHtmlElement htmlElement, HtmlHelper html);
    }
}
