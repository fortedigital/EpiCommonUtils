using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forte.EpiCommonUtils.Infrastructure
{
    public class EntryValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<string>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return new WebpackManifest.EntryValue(token.ToObject<List<string>>());
            }
            return new WebpackManifest.EntryValue { token.ToObject<string>() };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override bool CanWrite => false;
    }
}