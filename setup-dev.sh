#!/bin/bash
# Setup script for SpaBooker development environment
# Run this after cloning the repository

set -e

echo "🚀 Setting up SpaBooker development environment..."
echo ""

# Check for required tools
echo "📋 Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi
echo "✅ .NET SDK found: $(dotnet --version)"

if ! command -v git &> /dev/null; then
    echo "❌ Git not found. Please install Git from https://git-scm.com/"
    exit 1
fi
echo "✅ Git found: $(git --version)"

# Configure Git hooks
echo ""
echo "🔧 Configuring Git hooks..."
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit
echo "✅ Git hooks configured"

# Restore dependencies
echo ""
echo "📦 Restoring NuGet packages..."
dotnet restore
echo "✅ Dependencies restored"

# Build solution
echo ""
echo "🔨 Building solution..."
dotnet build --no-restore
echo "✅ Solution built successfully"

# Run tests to verify setup
echo ""
echo "🧪 Running tests to verify setup..."
dotnet test --no-build --verbosity quiet
echo "✅ All tests passed"

# Optional: Install coverage tools
echo ""
read -p "📊 Would you like to install code coverage tools? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Installing coverage tools..."
    dotnet tool install --global dotnet-coverage 2>/dev/null || echo "Coverage tool already installed"
    dotnet tool install --global dotnet-reportgenerator-globaltool 2>/dev/null || echo "Report generator already installed"
    echo "✅ Coverage tools installed"
fi

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    echo ""
    echo "📝 Creating .env file..."
    cat > .env << 'EOF'
# Database Configuration
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=spabooker;Username=postgres;Password=your_password

# Environment
ASPNETCORE_ENVIRONMENT=Development

# Add other environment variables as needed
EOF
    echo "✅ .env file created (please update with your settings)"
fi

echo ""
echo "✅ Setup complete!"
echo ""
echo "📚 Next steps:"
echo "  1. Read TESTING_GUIDELINES.md to understand testing requirements"
echo "  2. Update .env with your database connection string"
echo "  3. Run 'dotnet ef database update' to create the database"
echo "  4. Start coding! Remember: all changes must include tests"
echo ""
echo "🔍 Useful commands:"
echo "  • Run all tests:        dotnet test"
echo "  • Run unit tests:       dotnet test --filter 'FullyQualifiedName~Tests.Unit'"
echo "  • Run with coverage:    dotnet test --collect:'XPlat Code Coverage'"
echo "  • Start app:            cd src/SpaBooker.Web && dotnet run"
echo ""
echo "Happy coding! 🎉"

