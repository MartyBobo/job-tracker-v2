#!/bin/bash
# Script to run the .NET backend using Windows .NET from WSL

echo "🚀 Starting JobTracker Backend API..."
echo "=================================="

# Change to the project directory
cd /mnt/c/Code/Job_app_resume_design

# Run the backend using Windows .NET
/mnt/c/Program\ Files/dotnet/dotnet.exe run --project src/JobTracker.API/JobTracker.API.csproj

# Note: This will run until you press Ctrl+C to stop it
