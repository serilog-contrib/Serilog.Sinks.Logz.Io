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
using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Logz.Io.Client;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logz.Io.Ecs
{
    /// <summary>
    /// Send log events using HTTP POST over the network.
    /// </summary>
    public sealed class LogzioSink : PeriodicBatchingSink
    {
        private readonly LogzIoUrl LogzIoHttpUrl = new LogzIoUrl("http://{2}.logz.io:{3}/?token={0}&type={1}", 8070);
        private readonly LogzIoUrl LogzIoHttpsUrl = new LogzIoUrl("https://{2}.logz.io:{3}/?token={0}&type={1}", 8071);

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
        private readonly LogzioOptions _options;
        private readonly string _requestUrl;

        private static readonly EcsTextFormatterConfiguration Config = new EcsTextFormatterConfiguration();
        private readonly EcsTextFormatter _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogzioSink"/> class. 
        /// </summary>
        /// <param name="client">The client responsible for sending HTTP POST requests.</param>
        /// <param name="authToken">The token for logzio.</param>
        /// <param name="type">Your log type - it helps classify the logs you send.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="useHttps">When true, uses HTTPS protocol.</param>
        /// <param name="boostProperties">When true, does not add 'properties' prefix.</param>
        /// <param name="dataCenterSubDomain">The logz.io datacenter specific sub-domain to send the logs to. options: "listener" (default, US), "listener-eu" (EU)</param>
        /// <param name="includeMessageTemplate">When true the message template is included in the logs</param>
        /// <param name="port">When specified overrides default port</param>
        public LogzioSink(IHttpClient client, string authToken, string type, int batchPostingLimit, TimeSpan period, bool useHttps = true, bool boostProperties = false, string dataCenterSubDomain = "listener", bool includeMessageTemplate = false, int? port = null)
            : this(client, authToken, type, new LogzioOptions
            {
                BatchPostingLimit = batchPostingLimit,
                Period = period,
                UseHttps = useHttps,
                BoostProperties = boostProperties,
                DataCenterSubDomain = dataCenterSubDomain,
                IncludeMessageTemplate = includeMessageTemplate,
                Port = port
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogzioSink"/> class. 
        /// </summary>
        /// <param name="client">The client responsible for sending HTTP POST requests.</param>
        /// <param name="authToken">The token for logzio.</param>
        /// <param name="type">Your log type - it helps classify the logs you send.</param>
        /// <param name="options">LogzIo configuration options</param>
        public LogzioSink(IHttpClient client, string authToken, string type, LogzioOptions options)
            : base(options?.BatchPostingLimit ?? DefaultBatchPostingLimit, options?.Period ?? DefaultPeriod)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            if (authToken == null)
                throw new ArgumentNullException(nameof(authToken));

            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (!string.IsNullOrWhiteSpace(OverrideLogzIoUrl))
            {
                _requestUrl = OverrideLogzIoUrl;
            }
            else
            {
                _requestUrl = _options.UseHttps
                    ? LogzIoHttpsUrl.Format(authToken, type, _options.DataCenterSubDomain, _options.Port)
                    : LogzIoHttpUrl.Format(authToken, type, _options.DataCenterSubDomain, _options.Port);
            }

            Config.MapCustom((log, logEvent) =>
            {
                if (log.Metadata.TryGetValue("user", out var value))
                {
                    log.User ??= new User();
                    log.User.Name = value.ToString();
                    log.Metadata.Remove("user");
                }

                log.Labels = log.Metadata;
                log.Metadata = null;

                log.Labels["environment"] = _options.Environment;

                log.Service ??= new Service();

                log.Service.Name = _options.ServiceName;
                //log.User = new User()

                return log;
            });

            Config.MapCurrentThread(true);

            _formatter = new EcsTextFormatter(Config);
        }

        #region PeriodicBatchingSink Members

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using var stream = FormatStream(events, _formatter);
            using var content = new StreamContent(stream);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await _client
                .PostAsync(_requestUrl, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();

                Console.WriteLine($"Unable to send stream to LogzIo: {response}");
                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {_requestUrl}");
            }
        }

        #endregion

        private MemoryStream FormatStream(IEnumerable<LogEvent> events, ITextFormatter formatter)
        {
            var memoryStream = new MemoryStream();

            var writer = new StreamWriter(memoryStream) {AutoFlush = true};

            foreach (var logEvent in events)
            {
                _formatter.Format(logEvent, writer);
            }

            memoryStream.Position = 0;

            return memoryStream;
        }
    }

}
