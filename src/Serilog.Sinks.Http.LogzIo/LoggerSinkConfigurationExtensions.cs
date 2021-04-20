using System;
using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.LogzIo;

// ReSharper disable once CheckNamespace
namespace Serilog
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration LogzIoDurableHttp(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            string bufferPathFormat = "Buffer-{Date}.json",
            long? bufferFileSizeLimitBytes = 104857600,
            bool bufferFileShared = false,
            int? retainedBufferFileCountLimit = 31,
            int batchPostingLimit = 1000,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IHttpClient? httpClient = null,
            IConfiguration? configuration = null,
            LogzioTextFormatterOptions? logzioTextFormatterOptions = null)
        {
            return sinkConfiguration.DurableHttpUsingTimeRolledBuffers(
                requestUri
                , bufferPathFormat
                , bufferFileSizeLimitBytes
                , bufferFileShared
                , retainedBufferFileCountLimit
                , batchPostingLimit
                , period
                , new LogzIoTextFormatter(logzioTextFormatterOptions)
                , new LogzIoBatchFormatter(renameRenderedMessageJsonNode: false)
                , restrictedToMinimumLevel
                , httpClient
                , configuration);
        }
    }
}
