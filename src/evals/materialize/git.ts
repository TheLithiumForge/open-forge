import type { CommandRunner } from "./runner.ts";
import { seedRepository } from "./seed-repository.ts";

export const GitCommand = "git";
export const GitRequests = {
  init: ["init", "--initial-branch=main", "--template="],
  add: ["add", "--all"],
  commit: ["-c", "user.name=Open Forge Evals", "-c", "user.email=evals@example.invalid", "-c", "commit.gpgsign=false", "-c", "core.hooksPath=.git/no-hooks", "commit", "--no-verify", "-m", "Initial project"],
} as const;

export async function initialCommit(destination: string, run?: CommandRunner): Promise<void> {
  if (run === undefined) return seedRepository(destination);
  for (const args of [GitRequests.init, GitRequests.add, GitRequests.commit]) {
    const result = await run(GitCommand, args, destination);
    if (result.code !== 0) throw new Error(`Git initial commit stopped at ${args[0]}.`);
  }
}
