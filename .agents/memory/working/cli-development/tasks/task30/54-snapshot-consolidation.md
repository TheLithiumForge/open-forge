---
open-forge:
  description: Task 30 phase 5-D slice 54 consolidating the snapshot roots, settling the naming convention and pruning anything no live test owns
  tags: [Memory, Working, CLI, Task, Subtask, Testing, Snapshots, Contextual, Active]
---

# 54 — Snapshot consolidation and pruning

## Outcome

One rule for where a snapshot lives and what it is called, applied everywhere,
with nothing retained that no live test reads.

## Depends on

Nothing. **The location half was applied by the overseer on 2026-09-17** and the
file moves are already done, so this slice no longer relocates anything and can
run beside a lane that regenerates captures.

### Already applied — do not redo

`MatchDetailSnapshot` no longer overrides `RootDirectory`. All 3,300 command
output captures now live in the library's default location, in 28
`__snapshots__` directories beside the test class that owns them; the old flat
`tests/integration/snapshots` root is deleted. Git recorded 3,300 renames and a
content-hash comparison confirmed the trees were byte-identical first.

`CliReportInvariantsTests` was pointed at the new layout — it enumerates every
`__snapshots__` directory under the integration project and throws when it finds
none, so an empty corpus cannot pass silently.

**What is left for this slice** is the convention and the guard, below.

## Current state, measured

Measured on 2026-09-16. **3,433 snapshot files, all with the `.txt` extension.**

| Root | Files |
| --- | --- |
| `tests/integration/snapshots/` | 3,300 |
| `tests/unit/OpenForge.Cli.Core.UnitTests/__snapshots__/` | 23 |
| `tests/unit/.../Commands/Doctor/Shared/Rendering/__snapshots__/` | 110 |

Two facts worth stating plainly, because both are easy to misread:

- **There is no `.json` / `.txt` format split.** Every file is `.txt`. JSON
  *content* lives in files named `<situation>.json.<detail>.txt`, where `.json.`
  is a name segment marking content type, not an extension. 1,705 files carry
  JSON content and 1,728 carry text.
- **There is a location split.** Unit snapshots live in two different roots: a
  top-level `__snapshots__` mirroring the namespace, and one nested beside the
  Doctor rendering tests. Integration snapshots live in a third, flat root.

A sweep on 2026-09-16 found **zero orphaned snapshot directories** — an earlier
sweep removed eight orphaned `DoctorLifecycleOutputSnapshotTests` files, and
nothing unowned remains. So pruning is a guard to keep, not a backlog to clear.

## Actionable boundary

- **Record the location rule now that it is settled**: captures live in the
  library's default `__snapshots__` directory beside the test class that owns
  them, and no helper overrides `RootDirectory`. Write it where a future author
  will meet it, not only here — the override existed for months with no recorded
  reason, which is how it survived.
- Decide whether `.json.` as a name segment stays. It works and it sorts
  usefully; changing it churns 1,705 files for no behavioural gain. **Prefer
  keeping it** and recording the convention explicitly so it stops looking
  accidental.
- **Snapshot contents must not change.** This slice moves and deletes files; it
  never rewrites a captured byte. Do not run with `OPENFORGE_SNAPSHOT_UPDATE=1`.
  If a snapshot is reported missing, a directory was moved wrong.
- Prune only what no live test can produce. A test may build a snapshot name
  from parameters rather than from its method name, so a directory with no
  obviously matching method is not automatically an orphan. **If you cannot
  positively establish that nothing owns it, keep it and list it as
  unresolved.** Deleting a live snapshot converts a real assertion into a
  silently regenerated one, which is far worse than leaving an orphan.
- Add a test that fails when a snapshot directory has no owning test, so the
  next orphan is caught rather than swept for later. Note its limit honestly: it
  can only see what the corpus contains.

## Acceptance

- One recorded location rule, applied to all three roots.
- The naming convention recorded, whether or not it changes.
- Zero orphaned snapshot directories, with any unresolved directory listed
  rather than deleted.
- A guard test against future orphans.
- All four gates green and **all counts unchanged** — a move removes no test and
  an orphan is by definition unread, so any count movement means something live
  was deleted.

## Changes ledger

- snapshot support convention: the shared helper had no recorded placement rule -> `CommandOutputSnapshot` records the library-default `__snapshots__` placement beside the owning test class and leaves `RootDirectory` unset.
- snapshot naming: `.json.` was repeated inline and looked accidental -> `CommandOutputSnapshot.JsonContentNameSegment` names the retained content segment, while every persisted capture remains `.txt`.
- orphan evidence: no corpus owner guard existed -> `CliReportInvariantsTests.CommandOutputSnapshotDirectoriesHaveLiveOwners` fails when an integration snapshot directory has no adjacent live `[Fact]` or `[Theory]` owner.
- snapshot corpus: the capture helper could update names at several call sites -> all affected call sites use the shared `.json.` segment and preserve the existing names; no capture was regenerated.

## Divergences observed

- measured location: the slice recorded the pre-move split of one 3,300-file flat integration root plus two unit roots -> the current tree has 28 integration `__snapshots__` directories containing 3,300 command-output captures, with the old flat root absent; the already-applied move was not repeated and this record remains accurate.
- naming decision: the slice preferred retaining `.json.` because changing it would churn 1,705 files without behavioral gain -> the segment was retained and centralized; no naming or content diff was needed.
- corpus ownership: the 2026-09-16 sweep reported zero orphaned directories -> all 28 integration snapshot roots have a matching live owner source with a `[Fact]` or `[Theory]`; no directory was deleted and no unresolved owner remains.
- gate baseline: the packet states unit `3,191` and integration `2,222` total -> this tree reports unit `3,206` after the one new guard test and integration `2,223` with no integration test added; the remaining baseline offsets predate this slice and require no code action.
- build output: the required repository `artifacts/bin` and `artifacts/obj` destinations were inaccessible to the sandbox -> the exact build path and its offline fallback stopped on access denied, while an isolated system-temp build passed with 0 warnings and 0 errors; the host should rerun the prescribed artifact-path build if that receipt is required.
