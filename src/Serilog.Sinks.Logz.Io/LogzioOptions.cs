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

namespace Serilog.Sinks.Logz.Io;

public class LogzioOptions
{
    /// <summary>
    /// LogzIo Data center
    /// </summary>
    public LogzioDataCenter? DataCenter { get; set; }

    /// <summary>
    /// The maximum number of events to post in a single batch. The default is <see cref="LogzIoDefaults.DefaultBatchPostingLimit"/>.
    /// </summary>
    public int? LogEventsInBatchLimit { get; set; }

    /// <summary>
    /// The time to wait between checking for event batches. The default is <see cref="LogzIoDefaults.DefaultPeriod"/>.
    /// </summary>
    public TimeSpan? Period { get; set; }

    /// <summary>
    /// The minimum level for events passed through the sink. The default is <see cref="LogEventLevel.Verbose"/>.
    /// </summary>
    public LogEventLevel? RestrictedToMinimumLevel { get; set; }

    /// <summary>
    /// Set to true to push all property names up to the event instead of using Properties.property_name
    /// </summary>
    public bool BoostProperties { get; set; } = false;

    /// <summary>
    /// Set to true to push lowercased log level
    /// </summary>
    public bool LowercaseLevel { get; set; } = false;
        
    /// <summary>
    /// Set environment name, default is empty and not sent to server
    /// </summary>
    public string? Environment { get; set; } = string.Empty;
        
    /// <summary>
    /// Set service name, default is empty and not sent to server
    /// </summary>
    public string? ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Set to true to include message template
    /// </summary>
    public bool IncludeMessageTemplate { get; set; } = false;

    /// <summary>
    /// Specifies how to rename properties before sending to target
    /// </summary>
    public Dictionary<string, string> PropertyTransformationMap { get; set; } = new()
    {
        {"SourceContext", "logger"},
        {"ThreadId", "thread"},
    };

    /// <summary>
    /// Gets or sets the failure callback.
    /// </summary>
    public Action<Exception>? FailureCallback { get; set; } = null;
}
