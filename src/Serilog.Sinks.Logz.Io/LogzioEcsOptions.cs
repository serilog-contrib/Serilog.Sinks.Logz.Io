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

using Elastic.CommonSchema.Serilog;

namespace Serilog.Sinks.Logz.Io;

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