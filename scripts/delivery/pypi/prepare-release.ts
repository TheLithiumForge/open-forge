import { readOptions } from "../options.ts";
import { repositoryRoot } from "../repository.ts";
import { sourceIdentity } from "../source.ts";
import { committedVersion } from "../version.ts";
import { reportFailure } from "../process.ts";
import { ReleaseDirectory } from "../release/release-plan.ts";
import { preparePyPI } from "./release-wheels.ts";

try {
  const options = readOptions("release:pypi");
  if (options) preparePyPI(repositoryRoot, options.from ?? ReleaseDirectory, sourceIdentity(repositoryRoot), committedVersion(repositoryRoot));
} catch (error) {
  reportFailure(error);
}
