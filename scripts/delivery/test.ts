import { resetOutput } from "./output.ts";
import { TestAssemblies } from "./layout.ts";
import { managedBuild } from "./managed-build.ts";
import { readBuildOptions } from "./options.ts";
import { reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { runSuites } from "./test-suites.ts";
import { committedVersion } from "./version.ts";

function testManaged(root: string): void {
  const selections = Object.entries(TestAssemblies).map(([name, assembly]) => ({ name, executable: `artifacts/bin/${assembly}/release/${assembly}.dll` }));
  runSuites(root, selections, resetOutput(root, "artifacts/delivery/managed-reports"));
}

try {
  const values = readBuildOptions(false, true);
  managedBuild(repositoryRoot, committedVersion(repositoryRoot), values);
  testManaged(repositoryRoot);
} catch (error) {
  reportFailure(error);
}
