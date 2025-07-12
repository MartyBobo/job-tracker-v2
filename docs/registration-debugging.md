# Registration Debugging Guide

## Issue
User reported that accounts created through the frontend registration form were not being added to the backend database.

## Investigation Findings
1. The registration system DOES work - `test@test.com` was successfully created earlier
2. The issue appears to be user confusion about credentials or silent validation failures

## Debugging Tools Added

### 1. Debug Users Endpoint
- **URL**: `GET /api/debug/users` (Development only)
- **Purpose**: Lists all registered users in the database
- **Usage**: Check which accounts actually exist

### 2. Enhanced Console Logging
Added detailed logging to both registration and login forms:
- Logs form submission data
- Tracks API request/response
- Shows validation errors
- Displays success/failure details

### 3. Backend Logging
Enhanced registration endpoint logging:
- Logs all registration attempts with timestamps
- Shows validation failures with specific field errors
- Tracks successful registrations with user IDs

### 4. Registration Test Page
- **URL**: http://localhost:3000/test/registration
- **Features**:
  - Visual registration testing tool
  - Shows API responses in real-time
  - Lists all users in database
  - Tests both registration and login
  - Displays API configuration

## How to Debug Registration Issues

1. **Open Browser Console** (F12)
   - Watch for console logs during registration
   - Check for validation errors

2. **Visit Test Page**
   ```
   http://localhost:3000/test/registration
   ```
   - Try registering with different credentials
   - Check the "Current Users" section
   - View detailed API responses

3. **Check Backend Logs**
   ```bash
   docker-compose -f infrastructure/docker-compose.dev.yml logs backend -f
   ```
   - Look for "Registration attempt" messages
   - Check for validation failures

4. **Common Issues**:
   - **"Account already exists"**: Email is already registered
   - **Validation errors**: Password too short, passwords don't match
   - **No console output**: Form validation prevented submission
   - **Network errors**: Backend not running or CORS issues

## Test Credentials
These accounts are available for testing:
- test@test.com / Test123!
- demo@demo.com / Demo123!
- admin@admin.com / Admin123!

## Next Steps
With these debugging tools, you can now:
1. See exactly which accounts exist
2. Track registration attempts in real-time
3. Identify validation or network issues
4. Test registration flow systematically