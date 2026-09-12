import { readOptions } from "./options.ts";
import { reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { restoreDependencies } from "./restore-dependencies.ts";

try {
  const values = readOptions("restore");
  if (values) {
    restoreDependencies(repositoryRoot, values.offline);
  }
} catch (error) {
  reportFailure(error);
}
