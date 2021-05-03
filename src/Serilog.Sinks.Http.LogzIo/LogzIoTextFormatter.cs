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

        private readonly string _messageFieldName = "message";  // must be always in lowercase because of logz.io kibana configuration
        private readonly string _messageTemplateFieldName = "MessageTemplate";
        private readonly string _levelFieldName = "Level";
        private readonly string _exceptionFieldName = "Exception";
        private readonly string _propertiesPrefix = "Properties.";

        private readonly Func<string, string> _transformFieldName;

        public LogzIoTextFormatter(): this(null)
        {
        }

        public LogzIoTextFormatter(LogzioTextFormatterOptions? options)
        {
            _options = options ?? new LogzioTextFormatterOptions();

            _transformFieldName = field => field;

            var fieldNaming = _options.FieldNaming;
            if (fieldNaming.HasValue)
            {
                switch (fieldNaming.Value)
                {
                    case LogzIoTextFormatterFieldNaming.CamelCase:
                        _transformFieldName = field =>
                        {
                            if (string.IsNullOrEmpty(field))
                                return field;

                            return char.ToLower(field[0]) + field.Substring(1);
                        };
                        break;
                    case LogzIoTextFormatterFieldNaming.LowerCase:
                        _transformFieldName = field => field.ToLower();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _messageTemplateFieldName = _transformFieldName(_messageTemplateFieldName);
            _levelFieldName = _transformFieldName(_levelFieldName);
            _exceptionFieldName = _transformFieldName(_exceptionFieldName);
            _propertiesPrefix = _transformFieldName(_propertiesPrefix);
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
                LogAsInternalEvent(logEvent, e);
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
                {_levelFieldName, level},
                {_messageFieldName, loggingEvent.RenderMessage()},
                {_exceptionFieldName, loggingEvent.Exception}
            };

            if (_options.IncludeMessageTemplate)
            {
                values.Add(_messageTemplateFieldName, loggingEvent.MessageTemplate.Text);
            }

            if (loggingEvent.Properties != null)
            {
                var propertyPrefix = _options.BoostProperties ? "" : _propertiesPrefix;

                foreach (var property in loggingEvent.Properties)
                {
                    if (!OverrideFieldName(property.Key, out var fieldName))
                    {
                        fieldName = $"{propertyPrefix}{property.Key}";
                    }

                    fieldName = _transformFieldName(fieldName!);

                    var propertyInternalValue = GetPropertyInternalValue(property.Value);
                    values[fieldName] = propertyInternalValue;
                }
            }

            return values;
        }

        private bool OverrideFieldName(string key, out string? overridenFieldName)
        {
            overridenFieldName = null;

            if (_options.FieldNameTransformationMap == null)
            {
                return false;
            }

            if (!_options.FieldNameTransformationMap.TryGetValue(key, out var fieldName))
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

        private static void LogAsInternalEvent(LogEvent logEvent, Exception e)
        {
            SelfLog.WriteLine(
                "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
                logEvent.Timestamp.ToString("o"),
                logEvent.MessageTemplate.Text,
                e);
        }
    }
}
