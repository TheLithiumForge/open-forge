import { existsSync } from "node:fs";
import { mkdir, readFile, rename, writeFile } from "node:fs/promises";
import { dirname } from "node:path";
import { randomUUID } from "node:crypto";

export async function readJson<T>(path: string): Promise<T> {
  return JSON.parse(await readFile(path, "utf8")) as T; // The persisted JSON is the external type boundary.
}

export async function writeJson(path: string, value: unknown): Promise<void> {
  await mkdir(dirname(path), { recursive: true });
  const temporary = `${path}.${randomUUID()}.tmp`;
  await writeFile(temporary, `${JSON.stringify(value, null, 2)}\n`);
  await rename(temporary, path);
}

export async function readJsonLines<T>(path: string): Promise<T[]> {
  if (!existsSync(path)) return [];
  return (await readFile(path, "utf8")).split("\n").filter((line) => line.trim() !== "").map((line) => JSON.parse(line) as T);
}

export function jsonLines(records: readonly unknown[]): string {
  return records.map((record) => `${JSON.stringify(record)}\n`).join("");
}

export function safeId(id: string): string {
  if (!/^[\w][\w.-]*$/.test(id) || id === "." || id === "..") throw new Error("Use a record ID within its folder.");
  return id;
}
