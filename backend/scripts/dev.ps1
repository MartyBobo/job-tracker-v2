# JobTracker Backend Development Script for Windows

Write-Host "Starting JobTracker Backend Development Server..." -ForegroundColor Green

Set-Location "$PSScriptRoot\.."

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5250"

if (-not (Test-Path "appsettings.Development.json")) {
    Write-Host "Creating appsettings.Development.json..." -ForegroundColor Yellow
    Copy-Item "src\JobTracker.API\appsettings.Development.json" -Destination "." -ErrorAction SilentlyContinue
}

Write-Host "Running database migrations..." -ForegroundColor Yellow
dotnet ef database update --project src\JobTracker.Infrastructure --startup-project src\JobTracker.API
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migrations skipped or failed" -ForegroundColor Yellow
}

Write-Host "Starting API server on http://localhost:5250..." -ForegroundColor Green
dotnet watch run --project src\JobTracker.API --launch-profile "Development"