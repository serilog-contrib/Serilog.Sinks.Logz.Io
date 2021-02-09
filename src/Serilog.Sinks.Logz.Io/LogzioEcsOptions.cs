using Elastic.CommonSchema.Serilog;

namespace Serilog.Sinks.Logz.Io
{
    public class LogzioEcsOptions
    {
        public string Type { get; set; } = null!;

        public string AuthToken { get; set; } = null!;

        public LogzioDataCenter? DataCenter { get; set; }
    }

    public class LogzioDataCenter
    {
        /// <summary>
        /// The data center specific endpoint sub domain to use, select one of the following
        /// 1) listener (default) = US
        /// 2) listener-eu = UE
        /// </summary>
        public string? SubDomain { get; set; }
        public int? Port { get; set; }
        public bool UseHttps { get; set; } = true;
    }

    public interface IFormatterConfigurationFactory
    {
        EcsTextFormatterConfiguration Create();
    }

    public class FormatterConfigurationFactory : IFormatterConfigurationFactory
    {
        public EcsTextFormatterConfiguration Create()
        {
            return new EcsTextFormatterConfiguration();
        }
    }
}
