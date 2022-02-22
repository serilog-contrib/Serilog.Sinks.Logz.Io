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

using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Logz.Io;

// ReSharper disable once CheckNamespace
namespace Serilog;

public static class DurableSinkConfigurationExtensions
{
    public static LoggerConfiguration LogzIoDurableHttp(
        this LoggerSinkConfiguration sinkConfiguration,
        string requestUri,
        string bufferPathFormat = "Buffer-{Hour}.json",
        long? bufferFileSizeLimitBytes = 104857600,
        bool bufferFileShared = false,
        int? retainedBufferFileCountLimit = 31,
        int batchPostingLimit = 1000,
        TimeSpan? period = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        IHttpClient? httpClient = null,
        IConfiguration? configuration = null,
        LogzioTextFormatterOptions? logzioTextFormatterOptions = null)
    {
        return sinkConfiguration.DurableHttpUsingTimeRolledBuffers(requestUri
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
            , configuration
        );
    }
}