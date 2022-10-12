using Elastic.CommonSchema.Serilog;

// ReSharper disable once CheckNamespace
namespace Serilog.Sinks.Logz.Io;

public class FormatterConfigurationFactory : IFormatterConfigurationFactory
{
    public EcsTextFormatterConfiguration Create()
    {
        return new EcsTextFormatterConfiguration();
    }
}