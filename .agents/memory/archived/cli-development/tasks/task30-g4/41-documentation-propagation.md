---
open-forge:
  description: Propagate the accepted changes recorded in every subtask's changes ledger into contracts, public documentation, Directives and memory records
  tags: [Memory, CLI, Task, Plan, G4, Documentation, Contextual, Archived, Historical]
---

# 41 — Documentation propagation

> Read [00 — G4 conventions](00-conventions.md) first. This task is meant for
> a lower-cost model working from the ledgers. It changes no source.

## Goal

Every contract, public document, Directive sentence and memory record that
describes CLI output, flags, statuses, JSON, prompts or the presentation
layout says what the merged code does, using the words the subtasks chose.
Historical analysis records are not rewritten.

## Depends on / Blocks

- Depends on: 40.
- Blocks: Task 30 G4 closeout.

## Inputs

Read, in this order, and collect every bullet from each **Changes ledger**
and every **Divergences observed** entry marked as needing a durable record:

1. [02 naming](02-naming.md), [03 rendering system](03-rendering-system.md),
   [04 interaction system](04-interaction-system.md), [05 index region](05-index-entries-region.md).
2. Every command subtask 10 to 37.
3. [40 verification](40-verification.md).

## Surfaces to update

| Surface                                                                                                                                                                     | What changes                                                                                                                                                           |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.agents/memory/crystallized/documents/cli/contracts/shared/global-flags/interface.md` and `behavior.md`                                                                    | `--detail`, `--detail-filter`, `--format`; remove `--view`, `--json`, `--verbose`; colour paragraph unchanged                                                          |
| `.../contracts/shared/result-coordinates/interface.md` and `behavior.md`                                                                                                    | schema 3 envelope, renamed statuses, exits unchanged, the severity vocabulary, plain scalars                                                                           |
| `.../cli/shared-operation-contract.md`                                                                                                                                      | the `Next:` paragraph, the stream paragraph, the Guided Leaves section (prompts), status names                                                                         |
| Every command interface under `.../contracts/<command>/interface.md`: Human Output (or Output), Structured Output, Compact JSON Output, Semantic Results, Errors, Scenarios | replace with the catalogue's rules and one representative transcript per status; delete the compact-JSON section; rename statuses; renamed or removed finding codes    |
| Every command behavior contract                                                                                                                                             | only where a finding kind was removed or a status changed (route init, extension install no-content, doctor coverage kinds)                                            |
| `.../cli/layers/presentation.md`, `shell.md`, `architecture.md`                                                                                                             | the report model, the selection stage name, the `Presentation/` folder, the dependency rules, the future project split                                                 |
| `.agents/directives/open-forge/cli/implementation.md`                                                                                                                       | the "Human Views And Interaction" paragraph: levels, `debug`, no `--verbose`, prompts                                                                                  |
| `.agents/guidance/cli-design.md`                                                                                                                                            | only if a rule there contradicts the accepted decisions (report, do not silently rewrite)                                                                              |
| `docs/cli.md`                                                                                                                                                               | global options table, status table names, every example output, the stale lifecycle-file line, the `update` preservation sentence, `extension remove --prune`, prompts |
| `README.md`                                                                                                                                                                 | any output sample                                                                                                                                                      |
| `.agents/memory/crystallized/decisions/framework/workspace-state-files.md`                                                                                                  | the previous-content pointer gap (C15)                                                                                                                                 |
| `.agents/memory/working/cli-development/tasks/task30-cli-experience-remediation.md`, `plan.md`, `project-control.md`                                                        | phase state, completion evidence, the G4 packet as complete                                                                                                            |
| `.agents/memory/working/cli-development/tasks/task31/phase-escaper.md` and the Task 31 record                                                                               | M3 complete                                                                                                                                                            |
| `.agents/memory/working/cli-development/tasks/task30/phase-4b-g4.md`                                                                                                        | closeout                                                                                                                                                               |

## Rules

- Change meaning only where a ledger entry says the code changed. Do not
  invent behavior. When a ledger entry and a contract disagree in a way the
  ledger does not explain, stop and record it under Divergences.
- Keep the writing standard: short sentences, no internal vocabulary in
  user-facing examples, the accepted names from 02.
- Every transcript in a contract must be one the snapshots contain, copied
  from the snapshot file with the workspace placeholder.
- Do not touch `.agents/memory/emerging/` or `.agents/memory/archived/`.
- Run Prettier over the changed Markdown. Check every link you touched.
- Do not run `index` over this repository unless 05 is merged and its
  acceptance is recorded.

## Steps

1. [ ] Build the consolidated change list from the ledgers, one file.
2. [ ] Update the shared contracts.
3. [ ] Update each command contract from its subtask.
4. [ ] Update layers, Directive, `docs/cli.md`, README, the decision record.
5. [ ] Update the Task 30, Task 31, plan and control records to closeout state.
6. [ ] Prettier and link check.

## Acceptance

- [ ] No contract, document or Directive names `--view`, `--json`, `--verbose`, `compact`, `expanded`, `schemaVersion: 1` or `2`, or a removed finding kind.
- [ ] Every ledger bullet maps to a changed line or to a recorded reason not to change.

## Changes ledger

- `.agents/guidance/cli-design.md`: Compact/Expanded views, generic verbose
  advice, and a separate AI view -> one native report for people and machines,
  with `--format`, four `--detail` levels, repeatable `--detail-filter`, and
  plan review before confirmation.
- `.agents/memory/crystallized/documents/cli/shared-operation-contract.md`:
  wizard language, retired global flags, old statuses, and apply-time grant
  persistence -> prompt-capable leaves, the schema-3 report and seven current
  statuses, confirmed-application grant timing, the `Next:` exception, and the
  three Library confirmation-required codes.
- `.agents/memory/crystallized/documents/cli/technical-designs/workspace-permissions.md`:
  explicit grants saved before the final prompt -> explicit and Allow-always
  grants staged in the permission plan and published only during confirmed
  application.
- `.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md`:
  stale interactive-session and permission timing language, plus a Linux AOT
  claim -> neutral interaction composition, confirmed-application timing, and
  the Windows `vswhere`-on-`PATH` host-only AOT gate.
- `.agents/memory/crystallized/documents/cli/architecture.md`: AOT capability
  statement without the host limitation -> the supported Windows gate,
  `vswhere` requirement, and unsandboxed-host ownership.
- `docs/extensions.md`: retired lifecycle filename and preservation wording ->
  `.agents/open-forge.lock.json`, current Update replacement/restoration rules,
  `--prune`, and confirmed-application permission persistence.
- `.agents/memory/working/cli-development/tasks/task30-g4/00-conventions.md`:
  unconditional final-line and old stream vocabulary -> the qualified
  `Next:` exception and current report statuses/streams.
- `.agents/memory/working/cli-development/tasks/task30/00-conventions.md`:
  serial test commands and an unqualified AOT note -> collection-parallel
  managed commands and the unsandboxed `vswhere` host gate.
- `.agents/memory/working/cli-development/tasks/task30-g4/_task30-g4.md`:
  pre-closeout packet state and missing host qualification -> merged/green G4,
  `--parallel collections`, the overseer AOT gate, and current flags/statuses.
- `.agents/memory/working/cli-development/tasks/task30-cli-experience-remediation.md`:
  G4 and Task 31 pending -> G4 and M3 closeout, confirmed plan/grant behavior,
  six collection-parallel suites, and the host-only AOT gate.
- `.agents/memory/working/cli-development/plan.md`: G4 and M3 queued -> G4
  complete, M3 complete, collection-parallel qualification, and the current
  AOT host boundary.
- `.agents/memory/working/cli-development/project-control.md`: 01/G4 and M3
  still next -> G4 and M3 complete with Phase 5-D and M5 as the next boundaries.
- `.agents/memory/working/cli-development/tasks/task30/phase-4b-g4.md`:
  pre-implementation Presentation gate and stale view/verbosity wording -> a
  completed Presentation Model record with current report flags, statuses,
  interaction timing, codes, and open questions.
- `.agents/memory/working/cli-development/tasks/task30-g4/03-rendering-system.md`:
  bridge-era current steps and renderer-axis wording -> completed shared-report
  pipeline language, with the bridge receipt explicitly marked historical.
- `.agents/memory/working/cli-development/tasks/task30-g4/02-naming.md`:
  unmapped Today-column vocabulary -> a note distinguishing historical names
  from the accepted current vocabulary.
- `.agents/memory/working/cli-development/tasks/task30-g4/04-interaction-system.md`:
  pre-qualification interaction divergences -> a closeout note recording the
  merged plan-review, staged-grant, and Library-code behavior without deciding
  the retained questions.
- `.agents/memory/working/cli-development/tasks/task30-g4/01-before-snapshots.md`:
  an unqualified pre-G4 snapshot description -> an explicitly historical
  baseline note pointing to the current report flags.
- `.agents/memory/working/cli-development/tasks/task31-implementation-duplication.md`:
  M3 deferred -> M3 complete in G4; M5 remains after structure stabilization.
- `.agents/memory/working/cli-development/tasks/task31/phase-escaper.md`:
  deferred escaper -> complete shared escaping boundary and reviewed output
  diff delivered with G4.
- `.agents/memory/working/cli-development/tasks/task31/_task31.md`:
  a Planned M3 entry -> a Completed M3 entry.
- `.agents/memory/working/cli-development/tasks/task31/phase-structure.md`:
  structure cleanup planned after G4 -> structure cleanup queued after the
  completed G4 seams.
- `.agents/memory/working/cli-development/tasks/task31/phase-3-5.md`:
  a pre-G4 sequencing statement -> a historical note pointing to completed M3
  and queued M5.
- `.agents/memory/working/cli-development/tasks/task31/08-m1-enumerated-extractions.md`:
  pre-G4 dependency statements -> a superseded historical note while retaining
  the M1 evidence.
- `.agents/memory/working/cli-development/tasks/task30/b1-help-before.md`:
  a pre-G4 published-help capture -> an explicit historical note pointing to
  the schema-3 report and current flags.
- `.agents/memory/working/cli-development/tasks/task31/phase-constants.md`:
  command-local schema versioning -> one shared schema-3 envelope, with the
  older A4 snapshot vocabulary explicitly marked historical.
- `.agents/memory/working/cli-development/tasks/task30/phase-7-8-test-architecture.md`:
  serial delivery baseline -> current six-suite `--parallel collections` state,
  with the old baseline retained under an explicit superseded note.
- `.agents/memory/working/cli-development/tasks/task30/06p-p1-shared-permissions.md`:
  historical G1 separate-authored-edit timing -> a superseded note pointing to
  G4 staged grants and confirmed-application publication.
- `.agents/memory/working/cli-development/tasks/task30/phase-4a-g1.md`:
  G1's persist-before-confirmation statement -> a superseded note pointing to
  G4's confirmed-application timing and current G4 state.
- `.agents/memory/working/cli-development/tasks/task30/04-a4-extension-readers.md`:
  a G1 output plan with pre-G4 terms -> an explicit historical note pointing to
  the current schema-3 report and flags.
- `.agents/memory/working/cli-development/tasks/task30/07-b1-heading-entries.md`:
  serial qualification commands -> an explicit historical receipt note and the
  current six-suite collection-parallel setting.
- `.agents/memory/working/cli-development/tasks/potential/analysis-disposition.md`:
  a 2026-09-12 analysis snapshot with G4/M3 marked open -> an explicit
  superseded-snapshot note pointing to current Task 30/31 records.
- `.agents/memory/working/cli-development/tasks/task30/06-a6-delete-lifecycle.md`:
  a pre-G4 stop boundary -> a superseded note pointing to the completed packet.
- `.agents/memory/working/cli-development/tasks/task30/phase-4a-structural.md`:
  a pre-G4 stop condition -> a current note that G4 is complete while deferred
  Framework decisions remain deferred.
- `.agents/memory/working/cli-development/tasks/task30/p1-permission-help-snapshot.md`:
  a G1 help capture with retired options -> an explicitly historical snapshot
  note pointing to current help vocabulary.
- `.agents/memory/working/cli-development/tasks/task30-g4/41-documentation-propagation.md`:
  empty ledger and divergence sections -> this Lane E closeout ledger and its
  explicit scope, historical-record, and maintainer-question notes.

## Divergences observed

### Scope and placement

- The parent surface table includes command contracts, the presentation and
  shell layer records, the implementation Directive, `docs/cli.md`, README, and
  the workspace-state-files decision record. Lane E does not own those paths:
  contracts and layers belong to other lanes, `docs/cli.md` is explicitly
  excluded, and the Directive, README, and decision record are outside this
  surface. Their ledger bullets could not be placed here without crossing the
  assigned boundary.
- The per-command catalogues under `task30-g4/10` through `37` remain the
  source ledgers and were not edited. Their command-local wording, status,
  finding-row, and transcript questions remain in those catalogues for the
  owners/overseer to resolve; shared facts were recorded in the documents and
  memory records above.
- Sealed proposals and other historical analysis that was true when written
  retain their old examples. Current records were corrected in place or carry
  an explicit superseded note; no historical proposal was rewritten.

### Durable current facts

- `reference.cycle` is dead. The remaining literal is only in unrelated
  Workspace Settings unknown-key tests in catalogue [11](11-doctor.md); this
  record documents that exception without changing the catalogue.
- All six test suites use `--parallel collections`; the former `--parallel none`
  default is gone. The Native AOT gate requires `vswhere` on `PATH` and cannot
  run in a sandboxed worker; the overseer must run it on an unsandboxed host.
- Explicit `--allow-path` grants and interactive Allow-always decisions are
  staged and published only during confirmed application under lease,
  revalidation, and recovery. Cancellation or refusal before application leaves
  settings unchanged. The three Library confirmation-required codes are
  recorded in the shared operation contract and G4 closeout.
- Every confirmation has the established minimal plan review on stderr before
  the prompt. The review does not rerun the operation or acquire authority.
- The repository has no local Prettier executable, and
  `npm exec --offline -- prettier` cannot use the unavailable npm cache entry;
  therefore the formatter could not be run in this sandbox. A scoped relative
  link check covered 423 links with zero missing targets, and all 35 changed
  Markdown files were checked for LF-only line endings.

### Open maintainer questions — not decided here

- **Final `Next:` placement:** Extension Install emits a required advisory after
  its `Next:` line. The maintainer must choose whether to relax the final-line
  invariant or move the advisory above `Next:`.
- **Nullable counts:** the maintainer must choose whether intentional nullable
  count members are omitted from the payload or each receives an explicit
  limitation.
- **Forbidden vocabulary scope:** the maintainer must decide whether the
  human-only vocabulary rule also governs JSON and machine diagnostic channels.
- **Workspace echo examples:** Index and Update emit a required `Workspace:`
  line in cases whose catalogue examples omit it. One ruling should settle
  whether the echo is exempt from the one-line example rule or the examples
  should use two lines.
- **Status and Doctor:** [10](10-status.md) leaves four frozen message choices
  open (lifecycle unavailable, generated-navigation blocked, interrupted, and
  recovery-catalogue unavailable). [11](11-doctor.md) leaves the hint wording,
  duplicate action/reason text, and the `error-and-warnings` fixture/severity
  contradiction open.
- **Index, Route, and Repair:** [14](14-index.md) leaves Workspace echo versus
  the one-line no-op example open. [20](20-route-list.md) leaves unreadable
  metadata status/row behavior and malformed-Loader status open. [21](21-route-inspect.md)
  leaves the authoritative next action and status/stream rows open. [22](22-route-init.md)
  leaves the direct-operation `invalid-metadata` wording open. [25](25-route-move.md)
  leaves self-move and destination-inside-source status, identity-collision
  severity, missing coordinates, next-action wording, ownership-unavailable
  headline, and parser-boundary data shape open. [26](26-route-remove.md)
  leaves the source-not-found success/no-op versus invalid-input behavior open.
  [15](15-repair.md) leaves situation/code naming, proposal-unavailable wording,
  dry-run warning wording, and singular grammar open.
- **References and Extension:** [19](19-references.md) leaves the incomplete
  sentence and `identity-collision` finding-row/code choice open. [27](27-extension-list.md)
  leaves the installed-source-missing row open. [28](28-extension-inspect.md)
  leaves the `ambiguous-source` versus `identity-ambiguous` choice open.
  [29](29-extension-create.md) leaves version/dependency classification,
  `.agents` destination safety, completed-effect wording, and `folder` versus
  `packagePath` meaning open. [30](30-extension-install.md) and
  [31](31-extension-update.md) share the `--all` wording choice; [31] also
  leaves the retired-file finding row/name and blocked-effect suppression open.
- **Libraries:** [34](34-library-inspect.md) leaves root-destination punctuation
  open. [35](35-library-attach.md), [36](36-library-sync.md), and
  [37](37-library-detach.md) leave the shared held-lock status open; [35] also
  leaves the unsupported-link fixture and interruption counts open; [36] leaves
  `registered-link-gone` behavior open; and [37] leaves `record-invalid`,
  `mapping-blocked` detail, the confirmation-required row, and
  `interrupted-after-effects` wording open. No command-local choice was made
  in Lane E.

## Rollback

Revert the documentation commit; code is unaffected.
