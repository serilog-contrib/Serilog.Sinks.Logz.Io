using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Sinks.Logz.Io.Client;

namespace Serilog.Sinks.Logz.Io.Tests.Support
{
    class GoodFakeHttpClient : IHttpClient, IDisposable
    {
        public List<HttpContent> HttpSendData { get; }

        public GoodFakeHttpClient(List<HttpContent> httpSendData)
        {
            HttpSendData = httpSendData;
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            HttpSendData.Add(content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public void Dispose()
        {
            //Nothing;
        }
    }

    class BadRequestFakeHttpClient : IHttpClient, IDisposable
    {
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return Task.FromResult( new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(string.Empty )});
        }

        public void Dispose()
        {
            //Nothing;
        }
    }

    class ErrorFakeHttpClient : IHttpClient, IDisposable
    {
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }

        public void Dispose()
        {
            //Nothing;
        }
    }
}
