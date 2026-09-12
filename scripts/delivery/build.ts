import { managedBuild } from "./managed-build.ts";
import { readBuildOptions } from "./options.ts";
import { reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { committedVersion } from "./version.ts";

try {
  readBuildOptions();
  managedBuild(repositoryRoot, committedVersion(repositoryRoot));
} catch (error) {
  reportFailure(error);
}
