import { createHash } from "node:crypto";
import { readFile, stat } from "node:fs/promises";
import { join } from "node:path";
import { deflateSync, inflateSync } from "node:zlib";
import { GitDirectory, inventory, writeNew, type FileBytes } from "./files.ts";

const ObjectKind = { blob: "blob", tree: "tree", commit: "commit" } as const;
const FileMode = 0o100644;
const IndexVersion = 2;
const IndexEntrySize = 62;
const IndexAlignment = 8;
const MaximumNameBytes = 0xfff;
const SecondMs = 1000;
const NanosecondMs = 1_000_000;
const InitialIdentity = "Open Forge Evals <evals@example.invalid> 0 +0000";
export const InitialCommitMessage = "Initial project";

function digest(bytes: Uint8Array): Buffer { return createHash("sha1").update(bytes).digest(); }

async function writeObject(root: string, kind: string, bytes: Buffer): Promise<Buffer> {
  const object = Buffer.concat([Buffer.from(`${kind} ${bytes.length}\0`), bytes]);
  const hash = digest(object);
  const hex = hash.toString("hex");
  // Identical content can appear at several paths in the same initial tree.
  try { await writeNew(root, `${GitDirectory}/objects/${hex.slice(0, 2)}/${hex.slice(2)}`, deflateSync(object)); }
  catch (error) { if (!(error instanceof Error && "code" in error && error.code === "EEXIST")) throw error; }
  return hash;
}

async function writeTree(root: string, files: readonly FileBytes[], blobs: ReadonlyMap<string, Buffer>, prefix = ""): Promise<Buffer> {
  const entries = new Map<string, { directory: boolean; hash: Buffer }>();
  for (const file of files.filter((file) => file.path.startsWith(prefix))) {
    const path = file.path.slice(prefix.length);
    const part = path.split("/")[0];
    if (!part || entries.has(part)) continue;
    const directory = path.includes("/");
    const hash = directory ? await writeTree(root, files, blobs, `${prefix}${part}/`) : blobs.get(file.path);
    if (!hash) throw new Error("Missing initial repository object.");
    entries.set(part, { directory, hash });
  }
  const sorted = [...entries].sort(([a, x], [b, y]) => Buffer.compare(Buffer.from(a + (x.directory ? "/" : "")), Buffer.from(b + (y.directory ? "/" : ""))));
  const bytes = sorted.map(([name, entry]) => Buffer.concat([Buffer.from(`${entry.directory ? "40000" : "100644"} ${name}\0`), entry.hash]));
  return writeObject(root, ObjectKind.tree, Buffer.concat(bytes));
}

async function writeIndex(root: string, files: readonly FileBytes[], blobs: ReadonlyMap<string, Buffer>): Promise<void> {
  const header = Buffer.alloc(12);
  header.write("DIRC");
  header.writeUInt32BE(IndexVersion, 4);
  header.writeUInt32BE(files.length, 8);
  const entries: Buffer[] = [];
  for (const file of files) {
    const info = await stat(join(root, file.path));
    const name = Buffer.from(file.path);
    const bytes = Buffer.alloc(Math.ceil((IndexEntrySize + name.length + 1) / IndexAlignment) * IndexAlignment);
    for (const [offset, time] of [[0, info.ctimeMs], [8, info.mtimeMs]] as const) {
      bytes.writeUInt32BE(Math.floor(time / SecondMs), offset);
      bytes.writeUInt32BE(Math.floor(time % SecondMs * NanosecondMs), offset + 4);
    }
    bytes.writeUInt32BE(FileMode, 24);
    bytes.writeUInt32BE(file.bytes.length, 36);
    const hash = blobs.get(file.path);
    if (!hash) throw new Error("Missing initial index object.");
    hash.copy(bytes, 40);
    bytes.writeUInt16BE(Math.min(name.length, MaximumNameBytes), 60);
    name.copy(bytes, IndexEntrySize);
    entries.push(bytes);
  }
  const bytes = Buffer.concat([header, ...entries]);
  const content = Buffer.concat([bytes, digest(bytes)]);
  await writeNew(root, `${GitDirectory}/index`, content);
}

/** Setup generation writes seed data without invoking Git or reading a user's Git settings. */
export async function seedRepository(root: string): Promise<void> {
  const files = await inventory(root);
  const blobs = new Map<string, Buffer>();
  for (const file of files) blobs.set(file.path, await writeObject(root, ObjectKind.blob, file.bytes));
  const tree = await writeTree(root, files, blobs);
  const commit = Buffer.from(`tree ${tree.toString("hex")}\nauthor ${InitialIdentity}\ncommitter ${InitialIdentity}\n\n${InitialCommitMessage}\n`);
  const hash = await writeObject(root, ObjectKind.commit, commit);
  await writeNew(root, `${GitDirectory}/HEAD`, "ref: refs/heads/main\n");
  await writeNew(root, `${GitDirectory}/refs/heads/main`, `${hash.toString("hex")}\n`);
  await writeNew(root, `${GitDirectory}/config`, "[core]\n\trepositoryformatversion = 0\n\tfilemode = false\n\tbare = false\n\tlogallrefupdates = true\n");
  await writeIndex(root, [...files].sort((a, b) => Buffer.compare(Buffer.from(a.path), Buffer.from(b.path))), blobs);
}

export async function initialRepository(root: string): Promise<void> {
  if (await readFile(join(root, GitDirectory, "HEAD"), "utf8") !== "ref: refs/heads/main\n") throw new Error("The setup has no initial main branch.");
  const hash = (await readFile(join(root, GitDirectory, "refs/heads/main"), "utf8")).trim();
  if (!/^[a-f\d]{40}$/.test(hash)) throw new Error("The initial commit reference is invalid.");
  const object = inflateSync(await readFile(join(root, GitDirectory, "objects", hash.slice(0, 2), hash.slice(2))));
  if (digest(object).toString("hex") !== hash || !object.toString().startsWith(`${ObjectKind.commit} `)) throw new Error("Invalid setup history object.");
  if (/^parent /m.test(object.toString()) || !object.toString().endsWith(`\n\n${InitialCommitMessage}\n`)) throw new Error("The setup needs one Initial project commit without a parent.");
  const objects = await inventory(join(root, GitDirectory, "objects"));
  if (objects.filter((file) => inflateSync(file.bytes).toString().startsWith(`${ObjectKind.commit} `)).length !== 1) throw new Error("The setup needs exactly one commit object.");
  const index = await readFile(join(root, GitDirectory, "index"));
  if (index.toString("utf8", 0, 4) !== "DIRC" || !digest(index.subarray(0, -20)).equals(index.subarray(-20))) throw new Error("The initial repository index is invalid.");
}
