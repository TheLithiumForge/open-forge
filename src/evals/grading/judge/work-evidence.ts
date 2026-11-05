import { existsSync } from "node:fs";
import { readFile } from "node:fs/promises";
import { posix } from "node:path";
import type { Suite } from "../../benchmarks/suite.types.ts";
import type { FileBytes } from "../../materialize/files.ts";
import { ForgePreset } from "../../materialize/forge.ts";
import { placedFiles, type Placement } from "../../materialize/placement.ts";
import { splitMarkdown } from "../../materialize/frontmatter.ts";
import { AcceptanceLine, ProperAcceptanceLine, SetupFiles } from "../../materialize/setup.ts";

export const WorkEvidenceFiles = { folder: "work", documents: "documents.json", statePages: "state-pages.json", tests: "tests.json" } as const;
/** A later judge has to read these, so a large rewrite is cut rather than crowding out the source. */
const LineLimits = { perPage: 400, perFile: 400, total: 4000 } as const;
const TestFile = /(^|\/)(tests?|e2e|__tests__)\/|\.(test|spec|e2e)\.[^/]+$/;
const FrameworkFile = /^\.agents\/([^/]+$|skills\/|templates\/)|(^|\/)_[^/]+\.md$|^\.open-forge-state\//;
const WrittenPlace = /^(docs\/|\.agents\/)|^[^/]+\.md$/;
const RootInstructions = new Set(["AGENTS.md", "CLAUDE.md"]);
const RuleLink = /The binding rule is in \[its rule file\]\([^)]*\)\.?/g;
const MarkdownLink = /\[([^\]]+)\]\([^)]*\)/g;
const SetupPath = /(?:\.agents|docs)\/[\w./-]*/g;
const ProjectDocument = "a project document";
/** What a setup adds to the project's README, which says nothing about the run. */
const SetupLines = [AcceptanceLine, ProperAcceptanceLine];
const ProjectReadme = "project-README.md";
/** A placed pack's README is two pages. Judges see the one name a plain project would show. */
const PackReadme = "README.md";

export interface StaleStatements { readonly document: string; readonly statements: readonly string[]; readonly current: string }
/** Whether a page that described the game before the work now describes it after. */
export interface StalePage { readonly document: string; readonly statements: number; readonly remaining: number; readonly mentionsWork: boolean; readonly current: boolean }
export interface WorkEvidence {
  /** Files for the judge, each a JSON array with one line per entry, so a line can be cited. */
  readonly files: readonly { readonly name: string; readonly lines: readonly string[] }[];
  readonly staleDocs: readonly StalePage[];
}

/** A page as a judge may read it: no metadata, links reduced to their words, and nothing that says where the file sits. */
function neutral(bytes: Uint8Array): string {
  return splitMarkdown(bytes).body.toString("utf8").replaceAll("\r\n", "\n").replace(RuleLink, "").replace(MarkdownLink, "$1").replace(SetupPath, ProjectDocument)
    .split("\n").filter((line) => !SetupLines.includes(line) && !line.includes(`\`${SetupFiles.docs}/\``)).join("\n");
}
const name = (path: string): string => path === SetupFiles.readme ? ProjectReadme : posix.basename(path);
const written = (path: string): boolean => WrittenPlace.test(path) && path.endsWith(".md") && !FrameworkFile.test(path) && !RootInstructions.has(path);

/** The pages a project started with, by file name. A placed project's pages hold links to their rules, and a pack's don't. */
function startingPages(seed: readonly FileBytes[], documents: readonly FileBytes[], placement: Placement | undefined, rung: ForgePreset): Map<string, string> {
  const files = placement === undefined ? documents.map((file) => ({ ...file, path: `${SetupFiles.docs}/${file.path}` })) : placedFiles(placement, rung, documents).filter((file) => written(file.path));
  return new Map([...seed.filter((file) => file.path === SetupFiles.readme), ...files].map((file) => [name(file.path), neutral(file.bytes)]));
}

function changedLines(before: string, after: string): { readonly removed: string[]; readonly added: string[] } {
  const old = before.split("\n"), now = after.split("\n"), had = new Set(old), has = new Set(now);
  return { removed: old.filter((line) => line.trim() !== "" && !has.has(line)), added: now.filter((line) => line.trim() !== "" && !had.has(line)) };
}

export function stalePages(stale: readonly StaleStatements[], pages: ReadonlyMap<string, string>): StalePage[] {
  return stale.map(({ document, statements, current }) => {
    const text = pages.get(document) ?? "";
    const remaining = statements.filter((statement) => text.includes(statement.replace(MarkdownLink, "$1"))).length;
    const mentionsWork = new RegExp(current, "i").test(text.split("\n").filter((line) => !statements.some((statement) => line.includes(statement))).join("\n"));
    return { document, statements: statements.length, remaining, mentionsWork, current: remaining === 0 && mentionsWork };
  });
}

/**
 * What a run left beside its code: documents it changed or added, the pages that describe the game's current state, and its own
 * tests. Judges don't otherwise see Markdown or tests, because those could say which setup made the run. Here names lose their
 * folders and pages lose their metadata, so the words are all that's left.
 */
export function workEvidence(files: readonly FileBytes[], seed: readonly FileBytes[], documents: readonly FileBytes[], stale: readonly StaleStatements[], placement: Placement | undefined, rung: ForgePreset): WorkEvidence {
  const starting = startingPages(seed, documents, placement, rung);
  const shown = (page: string): string => placement !== undefined && (page === posix.basename(placement.readme.overview.route) || page === posix.basename(placement.readme.list[rung]?.route ?? "")) ? PackReadme : page;
  const seeded = new Map(seed.map((file) => [file.path, file.bytes]));
  const finalPages = new Map(files.filter((file) => written(file.path)).map((file) => [name(file.path), neutral(file.bytes)]));
  const lines: string[] = [];
  for (const [page, text] of [...finalPages].sort(([a], [b]) => a.localeCompare(b))) {
    const before = starting.get(page);
    if (before === undefined) {
      const added = text.split("\n").filter((line) => line.trim() !== "");
      lines.push(`=== ${shown(page)} (new)`, ...added.slice(0, LineLimits.perPage).map((line) => `+ ${line}`));
    } else if (before !== text) {
      const { removed, added } = changedLines(before, text);
      lines.push(`=== ${shown(page)} (changed)`, ...removed.slice(0, LineLimits.perPage).map((line) => `- ${line}`), ...added.slice(0, LineLimits.perPage).map((line) => `+ ${line}`));
    }
  }
  for (const page of starting.keys()) if (!finalPages.has(page)) lines.push(`=== ${shown(page)} (removed)`);
  const statePages = stale.flatMap(({ document }) => {
    const text = finalPages.get(document);
    return text === undefined ? [`=== ${document} (missing)`] : [`=== ${document}`, ...text.split("\n").filter((line) => line.trim() !== "")];
  });
  const testLines = files.filter((file) => TestFile.test(file.path) && !(seeded.get(file.path) !== undefined && Buffer.compare(Buffer.from(seeded.get(file.path) ?? []), Buffer.from(file.bytes)) === 0) && !file.path.endsWith(".md"))
    .sort((a, b) => a.path.localeCompare(b.path))
    .flatMap((file) => [`=== ${file.path} (${seeded.has(file.path) ? "changed" : "new"})`, ...Buffer.from(file.bytes).toString("utf8").replaceAll("\r\n", "\n").split("\n").slice(0, LineLimits.perFile)]);
  return {
    files: [{ name: WorkEvidenceFiles.documents, lines: lines.slice(0, LineLimits.total) }, { name: WorkEvidenceFiles.statePages, lines: statePages.slice(0, LineLimits.total) }, { name: WorkEvidenceFiles.tests, lines: testLines.slice(0, LineLimits.total) }],
    staleDocs: stalePages(stale, finalPages),
  };
}

/** A file a judge can cite by line. */
export const evidenceFileText = (lines: readonly string[]): string => `[\n${lines.map((line) => JSON.stringify(line)).join(",\n")}\n]\n`;

export async function caseStaleStatements(suite: Pick<Suite, "brownfield">): Promise<readonly StaleStatements[]> {
  const item = suite.brownfield?.case;
  if (item === undefined || !existsSync(`${item.root}/rules.json`)) return [];
  const value = JSON.parse(await readFile(`${item.root}/rules.json`, "utf8")) as { stale?: readonly StaleStatements[] };
  return value.stale ?? [];
}
