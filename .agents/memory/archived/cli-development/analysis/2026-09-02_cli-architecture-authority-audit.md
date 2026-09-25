---
open-forge:
  description: Decision-ready audit of replacement CLI Architecture placement, provenance, references, and proposed narrower authority boundaries
  tags: [Memory, Analysis, Contextual, CLI, Architecture, Authority, Audit, Documentation, CSharp, NativeAOT, Package, Archived, Historical]
---

# Replacement CLI Architecture Authority Audit

## Status And Conclusion

This audit covers the complete 1,053-line [Replacement CLI
Architecture](../../../crystallized/documents/cli/architecture.md) at exact commit
`975008d6c046d9c5c9162ba113896c50c6e332e7`. It is decision-ready analysis, not
an accepted Architecture rewrite. The audit preserves every current requirement
and recommends source changes only after the maintainer accepts the proposed
authority map.

The document still provides a coherent top-down system model. Its architectural
goals, threat boundary, physical and project boundaries, dependency direction,
host and shell model, filesystem identity model, and high-level release boundary
belong there. The material placement problem is narrower but substantial:

- exact public result/status schema is assigned to Architecture even though the
  opening authority statement says command contracts define product behavior;
- later foundation and command closeouts appended exact callable designs,
  command-specific exceptions, schemas, operating procedures, completion state,
  commit receipts, and evidence detail;
- package versions, managed development publication mechanics, CI paths, and the
  delivery sequence duplicate narrower configuration, Decision, development,
  delivery-Task, and Working sources;
- the plural thin-wrapper wording at the frozen base does not record the now
  accepted npm package graph and platform horizon, whose exact authority belongs
  to permanent Task 7 and a narrower durable distribution source; and
- the final delegation section retains a legacy `Mastermind` authority statement
  that conflicts with the current Overseer and Task Mastermind model.

Git evidence shows deliberate accretion rather than unexplained debris. Each
major block entered through an accepted integration or closeout commit. The
problem is that those commits treated Architecture as the durable destination
for several different knowledge roles. Deliberate history does not make every
result an architectural invariant.

No CLI, contract, Architecture, package, project, test, public documentation, or
release source changed during this audit.

## Audit Boundary And Method

- Task: [Task 9, CLI Architecture Authority
  Audit](../tasks/cli-architecture-authority-audit.md).
- Starting condition: Established greenfield-replacement program with an accepted
  current Architecture and substantial implemented source.
- Decision authority: The maintainer decides any accepted source relocation or
  change to product meaning. This audit classifies and recommends only.
- Protected meaning: Public command behavior, implementation invariants,
  filesystem and recovery guarantees, dependency direction, supported platform,
  release boundary, and existing evidence requirements.
- Evidence: Complete Architecture read, heading/range inventory, outbound and
  inbound reference census, comparison with current Contracts, Decisions,
  Directives, development and delivery sources, first-parent history, file
  history, blame, and targeted independent reviews.
- Deliberate limit: This is not a command-surface correctness scrub. Neighboring
  sources were inspected only to establish authority, duplication, conflict, and
  safe routing.

Every semantic reviewer independently read the complete current C# Directive
set. The audit-base SHA-256 fingerprints are:

| Source                                 | SHA-256                                                            |
| -------------------------------------- | ------------------------------------------------------------------ |
| `.agents/directives/csharp/_csharp.md` | `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53` |
| `.agents/directives/csharp/design.md`  | `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9` |
| `.agents/directives/csharp/style.md`   | `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb` |

## Placement Standard

The audit uses the repository's current knowledge-role boundary:

| Content type                     | Primary question                                                                                         | Proposed source role                                                                                         |
| -------------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Architecture invariant           | How is the current system structured, how do parts relate, and which cross-cutting boundaries govern it? | CLI Architecture or a narrower accepted Architecture view.                                                   |
| Accepted rationale               | Why was one consequential choice accepted and what tradeoff followed?                                    | Crystallized Decision linked from Architecture.                                                              |
| Product behavior                 | What may callers enter and observe, and how must a conforming operation behave?                          | Shared or command-local Interface and Behavior Contracts.                                                    |
| Concrete local design            | How does one command or shared capability realize accepted behavior?                                     | Command-local or capability-local Technical Design.                                                          |
| Build/package/platform mechanism | Which exact configuration, artifact, or package-manager mechanism is current?                            | Machine-readable configuration plus a Decision for rationale and development/delivery documentation for use. |
| Active order and state           | What is implemented, active, blocked, next, or evidenced now?                                            | Plan, Task, project ledger, or Checkpoint in Working Memory.                                                 |
| Operating procedure              | What should a maintainer or agent do?                                                                    | Directive, Workflow, Task, or development documentation.                                                     |
| Evidence receipt                 | What exact commit, tree, command, count, result, or limitation proved one boundary?                      | Task, Checkpoint, Decision evidence, or historical record.                                                   |

Architecture may summarize a related fact when readers need it to understand a
boundary. The summary should link to the narrower source and should not copy the
complete schema, procedure, receipt, or task state.

## Complete Coverage Map

The locations below are frozen to the audited base. A later remediation should
also use the heading and subject because line numbers will move.

| Architecture location                                 | Current subject                                                                                                                                  | Audit disposition         | Reason                                                                                                                                                                                                                                                                                                       |
| ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 10-27, `Status And Authority`                         | Authority split, removed implementation, non-shipping release state                                                                              | Split                     | Retain the Architecture-versus-contract boundary and non-shipping boundary. Move exact historical commit and current completion checklist to reset/delivery/Working sources.                                                                                                                                 |
| 28-52, `Architectural Goals`                          | System goals and excluded mechanisms                                                                                                             | Retain                    | Cross-cutting drivers and explicit non-goals.                                                                                                                                                                                                                                                                |
| 53-78, `Project Criticality And Threat Boundary`      | Consequence, recovery, threat, and platform limits                                                                                               | Retain                    | Governs filesystem, mutation, portability, and exceptional-machinery choices across the system.                                                                                                                                                                                                              |
| 79-136, `Physical Workspace`                          | Root/source/test/artifact layout and removed preserved tests                                                                                     | Split                     | Retain physical structure and compile-item boundaries. Move lines 126-129 historical disposition to the completed Task or history.                                                                                                                                                                           |
| 137-167, `Project Graph`                              | Six projects and dependency direction                                                                                                            | Retain                    | Stable cross-project structure and visibility boundary.                                                                                                                                                                                                                                                      |
| 168-189, `Root Host Boundary`                         | Process/composition responsibilities                                                                                                             | Retain                    | Stable host boundary and explicit composition.                                                                                                                                                                                                                                                               |
| 190-261, `Core Source Organization`                   | Capability layout, leaf placement, model migration rules                                                                                         | Split                     | Retain capability/leaf/shared ownership. Route lines 247-257 authoring and migration procedure to the C#/CLI Directives, leaving a concise architectural locality invariant.                                                                                                                                 |
| 262-282, `Dependency Direction`                       | Inward dependency graph and command/framework separation                                                                                         | Retain                    | Core top-down invariant.                                                                                                                                                                                                                                                                                     |
| 283-328, `Shell Definitions And Composition`          | Typed identities, closed bindings, direct construction                                                                                           | Retain                    | Cross-cutting callable architecture.                                                                                                                                                                                                                                                                         |
| 329-351, `Native Interaction`                         | Shared prompt transport and consumer boundary                                                                                                    | Retain with compression   | The capability boundary is architectural. Command examples and negative mechanism detail can be shortened after a narrower accepted design exists.                                                                                                                                                           |
| 352-397, `Parsing And Invocation`                     | Parser ownership and one Route Update raw-spelling exception                                                                                     | Split                     | Retain parser and request invariants. Move lines 378-389 to Route Update Technical Design and resolve the reused edge identifier.                                                                                                                                                                            |
| 398-432, `Execution Pipeline`                         | Immutable stages and one-operation/one-output rules                                                                                              | Retain                    | Stable process-wide control flow.                                                                                                                                                                                                                                                                            |
| 433-507, `Result JSON Coordinates And Process Status` | Exact public JSON envelope, source location, statuses, exits, streams, compatibility                                                             | Relocate and link         | This is exact shared public contract meaning, not implementation structure. Architecture should retain only the implementation relationship to concrete source-generated result graphs.                                                                                                                      |
| 508-528, `Presentation, Help, And Diagnostics`        | Renderer ownership, exact JSON/status restatement, help and diagnostics                                                                          | Split                     | Retain renderer/help/diagnostic implementation boundaries. Link shared result coordinates instead of restating the schema and status table.                                                                                                                                                                  |
| 529-536, `Framework Capability Model`                 | Foundation versus consumer-local semantics                                                                                                       | Retain                    | Placement rule for accepted shared consumers.                                                                                                                                                                                                                                                                |
| 537-543, `Workspace`                                  | Workspace selection and absence                                                                                                                  | Retain                    | Shared process and fact boundary.                                                                                                                                                                                                                                                                            |
| 544-580, `Filesystem And Resolved Path Identity`      | Typed path identity, containment, BCL boundary                                                                                                   | Retain                    | Core safety and platform architecture.                                                                                                                                                                                                                                                                       |
| 581-646, `Sources, Routing, And Documents`            | Shared facts plus exact Generated Navigation overload, algorithms, completion state, and commits                                                 | Split                     | Retain source/document capability and observed-versus-intended invariants. Move exact signature, completion labels, commit/tree receipts, and detailed formation algorithm to a durable capability design and Working/history.                                                                               |
| 647-666, `Embedded Framework Distribution`            | Embedded payload boundary, exact hashing mechanics, and proof procedure                                                                          | Split                     | Retain the runtime resource location, single canonical embedded-payload boundary, and consumer map. Move exact hashing/parity mechanics and checkout-relocation procedure to a shared capability design and evidence sources.                                                                                |
| 667-889, `Lifecycle, Mutation, And Recovery`          | Cross-cutting mutation shape plus exact command behavior, lock/storage paths, ZIP schema, receipts, cleanup, lifecycle schema, and command rules | Split into several owners | Retain the high-level effect, lock, recovery, lifecycle, and no-rollback invariants. Route public/semantic rules to shared and command contracts, concrete mechanism to a capability Technical Design, rationale to Decisions, and receipts/state to Tasks.                                                  |
| 890-908, `Serialization And Dependencies`             | Serialization implementation and exact package versions                                                                                          | Split                     | Retain source-generation/reflection/AOT boundaries and allowed dependency roles. Let `Directory.Packages.props` define exact current versions and Decisions preserve rationale.                                                                                                                              |
| 909-945, `Test Architecture`                          | Tier responsibilities, naming/traits, isolation, historical tests, proportional gate procedure                                                   | Split                     | Retain project-tier boundaries and isolation. Route authoring procedure and gate selection to testing/CLI Directives and historical disposition to completed Tasks.                                                                                                                                          |
| 946-981, `Build, Native AOT, CI, And Artifacts`       | Toolchain constraints, managed development publication, base supported RID, CI paths and procedure                                               | Split                     | Retain toolchain, artifact topology, Native AOT, and no ambient override invariants. Link exact current platform/RID support to permanent Task 7 and narrower distribution authority, and link publication/CI mechanics to the tooling Decision, build configuration, development guide, and delivery Tasks. |
| 982-1024, `Durable Implementation Sequence`           | Sixteen-step program order with completed foundation labels and delivery step                                                                    | Relocate and summarize    | The exact queue and completion state belongs to Plan/Tasks. Retain only dependency-order invariants needed to prevent architectural dead ends.                                                                                                                                                               |
| 1025-1042, `Planning, Tasks, And Delegation`          | Task packet and acceptance operating procedure with legacy `Mastermind` authority                                                                | Relocate                  | Current Directives, Workflows, role sources, and Task records govern execution. The legacy authority sentence conflicts with them.                                                                                                                                                                           |
| 1043-1053, `Release Boundary`                         | One-RID base state, thin wrappers, deferred supply-chain features, no partial release                                                            | Split and link            | Retain only thin, no-domain-behavior, no-download, no-postinstall, and no-fallback invariants plus links. Route the exact graph, platform horizon, atomic-publication rule, and proof to permanent Task 7 and narrower distribution authority; require a new decision for expansion.                         |

## Material Findings

### T9-ARCH-001: Architecture owns exact public result behavior contrary to its authority statement

- Severity: High.
- Category: Authority conflict and contract placement.
- Exact source: Architecture lines 12-17, 433-506, and 510-513 under `Status And
Authority`, `Result JSON Coordinates And Process Status`, and `Presentation,
Help, And Diagnostics`.
- Evidence: Lines 12-17 say the Command Contract Set, Shared CLI Operation
  Contract, and command contracts define product behavior. Lines 433-506 then
  define the exact public JSON envelope, member order and nullability, semantic
  statuses, numeric exits, primary streams, source-location coordinates, and
  compatibility rules. The [Shared CLI Operation
  Contract](../../../crystallized/documents/cli/shared-operation-contract.md#repetition-results-and-streams)
  already defines the same seven statuses and stream policy while claiming it
  does not create a second command schema. Many command contracts direct readers
  back to Architecture for the exact shared schema.
- Why placement is wrong: These statements answer what callers observe and what
  compatibility means. That is shared Interface/Behavior contract meaning. The
  implementation choice to form concrete source-generated graphs is
  architectural, but the wire and exit schema is not made architectural merely
  because every command consumes it.
- Canonical proposed owner: A dedicated shared result-coordinates Interface and
  Behavior contract under the existing `contracts/shared/` authority. The Shared
  CLI Operation Contract may retain the concise cross-command status/stream rule.
  Architecture retains concrete-result, source-generation, and pipeline
  dependency facts and links the shared result contract.
- Reference/relocation strategy: Add the shared contract first by moving meaning
  without weakening or extending it. Update every command-contract inbound link
  that currently names Architecture as exact schema owner. Then replace the
  Architecture schema/table with a concise implementation relationship and link.
- Dependency and order: This precedes Architecture reduction because current
  command contracts depend on the Architecture anchor. It may be combined with
  `T9-ARCH-004` only if the shared contract remains readable.
- Acceptance evidence: Exact field, order, type, nullability, status, exit,
  stream, source-coordinate, and compatibility mapping before versus after;
  repository-wide inbound-link audit; generated navigation; Markdown links; and
  existing shared-schema serialization/public evidence with unchanged source and
  results.
- Decision needed: The maintainer must accept the new shared contract location.
  No product schema choice is required if the move is exact.

### T9-ARCH-002: The Route Update parser exception reuses a contradictory Working edge identity

- Severity: High.
- Category: Command-local design placement and current-source conflict.
- Exact source: Architecture lines 378-389 under `Parsing And Invocation`;
  [edge-case ledger](../edge-cases.md#cli-edge-005--raw-lexical-option-edge)
  lines 134-172; and [Route Update Task](../tasks/route-mutation/route-update.md)
  lines 550-557 at the audited base.
- Evidence: Architecture uses `CLI-EDGE-005` for Route Update's attached-empty
  `--responsibility` exception. The active edge ledger uses the same identity for
  the older Route List depth delimiter boundary, says the one documented
  equals-only exception remains, and does not describe Route Update. The Route
  Update Task records the newer exception and its accepted evidence.
- Why placement is wrong: The parser-wide rule is architectural. The exact
  spelling recognizer and command exception are Route Update design. Reusing one
  stable edge ID for two distinct exceptions makes Working continuity internally
  misleading and makes Architecture appear to supersede a record it does not
  update.
- Canonical proposed owner: A Route Update Technical Design subordinate to the
  Route Update Interface and Behavior contracts. The generic parser architecture
  retains only the standard-behavior rule and the existence of explicitly
  accepted bounded exceptions. The edge ledger must use distinct stable
  identities or explicitly consolidate the two scopes after maintainer review.
- Reference/relocation strategy: First reconcile the edge identity without
  changing either accepted grammar. Then add the command-local Technical Design
  from the exact Task wording and tests. Finally replace Architecture lines
  378-389 with a link and a one-sentence exception boundary.
- Dependency and order: Resolve before removing the Architecture detail because
  it is currently the only Crystallized source that names the Route Update
  implementation exception.
- Acceptance evidence: Exact accepted and rejected Route Update spellings,
  `--` stop behavior, typed-zero-token precondition, absence of raw arguments
  from request/domain, Route List delimiter regression, link census, and a proof
  that no stable finding/edge ID names two unrelated subjects.
- Decision needed: The maintainer must accept the durable Technical Design and
  the identity repair. The audit does not choose the replacement ID.

### T9-ARCH-003: Generated Navigation design and integration receipts are embedded in the system view

- Severity: Medium.
- Category: Shared capability design, task state, and evidence placement.
- Exact source: Architecture lines 604-645 under `Sources, Routing, And
Documents`, especially the exact overload at 606-607 and `Complete`, feature,
  integration commit, and tree claims at 628-632.
- Evidence: The exact overload also appears in the active Plan, lifecycle and
  route-mutation parent Tasks, and Checkpoint. `git blame` attributes 29 lines in
  this range to `18f2acff`, whose subject and body explicitly say it recorded an
  integrated formation expansion and its evidence in durable Architecture and
  active ledgers.
- Why placement is wrong: Observed-versus-intended formation and dependency
  direction are cross-cutting architecture. An exact C# signature and collision
  algorithm are a shared capability design. `GN1`/`I1` completion, commits, and
  trees are execution state and evidence receipts.
- Canonical proposed owner: A durable Generated Navigation capability Technical
  Design for the exact callable surface and formation algorithm; active Plan and
  Tasks for current dependency and completion state; Git/closeout records for
  commit and tree receipts.
- Reference/relocation strategy: Preserve a short Architecture invariant: one
  neutral formation combines observed catalogue evidence with intended logical
  membership and owns no command policy or effects. Link the capability design.
  Remove completion and commit receipts after verifying the Working and Git
  records.
- Dependency and order: The durable capability design must exist before exact
  signature and algorithm detail leave Architecture. Working receipt cleanup can
  follow independently.
- Acceptance evidence: Direct consumer and signature census; exact behavior map
  for observed candidates, intended membership, aliases, topology, collisions,
  and missing Loader; Task/Plan dependency links; and unchanged focused Generated
  Navigation evidence.
- Decision needed: The maintainer must accept the durable shared-design location.

### T9-ARCH-004: Mutation and recovery combines architecture, product behavior, local design, and receipts in one block

- Severity: High.
- Category: Cross-cutting authority overreach and duplication.
- Exact source: Architecture lines 667-889 under `Lifecycle, Mutation, And
Recovery`, with product/command-heavy concentrations at 672-688, 753-862, and
  864-888.
- Evidence: The section contains a valid high-level mutation flow, but also exact
  `LocalApplicationData` modes, lock filename construction, ZIP manifest schema,
  payload validation, `RecoveryBundlePreparation` construction, receipt states,
  cleanup statuses and deletion semantics, lifecycle schema fields, and Root
  Install/Update/Route Init rules. The [Shared CLI Operation
  Contract](../../../crystallized/documents/cli/shared-operation-contract.md#recovery-and-cleanup-boundaries)
  repeats much of the same detail, while numerous command contracts repeat the
  command-local subset. `git blame` attributes most of the block to accepted
  mutation, Index, lifecycle-freeze, and Install commits.
- Why placement is wrong: The section answers at least four different questions:
  cross-cutting safety structure, technology-neutral operation behavior, concrete
  BCL/ZIP/lock realization, and command-specific semantics. Keeping all four in
  Architecture makes it impossible to know which narrower source should change
  when one command, schema, or mechanism evolves.
- Canonical proposed owner: Architecture retains the mutation stage order,
  cooperating-process lock boundary, external recovery-before-existing-target
  invariant, no automatic rollback/restore/compensation rule, and lifecycle
  provenance boundary. Shared and command contracts own observable statuses,
  effects, cleanup semantics, and command-specific rules. A durable Mutation And
  Recovery Technical Design owns exact `LocalApplicationData`, `FileShare.None`,
  ZIP, manifest, type, and atomic-file mechanics. Decisions own consequential
  rationale such as proportional recovery and the accepted threat boundary.
- Reference/relocation strategy: Create and validate the narrower contract/design
  sources first. Build a paragraph-level preserved-meaning map. Update inbound
  callers to the precise owner. Replace the Architecture block with the
  cross-cutting invariants, component relationships, honest limits, and links.
- Dependency and order: This is the largest and highest-risk relocation. Do it
  after `T9-ARCH-001` establishes the shared-contract pattern and before removing
  duplicate command wording. Do not combine it with product behavior changes.
- Acceptance evidence: Paragraph-by-paragraph requirement-strength comparison;
  exact lock, recovery, lifecycle, cleanup, residual-state, and no-rollback
  contract mapping; source/type/consumer audit; all links; generated navigation;
  and existing managed plus supported Native AOT recovery/mutation evidence from
  an unchanged implementation baseline.
- Decision needed: The maintainer must accept the new durable cross-cutting
  Technical Design boundary. If that boundary is rejected, retain the mechanism
  detail in Architecture and remove only receipts and command-local duplication.

### T9-ARCH-005: Binding authoring and evidence procedures are duplicated as architecture

- Severity: Medium.
- Category: Directive and Workflow duplication.
- Exact source: Architecture lines 247-257 in `Core Source Organization`, lines
  925-944 in `Test Architecture`, and lines 1018-1023 in `Durable Implementation
Sequence`.
- Evidence: These lines tell future Tasks how to migrate materially changed
  models, when to split model folders, how to name and trait every test, how to
  treat historical tests, how to select focused/full evidence, and when an
  unchanged predecessor may supply a baseline. The same rules exist in the C#
  Style Directive and the CLI Implementation Directive, where binding work
  behavior belongs.
- Why placement is wrong: Architecture should state the resulting source and test
  boundaries. Directives govern how authors and Tasks must work. Duplicating the
  mandatory procedure creates two current rule sources that must remain manually
  synchronized.
- Canonical proposed owner: C# Style owns model-folder authoring shape. CLI
  Implementation and testing Directives own per-Task applicability, evidence
  selection, traits, historical-test promotion, and full-gate triggers.
  Architecture retains only the accepted folder/project boundaries, locality,
  evidence-tier responsibilities, isolation, and system-level full-gate need.
- Reference/relocation strategy: Confirm the Directives contain every required
  condition, add links from Architecture, then remove the duplicated work
  instructions. Do not weaken Architecture's structural result.
- Dependency and order: May proceed after the authority moves in
  `T9-ARCH-001` and `T9-ARCH-004`, so test obligations point to their correct
  contract/design owners.
- Acceptance evidence: Requirement-strength matrix, Directive route loading,
  selected task-record applicability examples, source/test path audit, Markdown
  links, and no change to existing gate requirements.
- Decision needed: None if the move is exact and current Directives already own
  the rules. Any changed trigger requires maintainer acceptance.

### T9-ARCH-006: Exact dependency versions have two current authorities

- Severity: Medium.
- Category: Package and configuration authority.
- Exact source: Architecture lines 897-907 under `Serialization And
Dependencies`; `Directory.Packages.props`; and the CLI Implementation Directive
  `Dependencies And Native AOT` section.
- Evidence: Architecture lists exact package versions while
  `Directory.Packages.props` supplies the exact build-consumed versions. The
  Directive currently requires versions accepted by both Architecture and the
  central package file. The values match at the audited base, so this is drift
  risk rather than a current mismatch.
- Why placement is wrong: A version update must change executable configuration.
  Requiring the same exact value in prose creates a second manually synchronized
  current fact. Architecture should own dependency roles, constraints, and the
  decision boundary, not mirror the package manager's exact value.
- Canonical proposed owner: `Directory.Packages.props` for exact current NuGet
  versions; one or more Decisions for why a dependency and pinned upgrade policy
  were accepted; Architecture for allowed roles and Native AOT/trimming/security
  constraints; the CLI Implementation Directive for the no-local-update rule.
- Reference/relocation strategy: Add a direct Architecture link to the central
  file and relevant Decisions. Change the Directive from dual exact-version
  authority to Architecture-approved dependency roles plus central exact
  versions. Remove only the version literals from Architecture.
- Dependency and order: Requires one accepted authority change because the current
  Directive explicitly names Architecture as an exact-version source.
- Acceptance evidence: Machine-readable package/version inventory; no floating or
  uncentralized runtime/test packages; dependency rationale links; locked restore,
  package audit, managed build, and Native AOT evidence at the next actual version
  change, not merely for this prose relocation.
- Decision needed: Yes. The maintainer must accept central configuration as the
  sole exact-version source.

### T9-ARCH-007: Developer publication and CI mechanics duplicate a dedicated Decision and guide

- Severity: Medium.
- Category: Build mechanism and operating procedure.
- Exact source: Architecture lines 952-961 and 973-980 under `Build, Native AOT,
CI, And Artifacts`; [Repository-Root CLI Tooling
  Decision](../../../crystallized/decisions/repository-root-cli-tooling.md) lines
  18-38 and 65-76; `Directory.Build.props`, the root CLI project, and
  `docs/development.md`.
- Evidence: The Architecture and Decision both state the managed
  `open-forge-dev` path, version marker, no-environment discovery, RID-selected
  native path, explicit opt-out, and `--no-build` precondition. The build files
  implement those exact paths; the development guide tells maintainers how to use
  them.
- Why placement is wrong: Repository-root ownership, artifact isolation, no
  ambient executable override, and separate native proof are architectural. The
  exact target mechanics and operator preconditions are Decision consequences,
  configuration, and development procedure.
- Canonical proposed owner: The existing Repository-Root CLI Tooling Decision for
  rationale and consequences; MSBuild/project files for exact current target and
  path mechanics; `docs/development.md` for operator procedure; delivery Tasks for
  CI and release evidence. Architecture keeps and links the structural
  constraints.
- Reference/relocation strategy: Replace duplicated target/path procedure with a
  short architecture summary and Decision link. Route exact current RID/platform
  delivery support through `T9-ARCH-011` to permanent Task 7 and narrower
  distribution authority rather than duplicating it here.
- Dependency and order: Independent after ensuring every inbound caller that needs
  an exact path points to the Decision/configuration or development guide.
- Acceptance evidence: Root-build and artifact-path static audit, development
  guide link check, no executable/version environment override, and current
  managed/Native AOT evidence references. A docs-only move does not require a new
  build unless configuration changes.
- Decision needed: None for exact deduplication. Changing the publish behavior,
  RID, or evidence boundary remains a maintainer decision.

### T9-ARCH-008: The exact implementation sequence is Working state presented as durable architecture

- Severity: High.
- Category: Plan, migration state, and evidence placement.
- Exact source: Architecture lines 982-1024 under `Durable Implementation
Sequence`, plus current-state phrases at 628-632 and delivery checklist phrases
  at 24-26.
- Evidence: The section names sixteen ordered implementation steps, Task shorthand
  such as `GN1`, `M1`, and `I1`, completed foundations, current command order, and
  the final delivery sequence. The active Plan and hierarchical Task records
  already own order, readiness, completion, and exact evidence. The Map route and
  Architecture description currently advertise `delivery sequence`, reinforcing
  the overlap.
- Why placement is wrong: Architecture should preserve dependency order that
  constrains valid designs. A live queue, completion label, and delivery checklist
  change as Tasks finish. Keeping that state in `#Evergreen` Architecture creates
  repeated closeout edits and makes old migration state look like a permanent
  system property.
- Canonical proposed owner: Active Plan and Task records for exact sequence and
  state; project ledger/Checkpoint for current progress; delivery parent Task for
  package/release order. Architecture retains a short dependency-order invariant:
  foundations before consumers, read-only fact formation before mutations,
  mutation producers before aggregate operations, and complete product before
  release.
- Reference/relocation strategy: Verify the Plan contains every still-relevant
  dependency, remove completed/receipt language from Architecture, link the active
  CLI Development route, and update the Architecture frontmatter and Sources Of
  Truth map so they no longer advertise a live delivery sequence.
- Dependency and order: Perform after durable shared capability designs from
  `T9-ARCH-003` and `T9-ARCH-004` exist, because some sequence detail currently
  preserves their consumer horizon.
- Acceptance evidence: Plan/Task dependency graph census, no orphaned shorthand,
  no current state or commit IDs left in Architecture, updated map/description,
  generated navigation, links, and a maintainer review of the retained dependency
  summary.
- Decision needed: The maintainer must accept Plan/Tasks as the sole exact active
  sequence authority.

### T9-ARCH-009: The delegation section conflicts with current orchestration authority

- Severity: High.
- Category: Stale operating procedure and authority conflict.
- Exact source: Architecture lines 1025-1041 under `Planning, Tasks, And
Delegation`, especially lines 1032-1036.
- Evidence: The section says “The Mastermind implements architectural foundations
  and cross-cutting callable contracts directly” and routes unresolved choices to
  “the Mastermind.” Current Directives and role sources distinguish the user-facing
  Overseer, bounded Task Mastermind, and Integration Mastermind. Existing Working
  continuity explicitly treats unqualified historical `Mastermind` labels as
  legacy provenance.
- Why placement is wrong: This is work orchestration, not CLI runtime structure.
  It is also stale enough to assign project-level authority ambiguously to a role
  name that no longer denotes one current owner.
- Canonical proposed owner: Hierarchical Orchestration and Program Architecture
  Directives, the selected development Workflows, Overseer/Task Mastermind role
  sources, and Task records. Architecture may retain only the invariant that a
  bounded implementation Task must not invent cross-cutting architecture.
- Reference/relocation strategy: Remove the operating procedure after current
  Directives are linked from the CLI Development route or task process. Do not
  rewrite the legacy word to `Overseer` inside Architecture because that would
  preserve the wrong knowledge role.
- Dependency and order: This correction can precede the larger content split, but
  the retained one-sentence implementation invariant should remain aligned with
  the Architecture goals at lines 33-46.
- Acceptance evidence: Search for live unqualified `Mastermind` authority,
  Directive/Workflow link validation, Task packet conformance, and review that no
  CLI runtime or product meaning moved.
- Decision needed: None for removing a stale duplicate after current authority is
  verified. Any orchestration-policy change remains outside this audit.

### T9-ARCH-010: Historical dispositions and commit receipts create unnecessary Evergreen churn

- Severity: Medium.
- Category: History and evidence placement.
- Exact source: Architecture lines 19-22, 126-129, and 628-632 under `Status And
Authority`, `Physical Workspace`, and `Sources, Routing, And Documents`.
- Evidence: The document names the removed implementation commit, removed
  preserved-test disposition, `GN1`/`I1` completion, and exact
  feature/integration tree identities. The reset record, completed Tasks,
  Checkpoint, Git, and closeout commits already preserve those facts. Current
  runtime receipt and Cleanup semantics remain governed by `T9-ARCH-004`,
  build/CI mechanics by `T9-ARCH-007` and `T9-ARCH-011`, and evidence procedure
  by `T9-ARCH-005` and `T9-ARCH-008`; they are not historical receipts.
- Why placement is wrong: A current Architecture may link consequential history
  that explains a present boundary. It does not need exact delivery receipts to
  define the current structure. Receipt additions cause `#Evergreen` edits even
  when architecture did not change.
- Canonical proposed owner: Reset and archived records for removed design history;
  completed Task records and Git for implementation/evidence receipts; current
  Architecture for the resulting boundary only.
- Reference/relocation strategy: Retain the reset-record link and remove redundant
  commit ID. Replace removed-test history with the current “do not restore a
  parallel test architecture” invariant if it still matters, linked to the
  completed disposition. Remove completion and tree receipts once their Task/Git
  locations are verified.
- Dependency and order: May be grouped with `T9-ARCH-003` and
  `T9-ARCH-008` after their durable dependency meaning is secured.
- Acceptance evidence: Exact receipt-location map, Git object existence,
  historical/current-link validation, and negative search for commit/tree IDs and
  completion labels in the final Architecture.
- Decision needed: None if current structural meaning remains explicit.

### T9-ARCH-011: The accepted npm package graph needs a narrower durable authority

- Severity: High.
- Category: Accepted release-authority placement and delivery/package mechanics.
- Exact source: Architecture lines 24-26, 963-980, 1015-1016, and 1043-1053;
  [CLI Delivery](../tasks/delivery/_delivery.md) and
  [thin npm package Task](../tasks/delivery/01-npm-packages.md).
- Evidence: At the frozen base, Architecture limits delivery to `linux-x64`,
  forbids partial release, and constrains plural thin wrappers to no
  behavior/download/postinstall/fallback, but does not name an exact package
  graph. The base npm Task describes one launcher plus one `linux-x64` platform
  package. Supplemental maintainer authority subsequently accepted one
  `@thelithiumforge/open-forge` main npm package with exact optional
  `@thelithiumforge/open-forge-linux-x64` and
  `@thelithiumforge/open-forge-win-x64` platform packages, supporting exactly
  `linux-x64` with glibc and `win-x64`, with no ARM or macOS support. This
  supplemental authority resolves the product question but is newer than the
  audited blob. The maintainer further clarified that permanent Task 7 was not
  reused: its old completed `npm Link Shims` label and implementation were a
  rejected mistaken realization of the same intended package-manager task. Its
  canonical identity is now Task 7 “npm Package Manager Release and Local
  Linking”; the stale ledger label remains historical provenance until Task 7
  integration corrects project control. The repository-local `open-forge-dev`
  npm link remains separate private developer tooling documented in
  `docs/development.md`.
- Why placement is wrong or incomplete: Thin-wrapper and prohibited-behavior
  constraints shape Architecture. The exact channel, package inventory, platform
  horizon, atomic-publication rule, source layout, staging, packing, checksums,
  CI commands, and proof need a narrower current distribution authority;
  otherwise every platform or package change forces system-Architecture
  accretion.
- Canonical proposed owner: Permanent Task 7 “npm Package Manager Release and
  Local Linking” for the accepted graph and platform horizon; the active
  [thin npm package Task](../tasks/delivery/01-npm-packages.md)
  and a later narrow durable CLI distribution source for package source, staging,
  packing, CI, publication, and evidence; Architecture only for thin,
  no-domain-behavior, no-download, no-postinstall, and no-fallback invariants and
  links; `docs/development.md` and the private package for local linking; a new
  maintainer Decision for any future channel, RID, architecture, OS, or libc
  expansion.
- Reference/relocation strategy: At Task 7 integration, correct project control
  to the canonical name and active/reopened provenance without changing permanent
  ID 7; preserve the rejected `npm Link Shims` realization as history. Make the
  accepted package graph and platform horizon explicit in the resulting narrow
  authority, link it from a concise Architecture Release Boundary, and keep exact
  mechanics and evidence in the npm delivery Task. Explicitly keep private
  `open-forge-dev` linking outside public distribution.
- Dependency and order: The Task 7 project-control correction accompanies Task 7
  integration and precedes an Architecture link or `T9-ARCH-008` sequence
  condensation. Concurrent uncommitted package-manager work is outside this
  audit's frozen source identity and must be checked against this finding before
  integration.
- Acceptance evidence: The exact three-package npm inventory, exact
  `linux-x64`/glibc and `win-x64` horizon, absence of ARM/macOS packages,
  synchronized version rule, artifact-hash binding, packed install and
  invocation, unsupported-platform behavior, public-versus-private identity
  audit, Architecture/Task 7/distribution-document agreement, preserved permanent
  ID 7, corrected canonical name/state, historical rejected-realization
  provenance, and complete links.
- Decision needed: No product decision is needed for the current graph or
  platform horizon or permanent identity; the maintainer has accepted them. The
  stale project-control label/state must be corrected when Task 7 integrates.
  Any expansion requires a new product decision.

### T9-ARCH-012: Completed shared foundations accreted exact mechanisms and evidence into Architecture

- Severity: Medium.
- Category: Shared capability design and completed-Task accretion.
- Exact source: Architecture lines 647-665 under `Embedded Framework
Distribution`, lines 736-751 under `Lifecycle, Mutation, And Recovery`, and
  lines 864-882 in that same section.
- Evidence: These passages preserve the exact embedded-resource hashing and
  parity proof, directory-create callable and real-consumer policies, and
  lifecycle provenance fields and evidence. `git blame` attributes them to
  `38e1498d`, whose commit body deliberately froze those foundations. Their
  wording closely follows the completed Framework Payload, Directory Create,
  and Lifecycle Provenance Tasks.
- Why placement is wrong: The capability locations, dependency direction,
  canonical payload identity, separate directory effect, and provenance identity
  are architecture. Exact algorithms, callables, consumer eligibility, and proof
  procedure answer capability-design, command-contract, and evidence questions.
  Treating completed Task packets as permanent subsections obscures the top-down
  boundary and makes later Task archival unsafe.
- Canonical proposed owner: Narrow shared capability Technical Designs for exact
  hashing, directory-effect, and lifecycle-provenance realization; command-local
  Install, Update, and Route Init contracts for consumer policy; Architecture for
  location and cross-cutting invariants; completed Tasks and Git for receipts.
- Reference/relocation strategy: Establish or confirm the durable shared designs,
  map their real consumers, then compress Architecture to capability boundaries
  and links. Preserve every Task receipt in Working or historical records rather
  than copying it into the new designs.
- Dependency and order: May be executed with `T9-ARCH-004`, but durable design
  destinations must exist before completed Tasks are archived or the detailed
  paragraphs are removed.
- Acceptance evidence: Exact source/consumer map, old-to-new requirement-strength
  comparison, hashing and provenance identity preservation, command-contract
  policy mapping, receipt discoverability, generated navigation, and link checks.
- Decision needed: None if the split preserves accepted meaning exactly.

## Independent Review Disposition

The targeted architecture reviewer returned eight material issues. They
converge with `T9-ARCH-002` through `005` and `T9-ARCH-007` through `012`; several
review issues span more than one synthesized finding. This audit accepted and
incorporated the distribution-authority and separate completed-foundation
findings during grouped correction `T9-C1`. The reviewer initially reported the
package graph as unresolved. Supplemental maintainer authority subsequently
accepted the exact current npm graph and platform horizon, so `T9-ARCH-011` now
records a placement remediation rather than an open product decision.

The fresh whole-task reviewer then reported a collision between the instruction's
“Task 7” label and the base ledger's completed `npm Link Shims` label. That
finding was superseded by maintainer provenance unavailable to the reviewer: the
old label and implementation were a rejected mistaken realization of the same
intended package-manager task. Permanent ID 7 is preserved, its canonical name is
now “npm Package Manager Release and Local Linking,” and project control must
record the correction when Task 7 integrates.

The same review's other two material findings were accepted. `T9-ARCH-010` now
contains only actual history/completion receipts, leaving active runtime and
delivery meaning in `T9-ARCH-004`, `T9-ARCH-007`, and `T9-ARCH-011`. The Task
record now reports the actual review and validation sequence instead of a
premature closeout claim.

One material judgment remains intentionally visible. The reviewer favored
retaining Architecture lines 433-506 as a stable shared JSON/status/exit anchor
because numerous command contracts already depend on it. The audit author
instead classifies that exact public schema as shared contract meaning in
`T9-ARCH-001`. Both agree that current callers need one stable durable anchor and
that no product schema may change. The maintainer must choose the canonical
knowledge role before any relocation; until then the present Architecture block
remains authoritative and unchanged.

## Provenance And Accretion

The current placement is not random. The following commits deliberately added or
expanded the material now under review:

| Commit                    | Architecture contribution                                                                                                                        | Placement implication                                                                                                                       |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `768bd51a` and `edca509d` | Greenfield system shape, then the first accepted route-list architecture and most enduring headings.                                             | Primary architectural base. The broad rewrite also established the habit of keeping Task/delegation and delivery sequence in the same file. |
| `1f03d16c`                | Added the complete shared JSON/status/source-location block during Find acceptance.                                                              | Deliberate public-contract accretion, not an unexplained addition. It also created the present authority contradiction.                     |
| `93dd4401`                | Added managed development publication and root developer-workflow mechanics.                                                                     | A later dedicated Decision now provides a narrower rationale and consequence source.                                                        |
| `234199d5`                | Added most recovery store, ZIP, receipt, cleanup, and proportional mutation detail.                                                              | Deliberate safety freeze. The content still spans Architecture, Contract, Technical Design, and evidence roles.                             |
| `09aa03ed`                | Added Generated Navigation/Index state, proportional full-gate policy, supported RID language, and implementation sequence updates.              | Deliberate task closeout and evidence policy accretion.                                                                                     |
| `18f2acff`                | Recorded the exact observed-plus-intended overload, formation detail, completion state, commit, and tree.                                        | Commit body explicitly says it wrote both durable Architecture and active ledgers, proving purposeful duplication.                          |
| `38e1498d` and `c60fcb98` | Added native interaction, embedded payload, directory creation, lifecycle provenance, Install/Route sequencing, and later lifecycle refinements. | Accepted shared foundations, but several command and Task facts entered the system view with them.                                          |
| `272f5121`                | Added the Route Update attached-empty lexical exception after Task integration.                                                                  | Latest and clearest example of a command-local accepted design appended directly to Architecture; it also exposed the reused edge identity. |

The remediation should therefore preserve the accepted decisions and evidence
behind these commits. It should not treat removal from Architecture as rejection
of the implemented design.

## Inbound And Outbound Reference Consequences

The Architecture has only four direct outbound targets at the audited base: the
Command Contract Set, Shared CLI Operation Contract, detailed command-contract
route, and reset record. All resolve, and none uses a fragment. The first two
targets share one wrapped Markdown sentence, which a line-oriented link census
can undercount as three links. This audit counts actual target tokens.

The tracked-base inbound census found 137 inline links from 87 Markdown files.
Fourteen use fragments: thirteen target
`#result-json-coordinates-and-process-status`, and one targets
`#sources-routing-and-documents`; every fragment resolves. The two new Task 9
records add two more current-worktree links, producing 139 inbound links from 89
files before generated index entries. No C# source or test file links to this
replacement Architecture; generic `architecture.md` paths and `Architecture`
test tags are unrelated.

Inbound references are broad and consequential:

- current source maps, the top Open Forge Architecture, public CLI and Extension
  documentation, and development documentation use the file as the accepted
  replacement implementation boundary;
- the CLI route, Command Contract Set, Shared CLI Operation Contract, contracts
  root, shared contract scopes, and many command-local Interface/Behavior files
  name it as the owner of structure, exact shared schema, filesystem realization,
  serialization, recovery, status, exits, and Native AOT detail;
- the active Plan, project ledger, Checkpoint, parent Tasks, and leaf Tasks use it
  for dependency, placement, safety, and evidence constraints; and
- Directives require implementation to follow it and currently make it part of
  exact dependency-version authority.

The exhaustive direct-link counts and the intentionally broad phrase scan are
different evidence classes:

| Tracked-base group                               | Direct inline links (files) | Heuristic phrase hits (files) |
| ------------------------------------------------ | --------------------------: | ----------------------------: |
| Directives                                       |                       0 (0) |                         5 (3) |
| Crystallized documents, contracts, and Decisions |                     95 (51) |                      204 (75) |
| Working Tasks, Plan, and Checkpoint              |                     26 (24) |                       47 (30) |
| Public and development documentation             |                       4 (3) |                         3 (2) |
| Source and tests                                 |                       0 (0) |                         1 (1) |
| Templates and patterns                           |                       0 (0) |                         1 (1) |
| Archived and Emerging                            |                      11 (8) |                       47 (32) |
| Maps                                             |                       1 (1) |                         0 (0) |

The direct-link column is the authoritative 137-link/87-file census. The phrase
column is heuristic: it deliberately includes historical, template, generic
`Architecture`, and semantic wording that is not necessarily a link or a caller.
It supports duplicate-cluster discovery only and must not be used as an authority
count.

Crystallized documents, contracts, and Decisions are therefore the dominant
direct callers, followed by Working continuity, historical/candidate sources,
public documentation, and the Sources Of Truth map.
Several contract callers also assign Architecture narrower token-estimation,
diagnostic/redaction, cleanup-artifact, package/runtime, and review-state facts.
Those callers require question-by-question rerouting during remediation; a link
that still resolves is not proof that its authority role remains correct.

This means remediation cannot begin by deleting paragraphs. It must first replace
the exact authority destination for every inbound reference that currently relies
on the moved detail. Archived references remain history and normally should not
be rewritten.

## Remediation Program

Use one accepted, reviewable program rather than many opportunistic edits:

1. **Accept the authority map.** Decide `T9-ARCH-001`, `T9-ARCH-002`,
   `T9-ARCH-004`, `T9-ARCH-006`, and `T9-ARCH-008` before mutation. Preserve the
   maintainer-accepted Task 7 package graph, platform horizon, and permanent ID
   from `T9-ARCH-011`; correct its stale project-control label/state at Task 7
   integration. Freeze the statement that this is meaning-preserving relocation,
   not product redesign, except for the explicitly unresolved Route exception
   authority in `T9-ARCH-002`.
2. **Create missing durable owners.** Add shared result coordinates, Route Update
   Technical Design, Generated Navigation capability design, and Mutation And
   Recovery capability design only after their boundaries are accepted. Copy no
   evidence receipts into them.
3. **Move contract meaning first.** Establish exact public result, recovery,
   lifecycle, cleanup, and command-local behavior in the appropriate shared or
   local Contracts. Reconcile the `CLI-EDGE-005` identity.
4. **Move concrete design and rationale.** Route exact callable/mechanism detail
   to Technical Designs, exact package versions to central configuration, and
   consequential why/tradeoffs to Decisions.
5. **Move state, procedure, and evidence.** Leave exact sequence and completion
   in Plan/Tasks/ledger/Checkpoint, operating rules in Directives/Workflows, and
   commit/test receipts in Task/Git history.
6. **Reduce and reconnect Architecture.** Retain the cross-cutting model,
   invariants, limits, and release boundary. Add links to the narrower sources and
   update its metadata and the Sources Of Truth map.
7. **Prove semantic equivalence.** Use a paragraph-level old-to-new mapping,
   inbound/outbound link census, generated navigation, protected-path audit, and
   targeted existing behavior evidence. Run executable gates only when a contract,
   configuration, or implementation fact changes, not merely because prose moved.

Recommended dependency order:

```text
shared result authority
  -> command contract references
  -> recovery/lifecycle authority split
  -> capability Technical Designs
  -> Task 7 project-control correction and distribution authority placement
  -> package/build source split
  -> Plan/evidence/procedure extraction
  -> Architecture reduction and map refresh
  -> complete reference and meaning-preservation gate
```

## Acceptance Evidence For A Later Rewrite

A later remediation is complete only when all of the following are true:

- every paragraph removed from Architecture has one accepted destination or is a
  verified redundant receipt/history copy;
- every requirement retains the same strength, conditions, exceptions, limits,
  and product meaning;
- every current inbound caller points to the source that now answers its exact
  question;
- no current source uses one stable identity for two unrelated edge cases;
- Architecture contains no active completion labels, queue state, commit/tree
  receipts, or unqualified legacy `Mastermind` authority;
- exact package versions have one current machine-readable source and accepted
  dependency rationale remains discoverable;
- the accepted current npm package graph and platform horizon have one narrow
  authority and agree with Architecture, Task 7, and delivery
  documentation;
- private `open-forge-dev` npm linking remains separate from future public npm
  distribution;
- Architecture metadata, the Sources Of Truth map, generated `Entries`, and all
  Markdown links match the new boundary;
- the final diff changes no product, C# source, test, package, project, public
  schema, release behavior, or generated runtime projection unless separately
  authorized; and
- a fresh architecture review inspects the complete reduced document and the
  new narrower sources together.

## Residual Risks And Open Decisions

- The Shared CLI Operation Contract itself contains concrete BCL, ZIP, lock, and
  command detail despite claiming to be technology-neutral. This audit establishes
  that duplication only far enough to route Architecture. The later recovery
  split must review that source as part of the same authority change.
- Existing command contracts repeatedly point to Architecture for exact shared
  result and recovery detail. A partial move would create broken or misleading
  authority even if links still resolve.
- The exact public distribution contract is not yet crystallized. Supplemental
  maintainer authority has accepted permanent Task 7's three-package npm graph
  and exact `linux-x64`/glibc plus `win-x64` horizon. The base ledger still shows
  the rejected `npm Link Shims` realization as complete; Task 7 integration must
  preserve ID 7, correct the canonical name/state, and retain the old label as
  history. A later durable source must preserve the accepted distribution meaning
  before the active Task is archived. Any expansion remains a new maintainer
  decision.
- A separate package-manager Task is allowed to evolve Architecture after this
  audit's exact base. Its later delta is not evidence about the audited source
  identity and must be re-audited against these findings before integration.
- The current Architecture description and Sources Of Truth map explicitly include
  evidence and delivery sequence. They must change with the accepted boundary or
  the old placement will remain invited.
- This audit does not decide whether one or several new durable Technical Designs
  are preferable. The proposed boundaries minimize mixed authority, but the
  maintainer must accept their final routing shape before files are created.

## Related Sources

- [CLI Command Contract Set](../../../crystallized/documents/cli/command-contract-set.md)
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
- [Repository-Root CLI Tooling Decision](../../../crystallized/decisions/repository-root-cli-tooling.md)
- [CLI Development Plan](../../../working/cli-development/plan.md)
- [CLI Project Control Ledger](../../../working/cli-development/project-control.md)
- [CLI Delivery](../tasks/delivery/_delivery.md)
- [CLI Distribution Channels Idea](../../../emerging/ideas/cli-distribution-channels.md)
- [Agent And Workflow Change Audit](../../../emerging/observations/2026-09-02_agent-and-workflow-change-audit.md)
