# Email Notifications System

## Overview

Automated email notifications for booking confirmations, cancellations, reminders, and membership updates. The system uses MailKit for email delivery and includes branded HTML templates.

---

## Features

### Booking Emails
- **Confirmation**: Sent after successful payment
- **Cancellation**: Sent when client cancels
- **24-Hour Reminder**: Automated reminder before appointment

### Membership Emails
- **Welcome**: Sent when subscription is activated
- **Cancellation**: Sent when member cancels

---

## Email Templates

All emails feature:
- Beautiful HTML design
- Pink/rose gold theme matching the brand
- Mobile-responsive layout
- Plain text fallback for accessibility

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
| `SmtpHost` | SMTP server hostname | `smtp.gmail.com` |
| `SmtpPort` | SMTP port (usually 587 for TLS) | `587` |
| `SmtpUsername` | SMTP authentication username | `your_email@gmail.com` |
| `SmtpPassword` | SMTP authentication password | `your_app_password` |
| `EnableSsl` | Use SSL/TLS encryption | `true` |
| `FromEmail` | Email address shown as sender | `noreply@spabooker.com` |
| `FromName` | Friendly name shown as sender | `SpaBooker` |
| `EnableNotifications` | Master toggle for all emails | `true` |

---

## SMTP Provider Setup

### Gmail (Development)

1. Enable 2-Factor Authentication on your Google account
2. Generate App Password:
   - Go to Google Account Settings → Security
   - Under "2-Step Verification", find "App passwords"
   - Generate password for "Mail"
3. Configure with generated 16-character password

**Limits**: 500 emails/day

### SendGrid (Recommended for Production)

1. Sign up at https://sendgrid.com/
2. Create API Key with "Mail Send" permission
3. Configure:
   ```json
   {
     "SmtpHost": "smtp.sendgrid.net",
     "SmtpPort": 587,
     "SmtpUsername": "apikey",
     "SmtpPassword": "YOUR_SENDGRID_API_KEY"
   }
   ```

**Benefits**:
- 100 emails/day free tier
- 40,000+ emails/month on paid plans
- Email analytics and tracking
- Domain authentication support

### Amazon SES (Large Scale)

1. Sign up for AWS
2. Verify domain or email address
3. Get SMTP credentials from SES console
4. Configure with SES SMTP endpoint

**Benefits**:
- $0.10 per 1,000 emails
- Highly scalable
- 99.9% uptime SLA

---

## Email Types & Triggers

### Booking Confirmation
- **Trigger**: After successful payment
- **Contents**: Service, therapist, location, date, time, pricing, arrival instructions

### Booking Cancellation
- **Trigger**: When client cancels
- **Contents**: Cancelled appointment details, refund information, rebooking invitation

### 24-Hour Reminder
- **Trigger**: Automated by background service (runs hourly)
- **Timing**: 23-25 hours before appointment
- **Contents**: Appointment details, therapist, location, arrival reminder

### Membership Welcome
- **Trigger**: When subscription is activated
- **Contents**: Plan details, credits, discount, benefits, next billing date

### Membership Cancellation
- **Trigger**: When member cancels
- **Contents**: Confirmation, remaining credits, access until date

---

## Background Service

### BookingReminderService
- **Schedule**: Runs every hour
- **Logic**: Queries bookings 23-25 hours away, sends reminders
- **Error Handling**: Individual failures don't crash service

---

## Error Handling

Email failures don't block operations:
- **Booking confirmation fails**: User still sees success page
- **Cancellation email fails**: Cancellation still processed
- **Reminder fails**: Service continues to next booking

All operations are logged for troubleshooting.

---

## Security Considerations

### SMTP Credentials
- Store in User Secrets (development)
- Use environment variables (production)
- Never commit passwords to source control
- Use app passwords, not account passwords

### User Data
- Only send emails to verified addresses
- Include unsubscribe options (future)
- Respect user privacy preferences

---

## Cost Analysis

| Provider | Free Tier | Paid Tier |
|----------|-----------|-----------|
| Gmail | 500/day | N/A |
| SendGrid | 100/day | $19.95/mo (40k) |
| Amazon SES | N/A | $0.10/1,000 |

### Example Usage
Two locations, 10 employees, 5,000 bookings/year:
- ~10,500 emails/year
- ~875 emails/month
- **Recommendation**: SendGrid Free Tier

