using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TerraNotes.Data;

public class APIKeyAuthFilter : IAsyncAuthorizationFilter
{
    private readonly AppDbContext _context;
    private readonly ILogger<APIKeyAuthFilter> _logger;

    public APIKeyAuthFilter(AppDbContext context, ILogger<APIKeyAuthFilter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var potentialAPIKey))
        {
            context.Result = new UnauthorizedObjectResult("Missing API Key");
            return;
        }

        // Try to parse the API key as a Guid
        if (!Guid.TryParse(potentialAPIKey, out var potentialAPIKeyGuid))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
            return;
        }
        
        // Find the API key in the database
        var apiKey = await _context.APIKeys.Include(k => k.UserCreated).FirstOrDefaultAsync(k => k.Key == potentialAPIKeyGuid);
        if (apiKey == null)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
            return;
        }
        _logger.LogInformation($"API Key {apiKey.Key} used by {apiKey.UserCreated.FirstName}");
    }
}