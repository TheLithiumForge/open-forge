import { inflateRawSync } from "node:zlib";
import { ZipFormat } from "./zip-format.ts";

// Central directory and local header offsets that wheelZip writes and this reader needs.
const CentralRead = { method: 10, compressedSize: 20, size: 24, nameLength: 28, extraLength: 30, commentLength: 32, offset: 42 } as const;
const LocalRead = { nameLength: 26, extraLength: 28 } as const;
const EndRead = { entries: 10, offset: 16 } as const;
const StoredMethod = 0;

/** Returns one entry's bytes from a ZIP32 archive, or undefined when the archive has no entry at that path. */
export function readZipEntry(archive: Buffer, path: string): Buffer | undefined {
  const end = archive.lastIndexOf(uint32(ZipFormat.endSignature));
  if (end < 0) throw new Error("The archive has no ZIP end record.");
  let cursor = archive.readUInt32LE(end + EndRead.offset);
  for (let index = 0; index < archive.readUInt16LE(end + EndRead.entries); index++) {
    // Every step moves forward by at least one header, and the directory must end before the end record.
    if (cursor + ZipFormat.centralBytes > end || archive.readUInt32LE(cursor) !== ZipFormat.centralSignature) throw new Error("The ZIP central directory is malformed.");
    const nameLength = archive.readUInt16LE(cursor + CentralRead.nameLength);
    const name = archive.subarray(cursor + ZipFormat.centralBytes, cursor + ZipFormat.centralBytes + nameLength).toString("utf8");
    if (name === path) return entryBytes(archive, cursor);
    cursor += ZipFormat.centralBytes + nameLength + archive.readUInt16LE(cursor + CentralRead.extraLength) + archive.readUInt16LE(cursor + CentralRead.commentLength);
  }
  return undefined;
}

function entryBytes(archive: Buffer, central: number): Buffer {
  const local = archive.readUInt32LE(central + CentralRead.offset);
  if (archive.readUInt32LE(local) !== ZipFormat.localSignature) throw new Error("The ZIP local header is malformed.");
  const start = local + ZipFormat.localBytes + archive.readUInt16LE(local + LocalRead.nameLength) + archive.readUInt16LE(local + LocalRead.extraLength);
  const data = archive.subarray(start, start + archive.readUInt32LE(central + CentralRead.compressedSize));
  const method = archive.readUInt16LE(central + CentralRead.method);
  if (method === StoredMethod) return Buffer.from(data);
  // Cap the output at the recorded size, so a malformed entry fails instead of inflating without limit.
  if (method === ZipFormat.deflate) return inflateRawSync(data, { maxOutputLength: Math.max(1, archive.readUInt32LE(central + CentralRead.size)) });
  throw new Error(`Unsupported ZIP compression method ${method}.`);
}

function uint32(value: number): Buffer {
  const bytes = Buffer.alloc(4);
  bytes.writeUInt32LE(value);
  return bytes;
}
