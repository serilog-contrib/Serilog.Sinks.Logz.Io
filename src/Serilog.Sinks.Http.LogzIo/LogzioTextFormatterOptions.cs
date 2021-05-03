using System.Collections.Generic;

namespace Serilog.Sinks.Http.LogzIo
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
    }

    public enum LogzIoTextFormatterFieldNaming
    {
        CamelCase,
        LowerCase
    }
}
