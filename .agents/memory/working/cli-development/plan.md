---
open-forge:
  description: Executable top-down work graph for completing the greenfield replacement CLI
  tags: [Memory, Working, CLI, Plan, Architecture, Development, Contextual, Active]
---

# Replacement CLI Development Plan

## Task And Planning Boundary

- Task: [Complete The Replacement CLI](tasks/00-cli-development.md).
- Plan state: Active.
- Planning authority: The maintainer accepts consequential decisions. The
  Mastermind owns architecture, sequencing, Task decomposition, integration, and
  Plan maintenance within that direction.
- Last updated: 2026-08-22.
- Current step: Route Inspect is squash-integrated into `develop` at `bd5d280`; generic improvements are accepted on `feature/cli-generic-improvements`. Parser remediation, active-test architecture, and callable/project architecture are Complete; the final gate from clean `7871764` passes managed Unit `580/580`, Integration `213/213`, and EndToEnd `57/57`; local `win-x64` Native AOT root with managed EndToEnd `57/57`; Native AOT Integration `213/213`; Native AOT EndToEnd `57/57`; package/artifact/public audits; and integrated review. The accepted branch's authorized squash-merge into `develop` is next.
- Route-inspect baseline and sequence: exact `edca509` (`Establish and accept route list`) on `feature/cli-route-inspect`; planning commit `37d2e70`; contracts/evidence (Complete) → resolution/promotion (Complete at `a54f4e0`) → profile (Complete at `c407e24`) → presentation (Complete from production commit `51c0960`) → behavior-neutral locality correction (Complete at `9c690b4`) → integrated acceptance and Route Discovery closeout (Complete).
- Maintainer-authorized continuation: after route-inspect acceptance, squash-merge this focused branch into `develop`; create one generic-improvements branch, record one full-suite baseline, and first remediate parser/raw-argument/special-edge deviations under the updated standard-behavior Directive and pinned `System.CommandLine` semantics; only then improve reusable test fixtures, followed by the closed migration to named component inputs with the accepted root-host/Core split retained; run the full suite once at final acceptance, accept and squash-merge that branch into `develop`, and only then begin the next product Task. Feature-branch commits are pre-authorized after each Working-only planning boundary.
- Active generic-improvements Task: [Improve Generic CLI Structure](tasks/generic-improvements/_generic-improvements.md). Feature-branch acceptance is met with zero skips and no blocker; commit the final record, then squash-merge into `develop` and verify the integrated tree before the next product Task.

This Plan defines how the accepted replacement CLI reaches complete local and
release acceptance. The CLI Architecture and command contracts define what the
system means. Child Tasks define bounded outcomes and acceptance. This Plan
defines dependencies, order, integration gates, evidence, and resumption.

## Planning Basis

- Outcome: One complete, predictable, Native-AOT replacement executable and thin
  package wrappers implement every retained command without importing or falling
  back to the frozen MVP.
- Acceptance: Every command contract, cross-command invariant, filesystem and
  mutation safety boundary, six native RIDs, package journey, support floor,
  supply-chain artifact, and release gate has reproducible evidence and maintainer
  acceptance.
- Initial starting point: Greenfield implementation with product contracts and
  selected prior evidence but no production C# source or active C# workspace.
  The current `edca509` baseline contains the accepted scoped workspace,
  Foundation, and route-list implementation.
- Integration risk: High. The work crosses a complete command tree, filesystem
  identity, persisted lifecycle state, mutation and recovery, Native AOT, package
  distribution, and public release.
- Non-goals: Legacy compatibility, partial publication, runtime plug-ins, a fake
  filesystem, native interop, a universal domain engine, and implementation before
  its parent architecture and Task are ready.

### References And Authority

| Source                                                                                     | Question it answers                                       | Status or authority                                                        | Use in this Plan                            |
| ------------------------------------------------------------------------------------------ | --------------------------------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------- |
| [CLI Architecture](../../crystallized/documents/cli/architecture.md)                       | How is the replacement structured and integrated?         | Accepted current architecture                                              | Governs all steps                           |
| [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md)           | Which command-local sources define behavior?              | Accepted current contract map                                              | Selects command Tasks                       |
| [Detailed Contracts](../../crystallized/documents/cli/contracts/_contracts.md)             | What must each command and shared operation do?           | Accepted current product contracts                                         | Requirements and evidence                   |
| [Shared Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md) | Which conventions cross commands?                         | Accepted current contract                                                  | Foundation and integration                  |
| [Program Architecture Directive](../../../directives/program-architecture.md)              | Who owns architecture and when is delegation ready?       | Binding workspace Directive                                                | Task readiness and integration              |
| [CLI Directive](../../../directives/open-forge/cli/_cli.md)                                | What rules apply to replacement work?                     | Binding CLI Directive                                                      | Every CLI step                              |
| [Architectural Perspectives](../../../guidance/architectural-perspectives.md)              | Which top-down and task-master questions apply?           | Accepted Guidance for this program                                         | Planning and review horizon                 |
| [Task Template](../../../templates/memory/task.md)                                         | Which fields make one Task durable?                       | Experimental local Template                                                | Child Task shape                            |
| [Plan Template](../../../templates/memory/plan.md)                                         | Which fields make coordinated execution resumable?        | Experimental local Template                                                | This Plan's shape                           |
| [Implementation Reset](../../archived/cli-release/implementation-reset-2026-08-21.md)      | What was useful or harmful in the removed implementation? | Historical evidence                                                        | Avoid rediscovery and sunk-cost restoration |
| Maintainer-supplied review files under `.temp/review-20.08.2026/`                          | What did the independent architecture review find?        | Historical external evidence accepted where projected into current sources | Task detail and risk checks                 |
| [Replacement CLI edge-case ledger](edge-cases.md)                                           | Which deferred edge cases need owner resolution or explicit acceptance? | Active working evidence | Route discovery and delivery gate tracking |
| [CLI development flow evaluation](../../emerging/observations/2026-08-21_cli-development-flow-evaluation.md) | Which current-flow lessons should shape later slices? | Contextual Emerging observation | Route-inspect planning and review |

## Approach

Complete the program top down:

1. Freeze the physical workspace, project graph, dependency direction, and shell
   call surfaces.
2. Author the complete Task hierarchy before production source returns.
3. Implement the actual route-free foundation in one integrated architecture
   increment owned by the Mastermind.
4. Close shared safety and Framework fact foundations before commands consume
   them.
5. Implement read-only commands in dependency order, promoting only identical
   facts proved by real consumers.
6. Establish lock, lifecycle, mutation, recovery, and Git foundations before the
   first mutating command.
7. Implement mutations from narrow route operations to extension and root
   lifecycle operations.
8. Implement aggregate status, diagnosis, repair, and cleanup only after all state
   producers exist.
9. Complete package, six-RID, supply-chain, support-floor, documentation, and
   release evidence without partial publication.

Each step ends in one inspectable commit. Architecture and cross-cutting callable
contracts stay with the Mastermind. A smaller implementer receives one closed
child Task and exact predecessor outputs. A reviewer receives the exact commit or
diff, parent requirements, and claimed evidence.

## Prerequisites

| ID  | Prerequisite        | Required state and evidence                                                             | Responsible source or role        | Blocks                        |
| --- | ------------------- | --------------------------------------------------------------------------------------- | --------------------------------- | ----------------------------- |
| P1  | Product contracts   | Current contract route is complete and conflicts are explicit                           | Crystallized CLI contracts        | All command Tasks             |
| P2  | Greenfield boundary | Old production removed and useful evidence preserved                                    | Commit `40ba03e` and reset record | Foundation                    |
| P3  | Architecture        | Physical, project, dependency, call-surface, evidence, and sequence boundaries accepted | CLI Architecture                  | Task authoring and foundation |
| P4  | Task governance     | Top-down and task-master perspectives are binding                                       | Program Architecture Directive    | Delegation                    |
| P5  | Local toolchain     | Stable .NET 10 SDK and native prerequisites are available                               | Foundation verification           | Foundation acceptance         |

## Resources

| Resource                           | Purpose                                                | Availability or source | Needed by              | Responsible role |
| ---------------------------------- | ------------------------------------------------------ | ---------------------- | ---------------------- | ---------------- |
| Stable .NET 10 SDK                 | Build, test, format, publish, and AOT                  | Local and CI setup     | F1 onward              | Mastermind       |
| Six native runners                 | Final RID and support-floor evidence                   | CI                     | D1                     | Release Task     |
| Real OS temporary filesystems      | Identity, containment, mutation, and no-write evidence | TestSupport            | F4 and commands        | Owning Task      |
| Git repositories                   | Mutation and recovery evidence                         | Isolated fixtures      | M1 onward              | Owning Task      |
| Preserved test inventory           | Candidate expectations and fixtures                    | `src/cli/tests/`       | Relevant command Tasks | Task creator     |
| Independent advisors               | Named architecture or safety uncertainty only          | Optional               | Decision points        | Mastermind       |
| Bounded implementers and reviewers | Closed implementation and fresh diff review            | After Task readiness   | Commands               | Mastermind       |

## Work Graph

| ID  | State    | Action and observable result                                                                                                | Depends on | Lane        | Task group           | Verification                                 |
| --- | -------- | --------------------------------------------------------------------------------------------------------------------------- | ---------- | ----------- | -------------------- | -------------------------------------------- |
| S0  | Complete | Preserve WIP, define architecture delegation rules, and reset old production                                                | None       | Sequential  | Historical           | Commits `4b873de`, `aa7d178`, `40ba03e`      |
| S1  | Complete | Define the complete top-down Architecture and executable Plan                                                               | S0, P1-P4  | Sequential  | Program              | Current sources and link checks              |
| S2  | Complete | Author the complete hierarchical Task set with closed foundation and command boundaries                                     | S1         | Sequential  | `tasks/`             | Task graph audit and backlinks               |
| F1  | Complete | Create the scoped C# workspace, project graph, dependencies, artifacts, and preserved-test quarantine                       | S2, P5     | Sequential  | Foundation           | Restore/build topology and no root C# files  |
| F2  | Complete | Implement Shell definitions, invocation, composition contracts, parser, pipeline, output, and serialization with no command | F1         | Foundation  | Foundation           | Unit and integration evidence                |
| F3  | Complete | Implement the thin root host and explicit route-free composition                                                            | F2         | Foundation  | Foundation           | Managed process and terminal evidence        |
| F4  | Complete | Implement workspace, filesystem identity, typed reads, and physical-containment foundations                                 | F2         | Foundation  | Safety foundation    | Real-OS matrix including leave-and-reenter   |
| F5  | Complete | Establish active Unit, Integration, EndToEnd, TestSupport, Native AOT, and CI foundations                                   | F1-F4      | Sequential  | Foundation           | Managed and published process evidence       |
| G1  | Complete | Accept the actual command-free architectural foundation                                                                     | F1-F5      | Sequential  | Foundation gate      | Full diff, dependency audit, AOT execution   |
| R1  | Complete | Accept the complete `route list` slice and keep shared route facts local until route inspect proves identical consumers     | G1         | Sequential  | Route discovery      | Complete contract and public scenario        |
| R2  | Complete | Split and close route-inspect child Tasks before implementation; then implement and accept `route inspect` and promote proved shared route facts | R1         | Sequential  | Route discovery      | List and inspect regressions                 |
| GI1 | Active   | Remediate parser behavior, improve active-test architecture, and accept the closed callable/root-host structure before the next product Task | R2 | Sequential | Generic improvement | Beginning/final complete suites and focused phase evidence |
| Q1  | Pending  | Implement and accept `find`                                                                                                 | GI1        | Read-only A | Source queries       | Contract, CommonMark, process, AOT           |
| Q2  | Pending  | Implement and accept `references`                                                                                           | GI1        | Read-only B | Source queries       | Direct-reference evidence                    |
| Q3  | Pending  | Implement and accept `context` after Q1 and Q2 facts stabilize                                                              | Q1, Q2     | Sequential  | Context              | Ordered context and token evidence           |
| E1  | Pending  | Implement and accept `extension list` and `extension inspect`                                                               | G1, GI1    | Read-only C | Extension discovery  | Catalogue and package-source evidence        |
| I1  | Pending  | Implement and accept `index`                                                                                                | Q1-Q3, R2  | Sequential  | Generated navigation | Idempotence and unchanged-authority evidence |
| M1  | Pending  | Implement shared lock, lifecycle, mutation, recovery, and Git foundations                                                   | I1, E1     | Sequential  | Mutation foundation  | Direct failure and crash-boundary evidence   |
| M2  | Pending  | Implement route init/create/update/move/remove in dependency order                                                          | M1, R2, I1 | Sequential  | Route mutation       | Per-command public and recovery evidence     |
| M3  | Pending  | Implement extension create and root install/update                                                                          | M1, E1, I1 | Sequential  | Lifecycle mutation   | Package, lifecycle, and workspace evidence   |
| M4  | Pending  | Implement extension install/update/remove                                                                                   | M3         | Sequential  | Extension mutation   | Collision, recovery, and catalogue evidence  |
| O1  | Pending  | Implement status and doctor from all produced facts                                                                         | M2-M4      | Sequential  | Operations           | Complete aggregate and diagnostic evidence   |
| O2  | Pending  | Implement repair and cleanup                                                                                                | O1         | Sequential  | Operations           | Plan/apply/recovery and idempotence evidence |
| D1  | Pending  | Implement wrappers, package graph, six-RID CI, supply chain, support floors, and docs                                       | O2         | Delivery    | Distribution         | Packed journeys and native matrix            |
| A1  | Pending  | Run final whole-program acceptance and local integration                                                                    | D1         | Sequential  | Acceptance           | Maintainer acceptance; no partial release    |

### Parallel Lanes

Parallelism begins only after the shared predecessor is committed and each lane
has non-overlapping production and test ownership.

| Lane          | Steps                          | May start when                                       | Owned surfaces                                | Shared dependency                    | Integration point |
| ------------- | ------------------------------ | ---------------------------------------------------- | --------------------------------------------- | ------------------------------------ | ----------------- |
| Foundation    | F2, F4 preparation             | F1 complete; callable contracts frozen by Mastermind | Separate Shell and Framework capability paths | Core models and project graph        | F5                |
| Read-only A/B | Q1, Q2                         | GI1 complete and shared source contracts frozen      | Separate Find and References command roots    | Routing, documents, source catalogue | Q3                |
| Read-only C   | E1                             | G1 and GI1 complete; extension source contract frozen | Extension List/Inspect roots                 | Shell and filesystem foundation      | M1                |
| Delivery      | CI, packages, docs preparation | O2 behavior complete; release contracts frozen       | Separate workflow, wrapper, and docs paths    | Published native artifacts           | D1 acceptance     |

No parallel implementation may change the same shared capability. Promotion or
cross-lane contract changes return to a sequential Mastermind integration step.

## Step Rules

### S2: Task Authoring

- Instantiate one parent program Task, one Task-group entrypoint per phase, and one
  Task per coherent independently accepted result.
- Give foundation Tasks accepted class maps, project paths, dependencies, and
  exact evidence.
- Give command Tasks contract matrices, predecessor facts, local models, promoted
  capability rules, test disposition, and public scenarios.
- Split a Task into child or subchild files when separate ownership, state,
  evidence, or integration justifies it. Keep checklists inside a Task when another
  file would add only ceremony.
- Do not add production source in S2.

### F1-F5: Actual Foundation

- The Mastermind authors the first foundation directly from the Architecture and
  foundation Tasks.
- The result is an actual retained host and Core, not a probe or spike.
- No command symbol, route behavior, fake operation, or Foundation-named domain
  model is introduced merely to prove plumbing.
- Tests prove shell stages, terminal modes, process boundaries, serialization,
  filesystem safety, and AOT without inventing a retained command.

### Command Steps

- Begin with a contract-to-evidence matrix and preserved-test disposition.
- Freeze command-local definitions, request, result, binding, and shared-fact
  dependencies before behavior delegation.
- Implement one complete command. Do not create placeholders for later commands
  beyond symbol/help entries explicitly required by current product help.
- Promote a semantic unit only at the integration point where a second real
  consumer proves identical meaning.
- End with managed, process, unchanged-state, AOT, diff, and architecture evidence.

### Mutation And Delivery Steps

- Mutation Tasks separate planning from application and prove revalidation after
  lock acquisition.
- Recovery and lifecycle schemas are frozen by the Mastermind before command
  implementation.
- Delivery Tasks consume accepted binaries. Wrappers never reproduce behavior.

## Decision Points

| ID  | Decision                                                                             | Current direction                                           | Decision-maker                       | Needed before                | Result if reopened                                 |
| --- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------- | ------------------------------------ | ---------------------------- | -------------------------------------------------- |
| D1  | Can the managed BCL prove required component-wise physical identity on every target? | Prove with real OS and AOT evidence; no interop             | Maintainer after Mastermind evidence | F4 acceptance                | Narrow Architecture return                         |
| D2  | Does `route init` gain a Framework-shape mode?                                       | Deferred; current command contract remains authoritative    | Maintainer                           | M2 route-init Task           | Update contract, architecture, Tasks, and evidence |
| D3  | Does a dependency version need replacement?                                          | Keep accepted exact versions until evidence requires change | Maintainer                           | Owning foundation/command    | Focused dependency decision and full AOT proof     |
| D4  | Do perspectives, Tasks, or Plans become Framework primitives?                        | No; continue local trial                                    | Maintainer                           | After several complete Tasks | Separate Framework proposal, not CLI scope         |

## Risks, Recovery, And Stop Conditions

| Risk or trigger                                           | Affected steps | Safeguard                                                        | Recovery or stop response                                             |
| --------------------------------------------------------- | -------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| Local command design bypasses the system architecture     | All commands   | Parent links, frozen foundation, Mastermind integration          | Reject local implementation and return to parent Task                 |
| A Task still contains an architecture choice              | S2 onward      | Task readiness audit                                             | Keep Task blocked; resolve in Architecture first                      |
| Preserved tests anchor obsolete structure                 | Command Tasks  | Map behavior to current contracts before porting                 | Rewrite fixture or test; never restore structure for test convenience |
| Shared capability is promoted without identical consumers | R2 onward      | Promotion evidence in integrating Task                           | Move it back to narrow scope or split semantics                       |
| Filesystem safety cannot be proved portably               | F4, mutations  | BCL-first real-OS and AOT matrix                                 | Stop and return to D1; do not weaken or add native code               |
| Native AOT differs from managed behavior                  | F5 onward      | Publish and execute affected boundaries every increment          | Reject managed-only pass; correct or reopen dependency                |
| Mutation leaves unverified partial state                  | M1 onward      | Plan, lock, revalidate, apply, verify, recovery evidence         | Block command acceptance and preserve owned fixture evidence          |
| Parallel lanes modify shared contracts                    | Parallel work  | Non-overlapping paths and sequential integration                 | Stop lanes and integrate one accepted contract first                  |
| Task records become stale bureaucracy                     | S2 onward      | Update only at state/evidence boundaries; prune completed detail | Consolidate outcomes and archive or prune temporary records           |

## Verification And Integration

| Gate           | Inputs                                 | Verification                                                                                                  | Pass condition                                                        | Resulting update         |
| -------------- | -------------------------------------- | ------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------- | ------------------------ |
| VG0 Planning   | Architecture, Plan, Task hierarchy     | Link, hierarchy, dependency, scope, and stop-condition audit                                                  | Every implementation Task is closed or explicitly blocked             | Foundation authorized    |
| VG1 Foundation | F1-F5                                  | Restore, format, build, unit, integration, end-to-end host, dependency audit, local AOT publish and execution | Clean route-free architecture with no probe or root C# clutter        | G1 accepted              |
| VG2 Command    | One command and affected shared facts  | Focused managed tests, full regressions, public process scenario, unchanged-state check, local AOT            | Contract complete with no architectural debt deferred to next command | Next command authorized  |
| VG3 Mutation   | M1 and one mutation command            | Failure matrix, lock/revalidation, planned effects, Git/recovery, idempotence, process and AOT                | No unverified partial state or hidden lifecycle behavior              | Next mutation authorized |
| VG4 Delivery   | Complete commands and release surfaces | Six native RIDs, support floors, packed wrappers, checksums, signatures, SBOM, provenance, attestation, docs  | Complete non-shipping candidate                                       | Final acceptance         |
| VG5 Release    | VG4 and maintainer review              | Main-only release procedure and public smoke tests                                                            | Maintainer explicitly accepts shipping release                        | Release and closeout     |

## Coordination And Continuity

- Child Tasks: The `tasks/` hierarchy created in S2.
- Checkpoint: [CLI Development Checkpoint](../checkpoints/cli-development.md).
- Handoffs: None. Create one only for an actual transfer.
- Related plans: The removed release Plan is historical at
  `../../archived/cli-release/release-plan-2026-08-21.md`.
- Update points: After S2, every foundation gate, every accepted command, each
  shared promotion, mutation foundation acceptance, delivery acceptance, and any
  Architecture return.
- Resumption path: Read the CLI Architecture, this Plan, the Checkpoint, the
  selected parent Task, and the active leaf Task. Then execute the leaf Task's
  stated next action.

## Completion

This Plan completes only when every child Task is accepted or deliberately
cancelled, all retained commands and release surfaces are integrated, complete
managed and native evidence passes, current Architecture and contracts match the
implementation, temporary records are consolidated, and the maintainer accepts
the release. Until then the replacement remains non-shipping.
