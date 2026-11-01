export const ReferenceImplementer = "reference";
export const ReferenceProject = "benchmarks/neon-pairs/reference";

export interface ReferenceVersion {
  readonly specVersion: number;
  readonly commit: string;
  readonly contentHash: string;
  readonly specHash: string;
  readonly files: readonly { readonly path: string; readonly bytes: number; readonly sha256: string }[];
}
