import type { ChildProcessWithoutNullStreams } from "node:child_process";
import { appendFileSync } from "node:fs";
import { createInterface } from "node:readline";
import type { AgentTelemetry } from "../results/results.types.ts";
import { outcomeForError, TurnOutcome, type AgentSession, type SessionContext, type TurnResult } from "./agent-session.ts";
import { ClaudeTelemetry, type ClaudeEvent } from "./claude-telemetry.ts";
import { spawnInRun } from "./executor.ts";
import { PinnedTool } from "./pinned-tools.ts";

const ExitWaitMs = 30_000;

export function claudeArguments(context: SessionContext): string[] {
  const { model, effort } = context.choice;
  return [
    "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose",
    "--permission-mode", "bypassPermissions",
    "--strict-mcp-config",
    "--disallowedTools", "WebSearch", "WebFetch",
    ...(model === null ? [] : ["--model", model]),
    ...(effort === null ? [] : ["--effort", effort]),
  ];
}

/**
 * Keeps one Claude Code process for the whole conversation. Answers go in on stdin as stream-json
 * user messages, which avoids resuming by session ID.
 */
export class ClaudeSession implements AgentSession {
  private readonly telemetry = new ClaudeTelemetry();
  private readonly context: SessionContext;
  private child: ChildProcessWithoutNullStreams | null = null;
  private pending: ((result: TurnResult) => void) | null = null;
  private stderr = "";

  constructor(context: SessionContext) {
    this.context = context;
  }

  turn(message: string, timeoutMs: number): Promise<TurnResult> {
    const child = this.child ?? this.start();
    return new Promise((resolve) => {
      const timer = setTimeout(() => this.settle({ outcome: TurnOutcome.timedOut, message: "", error: "The run hit its time limit." }), timeoutMs);
      this.pending = (result) => {
        clearTimeout(timer);
        resolve(result);
      };
      const userMessage = { type: "user", message: { role: "user", content: [{ type: "text", text: message }] } };
      child.stdin.write(`${JSON.stringify(userMessage)}\n`);
    });
  }

  async finish(): Promise<AgentTelemetry> {
    const child = this.child;
    if (child !== null && child.exitCode === null) {
      const exited = new Promise<void>((resolve) => child.once("exit", () => resolve()));
      child.stdin.end();
      const timer = setTimeout(() => child.kill(), ExitWaitMs);
      await exited;
      clearTimeout(timer);
    }
    return this.telemetry.summary();
  }

  private start(): ChildProcessWithoutNullStreams {
    const child = spawnInRun({ ...this.context, command: PinnedTool.claude, args: claudeArguments(this.context), network: true });
    this.child = child;
    child.stderr.on("data", (chunk: Buffer) => (this.stderr += chunk.toString()));
    createInterface({ input: child.stdout }).on("line", (line) => this.onLine(line));
    child.on("exit", () => this.settle({ outcome: outcomeForError(this.stderr), message: "", error: this.stderr.slice(-2000) || "Claude Code exited." }));
    return child;
  }

  private onLine(line: string): void {
    if (line.trim() === "") return;
    appendFileSync(this.context.eventsPath, `${line}\n`);
    let event: ClaudeEvent;
    try {
      event = JSON.parse(line);
    } catch {
      return; // Not an event line.
    }
    this.telemetry.record(event);
    if (event.type !== "result") return;
    const text = event.result ?? "";
    this.settle(event.is_error === true ? { outcome: outcomeForError(text), message: text, error: text } : { outcome: TurnOutcome.answered, message: text, error: null });
  }

  private settle(result: TurnResult): void {
    const pending = this.pending;
    this.pending = null;
    pending?.(result);
  }
}
