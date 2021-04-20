# Serilog.Sinks.Http.LogzIo - A Serilog sink sending log events over HTTP to logz.io

## Super simple to use

In the following example, the sink will POST log events to `https://listener-eu.logz.io:8071/?type=app&token=<token>` over HTTP. We configure the sink using **[named arguments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#named-arguments) instead of positional** because historically we've seen that most breaking changes where the result of a new parameter describing a new feature. Using named arguments means that you more often than not can migrate to new major versions without any changes to your code.

Used in conjunction with [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) the same sink can be configured in the following way:

```json
{
  "Serilog": {
    "MinimumLevel": "Verbose",
    "WriteTo": [
      {
        "Name": "LogzIoDurableHttp",
        "Args": {
          "requestUri": "https://listener-eu.logz.io:8071/?type=app&token=<token>"
        }
      }
    ]
  }
}
```

The sink will be configured as durable, i.e. log events are persisted on disk before being sent over the network, thus protected against data loss after a system or process restart. For more information please read the [wiki](https://github.com/FantasticFiasco/serilog-sinks-http/wiki).

## More advanced examples

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.AspNetCore": "Debug"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "WriteTo": [
      {
        "Name": "LogzIoDurableHttp",
        "Args": {
          "requestUri": "https://listener-eu.logz.io:8071/?type=app&token=<token>",
          "bufferPathFormat": "C:/Temp/logzio/buffer-{Hour}.json",
          "bufferFileSizeLimitBytes": "104857600",
          "batchPostingLimit": 100,
          "logzioTextFormatterOptions": {
            "BoostProperties": true,
            "LowercaseLevel": true,
            "IncludeMessageTemplate": true
          }
        }
      }
    ]
  }
}
```

In the example above logzIo formatter options are default, there is no need to specify these if it fits your needs.

- BoostProperties: When true, does not add 'properties' prefix.
- LowercaseLevel: Set to true to push log level as lowercase.
- IncludeMessageTemplate: When true the message template is included in the logs.

## Configuration from code

```csharp
    configuration
        .WriteTo.LogzIoDurableHttp(
            "https://listener-eu.logz.io:8071/?type=app&token=<token>",
            logzioTextFormatterOptions: new LogzioTextFormatterOptions
            {
                BoostProperties = true,
                IncludeMessageTemplate = true,
                LowercaseLevel = true
            })
        .MinimumLevel.Verbose();
```

## Install via NuGet

If you want to include the HTTP sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Http.LogzIo/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Http.LogzIo
```
