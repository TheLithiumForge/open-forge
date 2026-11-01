export const SelectionKind = { closed: 'closed', one: 'one', mismatch: 'mismatch' } as const;

export type Selection =
  | { readonly kind: typeof SelectionKind.closed }
  | { readonly kind: typeof SelectionKind.one; readonly index: number }
  | {
      readonly kind: typeof SelectionKind.mismatch;
      readonly first: number;
      readonly second: number;
      readonly remainingMs: number;
    };
