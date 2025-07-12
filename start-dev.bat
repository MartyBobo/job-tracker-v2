@echo off
echo Starting Job Tracker Application...
echo ==================================

REM Kill existing processes on ports
echo Cleaning up existing processes...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :5250') do taskkill /F /PID %%a 2>nul
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :3000') do taskkill /F /PID %%a 2>nul

timeout /t 2 /nobreak > nul

REM Start mock backend
echo Starting mock backend on port 5250...
start /B node mock-backend.js

timeout /t 3 /nobreak > nul

REM Start frontend
echo Starting frontend on port 3000...
cd frontend
start /B cmd /c npm run dev

cd ..

echo.
echo Application is running!
echo ======================
echo Frontend: http://localhost:3000
echo Backend:  http://localhost:5250
echo.
echo Press Ctrl+C to stop both servers
echo.

REM Keep the window open
pause > nul