---
open-forge:
  description: Define protocol-significant and otherwise magic TypeScript strings and numbers once through named enums or readonly const objects
  tags: [Pattern, TypeScript, Constant, Enum, Literal, TypeSafety, Readability, Protocol]
---

# Named TypeScript Values

## Shape

Give a semantic string or number one named runtime definition and derive its type from that definition.

Use a readonly const object when values cross JSON, file, process, or library boundaries, or when ordinary object access and iteration remain useful:

```ts
export const DocumentState = {
  draft: "draft",
  ready: "ready",
  archived: "archived",
} as const;

export type DocumentState = (typeof DocumentState)[keyof typeof DocumentState];
```

Use an enum when enum semantics make a contained TypeScript boundary clearer:

```ts
export enum TraversalState {
  unseen = "unseen",
  visiting = "visiting",
  complete = "complete",
}
```

Choose one representation for one concept. Do not define an enum and a const object that repeat the same values.

## Mappings

Build mappings from named values:

```ts
export const TraversalRank = {
  [TraversalState.unseen]: 0,
  [TraversalState.visiting]: 1,
  [TraversalState.complete]: 2,
} as const satisfies Record<TraversalState, number>;
```

`satisfies` is deliberate here because the mapping retains its concrete inferred keys and values while TypeScript checks that every state is covered.

Consumers use `TraversalState.visiting` and `TraversalRank[state]`. They do not repeat `"visiting"` or its policy number.

Keep protocol versions named:

```ts
export const FormatVersion = {
  current: 1,
} as const;
```

The new CLI's concrete result statuses and exit values remain unsettled. This
general TypeScript Pattern demonstrates a representation without defining that
protocol.

## Placement

Place a value object beside the contract it defines:

- CLI-wide schema versions, operation ids, result statuses, message levels, and exit values live at CLI scope.
- Command or family diagnostic codes begin inside that command or family.
- A domain-specific state object remains with its domain.
- Move a definition only to the nearest common ancestor of demonstrated consumers.

Do not create one global constants catalogue. Sharing follows meaning and actual reuse, not the fact that several values are literals.

## Magic Boundary

A value is magic when its semantic or control meaning is not obvious from the use site, must remain consistent across consumers, participates in a protocol, or would be risky to change through search and replacement.

Ordinary one-off presentation copy, test fixture content, and self-explanatory local arithmetic are not automatically magic merely because they are strings or numbers. Name them when repetition, domain meaning, or change risk makes that useful.

At minimum, always name:

- Schema and format versions.
- Command and operation identifiers.
- Result statuses and message levels.
- Diagnostic and error codes.
- Exit values.
- Lifecycle and persistence states.
- Wire-format discriminants.
- Retry, timeout, size, and count limits whose values express policy.

## Review Checks

- No protocol-significant string or number is repeated raw at use sites.
- One enum or readonly const object owns each semantic value set.
- Type unions derive from the runtime source instead of restating its members.
- Mappings use named keys and satisfy a complete typed record.
- Diagnostic codes remain local until reuse is real.
- A global constants dumping ground does not replace meaningful placement.
- Ordinary presentation copy is not buried behind unnecessary indirection.
