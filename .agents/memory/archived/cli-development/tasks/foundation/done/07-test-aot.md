---
open-forge:
  description: Create active test projects, shared test support, local Native AOT evidence, and six-RID CI scaffolding
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Testing, NativeAOT, CI, Complete]
---

# Establish Test And Native AOT Evidence

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [CLI Foundation](../_foundation.md).
- Plan step: F5.

## Expected Outcome

Three independently runnable active test projects and one support library prove
the route-free Core, host, serialization, filesystem, process, and Native AOT
boundaries. A six-RID workflow is ready to run the same accepted project graph.

## Active Test Boundaries

- Unit: pure Shell definitions, parsing, binding, messages, stages, policies, and
  pure filesystem values. No real-boundary claim.
- Integration: Core modules, strict YAML/JSON source generation, real filesystem,
  physical containment, explicit host calls, and published AOT internal evidence.
- EndToEnd: built or published `OpenForge.Cli` process only. Prove help, version,
  invalid input, streams, exits, cancellation handling, and no workspace writes.
- TestSupport: owned temporary directory/workspace, process result, hash snapshot,
  and platform capability fixtures used identically by at least two active tests.

Every test declares a readable display name, `Feature`, and one `Evidence` trait.
Projects run independently by exact path.

## Support Safety

Temporary workspace cleanup requires positive ownership through an unpredictable
token and marker. It refuses marker mismatch, replacement, absent ownership, and
unsafe path shape. Snapshots ignore only explicitly excluded owned build output
and preserve deterministic ordering.

Process helpers drain redirected streams, kill and await owned children on
cancellation, preserve stdout/stderr separately, and never invoke a shell.

## Native AOT Evidence

1. Publish the root executable for local `win-x64` into root `/artifacts/`.
2. Execute published help, version, and invalid-input journeys.
3. Publish Integration and EndToEnd test executables through the accepted xUnit v3
   AOT runner.
4. Execute both published test binaries.
5. Run publications sequentially when they share production build output.

The CI workflow defines six native runner jobs and exact scoped path triggers. It
restores, formats, builds, runs managed projects, publishes the native executable
and AOT test executables, executes them, audits packages, and uploads bounded
artifacts. It performs no publication.

## Preserved Evidence Rules

Historical tests are candidate evidence only. Port an expectation only after
mapping it to current Architecture and contracts; do not recreate a preserved
test project or parallel fixture architecture.

## Verification

- Independent managed runs for all three active test projects.
- Warning-free Release build.
- Local published executable and AOT test runs.
- Correct informational version derived from one project source.
- No project-local output, shared mutable state, shell process, or frozen MVP use.
- Workflow formatting and path audit.

## Stop Conditions

Stop if an active test requires old production structure, if TestSupport needs a
production dependency, if managed tests pass but an AOT executable differs, or if
parallel publication produces shared-output warnings.

## Completion

Complete when the exact test and AOT boundaries can serve every later command
without project-topology changes.
