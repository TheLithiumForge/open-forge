---
open-forge:
  description: Implement explicit repair planning, dry run, application, verification, and recovery
  tags: [Memory, Working, CLI, Task, Repair, Mutation, Recovery, Contextual]
---

# Task 19: Repair

## Task State

- State: Active at phase 4 of 5, milestone 3 of 8. Preflight, Gray, Red, and the
  upstream merge/refreeze gate are accepted. Coherent Green is current and
  explicitly authorized; milestone 4 remains incomplete until Green acceptance.
- Permanent mapping: Task 19 “Repair” in the
  [project control ledger](../../project-control.md).
- Queue relation: Task 18 “Extension Remove” is complete and integrated. Task
  20 “Cleanup” has accepted Red but holds Green until Task 19 is accepted and
  integrated.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/repair/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/repair/behavior.md).

The exact immutable preparation base is commit
`76e8e5f1f58e60a9de159318d10e7b3e9f8fc9c9`, tree
`ed698b992b68f76b037b4567d154eadbc013239b`, with direct parent
`5cabb10de31cc522e80e82f1ac2ada49e60929c0`. Relative to its parent, the
candidate changes only non-executable planning and authority records. It
includes Task 18 activation and contains no Task 19 implementation. At that
base, Task 19 was queued/prepared and had no implementation or activation
mutation.

## Expected Outcome

`repair` converts accepted repairable Doctor findings into an explicit
reviewable plan, applies only authorized repairs under mutation safeguards,
verifies results, and preserves exact recovery for incomplete work.

## Outcome And Profile

The command applies only fresh diagnosis-backed, conflict-free,
meaning-preserving local-reference corrections and explicitly selected
contained relinks. It does not promise to eliminate every Doctor finding.

The selected profile is streamlined assured. Its five phases are:

1. Preflight and activation.
2. Gray callable and public shape.
3. Red evidence.
4. One coherent Green and focused verification under one Brilliant
   Implementer.
5. Fresh whole-task review, at most one grouped correction, final acceptance,
   and integration.

The eight milestones are: 1 Preflight and preparation; 2 Gray; 3 Red; 4
coherent Green; 5 focused, public, full managed, and supported `linux-x64`
Native AOT verification; 6 fresh whole-task review (`T19-R1`); 7 grouped
correction or documented no-op (`T19-C1`); and 8 acceptance. There is no
council. Exactly one holistic review ID, `T19-R1`, and one correction ID,
`T19-C1`, are reserved; neither is consumed. The current owner remains Task
Mastermind Kepler II. Hypatia II owned only the accepted Gray implementation
root. Emmy II and Faraday II owned the accepted disjoint Red boundaries.
Brilliant Implementer Curie IV owns coherent Green, focused verification, and
the later grouped correction pass if `T19-C1` is required.

## Activation And Accepted Architecture

The accepted authority fingerprints are:

- Repair Interface: `92ea7c6a888149f22a4e473aac492ba042fd249368f16651704f6968239b453b`.
- Repair Behavior: `7a33b333f78a822092fdbe39ef0e9c00c76b18daa4814130e7996021cb08bb89`.
- C# Directive `.agents/directives/csharp/_csharp.md`:
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`.
- C# design Directive `.agents/directives/csharp/design.md`:
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`.
- C# style Directive `.agents/directives/csharp/style.md`:
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.

The public syntax is exactly:

```text
open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global flags]
```

The first-release catalogue contains only same-target canonical path, case,
and encoding corrections; a unique canonical fragment correction; and an
explicitly selected missing-target relink from bounded Doctor candidates.
Repair never repairs generated navigation, route topology, metadata,
Framework or Extension lifecycle, ownership, recovery artifacts, prose,
external references, or arbitrary links. Automatic selection admits only
safe-exact effects. A guided candidate remains unselected until the wizard or
an exact `--relink` supplies user intent.

Repair consumes fresh producer-owned Doctor and local-reference views,
including canonicalizations and bounded candidates. It reuses source sessions,
exact source locations, snapshots, destination resolution,
`PlannedFileChange.Replace`, preflight and revalidation,
`WorkspaceLockManager`, `RecoveryBundleStore`, `FileChangeApplier`, native
interaction, and command-local source-generated JSON. All Repair semantics stay
local: request normalization, selection and catalogue formation, exact relink
resolution, effect coalescing and conflict handling, plan formation, byte
edits, recovery mapping, application orchestration, post-diagnosis, result
formation, and rendering.

Do not import another command's private `Shared/**` implementation or add a
generic repair engine, dependency injection, a service locator, a runtime
registry, reflection, JavaScript/MJS/CJS, or compatibility machinery. Prohibit
unauthorized destructive repository, worktree, or source operations and
out-of-plan destructive effects. Accepted in-plan `Replace` effects remain
allowed when their recovery and safety gates pass. Remote, release, and
publication actions remain prohibited. Every planned `Replace` has external
recovery. A Doctor canonicalization proposal marked `NoPersistentState` does
not waive Repair's recovery requirement.

The provisional `codex/repair-provisional` commits `e6b906fd`, `a90a8007`, and
`0521b278` are read-only historical evidence from stale base `0d269b7a`.
They contain useful finite-catalogue and deduplication ideas, but duplicate
`FileStateSnapshot` as `RepairFileState` and omit the required diagnosis,
recovery, result, composition, public, and Native AOT boundaries. They are not
transplant units.

## Architecture

- Doctor findings remain observation input. `RepairPlanner` maps only recognized
  repair codes and complete provenance to command-local `RepairPlan` steps.
- Each repair step names target, expected state, intended effect, verification,
  dependency, and recovery boundary.
- Reuse shared mutation primitives and producer-owned repair capabilities. Do not
  duplicate install, update, index, or Extension behavior inside Repair.
- Dry run forms the complete plan and stops before lock/effects.
- Keep request normalization, catalogue and selection policy, relink resolution,
  coalescing and conflicts, plan/effect formation, recovery attribution,
  application orchestration, post-diagnosis, result, and rendering under Repair.
- Consume fresh producer-owned Doctor/local-reference views and reuse the
  accepted source-session, snapshot, destination, preflight, lock, recovery,
  applier, interaction, and source-generated JSON capabilities.

## Accepted Gray Freeze

The cumulative Gray boundary is accepted at immutable tip commit
`140920d3116fe0744bac933c77041f22107c8ce7`, tree
`4060c945011b41f17822baf1c4f2aeb535069112`. Its immutable sequence is
initial Gray `2f660f52`, followed by corrections `2597c413`, `9ebbc064`,
`4d25c287`, and `140920d3`.

The accepted Gray root and inventory contain exactly 20 paths under
`src/cli/core/OpenForge.Cli.Core/Commands/Repair/**`. Its exact root/inventory
hash is `b00ec856f633289a1a916472e93844eeccb15b20c01ab29d69e5463493a758a3`.
The boundary freezes the command-local definitions and options, binding and
request/relink grammar, selection and finite catalogue, plan/effect/no-op/
conflict models, result, command-local JSON and presentation, and the
explicitly failing operation and binding skeleton. It does not change root or
static composition, Doctor, Framework, Shell, projects or configuration,
TestSupport, shared JSON, tests, or another command.

The accepted corrections tighten option metadata, selected-proposal occurrence
identity and non-overlap, step-to-selection coverage, outcome/recovery
coherence, and blocked/no-op projection. They also enforce typed result-status
and recovery/residual coherence, normalized residual paths, cleanup next-action
selection for retained recovery, and lifecycle precedence that places
`Interrupted` after `Incomplete`.

Red opens only in the already-frozen disjoint Unit and combined
Integration/EndToEnd lanes described in the Execution Capsule below.

## Accepted Red Freeze

The cumulative Red boundary is accepted at immutable tip commit
`af59c957e5b74550731554f6195e1330de98ece9`, tree
`903ddc56e487e2976ed0019b8a5973c09c3242a6`. Unit evidence is commit
`5bf5f690c4ce3ebfbc40f18a9cdd9fef5d4f2379`, tree
`9e3041563ca1e2481835c1cae49b9eca1ea88a7d`, over Red activation parent
`cd4ce0899040c06836a6061b3c65afc55d4ad61d`. Integration and EndToEnd
evidence are the accepted tip over that Unit commit.

The exact 12-path Red inventory contains eight Unit files under
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Repair/**`, three
Integration files under
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/**`,
and
`src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs`.
Its sorted path-manifest SHA-256 is
`1a058269d30e95c7e405d1fb8c8f7baf2c16397bb21d3ad91d565c64ae4ed638`, and
its path-plus-content manifest SHA-256 is
`e36ea642fe03c0effb16567e1c9c3da5e568005a2e9d8c1408c8839374878741`.

Fresh warning-free Release compiles preceded the exact no-build selections.
Unit selected and executed 51 cases: 49 passed and exactly two failed only at
the deferred `RepairPlanner.Build` seam. Integration selected and executed six
cases, all failing only at the deferred `RepairOperation.ExecuteAsync` seam.
EndToEnd selected and executed exactly three Repair journeys, all reaching the
expected missing-composition boundary. Every selection had zero skips. The
three existing Doctor EndToEnd journeys remain unchanged. Formatting,
targeted analyzers, diff, expected/protected-path, callable/count,
prohibited-pattern, machine-path, changed-line, and 200-character line checks
passed.

Fresh combined C#, behavior-contract, and test-evidence acceptance review
`T19-RED-ACC-01` passed without findings on the immutable tip. It confirmed
that the evidence is Open Forge-owned, non-tautological, and free of setup
failures at the intended Red boundaries. This phase-boundary review consumes
neither `T19-R1` nor `T19-C1`. Milestone 3 is complete. The accepted upstream
refreeze below satisfied the remaining Green gate, and the Overseer explicitly
authorized coherent Green.

## Accepted Upstream Refreeze

The integrated Task 18 baseline was merged without rebasing at commit
`f043751247d356c458ccef8efddd4cc3c7d6f528`, tree
`25d88102be8456c0004b2a039f690145960e7c3a`, with parents `fb8ce672` and
`2c62f59a`. The 20-path Gray manifest remains
`b00ec856f633289a1a916472e93844eeccb15b20c01ab29d69e5463493a758a3`, the
12-path Red manifest remains
`1a058269d30e95c7e405d1fb8c8f7baf2c16397bb21d3ad91d565c64ae4ed638`, and
the combined 33-path manifest remains
`10a3afd96c085cbb8da6b7b608a29f59d3d4a29aaebaac14d319518f06a3fd56`.
Every frozen Repair blob is unchanged.

Locked restore and a fresh non-incremental Release solution build passed with
zero warnings and errors. Exact no-build refreeze selected 51 Unit cases with
49 passing and two failing only at `RepairPlanner.Build`; six Integration cases
failed only at `RepairOperation.ExecuteAsync`; and exactly three Repair public
cases failed only at the missing-composition boundary. The retained three
Doctor public cases passed. Every selection had zero skips. Formatting,
static, protected-path, callable-shape, prohibited-pattern, machine-path,
changed-line, and line-length checks passed. The public-help invalidation from
Task 18 is therefore discharged for the frozen Red boundary.

## Execution Capsule

This capsule is frozen at Preflight. It records the accepted boundaries for
activation, Gray, Red, Green, evidence, review, correction, and acceptance.

### Gray Boundary

The exact isolated Gray mutation root from the preparation base is only
`src/cli/core/OpenForge.Cli.Core/Commands/Repair/**`. Gray freezes definitions
and options, binding/request/relink grammar, selection, the finite catalogue,
plan/effect/no-op/conflict models, result, command-local JSON and presentation,
and an explicitly failing operation and binding skeleton.

Gray must not touch root or static composition, Doctor, Framework, Shell,
projects or configuration, TestSupport, shared JSON, task or control records,
or any other command. Overseer acceptance of this frozen Preflight packet is
required before Gray.

### Red Boundary

After accepted immutable Gray, Red opens only in the already-frozen disjoint
Unit and combined Integration/EndToEnd lanes from that Gray snapshot:

- Unit only:
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Repair/**`.
- Combined Integration/EndToEnd only:
  - Integration:
    `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Repair/**`.
  - EndToEnd: the new exact file
    `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs`.

Fixtures remain lane-local. No shared TestSupport change is allowed unless a
later accepted finding proves it necessary.

The public Repair evidence retains exactly three simple journeys:

1. `repair --help` succeeds without workspace inspection or writes.
2. `repair --automatic --dry-run --json` previews one safe-exact correction,
   leaves a guided candidate unselected, and creates no workspace, lock,
   recovery, or temporary effect.
3. One explicit `--relink` applies one contained missing-target correction,
   preserves the label and unrelated bytes, deletes the successfully handled
   recovery bundle, and an automatic rerun converges to a verified no-op.

Retain exactly three existing `PublishedDoctorProcessTests` journeys.

### Focused Evidence

After Red is frozen, Unit and Integration use exact project selection with a
nonzero Red-frozen count:

```text
dotnet test --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --filter-trait "Feature=repair" --minimum-expected-tests <Red-frozen-count>
dotnet test --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --filter-trait "Feature=repair" --minimum-expected-tests <Red-frozen-count>
```

Repair and retained Doctor public evidence use the EndToEnd project and exact
class filters:

```text
dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedRepairProcessTests" --minimum-expected-tests 3
dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedDoctorProcessTests" --minimum-expected-tests 3
```

Zero-test, stale `--no-build`, skipped, warning-bearing, partially loaded, or
wrong-scope receipts do not pass a gate. Focused evidence proves Repair-owned
grammar, selection, catalogue, planning, conflicts, dry-run, byte preservation,
revalidation, recovery, application, verification, no-op convergence, results,
presentation, and public reachability at the cheapest decisive boundary.

### Full Acceptance Gate

After Green, and again whenever that evidence is invalidated, acceptance
requires locked restore; a warning-free Release solution build; direct managed
Unit, Integration, and EndToEnd execution; supported `linux-x64` native root,
Integration, and EndToEnd publish and execution; managed EndToEnd execution
against the same native root; and formatting, diff, static, protected-path,
callable-shape, prohibited-pattern, machine-path, changed-line and line-length,
no-JavaScript/MJS/CJS, no-public-command-subprocess, and no-direct-file-effect
checks. Use only artifacts from the same worktree. Do not publish packages.

The full project paths are:

- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj`.
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj`.
- `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj`.

### Upstream-Green Gate

Task 18 is accepted and integrated into `develop`. Its baseline was merged, not
rebased, into the Repair lane and refrozen at the accepted commit above. The
completed gate required:

1. Prove that the preparation base commit
   `76e8e5f1f58e60a9de159318d10e7b3e9f8fc9c9` is an ancestor.
2. Prove zero intersection between the final Task 18 delta and the frozen Gray
   and Red paths.
3. Reconcile the integrated static root while leaving only Repair composition
   deferred to coherent Green.
4. Rebuild Gray, rerun Red, and prove the intended missing-behavior failures.
5. Freeze the new base before coherent Green.

All five requirements passed. Root/static Repair composition and any necessary
narrow directly callable Doctor diagnosis reader now belong to Curie IV's
authorized Green boundary. The same Brilliant Implementer owns focused
verification and the later grouped correction pass.

### Invalidation Rules

Return to the earliest affected Gray or Red boundary if Task 18 changes any of
these paths or meanings:

- `src/cli/core/OpenForge.Cli.Core/Framework/Sources/Operational/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Sources/References/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Mutation/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Recovery/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Interaction/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Parsing/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/**`.

Also invalidate on a shared result or serialization contract change; Repair
contracts; C# or CLI Directives; test-project or TestSupport change;
public help or parser convention change; `repair/repair/workspace` recovery
attribution change; Doctor change beyond the agreed one-kind removal; a
non-ancestor final baseline; any frozen-path overlap; or a merge conflict in a
frozen path. The repeated
`src/cli/core/OpenForge.Cli.Core/` prefix is written explicitly above so each
protected path is unambiguous.

## Expected, Protected, And Integration Paths

Expected production mutation is limited to
`src/cli/core/OpenForge.Cli.Core/Commands/Repair/**` during Gray. Later
directly required composition and producer-reader neighbors remain deferred to
the accepted upstream-Green gate. Red is limited to the exact Unit,
Integration, and EndToEnd paths above.

Protected meaning includes all Task 18 mutable authority, other commands'
private `Shared/**`, Framework lifecycle semantics and files, public shared
result coordinates, dependency/platform/project/build/package/release and
generated-source authority, legacy CLI and npm paths, and every target outside
a complete accepted Repair plan. No neighboring expansion is allowed without
Task Mastermind acceptance. Shared or public contract changes require an
Overseer decision.

## Evidence

Cover no repairs, one/many independent and dependent repairs, unrepairable and
unavailable findings, stale Doctor facts, lock/revalidation race, dry run,
confirmation/write policy, partial failure at each step, recovery, post-repair
Doctor outcome, idempotence, preservation, process, and AOT.

The accepted recovery contract requires every existing-target `Replace` to have
one immutable, externally prepared and verified recovery bundle before the
first effect. A verified no-op and a dry run create no recovery or temporary
effect. Recovery disposition, residual paths, exact byte preservation, and
fresh relevant-domain post-diagnosis remain visible in the typed result.

Run the focused commands only after Red freezes nonzero counts. Full acceptance
then repeats locked restore, warning-free Release build, direct managed and
supported `linux-x64` Native AOT execution, managed-on-native EndToEnd, and all
formatting, static, protected-path, callable-shape, prohibited-pattern,
machine-path, line-length, and no-direct-effect checks from the frozen capsule.

## Stop Conditions

Stop before repairing a finding without complete provenance, executing free-form
instructions, hiding producer behavior, claiming all findings repairable, or
continuing after a dependency-invalidating failure.

Also stop before Task 18 integration for semantic Green and before semantic
Green until the upstream-Green gate has passed; stop on upstream invalidation;
on any product, architecture, shared-contract,
or safety change; before repairing incomplete-provenance findings; before any
unauthorized destructive repository, worktree, or source operation or
out-of-plan destructive effect; an accepted in-plan `Replace` remains allowed
only after its recovery, preflight, revalidation, and verification gates; stop
before any remote, release, or publication effect; or when accepted evidence
would require an unaccepted seam, workaround, shared abstraction, or
protected-path expansion.
