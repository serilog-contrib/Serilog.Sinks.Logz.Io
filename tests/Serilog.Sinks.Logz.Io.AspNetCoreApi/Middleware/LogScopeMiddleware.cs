using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Serilog.Sinks.Logz.Io.AspNetCoreApi.Middleware
{
    public class LogScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogScopeMiddleware> _logger;

        public LogScopeMiddleware(RequestDelegate next, ILogger<LogScopeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // it must be a KeyValuePair<string, object> other types does not work
            var stateList = new List<KeyValuePair<string, object>>
            {
                new("user", "mantas@mentalist.dev")
            };

            using var scope = _logger.BeginScope(stateList);
            {
                await _next(context);
            }
        }
    }
}
