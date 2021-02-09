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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Elastic.CommonSchema.Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Logz.Io.Client;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logz.Io
{
    /// <summary>
    /// Send log events using HTTP POST over the network.
    /// https://app-eu.logz.io/#/dashboard/data-sources/Bulk-HTTPS
    /// </summary>
    public sealed class LogzioEcsSink : PeriodicBatchingSink
    {
        private readonly LogzIoUrl _logzIoHttpUrl = new("http://{2}.logz.io:{3}/?token={0}&type={1}", 8070);
        private readonly LogzIoUrl _logzIoHttpsUrl = new("https://{2}.logz.io:{3}/?token={0}&type={1}", 8071);

        /// <summary>
        /// The default batch posting limit.
        /// </summary>
        public static int DefaultBatchPostingLimit { get; } = 1000;

        /// <summary>
        /// The default period.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        /// <summary>
        /// When set uses specified Url to send events instead of logz.io
        /// </summary>
        public static string OverrideLogzIoUrl = "";
        
        private readonly IHttpClient _client;
        private readonly string _requestUrl;

        private readonly IEcsTextFormatterConfiguration _formatterConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogzioSink"/> class. 
        /// </summary>
        /// <param name="period"></param>
        /// <param name="batchPostingLimit"></param>
        /// <param name="httpClient"></param>
        /// <param name="options">LogzIo configuration options</param>
        /// <param name="formatterConfiguration"></param>
        public LogzioEcsSink(LogzioEcsOptions options
            , int? batchPostingLimit = null
            , TimeSpan? period = null
            , IHttpClient? httpClient = null
            , IEcsTextFormatterConfiguration? formatterConfiguration = null)
            : base(batchPostingLimit ?? DefaultBatchPostingLimit, period ?? DefaultPeriod)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _client = httpClient ?? new HttpClientWrapper();

            var authToken = options.AuthToken;
            var type = options.Type;

            var dataCenterSubDomain = options.DataCenter?.SubDomain ?? string.Empty;
            if (string.IsNullOrWhiteSpace(dataCenterSubDomain))
                dataCenterSubDomain = "listener";

            var dataCenterPort = options.DataCenter?.Port ?? 0;
            if (dataCenterPort == 0)
                dataCenterPort = 8071;

            var useHttps = options.DataCenter?.UseHttps ?? true;

            if (!string.IsNullOrWhiteSpace(OverrideLogzIoUrl))
            {
                _requestUrl = OverrideLogzIoUrl;
            }
            else
            {
                _requestUrl = useHttps
                    ? _logzIoHttpsUrl.Format(authToken, type, dataCenterSubDomain, dataCenterPort)
                    : _logzIoHttpUrl.Format(authToken, type, dataCenterSubDomain, dataCenterPort);
            }

            _formatterConfiguration = formatterConfiguration ?? new EcsTextFormatterConfiguration();
        }

        #region PeriodicBatchingSink Members

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using var stream = FormatToStream(events, _formatterConfiguration);
            using var content = new StreamContent(stream);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await _client
                .PostAsync(_requestUrl, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {_requestUrl}. Response: {response}.");
            }
        }

        #endregion

        private MemoryStream FormatToStream(IEnumerable<LogEvent> events, IEcsTextFormatterConfiguration formatterConfiguration)
        {
            var memoryStream = new MemoryStream();

            var writer = new StreamWriter(memoryStream) {AutoFlush = true};

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
}
