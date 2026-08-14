using System.Security.Claims;

namespace AzilEdu.Api.Services;

public class AiAuditService
{
    private readonly ILogger<AiAuditService> _logger;

    public AiAuditService(ILogger<AiAuditService> logger)
    {
        _logger = logger;
    }

    public void LogRequest(ClaimsPrincipal user, string purpose)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var email = user.FindFirstValue(ClaimTypes.Email) ?? "unknown";

        _logger.LogInformation(
            "AI audit: Purpose={Purpose} UserId={UserId} Email={Email}",
            purpose,
            userId,
            email);
    }
}
