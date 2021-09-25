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
        private readonly LogzIoUrl _logzIoHttpUrl = new("http://{2}.logz.io:{3}/?token={0}&type={1}", 8070);
        private readonly LogzIoUrl _logzIoHttpsUrl = new("https://{2}.logz.io:{3}/?token={0}&type={1}", 8071);

        /// <summary>
        /// The default batch posting limit.
        /// </summary>
        public static int DefaultBatchPostingLimit => 1000;

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
        public LogzioSink(IHttpClient client
            , string authToken
            , string type
            , int batchPostingLimit
            , TimeSpan period
            , bool useHttps = true
            , bool boostProperties = false
            , string dataCenterSubDomain = "listener"
            , bool includeMessageTemplate = false
            , int? port = null
            ) : this(client, authToken, type, new LogzioOptions
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
        public LogzioSink(IHttpClient client
            , string authToken
            , string type
            , LogzioOptions? options
            ) : base(options?.BatchPostingLimit ?? DefaultBatchPostingLimit, options?.Period ?? DefaultPeriod)
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
                    ? _logzIoHttpsUrl.Format(authToken, type, _options.DataCenterSubDomain, _options.Port)
                    : _logzIoHttpUrl.Format(authToken, type, _options.DataCenterSubDomain, _options.Port);
            }
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
                .PostAsync(_requestUrl, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {_requestUrl}");
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
            switch (propertyValue)
            {
                case ScalarValue sv: return GetInternalValue(sv.Value);
                case SequenceValue sv: return sv.Elements.Select(GetPropertyInternalValue).ToArray();
                case DictionaryValue dv: return dv.Elements.Select(kv => new { Key = kv.Key.Value, Value = GetPropertyInternalValue(kv.Value) }).ToDictionary(i => i.Key, i => i.Value);
                case StructureValue sv: return sv.Properties.Select(kv => new { Key = kv.Name, Value = GetPropertyInternalValue(kv.Value) }).ToDictionary(i => i.Key, i => i.Value);
            }
            return propertyValue.ToString();
        }

        private static object GetInternalValue(object value)
        {
            switch (value)
            {
                case Enum e: return e.ToString();
            }

            return value;
        }
    }
}
