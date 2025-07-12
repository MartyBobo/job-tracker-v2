# Simple PowerShell script to start both servers in separate windows

Write-Host "Starting Job Tracker Application..." -ForegroundColor Green

# Start backend in new window
Write-Host "Starting backend server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "node mock-backend.js"

# Wait a bit for backend to start
Start-Sleep -Seconds 2

# Start frontend in new window  
Write-Host "Starting frontend server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd frontend; npm run dev"

Write-Host ""
Write-Host "Both servers are starting in separate windows!" -ForegroundColor Green
Write-Host ""
Write-Host "Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Backend:  http://localhost:5250" -ForegroundColor Cyan
Write-Host ""
Write-Host "Close the server windows to stop them." -ForegroundColor Yellow