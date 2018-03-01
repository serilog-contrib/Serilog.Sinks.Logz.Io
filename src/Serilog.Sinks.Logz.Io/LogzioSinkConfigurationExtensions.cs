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

using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Logz.Io;
using Serilog.Sinks.Logz.Io.Client;

// ReSharper disable once CheckNamespace
namespace Serilog
{
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
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. The default is
        /// <see cref="LogzioSink.DefaultBatchPostingLimit"/>.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. The default is
        /// <see cref="LogzioSink.DefaultPeriod"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. The default is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="useHttps">Use HTTPS</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration LogzIo(
            this LoggerSinkConfiguration sinkConfiguration,
            string authToken,
            string type,
            int? batchPostingLimit = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            bool useHttps = false)
        {
            if (sinkConfiguration == null)
                throw new ArgumentNullException(nameof(sinkConfiguration));

            var client = new HttpClientWrapper();
            var sink = new LogzioSink(
                client,
                authToken,
                type,
                batchPostingLimit ?? LogzioSink.DefaultBatchPostingLimit,
                period ?? LogzioSink.DefaultPeriod,
                useHttps);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
