using VerdictApp.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using VerdictApp.Data;
using Microsoft.AspNetCore.Mvc;
using VerdictApp.Services;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add identity services
builder.Services
    .AddIdentityCore<ApplicationUser>(
        options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        }
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<ExpiryNotificationService>();
builder.Services.AddSingleton<RecentGroupsNotifier>();
builder.Services.AddSingleton<BadgeNotifier>();
builder.Services.AddTransient<EmailSender>();
builder.Services.AddTransient<IEmailSender<ApplicationUser>>(sp => sp.GetRequiredService<EmailSender>());
builder.Services.AddTransient<SupabaseAuthService>();
builder.Services.AddSingleton<FounderService>();
builder.Services.AddHttpClient();
builder.Services.AddAntiforgery();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseStaticFiles();
// Logout endpoint: perform SignOut on the server and redirect to login.
// We expose a simple GET endpoint so components can navigate to it with forceLoad to clear the cookie.
// Called when the user clicks "Verify & Log in" on the /auth/confirm Blazor page.
// Verifies the Supabase token, confirms the Identity user, signs them in, and redirects home.
app.MapGet("/auth/do-signin", async (
    string? token_hash, string? type, string? access_token,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IPasswordHasher<ApplicationUser> passwordHasher,
    ApplicationDbContext db,
    SupabaseAuthService supabase) =>
{
    string? email = null;

    if (!string.IsNullOrEmpty(token_hash))
        email = await supabase.VerifyTokenHashAsync(token_hash, type ?? "email");
    else if (!string.IsNullOrEmpty(access_token))
        email = await supabase.GetEmailFromAccessTokenAsync(access_token);

    if (email == null) return Results.Redirect("/login?error=confirm");

    // Check if the Identity user already exists (e.g. re-click of the same link)
    var user = await userManager.FindByEmailAsync(email);

    if (user == null)
    {
        // First-time confirmation — create the Identity user from the pending registration
        var pending = await db.PendingRegistrations
            .FirstOrDefaultAsync(p => p.Email == email);

        if (pending == null) return Results.Redirect("/login?error=confirm");

        user = new ApplicationUser
        {
            UserName       = email,
            Email          = email,
            DisplayName    = pending.DisplayName,
            EmailConfirmed = true,
            SecurityStamp  = Guid.NewGuid().ToString()
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded) return Results.Redirect("/login?error=confirm");

        // Restore the hashed password that was stored at registration time
        user.PasswordHash = pending.PasswordHash;
        await userManager.UpdateAsync(user);

        db.PendingRegistrations.Remove(pending);
        await db.SaveChangesAsync();
    }
    else if (!user.EmailConfirmed)
    {
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
    }

    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.Redirect("/");
});

// Legacy Identity email confirmation (kept for existing unconfirmed accounts)
app.MapGet("/account/confirm-email", async (
    string userId, string token,
    UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null) return Results.Redirect("/login?error=confirm");
    var decoded = HttpUtility.UrlDecode(token);
    var result = await userManager.ConfirmEmailAsync(user, decoded);
    return result.Succeeded
        ? Results.Redirect("/login?confirmed=true")
        : Results.Redirect("/login?error=confirm");
});

// Resend confirmation email endpoint
app.MapGet("/account/resend-confirmation", async (
    string email,
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    HttpRequest request) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null || user.EmailConfirmed)
        return Results.Redirect("/login");

    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
    var encoded = HttpUtility.UrlEncode(token);
    var baseUrl = $"{request.Scheme}://{request.Host}";
    var link = $"{baseUrl}/account/confirm-email?userId={user.Id}&token={encoded}";
    await emailSender.SendConfirmationLinkAsync(user, user.Email!, link);
    return Results.Redirect("/login?resent=true");
});

app.MapGet("/account/logout", async (SignInManager<ApplicationUser> signInManager, HttpContext http) =>
{
    // Perform sign-out server-side and return an explicit redirect result.
    // Call both SignInManager and the authentication scheme sign-out to ensure the cookie is cleared.
    await signInManager.SignOutAsync();
    try
    {
        await http.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
    catch
    {
        // ignore if scheme not registered or sign-out fails here; SignInManager already attempts sign-out
    }

    return Results.Redirect("/login");
});

// app.MapPost("/account/login", async (
//     UserManager<IdentityUser> userManager,
//     SignInManager<IdentityUser> signInManager,
//     [FromForm] string email,
//     [FromForm] string password,
//     [FromForm] bool rememberMe) =>
// {
//     var user = await userManager.FindByEmailAsync(email);
//     if (user is null) return Results.Redirect("/login?error=invalid");

//     var result = await signInManager.PasswordSignInAsync(
//         user.UserName!, password, isPersistent: rememberMe, lockoutOnFailure: false);

//     return result.Succeeded
//         ? Results.Redirect("/")
//         : Results.Redirect("/login?error=1");
// });
// app.MapPost("/account/login", async (
//     UserManager<IdentityUser> userManager,
//     SignInManager<IdentityUser> signInManager,
//     [FromForm] string email,
//     [FromForm] string password,
//     [FromForm] bool rememberMe) =>
// {
//     var user = await userManager.FindByEmailAsync(email);
//     if (user is null) return Results.Redirect("/login?error=invalid");

//     var result = await signInManager.PasswordSignInAsync(
//         user.UserName!, password, isPersistent: rememberMe, lockoutOnFailure: false);

//     return result.Succeeded
//         ? Results.Redirect("/")
//         : Results.Redirect("/login?error=1");
// });


// app.MapPost("/account/register", async (
//     UserManager<IdentityUser> userManager,
//     SignInManager<IdentityUser> signInManager,
//     [FromForm] string email,
//     [FromForm] string password) =>
// {
//     var user = new IdentityUser { UserName = email, Email = email };
//     var create = await userManager.CreateAsync(user, password);

//     if (!create.Succeeded)
//         return Results.Redirect("/register?error=1");

//     await signInManager.SignInAsync(user, isPersistent: false);
//     return Results.Redirect("/");
// });

// Debug endpoint: returns number of Identity users (helps verify DB connectivity and Identity setup)
app.MapGet("/debug/users", async (IServiceProvider services) =>
{
    try
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var usersQuery = userManager.Users;
        var count = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(usersQuery);
        return Results.Ok(new { userCount = count });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.ToString());
    }
});
// test user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var email = "test@test.com";
    var existing = await userManager.FindByEmailAsync(email);

    if (existing == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = "TestUser",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Password123!");
    }
    else if (!existing.EmailConfirmed)
    {
        // Ensure the seeded test user is always confirmed
        existing.EmailConfirmed = true;
        await userManager.UpdateAsync(existing);
    }
}



app.Run();

public record LoginDto(string Email, string Password, bool RememberMe);