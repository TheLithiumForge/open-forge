/**
 * A small shell reader for the commands agents run. It splits a script into pipelines and stages with their words and
 * redirections, which is enough to tell which files a command read or wrote. It never runs or expands anything.
 */

export interface Redirect { readonly operator: string; readonly target: string }
export interface Stage { readonly words: readonly string[]; readonly redirects: readonly Redirect[] }
/** Stages joined by pipes. Pipelines follow each other through ;, &&, || or new lines. */
export type Pipeline = readonly Stage[];

const Separators = ["&&", "||", ";", "\n", "&"] as const;
/** Joins error output to standard output, so it takes no target. */
const CompleteRedirect = "2>&1";
const RedirectOperators = [CompleteRedirect, "&>>", "&>", ">>", ">|", ">&", "<<-", "<<<", "<<", "<", ">"] as const;
const HeredocOperators: readonly string[] = ["<<", "<<-"];
const Shells = new Set(["sh", "bash", "zsh", "dash"]);
const ScriptFlags = new Set(["-c", "-lc", "-ec", "-cl", "-lic"]);

/** The script a shell wrapper runs, such as the quoted part of /bin/sh -lc '...', or the command itself. */
export function unwrapShell(command: string | readonly string[]): string {
  const words = typeof command === "string" ? parseScript(command).flat()[0]?.words ?? [] : command;
  const [shell = "", flag = "", script] = words;
  const name = shell.split("/").at(-1) ?? "";
  if (Shells.has(name) && ScriptFlags.has(flag) && script !== undefined) return script;
  return typeof command === "string" ? command : command.join(" ");
}

class Reader {
  readonly pipelines: Stage[][] = [];
  private stage: { words: string[]; redirects: Redirect[] } = { words: [], redirects: [] };
  private pipeline: Stage[] = [];
  private word: string | null = null;
  private redirect: string | null = null;
  readonly heredocs: string[] = [];

  push(text: string): void { this.word = (this.word ?? "") + text; }
  endWord(): void {
    if (this.word === null) return;
    if (this.redirect !== null) {
      this.stage.redirects.push({ operator: this.redirect, target: this.word });
      if (HeredocOperators.includes(this.redirect)) this.heredocs.push(this.word);
      this.redirect = null;
    } else this.stage.words.push(this.word);
    this.word = null;
  }
  startRedirect(operator: string): void {
    // A file descriptor number such as the 2 of 2>/dev/null belongs to the operator, not to the command's words.
    if (this.word !== null && /^\d+$/.test(this.word)) this.word = null;
    this.endWord();
    if (operator === CompleteRedirect) this.stage.redirects.push({ operator, target: "" });
    else this.redirect = operator;
  }
  endStage(): void {
    this.endWord();
    if (this.stage.words.length > 0 || this.stage.redirects.length > 0) this.pipeline.push(this.stage);
    this.stage = { words: [], redirects: [] };
  }
  endPipeline(): void {
    this.endStage();
    if (this.pipeline.length > 0) this.pipelines.push(this.pipeline);
    this.pipeline = [];
  }
}

function quoted(script: string, start: number, quote: string): { text: string; end: number } {
  let text = "", index = start + 1;
  while (index < script.length && script[index] !== quote) {
    const char = script[index] ?? "";
    if (quote === "\"" && char === "\\" && index + 1 < script.length) { text += script[index + 1]; index += 2; continue; }
    text += char;
    index += 1;
  }
  return { text, end: index + 1 };
}

/** Skips heredoc bodies after a line ends, up to each delimiter line. */
function skipHeredocs(script: string, start: number, delimiters: string[]): number {
  let index = start;
  while (delimiters.length > 0 && index < script.length) {
    const end = script.indexOf("\n", index);
    const line = script.slice(index, end < 0 ? script.length : end);
    index = end < 0 ? script.length : end + 1;
    if (line.trim() === delimiters[0]) delimiters.shift();
  }
  delimiters.length = 0;
  return index;
}

export function parseScript(script: string): Pipeline[] {
  const reader = new Reader();
  let index = 0;
  while (index < script.length) {
    const char = script[index] ?? "";
    const rest = script.slice(index);
    if (char === "'" || char === "\"") { const part = quoted(script, index, char); reader.push(part.text); index = part.end; continue; }
    if (char === "\\" && index + 1 < script.length) { if (script[index + 1] !== "\n") reader.push(script[index + 1] ?? ""); index += 2; continue; }
    if (char === "#" && (index === 0 || /\s/.test(script[index - 1] ?? ""))) { const end = script.indexOf("\n", index); index = end < 0 ? script.length : end; continue; }
    const operator = RedirectOperators.find((item) => rest.startsWith(item));
    if (operator !== undefined) { reader.startRedirect(operator); index += operator.length; continue; }
    if (rest.startsWith("|") && !rest.startsWith("||")) { reader.endStage(); index += 1; continue; }
    const separator = Separators.find((item) => rest.startsWith(item));
    if (separator !== undefined) {
      reader.endPipeline();
      index += separator.length;
      if (separator === "\n" && reader.heredocs.length > 0) index = skipHeredocs(script, index, reader.heredocs);
      continue;
    }
    if (/\s/.test(char)) { reader.endWord(); index += 1; continue; }
    reader.push(char);
    index += 1;
  }
  reader.endPipeline();
  return reader.pipelines;
}
