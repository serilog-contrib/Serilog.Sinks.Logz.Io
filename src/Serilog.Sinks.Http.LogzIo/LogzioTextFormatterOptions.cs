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
        /// Specifies how to rename properties before sending to target
        /// </summary>
        public Dictionary<string, string> PropertyTransformationMap { get; set; } = new()
        {
            {"SourceContext", "logger"},
            {"ThreadId", "thread"},
        };
    }
}
