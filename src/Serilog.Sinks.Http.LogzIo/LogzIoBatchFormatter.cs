using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Sinks.Http.BatchFormatters;

namespace Serilog.Sinks.Http.LogzIo
{
    public class LogzIoBatchFormatter: BatchFormatter
    {
        private readonly bool _renameRenderedMessageJsonNode;

        public LogzIoBatchFormatter(long? eventBodyLimitBytes = 256 * ByteSize.KB, bool renameRenderedMessageJsonNode = true) 
            : base(eventBodyLimitBytes)
        {
            _renameRenderedMessageJsonNode = renameRenderedMessageJsonNode;
        }

        public override void Format(IEnumerable<string> logEvents, TextWriter output)
        {
            if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var delimiter = string.Empty;

            foreach (var logEvent in logEvents)
            {
                if (string.IsNullOrWhiteSpace(logEvent))
                {
                    continue;
                }

                var eventToWrite = logEvent;
                if (_renameRenderedMessageJsonNode)
                {
                    // make sure you can see message in logzio kibana view
                    eventToWrite = logEvent.Replace("\"RenderedMessage\"", "\"message\"");
                }

                if (CheckEventBodySize(eventToWrite))
                {
                    output.Write(delimiter);
                    output.Write(eventToWrite);
                    delimiter = ",\n";
                }
            }
        }
    }
}
