namespace Serilog.Sinks.Logz.Io.Ecs
{
    public class LogzIoUrl
    {
        public LogzIoUrl(string urlTemplate, int defaultPort)
        {
            UrlTemplate = urlTemplate;
            DefaultPort = defaultPort;
        }

        public string UrlTemplate { get; }
        public int DefaultPort { get; }

        public string Format(string token, string type, string dataCenter, int? port)
        {
            if (port == null)
                port = DefaultPort;

            return string.Format(UrlTemplate, token, type, dataCenter, port);
        }
    }
}