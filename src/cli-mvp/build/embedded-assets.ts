export const EmbeddedAssetFingerprintDomain = {
  current: "open-forge.embedded-assets.v1",
} as const;

export type Sha256Checksum = `sha256:${string}`;

export interface BuildIdentity {
  readonly name: string;
  readonly version: string;
}

export interface EmbeddedTextFile {
  readonly path: string;
  readonly text: string;
  readonly checksum: Sha256Checksum;
}

export interface EmbeddedFileSet {
  readonly fingerprint: Sha256Checksum;
  readonly files: readonly EmbeddedTextFile[];
}

export interface EmbeddedAssets {
  readonly framework: EmbeddedFileSet;
  readonly extensionCatalogue: EmbeddedFileSet;
}
