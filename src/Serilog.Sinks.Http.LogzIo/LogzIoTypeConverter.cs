using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serilog.Sinks.Http.LogzIo
{
    /// <summary>
    /// Solves:
    /// System.NotSupportedException: Serialization and deserialization of 'System.Type' instances are not supported and should be avoided since they can lead to security issues.
    /// </summary>
    public class LogzIoTypeConverter : JsonConverter<Type>
    {
        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var assemblyQualifiedName = reader.GetString();
            if (assemblyQualifiedName == null || string.IsNullOrWhiteSpace(assemblyQualifiedName))
                return null;

            return Type.GetType(assemblyQualifiedName);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.AssemblyQualifiedName);
        }
    }
}