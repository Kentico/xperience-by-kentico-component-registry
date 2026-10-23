import { expect, type Page } from "@playwright/test";
import { adminBaseUrl } from "../shared/config";

export class AdminShellPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto(adminBaseUrl);
  }

  async expectSignedIn(): Promise<void> {
    await expect(this.page).toHaveURL(/\/admin/i);
    await expect(this.page.locator('a[aria-label="System"]')).toBeVisible({ timeout: 30_000 });
  }
}
