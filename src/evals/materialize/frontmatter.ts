import { isDeepStrictEqual } from "node:util";
import { Document, isMap, parseDocument } from "yaml";

export interface Metadata {
  readonly description: string;
  readonly tags: readonly string[];
}

export const MetadataNamespace = "open-forge";
const decoder = new TextDecoder("utf-8", { fatal: true, ignoreBOM: true });
const metadataKeys = new Set(["description", "tags"]);

export function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export function validateMetadata(value: unknown): Metadata {
  if (!isRecord(value) || Object.keys(value).some((key) => !metadataKeys.has(key))) throw new Error("Unknown Open Forge metadata.");
  const { description, tags } = value;
  if (typeof description !== "string" || !description.trim()) throw new Error("Open Forge description must be nonempty.");
  if (!Array.isArray(tags) || !tags.every((tag: unknown): tag is string => typeof tag === "string" && !!tag.trim()) || new Set(tags).size !== tags.length) {
    throw new Error("Open Forge tags must be distinct strings.");
  }
  return { description, tags };
}

export function splitMarkdown(bytes: Uint8Array): { body: Buffer; document: Document | null } {
  const text = decoder.decode(bytes);
  if (!text.startsWith("---\n") && !text.startsWith("---\r\n")) return { body: Buffer.from(bytes), document: null };
  const opening = text.indexOf("\n") + 1;
  const delimiter = /^---\r?$/gm;
  delimiter.lastIndex = opening;
  const closing = delimiter.exec(text);
  if (!closing) throw new Error("Malformed YAML frontmatter: missing closing delimiter.");
  const end = closing.index + closing[0].length;
  const offset = end + (text[end] === "\n" ? 1 : 0);
  const document = parseDocument(text.slice(opening, closing.index), { uniqueKeys: true });
  if (document.errors.length || !isMap(document.contents)) throw new Error("Malformed YAML frontmatter: expected one mapping.");
  return { body: Buffer.from(text.slice(offset)), document };
}

export function documentMetadata(bytes: Uint8Array): Metadata {
  const { document } = splitMarkdown(bytes);
  if (!document?.has(MetadataNamespace)) throw new Error("Open Forge metadata is missing.");
  const value: unknown = document.toJS();
  if (!isRecord(value)) throw new Error("Malformed YAML frontmatter.");
  return validateMetadata(value[MetadataNamespace]);
}

export function addMetadata(bytes: Uint8Array, metadata: Metadata): Buffer {
  const { document, body } = splitMarkdown(bytes);
  if (document?.has(MetadataNamespace)) throw new Error("Source already contains Open Forge metadata.");
  const header = document ?? new Document({});
  header.set(MetadataNamespace, validateMetadata(metadata));
  const namespace = header.get(MetadataNamespace, true);
  if (isMap(namespace)) {
    const tags = namespace.get("tags", true);
    if (tags && typeof tags === "object" && "flow" in tags) tags.flow = true;
  }
  return Buffer.concat([Buffer.from(`---\n${header.toString()}---\n`), body]);
}

export function removeMetadata(bytes: Uint8Array, expected: Metadata): Buffer {
  if (!isDeepStrictEqual(documentMetadata(bytes), validateMetadata(expected))) throw new Error("Open Forge metadata differs from the manifest.");
  const { document, body } = splitMarkdown(bytes);
  if (!document) throw new Error("Open Forge metadata is missing.");
  document.delete(MetadataNamespace);
  return isMap(document.contents) && document.contents.items.length === 0
    ? body
    : Buffer.concat([Buffer.from(`---\n${document.toString()}---\n`), body]);
}
