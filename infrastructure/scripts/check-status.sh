#!/bin/bash
# Script to check the status of backend and frontend services

echo "🔍 JobTracker Service Status"
echo "============================"
echo ""

# Check backend
echo "Backend API (port 5250):"
if curl -s http://localhost:5250/health > /dev/null 2>&1; then
    echo "✅ Running and healthy"
    HEALTH=$(curl -s http://localhost:5250/health)
    echo "   Response: $HEALTH"
else
    echo "❌ Not running or not responding"
fi

echo ""

# Check frontend on port 3000
echo "Frontend (port 3000):"
if curl -s http://localhost:3000 > /dev/null 2>&1; then
    echo "✅ Running"
else
    echo "❌ Not running"
fi

# Check frontend on port 3001
echo ""
echo "Frontend (port 3001):"
if curl -s http://localhost:3001 > /dev/null 2>&1; then
    echo "✅ Running"
else
    echo "❌ Not running"
fi

# Test login endpoint
echo ""
echo "Testing login endpoint:"
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5250/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"test@example.com\",\"password\":\"Test123!\"}" \
  -w "\n%{http_code}")

HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ]; then
    echo "✅ Login endpoint working (HTTP 200)"
else
    echo "❌ Login endpoint failed (HTTP $HTTP_CODE)"
fi

echo ""
echo "============================"
echo ""
echo "To start services:"
echo "  Backend:  ./run-backend.sh"
echo "  Frontend: cd frontend && npm run dev"
echo ""
echo "To stop backend: Run stop-backend.bat from Windows"