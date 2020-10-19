# Serilog.Sinks.Logz.Io - A Serilog sink sending log events over HTTP

[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Logz.Io.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io/) 
[![NuGet](https://img.shields.io/nuget/dt/Serilog.Sinks.Logz.Io.svg)](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io/)
[![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki)
[![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog)
[![Help](https://img.shields.io/badge/stackoverflow-serilog-orange.svg)](http://stackoverflow.com/questions/tagged/serilog)

__Package__ - [Serilog.Sinks.Logz.Io](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io)
| __Platforms__ - .NET 4.5, .NET Standard 2.0

## Table of contents

- [Super simple to use](#super-simple-to-use)
- [Install via NuGet](#install-via-nuget)

---

## Super simple to use

In the following example, the sink will POST log events to https://app.logz.io over HTTPS.

```csharp
ILogger log = new LoggerConfiguration()
  .MinimumLevel.Verbose()
  .WriteTo.LogzIo("<logzio token>", "<log type>", useHttps: true)
  .CreateLogger();

log.Information("Logging {@Heartbeat} from {Computer}", heartbeat, computer);
```

More advanced configuration is also available:

```csharp
ILogger log = new LoggerConfiguration()
  .MinimumLevel.Verbose()
  .WriteTo.LogzIo("<logzio token>", "<log type>",
    new LogzioOptions 
    { 
        UseHttps = true, 
        RestrictedToMinimumLevel = LogEventLevel.Debug,
        Period = TimeSpan.FromSeconds(15),
        BatchPostingLimit = 50
    })
  .CreateLogger();
```

Alternatively configuration can be done within your `appsettings.json` file:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Verbose"
    },
    "WriteTo": [
      {
        "Name": "LogzIo",
        "Args": {
          "authToken": "<logzio token>",
          "type": "<log type>",
          "dataCenterSubDomain": "listener",
          "useHttps": true,
          "batchPostingLimit": 5000,
          "period": "00:00:02",
          "restrictedToMinimumLevel": "Debug",
          "lowercaseLevel": false,
          "environment": "",
          "serviceName": ""
        }
      }
    ]
  }
}
```

## Install via NuGet

If you want to include the HTTP sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Logz.Io
```
