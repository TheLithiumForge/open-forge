---
open-forge:
  description: Open Task 39 to audit every CLI output for actionability, confirm the G4 conversion actually improved each command, and find remaining legacy and evidence gaps
  tags: [Memory, Working, CLI, Task, Output, Audit, Regression, Contextual, Active]
---

# Task 39 — Output Audit

## Task state

- State: **Open; representative correction passed focused and complete managed/native gates.** Raised by the
  maintainer on 2026-09-16.
- Owner: Root.

The 2026-09-21 [error packet](beta-follow-ups/task39-errors.md) records the current caller inventory, reproduced manifest-subject loss and proposed worker boundaries. Historical counts below are dated evidence, not a current defect count. E1 and E2 passed focused and complete managed/native gates under the [execution record](beta-follow-ups/execution.md). The wider command-family audit remains open.

- Trigger: the maintainer read a before/after from the cause-bounding work and
  observed that the new message names no path — _"what purpose does an error
  serve if not fixing the issue"_.

## 1. Original regression evidence — bounded causes destroyed the actionable detail

The regression was introduced on 2026-09-16. The manifest-error example below
is corrected by E1/E2; other command families still need their own inventory.
The original work that removed leaked .NET internals over-corrected: it replaced
whole causes with generic phrases instead of replacing only the internal part.

```text
before:  available packages: The process cannot access the file '<src>/toolkit/extension.json' because it is being used by another process.
after:   available packages: the filesystem operation failed.
```

The "before" carried **no exception type and no HRESULT**. It was already plain
English, and it named the file and the reason. The "after" is unactionable.

### The mechanism

`CliFindingWording.PlainCause` classifies by scanning for a marker and then
**truncating from the marker onward**:

```csharp
var prefix = value[..markerIndex].Trim().TrimEnd(':').Trim();
return prefix.Length == 0 ? reason : $"{prefix}: {reason}";
```

So the damage is uneven, and predictably so:

- A path **before** the marker survives —
  `Writing .agents/memory/_memory.md failed: UnauthorizedAccessException…`
  keeps its path because the path is in the prefix.
- A path **inside or after** the marker is destroyed —
  `The process cannot access the file 'X' because…` has the marker at index 0,
  so the prefix is empty and the filename is discarded.

### Measured damage

Across the integration capture corpus on 2026-09-16:

|                                          | Lines   |
| ---------------------------------------- | ------- |
| Generic reason **with** a path alongside | 32      |
| Generic reason **with no path at all**   | **114** |

Worst observed, all currently shipping:

```text
filesystem access was denied.
Cannot list Extensions: the content is not valid JSON.
available packages: the content is not valid JSON.
```

### Partially fixed on 2026-09-17

The overseer applied the surgical half immediately, under maintainer
instruction. `PlainCause` no longer discards the subject: where the caller framed
the failure its prefix is kept, and otherwise the path the operating system
quoted inside its own sentence is kept. The specific reason is also preserved
where the platform gave one, so a locked file now says so instead of reporting a
generic filesystem failure.

Measured effect on the capture corpus: lines carrying a bounded reason with **no
path at all** fell from **114 to 12**.

The rule is now recorded in
[CLI design guidance](../../../../guidance/cli-design.md) under
_Build Failures From Your Own Facts_.

**What is left for this Task**: the remaining 12 lines, where the platform
quoted no path and the caller supplied no prefix. Those cannot be fixed by
reading the string — the call site has to pass the subject it already holds.
That is the structured-facts change below, across 38 call sites of `PlainCause`
and `CauseSentence`, and it is the real fix. Marker-based classification should
be retired at the same time, not tuned.

### The accepted direction

**Not leaking an exception type does not mean discarding what failed.** Compose
the sentence from **facts the operation already holds** — the path it was
operating on and the classified failure kind — rather than by sanitising an
operating-system string after the fact. The operation knows which file it was
reading; that is structured data, not something to recover by regex.

Shape to aim for, not frozen wording:

```text
<path> could not be read because another process is using it.
<path> is not valid JSON.
<path> could not be read: permission was denied.
```

Keep the raw cause where it already lives — the `cause` evidence at `full` and
`debug`. That part of the earlier work was right and stays.

Marker-based truncation should be retired, not tuned. Any classifier that has to
guess where the useful half of a string ends will keep making this mistake.

## 2. Did every command's output actually improve?

G4 converted all 28 commands, but "converted" was verified, not "improved".
Confirm per command, from the captures, that the current output answers the
question its catalogue says the command exists to answer. Record any command
whose output is merely _different_ rather than better.

## 3. What legacy remains?

Re-derive rather than trusting the closing claims:

- `Presentation/Legacy/` held three composer-bound `*HelpSections.cs` at G4
  close. Confirm that is still true and still justified.
- Retired vocabulary on live surfaces was reported as zero, then found not to be
  and fixed. Re-sweep.
- Dead code kept alive only by its own test has been found twice
  (`WorkspaceSelectionWireVocabulary`, the compact-JSON models). Sweep for more.

## 4. Evidence gaps

Known and unfixed:

- **8 of 9** `extension-list` `installed-*` situations have no capture at all.
- `repair.plan-conflict` has no native output fixture.
- `TargetIdentityMismatch` is reachable but unsnapshotted.
- 17 integration tests skip on Windows; they cover Unix permission paths, so
  permission-failure output is unverified on the primary development platform.

A finding with no capture is invisible to every corpus invariant, including
`NoTwoFindingCodesOfOneCommandRenderTheSameMessage`. Record the true count.

## 5. Snapshot location — settled on 2026-09-17

The overseer had claimed a per-test `__snapshots__` layout "would bury
`Commands/`". That did not hold up: `__snapshots__` is the library default, each
test class gets its own adjacent directory, and no reason for the override was
ever recorded.

**The override is removed.** `MatchDetailSnapshot` no longer sets
`RootDirectory`, so all 3,300 command output captures now live in the library's
default location beside the test that owns them, in 28 `__snapshots__`
directories. Git recorded the change as 3,300 renames and a content-hash
comparison confirmed the two trees were byte-identical before the old root was
deleted; the library does **not** clean up the old location itself.

One consequence worth remembering: `CliReportInvariantsTests` loaded the corpus
from the old hardcoded root and failed with `DirectoryNotFoundException` until it
was pointed at the new layout. It now enumerates every `__snapshots__` directory
under the integration project and throws when it finds none, so an empty corpus
cannot pass silently.

[Slice 54](task30/54-snapshot-consolidation.md) still owns the remaining
question: the naming convention, and whether anything unowned survives.

## Actionable boundary

- Item 1 is a regression and should be fixed first, ahead of the audit.
- Every message change is a frozen-string change: catalogue row updated in the
  same change, captures regenerated and reviewed per situation.
- Composing from structured facts may require a result model to carry a path it
  does not carry today. That is a contract decision to report, not a reason to
  fall back on string sanitising.
- **One code, one situation** binds: if a better sentence only works for one of
  two situations a code carries, that is a split to propose.

## Acceptance

- No user-facing message states a failure without naming what failed, wherever
  the operation holds that fact.
- Marker-based cause truncation is gone.
- A recorded per-command verdict that the output improved, with the exceptions
  named.
- A re-derived legacy inventory and a true count of uncaptured situations.
- All four gates green.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
