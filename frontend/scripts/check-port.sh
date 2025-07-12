#!/bin/bash
# Port availability check script

PORT=${1:-3000}

# Cross-platform port check
check_port() {
    if command -v lsof >/dev/null 2>&1; then
        # Linux/Mac
        lsof -Pi :$PORT -sTCP:LISTEN -t >/dev/null 2>&1
    elif command -v netstat >/dev/null 2>&1; then
        # Windows/Linux fallback
        netstat -an | grep -q ":$PORT.*LISTEN"
    else
        echo "Warning: Cannot check port availability (lsof/netstat not found)"
        return 1
    fi
}

if check_port; then
    echo "❌ Port $PORT is already in use!"
    echo "Please stop the service using port $PORT or configure a different port"
    
    # Try to identify the process
    if command -v lsof >/dev/null 2>&1; then
        echo -e "\nProcess using port $PORT:"
        lsof -i :$PORT | grep LISTEN
    fi
    
    exit 1
else
    echo "✅ Port $PORT is available"
    exit 0
fi