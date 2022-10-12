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

using Serilog.Debugging;
using Serilog.Sinks.Http;

// ReSharper disable once CheckNamespace
namespace Serilog.Sinks.Logz.Io;

public class LogzIoBatchFormatter : IBatchFormatter
{
    private readonly long? _eventBodyLimitBytes;
    private readonly bool _renameRenderedMessageJsonNode;

    public LogzIoBatchFormatter(long? eventBodyLimitBytes = 256 * ByteSize.KB, bool renameRenderedMessageJsonNode = true)
    {
        _eventBodyLimitBytes = eventBodyLimitBytes;
        _renameRenderedMessageJsonNode = renameRenderedMessageJsonNode;
    }

    public void Format(IEnumerable<string> logEvents, TextWriter output)
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

    protected bool CheckEventBodySize(string json)
    {
        if (_eventBodyLimitBytes.HasValue && ByteSize.From(json) > _eventBodyLimitBytes.Value)
        {
            SelfLog.WriteLine(
                "Event JSON representation exceeds the byte size limit of {0} set for this sink and will be dropped; data: {1}",
                _eventBodyLimitBytes,
                json);

            return false;
        }

        return true;
    }
}