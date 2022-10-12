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

using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http;

// ReSharper disable once CheckNamespace
namespace Serilog.Sinks.Logz.Io;

public class LogzIoTextFormatter : ITextFormatter
{
    private readonly FormattingOptions _formattingOptions;

    // ReSharper disable once UnusedMember.Global
    public LogzIoTextFormatter(): this(null)
    {
    }

    public LogzIoTextFormatter(LogzioTextFormatterOptions? options)
    {
        options ??= new LogzioTextFormatterOptions();
        _formattingOptions = new FormattingOptions(options.FieldNaming)
        {
            BoostProperties = options.BoostProperties,
            IncludeMessageTemplate = options.IncludeMessageTemplate,
            LowercaseLevel = options.LowercaseLevel,
            PropertyTransformationMap = options.PropertyTransformationMap,
            EventSizeLimitBytes = options.EventSizeLimitBytes ?? 255 * ByteSize.KB
        };
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var content = logEvent.Format(_formattingOptions);
        if (!string.IsNullOrWhiteSpace(content))
        {
            output.WriteLine(content);
        }
    }
}