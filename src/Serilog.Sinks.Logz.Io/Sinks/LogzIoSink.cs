using System.Text;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Logz.Io.Client;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logz.Io.Sinks;

public sealed class LogzIoSink : IBatchedLogEventSink
{
    private readonly string? _requestUrl;
    private readonly LogzioOptions _options;
    private readonly IHttpClient _client;

    public LogzIoSink(string authToken, string type, LogzioOptions? options, IHttpClient? client = null)
    {
        _options = options ?? new LogzioOptions();
        _requestUrl = LogzIoDefaults.GetUrl(authToken, type, _options.DataCenter);
        _client = client ?? new HttpClientWrapper();
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        if (_requestUrl == null || string.IsNullOrWhiteSpace(_requestUrl))
            return;

        var payload = FormatPayload(batch);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        try
        {
            var result = await _client
                .PostAsync(_requestUrl, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                var exception = new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {_requestUrl}");
                SelfLog.WriteLine($"{exception.Message} {exception.StackTrace}");
                _options.FailureCallback?.Invoke(exception);
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine($"{ex.Message} {ex.StackTrace}");
            _options.FailureCallback?.Invoke(ex);
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private string FormatPayload(IEnumerable<LogEvent> events)
    {
        var result = events
            .Select(FormatLogEvent)
            .ToArray();

        return string.Join(",\n", result);
    }

    private string FormatLogEvent(LogEvent loggingEvent)
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

        if (_options.ServiceName != null)
        {
            values.Add("service", _options.ServiceName);
        }

        if (_options.Environment != null)
        {
            values.Add("environment", _options.Environment);
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

        return LogzIoSerializer.Instance.Serialize(values);
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
}