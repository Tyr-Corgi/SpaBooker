#!/bin/bash
# SpaBooker User Secrets Setup Script
# This script helps you configure user secrets for local development

echo "🔒 SpaBooker User Secrets Setup"
echo "================================"
echo ""
echo "This script will help you set up user secrets for local development."
echo "These secrets will be stored securely on your machine (not in git)."
echo ""

# Navigate to web project
cd "$(dirname "$0")/../src/SpaBooker.Web" || exit

# Initialize user secrets if not already initialized
echo "📝 Initializing user secrets..."
dotnet user-secrets init

echo ""
echo "Now, let's set up your secrets one by one."
echo ""

# Database Connection
echo "🗄️  DATABASE CONFIGURATION"
echo "-------------------------"
read -p "PostgreSQL Host (default: localhost): " DB_HOST
DB_HOST=${DB_HOST:-localhost}

read -p "PostgreSQL Database (default: spabooker): " DB_NAME
DB_NAME=${DB_NAME:-spabooker}

read -p "PostgreSQL Username (default: postgres): " DB_USER
DB_USER=${DB_USER:-postgres}

read -sp "PostgreSQL Password: " DB_PASS
echo ""

CONNECTION_STRING="Host=$DB_HOST;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASS"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$CONNECTION_STRING"
echo "✅ Database connection configured"
echo ""

# Stripe Configuration
echo "💳 STRIPE CONFIGURATION"
echo "----------------------"
echo "Get your Stripe keys from: https://dashboard.stripe.com/test/apikeys"
read -p "Stripe Publishable Key (pk_test_...): " STRIPE_PUB
read -sp "Stripe Secret Key (sk_test_...): " STRIPE_SECRET
echo ""
read -sp "Stripe Webhook Secret (whsec_...): " STRIPE_WEBHOOK
echo ""

dotnet user-secrets set "Stripe:PublishableKey" "$STRIPE_PUB"
dotnet user-secrets set "Stripe:SecretKey" "$STRIPE_SECRET"
dotnet user-secrets set "Stripe:WebhookSecret" "$STRIPE_WEBHOOK"
echo "✅ Stripe keys configured"
echo ""

# Email Configuration
echo "📧 EMAIL CONFIGURATION"
echo "---------------------"
read -p "SMTP Host (default: smtp.gmail.com): " SMTP_HOST
SMTP_HOST=${SMTP_HOST:-smtp.gmail.com}

read -p "SMTP Port (default: 587): " SMTP_PORT
SMTP_PORT=${SMTP_PORT:-587}

read -p "SMTP Username (your email): " SMTP_USER
read -sp "SMTP Password (app password for Gmail): " SMTP_PASS
echo ""

read -p "From Email (default: noreply@spabooker.com): " FROM_EMAIL
FROM_EMAIL=${FROM_EMAIL:-noreply@spabooker.com}

dotnet user-secrets set "EmailSettings:SmtpHost" "$SMTP_HOST"
dotnet user-secrets set "EmailSettings:SmtpPort" "$SMTP_PORT"
dotnet user-secrets set "EmailSettings:SmtpUsername" "$SMTP_USER"
dotnet user-secrets set "EmailSettings:SmtpPassword" "$SMTP_PASS"
dotnet user-secrets set "EmailSettings:FromEmail" "$FROM_EMAIL"
echo "✅ Email settings configured"
echo ""

# List all secrets (without values)
echo "📋 Your configured secrets:"
dotnet user-secrets list

echo ""
echo "✅ Setup complete!"
echo ""
echo "Your secrets are stored securely at:"
echo "  Windows: %APPDATA%\\Microsoft\\UserSecrets\\[user-secrets-id]"
echo "  macOS/Linux: ~/.microsoft/usersecrets/[user-secrets-id]"
echo ""
echo "These secrets will be used automatically when you run 'dotnet run'."
echo ""
echo "⚠️  IMPORTANT: Never commit these secrets to git!"
echo ""

