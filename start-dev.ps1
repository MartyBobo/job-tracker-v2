# PowerShell script to start Job Tracker Application

Write-Host "Starting Job Tracker Application..." -ForegroundColor Green

# Kill any existing processes on ports 3000 and 5250
Write-Host "Cleaning up existing processes..." -ForegroundColor Yellow
$processes3000 = netstat -ano | findstr :3000
$processes5250 = netstat -ano | findstr :5250

if ($processes3000) {
    $pids = $processes3000 | ForEach-Object { $_.Split()[-1] } | Sort-Object -Unique
    foreach ($pid in $pids) {
        if ($pid -match '^\d+$' -and $pid -ne '0') {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

if ($processes5250) {
    $pids = $processes5250 | ForEach-Object { $_.Split()[-1] } | Sort-Object -Unique
    foreach ($pid in $pids) {
        if ($pid -match '^\d+$' -and $pid -ne '0') {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

Start-Sleep -Seconds 2

# Start mock backend
Write-Host "Starting mock backend on port 5250..." -ForegroundColor Cyan
$backend = Start-Process node -ArgumentList "mock-backend.js" -PassThru -WindowStyle Hidden

# Wait for backend to start
Start-Sleep -Seconds 3

# Start frontend
Write-Host "Starting frontend on port 3000..." -ForegroundColor Cyan
$frontend = Start-Process powershell -ArgumentList "-Command", "cd frontend; npm run dev" -PassThru -WindowStyle Hidden

Write-Host ""
Write-Host "Application is starting..." -ForegroundColor Green
Write-Host "Frontend: http://localhost:3000" -ForegroundColor Yellow
Write-Host "Backend: http://localhost:5250" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop both servers" -ForegroundColor Red

# Register cleanup on exit
Register-EngineEvent PowerShell.Exiting -Action {
    Stop-Process -Id $backend.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $frontend.Id -Force -ErrorAction SilentlyContinue
}

# Keep script running
try {
    while ($true) {
        Start-Sleep -Seconds 1
        if (-not $backend.HasExited -and -not $frontend.HasExited) {
            # Both processes are still running
        } else {
            Write-Host "One of the processes has stopped!" -ForegroundColor Red
            break
        }
    }
} finally {
    Write-Host "Stopping servers..." -ForegroundColor Yellow
    Stop-Process -Id $backend.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $frontend.Id -Force -ErrorAction SilentlyContinue
}