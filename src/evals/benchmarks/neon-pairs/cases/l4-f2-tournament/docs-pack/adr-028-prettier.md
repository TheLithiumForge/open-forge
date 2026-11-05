# ADR 028: Use Prettier for mechanical formatting

This record explains the formatter’s narrow role and how formatting checks differ from behavioral review.

## Context

The repository has TypeScript, TSX, CSS, JSON, configuration and tests. Reviewing spacing and wrapping by hand would consume attention that is better spent on state transitions, validation and browser behavior. Consistent mechanical formatting also makes small files easier to compare when a behavior moves between modules.

The decision is to use the pinned Prettier tool already declared in `package.json`. `npm run format` invokes `prettier --write .`; `npm run format:check` invokes `prettier --check .`. The first changes files and the second reports whether formatting matches. Neither command replaces compilation or tests.

## What this settles

Formatting gives the project a repeatable answer to indentation, line wrapping and common syntactic layout. A reviewer can focus on whether a switch handles the selection variants, whether a parser checks the right keys or whether a clock listener cleans up. The code does not need a personal style debate at each call site.

Readable diffs are particularly useful here because many important changes are small. Moving synchronization before an activation or changing an exact bound can alter behavior without changing much text. A formatter reduces unrelated visual noise around that change. It cannot determine whether the ordering or number is correct.

## What remains a design question

Prettier does not decide whether `useGame` has too many responsibilities, whether two tests share an independent expectation, or whether a component prop leaks hidden identity. Those concerns need architectural and behavioral review. The existing source-size aim also remains separate: wrapping can change line count, but the reason to split a file is its behavior boundary, not merely making a formatter produce fewer lines.

The project uses focused modules and direct imports. Formatting can make that structure easy to read; it cannot create the structure from a monolithic implementation. [ADR 057](adr-057-small-components.md) and [Code review guide](code-review-guide.md) cover those concerns.

## Alternatives

A hand-maintained style guide would have required reviewers to enforce mechanical choices and authors to remember them across several file types. A more elaborate linting system could have combined style and semantic rules, but that would introduce a different configuration surface. The current formatter supplies the narrow mechanical function the project actually uses.

Letting every contributor's editor choose independently was easy initially and costly in reviews. Whole-file rewrites caused by different wrapping choices can conceal the actual game change. A pinned command gives both editor integrations and command-line users a common reference.

## Operational consequences

The formatter is development tooling, not browser code. Its exact version is recorded with the other tool pins. The README lists the check command alongside the compiler, unit and browser checks. That record makes clear that formatting is one piece of verification rather than a substitute for it.

The write command is broad by design, so its diff deserves inspection when used on a working tree. A formatting-only result should be understandable as a mechanical change. A functional fix still needs the evidence appropriate to that behavior.

[Technology](technology.md), [ADR 029](adr-029-pinned-versions.md) and [ADR 055](adr-055-review-before-merge.md) are the neighboring records. The tool's value is in removing routine debate while leaving engineering judgment visible where it matters.

## The existing formatting commands

These are the manifest’s commands in context. Formatting, type checking, unit tests and browser checks are separately invocable operations.

```json
{
  "scripts": {
    "dev": "vite --host 127.0.0.1",
    "typecheck": "tsc -p tsconfig.app.json && tsc -p tsconfig.node.json && tsc -p tsconfig.e2e.json",
    "test": "vitest run",
    "build": "npm run typecheck && vite build",
    "build:art": "vite build --config vite.art.config.ts && vite build --config vite.session.config.ts",
    "test:e2e": "npm run build && npm run build:art && node --test --test-concurrency=1 \"tests/*.e2e.test.ts\"",
    "replay": "node scripts/replay.ts",
    "format": "prettier --write .",
    "format:check": "prettier --check ."
  }
}
```
