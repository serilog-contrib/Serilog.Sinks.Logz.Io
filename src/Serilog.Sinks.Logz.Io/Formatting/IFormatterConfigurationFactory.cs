using Elastic.CommonSchema.Serilog;

// ReSharper disable once CheckNamespace
namespace Serilog.Sinks.Logz.Io;

public interface IFormatterConfigurationFactory
{
    EcsTextFormatterConfiguration Create();
}