import { mkdirSync, mkdtempSync } from "node:fs";
import { join } from "node:path";
import { TestAssemblies } from "./layout.ts";
import { managedBuild } from "./managed-build.ts";
import { readBuildOptions } from "./options.ts";
import { reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { runSuites } from "./test-suites.ts";
import { committedVersion } from "./version.ts";

function testManaged(root: string): void {
  const directory = join(root, "artifacts/delivery");
  mkdirSync(directory, { recursive: true });
  const selections = Object.entries(TestAssemblies).map(([name, assembly]) => ({ name, executable: `artifacts/bin/${assembly}/release/${assembly}.dll` }));
  runSuites(root, selections, mkdtempSync(join(directory, "managed-reports-")));
}

try {
  readBuildOptions();
  managedBuild(repositoryRoot, committedVersion(repositoryRoot));
  testManaged(repositoryRoot);
} catch (error) {
  reportFailure(error);
}
