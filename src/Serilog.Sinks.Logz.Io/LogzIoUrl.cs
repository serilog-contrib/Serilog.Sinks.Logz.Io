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

namespace Serilog.Sinks.Logz.Io;

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
        if (port == null || port == 0)
            port = DefaultPort;

        return string.Format(UrlTemplate, token, type, dataCenter, port);
    }
}