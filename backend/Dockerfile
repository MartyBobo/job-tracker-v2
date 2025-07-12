# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY JobTracker.sln ./

# Copy project files maintaining directory structure
COPY src/JobTracker.API/*.csproj ./src/JobTracker.API/
COPY src/JobTracker.Domain/*.csproj ./src/JobTracker.Domain/
COPY src/JobTracker.Application/*.csproj ./src/JobTracker.Application/
COPY src/JobTracker.Infrastructure/*.csproj ./src/JobTracker.Infrastructure/
COPY src/Shared/*.csproj ./src/Shared/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ ./src/

# Build
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/JobTracker.API/JobTracker.API.csproj" -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Create directory for database
RUN mkdir -p /app/data

EXPOSE 5250
ENTRYPOINT ["dotnet", "JobTracker.API.dll"]