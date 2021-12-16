using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Castle.Core.Internal;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc.Html;

namespace Forte.EpiCommonUtils.Infrastructure.HtmlTransformations
{
     public static class HtmlHelperExtensions
    {
        public static MvcHtmlString TransformedXhtmlString(this HtmlHelper html, XhtmlString xhtmlString,
            string xhtmlWrapperClass = null, string blocksWrapperClass = null, IEnumerable<Type> blockTypesToExclude = null)
        {
            if (xhtmlString == null)
            {
                return MvcHtmlString.Empty;
            }

            if (xhtmlWrapperClass != null || blocksWrapperClass != null)
            {
                xhtmlString = WrapContent(xhtmlString, xhtmlWrapperClass, blocksWrapperClass, blockTypesToExclude);
            }

            return TransformHtmlString(html.XhtmlString(xhtmlString).ToHtmlString(), html);
        }
        
        public static XhtmlString WrapContent(XhtmlString xhtmlString, 
            string wrapperClass, 
            string blockWrapperClass, 
            IEnumerable<Type> blockTypesToExclude)
        {
            blockTypesToExclude = blockTypesToExclude?.ToList() ?? new List<Type>();
            var textWrapperStart = new StaticFragment($"<div class=\"{wrapperClass}\">");
            var textWrapperEnd = new StaticFragment("</div>");
            var blockWrapperStart = blockWrapperClass != null
                ? new StaticFragment($"<section class=\"{blockWrapperClass}\">")
                : new StaticFragment("");
            var blockWrapperEnd = blockWrapperClass != null
                ? new StaticFragment("</section>")
                : new StaticFragment("");
            
            xhtmlString = xhtmlString.CreateWritableClone();

            var isNextBlock = xhtmlString.Fragments.LastOrDefault() is ContentFragment;
            xhtmlString.Fragments.Add(isNextBlock
                ? blockWrapperEnd
                : textWrapperEnd);
            
            for (var i = xhtmlString.Fragments.Count-3; i >= 0 ; i--)
            {
                var currentFragment = xhtmlString.Fragments[i];
                var isCurrentBlock = currentFragment is ContentFragment;

                if (!isCurrentBlock && string.IsNullOrWhiteSpace(currentFragment.InternalFormat))
                    continue;

                if (isCurrentBlock && blockTypesToExclude.Any(t => t.IsInstanceOfType(((ContentFragment) currentFragment).GetContent())))
                    continue;

                if (isCurrentBlock)
                {
                    xhtmlString.Fragments.Insert(i+1, isNextBlock ? blockWrapperStart : textWrapperStart);
                    xhtmlString.Fragments.Insert(i+1, blockWrapperEnd);
                }
                else if(isNextBlock)
                {
                    xhtmlString.Fragments.Insert(i+1, blockWrapperStart);
                    xhtmlString.Fragments.Insert(i+1, textWrapperEnd);
                }

                isNextBlock = isCurrentBlock;
            }

            xhtmlString.Fragments.Insert(0, isNextBlock ? blockWrapperStart : textWrapperStart);
            return xhtmlString;
        }
        
        private static MvcHtmlString TransformHtmlString(string htmlString, HtmlHelper html)
        {
            var htmlTransformations = ServiceLocator.Current.GetAllInstances<IHtmlTransformation>()
                .OrderBy(t => t.GetType().GetAttribute<HtmlTransformationAttribute>()?.Order);
            
            return TransformHtmlString(htmlString, html, htmlTransformations);
        }

        private static MvcHtmlString TransformHtmlString(string htmlString, HtmlHelper html, IEnumerable<IHtmlTransformation> transformations)
        {
            if (string.IsNullOrEmpty(htmlString))
            {
                return new MvcHtmlString(string.Empty);
            }
            
            var parser = new HtmlParser();
            var document = parser.ParseDocument("<html><body></body></html>");
            var htmlFragment = parser.ParseFragment(htmlString, document.Body);
            if (htmlFragment.Any() == false)
            {
                return new MvcHtmlString(string.Empty);
            }
            
            var htmlElement = (IHtmlElement) htmlFragment.First().Parent;
            foreach (var transformation in transformations)
            {
                transformation.Apply(htmlElement, html);
            }

            return new MvcHtmlString(htmlFragment.ToHtml());
        }
    }
}
