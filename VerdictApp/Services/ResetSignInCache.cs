using System.Collections.Concurrent;

namespace VerdictApp.Services;

/// <summary>
/// Short-lived in-memory store that bridges the Blazor password-reset component
/// (which can't write cookies) with the /auth/post-reset-signin endpoint (which can).
/// Each token is one-time-use and expires after 60 seconds.
/// </summary>
public class ResetSignInCache
{
    private readonly ConcurrentDictionary<string, (string Email, DateTime Expires)> _store = new();

    public string Issue(string email)
    {
        var token = Guid.NewGuid().ToString("N");
        _store[token] = (email, DateTime.UtcNow.AddSeconds(60));
        return token;
    }

    public string? Consume(string token)
    {
        if (_store.TryRemove(token, out var entry) && entry.Expires > DateTime.UtcNow)
            return entry.Email;
        return null;
    }
}
