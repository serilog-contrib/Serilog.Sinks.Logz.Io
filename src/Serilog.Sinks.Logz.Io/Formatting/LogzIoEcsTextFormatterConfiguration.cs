using Elastic.CommonSchema.Serilog;

namespace Serilog.Sinks.Logz.Io.Formatting;

public class LogzIoEcsTextFormatterConfiguration: EcsTextFormatterConfiguration
{
    public LogzIoEcsTextFormatterConfiguration(): this(null)
    {
    }

    public LogzIoEcsTextFormatterConfiguration(LogzioEcsTextFormatterOptions? options)
    {
        options ??= new LogzioEcsTextFormatterOptions();


        MapCustom = (doc, _) =>
        {
            if (options.LowercaseLevel && doc.Log != null && !string.IsNullOrWhiteSpace(doc.Log.Level))
            {
                doc.Log.Level = doc.Log.Level?.ToLower();
            }

            doc.Labels = doc.Labels;
            doc.Metadata = null;
            return doc;
        };
    }
}