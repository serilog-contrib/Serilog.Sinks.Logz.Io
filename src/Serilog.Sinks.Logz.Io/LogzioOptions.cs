using System;
using Serilog.Events;

namespace Serilog.Sinks.Logz.Io
{
    public class LogzioOptions
    {
        /// <summary>
        /// The maximum number of events to post in a single batch. The default is <see cref="LogzioSink.DefaultBatchPostingLimit"/>.
        /// </summary>
        public int? BatchPostingLimit { get; set; }

        /// <summary>
        /// The time to wait between checking for event batches. The default is <see cref="LogzioSink.DefaultPeriod"/>.
        /// </summary>
        public TimeSpan? Period { get; set; }

        /// <summary>
        /// The minimum level for events passed through the sink. The default is <see cref="LogEventLevel.Verbose"/>.
        /// </summary>
        public LogEventLevel? RestrictedToMinimumLevel { get; set; }

        /// <summary>
        /// Use https
        /// </summary>
        public bool UseHttps { get; set; }
    }
}
