@echo off
echo Testing Registration API...
echo ==========================

echo.
echo Testing with correct format (firstName/lastName):
curl -X POST http://localhost:5250/api/auth/register ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"test@example.com\",\"password\":\"Test123!\",\"firstName\":\"John\",\"lastName\":\"Doe\"}"

echo.
echo.
pause