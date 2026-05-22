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
    private string? ServiceRoleKey => _config["Supabase:ServiceRoleKey"];
    private bool IsAdminConfigured => !string.IsNullOrWhiteSpace(ServiceRoleKey) && ServiceRoleKey != "YOUR_SERVICE_ROLE_KEY";

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

    // ── Admin methods (require ServiceRoleKey) ──────────────────────────────

    private HttpClient AdminClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apikey", ServiceRoleKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceRoleKey);
        return client;
    }

    /// <summary>
    /// Finds the Supabase Auth user UID by email address.
    /// Paginates through all users and matches by email because Supabase's admin
    /// users endpoint does not support server-side email filtering.
    /// </summary>
    private async Task<string?> FindUidByEmailAsync(string email)
    {
        if (!IsAdminConfigured) return null;
        var client = AdminClient();
        const int perPage = 50;
        var page = 1;

        while (true)
        {
            var response = await client.GetAsync($"{Url}/auth/v1/admin/users?page={page}&per_page={perPage}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("users", out var users)) return null;

            foreach (var u in users.EnumerateArray())
            {
                if (u.TryGetProperty("email", out var emailEl) &&
                    string.Equals(emailEl.GetString(), email, StringComparison.OrdinalIgnoreCase) &&
                    u.TryGetProperty("id", out var idEl))
                    return idEl.GetString();
            }

            // If this page was not full we've seen all users
            if (users.GetArrayLength() < perPage) return null;
            page++;
        }
    }

    /// <summary>
    /// Changes the Supabase Auth user's password using their own session token so that
    /// Supabase fires the "Password changed" notification email.
    /// Falls back to the admin API if the user-level sign-in fails.
    /// </summary>
    public async Task UpdatePasswordAsync(string email, string currentPassword, string newPassword)
    {
        if (!IsConfigured) return;

        // Step 1 — sign in as the user to get their access token
        var loginClient = _factory.CreateClient();
        loginClient.DefaultRequestHeaders.Add("apikey", AnonKey);
        loginClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AnonKey);

        var loginBody = JsonSerializer.Serialize(new { email, password = currentPassword });
        var loginResp = await loginClient.PostAsync(
            $"{Url}/auth/v1/token?grant_type=password",
            new StringContent(loginBody, Encoding.UTF8, "application/json"));

        string? accessToken = null;
        if (loginResp.IsSuccessStatusCode)
        {
            var loginJson = await loginResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(loginJson);
            if (doc.RootElement.TryGetProperty("access_token", out var t))
                accessToken = t.GetString();
        }

        if (accessToken != null)
        {
            // Step 2 — update password with user token (triggers notification email)
            var userClient = _factory.CreateClient();
            userClient.DefaultRequestHeaders.Add("apikey", AnonKey);
            userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var updateBody = JsonSerializer.Serialize(new { password = newPassword });
            await userClient.PutAsync(
                $"{Url}/auth/v1/user",
                new StringContent(updateBody, Encoding.UTF8, "application/json"));
        }
        else if (IsAdminConfigured)
        {
            // Fallback — admin API (no notification email, but keeps passwords in sync)
            var uid = await FindUidByEmailAsync(email);
            if (uid == null) return;
            var body = JsonSerializer.Serialize(new { password = newPassword });
            await AdminClient().PutAsync(
                $"{Url}/auth/v1/admin/users/{uid}",
                new StringContent(body, Encoding.UTF8, "application/json"));
        }
    }

    /// <summary>Deletes the Supabase Auth user record.</summary>
    public async Task DeleteUserAsync(string email)
    {
        if (!IsAdminConfigured) return;
        var uid = await FindUidByEmailAsync(email);
        if (uid == null) return;

        await AdminClient().DeleteAsync($"{Url}/auth/v1/admin/users/{uid}");
    }
}
