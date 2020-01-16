using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Forte.EpiCommonUtils.Jobs
{
    [ScheduledPlugIn(DisplayName = "Delete content types missing properties")]
    public class DeleteMissingPropertiesJob : ScheduledJobBase
    {
        private readonly IContentTypeRepository contentTypeRepository;
        private readonly ContentTypeModelRepository contentTypeModelRepository;
        private readonly IPropertyDefinitionRepository propertyDefinitionRepository;

        public DeleteMissingPropertiesJob(IContentTypeRepository contentTypeRepository,
            ContentTypeModelRepository contentTypeModelRepository,
            IPropertyDefinitionRepository propertyDefinitionRepository)
        {
            this.contentTypeRepository = contentTypeRepository;
            this.contentTypeModelRepository = contentTypeModelRepository;
            this.propertyDefinitionRepository = propertyDefinitionRepository;
        }

        public override string Execute()
        {
            var contentTypes = contentTypeRepository.List();

            var removedPropertyDefinitions = new List<Tuple<string, string>>();
            
            foreach (var contentType in contentTypes)
            {
                var missingProperties = GetContentTypeMissingProperties(contentType);
                foreach (var missingProperty in missingProperties)
                {
                    propertyDefinitionRepository.Delete(missingProperty);
                    removedPropertyDefinitions.Add(Tuple.Create(contentType.Name, missingProperty.Name));                    
                }
            }

            if (removedPropertyDefinitions.Any())
            {
                return FormatResultMessage(removedPropertyDefinitions);
            }

            return "No property has been deleted, schema is up to date.";
        }

        private static string FormatResultMessage(IReadOnlyCollection<Tuple<string, string>> missingProperties)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Following ({missingProperties.Count}) properties have been removed:");

            foreach (var (contentType, propertyName) in missingProperties)
            {
                stringBuilder.AppendLine($"[{contentType}: {propertyName}]");
            }

            return stringBuilder.ToString();
        }

        private IEnumerable<PropertyDefinition> GetContentTypeMissingProperties(ContentType contentType)
        {
            return contentType.PropertyDefinitions.Where(p =>
                p.ExistsOnModel && contentTypeModelRepository.GetPropertyModel(p.ContentTypeID, p) == null);
        }
    }
}