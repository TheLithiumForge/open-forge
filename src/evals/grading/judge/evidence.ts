import { createHash } from "node:crypto";
import { lstat, readFile, readdir } from "node:fs/promises";
import { dirname, join } from "node:path";
import { PNG } from "pngjs";
import { AssessorEvidenceLimit } from "../../execution/containers/assessor.ts";
import { AssessorImageLimits, readAssessorImages } from "../../execution/containers/files.ts";
import { visibleToJudge } from "./judge-folder.ts";

export interface JudgeSource { readonly path: string; readonly text: string; readonly lines: number }
export interface JudgeImage { readonly id: string; readonly sha256: string; readonly png: Buffer; readonly width: number; readonly height: number }
export interface JudgeEvidence { readonly sources: readonly JudgeSource[]; readonly images: readonly JudgeImage[] }
const MaxImagePixels = 16 * 1024 * 1024;
const decoder = new TextDecoder("utf-8", { fatal: true });

export async function judgeEvidence(folder: string, imagePaths: readonly string[] = []): Promise<JudgeEvidence> {
  if (imagePaths.length > AssessorImageLimits.count) throw new Error("Too many judge images.");
  const sources: JudgeSource[] = [];
  if ((await lstat(folder)).isSymbolicLink()) throw new Error("Judge evidence cannot be a link.");
  async function read(path: string, prefix: string): Promise<void> {
    for (const entry of (await readdir(path, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
      if (entry.isSymbolicLink() || !visibleToJudge(entry.name, entry.isDirectory())) continue;
      const name = prefix ? `${prefix}/${entry.name}` : entry.name;
      if (entry.isDirectory()) await read(join(path, entry.name), name);
      else if (entry.isFile()) {
        const text = decoder.decode(await readFile(join(path, entry.name)));
        sources.push({ path: name, text, lines: text.split("\n").length });
      }
      if (Buffer.byteLength(JSON.stringify(sources)) > AssessorEvidenceLimit) throw new Error("Judge source exceeds the assessor input limit.");
    }
  }
  await read(folder, "");
  const images: JudgeImage[] = [];
  for (const path of imagePaths) {
    const [image] = await readAssessorImages(dirname(path), [path]);
    if (image === undefined) throw new Error("Missing judge image.");
    const png = image.bytes;
    if (png.length < 24 || png.readUInt32BE(16) * png.readUInt32BE(20) > MaxImagePixels) throw new Error("Judge image exceeds the pixel limit.");
    const decoded = PNG.sync.read(png);
    const id = image.name;
    if (images.some((image) => image.id === id)) throw new Error("Duplicate judge image ID.");
    images.push({ id, sha256: createHash("sha256").update(png).digest("hex"), png, width: decoded.width, height: decoded.height });
  }
  return { sources, images };
}

export function validCitation(citation: string, evidence: JudgeEvidence): boolean {
  if (citation === "missing:images") return evidence.images.length === 0;
  if (citation === "missing:source") return evidence.sources.length === 0;
  if (evidence.images.some((image) => citation === image.id)) return true;
  const match = /^source:(.+):(\d+)(?:-(\d+))?$/.exec(citation);
  if (!match) return false;
  const file = evidence.sources.find((source) => source.path === match[1]);
  const start = Number(match[2]), end = Number(match[3] ?? match[2]);
  return file !== undefined && start >= 1 && end >= start && end <= file.lines;
}
