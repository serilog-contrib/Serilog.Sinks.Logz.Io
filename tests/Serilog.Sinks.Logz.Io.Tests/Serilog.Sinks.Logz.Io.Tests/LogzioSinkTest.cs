using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Sinks.Logz.Io.Sinks;
using Serilog.Sinks.Logz.Io.Tests.Support;
using Serilog.Sinks.PeriodicBatching;
using Xunit;

namespace Serilog.Sinks.Logz.Io.Tests;

public class LogzioSinkTest
{
    [Fact]
    public async Task AllLogzIoShouldHaveTimestampAndMessage()
    {
        //Arrange

        var httpData = new List<HttpContent>();

        var sink = new PeriodicBatchingSink(
            new LogzIoSink("testAuthCode", "testTyoe", null, new GoodFakeHttpClient(httpData)),
            LogzIoDefaults.CreateBatchingSinkOptions(100, TimeSpan.FromSeconds(1))
        );

        var log = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        //Act
        var logMsg = "This a Information Log Trace";
        log.Information(logMsg);
        await log.DisposeAsync();

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
        dataDic["level"].Should().Be(LogEventLevel.Information.ToString().ToLower());
    }

    [Fact]
    public async Task ExtraPropertiesShouldBeSentToLogzIo()
    {
        //Arrange
        var httpData = new List<HttpContent>();

        var sink = new PeriodicBatchingSink(
            new LogzIoSink("testAuthCode", "testTyoe", null, new GoodFakeHttpClient(httpData)),
            LogzIoDefaults.CreateBatchingSinkOptions(100, TimeSpan.FromSeconds(1))
        );

        var log = new LoggerConfiguration()
            .Enrich.WithProperty("PropStr1", "banana")
            .Enrich.WithProperty("PropInt1", 42)
            .Enrich.WithProperty("PropInt2", -42)
            .Enrich.WithProperty("PropFloat1", 88.8)
            .Enrich.WithProperty("PropFloat2", -43.5)
            .Enrich.WithProperty("PropBool1", false)
            .Enrich.WithProperty("PropBool2", true)
            .Enrich.WithProperty("PropEnum1", DateTimeKind.Utc)
            .Enrich.WithProperty("PropEnum2", StringComparison.CurrentCultureIgnoreCase)
            .Enrich.WithProperty("PropArr1", new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0})
            .Enrich.WithProperty("PropArr2", new[] {"banana", "apple", "lemon"})
            .Enrich.WithProperty("PropArr3", new object[] {1, "banana", 3.5, false})
            .Enrich.WithProperty("PropNull1", null)
            .Enrich.WithProperty("PropDic1", new Dictionary<string, int> {{"banana", 2}, {"apple", 5}, {"lemon", 76}})
            .Enrich.WithProperty("PropObj1", new {Name = "banana", Itens = new[] {1, 2, 3, 4}, Id = 99, active = true})
            .Enrich.WithProperty("PropObj2", new {Name = "banana", Itens = new[] {1, 2, 3, 4}, Id = 99, active = true}, true)
            .WriteTo.Sink(sink)
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

        var dataDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
        dataDic.Should().ContainKey("@timestamp"); //LogzIo Requered a @timestamp (Iso DateTime) to indicate the time of the event.
        dataDic.Should().ContainKey("message"); //LogzIo Requered a lowercase message string
        dataDic["@timestamp"].Should().NotBeNull();
        dataDic["message"].Should().Be(logMsg);
        dataDic["level"].Should().Be(LogEventLevel.Warning.ToString().ToLower());

        dataDic.Should().ContainKeys("properties.PropStr1", "properties.PropInt1", "properties.PropInt2",
            "properties.PropBool1", "properties.PropArr1", "properties.PropArr2",
            "properties.PropObj1", "properties.PropObj2", "properties.PropNull1",
            "properties.PropDic1", "properties.PropFloat1", "properties.PropFloat2",
            "properties.PropArr3", "properties.PropEnum1", "properties.PropEnum2");

        dataDic["properties.PropStr1"].Should().Be("banana");
        dataDic["properties.PropInt1"].Should().Be(42);
        dataDic["properties.PropInt2"].Should().Be(-42);
        dataDic["properties.PropFloat1"].Should().Be(88.8);
        dataDic["properties.PropFloat2"].Should().Be(-43.5);
        dataDic["properties.PropBool1"].Should().Be(false);
        dataDic["properties.PropBool2"].Should().Be(true);
        dataDic["properties.PropNull1"].Should().BeNull();
        dataDic["properties.PropEnum1"].Should().Be(DateTimeKind.Utc.ToString());
        dataDic["properties.PropEnum2"].Should().Be(StringComparison.CurrentCultureIgnoreCase.ToString());

        var dataDinamic = JObject.Parse(data);
        dataDinamic["properties.PropStr1"].Should().BeNullOrEmpty();

        ValidateArray(dataDinamic["properties.PropArr1"]);
        ValidateArray(dataDinamic["properties.PropArr2"]);
        ValidateArray(dataDinamic["properties.PropArr3"]);

        dataDinamic["properties.PropDic1"].Should().BeOfType<JObject>();
        dataDinamic["properties.PropObj2"].Should().BeOfType<JObject>();

        //TODO More Test for other Props
    }

    private static void ValidateArray(JToken arr1)
    {
        arr1.Should().NotBeNull();
        arr1.Should().BeOfType<JArray>();
//            arr1.Last.Should().BeOfType<JProperty>();
//            arr1.Last.Last.Should().BeOfType<JArray>();
    }

    [Fact]
    public async Task GivenBoostedPropertiesIsDisabled_PropertiesHavePropertiesPrefix()
    {
        //Arrange
        var httpData = new List<HttpContent>();

        var logzioOptions = new LogzioOptions
        {
            TextFormatterOptions = new LogzioTextFormatterOptions {BoostProperties = false}
        };

        var sink = new PeriodicBatchingSink(
            new LogzIoSink("testAuthCode", "testTyoe", logzioOptions, new GoodFakeHttpClient(httpData)),
            LogzIoDefaults.CreateBatchingSinkOptions(100, TimeSpan.FromSeconds(1))
        );

        var log = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .Enrich.WithProperty("EnrichedProperty", "banana")
            .CreateLogger();

        //Act
        var logMsg = "This a Information Log Trace {MessageTemplateProperty}";
        log.Information(logMsg, "pear");
        log.Dispose();

        //Assert
        httpData.Should().NotBeNullOrEmpty();
        httpData.Should().HaveCount(1);

        var data = await httpData.Single().ReadAsStringAsync();
        data.Should().NotBeNullOrWhiteSpace();
        var dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

        dataDic["properties.EnrichedProperty"].Should().Be("banana");
        dataDic["properties.MessageTemplateProperty"].Should().Be("pear");
    }

    [Fact]
    public async Task GivenBoostPropertiesIsEnabled_PropertiesDoNotHavePropertiesPrefix()
    {
        //Arrange
        var httpData = new List<HttpContent>();

        var logzioOptions = new LogzioOptions
        {
            TextFormatterOptions = new LogzioTextFormatterOptions { BoostProperties = true }
        };

        var sink = new PeriodicBatchingSink(
            new LogzIoSink("testAuthCode", "testTyoe", logzioOptions, new GoodFakeHttpClient(httpData)),
            LogzIoDefaults.CreateBatchingSinkOptions(100, TimeSpan.FromSeconds(1))
        );

        var log = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .Enrich.WithProperty("EnrichedProperty", "banana")
            .CreateLogger();

        //Act
        var logMsg = "This a Information Log Trace {MessageTemplateProperty}";
        log.Information(logMsg, "pear");
        log.Dispose();

        //Assert
        httpData.Should().NotBeNullOrEmpty();
        httpData.Should().HaveCount(1);

        var data = await httpData.Single().ReadAsStringAsync();
        data.Should().NotBeNullOrWhiteSpace();
        var dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

        dataDic["enrichedProperty"].Should().Be("banana");
        dataDic["messageTemplateProperty"].Should().Be("pear");
    }

    [Fact]
    public async Task GivenIncludeMessageTemplateIsEnabled_MessageTemplateShouldBeIncluded()
    {
        //Arrange
        var httpData = new List<HttpContent>();

        var logzioOptions = new LogzioOptions
        {
            TextFormatterOptions = new LogzioTextFormatterOptions { IncludeMessageTemplate = true }
        };

        var sink = new PeriodicBatchingSink(
            new LogzIoSink("testAuthCode", "testTyoe", logzioOptions, new GoodFakeHttpClient(httpData)),
            LogzIoDefaults.CreateBatchingSinkOptions(100, TimeSpan.FromSeconds(1))
        );

        var log = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        //Act
        var logMsg = "This a Information Log Trace {MessageTemplateProperty}";
        log.Information(logMsg, "pear");
        log.Dispose();

        //Assert
        httpData.Should().NotBeNullOrEmpty();
        httpData.Should().HaveCount(1);

        var data = await httpData.Single().ReadAsStringAsync();
        data.Should().NotBeNullOrWhiteSpace();
        var dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

        dataDic["messageTemplate"].Should().Be(logMsg);
    }

    [Fact]
    public async Task GivenExceptionWithPropertyThrowingException_ExcludesThrowingPropertyInJson()
    {
        //Arrange
        var httpData = new List<HttpContent>();

        var logzioOptions = new LogzioOptions
        {
            TextFormatterOptions = new LogzioTextFormatterOptions { IncludeMessageTemplate = true }
        };

        var sink = new PeriodicBatchingSink(
            new LogzIoSink("testAuthCode", "testTyoe", logzioOptions, new GoodFakeHttpClient(httpData)),
            LogzIoDefaults.CreateBatchingSinkOptions(100, TimeSpan.FromSeconds(1))
        );

        var log = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .CreateLogger();

        //Act
        try
        {
            throw new ExceptionWithThrowingProperty();
        }
        catch (Exception e)
        {
            log.Error(e, "Error message");
        }

        log.Information("Info message");
        log.Dispose();

        //Assert
        var contentString = await httpData.Should().ContainSingle().Which.ReadAsStringAsync();
        contentString.Should().NotBeNullOrWhiteSpace();

        List<JObject> logEvents = contentString
            .ReplaceLineEndings()
            .Split("," + Environment.NewLine)
            .Select(JObject.Parse)
            .ToList();

        logEvents.Should().HaveCount(2, because: "both info and error message should be logged");
        logEvents.Should().ContainSingle(x => x["messageTemplate"].Value<string>() == "Info message");

        var errorEvent = logEvents.Should().ContainSingle(x => x["messageTemplate"].Value<string>() == "Error message").Which;
        var exceptionValue = errorEvent.Should().ContainKey("exception").WhoseValue.ToObject<JObject>();
        exceptionValue.Should().ContainKey("Message").WhoseValue.Value<string>().Should().Be("Exception of type 'Serilog.Sinks.Logz.Io.Tests.LogzioSinkTest+ExceptionWithThrowingProperty' was thrown.");
        exceptionValue.Should().ContainKey(nameof(ExceptionWithThrowingProperty.StringProperty)).WhoseValue.Value<string>().Should().Be("I'm OK.");
        exceptionValue.Should().NotContainKey(nameof(ExceptionWithThrowingProperty.ThrowingProperty));
    }

    public class ExceptionWithThrowingProperty : Exception
    {
        public string StringProperty => "I'm OK.";
        public string ThrowingProperty => throw new InvalidOperationException("ExceptionWithThrowingProperty.Throwing property throws to test JSON serialization.");
    }
}