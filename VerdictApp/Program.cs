using VerdictApp.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using VerdictApp.Data;
using Microsoft.AspNetCore.Mvc;

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
    .AddIdentityCore<IdentityUser>(
        options =>
        {
            options.User.RequireUniqueEmail = true;
            // options.SignIn.RequireConfirmedEmail = true;P12
        }
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<IdentityUser>>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAntiforgery();
builder.Services.AddRazorPages();

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


// Logout endpoint: perform SignOut on the server and redirect to login.
// We expose a simple GET endpoint so components can navigate to it with forceLoad to clear the cookie.
app.MapGet("/account/logout", async (SignInManager<IdentityUser> signInManager, HttpContext http) =>
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
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var email = "test@test.com";
    var existing = await userManager.FindByEmailAsync(email);

    if (existing == null)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        await userManager.CreateAsync(user, "Password123!");
    }
}



app.Run();

public record LoginDto(string Email, string Password, bool RememberMe);