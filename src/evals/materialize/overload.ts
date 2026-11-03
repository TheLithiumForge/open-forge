import { readFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { addMetadata, isRecord } from "./frontmatter.ts";
import { safeRelative, sha256, type FileHash } from "./files.ts";

/**
 * The notes a real project collects beside its spec: decisions, analyses, ideas, tryouts and spikes. They add no
 * requirement and reveal no defect. Overloaded docs mix them into docs/. An Open Forge setup keeps each one on the
 * Memory route for its kind, so decisions are accepted knowledge and the rest stay candidates.
 */
export const OverloadVersion = 1;
export const NoteKind = { decision: "decision", analysis: "analysis", idea: "idea", observation: "observation" } as const;
export type NoteKind = (typeof NoteKind)[keyof typeof NoteKind];
export const NoteRoutes: Readonly<Record<NoteKind, string>> = {
  [NoteKind.decision]: ".agents/memory/crystallized/decisions",
  [NoteKind.analysis]: ".agents/memory/emerging/analysis",
  [NoteKind.idea]: ".agents/memory/emerging/ideas",
  [NoteKind.observation]: ".agents/memory/emerging/observations",
};
/** The bundled Extensions that install the routes above. */
export const NoteExtensions = ["planning", "observations-and-handoffs"] as const;
const NotesFolder = "notes";

export interface Note { readonly file: string; readonly kind: NoteKind; readonly description: string; readonly tags: readonly string[]; readonly bytes: Buffer }
export interface Overload { readonly notes: readonly Note[] }

const kinds = new Set<string>(Object.values(NoteKind));

export async function loadOverload(manifestPath: string): Promise<Overload> {
  const manifest: unknown = JSON.parse(await readFile(manifestPath, "utf8"));
  if (!isRecord(manifest) || manifest["schemaVersion"] !== OverloadVersion || !Array.isArray(manifest["notes"])) throw new Error("Invalid overload manifest.");
  const notes = await Promise.all(manifest["notes"].map(async (entry: unknown): Promise<Note> => {
    if (!isRecord(entry) || typeof entry["file"] !== "string" || typeof entry["kind"] !== "string" || !kinds.has(entry["kind"])
      || typeof entry["description"] !== "string" || !Array.isArray(entry["tags"]) || entry["tags"].some((tag) => typeof tag !== "string")) throw new Error("Invalid overload note.");
    const file = safeRelative(entry["file"]);
    if (file.includes("/") || !file.endsWith(".md")) throw new Error("An overload note is one Markdown file.");
    return { file, kind: entry["kind"] as NoteKind, description: entry["description"], tags: entry["tags"] as string[], bytes: await readFile(join(dirname(manifestPath), NotesFolder, file)) };
  }));
  if (new Set(notes.map((note) => note.file)).size !== notes.length) throw new Error("Overload notes need distinct names.");
  return { notes };
}

/** Where a setup writes a note: on its Memory route for Open Forge, or among the documents otherwise. */
export function notePath(note: Pick<Note, "file" | "kind">, forge: boolean, docs: string): string {
  return forge ? `${NoteRoutes[note.kind]}/${note.file}` : `${docs}/${note.file}`;
}

/** A note as a setup writes it. Open Forge notes carry their description and tags, as an author would add them. */
export function noteBytes(note: Note, forge: boolean): Buffer {
  return forge ? addMetadata(note.bytes, { description: note.description, tags: [...note.tags] }) : note.bytes;
}

/** Each note's place and body hash, so qualification can prove the notes kept every word. */
export function noteHashes(overload: Overload, forge: boolean, docs: string): FileHash[] {
  return overload.notes.map((note) => ({ path: notePath(note, forge, docs), sha256: sha256(note.bytes) }));
}
