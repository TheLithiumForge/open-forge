import { execFile } from "node:child_process";
import { promisify } from "node:util";

/**
 * Starts the results repository's grading workflow from this repository. The workflow checks the published runs
 * that wait for their checks with the graders of one Open Forge commit, then pushes the grades to its main branch.
 */
export const GradeWorkflow = { file: "grade.yml", refInput: "open-forge-ref" } as const;

export interface CommandResult { readonly code: number; readonly stdout: string; readonly stderr: string }
export type RunCommand = (command: string, args: readonly string[], cwd: string) => Promise<CommandResult>;

export const runCommand: RunCommand = (command, args, cwd) => promisify(execFile)(command, [...args], { cwd, windowsHide: true })
  .then(({ stdout, stderr }) => ({ code: 0, stdout, stderr }), (error: { code?: unknown; stdout?: string; stderr?: string }) =>
    ({ code: typeof error.code === "number" ? error.code : 1, stdout: error.stdout ?? "", stderr: error.stderr ?? String(error) }));

/** owner/name from a GitHub remote URL, in HTTPS or SSH form. */
export function githubRepository(remote: string): string {
  const match = /github\.com[/:]([\w.-]+)\/([\w.-]+?)(?:\.git)?\/?$/.exec(remote.trim());
  if (match === null) throw new Error(`The results repository's remote isn't on GitHub: ${remote.trim()}`);
  return `${match[1]}/${match[2]}`;
}

/**
 * Triggers grading with the graders of `ref`, this repository's HEAD by default. The workflow checks out that commit
 * from GitHub, so it must already be pushed. Returns the results repository and the commit that grades.
 */
export async function triggerGrading(root: string, results: string, ref: string | null, run: RunCommand = runCommand): Promise<{ readonly repository: string; readonly commit: string }> {
  const required = async (command: string, args: readonly string[], cwd: string): Promise<string> => {
    const result = await run(command, args, cwd);
    if (result.code !== 0) throw new Error(`${command} ${args[0] ?? ""} failed: ${result.stderr.trim()}`);
    return result.stdout.trim();
  };
  const repository = githubRepository(await required("git", ["remote", "get-url", "origin"], results));
  const commit = await required("git", ["rev-parse", ref ?? "HEAD"], root);
  if (await required("git", ["branch", "--remotes", "--contains", commit], root) === "") throw new Error(`Push ${commit} first. The grading workflow checks out its graders from GitHub.`);
  await required("gh", ["workflow", "run", GradeWorkflow.file, "--repo", repository, "-f", `${GradeWorkflow.refInput}=${commit}`], root);
  return { repository, commit };
}
