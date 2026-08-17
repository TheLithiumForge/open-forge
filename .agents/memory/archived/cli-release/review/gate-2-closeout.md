---
open-forge:
  description: Rejected/superseded Queue 30 analysis of CLI slicing, remaining commands, and Gate 2 handoff
  responsibility: Preserve Packet 3 product analysis for maintainer review without becoming Decision Agenda, command-contract, gate-state, or release authority
  tags: [Memory, Archived, CLI, Release, Review, Packet, Gate, Product, Candidate, Contextual, Historical]
---

# Gate 2 Product Closeout History

## Status And Authority

This was Packet 3 of 3 in the former Queue 30 review sequence. The maintainer
rejected and superseded its first-release/closeout proposal. It is now
contextual historical analysis, not an active review, accepted Decision, command
contract, Gate record, or release record. The [review route](gate-2-review-overview.md) keeps
that boundary explicit. The maintainer decides every product, scope, safety, and
acceptance question.

The current program state remains Gate 2 open and non-shipping. The new CLI is
not shipped.

The former proposal did not accept `CLI-D005` or `CLI-D018`, and it did not add
taxonomy or behavior acceptance beyond the then-current high-level `CLI-D015`
and `CLI-D017A` directions. The maintainer has since rejected its first-release
slice and retained-later release slicing. This file still does not author an
Interface or Behavior Contract, change Gate 2 state, promote Working knowledge,
or claim implementation, binary, package, dogfooding, or release evidence.
Historical command counts and deleted CLI-v2 proposals are context only.

Queue 29's accepted Framework and Extension lifecycle direction is now
integrated into the current Install, Update, Extension, Status, Doctor, Agenda,
overview, [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md), and
public documentation sources. Packet 2 remains
contextual review history and does not become a second authority. This packet's
recommendations were not accepted. It does not define a release slice, begin
Gate 3, or authorize implementation.

## Decision Frame

This historical frame asked whether the CLI had a smallest useful first release,
an honest disposition for every other proposal, and a clean handoff into
Architecture without silently promising implementation or release work. The
maintainer rejected that slicing frame. Current command-definition work is
tracked by Queues 31–33 and the current Release Plan.

The criteria are:

- complete the highest-value agent job with the smallest publishable and
  dogfoodable read-only slice;
- preserve exact Framework meaning, source identity, ownership, and generated
  boundaries;
- keep destructive work explicit, bounded, recoverable, and honest about
  idempotence;
- distinguish retained contracts from first-release inclusion, future product
  work, and rejected surfaces; and
- leave Gate 3 responsible for architecture and Gate 5 responsible for release
  proof.

### Why the frontier remained open

| Item                               | Why it remained open                                                                                                                                                   | Direction already accepted                                                                                                                         |
| ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `CLI-D005` first useful release    | Accepted contracts and Gate 1 measurements did not select a first slice. There is no target command count.                                                             | The CLI is an optional agent-first accelerator with a predictable human surface.                                                                   |
| `CLI-D015` move/remove             | The Agenda accepts dedicated explicit structural mutation, but not its taxonomy, source boundary, reference coverage, ownership, recovery, or repeated-operation rule. | Writes require explicit planning, authority, verification, and recovery. Generic batch `apply` is not accepted.                                    |
| `CLI-D017A` cleanup                | Artifact identity, retention, backup recovery, spelling, and the boundary against lifecycle work remain open.                                                          | Cleanup is an explicit CLI product concern, must inspect before deleting, preserve recovery, reject ambiguity, support dry-run, and be idempotent. |
| `CLI-D018` completion              | Script generation, shell or package responsibility, and selected command-library feasibility remain open.                                                              | Shell-profile editing and install/remove profile lifecycle are rejected. Script generation is optional when it is standard, small, and safe.       |
| Exhaustive disposition and handoff | Existing contracts are not a release slice, while old proposals are not current authority. The Release Plan also repeats physical cleanup in Gate 2 and Gate 4.        | The Review Queue places this closeout after the Queue 29 lifecycle rereview; Gate 3 remains blocked until Gate 2 is resolved.                      |

## Sources Consulted

The linked source remains authoritative for its own question. This packet links
those sources rather than replacing them.

- **Product and program:** [Decision Agenda](../../../working/cli-release/decision-agenda.md), [Release
  Plan](../../../working/cli-release/release-plan.md), [CLI Release Checkpoint](../../../working/checkpoints/cli-release.md),
  [Contract Migration Ledger](../contract-migration-ledger.md), and the [CLI
  release route](../_cli-release.md).
- **Contract topology and current contracts:** [Command Contract Set](../../../crystallized/documents/cli/command-contract-set.md),
  [Interface view](../../../crystallized/documents/cli/command-interface-contract.md), [Behavior view](../../../crystallized/documents/cli/command-behavior-contract.md),
  [Technical Design view](../../../crystallized/documents/cli/command-technical-design.md), [Context Interface](../../../crystallized/documents/cli/contracts/context/interface.md),
  [Context Behavior](../../../crystallized/documents/cli/contracts/context/behavior.md), [Find Interface](../../../crystallized/documents/cli/contracts/find/interface.md),
  and [Find Behavior](../../../crystallized/documents/cli/contracts/find/behavior.md).
- **Shared contract meaning:** [shared contract route](../../../crystallized/documents/cli/contracts/shared/_shared.md),
  [Global Flags Interface](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md) and
  [Behavior](../../../crystallized/documents/cli/contracts/shared/global-flags/behavior.md), [Source References
  Interface](../../../crystallized/documents/cli/contracts/shared/source-references/interface.md) and [Behavior](../../../crystallized/documents/cli/contracts/shared/source-references/behavior.md),
  and [Source Universe Filters Interface](../../../crystallized/documents/cli/contracts/shared/source-universe-filters/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/shared/source-universe-filters/behavior.md).
- **Reference and lifecycle safety:** [References Interface](../../../crystallized/documents/cli/contracts/references/interface.md)
  and [Behavior](../../../crystallized/documents/cli/contracts/references/behavior.md), [Doctor
  Interface](../../../crystallized/documents/cli/contracts/doctor/interface.md), and [Doctor Behavior](../../../crystallized/documents/cli/contracts/doctor/behavior.md).
- **Preceding review packets:** [Packet 1 — Framework lifecycle](framework-lifecycle.md)
  records the settled Queue 28 direction and remains contextual historical
  review. The [Packet 2 — Framework and Extension lifecycle
  revision](extension-lifecycle.md) records Queue 29's accepted direction and
  remains contextual history after its result was integrated into the current
  contracts and Agenda. Neither packet is a second authority.
- **Framework boundaries:** [Routing Model](../../../crystallized/documents/framework/routing/model.md),
  [Route Scope and Inheritance](../../../crystallized/documents/framework/routing/scope.md),
  [Routing Paths and Identity](../../../crystallized/documents/framework/routing/paths.md),
  [Overwrite Customization](../../../crystallized/documents/framework/routing/overwrites.md),
  [Routing Loading and Continuity](../../../crystallized/documents/framework/routing/loading.md),
  and [Routed Markdown and Generated Regions](../../../crystallized/documents/framework/markdown/routes.md).
- **Evidence and review history:** [Gate 1 Audit](../gate-1-audit.md), [Gate 1
  Findings](../gate-1-findings.md), [Review Queue](queue.md), and the archived
  [route-family review history](../../../archived/cli-release/review/route-family-contract-migration-council.md).
- **Current shared operation contract:** [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md).

The [historical CLI-v2 archive](../../../archived/cli-v2/_cli-v2.md) and the
proposal inventory in the Gate 1 Audit are used only to avoid losing context.
They do not supply current command names, product direction, or evidence.

## Review Queue Placement

This packet was routed as [Queue 30](queue.md#prepared-review-history). Queue
30's first-release/closeout proposal was rejected and superseded by the
maintainer's full-retained-delivery direction. The file remains discoverable as
contextual history only. Queue 29's disposition and accepted integration are
complete; neither its generated review entry nor this historical queue placement
accepts this packet's recommendations or changes its contextual authority.

## First-Round Lenses

These were independent analytical lenses, not votes. Each separated facts,
recommendation, and dissent before reconciliation.

### Cold product and release slice

The current job is to discover exact sources and then assemble or read ordered
startup or selected context. `find` provides a flat, exact source inventory and
tag or heading discovery. `context` resolves startup and explicit closures and
returns ordered projections or authored content. Their current contracts are
read-only, stateless, deterministic, and based on the accepted source-reference,
projection, result, and Framework foundations.

This lens recommends `context` plus `find` as the first release. Its dissent is
that a broader Explore/Understand/Verify bundle, adding `status`, route
catalogue or inspection, references, and doctor, would offer better orientation,
relationship explanation, and health feedback with fewer later calls. It loses
on the smallest publication and dogfood scope: it adds several distinct result
and coverage boundaries before the core discover-then-read loop has evidence.

### Grounded operations and safety

The Framework distinguishes route meaning, physical paths, source identity,
scope, management, overwrite layers, authored links, and generated navigation.
The current shared contracts require exact source IDs or paths, containment,
collision handling, logical base/overwrite pairing, and non-interactive blocking
of unresolved choices. The Pattern requires one typed operation, one plan for
dry-run and apply, explicit authority, and verification or recovery for writes.

This lens recommends keeping all mutation, lifecycle, cleanup, and completion
out of the first slice. It supports a future `route move`/`route remove` family
only with a narrow leaf boundary and complete reference and recovery planning.
Its dissent is that a safety-oriented release could include `doctor` or `index`
early to establish a visible maintenance gate. That value is real, but neither
operation is necessary to complete the first read-only agent job, and current
contract or source presence is not implementation evidence.

### Adversarial simplifier

This lens attacks every surface that could turn an accelerator into a general
workspace operator. It asks whether `context` alone would be enough, rejects
generic apply, semantic search, sessions, receipts, profile lifecycle, and
unbounded cleanup, and treats move/remove as a future safety project rather than
an early demonstration of capability.

It accepts the two-command slice only because `find` closes the exact-discovery
half of the job that `context` alone leaves to manual browsing. It also warns
that the proposed guided cleanup shape may be more product design than the
available evidence proves, and that completion should disappear rather than
become a command promise if the selected AOT-compatible library cannot provide
a small standard generator.

## Focused Reconciliation

The reconciliation considered only material disagreements. It did not average
the lenses or count agreement.

| Question                                               | Reconciled candidate                                                                                                                                                                                                                                                                                                                                                                      | Dissent preserved                                                                                                                                                                            |
| ------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| How small should the first slice be?                   | Select exactly `context` and `find`. Together they complete exact discovery followed by ordered context reading without mutation or lifecycle.                                                                                                                                                                                                                                            | `context` alone is smaller; the broader Explore/Understand/Verify bundle is more convenient. Both remain the strongest alternatives and are rejected only for this smallest-scope objective. |
| When must future commands be fully contracted?         | Decide product disposition and architecture-relevant boundaries in Gate 2. Require full Interface and Behavior Contracts before implementation or release of a retained command. If Queue 29's superseding lifecycle revision is accepted, author the accepted Framework Install/Update and Extension contracts before Queue 30 advances because lifecycle semantics affect architecture. | Requiring every future contract before opening Gate 3 would delay architecture without resolving implementation questions; implementing from a disposition alone would be unsafe.            |
| How should destructive repeat operations be described? | Narrow the universal verified-no-op wording for consumed-source operations. A verified move may be followed by a missing-source/invalid non-mutating result, because a path-derived ID cannot prove provenance.                                                                                                                                                                           | A future stable identity or proof model could restore a verified no-op. No receipt, tombstone, or hidden state is proposed now.                                                              |
| How much cleanup shape is supported by evidence?       | Retain cleanup for later. Treat the guided leaf below as an explicit product choice, not as a fact forced by Gate 1 evidence.                                                                                                                                                                                                                                                             | The core safe artifact boundary is stronger than the exact catalogue, prompt shape, temporary directory, and retention policy. Those remain for maintainer judgment and Gate 3 design.       |

## Genuine Candidate Improvements

The candidate improves the open frontier without reopening accepted direction:

- It turns an unselected first-release question into one small end-to-end
  read-only job instead of a command-count target.
- It gives D015 a route-family boundary, a complete reference and generated
  navigation plan, an ownership hard stop, and an honest consumed-source retry
  rule without inventing persistent state.
- It makes D015's reference coverage honest: `.agents` layers are the rewrite
  universe, while detected or uninspectable incoming links from supported
  workspace Markdown outside `.agents` block rather than being silently broken.
- It requires positive lifecycle evidence for management classification instead
  of treating route shape, tags, generated entries, or matching bytes as proof.
- It keeps D017A useful while separating recognized CLI artifacts from arbitrary
  repository and Gate 4 knowledge cleanup, without an automatic deletion mode.
- It makes D018 a conditional presentation convenience rather than a profile or
  lifecycle promise and assigns script installation to the shell, package
  manager, or user.
- It replaces duplicate Gate 2/Gate 4 process ownership with a handoff that lets
  Architecture follow accepted product closure without claiming release proof.

These are candidate improvements to the decision record and future contracts.
They do not alter the current accepted command contracts or Framework sources.

## Mastermind Candidate Synthesis

The following is one candidate recommendation for the maintainer. It is not an
Agenda update or a command contract.

### 1. First useful release: `context` and `find`

The first useful release should be exactly `context` plus `find`, limited to
already-initialized workspaces. This completes the agent job **“discover exact
sources, then assemble/read ordered startup or selected context.”** The pair
shares the accepted exact workspace and source-reference model, parsed-source
and projection foundations, and one typed-result direction while remaining
read-only and stateless.

The first slice excludes `status`, `route list`, `route inspect`, `references`,
`doctor`, `index`, `repair`, `route init`, `route create`, `route update`, all
lifecycle operations, `route move`, `route remove`, `cleanup`, and completion.
Those exclusions are release slicing, not rejection. The existing accepted
contracts remain retained for later slices. Gate 3 defines the architecture that
can support the slices, and Gate 5 proves the binary, package, wrapper, and
release behavior.

The strongest losing alternative is a broader read-only Explore/Understand/Verify
bundle: `find` and `context` plus `status`, route list/inspect, references, and
doctor. It would improve orientation, structural explanation, relationship
inspection, and health diagnosis. It loses because it is not the smallest
publication or dogfood scope, combines several independent coverage and result
boundaries, and would make a first release claim depend on more unproven surface
than the core job requires.

### 2. Gate 2 closure and contract timing

Gate 2 can close with an accepted product boundary, minimal coherent taxonomy,
first-release slice, explicit disposition for every proposal, Framework-versus-
CLI boundaries, non-goals, deferrals, and the accepted current contract set.
Full Interface and Behavior Contracts are required before implementing or
releasing a retained command, but not necessarily before opening Gate 3.

Queue 29 is the explicit lifecycle exception. If its superseding direction is
accepted, author the accepted Framework Install/Update and Extension Interface
and Behavior Contracts promptly because installation, managed ownership,
reconciliation, replacement, recovery, and removal semantics affect the shared
architecture. This is not permission to implement them or to treat the packet
as accepted before review and integration.

### 3. Future mutation taxonomy and boundary (`CLI-D015`)

The future taxonomy should be grouped `route move` and `route remove`, not root
generic `move`, `remove`, or `apply` commands. This is a product boundary for
later contract authoring, not the future public grammar.

The initial future scope is one ordinary, unambiguously routed, unmanaged
Markdown logical leaf per invocation. Entrypoints, subtrees, the Loader,
`SKILL.md`, native resources, lifecycle-managed sources, receipt-owned sources,
and batch moves or removals are deferred. Source subjects reuse the shared
source-reference ID or exact `.agents/...` path grammar. The destination is one
exact ordinary routed target under an existing valid route. Exact-path
disambiguation never bypasses route, containment, ownership, or generated-safety
checks.

### 4. Logical source and ownership boundary

A base file and its adjacent overwrite companion are one logical source. A move
or removal operates on both layers atomically. An orphan or ambiguous overwrite
blocks rather than becoming an independent source.

For this candidate, `lifecycle-managed` means that an exact trusted Framework
lifecycle baseline or an Extension receipt or manager claim identifies the
selected file or managed region. Tags, a parent route, a path or familiar slug,
generated `Entries`, and matching bytes never prove management or lack of
management. Missing, malformed, or ambiguous lifecycle evidence blocks an
ownership-sensitive move or removal. Only a positively established unmanaged
ordinary routed leaf qualifies for this future family.

Managed, receipt-owned, or uncertain-ownership content blocks and directs the
caller to the relevant Framework or Extension lifecycle operation. The future
operation does not adopt content, change ownership, repair receipts, or update
receipt state.

### 5. Candidate move plan

A move plan must cover, in one plan:

- the exact source and destination and any destination collision;
- incoming authored references across every eligible physical `.agents` Markdown
  layer, including base and overwrite layers;
- path-sensitive outgoing references in the moved layers;
- old and new generated parents and their bounded generated regions;
- ownership, containment, Git cleanliness, adjacent-backup recovery, expected
  state, application, and complete verification.

The current [References Interface](../../../crystallized/documents/cli/contracts/references/interface.md)
and [References Behavior](../../../crystallized/documents/cli/contracts/references/behavior.md) define
the direct incoming and outgoing occurrence facts and the complete eligible
`.agents` scan boundary. Within that rewrite universe, rewrite only exact,
resolvable local authored references whose target meaning is known. Preserve
labels, fragments, and unrelated bytes. Report external URLs and leave them
unchanged.

Framework ordinary links may leave `.agents`, and supported Markdown elsewhere
inside the selected workspace may point into `.agents`. Therefore the move must
also run a complete physically contained supported-workspace-Markdown detection
pass for incoming links from outside `.agents`. A detected outside incoming link
blocks the move and is never rewritten. If that broader detection universe cannot
be bounded or completely inspected, the result is `incomplete` and no move
occurs. Exact file enumeration, exclusions, and parser realization remain Gate 3
choices. The product safety boundary is no silent breakage outside the rewrite
universe. No fuzzy, semantic, proximity-based, or relevance-based rewrite is
allowed.

The current [Doctor Interface](../../../crystallized/documents/cli/contracts/doctor/interface.md) and [Doctor
Behavior](../../../crystallized/documents/cli/contracts/doctor/behavior.md) remain read-only diagnosis boundaries.
They may report local-reference, recovery, route, Framework-lifecycle, and
Extension-lifecycle coverage, but they do not authorize, plan, or perform a
move or removal. The future mutation contracts must consume these facts without
turning Doctor recommendations into authority.

### 6. Candidate remove plan

A remove plan covers the logical base/overwrite pair and the old parent's
generated navigation. Existing incoming authored references block. Removal never
deletes referring prose and never semantically rewrites it.

Removal uses the same complete eligible `.agents` reference scan and the same
physically contained supported-workspace-Markdown detection pass as move. Any
incoming reference from either universe blocks before writes and is never
deleted or rewritten. Incomplete or unbounded reference coverage is
`incomplete`, and no removal begins.

A missing source may be a complete absence/no-op only when desired absence is
independently provable. An orphan or ambiguous residual, generated boundary, or
ownership state blocks. The operation does not turn “not found” into proof that a
previous destructive request succeeded.

### 7. Destructive authority and preview

The recommendation is that the explicit `route move` or `route remove` command
together with its exact subjects is itself the confirmation and consent
boundary, including JSON and other non-interactive application. This matches the
current explicit [route-write contracts](../../../crystallized/documents/cli/contracts/route/_route.md), where the
operation and exact target supply authority rather than a second generic
confirmation mode. A separately named confirmation flag or prompt is a losing
alternative if the maintainer wants stronger friction, not part of this
recommendation. Do not add generic `--yes` or `--force`, `--automatic`, a saved
plan, or generic `apply`. Keep `--dry-run` as the sole preview and keep
`--skip-git-check` limited to the Git cleanliness check. An unresolved source or
effect blocks rather than prompting, choosing, or guessing for a destructive
operation. Planned destruction alone does not create `attention`.

These are candidate product constraints. Future Interface and Behavior Contracts
must still define complete request, result, status, output, recovery, and
verification meaning before implementation or release.

### 8. Honest idempotence exception for consumed sources

The current broad statement that every repeated successful write reports a
verified no-op is not honest for a move that consumes its source. A successful
move can be deterministic and fully verified, but a later identical request
using the old path-derived identity cannot prove that the missing source was
consumed by that earlier move rather than removed or changed elsewhere.

The candidate decision is therefore to narrow the universal no-op language for
consumed-source operations: a later identical old-source request returns a
non-mutating missing-source/invalid result instead of falsely claiming a
verified no-op. A future stable identity or proof model could remove this
exception. Do not hide the exception or invent receipts, tombstones, or other
persistent provenance to avoid it. If accepted, this choice must amend the
broad repeated-write statement in the authoritative Agenda and any affected
contract before implementation.

### 9. Future CLI cleanup (`CLI-D017A`)

Root `cleanup` remains accepted product work for a later slice. It is not Gate 4
repository knowledge cleanup. The former is a user-facing operation over
recognized CLI-owned recovery or temporary artifacts. The latter is a maintainer
transition of repository knowledge after architecture, including physical
Working-to-Crystallized promotion, temporary label and path cleanup, and
Template, document, map, and history reconciliation.

The candidate product shape is a guided leaf, but this exact bare shape remains a
maintainer choice:

- explicit recognized artifact operands select exact candidates and are required
  for non-interactive or JSON application;
- a bare human invocation may open a finite selection wizard;
- a bare JSON or other non-interactive invocation with no semantic selection is
  `invalid` and never prompts; and
- `--dry-run` and `--skip-git-check` retain their shared meanings.

No automatic cleanup selection or deletion is proposed. The candidate follows
the [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
and [CLI-D086](../../../working/cli-release/decision-agenda.md#contract-system-decisions) by requiring
explicit semantic input for non-interactive application rather than using an
automatic deletion mode. The current automatic-selection rule cannot authorize
deletion.

It removes only recognized CLI-owned operation temporary or residual artifacts
that are proven unnecessary. An adjacent `.bak` is eligible only after proof
that recovery no longer needs it. It leaves generic repository `.temp/`, raw
snapshots, arbitrary `.bak` files, build output, receipts, user files, and
unknown, ambiguous, or still-needed artifacts untouched. It uses no age or glob
heuristic, receipt repair, or lifecycle cleanup.

The exact artifact names, temporary directory, and retention implementation
belong to Gate 3. The safe retained-later disposition is the strong conclusion;
the guided shape is a marked product choice that may be narrowed or rejected
without reopening `CLI-D017A`'s core boundary.

### 10. Conditional shell completion (`CLI-D018`)

Completion is optional later script generation only. A candidate public shape is
`completion <shell>` or `completion script <shell>`, to be chosen only if the
selected command library supports a standard, small, safe generator.

Completion does not mutate a workspace, edit a shell profile, install or remove
a lifecycle component, configure itself automatically, or enter the first
release. It is a presentation of the command surface, not semantic process
completion and not an authority or release-readiness signal. If the selected
AOT-compatible library cannot provide the generator safely, defer it as an
explicit conditional work disposition rather than retaining an unresolved
command promise. The maintainer would accept that condition with the rest of
the D018 disposition.

When supported, the CLI only emits the script. The shell, package manager, or
user owns installing, removing, and sourcing it. The CLI does not own a profile
lifecycle or automatic configuration. If the generator is omitted, no dangling
package or shell responsibility remains in the CLI; any later distribution
mechanics require their own decision.

## Exhaustive Candidate Disposition

“Retained later” means the proposal is not in the first slice. “Future” means
the product boundary is candidate material and still needs a command contract.
“Rejected” means no current command promise should be carried forward.

| Proposal                                                                   | Candidate disposition                                                         | Current authority or boundary                                                                                                                                                                                                                             |
| -------------------------------------------------------------------------- | ----------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `context`                                                                  | **First release**                                                             | [Context contracts](../../../crystallized/documents/cli/contracts/context/_context.md); startup or selected ordered context, read-only.                                                                                                                                                     |
| `find`                                                                     | **First release**                                                             | [Find contracts](../../../crystallized/documents/cli/contracts/find/_find.md); exact flat inventory and tag or heading discovery, read-only.                                                                                                                                                |
| `status`                                                                   | Retained later with existing contract                                         | [Status contracts](../../../crystallized/documents/cli/contracts/status/_status.md); not first-release orientation.                                                                                                                                                                         |
| `route list`                                                               | Retained later with existing contract                                         | [Route List contracts](../../../crystallized/documents/cli/contracts/route/list/_list.md); not first-release topology.                                                                                                                                                                      |
| `route inspect`                                                            | Retained later with existing contract                                         | [Route Inspect contracts](../../../crystallized/documents/cli/contracts/route/inspect/_inspect.md); not first-release profiling.                                                                                                                                                            |
| `references`                                                               | Retained later with existing contract                                         | [References contracts](../../../crystallized/documents/cli/contracts/references/_references.md); not first-release relationship inspection.                                                                                                                             |
| `doctor`                                                                   | Retained later with existing contract                                         | [Doctor contracts](../../../crystallized/documents/cli/contracts/doctor/_doctor.md); accepted read-only diagnosis, not release inclusion.                                                                                                                                                   |
| `index`                                                                    | Retained later with existing contract                                         | [Index contracts](../../../crystallized/documents/cli/contracts/index/_index.md); generated navigation remains a later operation and automatic write postcondition.                                                                                                     |
| `repair`                                                                   | Retained later with existing contract                                         | [Repair contracts](../../../crystallized/documents/cli/contracts/repair/_repair.md); accepted targeted repair, not first-release mutation.                                                                                                                                                  |
| `route init`, `route create`, `route update`                               | Retained later with existing contracts                                        | [Route write contracts](../../../crystallized/documents/cli/contracts/route/_route.md); current contracts remain accepted and are not rejected by slicing.                                                                                                                                  |
| Framework and Extension lifecycle and local package creation               | Retained later; Queue 29's accepted unified lifecycle direction is integrated | Queue 29 added root Framework `install`/`update`, a six-leaf Extension family, shared lifecycle facts, AST fingerprints, and no current core uninstall. The current contracts and Agenda are authoritative; the review packet remains contextual history. |
| `route move`, `route remove`                                               | Retained future                                                               | Use the narrow unmanaged logical-leaf boundary, complete reference plan, explicit destructive authority, and consumed-source retry exception above.                                                                                                       |
| `cleanup`                                                                  | Retained future                                                               | Keep CLI artifact cleanup separate from Gate 4 knowledge cleanup; explicit operands for non-interactive/JSON application and a finite human wizard remain a product choice, with no automatic deletion.                                                   |
| Shell completion                                                           | Conditional optional defer                                                    | CLI emits only when safely supported; shell/package manager/user owns installation, removal, and sourcing. No profile lifecycle, automatic configuration, or first-release inclusion.                                                                     |
| Generic `apply` or batch mutation                                          | **Rejected**                                                                  | No generic saved-plan or batch-apply authority; future mutation remains command-specific.                                                                                                                                                                 |
| Semantic, fuzzy, ranked, or relevance search                               | **Rejected**                                                                  | Exact `find` predicates remain the discovery boundary.                                                                                                                                                                                                    |
| Sessions, context receipts, or hidden persistent context state             | **Rejected**                                                                  | Context remains stateless; no receipt-based suppression or session promise.                                                                                                                                                                               |
| Standalone graph, neighborhood, outline, provenance, or `discover` command | **Rejected**                                                                  | Keep projections and relationships on their owning operations and use organized help.                                                                                                                                                                     |
| `check`/`fix` aliases                                                      | **Rejected**                                                                  | `doctor` and `repair` retain their distinct accepted boundaries.                                                                                                                                                                                          |
| Route rebuild or reindex aliases                                           | **Rejected**                                                                  | `index` is the accepted generated-navigation operation; aliases add no job.                                                                                                                                                                               |
| Completion profile lifecycle                                               | **Rejected**                                                                  | Shell-profile editing and install/remove lifecycle are outside the CLI.                                                                                                                                                                                   |
| Rune                                                                       | **Rejected and outside this effort**                                          | The accepted Rune boundary remains unchanged.                                                                                                                                                                                                             |

Historical 3-, 16-, 17-, and 19-leaf counts, `load`/`enter`/`read` naming,
former lifecycle names, and CLI-v2 command inventories do not add rows or
authority to this table.

## Gate 2, Gate 4, and Gate 3 Handoff

The current [Release Plan](../../../working/cli-release/release-plan.md) repeats physical promotion and
Template reconciliation in the Gate 2 exit list and the Gate 4 knowledge
cleanup section. The candidate correction is:

- **Gate 2 exit owns:** accepted product vision and minimal taxonomy, first
  release, every proposal disposition, Framework-semantic versus CLI-mechanical
  boundaries, accepted current contracts, and explicit non-goals and deferrals.
- **Gate 4 owns after Architecture:** physical Working-to-Crystallized
  promotion, removal of competing Working copies, temporary Find and Index
  labels and path cleanup, and Template, documentation, map, package/build,
  generated-navigation, and history reconciliation.

This removes duplicate process. Before Gate 2 closure under this proposed
process, the accepted transition must explicitly amend the [Release Plan](../../../working/cli-release/release-plan.md)
Gate 2 exit, the [CLI release route](../_cli-release.md), the [Contract Migration
Ledger](../contract-migration-ledger.md), the affected [Command Contract Set](../../../crystallized/documents/cli/command-contract-set.md),
[Interface](../../../crystallized/documents/cli/command-interface-contract.md), [Behavior](../../../crystallized/documents/cli/command-behavior-contract.md),
[Technical Design](../../../crystallized/documents/cli/command-technical-design.md), and [Checkpoint](../../../working/checkpoints/cli-release.md),
and the [Decision Agenda](../../../working/cli-release/decision-agenda.md). Those authoritative updates
must state that Gate 4 owns physical promotion, Template reconciliation, and
temporary-label/path cleanup. Do not use this proposed handoff while current
authority still assigns those responsibilities to Gate 2.

After that authoritative integration and validation, Architecture can follow
technical Gate 2 closure while Gate 4 waits for the architecture it needs.

The future general Architecture should own cross-cutting C#/.NET modules and
dependencies, filesystem identity and containment, serialization and schemas,
concurrency, planning and recovery, diagnostics, testing boundaries, and
distribution. Command Technical Designs should remain absent or minimal unless
there is a genuinely command-local technology choice. A local Technical Design
cannot become a second Interface, Behavior, or Architecture source.

## Post-Approval Sequence

1. Packet 1 — Framework lifecycle records the settled Queue 28 direction. Its
   direct-root `install` result is historical context, and the Packet 1 file
   remains contextual history.
2. Queue 29's [Framework and Extension lifecycle revision](extension-lifecycle.md)
   received explicit acceptance. Its root Framework Install/Update, six-leaf
   Extension, Status, Doctor, Agenda, overview, Shared CLI Operation Contract, and documentation
   integration is complete; Packet 2 remains contextual history.
3. The maintainer rejected Queue 30's first-release and retained-later slicing
   proposal. Keep this file as contextual history and continue with the
   sequential Queue 31–33 remaining-command reviews instead.
4. After each remaining command disposition is explicitly accepted, amend the
   authoritative Agenda, command contracts, Release Plan, CLI release route,
   affected program views, and Checkpoint as needed. Do not treat this historical
   packet as the integration.
5. Complete the full Architecture discussion and maintainer acceptance before
   implementation. Then crystallize the important accepted contracts and
   Architecture Documents, run the scoped prose and routing validations, and
   keep Gate 2 and Gate 3 state explicit.
6. Do not begin implementation or release proof from this packet. The complete
   retained CLI, not a `context` plus `find` slice, is the delivery target.

## Historical Maintainer Checklist

The checklist below records the questions that Queue 30 would have put to the
maintainer. Its first-release and retained-later questions were rejected with
the packet; the remaining command questions now have their own Queue 31–33
packets.

- [x] Reject the exact `context` plus `find` first-release slice and the
      retained-later release slicing proposal; the full retained CLI is the
      delivery target.
- [x] Confirm that Queue 29's unified lifecycle
      revision is integrated into the current Install/Update/Extension
      contracts and Agenda/program records.
- [ ] Review the grouped `route move`/`route remove` taxonomy and the ordinary
      unmanaged logical-leaf boundary in Queue 31.
- [ ] Review the logical base/overwrite and positive management-proof boundary:
      lifecycle evidence must be a trusted Framework baseline or Extension
      receipt/manager claim, and absence of a tag, route, path, generated entry,
      or matching bytes is not proof of unmanaged content.
- [ ] Confirm the `.agents` rewrite universe, the complete physically contained
      outside-`.agents` incoming-link detection pass, its incomplete/no-write
      stop, generated-parent, Git/recovery, and no-fuzzy-rewrite boundaries.
- [ ] Decide whether explicit destructive commands and exact subjects are the
      complete confirmation/consent boundary, including JSON/non-interactive
      use. Preserve the separately named confirmation prompt or flag only as a
      losing alternative, with no generic `--yes` or `--force`.
- [ ] Decide the consumed-source idempotence exception and amend the broad
      repeated-write statement if accepted. Do not add receipts or tombstones by
      implication.
- [ ] Review the cleanup disposition and separately decide whether the proposed
      guided shape is worth preserving as product detail in Queue 32.
      No automatic cleanup selection or deletion is included; explicit operands
      are required for JSON/non-interactive application.
- [ ] Review the conditional script-only completion disposition in Queue 33,
      including the possibility of rejecting the command when safe standard
      generation is unavailable. Shell/package manager/user ownership of
      installation, removal, and sourcing remains outside the CLI.
- [x] Retain Packet 1, Packet 2, and this Queue 30 packet as contextual history.
      Continue only with explicit decisions in Queues 31–33, then complete the
      Architecture prerequisite before implementation.

## Acceptance Consequences And Exclusions

If accepted, the candidate recommendations would update the authoritative
Agenda and program records, not this packet's status. They would establish the
first-release boundary, future product dispositions, the move retry exception,
and the Gate 2/Gate 4 handoff. They would not implement a command, create a
package, move a source, remove an artifact, generate a shell script, promote a
Working file, or produce release evidence.

Before Gate 3, this packet does not choose a .NET baseline or command library,
Markdown or YAML parser, serialization/schema format, numeric exits, filesystem
identity realization, concurrency or lock design, backup names, source/module
layout, test implementation, Native AOT mechanics, package artifacts, wrapper
behavior, supported platforms, or distribution proof. Gate 3 Architecture and
Gate 5 release evidence remain separate decisions and evidence boundaries.

The two extra product choices that must not be hidden are whether the maintainer
accepts the narrower consumed-source idempotence rule and how much of the guided
cleanup shape to preserve. The first-release, retained-later, future, conditional,
and rejected dispositions are all candidate recommendations, not acceptance;
the maintainer may accept, revise, or reject each one.

## Queue 30 disposition

The maintainer rejected and superseded this packet's first-release/closeout
proposal. In particular, `context` plus `find` is not a publication target, and
retained accepted commands are not held for a later release. This file preserves
its analysis only to explain the rejected direction and its useful constraints.
Queue 31 `route move`/`route remove`, Queue 32 `cleanup`, and Queue 33 completion
now carry the remaining command decisions. Gate 2 remains open, Gate 3 remains
blocked, and no implementation or release evidence follows from this history.
