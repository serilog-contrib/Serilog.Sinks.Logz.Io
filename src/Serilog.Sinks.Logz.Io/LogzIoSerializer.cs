using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog.Sinks.Logz.Io.Converters;

namespace Serilog.Sinks.Logz.Io
{
    public interface ILogzIoSerializer
    {
        string Serialize<T>(T value);
    }

    public class LogzIoSerializer: ILogzIoSerializer
    {
        public static ILogzIoSerializer Instance { get; set; } = new LogzIoSerializer(LogzIoTextFormatterFieldNaming.CamelCase);

        public JsonSerializerSettings SerializerSettings { get; private set; }

        public LogzIoSerializer(LogzIoTextFormatterFieldNaming fieldNaming, bool includeMulticastDelegateConverter = true)
        {
            SerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            NamingStrategy namingStrategy = fieldNaming == LogzIoTextFormatterFieldNaming.CamelCase
                ? new CamelCaseNamingStrategy()
                : new DefaultNamingStrategy();

            SerializerSettings.Converters.Add(new StringEnumConverter(namingStrategy));

            if (includeMulticastDelegateConverter)
            {
                SerializerSettings.Converters.Add(new MulticastDelegateJsonConverter());
            }
        }

        public ILogzIoSerializer WithSerializerSettings(JsonSerializerSettings settings)
        {
            SerializerSettings = settings;
            return this;
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, SerializerSettings);
        }
    }
}
