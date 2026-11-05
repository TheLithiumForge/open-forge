import { readFile } from "node:fs/promises";
import { posix } from "node:path";
import type { FileBytes } from "./files.ts";
import { addMetadata, isRecord, splitMarkdown, validateMetadata, type Metadata } from "./frontmatter.ts";
import type { ForgePreset } from "./forge.ts";
import { rewriteLinks } from "./scopes.ts";

export const PlacementVersion = 2;
/** Where placed files live inside a project. A route in a placement is relative to it. */
export const PlacementRoot = ".agents";
const InstructionsHeading = "## Instructions";

/** One document's place on each install rung, with the tags it carries there. */
export interface PlacedDocument {
  readonly file: string;
  readonly description: string;
  readonly routes: Readonly<Record<string, string>>;
  readonly tags: Readonly<Record<string, readonly string[]>>;
}
/** A sentence that binds new work. It leaves its page word for word for a rule file, and the page links to it instead. */
export interface MovedSentence { readonly document: string; readonly sentence: string; readonly directive: string }
export interface AuthoredFile { readonly title: string; readonly description: string; readonly tags: readonly string[]; readonly applyTo?: readonly string[] }
export interface Catalogue extends AuthoredFile { readonly question: string; readonly summary: string; readonly axioms: readonly string[] }
export interface PlacedPart { readonly description: string; readonly route: string; readonly tags: readonly string[] }
/** The pack's README is two sources: an overview that loads up front, and a list of documents to open on demand. */
export interface PlacedReadme {
  readonly file: string;
  readonly marker: string;
  readonly overview: PlacedPart;
  readonly list: Readonly<Record<string, PlacedPart>>;
  /** Links inside the list that point at a heading in the overview. */
  readonly fragments: readonly string[];
}
export interface Placement {
  readonly schemaVersion: number;
  /** A file the case's work plausibly touches, which qualification asks the CLI about. */
  readonly probe: string;
  readonly readme: PlacedReadme;
  readonly documents: readonly PlacedDocument[];
  readonly sentences: readonly MovedSentence[];
  readonly directiveFiles: Readonly<Record<string, AuthoredFile>>;
  readonly catalogues: Readonly<Record<string, Readonly<Record<string, Catalogue>>>>;
  /** Categories the rung's preset doesn't ship, which the setup adds to the loader. */
  readonly customRoots: Readonly<Record<string, readonly string[]>>;
}

export async function loadPlacement(path: string): Promise<Placement> {
  const value: unknown = JSON.parse(await readFile(path, "utf8"));
  if (!isRecord(value) || value["schemaVersion"] !== PlacementVersion) throw new Error("A placement needs its version.");
  return value as unknown as Placement;
}

const inRoot = (route: string): string => `${PlacementRoot}/${route}`;
const relativeTo = (from: string, to: string): string => posix.relative(posix.dirname(from), to);
const titleOf = (bytes: Uint8Array): string => Buffer.from(bytes).toString("utf8").split("\n", 1)[0]?.replace(/^#+\s*/, "").trim() ?? "";

function routesFor(placement: Placement, rung: ForgePreset): Map<string, string> {
  const located = new Map(placement.documents.map((document) => {
    const route = document.routes[rung];
    if (route === undefined) throw new Error(`${document.file} has no place on ${rung}.`);
    return [document.file, route];
  }));
  located.set(placement.readme.file, placement.readme.overview.route);
  return located;
}

/** A catalogue or rule file as its author writes it, with the file filter Open Forge reads beside the metadata. */
function authored(file: AuthoredFile, body: string): Buffer {
  const metadata = validateMetadata({ description: file.description, tags: [...file.tags] });
  const filter = file.applyTo === undefined ? "" : `  applyTo: [${file.applyTo.map((pattern) => JSON.stringify(pattern)).join(", ")}]\n`;
  return Buffer.from(`---\nopen-forge:\n  description: ${JSON.stringify(metadata.description)}\n  tags: [${metadata.tags.join(", ")}]\n${filter}---\n${body}`);
}

export function catalogueText(catalogue: Catalogue): Buffer {
  return authored(catalogue, `# ${catalogue.title}\n\n## ${catalogue.question}\n\n${catalogue.summary}\n\n## Axioms\n\n${catalogue.axioms.join("\n")}\n\n## Entries\n`);
}

/** Points links between pack documents at their placed routes. A pack document's link is a bare file name. */
function placedLinks(text: string, route: string, located: ReadonlyMap<string, string>): string {
  return rewriteLinks(Buffer.from(text), (link) => {
    const target = link.includes("/") ? undefined : located.get(link);
    return target === undefined ? null : relativeTo(route, target);
  }).toString("utf8");
}

/** The inverse of placedLinks, so a check can compare a placed document with the frozen one. */
function packLinks(text: string, route: string, reverse: ReadonlyMap<string, string>): string {
  return rewriteLinks(Buffer.from(text), (link) => reverse.get(posix.normalize(posix.join(posix.dirname(route), link))) ?? null).toString("utf8");
}

function linkBack(placement: Placement, route: string, directive: string): string {
  const file = placement.directiveFiles[directive];
  if (file === undefined) throw new Error(`${directive} isn't a placed rule file.`);
  return `The binding rule is in [its rule file](${relativeTo(route, directive)}).`;
}

const sentencesOf = (placement: Placement, document: string): readonly MovedSentence[] => placement.sentences.filter((moved) => moved.document === document);

/** Every file a placement adds to a project on one rung: catalogues, rule files and documents. */
export function placedFiles(placement: Placement, rung: ForgePreset, documents: readonly FileBytes[]): FileBytes[] {
  const located = routesFor(placement, rung);
  const pack = new Map(documents.map((file) => [file.path, file.bytes]));
  const output: FileBytes[] = [];
  for (const [route, catalogue] of Object.entries(placement.catalogues[rung] ?? {})) output.push({ path: inRoot(route), bytes: catalogueText(catalogue) });
  for (const [route, file] of Object.entries(placement.directiveFiles)) {
    const moved = placement.sentences.filter((item) => item.directive === route);
    const lines = moved.map((item) => `- ${placedLinks(item.sentence, route, located)}`);
    const pages = [...new Set(moved.map((item) => item.document))].map((name) => {
      const page = located.get(name), bytes = pack.get(name);
      if (page === undefined || bytes === undefined) throw new Error(`${name} isn't in the pack.`);
      return `[${titleOf(bytes)}](${relativeTo(route, page)})`;
    });
    output.push({ path: inRoot(route), bytes: authored(file, `# ${file.title}\n\n${InstructionsHeading}\n\n${lines.join("\n")}\n\nBackground: ${pages.join(", ")}.\n`) });
  }
  for (const document of placement.documents) {
    const bytes = pack.get(document.file), route = located.get(document.file);
    if (bytes === undefined || route === undefined) throw new Error(`${document.file} isn't in the pack.`);
    let text = Buffer.from(bytes).toString("utf8");
    const moved = sentencesOf(placement, document.file);
    // Each sentence holds its place through relinking, so a link inside it can't change what it matches.
    moved.forEach((item, index) => {
      if (text.split(item.sentence).length !== 2) throw new Error(`A moved sentence must appear once in ${document.file}.`);
      text = text.replace(item.sentence, () => `\u0000${index}\u0000`);
    });
    text = placedLinks(text, route, located);
    moved.forEach((item, index) => { text = text.replace(`\u0000${index}\u0000`, () => linkBack(placement, route, item.directive)); });
    output.push({ path: inRoot(route), bytes: addMetadata(Buffer.from(text), metadataFor(document, rung)) });
  }
  output.push(...readmeParts(placement, rung, pack.get(placement.readme.file), located));
  return output;
}

function metadataFor(document: PlacedDocument, rung: ForgePreset): Metadata {
  const tags = document.tags[rung];
  if (tags === undefined) throw new Error(`${document.file} has no tags on ${rung}.`);
  return { description: document.description, tags };
}

function readmeParts(placement: Placement, rung: ForgePreset, bytes: Uint8Array | undefined, located: ReadonlyMap<string, string>): FileBytes[] {
  const { marker, overview, list, fragments } = placement.readme, part = list[rung];
  if (bytes === undefined || part === undefined) throw new Error("A placement needs its README and its list's place.");
  const text = Buffer.from(bytes).toString("utf8"), cut = text.indexOf(marker);
  if (cut < 0) throw new Error("The README has no document list.");
  let listed = placedLinks(text.slice(cut), part.route, located);
  for (const fragment of fragments) listed = listed.replaceAll(`](${fragment})`, `](${relativeTo(part.route, overview.route)}${fragment})`);
  return [{ path: inRoot(overview.route), bytes: addMetadata(Buffer.from(placedLinks(text.slice(0, cut), overview.route, located)), { description: overview.description, tags: overview.tags }) },
    { path: inRoot(part.route), bytes: addMetadata(Buffer.from(listed), { description: part.description, tags: part.tags }) }];
}

/** The pack as it was frozen, from a placed project's files. Every fact comes back byte for byte or the setup isn't fair. */
export function restoreDocuments(placement: Placement, rung: ForgePreset, files: ReadonlyMap<string, Uint8Array>): Map<string, Buffer> {
  const located = routesFor(placement, rung);
  const reverse = new Map([...located].map(([file, route]) => [route, file]));
  const read = (route: string): string => {
    const bytes = files.get(inRoot(route));
    if (bytes === undefined) throw new Error(`Placed file is missing: ${route}`);
    return splitMarkdown(bytes).body.toString("utf8");
  };
  const restored = new Map<string, Buffer>();
  for (const document of placement.documents) {
    const route = located.get(document.file) ?? "";
    let text = packLinks(read(route), route, reverse);
    for (const moved of sentencesOf(placement, document.file)) {
      const link = linkBack(placement, route, moved.directive);
      if (!text.includes(link)) throw new Error(`${document.file} lost the link to its rule.`);
      text = text.replace(link, () => moved.sentence);
    }
    restored.set(document.file, Buffer.from(text));
  }
  const { overview, list, file, fragments } = placement.readme, part = list[rung];
  if (part === undefined) throw new Error("A placement needs its list's place.");
  let listed = packLinks(read(part.route), part.route, reverse);
  for (const fragment of fragments) listed = listed.replaceAll(`](${file}${fragment})`, `](${fragment})`);
  restored.set(file, Buffer.from(packLinks(read(overview.route), overview.route, reverse) + listed));
  return restored;
}

/** A moved sentence as its rule file holds it, with any link pointing at its placed target. */
export const placedSentence = (placement: Placement, rung: ForgePreset, moved: MovedSentence): string => placedLinks(moved.sentence, moved.directive, routesFor(placement, rung));

/** Whether each sentence left its page for its rule file's Instructions. Returns what's wrong, for a qualification to report. A page that merely repeats a sentence keeps its copy. */
export function misplacedSentences(placement: Placement, rung: ForgePreset, files: ReadonlyMap<string, Uint8Array>): string[] {
  const located = routesFor(placement, rung);
  const text = (route: string): string => Buffer.from(files.get(inRoot(route)) ?? []).toString("utf8");
  return placement.sentences.flatMap((moved) => {
    const short = `${moved.document}: "${moved.sentence.slice(0, 60)}"`;
    if (text(located.get(moved.document) ?? "").includes(moved.sentence)) return [`${short} is still in its page`];
    return text(moved.directive).split(InstructionsHeading)[1]?.includes(placedLinks(moved.sentence, moved.directive, located)) ? [] : [`${short} isn't under Instructions in ${moved.directive}`];
  });
}

/** The catalogue a rule file sits under, whose tags decide whether it loads by itself. */
export function catalogueOf(placement: Placement, directive: string, rung: ForgePreset): Catalogue {
  const folder = posix.dirname(directive), catalogue = placement.catalogues[rung]?.[`${folder}/_${posix.basename(folder)}.md`];
  if (catalogue === undefined) throw new Error(`${directive} has no catalogue on ${rung}.`);
  return catalogue;
}

/** Every placed path on a rung, for a setup's list of expected files. */
export function placedPaths(placement: Placement, rung: ForgePreset): string[] {
  const list = placement.readme.list[rung];
  return [...Object.keys(placement.catalogues[rung] ?? {}), ...Object.keys(placement.directiveFiles), ...placement.documents.flatMap((document) => document.routes[rung] ?? []),
    placement.readme.overview.route, ...(list === undefined ? [] : [list.route])].map(inRoot);
}
