@echo off
echo Starting Job Tracker with Docker...
echo ==================================

REM Check if Docker is running
docker version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not running!
    echo Please start Docker Desktop first.
    pause
    exit /b 1
)

echo Building and starting containers...
docker-compose up --build

echo.
echo Containers stopped.
pause