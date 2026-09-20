---
open-forge:
  description: First journey run, covering arrival and hand-authoring, with seven defects found and their minimal reproductions
  tags: [Memory, Working, CLI, Task, Subtask, Scenario, Findings, Beta, Contextual, Active]
---

# Findings, run 1 — arrival and hand-authoring

Run by the overseer on 2026-09-17 against the release build, in scratch
workspaces. Journeys A and C only; the agent fleet was unavailable, so this was
done by hand and stops short of the full set.

**Headline: the most natural thing a user can do — create a Markdown file by
hand — breaks indexing, and then makes `doctor` report a Framework failure that
never happened.**

## What works, and reads well

- `install` into an empty folder, and the refusal when it cannot ask for
  confirmation, whose `Next:` runs as typed.
- A second `install` is a one-line no-op.
- `status` and `doctor` on a clean install are short and boring, which is right.
- `index` with a well-formed file is exact: `.agents/guidance/_guidance.md  1 -> 2 entries`.
- An `index` rewrite alone does **not** upset `doctor`. The generated region is
  correctly excluded from the managed-file fingerprint.

## B-1 — A hand-authored file stops the whole index

```
printf '# My Notes\n\nnotes\n' > .agents/guidance/my-notes.md
open-forge index
```

```
The Entries sections could not be rebuilt completely. Nothing was changed.
  Warning  .agents/guidance/my-notes.md  Frontmatter could not be read
  Nothing was written. The other 19 sections are current.
Next: open-forge doctor
```

Exit 3, nothing indexed, the file never listed. **One incomplete file blocks all
20 sections**, including the 19 it has nothing to do with.

`index` requires **both** `description` and `tags`. Measured: no frontmatter
fails; an empty `open-forge:` block fails; `description` without `tags` fails.
The maintainer's intent is the opposite — a nearly empty header should index,
leaving the gaps for the user or their agent to fill.

## B-2 — The message is not true

"The frontmatter of X could not be read completely" is said when the file has
**no frontmatter at all**, and when the frontmatter is present, valid and merely
missing `tags`. Nothing could not be read. It also never names the missing
field, so the reader cannot act on it.

## B-3 — The offered next action cannot help

`Next: open-forge doctor` follows a failure `doctor` does not fix and does not
explain. The reader is sent somewhere that leaves them no better off.

## B-4 — `doctor` reports a Framework failure that never happened

Minimal reproduction, from a clean install:

```
printf '# Plain\n\nno frontmatter\n' > .agents/guidance/plain.md
open-forge doctor
```

```
  Error  Framework files  Framework update did not finish
         Some Framework files are current and others are not, so an update did not finish.
         open-forge update
Next: open-forge update
```

The user created one file. No update was ever run. **`doctor` invents a failed
update and recommends a write command that cannot fix it.** Deleting the one
file restores "No problems found".

The mechanism: with the unreadable file present, the entrypoint's generated
region can no longer be identified, so the managed-file fingerprint falls back
to exact bytes, the `index` rewrite now counts as a change, and the lifecycle
check reads that as a half-finished update.

## B-5 — A resolution line is printed twice

```
  Warning  .agents/guidance/plain.md  Required route metadata is missing
         Required route metadata is missing.
         Fix it by hand.
         Fix it by hand.
```

Three defects in five lines: the message repeats the title verbatim and adds
nothing, it never says *which* metadata is missing, and `Fix it by hand.` is
rendered twice.

## B-6 — One file stops every route check

`Routes were not checked: Required route metadata is missing.` A single
hand-authored file disables route checking for the whole workspace, the same
all-or-nothing shape as B-1.

## B-7 — The same output contradicts itself

The body says `21 links and 22 routes checked.` while the summary says routes
were not checked. Both cannot be true.

## B-8 — Install counts are wrong

```
Created 21 files and 20 directories under .agents
```

Actual: **22 files and 19 directories.** The lock file is excluded from the file
count, which is defensible, but `.agents` itself is counted as a directory
"under .agents", which is not.

## For Task 42, measured rather than assumed

A fresh install is 21 files across 19 directories, and startup reads
**about 8.0k tokens before the user has written anything**. The installed core
includes `ideas`, `observations`, `analysis`, `decisions`, `documents`,
`checkpoints` and `handoffs` — the third-degree routes already proposed for
Extensions.

## Why G4 did not catch these

Checked rather than assumed. The eight defects fall into three different
failure modes, and only one of them is G4 missing something it could have seen.

### Mode 1 — the code was reviewed against the one scenario where it is true

`index.metadata-incomplete` has **six captures**. Every one is the
`UnreadableChild` fixture, where the file genuinely cannot be read and
`The frontmatter of <path> could not be read completely.` is accurate.

The same code is also raised for a file with **no frontmatter**, and for one
whose frontmatter is valid but missing a required field. Neither has a capture.
G4 reviewed the sentence in the scenario that makes it true and had no way to
see the two that make it false.

This is the **one code, one scenario** defect again, and it is the fourth
instance found in this repository. The rule catches it only where a capture
exists to compare.

`Required route metadata is missing` has **zero** captures.

### Mode 2 — outside G4 by its own definition

G4's outcome is that every command "prints, for every status it can reach,
... exactly the messages specified in its subtask". It specified the **wording
of each status**, never whether the status was the right one to reach.

Two consequences:

- **B-4 was an explicit non-goal.** G4's non-goals name "the phase 5 diagnosis
  and interoperability changes". A `doctor` finding that invents a failed
  Framework update is a diagnosis defect, which is phase 5's subject.
  `framework.partial-lifecycle` also has zero captures, and
  [slice 51](../task30/51-truthful-findings.md) had already recorded it among
  the 39 Doctor blocking rows no capture reaches.
- **B-1 is not a miss at all; it is accepted design.** G4's index status table
  specifies: incomplete, "a routed file, its metadata, or recovery unreadable",
  headline `The Entries sections could not be rebuilt completely. Nothing was
  changed.`, exit 3. The all-or-nothing rebuild was written down and accepted.

  So B-1 is a **product disagreement**, not a regression. The accepted design
  says one unreadable file stops the rebuild; the maintainer's intent is that a
  nearly empty header should still index. That conflict has to be settled as a
  decision, not fixed as a bug.

### Mode 3 — captured, reviewed, and wrong anyway

The two that G4 could and should have caught:

- `Fix it by hand.` is rendered **twice**, and the doubling is frozen in at
  least five committed captures, including
  `SafetyBoundary_unsafe-link-detach` at every detail level.
- `Created 21 files and 20 directories` is frozen in a capture. A real install
  creates 22 files and 19 directories.

Both passed review. **A capture proves the output did not change; it cannot
prove the output is true.** Reviewers compared output against output, so a
repeated line read as noise in a clean diff and nobody counted the files on
disk. More captures do not fix this mode — only checking a claim against the
world does.

### What this says about the guards

Mode 1 is [Task 40](../task40-capture-coverage.md)'s thesis demonstrated on a
live example rather than argued: the corpus reaches a quarter of the finding
vocabulary, so both the automated rules and the human review operate on that
quarter. Mode 3 is the limit of snapshot testing itself, and is the argument for
[Task 41](../task41-beta-journey-scenarios.md) existing at all — a journey run
compares behaviour against what a user expected, which is the only thing that
catches a frozen lie.
