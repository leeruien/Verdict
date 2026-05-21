using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using VerdictApp.Data;

namespace VerdictApp.Services;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config) => _config = config;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_config["Email:Username"]);

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your Verdict account", ConfirmationTemplate(user.DisplayName ?? email, confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your Verdict password", ResetTemplate(user.DisplayName ?? email, resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, "Your Verdict password reset code", $"<p>Your reset code is: <strong>{resetCode}</strong></p>");

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var s = _config.GetSection("Email");
        var host = s["SmtpHost"];
        var user = s["Username"];

        // SMTP not configured — skip silently (dev mode)
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user)) return;

        var port = int.Parse(s["SmtpPort"] ?? "587");
        var ssl  = bool.Parse(s["EnableSsl"] ?? "true");
        var pass = s["Password"] ?? "";
        var from = s["FromAddress"] ?? user;
        var name = s["FromName"] ?? "Verdict";

        using var client = new SmtpClient(host, port) { EnableSsl = ssl, Credentials = new NetworkCredential(user, pass) };
        using var msg = new MailMessage { From = new MailAddress(from, name), Subject = subject, Body = htmlBody, IsBodyHtml = true };
        msg.To.Add(toEmail);
        await client.SendMailAsync(msg);
    }

    private static string ConfirmationTemplate(string name, string link) => $"""
        <div style="font-family:Georgia,serif;max-width:480px;margin:0 auto;padding:2rem;">
            <h2 style="color:#052767">⚖ Verdict</h2>
            <p>Hi <strong>{name}</strong>,</p>
            <p>Thanks for signing up! Please confirm your email address to activate your account.</p>
            <a href="{link}"
               style="display:inline-block;margin:1.25rem 0;padding:0.75rem 1.75rem;background:#052767;color:#fff;text-decoration:none;border-radius:8px;font-size:1rem;">
                Confirm email
            </a>
            <p style="color:#888;font-size:0.85rem;">If you didn't create a Verdict account, you can ignore this email.</p>
        </div>
        """;

    private static string ResetTemplate(string name, string link) => $"""
        <div style="font-family:Georgia,serif;max-width:480px;margin:0 auto;padding:2rem;">
            <h2 style="color:#052767">⚖ Verdict</h2>
            <p>Hi <strong>{name}</strong>,</p>
            <p>We received a request to reset your password.</p>
            <a href="{link}"
               style="display:inline-block;margin:1.25rem 0;padding:0.75rem 1.75rem;background:#052767;color:#fff;text-decoration:none;border-radius:8px;font-size:1rem;">
                Reset password
            </a>
            <p style="color:#888;font-size:0.85rem;">If you didn't request this, you can safely ignore this email.</p>
        </div>
        """;
}
