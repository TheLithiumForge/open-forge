---
open-forge:
  description: Independent second-gate review of A1 through B1 before G4, with two corrections required and the evidence behind each verdict
  tags: [Memory, CLI, Task, Review, Evidence, Contextual, Archived, Historical]
---

# Second-gate review before G4

Independent review from a different model family, run against artefacts rather
than reports: suites re-run, diffs read, invariants checked in code, three claims
proved live.

**Verdict: the work is sound. Two corrections are required before G4, neither
blocking implementation.**

## Verified

| Check                                          | Result                                                        |
| ---------------------------------------------- | ------------------------------------------------------------- |
| Unit                                           | 3366 total, 0 failed, 0 skipped                               |
| Integration                                    | 1866 total, 0 failed, 17 skipped                              |
| End-to-end                                     | 131 total, 0 failed, 0 skipped                                |
| `Framework.Lifecycle` references in Core       | 0                                                             |
| `Framework.Permissions` references in Core     | 0                                                             |
| Old state-file names in crystallized contracts | only the decision's own Context, which is history and correct |

**The 97-test unit drop is accounted for, not weakening.** Deleted test files map
one-to-one onto deleted subsystems: `Framework/Lifecycle/*`,
`Framework/Permissions/*`, and the Libraries record codec. No surviving test was
narrowed to pass. The A3 idempotency test added by the previous gate survives.

**Both permanently retired M1 extractions were respected.** Route `Safety()`
remains duplicated across the human and JSON vocabularies, and Library
`PlanState()` remains duplicated. These were the clearest test of whether a
recorded "do not" survives a capable agent that could see an easy extraction.
They did.

**Lock invariants hold.** `OwnershipWritePlanState.Skipped` exists and `Blocked`
does not; every consumer maps `Skipped` to `None`, `NotRequested` or `null`, so a
skipped lock never gates a command.

**Proved live, not just asserted:**

- `doctor` on a Framework source checkout reports **0 errors**. The pre-A5
  baseline was `ERROR framework.install-incomplete` with "repair is blocked". The
  missing lock is now informational, which is what the decision requires.
- `index` over 120 regions reports 0 updates on two consecutive runs, on a tree
  Prettier formats and whose `.prettierignore` fences have been removed. That is
  B1's whole purpose and it is real.
- Extension destination policy reserves `open-forge.json`,
  `open-forge.lock.json` and the lease store.

**Scope discipline.** P1 (shared permissions) was outside the eight planned
slices but inside the accepted decision, and it was given its own plan file
rather than done silently. That is the right way to widen scope.

## Correction 1 — stale contract prose on a safety boundary

**34 lines across roughly 14 contract files still make present-tense claims about
a "lifecycle document" that no longer exists.**

The consequential ones are the reserved-path rules, because they are wrong in a
way a reader would act on:

- `extension/install/behavior.md:298` — "Reject package paths targeting the
  lifecycle document"
- `extension/install/interface.md:294`
- `extension/update/behavior.md:180`
- `install/interface.md:143` and `install/behavior.md:134`

The code reserves `open-forge.json`, `open-forge.lock.json` and the lease store.
These contracts name a deleted file and omit both files that are actually
protected. Someone implementing to the contract protects the wrong thing.

Also stale, lower cost: `doctor/interface.md:488` describes
`extension.duplicate-id` in terms of a "lifecycle record", and the "never mutates
a lifecycle document" lines in `extension/create`, `extension/list` and
`extension/inspect`.

**Why it happened, which matters more than the lines themselves.** Each slice's
acceptance named that slice's own contracts. A contract that merely _mentions_ the
changed thing belonged to no slice, so nothing claimed it. Diligence did not fail;
the slicing did. This is the drift described in
[Evergreen drift needs a signal](../../../../emerging/ideas/evergreen-drift-signal.md),
and the durable fix is a check, not a reminder.

**Correction:** sweep `contracts/` for `lifecycle document`, `lifecycle record`
and `lifecycle file`, and replace each with what the code actually does. Do this
before G4, because G4 rewrites presentation against these contracts and would
inherit the error.

## Correction 2 — an accepted consequence was never implemented

[Workspace State Files](../../../../crystallized/decisions/framework/workspace-state-files.md)
says: "The legacy `open-forge.extensions.json` is reported once as information."

No source file references it. `doctor` surfaces the path only incidentally, as a
link target inside an unrelated reference finding, not as the informational report
the decision describes.

This was in the decision and in none of the eight slices, so no slice owned it.
It is small, and it is a stated consequence that is currently untrue.

**Correction:** either implement the one-time informational report, or record an
accepted change to that line of the decision. Do not leave the decision claiming
behaviour that does not exist.

## Not findings

Recorded so they are not re-raised:

- **Nine `generated-index` references remain in `src/cli`.** All legitimate: one
  is `MarkdownEntriesSectionReader`, the migration reader that recognises and
  strips legacy guards, and the rest are its tests, including assertions that the
  marker is absent afterwards.
- **Thirteen documents still contain the guard.** All archived, sealed analysis,
  or the plan files that quote it. No live entrypoint and no shipped payload.
- **`Ownership` members in public JSON documents.** Pre-existing, and a different
  concept: they describe whether a document or region is user-owned or managed,
  not the lock.

## Recommendation

Proceed to G4. Take correction 1 first: G4 rewrites renderers against these
contracts, so a contract that misstates a safety boundary is cheaper to fix now
than after the presentation layer is built on it.

## Accepted Disposition And Follow-ups

The primary owner rechecked this review at `5c90ddbc` against the qualified
implementation at `4eaf78bd`. The maintainer authorized integration with both
corrections retained, and `e40492aa` merged the reviewed tree into
`feature/development-2`. The original review above remains historical evidence.
The following dispositions incorporate the maintainer's subsequent annotations.

- **SG-R1 — fixed.** Current contract prose now names the ownership lock and
  authored settings according to each command's actual effects. Extension
  Install/Update explicitly reserve `.agents/open-forge.json`,
  `.agents/open-forge.lock.json`, `.agents/open-forge.lock`, and descendants,
  matching `ExtensionDestinationPolicy.IsAllowed`. Library Sync's exclusion
  now protects Framework and Extension ownership while allowing Library claims
  in the shared lock. Doctor's duplicate-ID description now names ambiguous
  source/package identity. Framework Install's footprint, skipped ownership
  publication and heading-based Entries prose match the current implementation.
  Explicit old-file rejection statements and historical explanations remain.
- **SG-R2 — retired by maintainer direction.** The maintainer rejected retaining
  a compatibility notice for the unused root receipt. The state-file decision
  now explicitly omits that notice. No one-time reporting state or new legacy
  reader was implemented. Existing unsupported files are not automatically
  deleted by this documentation correction.

### Evidence And Verification

The correction was checked against `ExtensionDestinationPolicy`,
`ExtensionSourceDoctorInspector`, `ExtensionPackageDoctorInspector`,
`LibrarySyncPlanner`, `LibraryMutationPlanningPolicy`, Framework ownership
publication, the ownership reads in Route Move/Remove, and Extension Inspect.
The contract search for `lifecycle document`, `lifecycle record`, and
`lifecycle file` now leaves only explicit statements rejecting old-file input.

`RetiredGuardsMigrateOnce` already proves Index's first-run removal and second-run
byte stability for LF and CRLF, with authored prefix/suffix preservation. B1's
qualification includes that test. No new production, test, fixture, payload,
dependency or build configuration changed in this correction. The recorded
managed/native suite qualification still applies; no fresh suite run or output
snapshot is claimed for this prose-only correction. The changed Markdown was
formatted, its local links checked, and its final diff reviewed.

### Divergences Observed

- The review proposed replacing stale lifecycle-file references. Source inspection
  showed that explicit old-file rejection statements remain correct, so those
  were retained instead of performing a blanket replacement. The same inspection
  exposed adjacent Framework/Extension Install and Extension Update claims about
  old generated boundaries and a lock-write gate; those were reconciled with B1
  and the existing skipped-write contract.
- The earlier decision required a one-time legacy-receipt notice. The maintainer
  explicitly retired that requirement, so the decision was amended instead of
  implementing the missing behavior. This is a superseding decision, not an
  unimplemented accepted requirement.
- The suggested preparation pass would recheck all old output proposals. The
  maintainer questioned duplicating the upcoming design effort. The pass was
  narrowed to obsolete assumptions that could contaminate G4, recorded in
  [G4 preparation](phase-4b-g4.md#preparation-reconciliation). Historical proposals
  were not rewritten, and new transcripts remain part of the design gate.
- The requested Index cleanup already exists in B1. Its code and evidence were
  checked; no duplicate migration was added. The suggested AGENTS heading
  transition is recorded in the [structural follow-up](../../../../working/cli-development/tasks/task30/phase-4a-structural.md#managed-host-heading-direction).
  Its shared CLAUDE boundary and preservation behavior require a focused contract
  before changing managed-host parsing; they were not silently implemented here.
