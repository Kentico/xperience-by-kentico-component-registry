import { expect, type Locator, type Page } from "@playwright/test";
import { adminBaseUrl, adminPassword, adminUsername } from "../shared/config";

// Xperience owns the sign-in markup; keep version-sensitive selectors here.
const usernameSelectors = [
  'input[name="username"]',
  'input[name="userName"]',
  'input[type="email"]',
  '[data-testid="username"] input',
];
const passwordSelectors = [
  'input[name="password"]',
  'input[type="password"]',
  '[data-testid="password"] input',
];
const submitSelectors = [
  'button[type="submit"]',
  'button:has-text("Sign in")',
  'button:has-text("Log in")',
  '[data-testid="signInButton"]',
];

export async function signInToAdmin(page: Page): Promise<void> {
  if (!adminPassword) {
    throw new Error("Set XPERIENCE_ADMIN_PASSWORD to the local test administrator password.");
  }

  await page.goto(adminBaseUrl);
  await (await waitForAnyVisible(page, usernameSelectors)).fill(adminUsername);
  await (await waitForAnyVisible(page, passwordSelectors)).fill(adminPassword);
  await (await waitForAnyVisible(page, submitSelectors)).click();
  await expect(page.locator('a[aria-label="System"]')).toBeVisible({ timeout: 60_000 });
}

async function waitForAnyVisible(page: Page, selectors: string[]): Promise<Locator> {
  const startedAt = Date.now();
  while (Date.now() - startedAt < 30_000) {
    for (const selector of selectors) {
      const locator = page.locator(selector).first();
      if (await locator.isVisible()) {
        return locator;
      }
    }
    await page.waitForTimeout(150);
  }
  throw new Error(`No visible sign-in element for [${selectors.join(", ")}].`);
}
