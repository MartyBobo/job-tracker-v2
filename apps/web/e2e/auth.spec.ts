import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Clear any existing auth state
    await page.context().clearCookies();
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
  });

  test('should successfully login with valid credentials', async ({ page }) => {
    // Navigate to login page
    await page.goto('/auth/login');

    // Check that we're on the login page
    await expect(page.locator('h1')).toContainText('Sign in');

    // Fill in login form
    await page.fill('input[type="email"]', 'test@example.com');
    await page.fill('input[type="password"]', 'Test123!');

    // Click submit button
    await page.click('button[type="submit"]');

    // Wait for navigation to dashboard
    await page.waitForURL('/dashboard', { timeout: 10000 });

    // Verify we're on the dashboard
    expect(page.url()).toContain('/dashboard');

    // Check that tokens are stored
    const tokens = await page.evaluate(() => {
      return {
        hasAccessToken: !!localStorage.getItem('auth-storage'),
        authStorage: localStorage.getItem('auth-storage')
      };
    });

    expect(tokens.hasAccessToken).toBe(true);
    
    // Parse auth storage to verify it contains user data
    if (tokens.authStorage) {
      const authData = JSON.parse(tokens.authStorage);
      expect(authData.state.isAuthenticated).toBe(true);
      expect(authData.state.user).toBeTruthy();
      expect(authData.state.user.email).toBe('test@example.com');
    }
  });

  test('should show error message with invalid credentials', async ({ page }) => {
    await page.goto('/auth/login');

    // Fill in login form with invalid credentials
    await page.fill('input[type="email"]', 'wrong@example.com');
    await page.fill('input[type="password"]', 'wrongpassword');

    // Click submit button
    await page.click('button[type="submit"]');

    // Wait for error toast
    await page.waitForSelector('.sonner-toast-error', { timeout: 5000 });

    // Verify error message is displayed
    const errorToast = page.locator('.sonner-toast-error');
    await expect(errorToast).toBeVisible();
    
    // Should still be on login page
    expect(page.url()).toContain('/auth/login');
  });

  test('should redirect to login when accessing protected route without auth', async ({ page }) => {
    // Try to access dashboard without being logged in
    await page.goto('/dashboard');

    // Should be redirected to login
    await page.waitForURL('/auth/login', { timeout: 5000 });
    expect(page.url()).toContain('/auth/login');
  });

  test('should validate form fields', async ({ page }) => {
    await page.goto('/auth/login');

    // Click submit without filling form
    await page.click('button[type="submit"]');

    // Check for validation errors
    await expect(page.locator('text=Invalid email address')).toBeVisible();
    await expect(page.locator('text=Password must be at least 6 characters')).toBeVisible();

    // Fill invalid email
    await page.fill('input[type="email"]', 'notanemail');
    await page.click('button[type="submit"]');

    // Should still show email validation error
    await expect(page.locator('text=Invalid email address')).toBeVisible();
  });

  test('should successfully logout', async ({ page }) => {
    // First login
    await page.goto('/auth/login');
    await page.fill('input[type="email"]', 'test@example.com');
    await page.fill('input[type="password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await page.waitForURL('/dashboard', { timeout: 10000 });

    // Find and click logout button (adjust selector based on your UI)
    await page.click('button:has-text("Logout")');

    // Should be redirected to login
    await page.waitForURL('/auth/login', { timeout: 5000 });

    // Verify auth state is cleared
    const authCleared = await page.evaluate(() => {
      const authStorage = localStorage.getItem('auth-storage');
      if (!authStorage) return true;
      const data = JSON.parse(authStorage);
      return !data.state.isAuthenticated;
    });

    expect(authCleared).toBe(true);
  });
});