using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Http;

namespace Serilog.Sinks.Logz.Io;

internal static class LogEventExtensions
{
    public static string? Format(this LogEvent loggingEvent, FormattingOptions options)
    {
        var level = loggingEvent.Level.ToString();
        if (options.LowercaseLevel)
            level = level.ToLower();

        var values = new Dictionary<string, object>
        {
            {"@timestamp", loggingEvent.Timestamp.ToString("O")},
            {"message", loggingEvent.RenderMessage()},
            {options.LevelFieldName, level},
            {"traceId", loggingEvent.TraceId?.ToString() ?? ""},
            {"spanId", loggingEvent.SpanId?.ToString() ?? ""},
        };

        if (loggingEvent.Exception != null)
        {
            values[options.ExceptionFieldName] = loggingEvent.Exception;
        }

        if (options.IncludeMessageTemplate)
        {
            values.Add("messageTemplate", loggingEvent.MessageTemplate.Text);
        }

        if (loggingEvent.Properties != null!)
        {
            var propertyPrefix = options.BoostProperties ? "" : "properties.";

            foreach (var property in loggingEvent.Properties)
            {
                if (!OverrideFieldName(options, property.Key, out var fieldName))
                {
                    fieldName = $"{propertyPrefix}{property.Key}";
                }

                fieldName = options.TransformFieldName(fieldName!);

                var propertyInternalValue = GetPropertyInternalValue(property.Value);
                values[fieldName] = propertyInternalValue;
            }
        }

        try
        {
            var content = LogzIoSerializer.Instance.Serialize(values);

            if (CheckEventBodySize(content, options.EventSizeLimitBytes))
            {
                return content;
            }
        }
        catch (Exception e)
        {
            LogAsInternalEvent(loggingEvent, e);
        }

        return null;
    }

    private static bool OverrideFieldName(FormattingOptions options, string key, out string? overridenFieldName)
    {
        overridenFieldName = null;

        if (options.PropertyTransformationMap == null)
        {
            return false;
        }

        if (!options.PropertyTransformationMap.TryGetValue(key, out var fieldName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        overridenFieldName = fieldName;
        return true;
    }

    private static object GetPropertyInternalValue(LogEventPropertyValue propertyValue)
    {
        return propertyValue switch
        {
            ScalarValue sv => GetInternalValue(sv.Value),
            SequenceValue sv => sv.Elements.Select(GetPropertyInternalValue).ToArray(),
            DictionaryValue dv => dv.Elements.Select(kv => new { Key = kv.Key.Value, Value = GetPropertyInternalValue(kv.Value) }).ToDictionary(i => i.Key, i => i.Value),
            StructureValue sv => sv.Properties.Select(kv => new { Key = kv.Name, Value = GetPropertyInternalValue(kv.Value) }).ToDictionary(i => i.Key, i => i.Value),
            _ => propertyValue.ToString()
        };
    }

    private static object GetInternalValue(object value)
    {
        return value switch
        {
            Enum e => e.ToString(),
            _ => value
        };
    }

    private static bool CheckEventBodySize(string json, long eventSizeLimitBytes)
    {
        if (eventSizeLimitBytes > 0 && ByteSize.From(json) > eventSizeLimitBytes)
        {
            SelfLog.WriteLine(
                "Event JSON representation exceeds the byte size limit of {0} set for this sink and will be dropped; data: {1}",
                eventSizeLimitBytes,
                json);

            return false;
        }

        return true;
    }

    private static void LogAsInternalEvent(LogEvent logEvent, Exception e)
    {
        SelfLog.WriteLine(
            "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
            logEvent.Timestamp.ToString("o"),
            logEvent.MessageTemplate.Text,
            e);
    }
}

internal sealed class FormattingOptions
{
    public bool LowercaseLevel { get; set; }
    public bool IncludeMessageTemplate { get; set; }
    public bool BoostProperties { get; set; }
    public Dictionary<string, string>? PropertyTransformationMap { get; set; }
    public Func<string, string> TransformFieldName { get; set; }
    public long EventSizeLimitBytes { get; set; }

    public string MessageTemplateFieldName { get; }
    public string LevelFieldName { get; }
    public string ExceptionFieldName { get; }
    public string PropertiesPrefix { get; }

    public FormattingOptions(LogzIoTextFormatterFieldNaming? fieldNaming)
    {
        var naming = fieldNaming ?? LogzIoTextFormatterFieldNaming.CamelCase;

        TransformFieldName = naming switch
        {
            LogzIoTextFormatterFieldNaming.CamelCase => field => string.IsNullOrEmpty(field) ? field : char.ToLower(field[0]) + field.Substring(1),
            LogzIoTextFormatterFieldNaming.LowerCase => field => field.ToLower(),
            _ => field => field
        };

        MessageTemplateFieldName = TransformFieldName("MessageTemplate");
        LevelFieldName = TransformFieldName("Level");
        ExceptionFieldName = TransformFieldName("Exception");
        PropertiesPrefix = TransformFieldName("Properties.");
    }
}