#!/bin/bash
# Create and run initial migration

echo "Installing EF Core tools..."
dotnet tool install --global dotnet-ef

echo "Creating initial migration..."
cd src/JobTracker.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../JobTracker.API

echo "Updating database..."
dotnet ef database update --startup-project ../JobTracker.API

echo "Migration complete! Database created at: job_tracker.db"