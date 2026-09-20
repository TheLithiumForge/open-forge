---
open-forge:
  description: Beta journey scenarios G and H covering interruption and recovery, and the cross-cutting output quality every command must meet
  tags: [Memory, Working, CLI, Task, Subtask, Scenario, Beta, Contextual, Active]
---

# Journeys G–H — recovery, and output quality everywhere

## G — When things go wrong

A beta is judged less by the happy path than by what happens the first time
something fails.

- **G1. Interrupt a write.** Cancel a command part-way through applying changes.
  The workspace must not be left half-written, the output must say what was and
  was not done, and recovery must be offered.
- **G2. Run two writing commands at once.** The second must say plainly that
  another command holds the workspace, not leak lock vocabulary.
- **G3. Write where permission is denied.** The message names the path and the
  reason in ordinary words.
- **G4. Damage the ownership record by hand.** `doctor` must diagnose it and say
  how to recover, rather than failing obscurely.
- **G5. Point a command at a folder that is not a workspace.** The message says
  so and suggests `install`.
- **G6. Pass a bad argument.** Invalid input is refused with the accepted value,
  never with `--help` as the answer to an identity error.

## H — Output quality, on every command

Cross-cutting. Run against the whole command surface rather than one journey.

- **H1. `--detail minimal` everywhere.** Is each one the least a user needs, or
  is it padded? The maintainer's bar: it should look good on screen and be token
  friendly.
- **H2. No internal vocabulary reaches a reader.** None of `lifecycle`,
  `residual`, `preflight`, `projection`, `lease`, `occupant`, `provenance`,
  `topology`, `semantic`, `trusted`, `payload`.
- **H3. Every finding names its subject.** A path or an identifier, never the
  finding's own code standing in for one.
- **H4. Every blocking finding gives a next action that runs as typed**, with
  real values rather than placeholders.
- **H5. Counts agree with the rows above them**, and read as words and numbers.
- **H6. A dry run never changes anything** and says so.
- **H7. The same scenario reads consistently across commands.** A missing file
  should not be three different sentences in three commands.

## How to report

For each scenario, one entry:

```
<id> <pass | fail | unclear>
  did:       the exact commands
  saw:       the literal output, trimmed only where repetitive
  expected:  what a first-time reader would have needed
  verdict:   why it passes or fails
```

**`unclear` is a valid verdict, and the most useful one.** If the runner cannot tell whether
the behaviour is right, that is itself a finding about the output: a user would
not have been able to tell either.

Do not fix anything. This run only records, so the same scenarios can be run
again after the fixes and compared.
