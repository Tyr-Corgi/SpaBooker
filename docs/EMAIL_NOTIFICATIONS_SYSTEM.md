# Email & SMS Notifications System

## Overview

Complete implementation of automated email notifications for booking confirmations, cancellations, reminders, and membership updates. The system uses MailKit for robust email delivery and includes beautiful HTML templates styled with the pink/rose gold theme.

---

## ✅ Features Implemented

### 1. **Email Service Infrastructure**
- MailKit-based email service (works with any SMTP provider)
- Configurable SMTP settings (Gmail, SendGrid, Amazon SES, etc.)
- HTML and plain text email support
- Error handling and logging
- Enable/disable toggle for development

### 2. **Booking Confirmation Emails**
- Sent automatically after successful payment
- Includes complete booking details
- Shows therapist, location, date, time
- Displays pricing and deposit information
- Arrival instructions included

### 3. **Booking Cancellation Emails**
- Sent when client cancels appointment
- Shows refund information if applicable
- Includes cancelled appointment details
- Professional and empathetic messaging

### 4. **24-Hour Reminder Emails**
- Background service runs hourly
- Automatically sends reminders 23-25 hours before appointment
- Includes all appointment details
- Reminder about arrival time
- Only sent for confirmed bookings

### 5. **Membership Emails**
- Welcome email when membership is activated
- Shows plan details and benefits
- Cancellation confirmation emails
- Credit balance information

---

## Email Templates

All emails feature:
- ✨ Beautiful HTML design
- 🎨 Pink/rose gold theme matching the brand
- 📱 Mobile-responsive layout
- 📧 Plain text fallback for accessibility
- 🖼️ Professional formatting with gradients and cards

### Template Styles
- **Header**: Rose gold gradient background
- **Content**: Clean white cards with rounded corners
- **Details**: Highlighted information boxes
- **Colors**: Brand-consistent pink (#B76E79) and rose gold (#D4A5A5)
- **Typography**: Clear, readable fonts with proper hierarchy

---

## Configuration

### Email Settings (`appsettings.json`)

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your_email@gmail.com",
  "SmtpPassword": "your_app_password",
  "EnableSsl": true,
  "FromEmail": "noreply@spabooker.com",
  "FromName": "SpaBooker",
  "EnableNotifications": true
}
```

### Configuration Options

| Setting | Description | Example |
|---------|-------------|---------|
| `SmtpHost` | SMTP server hostname | `smtp.gmail.com`, `smtp.sendgrid.net` |
| `SmtpPort` | SMTP port (usually 587 for TLS) | `587` |
| `SmtpUsername` | SMTP authentication username | `your_email@gmail.com` |
| `SmtpPassword` | SMTP authentication password/app password | `your_app_password` |
| `EnableSsl` | Use SSL/TLS encryption | `true` |
| `FromEmail` | Email address shown as sender | `noreply@spabooker.com` |
| `FromName` | Friendly name shown as sender | `SpaBooker` |
| `EnableNotifications` | Master toggle for all emails | `true` / `false` |

---

## SMTP Provider Setup

### Option 1: Gmail (Development/Small Scale)

1. **Enable 2-Factor Authentication** on your Google account
2. **Generate App Password**:
   - Go to Google Account Settings → Security
   - Under "2-Step Verification", find "App passwords"
   - Generate password for "Mail"
3. **Configure**:
   ```json
   {
     "SmtpHost": "smtp.gmail.com",
     "SmtpPort": 587,
     "SmtpUsername": "your_email@gmail.com",
     "SmtpPassword": "your_16_character_app_password",
     "EnableSsl": true
   }
   ```

**Limits**: 500 emails/day

---

### Option 2: SendGrid (Recommended for Production)

1. **Sign up** at https://sendgrid.com/
2. **Create API Key**:
   - Go to Settings → API Keys
   - Create API Key with "Mail Send" permission
3. **Configure**:
   ```json
   {
     "SmtpHost": "smtp.sendgrid.net",
     "SmtpPort": 587,
     "SmtpUsername": "apikey",
     "SmtpPassword": "YOUR_SENDGRID_API_KEY",
     "EnableSsl": true,
     "FromEmail": "noreply@yourdomain.com"
   }
   ```

**Benefits**:
- ✅ 100 emails/day free tier
- ✅ 40,000+ emails/month on paid plans
- ✅ Email analytics and tracking
- ✅ Professional delivery rates
- ✅ Domain authentication support

---

### Option 3: Amazon SES (Large Scale)

1. **Sign up** for AWS
2. **Verify domain** or email address
3. **Get SMTP credentials** from SES console
4. **Configure**:
   ```json
   {
     "SmtpHost": "email-smtp.us-east-1.amazonaws.com",
     "SmtpPort": 587,
     "SmtpUsername": "YOUR_SES_SMTP_USERNAME",
     "SmtpPassword": "YOUR_SES_SMTP_PASSWORD",
     "EnableSsl": true
   }
   ```

**Benefits**:
- ✅ $0.10 per 1,000 emails
- ✅ Highly scalable
- ✅ 99.9% uptime SLA

---

### Option 4: Mailgun, Postmark, etc.

Similar configuration - just update SMTP host, port, and credentials according to your provider's documentation.

---

## Email Types & Triggers

### 1. Booking Confirmation Email

**Triggered**: After successful payment (in `PaymentSuccess.razor`)

**Recipient**: Client who made the booking

**Contents**:
- ✅ Service name and description
- ✅ Therapist name
- ✅ Location name and address
- ✅ Date and time
- ✅ Duration
- ✅ Price breakdown (total, deposit, balance due)
- ✅ Arrival instructions
- ✅ Cancellation policy reminder

**Code**:
```csharp
await EmailService.SendBookingConfirmationAsync(bookingId);
```

---

### 2. Booking Cancellation Email

**Triggered**: When client cancels booking (in `MyBookings.razor`)

**Recipient**: Client who cancelled

**Contents**:
- ✅ Cancelled appointment details
- ✅ Refund information (if applicable)
- ✅ Rebooking invitation
- ✅ Contact information

**Code**:
```csharp
await EmailService.SendBookingCancellationAsync(bookingId);
```

---

### 3. 24-Hour Reminder Email

**Triggered**: Automatically by `BookingReminderService` background task

**Timing**: 23-25 hours before appointment (checks every hour)

**Recipient**: Client with upcoming appointment

**Contents**:
- ✅ Prominent "Appointment Tomorrow" message
- ✅ Full appointment details
- ✅ Therapist and location
- ✅ Arrival time reminder
- ✅ Contact information

**Code** (automated):
```csharp
// Runs automatically in background
// No manual trigger needed
```

---

### 4. Membership Confirmation Email

**Triggered**: When membership subscription is activated

**Recipient**: New member

**Contents**:
- ✅ Welcome message
- ✅ Membership plan details
- ✅ Monthly credits
- ✅ Discount percentage
- ✅ Benefits list
- ✅ Next billing date

**Code**:
```csharp
await EmailService.SendMembershipConfirmationAsync(userMembershipId);
```

---

### 5. Membership Cancellation Email

**Triggered**: When member cancels subscription

**Recipient**: Former member

**Contents**:
- ✅ Cancellation confirmation
- ✅ Remaining credits
- ✅ Access until date
- ✅ Reactivation information

**Code**:
```csharp
await EmailService.SendMembershipCancellationAsync(userMembershipId);
```

---

## Background Service Details

### BookingReminderService

**Type**: Hosted Background Service

**Schedule**: Runs every hour

**Logic**:
1. Query database for bookings 23-25 hours away
2. Filter for confirmed bookings only
3. Send reminder email for each booking
4. Log success/failure for each attempt

**Error Handling**:
- Individual email failures don't crash service
- Errors logged for troubleshooting
- Service continues on next scheduled run

**Code Location**: `src/SpaBooker.Infrastructure/Services/BookingReminderService.cs`

**Registration**: Automatically registered as `HostedService` in `Program.cs`

---

## Implementation Details

### EmailService Methods

#### `SendEmailAsync(to, subject, htmlBody, plainTextBody)`
Low-level method for sending any email.

#### `SendBookingConfirmationAsync(bookingId)`
Loads booking with all relations, generates HTML template, sends email.

#### `SendBookingCancellationAsync(bookingId)`
Loads booking, checks refund status, generates template, sends email.

#### `SendBookingReminderAsync(bookingId)`
Loads booking, generates reminder template with prominent styling, sends email.

#### `SendMembershipConfirmationAsync(userMembershipId)`
Loads membership and plan details, generates welcome email, sends.

#### `SendMembershipCancellationAsync(userMembershipId)`
Loads membership, generates cancellation confirmation, sends.

---

## Testing

### Test Email Sending (Development)

1. **Set EnableNotifications to false** initially:
   ```json
   "EmailSettings": {
     "EnableNotifications": false
   }
   ```

2. **Configure SMTP with Gmail** (easiest for testing)

3. **Enable notifications**:
   ```json
   "EnableNotifications": true
   ```

4. **Test each email type**:

**Test Booking Confirmation**:
1. Create a booking
2. Complete payment
3. Check email inbox

**Test Cancellation**:
1. Cancel a booking
2. Check email inbox

**Test Reminder**:
1. Create booking scheduled for ~24 hours from now
2. Wait for background service to run (checks every hour)
3. OR restart application to trigger check immediately
4. Check email inbox

**Test Membership**:
1. Subscribe to membership
2. Check email inbox
3. Cancel membership
4. Check email inbox

---

## Email Delivery Best Practices

### 1. **Domain Authentication**
For production, set up SPF, DKIM, and DMARC records:
- Prevents emails from going to spam
- Improves delivery rates
- Professional reputation

### 2. **Unsubscribe Links** (Future Enhancement)
Consider adding unsubscribe options for:
- Reminder emails (require opt-in/opt-out)
- Marketing emails

### 3. **Rate Limiting**
Background service sends emails hourly to avoid:
- SMTP provider rate limits
- Spam detection triggers

### 4. **Monitoring**
Consider implementing:
- Email delivery tracking
- Bounce handling
- Open/click tracking (with user consent)

---

## Error Handling

### Email Failures Don't Block Operations

All email sending is wrapped in try-catch blocks:
- **Booking confirmation fails**: User still sees success page
- **Cancellation email fails**: Cancellation still processed
- **Reminder fails**: Service continues to next booking

### Logging

All email operations are logged:
- **Success**: Info level with recipient and subject
- **Failure**: Error level with exception details

### Troubleshooting

**Email not received?**
1. Check `EnableNotifications` is `true`
2. Verify SMTP credentials
3. Check spam folder
4. Review application logs
5. Test SMTP connection separately

**Gmail "Less Secure Apps" Error**:
- Use App Password, not account password
- Enable 2-Factor Authentication first

**SendGrid Errors**:
- Verify API key has "Mail Send" permission
- Check sender email is verified
- Review SendGrid dashboard for blocks

---

## Future Enhancements (Optional)

### Phase 2
1. **SMS Notifications** via Twilio
2. **Email Templates in Database** (editable by admins)
3. **Scheduled Send** (queue emails for later)
4. **Email Preferences** (let users choose which emails to receive)

### Phase 3
1. **Email Analytics** (open rates, click rates)
2. **A/B Testing** for email templates
3. **Drip Campaigns** for marketing
4. **Automated Re-engagement** emails

---

## Architecture

### Service Layer
```
IEmailService (Interface)
  ↓
EmailService (Implementation)
  ↓
MailKit SMTP Client
  ↓
SMTP Provider (Gmail, SendGrid, etc.)
```

### Dependencies
- **MailKit** (v4.14.1): Email sending
- **MimeKit** (v4.14.0): Email message construction
- **Microsoft.Extensions.Hosting.Abstractions** (v10.0.0): Background services

### Registration
```csharp
// Program.cs
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<BookingReminderService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
```

---

## Security Considerations

### 1. **SMTP Credentials**
- ✅ Store in `appsettings.json` (local dev)
- ✅ Use environment variables (production)
- ✅ Never commit passwords to source control
- ✅ Use app passwords, not account passwords

### 2. **User Data**
- ✅ Only send emails to verified addresses
- ✅ Include unsubscribe options (future)
- ✅ Respect user privacy preferences

### 3. **Email Content**
- ✅ No sensitive data in plain text
- ✅ Use HTTPS for any links
- ✅ Sanitize user-generated content

---

## Cost Analysis

### Gmail (Free Tier)
- **Limit**: 500 emails/day
- **Cost**: Free
- **Best For**: Development, very small businesses

### SendGrid (Free Tier)
- **Limit**: 100 emails/day
- **Cost**: Free
- **Best For**: Small businesses, testing

### SendGrid (Paid)
- **Essentials**: $19.95/month (40,000 emails/month)
- **Pro**: $89.95/month (100,000 emails/month)
- **Best For**: Growing businesses

### Amazon SES
- **Cost**: $0.10 per 1,000 emails
- **Example**: 10,000 emails/month = $1/month
- **Best For**: High volume, cost-conscious

---

## Example Spa Usage

### Two Locations, 10 Employees, 5,000 Bookings/Year

**Email Volume Estimate**:
- Booking confirmations: 5,000/year
- Reminders: 5,000/year (24h before)
- Cancellations: ~500/year (10% cancellation rate)
- **Total**: ~10,500 emails/year
- **Monthly Average**: ~875 emails/month

**Recommended Provider**: SendGrid Free Tier (100/day = 3,000/month)

**Cost**: $0/month

---

## Summary

**Status**: ✅ Complete and Production Ready

The email notification system provides:
- ✅ Automated booking confirmations
- ✅ Cancellation notifications with refund info
- ✅ 24-hour appointment reminders
- ✅ Membership welcome and cancellation emails
- ✅ Beautiful, branded HTML templates
- ✅ Flexible SMTP provider support
- ✅ Background service for automated reminders
- ✅ Robust error handling and logging
- ✅ Easy configuration and testing

Clients now receive professional, timely email communications for all important events! 📧✨

