---
open-forge:
  description: Consider suite-end cleanup of test-owned workspaces and external recovery data after current CLI work
  tags: [Memory, Idea, Contextual, Candidate, CLI, Testing, Cleanup]
---

# Test Environment Cleanup

## Motivation

The user proposed an after-all cleanup for environments created by tests during
storage investigation, then deferred implementation on 2026-09-07 after build
outputs proved to be the larger removable category. This is an idea only; it
adds no active task, milestone, or implementation requirement.

The investigation found about 27.35 GiB of regenerable build outputs, which the
user authorized removing. Separate retained recovery data occupied about
1.1 GiB: all 130 inspected archive manifests identified temporary Open Forge
test workspaces. Recovery data was not removed. Build-output housekeeping and
test-environment lifetime solve different problems.

## Candidate Direction

[TemporaryWorkspace](../../../../src/cli/tests/support/OpenForge.Cli.TestSupport/TemporaryWorkspace.cs)
creates marker-owned directories in the operating-system temporary directory
and already supports per-instance disposal. A later investigation could add
suite-end cleanup as a backstop for test-owned residual environments and
external recovery data created for those exact workspaces.

Use the pinned test framework's lifecycle support and keep cleanup scoped to
the current run's positively identified resources. Preserve concurrent runs,
unrelated temporary files, real workspace recovery evidence, ownership-marker
checks, and no-follow handling. Do not sweep the global temporary or recovery
root. Inspect normal completion, failed tests, and interrupted-run limitations
before selecting the smallest mechanism.

This candidate does not authorize automatic artifact deletion, broad fixture
refactoring, a production dependency, or changes to command behavior.
