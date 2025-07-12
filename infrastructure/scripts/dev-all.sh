#!/bin/bash

echo "Starting Job Tracker Development Environment..."

# Function to kill processes on exit
cleanup() {
    echo "Stopping services..."
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    exit
}

trap cleanup EXIT

# Start backend
echo "Starting backend on http://localhost:5250..."
(cd ../backend && ./run dev) &
BACKEND_PID=$!

# Wait for backend to be ready
echo "Waiting for backend to start..."
sleep 5

# Start frontend
echo "Starting frontend on http://localhost:3000..."
(cd ../frontend && npm run dev) &
FRONTEND_PID=$!

echo "Both services are starting..."
echo "Backend: http://localhost:5250"
echo "Frontend: http://localhost:3000"
echo "Press Ctrl+C to stop all services"

# Wait for both processes
wait