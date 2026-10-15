---
open-forge:
  description: Define protocol-significant and otherwise magic TypeScript strings and numbers once through named enums or readonly const objects
  tags: [Pattern, TypeScript, Constant, Enum, Literal, TypeSafety, Readability, Protocol]
---

# Named TypeScript Values

## Shape

Complete declarations with illustrative names and values. The enum and readonly object represent different concepts, not duplicate definitions of one concept.

```ts
export const DocumentState = {
  draft: "draft",
  ready: "ready",
  archived: "archived",
} as const;

export type DocumentState = (typeof DocumentState)[keyof typeof DocumentState];

export enum TraversalState {
  unseen = "unseen",
  visiting = "visiting",
  complete = "complete",
}

export const TraversalRank = {
  [TraversalState.unseen]: 0,
  [TraversalState.visiting]: 1,
  [TraversalState.complete]: 2,
} as const satisfies Record<TraversalState, number>;

export const FormatVersion = {
  current: 1,
} as const;
```

| Part                                                | Status                                          |
| --------------------------------------------------- | ----------------------------------------------- |
| One named runtime definition per semantic value set | Required                                        |
| Derived union instead of restated members           | Required when a union represents that value set |
| Named mapping keys and complete typed record        | Required                                        |
| Runtime values beside their contract                | Required                                        |
| Object or enum representation                       | Choice, one per concept                         |
| Example values, member names and order              | Illustrative                                    |

## Applicability And Sources

For protocol-significant or otherwise magic strings and numbers. A value is magic when its semantic or control meaning is unclear locally, must stay consistent across consumers, participates in a protocol or is risky to change by search and replacement. Ordinary one-off copy, fixture content and self-explanatory local arithmetic are not automatically magic.

A readonly const object fits JSON, file, process or library boundaries and useful object access or iteration. An enum fits a contained boundary when enum semantics read more clearly. `satisfies` retains the mapping's inferred keys and values while checking coverage.

The CLI's result statuses and exit values are defined by its
[shared operation contract](../../../memory/crystallized/documents/cli/shared-operation-contract.md).
This general TypeScript Pattern demonstrates a representation without defining
that protocol.

- [TypeScript Source Structure](../../../directives/open-forge/typescript/typescript-source-structure.md#exact-contracts-and-named-values) binds naming, mapping and placement.
- [Source Locality](../../../directives/source-locality.md) binds promotion.
