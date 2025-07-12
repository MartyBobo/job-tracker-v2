@echo off
echo Testing Docker setup...
echo ======================

REM Check Docker
docker --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not installed or not in PATH
    pause
    exit /b 1
)

echo Docker is installed

REM Test build
echo.
echo Testing Docker build...
docker-compose build --no-cache backend

if errorlevel 1 (
    echo ERROR: Docker build failed
    pause
    exit /b 1
)

echo.
echo Docker build successful!
echo.
echo You can now run: docker-compose up
pause