import { ContextClass } from "./classes.ts";
import type { CommandFacts, OutputSource } from "./command-facts.ts";
import { expandGlob, fileClass, fileType, isGlob, projectPath } from "./paths.ts";

/** A share of one output: its class, its file type when it is file content, and its characters. */
export interface Segment {
  readonly class: ContextClass;
  readonly fileType: string | null;
  readonly chars: number;
}

/** The project's files by path with their sizes in bytes, from the start manifest and the saved files. */
export type KnownFiles = ReadonlyMap<string, number>;

// Headers that name the file the following lines came from: head and tail with several files, and open-forge context.
const FileHeader = /^==> (.+) <==$/;
const ContextHeader = /^=== ([^\s(]+)(?: \([^)]*\))? ===$/;
// A search result line that starts with its file, such as src/game/rules.ts:12:const size = 4.
const SearchPrefix = /^(?:\.\/|\/work\/)?([\w.@-][\w./@-]*\.\w+)[:-]/;
const CommandOutput: OutputSource = { class: ContextClass.command, path: null };

/** Each line's characters, counting the line break that ends every line but the last, so they sum to the text's length. */
function lineLengths(text: string): { readonly line: string; readonly chars: number }[] {
  const lines = text.split("\n");
  return lines.map((line, index) => ({ line, chars: line.length + (index < lines.length - 1 ? 1 : 0) }));
}

/** A read file's operands with each glob replaced by the known files it names. A glob that names none keeps its own class. */
export function expandedSources(sources: readonly OutputSource[], files: KnownFiles): OutputSource[] {
  return sources.flatMap((source) => {
    if (source.path === null || !isGlob(source.path)) return [source];
    const matched = expandGlob(source.path, files.keys());
    return matched.length === 0 ? [source] : matched.map((path) => ({ class: fileClass(path), path }));
  });
}

/** Splits a total by weights. Unknown weights share what the known ones leave, and known ones scale down when they exceed it. */
export function allocate(total: number, weights: readonly (number | null)[]): number[] {
  const known = weights.reduce<number>((sum, weight) => sum + (weight ?? 0), 0);
  const unknown = weights.filter((weight) => weight === null).length;
  if (unknown === 0 || known >= total) {
    if (known === 0) return weights.map(() => (weights.length === 0 ? 0 : total / weights.length));
    return weights.map((weight) => ((weight ?? 0) * total) / known);
  }
  return weights.map((weight) => weight ?? (total - known) / unknown);
}

function segment(source: OutputSource, chars: number): Segment {
  return { class: source.class, fileType: source.path === null ? null : fileType(source.path), chars };
}

/** Lines under headers that name their file, with the text before the first header. Null when the output has no such headers. */
function headedSections(output: string, header: RegExp): { readonly preamble: number; readonly files: readonly Segment[] } | null {
  const files: Segment[] = [];
  let preamble = 0, current: string | null = null;
  for (const { line, chars } of lineLengths(output)) {
    const named = header.exec(line)?.[1];
    if (named !== undefined) current = projectPath(named);
    if (current === null) preamble += chars;
    else files.push({ class: fileClass(current), fileType: fileType(current), chars });
  }
  return files.length === 0 ? null : { preamble, files };
}

function searchLines(output: string): Segment[] {
  return lineLengths(output).map(({ line, chars }) => {
    const path = SearchPrefix.exec(line)?.[1];
    return { class: ContextClass.search, fileType: path === undefined ? null : fileType(path), chars };
  });
}

/** Splits one command's output into the files and command outputs it came from. Characters always sum to the output's length. */
export function outputSegments(facts: CommandFacts, output: string, files: KnownFiles): Segment[] {
  if (output.length === 0) return [];
  const sources = expandedSources(facts.sources, files);
  const readsFramework = sources.some((source) => source.class === ContextClass.framework && source.path === null);
  const headed = headedSections(output, readsFramework ? ContextHeader : FileHeader);
  if (headed !== null && (readsFramework || sources.filter((source) => source.path !== null).length > 1)) {
    // Text before the first header is the commands' own output, such as a listing before the files it printed.
    const own = sources.filter((source) => source.path === null);
    const preamble: readonly OutputSource[] = own.length === 0 ? [CommandOutput] : own;
    const shares = allocate(headed.preamble, preamble.map(() => null));
    return [...headed.files, ...preamble.map((source, index) => segment(source, shares[index] ?? 0))].filter((item) => item.chars > 0);
  }
  if (facts.search) return searchLines(output).filter((item) => item.chars > 0);
  if (sources.length === 0) return [{ class: ContextClass.command, fileType: null, chars: output.length }];
  const shares = allocate(output.length, sources.map((source) => (source.path === null ? null : files.get(source.path) ?? null)));
  return sources.map((source, index) => segment(source, shares[index] ?? 0));
}
