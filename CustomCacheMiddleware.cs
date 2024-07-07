using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class CustomCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public CustomCacheMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Get)
        {
            var cacheKey = GenerateCacheKeyFromRequest(context.Request);

            // Try to get the cached response
            var cachedResponse = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                // Serve the cached response
                context.Response.ContentType = "application/json"; // Adjust according to your content type
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;
                await _next(context);

                // Cache the response
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                await _cache.SetStringAsync(cacheKey, responseBodyText);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
       
    }

    private string GenerateCacheKeyFromRequest(HttpRequest request)
    {
        // Create a unique cache key based on the request
        var keyBuilder = new StringBuilder();
        keyBuilder.Append($"{request.Path}");
        foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
        {
            keyBuilder.Append($"|{key}-{value}");
        }
        return keyBuilder.ToString();
    }
}
