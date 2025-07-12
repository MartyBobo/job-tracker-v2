#!/bin/bash

echo "Starting JobTracker Backend Development Server..."

cd "$(dirname "$0")/.."

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5250"

if [ ! -f "appsettings.Development.json" ]; then
    echo "Creating appsettings.Development.json..."
    cp src/JobTracker.API/appsettings.Development.json . 2>/dev/null || echo "Warning: No development settings found"
fi

echo "Running database migrations..."
dotnet ef database update --project src/JobTracker.Infrastructure --startup-project src/JobTracker.API || echo "Migrations skipped"

echo "Starting API server on http://localhost:5250..."
dotnet watch run --project src/JobTracker.API --launch-profile "Development"