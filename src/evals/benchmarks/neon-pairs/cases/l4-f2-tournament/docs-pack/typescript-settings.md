# TypeScript settings

This page explains the four TypeScript configuration files in Neon Pairs and how they cover application code, tooling and browser journeys.

Every project extends `tsconfig.base.json`. The application, Node tooling and end-to-end projects then provide different libraries, ambient types and entry patterns. The manifest checks all three explicitly before a production build. TypeScript produces diagnostics; Vite produces the executable browser output.

## Project coverage

| Configuration | Libraries and ambient types | Entry patterns |
| --- | --- | --- |
| `tsconfig.base.json` | Shared compiler behavior | No project entry list |
| `tsconfig.app.json` | ES2023, DOM, DOM.Iterable; `vite/client`; automatic React JSX | `src/**/*.ts`, `src/**/*.tsx`, `art/**/*.tsx` |
| `tsconfig.node.json` | ES2023; Node types | Vite configs, Vitest config, scripts, colocated tests and fixtures |
| `tsconfig.e2e.json` | ES2023, DOM, DOM.Iterable; Node types | `tests/**/*.ts` |

The application project excludes `src/**/*.test.ts` and `src/**/*.fixture.ts` as initial files. The Node project explicitly includes those patterns. The browser-test project needs both Node and DOM declarations because its runner performs Node operations while its evaluated browser functions refer to elements and events.

An include or exclude list describes initial project discovery, not a universal import firewall. Imported modules can enter a compiler program through its graph. The actual source dependency direction therefore still matters. [React boundaries](react-boundaries.md) explains why the CLI and pure tests can import core behavior without importing a component tree.

## Shared configuration, exactly

The base compiler options are:

```json
{
  "target": "ES2023",
  "module": "ESNext",
  "moduleResolution": "bundler",
  "strict": true,
  "noUncheckedIndexedAccess": true,
  "exactOptionalPropertyTypes": true,
  "verbatimModuleSyntax": true,
  "erasableSyntaxOnly": true,
  "noEmit": true,
  "noUnusedLocals": true,
  "noUnusedParameters": true,
  "allowImportingTsExtensions": true,
  "skipLibCheck": false
}
```

These settings work together. The project uses static ESM and explicit TypeScript extensions, and the compiler does not try to emit those imports as a standalone runnable JavaScript tree. Browser output comes from Vite. The replay script runs through the documented Node environment instead of consuming compiler-generated files.

## Target and module resolution

`target: ES2023` is shared across projects, while Vite's build configs also target ES2023. The compiler's setting affects the language assumptions used while checking, and the bundler setting affects its output transformation. One does not silently replace the other.

`module: ESNext` preserves the ESM shape expected by the source. The package manifest declares `type: module`, and imports name the files that own behaviors. `moduleResolution: bundler` matches the browser-oriented import graph rather than asking TypeScript to model a CommonJS application.

The source audit separately rejects dynamic import and require patterns in authored TypeScript. Compiler configuration and textual conventions therefore cover different aspects of the module style. A resolved import can still violate a source convention, and a convention-compliant import can still fail to resolve.

## Strictness and unknown input

`strict: true` enables the compiler's strict family of checks. The code makes particular use of narrowing at serialized boundaries. `parseJson` returns `unknown`, `isJsonObject` narrows object shape, and validators examine fields before constructing typed state.

The snapshot decoder does not cast parsed JSON to `GameState`. It validates the code, creates an initial game, validates matched positions and selection against that deck and checks numerical consistency. This is a useful example of strictness influencing program shape: a type annotation cannot make external text valid.

The storage reader follows the same approach with a different schema. `bestMoves` is inspected entry by entry, and failure returns a default preference object. Replay returns a structured problem rather than pretending a malformed action already has the expected interface. [Purity guide](purity-guide.md) discusses these boundaries as ordinary data functions.

## Checked indexing

`noUncheckedIndexedAccess` makes an indexed result potentially undefined unless the type or narrowing establishes otherwise. The code consequently checks values that a human might expect to exist from a loop invariant.

In `deck.ts`, both swap positions are read and checked before assignment. In `face-design.ts`, the selected shape, marker, center and colors are checked before constructing the design. In `fresh-board.ts`, the single buffer element is checked after the browser fills a one-element typed array.

These guards are not extra game variants or user errors. They make indexed assumptions explicit under the compiler setting. The deck's out-of-range error and face-design's invalid-face error identify an internal inconsistency rather than silently returning an incomplete design.

The tests also narrow array reads and unknown fixture objects. Test code is not outside the compiler policy simply because it can fail loudly at runtime. It is included in the Node and end-to-end projects and scanned by source audits.

## Exact optional properties

`exactOptionalPropertyTypes` distinguishes an omitted optional field from explicitly providing undefined unless the property type includes undefined. The replay result uses an optional action position on a problem result: general failures omit it, while action-specific failures include a number.

A caller checks `result.action === undefined` before formatting an action-specific error. That branch handles absence without requiring every problem to carry a placeholder action. The shape remains useful when the CLI maps a zero-based action position to a one-based human message.

The status component illustrates a related but different declaration. Its `best` prop is `number | undefined`, not an optional prop. `Game` supplies the indexed best value and the component renders `none` when it is undefined. This makes the expected absence explicit in the prop contract.

## Type imports stay visible

`verbatimModuleSyntax` encourages the source to say which imports are types. Modules use dedicated `import type` declarations and, in some places, inline `type` markers. `Board` imports `GameState`, CSS property types and its ref type without turning those into intended runtime values.

The selection module exports a runtime constant object plus a union type derived from its values. Consumers can import `SelectionKind` for exhaustive branching and `Selection` for annotations. This keeps the actual discriminant values and their type representation aligned.

Direct imports also help the source convention test identify owners. That test recognizes specific declaration and import forms through regular expressions; it is not the TypeScript checker. A declaration can be valid TypeScript while falling outside what the convention test notices. [Source audit notes](source-audit-notes.md) records those limits.

## Erasable syntax

`erasableSyntaxOnly` is enabled in the base. The source uses interfaces, type aliases, type imports, annotations and const assertions rather than relying on TypeScript declarations that require an additional runtime construct.

`SelectionKind` and `ReplayProblem` are ordinary constant objects with `as const`; their unions are derived from their values. Those objects exist at runtime because the code explicitly creates them, and their type-level precision disappears without needing a separate enum representation.

The source audit's unsafe-cast pattern allows the lowercase const assertion used here while rejecting selected assertion forms. It would be inaccurate to describe the repository as forbidding the token `as` everywhere. The exact pattern and the actual uses matter.

## Unused declarations and public surface

`noUnusedLocals` and `noUnusedParameters` catch unused declarations within the compiler's understanding of the program. Exported declarations are a different case: a compiler can preserve an exported surface even when this repository has no consumer for it.

The convention suite adds a textual check that every recognized export name has a recognized named importer. It also checks single ownership for recognized exported constants. This complements unused-local diagnostics by discouraging speculative public APIs in the application tree.

The two checks are not interchangeable. A name found in an unrelated named import can satisfy the textual set membership, and an unrecognized export syntax may not be scanned. TypeScript resolves real symbols, while the convention test enforces a small repository convention through a simpler model.

## Library declarations remain checked

`skipLibCheck` is false. The compiler does not skip declaration-file checking merely to make the project pass more quickly. The reviewed package set includes both React and Node declarations, whose versions are pinned alongside the tools.

The application project sets `types: ["vite/client"]`; Node and end-to-end projects set `types: ["node"]`. This controls automatically included ambient type packages. It does not prevent declarations reached through ordinary imports from participating in a project.

Separating libraries helps catch accidental environment coupling. The Node project does not declare the DOM library, while end-to-end support deliberately does because it includes browser-evaluated code. That design is stronger than compiling every file with every possible global available and hoping imports stay disciplined.

## What typecheck does not establish

`noEmit` leaves executable output to Vite, whose transformation is not the strictness check. Browser behavior and built-file delivery require separate evidence. Source audits also inspect suppression markers and unsafe type patterns across authored tests and diagnostics. [Source audit notes](source-audit-notes.md) records their exact coverage.

## Interpreting a compiler failure

An application diagnostic commonly concerns component props, DOM events or game data consumed by the browser entry. A Node-project diagnostic can concern the CLI, source readers, build configs or unit tests. An end-to-end diagnostic can concern Playwright values, Node runner support or code executed in the page.

Imported core files can appear in more than one project's graph. A failure may therefore reveal a real environment assumption rather than a duplicated source file. `saved-format.ts`, for example, uses JSON and text encoding without reading the DOM, allowing the same validators to serve both browser and Node clients.

The exact command order is in [Build pipeline](build-pipeline.md). For the rationale behind the language choice see [TypeScript decision](adr-002-typescript.md); for how compiler evidence fits other checks see [Test policy](test-policy.md).
