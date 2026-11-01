import { spawn } from "node:child_process";
import { mkdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import { copySignInTo, keepRefreshedSignIn } from "./codex-sign-in.ts";
import { localEnvironment } from "../containers/executor.ts";
import { localToolCommand, PinnedTool } from "./pinned-tools.ts";
import { waitForProcess } from "../process.ts";

/** One question to a model that answers with JSON matching a schema, for the simulated user and the judge. */
export interface StructuredRequest {
  readonly choice: ModelChoice;
  /** The folder the model may read, such as a judged project. */
  readonly cwd: string;
  /** A fresh home for this call's tool files. */
  readonly home: string;
  readonly prompt: string;
  readonly schema: object;
  /** Claude Code tool names the model may use. Codex gets a read-only sandbox instead. */
  readonly readTools: readonly string[];
  readonly timeoutMs: number;
}

export interface StructuredResult {
  readonly value: unknown;
  readonly error: string | null;
}

interface ClaudeJsonResult {
  readonly result?: string;
  readonly structured_output?: unknown;
  readonly is_error?: boolean;
}

interface CodexMessageEvent {
  readonly type?: string;
  readonly item?: { readonly type?: string; readonly text?: string };
}

function claudeArguments(request: StructuredRequest): string[] {
  const { model, effort } = request.choice;
  return [
    "-p", "--output-format", "json", "--json-schema", JSON.stringify(request.schema),
    "--strict-mcp-config", "--tools", request.readTools.length === 0 ? "" : request.readTools.join(","),
    ...(model === null ? [] : ["--model", model]),
    ...(effort === null ? [] : ["--effort", effort]),
  ];
}

function codexArguments(request: StructuredRequest, schemaPath: string): string[] {
  const { model, effort } = request.choice;
  return [
    // Codex's read-only sandbox blocks every file read on Windows. The judge only gets a scratch copy of the app.
    "exec", "--json", "--skip-git-repo-check", "--dangerously-bypass-approvals-and-sandbox", "-c", 'web_search="disabled"', "--output-schema", schemaPath,
    ...(model === null ? [] : ["-m", model]),
    ...(effort === null ? [] : ["-c", `model_reasoning_effort="${effort}"`]),
    "-",
  ];
}

/** Model output is untrusted. Callers check the shape of what comes back. */
function parseJson<T>(text: string): T | null {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

export async function askModel(request: StructuredRequest): Promise<StructuredResult> {
  if (request.choice.tool === Tool.mock) return { value: null, error: null };
  mkdirSync(request.home, { recursive: true });
  const isClaude = request.choice.tool === Tool.claude;
  const tool = localToolCommand(isClaude ? PinnedTool.claude : PinnedTool.codex);
  const schemaPath = join(request.home, "schema.json");
  writeFileSync(schemaPath, JSON.stringify(request.schema));
  const signIn = isClaude ? null : copySignInTo(request.home);
  const args = isClaude ? claudeArguments(request) : codexArguments(request, schemaPath);
  const child = spawn(tool.command, [...tool.args, ...args], { cwd: request.cwd, env: localEnvironment(request.home, true), windowsHide: true });
  const result = await waitForProcess(child, { timeoutMs: request.timeoutMs, input: request.prompt });
  keepRefreshedSignIn(signIn);
  if (result.code !== 0 || result.timedOut) return { value: null, error: (result.stderr || result.stdout).slice(-2000) };
  if (isClaude) {
    const output = parseJson<ClaudeJsonResult>(result.stdout) ?? {};
    if (output.is_error === true) return { value: null, error: output.result ?? "Claude Code failed." };
    return { value: output.structured_output ?? parseJson<unknown>(output.result ?? ""), error: null };
  }
  const messages = result.stdout.split("\n").map((line): CodexMessageEvent => parseJson(line) ?? {});
  const last = messages.filter((event) => event.item?.type === "agent_message").at(-1)?.item?.text ?? "";
  return { value: parseJson<unknown>(last), error: null };
}
