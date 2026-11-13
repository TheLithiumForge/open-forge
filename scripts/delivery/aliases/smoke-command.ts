import { readOptions } from "../options.ts";
import { deliveryDirectory, hostRuntime } from "../layout.ts";
import { repositoryRoot } from "../repository.ts";
import { committedVersion } from "../version.ts";
import { reportFailure } from "../process.ts";
import { validateOutput } from "../output.ts";
import { smokeAliases } from "./smoke.ts";

try {
  const options = readOptions("smoke:aliases");
  if (options) {
    const runtime = hostRuntime(options.rid);
    smokeAliases(validateOutput(repositoryRoot, options.from ?? `${deliveryDirectory(runtime)}/packages`), runtime, options.version ?? committedVersion(repositoryRoot));
  }
} catch (error) {
  reportFailure(error);
}
