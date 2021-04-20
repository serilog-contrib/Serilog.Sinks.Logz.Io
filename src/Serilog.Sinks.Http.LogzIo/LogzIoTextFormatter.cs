using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Http.LogzIo
{
    public class LogzIoTextFormatter: ITextFormatter
    {
        private readonly LogzioTextFormatterOptions _options;

        public LogzIoTextFormatter(): this(null)
        {
        }

        public LogzIoTextFormatter(LogzioTextFormatterOptions? options)
        {
            _options = options ?? new LogzioTextFormatterOptions();
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            try
            {
                var values = Format(logEvent);
                string content = JsonSerializer.Serialize(values);
                output.WriteLine(content);
            }
            catch (Exception e)
            {
                LogNonFormattableEvent(logEvent, e);
            }
        }

        protected virtual Dictionary<string, object> Format(LogEvent loggingEvent)
        {
            var level = loggingEvent.Level.ToString();
            if (_options.LowercaseLevel)
                level = level.ToLower();

            var values = new Dictionary<string, object>
            {
                {"@timestamp", loggingEvent.Timestamp.ToString("O")},
                {"level", level},
                {"message", loggingEvent.RenderMessage()},
                {"exception", loggingEvent.Exception}
            };

            if (_options.IncludeMessageTemplate)
            {
                values.Add("messageTemplate", loggingEvent.MessageTemplate.Text);
            }

            if (loggingEvent.Properties != null)
            {
                var propertyPrefix = _options.BoostProperties ? "" : "properties.";

                foreach (var property in loggingEvent.Properties)
                {
                    var propertyName = $"{propertyPrefix}{property.Key}";
                    if (_options.PropertyTransformationMap != null &&
                        _options.PropertyTransformationMap.TryGetValue(property.Key, out var mappedPropertyName) &&
                        !string.IsNullOrWhiteSpace(mappedPropertyName))
                    {
                        propertyName = mappedPropertyName;
                    }

                    var propertyInternalValue = GetPropertyInternalValue(property.Value);
                    values[propertyName] = propertyInternalValue;
                }
            }

            return values;
        }

        private static object GetPropertyInternalValue(LogEventPropertyValue propertyValue)
        {
            switch (propertyValue)
            {
                case ScalarValue sv: return GetInternalValue(sv.Value);
                case SequenceValue sv: return sv.Elements.Select(GetPropertyInternalValue).ToArray();
                case DictionaryValue dv: return dv.Elements.Select(kv => new { Key = kv.Key.Value, Value = GetPropertyInternalValue(kv.Value) }).ToDictionary(i => i.Key, i => i.Value);
                case StructureValue sv: return sv.Properties.Select(kv => new { Key = kv.Name, Value = GetPropertyInternalValue(kv.Value) }).ToDictionary(i => i.Key, i => i.Value);
            }
            return propertyValue.ToString();
        }

        private static object GetInternalValue(object value)
        {
            switch (value)
            {
                case Enum e: return e.ToString();
            }

            return value;
        }

        private static void LogNonFormattableEvent(LogEvent logEvent, Exception e)
        {
            SelfLog.WriteLine(
                "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
                logEvent.Timestamp.ToString("o"),
                logEvent.MessageTemplate.Text,
                e);
        }
    }
}
