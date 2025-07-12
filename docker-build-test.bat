@echo off
echo Testing Docker build fix...
echo ==========================

REM Build just the backend to test
echo Building backend container...
docker-compose build backend

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above.
    pause
    exit /b 1
)

echo.
echo SUCCESS: Docker build completed!
echo.
echo You can now run the full application with:
echo   docker-compose up
echo.
pause