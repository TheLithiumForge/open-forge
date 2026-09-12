import { tool } from "@opencode-ai/plugin";

import { GitObjectInspectionError, GitObjectOperations, inspectGitObjects } from "../../scripts/agent-tooling/review/inspect-git-objects.ts";

export async function executeInspectionRequest(args: unknown, worktree: string) {
  try {
    const result = inspectGitObjects(args, worktree);
    const operation = typeof args === "object" && args !== null && "operation" in args ? args.operation : "rejected";
    return {
      title: `Immutable Git object inspection: ${operation}`,
      output: JSON.stringify({ operation, status: result.status, stdout: result.stdout.toString("utf8"), stderr: result.stderr.toString("utf8") }),
      metadata: { operation, status: result.status, stdoutBytes: result.stdout.length, stderrBytes: result.stderr.length },
    };
  } catch (error) {
    if (error instanceof GitObjectInspectionError) throw new Error(`[inspect-git-objects exit ${error.result.status}] ${error.message}`);
    throw error;
  }
}

export default tool({
  description: "Read bounded immutable Git objects through the repository's fixed local-only inspection engine.",
  args: {
    operation: tool.schema.enum(GitObjectOperations).describe("One fixed Git-object inspection operation."),
    object: tool.schema.string().describe("A complete 40- or 64-character hexadecimal Git object ID."),
    otherObject: tool.schema.string().optional().describe("The second complete object ID required by two-object operations."),
    path: tool.schema.string().optional().describe("The exact repository-relative POSIX blob path required by tree-path."),
  },
  async execute(args, context) {
    return executeInspectionRequest(args, context.worktree);
  },
});
