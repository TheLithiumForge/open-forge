import { managedBuild } from "./managed-build.ts";
import { readOptions } from "./options.ts";
import { reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { committedVersion } from "./version.ts";

try {
  const values = readOptions("build");
  if (values) {
    managedBuild(repositoryRoot, committedVersion(repositoryRoot), values);
  }
} catch (error) {
  reportFailure(error);
}
