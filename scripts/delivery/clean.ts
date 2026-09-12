import { readdirSync, existsSync } from "node:fs";
import { join } from "node:path";
import { parseArgs } from "node:util";
import { removeOutputs, validateOutput } from "./output.ts";
import { PlatformPackages } from "./package-model.ts";
import { repositoryRoot } from "./repository.ts";
import { reportFailure } from "./process.ts";

export function cleanOutputs(root: string): void {
  const delivery = "artifacts/delivery";
  validateOutput(root, delivery);
  const platforms = Object.keys(PlatformPackages);
  const owned = existsSync(join(root, delivery))
    ? readdirSync(join(root, delivery)).filter(
        (name) =>
          name === "wrapper" ||
          name === "managed-reports" ||
          name.startsWith("managed-reports-") ||
          platforms.some((rid) => name === rid || new RegExp(`^${rid}-[0-9]+$`, "u").test(name)),
      )
    : [];
  removeOutputs(root, ["artifacts/bin", "artifacts/obj", "artifacts/publish", ...owned.map((name) => `${delivery}/${name}`)]);
}

if (import.meta.main) {
  try {
    parseArgs({ options: {} });
    cleanOutputs(repositoryRoot);
    process.stdout.write("Cleaned generated .NET and delivery outputs; dependencies, offline feed and local npm links retained.\n");
  } catch (error) {
    reportFailure(error);
  }
}
