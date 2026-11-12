import assert from "node:assert/strict";
import { inflateRawSync } from "node:zlib";

export const WheelFixture = {
  version: "0.9.0-beta.18",
  pythonVersion: "0.9.0b18",
  native: Buffer.from("owned inert executable fixture"),
  license: Buffer.from("owned license fixture\n"),
} as const;

// Read ZIP's central directory independently of the writer to check its offsets and payloads.
export function readWheel(bytes: Buffer): { path: string; bytes: Buffer; mode: number; checksum: number }[] {
  const end = bytes.length - 22;
  assert.equal(bytes.readUInt32LE(end), 0x06054b50);
  const count = bytes.readUInt16LE(end + 10);
  let offset = bytes.readUInt32LE(end + 16);
  return Array.from({ length: count }, () => {
    assert.equal(bytes.readUInt32LE(offset), 0x02014b50);
    const length = bytes.readUInt16LE(offset + 28);
    const path = bytes.subarray(offset + 46, offset + 46 + length).toString();
    const local = bytes.readUInt32LE(offset + 42);
    assert.equal(bytes.readUInt32LE(local), 0x04034b50);
    assert.equal(bytes.readUInt16LE(local + 8), 8);
    assert.equal(bytes.readUInt16LE(local + 10), 0);
    assert.equal(bytes.readUInt16LE(local + 12), 33);
    assert.equal(bytes.readUInt16LE(local + 28), 0);
    const start = local + 30 + bytes.readUInt16LE(local + 26);
    const content = inflateRawSync(bytes.subarray(start, start + bytes.readUInt32LE(offset + 20)));
    assert.equal(content.length, bytes.readUInt32LE(offset + 24));
    const entry = { path, bytes: content, mode: bytes.readUInt32LE(offset + 38) >>> 16, checksum: bytes.readUInt32LE(offset + 16) };
    offset += 46 + length + bytes.readUInt16LE(offset + 30) + bytes.readUInt16LE(offset + 32);
    return entry;
  });
}
