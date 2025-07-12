#!/bin/bash
# Setup JobTracker solution using Windows .NET installation

DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"

echo "Creating JobTracker solution..."

# Create solution file
"$DOTNET" new sln -n JobTracker

# Create projects
echo "Creating Domain project..."
"$DOTNET" new classlib -n JobTracker.Domain -o src/JobTracker.Domain -f net8.0

echo "Creating Application project..."
"$DOTNET" new classlib -n JobTracker.Application -o src/JobTracker.Application -f net8.0

echo "Creating Infrastructure project..."
"$DOTNET" new classlib -n JobTracker.Infrastructure -o src/JobTracker.Infrastructure -f net8.0

echo "Creating API project..."
"$DOTNET" new webapi -n JobTracker.API -o src/JobTracker.API -f net8.0 --use-minimal-apis

echo "Creating Shared project..."
"$DOTNET" new classlib -n Shared -o src/Shared -f net8.0

# Add projects to solution
"$DOTNET" sln add src/JobTracker.Domain/JobTracker.Domain.csproj
"$DOTNET" sln add src/JobTracker.Application/JobTracker.Application.csproj
"$DOTNET" sln add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj
"$DOTNET" sln add src/JobTracker.API/JobTracker.API.csproj
"$DOTNET" sln add src/Shared/Shared.csproj

# Add project references (following Clean Architecture dependencies)
echo "Setting up project references..."

# Application depends on Domain and Shared
"$DOTNET" add src/JobTracker.Application/JobTracker.Application.csproj reference src/JobTracker.Domain/JobTracker.Domain.csproj
"$DOTNET" add src/JobTracker.Application/JobTracker.Application.csproj reference src/Shared/Shared.csproj

# Infrastructure depends on Application (and transitively Domain)
"$DOTNET" add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj reference src/JobTracker.Application/JobTracker.Application.csproj

# API depends on Application and Infrastructure
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj reference src/JobTracker.Application/JobTracker.Application.csproj
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj reference src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj

# Install required NuGet packages
echo "Installing NuGet packages..."

# Domain - no external dependencies (Clean Architecture rule)

# Application - MediatR
"$DOTNET" add src/JobTracker.Application/JobTracker.Application.csproj package MediatR
"$DOTNET" add src/JobTracker.Application/JobTracker.Application.csproj package FluentValidation
"$DOTNET" add src/JobTracker.Application/JobTracker.Application.csproj package FluentValidation.DependencyInjectionExtensions

# Infrastructure - EF Core with SQLite
"$DOTNET" add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
"$DOTNET" add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
"$DOTNET" add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# API - Additional packages
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj package Microsoft.EntityFrameworkCore.Design
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj package Serilog.AspNetCore
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj package Serilog.Sinks.Console
"$DOTNET" add src/JobTracker.API/JobTracker.API.csproj package Serilog.Sinks.File

echo "Solution setup complete!"
echo "Run 'dotnet build' to verify everything is set up correctly."