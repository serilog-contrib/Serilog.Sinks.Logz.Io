using System;
using Newtonsoft.Json;

namespace Serilog.Sinks.Logz.Io.Converters
{
    public class MulticastDelegateJsonConverter: JsonConverter<MulticastDelegate>
    {
        public override void WriteJson(JsonWriter writer, MulticastDelegate? value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue($"{value.Method.DeclaringType?.FullName}.{value.Method.Name}");
            }
        }

        public override MulticastDelegate? ReadJson(JsonReader reader, Type objectType, MulticastDelegate? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
