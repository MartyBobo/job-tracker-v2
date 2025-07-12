# Debugging Lessons: React Form Submission Handler Not Executing

## Issue
Login form submission not triggering React event handler in Next.js 15 application. Form submits as regular HTML form instead of being intercepted by React Hook Form.

## Root Cause Analysis
The issue was caused by React hydration mismatch where:
1. Server-side rendered form had no event handlers
2. Client-side hydration failed to properly attach React Hook Form's `handleSubmit`
3. Form defaulted to HTML form submission behavior (GET request to same URL)

## Investigation Process
1. **PLANNER**: Identified potential causes (hydration, event binding, form conflicts)
2. **RESEARCHER**: Found form had no attributes but also no event listeners attached
3. **CODE REVIEWER**: Confirmed hydration failure as root cause
4. **IMPLEMENTER**: Added defensive hydration handling

## Solution

### OLD CODE:
```tsx
export default function LoginPage() {
  const router = useRouter()
  const setAuth = useAuthStore((state) => state.setAuth)
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  // ... rest of component

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      {/* form content */}
    </form>
  )
}
```

### NEW CODE:
```tsx
export default function LoginPage() {
  const router = useRouter()
  const setAuth = useAuthStore((state) => state.setAuth)
  const [isLoading, setIsLoading] = useState(false)
  const [isMounted, setIsMounted] = useState(false)

  // Ensure client-side only initialization
  useEffect(() => {
    console.log('LoginPage mounted, setting isMounted to true')
    setIsMounted(true)
  }, [])

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  // ... rest of component

  return (
    <form 
      onSubmit={(e) => {
        e.preventDefault()
        console.log('Form submitted, isMounted:', isMounted)
        if (isMounted) {
          handleSubmit(onSubmit)(e)
        }
      }}
    >
      {/* form content */}
      <Button type="submit" disabled={isLoading || !isMounted}>
        {!isMounted ? 'Loading...' : 'Sign in'}
      </Button>
    </form>
  )
}
```

## Key Changes
1. Added `isMounted` state to track hydration status
2. Explicit `preventDefault()` in form onSubmit handler
3. Conditional execution of React Hook Form handler after mount
4. Visual feedback showing hydration state in button

## Testing Status
⚠️ **Partial Success**: Form no longer submits as HTML form, but appears to navigate to `about:blank` in Puppeteer tests. This may be due to:
- Puppeteer security restrictions with localStorage
- Additional navigation happening after form submission
- Need for further investigation

## Prevention Strategies
1. **Always use defensive hydration checks** in Next.js client components with forms
2. **Add explicit preventDefault** for form submissions
3. **Provide visual feedback** during hydration (loading states)
4. **Test with real browsers** in addition to Puppeteer for hydration issues
5. **Consider using** `suppressHydrationWarning` if mismatch is intentional

## Related Issues
- Next.js 15 hydration behavior changes
- React Hook Form with SSR frameworks
- Puppeteer localStorage access restrictions

## Next Steps
1. Test in real browser to confirm fix works outside Puppeteer
2. Apply same pattern to registration form
3. Consider adding error boundary for better error handling
4. Investigate Puppeteer navigation to `about:blank` issue

---

# Authentication Infrastructure Lessons Learned

## Overview
This section captures additional issues encountered with CORS, port configuration, and static file serving.

---

## Issue: CORS Policy Blocking Frontend Requests

### Issue
```
Access to XMLHttpRequest at 'http://localhost:5250/api/auth/login' from origin 'http://localhost:3001' 
has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

### Reason
The backend CORS configuration only allowed origins `http://localhost:3000` and `http://localhost:3100`, but the frontend was running on port 3001.

### Solution
Updated CORS configuration to include port 3001 and implemented environment-based policies.

### Old Code
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3100")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Usage
app.UseCors("AllowAll");
```

### New Code
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3100")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
    
    options.AddPolicy("ProductionCors", policy =>
    {
        var frontendUrl = builder.Configuration["Frontend:Url"] ?? 
                         Environment.GetEnvironmentVariable("FRONTEND_URL") ?? 
                         "https://jobtracker.app";
        
        policy.WithOrigins(frontendUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Usage
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}
else
{
    app.UseCors("ProductionCors");
}
```

### Prevention Advice
1. Always include all possible development ports in CORS configuration
2. Use environment-based configuration for flexibility
3. Log CORS rejections for easier debugging
4. Consider using a more permissive policy for development

---

## Issue: Frontend Running on Wrong Port (3001 instead of 3000)

### Issue
Frontend was auto-selecting port 3001 when port 3000 appeared to be in use, causing CORS mismatches.

### Reason
Next.js automatically increments the port number when the default port is occupied, and there was no explicit port configuration.

### Solution
Added explicit port configuration and port availability checking.

### Old Code
```json
{
  "scripts": {
    "dev": "next dev",
    "start": "next start"
  }
}
```

### New Code
```json
{
  "scripts": {
    "check:port": "./scripts/check-port.sh 3000",
    "dev": "npm run check:port && next dev -p 3000",
    "dev:force": "next dev -p 3000",
    "start": "next start -p 3000"
  }
}
```

### Port Check Script
```bash
#!/bin/bash
# frontend/scripts/check-port.sh
PORT=${1:-3000}

if lsof -Pi :$PORT -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "❌ Port $PORT is already in use!"
    echo "Process using port $PORT:"
    lsof -i :$PORT | grep LISTEN
    exit 1
else
    echo "✅ Port $PORT is available"
    exit 0
fi
```

### Prevention Advice
1. Always explicitly specify ports in development scripts
2. Implement port conflict detection before starting services
3. Document which ports your application uses
4. Provide a force option for developers who know what they're doing

---

## Issue: Static File Serving Error (icon.svg)

### Issue
```
GET http://localhost:3001/icon.svg 500 (Internal Server Error)
```

### Reason
Investigation revealed the file existed in the correct location (`/frontend/public/icon.svg`). The error was likely due to duplicate files or Next.js serving confusion.

### Solution
No code changes were required. The issue was resolved by verifying:
1. File exists in the public directory
2. Proper Docker volume mounts
3. Correct metadata configuration

### Prevention Advice
1. Keep static assets only in the public directory
2. Avoid duplicating files between app and public directories
3. Test static file serving early in development
4. Ensure Docker volumes include the public directory

---

## General Best Practices

### CORS Configuration
- Use environment-specific policies
- Include all development ports
- Add proper error logging
- Document allowed origins
- Consider security implications of AllowCredentials()

### Port Management
- Explicitly configure ports
- Check port availability before starting
- Use consistent ports across environments
- Document port usage
- Provide override options for developers

### Static Assets
- Keep assets in public directory only
- Verify Docker volume mounts
- Test asset loading early
- Use proper caching headers
- Avoid name conflicts with routes

### Security Considerations
1. **CORS with Credentials**: Be careful when using `AllowCredentials()` with multiple origins
2. **Environment Variables**: Always validate environment variables before using them
3. **Production URLs**: Never hardcode production URLs in source code
4. **Static Files**: Implement proper security headers for static file serving

---

## New Debugging Session: Authentication Flow Issues (Current)

### Issue #1: Browser Cache Serving Old Code

#### Description
The browser was using cached JavaScript files that contained old code, including removed console.log statements and API calls to `/api/auth/me`.

#### Root Cause
Next.js hot module replacement (HMR) wasn't properly updating the browser cache after code changes.

#### Exact Error
```
❌ API Error: GET /auth/me - Status: 401
Error: No refresh token
```

#### Solution
1. Delete `.next` directory to force rebuild
2. Clear browser cache
3. Restart development server

#### Fix Applied
```bash
cd /mnt/c/Code/Job_app_resume_design/frontend && rm -rf .next
```

#### Prevention
- Always clear `.next` directory when experiencing strange caching issues
- Use hard refresh in browser (Ctrl+Shift+R)
- Consider adding cache-busting headers in development

---

### Issue #2: Unnecessary API Call After Login

#### Description
The login flow was attempting to call `/api/auth/me` after successful login, causing a 401 error.

#### Root Cause
Original implementation assumed login endpoint only returned tokens, requiring a separate call to get user data.

#### Old Code
```typescript
const response = await authApi.login(data)
const { accessToken, refreshToken } = response.data

// Get user info
const userResponse = await authApi.me()
const user = userResponse.data
```

#### New Code
```typescript
const response = await authApi.login(data)
const { accessToken, refreshToken, user } = response.data

// The login response already includes user data, no need for separate call
setAuth({ user, accessToken, refreshToken })
```

#### Prevention
- Always check API response structure before making additional calls
- Document API contracts clearly
- Use TypeScript interfaces for API responses

---

### Issue #3: Axios Interceptor Token Refresh Logic

#### Description
The axios interceptor was trying to call `/api/auth/me` during token refresh and not handling the login page scenario.

#### Root Cause
1. Interceptor assumed refresh endpoint didn't return user data
2. No check for current page to prevent refresh attempts on login page

#### Old Code
```typescript
// Get user data and update store
const userResponse = await axios.get(`${API_URL}/auth/me`, {
  headers: { Authorization: `Bearer ${accessToken}` }
})

useAuthStore.getState().setAuth({
  user: userResponse.data,
  accessToken,
  refreshToken: newRefreshToken
})
```

#### New Code
```typescript
// Don't try to refresh if we're already on the login page
if (window.location.pathname === '/auth/login') {
  return Promise.reject(error)
}

const { accessToken, refreshToken: newRefreshToken, user } = response.data

// The refresh endpoint should return user data too
useAuthStore.getState().setAuth({
  user,
  accessToken,
  refreshToken: newRefreshToken
})
```

#### Prevention
- Always handle edge cases in interceptors
- Check current route before authentication actions
- Ensure all auth endpoints return consistent data structures

---

### Testing Infrastructure Added

#### Playwright E2E Tests
Created comprehensive E2E tests for authentication flow:
- Login success flow
- Login failure handling
- Protected route access
- Form validation
- Logout flow

#### Benefits
- Catches integration issues early
- Verifies user flows work end-to-end
- Provides confidence in deployments

---

### Key Takeaways from Current Session

1. **Cache Issues**: Always consider caching when debugging frontend issues
2. **API Contracts**: Document and verify API response structures
3. **Error Handling**: Handle edge cases in authentication flows
4. **Testing**: E2E tests catch integration issues unit tests miss
5. **Agent-Based Debugging**: Using different "agent" roles helps systematic problem solving

### Recommended Next Steps

1. Restart the frontend server to ensure new code is loaded
2. Test authentication manually in browser
3. Run Playwright E2E tests to verify fixes
4. Add unit tests for auth store and components
5. Monitor for any remaining console errors