using System.Net.Http;
using System.Net.Http.Headers;
using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;

using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Logz.Io.Client;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logz.Io.Sinks;

public sealed class LogzIoEcsSink : IBatchedLogEventSink
{
    private readonly string? _requestUrl;
    private readonly IEcsTextFormatterConfiguration<EcsDocument> _formatterConfiguration;
    private readonly IHttpClient _client;

    public LogzIoEcsSink(LogzioEcsOptions? options, IEcsTextFormatterConfiguration<EcsDocument>? formatterConfiguration = null, IHttpClient? httpClient = null)
    {
        options ??= new LogzioEcsOptions();
        _requestUrl = LogzIoDefaults.GetUrl(options.AuthToken, options.Type, options.DataCenter);
        _formatterConfiguration = formatterConfiguration ?? new EcsTextFormatterConfiguration();
        _client = httpClient ?? new HttpClientWrapper();
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        if (_requestUrl == null || string.IsNullOrWhiteSpace(_requestUrl))
            return;

        using var stream = FormatToStream(batch, _formatterConfiguration);
        using var content = new StreamContent(stream);

        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var result = await _client
            .PostAsync(_requestUrl, content)
            .ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadAsStringAsync();

            SelfLog.WriteLine($"Received failed result {result.StatusCode} when posting events to {_requestUrl}. Response: {response}.");

            throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {_requestUrl}. Response: {response}.");
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private MemoryStream FormatToStream(IEnumerable<LogEvent> events, IEcsTextFormatterConfiguration<EcsDocument> formatterConfiguration)
    {
        var memoryStream = new MemoryStream();

        var writer = new StreamWriter(memoryStream) { AutoFlush = true };

        foreach (var logEvent in events)
        {
            var ecsEvent = LogEventConverter.ConvertToEcs(logEvent, formatterConfiguration);
            ecsEvent.Serialize(writer.BaseStream);
            writer.WriteLine();
        }

        memoryStream.Position = 0;

        return memoryStream;
    }
}