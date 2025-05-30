using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventsWebApi.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyMiddleware : Attribute, IAsyncActionFilter
{
    private const string headerName = "X-API-KEY";
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = config["SecretKeys:ApiKey"];

        if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out var providedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid or missing Api-Key");
            return;
        }

        if (!string.Equals(providedApiKey, apiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid Api-Key provided");
            return;
        }

        await next();
    }
}
