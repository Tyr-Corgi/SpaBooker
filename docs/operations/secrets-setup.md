# Secrets Configuration Guide

## Important

**Never commit secrets to Git!**

This project uses User Secrets for development and should use Azure Key Vault or environment variables for production.

---

## Development: User Secrets

### Step 1: Initialize User Secrets

```bash
cd src/SpaBooker.Web
dotnet user-secrets init
```

### Step 2: Configure Required Secrets

#### Database Connection
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=spabooker;Username=postgres;Password=YOUR_ACTUAL_PASSWORD"
```

#### Stripe API Keys
Get these from your [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)

```bash
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_ACTUAL_KEY"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_ACTUAL_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_ACTUAL_SECRET"
```

#### Email Configuration
For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833)

```bash
dotnet user-secrets set "EmailSettings:SmtpUsername" "your_email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "YOUR_APP_PASSWORD"
```

### Step 3: Verify Secrets

```bash
dotnet user-secrets list
```

---

## Production Options

### Option 1: Azure Key Vault (Recommended)

1. Create an Azure Key Vault
2. Add secrets to the vault
3. Configure managed identity for your App Service
4. Add Key Vault reference in Azure App Service Configuration:

```
@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/ConnectionStrings--DefaultConnection/)
```

### Option 2: Environment Variables

Set environment variables in your hosting platform:

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Stripe__PublishableKey` | Stripe public key |
| `Stripe__SecretKey` | Stripe secret key |
| `Stripe__WebhookSecret` | Stripe webhook signing secret |
| `EmailSettings__SmtpUsername` | SMTP username |
| `EmailSettings__SmtpPassword` | SMTP password |

#### Azure App Service
In Azure Portal → Configuration → Application Settings, add each variable.

#### Docker
```bash
docker run -e "ConnectionStrings__DefaultConnection=Host=..." \
           -e "Stripe__SecretKey=sk_live_..." \
           -e "EmailSettings__SmtpPassword=..." \
           spabooker:latest
```

### Option 3: AWS Secrets Manager

Use AWS Secrets Manager and configure your application to retrieve secrets at runtime using the AWS SDK.

---

## Security Checklist

- [ ] All secrets removed from appsettings.json
- [ ] User secrets configured for local development
- [ ] Production secrets stored in Azure Key Vault / AWS Secrets Manager
- [ ] Old exposed credentials rotated (database password, Stripe keys, email password)
- [ ] No secrets in git history (use git filter-repo if needed)

---

## Rotating Exposed Credentials

If credentials were previously committed to git:

1. **Database**: Change PostgreSQL password
2. **Stripe**: Go to Stripe Dashboard → Developers → API keys → Roll keys
3. **Email**: Generate new Gmail App Password
4. **Webhook**: Update webhook secret in Stripe Dashboard

---

## Troubleshooting

### "Secrets not loading"
- Ensure you're in the correct project directory
- Verify secrets file exists: `%APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json`
- Check the `UserSecretsId` in your `.csproj` file

### "Connection string not found"
- Verify the secret name matches exactly (case-sensitive)
- Check for typos in the connection string format

### "Stripe keys not working"
- Ensure you're using test keys for development
- Verify keys are from the correct Stripe account
- Check that webhook secret matches the endpoint

