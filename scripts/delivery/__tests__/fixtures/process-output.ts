import { ProcessFixture } from "../process-fixture.ts";

process.stdout.write(ProcessFixture.stdout);
process.stderr.write(ProcessFixture.stderr);
process.exitCode = Number(process.env[ProcessFixture.exitVariable] ?? 0);
