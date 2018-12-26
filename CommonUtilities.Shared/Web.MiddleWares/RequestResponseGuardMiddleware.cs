#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System.Diagnostics;
    using System.Threading.Tasks;
    public class RequestResponseGuardMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestResponseGuardMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            //Console.WriteLine("Request begin ...");
            stopwatch.Start();
            context
                .Response
                .OnStarting
                    (
                        () =>
                        {
                            stopwatch.Stop();
                            var duration = stopwatch.ElapsedMilliseconds;
                            context
                                .Response
                                .Headers["X-Request-Response-Timing"] = duration.ToString();
                            //Console.WriteLine($"Response end {duration}!!!");
                            return
                                Task.CompletedTask;
                        }
                    );
            await _next(context);
            stopwatch.Stop();
            stopwatch = null;
        }
    }
    public static class RequestResponseGuardMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseGuard
            (
                this IApplicationBuilder target
            )
        {
            return
                target
                    .UseMiddleware
                        <RequestResponseGuardMiddleware>();
        }
    }
}
#endif