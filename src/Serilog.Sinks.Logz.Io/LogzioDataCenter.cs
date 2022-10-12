namespace Serilog.Sinks.Logz.Io;

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