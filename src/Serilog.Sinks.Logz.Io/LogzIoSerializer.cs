using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Serilog.Sinks.Logz.Io
{
    public interface ILogzIoSerializer
    {
        string Serialize<T>(T value);
    }

    public class LogzIoSerializer: ILogzIoSerializer
    {
        public static ILogzIoSerializer Instance { get; set; } = new LogzIoSerializer(LogzIoTextFormatterFieldNaming.CamelCase);

        public static JsonSerializerSettings SerializerOptions { get; set; }

        static LogzIoSerializer()
        {
            SerializerOptions = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };

            SerializerOptions.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
        }
        
        public LogzIoSerializer(LogzIoTextFormatterFieldNaming fieldNaming)
        {
            SerializerOptions = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            NamingStrategy namingStrategy = fieldNaming == LogzIoTextFormatterFieldNaming.CamelCase
                ? new CamelCaseNamingStrategy()
                : new DefaultNamingStrategy();

            SerializerOptions.Converters.Add(new StringEnumConverter(namingStrategy));
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, SerializerOptions);
        }
    }
}
