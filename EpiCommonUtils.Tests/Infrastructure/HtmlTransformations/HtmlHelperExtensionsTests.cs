using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.Web;
using Forte.EpiCommonUtils.Infrastructure.HtmlTransformations;
using Moq;
using NUnit.Framework;

namespace EpiCommonUtils.Tests.Infrastructure.HtmlTransformations
{
    [TestFixture]
    public class XHtmlWrappingTests
    {
        private static ContentFragment CreateContentFragmentMock()
        {
            return new ContentFragment(new Mock<IContentLoader>().Object,
                new Mock<ISecuredFragmentMarkupGenerator>().Object, new Mock<DisplayOptions>().Object,
                new Mock<IPublishedStateAssessor>().Object, new Mock<IContextModeResolver>().Object,
                new Mock<IContentAccessEvaluator>().Object, new Dictionary<string, object>());
        }

        [Test, TestCaseSource(nameof(TestCasesWithTextWrappers))]
        public void TextContentIsWrapped(StringFragmentCollection stringFragments, StringFragmentCollection expected)
        {
            var xhtml = new FakeXHtmlString(stringFragments);

            var result = HtmlHelperExtensions.WrapContent(xhtml, "wrapper", null, null);

            CollectionAssert.AreEqual(expected, result.Fragments, new FragmentComparer(), 
                this.FormatError(expected, result.Fragments));
        }
        
        [Test, TestCaseSource(nameof(TestCasesWithTextAndBlockWrappers))]
        public void TextAndBlockContentIsWrapped(StringFragmentCollection stringFragments, StringFragmentCollection expected)
        {
            var xhtml = new FakeXHtmlString(stringFragments);

            var result = HtmlHelperExtensions.WrapContent(xhtml, "wrapper", "blockWrapper", null);

            CollectionAssert.AreEqual(expected, result.Fragments, new FragmentComparer(), 
                this.FormatError(expected, result.Fragments));
        }
        
        private string FormatError(StringFragmentCollection expected, StringFragmentCollection resultFragments)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Expected: {expected.Aggregate("", (x, y) => x + FormatSingle(y))}");
            stringBuilder.AppendLine($"  Actual:   {resultFragments.Aggregate("", (x, y) => x + FormatSingle(y))}");
            return stringBuilder.ToString();
        }

        private static string FormatSingle(IStringFragment stringFragment)
        {
            if (stringFragment is ContentFragment)
            {
                return "{C}";
            }

            return stringFragment.GetViewFormat();
        }

        private static IEnumerable<TestCaseData> TestCasesWithTextWrappers() => TestCases(true, false);
        private static IEnumerable<TestCaseData> TestCasesWithTextAndBlockWrappers() => TestCases(true, true);
        

        private static IEnumerable<TestCaseData> TestCases(bool includeTextWrappers, bool includeBlockWrappers)
        {
            var textWrapperStart = includeTextWrappers
                ? new StaticFragment("<div class=\"wrapper\">")
                : new StaticFragment("");
            var textWrapperEnd = includeTextWrappers
                ? new StaticFragment("</div>")
                : new StaticFragment("");
            var blockWrapperStart = includeBlockWrappers
                ? new StaticFragment("<section class=\"blockWrapper\">")
                : new StaticFragment("");
            var blockWrapperEnd = includeBlockWrappers
                ? new StaticFragment("</section>")
                : new StaticFragment("");

            var text = new StaticFragment("<p>text</p>");
            var contentFragment = CreateContentFragmentMock();
            
            var stringFragments = new StringFragmentCollection(new List<IStringFragment>
            {
                text
            });
            var expected = new StringFragmentCollection(new List<IStringFragment>
            {
                textWrapperStart,
                text,
                textWrapperEnd
            });
            yield return new TestCaseData(stringFragments, expected).SetName(
                "1");
            
            stringFragments = new StringFragmentCollection(new List<IStringFragment>
            {
                text,
                contentFragment,
                contentFragment,
                text,
                contentFragment
            });
            expected = new StringFragmentCollection(new List<IStringFragment>
            {
                textWrapperStart,
                text,
                textWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                textWrapperStart,
                text,
                textWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd
            });
            yield return new TestCaseData(stringFragments, expected).SetName(
                "2");
            
            stringFragments = new StringFragmentCollection(new List<IStringFragment>
            {
                contentFragment,
                contentFragment,
                contentFragment
            });
            expected = new StringFragmentCollection(new List<IStringFragment>
            {
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd
            });

            yield return new TestCaseData(stringFragments, expected).SetName(
                "3");
            
            stringFragments = new StringFragmentCollection(new List<IStringFragment>
            {
                contentFragment,
                text,
                contentFragment,
                contentFragment
            });
            expected = new StringFragmentCollection(new List<IStringFragment>
            {
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                textWrapperStart,
                text,
                textWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd
            });
            yield return new TestCaseData(stringFragments, expected).SetName("4");
            
            var emptyStringFragment = new StaticFragment("\n   \r\n");
            stringFragments = new StringFragmentCollection(new List<IStringFragment>
            {
                contentFragment,
                emptyStringFragment,
                contentFragment
            });
            expected = new StringFragmentCollection(new List<IStringFragment>
            {
                blockWrapperStart,
                contentFragment,
                blockWrapperEnd,
                blockWrapperStart,
                emptyStringFragment,
                contentFragment,
                blockWrapperEnd,
            });
            
            yield return new TestCaseData(stringFragments, expected).SetName("Whitespace HTML block is not wrapped");

        }

        private class FakeXHtmlString : XhtmlString
        {
            public FakeXHtmlString(StringFragmentCollection fragments)
            {
                this.Fragments = fragments;
            }

            public override StringFragmentCollection Fragments { get; }

            protected override XhtmlString CreateWriteableCloneImplementation()
            {
                return this;
            }
        }
    }

    public class FragmentComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is ContentFragment && y is ContentFragment)
            {
                return 0;
            }

            if (x is StaticFragment static1 && y is StaticFragment static2)
            {
                return String.Compare(static1.InternalFormat, static2.InternalFormat, StringComparison.Ordinal);
            }

            return -1;
        }
    }
}
