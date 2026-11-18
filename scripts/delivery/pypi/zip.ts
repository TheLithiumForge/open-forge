import assert from "node:assert/strict";
import { crc32, deflateRawSync } from "node:zlib";
import { ZipFormat, LocalField, CentralField, EndField } from "./zip-format.ts";

export interface ZipEntry {
  path: string;
  bytes: Buffer;
  executable?: boolean;
}

// Wheel archives are small enough for ZIP32. Fixed headers omit host timestamps and extra fields.
export function wheelZip(entries: readonly ZipEntry[]): Buffer {
  const local: Buffer[] = [];
  const central: Buffer[] = [];
  let offset = 0;
  for (const entry of entries) {
    const name = Buffer.from(entry.path);
    const compressed = deflateRawSync(entry.bytes);
    const checksum = crc32(entry.bytes);
    assert.ok(entry.bytes.length < ZipFormat.maximumBytes && offset < ZipFormat.maximumBytes, "Wheel exceeds ZIP32 limits.");
    const header = Buffer.alloc(ZipFormat.localBytes);
    header.writeUInt32LE(ZipFormat.localSignature);
    header.writeUInt16LE(ZipFormat.version, LocalField.version);
    header.writeUInt16LE(ZipFormat.utf8, LocalField.flags);
    header.writeUInt16LE(ZipFormat.deflate, LocalField.method);
    header.writeUInt16LE(ZipFormat.epochDate, LocalField.date);
    header.writeUInt32LE(checksum, LocalField.crc);
    header.writeUInt32LE(compressed.length, LocalField.compressedSize);
    header.writeUInt32LE(entry.bytes.length, LocalField.size);
    header.writeUInt16LE(name.length, LocalField.nameLength);
    const directory = Buffer.alloc(ZipFormat.centralBytes);
    directory.writeUInt32LE(ZipFormat.centralSignature);
    directory.writeUInt16LE(ZipFormat.unixVersion, CentralField.creator);
    header.copy(directory, CentralField.localFields, LocalField.version);
    directory.writeUInt32LE(((entry.executable ? ZipFormat.executableMode : ZipFormat.fileMode) << ZipFormat.permissionShift) >>> 0, CentralField.attributes);
    directory.writeUInt32LE(offset, CentralField.offset);
    local.push(header, name, compressed);
    central.push(directory, name);
    offset += header.length + name.length + compressed.length;
  }
  const directory = Buffer.concat(central);
  const end = Buffer.alloc(ZipFormat.endBytes);
  end.writeUInt32LE(ZipFormat.endSignature);
  end.writeUInt16LE(entries.length, EndField.entriesOnDisk);
  end.writeUInt16LE(entries.length, EndField.entries);
  end.writeUInt32LE(directory.length, EndField.size);
  end.writeUInt32LE(offset, EndField.offset);
  return Buffer.concat([...local, directory, end]);
}
