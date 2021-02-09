using System;
using Elastic.CommonSchema;
using Elastic.CommonSchema.Serilog;
using Serilog.Events;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging
{
    public class CustomEcsTextFormatterConfiguration: IEcsTextFormatterConfiguration
    {
        public CustomEcsTextFormatterConfiguration()
        {
            MapCustom = (log, logEvent) =>
            {
                if (log.Metadata.TryGetValue("user", out var userValue))
                {
                    var user = userValue.ToString();
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        log.User ??= new User();
                        log.User.Name = user;
                        log.Metadata.Remove("user");
                    }
                }

                if (log.Metadata.TryGetValue(LoggerFields.ServiceName, out var serviceValue))
                {
                    var service = serviceValue.ToString();
                    if (!string.IsNullOrWhiteSpace(service))
                    {
                        log.Service ??= new Service();
                        log.Service.Name = service;
                    }
                    log.Metadata.Remove(LoggerFields.ServiceName);
                }

                /*
                if (log.Metadata.TryGetValue(LoggerFields.Environment, out var environmentValue))
                {
                    var environment = environmentValue.ToString();
                    if (!string.IsNullOrWhiteSpace(environment))
                    {
                        log.Labels ??= new Dictionary<string, object>();
                        log.Labels[LoggerFields.Environment] = environment;
                    }
                    log.Metadata.Remove(LoggerFields.Environment);
                }
                */

                // logzio does not represent _metadata field in UI..
                log.Labels = log.Metadata;
                log.Metadata = null;

                return log;
            };
        }

        public bool MapCurrentThread { get; set; }
        public Func<Base, LogEvent, Base> MapCustom { get; set; }
        public bool MapExceptions { get; set; }
        public IHttpAdapter MapHttpAdapter { get; set; }
    }
}
