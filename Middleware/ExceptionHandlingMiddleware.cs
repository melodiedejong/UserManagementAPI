namespace UserManagementAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorResponse = System.Text.Json.JsonSerializer.Serialize(new { error = "Internal server error." });
            await context.Response.WriteAsync(errorResponse);
            Console.WriteLine($"Unhandled exception: {ex}");
        }
    }
}