import { Tool } from "../config/choices.ts";

/**
 * What one run spent and read, from its saved tool events. It shows where tokens go: how much of the context the
 * agent read from framework files, spec documents or code, and how much it spent before its first edit.
 */
export const ReadKind = { framework: "framework", docs: "docs", code: "code" } as const;
export type ReadKind = (typeof ReadKind)[keyof typeof ReadKind];

export interface TokenSplit { readonly newInput: number; readonly cachedInput: number; readonly cacheWrite: number; readonly output: number }
export interface ReadCount { readonly actions: number; readonly characters: number }
export interface EventUsage {
  readonly steps: number;
  /** Tool actions before the first edit. Null when the run never edited a file. */
  readonly stepsBeforeEdit: number | null;
  readonly reads: Readonly<Record<ReadKind, ReadCount>>;
  /**
   * Input tokens before and after the first edit. Output stays at zero here, because the stream only reports it per
   * turn. Null for a tool that reports one total per turn, as Codex does.
   */
  readonly phases: { readonly analysis: TokenSplit; readonly implementation: TokenSplit } | null;
  /** Output tokens from each turn's result, thinking included. Null when the tool's events don't report it. */
  readonly output: number | null;
}

// Framework files are what Open Forge installs or a tool loads by name. The spec lives in docs/.
const Framework = /(^|[\s/'"])(\.agents(\/|\b)|AGENTS\.md|CLAUDE\.md|open-forge\s)/;
const Docs = /(^|[\s/'"])docs\//;
const ClaudeEdits = new Set(["Write", "Edit", "MultiEdit", "NotebookEdit"]);
// Codex edits through apply_patch or shell redirection into a file.
const CodexEdit = /apply_patch|\bcat\s*>|\btee\s|>\s*[\w./-]+\.(tsx?|jsx?|css|html|json|md)\b/;
// Commands that only change or check files aren't reads, even when they name a path.
const Reading = /\b(cat|sed|head|tail|less|rg|grep|ls|find|nl|wc|jq|node\s+-e)\b/;

const empty = (): Record<ReadKind, { actions: number; characters: number }> => ({ framework: { actions: 0, characters: 0 }, docs: { actions: 0, characters: 0 }, code: { actions: 0, characters: 0 } });
const zero = (): TokenSplit => ({ newInput: 0, cachedInput: 0, cacheWrite: 0, output: 0 });

export function readKind(target: string): ReadKind {
  return Framework.test(target) ? ReadKind.framework : Docs.test(target) ? ReadKind.docs : ReadKind.code;
}

function record(value: unknown): Record<string, unknown> | null {
  return value !== null && typeof value === "object" && !Array.isArray(value) ? value as Record<string, unknown> : null;
}
const text = (value: unknown): string => typeof value === "string" ? value : Array.isArray(value) ? value.map((part) => text(record(part)?.["text"] ?? part)).join("") : "";
const count = (value: unknown): number => typeof value === "number" && Number.isFinite(value) ? value : 0;

function claudeUsage(events: readonly unknown[]): EventUsage {
  const reads = empty(), seen = new Set<string>(), targets = new Map<string, string>();
  const analysis = zero(), implementation = zero();
  let steps = 0, stepsBeforeEdit: number | null = null, output: number | null = null;
  for (const event of events.map(record)) {
    const message = record(event?.["message"]);
    if (event?.["type"] === "result" && record(event["usage"]) !== null) output = (output ?? 0) + count(record(event["usage"])?.["output_tokens"]);
    if (event?.["type"] === "assistant" && message !== null) {
      for (const block of (Array.isArray(message["content"]) ? message["content"] : []).map(record)) {
        if (block?.["type"] !== "tool_use") continue;
        steps += 1;
        const name = String(block["name"]), input = record(block["input"]);
        if (ClaudeEdits.has(name)) { stepsBeforeEdit ??= steps - 1; continue; }
        const target = name === "Bash" ? String(input?.["command"] ?? "") : String(input?.["file_path"] ?? input?.["path"] ?? input?.["pattern"] ?? "");
        if (name !== "Bash" || Reading.test(target)) targets.set(String(block["id"]), target);
      }
      // Each content block repeats its message's usage, so a message counts once.
      const id = String(message["id"] ?? ""), usage = record(message["usage"]);
      if (usage !== null && !seen.has(id)) {
        seen.add(id);
        const phase = stepsBeforeEdit === null ? analysis : implementation;
        Object.assign(phase, { newInput: phase.newInput + count(usage["input_tokens"]), cachedInput: phase.cachedInput + count(usage["cache_read_input_tokens"]),
          cacheWrite: phase.cacheWrite + count(usage["cache_creation_input_tokens"]) });
      }
    }
    if (event?.["type"] === "user" && message !== null) {
      for (const block of (Array.isArray(message["content"]) ? message["content"] : []).map(record)) {
        const target = targets.get(String(block?.["tool_use_id"]));
        if (block?.["type"] !== "tool_result" || target === undefined) continue;
        const kind = reads[readKind(target)];
        kind.actions += 1;
        kind.characters += text(block["content"]).length;
      }
    }
  }
  return { steps, stepsBeforeEdit, reads, phases: { analysis, implementation }, output };
}

function codexUsage(events: readonly unknown[]): EventUsage {
  const reads = empty();
  let steps = 0, stepsBeforeEdit: number | null = null;
  for (const event of events.map(record)) {
    const item = record(event?.["item"]);
    if (event?.["type"] !== "item.completed" || item?.["type"] !== "command_execution") continue;
    steps += 1;
    const command = String(item["command"] ?? "");
    if (CodexEdit.test(command)) { stepsBeforeEdit ??= steps - 1; continue; }
    if (!Reading.test(command)) continue;
    const kind = reads[readKind(command)];
    kind.actions += 1;
    kind.characters += text(item["aggregated_output"]).length;
  }
  return { steps, stepsBeforeEdit, reads, phases: null, output: null };
}

/** Reads one run's saved events. Unparsable lines are skipped, since a stopped tool can leave a partial line. */
export function eventUsage(tool: string, lines: string): EventUsage {
  const events = lines.split("\n").flatMap((line) => { try { return line.trim() === "" ? [] : [JSON.parse(line) as unknown]; } catch { return []; } });
  return tool === Tool.claude ? claudeUsage(events) : codexUsage(events);
}
