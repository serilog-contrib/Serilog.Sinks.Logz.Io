using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                .WriteTo.Sink(new LogzioSink(new GoodFakeHttpClient(httpData), "testAuthCode", "testTyoe", 100,
                    TimeSpan.FromSeconds(1)))
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

        [Fact]
        public async Task ExtraPropertiesShouldBeSentToLogzIo()
        {
            //Arrange
            var httpData = new List<HttpContent>();
            var log = new LoggerConfiguration()
                .Enrich.WithProperty("PropStr1", "banana")
                .Enrich.WithProperty("PropInt1", 42)
                .Enrich.WithProperty("PropInt2", -42)
                .Enrich.WithProperty("PropBool1", false)
                .Enrich.WithProperty("PropBool2", true)
                .Enrich.WithProperty("PropArr1", new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0})
                .Enrich.WithProperty("PropArr2", new[] {"banana", "apple", "lemon"})
                .Enrich.WithProperty("PropNull1", null )
                .Enrich.WithProperty("PropDic1", new Dictionary<string, int> {{"banana", 2}, {"apple", 5}, {"lemon", 76}} )
                .Enrich.WithProperty("PropObj1", new { Name = "banana", Itens = new[] {1, 2, 3, 4}, Id = 99, actice = true})
                .Enrich.WithProperty("PropObj2", new { Name = "banana", Itens = new[] {1, 2, 3, 4}, Id = 99, actice = true}, true)
                .WriteTo.Sink(new LogzioSink(new GoodFakeHttpClient(httpData), "testAuthCode", "testTyoe", 100, TimeSpan.FromSeconds(1)))
                .CreateLogger();

            //Act
            var logMsg = "This a Information Log Trace";
            log.Warning(logMsg);
            log.Dispose();

            //Assert
            httpData.Should().NotBeNullOrEmpty();
            httpData.Should().HaveCount(1);

            var data = await httpData.Single().ReadAsStringAsync();
            data.Should().NotBeNullOrWhiteSpace();
            
            var dataDic = JsonConvert.DeserializeObject<Dictionary<string, Object>>(data);
            dataDic.Should().ContainKey("@timestamp"); //LogzIo Requered a @timestamp (Iso DateTime) to indicate the time of the event.
            dataDic.Should().ContainKey("message"); //LogzIo Requered a lowercase message string
            dataDic["@timestamp"].Should().NotBeNull();
            dataDic["message"].Should().Be(logMsg);
            dataDic["level"].Should().Be(LogEventLevel.Warning.ToString());

            dataDic.Should().ContainKeys("properties.PropStr1", "properties.PropInt1", "properties.PropInt2", 
                                         "properties.PropBool1", "properties.PropArr1", "properties.PropArr2", 
                                         "properties.PropObj1", "properties.PropObj2", "properties.PropNull1", "properties.PropDic1");

            dataDic["properties.PropStr1"].Should().Be("banana");
            dataDic["properties.PropInt1"].Should().Be(42);
            dataDic["properties.PropInt2"].Should().Be(-42);
            dataDic["properties.PropBool1"].Should().Be(false);
            dataDic["properties.PropBool2"].Should().Be(true);
            dataDic["properties.PropNull1"].Should().BeNull();

            var dataDinamic = JObject.Parse(data);
            dataDinamic["properties.PropStr1"].Should().BeNullOrEmpty();
            dataDinamic["properties.PropArr1"].Should().BeOfType<JArray>();
            dataDinamic["properties.PropArr2"].Should().BeOfType<JArray>();
            dataDinamic["properties.PropDic1"].Should().BeOfType<JObject>();
            dataDinamic["properties.PropObj2"].Should().BeOfType<JObject>();

            //TODO More Test for other Props

        }
    }
}