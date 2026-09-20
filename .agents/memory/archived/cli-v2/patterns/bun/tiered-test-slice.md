---
open-forge:
  description: "Historical CLI-v2 source: Place direct, focused integration, and built-CLI snapshot evidence at the scope of the behavior each tier proves"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Tiered Test Slice

## Applicability

This Pattern applies only to replacement CLI source and its direct,
integration, and end-to-end evidence. The repository [development
guide](../../../../../docs/development.md#frozen-mvp-closure-practice) owns the
frozen MVP's operational test tiers until that source is retired.

The runner-neutral [Evidence Tiers](../../../testing/evidence-tiers.md) Pattern
owns the common direct, integration, end-to-end, locality, and snapshot
boundaries. This Bun specialization owns the replacement CLI's file names,
runner commands, tag grammar, worker behavior, fixture helpers, and process
snapshot schema.

## Shape

Use increasing evidence depth without moving focused tests away from their behavior:

```text
src/cli/
  commands/
    status/
      status.ts
      __tests__/
        status.test.ts
        status-workspace.integration.test.ts
  e2e/
    status.e2e.test.ts
    __snapshots__/
      status.e2e.test.ts.snap
```

- `*.test.ts` calls a focused function or named handler directly with typed inputs and explicit dependencies.
- `*.integration.test.ts` crosses one selected real boundary such as filesystem, Git, process, or package layout.
- `*.e2e.test.ts` spawns the built CLI in an operating-system temporary workspace and proves a complete critical journey.
- Bun's `toMatchSnapshot()` records either one focused typed value/state
  projection or the minimal built-process record.

Every tier retains the final `.test.ts` suffix so Bun, editors, and generic
test tooling discover it normally. `integration` and `e2e` refine the evidence
depth; they do not replace the common test identity. Do not shorten these to
`*.integration.ts` or `*.e2e.ts`.

One production subject expected to remain a one-test subject keeps that test
adjacent. When a subject has or is expected to need multiple files, create the
nearest `__tests__/` directory from its first test. Split filenames by
production behavior, not into `unit/`, `integration/`, or other tier
directories; the suffix and runner already own evidence depth.

## Script Surface

```text
build:assets       generate .temp/cli/embedded-assets.generated.ts
build              generate assets, then build the candidate
typecheck          validate an already-prepared workspace
lint               validate an already-prepared workspace
check:fast         typecheck, then lint
test               native direct-tier selection
test:integration   native integration-tier selection
test:e2e           build once, then native end-to-end selection
test:all           build once, then all native replacement tests
check              format check, build once, static checks, then complete native tests
test:snapshot:update -- <exact-test-file>
                    update one direct, integration, or end-to-end snapshot file
```

Keep `test` fast and `check` complete. Native Bun paths and
`--test-name-pattern` select focused evidence. Do not add overlapping aliases
such as `test:unit`, `test:command`, or `test:ci`.

Every parallel selection uses bare `--parallel --parallel-delay=0`. This shape
lets Bun resolve the portable CPU-count file-worker ceiling and starts all
resolved workers immediately. Build-bearing scripts still build once before
delegating to the prepared parallel selection. `test:integration` and
`test:all:prepared` exclude `update-snapshot.integration.test.ts`; run that
updater evidence explicitly by its exact path and serially.

The snapshot-update action requires exactly one existing replacement test file,
rejects directories, multiple paths, and files outside the suite, derives the
tier from its complete suffix, and builds first only for an end-to-end target.
Do not provide an update-all shortcut.

## Selection And Isolation

Every replacement test full name starts with exactly one tier tag: `@unit`,
`@integration`, or `@e2e`. It includes at least one durable subject tag such as
`@status`. Runtime cases may add `@node`, `@bun`, or `@deno`. Do not add
temporary Task tags.

Test files run across Bun's resolved worker processes, while cases within each
file remain sequential. Enable case concurrency only as a separate change after
its isolation is explicitly proved. Every mutable workspace, home, temporary
directory, cache, build output, and port belongs to one test or one invocation.
A parallel end-to-end file may own one unique package-manager environment and
cache: they are isolated between file workers and shared only by that file's
sequential cases. Shared state may be read-only. Replacement filesystem and
process tests use `createTestWorkspace()`. End-to-end evidence builds once
before parallel files run. Snapshot updates are explicit one-target serial
mutations.

Keep a command's direct and focused integration evidence in its folder or local
`__tests__/` directory according to the multi-test rule. Keep whole-executable
journeys at CLI scope because they verify dispatch, packaging, runtime,
rendering, exit status, and multiple capabilities together.

Test support begins beside its only consumer. Promote temporary-workspace, process-execution, Git, normalization, or fixture capabilities only to the nearest common ancestor of their actual test consumers.

Prefer real functions, filesystems, Git repositories, processes, and resulting state. A test double must close a boundary that cannot be exercised safely and deterministically with the real implementation.

## Test Values And Fixtures

Tests import authoritative production named values for every protocol or
control concept. Direct and integration tests do not restate those strings or
numbers in inputs or expectations.

Keep arbitrary test-only values in named deterministic fixtures rather than
repeating them across tests. A fixture may provide a focused builder with
explicit overrides when several cases share one valid baseline. Keep it beside
one consumer and promote it only when demonstrated consumers share the same
meaning. Test fixtures must not introduce nondeterministic random data.

Evidence depth does not create a fixture boundary. Direct, integration, and
end-to-end tests may share one fixture when they genuinely use the same
semantic data; place it at their nearest common test-support scope. Keep
runtime launchers, operating-system workspaces, and other depth-specific setup
inside the deepest tier that actually needs them.

An inventory test may assert the exact wire value of the production definition
whose stability it proves. A built end-to-end interoperability assertion may
likewise state the expected serialized wire value at the process boundary.
Keep either exception scoped to that named contract assertion; it does not
permit raw protocol values in ordinary behavioral setup or expectations.

## Snapshot Boundaries

Direct and unit tests snapshot the returned typed value only when its complete
review is clearer than several assertions. An integration test may define one
small local projection containing its typed result and selected real-boundary
evidence:

```ts
const snapshot: RebuildIntegrationSnapshot = {
  result,
  files: await readSelectedTextFiles(expectedPaths),
};

expect(snapshot).toMatchSnapshot();
```

Do not create a universal value wrapper or repeat inputs already visible in the
test.

Every ordinary end-to-end snapshot has exactly three fields:

```ts
export interface CliProcessSnapshot {
  readonly exitCode: number;
  readonly stdout: string;
  readonly stderr: string;
}
```

Runtime, arguments, fixtures, workspace trees, timings, identifiers, and file
metadata do not enter that schema. Verify mutation state separately with
focused projections and explicit assertions. Normalize only stream CRLF and
the exact known temporary root; never reserialize JSON or broadly scrub text.
Interruption tests use explicit termination assertions.

Use focused semantic assertions when a safety invariant could disappear inside
a snapshot diff. The focused update action is the only repository script that
invokes `bun test --update-snapshots`.

## Review Checks

- The cheapest tier proves each behavior that does not require a deeper boundary.
- Subjects expected to remain one-test use adjacency; subjects expected to need multiple tests start with one local `__tests__/` directory without tier subdirectories.
- Integration tests identify the one real boundary they cross.
- End-to-end tests use the built artifact and isolated OS-temporary workspaces.
- Value snapshots stay local to one returned value or selected boundary state.
- Process snapshots contain only exit code, stdout, and stderr.
- Stream normalization preserves public formatting changes.
- Critical preservation, containment, rollback, and no-partial-write invariants have explicit assertions.
- Shared test support follows the same locality rule as production support.
- Direct and integration behavior tests import production named values instead
  of restating protocol or control literals.
- Reused arbitrary data comes from deterministic named fixtures at the nearest
  shared scope.
- Raw wire values appear only in deliberately scoped inventory or built-process
  interoperability assertions.
