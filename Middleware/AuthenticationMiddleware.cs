using System.Threading.Tasks;

namespace UserManagementAPI.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] validTokens = { "mysecrettoken123", "another-valid-token" };

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Unauthorized: Missing or invalid token.\"}");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (!validTokens.Contains(token))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Unauthorized: Invalid token.\"}");
            return;
        }

        await _next(context);
    }
}