using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;

namespace Forte.EpiCommonUtils.Infrastructure.Translation
{
    public class TranslationFileGenerator
    {
        private readonly StreamWriter _output;

        public TranslationFileGenerator(StreamWriter output)
        {
            this._output = output;

        }

        private void WriteLine(string line)
        {
            this._output.WriteLine(line);
        }

      
        /// <summary>
        /// Generates code for all XML files in specified directory. Code file will have a specified namespace and root class will have a specified name
        /// </summary>
        /// <param name="sourceDirectory">Folder to look translation files at</param>
        /// <param name="rootNamespace">Generated code file namespace</param>
        /// <param name="rootClassName">Generated code file root class name</param>
        public void Generate(string sourceDirectory, string rootNamespace, string rootClassName)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                this.WriteLine(
                    $"// Directory not found: {sourceDirectory}. Please provide a directory that contains XML translation files.");
                return;
            }

            var directoryInfo = new DirectoryInfo(sourceDirectory);
            var fileInfos = directoryInfo.GetFiles("*.xml");
            if (fileInfos.Length <= 0)
            {
                this.WriteLine(
                    $"// Specified directory doesn't contain translation files: {sourceDirectory}. Please provide a directory that contains XML translation files.");
                return;
            }

            var sw = Stopwatch.StartNew();
            var provider = new XmlLocalizationProvider();
            // Load all resources files from directory
            foreach (var fileInfo in fileInfos)
            {
                using (var stream = fileInfo.OpenRead())
                {
                    provider.Load(stream);
                }
            }

            var resourceList = this.GetResourcesList(provider, rootClassName);
            this.RenderResources(resourceList, rootNamespace);

            sw.Stop();
            this.WriteLine("// Code generation time (msec):" + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Finds all unsafe for the C# member name chars and strings starting with numbers
        /// </summary>
        private readonly Regex _nameReplacementsRegex = new Regex(@"(^\d)|\s+|(\W)",
                                                                  RegexOptions.IgnoreCase
                                                                  | RegexOptions.Singleline
                                                                  | RegexOptions.CultureInvariant
                                                                  | RegexOptions.Compiled);

        /// <summary>
        /// Cleanups duplicate underscores with a single one
        /// </summary>
        private readonly Regex _nameCleanupRegex = new Regex(@"_+",
                                                             RegexOptions.IgnoreCase
                                                             | RegexOptions.Singleline
                                                             | RegexOptions.CultureInvariant
                                                             | RegexOptions.Compiled);

        /// <summary>
        /// Processes translation keys containing attribute filtering XPath expression
        /// </summary>
        private readonly Regex _selectByAttributeRegex = new Regex(
            @"(?<nodeName>\w+)\[\s*@\s*(?<propertyName>\w+)\s*=\s*'(?<propertyValue>[^'\r\n]+)'\s*\]",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.ExplicitCapture
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

     
        /// <summary>
        /// Gets a resources list from specified localization provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="rootClassName"> </param>
        /// <returns></returns>
        private List<Resource> GetResourcesList(LocalizationProvider provider, string rootClassName)
        {
            var resourceList = new List<Resource>();
            var keyHandler = new ResourceKeyHandler();
            foreach (CultureInfo culture in provider.AvailableLanguages)
            {
                foreach (ResourceItem resource in provider.GetAllStrings("", new string[] { }, culture))
                {
                    var normalized = this.ProcessNormalizedKeys(keyHandler.NormalizeKey(resource.Key), rootClassName);
                    if (normalized.Count == 0)
                    {
                        continue;
                    }

                    var resourceClass = new Resource
                    {
                        Language = culture.ToString(),
                        Key = resource.Key,
                        Value = resource.Value,
                        NormalizedKey = normalized,
                        Level = normalized.Count
                    };
                    resourceList.Add(resourceClass);
                }
            }
            return resourceList;
        }

        /// <summary>
        /// Prepares raw values on normalized key to be used as C# member names
        /// </summary>
        /// <param name="normalized"></param>
        /// <param name="rootClassName"> </param>
        /// <returns></returns>
        private List<string> ProcessNormalizedKeys(IEnumerable<string> normalized, string rootClassName)
        {
            var result = new List<String> { rootClassName };
            foreach (string normValue in normalized)
            {
                if (!normValue.Contains("@"))
                {
                    result.Add(this.GetSafeName(normValue));
                    continue;
                }

                // if there is attribute selection XPath in a key - add attribute value as a key below the node - so it will be rendered as a nested class
                MatchCollection matches = this._selectByAttributeRegex.Matches(normValue);
                if (matches.Count == 0)
                {
                    result.Add(this.GetSafeName(normValue));
                    continue;
                }
                foreach (Match match in matches)
                {
                    string node = match.Groups["nodeName"].Value;
                    string value = match.Groups["propertyValue"].Value;
                    result.Add(this.GetSafeName(node));
                    result.Add(this.GetSafeName(value));
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a string that can be used as a C# member name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetSafeName(string name)
        {
            name = this._nameReplacementsRegex.Replace(name, "_$1");
            name = this._nameCleanupRegex.Replace(name, "_"); // removes multiple undrscores with a single one 
            return name;
        }

        /// <summary>
        /// Renders resources from specified resources list
        /// </summary>
        /// <param name="resourceList"></param>
        /// <param name="rootNamespace"></param>
        private void RenderResources(IEnumerable<Resource> resourceList, string rootNamespace)
        {
            this.WriteLine("using EPiServer.Framework.Localization;");
            this.WriteLine($"namespace {rootNamespace}");
            this.WriteLine("{");
            this.RenderClasses(resourceList, String.Empty, 0);
            this.WriteEndBracket(0);
        }

        /// <summary>
        /// Recursivly renders C# classes with strongly typed resource  
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="parentClassName"></param>
        /// <param name="level"></param>
        private void RenderClasses(IEnumerable<Resource> resources, string parentClassName, int level)
        {
            var resourceGroupig = resources.Where(r => r.NormalizedKey.Count > level)
                .GroupBy(r => r.NormalizedKey[level], r => r).Select(r => new { ContainingClassName = r.Key, Resources = r });

            foreach (var resource in resourceGroupig)
            {
                this.WriteClass(resource.ContainingClassName, parentClassName, level + 1);

                List<Resource> properties = resource.Resources.Where(r => r.NormalizedKey.Count <= level + 2).ToList();
                foreach (var property in properties.GroupBy(p => p.NormalizedKey[level + 1]).Select(p => new { PropertyName = p.Key, Values = p }))
                {
                    this.WriteProperty(resource.ContainingClassName, property.PropertyName, property.Values.ToList(), level + 2);
                }
                this.RenderClasses(resource.Resources.Except(properties), resource.ContainingClassName, level + 1);
                this.WriteEndBracket(level + 1);
            }
        }

        /// <summary>
        /// Helper for rendering a C# class 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="parentClassName"></param>
        /// <param name="level"></param>
        private void WriteClass(string className, string parentClassName, int level)
        {
            if (className.Equals(parentClassName))
            {
                className = "_" + className;
            }
            this.WriteLineIndent(level, $"public static partial class @{FirstLetterToUpperCase(className)}");
            this.WriteLineIndent(level, "{");
        }

        private static string FirstLetterToUpperCase(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        /// <summary>
        /// Helper for rendering a property of strongly typed resource class with translation values as property comments
        /// </summary>
        /// <param name="className"></param>
        /// <param name="propertyName"></param>
        /// <param name="values"></param>
        /// <param name="level"></param>
        private void WriteProperty(string className, string propertyName, IList<Resource> values, int level)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                return;
            }
            this.WriteLineIndent(level, "///<summary>");
            foreach (Resource resource in values)
            {
                this.WriteLineIndent(level, 
                    $"/// <para>{resource.Language}: {resource.Value.Replace("\r", " ").Replace("\n", " ")}</para>");
            }
            this.WriteLineIndent(level, "///</summary>");
            if (propertyName.Equals(className))
            {
                propertyName = "_" + propertyName;
            }
            this.WriteLineIndent(level,
                $"public static string @{FirstLetterToUpperCase(propertyName)}{{ get {{ return LocalizationService.Current.GetString(\"{values.First().Key}\"); }} }}"); // taking a value of the first key, since all keys should be the same for all languages
        }

        /// <summary>
        /// Helper for rendering line indents
        /// </summary>
        /// <param name="indentLevel"></param>
        /// <param name="value"></param>
        private void WriteLineIndent(int indentLevel, string value)
        {
            this.WriteLine($"{Tabs(indentLevel)}{value}");
        }

        /// <summary>
        /// Helper for rendering closing courly braces
        /// </summary>
        /// <param name="level"></param>
        private void WriteEndBracket(int level)
        {
            this.WriteLineIndent(level, "}");
        }

        /// <summary>
        /// Helper for rendering a specified amount for tabs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static string Tabs(int count)
        {
            return new String('\t', count);
        }

        /// <summary>
        /// Resource item container class
        /// </summary>
        private class Resource
        {
            public string Language { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public int Level { get; set; }
            public List<string> NormalizedKey { get; set; }
        }
    }
}