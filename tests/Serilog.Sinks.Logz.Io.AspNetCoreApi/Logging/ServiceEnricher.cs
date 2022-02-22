using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging
{
    public class ServiceEnricher: ILogEventEnricher
    {
        private readonly string _serviceName;
        private readonly string _environment;

        public ServiceEnricher(IConfiguration configuration)
        {
            _serviceName = configuration.GetValue("Service:Name", string.Empty);
            _environment = configuration.GetValue("Service:Environment", string.Empty);
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!string.IsNullOrWhiteSpace(_serviceName))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(LoggerFields.ServiceName, new ScalarValue(_serviceName)));
            }

            if (!string.IsNullOrWhiteSpace(_environment))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(LoggerFields.Environment, new ScalarValue(_environment)));
            }
        }
    }
}
