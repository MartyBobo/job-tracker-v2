#!/bin/bash
# Setup JobTracker solution with Clean Architecture

echo "Creating JobTracker solution..."

# Create solution file
dotnet new sln -n JobTracker

# Create projects
echo "Creating Domain project..."
dotnet new classlib -n JobTracker.Domain -o src/JobTracker.Domain -f net8.0

echo "Creating Application project..."
dotnet new classlib -n JobTracker.Application -o src/JobTracker.Application -f net8.0

echo "Creating Infrastructure project..."
dotnet new classlib -n JobTracker.Infrastructure -o src/JobTracker.Infrastructure -f net8.0

echo "Creating API project..."
dotnet new webapi -n JobTracker.API -o src/JobTracker.API -f net8.0 --use-minimal-apis

echo "Creating Shared project..."
dotnet new classlib -n Shared -o src/Shared -f net8.0

# Add projects to solution
dotnet sln add src/JobTracker.Domain/JobTracker.Domain.csproj
dotnet sln add src/JobTracker.Application/JobTracker.Application.csproj
dotnet sln add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj
dotnet sln add src/JobTracker.API/JobTracker.API.csproj
dotnet sln add src/Shared/Shared.csproj

# Add project references (following Clean Architecture dependencies)
echo "Setting up project references..."

# Application depends on Domain and Shared
dotnet add src/JobTracker.Application/JobTracker.Application.csproj reference src/JobTracker.Domain/JobTracker.Domain.csproj
dotnet add src/JobTracker.Application/JobTracker.Application.csproj reference src/Shared/Shared.csproj

# Infrastructure depends on Application (and transitively Domain)
dotnet add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj reference src/JobTracker.Application/JobTracker.Application.csproj

# API depends on Application and Infrastructure
dotnet add src/JobTracker.API/JobTracker.API.csproj reference src/JobTracker.Application/JobTracker.Application.csproj
dotnet add src/JobTracker.API/JobTracker.API.csproj reference src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj

# Install required NuGet packages
echo "Installing NuGet packages..."

# Domain - no external dependencies (Clean Architecture rule)

# Application - MediatR
dotnet add src/JobTracker.Application/JobTracker.Application.csproj package MediatR
dotnet add src/JobTracker.Application/JobTracker.Application.csproj package FluentValidation
dotnet add src/JobTracker.Application/JobTracker.Application.csproj package FluentValidation.DependencyInjectionExtensions

# Infrastructure - EF Core with SQLite
dotnet add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/JobTracker.Infrastructure/JobTracker.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# API - Additional packages
dotnet add src/JobTracker.API/JobTracker.API.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/JobTracker.API/JobTracker.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/JobTracker.API/JobTracker.API.csproj package Serilog.AspNetCore
dotnet add src/JobTracker.API/JobTracker.API.csproj package Serilog.Sinks.Console
dotnet add src/JobTracker.API/JobTracker.API.csproj package Serilog.Sinks.File

echo "Solution setup complete!"
echo "Run 'dotnet build' to verify everything is set up correctly."