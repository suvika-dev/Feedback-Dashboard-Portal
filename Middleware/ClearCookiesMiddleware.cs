using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace FDP.Middleware
{
    public class ClearCookiesMiddleware
    {
        private readonly RequestDelegate _next;

        public ClearCookiesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Clear all cookies
            foreach (var cookie in context.Request.Cookies.Keys)
            {
                context.Response.Cookies.Delete(cookie);
            }

            await _next(context);
        }
    }
}
