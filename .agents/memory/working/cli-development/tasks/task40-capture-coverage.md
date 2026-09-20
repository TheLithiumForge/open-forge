---
open-forge:
  description: Open Task 40 to close the gap between the accepted finding vocabulary and the situations any capture actually exercises, so the output invariants guard more than a quarter of what the CLI can print
  tags: [Memory, Working, CLI, Task, Capture, Coverage, Evidence, Contextual, Active]
---

# Task 40 — Capture coverage

## Task state

- State: **Open, not started.** Raised by the overseer on 2026-09-17 while
  clearing the maintenance debt before Task 32.
- Owner: Root.
- Trigger: three separate pieces of work each ended with the same sentence —
  "this situation has no capture" — so the gap was measured rather than noted
  again.

## Measured, 2026-09-17

**604 of the 813 finding codes in the accepted contracts never appear in any
capture. The corpus exercises 25% of what the CLI can print.**

| command | uncaptured | of |
| --- | --- | --- |
| doctor | 90 | 101 |
| status | 34 | 50 |
| extension install | 27 | 36 |
| extension update | 27 | 35 |
| route init | 27 | 34 |
| library attach | 26 | 34 |
| library sync | 26 | 34 |
| route move | 26 | 36 |
| extension remove | 25 | 32 |
| update | 24 | 29 |

The full list is reproducible by matching contract findings rows against the
code-shaped tokens in `src/cli/tests/**/__snapshots__/**`.

## Why this matters more than it looks

Every output rule this repository enforces is enforced **through the capture
corpus**. A situation with no capture is invisible to all of them:

- `CapturedMessagesMatchTheirContractRow` cannot tell whether a contract row
  still describes the command. That check caught three drifted rows in the
  quarter it can see; nothing looks at the other three quarters.
- `CliReportInvariantsTests`' subject, severity-ordering, `Next:` and vocabulary
  rules only bind where a capture exists.
- Slice 51's bar for blocking findings was accepted knowing 39 Doctor rows could
  not be checked. That number is now known to be 90.

So the risk is not that these situations are broken. It is that **nothing would
tell us if they became broken**, including the guards added specifically to stop
that happening.

## Actionable boundary

- This is fixture work, not wording work. **No message may change here.** A
  situation that cannot be captured without changing behaviour is a finding to
  report, not a licence to reword.
- Prioritise by what the guards protect: a blocking finding with a next action
  earns a capture before an informational row does. Doctor is the largest gap
  and also the command whose findings other commands consume.
- Some situations need a seeded workspace that is expensive or platform-specific
  to build. Where that is true, **record it as genuinely uncapturable and say
  why**, so the residue is a known list rather than an unexamined remainder.
- Prefer extending an existing situation's fixture over inventing a parallel
  harness. The corpus already has one shape per command.

## Acceptance

- Every blocking finding code in the contracts is either captured or recorded as
  uncapturable with its reason.
- The uncaptured count is published in this file after the work, so the next
  reader sees the residue rather than re-measuring.
- All four gates green.

## Related

[51](task30/51-truthful-findings.md) set the bar this task makes checkable.
[73](task30/73-consequence-clauses.md) and the Extension Install recorded-package
split both ended with uncaptured situations. [Task 39](task39-output-audit.md)
owns whether an output is *good*; this task owns whether it is *watched*.
