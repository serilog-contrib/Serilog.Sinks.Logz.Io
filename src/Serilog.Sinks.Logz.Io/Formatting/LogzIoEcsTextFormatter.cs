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

using System.Text;
using Elastic.CommonSchema.Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Logz.Io.Formatting;

// ReSharper disable once CheckNamespace
namespace Serilog.Sinks.Logz.Io;

public class LogzIoEcsTextFormatter : ITextFormatter
{
    private readonly IEcsTextFormatterConfiguration _configuration;

    public LogzIoEcsTextFormatter(): this(null)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public LogzIoEcsTextFormatter(LogzioEcsTextFormatterOptions? options)
    {
        _configuration = new LogzIoEcsTextFormatterConfiguration(options);
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        try
        {
            using var stream = FormatToStream(logEvent, _configuration);
            output.Write(Encoding.UTF8.GetString(stream.ToArray()));
        }
        catch (Exception ex)
        {
            LogAsInternalEvent(logEvent, ex);
        }
    }

    private MemoryStream FormatToStream(LogEvent logEvent, IEcsTextFormatterConfiguration formatterConfiguration)
    {
        var memoryStream = new MemoryStream();

        var writer = new StreamWriter(memoryStream) { AutoFlush = true };

        var ecsEvent = LogEventConverter.ConvertToEcs(logEvent, formatterConfiguration);
        ecsEvent.Serialize(writer.BaseStream);
        writer.WriteLine();

        memoryStream.Position = 0;

        return memoryStream;
    }

    private static void LogAsInternalEvent(LogEvent logEvent, Exception e)
    {
        SelfLog.WriteLine(
            "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
            logEvent.Timestamp.ToString("o"),
            logEvent.MessageTemplate.Text,
            e);
    }
}