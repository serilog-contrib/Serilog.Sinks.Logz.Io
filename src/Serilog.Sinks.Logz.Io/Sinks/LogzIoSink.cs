using System.Text;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Logz.Io.Client;
using Serilog.Sinks.PeriodicBatching;
using IHttpClient = Serilog.Sinks.Logz.Io.Client.IHttpClient;

namespace Serilog.Sinks.Logz.Io.Sinks;

public sealed class LogzIoSink : IBatchedLogEventSink
{
    private readonly string? _requestUrl;
    private readonly LogzioOptions _options;
    private readonly IHttpClient _client;
    private readonly FormattingOptions _formattingOptions;

    public LogzIoSink(string authToken, string type, LogzioOptions? options, IHttpClient? client = null)
    {
        _options = options ?? new LogzioOptions();
        _options.TextFormatterOptions ??= new LogzioTextFormatterOptions();

        _requestUrl = LogzIoDefaults.GetUrl(authToken, type, _options.DataCenter);
        _client = client ?? new HttpClientWrapper();
        _formattingOptions = new FormattingOptions(_options.TextFormatterOptions.FieldNaming)
        {
            BoostProperties = _options.TextFormatterOptions.BoostProperties,
            IncludeMessageTemplate = _options.TextFormatterOptions.IncludeMessageTemplate,
            LowercaseLevel = _options.TextFormatterOptions.LowercaseLevel,
            PropertyTransformationMap = _options.TextFormatterOptions.PropertyTransformationMap,
            EventSizeLimitBytes = _options.TextFormatterOptions.EventSizeLimitBytes ?? 255 * ByteSize.KB
        };
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        if (_requestUrl == null || string.IsNullOrWhiteSpace(_requestUrl))
            return;

        try
        {
            var payload = FormatPayload(batch);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

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
            .Select(e => e.Format(_formattingOptions))
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToArray();

        return string.Join(",\n", result);
    }
}