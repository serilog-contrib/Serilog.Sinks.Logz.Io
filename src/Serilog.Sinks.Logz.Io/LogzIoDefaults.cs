using Serilog.Debugging;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logz.Io;

public static class LogzIoDefaults
{
    public static string OverrideLogzIoUrl { get; set; } = string.Empty;
    public static int DefaultBatchPostingLimit => 1000;
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

    private static readonly LogzIoUrl LogzIoHttpUrl = new("http://{2}.logz.io:{3}/?token={0}&type={1}", 8070);
    private static readonly LogzIoUrl LogzIoHttpsUrl = new("https://{2}.logz.io:{3}/?token={0}&type={1}", 8071);

    public static string GetUrl(string authToken, string type, LogzioDataCenter? dataCenter)
    {
        if (!string.IsNullOrWhiteSpace(OverrideLogzIoUrl))
        {
            return OverrideLogzIoUrl;
        }

        if (string.IsNullOrWhiteSpace(authToken))
        {
            SelfLog.WriteLine("LogzIo token is not specified! Sink will not send any events.");
            return string.Empty;
        }

        var dataCenterSubDomain = dataCenter?.SubDomain ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dataCenterSubDomain))
        {
            dataCenterSubDomain = "listener";
        }

        var dataCenterPort = dataCenter?.Port ?? 0;
        if (dataCenterPort == 0)
            dataCenterPort = 8071;

        var useHttps = dataCenter?.UseHttps ?? true;

        return useHttps
            ? LogzIoHttpsUrl.Format(authToken, type, dataCenterSubDomain, dataCenterPort)
            : LogzIoHttpUrl.Format(authToken, type, dataCenterSubDomain, dataCenterPort);
    }

    public static PeriodicBatchingSinkOptions CreateBatchingSinkOptions(int? batchPostingLimit, TimeSpan? period)
    {
        return new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchPostingLimit ?? DefaultBatchPostingLimit,
            Period = period ?? DefaultPeriod
        };
    }
}