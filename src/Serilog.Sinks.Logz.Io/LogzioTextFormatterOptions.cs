using System.Collections.Generic;

namespace Serilog.Sinks.Logz.Io
{
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
}