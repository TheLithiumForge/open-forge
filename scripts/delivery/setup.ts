import { readDeliveryOptions } from "./options.ts";
import { npm, reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { restoreDependencies } from "./restore-dependencies.ts";

try {
  const values = readDeliveryOptions();
  npm(["ci", "--ignore-scripts", "--no-audit", "--no-fund", ...(values.offline ? ["--offline"] : [])], repositoryRoot);
  restoreDependencies(repositoryRoot, values.offline);
} catch (error) {
  reportFailure(error);
}
