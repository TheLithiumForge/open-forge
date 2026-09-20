---
open-forge:
  description: Apply the accepted CLI Architecture authority audit without changing product behavior or requirement strength
  tags: [Memory, Archived, Contextual, Historical, Complete, CLI, Task, Architecture, Authority, Documentation, Remediation]
---

# Task 12: CLI Architecture Authority Remediation

## Task State

- State: Complete in the isolated Task lane after one fresh whole-task review,
  one grouped Task correction, protected-identity closeout, and one bounded
  post-closeout integration review/correction. It precedes Task 14 “Extension
  Install”.
- Permanent mapping: Task 12 “CLI Architecture Authority Remediation” in the
  [project control ledger](../project-control.md).
- Progress: phase 5/5, milestone 6/6 complete. M1 integrated-base
  preflight/acceptance, M2 Gray authority freeze, M3 Red evidence freeze, M4
  coherent rewrite and focused validation, M5 fresh whole-task review and
  grouped correction, and M6 protected-identity closeout are complete. The
  subsequent `INT-T12-R1`/`INT-T12-R2` integration findings were corrected
  without reopening or extending the accepted phase/milestone horizon.
- Profile: Streamlined assured documentation remediation. One Brilliant
  Implementer owns the coherent authority rewrite and focused validation. One
  fresh whole-task review may be followed by at most one grouped correction.
- Review budget: Maximum 1, stable ID `T12-R1`, consumed once against commit
  `6399ed471fb2e28b29d4572cfccf94a3e2a18a04`, tree
  `93c143466c05ea22846d4ab2022da8b16903ff71`.
- Correction budget: Maximum 1, stable ID `T12-C1`, consumed once at commit
  `89ca2eafd9dcd14b5d64da61a294fff731ea97d8`, tree
  `a31597c23bf14832725a04e9f75af25d9f3dc8cf`.
- Council budget: 0.
- Responsible role: A dedicated Task Mastermind in the isolated
  `codex/cli-architecture-authority-remediation` branch and
  `<home>/dev/open-forge-worktree/cli-architecture-authority-remediation`
  worktree based on the accepted integrated commit
  `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, exact tree
  `6959b51e148af44d59512d8bdd801350d88fc651`.

This Task record defines the accepted outcome, exact authority map, execution
horizon, scope, protected identity, and task-local budget state. It is the sole
mutable authority for the `T12-R1` and `T12-C1` budgets and their consumption.
The project control ledger defines permanent identity, queue state, worktree
mapping, and integration state; it does not duplicate or advance these budgets.
The separately assigned post-closeout integration review is recorded below and
does not constitute a second `T12-R1` review or `T12-C1` correction.

## Frozen Authority Map

The maintainer accepted the complete prepared map exactly at M1. The move is an
authority relocation and reduction only: it changes no behavior, requirement
strength, uncertainty, public schema, dependency version, platform semantics,
or implementation policy. All twelve Task 9 findings retain stable identities
and have the following final disposition.

| Finding       | Frozen disposition and destination                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `T9-ARCH-001` | Move the exact shared result coordinates into the routed `contracts/shared/result-coordinates/` contract set: one entrypoint plus `interface.md` and `behavior.md`. The Shared CLI Operation Contract may retain its concise cross-command status/stream rule. Architecture retains concrete-result, source-generation, and pipeline relationships and links this contract. Every command-contract link that currently names Architecture as exact schema owner moves to the shared contract without changing a field, order, type, nullability, semantic status, numeric exit, stream, source coordinate, or compatibility rule. |
| `T9-ARCH-002` | Move the accepted Route Update attached-empty parser exception unchanged to `contracts/route/update/technical-design.md` and assign stable `CLI-EDGE-016`. The generic parser rule remains in Architecture. `CLI-EDGE-005` remains exclusively the Route List raw lexical depth-delimiter edge; the two grammars are not consolidated.                                                                                                                                                                                                                                                                                            |
| `T9-ARCH-003` | Move Generated Navigation’s exact callable and formation mechanics to the routed `documents/cli/technical-designs/` set. Architecture retains neutral observed-plus-intended formation and dependency direction. Current order belongs only to Plan/Tasks/project control; history and integration receipts remain in existing Task/history/Git authorities.                                                                                                                                                                                                                                                                      |
| `T9-ARCH-004` | Move exact mutation and recovery realization to the routed `documents/cli/technical-designs/` set. Architecture retains stage order, the cooperating-process lock boundary, external recovery before an existing target, no automatic rollback/restore/compensation, lifecycle provenance boundaries, and links. Existing shared and command contracts continue to own observable statuses, effects, cleanup semantics, and command-local behavior.                                                                                                                                                                               |
| `T9-ARCH-005` | Remove stale authoring and evidence procedure from Architecture after verifying the current C# Style, CLI Implementation, testing Directives, and selected Workflows own it. Architecture retains resulting source/test boundaries, locality, tier responsibilities, isolation, and system-level evidence needs. No trigger or obligation is changed.                                                                                                                                                                                                                                                                             |
| `T9-ARCH-006` | Move dependency rationale and pinned-upgrade policy to `decisions/cli-dependency-policy.md`. Architecture owns allowed dependency roles and Native AOT, trimming, and security constraints. Root `Directory.Packages.props` is the sole exact-version authority. The CLI Implementation Directive changes only from dual exact-version wording to Architecture-approved roles plus centrally owned exact versions.                                                                                                                                                                                                                |
| `T9-ARCH-007` | Retain repository-root developer publication rationale and consequences in the existing Repository-Root CLI Tooling Decision, exact target/path mechanics in MSBuild and project configuration, operator procedure in `docs/development.md`, and CI evidence in delivery Tasks. Architecture retains structural artifact and override constraints. Public package/release mechanics route through the distribution disposition in `T9-ARCH-011`, not this developer-tooling authority.                                                                                                                                            |
| `T9-ARCH-008` | Retain live implementation order, readiness, and completion solely in the active Plan, Task records, and project-control sources. Architecture keeps only stable dependency-order invariants and links the active CLI Development route. Its description, Sources Of Truth entry, and inbound links stop advertising a live delivery sequence.                                                                                                                                                                                                                                                                                    |
| `T9-ARCH-009` | Remove stale delegation procedure from Architecture. Current Directives, Workflows, role sources, and Task records remain the only operating authorities. Architecture may retain only the invariant that a bounded Task cannot invent cross-cutting architecture.                                                                                                                                                                                                                                                                                                                                                                |
| `T9-ARCH-010` | Keep removed-design history in existing history/reset records and implementation or evidence receipts in existing completed Tasks and Git. Architecture retains only resulting current boundaries and consequential history links; active runtime, cleanup, build, CI, and evidence-procedure meaning is not mislabeled as history.                                                                                                                                                                                                                                                                                               |
| `T9-ARCH-011` | Move the accepted package graph, platform horizon, atomic-publication rule, package source/staging/packing mechanics, checksums, and release proof to `documents/cli/distribution.md`, with current delivery details and receipts still owned by Task 7, Task 13, and their delivery records. Architecture retains only thin-wrapper, no-domain-behavior, no-download, no-postinstall, no-fallback, and no-partial-release invariants plus links.                                                                                                                                                                                 |
| `T9-ARCH-012` | Move exact embedded-payload, directory-creation, and lifecycle-provenance realizations to separate routed designs under `documents/cli/technical-designs/`. Architecture retains capability locations, dependency direction, canonical payload identity, separate directory effect, provenance identity, and links. Existing command contracts retain consumer policy; completed Tasks and Git retain receipts.                                                                                                                                                                                                                   |

The routed technical-design set therefore contains generated navigation,
mutation and recovery, embedded payload, directory creation, and lifecycle
provenance. No new contract or design becomes an alternative owner
for meaning that remains in an accepted Interface, Behavior, Directive,
Workflow, Decision, Working record, configuration source, or Git history.

## Frozen Distribution Boundary

The accepted distribution target is exactly one x64 package graph for Linux,
macOS, and Windows:

- `@thelithiumforge/open-forge` is the main package;
- `@thelithiumforge/open-forge-linux-x64` carries the `linux-x64` glibc
  executable;
- `@thelithiumforge/open-forge-darwin-x64` carries the `osx-x64` executable and
  uses npm `os: [darwin]`, `cpu: [x64]`; and
- `@thelithiumforge/open-forge-win-x64` carries the `win-x64` executable.

This accepted target is not a claim about the present implementation. The
historical Task 7 baseline contains Linux and Windows packages, while macOS x64
is an accepted follow-up gap. Task 13’s present delivery evidence and ownership
remain Linux D1 preparation. Task 7 owns package-graph realization, npm release,
and local-linking mechanics; Task 13 owns the current platform-feasibility and
CI/artifact preparation boundary. This Task owns only durable authority
placement and Architecture/distribution agreement. ARM remains undecided: this
Task neither accepts an ARM RID nor broadens the package graph. The older Task 9
audit’s no-macOS snapshot is historical evidence and does not override this
later accepted target.

## Frozen Future Composition Boundary

Future operational composition is one immutable, application-scoped
`OperationalContributorCatalogue` explicitly built by `CliCompositionRoot`.
Producer-owned typed contributors project narrow Status and Doctor views from
fresh per-invocation observations. The catalogue is not dependency injection, a
service locator, reflection discovery, a runtime registry, a generic operational
engine, or ambient registration, and none of those mechanisms may be introduced
as a substitute. Composition alone changes no Status or Doctor public contract.

This Task places that accepted meaning durably in Architecture. It deliberately
does not freeze a C# callable signature or add a production surface. Task 15 Gray
owns exact contributor, catalogue, and view signatures against its then-current
integrated producer baseline.

## Expected Outcome

Apply all twelve accepted `T9-ARCH-*` findings from the
[CLI Architecture Authority Audit](../../../emerging/analysis/2026-09-02_cli-architecture-authority-audit.md).
The resulting Architecture contains cross-cutting system structure and
invariants. Decisions preserve accepted rationale. Shared and command contracts
own observable meaning. Technical designs own exact realization. Delivery,
package-manager, development, and Working Memory sources contain their narrower
detail and receipts. Links preserve navigation without duplicating authority.

The remediation preserves product behavior, requirement strength, uncertainty,
public schemas, CLI source, tests, package behavior, and supported-platform
meaning. It does not use documentation movement to choose an unresolved product
or implementation decision.

## Authority And Dependencies

- The [Task 9 audit](cli-architecture-authority-audit.md) is complete and records
  the classification, retained meaning, proposed destinations, dependency order,
  reviewer dissent, and acceptance evidence for every finding. Its older
  distribution snapshot is superseded only by the later accepted boundary
  frozen above.
- The [Replacement CLI Architecture](../../../crystallized/documents/cli/architecture.md)
  remains current until this Task integrates a reviewed replacement.
- The [CLI Command Contract Set](../../../crystallized/documents/cli/command-contract-set.md)
  and linked command contracts define public and command-local meaning.
- The [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
  defines technology-neutral cross-command conventions.
- Permanent Task 7 defines the accepted npm graph realization and Task 13 owns
  the current platform-delivery preparation described above. This Task may place
  and link their accepted meaning but may not implement or expand it.
- Route Move is already integrated in the accepted base. Route Remove consumes
  the cleaned authority after this Task rather than changing against a moving
  Architecture.

## Expected Path Forecast

Expected paths are a forecast, not permission to cross protected identity or
invent meaning. M3 freezes the exact paragraph/link census before M4 edits.

- New shared-result route and contract:
  `.agents/memory/crystallized/documents/cli/contracts/shared/result-coordinates/_result-coordinates.md`,
  `interface.md`, and `behavior.md`, plus the shared-contract entrypoint.
- New Route Update design:
  `.agents/memory/crystallized/documents/cli/contracts/route/update/technical-design.md`,
  plus the Route Update entrypoint.
- New technical-design route:
  `.agents/memory/crystallized/documents/cli/technical-designs/_technical-designs.md`
  with `generated-navigation.md`, `mutation-and-recovery.md`,
  `embedded-payload.md`, `directory-creation.md`, and
  `lifecycle-provenance.md`.
- New distribution authority:
  `.agents/memory/crystallized/documents/cli/distribution.md`.
- New dependency decision:
  `.agents/memory/crystallized/decisions/cli-dependency-policy.md`, plus the
  Decisions route.
- Existing core authority:
  `.agents/memory/crystallized/documents/cli/architecture.md`.
- Exact routing and authority references: the CLI, shared-contract, command-
  contract-set, relevant shared/command/Route Update entrypoints, and only those
  existing command-contract paragraphs that name Architecture as the exact
  owner of meaning moved by this Task. Their observable Interface/Behavior text
  remains byte- and meaning-protected except for exact authority links and
  routing prose.
- Existing process-authority wording:
  `.agents/directives/open-forge/cli/implementation.md`, limited to the accepted
  dependency-role/exact-version authority correction.
- Existing authority maps and stable identity:
  `.agents/maps/sources-of-truth.md` and
  `.agents/memory/working/cli-development/edge-cases.md`.
- Existing Task state: this Task record. Plan, other Tasks, project control,
  Checkpoint, history, and audit records are reference authorities, not expected
  mutation targets.
- Generated `Entries` for routed Markdown changed by M4, produced only by the
  repository generator after semantic edits are complete.

Any directly required neighboring production-authority prose path absent from
this forecast is reported to the Task Mastermind before editing it. No source,
test, package, build, CI, public-documentation, or runtime-projection neighbor is
implicitly authorized.

## Protected Surfaces

The immutable pre-remediation base protects all bytes in CLI production source,
tests, solution/project files, package-manager implementation, CI, build and
dependency configuration, public `docs/`, generated runtime projections,
history, completed Tasks, and audits. This includes `src/cli/**`, CLI test
projects, package sources and metadata, `.github/workflows/**`, root CLI build
configuration, and `Directory.Packages.props`. The directly authorized current
prose routing paths above are the only exception, and only for exact relocation,
links, routing descriptions, and the accepted Directive authority wording.

Public behavior, Interface/Behavior schemas, field/order/type/nullability
coordinates, statuses, exits, streams, grammar, diagnostics, filesystem effects,
recovery and cleanup guarantees, requirement strength, dependency versions,
package semantics, supported-platform semantics, and unresolved uncertainty are
absolutely protected. Generated navigation may change only as the deterministic
projection of authorized routed prose.

The later integration review explicitly authorizes one completed-Task exception:
the stale Route Update edge identity may change from `CLI-EDGE-005` to
`CLI-EDGE-016` in its existing implementation receipt without changing grammar,
behavior, evidence, or any other historical statement. No other completed-Task
mutation is authorized.

## Direct Integration Neighborhood

The rewrite integrates the reduced Architecture with the new result-coordinate,
Route Update, technical-design, distribution, and dependency-decision
authorities; the CLI and contract route entrypoints; the Command Contract Set and
Shared CLI Operation Contract; exact inbound authority references in relevant
command contracts; the CLI Implementation Directive’s version-authority
sentence; Sources Of Truth; the stable edge ledger; this Task state; and
generator-owned Entries. Plan, Task 7, Task 13, project control, Checkpoint,
existing Decisions/configuration, history, audits, and Git are read-only
evidence and destinations for links, not duplicate mutable owners in this Task.

## Evidence Freeze And Acceptance

M3 freezes the immutable old-to-new evidence packet before the coherent rewrite:

- a complete Architecture heading and paragraph inventory mapped to retained,
  relocated, linked, or receipt-only disposition;
- one row for every `T9-ARCH-001` through `T9-ARCH-012`, including old source,
  new exact owner, preserved requirement strength, and inbound/outbound links;
- exact result-coordinate and mutation/recovery matrices proving that no public
  or operational coordinate changes;
- the separate Route List `CLI-EDGE-005` and Route Update `CLI-EDGE-016`
  grammars and identity census;
- exact dependency-version and platform/package authority censuses, including
  central versions only and the accepted target versus present implementation;
- the future-composition invariant and explicit public-contract/signature
  deferrals;
- protected-path identity against the accepted base; and
- expected generated navigation, link, formatting, and diff checks.

M4 may begin only when the Task Mastermind confirms the evidence would fail on a
lost, weakened, strengthened, duplicated, or falsely settled statement. The
single Brilliant Implementer then performs the whole rewrite and focused
validation in one coherent ownership context.

Acceptance requires every finding to have its frozen disposition, every moved
statement to have one exact current owner, all inbound and outbound links to
resolve, generated Entries to match selected sources, formatting and
`git diff --check` to pass, and all protected surfaces to remain identical. One
fresh `T12-R1` whole-task behavior/architecture/evidence review must compare the
result with the immutable base. At most one `T12-C1` grouped correction may
address accepted findings, after which the same focused checks run again. The
final Task record names residual dissent or deferred decisions instead of
presenting them as settled.

## Red Immutable Baseline

The independent M3 Red owner froze the accepted base as commit
`d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
`6959b51e148af44d59512d8bdd801350d88fc651`. The Architecture is 1,053 lines
with 27 H1-H3 headings and SHA-256
`6360cacbb213c2618eb932bd8f05d0c7079ac3a3be0b8db10c9e0d852ca58bfc`.
The Shared CLI Operation Contract is
`9dd95845d1719abaecd53b112f29e0ac2d32d8bbaf19ad579fb9653da118e21e`.
`Directory.Packages.props` is
`2af79560f3e7a11aa53219b609050b3c809ca1a79bd0be01f49ff6b190af7c2f`.

The complete heading disposition, in source order, is: document identity
retain; Status And Authority split; Architectural Goals retain; Project
Criticality And Threat Boundary retain; Physical Workspace split; Project Graph
retain; Root Host Boundary retain; Core Source Organization split; Dependency
Direction retain; Shell Definitions And Composition retain; Native Interaction
retain with compression; Parsing And Invocation split; Execution Pipeline
retain; Result JSON Coordinates And Process Status relocate; Presentation, Help,
And Diagnostics split; Framework Capability Model retain; Workspace retain;
Filesystem And Resolved Path Identity retain; Sources, Routing, And Documents
split; Embedded Framework Distribution split; Lifecycle, Mutation, And Recovery
split; Serialization And Dependencies split; Test Architecture split; Build,
Native AOT, CI, And Artifacts split; Durable Implementation Sequence relocate;
Planning, Tasks, And Delegation relocate; Release Boundary split.

| Finding       | Frozen old coordinates and decisive Green postcondition                                                                                                                                                                                                    |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `T9-ARCH-001` | Architecture 12-17, 433-506, and 510-513 move exactly to the shared result-coordinate contracts; Architecture retains only concrete/source-generated pipeline relationships.                                                                               |
| `T9-ARCH-002` | Architecture 378-389, edge ledger 134-172, and the audited Route Update Task 550-557 separate into Update `CLI-EDGE-016` while List `CLI-EDGE-005` remains unique.                                                                                         |
| `T9-ARCH-003` | Architecture 604-645 moves callable/formation mechanics to Generated Navigation design; observed-plus-intended and dependency direction remain, receipts leave.                                                                                            |
| `T9-ARCH-004` | Architecture 667-889, especially 672-688, 753-862, and 864-888, splits exact realization to Mutation And Recovery design and observable meaning to existing contracts while stage, lock, external-recovery, no-rollback, and provenance invariants remain. |
| `T9-ARCH-005` | Architecture 247-257, 925-944, and 1018-1023 leaves procedure to current Directives/Workflows while structure, tiers, isolation, and system evidence remain.                                                                                               |
| `T9-ARCH-006` | Architecture 897-907 and the Directive dependency sentence leave exact versions solely in `Directory.Packages.props`; the Decision owns rationale and Architecture retains constraints.                                                                    |
| `T9-ARCH-007` | Architecture 952-961 and 973-980 leaves exact tooling mechanics to the existing Decision/configuration/development/delivery sources while artifact and override structure remains.                                                                         |
| `T9-ARCH-008` | Architecture 982-1024, 628-632, and 24-26 leaves live order/state to Plan/Tasks/project control while stable dependency order remains.                                                                                                                     |
| `T9-ARCH-009` | Architecture 1025-1041 leaves operating authority to current orchestration sources while the no-cross-cutting-invention invariant remains.                                                                                                                 |
| `T9-ARCH-010` | Architecture 19-22, 126-129, and 628-632 leaves receipts/history to existing history, Tasks, and Git while consequential current boundaries remain.                                                                                                        |
| `T9-ARCH-011` | Architecture 24-26, 963-980, 1015-1016, and 1043-1053 moves distribution detail to the new authority and Task 7/13 while the thin/no-behavior/no-download/no-postinstall/no-fallback/no-partial invariants remain.                                         |
| `T9-ARCH-012` | Architecture 647-665, 736-751, and 864-882 moves exact payload, directory, and provenance realization to their designs and consumer policy to existing contracts while locations, direction, identity, and separate-effect invariants remain.              |

The shared schema-v1 envelope order is exactly `schemaVersion`, `command`,
`status`, `workspace`, `result`, `next`. Every member is present; `workspace`
and `next` may be null, the command-local concrete `result` is non-null, and no
shared member is duplicated under `result`. `SourceLocation.line` and `column`
are 1-based Unicode-scalar coordinates. `byteOffset` and `byteLength` are
zero-based UTF-8 half-open byte coordinates; location values are nullable only
when unavailable or not applicable with typed evidence. V1 freezes shared field
order, names, types, presence, nullability, finite values, and coordinates.
Command-local additive fields are compatible only when their absence preserves
prior meaning.

| Status        | Exit | Primary human stream |
| ------------- | ---: | -------------------- |
| `complete`    |    0 | stdout               |
| `attention`   |    2 | stdout               |
| `incomplete`  |    3 | stdout               |
| `invalid`     |    4 | stderr               |
| `blocked`     |    5 | stderr               |
| `failed`      |    1 | stderr               |
| `interrupted` |  130 | stderr               |

JSON is one complete stdout document for all seven statuses, bounded diagnostics
remain stderr, and help/version retain their text-only terminal bypass.

Route List `CLI-EDGE-005` accepts only
`--depth=<non-negative-integer|all>`. It rejects bare, spaced, colon, and
attached-empty forms before `--`; inspection stops at `--`, never parses,
counts, or diagnoses after it, and following option-like tokens remain operands.
Route Update `CLI-EDGE-016` applies only after typed parsing proves exactly one
selected `--responsibility` option with zero value tokens; lexical evidence may
then recognize exactly `--responsibility=` or `--responsibility:`. Bare remains
invalid, ordinary valued forms including `--responsibility ""` retain their
contract meaning, inspection stops at `--`, and raw arguments enter neither the
request nor the domain.

Generated Navigation retains observed-plus-intended projection, authoritative
topology, aliases, collision handling, and missing-Loader behavior. Mutation and
recovery retains the application-owned `LocalApplicationData` subtree, an
immutable verified final ZIP before existing-target effects, ordinal manifest
and payload identity, expected-state checks, same-directory replacement,
ordered/coalesced effects, receipts, verification, retained partial state, and
no automatic restore, rollback, or compensation. Embedded-payload hashing and
resource parity, the separate Create-only directory effect, and lifecycle
provenance fields and consumer policy remain exact. Future catalogue composition
and exact signature/public-contract deferrals remain as frozen above.

At Red, all 12 forecast destination files are absent; the old result anchor has
13 occurrences across 5 CLI files; 87 Architecture links exist across 43 CLI
contract files; the paragraph-level moved-authority closure finds 130 candidate
paragraphs across 54 files; the Route Update/List identity collision, prose
version duplication, and live order/receipts remain. Green requires all approved
destinations and generator-owned Entries, zero old result-anchor links, exactly
one owner for every moved claim, preserved generic structural Architecture
links, and clean link, generation, format, and diff evidence. Only paragraphs
that actually name authority moved by this Task may change; the 130-paragraph
inventory is a review closure, not a bulk-edit instruction.

Protected Git-tree manifests use
`git ls-tree -r --full-tree <base> -- <group paths> | sha256sum` and freeze:

- all `src`: 1,436 files,
  `566fc80bf604796f9b046af6f73d7170bfcb42b9bbc8295c77081343f6a2b345`;
- `src/cli`: 1,370 files,
  `4c1e19d7a7d52a8369b3d754e953363c471be44bfc46dcbb72e1da71b33f3e4e`;
- delivery/build configuration (`.github`, the root CLI solution/build/package,
  SDK, NuGet, TypeScript, package-lock, lint, formatting, Git, and editor files):
  19 files,
  `3b297d3b3277453af7a177a9ddadaea47f92154884d6da024f37b5720962f2bd`;
- runtime projections (`.apm`, `.opencode`, `AGENTS.md`, `CLAUDE.md`, and root
  APM/OpenCode projection inputs and locks): 34 files,
  `5fdc7d6c774c58eb7fe93ec74e15281c402f3c7e7d08aeec927bbcee233026f4`;
- public `docs`, `README.md`, and `LICENSE`: 5 files,
  `98e53943d4c1e0797532798fe601ea02c1c25f9fdc46516f465596e188f5dc22`;
- `.agents/memory/archived` and `.agents/memory/working/checkpoints`: 144
  files, `8af7c1440b38870bea2172d17fc6abe28736964b115230b0e9004eb15fd4fef4`;
- the Task 9 audit and its Task record: 2 files,
  `0a4585d00a409d8b483be3c98a5ad29600f74d7245a28f2599da6702da4028fa`.

M3 independently read the complete current C# directives at these SHA-256
identities: `_csharp.md`
`31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
`design.md`
`76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
and `style.md`
`c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.

## Five-Phase, Six-Milestone Horizon

1. **Phase 1 — Preflight and Gray.** M1 verifies the integrated base and obtains
   exact map acceptance. M2 freezes this authority, path, protection,
   composition, distribution, budget, and escalation contract. Both are
   complete.
2. **Phase 2 — Red.** M3 froze the old-to-new evidence packet described above.
   It is complete.
3. **Phase 3 — Rewrite.** M4 assigns one Brilliant Implementer the coherent
   authority rewrite plus focused validation. It is complete.
4. **Phase 4 — Review and bounded correction.** M5 consumes `T12-R1` for one
   fresh whole-task behavior/architecture/evidence review and, only if needed,
   consumes `T12-C1` for at most one grouped correction and revalidation. M5 is
   complete; both stable budgets were consumed exactly once.
5. **Phase 5 — Closeout.** M6 proves protected identity, complete authority and
   link accounting, deterministic generated navigation, final budget state, and
   Task closeout. M6 is complete.

The phases may not be collapsed in a way that removes Gray authority, Red
evidence, fresh review, or protected-identity closeout. No extra review,
correction, council, or parallel implementation wave is implied.

## Stop And Escalation Conditions

Stop before mutation and return the issue to the Task Mastermind if:

- any destination, owner, compatibility consequence, requirement-strength
  mapping, or direct integration path is unresolved or conflicts with a current
  authority;
- relocation would remove, weaken, strengthen, duplicate, or newly settle
  product behavior, schema, status, exit, stream, grammar, diagnostic,
  filesystem, mutation, recovery, cleanup, package, or platform meaning;
- a C# callable signature or public Status/Doctor contract must be chosen before
  Task 15 Gray, or composition would require dependency injection, a service
  locator, reflection, a runtime registry, a generic engine, or ambient
  registration;
- an exact dependency version must change, a new RID/package/platform/libc is
  proposed, ARM must be decided, or current implementation is made to appear to
  satisfy the accepted macOS target;
- any source, test, package, CI, build/configuration, public-documentation,
  generated runtime, history, completed-Task, or audit byte must change outside
  the directly authorized prose routing and deterministic Entries projection;
- generation would change unrelated or protected Entries, an unforecast path is
  more than a directly required authority-integration neighbor, or historical
  receipts would need rewriting;
- the exact base/tree or protected-path identity no longer matches the accepted
  preflight, unrelated work is present in the isolated worktree, or the coherent
  diff cannot remain reviewable within one implementation context; or
- a new product, architecture, compatibility, platform, package, dependency, or
  operating-policy decision is required.

## Gray Completion Evidence

- M1 verified branch, worktree, commit
  `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, and tree
  `6959b51e148af44d59512d8bdd801350d88fc651` before this Task-state edit.
- Maintainer acceptance closes the destination frontier without approving a
  behavior or requirement-strength change.
- The complete current C# directive fingerprints independently read for this
  architecture judgment are:
  - `.agents/directives/csharp/_csharp.md`:
    `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`;
  - `.agents/directives/csharp/design.md`:
    `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`;
  - `.agents/directives/csharp/style.md`:
    `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- M2 changes only this Task record. It does not rewrite an authoritative
  destination, generate navigation, build, test, freeze Task 15 callable
  signatures, or claim implementation progress.
- Current boundary: M6 protected-identity closeout is complete. Task 12 is ready
  for Overseer integration before Task 14 begins.

## M4 Completion Evidence

- M4 created all twelve frozen destinations: the three-file shared result route,
  Route Update Technical Design, the six-file shared Technical Design route,
  CLI Distribution, and the CLI Dependency Policy Decision. It reduced
  Architecture and the Shared CLI Operation Contract, repaired exact inbound
  authority references, updated routing, Sources Of Truth, the one authorized
  Directive sentence, and the live edge ledger, and changed no executable
  source or configuration.
- The coherent diff contains 81 paths including this Task record: 12 new
  permanent authorities, 59 existing CLI contract/routing files, four existing
  core CLI authority/route files, two generator-only direct exposing parents,
  the Directive, Sources Of Truth, edge ledger, and this Task. The only
  additional existing prose neighbor discovered during closure was the routed
  Extension entrypoint, whose literal recovery-bundle owner moved from
  Architecture to the Mutation And Recovery Technical Design.
- Architecture retains all 27 frozen H1-H3 headings in their original order and
  is reduced from 1,053 to 653 lines. Its focused post-rewrite SHA-256 is
  `cd4c3f919d25cc2417fcedd9234a894d4506c5daa1ced011d32c3359f78ccc7c`.
  The focused Shared CLI Operation Contract SHA-256 is
  `2d83005c17ae9ad7e84d71e36b649bf553e5e28ade47322d900b0f6d63dbb44b`.
- The old `architecture.md#result-json-coordinates-and-process-status` anchor has
  zero occurrences. The shared envelope, location, seven-status/exit/stream,
  JSON/help/version, and compatibility matrices match Red. `CLI-EDGE-005`
  remains Route List-only and `CLI-EDGE-016` is separately routed to Route
  Update. The distribution target remains the accepted Linux/macOS/Windows x64
  graph with historical Linux/Windows implementation, current Linux D1, macOS
  follow-up, and undecided ARM represented distinctly. Architecture contains
  the frozen contributor-catalogue composition and no callable signature.
- The native generator was previewed and applied only to six explicit routed
  entrypoints and their direct exposing parents. Its final dry-run checked 39
  regions and made no change. All changed Markdown was formatted before the
  generator restored its owned interiors; the authored/non-generated files pass
  the explicit Prettier check and generator-owned regions pass the final
  idempotence check. Added and relocated local links resolve; the unchanged
  baseline-only Sources Of Truth link to absent derived `dist/` remains outside
  this Task. `git diff --check` is clean.
- Fresh protected manifests against semantic base
  `d3d2dc1362ec1ba03844927f44fefdffb2fd466d` reproduce all frozen file counts
  and hashes exactly: all `src` 1,436/
  `566fc80bf604796f9b046af6f73d7170bfcb42b9bbc8295c77081343f6a2b345`;
  `src/cli` 1,370/
  `4c1e19d7a7d52a8369b3d754e953363c471be44bfc46dcbb72e1da71b33f3e4e`;
  delivery/build configuration 19/
  `3b297d3b3277453af7a177a9ddadaea47f92154884d6da024f37b5720962f2bd`;
  runtime projections 34/
  `5fdc7d6c774c58eb7fe93ec74e15281c402f3c7e7d08aeec927bbcee233026f4`;
  public documents 5/
  `98e53943d4c1e0797532798fe601ea02c1c25f9fdc46516f465596e188f5dc22`;
  archive/checkpoints 144/
  `8af7c1440b38870bea2172d17fc6abe28736964b115230b0e9004eb15fd4fef4`;
  and the Task 9 audit/record 2/
  `0a4585d00a409d8b483be3c98a5ad29600f74d7245a28f2599da6702da4028fa`.
  The worktree diff across those protected paths is empty.
- M4 independently read the complete current C# directives and recomputed
  `_csharp.md`
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `design.md`
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and `style.md`
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
  `Directory.Packages.props` remains byte-identical at
  `2af79560f3e7a11aa53219b609050b3c809ca1a79bd0be01f49ff6b190af7c2f`.
- No executable build or test ran because M4 changes only authority prose and
  deterministic generated Entries while every source, test, project, package,
  and build surface is byte-identical. M5 provides the required fresh
  whole-task review; M4 does not claim that review or consume `T12-R1`.

## M5 Review And Grouped Correction

- The Task Mastermind consumed the one `T12-R1` review directly against commit
  `6399ed471fb2e28b29d4572cfccf94a3e2a18a04`, tree
  `93c143466c05ea22846d4ab2022da8b16903ff71`. It independently read the complete
  current C# Directives at `_csharp.md`
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `design.md`
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and `style.md`
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- The review inspected every `T9-ARCH-001` through `T9-ARCH-012` disposition,
  the complete 27-heading Architecture inventory, all twelve destinations,
  command-contract reference movement, requirement strength, generated routes,
  protected identities, platform truth, and the future Status/Doctor catalogue
  boundary. Result coordinates, Route List and Route Update identities, shared
  realization designs, distribution, dependency rationale, stable-order versus
  live-state placement, and history/evidence placement otherwise matched Gray
  and Red without a public, implementation, or uncertainty change.
- `T12-R1` accepted one material finding: the CLI Implementation Directive still
  named `System.CommandLine` `2.0.11`, leaving one exact dependency-version
  authority outside `Directory.Packages.props`. The same Brilliant Implementer
  consumed `T12-C1` once and changed only that sentence at commit
  `89ca2eafd9dcd14b5d64da61a294fff731ea97d8`, tree
  `a31597c23bf14832725a04e9f75af25d9f3dc8cf`. The centrally pinned package now
  retains the same accepted long-option forms and package-change re-verification
  obligation without duplicating its current version.
- Focused revalidation found no remaining dependency-version literal in current
  CLI Directives, crystallized CLI documents, or the dependency Decision; exact
  package versions occur only in `Directory.Packages.props`. Command-contract
  Extension package examples retain product versions rather than dependency
  pins. The correction passes Prettier and `git diff --check`, preserves the
  generated 40-region no-op, and changes no protected byte.
- At M5, no other material behavior, architecture/structure, or evidence-quality
  finding was accepted. The review and correction budgets are exhausted; no
  council or second Task review was used. The later integration findings are
  distinct and recorded below.

## M6 Closeout Evidence

- All twelve approved destinations exist and are routed. Architecture retains
  all 27 H1-H3 headings in source order, contains zero old shared-result anchor
  references, and owns no relocated exact dependency version, generated-
  navigation callable, recovery-store/lock/ZIP/file-application mechanism,
  embedded-resource mechanism, directory-create mechanism, lifecycle field
  realization, package graph, or live program receipt. Direct generic structural
  Architecture links remain, while exact shared result and realization links
  resolve to their new owners.
- `OperationalContributorCatalogue` remains one immutable application-scoped
  catalogue built explicitly by `CliCompositionRoot`; producer-owned typed
  contributors provide narrow Status and Doctor views from fresh invocation
  observations. Composition changes no public contract, introduces no DI,
  service locator, reflection, runtime registry, generic engine, or ambient
  registration, and leaves callable signatures to Task 15 Gray.
- Distribution preserves the accepted Linux, macOS, and Windows x64 target while
  distinguishing Task 7's historical Linux/Windows package baseline, the macOS
  follow-up gap, Task 13's current Linux D1 ownership, and undecided ARM. This
  Task changes no package, platform, RID, libc, release, or implementation fact.
- Before the integration correction, the final authority diff contained 82
  Markdown paths. Twelve are new permanent
  authorities; existing changes are limited to the accepted Architecture,
  contract/routing, Directive, Sources Of Truth, edge-ledger, Task-state, and
  deterministic generated-Entries neighborhood. The post-closeout correction
  raises the final range to 84 Markdown paths by adding the authorized Route
  Update Task identity correction and the generator's directly affected Route
  Mutation parent normalization. No JavaScript, C#, project, package, build, CI,
  public-documentation, runtime-projection, other completed-Task, history, or
  audit path changed.
- The explicit six-source generator selection checks 40 regions with zero
  updates and 40 verified already-current regions. After the Route Update Task
  joins the final selection, 41 regions are current. Prettier passes all 70
  authored/non-generated changed Markdown files; the 14 generator-owned
  entrypoints remain canonical under the generator. Added and relocated local
  links resolve. The five unresolved local-link strings are unchanged baseline
  examples or the pre-existing derived `dist/` map target. `git diff --check` is
  clean.
- Protected Git-tree manifests reproduce Red exactly: all `src` 1,436/
  `566fc80bf604796f9b046af6f73d7170bfcb42b9bbc8295c77081343f6a2b345`;
  `src/cli` 1,370/
  `4c1e19d7a7d52a8369b3d754e953363c471be44bfc46dcbb72e1da71b33f3e4e`;
  delivery/build 19/
  `3b297d3b3277453af7a177a9ddadaea47f92154884d6da024f37b5720962f2bd`;
  runtime projections 34/
  `5fdc7d6c774c58eb7fe93ec74e15281c402f3c7e7d08aeec927bbcee233026f4`;
  public documents 5/
  `98e53943d4c1e0797532798fe601ea02c1c25f9fdc46516f465596e188f5dc22`;
  archive/checkpoints 144/
  `8af7c1440b38870bea2172d17fc6abe28736964b115230b0e9004eb15fd4fef4`;
  and Task 9 audit/record 2/
  `0a4585d00a409d8b483be3c98a5ad29600f74d7245a28f2599da6702da4028fa`.
  `Directory.Packages.props` remains byte-identical at
  `2af79560f3e7a11aa53219b609050b3c809ca1a79bd0be01f49ff6b190af7c2f`.
- No executable build or test was selected because the final diff changes only
  authority prose and deterministic generated navigation and all executable,
  schema, package, project, and build bytes are identical. Residual owned work is
  explicit: Task 15 Gray freezes contributor signatures; Task 7 owns the macOS
  package gap and graph realization; Task 13 remains Linux D1; ARM remains
  undecided. Task 14 may consume this cleaned authority only after integration.

## Post-Closeout Integration Review And Correction

- One separately assigned Sol/xhigh integration review inspected completed head
  `a47243aac75d0a9414fd5ca9196277618b38238c`, tree
  `566f154f02e3478cb6fa5ee2c2340fdc4989b684`, and returned two material
  findings. This review is not a second Task-owned `T12-R1` review, and its
  grouped repair does not alter the exhausted `T12-C1` budget.
- `INT-T12-R1` found the completed Route Update implementation receipt still
  assigning its attached-empty responsibility recognizer to `CLI-EDGE-005` after
  Task 12 made that identity Route List-only. The explicitly authorized
  completed-Task correction changes that one identity to `CLI-EDGE-016` without
  changing the accepted `--responsibility=`/`--responsibility:` grammar, typed
  precondition, `--` boundary, request/domain isolation, evidence, or any other
  historical statement. The corrected Route Update Task has one
  `CLI-EDGE-016` occurrence and zero `CLI-EDGE-005` occurrences; the edge ledger
  retains one distinct heading for each identity.
- `INT-T12-R2` found five Status authority references inconsistent: two new
  Interface claims and one new Behavior claim made those contracts diagnostic-
  field owners, while two existing Interface references routed exact fields and
  redaction differently. The grouped correction makes all five state one
  boundary: exact Status diagnostic fields and redaction remain bounded
  command-local implementation details under CLI Architecture and Gate 5
  evidence. It enumerates no field, creates no public diagnostic contract, and
  changes no status, result, stream, output, or redaction requirement. The
  existing Behavior statement that bounded diagnostics use stderr and follow
  Architecture remains unchanged.
- The same Brilliant Implementer owned the exact three semantic correction
  paths: the Route Update Task receipt plus Status Interface and Behavior. The
  Task Mastermind alone updated this Task state and the hand-authored Tasks route;
  the Task's generated entry already remains `Complete`, and the native generator
  canonicalized the directly affected Route Mutation parent. Both owners
  independently read the complete C# Directives at `_csharp.md`
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `design.md`
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and `style.md`
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- Focused revalidation reports zero Status Interface/Behavior diagnostic-
  ownership claims, 72 local links checked across the three semantic paths with
  zero missing, Prettier and `git diff --check` clean, and the final explicit
  seven-source generator selection at 41 regions with zero updates. The
  protected manifests and
  `Directory.Packages.props` identity remain the exact M6 values above. No
  executable build or test is selected because the correction changes only
  authority/identity prose and no protected executable or public-contract byte.
