using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Lykke.Job.NeoGasDistributor
{
    public class ParseDecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(decimal) || t == typeof(decimal?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = serializer.Deserialize<string>(reader);

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            {
                return d;
            }

            throw new InvalidOperationException($"Cannot parse {value} as decimal. Path: {reader.Path}");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (decimal)untypedValue;
            serializer.Serialize(writer, value.ToString(CultureInfo.InvariantCulture));
            return;
        }
    }
}
