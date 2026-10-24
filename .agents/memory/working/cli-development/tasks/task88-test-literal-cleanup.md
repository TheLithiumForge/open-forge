---
open-forge:
  description: Open Task 88 to remove the older magic strings, literal-string assertions and tier problems from the CLI tests, which the Task 86 audit counted but did not fix
  tags: [Memory, Working, Task, CLI, Testing, Evidence, Contextual]
---

# Task 88: Test literal cleanup

Status: Open since 2026-10-24, from the maintainer's rule that tests check literal strings only through snapshots. Priority is the maintainer's to choose.

## Outcome

The CLI tests follow `.agents/directives/open-forge/testing/evidence-integrity.md` and the one-owner rule in `.agents/directives/csharp/design.md` everywhere, not only in new code:

- Every value with one owner (folder and file names, finding codes, environment variables, JSON field names, layout segments) comes from that owner.
- No test asserts user-facing text with a literal, substring or pattern. Exact text lives in reviewed snapshots.
- Unit tests stay in memory, Integration tests cross one real boundary, and EndToEnd tests drive the published executable.

## Scope

The [test literal audit](../../../emerging/analysis/test-literal-audit.md) counted 2,141 magic-string locations, 173 literal-string assertions and 9 tier problems in the test files the Task 86 branch touched. Tests that branch did not touch were not audited, so the whole suite needs one more pass first.

## Approach

- Audit the remaining test files with the same three rules, read only.
- Group the fixes by test area, one packet per area with no shared files, and keep each packet's suites green.
- EndToEnd and test support cannot reference production code. Use the tracked build-time projection of owned values that Task 85 introduces, instead of copied constants.
