#!/usr/bin/env node

import { finishDevelopmentLink, runDevelopmentLink } from "./launcher.mjs";

const completion = runDevelopmentLink({
  arguments: process.argv.slice(2),
  launcherUrl: import.meta.url,
  platform: process.platform,
  environment: process.env,
  currentDirectory: process.cwd(),
});

const exitCode = finishDevelopmentLink(
  completion,
  (message) => process.stderr.write(message),
  (signal) => process.kill(process.pid, signal),
);

if (exitCode !== undefined) {
  process.exitCode = exitCode;
}
