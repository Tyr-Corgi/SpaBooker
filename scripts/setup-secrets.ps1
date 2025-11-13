# SpaBooker User Secrets Setup Script (PowerShell)
# This script helps you configure user secrets for local development

Write-Host "🔒 SpaBooker User Secrets Setup" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will help you set up user secrets for local development."
Write-Host "These secrets will be stored securely on your machine (not in git)."
Write-Host ""

# Navigate to web project
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$webProjectPath = Join-Path $scriptPath "..\src\SpaBooker.Web"
Set-Location $webProjectPath

# Initialize user secrets if not already initialized
Write-Host "📝 Initializing user secrets..." -ForegroundColor Yellow
dotnet user-secrets init

Write-Host ""
Write-Host "Now, let's set up your secrets one by one."
Write-Host ""

# Database Connection
Write-Host "🗄️  DATABASE CONFIGURATION" -ForegroundColor Green
Write-Host "-------------------------" -ForegroundColor Green
$dbHost = Read-Host "PostgreSQL Host (default: localhost)"
if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "localhost" }

$dbName = Read-Host "PostgreSQL Database (default: spabooker)"
if ([string]::IsNullOrWhiteSpace($dbName)) { $dbName = "spabooker" }

$dbUser = Read-Host "PostgreSQL Username (default: postgres)"
if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "postgres" }

$dbPass = Read-Host "PostgreSQL Password" -AsSecureString
$dbPassPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPass))

$connectionString = "Host=$dbHost;Database=$dbName;Username=$dbUser;Password=$dbPassPlain"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$connectionString"
Write-Host "✅ Database connection configured" -ForegroundColor Green
Write-Host ""

# Stripe Configuration
Write-Host "💳 STRIPE CONFIGURATION" -ForegroundColor Green
Write-Host "----------------------" -ForegroundColor Green
Write-Host "Get your Stripe keys from: https://dashboard.stripe.com/test/apikeys" -ForegroundColor Yellow
$stripePub = Read-Host "Stripe Publishable Key (pk_test_...)"
$stripeSecret = Read-Host "Stripe Secret Key (sk_test_...)" -AsSecureString
$stripeSecretPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($stripeSecret))
$stripeWebhook = Read-Host "Stripe Webhook Secret (whsec_...)" -AsSecureString
$stripeWebhookPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($stripeWebhook))

dotnet user-secrets set "Stripe:PublishableKey" "$stripePub"
dotnet user-secrets set "Stripe:SecretKey" "$stripeSecretPlain"
dotnet user-secrets set "Stripe:WebhookSecret" "$stripeWebhookPlain"
Write-Host "✅ Stripe keys configured" -ForegroundColor Green
Write-Host ""

# Email Configuration
Write-Host "📧 EMAIL CONFIGURATION" -ForegroundColor Green
Write-Host "---------------------" -ForegroundColor Green
$smtpHost = Read-Host "SMTP Host (default: smtp.gmail.com)"
if ([string]::IsNullOrWhiteSpace($smtpHost)) { $smtpHost = "smtp.gmail.com" }

$smtpPort = Read-Host "SMTP Port (default: 587)"
if ([string]::IsNullOrWhiteSpace($smtpPort)) { $smtpPort = "587" }

$smtpUser = Read-Host "SMTP Username (your email)"
$smtpPass = Read-Host "SMTP Password (app password for Gmail)" -AsSecureString
$smtpPassPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($smtpPass))

$fromEmail = Read-Host "From Email (default: noreply@spabooker.com)"
if ([string]::IsNullOrWhiteSpace($fromEmail)) { $fromEmail = "noreply@spabooker.com" }

dotnet user-secrets set "EmailSettings:SmtpHost" "$smtpHost"
dotnet user-secrets set "EmailSettings:SmtpPort" "$smtpPort"
dotnet user-secrets set "EmailSettings:SmtpUsername" "$smtpUser"
dotnet user-secrets set "EmailSettings:SmtpPassword" "$smtpPassPlain"
dotnet user-secrets set "EmailSettings:FromEmail" "$fromEmail"
Write-Host "✅ Email settings configured" -ForegroundColor Green
Write-Host ""

# List all secrets (without values)
Write-Host "📋 Your configured secrets:" -ForegroundColor Cyan
dotnet user-secrets list

Write-Host ""
Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Your secrets are stored securely at:"
Write-Host "  $env:APPDATA\Microsoft\UserSecrets\[user-secrets-id]" -ForegroundColor Yellow
Write-Host ""
Write-Host "These secrets will be used automatically when you run 'dotnet run'."
Write-Host ""
Write-Host "⚠️  IMPORTANT: Never commit these secrets to git!" -ForegroundColor Red
Write-Host ""

Read-Host "Press Enter to exit"

