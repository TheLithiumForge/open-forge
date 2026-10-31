import { appendFileSync } from "node:fs";
import type { AgentTelemetry } from "../results/results.types.ts";
import { outcomeForError, TurnOutcome, type AgentSession, type SessionContext, type TurnResult } from "./agent-session.ts";
import { codexHomeOf, copySignInTo, keepRefreshedSignIn, type CodexSignInCopy } from "./codex-sign-in.ts";
import { CodexTelemetry, type CodexEvent } from "./codex-telemetry.ts";
import { spawnInRun } from "./executor.ts";
import { stopProcessesUnder } from "./leftovers.ts";
import { PinnedTool } from "./pinned-tools.ts";
import { waitForProcess } from "./process.ts";

/** How long Codex can take to exit by itself after its turn ends. */
const LingerMs = 10_000;

/** Reads the prompt from stdin, so it never lands in a command line. */
const PromptFromStdin = "-";

export function codexArguments(context: SessionContext, threadId: string | null): string[] {
  const { model, effort } = context.choice;
  return [
    "exec", ...(threadId === null ? [] : ["resume", threadId]),
    "--json", "--skip-git-repo-check",
    // The run folder or its container is the sandbox, the same for every setup.
    "--dangerously-bypass-approvals-and-sandbox",
    "-c", 'web_search="disabled"',
    ...(model === null ? [] : ["-m", model]),
    ...(effort === null ? [] : ["-c", `model_reasoning_effort="${effort}"`]),
    PromptFromStdin,
  ];
}

/** Runs one `codex exec` per turn. Later turns resume the same thread with the user's answer. */
export class CodexSession implements AgentSession {
  private readonly telemetry = new CodexTelemetry();
  private readonly context: SessionContext;
  private readonly signIn: CodexSignInCopy | null;

  constructor(context: SessionContext) {
    this.context = context;
    this.signIn = copySignInTo(context.space.home);
  }

  async turn(message: string, timeoutMs: number): Promise<TurnResult> {
    const args = codexArguments(this.context, this.telemetry.threadId);
    const child = spawnInRun({ ...this.context, command: PinnedTool.codex, args, network: true });
    let reply = "";
    let failure: string | null = null;
    let ended = false;
    let linger: ReturnType<typeof setTimeout> | undefined;
    // Codex can linger after its turn while a dev server it started is still running. The timer ends
    // with Codex itself, so it never stops what runs in the folder next, such as the offline build.
    const endLingering = (): void => {
      if (ended) return;
      ended = true;
      linger = setTimeout(() => {
        if (child.exitCode === null) void stopProcessesUnder(this.context.space.root).then(() => child.kill());
      }, LingerMs);
    };
    const result = await waitForProcess(child, {
      timeoutMs,
      input: message,
      onLine: (line) => {
        if (line.trim() === "") return;
        appendFileSync(this.context.eventsPath, `${line}\n`);
        let event: CodexEvent;
        try {
          event = JSON.parse(line);
        } catch {
          return; // Not an event line.
        }
        this.telemetry.record(event);
        if (event.type === "item.completed" && event.item?.type === "agent_message") reply = event.item.text ?? reply;
        if (event.type === "turn.failed" || event.type === "error") failure = event.error?.message ?? event.message ?? "Codex failed.";
        if (event.type === "turn.completed" || event.type === "turn.failed") endLingering();
      },
    });
    clearTimeout(linger);
    if (result.timedOut && !ended) return { outcome: TurnOutcome.timedOut, message: reply, error: "The run hit its time limit." };
    // A turn that completed counts as answered, even when Codex had to be stopped afterwards.
    const error = failure ?? (ended || result.code === 0 ? null : result.stderr.slice(-2000));
    return error === null ? { outcome: TurnOutcome.answered, message: reply, error: null } : { outcome: outcomeForError(error), message: reply, error };
  }

  async finish(): Promise<AgentTelemetry> {
    keepRefreshedSignIn(this.signIn);
    return this.telemetry.summary(codexHomeOf(this.context.space.home), this.context.choice.model);
  }
}
