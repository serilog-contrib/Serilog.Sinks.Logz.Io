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

namespace Serilog.Sinks.Logz.Io;

public class LogzioTextFormatterOptions
{
    /// <summary>
    /// Set to true to push all property names up to the event instead of using Properties.property_name
    /// </summary>
    public bool BoostProperties { get; set; } = true;

    /// <summary>
    /// Set to true to push lowercased log level
    /// </summary>
    public bool LowercaseLevel { get; set; } = true;

    /// <summary>
    /// Set to true to include message template
    /// </summary>
    public bool IncludeMessageTemplate { get; set; } = true;

    /// <summary>
    /// Enables field name transformation
    /// </summary>
    public LogzIoTextFormatterFieldNaming? FieldNaming { get; set; }

    /// <summary>
    /// Specifies how to rename field names before sending to target
    /// </summary>
    public Dictionary<string, string>? FieldNameTransformationMap { get; set; } = new()
    {
        {"SourceContext", "Logger"},
        {"ThreadId", "Thread"}
    };

    /// <summary>
    /// Limit single event size, default 255kb
    /// </summary>
    public long? EventSizeLimitBytes { get; set; }
}