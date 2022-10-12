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

        MapCustom((log, logEvent) =>
        {
            if (options.LowercaseLevel && log.Log != null && !string.IsNullOrWhiteSpace(log.Log.Level))
            {
                log.Log.Level = log.Log.Level?.ToLower();
            }

            // logzio does not represent _metadata field in UI..
            log.Labels = log.Metadata;
            log.Metadata = null;
            return log;
        });
    }
}