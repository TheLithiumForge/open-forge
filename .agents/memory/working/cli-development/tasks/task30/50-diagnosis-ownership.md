---
open-forge:
  description: Task 30 phase 5-D slice 50 naming which command owns each blocking diagnostic question and testing the index, doctor and repair handoff
  tags: [Memory, Working, CLI, Task, Subtask, Diagnosis, Contextual, Active]
---

# 50 — Command ownership of diagnosis

## Outcome

One table naming, for every diagnostic question the CLI can answer, which
command owns it. The accepted division:

- **`index`** — uniquely derivable safe structural fixes, where there is exactly
  one correct result and applying it needs no judgement.
- **`doctor`** — remaining workspace state. It reports; it does not repair.
- **`repair`** — only what it can truthfully complete. If it cannot finish the
  requested recovery, it must not report success.

This slice produces the table and the tests that hold the handoff to it. It
changes no output.

## Depends on

Nothing. It gates [51](51-truthful-findings.md), because the table decides which
findings must meet the quality bar in which command.

## Actionable boundary

- Enumerate every currently blocking diagnosis from the finding codes that exist
  today, not from the analyses. `*Definitions.cs` carries the wire codes and
  statuses; the G4 catalogues under `task30-g4/` carry the situations.
- For each, record the owning command and, where more than one command can
  surface it, which one owns the **fix** and which merely **reports**.
- The same underlying condition may legitimately appear in several commands.
  That is not a defect — `library-inspect.link-missing` and
  `library-list.link-missing` both report one condition. Ownership is about who
  acts, not who mentions it.
- Test the handoff from **one shared seeded workspace per scenario**, so that
  `doctor` reporting a condition, `index` fixing the derivable part, and
  `repair` completing the rest are proven against the same starting state rather
  than three independently seeded ones.
- Do not move a diagnosis between commands in this slice. If the table shows a
  condition is owned by the wrong command, **record it and stop**; moving it is
  a behaviour change and belongs to a later slice with its own decision.

## Acceptance

- A table in this file covering every blocking diagnosis, with its owning
  command and its report-versus-fix role.
- Handoff tests from shared seeded workspaces for at least the conditions where
  ownership is split between commands.
- Every condition whose ownership looks wrong is recorded as an open question,
  not silently relocated.
- All four gates green; no output change, so no capture may move.

## Changes ledger

- doc: this slice had no ownership inventory -> it now records the 101 current
  Doctor wire codes, the current Status vocabulary, command reporters, and the
  command that may fix each condition.
- test: no shared Doctor/Index/Repair handoff test -> added
  `DiagnosisOwnershipIntegrationTests.SharedWorkspaceHandoffUsesReportAndFixOwners`.
- capture: no capture was regenerated -> output baselines remain unchanged
  because this slice changes no strings, statuses, or result shapes.

### Inventory and reading rule

The inventory below was taken from the current wire-code switches in
`src/cli/core/OpenForge.Cli.Core/Commands/**/*Definitions.cs` and the Status
finding vocabulary, then checked against the situation and findings tables in
G4 catalogues 10 through 37. It is deliberately not derived from Emerging
Analysis.

Here, a **diagnostic question** is a workspace or recovery condition that a
user can act on, including an informational `safe-exact` or ownership
observation when it creates a command handoff. Parser input errors,
confirmation/selection prompts, workspace selection failures, locks, writes,
verification, cancellation, and unexpected operation failures are command
execution boundaries rather than cross-command diagnoses. They remain owned by
the command that raised them; the shared G4 family table and each command's
current `Definitions.cs` still govern their status and wording.

`doctor` is the reporting owner for the matrix. It never applies one of these
fixes. A command named in the Fix owner column owns the action only when the
G4 catalogue names that action and the operation can establish its safety.
`hand` means that no command currently has authority to choose or apply the
fix. A command listed in Reports is a view of the same condition, not a second
owner.

### Ownership matrix

The rows are one-for-one with the 101 current `DoctorFindingKind` wire codes.
The command-prefixed mirrors called out in the Reporters column are included
only where the current catalogues give them the same underlying meaning. The
severity and resolution lane are the G4 values; command-local semantic status
is intentionally not inferred from severity.

| Diagnosis code | Severity / lane | Reporters | Fix owner | Report-versus-fix role |
| --- | --- | --- | --- | --- |
| `workspace.unavailable` | error / blocked-repair | doctor; command workspace guards | hand | doctor reports; no command can repair an unreadable workspace |
| `workspace.not-directory` | error / blocked-repair | doctor; command workspace guards | hand | doctor reports; no command can repair the selected file |
| `workspace.agents-missing` | info / informational | doctor | install | doctor reports; install creates the absent installation |
| `workspace.agents-inaccessible` | error / blocked-repair | doctor | hand | doctor reports; access must be corrected outside the CLI |
| `workspace.loader-missing` | error / blocked-repair | doctor; status/context startup checks | update when an ownership record exists, otherwise install | doctor reports; the named lifecycle command restores the Loader |
| `workspace.loader-unreadable` | error / blocked-repair | doctor; route list/inspect | hand | doctor reports; unreadable bytes need an external fix |
| `workspace.loader-malformed` | error / blocked-repair | doctor; route list/inspect | update | doctor reports; update restores the shipped Loader |
| `workspace.entry-missing` | warning / manual-decision | doctor; route list/inspect | route init | doctor reports; route init creates the missing entrypoint |
| `workspace.entry-ambiguous` | error / blocked-repair | doctor; route list/inspect | hand | doctor reports; choosing which entrypoint survives is a human decision |
| `workspace.entry-compatibility-collision` | error / blocked-repair | doctor; route list/inspect | hand | doctor reports; choosing the compatible name is a human decision |
| `workspace.source-id-collision` | warning / manual-decision | doctor; route list/inspect; context/find/references selectors | hand | all views report; a rename or exact-path choice is manual |
| `workspace.path-invalid` | error / blocked-repair | doctor; route list/inspect | hand | doctor reports; the authored path must be corrected |
| `workspace.path-containment` | error / blocked-repair | doctor; route list/inspect; context/references selectors | hand | doctor reports; an out-of-workspace path has no safe automatic owner |
| `workspace.physical-alias` | error / blocked-repair | doctor; references and source selectors | hand | doctor reports; resolving the identity collision is manual |
| `workspace.frontmatter-malformed` | warning / manual-decision | doctor; route list/inspect; context/find | hand | readers report; authored metadata needs editing |
| `workspace.frontmatter-duplicate` | warning / manual-decision | doctor; route list/inspect; context/find | hand | readers report; duplicate authored keys need editing |
| `workspace.parse-incomplete` | info / informational | doctor; route list/inspect; index/context/find/references | hand | readers report; the unreadable or malformed source needs correction |
| `workspace.unsupported-source` | info / informational | doctor; route list/inspect | hand | doctor reports; no command converts an unsupported source |
| `workspace.root-missing` | warning / manual-decision | doctor; route list/inspect | route init or hand | doctor reports; route init may recreate it, otherwise remove the stale entry |
| `workspace.root-unreachable` | warning / manual-decision | doctor; route list/inspect | hand | doctor reports; the Loader or parent route needs an authored correction |
| `workspace.detached` | info / informational | doctor; route inspect | index when the parent is routed, otherwise hand | doctor reports; Index may expose a uniquely derivable detached source |
| `recovery.bundle-recognized` | info / informational | doctor; status; creating command recovery result | cleanup | doctor reports; Cleanup owns removal after its safety checks |
| `recovery.draft-recognized` | warning / informational | doctor; status | cleanup | doctor reports; Cleanup owns the dry-run/apply recovery cleanup |
| `recovery.bundle-collision` | error / blocked-repair | doctor; cleanup | hand / cleanup only after a verified catalogue | doctor reports; Cleanup can act only when its deletion contract accepts the bundle |
| `recovery.provenance-unavailable` | error / blocked-repair | doctor; cleanup | hand | doctor reports; absent provenance forbids automatic deletion |
| `route.entrypoint-missing` | warning / manual-decision | doctor; route list/inspect | route init | doctor reports; route init owns deterministic scaffold creation |
| `route.entrypoint-duplicate` | error / blocked-repair | doctor; route list/inspect; route init/update/move/remove guards | hand | command guards report; keeping one entrypoint is manual |
| `route.escape` | error / blocked-repair | doctor; route list/inspect and route mutation guards | hand | doctor reports; an escaping route link needs editing |
| `route.unreachable` | warning / manual-decision | doctor; route list/inspect | index | doctor reports; Index can expose the uniquely derivable parent entry |
| `route.detached` | warning / manual-decision | doctor; route inspect | hand | doctor reports; deciding whether to route the source is manual |
| `route.metadata-required-missing` | warning / manual-decision | doctor; route list/inspect; route update | route update | doctor reports; route update owns the explicit metadata operation |
| `route.title-invalid` | warning / manual-decision | doctor; route list/inspect | hand | the authored heading is a human content decision |
| `route.axioms-invalid` | info / manual-decision | doctor; route inspect | hand | doctor reports; Axioms content has no deterministic writer |
| `route.generated-region-stale` | warning / targeted-operation | doctor; status; index and route mutation preflights | index | doctor/status report; Index owns the bounded generated rewrite |
| `route.generated-region-missing` | warning / targeted-operation | doctor; status; index and route mutation preflights | index | doctor/status report; Index owns creation only where the region boundary is safe |
| `route.generated-region-malformed` | error / blocked-repair | doctor; index and route mutation guards | hand | doctor reports; Index must not guess a malformed boundary |
| `route.generated-region-misplaced` | warning / manual-decision | doctor; route mutation guards | hand | the authored section order needs a human decision |
| `route.generated-region-duplicate` | error / blocked-repair | doctor; index and route mutation guards | hand | doctor reports; duplicate boundaries must be resolved by editing |
| `route.generated-entry-missing` | warning / targeted-operation | doctor; status; index | index | doctor/status report; Index derives the one missing entry |
| `route.generated-entry-extra` | warning / targeted-operation | doctor; status; index | index | doctor/status report; Index derives the one obsolete entry |
| `route.generated-entry-order` | warning / targeted-operation | doctor; status; index | index | doctor/status report; Index derives canonical order |
| `route.generated-entry-path` | warning / targeted-operation | doctor; status; index | index | doctor/status report; Index derives the canonical path |
| `route.generated-entry-description` | warning / targeted-operation | doctor; status; index | index | doctor/status report; Index derives current metadata |
| `route.generated-entry-tags` | warning / targeted-operation | doctor; status; index | index | doctor/status report; Index derives current tags |
| `route.overwrite-orphan` | warning / manual-decision | doctor; route inspect and route mutation guards | hand | a base-file choice is required before any command can act |
| `route.overwrite-independent-index` | warning / targeted-operation | doctor; route list/inspect; index | index | doctor reports; Index owns removal from generated navigation |
| `route.compatibility-conflict` | error / blocked-repair | doctor; route list/inspect and route mutation guards | hand | choosing one route name is manual |
| `reference.target-missing` | warning / guided-choice | doctor; context; references | repair when candidates exist, otherwise hand | doctor/context/references report; Repair acts only after a target choice |
| `reference.fragment-missing` | warning / guided-choice | doctor; context; references | repair when a heading candidate exists, otherwise hand | readers report; Repair acts only after a fragment choice |
| `reference.fragment-unverified` | warning / blocked-repair | doctor; context | hand, then doctor | doctor reports the unreadable source; no command may invent heading facts |
| `reference.destination-malformed` | warning / manual-decision | doctor; references | hand | readers report; the link syntax needs editing |
| `reference.destination-absolute` | warning / manual-decision | doctor; references | hand | readers report; converting an absolute link requires author intent |
| `reference.destination-query` | warning / manual-decision | doctor; references | hand | readers report; query semantics are not inferred |
| `reference.destination-encoding` | error / blocked-repair | doctor; context/references | hand | the unsafe encoding has no automatic repair owner |
| `reference.target-outside-workspace` | error / blocked-repair | doctor; context/references | hand | an escaping target must be authored differently |
| `reference.target-physical-escape` | error / blocked-repair | doctor; references | hand | physical identity is unsafe; no command may follow or rewrite it |
| `reference.target-alias` | error / blocked-repair | doctor; context/references | hand | target identity is ambiguous and needs a human choice |
| `reference.target-unreadable` | warning / blocked-repair | doctor; context/references | hand / external access fix | readers report; no command can safely repair unreadable target bytes |
| `reference.target-unsupported` | info / informational | doctor; context/references | none | doctor/readers report and intentionally take no action |
| `reference.same-target-path` | info / safe-exact | doctor; repair diagnosis | repair | doctor reports; Repair owns the automatic canonical rewrite |
| `reference.same-target-case` | info / safe-exact | doctor; context; repair diagnosis | repair | doctor/context report; Repair owns the automatic canonical rewrite |
| `reference.same-target-encoding` | info / safe-exact | doctor; repair diagnosis | repair | doctor reports; Repair owns the automatic canonical rewrite |
| `reference.same-target-fragment` | info / safe-exact | doctor; repair diagnosis | repair | doctor reports; Repair owns the automatic canonical rewrite |
| `framework.install-absent` | info / informational | doctor; install/status preflight | install | doctor reports; Install owns creating Framework files |
| `framework.ownership-observation` | info / informational | doctor; status; update/install/extension lifecycle readers | none | every reader reports the absence; no command infers ownership |
| `framework.managed-missing` | warning / targeted-operation | doctor; status; update | update | doctor/status report; Update owns Framework restoration |
| `framework.managed-changed` | warning / targeted-operation | doctor; status; update | update | doctor/status report; Update owns Framework refresh |
| `framework.lifecycle-evidence-unavailable` | warning / blocked-repair | doctor; status; update/install guards | hand / doctor | readers report; missing ownership evidence blocks mutation |
| `framework.bridge-boundary` | error / blocked-repair | doctor; update/route-init guards | update | doctor reports; Update owns the shipped bridge rewrite when its boundary is clear |
| `framework.root-region-boundary` | error / blocked-repair | doctor; update/route-init guards | hand | an unclear authored boundary must be edited |
| `framework.ownership-conflict` | warning / manual-decision | doctor; status; update/install guards | hand | readers report; the competing claim needs a human decision |
| `framework.partial-lifecycle` | error / blocked-repair | doctor; status; update/install result | update | doctor reports; Update owns completing a Framework lifecycle |
| `framework.partial-recovery` | error / blocked-repair | doctor; status; update/recovery result | doctor / hand | Doctor diagnoses the residual; no mutation command may guess the intended side |
| `framework.distributed-payload-defect` | error / manual-decision | doctor; install/update payload checks | reinstall CLI | doctor reports; the bundled CLI payload, not the workspace, must be replaced |
| `extension.ownership-observation` | info / informational | doctor; status; extension list/inspect/remove readers | none | readers report the absence; no command infers installed ownership |
| `extension.manifest-missing` | warning / manual-decision | doctor; extension list/inspect | hand | manifest intent is authored and must be restored manually |
| `extension.manifest-malformed` | error / blocked-repair | doctor; extension list/inspect/install/update guards | hand | doctor reports; manifest repair is manual |
| `extension.duplicate-id` | error / blocked-repair | doctor; extension list/inspect/install/update guards | hand | choosing an ID owner is manual |
| `extension.unknown-id` | warning / manual-decision | doctor; status; extension list/inspect | hand | list can establish the current catalogue; ownership naming remains manual |
| `extension.version-invalid` | warning / manual-decision | doctor; extension inspect/list | hand | version content needs an authored correction |
| `extension.managed-missing` | warning / targeted-operation | doctor; status; extension update/list | extension update | doctor/status/list report; Extension Update restores managed files |
| `extension.managed-changed` | warning / targeted-operation | doctor; status; extension update/list | extension update | doctor/status/list report; Extension Update owns the selected extension refresh |
| `extension.dependency-missing` | warning / manual-decision | doctor; extension inspect/list/install | extension install | doctor reports; installing the named dependency is the explicit fix |
| `extension.dependency-cycle` | error / blocked-repair | doctor; extension inspect/install/update guards | hand | cycle removal requires changing authored dependency declarations |
| `extension.dependency-incompatible` | warning / manual-decision | doctor; extension inspect/list/install/update | hand | selecting a compatible version or dependency is manual |
| `extension.source-unavailable` | info / informational | doctor; status; extension list/inspect | none | readers report that comparison is incomplete; no safe fix is implied |
| `extension.catalogue-unavailable` | error / blocked-repair | doctor; extension list/inspect/install/update | hand / external access fix | doctor reports; the package catalogue cannot be repaired from workspace state |
| `extension.partial-lifecycle` | error / blocked-repair | doctor; status; extension install/update/remove result | extension update | doctor reports; Extension Update owns completing the selected lifecycle |
| `extension.ownership-collision` | warning / manual-decision | doctor; status; extension list/inspect | hand | competing ownership must be resolved by the maintainer |
| `extension.bridge-registration` | warning / manual-decision | doctor; extension list/inspect; index | index | doctor reports; Index owns the uniquely derivable parent entry |
| `library.ownership-observation` | info / informational | doctor; status; library list/inspect/sync readers | none | readers report the absence; no command infers a Library record |
| `library.source-root-invalid` | error / blocked-repair | doctor; status; library list/inspect/attach/sync | hand | Inspect establishes the fact; correcting the recorded root is manual |
| `library.source-root-aliased` | error / blocked-repair | doctor; status; library list/inspect | hand | ambiguous physical identity forbids automatic Library action |
| `library.inventory-incomplete` | warning / informational | doctor; status; library inspect/list/sync | hand | Inspect owns the deeper read; no mutation is authorized from an incomplete scan |
| `library.projection-missing` | warning / manual-decision | doctor; status; `library-inspect.link-missing`; `library-list.link-missing` | library sync | all three readers report the same missing registered link; Sync owns restoration |
| `library.projection-dangling` | error / blocked-repair | doctor; status; library inspect/list | hand | Inspect reports the missing target; it does not choose a replacement |
| `library.projection-retargeted` | error / blocked-repair | doctor; status; `library-inspect.link-changed`; `library-list.link-changed` | hand | readers report one changed projection; no command may silently retarget it |
| `library.path-collision` | warning / manual-decision | doctor; status; library list/inspect/attach/sync | hand | competing destinations need an explicit owner decision |
| `library.link-capability-unsupported` | error / blocked-repair | doctor; library attach/sync/detach | hand / platform fix | the platform capability must be supplied outside the command |
| `library.extension-collision` | warning / manual-decision | doctor; status; library list/inspect | hand | the Library/Extension ownership choice is manual |
| `library.recovery-safe-exact` | info / safe-exact | doctor; status; repair diagnosis | repair | Doctor reports; Repair owns the verified automatic recovery step |

### Command-local mirrors and operation boundaries

The matrix deliberately names the canonical condition once. The following
current command-local codes are mirrors or operation boundaries, not new
conditions. Their owner is shown so a future finding cannot be mistaken for a
second diagnosis owner.

| Current code family from `Definitions.cs` | Reports / acts | Ownership decision |
| --- | --- | --- |
| `status.invalid-input`, `status.workspace-unavailable`, `status.workspace-not-directory`, `status.workspace-unsafe`, `status.embedded-framework-unavailable`, `status.operation-failed`, `status.interrupted` | status reports its own input, workspace, bundled-content, or operation boundary | Status owns only the current invocation result; Doctor owns any underlying workspace diagnosis, and no repair owner is inferred for input or failure |
| `status.entry-unavailable`, `status.context-inventory-incomplete`, `status.startup-context-unavailable`, `status.continuity-context-unavailable`, `status.root-categories-unavailable` | status reports; doctor investigates | Status is a report; Doctor owns the workspace diagnosis and any manual correction |
| `status.generated-navigation-changed`, `status.generated-navigation-missing` | status reports; index acts | Index fixes the generated region; Status only reports |
| `status.generated-navigation-unavailable`, `status.generated-navigation-blocked` | status and doctor report | Doctor owns diagnosis; hand fix when the boundary is unsafe |
| `status.framework-lifecycle-untrusted`, `status.framework-lifecycle-incomplete`, `status.framework-lifecycle-blocked` | status reports; doctor investigates | Doctor owns the lifecycle diagnosis; Update may act only after trusted ownership facts |
| `status.framework-ownership-observation`, `status.extension-ownership-observation`, `status.library-ownership-observation` | status reports an absent ownership record | The relevant command may report the observation; no command infers ownership or a fix |
| `status.framework-target-changed`, `status.framework-target-missing` | status reports; update acts | Update owns Framework restoration |
| `status.framework-target-unavailable`, `status.framework-target-blocked` | status and doctor report | Doctor owns the incomplete or unsafe diagnosis; no automatic restoration is inferred |
| `status.extension-lifecycle-untrusted`, `status.extension-lifecycle-incomplete`, `status.extension-lifecycle-blocked` | status reports; doctor investigates | Doctor owns the lifecycle diagnosis; Extension Update may act only after trusted facts |
| `status.extension-target-changed`, `status.extension-target-missing` | status reports; extension update acts | Extension Update owns selected package restoration |
| `status.extension-source-unavailable`, `status.extension-target-unavailable`, `status.extension-target-blocked` | status and doctor report | Doctor owns the incomplete or unsafe diagnosis; Extension Inspect is read-only |
| `status.library-projection-missing`, `status.library-projection-changed` | status reports; library sync/inspect act or report | Sync owns missing-link restoration; Inspect reports changed identity |
| `status.library-record-malformed`, `status.library-record-unavailable`, `status.library-source-root-invalid`, `status.library-source-root-aliased`, `status.library-source-root-unavailable` | status reports; doctor and library inspect investigate | Doctor owns the diagnosis; malformed, ambiguous, or unavailable ownership facts are not auto-fixed |
| `status.library-projection-unavailable`, `status.library-projection-blocked`, `status.library-extension-collision` | status and doctor report | Doctor owns the diagnosis; hand or external correction is required |
| `status.recovery-candidate-verified`, `status.recovery-draft-incomplete`, `status.recovery-final-malformed`, `status.recovery-final-unsupported`, `status.recovery-final-unavailable`, `status.recovery-catalogue-unavailable` | status reports; cleanup or doctor follows the G4 action | Cleanup owns only an admissible deletion; Doctor owns residual diagnosis |
| `index.topology-ambiguous`, `index.target-unexposed`, `index.target-unsafe`, `index.metadata-unsafe`, `index.generated-region-unsafe` | index reports its inability to apply | Doctor reports the workspace condition; hand editing resolves it |
| `index.discovery-incomplete`, `index.metadata-incomplete`, `index.projection-incomplete` | index reports; doctor reports remaining state | Doctor owns the incomplete diagnosis; Index does not claim a successful rewrite |
| `context.target-missing`, `context.fragment-missing`, `context.target-ambiguous`, `context.target-unsafe`, `context.target-case-mismatch` | context reports; repair may act only on the safe-exact case | Repair owns an automatic canonical rewrite; missing/ambiguous targets remain guided or manual |
| `references.target-missing`, `references.fragment-missing`, `references.target-ambiguous`, `references.target-unsafe`, `references.target-unreadable` | references reports row state; doctor reports the aggregate | Repair owns only a selected, verified replacement; unsafe/unreadable targets are manual |
| `repair.diagnosis-blocked`, `repair.diagnosis-incomplete`, `repair.proposal-unavailable`, `repair.facts-conflicting` | repair reports its incomplete or conflicting evidence | Doctor owns re-diagnosis; Repair must not convert these into success |
| `repair.guided-finding-remaining`, `repair.manual-finding-remaining` | repair reports residual work | Repair owns the action only with explicit selection/authority; otherwise it reports the remaining problem |
| `install.*`, `update.*`, and `route-init.*` local content/lifecycle findings | the invoking mutation command reports | the invoking command owns its accepted mutation; Doctor remains the follow-up reporter for residual state |
| `extension-install.*`, `extension-update.*`, `extension-remove.*` local content/lifecycle findings | the invoking Extension command reports | the invoking command owns its selected lifecycle; Doctor remains the follow-up reporter |
| `library-attach.*`, `library-sync.*`, `library-detach.*` mapping, permission, and projection findings | the invoking Library command reports | Attach/Sync/Detach own their own record/link effects; Inspect/List report |
| `route-create.*`, `route-update.*`, `route-move.*`, `route-remove.*` target, metadata, projection, and reference findings | the invoking Route command reports | the invoking command owns its requested route mutation; Doctor reports any residual workspace condition |
| `route-list.*`, `route-inspect.*`, `find.*`, and `references.*` selector/inspection findings | the read-only command reports | no fix is inferred from a read-only selector result; use the matrix owner |

The shared execution families are intentionally not assigned a diagnosis
owner: `invalid-input`, `confirmation-required`, `selection-required`,
`interaction-ended`, workspace selection guards, locks, target races,
permission failures, recovery/write/verification failures, `operation-failed`,
and `interrupted` stay with the command that emitted the current code. Their
G4 next actions may point to Doctor or Cleanup, but that continuation does not
transfer ownership of the failed operation.

### Handoff evidence

`DiagnosisOwnershipIntegrationTests.SharedWorkspaceHandoffUsesReportAndFixOwners`
seeds one workspace containing both a stale generated Entries list and a
canonicalizable `./guide.md` link. It runs the following sequence against the
same path:

| Stage | Evidence | Owner demonstrated |
| --- | --- | --- |
| `doctor --format json --detail full` | reports `route.generated-region-stale` and `reference.same-target-path` without writes | Doctor reports both conditions |
| `index .agents/docs/_docs.md` | rewrites only the deterministic Entries projection; a second Doctor pass no longer reports the generated-region condition | Index fixes the structural condition; Doctor only reports |
| `repair --automatic --format json` | rewrites `./guide.md` to `guide.md`, with no remaining Repair work | Repair fixes the safe-exact link |
| final `doctor --format json --detail full` | neither condition remains | the handoff converges from one seeded state |

No capture harness is used by this test, and no output baseline is regenerated.
The assertions inspect existing JSON fields and resulting workspace bytes only.

## Divergences observed

- Resolved on 2026-09-17, one outcome per code rather than one ruling for all
  three. `update.managed-divergence` is already absent from the code, so that
  half of this note was stale. `extension-remove.managed-divergence` was
  declared but never raised, and the contract's own row said
  `removed ... delete if unreachable`, so it is deleted; remove has no content
  comparison to raise it with, and the state it would have reported is already
  carried by `doctor`, `status` and `extension list`.
  `extension-update.managed-divergence` stays, because unlike the other two it
  is reachable: it reports a retired file kept without `--prune`. The contract
  conflicts with itself there and says so at
  `contracts/extension/update/interface.md:526`, so that one remains an open
  maintainer decision about wording, not about reachability.
- The G4 catalogues use different wire-code families for the same Library
  projection fact (`library.projection-missing`, `status.library-projection-missing`,
  `library-inspect.link-missing`, and `library-list.link-missing`). The table
  treats these as one condition and keeps Library Sync as the fix owner; no
  code or message was moved.
- `framework.partial-recovery` names a residual state whose next action is
  Doctor, while the recovery-producing mutation command still owns the
  original effect. This slice records that split and does not invent a
  recovery action.
- Some command-local G4 rows (especially selector, permission, lifecycle, and
  target-boundary findings) are operation-specific projections of the shared
  families. They remain with their invoking command; the table does not
  silently relocate them to Doctor.
