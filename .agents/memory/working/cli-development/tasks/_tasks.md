---
open-forge:
  description: Hierarchical implementation Tasks for the complete greenfield replacement CLI
  tags: [Memory, Working, Contextual, Active, CLI, Task, Architecture, Development]
---

# CLI Development Tasks

Read the [program Task](00-cli-development.md),
[Architecture](../../../crystallized/documents/cli/architecture.md), and
[Plan](../plan.md) before selecting a Task group.

## Task Groups

- [x] [CLI Foundation](foundation/_foundation.md) — Complete — Implementer: Mastermind
- [x] [Route Discovery](route-discovery/_route-discovery.md) — Complete — Implementer: Mastermind
- [x] [Generic CLI Improvements](generic-improvements/_generic-improvements.md) — Complete — Implementer: Mastermind
- [x] [Read-Only Commands](read-only/_read-only.md) — Complete; public Index feature candidate `4e89d945b38a2d1e24600dd22789b55e4395a534` is locally squash-integrated at `09aa03eddb97831ff544afe1eac54ad9af501f5c`, whose tree exactly equals final Index closeout tip `2b353c48978ee88e53345be8037776181612222c` — Implementer: Overseer-managed bounded Index owner
- [x] [Modern C# Improvements](modern-csharp-improvements.md) — Complete; Preflight `55eb82e`, Framework `a90af59`, Shell/root `fe10525`, Route Inspect/family `62a1dd9`, Route List `273eb45`, Tests/support `6af5fb1`, and final managed, Native AOT, package, audit, and public no-write gates accepted in the commit containing this record — Implementer: Mastermind
- [x] [Repository-root CLI developer workflow](repository-root-developer-workflow.md) — Complete and squash-integrated into local `develop` at `d9e0686` — Implementer: Mastermind
- [x] [Mutation Foundation](mutation-foundation/_mutation-foundation.md) — Complete at exact production candidate `e7d937f` under authority `01dd552`; final managed/native, static-absence, diff, and independent-review gates pass — Implementer: Overseer-managed Task Mastermind, sequential
- [x] [Test Architecture And Constants](test-architecture-and-constants.md) — Complete and squash-integrated at `b6ce31f` — Implementer: Overseer
- [x] [Read-Only CLI Dogfooding Corrections](read-only-dogfooding-corrections.md) — Complete and squash-integrated at `bba84b6` — Implementer: Overseer
- [x] [Proportional CLI Corrections](proportional-cli-corrections.md) — Complete and squash-integrated through `0d88606`; Mutation Foundation may resume — Implementer: Overseer with bounded Task Masterminds
- [x] [Next-Wave Shared Foundations](shared-foundations/_shared-foundations.md) — Complete at integrated D0/SF1-SF4 baseline — Implementer: Overseer-managed bounded Task Masterminds
- [x] [Route Mutation Commands](route-mutation/_route-mutation.md) — Complete: Route Init, Route Create, [CLI Quality Remediation](cli-quality-remediation.md), Task 3 “Route Update”, and Task 4 “Route Move” are Complete; Route Remove is Complete and integrated at `5a2e650a` — Implementer: Overseer-managed command lanes
- [x] [CLI Quality Remediation](cli-quality-remediation.md) — Complete and squash-integrated at `862cbf2a`, exact tree `571f104f`, from accepted implementation candidate `a4ccf19a`, tree `97254e65`; all thirteen findings/candidates and final `QR-R1-001` are closed — Implementer: Overseer-managed Task Mastermind
- [x] [CLI Architecture Authority Audit](cli-architecture-authority-audit.md) — Task 9 is Complete and integrated at `e431395a`, exact tree `656cccdf`; twelve location-precise findings are retained for Task 12 — Implementer: Dedicated Task Mastermind
- [x] [CLI Architecture Authority Remediation](cli-architecture-authority-remediation.md) — Task 12 is Complete after all twelve authority dispositions, one fresh Task review/correction, protected-identity closeout, and two bounded integration findings/correction — Implementer: Dedicated Task Mastermind with one Brilliant Implementer and one fresh review
- [x] [Root Tooling Placement Remediation](root-tooling-placement.md) — Complete and squash-integrated at `f8377094`, exact tree `c05c2ed6`; dequeued after completion-update grace — Implementer: Task 11 Task Mastermind with one Brilliant Implementer and one fresh review
- [x] [Lifecycle Commands](lifecycle/_lifecycle.md) — Complete: Extension Create, Extension Install, root Install, root Update, Extension Update, and Extension Remove are integrated — Implementer: Overseer-managed command lanes
- [x] [Operational Commands](operations/_operations.md) — Complete: Status, Doctor, Repair, and Cleanup are integrated; Task 20 Cleanup completed at phase 5/5, milestone 8/8 in `148d378d` — Implementer: Overseer-managed command lanes
- [x] [CLI Command Surface Audit](cli-command-surface-audit.md) — Task 10 is Complete, phase 3/3, milestone 5/5; integrated at `fbcec295`, seven findings selected for Task 21
- [ ] [CLI Command Surface Remediation](cli-command-surface-remediation.md) — Task 21 is Active, phase 4/4, milestone 5/6; review and full managed/native gates passed, integration pending
- [ ] [C# Structural Streamlining](csharp-structural-streamlining.md) — Task 27 is Planned after Task 21 and before Task 7 ARM64 expansion; phase/milestone horizon unassigned
- [ ] [Task 7 ARM64 package expansion](delivery/01-npm-packages.md#arm64-expansion-horizon) — Queued after Task 27; new phase/milestone horizon unassigned
- [ ] [CLI Delivery](delivery/_delivery.md) — Task 13 covers all six accepted platforms after Task 7; Task 22 remains last
- [ ] [Source Framework Wording and Logic Review](source-framework-review.md) — Task 28 is a separate queued user-owned parallel review; no implementation or assigned horizon
- [x] [Task 23: Workspace Libraries](workspace-libraries.md) — Complete, phase 5/5, milestone 8/8; Sol return and root placement correction accepted at `25c65bf8`.
- [x] [Task 24: Extensions Evolution](extensions-evolution.md) — Complete, phase 5/5, milestone 8/8; integrated at `2eedaf87`
- [x] [Task 25: Workspace Library Destination Projections](workspace-library-destination-projections.md) — Complete, phase 5/5, milestone 8/8; full managed/native acceptance, integrated at `3b4aba9d`
- [x] [Task 26: Extension Internal Consolidation](extension-internal-consolidation.md) — Complete, phase 4/4, milestone 6/6; full managed/native and differential acceptance, integrated at `aab57058`

## Axioms

- Read every parent Task before a child Task. Child scope inherits all parent
  constraints and may only narrow them.
- The Mastermind owns architecture, cross-cutting contracts, Task state,
  integration, and acceptance.
- A leaf Task is delegation-ready only when its decisions, predecessor outputs,
  class or algorithm model, allowed paths, tests, verification, and stop
  conditions are closed.
- An implementer changes only the leaf Task's allowed paths and returns evidence.
  It does not update Tasks, commit, promote shared code, or resolve architecture.
- A reviewer receives the accepted parent chain, exact baseline and changed paths,
  claimed evidence, and one named review horizon.
- Stop and return to the parent when implementation exposes a public-contract,
  architecture, dependency, platform, lifecycle, safety, or release choice.

## Task State Vocabulary

- `Planned`: Meaning exists, but a predecessor or design closure still blocks it.
- `Ready`: Every prerequisite and decision required to start is closed.
- `Active`: This is the selected implementation or integration Task.
- `Blocked`: A named unmet condition prevents progress.
- `Complete`: Acceptance evidence and integration are committed.
- `Cancelled`: The outcome is no longer required.

Generated Entries were refreshed after the temporary compatibility-name changes.
They provide current navigation only; the Task Group list and each Task record
define execution state.

## Entries

<!-- open-forge:generated-index:start -->

- [Parent outcome, scope, authority, and acceptance for the complete replacement CLI program](00-cli-development.md) - #Memory #Working #CLI #Task #Program #Architecture #Development #Contextual #Active
- [Audit the current replacement CLI Architecture authority and route misplaced detail to narrower sources without changing accepted meaning](cli-architecture-authority-audit.md) - #Memory #Working #Contextual #Complete #CLI #Task #Architecture #Audit #Authority #Documentation
- [Apply the accepted CLI Architecture authority audit without changing product behavior or requirement strength](cli-architecture-authority-remediation.md) - #Memory #Working #Contextual #Complete #CLI #Task #Architecture #Authority #Documentation #Remediation
- [Record the immutable 28-command CLI audit, validated findings, retained boundaries, coverage, and remediation decisions](cli-command-surface-audit-report.md) - #Memory #Working #Contextual #CLI #Audit #Review #Architecture #Refactoring #Testing
- [Review the complete retained CLI command surface for direct PR-level architecture, design, refactoring, and test-evidence problems](cli-command-surface-audit.md) - #Memory #Working #Contextual #Complete #CLI #Task #Audit #Architecture #Refactoring #Testing #Review
- [Correct the seven validated CLI audit findings within accepted behavior, test tiers, and local callable ownership](cli-command-surface-remediation.md) - #Memory #Working #Contextual #Active #CLI #Task #Remediation #Architecture #Refactoring #Testing
- [Remediate the accepted first-pass CLI architecture, C# design, authority, and test-evidence findings after Route Create and before Route Update](cli-quality-remediation.md) - #Memory #Working #CLI #Task #Audit #Architecture #Refactoring #Testing #Review #Contextual
- [Assess remaining C# internals and implement justified simplifications beyond the completed strategic command audit](csharp-structural-streamlining.md) - #Memory #Working #Contextual #CLI #Task #CSharp #Architecture #Refactoring
- [Package, prove, document, and release the complete native CLI without partial publication](delivery/_delivery.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #SupplyChain #Release #Contextual
- [Track accepted Extension manifest consolidation and exact managed/native behavior preservation](extension-internal-consolidation.md) - #Memory #Working #CLI #Task #Extension #Refactoring #Testing #Contextual #Complete
- [Review the candidate Extension content layout and exact consumer permissions for copied files and Library links](extensions-destination-proposal.md) - #Memory #Working #CLI #Extension #Library #Proposal #Contextual #Candidate
- [Review the concrete Task 24 content rename, permission contracts, implementation seams and decisive evidence before M1 freeze](extensions-evolution-contract-draft.md) - #Memory #Working #CLI #Extension #Permission #Contract #Design #Contextual #Candidate
- [Track completed Extension content naming, consumer permission, review corrections and full acceptance](extensions-evolution.md) - #Memory #Working #CLI #Task #Extension #Evolution #Content #Destination #Contextual #Complete
- [Build and accept the actual command-free C# workspace, Core, host, safety, tests, and Native AOT foundation](foundation/_foundation.md) - #Memory #Working #CLI #Task #Foundation #Architecture #DotNet #NativeAOT #Contextual #Complete
- [Improve cross-cutting CLI parser, test, callable, and project structure without changing accepted command meaning](generic-improvements/_generic-improvements.md) - #Memory #Working #CLI #Task #Generic #Parser #Testing #Architecture #Contextual #Complete
- [Implement root and Extension creation, installation, update, and removal lifecycle commands](lifecycle/_lifecycle.md) - #Memory #Working #CLI #Task #Lifecycle #Extension #Install #Update #Contextual
- [Apply accepted truthful nullability, construction, and modern C# syntax rules across the complete replacement solution](modern-csharp-improvements.md) - #Memory #Working #CLI #Task #CSharp #Nullability #Initialization #Refactoring #Contextual #Complete
- [Build and accept locking, lifecycle, planning, application, and recovery foundations before mutations](mutation-foundation/_mutation-foundation.md) - #Memory #Working #CLI #Task #Mutation #Lifecycle #Recovery #Contextual
- [Implement incremental aggregate status and diagnosis, then repair and cleanup after the complete producer inventory](operations/_operations.md) - #Memory #Working #CLI #Task #Status #Doctor #Repair #Cleanup #Contextual
- [Correct confirmed read-only CLI defects and preserve proportionate architecture findings before Mutation Foundation resumes](proportional-cli-corrections.md) - #Memory #Working #CLI #Task #Audit #Correctness #Architecture #Proportionality #Contextual #Complete
- [Correct three bounded read-only CLI dogfooding defects before Mutation Foundation resumes](read-only-dogfooding-corrections.md) - #Memory #Working #CLI #Task #ReadOnly #Context #Find #RouteInspect #Dogfooding #Contextual #Complete
- [Implement retained read-only source, context, extension, and generated-navigation commands](read-only/_read-only.md) - #Memory #Working #CLI #Task #ReadOnly #Source #Extension #Index #Contextual
- [Move replacement CLI tooling to the repository root and make ordinary test runs publish and discover the local development executable](repository-root-developer-workflow.md) - #Memory #Working #CLI #Task #DotNet #Testing #DeveloperExperience #Contextual #Complete
- [Place live repository-owned agent tooling in explicit source scopes and remove the unexplained root scripts bucket](root-tooling-placement.md) - #Memory #Working #Contextual #CLI #Task #Tooling #TypeScript #Testing
- [Implement read-only route discovery, beginning with route list and then route inspect](route-discovery/_route-discovery.md) - #Memory #Working #CLI #Task #Route #Discovery #ReadOnly #Contextual #Complete
- [Implement retained route mutation commands on the accepted mutation foundation](route-mutation/_route-mutation.md) - #Memory #Working #CLI #Task #Route #Mutation #Contextual
- [Add the interaction, Framework distribution, lifecycle provenance, and directory-create prerequisites for the next command wave](shared-foundations/_shared-foundations.md) - #Memory #Working #CLI #Task #Foundation #Shell #Framework #Lifecycle #Contextual
- [Independently review shipped framework wording and logic before assessing local extension candidates](source-framework-review.md) - #Memory #Working #Contextual #Task #Framework #Writing #Review
- [Audit symbolic constants, generated-region authority, and composable active-test foundations across the replacement CLI](test-architecture-and-constants.md) - #Memory #Working #CLI #Task #Testing #Architecture #Constants #Fixtures #Snapshot #Contextual
- [Execute the accepted Workspace Libraries contracts through bounded preparation, implementation, evidence, and integration](workspace-libraries.md) - #Memory #Working #CLI #Task #Workspace #Library #Contextual #Active
- [Track accepted mapped Library leaf projections, scoped permissions, recovery and complete managed/native evidence](workspace-library-destination-projections.md) - #Memory #Working #CLI #Task #Workspace #Library #Destination #Symlink #Contextual #Complete

<!-- open-forge:generated-index:end -->
