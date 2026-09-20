---
open-forge:
  description: Contextual historical review of Framework installation, managed update, replacement, boundaries, recovery, and lifecycle state
  responsibility: Preserve the Queue 28 review history and maintainer disposition without replacing current Install authorities
  tags: [Memory, Archived, CLI, Release, Review, Candidate, Framework, Lifecycle, Installation, Replacement, Safety, Contextual, Historical]
---

# Framework Lifecycle Review Packet

## Status and use

This is **Packet 1 of 3** in the sequential review history. It is contextual
historical analysis, not accepted CLI contract authority. The
[Review Queue](queue.md), the [Maintainer Decision Authority
Directive](../../../../directives/decision-authority.md), and the current
[CLI Decision Agenda](../../../working/cli-release/decision-agenda.md) keep that boundary explicit.

This packet does not replace an accepted contract, author a second command
Interface or Behavior Contract, or claim Gate 2 closure. It records the Queue 28
direction as historical context. Queue 29 later accepted and integrated a
superseding Framework/Extension lifecycle direction; the current linked
contracts, Agenda, and program records answer the present meaning. The new CLI
does not ship, and Gate 2 remains open.

The audience is the maintainer reviewing the Framework lifecycle product
meaning alongside its current command contracts. The purpose is to make first
install, normal managed update or reinstallation, bounded replacement,
preservation, recovery, and lifecycle state reviewable together.

## Queue 28 disposition — 2026-08-16

The maintainer accepted the Queue 28 Framework lifecycle decision as the direct
root operation below. This section preserves that historical disposition; Queue
29 later superseded its normal managed-reconciliation meaning:

```text
open-forge install [--force] [--dry-run] [--skip-git-check] [global flags]
```

At Queue 28, the [Install Interface Contract](../../../crystallized/documents/cli/contracts/install/interface.md)
and [Install Behavior Contract](../../../crystallized/documents/cli/contracts/install/behavior.md) were the
accepted Working authorities for this public and technology-neutral meaning.
Queue 28's `install` covered first installation and ordinary managed update or
reinstallation. Queue 29 later moved trusted managed reconciliation to root
`update` and narrowed initial `install --force`; the current contracts are the
authority wherever they differ from this historical packet. The Queue 28
command path plus `--force` supplied replacement and confirmation authority for
the same operation in human and JSON noninteractive use, but only for the exact
recognized current Framework footprint.

There was no `framework` group, root `init`, or separate update, reinstall,
replace, restore, recover, uninstall, or exclusion operation in Queue 28. The
earlier two-leaf separation remains historical dissent and the strongest losing
alternative to that decision. This packet records that history; it does not
become a second authority source.

Current examples are `open-forge install` for normal application and
`open-forge install --force --dry-run` for a force-authorized preview. The
Install Interface owns the complete example set and local flag meaning.

## Historical council recommendation and accepted divergence

Before the disposition above, the reconciled council recommendation was one
`framework` group with two candidate leaves:

```text
framework
├── install
└── replace
```

- **Historical `framework install`** covered first installation and ordinary safe managed
  update or reinstallation. A repeated satisfied request with no remaining
  finite attention condition is a verified no-op.
- **Historical `framework replace`** supplied explicit fresh or replacement authority for
  the exact recognized current Framework distribution footprint. It is not a
  generic `--force`, `--pro`, or broad filesystem bypass.

The council also compared three leaves, `install`, `update`, and `replace`. The
earlier two-leaf separation was the strongest losing alternative because first
install and ordinary update use the same normal preservation authority: add
absent safe payload, change only baseline-proven unchanged managed content,
preserve divergence, and stop before unsafe or incomplete writes. The accepted
direct command won because a group with one actual operation is ceremonial and
force remains one bounded authority dimension of that operation. Cold
discoverability evidence may still explain the historical comparison, but it
does not authorize another lifecycle leaf.

The losing two-leaf proposal treated `replace` as one operation over the **whole
recognized current Framework footprint for the exact selected workspace**,
planned and reported at exact file or managed-region granularity. The accepted
`install --force` retains that bounded replacement scope without making it an
arbitrary filesystem overwrite or accepting an unspecified path wildcard. A
narrower explicit selection rule could be a later bounded product choice if
evidence requires it; it is not implied by `--force`.

## Review basis and authority map

Each source below answers its own question. This packet links to those sources
without promoting review analysis into their authority.

| Question                                                  | Current source and boundary                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Product scope and the accepted Framework lifecycle choice | [CLI-D016 in the Command Decisions](../../../working/cli-release/decision-agenda.md#command-decisions) and [CLI-D091](../../../working/cli-release/decision-agenda.md#contract-system-decisions) record the direct root `install` syntax, normal and bounded `--force` modes, and the major lifecycle deferrals. The [Install Interface Contract](../../../crystallized/documents/cli/contracts/install/interface.md) owns the complete public meaning.                                                                                                                                                                                                                                             |
| Mutation and recovery baseline                            | [Safety Decisions](../../../working/cli-release/decision-agenda.md), especially CLI-D041–D048, define explicit plans and authority, shared dry-run, affected-path Git checks, expected-state revalidation, verification and reverse handled recovery, bounded formatting, and preservation of user content.                                                                                                                                                                                                                                                                                                                                                                                         |
| Framework meaning and distribution                        | [Framework Architecture](../../../crystallized/documents/framework/architecture.md) defines the shipped product, canonical entry, recursive customization, generated state, ownership boundaries, and distribution obligations.                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| Scope, identity, and customization                        | [Route Scope and Inheritance](../../../crystallized/documents/framework/routing/scope.md), [Routing Paths and Identity](../../../crystallized/documents/framework/routing/paths.md), [Overwrite Customization](../../../crystallized/documents/framework/routing/overwrites.md), and the [Routing Model](../../../crystallized/documents/framework/routing/model.md) define managed-route recognition, path identity, overwrite ownership, and route meaning.                                                                                                                                                                                                                                       |
| Markdown and generated boundaries                         | [Routed Markdown Representation](../../../crystallized/documents/framework/markdown/routes.md) and the [Markdown Compatibility Boundary](../../../crystallized/documents/framework/markdown/compatibility.md) define canonical entrypoints, generated `Entries`, supported compatibility names, and fail-closed marker behavior.                                                                                                                                                                                                                                                                                                                                                                    |
| Root and provider blocks                                  | The [Managed Root Entry Pattern](../../../../patterns/open-forge/managed-root-entry.md), [Payload Maintenance](../../../crystallized/documents/maintenance/payload/_payload.md), [AGENTS Entry Maintenance Contract](../../../crystallized/documents/maintenance/payload/AGENTS.md), and [Claude Code Bridge Maintenance Contract](../../../crystallized/documents/maintenance/payload/CLAUDE.md) define the bounded block and bridge responsibilities.                                                                                                                                                                                                                                             |
| Generated `Entries` projection                            | The current [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md) and [Index Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md) define authoritative topology and authored-metadata projection, bounded generated interiors, dry-run/apply parity, verification, recovery, and the prohibition on hidden subprocess maintenance. Every lifecycle-generated `Entries` effect uses this authority.                                                                                                                                                                                                                                              |
| Operation shape and shared results                        | The [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md), [Global CLI Flags](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md), [CLI-D090](../../../working/cli-release/decision-agenda.md#contract-system-decisions), and the current [route init](../../../crystallized/documents/cli/contracts/route/init/interface.md), [route create](../../../crystallized/documents/cli/contracts/route/create/interface.md), and [route update](../../../crystallized/documents/cli/contracts/route/update/interface.md) contracts define the one-plan, dry-run, seven-status, stream, no-op, and recovery shape to reuse later. |
| Orientation and diagnosis boundaries                      | The current [Status Interface](../../../crystallized/documents/cli/contracts/status/interface.md) and [Status Behavior](../../../crystallized/documents/cli/contracts/status/behavior.md) keep orientation read-only and stateless. The [Doctor Interface](../../../crystallized/documents/cli/contracts/doctor/interface.md) and [Doctor Behavior](../../../crystallized/documents/cli/contracts/doctor/behavior.md) diagnose Framework lifecycle and recovery domains without mutating them.                                                                                                                                                                                                      |

The [Writing Standard](../../../crystallized/documents/maintenance/writing.md),
[Dictionary](../../../crystallized/documents/maintenance/helpers/dictionary.md),
and [source-locality directive](../../../../directives/source-locality.md) guide
this packet's prose and its later command-local placement.

Historical CLI-v2 material and the frozen MVP are evidence only. The [CLI-v2
archive](../../../archived/cli-v2/_cli-v2.md) and [frozen MVP
implementation](../../../../../src/cli-mvp/cli.ts) do not define this lifecycle.
The repository `.temp/` directory is raw repository evidence storage, not
Framework lifecycle temporary state and not a lifecycle cleanup or installation
target.

## Review method

Three independent first-round lenses were used, followed by one focused
reconciliation. They supplied evidence and challenges, not a vote or an
acceptance event.

| Lens                      | Question it tested                                                                                      | Material emphasis carried into the reconciliation                                                                                                                                                                              |
| ------------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Cold journey and product  | Can a new user understand the normal path, the exceptional path, and the result after repeating either? | Keep normal install/update convergent, make replacement explicit in the command path, and avoid a taxonomy whose distinctions users cannot discover.                                                                           |
| Grounded lifecycle safety | What durable evidence is required before changing a file, region, bridge, or recovery artifact?         | Use a transparent baseline, exact recognized identities, complete plans, bounded regions, Git or `.bak` recovery, and fail-closed ambiguity.                                                                                   |
| Adversarial simplifier    | Which leaves, records, or recovery concepts can be removed without weakening a real guarantee?          | Reject a generic force flag, separate restore and recover leaves, persistent journals, implicit adoption, and automatic retired-file deletion.                                                                                 |
| Focused reconciliation    | Where did product simplicity and safety prefer different boundaries?                                    | Record the former two-leaf separation as the strongest losing alternative, accept one direct `install` operation with whole recognized-footprint `--force` authority, and preserve the three-leaf and narrower-target dissent. |

## Lifecycle model retained in the accepted contracts

The following model preserves the packet's detailed reasoning and historical
tradeoffs. The linked Install Interface and Behavior Contracts are the current
authorities when this summary and a contract differ.

### Exact subject and recognized footprint

Every lifecycle operation uses the exact current working directory or exact
`--workspace` value already established by CLI-D020. It never searches upward,
chooses a Git root, or discovers a nearby Framework. The subject is one exact
workspace and its recognized current Framework distribution footprint.

For the accepted operation, that footprint contains:

1. The embedded canonical Framework payload's current recognized targets below
   `.agents`.
2. The current explicitly supported root and provider managed regions. Current
   repository evidence names the canonical `AGENTS.md` block and the Claude
   Code `CLAUDE.md` bridge. It does not authorize arbitrary provider files,
   profile files, shell configuration, or discovered configuration.
3. The transparent lifecycle metadata needed to compare and preserve those
   targets and regions. The metadata is operational state, not Framework
   context.

The planner can report each file and region separately, but the accepted
`install --force` authority is over this recognized footprint as a whole. It
never expands the footprint from filename resemblance, tags, route location, or
byte coincidence.

### Transparent Framework lifecycle baseline

A transparent Framework lifecycle baseline is a necessary product concept for
managed update. It records only durable distribution and managed-target or
managed-region baseline facts needed to distinguish:

- a target or region that is unchanged from the last verified managed state;
- a target or region changed after that state;
- a previously managed target that is now missing, including a default the user
  intentionally removed;
- a genuinely new target in the current distributed payload; and
- a target or region that the current payload has retired.

The baseline gives the normal installer evidence for preservation. It does not
decide authored meaning, route authority, user intent, or Extension ownership.
In particular, a missing or malformed baseline never grants inferred ownership
from a familiar path or matching bytes. An existing manually installed or
untracked Framework does not become managed by byte or path coincidence.

The baseline is lifecycle metadata, not:

- runtime Framework meaning or a second Loader;
- a Context receipt, session, or Status `Initial (shipped)` measurement;
- a saved plan or executable report;
- operation history; or
- a transaction journal.

The [Status contracts](../../../crystallized/documents/cli/contracts/status/interface.md#initial-and-current)
already distinguish their embedded comparison payload from a saved historical
baseline. The [Doctor Framework lifecycle domain](../../../crystallized/documents/cli/contracts/doctor/interface.md#framework-lifecycle)
may diagnose missing, changed, malformed, or unavailable lifecycle evidence,
but Doctor does not install, replace, restore, or rebaseline it.

The baseline has logical state transitions, but this packet does not choose how
that state is stored or persisted. A fully applied and verified normal install
that returns `attention` may refresh the expected current distribution and
baseline facts for verified non-divergent managed targets and genuinely new
payload targets. It must retain changed, missing, and retired divergence facts,
and it must never promote divergent current workspace bytes into the expected
distribution baseline.

If a newly added target is later intentionally removed, the retained lifecycle
fact continues to identify it as a previously recognized managed target. A
later normal install therefore reports the target as missing or intentionally
removed and does not repeatedly re-add it. A dry-run, `incomplete`, `blocked`,
`failed`, or `interrupted` result makes no baseline change. These are logical
transition rules, not a choice of file location, schema, digest representation,
serialization, or persistence mechanics. Those details, including retention of
retired facts, remain Gate 3 decisions. CLI-D047's transparent lifecycle JSON
may also hold one bounded configured formatter, but formatter allowlisting,
affected-file scope, digest refresh, and failure behavior remain deferred as
that Agenda item states.

The [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md) and [Index
Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md) are also the sole detailed
authority for any generated `Entries` effect in either `install` mode. The
lifecycle planner must project each intended generated region from the
hypothetical post-operation workspace's current authored topology and metadata,
including preserved user-added routes and the continued absence of removed
defaults. It must not copy generated interior bytes from the embedded payload or
start a hidden `index` subprocess. The same projection, bounded-region rules,
plan, dry-run, application, verification, and recovery remain one lifecycle
operation.

### Lifecycle states are not ownership shortcuts

The useful distinction is between the distributed payload, the durable baseline,
current workspace facts, and recovery evidence:

| Fact source                                                              | What it can establish                                                                                                     | What it cannot establish                                                                                             |
| ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| Embedded current payload and supported bridge templates                  | Which current canonical targets and bounded regions the Framework distribution recognizes                                 | That an existing path is managed, safe to replace, or owned by Framework merely because it exists                    |
| Transparent lifecycle baseline                                           | Which recognized targets or regions were previously verified as managed, and how current state differs from that baseline | Runtime meaning, user intent, Extension ownership, or permission to replace a divergent target during normal install |
| Current workspace bytes, markers, route identity, and ownership evidence | Whether a recognized target or bounded region is present, changed, missing, ambiguous, or colliding                       | A missing target's intent, or authority to adopt an untracked installation                                           |
| Git, adjacent `.bak`, and current-operation evidence                     | Review or recovery facts for an affected operation                                                                        | Framework meaning, a saved plan, or a new managed target                                                             |

## Primary journey and state table

The following table is the primary review surface. Each row assumes one exact
workspace and one complete plan. A blocked or incomplete fact prevents all
writes; the lifecycle does not apply a safe subset.

| Journey and current state                                                                                                           | Accepted `install` normal behavior                                                                                                                                                                                                                                       | Accepted `install --force` behavior                                                                                                                                                                               | Resulting lifecycle meaning                                                                                                                                                                                       |
| ----------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Safe absence of the Framework and absent canonical targets                                                                          | Add absent current canonical payload targets and valid bounded root/provider blocks. Establish the baseline only after verification.                                                                                                                                     | Uses the same recognized footprint and may establish it, but adds no authority that normal installation needs.                                                                                                    | A fresh install is `complete` when fully verified and has no finite attention condition.                                                                                                                          |
| An occupied canonical target, unknown file, route collision, or unrecognized target blocks a planned addition                       | Stop before writes. Do not overwrite or silently adopt the occupant.                                                                                                                                                                                                     | Stop unless the target is an exact recognized current Framework target and every identity, ownership, and boundary check is safe. Never override an unknown collision.                                            | Unsafe or ambiguous identity is `blocked`.                                                                                                                                                                        |
| Trusted baseline and current bytes match the baseline; current payload has no applicable change and no preserved divergence remains | Produce a verified no-op. Do not rewrite timestamps or normalize unrelated bytes.                                                                                                                                                                                        | The same plan may verify no change; `--force` remains explicit replacement and confirmation authority for the operation.                                                                                          | `complete`; planned changes alone never create `attention`.                                                                                                                                                       |
| Trusted baseline and a current payload target or managed region has changed                                                         | Change only the baseline-proven unchanged targets or regions. Preserve the changed target. Add safe genuinely new absent targets.                                                                                                                                        | Replace the exact recognized current target or valid bounded root/provider region under explicit authority, subject to the complete plan, Git, confirmation, revalidation, verification, and recovery boundaries. | Normal install completes with `attention` for preserved divergence. Force is `complete` when its explicit effects verify.                                                                                         |
| A repeated normal install still has preserved changed, missing, or retired divergence from an earlier `attention` result            | Reinspect the same facts and make no effect. Do not re-add a missing target or promote divergent bytes into the baseline.                                                                                                                                                | No replacement occurs unless the caller explicitly supplies `--force`.                                                                                                                                            | `attention` with verified no-effect facts, not `complete`; `complete` no-op requires no remaining finite attention condition.                                                                                     |
| A previously baseline-managed target is missing, including an intentional default removal                                           | Preserve the absence. Do not restore it during ordinary install or infer a persistent exclusion from the absence.                                                                                                                                                        | Restore the missing recognized current target only at its exact current canonical destination, then establish the verified new baseline.                                                                          | Normal install can be safely completed with `attention`; explicit restoration belongs to `install --force`.                                                                                                       |
| User-added file, route, scope, or other content is outside the recognized payload                                                   | Preserve it. It is not a candidate managed target.                                                                                                                                                                                                                       | Preserve it. An overlap or ownership conflict blocks rather than granting replacement authority.                                                                                                                  | User content remains user-owned and visible to later diagnosis.                                                                                                                                                   |
| Adjacent `{name}.overwrite.md` exists beside a recognized base                                                                      | Preserve the overwrite as a separate user-owned layer. A safe base update does not merge or replace it.                                                                                                                                                                  | Never touch the overwrite companion. A base/overwrite identity or route collision blocks.                                                                                                                         | The overwrite remains part of Framework runtime layering, not lifecycle-owned replacement bytes.                                                                                                                  |
| A target or managed region was retired from the current payload                                                                     | Preserve the existing content. Do not delete it or silently remove it from the user's workspace.                                                                                                                                                                         | Do not touch a target that is outside the current recognized footprint merely because an old baseline names it.                                                                                                   | Normal install reports preserved retired state as `attention`; automatic deletion is deferred.                                                                                                                    |
| Root/provider markers are absent but the target file is otherwise a valid supported host                                            | Apply only the bounded block rule: append the canonical block when no matching markers exist. Preserve all outside bytes.                                                                                                                                                | Replace only the exact recognized block, never the host file outside it.                                                                                                                                          | A valid managed-region identity is not whole-file ownership.                                                                                                                                                      |
| Root/provider markers are incomplete, reversed, duplicate, or otherwise ambiguous                                                   | Stop before writing.                                                                                                                                                                                                                                                     | Stop before writing. `--force` is not marker-repair authority.                                                                                                                                                    | `blocked`, not a best-effort repair.                                                                                                                                                                              |
| A lifecycle plan changes routed topology or indexed metadata                                                                        | Project every affected generated region from the hypothetical post-operation workspace's current authored topology and metadata, including preserved user-added routes and removed defaults. Do not copy embedded generated interiors or invoke `index` as a subprocess. | Use the same current Index projection, bounded-region rules, one complete plan, and post-operation verification.                                                                                                  | Generated navigation follows [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md) and [Index Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md) authority. |
| Baseline or target identity is unavailable but safe absence is not established                                                      | Do not guess from paths, bytes, tags, or generated entries.                                                                                                                                                                                                              | Do not broaden replacement to compensate. A rebaseline requires a complete recognized-footprint plan.                                                                                                             | Safe but unavailable coverage is `incomplete`; unsafe or ambiguous identity is `blocked`.                                                                                                                         |
| Git reports an affected planned path as dirty                                                                                       | Block by default.                                                                                                                                                                                                                                                        | Block by default. `--skip-git-check` may bypass only the relevant-path cleanliness check.                                                                                                                         | Git policy remains separate from replacement authority.                                                                                                                                                           |
| Gitless or skipped-Git replacement needs old bytes                                                                                  | Use an adjacent target-associated `.bak` under the recovery policy. Do not overwrite an unknown adjacent artifact.                                                                                                                                                       | Use the same bounded recovery policy. Retain the backup until verification proves it unnecessary.                                                                                                                 | `.bak` is recovery evidence, not Framework restore or lifecycle meaning.                                                                                                                                          |
| The result is dry-run, `incomplete`, `blocked`, `failed`, or `interrupted`                                                          | Do not establish or refresh lifecycle baseline state. A dry-run has no persistent effect, and an unsuccessful operation cannot publish a new expected distribution.                                                                                                      | The same. A force rebaseline requires complete application and verification.                                                                                                                                      | Logical baseline state remains unchanged. Exact persistence mechanics remain Gate 3.                                                                                                                              |
| Application, verification, or handled recovery is interrupted                                                                       | Stop new effects, reverse applied effects only where identity remains expected, preserve needed recovery evidence, and report residual state. Rerun from fresh current facts.                                                                                            | The same. Do not replay a saved plan or create a persistent journal.                                                                                                                                              | `failed` when an unexpected or residual recovery failure remains; otherwise preserve the accepted interruption meaning.                                                                                           |

## Accepted `install` behavior summary

### First install

The fresh-install path consumes the embedded canonical payload and the current
supported bridge templates for the exact selected workspace. It creates absent
canonical targets and bounded root/provider blocks through one complete plan.

- A missing canonical target is added only when its path, source identity,
  containment, and planned bytes are safe.
- A valid supported root or provider host with no matching markers receives the
  canonical block according to the [Managed Root Entry Pattern](../../../../patterns/open-forge/managed-root-entry.md).
  A host with exactly one complete ordered marker pair receives replacement of
  that bounded block only; surrounding bytes remain unchanged.
- An occupied canonical target, unknown file, duplicate target identity,
  route/source collision, Extension-owned path, or ambiguous block stops the
  complete plan. Fresh installation never silently adopts a manually installed
  Framework or another user's content.
- The plan includes every selected target before any effect. It does not create
  a partial installation and leave the baseline claiming a complete one.
- The baseline is established only after all planned effects and final
  verification succeed.

Fresh installation is therefore additive where absence is safely established,
bounded where a supported managed block is recognized, and fail-closed where an
existing path could represent another authority.

### Normal managed update or reinstallation

The same direct command handles a later request against a trusted lifecycle baseline. It
does not require a separate `update` or `reinstall` operation because its normal
authority is the same preservation authority as first install.

The planner compares the current workspace with the baseline and current
payload:

1. Change a managed file or managed region only when the current bytes remain
   proven unchanged from the baseline.
2. Add a genuinely new current-payload target only when it is absent and its
   identity and boundaries are safe.
3. Preserve a target changed after the baseline, a target missing from the
   workspace, a user-added target, an adjacent overwrite, and a retired target.
4. Preserve authored bytes outside recognized generated or root/provider
   regions. Generated `Entries` remain derived navigation and are changed only
   inside a valid bounded generated region.
5. Refresh lifecycle facts only for the verified result. Preserved divergence
   remains visible to the next run.

### Baseline transitions for normal install

When a fully applied and verified normal install returns `attention`, it may
refresh the expected current distribution and baseline facts for the managed
targets and regions it verified as non-divergent, including genuinely new
payload targets it added. It retains the facts that a changed, missing, or
retired target diverges from that expected distribution. It never copies
divergent current workspace bytes into the baseline.

If a newly added target is removed after that verified install, its retained
managed-target fact makes the later absence recognizable. A subsequent normal
install returns `attention` with verified no-effect facts and does not repeatedly
re-add the target. A dry-run, `incomplete`, `blocked`, `failed`, or `interrupted`
result changes no logical baseline state. These transitions do not choose the
baseline's file, schema, digest, serialization, or persistence mechanics; those
remain Gate 3.

The absence of a previously managed default is not evidence that the user wants
it restored, and it is not a persistent exclusion record. The normal operation
does not invent exclusions. It reports safely preserved changed, missing, or
retired facts through the finite `attention` condition.

A missing, malformed, ambiguous, or incomplete baseline cannot be made safe by
scanning more files or comparing more bytes. If safe absence is established, the
request can take the first-install path. Otherwise an unsafe or ambiguous
identity is `blocked`, and safe but unavailable coverage is `incomplete`, with
no writes in either case.

### Generated `Entries` effects in both `install` modes

Every generated `Entries` effect planned by `install`, with or without `--force`,
uses the current [Index Interface](../../../crystallized/documents/cli/contracts/index/interface.md)
and [Index Behavior](../../../crystallized/documents/cli/contracts/index/behavior.md) as its detailed
authority. The lifecycle operation:

- first forms the hypothetical post-operation workspace, including intended
  authored effects, preserved user-added routes, and the continued absence of
  removed defaults;
- derives each affected generated region from that current authored topology
  and metadata, not from embedded generated interior bytes or stale generated
  lines;
- changes only a valid bounded generated interior and preserves its markers and
  outside bytes; and
- includes the projection, exact bounded effects, dry-run/apply parity,
  verification, and recovery in the same complete lifecycle plan.

It does not start a hidden `index` subprocess or create a second generated
navigation projection. A lifecycle write that cannot establish the current
Index boundary or required metadata blocks or is incomplete before any write.

## Accepted `install --force` behavior summary

`install --force` is explicit replacement and confirmation authority for the
same install operation, including human and JSON noninteractive use. It is not a
separate operation or generic safety bypass. It still requires one complete
plan, the shared dry-run planner and preflight, affected-path Git policy,
expected-state revalidation, verification, and handled recovery. Explicit
authority is not permission to ignore an unsafe boundary.

Within the exact recognized current Framework footprint, `install --force` may:

- replace a recognized canonical current-payload target whose identity is
  unambiguous;
- replace a valid bounded managed block in the canonical `AGENTS.md` or
  supported Claude `CLAUDE.md` host;
- restore a missing recognized current target; and
- establish or rebaseline the transparent lifecycle baseline after explicit
  replacement and complete verification.

Generated navigation remains the current Index projection inside the same
complete plan. `--force` adds no generated-marker repair or other generated
boundary authority.

It never replaces or deletes:

- arbitrary user files or bytes outside a recognized managed region;
- an adjacent `.overwrite.md` companion;
- Extension-owned content or a path with a Framework/Extension ownership
  collision;
- a route or source-identity collision;
- an unknown, retired-only, or otherwise unrecognized target; or
- malformed, duplicate, reversed, or ambiguous managed markers.

Each case blocks rather than being overridden. `--skip-git-check` bypasses only
relevant-path Git cleanliness as CLI-D043 requires. It does not grant
replacement, deletion, adoption, marker repair, ownership, containment,
verification, or recovery authority. The accepted `--force` flag is bounded to
the exact current recognized destinations and valid root/provider blocks; it is
not a generic `--force` or `--pro` bypass.

Replacement is not implicit adoption. When a manually installed Framework
happens to resemble the embedded payload, `install --force` may act only on
exact recognized current targets and bounded regions in a complete safe plan;
it does not bless arbitrary existing bytes as managed. A new baseline records
the verified replacement outcome, not a guessed history.

## Root and provider bridge boundary

Only the current explicitly supported root/provider managed regions are in the
recognized footprint. Current maintenance evidence names:

- the canonical `AGENTS.md` managed entry block, whose source is
  [`src/open-forge/AGENTS.md`](../../../../../src/open-forge/AGENTS.md); and
- the Claude Code `CLAUDE.md` bridge, whose source is
  [`src/open-forge/CLAUDE.md`](../../../../../src/open-forge/CLAUDE.md).

The [AGENTS maintenance contract](../../../crystallized/documents/maintenance/payload/AGENTS.md)
keeps the canonical root entry small and aligned with the dogfood block. The
[Claude bridge maintenance contract](../../../crystallized/documents/maintenance/payload/CLAUDE.md)
keeps the provider file to its exact imports and preserves the canonical
authority. Neither contract authorizes discovery of arbitrary providers,
profiles, shell configuration, or workspace config files.

The managed-region rules are closed:

1. No matching markers mean that installation may append the one canonical
   source block.
2. Exactly one complete ordered marker pair means that only its bounded content
   may be replaced.
3. Incomplete, reversed, duplicate, nested, or otherwise ambiguous markers
   block before writing.
4. Every byte outside the managed block remains workspace content and is
   preserved byte-for-byte.
5. Canonical source and dogfood managed blocks remain identical; a provider
   bridge carries provider-native imports rather than restating Framework
   policy.

This is a managed-region boundary, not whole-file ownership. A future provider
requires an explicit support decision and its own maintenance contract before
it enters the recognized footprint.

## Recovery and artifact distinctions

The lifecycle must keep four kinds of evidence separate. The [Shared CLI
Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md) and CLI-D041–D046 supply
the shared plan, dry-run, Git, revalidation, and recovery baseline.

| Evidence or artifact                                   | Accepted install meaning                                                                                                                                                           | Boundary                                                                                                                                                                                            |
| ------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Git                                                    | Review and default recovery evidence when available. Git cleanliness is checked only for paths the accepted plan may change.                                                       | Git does not grant ownership or replacement authority. `--skip-git-check` bypasses only that affected-path cleanliness check.                                                                       |
| Adjacent `.bak`                                        | Persistent target-associated recovery for Gitless replacement or for a replacement/deletion operation that skips Git. Retain it until complete verification proves it unnecessary. | It is not a Framework `restore` command, a baseline, runtime context, or permission to overwrite an unknown adjacent artifact. Exact name, identity, collision, and retention mechanics are Gate 3. |
| Operation temporary, staging, and comparison artifacts | Internal mechanics of the current plan, dry-run, comparison, or safe replacement. They produce no runtime Framework meaning and no lifecycle ownership or baseline authority.      | They are not saved plans, receipts, journals, or durable operation history. Exact temporary-directory and cleanup strategy is Gate 3.                                                               |
| Repository `.temp/`                                    | Raw repository evidence and working material that may contain historical or evaluation data.                                                                                       | It is never a Framework lifecycle temporary directory and is never an install, replacement, or cleanup target.                                                                                      |

There is no persistent transaction journal or saved executable plan in the
accepted operation. Every rerun resolves fresh current facts. Before writing, it
revalidates the expected state. After an effect, it verifies the target and the
complete operation; a handled failure reverses applied effects in reverse order
without overwriting an unexpected concurrent change. Needed `.bak` or residual
evidence remains visible after interruption. The [Doctor recovery and residual
state domain](../../../crystallized/documents/cli/contracts/doctor/interface.md#recovery-and-residual-state)
may diagnose these facts, but Doctor and general Repair do not clean or restore
Framework lifecycle state.

## Accepted and deferred dispositions

### Command and lifecycle dispositions

| Subject                                      | Disposition                                     | Why it is not a separate current leaf                                                                                                                                                                                         |
| -------------------------------------------- | ----------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Update and reinstallation                    | Keep inside direct `install`.                   | They have the same normal baseline-proven preservation authority and the same no-op and attention model. A separate name would split one convergent operation without changing authority.                                     |
| Framework restore                            | Do not add a `restore` leaf now.                | Explicit restoration of a missing recognized current target belongs to `install --force`; normal install must not restore removed defaults.                                                                                   |
| Recovery                                     | Do not add a `recover` leaf now.                | Reverse handled recovery is part of the current write operation. Adjacent `.bak` recovery evidence is not Framework restore. A user-facing recovery operation would need a separate authority and evidence decision.          |
| Framework uninstall                          | Defer.                                          | Destructive removal needs exact managed-target history, user-content preservation, baseline disposal, bridge handling, recovery, and confirmation semantics that are not accepted by CLI-D016. It is not hidden in `install`. |
| Persistent exclusions                        | Defer.                                          | An exclusion record would create new durable lifecycle meaning and must distinguish intentional removal from missing or unsafe evidence. The accepted operation preserves absence without inventing that state.               |
| Implicit adoption                            | Reject for this lifecycle.                      | Byte or path coincidence cannot prove ownership, history, user intent, or safe replacement. `install --force` may rebaseline only the exact recognized footprint after verified effects.                                      |
| Automatic deletion of retired files          | Defer.                                          | A retired payload target may still contain user value, and the current baseline cannot authorize destructive deletion. Preserve it and report attention until an explicit future removal contract exists.                     |
| Arbitrary provider discovery                 | Defer.                                          | Discovery would broaden the ownership and external-syntax surface beyond current supported maintenance evidence. Add providers only through explicit support and maintenance contracts.                                       |
| Legacy migration without trusted evidence    | Defer.                                          | Historical CLI-v2/raw and frozen-MVP behavior is non-authoritative. Migration requires trusted identity, ownership, compatibility, and recovery evidence rather than resemblance.                                             |
| CLI cleanup of recognized residual artifacts | Keep separate from Framework lifecycle meaning. | CLI-D017A already describes a future explicit cleanup operation, but exact artifact identity and retention remain open. It must never treat repository `.temp/` or unknown adjacent files as cleanup targets.                 |

### Explicit non-goals

This contextual packet does not replace the current Install contracts and does not:

- define a second public Interface or Behavior Contract, or override the exact
  grammar, output, schema, or parser boundaries owned by the current contracts;
- turn `--force` into a generic safety bypass, arbitrary overwrite, or silent
  adoption mode;
- make the Framework lifecycle a runtime authority, a second Loader, a Context
  receipt, a session, a saved plan, an operation-history store, or a transaction
  journal;
- replace authored Markdown meaning, route identity, `AGENTS.md` prose outside
  its managed block, a provider bridge outside its block, or any
  `.overwrite.md` companion;
- manage Extension lifecycle or change Extension-owned files;
- restore missing defaults during normal install, delete retired content
  automatically, or infer persistent exclusions;
- discover arbitrary provider, profile, shell, package, or configuration files;
- make `status` perform diagnosis or mutation, or make `doctor` perform a
  lifecycle operation; or
- decide the D005 first useful release slice. Packet 3 owns release slicing.

## Shared result behavior recorded in the accepted contracts

The single `install` operation, with or without `--force`, uses the current
seven-status vocabulary and shared human/structured result shape. This summary
records the accepted application of the
[Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
and the current route-write closure, not a new status authority. The Install
Interface and Behavior Contracts own the detail.

| Status        | Accepted Framework lifecycle meaning                                                                                                                                                                                                                                                                                                     |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Application completes and verifies, or dry-run establishes, a complete safe plan without a finite attention condition, or the request is a verified no-op. Planned changes alone do not create `attention`. A repeated normal install with preserved divergence is not this no-op.                                                       |
| `attention`   | Normal `install` safely completes or previews while preserving known changed, missing, or retired managed facts, including a repeated no-effect run while that divergence remains. Force uses this status only for a finite preserved lifecycle or recovery observation. The divergence is finite and visible; it is not a failed write. |
| `incomplete`  | Safe facts exist, but required baseline, target, payload, or coverage information is unavailable or cannot be completed safely. No write begins.                                                                                                                                                                                         |
| `invalid`     | Command input or a later accepted lifecycle grammar is invalid. Invalid input stops before operation resolution.                                                                                                                                                                                                                         |
| `blocked`     | An unsafe, ambiguous, unauthorized, colliding, malformed, or unrecognized identity or boundary prevents one complete safe plan. No write begins.                                                                                                                                                                                         |
| `failed`      | An unexpected application, verification, or recovery failure prevents normal completion after the operation's event boundary. Residual recovery failure remains failed.                                                                                                                                                                  |
| `interrupted` | The caller interrupts before completion and no residual recovery failure changes the event classification. Needed recovery evidence remains visible.                                                                                                                                                                                     |

Human primary `complete`, `attention`, and `incomplete` results use stdout.
Human primary `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. JSON emits one complete result to stdout for every status, and bounded
diagnostics use stderr. The operation derives human and JSON output from one
typed result and keeps at most one required `Next:` action. Exact wording, fields,
schema, and numeric exits remain open; a complete result has no required next
action, and no operation lists several required next actions.

The normal precedence remains `blocked` > `incomplete` > `attention` >
`complete`, with invalid input resolved before operation work and failed or
interrupted retaining their event meaning. This follows the current shared and
route-write contracts without choosing their deferred implementation details.

A normal install can return `attention` with no new effect when a prior safe
install preserved changed, missing, or retired divergence. `complete` is reserved
for a verified no-op or complete plan with no remaining finite attention
condition.

## Gate 3 deferrals

These are deliberate technical boundaries, not missing product decisions for
this packet:

- exact lifecycle metadata path, schema, versioning, payload identity, digest
  algorithm, digest serialization, encoding, and retired-state retention;
- YAML and Markdown parser behavior, frontmatter and generated-region
  serialization, compatibility parsing, line endings, and byte-preservation
  mechanics beyond the current observable boundaries;
- exact temporary/staging/comparison directory strategy, adjacent `.bak`
  filename and collision mechanics, artifact identity, cleanup, and residual
  discovery;
- filesystem identity and containment realization, symlink/junction and
  hardlink behavior, case and Unicode behavior, atomic replacement, and
  concurrency or lock mechanics beyond expected-state revalidation and
  identity-guarded reverse recovery;
- the exact configured formatter allowlist, argv and process boundary,
  affected-file tracking, digest refresh, and formatter failure behavior under
  CLI-D047;
- JSON schema and compatibility, structured field names, numeric process exits,
  diagnostic fields and redaction, and any .NET or Native AOT source/module
  boundary; and
- package, wrapper, distribution, and migration mechanics outside the accepted
  embedded-payload and exact-workspace product meaning.

Gate 3 may choose these mechanics only after Gate 2 product and command
decisions are complete. It may not use a technical choice to broaden the
recognized footprint, infer ownership, weaken preservation, or change the
accepted one-operation meaning or bounded `--force` authority.

This packet claims no implementation evidence. Its later verification plan names
observable conformance cases only; it does not claim that the new CLI or any
lifecycle operation exists or works.

## Council shared ground and material dissent

The focused reconciliation found shared ground across all three lenses:

- normal installation must preserve user divergence and converge to a verified
  no-op when the requested state is already satisfied;
- `install --force` must make replacement and confirmation authority visible
  without becoming a disguised generic force flag;
- a transparent lifecycle baseline is needed to distinguish safe managed
  change from user divergence, but it must not become runtime Framework state;
- exact workspace, target identity, managed-region boundaries, Extension
  ownership, and route/source collisions must be established before writes;
- Git and adjacent `.bak` serve different recovery roles, while operation
  temporary artifacts are not lifecycle authority;
- malformed, duplicate, reversed, unknown, or ambiguous structures fail closed;
  the repository `.temp/` directory is outside lifecycle cleanup; and
- uninstall, persistent exclusions, implicit adoption, retired-file deletion,
  arbitrary provider discovery, and untrusted legacy migration need separate
  decisions rather than silent defaults.

Material dissent is preserved here rather than flattened into a vote:

1. **The direct command versus two leaves.** The cold product lens considered a
   separate `update` label useful for discoverability. The earlier council
   recommendation then separated `framework install` and `framework replace`
   under a group. The accepted direct `install` command rejects that group
   because one actual operation would make it ceremonial, while retaining
   bounded `--force` authority on the same operation. The two-leaf separation
   is the strongest losing alternative, and the earlier three-leaf comparison
   remains useful historical evidence.
2. **Whole-footprint replacement versus narrower target selection.** The safety
   lens preferred explicit per-target selection to reduce blast radius. The
   product lens preferred one visible replacement request for a recognized
   distribution, with exact target-level planning and diffs. The accepted
   `install --force` uses whole recognized-footprint authority, never arbitrary
   paths, and leaves a narrower selection option open to evidence rather than
   hiding it in an unbounded force flag.
3. **No baseline versus durable baseline.** A simplifier could avoid durable
   lifecycle state by comparing current bytes to the embedded payload. That
   loses the distinction among user changes, missing defaults, new payload, and
   retired payload. The accepted operation retains the smallest transparent
   distribution fact record, while rejecting receipts, sessions, history, saved
   plans, and journals.
4. **`.bak` as restore versus recovery evidence.** A recovery-oriented view
   wanted an explicit restore interpretation for adjacent backups. The product
   and authority boundary keeps `.bak` tied to the current operation and its
   verified rollback needs. Reconciliation therefore does not create a
   Framework restore leaf.
5. **Automatic retirement cleanup versus preservation.** A cleanup-oriented
   view could remove old payload files once the current distribution no longer
   contains them. The user-content and recovery lens found no safe proof that a
   retired file is disposable. Reconciliation preserves and reports it until a
   separate destructive contract exists.

## Consequences of the accepted disposition

- One shallow root command covers first installation and ordinary managed
  update or reinstallation. A group would add ceremony without adding an
  operation.
- `--force` makes replacement and confirmation authority explicit while
  remaining one bounded dimension of `install`. Repeating `--force`,
  `--dry-run`, or `--skip-git-check` is idempotent. Dry-run shares the complete
  plan and writes nothing; skip-Git bypasses only the affected-path cleanliness
  check.
- Normal preservation, baseline comparison, generated Index projection,
  supported root/provider blocks, Git and `.bak` recovery, temporary-state
  boundaries, verification, recovery, statuses, streams, and Gate 3 deferrals
  remain the accepted contract meaning. The direct command changes taxonomy,
  not those safety boundaries.
- The former `framework install` and `framework replace` pair remains evidence
  for the strongest losing alternative. No separate replace leaf or contract
  will be authored for this decision.

## Disposition checklist and exact boundaries

Queue 28 is accepted. This checklist records the accepted meaning and the
remaining boundaries without turning the contextual packet into a second
authority source.

1. **Taxonomy — accepted.** Use the direct root form
   `open-forge install [--force] [--dry-run] [--skip-git-check] [global flags]`.
   There is no `framework` group, root `init`, or separate update, reinstall,
   replace, restore, recover, uninstall, or exclusion leaf. The former
   two-leaf separation is the strongest losing alternative, not an authoring
   plan.

2. **Lifecycle state — accepted.** Use a transparent baseline containing only
   durable payload identity and managed-target or managed-region facts needed
   for safe comparison and preservation. It is not Context state, a receipt,
   session, saved plan, operation history, or transaction journal.

3. **Normal install authority — accepted.** Add absent safe current payload,
   change only baseline-proven unchanged managed content, preserve
   changed/missing/user-added/overwrite/retired content, and report preserved
   finite divergence as `attention`. Do not restore missing defaults or delete
   retired files during normal `install`.

4. **Force authority — accepted.** `install --force` is the replacement and
   confirmation authority for the same operation in human and JSON
   noninteractive use. It covers the exact current canonical payload
   destinations and valid `AGENTS.md` or Claude `CLAUDE.md` managed blocks in
   one exact workspace. It does not bypass Git policy, arbitrary deletion,
   adoption or ownership, collisions, markers, containment, verification, or
   recovery.

5. **Generated and host boundaries — accepted.** Project affected generated
   `Entries` regions from the hypothetical post-install authored topology and
   metadata through the current Index contracts, including preserved user
   routes and absent defaults. Recognize only the supported `AGENTS.md` block
   and Claude `CLAUDE.md` bridge, preserve outside bytes, and block malformed,
   duplicate, or reversed markers.

6. **Recovery — accepted.** Git is review and default recovery when available;
   adjacent `.bak` is target-associated recovery for Gitless or skipped-Git
   replacement; operation temporary artifacts remain internal; and no
   persistent journal or saved plan exists. Keep backup, temp, verification, and
   recovery mechanics within the current contract's boundaries.

7. **Future dispositions — deferred.** Defer uninstall, persistent
   exclusions, automatic retired-file deletion, arbitrary provider discovery,
   untrusted legacy migration, and exact cleanup shape. Reject implicit
   adoption. Packet 2 and Packet 3 remain separate review units.

8. **Results and release boundary — accepted and bounded.** Reuse seven
   statuses, current human streams and one-result JSON, dry-run/apply parity,
   no attention from planned changes alone, normal preservation attention, and
   at most one required `Next:`. Schema, numeric exits, packaging, and other
   implementation details remain Gate 3 deferrals. Packet 3 owns release
   slicing; this disposition does not close Gate 2 or decide D005.

## Post-disposition authoring and verification plan

Queue 28 is accepted. The following work remains bounded follow-up; it is not
performed by this contextual packet:

1. Keep the Queue 28 history contextual while the accepted meaning remains in
   the [Decision Agenda](../../../working/cli-release/decision-agenda.md) and the current Install
   contracts. Packets 2 and 3 remain separate review units, and Gate 2 remains
   open until their required work is accepted and validated.
2. Maintain one split Interface and Behavior pair for the direct root `install`
   operation at [Install Interface](../../../crystallized/documents/cli/contracts/install/interface.md) and
   [Install Behavior](../../../crystallized/documents/cli/contracts/install/behavior.md). Do not create a
   `framework` group or a separate replace contract. Link shared flags, source
   identity, Index, Framework meaning, maintenance contracts, Status, and
   Doctor instead of copying their detailed authority.
3. Keep the Interface Contract responsible for caller-visible syntax, exact
   selection, target and region identity, output, statuses, errors, non-goals,
   and examples. Keep Behavior responsible for technology-neutral facts,
   complete planning, preservation, revalidation, effects, verification,
   recovery, and conformance. Do not add a Technical Design before Gate 3.
4. Verify the complete journey matrix: fresh absence, occupied collisions,
   untracked manual Framework, trusted no-op, unchanged managed targets,
   changed targets, missing or intentionally removed defaults, genuinely new
   payload, user additions, overwrite companions, retired targets, Extension
   collisions, valid and malformed root/provider markers, dirty Git paths,
   force replacement at exact current destinations, Gitless and skipped-Git
   `.bak` recovery, operation interruption, residual recovery, and fresh-plan
   rerun convergence. Include generated `Entries`
   effects whose expected bodies are projected from the hypothetical
   post-operation authored topology and metadata, including preserved user-added
   routes and removed defaults, without embedded generated-byte copying or a
   hidden `index` subprocess.
5. Verify dry-run and apply use one request, fact set, plan, preflight, status,
   exact bounded diff, and result. Verify that planned changes alone remain
   `complete`, normal preserved divergence is `attention`, unavailable safe
   coverage is `incomplete`, unsafe or ambiguous boundaries are `blocked`, and
   failed or interrupted event meaning is preserved. Verify that a fully applied
   and verified normal install returning `attention` refreshes only verified
   non-divergent and newly added target facts, retains changed/missing/retired
   divergence, and that a later removal of a newly added target is recognized as
   missing rather than repeatedly re-added. Verify that a repeated normal
   install with that divergence returns `attention` with verified no-effect
   facts, while dry-run, `incomplete`, `blocked`, `failed`, and `interrupted`
   results leave logical baseline state unchanged. Verify that `--force` does
   not bypass Git, identity, ownership, collision, marker, containment,
   verification, or recovery boundaries and that `--force --dry-run` writes
   nothing.
6. Verify human stream assignment, one complete JSON result for every status,
   bounded stderr diagnostics, and the at-most-one-`Next:` rule from one typed
   result. Verify no numeric exit or schema decision is smuggled into the
   product contract.
7. Verify transparent baseline state is inspectable but excluded from runtime
   Framework context, Context sessions or receipts, Status's stateless initial
   comparison, and Doctor's mutation boundary. Verify lifecycle commands never
   scan or delete repository `.temp/`, arbitrary provider files, unknown
   adjacent artifacts, or user-owned content.
8. Validate local links, source locality, generated route boundaries, canonical
   and dogfood managed-block alignment, and the complete installed payload
   after the later contract work. Do not regenerate or alter unrelated review
   navigation as part of this packet.

This sequence preserves the current Framework architecture and the accepted
shared operation shape while leaving Gate 3 mechanics and Packet 3 release
slicing to their proper decisions.
