using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SpaBooker.Core.Entities;
using SpaBooker.Core.Interfaces;
using SpaBooker.Core.Settings;
using SpaBooker.Infrastructure.Data;

namespace SpaBooker.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ApplicationDbContext context,
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _context = context;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
    {
        if (!_emailSettings.EnableNotifications)
        {
            _logger.LogInformation("Email notifications are disabled. Skipping email to {To}", to);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = plainTextBody ?? StripHtml(htmlBody)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, 
                _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername))
            {
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationUrl)
    {
        var subject = "Confirm Your Email - SpaBooker";
        var htmlBody = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #2D2D2D; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                    .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to SpaBooker!</h1>
                    </div>
                    <div class='content'>
                        <h2>Confirm Your Email Address</h2>
                        <p>Thank you for registering with SpaBooker. To complete your registration and start booking spa services, please confirm your email address by clicking the button below:</p>
                        <p style='text-align: center;'>
                            <a href='{confirmationUrl}' class='button'>Confirm Email Address</a>
                        </p>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #4CAF50;'>{confirmationUrl}</p>
                        <p><strong>Note:</strong> This link will expire in 24 hours for security reasons.</p>
                    </div>
                    <div class='footer'>
                        <p>If you didn't create an account with SpaBooker, please ignore this email.</p>
                        <p>&copy; 2025 SpaBooker. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";

        await SendEmailAsync(email, subject, htmlBody);
        _logger.LogInformation("Email confirmation sent to {Email}", email);
    }

    public async Task SendBookingConfirmationAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Therapist)
            .Include(b => b.Service)
            .Include(b => b.Location)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null || booking.Client.Email == null)
        {
            _logger.LogWarning("Cannot send booking confirmation - booking {BookingId} not found or client has no email", bookingId);
            return;
        }

        var subject = "Booking Confirmation - SpaBooker";
        var htmlBody = GenerateBookingConfirmationEmail(booking);

        await SendEmailAsync(booking.Client.Email, subject, htmlBody);
    }

    public async Task SendBookingCancellationAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Therapist)
            .Include(b => b.Service)
            .Include(b => b.Location)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null || booking.Client.Email == null)
        {
            _logger.LogWarning("Cannot send cancellation email - booking {BookingId} not found or client has no email", bookingId);
            return;
        }

        var subject = "Booking Cancelled - SpaBooker";
        var htmlBody = GenerateBookingCancellationEmail(booking);

        await SendEmailAsync(booking.Client.Email, subject, htmlBody);
    }

    public async Task SendBookingReminderAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Client)
            .Include(b => b.Therapist)
            .Include(b => b.Service)
            .Include(b => b.Location)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null || booking.Client.Email == null)
        {
            _logger.LogWarning("Cannot send reminder - booking {BookingId} not found or client has no email", bookingId);
            return;
        }

        var subject = "Reminder: Upcoming Spa Appointment Tomorrow";
        var htmlBody = GenerateBookingReminderEmail(booking);

        await SendEmailAsync(booking.Client.Email, subject, htmlBody);
    }

    public async Task SendMembershipConfirmationAsync(int userMembershipId)
    {
        var membership = await _context.UserMemberships
            .Include(m => m.User)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == userMembershipId);

        if (membership == null || membership.User.Email == null)
        {
            _logger.LogWarning("Cannot send membership confirmation - membership {MembershipId} not found or user has no email", userMembershipId);
            return;
        }

        var subject = "Welcome to Your Membership - SpaBooker";
        var htmlBody = GenerateMembershipConfirmationEmail(membership);

        await SendEmailAsync(membership.User.Email, subject, htmlBody);
    }

    public async Task SendMembershipCancellationAsync(int userMembershipId)
    {
        var membership = await _context.UserMemberships
            .Include(m => m.User)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == userMembershipId);

        if (membership == null || membership.User.Email == null)
        {
            _logger.LogWarning("Cannot send membership cancellation - membership {MembershipId} not found or user has no email", userMembershipId);
            return;
        }

        var subject = "Membership Cancellation Confirmation - SpaBooker";
        var htmlBody = GenerateMembershipCancellationEmail(membership);

        await SendEmailAsync(membership.User.Email, subject, htmlBody);
    }

    #region HTML Email Templates

    private string GenerateBookingConfirmationEmail(Booking booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); padding: 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .booking-details {{ background: #fff5f5; border-left: 4px solid #B76E79; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .detail-row {{ margin: 10px 0; display: flex; justify-content: space-between; }}
        .detail-label {{ font-weight: bold; color: #B76E79; }}
        .detail-value {{ color: #333; }}
        .button {{ display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); color: white; text-decoration: none; border-radius: 25px; margin: 20px 0; }}
        .footer {{ background: #f9f5f5; padding: 20px; text-align: center; color: #666; font-size: 14px; }}
        .highlight {{ color: #B76E79; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✨ Booking Confirmed!</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.Client.FirstName},</p>
            <p>Your spa appointment has been successfully confirmed. We look forward to pampering you!</p>
            
            <div class=""booking-details"">
                <h3 style=""margin-top: 0; color: #B76E79;"">Appointment Details</h3>
                <div class=""detail-row"">
                    <span class=""detail-label"">Service:</span>
                    <span class=""detail-value"">{booking.Service.Name}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Date:</span>
                    <span class=""detail-value"">{booking.StartTime:MMMM dd, yyyy}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Time:</span>
                    <span class=""detail-value"">{booking.StartTime:h:mm tt} - {booking.EndTime:h:mm tt}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Duration:</span>
                    <span class=""detail-value"">{booking.Service.DurationMinutes} minutes</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Therapist:</span>
                    <span class=""detail-value"">{booking.Therapist.FirstName} {booking.Therapist.LastName}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Location:</span>
                    <span class=""detail-value"">{booking.Location.Name}</span>
                </div>
                <div class=""detail-row"">
                    <span class=""detail-label"">Address:</span>
                    <span class=""detail-value"">{booking.Location.Address}, {booking.Location.City}, {booking.Location.State} {booking.Location.ZipCode}</span>
                </div>
                <div class=""detail-row"" style=""margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd;"">
                    <span class=""detail-label"">Total Price:</span>
                    <span class=""detail-value highlight"" style=""font-size: 20px;"">${booking.TotalPrice:F2}</span>
                </div>
                {(booking.DepositAmount > 0 ? $@"
                <div class=""detail-row"">
                    <span class=""detail-label"">Deposit Paid:</span>
                    <span class=""detail-value"">${booking.DepositAmount:F2}</span>
                </div>" : "")}
            </div>

            <p><strong>Important Reminders:</strong></p>
            <ul>
                <li>Please arrive 10-15 minutes early to complete any necessary paperwork</li>
                <li>If you need to cancel, please do so at least 24 hours in advance to avoid cancellation fees</li>
                <li>We'll send you a reminder 24 hours before your appointment</li>
            </ul>

            <p>If you have any questions, please don't hesitate to contact us at {booking.Location.Phone}.</p>
            
            <p>We can't wait to see you!</p>
            <p>Warm regards,<br><span class=""highlight"">The SpaBooker Team</span></p>
        </div>
        <div class=""footer"">
            <p>© {DateTime.Now.Year} SpaBooker. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingCancellationEmail(Booking booking)
    {
        var refundInfo = booking.DepositAmount > 0 && !string.IsNullOrEmpty(booking.StripeRefundId)
            ? $"<p>A refund of <strong>${booking.DepositAmount:F2}</strong> has been processed and will appear in your account within 5-10 business days.</p>"
            : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); padding: 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .booking-details {{ background: #fff5f5; border-left: 4px solid #B76E79; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .footer {{ background: #f9f5f5; padding: 20px; text-align: center; color: #666; font-size: 14px; }}
        .highlight {{ color: #B76E79; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Booking Cancelled</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.Client.FirstName},</p>
            <p>Your spa appointment has been cancelled as requested.</p>
            
            <div class=""booking-details"">
                <h3 style=""margin-top: 0; color: #B76E79;"">Cancelled Appointment</h3>
                <p><strong>Service:</strong> {booking.Service.Name}<br>
                <strong>Date:</strong> {booking.StartTime:MMMM dd, yyyy}<br>
                <strong>Time:</strong> {booking.StartTime:h:mm tt}<br>
                <strong>Location:</strong> {booking.Location.Name}</p>
            </div>

            {refundInfo}

            <p>We're sorry to see this appointment cancelled. We hope to see you again soon!</p>
            
            <p>To book a new appointment, please visit our website or contact us at {booking.Location.Phone}.</p>
            
            <p>Warm regards,<br><span class=""highlight"">The SpaBooker Team</span></p>
        </div>
        <div class=""footer"">
            <p>© {DateTime.Now.Year} SpaBooker. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateBookingReminderEmail(Booking booking)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); padding: 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .reminder-box {{ background: #fffbef; border: 2px solid #ffc107; padding: 20px; margin: 20px 0; border-radius: 8px; text-align: center; }}
        .booking-details {{ background: #fff5f5; border-left: 4px solid #B76E79; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .footer {{ background: #f9f5f5; padding: 20px; text-align: center; color: #666; font-size: 14px; }}
        .highlight {{ color: #B76E79; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>⏰ Appointment Reminder</h1>
        </div>
        <div class=""content"">
            <p>Dear {booking.Client.FirstName},</p>
            
            <div class=""reminder-box"">
                <h2 style=""margin: 0; color: #f57c00;"">Your appointment is tomorrow!</h2>
            </div>

            <p>This is a friendly reminder about your upcoming spa appointment:</p>
            
            <div class=""booking-details"">
                <h3 style=""margin-top: 0; color: #B76E79;"">Appointment Details</h3>
                <p><strong>Service:</strong> {booking.Service.Name}<br>
                <strong>Date:</strong> {booking.StartTime:MMMM dd, yyyy}<br>
                <strong>Time:</strong> {booking.StartTime:h:mm tt} - {booking.EndTime:h:mm tt}<br>
                <strong>Therapist:</strong> {booking.Therapist.FirstName} {booking.Therapist.LastName}<br>
                <strong>Location:</strong> {booking.Location.Name}<br>
                <strong>Address:</strong> {booking.Location.Address}, {booking.Location.City}, {booking.Location.State} {booking.Location.ZipCode}</p>
            </div>

            <p><strong>Important Reminders:</strong></p>
            <ul>
                <li>Please arrive 10-15 minutes early</li>
                <li>If you need to cancel, please do so at least 24 hours in advance</li>
                <li>Bring a valid ID if this is your first visit</li>
            </ul>

            <p>We're looking forward to seeing you tomorrow!</p>
            
            <p>If you have any questions, please contact us at {booking.Location.Phone}.</p>
            
            <p>Warm regards,<br><span class=""highlight"">The SpaBooker Team</span></p>
        </div>
        <div class=""footer"">
            <p>© {DateTime.Now.Year} SpaBooker. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateMembershipConfirmationEmail(UserMembership membership)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); padding: 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .membership-card {{ background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); padding: 25px; margin: 20px 0; border-radius: 12px; color: white; text-align: center; }}
        .benefit-list {{ background: #fff5f5; padding: 20px; margin: 20px 0; border-radius: 8px; }}
        .footer {{ background: #f9f5f5; padding: 20px; text-align: center; color: #666; font-size: 14px; }}
        .highlight {{ color: #B76E79; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>💎 Welcome to Your Membership!</h1>
        </div>
        <div class=""content"">
            <p>Dear {membership.User.FirstName},</p>
            <p>Congratulations! Your membership has been successfully activated.</p>
            
            <div class=""membership-card"">
                <h2 style=""margin: 0;"">{membership.MembershipPlan.Name}</h2>
                <p style=""font-size: 36px; margin: 10px 0;"">{membership.CurrentCredits:F0}</p>
                <p style=""margin: 0;"">Available Credits</p>
            </div>

            <div class=""benefit-list"">
                <h3 style=""margin-top: 0; color: #B76E79;"">Your Membership Benefits</h3>
                <ul>
                    <li><strong>{membership.MembershipPlan.MonthlyCredits:F0} credits</strong> added monthly</li>
                    <li><strong>{membership.MembershipPlan.DiscountPercentage:F0}% discount</strong> on all services</li>
                    <li>Credits roll over with no limit</li>
                    <li>Credits valid for 12 months</li>
                    <li>Cancel anytime</li>
                </ul>
                <p><strong>Monthly Price:</strong> ${membership.MembershipPlan.MonthlyPrice:F2}</p>
                <p><strong>Next Billing Date:</strong> {membership.NextBillingDate:MMMM dd, yyyy}</p>
            </div>

            <p>Start booking your spa appointments now and enjoy your exclusive member benefits!</p>
            
            <p>Warm regards,<br><span class=""highlight"">The SpaBooker Team</span></p>
        </div>
        <div class=""footer"">
            <p>© {DateTime.Now.Year} SpaBooker. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateMembershipCancellationEmail(UserMembership membership)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f5f5; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #B76E79 0%, #D4A5A5 100%); padding: 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .info-box {{ background: #fff5f5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #B76E79; }}
        .footer {{ background: #f9f5f5; padding: 20px; text-align: center; color: #666; font-size: 14px; }}
        .highlight {{ color: #B76E79; font-weight: bold; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Membership Cancelled</h1>
        </div>
        <div class=""content"">
            <p>Dear {membership.User.FirstName},</p>
            <p>Your <strong>{membership.MembershipPlan.Name}</strong> membership has been cancelled as requested.</p>
            
            <div class=""info-box"">
                <p><strong>Remaining Credits:</strong> {membership.CurrentCredits:F0}</p>
                <p><strong>Access Until:</strong> {membership.EndDate:MMMM dd, yyyy}</p>
                <p>You can continue to use your remaining credits until your membership expires.</p>
            </div>

            <p>We're sorry to see you go! If you change your mind, you can reactivate your membership at any time.</p>
            
            <p>Thank you for being a valued member.</p>
            
            <p>Warm regards,<br><span class=""highlight"">The SpaBooker Team</span></p>
        </div>
        <div class=""footer"">
            <p>© {DateTime.Now.Year} SpaBooker. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion

    private string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}

