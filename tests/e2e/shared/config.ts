import fs from "node:fs";
import path from "node:path";

const dancingGoatPath = path.resolve(__dirname, "../../../examples/DancingGoat");
const launchSettings = JSON.parse(
  fs.readFileSync(path.join(dancingGoatPath, "Properties/launchSettings.json"), "utf8"),
);

export const appBaseUrl = (
  process.env.DANCING_GOAT_BASE_URL ?? launchSettings.profiles.DancingGoat.applicationUrl.split(";")[0]
).replace(/\/+$/, "");
export const adminBaseUrl = process.env.ADMIN_BASE_URL ?? `${appBaseUrl}/admin`;
export const adminUsername = process.env.XPERIENCE_ADMIN_USERNAME ?? "administrator";
export const adminPassword = process.env.XPERIENCE_ADMIN_PASSWORD;
export const adminStorageStatePath = path.resolve(__dirname, "../playwright/.auth/admin.json");
export const webServerCommand =
  process.env.E2E_WEB_SERVER_COMMAND ?? `dotnet run --project "${dancingGoatPath}"`;
export const webServerTimeoutMs = Number.parseInt(
  process.env.E2E_WEB_SERVER_TIMEOUT_MS ?? "180000",
  10,
);
