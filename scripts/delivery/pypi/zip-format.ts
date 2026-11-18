export const ZipFormat = {
  localSignature: 0x04034b50,
  centralSignature: 0x02014b50,
  endSignature: 0x06054b50,
  localBytes: 30,
  centralBytes: 46,
  endBytes: 22,
  version: 20,
  unixVersion: (3 << 8) | 20,
  utf8: 0x0800,
  deflate: 8,
  epochDate: 33, // 1980-01-01, the first ZIP date.
  fileMode: 0o100644,
  executableMode: 0o100755,
  maximumBytes: 0xffffffff,
  permissionShift: 16,
} as const;

export const LocalField = { version: 4, flags: 6, method: 8, date: 12, crc: 14, compressedSize: 18, size: 22, nameLength: 26 } as const;
export const CentralField = { creator: 4, localFields: 6, attributes: 38, offset: 42 } as const;
export const EndField = { entriesOnDisk: 8, entries: 10, size: 12, offset: 16 } as const;
