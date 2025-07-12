# Test Authentication Endpoints

Write-Host "Testing JobTracker Authentication" -ForegroundColor Green

# Test Registration
Write-Host "`n1. Testing Registration..." -ForegroundColor Yellow
$registerBody = @{
    email = "test@example.com"
    password = "TestPassword123!"
    fullName = "Test User"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $registerBody -ContentType "application/json"
    Write-Host "Registration successful!" -ForegroundColor Green
    Write-Host "User ID: $($registerResponse.user.id)"
    Write-Host "Access Token: $($registerResponse.accessToken.Substring(0, 50))..."
    $accessToken = $registerResponse.accessToken
    $refreshToken = $registerResponse.refreshToken
} catch {
    Write-Host "Registration failed: $_" -ForegroundColor Red
}

# Test Login
Write-Host "`n2. Testing Login..." -ForegroundColor Yellow
$loginBody = @{
    email = "test@example.com"
    password = "TestPassword123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "Access Token: $($loginResponse.accessToken.Substring(0, 50))..."
    $accessToken = $loginResponse.accessToken
} catch {
    Write-Host "Login failed: $_" -ForegroundColor Red
}

# Test Protected Endpoint
Write-Host "`n3. Testing Protected Endpoint..." -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $accessToken"
}

try {
    $meResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/users/me" -Method Get -Headers $headers
    Write-Host "Protected endpoint accessed successfully!" -ForegroundColor Green
    Write-Host "User ID: $($meResponse.userId)"
    Write-Host "Email: $($meResponse.email)"
    Write-Host "Name: $($meResponse.name)"
} catch {
    Write-Host "Protected endpoint failed: $_" -ForegroundColor Red
}

# Test Refresh Token
Write-Host "`n4. Testing Refresh Token..." -ForegroundColor Yellow
$refreshBody = @{
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    $refreshResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/refresh" -Method Post -Body $refreshBody -ContentType "application/json"
    Write-Host "Token refresh successful!" -ForegroundColor Green
    Write-Host "New Access Token: $($refreshResponse.accessToken.Substring(0, 50))..."
} catch {
    Write-Host "Token refresh failed: $_" -ForegroundColor Red
}