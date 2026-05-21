namespace VerdictApp.Services;

public class FounderService
{
    private readonly HashSet<string> _founders;

    public FounderService(IConfiguration config)
    {
        _founders = config.GetSection("Founders").Get<string[]>()
            ?.Select(e => e.ToLowerInvariant()).ToHashSet()
            ?? new HashSet<string>();
    }

    public bool IsFounder(string? email) =>
        email != null && _founders.Contains(email.ToLowerInvariant());
}
