---
open-forge:
  description: Historical CLI-v2 source: Apply Bun selection, parallel-runner, workspace, and snapshot-update rules to replacement CLI tests
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Bun CLI Testing

## Instructions

- Every replacement test's full name starts with exactly one tier tag: `@unit`, `@integration`, or `@e2e`. It immediately includes at least one durable subject tag such as `@status`. Runtime cases may also include `@node`, `@bun`, or `@deno`. Do not use temporary Task tags.
- Use native Bun path filtering and `--test-name-pattern` for focused evidence.
- Test files are parallel-safe by design. Replacement parallel scripts use bare `--parallel --parallel-delay=0`, so Bun resolves the portable CPU-count file-worker ceiling and starts all resolved workers immediately. Test cases within each file remain sequential unless case concurrency is separately and explicitly proved safe.
- Replacement filesystem and process tests use the ordinary `createTestWorkspace()` helper. End-to-end evidence builds once before parallel test files run.
- `test:integration` and `test:all:prepared` ignore `update-snapshot.integration.test.ts`. Invoke that updater evidence explicitly by its exact path and serially. Snapshot updates remain explicit, one-target, serial mutations; ordinary test execution does not mutate snapshots.

The parent [Test Evidence Integrity](../../evidence-integrity.md) Directive owns runner-neutral evidence depth, isolation, mutation, and real-boundary requirements.
