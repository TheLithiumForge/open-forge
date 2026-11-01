import { isRecord, validateMetadata, type Metadata } from "./frontmatter.ts";
import { safeRelative } from "./files.ts";

export const ProjectionVersion = 1;
export const FullSpecification = "full";

export interface Projection {
  readonly $schema?: string;
  readonly schemaVersion: number;
  readonly fixture: string;
  readonly level: string;
  readonly frameworkVersion: string;
  readonly documents: Readonly<Record<string, Metadata>>;
}

export function readProjection(value: unknown): Projection {
  if (!isRecord(value) || typeof value["fixture"] !== "string" || !value["fixture"] || typeof value["frameworkVersion"] !== "string" || value["level"] !== FullSpecification || !isRecord(value["documents"])) {
    throw new Error("projection.json needs fixture, frameworkVersion, level and documents.");
  }
  if (value["schemaVersion"] !== ProjectionVersion) process.emitWarning("projection.json schemaVersion differs from the supported version.");
  const documents: Record<string, Metadata> = {};
  for (const [path, metadata] of Object.entries(value["documents"])) {
    safeRelative(path);
    if (path.includes("/") || !path.endsWith(".md")) throw new Error("projection.json document names must be Markdown filenames.");
    documents[path] = validateMetadata(metadata);
  }
  if (!("README.md" in documents)) throw new Error("projection.json documents needs README.md.");
  return { ...(typeof value["$schema"] === "string" ? { $schema: value["$schema"] } : {}), schemaVersion: ProjectionVersion, fixture: value["fixture"], level: value["level"], frameworkVersion: value["frameworkVersion"], documents };
}
