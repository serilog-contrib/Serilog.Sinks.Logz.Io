// Copyright 2015-2016 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;

using Microsoft.Extensions.Configuration;

using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Logz.Io;
using Serilog.Sinks.Logz.Io.Sinks;
using Serilog.Sinks.PeriodicBatching;

// ReSharper disable once CheckNamespace
namespace Serilog;

/// <summary>
/// Adds the WriteTo.LogzIo() extension method to <see cref="LoggerConfiguration"/>.
/// </summary>
public static class LogzioSinkConfigurationExtensions
{
    /// <summary>
    /// Adds a sink that sends log events using HTTP POST over the network.
    /// </summary>
    /// <param name="sinkConfiguration">The logger configuration.</param>
    /// <param name="authToken">The token for your logzio account.</param>
    /// <param name="type">Your log type - it helps classify the logs you send.</param>
    /// <param name="useHttps">Specifies to use https (default is true)</param>
    /// <param name="boostProperties">When true, does not add 'properties' prefix.</param>
    /// <param name="dataCenterSubDomain">The logz.io datacenter specific sub domain to send the logs to. options: "listener" (default, US), "listener-eu" (EU)</param>
    /// <param name="restrictedToMinimumLevel">Specifies minimal level for log events</param>
    /// <param name="logEventsInBatchLimit">The maximum number of events to post in a single batch</param>
    /// <param name="period">The time to wait between checking for event batches</param>
    /// <param name="lowercaseLevel">Set to true to push log level as lowercase</param>
    /// <param name="includeMessageTemplate">When true the message template is included in the logs</param>
    /// <param name="port">When specified overrides default port</param>
    /// <returns>Logger configuration, allowing configuration to continue.</returns>
    public static LoggerConfiguration LogzIo(
        this LoggerSinkConfiguration sinkConfiguration,
        string authToken,
        string type,
        bool useHttps = true,
        bool boostProperties = false,
        string dataCenterSubDomain = "listener",
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        int? logEventsInBatchLimit = null,
        TimeSpan? period = null,
        bool lowercaseLevel = false,
        bool includeMessageTemplate = false,
        int? port = null)
    {
        return LogzIo(sinkConfiguration, authToken, type, new LogzioOptions
        {
            RestrictedToMinimumLevel = restrictedToMinimumLevel,
            LogEventsInBatchLimit = logEventsInBatchLimit,
            Period = period,
            DataCenter = new LogzioDataCenter
            {
                SubDomain = dataCenterSubDomain,
                Port = port,
                UseHttps = useHttps
            },
            TextFormatterOptions = new LogzioTextFormatterOptions
            {
                BoostProperties = boostProperties,
                LowercaseLevel = lowercaseLevel,
                IncludeMessageTemplate = includeMessageTemplate
            }
        });
    }

    /// <summary>
    /// Adds a sink that sends log events using HTTP POST over the network.
    /// </summary>
    /// <param name="sinkConfiguration">The logger configuration.</param>
    /// <param name="authToken">The token for your logzio account.</param>
    /// <param name="type">Your log type - it helps classify the logs you send.</param>
    /// <param name="options">Logzio configuration options</param>
    /// <returns>Logger configuration, allowing configuration to continue.</returns>
    public static LoggerConfiguration LogzIo(this LoggerSinkConfiguration sinkConfiguration, string authToken, string type, LogzioOptions? options = null)
    {
        if (sinkConfiguration == null)
            throw new ArgumentNullException(nameof(sinkConfiguration));

        options ??= new LogzioOptions();

        var sink = new PeriodicBatchingSink(
            new LogzIoSink(authToken, type, options),
            LogzIoDefaults.CreateBatchingSinkOptions(options.LogEventsInBatchLimit, options.Period)
        );

        var restrictedToMinimumLevel = options.RestrictedToMinimumLevel ?? LogEventLevel.Verbose;

        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
    }

    /// <summary>
    /// Adds a sink that sends log events using HTTP POST over the network.
    /// </summary>
    /// <param name="sinkConfiguration">The logger configuration.</param>
    /// <param name="options">Logzio configuration options</param>
    /// <param name="logEventsInBatchLimit"></param>
    /// <param name="period"></param>
    /// <param name="restrictedToMinimumLevel"></param>
    /// <param name="formatterConfiguration"></param>
    /// <returns>Logger configuration, allowing configuration to continue.</returns>
    public static LoggerConfiguration LogzIoEcs(this LoggerSinkConfiguration sinkConfiguration
        , LogzioEcsOptions options
        , int? logEventsInBatchLimit = null
        , TimeSpan? period = null
        , LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning
        , IEcsTextFormatterConfiguration<EcsDocument>? formatterConfiguration = null)
    {
        if (sinkConfiguration == null)
            throw new ArgumentNullException(nameof(sinkConfiguration));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var sink = new PeriodicBatchingSink(
            new LogzIoEcsSink(options, formatterConfiguration),
            LogzIoDefaults.CreateBatchingSinkOptions(logEventsInBatchLimit, period)
        );

        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
    }

    public static LoggerConfiguration LogzIoDurableHttp(
        this LoggerSinkConfiguration sinkConfiguration,
        string requestUri,
        string bufferBaseFileName = "Buffer",
        BufferRollingInterval bufferRollingInterval = BufferRollingInterval.Day,
        long? bufferFileSizeLimitBytes = null,
        bool bufferFileShared = false,
        int? retainedBufferFileCountLimit = 31,
        long? logEventLimitBytes = null,
        int? logEventsInBatchLimit = 1000,
        long? batchSizeLimitBytes = null,
        TimeSpan? period = null,
        bool flushOnClose = true,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch? levelSwitch = null,
        IHttpClient? httpClient = null,
        IConfiguration? configuration = null,
        LogzioTextFormatterOptions? logzioTextFormatterOptions = null)
    {
        return sinkConfiguration.DurableHttpUsingTimeRolledBuffers(
            requestUri
            , bufferBaseFileName
            , bufferRollingInterval
            , bufferFileSizeLimitBytes
            , bufferFileShared
            , retainedBufferFileCountLimit
            , logEventLimitBytes
            , logEventsInBatchLimit
            , batchSizeLimitBytes
            , period
            , flushOnClose
            , new LogzIoTextFormatter(logzioTextFormatterOptions)
            , new LogzIoBatchFormatter(renameRenderedMessageJsonNode: false)
            , restrictedToMinimumLevel
            , levelSwitch
            , httpClient
            , configuration
        );
    }

    public static LoggerConfiguration LogzIoEcsDurableHttp(
        this LoggerSinkConfiguration sinkConfiguration,
        string requestUri,
        string bufferBaseFileName = "Buffer",
        BufferRollingInterval bufferRollingInterval = BufferRollingInterval.Day,
        long? bufferFileSizeLimitBytes = null,
        bool bufferFileShared = false,
        int? retainedBufferFileCountLimit = 31,
        long? logEventLimitBytes = null,
        int? logEventsInBatchLimit = 1000,
        long? batchSizeLimitBytes = null,
        TimeSpan? period = null,
        bool flushOnClose = true,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch? levelSwitch = null,
        IHttpClient? httpClient = null,
        IConfiguration? configuration = null,
        LogzioEcsTextFormatterOptions? logzioTextFormatterOptions = null)
    {
        return sinkConfiguration.DurableHttpUsingTimeRolledBuffers(
            requestUri
            , bufferBaseFileName
            , bufferRollingInterval
            , bufferFileSizeLimitBytes
            , bufferFileShared
            , retainedBufferFileCountLimit
            , logEventLimitBytes
            , logEventsInBatchLimit
            , batchSizeLimitBytes
            , period
            , flushOnClose
            , new LogzIoEcsTextFormatter(logzioTextFormatterOptions)
            , new LogzIoBatchFormatter(renameRenderedMessageJsonNode: false)
            , restrictedToMinimumLevel
            , levelSwitch
            , httpClient
            , configuration
        );
    }
}