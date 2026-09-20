---
open-forge:
  description: Contextual Queue 29 review history for the accepted unified Framework and Extension lifecycle
  responsibility: Present a contextual superseding lifecycle model for maintainer rereview without creating command authority
  tags: [Memory, Archived, CLI, Release, Review, Council, Candidate, Contextual, Framework, Extension, Lifecycle, Historical]
---

# Framework and Extension Lifecycle Review Packet

**Packet:** Queue 29, Packet 2 of 3; accepted and integrated, retained as history

**Classification:** `#Contextual` candidate analysis. This file is not an
accepted Interface Contract, Behavior Contract, Decision, implementation record,
or Gate 2 closeout.

## 1. Status, authority, and revision reason

This packet records the **superseding product model** prepared for Framework and
Extensions. At preparation time it was a candidate against the accepted Queue 28
direction. The maintainer later accepted Queue 29 and the result was authored
into the current [Install Interface Contract](../../../crystallized/documents/cli/contracts/install/interface.md),
[Install Behavior Contract](../../../crystallized/documents/cli/contracts/install/behavior.md), root Update and
Extension contracts, [CLI-D016](../../../working/cli-release/decision-agenda.md#command-decisions), and
[CLI-D091](../../../working/cli-release/decision-agenda.md#contract-system-decisions). Those current
sources now answer the product meaning; this packet preserves the reasoning,
dissent, and review evidence without becoming a second authority.

[Packet 1 — Framework lifecycle](framework-lifecycle.md) remains contextual
history of that earlier accepted direction. It is useful evidence about the
single-operation decision, its safety model, and its dissent. It is not a second
current contract. [Packet 3 — Gate 2 product closeout](gate-2-closeout.md) was
Queue 30 and is now rejected/superseded contextual history. Its first-release
slicing and closeout proposal does not govern the current program; Queues 31–33
own the remaining command reviews.

The revision was needed because the earlier single `install` model did not make
the newly unified lifecycle intent explicit enough across two different managed
subjects:

- the embedded Framework, whose source and recognized footprint are fixed by the
  running CLI; and
- Extensions, whose source, stable package identity, dependency closure, shared
  ownership, and removal boundary differ from Framework installation.

The candidate therefore separated `install` from `update` at the product level,
used the same transparent lifecycle facts and safety stages for both subjects,
and kept Extension package operations under an `extension` group. This was a
deliberate supersession of the Queue 28 direction. The follow-up authoring
sequence is complete, and existing current contracts win wherever this packet
differs.

In particular, the accepted direction moved ordinary trusted managed Framework
reconciliation out of `install` and into `update`, and narrowed initial
`install --force` to eligible unestablished occupants. That is a deliberate
supersession of the current Queue 28 install meaning, not an implicit contract
edit.

Nothing in this packet claims that the new CLI ships, that any command is
implemented, that the Gate 1 Extension divergence was repaired, or that Gate 2
or Gate 3 has closed or begun implementation. Acceptance changed documents and
product direction only.

## Queue 29 disposition — 2026-08-16

The maintainer accepted the unified Framework and Extension lifecycle direction
as recorded in [CLI-D091](../../../working/cli-release/decision-agenda.md#contract-system-decisions). The
accepted result is integrated into the current Install, Update, Extension,
Status, Doctor, overview,
[Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md), and public
documentation sources. This
packet remains `#Contextual` review history. Its candidate matrices, examples,
tradeoffs, and dissent explain the accepted direction but do not replace the
linked current contracts. Queue 30 was later rejected and superseded; Queues
31–33 now cover the remaining command reviews. Queue 29 acceptance does not
close Gate 2, begin Gate 3, or authorize implementation.

## 2. Maintainer direction and Queue 29 choices

### Direction already supplied

The following constraints are already accepted or are current evidence that
this candidate must respect:

- The CLI is an optional, agent-first Framework accelerator with a predictable
  human surface. There is no target command count. Each permanent operation must
  justify its value and cost.
- The current new CLI direction is .NET Native AOT, but detailed architecture,
  dependencies, packaging, testing, and implementation remain deferred.
- Framework meaning remains in the Framework sources. Extensions add complete
  files through ordinary routes; installed files retain the meaning of their
  destination routes and remain usable without package metadata or the CLI.
- Queue 28 accepted the direct root Framework `install` surface as the earlier
  integrated authority. Queue 29 records and supersedes that direction through
  the current Install and Update contracts.
- CLI-D017 accepts the Extension lifecycle capability direction while leaving
  exact command names, dependencies, and managed-state scope open.
- CLI-D041 through CLI-D049 supply the current plan, dry-run, affected-path Git,
  verification, recovery, user-content, formatter, and external-source
  boundaries. This packet specializes them at product level and does not silently
  change the Agenda.
- CLI-D086 and the [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
  already own the general guided-leaf model. This packet applies that model; it
  does not propose a duplicate Directive or Guidance source.
- Plain-file, idless, and direct-overlay Extension installation remains valid
  ordinary unmanaged content. Routing, matching bytes, or a familiar path never
  transfers ownership.
- At Queue 29 review time, Queue 30 was intended to decide a first useful release
  slice and Gate closeout. The maintainer later rejected that slicing frame and
  accepted full retained-CLI delivery instead. This packet remains lifecycle
  history, not a release commitment.

### Choices reviewed in Queue 29

The Queue 29 review asked the maintainer to accept, revise, or reject the
following superseding candidate as a whole or by clearly stated parts. The
accepted result is recorded in the current contracts and Decision Agenda; this
list preserves the review surface and its tradeoffs:

1. The root Framework taxonomy changes from the currently integrated one-leaf
   lifecycle to `install` for management establishment and exact no-op, and
   `update` for trusted managed reconciliation.
2. The Extension family uses exactly `list`, `inspect`, `create`, `install`,
   `update`, and `remove`, with `--force` and `--prune` as bounded dimensions on
   update and `--force` as a cautious initial-install authority.
3. The exact candidate syntax below, including source selection, `--all`,
   `--automatic`, and the absence of a Framework uninstall/remove leaf, is the
   right product surface.
4. A single future transparent lifecycle document may contain separate logical
   `framework` and `extensions` sections without merging their authority.
5. Parser/AST-derived, format-insensitive semantic fingerprints are the managed
   identity comparison, with exact bytes retained for operation-time planning and
   verification and fail-closed equivalence.
6. The candidate formatter direction revises the earlier D047 shape only if this
   packet is accepted: no CLI formatter execution or formatter receipt is needed
   for lifecycle identity, while conservative formatter detection and advice may
   remain informational.
7. The status, stream, output, dependency, ownership-release, removal, and
   recovery rules below are sufficient for later Interface and Behavior
   authoring.

At preparation time, acceptance of this packet still required the explicit
authoring sequence at the end. That sequence is now complete. The packet itself
did not change the current contracts, check an Agenda item, close Gate 2, or
authorize implementation; the linked authoritative sources record the accepted
result.

## 3. Current evidence and authority map

Each source answers a separate question. A link preserves that source's
authority; it does not promote this review file or merge the sources.

| Question                                                            | Source used here                                                                                                                                                                                                                                                                                                                                                                                                                                           | Boundary                                                                                                                                                                                                                            |
| ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Current accepted Framework lifecycle and its historical disposition | [Install Interface](../../../crystallized/documents/cli/contracts/install/interface.md), [Install Behavior](../../../crystallized/documents/cli/contracts/install/behavior.md), [CLI-D016](../../../working/cli-release/decision-agenda.md#command-decisions), [CLI-D091](../../../working/cli-release/decision-agenda.md#contract-system-decisions), and [Packet 1](framework-lifecycle.md)                                                               | At preparation time, Queue 28 remained authoritative until this candidate was accepted and integrated. After the 2026-08-16 integration, the current Install/Update contracts and D091 govern. Packet 1 remains contextual history. |
| Shared CLI operation shape                                          | [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md) and [CLI-D086](../../../working/cli-release/decision-agenda.md#contract-system-decisions)                                                                                                                                                                                                                                                                | One operation per leaf, composable inputs, guided leaves, no prompts in JSON, conservative automatic mode, one typed result, and shared statuses and streams.                                                                       |
| Safety, planning, Git, recovery, and user-content boundaries        | [CLI safety decisions](../../../working/cli-release/decision-agenda.md), especially D041–D049                                                                                                                                                                                                                                                                                                                                                              | Current accepted safety direction. Queue 29's lifecycle refinements were later accepted and integrated; exact implementation mechanics remain deferred.                                                                             |
| Framework runtime and routed-file meaning                           | [Framework Architecture](../../../crystallized/documents/framework/architecture.md), [Routing Model](../../../crystallized/documents/framework/routing/model.md), [Routing Paths and Identity](../../../crystallized/documents/framework/routing/paths.md), [Overwrite Customization](../../../crystallized/documents/framework/routing/overwrites.md), and [Routed Markdown Representation](../../../crystallized/documents/framework/markdown/routes.md) | Defines runtime routes, ownership boundaries, overwrite layering, and generated navigation meaning. It does not accept new lifecycle commands.                                                                                      |
| Generated navigation                                                | [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md) and [Index Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md)                                                                                                                                                                                                                                                                                  | Generated `Entries` are derived navigation. Lifecycle operations consume the same projection and bounded-region rules without invoking a hidden public operation.                                                                   |
| Current Extension package and ownership model                       | [Extensions MVP Architecture](../../../crystallized/documents/extensions/architecture.md), [Extension package boundary](../../../crystallized/decisions/extensions/extension-package-boundary.md), and [Extension documentation](../../../../../docs/extensions.md)                                                                                                                                                                                        | Current package, route, dependency, receipt, manual-installation, and safety evidence. They do not accept this new taxonomy.                                                                                                        |
| Current first-party package                                         | [Catalogue README](../../../../../src/extensions/README.md) and [development-toolkit manifest](../../../../../src/extensions/development-toolkit/extension.json)                                                                                                                                                                                                                                                                                           | One current package is evidence, not a first-release slice or a promise that future catalogues have one package.                                                                                                                    |
| Current managed Extension evidence                                  | [`open-forge.extensions.json`](../../../../../open-forge.extensions.json)                                                                                                                                                                                                                                                                                                                                                                                  | Workspace evidence only. Its divergent bytes are untouched by this packet and are not a repair or test result.                                                                                                                      |
| Gate 1 divergence and provenance                                    | [Gate 1 findings](../gate-1-findings.md) and [Gate 1 audit](../gate-1-audit.md)                                                                                                                                                                                                                                                                                                                                                                            | Evidence to preserve in the candidate. It does not authorize repair, adoption, or an implementation claim.                                                                                                                          |
| Review state and sequence                                           | [Review Queue](queue.md) and [CLI Release Program](../_cli-release.md)                                                                                                                                                                                                                                                                                                                                                                                     | Queue 29 is settled contextual history. Queue 30 is rejected/superseded history; Queues 31–33 own the remaining command reviews.                                                                                                    |
| Writing and terminology                                             | [Writing Standard](../../../crystallized/documents/maintenance/writing.md), [Dictionary](../../../crystallized/documents/maintenance/helpers/dictionary.md), and the workspace Loader                                                                                                                                                                                                                                                                      | Governs this repository prose; it does not create product authority.                                                                                                                                                                |

The earlier council material remains useful but qualified. The cold-user lens
showed that availability, package detail, managed installation, reconciliation,
explicit replacement, and removal answer different questions. The ownership
lens established the need for exact IDs, one offline source universe,
dependency-first planning, transparent ownership, shared-owner protection, and
route safety. The adversarial simplifier identified a credible smaller family:
`list`, `inspect`, `create`, `install`, and `remove`, with repeated installation
used for reconciliation. The earlier reconciled seven-leaf candidate added a
separate replacement lane. This revision retains the evidence and dissent but
replaces that separate lane with the narrower composition of `update --force`
and `update --prune`.

The earlier council did not vote on these choices. Its agreement was evidence,
not maintainer acceptance. Before the 2026-08-16 rereview and integration, the
prior single root `install` operation remained current authority and the
strongest losing alternative to this deliberate split. The current
Install/Update contracts and D091 now govern.

## 4. Shared lifecycle mental model and exact Framework/Extension differences

### One lifecycle operation model

Both subjects use one stateless, deterministic lifecycle flow:

```text
validated command input
  -> exact workspace and source boundary
  -> current lifecycle facts and coverage/trust
  -> baseline, current, and intended comparison
  -> authoritative generated-navigation projection
  -> one complete ordered plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> verification or reverse guarded recovery
  -> one typed result
  -> human or JSON rendering
```

`install` and `update` are different product intents, not hidden modes of one
generic operation. A flag may widen one named authority boundary, but it never
changes the subject, creates another operation, bypasses safety, or turns a
managed update into an initial install.

The three lifecycle views are:

| View         | Meaning                                                                                                                                                                                                     |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Baseline** | The last verified expected semantic identity and managed ownership facts for the selected Framework targets or Extension package paths.                                                                     |
| **Current**  | The actual selected workspace, source facts, route topology, generated-region markers, ownership evidence, and lifecycle-document facts at operation time.                                                  |
| **Intended** | The source's current expected content plus only the changes permitted by the request, preserving user content and unresolved divergence unless `--force` or `--prune` explicitly widens its named boundary. |

The baseline does not describe runtime meaning, user intent, a session, an
operation history, a saved plan, or a transaction journal. The intended state is
not a blind source overlay. It is the result of applying the request to current
facts after all ownership, route, containment, and recovery boundaries are
known.

### Install, update, force, prune, and automation meanings

- **`install` establishes management.** It may create an absent managed
  Framework or Extension state. An existing exact managed installation is a
  verified no-op. Existing managed divergence is not ordinary install work and
  directs the user to `update`.
- **`install --force` is cautious initial authority.** It may replace an exact
  recognized current initial occupant so the operation can establish management,
  but it does not adopt the occupant's old bytes, override another owner, ignore
  a collision, or update an already managed lifecycle. The operation writes the
  current source bytes and records only the verified result.
- **`update` requires trusted existing lifecycle state.** It reconciles the same
  managed identity with the explicitly selected current source. Framework source
  identity is the embedded current Framework; Extension identity is the stable
  package ID. Semantic versions describe packages only. They do not select a
  source, negotiate compatibility, or create a generic package-manager update.
- **Normal update is respectful.** It may apply baseline-unchanged managed
  content and genuinely new safe files. It keeps changed current expected paths,
  missing current expected paths, and retired managed paths visible and
  preserved unless the user supplies the authority that applies to that class.
- **`--force` widens only current expected-footprint replacement.** It may
  overwrite changed current expected managed paths and restore missing current
  expected managed paths. It never implies prune, Git bypass, adoption,
  ownership, collision or marker bypass, containment bypass, or weaker recovery.
- **`--prune` widens only retired managed-content deletion.** It may delete
  eligible retired managed content after ownership, route, Git, backup, and
  verification checks. It never overwrites or restores current expected content,
  bypasses Git, performs arbitrary cleanup, or deletes unknown, unowned, shared,
  or unsafe content. `--force --prune` composes the two exact boundaries and can
  converge to current source only where every safety and ownership fact permits.
- Changed retired content in an update is prune-eligible only when the trusted
  baseline identifies the exact previously managed path, the current source
  proves that it is retired, current physical identity is safe, the current
  semantic state is known, no other owner, manager, or route dependency blocks,
  and complete Git and recovery facts pass. Prune supplies deletion authority,
  not ownership inference. A changed final-owner path in `remove --prune` uses
  the same identity, semantic, ownership, Git, and recovery gates; the explicit
  selected removal does not adopt an unowned path.
- **`--automatic` is interaction policy, not authority.** Globally, the command
  path plus explicit IDs or other subjects select the `install`, `create`, or
  `remove` operation and supply its ordinary operation authority. Named
  authority flags still widen only their stated boundaries. Automatic mode only
  suppresses interaction and chooses documented conservative defaults. It never
  adds package selection, initial force, divergent replacement or restoration,
  prune, adoption, or a safety bypass. It may execute ordinary safe effects
  already authorized by the explicit operation and subjects. Repetition is
  idempotent.
- **`--dry-run` previews the same request.** It resolves the same current facts,
  baseline, intended state, complete plan, and preflight as apply, then reports
  the same pre-effect planning status and diagnostics without writing. It cannot
  prove application, verification, lifecycle publication, or recovery success,
  and it writes nothing, including lifecycle metadata, backups, or temporary
  artifacts.
- **`--skip-git-check` is one narrow exception.** It bypasses only affected-path
  cleanliness and activates the accepted adjacent-backup recovery where needed.
  It does not supply any replacement, deletion, ownership, collision,
  containment, marker, verification, or recovery authority.

For this candidate, an initial occupant is an exact current source destination
with no trusted lifecycle owner or competing manager. An untracked or manually
authored occupant at that exact destination may therefore be eligible for
explicit initial force authority. A known user-owned or Extension-owned path,
route collision, ambiguous managed block, or unsafe boundary is not an initial
occupant that force may override.

### Exact differences

| Dimension            | Framework                                                                                                         | Extensions                                                                                                                           |
| -------------------- | ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| Source               | One fixed current Framework payload embedded in the running CLI.                                                  | The embedded catalogue or one exact external package/catalogue read location.                                                        |
| Selection            | The exact selected workspace and the complete recognized Framework footprint.                                     | Stable-ID operands or explicit `--all`, one source universe, and the complete exact dependency closure.                              |
| Managed identity     | Framework source identity, target/region identity, and verified baseline facts.                                   | Stable package ID, package source identity, dependency facts, owned target-relative paths, and shared-owner sets.                    |
| Install intent       | Establish the Framework lifecycle in a safely absent or eligible initial state, or verify an exact managed no-op. | Add selected absent managed package roots and dependencies, or verify an exact managed no-op.                                        |
| Update intent        | Reconcile trusted Framework state with the current embedded source.                                               | Reconcile trusted managed package IDs with the explicitly selected current source.                                                   |
| Replacement boundary | Exact current Framework files and valid supported root/provider blocks.                                           | Exact current source footprint for the same managed package identity and safe owned routes.                                          |
| Deletion boundary    | Retired Framework managed content through `update --prune`; there is no core remove leaf.                         | Eligible retired or final-owner content through `update --prune` or `remove --prune`, subject to package ownership and route safety. |
| Source removal       | Not applicable. The embedded source is never a workspace lifecycle target.                                        | `remove` never deletes the package source, even when it releases every workspace owner.                                              |
| Runtime boundary     | Framework routes, Core, Memory, and supported provider blocks retain their Framework meaning.                     | Installed files retain route meaning. Extension metadata and ownership remain lifecycle evidence only.                               |

Physical co-location of lifecycle facts does not erase these differences. A
Framework baseline is not an Extension receipt, and an Extension package owner
is not Framework authority.

## 5. Candidate taxonomy and exact product-level syntax

The following is the exact candidate command surface for maintainer rereview. It
is **not** an accepted Interface Contract. Existing current contracts continue
to govern until the candidate is accepted and integrated.

### Framework

```text
open-forge install [--force] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge update [--force] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]
```

`install` and `update` are direct root operations because each is a real stable
operation over the embedded Framework. There is no `framework` group, root
`init`, replacement leaf, reinstall leaf, Framework uninstall/remove leaf,
generic `apply`, saved plan, `--yes`, or generic package update-by-semver
operation.

### Extensions

```text
open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
open-forge extension create [<stable-id>] [--path <catalogue-path>] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]
open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]
```

The `extension` group has several actual operations, so its bare form shows
help and performs no wizard or lifecycle work. The candidate does not add a
generic package operation, a replacement leaf, a reinstall alias, or a command
that turns flags into a hidden operation discriminator.

### Source and selection rules

`--source` is one exact external read location. Structural manifest and package
facts distinguish one package directory from one catalogue directory. The CLI
does not use resemblance, ambient search, a network, a registry, a cache, a
glob, fuzzy matching, or a fallback source to decide which one the user meant.

When `--source` is omitted, available-package operations use the embedded
catalogue. Installed facts still come from the selected workspace's lifecycle
state. An external source is read-only input; lifecycle operations never remove
or rewrite it.

The external package or catalogue source and the target workspace must be both
lexically and physically disjoint. Reject a source inside the target, a target
inside the source, lexical or physical aliases, and any other overlap. The CLI
never mutates the external source.

IDs are operands. For `install` and `update`:

- A selected source may provide an ID by deterministic inference only when it
  contains exactly one completely validated package and that package's manifest
  declares one valid stable ID. The manifest ID, not folder or path spelling or
  resemblance, is the only inferred identity. A missing, duplicate, malformed,
  or conflicting manifest ID makes the request `invalid` or `blocked` as
  applicable; it is never inferred.
- A selected source containing several packages requires explicit IDs or the
  explicit `--all` selection. A human wizard may select from the finite package
  list.
- `--automatic` never means `--all`. It does not choose one package from several
  candidates or broaden an omitted selection.
- `--all` is an explicit request for all applicable packages in the selected
  source universe. For install, that means all available package roots. For
  update, it means all currently managed IDs represented by the selected source
  and closure. Missing source coverage is not silently skipped. It is not a
  hidden default. Combining it with explicit IDs is an invalid conflicting
  selection rather than a last-wins rule.
- Dependencies are resolved only within the one selected source universe. An
  exact package source uses its containing package directory as the source
  universe for its complete dependency closure only when that directory is a
  structurally valid catalogue; otherwise the request is invalid or blocked.

`list` and `inspect` accept either one exact package or one exact catalogue as
their source. `create --path` has a different meaning: it names the destination
catalogue parent and is never a package source. The parent is recognized from
its structural catalogue shape; no persistent catalogue marker is required.

`create` writes:

```text
<catalogue>/<id>/extension.json
<catalogue>/<id>/payload/.agents/
```

It does not install the scaffold into a workspace and does not write Framework
or Extension lifecycle state. The package source remains a separate authored
location.

## 6. Human-first wizard and direct automation model

The general rule already belongs to the [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
and CLI-D086. The candidate applies it as follows:

- A bare group shows help. `open-forge extension` never opens a wizard.
- A wizard-capable leaf uses its simplest useful human invocation, normally the
  argumentless form. Wizard answers, operands, and flags populate one typed
  request. They do not create a hidden mode or a second operation.
- Explicit IDs, source paths, catalogue paths, and flags incrementally answer,
  narrow, or broaden those same questions. Conflicting explicit inputs are
  invalid.
- JSON and every other noninteractive mode never prompt. Missing semantic input
  is `invalid`. Missing authority or an unresolved safe choice is `blocked`.
- For a direct request, the command path plus explicit IDs or other subjects
  provide the `install`, `create`, or `remove` operation selection and ordinary
  authority. An explicit authority flag such as `--force` or `--prune` widens
  only its named boundary.
- `--automatic` suppresses the wizard and selects only documented deterministic
  conservative defaults. It is never a consent flag or an authority source. It
  never adds package selection, initial force, divergent replacement or
  restoration, prune, adoption, or a safety bypass. It may execute ordinary safe
  effects already selected by the explicit operation and subjects.

Framework `install` has a compact human inspection and confirmation flow because
its source is fixed and embedded. It need not ask the user to choose a package,
source, or target. `--automatic`, explicit authority flags, JSON, and other
noninteractive input run the direct form. `--dry-run` only previews and never
turns a preview into consent.

Framework `update` without operation-specific authority may show a finite human
decision summary for changed, missing, and retired content. The conservative
choice is preservation. `--force` and `--prune` are explicit authority inputs,
not answers inferred from a recommendation. A JSON request with no such
authority can safely preserve and report the facts; if an intended state cannot
be resolved without a destructive or ownership decision, it is blocked.

Argumentless human `create`, `install`, `update`, and `remove` leaves may open
finite wizards. Their questions are different:

| Leaf      | Finite human questions                                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------------------------------------- |
| `create`  | Stable ID and destination catalogue parent.                                                                                     |
| `install` | Package IDs or all, source when needed, and any initial collision or authority choice.                                          |
| `update`  | Existing managed IDs or all, source when needed, and whether the user supplies force or prune authority for visible divergence. |
| `remove`  | Managed IDs and whether eligible changed final-owner content may be deleted with `--prune`.                                     |

An explicit operand or flag answers the same question the wizard would ask. A
recommendation is only a displayed fact until the user supplies the relevant
authority. Automatic mode may preserve and report, but it never selects several
package IDs, overwrites or restores divergence, prunes, deletes changed content,
adopts, or bypasses a safety boundary. It may carry out ordinary safe effects
already selected by the explicit operation and subjects.

## 7. Framework install journey and state/result matrix

This section describes the candidate supersession. It is not a rewrite of the
current Install Interface or Behavior Contract.

### Candidate install journey

1. Resolve the exact CWD or exact global `--workspace` value. Do not discover a
   parent, Git root, nested `.agents` root, or nearby Framework.
2. Read only the embedded current Framework source and the exact supported
   Framework footprint: current payload destinations, supported root/provider
   managed regions, affected generated regions, and Framework lifecycle facts.
3. Classify the selected workspace as safely absent, exactly managed, an eligible
   initial occupant, managed but divergent, unavailable, or unsafe/ambiguous.
4. Form one intended installation state. An initial `--force` request may
   replace only an exact recognized current initial occupant after all boundaries
   pass. It never treats the occupant's previous bytes as lifecycle history.
5. Project affected `Entries` from the intended authored topology and metadata,
   then plan payload, managed-region, generated-region, and lifecycle-document
   effects together.
6. Preflight every effect, apply or preview it, verify the complete applied
   result only after application, and publish Framework lifecycle facts only
   after complete verification. A dry-run verifies the plan and diff, not
   application effects.

An exact managed installation means the managed identity and current semantic
fingerprints already equal the current embedded intended state. Exact-byte
formatting differences with equal fingerprints are informational and do not
make that state divergent.

### Framework install state/result matrix

| Current state                                                                                                                                                                                  | Normal candidate `install`                                                                               | `install --force`                                                                                                                         | Product result                                                                                                                                       |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| No trusted Framework lifecycle state, no occupied exact current targets, no managed root/provider blocks, and no recovery residuals                                                            | Create the absent recognized footprint, project navigation, and establish management after verification. | Same safe plan. Force adds no needed initial authority.                                                                                   | `complete` after verified apply or complete pre-effect dry-run.                                                                                      |
| Trusted current Framework lifecycle state and current source is semantically exact                                                                                                             | Do not write. Verify the managed no-op.                                                                  | Do not write. Force does not invent an effect.                                                                                            | `complete`; format-only observations are informational.                                                                                              |
| Trusted managed state has changed, missing, or retired content, or the managed source identity is not the current embedded source                                                              | Do not reconcile it under install. Show the facts and direct the user to `update`.                       | Do not turn force into managed update authority.                                                                                          | `blocked` with one useful `Next:` action and no writes.                                                                                              |
| An exact current Framework destination or supported managed block is occupied before management is established, with no competing owner, route collision, marker ambiguity, or unsafe boundary | Do not overwrite or adopt the occupant.                                                                  | May replace the exact recognized initial occupant, write current source bytes, and establish management only after complete verification. | Normal `blocked`; eligible force `complete`; force remains `blocked` for any unresolved ownership, collision, marker, containment, or recovery fact. |
| An Extension-owned path, user-owned managed block, unknown path, route collision, or another lifecycle claim overlaps the footprint                                                            | Preserve the other content and stop the complete plan.                                                   | Force does not bypass the overlap or adopt the content.                                                                                   | `blocked`, no partial installation.                                                                                                                  |
| Existing user routes, Memory, overwrite companions, or content outside the recognized footprint                                                                                                | Preserve it. It is not an install target.                                                                | Preserve it.                                                                                                                              | No status effect by itself; a complete plan may remain `complete`.                                                                                   |
| Baseline/lifecycle facts or marker topology are safely unavailable                                                                                                                             | Do not guess or establish ownership.                                                                     | Do not broaden the footprint to compensate.                                                                                               | `incomplete`, no writes.                                                                                                                             |
| Baseline/lifecycle facts, markers, source identity, physical identity, or containment are malformed or ambiguous                                                                               | Do not write.                                                                                            | Do not repair or bypass the boundary.                                                                                                     | `blocked`, no writes.                                                                                                                                |
| A planned existing affected path is dirty in Git                                                                                                                                               | Block the mutation.                                                                                      | Force remains separate from Git policy.                                                                                                   | `blocked` unless `--skip-git-check` applies only to that path; accepted backup recovery is then required where needed.                               |
| Affected authored topology changes generated `Entries`                                                                                                                                         | Include the current Index projection in the same plan.                                                   | Use the same bounded projection.                                                                                                          | The generated effect is verified as part of the one result.                                                                                          |
| `--dry-run` is present                                                                                                                                                                         | Resolve and preflight exactly as apply, then write nothing.                                              | Preview the exact force-authorized initial plan only.                                                                                     | Same pre-effect planning status as the corresponding apply request; no lifecycle change and no claim about apply-time events.                        |

`install` has no retired-content deletion authority. It therefore does not use
`--prune`, and it never deletes retired Framework content.

## 8. Framework update journey and state/result matrix

### Candidate update journey

`update` starts only after a trusted Framework lifecycle state is established
for the exact selected workspace. The embedded current Framework source defines
the intended state. There is no version-range solver and no source operand.

The planner compares every recognized target and bounded region by baseline
semantic fingerprint, current semantic fingerprint, current exact bytes, and
intended current source. It then forms one complete plan:

- normal mode may change baseline-unchanged current expected content and add
  genuinely new safe content;
- `--force` may overwrite changed current expected content and restore missing
  current expected content;
- `--prune` may delete eligible retired managed content;
- `--force --prune` may do both, but only where all safety and ownership facts
  permit; and
- all other divergence is preserved and reported.

### Framework update state/result matrix

| Baseline/current/source state                                                                                                             | Normal `update`                                                                                                  | `--force`                                                                                          | `--prune` and composition                                                                             | Result                                                                                                                                     |
| ----------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Trusted state, current equals baseline, source has no semantic change                                                                     | Verified no-op.                                                                                                  | Same.                                                                                              | Same.                                                                                                 | `complete`.                                                                                                                                |
| Trusted state, current baseline is unchanged, source has a genuinely new safe file or region                                              | Add the new expected content and refresh verified facts.                                                         | Same.                                                                                              | Same.                                                                                                 | `complete` if no other finite attention remains.                                                                                           |
| Baseline, current, and intended semantic fingerprints are equal, while current exact bytes differ only by parser-proven formatting trivia | Preserve the current formatting and report the observation. Do not run a formatter or treat it as divergence.    | Force does not needlessly replace an equal semantic identity.                                      | Prune is unrelated.                                                                                   | `complete` unless another condition applies.                                                                                               |
| Current expected file or managed region is semantically changed from baseline                                                             | Preserve it and plan other safe effects.                                                                         | Overwrite only that exact current expected footprint after revalidation, backup, and verification. | Prune does not widen this boundary.                                                                   | Normal `attention`; force `complete` if no other attention remains.                                                                        |
| Current expected file or managed region is missing                                                                                        | Preserve the absence. Do not restore a user removal implicitly.                                                  | Restore only the exact current expected footprint after all checks.                                | Prune does not restore it.                                                                            | Normal `attention`; force `complete` if restoration verifies.                                                                              |
| A baseline-managed path is retired from the current embedded source and is present                                                        | Preserve it.                                                                                                     | Preserve it; force is not retired deletion authority.                                              | Delete only if it is eligible retired managed content with no competing owner or unsafe route effect. | Normal/force `attention`; prune `complete` if deletion verifies.                                                                           |
| A retired path is absent                                                                                                                  | Retain the retired lifecycle observation without recreating the path.                                            | Same.                                                                                              | No deletion effect.                                                                                   | `complete` unless coverage or another finite condition remains.                                                                            |
| User-owned, Extension-owned, unknown, shared, or colliding content intersects an intended effect                                          | Do not claim or mutate it.                                                                                       | Force cannot bypass ownership, collision, marker, or containment.                                  | Prune cannot delete it.                                                                               | `blocked`, no unsafe partial plan.                                                                                                         |
| Trusted lifecycle state is missing, malformed, incompatible, or not verifiable                                                            | Do not infer managed identity from path or bytes.                                                                | Force does not make untrusted facts trusted.                                                       | Prune does not make retired identity appear.                                                          | `incomplete` for safe unavailable coverage; `blocked` for unsafe or ambiguous facts.                                                       |
| Required current source bytes or an embedded Framework payload fact are unavailable                                                       | No update plan can be formed.                                                                                    | No authority can replace missing source bytes.                                                     | No deletion plan can guess intended state.                                                            | `incomplete`, no writes. Framework's embedded source is expected to be available; an unavailable Extension package source remains visible. |
| Affected current path is dirty in Git                                                                                                     | Block the plan.                                                                                                  | Same.                                                                                              | Same.                                                                                                 | `blocked` unless `--skip-git-check` applies only to the affected path and required backup recovery is ready.                               |
| `--automatic` is present                                                                                                                  | Apply only safe baseline-unchanged/new effects. Preserve visible divergence and report it; never force or prune. | Automatic does not silently activate force.                                                        | Automatic does not silently activate prune.                                                           | `complete` or `attention` from the same facts, or `blocked` when a safe choice cannot be resolved.                                         |
| `--dry-run` is present                                                                                                                    | Show the complete intended effects, preserved facts, and pre-effect planning status without writing.             | Preview force effects without writing.                                                             | Preview prune effects without writing.                                                                | Same pre-effect planning status as apply; no lifecycle change and no claim about application, verification, or recovery.                   |

When current and baseline semantic fingerprints are equal but the intended
source fingerprint differs, normal update may apply the intended source because
the current exact formatting is not managed divergence. The result may replace
that formatting with current source bytes and report the applicable formatter
advice; the CLI does not run a formatter to recreate it.

Normal update may safely apply a subset of non-divergent effects while preserving
changed, missing, or retired content. It must not claim exact source convergence
when it did not achieve it. Planned changes alone do not create `attention`.
Preserved finite divergence does.

An illustrative human result is:

```text
Open Forge update
Source: embedded Framework; workspace D:/work/example
Mode: normal; apply
Baseline/current/intended: 10 / 10 / 10 managed targets
Applied: 2 safe updates; 1 new target
Preserved: 1 changed target; 1 missing target; 1 retired target
Generated: 2 Entries regions projected and verified
Lifecycle: safe facts refreshed; divergent facts retained
Status: requires attention
Next: review preserved targets, then rerun with --force or --prune only for intended boundaries.
```

## 9. Extension operations

Extension `install`, `update`, and `remove` use the exact workspace selected by
the shared global flags. Install and update use one source universe, one
dependency closure, one route projection, and one payload/lifecycle plan. Remove
uses trusted workspace ownership and route facts without package source bytes.
Create instead uses the exact catalogue parent selected by `--path`; the shared
`--workspace` flag is accepted as a no-op for that operation. It forms one
scaffold plan and never writes workspace lifecycle state. Every mutating leaf
still uses one complete plan, the shared safety stages, and one typed result. The
following outputs are illustrative shapes, not implementation evidence or
claims about current inventory.

### Framework anchor and route-host prerequisites

Managed Extension `install` and `update` require a trustworthy installed
Framework anchor and complete route-host facts before they can form a mutation
plan. Route-host facts include the affected authored hosts, generated-navigation
boundaries, ownership relationships, and cross-section preservation facts.

`list` and `create` do not require a healthy installed Framework. `list` and
`inspect` may report independently readable installed or legacy facts without a
healthy current Framework. `remove` may proceed without a healthy current
Framework only when complete trusted Extension ownership, route,
generated-navigation, and cross-section preservation facts can still be
established. Otherwise the result is `incomplete` or `blocked`, as applicable,
and no managed mutation occurs.

### 9.1 `extension list`

**Why it exists.** `list` answers which Extension packages are installed and
which packages are available. It does not equate availability with installation
and does not replace `status` or `doctor`.

**Inputs.** It accepts the optional exact `--source`, `--installed`, and
`--available` filters. With no filter it renders both sections. `--installed`
renders only Installed; `--available` renders only Available; both flags compose
back to both sections. Installed facts come from trusted, legacy, or partially
readable lifecycle state even when the package source is unavailable. The result
keeps the explicit lifecycle state rather than presenting legacy facts as current
trust. Available facts come only from the embedded catalogue or the one selected
source.

**Human flow.** Resolve the exact workspace, read installed lifecycle facts,
classify one optional source as a package or catalogue, enumerate available
facts, apply the section filters, and render the result. There is no prompt and
no write.

**Illustrative output.**

```text
Open Forge extension list
Installed
- development-toolkit 0.1.0; 21 managed paths; prior recorded source unavailable
Available
- development-toolkit 0.1.0; optional workflows, Skill, and Templates
Status: complete
```

**Internal mechanism peek.** The operation reads the Extension lifecycle
section for installed rows and the selected source for available rows. A shared
stable ID is shown as one relationship with separate installed and available
facts. It never invents a receipt from an available package, deletes a source,
or adopts an untracked workspace file.

### 9.2 `extension inspect`

**Why it exists.** `inspect` provides package-specific detail before a lifecycle
decision. `status` remains quick workspace orientation and `doctor` remains
complete read-only diagnosis.

**Inputs.** It requires one stable-ID operand and accepts an optional exact
`--source`. Without a selected source, the operation can inspect installed facts
and the embedded available package. With a selected source, it reads that exact
package or catalogue. The selected ID may be installed-only, available-only, or
both. Installed and legacy facts remain reportable without a healthy current
Framework.

**Human flow.** Resolve the ID, read the installed receipt/baseline facts if
present, read the selected package facts if available, resolve its exact
dependency closure, and render the strongest available comparison:

- installed-only when package source bytes are unavailable;
- available-only when no managed installation exists; or
- baseline/current/intended three-way comparison when trusted installed facts
  and current source bytes are both available.

Unavailable package source does not erase a receipt, current ownership, or
baseline fact. It prevents an actual update or restore plan from being formed.

**Illustrative output.**

```text
Open Forge extension inspect development-toolkit
Source: embedded catalogue; available
Installed: yes; managed identity trusted
Packages: 1 root; 0 dependencies
Footprint: 21 declared paths; 9 current matches; 11 recorded Template mismatches; 1 missing
Comparison: baseline / current / intended available
Generated: affected Entries are derived, not package-owned authored bytes
Status: requires attention
Next: use open-forge extension update development-toolkit to review reconciliation.
```

**Internal mechanism peek.** Inspection parses source and lifecycle facts without
writing. It computes current and intended fingerprints only when the required
source bytes are available, reports unavailable coverage otherwise, and never
turns a matching path or fingerprint into ownership.

### 9.3 `extension create`

**Why it exists.** `create` gives an author a stable local package boundary
without conflating package authoring with workspace installation.

**Inputs.** The human wizard may obtain a stable ID and destination catalogue
parent. Direct and noninteractive use must provide enough semantic input,
including the ID and destination path. `--path` is the catalogue parent, not a
source and not the target workspace. `--automatic` can suppress interaction only
after those inputs are explicit. `--dry-run` previews the scaffold, and
`--skip-git-check` affects only the destination paths that would be changed.

**Human flow.** Validate the stable ID and destination, confirm that the parent
has a safe catalogue shape and that the package destination is unused, show the
scaffold, and create it only after the direct request is confirmed. The parent
needs no marker file.

**Illustrative output.**

```text
Open Forge extension create development-toolkit
Catalogue: D:/packages/open-forge
Created: D:/packages/open-forge/development-toolkit/extension.json
Created: D:/packages/open-forge/development-toolkit/payload/.agents/
Workspace lifecycle: unchanged
Status: complete
```

**Internal mechanism peek.** The operation validates the ID, exact parent
containment, package-destination absence, and source-side Git/recovery facts.
It writes only `extension.json` and the `payload/.agents/` scaffold. It does not
resolve or install dependencies, copy payload files into a workspace, create a
receipt, or change generated navigation in a workspace.

### 9.4 `extension install`

**Why it exists.** `install` establishes managed ownership for selected absent
Extension IDs and verifies an exact managed installation. It is not ordinary
managed update.

**Inputs.** It accepts explicit stable-ID operands or explicit `--all`, one
optional source, and `--force`, `--automatic`, `--dry-run`, and
`--skip-git-check`. The selected package or catalogue supplies one exact source
universe. A trustworthy installed Framework anchor and complete route-host facts
are required before a managed installation plan can be formed. A complete
offline dependency closure is required before any write.

**Human flow.** Show the finite available candidates, selected roots, dependency
closure, target-relative footprint, route effects, ownership conflicts, and
initial collision facts. A human may select IDs and supply force authority for
eligible initial occupants. A direct or JSON request carries the same choices
explicitly. The operation then plans payload, generated navigation, and lifecycle
state together.

`install` has these important states:

- An absent package ID with safe absent targets is installed dependency-first and
  becomes managed after verification.
- An exact managed installation is a verified no-op.
- An existing managed ID with changed, missing, retired, or source-divergent
  state is not updated by install. The operation returns the facts and directs
  the user to `update`.
- If an ID exists only in installed state and no embedded or selected package
  source is available, `install <id>` returns `incomplete`, writes nothing, and
  directs the user to `inspect` or to supply `--source`. It cannot claim an exact
  no-op without the intended source bytes.
- `install --force` may replace an exact recognized initial occupant in the
  selected current package footprint only when no other ownership, route,
  collision, marker, containment, or recovery boundary is being bypassed. It
  records the newly written verified state; it does not adopt the old occupant.

**Illustrative output.**

```text
Open Forge extension install development-toolkit
Source: embedded catalogue
Packages: 1 root; 0 dependencies
Footprint: 21 payload files; 3 generated regions
Applied: 21 payload files; 3 generated regions; lifecycle ownership recorded
Preserved: none
Recovery: none
Status: complete
```

**Internal mechanism peek.** Validate the installed Framework anchor, complete
route-host facts, manifest, stable ID, exact dependency closure, duplicate IDs,
cycles, package paths, target containment, ownership, current expected state,
Git, and backup readiness. Order dependencies before dependents. Build one parent
plan for payload files, generated regions, shared owners, and the Extension
lifecycle section. Publish ownership only after per-effect and whole-operation
verification.

### 9.5 `extension update`

**Why it exists.** `update` reconciles a trusted existing managed Extension
identity with the current bytes from the explicitly selected source. It makes
source and intent visible instead of treating a package version as an implicit
operation.

**Inputs.** It accepts managed stable-ID operands or explicit `--all`, one exact
source, and `--force`, `--prune`, `--automatic`, `--dry-run`, and
`--skip-git-check`. It requires trusted existing lifecycle state and readable
current source bytes for every selected managed identity and dependency fact it
must affect. It also requires a trustworthy installed Framework anchor and
complete route-host facts. A descriptive semantic version never selects a
different source or grants compatibility authority.

**Human flow.** Show the selected source, roots, dependency closure, baseline /
current / intended comparison, safe additions, changed and missing expected
paths, retired paths, shared owners, route effects, and recovery requirements.
Normal update preserves unresolved divergence. A human may explicitly widen the
current expected replacement boundary with `--force`, the retired deletion
boundary with `--prune`, or both. The wizard never infers either choice from a
recommendation.

**Illustrative output.**

```text
Open Forge extension update development-toolkit
Source: D:/packages/catalogue; package development-toolkit
Mode: normal; apply
Baseline/current/intended: 21 / 20 / 23 managed paths
Applied: 3 safe updates; 2 new files
Preserved: 11 changed current files; 1 missing current file; 1 retired path
Generated: 2 Entries regions projected and verified
Lifecycle: safe facts refreshed; divergent facts retained
Status: requires attention
Next: review preserved paths, then rerun with --force or --prune only for intended boundaries.
```

**Internal mechanism peek.** Require a trustworthy installed Framework anchor,
complete route-host facts, and a trusted Extension section. Resolve the same
stable IDs in one source universe, validate dependency order and shared owners,
compute semantic fingerprints, and build one complete plan. Normal mode changes
only safe baseline-unchanged content and new safe files. Force changes only
changed or missing current expected paths. Prune changes only eligible retired
managed content. Neither flag transfers ownership or repairs a route or marker
boundary.

### 9.6 `extension remove`

**Why it exists.** `remove` releases explicit managed package ownership and
performs bounded cleanup that can be proven safe from the receipt. It remains a
separate Extension operation because package ownership makes the deletion scope
reviewable. Framework has no equivalent current leaf.

**Inputs.** It accepts explicit managed stable-ID operands, or a human wizard may
select from the finite managed IDs. It does not need package source bytes. A
missing source therefore does not erase the receipt or prevent read-only
ownership facts. `--prune` is explicit deletion authority for eligible changed
final-owner content, and it must be selected in the same request as the remove
operation. In a human wizard, the user chooses `Keep-as-unmanaged` or `Delete`
before planning. Noninteractive normal mode and automatic mode choose
`Keep-as-unmanaged` unless the same request includes explicit `--prune`, which
selects `Delete`. Automatic mode adds no deletion or ownership-release
authority. It may execute ordinary safe effects already authorized by the
explicit remove operation and IDs, but it never selects `Delete` for changed
content on its own.

**Human flow.** Show selected IDs, retained dependents, dependency edges, shared
owners, final-owner files, route-host effects, changed content, and the exact
ownership release. Before planning, the human wizard shows `Keep-as-unmanaged`
and `Delete` for changed final-owner files. `Delete` is the explicit `--prune`
choice. Noninteractive requests without `--prune`, including automatic mode,
use `Keep-as-unmanaged`. After that choice and complete preflight, removal:

1. blocks if a retained dependent would be stranded or a route host cannot be
   removed safely;
2. may release the selected ownership for every eligible package path;
3. deletes an unchanged eligible final-owner file;
4. retains shared files while another owner remains; and
5. preserves a changed final-owner file as visible unmanaged content with
   `Keep-as-unmanaged`, or deletes it only when `--prune` selected `Delete` in
   this same request.

Unknown, unowned, shared-unsafe, Extension-external, or route-unsafe content is
never deleted. Ownership release is explicit in the plan and result. The package
source is never removed. Once ownership is released, a later prune cannot act on
the resulting unmanaged file.

**Illustrative output.**

```text
Open Forge extension remove development-toolkit
Packages: 1 selected; 0 retained dependents
Ownership: released for 21 recorded paths
Deleted: 8 unchanged final-owner files
Retained: 3 shared files
Preserved unmanaged paths:
- .agents/templates/documents/architecture.md
- .agents/templates/documents/maintenance-contract.md
- .agents/templates/documents/principles.md
- .agents/templates/documents/vision.md
- .agents/templates/memory/_memory.md
- .agents/templates/memory/analysis.md
- .agents/templates/memory/decision.md
- .agents/templates/memory/handoff.md
- .agents/templates/memory/idea.md
- .agents/templates/memory/observation.md
Source: unchanged
Status: requires attention
```

**Internal mechanism peek.** Read only the trusted Extension lifecycle facts and
fresh current workspace state, validate dependency and route reachability, and
form one removal/index/lifecycle plan. For each path, freshly capture the
current semantic fingerprint and exact bytes. Compare the current semantic
fingerprint with the persisted baseline semantic fingerprint for removal and
prune classification. Keep exact current bytes in the operation plan for diff,
expected-state revalidation, backup, deletion or write verification, and
recovery. Account for the complete shared-owner set and remove ownership only
after the complete selected plan is safe. Verify the resulting routes and
lifecycle section together.

A repeated `remove` is a verified no-op only when a valid trusted current
lifecycle document proves the requested ID and all selected ownership are
already absent. Missing or untrusted lifecycle state is `incomplete` when safe
coverage is unavailable or `blocked` when its ambiguity is unsafe. It is never
presumed to prove a prior release or a successful no-op.

## 10. Shared transparent lifecycle document and safety boundaries

### One physical document with isolated logical sections

The candidate proposes one transparent physical lifecycle document with a common
schema envelope and separate logical sections:

```text
common envelope
  framework section
  extensions section
```

The exact future filename, path, schema version, migration, serialization, and
atomic write mechanics are deliberately not selected here. The current
[`open-forge.extensions.json`](../../../../../open-forge.extensions.json) remains
repository evidence and is untouched. No migration is implied by this packet.

Co-location is not authority fusion:

- Framework source identity, target/region facts, and Framework baseline remain
  separate from Extension package receipts and owner sets.
- An Extension cannot become Framework-owned because both facts occupy one
  document.
- A Framework baseline cannot claim an Extension path because it is nearby.
- A malformed unrelated section should not automatically erase trustworthy facts
  in the other section.
- An operation must completely validate every section, path, and owner
  relationship it can affect. Shared envelope facts and cross-section collisions
  are not ignored merely because the operation has a narrow subject.

### Finite lifecycle-document trust states

The document and each logical section have one finite trust state. A path,
matching fingerprint, current source, or force flag cannot promote untrusted
state to trusted state.

| State                                                                    | Read-only behavior                                                                                                                                                                                                                                                                                                                                      | Mutation behavior                                                                                                                                                                                                                                             |
| ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Safely absent document or section                                        | Report no managed lifecycle claim when complete inspection proves that no expected managed state, managed boundary, or recovery residual is present.                                                                                                                                                                                                    | A new lifecycle state may be established only by a complete operation plan that passes all other boundaries.                                                                                                                                                  |
| Valid trusted current section                                            | Report current managed facts as trusted only when the supported envelope and section versions and fingerprint policy are valid, workspace and managed identities are exact, internal consistency is intact, package/path/owner/dependency facts are reciprocal, IDs and paths have no duplicates or conflicts, and coverage is complete and verifiable. | Managed mutation may use those facts after complete preflight and whole-operation verification.                                                                                                                                                               |
| Recognized legacy receipt                                                | Report read-only list/inspect facts with an explicit `legacy` state. This includes evidence from the current `open-forge.extensions.json` schema, including its `schema: 2` package, dependency, path, owner, and `sha256` facts.                                                                                                                       | No mutation occurs unless an explicit verified migration establishes every required trusted fact atomically. Exact migration mechanics remain Gate 3. Divergent or insufficient legacy data does not auto-migrate or grant force, prune, or remove authority. |
| Malformed, unsupported, unverifiable, or internally inconsistent section | Retain safe partial facts when they are independently trustworthy and report `incomplete`; report unsafe ambiguity as `blocked`.                                                                                                                                                                                                                        | No managed mutation.                                                                                                                                                                                                                                          |
| Missing expected section                                                 | Report `incomplete` or `blocked`, as applicable, when the document or another trusted fact shows that the section is expected. Never treat it as empty and never reconstruct it from paths, bytes, or source.                                                                                                                                           | No managed mutation.                                                                                                                                                                                                                                          |

The candidate does not add persistent exact-byte baseline digests. For supported
parseable kinds, the legacy `sha256` fields are only legacy byte evidence until
an explicit migration establishes the candidate's semantic baseline. No path,
matching fingerprint, current source, or force flag makes untrusted lifecycle
state trusted.

### Shared physical document write rule

Read-only operations may retain and report independently trustworthy facts from
one section when another section is malformed. A mutation is stricter: it must
preserve the exact bytes and meaning of every unrelated section and every common-
envelope fact, and publish the whole physical document atomically. If an
unrelated section or common-envelope fact cannot be safely parsed, preserved,
round-tripped, or verified, the mutation is `incomplete` or `blocked`, as
applicable, and writes nothing.

The mutation never drops, normalizes, repairs, or rewrites opaque malformed
state as a side effect. Cross-section owner and path collisions are part of
complete preflight and block the mutation. Exact serialization, round-trip, and
atomic-publication mechanics remain Gate 3 decisions.

### Common lifecycle facts

The common envelope may carry only document-level facts needed to identify and
validate the physical lifecycle document, such as its supported envelope
version, fingerprint-policy identity, exact workspace binding, and section
presence. Exact fields remain a Gate 3 decision. The envelope must not duplicate
Framework source or target facts, Extension package or ownership facts,
per-managed-source baseline fingerprints, current or intended comparisons, or
operation recovery evidence. Those facts remain section-local or are collected
fresh for the active operation.

The document must not become an operation history, session store, saved plan,
journal, runtime interpretation layer, or hidden command state. Current and
intended facts are recomputed from fresh workspace/source evidence for every
operation. A complete verified transition may publish new section-local baseline
facts; a dry-run, incomplete, blocked, failed, or interrupted result does not.

The Framework logical section records Framework source identity, exact target and
region identities, generated-region relationships, semantic baseline
fingerprints, and coverage/trust. The Extension logical section records selected
roots, package identities, exact dependencies, dependency order facts, owned
target-relative paths, shared-owner sets, per-package/path semantic baseline
fingerprints, and coverage/trust.

### Source, target, and reserved paths

An external package or catalogue source and the target workspace must be both
lexically and physically disjoint. Reject a source inside the target, a target
inside the source, path or physical aliases, and every other overlap. The source
is read-only input and is never mutated.

Extension payloads cannot target the common lifecycle document or current
Extension receipt, including `open-forge.extensions.json`, `.git` internals, adjacent recovery or operation artifacts,
workspace-owned `.overwrite.md` companions, Framework root or provider blocks,
or paths owned by another manager. They cannot use a collision to adopt or
replace existing content. The exact supported package path grammar remains a
Gate 3 decision.

### Format-insensitive semantic fingerprints

The candidate accepts format-insensitive managed identity through
parser/AST-derived, syntax-aware canonical semantic fingerprints. It does not
accept a rule that strips all whitespace, folds all Unicode, or compares only a
loose text approximation.

The fingerprint policy is:

- Preserve Unicode and semantic text. Do not apply blanket ASCII conversion,
  Unicode loss, case folding, or normalization that the supported syntax does
  not prove safe.
- Preserve headings, tags, links and destinations, marker meaning, inline
  content, code-block content, and semantically significant whitespace.
- Normalize line endings and only parser-proven formatting trivia.
- Canonicalize supported frontmatter and Markdown semantics through a
  syntax-aware representation. Unsupported or ambiguous syntax fails closed.
- Exclude derived generated `Entries` interiors from authored package and
  Framework semantic identity. Marker boundaries, authored bytes outside the
  derived interior, and generated projection facts remain operation-time
  boundaries.
- Use exact bytes for unsupported, binary, or otherwise unparseable kinds because
  their persistent managed fingerprint is exact-byte based.
- Fail closed when equivalence cannot be proven. The CLI does not guess that two
  files are equivalent because they look similar.

For supported parseable kinds, the lifecycle document persists only the baseline
semantic fingerprint. It does not persist an exact-byte baseline digest. Each
operation computes current and intended fingerprints from fresh facts and
freshly captures current exact bytes in the operation plan. Those exact bytes
remain necessary for the plan, diff, expected-state revalidation, backup,
deletion or write verification, and recovery. A persistent semantic fingerprint
does not remove the need for exact-byte safety checks.

For removal and prune classification, compare the current semantic fingerprint
with the persisted baseline semantic fingerprint. Do not compare current exact
bytes with a recorded exact-byte baseline. If current exact bytes differ from
the current intended source bytes while the supported current, baseline, and
intended semantic fingerprints are equal, the operation reports a
formatting-only observation and does not treat the content as managed divergence
by itself. The candidate preserves the current formatting rather than running a
user formatter. No pre-format or post-format formatter receipt is required.

The earlier D047 direction remains the current Agenda direction until the
maintainer accepts this revision and the accepted result is integrated. If this
candidate is accepted, D047 must be revised to remove CLI formatter execution
and formatter receipts from this lifecycle identity. The CLI may conservatively
detect a supported formatter configuration and advise the user to exclude
`.agents` or intentionally format and review before commit. Detection never
grants authority, chooses a formatter, changes files, or makes a formatting
guess.

### Generated navigation

Framework and Extension lifecycle plans use the current [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md)
and [Index Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md):

1. Form the hypothetical post-operation workspace from current authored content
   plus the permitted payload, block, ownership, and deletion effects.
2. Preserve user-added routes and intentionally absent defaults.
3. Derive every affected generated `Entries` region from intended authored
   topology and metadata, not from package-owned generated interiors or stale
   embedded lines.
4. Change only a valid bounded generated interior and preserve its markers and
   outside bytes.
5. Verify generated navigation as part of the same parent lifecycle plan.

Generated `Entries` are derived navigation and are not package-owned authored
bytes. A package can own an entrypoint that causes a generated parent to change,
but its receipt does not claim the generated interior. A missing, duplicate,
reversed, nested, or ambiguous generated boundary blocks the complete plan; the
lifecycle does not repair it by force.

### Ownership, dependencies, and shared owners

Extension ownership is explicit and stable:

- A managed package has a stable ID and a valid lifecycle record. Whole
  target-relative package files may occupy supported routes, subject to route,
  source, containment, and ownership checks.
- Dependencies use exact stable IDs, resolve offline and transitively in the one
  selected source universe, and are ordered before dependents.
- Unknown IDs, duplicate active IDs, duplicate dependency declarations, invalid
  manifests, cycles, unsafe package paths, and incomplete closures reject the
  complete plan before writes.
- Shared-owner sets are explicit. When two explicit managed package owners target
  one physical path, compatible shared content requires equal supported
  canonical semantic fingerprints plus compatible path, route, and metadata
  facts. Formatting-only source-byte differences do not create a shared-owner
  conflict. Compatible shared files retain remaining owners when one package is
  removed. Different intended content targeting one portable path is a conflict.
  Semantic equality never authorizes adoption of an unowned existing file.
- A retained dependent blocks removal of a dependency. A dependency that becomes
  an orphan remains recorded and installed. There is no automatic orphan prune.
- A route host cannot be removed while retained routed descendants depend on it.
- A package source is never removed by install, update, or remove.

Manual copying, idless packages, direct overlays, and externally managed native
Skills remain valid ordinary unmanaged content. No routing operation, stable
path, matching fingerprint, or identical byte sequence transfers ownership.

### Git, backup, and recovery

Every mutating lifecycle operation uses the same product boundary:

- Build one complete plan before effects. Do not apply a safe subset when another
  selected path is blocked, incomplete, ambiguous, or unsafe.
- Check Git cleanliness only for existing affected paths the plan may change or
  delete. Dirty affected paths block by default.
- `--skip-git-check` bypasses only that affected-path cleanliness check and
  activates accepted adjacent `.bak` recovery where an existing-byte replacement
  or eligible deletion needs it. It never bypasses ownership, collision,
  containment, markers, expected state, verification, or recovery readiness.
- Prove backup readiness before the first effect. Never overwrite an unknown or
  colliding adjacent backup. Retain backups until complete per-effect and
  whole-operation verification proves they are unnecessary.
- Revalidate expected state immediately before each effect. An unexpected
  concurrent change is preserved rather than overwritten by recovery.
- Verify every effect and the complete postcondition. On handled failure, reverse
  applied effects in reverse order only while identity guards still match.
- Preserve and report backups or residuals needed after interruption. A later
  invocation forms a fresh plan from current facts; it never replays a saved plan
  or journal.
- Operation staging and temporary replacement state are ephemeral implementation
  details. They are not lifecycle facts, receipts, history, or runtime meaning.

Deletion requires the same recovery discipline as replacement. `--prune` and
Extension `remove` do not make recovery weaker or permit arbitrary cleanup.

### Dry-run parity and event boundary

Dry-run and apply resolve the same request, current facts, baseline, intended
state, complete plan, preflight, and pre-effect planning status. Dry-run shows
the complete selected plan and diff, then stops before effects and writes
nothing. It cannot prove application, expected-state revalidation immediately
before effects, verification, lifecycle publication, or recovery success, and
it never produces an apply-time `failed` or `interrupted` result. An actual
apply may subsequently become `failed` or `interrupted` when a concurrent edit,
I/O event, caller interruption, verification failure, or recovery event occurs.

### Results, streams, and output facts

All lifecycle operations reuse the seven shared statuses:

| Status        | Product meaning                                                                                                                                                                                |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The read-only observation, no-op, dry-run, or apply has complete applicable coverage and no finite attention condition.                                                                        |
| `attention`   | The operation has complete safe coverage but preserves a finite changed, missing, retired, changed-final-owner, or equivalent lifecycle condition that the selected authority did not resolve. |
| `incomplete`  | Safe required source, lifecycle, parser, or recovery coverage is unavailable. The operation does not invent a fact.                                                                            |
| `invalid`     | Command syntax, operands, required semantic input, conflicting selection, flag value, or noninteractive missing input prevents request resolution.                                             |
| `blocked`     | An unsafe, ambiguous, colliding, unauthorized, untrusted, dirty, retained-dependent, route-unsafe, or otherwise incomplete-to-mutate boundary prevents one safe plan.                          |
| `failed`      | Inspection, planning, application, verification, lifecycle persistence, or handled recovery failed unexpectedly or left an unsafe residual.                                                    |
| `interrupted` | The caller interrupted before completion and no stronger recovery failure changes the result.                                                                                                  |

For ordinary planning conditions, use `blocked` > `incomplete` > `attention` >
`complete`. Invalid input stops earlier. Failed and interrupted retain their
event meaning. Planned changes, planned destruction, force authority, prune
authority, or format-only observations alone do not create `attention`.

Human primary results use these streams:

- `complete`, `attention`, and `incomplete` go to stdout.
- `invalid`, `blocked`, `failed`, and `interrupted` go to stderr.
- Bounded diagnostics go to stderr without splitting the primary human result.

`--json` emits one complete typed result on stdout for every status. It never
prompts and never mixes ordinary human text into JSON stdout. Human and JSON
renderers consume the same result and do not rerun lifecycle work. Every result
may contain at most one required `Next:` action.

The expanded human result should expose, as applicable:

- exact workspace and source/location selection;
- Framework footprint or Extension package, root, dependency, and ownership
  counts;
- mode: human/direct, normal/force/prune, apply/dry-run, and automatic state;
- baseline, current, and intended coverage/trust and comparison facts;
- safe, planned, applied, unchanged, preserved, restored, overwritten,
  deleted, shared, and released-ownership effects;
- formatting-only observations without turning them into divergence;
- generated-navigation projections and verification;
- lifecycle-document publication or preservation facts; and
- Git, backup, temporary, residual, and recovery facts.

Compact output is only a density projection. It retains identity, source or
location, mode, key counts, safety and preservation facts, status, and at most
one required `Next:` action. Exact JSON field names, schema, diagnostics, and
numeric exits remain deferred.

### No core uninstall/remove boundary

The candidate rejects a current Framework uninstall or remove leaf. Never
recommend deleting `.agents` wholesale. Manual removal guidance must name exact
managed blocks and paths, remove them selectively, preserve user routes,
overwrite companions, Extensions, and Memory, and warn when retained
dependencies or routed descendants make a host unsafe to remove. A future safe
Framework uninstall requires a separate product and safety review.

Extension `remove` remains because a stable package receipt bounds the ownership
set and makes an explicit release operation reviewable. That distinction does
not grant Extension remove authority over Core, Framework routes, unknown files,
or another manager's content.

## 11. Product-level scenarios and conformance expectations

These are required future evidence shapes for the candidate if accepted. They
are not implementation, test, or release claims. Each mutating scenario expects
one complete plan and no effect before all selected safety facts pass.

### Request and interaction scenarios

1. Bare `open-forge` and bare `open-forge extension` show help and perform no
   lifecycle operation. The Extension group does not open a wizard.
2. Argumentless human Framework `install` uses the fixed embedded source and a
   compact confirmation/inspection flow. Argumentless human Framework `update`
   may present finite changed, missing, and retired decisions.
3. Argumentless human Extension `create`, `install`, `update`, and `remove`
   obtain only finite questions for their own typed request.
4. Explicit IDs, source paths, catalogue paths, `--all`, `--force`, `--prune`,
   `--automatic`, and `--dry-run` populate the same request as wizard answers.
   Conflicts are invalid rather than hidden precedence.
5. JSON and other noninteractive requests never prompt. Semantic input still
   missing after documented defaults and permitted single-package manifest-ID
   inference is invalid. Missing replacement/deletion authority or an unresolved
   safe choice is blocked.
6. The command path plus explicit IDs or subjects supplies operation authority.
   `--automatic` only suppresses interaction and chooses documented conservative
   defaults. It never adds package selection, initial force, divergent
   replacement or restoration, prune, adoption, or a safety bypass. It may
   execute ordinary safe effects already selected by the explicit operation and
   subjects. Repeating applicable Boolean flags is idempotent.
7. `--dry-run` resolves the same request, facts, intended state, complete plan,
   preflight, and pre-effect planning status as apply. It shows the complete
   plan and diff, writes nothing, and cannot prove application, verification, or
   recovery. It never produces an apply-time `failed` or `interrupted` result;
   actual apply may later do so because of events.
8. Unsupported names and shapes, including a Framework group, root `init`, a
   Framework uninstall/remove, a replacement/reinstall leaf, generic `apply`,
   saved-plan input, `--yes`, and semver-only package update selection, are not
   accepted by this candidate surface.

### Framework lifecycle scenarios

1. A safely absent workspace receives the embedded Framework, supported bounded
   root/provider blocks, projected generated navigation, and lifecycle facts
   only after complete verification.
2. An exact trusted managed Framework returns a verified install no-op. A
   formatting-only byte difference with an equal semantic fingerprint remains
   informational and does not create attention.
3. An initial exact current occupant blocks normal install. Force may replace it
   only when the complete recognized identity, ownership, route, containment,
   marker, Git, backup, and recovery facts are safe. The prior bytes are not
   adopted as history.
4. A managed Framework with changed, missing, retired, or old-source state is
   not reconciled by install. Install reports one useful next action to use
   update, with no force shortcut.
5. Normal update changes only baseline-unchanged and genuinely new safe content.
   Changed and missing expected content remains visible and preserved.
6. Force update overwrites only changed current expected paths and restores only
   missing current expected paths. It does not delete retired content or bypass
   any other safety boundary.
7. Prune update deletes only eligible retired managed content. Force plus prune
   composes both exact boundaries and never deletes unknown, unowned, shared, or
   unsafe content.
8. User routes, Memory, overwrite companions, Extension paths, and authored
   bytes outside valid generated or managed blocks remain unchanged.
9. Generated navigation is projected from intended authored topology and metadata
   in the same plan. A malformed generated boundary blocks instead of being
   repaired by force.

### Extension source and package scenarios

1. Omitted `--source` reads the embedded catalogue for available facts. One
   exact package or one exact catalogue is the only external source form. An
   external source and target workspace must be lexically and physically
   disjoint; aliases, containment in either direction, overlap, and source
   mutation are rejected.
2. A selected source may infer only the stable ID declared by a completely
   validated manifest when it contains exactly one valid package. Folder or path
   spelling and resemblance never supply identity. Missing, duplicate,
   malformed, or conflicting manifest IDs are invalid or blocked. A
   multi-package source requires exact IDs or explicit `--all` in noninteractive
   use. Automatic mode never chooses all.
3. A package directory and catalogue directory are classified by manifest and
   package structure. Resemblance, ambient search, network, registry, cache,
   glob, fuzzy matching, and embedded fallback are never used.
4. Dependency resolution is exact, offline, transitive, source-universe-local,
   dependency-first, deduplicated, and fail-closed for unknown, duplicate, or
   cyclic graphs.
5. `create` writes only the exact local scaffold under its catalogue parent. It
   does not install, index a workspace, or write lifecycle state.
6. `list` without filters shows separate Installed and Available sections.
   Installed facts survive source unavailability. Available facts never imply
   installation. Both filters together return both sections.
7. `inspect` produces installed-only, available-only, or baseline/current/intended
   three-way facts. It retains receipt facts when source bytes are unavailable
   and does not claim that an update or restore can proceed without those bytes.
   List and inspect may report installed or legacy facts without a healthy
   Framework; they never turn those facts into mutation trust.
8. Managed Extension install and update require a trustworthy installed Framework
   anchor and complete route-host facts. List and create do not. Fresh Extension
   install establishes ownership for absent selected IDs and
   dependencies. An exact managed result is a no-op. Managed divergence directs
   to update, including when force is present.
9. If `extension install <id>` finds the ID only in installed state and no
   embedded or selected package source is available, it returns `incomplete`,
   writes nothing, and directs the user to inspect or supply `--source`; it
   cannot claim an exact no-op without intended source bytes.
10. Initial Extension force can replace only an exact eligible current source
    footprint occupant. It never adopts old bytes or bypasses another owner,
    route collision, marker, containment, or recovery boundary.
11. Normal update applies safe baseline-unchanged and new files while preserving
    changed, missing, and retired facts. Force and prune affect only their named
    boundaries.
12. Remove reads managed IDs and receipt facts without requiring package source.
    Its human wizard shows `Keep-as-unmanaged` versus `Delete` before planning.
    Noninteractive requests without `--prune`, including automatic mode, choose
    Keep; explicit `--prune` in that same request selects Delete. Remove may release selected ownership,
    delete unchanged eligible final-owner files, retain shared files, block
    retained dependents and unsafe route removal, and preserve changed final-owner
    files as unmanaged. A later prune cannot act on a file after that ownership
    release. A repeated remove is a no-op only with valid trusted proof that the
    ID and all selected ownership are already absent; missing or untrusted state
    is incomplete or blocked, never presumed success.
13. Remove with prune may delete only eligible changed final-owner content. It
    never deletes unknown, unowned, shared, or route-unsafe content. Package
    source remains untouched.

### Gate 1 divergence scenario

The central observed fixture remains exactly the Gate 1 evidence:

- the `development-toolkit` receipt declares 21 files;
- 9 declared files match the recorded state;
- 11 receipt-owned Template files have mismatched digests;
- receipt-owned `.agents/workflows/development.md` is missing; and
- a colliding user-owned `.agents/workflows/development/` subtree occupies a
  different route/source identity.

This packet preserves that fixture as evidence. It does not repair it, remove
it, adopt the colliding subtree, or claim that the new CLI has been tested
against it. Future conformance evidence must show that list and inspect expose
the separate facts, while every mutation respects the collision and ownership
boundaries before any write. The receipt's `sha256` values remain legacy byte
evidence in this fixture; they are not a new persistent exact-byte baseline for
the candidate lifecycle document.

### Lifecycle-document trust scenarios

1. A safely absent document or section is reported as absent only after complete
   inspection proves that no expected managed state, managed boundary, or
   recovery residual exists. A missing expected section is `incomplete` or
   `blocked`, never an empty section reconstructed from paths, bytes, or source.
2. A current section is trusted only with supported envelope and section
   versions and fingerprint policy, exact workspace and managed identities,
   intact internal consistency, reciprocal package/path/owner/dependency facts,
   no duplicate or conflicting IDs or paths, and complete verifiable coverage.
3. A recognized legacy receipt, including current `open-forge.extensions.json`
   schema evidence, may provide explicit legacy list/inspect facts. It cannot be
   mutated without an explicit verified migration that establishes every trusted
   fact atomically. Divergent or insufficient legacy data does not grant force,
   prune, or remove authority.
4. A malformed, unsupported, unverifiable, or internally inconsistent section
   may retain safe independent read-only facts as incomplete, but unsafe
   ambiguity is blocked and no managed mutation occurs. A path, matching
   fingerprint, current source, or force flag never makes it trusted.

### Shared safety, lifecycle, and result scenarios

1. A read-only operation may retain independently trustworthy facts from one
   lifecycle section when another section is malformed. Every mutation preserves
   the exact bytes and meaning of unrelated sections and common-envelope facts,
   publishes the whole document atomically, and writes nothing if any unrelated
   or common fact cannot be safely parsed, preserved, round-tripped, or verified.
   Opaque malformed state is never dropped, normalized, repaired, or rewritten
   as a side effect. Subject-specific lifecycle facts remain section-local and
   are not duplicated in the common envelope. Cross-section owner/path
   collisions block complete preflight.
2. Baseline, current, and intended semantic fingerprints distinguish unchanged,
   changed, missing, new, retired, and format-only states. Unicode, headings,
   tags, links, destinations, marker meaning, inline content, code blocks, and
   significant whitespace remain meaningful. Unsupported or ambiguous kinds fail
   closed.
3. Supported parseable kinds persist semantic fingerprints, not exact-byte
   baseline digests. Unsupported, binary, and unparseable kinds use exact-byte
   managed identity. Exact bytes are freshly captured for operation-time diff,
   expected state, backup, write, deletion verification, and recovery.
4. For two explicit managed package owners, shared content requires equal
   supported canonical semantic fingerprints and compatible path, route, and
   metadata facts. Formatting-only source-byte differences do not conflict, and
   semantic equality never adopts an unowned existing file.
5. Git checks affect only planned existing paths. Skip-Git remains independent
   from force and prune and requires the accepted backup/recovery path where
   needed.
6. Dry-run and apply resolve the same request, facts, intended state, complete
   plan, preflight, and pre-effect planning status. Dry-run shows the complete
   plan and diff and writes no payload, generated region, lifecycle fact, backup,
   or temporary artifact. It cannot prove application, verification, or recovery
   success and never produces an apply-time failed or interrupted result; actual
   apply may later become either status because of events.
7. Expected state is revalidated before effects, every effect and the complete
   operation are verified, handled failures recover in reverse order, and
   unexpected concurrent edits are preserved and reported.
8. Human result streams and the one-result JSON boundary are identical for all
   seven statuses. Bounded diagnostics remain stderr and at most one required
   `Next:` action is emitted.
9. A complete verified no-op repeats idempotently. Planned changes, planned
   destruction, force/prune presence, and format-only observations do not alone
   produce attention.
10. No operation creates a session, operation history, saved plan, transaction
    journal, hidden runtime meaning, or persistent formatter receipt.

## 12. Consequences, dissent, risks, and Gate 3 deferrals

### Consequences of each maintainer disposition

**Accept.** At review time, acceptance meant the current Install contracts and
Agenda would remain the integrated authority until the explicit authoring
sequence revised them. That authoring is now complete. The later Queue 30
first-release slicing proposal was rejected and superseded.

**Revise.** At review time, revision would have retained the useful unified
lifecycle facts while naming changes to the taxonomy, source rule, fingerprint
rule, ownership boundary, or output model. No current contract would have been
silently demoted or consumed by a later packet.

**Reject.** Queue 28's direct root `install` remains the current Framework
authority. Extension capability remains an open D017 detail to revisit through a
later candidate. The prior single-operation model, its simpler user surface,
and its historical safety reasoning remain available through Packet 1. No
implementation or Gate 3 work follows from rejection.

### Preserved dissent and risks

- **The prior single `install` operation is simpler.** It was the strongest
  losing alternative and the last integrated authority before Queue 29. The
  accepted split adds taxonomy, but makes management establishment and
  reconciliation intent explicit and prevents an install request from silently
  becoming an update.
- **The earlier Extension replacement lane was clearer to some reviewers.** The
  candidate removes that separate leaf and composes the narrower `update
--force` and `update --prune` boundaries. This reduces command count but makes
  the current expected versus retired distinction essential.
- **AST fingerprints can hide changes if canonicalization is too broad.** The
  policy must preserve semantic text and syntax, use exact bytes operationally,
  and fail closed whenever equivalence is not proven. Gate 3 must not relax this
  product guarantee for implementation convenience.
- **One physical lifecycle file increases shared-file blast radius.** Logical
  section isolation, affected-section validation, atomic publication, and
  preservation of trustworthy unrelated facts are required. A convenient
  co-located file must not become a single undifferentiated authority.
- **Wizards can create hidden modes.** The Pattern's one typed request, explicit
  authority, no-prompt JSON rule, and conservative automatic behavior must be
  tested at every guided leaf. A recommendation cannot become consent merely
  because it was displayed.
- **Strict source-universe closure rejects some local-package convenience.**
  That cost is intentional: provenance, dependency resolution, and offline
  reproducibility are clearer without embedded fallback or ambient discovery.
- **Core uninstall remains omitted.** Careful manual removal is the only current
  path for Framework deletion. That leaves user-facing burden, but a broad core
  removal operation would have a larger and less bounded ownership risk.
- **D047 currently permits a configured formatter lane.** The candidate's
  format-insensitive identity and no-formatter direction must not be treated as
  current Agenda state until the maintainer accepts and integrates the revision.
- **The Gate 1 divergent receipt is uncomfortable evidence.** It demonstrates
  why receipts, fingerprints, route identity, and user-owned collisions cannot
  be inferred or repaired by a new command merely because a package exists.

### Gate 3 deferrals

This packet deliberately defers all of the following:

- exact lifecycle filename, path, common envelope, section schema, version,
  migration, canonical serialization, and atomic publication mechanics;
- semantic canonicalization details, hash algorithm, digest encoding, parser
  libraries, AST representation, and supported file-kind normalization;
- filesystem physical identity, symlink/junction/hard-link handling,
  containment algorithm, locks, concurrency guarantees, and platform behavior;
- exact backup names, temporary paths, staging, atomic replacement, residual
  retention, cleanup, and interruption mechanics;
- exact JSON schema, field compatibility, numeric exits, diagnostic structure,
  redaction, stream framing, and process-level error mapping;
- high-level C# architecture, command modules, dependency direction, shared
  service boundaries, Native AOT trimming, source generation, and package
  implementation;
- package catalogue implementation, exact supported package path grammar,
  manifest parser behavior, and release packaging; and
- tests, fixtures, Native AOT process evidence, package evidence, performance,
  and release evidence.

These are Gate 3 or later decisions. The product rules above are not permission
to choose their implementation now.

## Historical maintainer rereview checklist

The maintainer's rereview used the explicit **accept**, **revise**, or **reject**
questions below. This historical checklist records the review surface; its
accepted answers now live in the current contracts and Decision Agenda. No item
is authoritative merely because it appears here.

1. **Authority boundary:** Confirmed that Queue 29 remained contextual during
   review, that Queue 28's direct root `install` was the prior authority, that
   Packet 1 remains history, and that Queue 30 was then waiting. Queue 30's
   slicing proposal was later rejected and superseded; Queues 31–33 now cover the
   remaining command reviews.
2. **Superseding model:** Accept, revise, or reject the deliberate split of
   Framework `install` and `update` and the unified lifecycle facts across
   Framework and Extensions.
3. **Exact surface:** Accept, revise, or reject every command and flag in the
   candidate syntax, including the six Extension leaves, `--all`, `--source`,
   `--path`, `--automatic`, `--force`, `--prune`, `--dry-run`, and
   `--skip-git-check`. Confirm that no replacement/reinstall leaf, generic
   `apply`, saved plan, `--yes`, semver-only package update, or Framework
   uninstall/remove leaf is added.
4. **Install meaning:** Confirm that install establishes management, exact
   managed state is a verified no-op, managed divergence directs to update, and
   initial force can replace only an exact eligible recognized occupant without
   adopting old bytes or bypassing ownership, collision, containment, marker,
   or recovery boundaries. An installed-only ID without an embedded or selected
   source is `incomplete`, writes nothing, directs to inspect or `--source`, and
   is not an exact no-op.
5. **Update meaning:** Confirm that managed Framework and Extension update
   require a trustworthy installed Framework anchor and complete route-host facts,
   that Extension update also requires trusted existing lifecycle state and
   explicit current source identity, semantic versions remain descriptive,
   normal updates preserve changed/missing/retired content, force handles only
   changed/missing current expected paths, prune handles only retired managed
   content, and force plus prune composes only where safe.
6. **Human/direct behavior:** Confirm the application of the existing Pattern and
   CLI-D086: groups show help, finite argumentless leaves may wizard, explicit
   inputs populate one request, JSON never prompts, and automatic mode never
   adds package selection, initial force, divergent replacement/restoration,
   prune, adoption, or a safety bypass. It may execute ordinary safe effects
   already selected by the explicit operation and subjects, including safe
   unchanged final-owner removal.
7. **Source universe:** Confirm one exact package or catalogue source, embedded
   default for available operations, structural package/catalogue distinction,
   exact-ID dependency closure, no fallback/network/registry/cache/glob/fuzzy or
   mixed provenance, the single-package manifest-ID inference rule, disjoint
   lexical and physical source/target boundaries, reserved-path exclusions, and
   the `--all` rule. Leave exact package path grammar to Gate 3.
8. **Create and read-only projections:** Confirm that create writes only the
   catalogue scaffold, that list separates Installed from Available, that
   inspect supports installed-only/available-only/three-way comparison, and that
   source unavailability does not erase installed facts or pretend update bytes
   exist. List and inspect may report installed or legacy facts without a healthy
   Framework; list and create do not require that anchor.
9. **Extension lifecycle:** Confirm stable IDs, whole supported-route files,
   dependency-first ordering, unknown/duplicate/cycle rejection, shared-owner
   sets, retained-dependent blocking, orphan retention, route-host safety,
   package-source preservation, and unmanaged manual/idless/direct-overlay
   behavior.
10. **Removal authority:** Confirm that remove needs no package source bytes but
    does require explicit managed IDs and complete trusted ownership, route,
    generated-navigation, and cross-section preservation facts. Confirm that it
    releases ownership visibly, deletes safe unchanged final-owner files,
    retains shared files, preserves changed final-owner files by default, blocks
    retained dependents and unsafe route removal, and lets prune delete only
    eligible changed final-owner content.
11. **Gate 1 evidence:** Confirm that the exact development-toolkit divergence
    remains evidence only: 21 declared, 9 matching, 11 mismatched Template
    files, missing `development.md`, and the colliding user-owned
    `development/` subtree. Confirm that this packet makes no repair or test
    claim.
12. **Lifecycle document:** Confirm one future transparent physical document
    with separate logical Framework and Extension sections, no authority merge,
    finite trust states, section-local subject facts and validation, no
    subject-fact duplication in the common envelope, exact unrelated-section
    preservation, cross-section collision preflight, and no operation history,
    session, saved plan, journal, or runtime meaning. Leave exact path, schema,
    migration, serialization, and atomic mechanics to Gate 3.
13. **Semantic identity:** Accept, revise, or reject parser/AST-derived
    syntax-aware format-insensitive fingerprints, preservation of Unicode and
    semantic whitespace/content, exclusion of generated Entries interiors,
    semantic fingerprints as the only persistent identity for supported parseable
    kinds, exact-byte operation facts, exact-byte identity only for unsupported,
    binary, and unparseable kinds, and fail-closed equivalence. Do not add a
    persistent exact-byte baseline digest.
14. **Formatter disposition:** Queue 29 explicitly revised D047 to remove
    formatter execution and formatter receipts while allowing conservative
    detection/advice only. The change is recorded in the Decision Agenda rather
    than implied by this packet.
15. **Generated, Git, recovery, and results:** Confirm current Index projection,
    whole-plan mutation, affected-path Git policy, adjacent-backup recovery,
    expected-state revalidation, reverse recovery, seven statuses, human streams,
    one-result JSON, bounded diagnostics, one `Next:`, dry-run parity through
    request/facts/intended state/plan/preflight and pre-effect planning status,
    no dry-run claim of application/verification/recovery success, and no
    attention for planned changes/destruction/format-only observations alone.
16. **Core boundary:** Confirm that Framework uninstall/remove remains rejected,
    `.agents` wholesale deletion is never advised, and manual guidance is
    selective and dependency-aware.
17. **Consequences and sequence:** The accepted result moved into the current
    contracts and program records through the integration set below. Gate 2
    remains open, and no implementation or Gate 3 work followed from it.

### Completed post-acceptance integration set

The accepted Queue 29 result was authored and validated in the following set.
The listed sources remain their own authorities; this packet does not merge them:

1. The Decision Agenda and current Framework `install` Interface and Behavior
   contracts were revised.
2. Root Framework `update` and the six Extension Interface and Behavior sets
   were created.
3. The `status` Interface and Behavior contracts and the `doctor` lifecycle
   consumers were updated so their installed, legacy, trust, ownership, and
   coverage facts consume the accepted model without adding mutation authority.
4. The four overview Documents were aligned: `command-contract-set.md`,
   `command-interface-contract.md`, `command-behavior-contract.md`, and
   `command-technical-design.md`.
5. The public CLI and Extension documentation and Composable CLI Pattern
   examples were aligned.
6. Affected generated navigation and `Entries` were regenerated and validated
   through the available legacy index and Doctor checks.
7. The integrated contract surface and CLI release records were updated. Queue
   30 was later rejected and superseded; Queues 31–33 now carry the remaining
   command decisions.

The sequence still authorizes no implementation, no Native AOT or package
design, and no Gate 3 start. The review record remains `#Contextual` after any
accepted result is integrated elsewhere.
