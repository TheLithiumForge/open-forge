import { ProcessFixture } from "../process-fixture.ts";

const [, command] = process.argv.slice(2);
process.stdout.write(`executed:${command}\n`);
if (command === process.env[ProcessFixture.failCommandVariable]) {
  process.stderr.write(ProcessFixture.stderr);
  process.exitCode = ProcessFixture.failure;
}
