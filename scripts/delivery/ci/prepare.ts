import { appendFileSync, realpathSync } from "node:fs";
import { tmpdir } from "node:os";
import { assignmentLines, environmentAssignments, RunnerOs, RunnerVariable, RunnerNodePlatform } from "./environment.ts";
import { CiCommand } from "./command-names.ts";
import { readOptions } from "../options.ts";
import { reportFailure } from "../process.ts";

try {
  if (readOptions(CiCommand.prepare)) {
    const os =
      process.env[RunnerVariable.os] ??
      (process.platform === RunnerNodePlatform.mac ? RunnerOs.mac : process.platform === RunnerNodePlatform.windows ? RunnerOs.windows : RunnerOs.linux);
    const temporary = process.env[RunnerVariable.temporary] ?? tmpdir();
    const physicalTemporary = os === RunnerOs.mac ? realpathSync(temporary) : temporary;
    const lines = assignmentLines(environmentAssignments({ os, temporary, physicalTemporary }));
    const environmentFile = process.env[RunnerVariable.environment];
    if (environmentFile) appendFileSync(environmentFile, lines);
    else process.stdout.write(lines);
  }
} catch (error) {
  reportFailure(error);
}
