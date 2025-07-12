@echo off
echo Stopping JobTracker Backend on port 5250...

for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5250 ^| findstr LISTENING') do (
    echo Killing process %%a
    taskkill /PID %%a /F
)

echo Backend stopped.
pause