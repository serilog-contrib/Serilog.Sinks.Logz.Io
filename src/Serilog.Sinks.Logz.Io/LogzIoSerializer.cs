// Copyright 2015-2016 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog.Sinks.Logz.Io.Converters;

namespace Serilog.Sinks.Logz.Io;

public interface ILogzIoSerializer
{
    string Serialize<T>(T value);
}

public class LogzIoSerializer: ILogzIoSerializer
{
    public static ILogzIoSerializer Instance { get; set; } = new LogzIoSerializer(LogzIoTextFormatterFieldNaming.CamelCase);

    public JsonSerializerSettings SerializerSettings { get; private set; }

    public LogzIoSerializer(LogzIoTextFormatterFieldNaming fieldNaming, bool includeMulticastDelegateConverter = true)
    {
        SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        NamingStrategy namingStrategy = fieldNaming == LogzIoTextFormatterFieldNaming.CamelCase
            ? new CamelCaseNamingStrategy()
            : new DefaultNamingStrategy();

        SerializerSettings.Converters.Add(new StringEnumConverter(namingStrategy));

        if (includeMulticastDelegateConverter)
        {
            SerializerSettings.Converters.Add(new MulticastDelegateJsonConverter());
        }
    }

    public ILogzIoSerializer WithSerializerSettings(JsonSerializerSettings settings)
    {
        SerializerSettings = settings;
        return this;
    }

    public string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, SerializerSettings);
    }
}