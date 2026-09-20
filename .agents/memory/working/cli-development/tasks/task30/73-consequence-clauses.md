---
open-forge:
  description: Task 30 slice 73 measuring whether findings state what a failure costs the reader, ruling out a schema consequence field, and adding the missing clause as prose where a message stops at the fact
  tags: [Memory, Working, CLI, Task, Subtask, Wording, Contextual, Active]
---

# 73 — Consequence clauses

## Outcome

A reader who hits a warning learns what the tool did **not** do as a result, in
the same sentence, wherever that is not already obvious.

## The question this slice closed

Slice 51 set the bar that a blocking finding should say what is wrong, where,
what it costs, and one command forward. Three of the four are enforced. The
fourth could not be, and the agent correctly refused to invent wording rather
than report a gap.

The maintainer asked whether a consequence field was still needed, by how many
commands, and for what exactly.

## Measured, 2026-09-17

**There has never been a consequence field.** `git log -S"Consequence"` over
`src/cli/**/*.cs` returns nothing. `CliFinding` carries `Severity`, `Code`,
`Title`, `Message`, `Subject`, `Category`, `Resolution`, `Actions`,
`Candidates`, `Evidence` and `Provenance`. `CliResolution` is an enum of how to
fix a finding, never of what it costs. The absence is original, not a
regression.

**The consequence is carried as a trailing clause in the message**, and the
convention is real and already in use:

- `context.invalid-encoding` — "`<path>` is not valid UTF-8, so it was not included."
- `find.frontmatter-unavailable` — "... so its tags were not matched."
- `install.managed-divergence` — "... Install does not replace changed files."

**Coverage.** 373 contract rows carry message text. Excluding `doctor`, whose
rows are titles rather than messages, 87 warning rows first read as missing the
clause. Of those, 14 state it in a form the first pattern missed, and 8 are
listing rows where the row is the report. **65 across 18 commands remained**, of
which **25 belong to `status`**.

## Ruling

**No schema field.** The consequence is situation-specific prose; an enum cannot
carry it and a free-text field duplicates `Message` and creates two places to
keep in sync. JSON consumers already branch on `code`, `status` and `subject`. A
required field would also mean filling roughly 775 rows before release.

**`status` is excluded.** Its output is a state report, so the consequence is the
report's subject rather than a clause on each row.

**The remaining candidates get prose**, and only where the clause adds a fact the
reader does not already have.

## Actionable boundary

- The correct clause is usually supplied by a sibling in the same switch.
  `ContextWording.cs` states "It was not followed." on `TargetAmbiguous`,
  `TargetUnsafe`, `TargetMissing` and `LinkEncodingInvalid`, and omits it on
  `TargetUnreadable`. Match the sibling rather than composing new wording.
- State what the code does, never what it plausibly does. Where the behaviour
  cannot be read from the code, report the candidate instead of writing a
  sentence for it.
- Leave a candidate alone when the verb already carries the consequence, when the
  row is a listing column, or when the fact makes it self-evident. Over-applying
  this is worse than under-applying it.
- Every changed message must have its contract row changed to match.

## Acceptance

- Each candidate is either changed with a clause read from the code, or recorded
  as deliberately unchanged with the reason.
- No finding code, severity, status, exit code, count or ordering moves.
- Contract rows match the shipped messages.
- All four gates green, with every regenerated capture reviewed per situation.

## Related

[51](51-truthful-findings.md) set the bar this slice answers the fourth part of.
The broader rewording pass against the output proposals stays with
[task 37](../task37-wording-review-against-proposals.md); this slice is only the
consequence clause.

## Changes ledger

Seven messages gained a consequence clause. Each was checked against the
headline its command renders above it, not read in isolation.

- message: `context.layer-unavailable` -> `<path> could not be read, so it was not included.`
- message: `context.fragment-missing` -> appends `It was not followed.`, matching its four siblings in the same switch.
- message: `context.target-unreadable` -> appends `It was not followed.`, matching the same four siblings.
- message: `extension-inspect.dependency-incomplete` -> `The dependency <dependency> of <id> could not be resolved: <reason>. It was not compared.`
- message: `library-list.record-unavailable` -> `The Library section of .agents/open-forge.lock.json could not be read, so no Libraries are listed.`
- message: `references.candidate-unsafe` -> `<path> could not be scanned safely for incoming links, so none of its links were counted.`
- message: `update.target-unavailable` -> `<path> could not be read, so it was not updated.`

Eight captures changed, all Library List `RecordUnreadable`. The other six
situations have no capture.

**Contracts were updated in the same change.** Crystallized sources are kept
current, not left to drift: an Evergreen document is edited and linked back to
its source, and a source is either edited behind a change or replaced by
uprooting, extracting or archiving it. G4's "frozen" strings mean the stabilized
output wording is not to be reworded by an implementer on a whim; they do not
mean the contract may go stale behind the code. The seven rows above were
updated, and two findings tables were re-aligned because a new message is wider
than the column was.

Two rows in `contracts/extension/remove/interface.md` were also corrected
because they no longer described the command:

- `extension-remove.lifecycle-observation` was listed as `<id> is not recorded
  as installed.` with `open-forge extension list --installed`. That text exists
  nowhere in Remove and appears to have been copied from Update. The code emits
  `<dependency> remains installed and is no longer needed by <id>.` under the
  title `Dependency remains installed`, and offers
  `open-forge extension remove <dependency>`. Both cells now match.
- `extension-remove.managed-divergence` was removed. Its row had said
  `removed ... delete if unreachable (ledger)`, and the code was deleted, so the
  findings table no longer lists a code the command cannot emit.

## Wording pass

Four clauses were rewritten before merge because they did not read the way the
[writing standard](../../../../crystallized/documents/maintenance/writing.md)
asks. Recorded because the shape of the mistake repeats:

- `library-list.record-unavailable` said `so Libraries cannot be listed from
  it`. "From it" is clumsy; the plain result is `so no Libraries are listed.`
- `references.candidate-unsafe` said `so its links were not included in the
  incoming scan`, repeating both "incoming" and "links" inside one sentence.
  Now `so none of its links were counted.`
- `extension-inspect.dependency-incomplete` appended `, so the comparison could
  not finish` directly after an interpolated `<reason>`, which reads as a comma
  splice. Now a second sentence, `It was not compared.`, which also mirrors
  Context's `It was not followed.`
- `context.closure-unavailable` gained `Some context was not included.`, which
  is both vague and already carried by the headline `Context could not be read
  completely.` Dropped.

`library-sync.mapping-unavailable` was dropped for the headline reason below:
its Incomplete headline is `<id> could not be synchronized: <limitation>.
Nothing was changed.`, so the clause repeated the closing sentence.

## The rule this slice had to learn

**A finding message is not read in isolation. Several commands interpolate the
first finding's message into the headline**, so a clause that reads well alone
becomes a stutter once rendered. Nine proposed changes were rejected for this:

```
The toolkit Extension could not be updated: The source <path> could not be read,
so nothing was changed. Nothing was changed.
```

The headline already opened with the consequence and closed with it. Rejected on
that basis: `extension-create.catalogue-unavailable`,
`extension-install.source-unavailable`, `extension-update.source-unavailable`,
`cleanup.catalogue-incomplete`, `route-create.template-unavailable`,
`route-update.template-unavailable`, `library-attach.source-root-unavailable`
and `library-attach.mapping-unavailable`.

`extension-inspect.path-unavailable` was rejected separately: it belongs to the
double-space `Path*` listing family alongside `PathChanged`, `PathMissing`,
`PathNew` and `PathRetired`, and turning it into a sentence broke that column.

**Before adding a clause, read the command's headline construction.**
`RouteCreateReportSelector:203` and `RouteUpdateReportSelector:236` interpolate
`Message(result, first)`; `LibraryAttachReportSelector:319` interpolates
`FindingMessage(...)`. `ContextWording.Incomplete()` takes no argument,
`UpdateWording.Incomplete` takes the finding's `Cause` rather than its message,
and `LibrarySyncReportSelector` keeps its own reason vocabulary — those are the
commands where a clause is safe.

## Divergences observed

- Four of the nine kept changes have no capture, so their rendered form is
  argued from the selector rather than proven by a fixture. Slice 51 already
  recorded uncaptured situations as a standing gap; these join it.
- `references.invalid-encoding` is **one code covering two situations** with
  incompatible consequences: a source-scan failure and an outgoing-link
  resolution failure. The lane stopped rather than invent wording. This is the
  same "one code, one situation" class as the splits already ruled on, and it
  remains an open maintainer decision.
- `context.closure-unavailable` is shared by startup-closure and followed-link
  graph failures, so its clause is deliberately generic.
