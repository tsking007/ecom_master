using EcommerceApp.Application.Features.Notifications.DTOs;

namespace EcommerceApp.Application.Features.Notifications.Templates;

/// <summary>
/// Static HTML email template factory.
///
/// WHY static:
///   Templates are pure functions — they take data and return an EmailMessage.
///   No state, no dependencies, no async. Making the class static avoids
///   unnecessary DI registration and makes templates easy to unit test directly.
///
/// HTML approach:
///   All styles are inline because email clients (Outlook, Gmail) strip
///   &lt;style&gt; blocks. Inline styles are the only reliable cross-client method.
///
/// Adding a new template:
///   1. Add a value to EmailTemplateType enum
///   2. Add a static Build{Name} method here
///   3. Add a command + handler in the Commands folder
/// </summary>
public static class EmailTemplateEngine
{
    // ── Shared layout ─────────────────────────────────────────────────────────

    private static string Wrap(string fromEmail, string fromName, string bodyHtml)
        => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            </head>
            <body style="margin:0;padding:0;background-color:#f1f5f9;font-family:Arial,Helvetica,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table width="100%" cellpadding="0" cellspacing="0"
                           style="max-width:600px;background-color:#ffffff;border-radius:12px;
                                  overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">

                      <!-- Header -->
                      <tr>
                        <td style="background-color:#2563eb;padding:28px 32px;text-align:center;">
                          <h1 style="margin:0;color:#ffffff;font-size:22px;
                                     letter-spacing:0.5px;">EcommerceApp</h1>
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding:36px 32px;color:#1e293b;font-size:15px;
                                   line-height:1.7;">
                          {bodyHtml}
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="background-color:#f8fafc;padding:20px 32px;
                                   text-align:center;font-size:12px;color:#94a3b8;
                                   border-top:1px solid #e2e8f0;">
                          &copy; {DateTime.UtcNow.Year} EcommerceApp &mdash;
                          This email was sent from
                          <a href="mailto:{fromEmail}"
                             style="color:#2563eb;text-decoration:none;">{fromName}</a>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

    private static string Button(string url, string label)
        => $"""
            <table cellpadding="0" cellspacing="0" style="margin:28px 0;">
              <tr>
                <td style="background-color:#2563eb;border-radius:8px;">
                  <a href="{url}"
                     style="display:inline-block;padding:14px 32px;color:#ffffff;
                            font-size:15px;font-weight:bold;text-decoration:none;">
                    {label}
                  </a>
                </td>
              </tr>
            </table>
            """;

    // ── Welcome email ─────────────────────────────────────────────────────────

    public static EmailMessage BuildWelcomeEmail(
        string to,
        string firstName,
        string fromEmail,
        string fromName)
    {
        var body = $"""
            <h2 style="margin:0 0 16px;font-size:20px;">Welcome, {firstName}! 🎉</h2>
            <p>Your account has been verified and is ready to use.</p>
            <p>You can now browse products, place orders, and track deliveries
               all from one place.</p>
            <p style="margin-top:28px;">Happy shopping,<br/>
               <strong>The EcommerceApp Team</strong></p>
            """;

        return new EmailMessage(
            To: to,
            Subject: "Welcome to EcommerceApp!",
            HtmlBody: Wrap(fromEmail, fromName, body),
            From: fromEmail,
            FromName: fromName);
    }

    // ── OTP verification ──────────────────────────────────────────────────────

    public static EmailMessage BuildOtpEmail(
        string to,
        string firstName,
        string otp,
        string purpose,
        string fromEmail,
        string fromName)
    {
        var purposeLabel = purpose switch
        {
            "EmailVerification" => "verify your email address",
            "PhoneVerification" => "verify your phone number",
            "TwoFactorAuth" => "complete your login",
            _ => "complete your request"
        };

        var body = $"""
            <h2 style="margin:0 0 16px;font-size:20px;">Hi {firstName},</h2>
            <p>Use the code below to {purposeLabel}.</p>

            <!-- OTP box -->
            <table cellpadding="0" cellspacing="0" style="margin:28px 0;">
              <tr>
                <td style="background-color:#eff6ff;border:2px dashed #2563eb;
                           border-radius:12px;padding:24px 40px;text-align:center;">
                  <span style="font-size:40px;font-weight:bold;
                               letter-spacing:12px;color:#1d4ed8;">{otp}</span>
                </td>
              </tr>
            </table>

            <p style="color:#64748b;font-size:13px;">
              This code expires in <strong>10 minutes</strong>.
              If you did not request this, you can safely ignore this email.
            </p>
            """;

        return new EmailMessage(
            To: to,
            Subject: $"Your verification code: {otp}",
            HtmlBody: Wrap(fromEmail, fromName, body),
            From: fromEmail,
            FromName: fromName);
    }

    // ── Password reset ────────────────────────────────────────────────────────

    public static EmailMessage BuildPasswordResetEmail(
        string to,
        string firstName,
        string resetUrl,
        string fromEmail,
        string fromName)
    {
        var body = $"""
            <h2 style="margin:0 0 16px;font-size:20px;">Reset your password</h2>
            <p>Hi {firstName},</p>
            <p>We received a request to reset your password.
               Click the button below to choose a new one.</p>
            {Button(resetUrl, "Reset Password")}
            <p style="color:#64748b;font-size:13px;">
              This link expires in <strong>10 minutes</strong>.
              If you did not request a password reset, no action is needed —
              your password has not been changed.
            </p>
            <p style="font-size:12px;color:#94a3b8;word-break:break-all;">
              If the button above does not work, copy and paste this URL into
              your browser:<br/>{resetUrl}
            </p>
            """;

        return new EmailMessage(
            To: to,
            Subject: "Reset your EcommerceApp password",
            HtmlBody: Wrap(fromEmail, fromName, body),
            From: fromEmail,
            FromName: fromName);
    }

    // ── Order confirmation ────────────────────────────────────────────────────

    /// <summary>
    /// Stub — fleshed out fully in Part 20 when order details DTO is available.
    /// </summary>
    public static EmailMessage BuildOrderConfirmationEmail(
        string to,
        string firstName,
        string orderNumber,
        decimal orderTotal,
        string fromEmail,
        string fromName)
    {
        var body = $"""
            <h2 style="margin:0 0 16px;font-size:20px;">
              Order confirmed ✅
            </h2>
            <p>Hi {firstName}, thank you for your order!</p>
            <table style="width:100%;border-collapse:collapse;margin:20px 0;">
              <tr>
                <td style="padding:10px;border-bottom:1px solid #e2e8f0;
                           color:#64748b;">Order number</td>
                <td style="padding:10px;border-bottom:1px solid #e2e8f0;
                           font-weight:bold;">#{orderNumber}</td>
              </tr>
              <tr>
                <td style="padding:10px;color:#64748b;">Order total</td>
                <td style="padding:10px;font-weight:bold;">
                  ${orderTotal:F2}
                </td>
              </tr>
            </table>
            <p>We will send you another email when your order ships.</p>
            """;

        return new EmailMessage(
            To: to,
            Subject: $"Order #{orderNumber} confirmed",
            HtmlBody: Wrap(fromEmail, fromName, body),
            From: fromEmail,
            FromName: fromName);
    }

    // ── Price drop alert ──────────────────────────────────────────────────────

    /// <summary>
    /// Stub — wired up in Part 17 (Price Drop Alert handler).
    /// </summary>
    public static EmailMessage BuildPriceDropAlertEmail(
        string to,
        string firstName,
        string productName,
        decimal oldPrice,
        decimal newPrice,
        string productUrl,
        string fromEmail,
        string fromName)
    {
        var saving = oldPrice - newPrice;
        var savePct = (int)Math.Round(saving / oldPrice * 100);

        var body = $"""
            <h2 style="margin:0 0 16px;font-size:20px;">
              Price drop on your wishlist item 🎯
            </h2>
            <p>Hi {firstName},</p>
            <p><strong>{productName}</strong> just dropped in price!</p>
            <table style="width:100%;border-collapse:collapse;margin:20px 0;">
              <tr>
                <td style="padding:10px;border-bottom:1px solid #e2e8f0;
                           color:#64748b;">Was</td>
                <td style="padding:10px;border-bottom:1px solid #e2e8f0;
                           text-decoration:line-through;color:#94a3b8;">
                  ${oldPrice:F2}
                </td>
              </tr>
              <tr>
                <td style="padding:10px;color:#64748b;">Now</td>
                <td style="padding:10px;font-weight:bold;color:#16a34a;font-size:18px;">
                  ${newPrice:F2}
                  <span style="font-size:13px;color:#16a34a;">
                    (Save {savePct}%)
                  </span>
                </td>
              </tr>
            </table>
            {Button(productUrl, "Shop Now")}
            """;

        return new EmailMessage(
            To: to,
            Subject: $"Price drop: {productName} is now ${newPrice:F2}",
            HtmlBody: Wrap(fromEmail, fromName, body),
            From: fromEmail,
            FromName: fromName);
    }
}