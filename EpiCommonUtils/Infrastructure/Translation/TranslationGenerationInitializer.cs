using System;
using System.IO;
using System.Linq;

namespace Forte.EpiCommonUtils.Infrastructure.Translation
{
    public static class TranslationGenerationInitializer
    {
        public static void Generate(string sourceDirectory, string targetDirectory, string outputClassName = "Translations")
        {
            var rootNamespace = AppDomain.CurrentDomain.BaseDirectory
                .TrimEnd(Path.DirectorySeparatorChar)
                .Split(Path.DirectorySeparatorChar)
                .Last(); 

            var outputFilePath = Path.Combine(targetDirectory, $"{outputClassName}.cs");

            using (var output = new StreamWriter(outputFilePath, false))
            {
                new TranslationFileGenerator(output).Generate(sourceDirectory, rootNamespace, outputClassName);
            }
        }
    }
}