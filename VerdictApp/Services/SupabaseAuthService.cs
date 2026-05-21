using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VerdictApp.Services;

public class SupabaseAuthService
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _config;

    public SupabaseAuthService(IHttpClientFactory factory, IConfiguration config)
    {
        _factory = factory;
        _config = config;
    }

    private string Url => _config["Supabase:Url"]?.TrimEnd('/') ?? throw new InvalidOperationException("Supabase:Url not configured.");
    private string AnonKey => _config["Supabase:AnonKey"] ?? throw new InvalidOperationException("Supabase:AnonKey not configured.");

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Supabase:Url"]) &&
        !string.IsNullOrWhiteSpace(_config["Supabase:AnonKey"]);

    /// <summary>
    /// Registers the user in Supabase Auth, which automatically sends a confirmation email.
    /// </summary>
    public async Task SignUpAsync(string email, string password, string emailRedirectTo)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apikey", AnonKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AnonKey);

        var body = JsonSerializer.Serialize(new
        {
            email,
            password,
            options = new { emailRedirectTo }
        });

        var response = await client.PostAsync(
            $"{Url}/auth/v1/signup",
            new StringContent(body, Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // 400 with "User already registered" is fine — they may be retrying
            if (json.Contains("already registered", StringComparison.OrdinalIgnoreCase))
                return;

            throw new Exception($"Supabase signup failed ({response.StatusCode}): {json}");
        }
    }

    /// <summary>
    /// Retrieves the user's email from a Supabase access token (hash-fragment fallback).
    /// </summary>
    public async Task<string?> GetEmailFromAccessTokenAsync(string accessToken)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apikey", AnonKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync($"{Url}/auth/v1/user");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("email", out var emailEl))
            return emailEl.GetString();

        return null;
    }

    /// <summary>
    /// Verifies a token_hash received from the Supabase confirmation redirect.
    /// Returns the user's email if valid, or null if invalid.
    /// </summary>
    public async Task<string?> VerifyTokenHashAsync(string tokenHash, string type)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apikey", AnonKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AnonKey);

        var body = JsonSerializer.Serialize(new { token_hash = tokenHash, type });

        var response = await client.PostAsync(
            $"{Url}/auth/v1/verify",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("user", out var userEl) &&
            userEl.TryGetProperty("email", out var emailEl))
            return emailEl.GetString();

        return null;
    }
}
