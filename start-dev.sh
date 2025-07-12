#!/bin/bash

echo "Starting Job Tracker Application..."

# Kill any existing processes on ports 3000 and 5250
lsof -ti:3000 | xargs kill -9 2>/dev/null
lsof -ti:5250 | xargs kill -9 2>/dev/null

# Start mock backend
echo "Starting mock backend on port 5250..."
node mock-backend.js &
BACKEND_PID=$!

# Wait for backend to start
sleep 2

# Start frontend
echo "Starting frontend on port 3000..."
cd frontend && npm run dev &
FRONTEND_PID=$!

echo "Backend PID: $BACKEND_PID"
echo "Frontend PID: $FRONTEND_PID"
echo ""
echo "Application is starting..."
echo "Frontend: http://localhost:3000"
echo "Backend: http://localhost:5250"
echo ""
echo "Press Ctrl+C to stop both servers"

# Wait for Ctrl+C
trap "kill $BACKEND_PID $FRONTEND_PID; exit" INT
wait