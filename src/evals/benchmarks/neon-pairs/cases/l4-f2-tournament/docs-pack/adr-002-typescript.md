# ADR 002: Use TypeScript at the boundaries

This record explains the strict TypeScript model and why parsed data still needs runtime validation.

## The problem being solved

The game has a small number of meaningful states and several ways for invalid data to arrive. A selection can be closed, contain one index, or contain a mismatching pair with time remaining. A saved preference or snapshot arrives as JSON whose contents cannot be trusted merely because the project compiles. TypeScript is useful for the internal alternatives; explicit validation is useful for the external boundary. The decision uses both.

`tsconfig.base.json` enables strict checking, indexed-access checks, exact optional properties, verbatim module syntax and erasable syntax. These settings matter in this code rather than merely decorating a template. Reading `deck[index]` can produce `undefined`, so generation checks both values before swapping. A replay diagnostic can include an action position or omit it; consumers narrow the result before using either its state or problem.

## Decision

Model game data with readonly interfaces and named discriminated unions. `SelectionKind` defines the string values and `Selection` connects each value to its associated fields. The switch in `activateCard` can therefore use `index` only in the one-card branch. Its unreachable branch accepts `never`, making an unhandled variant a useful compiler problem.

Runtime JSON helpers return `unknown` until checks establish an object and its members. `decodeSnapshot` rebuilds the board from a validated code, reads matched positions and selection, then checks the relationships between progress fields. A type annotation cannot establish that two matched positions actually have the same face. [Snapshot format](snapshot-format.md) covers those relationships; this record explains why the compiler and parser have different jobs.

## Separate environments

The project has app, Node and browser-test compiler configurations. The app includes DOM libraries and React JSX; the Node project includes scripts, fixtures and unit tests; the end-to-end project includes its browser automation code. `npm run typecheck` invokes all three. Vite transpilation is separate from that command and does not replace it.

This separation gives errors a useful context. A filesystem dependency belongs in replay's command wrapper, not in a browser rule function. Browser automation legitimately understands both a page and Node facilities for writing artifacts. Treating everything as one large ambient environment would make those boundaries easier to cross accidentally.

## Alternatives and consequences

Plain JavaScript with tests could cover the observable game. It would leave routine mistakes about selection variants and optional diagnostic fields to execution. A looser TypeScript setup would catch some spelling errors while allowing unchecked array reads and ambiguous optional fields. Neither alternative expressed the state machine as directly as the chosen setup.

The cost is explicit narrowing and occasional defensive checks around table lookups. `faceDesign` checks table results after validating the face ID, even though the arithmetic makes those entries available. That code looks slightly more deliberate than an unchecked access and has the advantage that invalid assumptions fail near their source.

A successfully typed snapshot does not authenticate play history. Validation checks a coherent state, not a signed account record. The emitted distribution is browser JavaScript inside HTML. [TypeScript settings](typescript-settings.md), [ADR 020](adr-020-vitest.md) and [ADR 003](adr-003-single-file.md) cover those adjacent concerns.

## The selection union

The three alternatives carry different data. This is the central example of making an impossible internal combination difficult to express.

```ts
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
```
