import { managedBuild } from "./managed-build.ts";
import { readBuildOptions } from "./options.ts";
import { reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { committedVersion } from "./version.ts";

try {
  const values = readBuildOptions(false, true);
  managedBuild(repositoryRoot, committedVersion(repositoryRoot), values);
} catch (error) {
  reportFailure(error);
}
