import { readBuildOptions } from "./options.ts";
import { hostRuntime } from "./layout.ts";
import { npm, reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";

try {
  const values = readBuildOptions(true, true);
  const rid = hostRuntime(values.rid);
  const buildFlags = ["--rid", rid, ...(values.sha ? ["--sha"] : []), ...(values.offline ? ["--offline"] : []), ...(values["no-restore"] ? ["--no-restore"] : [])];
  npm(["run", "build:native", "--", ...buildFlags], repositoryRoot);
  npm(["run", "test:built", "--", "--rid", rid], repositoryRoot);
  npm(["run", "pack", "--", "--rid", rid], repositoryRoot);
} catch (error) {
  reportFailure(error);
}
