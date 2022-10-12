# A Serilog sink sending log events over HTTP to logz.io

# Available Packages
## Serilog.Sinks.Logz.Io

[![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.Logz.Io.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io/) 
[![NuGet](https://img.shields.io/nuget/dt/Serilog.Sinks.Logz.Io.svg)](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io/)
[![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/serilog/serilog/wiki)
[![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog)
[![Help](https://img.shields.io/badge/stackoverflow-serilog-orange.svg)](http://stackoverflow.com/questions/tagged/serilog)

__Package__ - [Serilog.Sinks.Logz.Io](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io)
| __Platforms__ - NET 6.0, NET 5.0, .NET Standard 2.0, .NET 4.6.1

### Warning: breaking changes

v7 has breaking changes.

- DEFAULT FIELD NAME FORMATTING SETTINGS MIGHT BE CHANGED DEPENDING ON YOUR CONFIGURATION, PLEASE DOUBLE CHECK
- batchPostingLimit field renamed to logEventsInBatchLimit to match Serilog.Sinks.Http library
- bufferPathFormat renamed to bufferBaseFileName
- in addition rolling interval is now set in separate property: bufferRollingInterval

So now instead of doing:
```
    "bufferPathFormat": "Buffer-{Hour}.json",
```

need to change config file to:
```
    "bufferBaseFileName": "Buffer",
    "bufferRollingInterval": "Hour",
```

- data center configuration is moved to a separate object

So now instead of doing:
```
    "useHttps": true,
    "dataCenterSubDomain": "listener",
    "port": null
```

need to change config file to:
```
    "dataCenter": {
        "useHttps": true,
        "dataCenterSubDomain": "listener",
        "port": null
    }
```

For more information see: https://github.com/FantasticFiasco/serilog-sinks-http/releases/tag/v8.0.0

### Installation

If you want to include the HTTP sink in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/Serilog.Sinks.Logz.Io/).

To install the sink, run the following command in the Package Manager Console:

```
PM> Install-Package Serilog.Sinks.Logz.Io
```

## Serilog.Sinks.Http.LogzIo

This package has been deprecated as it is legacy and is no longer maintained.
Please update to Serilog.Sinks.Logz.Io. Latest version contains exact the same functionality and extension methods to configure loggers.

# Usage 

In the following example, the sink will POST log events to `https://listener-eu.logz.io:8071/?type=app&token=<token>` over HTTP. We configure the sink using **[named arguments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments#named-arguments) instead of positional** because historically we've seen that most breaking changes where the result of a new parameter describing a new feature. Using named arguments means that you more often than not can migrate to new major versions without any changes to your code.

Used in conjunction with [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) the same sink can be configured in the following way:

```json
{
  "Serilog": {
    "MinimumLevel": "Warning",
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

## Internals

Library depends on a fantastic Serilog.Sinks.Http package which allows to create buffered files and controls HTTP uploads.

Here we are using DurableHttpUsingTimeRolledBuffers extension method to configure durable HTTP sink.

See: [DurableHttpUsingTimeRolledBuffers](https://github.com/FantasticFiasco/serilog-sinks-http/blob/v7.2.0/src/Serilog.Sinks.Http/LoggerSinkConfigurationExtensions.cs#L297)

Wiki: [Durable time rolled HTTP sink](https://github.com/FantasticFiasco/serilog-sinks-http/wiki/Durable-time-rolled-HTTP-sink)

## Serializer

Library is using Newtonsoft.Json package for serialization. However it is possible to change/configure serialization behavior using LogzIoSerializer.Instance property.
This instance should be configured at application startup (best if it happens before Serilog configuration).

Default behavior which is set automatically:
```
LogzIoSerializer.Instance = new LogzIoSerializer(LogzIoTextFormatterFieldNaming.CamelCase, false);
```

You can also set custom JsonSerializerSettings using following code:
```
LogzIoSerializer.Instance.WithSerializerSettings(customSetttings);
```

Of course you can override complete serialization logic by implementing ILogzIoSerializer interface and configuring your own implementation.

## Multicast delegates

Newtonsoft.JSON package which is used to serialize log entries is also able to serialize lamdbas, delegates and similar functional constructs which normally does not make sense to have in logs. It also might produce huge blobs with assembly information thus increasing log entry size to unacceptable sizes.

In order to handle this issue MulticastDelegate type serialization is overriden using custom [JsonConverter](https://github.com/serilog-contrib/Serilog.Sinks.Logz.Io/blob/master/src/Serilog.Sinks.Logz.Io/Converters/MulticastDelegateJsonConverter.cs).

If you still want to enable this behavior - at application startup you can configure package serializer using following code:
```
LogzIoSerializer.Instance = new LogzIoSerializer(LogzIoTextFormatterFieldNaming.CamelCase, false);
```

## More advanced examples

> NOTE: in example bellow only `requestUri` is required. All other properties represents default values.

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
          "bufferBaseFileName": "Buffer",
          "bufferRollingInterval": "Day",
          "bufferFileSizeLimitBytes": "104857600",
          "bufferFileShared": false,
          "retainedBufferFileCountLimit": 31,
          "logEventsInBatchLimit": 1000,
          "period": null,
          "restrictedToMinimumLevel": "Minimum",
          "logzioTextFormatterOptions": {
            "BoostProperties": true,
            "LowercaseLevel": true,
            "IncludeMessageTemplate": true,
            "FieldNaming": "CamelCase",
            "EventSizeLimitBytes": 261120
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
- FieldNaming: allows to transform field names, possible values: null (default), CamelCase, LowerCase

## Configuration from code

```csharp
    configuration
        .WriteTo.LogzIoDurableHttp(
            "https://listener-eu.logz.io:8071/?type=app&token=<token>",
            logzioTextFormatterOptions: new LogzioTextFormatterOptions
            {
                BoostProperties = true,
                LowercaseLevel = true,
                IncludeMessageTemplate = true,
                FieldNaming = LogzIoTextFormatterFieldNaming.CamelCase,
                EventSizeLimitBytes = 261120,
            })
        .MinimumLevel.Verbose();
```

# Logs without buffered file

NOTE: it is strongly recommended to use durable logger. It is much more robust and tollerates various network issues thus your logs won't be lost.

In the following example, the sink will POST log events to https://listener.logz.io over HTTPS.

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
        RestrictedToMinimumLevel = LogEventLevel.Debug,
        Period = TimeSpan.FromSeconds(15),
        LogEventsInBatchLimit = 50
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
          "dataCenter": {
              "subDomain": "listener",
              "useHttps": true
          },
          "logEventsInBatchLimit": 5000,
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

## Elastic Common Schema support

See for more details: https://www.elastic.co/guide/en/ecs/current/ecs-reference.html

```csharp
ILogger log = new LoggerConfiguration()
  .MinimumLevel.Verbose()
  .WriteTo.LogzIo(new LogzioEcsOptions
        {
            Type = "<log type>",
            AuthToken = "<logzio token>",
            DataCenter = new LogzioDataCenter
            {
                SubDomain = "listener",
                Port = 8701,
                UseHttps = true
            }
        },
        batchPostingLimit: 50,
        period: TimeSpan.FromSeconds(15),
        restrictedToMinimumLevel: LogEventLevel.Debug
    )
  .CreateLogger();
```

Alternatively configuration can be done within your `appsettings.json` file, please note CustomEcsTextFormatterConfiguration setup is optional:

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.Logz.Io"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "LogzIoEcs",
        "Args": {
          "options": {
            "type": "<log type>",
            "authToken": "<logzio token>"
          },
          "logEventsInBatchLimit": 30,
          "period": "00:00:02",
          "restrictedToMinimumLevel": "Debug",
          "formatterConfiguration": "Serilog.Sinks.Logz.Io.AspNetCoreApi.Logging.CustomEcsTextFormatterConfiguration, Serilog.Sinks.Logz.Io.AspNetCoreApi"
        }
      }
    ]
  }
}
```
