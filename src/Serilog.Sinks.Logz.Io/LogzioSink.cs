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
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Logz.Io.Client;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logz.Io
{
    /// <summary>
    /// Send log events using HTTP POST over the network.
    /// </summary>
    public sealed class LogzioSink : PeriodicBatchingSink
    {
        private IHttpClient _client;
        private readonly string _requestUri;

        /// <summary>
        /// The default batch posting limit.
        /// </summary>
        public static int DefaultBatchPostingLimit { get; } = 1000;

        /// <summary>
        /// The default period.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        /// <summary>
        /// 
        /// </summary>
        private const string LogzIoHttpUrl = "http://listener.logz.io:8070/?token={0}&type={1}";
        private const string LogzIoHttpsUrl = "https://listener.logz.io:8071/?token={0}&type={1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="LogzioSink"/> class. 
        /// </summary>
        /// <param name="client">The client responsible for sending HTTP POST requests.</param>
        /// <param name="authToken">The token for logzio.</param>
        /// <param name="type">Your log type - it helps classify the logs you send.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="useHttps">When true, uses HTTPS protocol.</param>
        public LogzioSink(
            IHttpClient client,
            string authToken,
            string type,
            int batchPostingLimit,
            TimeSpan period,
            bool useHttps = false)
            : base(batchPostingLimit, period)
        {
            if (authToken == null)
                throw new ArgumentNullException(nameof(authToken));

            _client = client ?? throw new ArgumentNullException(nameof(client));

            _requestUri = useHttps ? string.Format(LogzIoHttpsUrl, authToken, type) : string.Format(LogzIoHttpUrl, authToken, type);
        }

        #region PeriodicBatchingSink Members

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var payload = FormatPayload(events);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var result = await _client
                .PostAsync(_requestUri, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {_requestUri}");
        }

        /// <summary>
        /// Free resources held by the sink.
        /// </summary>
        /// <param name="disposing">
        /// If true, called because the object is being disposed; if false, the object is being
        /// disposed from the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing || _client == null)  return;

            _client.Dispose();
            _client = null;
        }

        #endregion

        private string FormatPayload(IEnumerable<LogEvent> events)
        {
            var result = events
                .Select(FormatLogEvent)
                .ToArray();

            return string.Join(",\n", result);
        }

        private string FormatLogEvent(LogEvent loggingEvent)
        {
            var values = new Dictionary<string, object>
            {
                {"@timestamp", loggingEvent.Timestamp.ToString("O")},
                {"Level", loggingEvent.Level.ToString()},
                {"Message", loggingEvent.RenderMessage()},
                {"Exception", loggingEvent.Exception}
            };

            if (loggingEvent.Properties != null)
            {
                if (loggingEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
                {
                    values["Logger"] = sourceContext.ToString();
                }

                foreach (var property in loggingEvent.Properties)
                {
                    values[$"Properties.{property.Key}"] = GetPropertyInternalValue(property.Value);
                }
            }

            return JsonConvert.SerializeObject(values, Newtonsoft.Json.Formatting.None);
        }

        private static Object GetPropertyInternalValue(LogEventPropertyValue propertyValue)
        {
            switch (propertyValue)
            {
                case ScalarValue sv: return sv.Value;
                case SequenceValue sv: return sv.Elements;
                case DictionaryValue dv: return dv.Elements;
                case StructureValue sv: return sv.ToString();
            }
            return propertyValue.ToString();
        }
    }
}
