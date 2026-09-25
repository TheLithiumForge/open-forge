---
open-forge:
  description: Open Task 32 to review whether the Workspace echo and the Next action belong in minimal output across all 28 commands
  tags: [Memory, Working, CLI, Task, Presentation, Minimal, Contextual, Active]
---

# Task 32 — Minimal Output Sweep

## Task state

- State: **Open, not started.** Queued as the last task after Task 30 G4's
  decisions are implemented.
- Owner: Root.
- Trigger: the maintainer's ruling on
  [40 — verification](../../../archived/cli-development/tasks/task30-g4/40-verification.md) divergence 6 and
  [30 — extension install](../../../archived/cli-development/tasks/task30-g4/30-extension-install.md), recorded
  2026-09-16. Both were settled in favour of the shared presentation rule for
  now, with this sweep queued to revisit the underlying question.

## The question

Two lines are currently required in `minimal` output by shared presentation
rules, and both were questioned by the command catalogues that had to carry
them:

- The `Workspace:` echo, required by accepted decision **C12** in
  [the G4 packet](../../../archived/cli-development/tasks/task30-g4/_task30-g4.md#accepted-decisions).
- The `Next:` action line.

`index` at `all-current` and `update` at `up-to-date` both print a `Workspace:`
line at `minimal` that their own one-line catalogue examples omit. The examples
were written as if the smallest useful answer were a single sentence; the shared
rule makes it three lines. The immediate conflict was resolved by keeping the
rule and correcting the examples, but that resolution did not decide whether the
rule itself is right for `minimal`.

`Next:` raises the same question from the other direction: Extension Install
requires a continuation line after it, which forced the "at most one `Next:`
line, and it is the last line" invariant to be relaxed.

The question this Task answers: **at `minimal`, which of these two lines earn
their place in every command, and which are better at `standard` and above?**

`minimal` is the default detail level, so this decides what most users see most
of the time.

## Actionable boundary

- Survey all 28 commands' `minimal` output as it actually renders, not as the
  catalogues describe it. Group by whether the echo and the next action add
  anything a reader of that specific output does not already have.
- A `Workspace:` echo earns its place where the workspace is ambiguous — where
  the command may have resolved a different workspace than the reader expects.
  It does not earn its place merely because a rule requires it.
- A `Next:` line earns its place where there is a genuine next step. A next
  action that restates what the reader just did, or that every run always emits,
  is noise at the default level.
- Decide per situation, not per command: the same command may warrant the echo
  when it changed something and not when it reports that nothing needed doing.
- Any change here is behaviour-changing and touches every affected capture.
  Treat it as a G4-scale presentation change with the same review discipline,
  not as a tidy-up.

## Acceptance

- A recorded decision for the `Workspace:` echo and for `Next:` at `minimal`,
  with the reasoning and the situations each applies to.
- If the rule changes, C12 and the shared presentation rules in
  [00 — conventions](../../../archived/cli-development/tasks/task30-g4/00-conventions.md) are amended, every affected
  catalogue's `Text by level` examples agree with the code, and every capture is
  regenerated and reviewed.
- If the rule stands, the two catalogues whose examples omitted the echo are
  corrected and the question is recorded as settled.
- All four gates green, and no other output changes.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
