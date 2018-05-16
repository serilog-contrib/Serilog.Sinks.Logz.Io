using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Sinks.Logz.Io.Tests.Support;
using Xunit;

namespace Serilog.Sinks.Logz.Io.Tests
{
    public class LogzioSinkTest
    {
        [Fact]
        public async Task AllLogzIoShoyldHaveTimestampAndMessage()
        {
            //Arrange
            var httpData = new List<HttpContent>();
            var log = new LoggerConfiguration()
                .WriteTo.Sink(new LogzioSink(new GoodFakeHttpClient(httpData), "testAuthCode", "testTyoe", 100, TimeSpan.FromSeconds(1)))
                .CreateLogger();

            //Act
            var logMsg = "This a Information Log Trace";
            log.Information(logMsg);
            log.Dispose();

            //Assert
            httpData.Should().NotBeNullOrEmpty();
            httpData.Should().HaveCount(1);

            var data = await httpData.Single().ReadAsStringAsync();
            data.Should().NotBeNullOrWhiteSpace();
            var dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            dataDic.Should().ContainKey("@timestamp"); //LogzIo Requered a @timestamp (Iso DateTime) to indicate the time of the event.
            dataDic.Should().ContainKey("message"); //LogzIo Requered a lowercase message string
            dataDic["@timestamp"].Should().NotBeNullOrWhiteSpace();
            dataDic["message"].Should().Be(logMsg); 
            dataDic["level"].Should().Be(LogEventLevel.Information.ToString());
        }

    }
}
