#!/bin/bash

# Development data seeding script
# This script creates test users with known credentials for development

echo "🌱 Seeding development data..."

API_URL="${API_URL:-http://localhost:5250/api}"

# Function to register a user
register_user() {
    local email="$1"
    local password="$2"
    local firstName="$3"
    local lastName="$4"
    
    echo "Creating user: $email"
    
    response=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/auth/register" \
        -H "Content-Type: application/json" \
        -d "{
            \"email\": \"$email\",
            \"password\": \"$password\",
            \"firstName\": \"$firstName\",
            \"lastName\": \"$lastName\"
        }")
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" = "201" ]; then
        echo "✅ User $email created successfully"
    elif [ "$http_code" = "409" ]; then
        echo "ℹ️  User $email already exists"
    else
        echo "❌ Failed to create user $email (HTTP $http_code)"
        echo "$body"
    fi
}

# Wait for backend to be ready
echo "Waiting for backend to be ready..."
max_attempts=30
attempt=0
while [ $attempt -lt $max_attempts ]; do
    if curl -s "$API_URL/../health" > /dev/null 2>&1; then
        echo "✅ Backend is ready"
        break
    fi
    echo "⏳ Waiting for backend... (attempt $((attempt + 1))/$max_attempts)"
    sleep 2
    attempt=$((attempt + 1))
done

if [ $attempt -eq $max_attempts ]; then
    echo "❌ Backend did not become ready in time"
    exit 1
fi

# Create test users
echo ""
echo "Creating test users..."
echo "===================="

# Test user 1
register_user "test@test.com" "Test123!" "Test" "User"

# Test user 2  
register_user "demo@demo.com" "Demo123!" "Demo" "User"

# Test user 3
register_user "admin@admin.com" "Admin123!" "Admin" "User"

echo ""
echo "🎉 Development data seeding complete!"
echo ""
echo "Available test accounts:"
echo "========================"
echo "Email: test@test.com    | Password: Test123!"
echo "Email: demo@demo.com    | Password: Demo123!"
echo "Email: admin@admin.com  | Password: Admin123!"
echo ""