export interface IgnoreDocument { readonly path: string; readonly content: string }
export interface ScopePolicy {
  readonly ignores: readonly IgnoreDocument[];
  readonly openForge: boolean;
  readonly forgeFiles: readonly string[];
}
