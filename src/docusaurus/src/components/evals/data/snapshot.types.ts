import type { Batch } from "../records/records.types.ts";
import type { Catalogue } from "../records/catalogue.ts";
export interface Snapshot {
  readonly batches: readonly Batch[];
  readonly index: Catalogue;
  readonly commit: string;
  readonly fetchedAt: string;
}
export interface EvalsData {
  readonly snapshot: Snapshot;
  readonly schemas: readonly object[];
  readonly indexSchema: object;
}
