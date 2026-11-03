import { readFile } from "node:fs/promises";
import { posix } from "node:path";
import { addMetadata, isRecord, validateMetadata, type Metadata } from "./frontmatter.ts";

/** Open Forge reads a document with this tag at the start of every task. Every other document waits until a task needs it. */
export const UpFrontTag = "LoadNow";
export const ScopeLayoutVersion = 1;
const InheritedAxioms = "- inherited - No local axioms; loaded ancestor axioms remain active.";
const SpecLink = /\]\(([\w./-]+\.md)(#[^)\s]*)?\)/g;

/** One scope folder. Its entrypoint's description and tags tell an agent when to read it. */
export interface SpecScope {
  readonly slug: string;
  readonly title: string;
  readonly description: string;
  readonly tags: readonly string[];
  readonly documents: readonly string[];
}

/** Where a scoped setup puts the spec documents. Documents in no scope stay at the docs root, and only the up-front ones load at the start. */
export interface ScopeLayout {
  readonly schemaVersion: number;
  readonly upFront: readonly string[];
  readonly scopes: readonly SpecScope[];
}

function documentNames(value: unknown): string[] {
  if (!Array.isArray(value) || !value.every((item): item is string => typeof item === "string" && /^[\w.-]+\.md$/.test(item))) throw new Error("Scope documents are spec file names.");
  return value;
}

export function readScopeLayout(value: unknown): ScopeLayout {
  if (!isRecord(value) || value["schemaVersion"] !== ScopeLayoutVersion || !Array.isArray(value["scopes"])) throw new Error("A scope layout needs its version and scopes.");
  const scopes = value["scopes"].map((scope: unknown): SpecScope => {
    if (!isRecord(scope) || typeof scope["slug"] !== "string" || !/^[a-z][a-z-]*$/.test(scope["slug"]) || typeof scope["title"] !== "string" || !scope["title"].trim()) throw new Error("A scope needs a folder name and a title.");
    return { slug: scope["slug"], title: scope["title"], ...validateMetadata({ description: scope["description"], tags: scope["tags"] }), documents: documentNames(scope["documents"]) };
  });
  const placed = scopes.flatMap((scope) => scope.documents), upFront = documentNames(value["upFront"]);
  if (new Set(placed).size !== placed.length || new Set(scopes.map((scope) => scope.slug)).size !== scopes.length) throw new Error("Each scope and each document appears once.");
  if (upFront.some((path) => placed.includes(path))) throw new Error("Up-front documents stay at the docs root, outside every scope.");
  return { schemaVersion: ScopeLayoutVersion, upFront, scopes };
}

export async function loadScopeLayout(path: string): Promise<ScopeLayout> {
  return readScopeLayout(JSON.parse(await readFile(path, "utf8")));
}

/** A spec document's path inside docs: in its scope folder, or at the docs root. */
export function scopedPath(layout: ScopeLayout, path: string): string {
  const scope = layout.scopes.find((item) => item.documents.includes(path));
  return scope === undefined ? path : `${scope.slug}/${path}`;
}

export function scopeEntrypoint(scope: SpecScope): string {
  return `${scope.slug}/_${scope.slug}.md`;
}

/** The scope's entrypoint before Open Forge indexes it. Open Forge fills the Entries and keeps the description and tags. */
export function entrypointText(scope: SpecScope): Buffer {
  return addMetadata(Buffer.from(`# ${scope.title}\n\n${scope.description}.\n\n## Axioms\n\n${InheritedAxioms}\n\n## Entries\n`), { description: scope.description, tags: scope.tags });
}

/** The document's own description and tags, loading up front only when the layout says so. */
export function scopedMetadata(layout: ScopeLayout, path: string, metadata: Metadata): Metadata {
  const tags = metadata.tags.filter((tag) => tag !== UpFrontTag);
  return { description: metadata.description, tags: layout.upFront.includes(path) ? [UpFrontTag, ...tags] : tags };
}

function rewriteLinks(bytes: Uint8Array, target: (link: string) => string | null): Buffer {
  return Buffer.from(Buffer.from(bytes).toString("utf8").replace(SpecLink, (link, path: string, anchor: string | undefined) => {
    const moved = target(path);
    return moved === null ? link : `](${moved}${anchor ?? ""})`;
  }));
}

/** Points the links between spec documents at their scoped paths. The spec keeps every document at its root, so a link is a file name. */
export function relink(layout: ScopeLayout, path: string, bytes: Uint8Array): Buffer {
  const folder = posix.dirname(scopedPath(layout, path));
  return rewriteLinks(bytes, (link) => link.includes("/") ? null : posix.relative(folder, scopedPath(layout, link)));
}

/** The inverse of relink, so a check can compare a scoped document with the frozen one byte for byte. */
export function unlink(layout: ScopeLayout, path: string, bytes: Uint8Array): Buffer {
  const located = scopedPath(layout, path);
  return rewriteLinks(bytes, (link) => {
    const resolved = posix.normalize(posix.join(posix.dirname(located), link));
    const original = posix.basename(resolved);
    return scopedPath(layout, original) === resolved ? original : null;
  });
}
