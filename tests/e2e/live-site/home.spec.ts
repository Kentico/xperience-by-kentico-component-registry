import { expect, test } from "@playwright/test";

test("renders the DancingGoat home page", async ({ page }) => {
  const response = await page.goto("/");
  expect(response?.status()).toBe(200);
  await expect(page).toHaveTitle(/Dancing Goat/);
  await expect(page.getByRole("navigation", { name: "Main" })).toBeVisible();
  await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
});
