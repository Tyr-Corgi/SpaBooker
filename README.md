# SpaBooker - Spa Booking Management System

A comprehensive spa booking and management system built with ASP.NET Core 8, Blazor Server, and PostgreSQL.

## Documentation

For detailed documentation, see the [docs/](docs/) folder:

- [Architecture Overview](docs/architecture/overview.md)
- [Security Guide](docs/guides/security.md)
- [Testing Guidelines](docs/guides/testing.md)
- [Stripe Integration](docs/operations/stripe-integration.md)
- [Changelog](docs/CHANGELOG.md)

## Architecture

- **Pattern**: Vertical Slice Architecture
- **Frontend**: Blazor Server with pink/rose gold theme
- **Backend**: ASP.NET Core 8 Web API
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Payments**: Stripe

## Features

### ✅ Completed
- **Authentication & Authorization**: Multi-role system (Client, Therapist, Admin) with ASP.NET Core Identity
- **Service Management**: Browse spa services by location with detailed descriptions and pricing
- **Location Selection**: Store-style location picker with search by city/ZIP code
- **Booking System**: 
  - Real-time availability checking with therapist schedule integration
  - Conflict prevention for double-booking
  - Therapist selection
  - Time slot selection with 30-minute intervals
  - Booking management (view, cancel)
  - Stripe payment integration for deposits (50%)
  - Refund handling for cancellations
- **Therapist Availability Management**:
  - Set weekly working hours per day
  - Block specific dates (vacations, personal time)
  - Automatic integration with booking system
  - Only show available time slots to clients
- **Membership System**:
  - Flexible membership plans with monthly credits
  - Unlimited credit rollover
  - Credits expire 12 months after issuance
  - Automatic expiration tracking
  - Transaction history with credit lifecycle
  - Stripe subscription integration
- **Email Notifications**:
  - Automated booking confirmation emails
  - Cancellation notifications with refund details
  - 24-hour appointment reminders (background service)
  - Membership welcome and cancellation emails
  - Beautiful HTML templates with pink/rose gold theme
  - Supports Gmail, SendGrid, Amazon SES, and other SMTP providers
- **Responsive Design**: Pink/rose gold themed UI that works on desktop and mobile

- **Admin Management**:
  - User management (CRUD operations, role assignment, account locking)
  - Service management (create, edit, activate/deactivate services)
  - Therapist assignment to services
  - Location-based filtering
  - Search and filtering capabilities

### 🚧 In Progress
- Inventory tracking system

## Project Structure

```
SpaBooker/
├── src/
│   ├── SpaBooker.Web/              # Blazor Server application
│   │   ├── Features/               # Vertical slices
│   │   │   ├── Auth/              # Authentication features
│   │   │   ├── Bookings/          # Booking management
│   │   │   ├── Memberships/       # Membership features
│   │   │   ├── Scheduling/        # Therapist scheduling
│   │   │   ├── Inventory/         # Inventory management
│   │   │   └── Admin/             # Admin features
│   │   └── Components/
│   │       ├── Layout/            # Layout components
│   │       ├── Pages/             # Page components
│   │       └── Shared/            # Shared components
│   ├── SpaBooker.Core/            # Domain models and interfaces
│   │   ├── Entities/              # Domain entities
│   │   ├── Enums/                 # Enumerations
│   │   └── Interfaces/            # Interfaces
│   └── SpaBooker.Infrastructure/  # Data access and external services
│       └── Data/                  # EF Core DbContext and configurations
└── tests/                         # Test projects (future)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Stripe Account](https://stripe.com/) (for payment processing)

## PostgreSQL Setup

### Option 1: Local PostgreSQL Installation

1. **Install PostgreSQL**:
   - Download and install PostgreSQL from https://www.postgresql.org/download/
   - During installation, set a password for the `postgres` user

2. **Create the Database**:
   ```bash
   # Connect to PostgreSQL
   psql -U postgres
   
   # Create database
   CREATE DATABASE spabooker;
   
   # Exit psql
   \q
   ```

3. **Update Connection String**:
   Edit `src/SpaBooker.Web/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=spabooker;Username=postgres;Password=your_actual_password"
   }
   ```

### Option 2: Docker PostgreSQL

```bash
docker run --name spabooker-postgres -e POSTGRES_PASSWORD=your_password -e POSTGRES_DB=spabooker -p 5432:5432 -d postgres:14
```

### Option 3: Azure PostgreSQL (Production)

See deployment section below for Azure setup instructions.

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd SpaBooker
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Settings

Update `src/SpaBooker.Web/appsettings.Development.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=spabooker;Username=postgres;Password=your_password"
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

### 4. Create Initial Migration

```bash
cd src/SpaBooker.Web
dotnet ef migrations add InitialCreate --project ../SpaBooker.Infrastructure
```

### 5. Update Database

```bash
dotnet ef database update --project ../SpaBooker.Infrastructure
```

### 6. Run the Application

```bash
dotnet run
```

Navigate to `https://localhost:5001` in your browser.

## Database Migrations

### Create a New Migration

```bash
cd src/SpaBooker.Web
dotnet ef migrations add <MigrationName> --project ../SpaBooker.Infrastructure
```

### Apply Migrations

```bash
dotnet ef database update --project ../SpaBooker.Infrastructure
```

### Remove Last Migration (if not applied)

```bash
dotnet ef migrations remove --project ../SpaBooker.Infrastructure
```

## Configuration

### Roles

The system supports three roles:
- **Client**: Can book appointments, manage profile, view membership
- **Therapist**: Can view schedule, manage availability, see client bookings
- **Admin**: Full system access, manage users, services, inventory

### Initial Admin Credentials

The database is automatically seeded with an admin account on first run:
- **Email**: admin@spabooker.com
- **Password**: Admin123!@#$ (12 characters minimum for security)
- **Role**: Admin

⚠️ **Important**: Change this password immediately after first login in a production environment.

### Test Data

The system includes comprehensive test data for development:
- **15 Cartoon Network Test Clients** (Ben Tennyson, Dexter McPherson, etc.)
- **5 Cartoon Character Therapists** (Kevin Levin, Marceline Abadeer, Princess Bubblegum, Raven Azarath, Starfire Tamaranean)
- **Sample Bookings**: Past, present, and future appointments for testing
- All test accounts use password: `CartoonTest123!@#$`

### Initial Data

The system automatically seeds:
- Two spa locations (Downtown Spa, Seaside Wellness Center)
- Three membership plans (Bronze, Silver, Gold)
- Eight spa services per location (Swedish Massage, Deep Tissue, Hot Stone, etc.)

## Development

### Adding a New Feature (Vertical Slice)

1. Create a new folder in `src/SpaBooker.Web/Features/<FeatureName>`
2. Add Razor components for UI
3. Add command/query handlers for business logic
4. Add DTOs and validators as needed
5. Register services in `Program.cs` if needed

### Color Theme

The application uses a pink and rose gold color palette:
- Primary: Rose Gold (#B76E79, #E8B4B8)
- Accent: Soft Pink (#FFC0CB, #FFB6C1)
- Backgrounds: Light cream (#FFF5F5, #FFFFFF)
- Text: Charcoal (#2D2D2D)

## Deployment

### Azure Deployment

1. **Create Azure Resources**:
   - App Service (Linux, .NET 8)
   - Azure Database for PostgreSQL

2. **Configure Connection String**:
   Add connection string to App Service configuration

3. **Deploy**:
   ```bash
   dotnet publish -c Release
   # Use Azure CLI or GitHub Actions for deployment
   ```

## Contributing

This is a private project for a specific spa chain. Contact the repository owner for contribution guidelines.

## License

Proprietary - All rights reserved

## Support

For issues or questions, contact the development team.

