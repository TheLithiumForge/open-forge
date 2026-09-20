---
open-forge:
  description: "Recover a missing link and protect a changed destination"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F17: Recover a missing link and protect a changed destination

**Who:** A maintainer who manually moved or edited linked destination files.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

An already absent destination must not prevent detaching the remaining known unchanged links and releasing registration. Preserve a changed ordinary user file and report it distinctly; do not require restoring a link only to remove it.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

The verified attached state of F16 before detachment.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Unlink one registered destination without touching its source, then list Libraries. | [C24-03](../scenarios/commands/c24-library-list.md#c24-03) | The missing destination link is distinguished from missing source content. |
| 2 | open-forge library sync team --automatic | [C27-08](../scenarios/commands/c27-library-sync.md#c27-08) | The missing registered link is restored with the declared warning. |
| 3 | Replace that link with an ordinary user file, then sync. | [C27-07](../scenarios/commands/c27-library-sync.md#c27-07) | The changed destination is preserved and reported; independently safe sync effects continue. Add another source member before this call to make continuation observable. |
| 4 | Deliberately move the user file to a backup outside the registered destination set, then sync again. | [X18](../scenarios/experience.md#x18) | The user’s bytes survive in the chosen backup; the registered link is restored. |
| 5 | open-forge library inspect team | [C25-01](../scenarios/commands/c25-library-inspect.md#c25-01) | Registration and links agree again. |

## Alternatives and recovery

### Detach attempted while destination changed

Preserve the user file, detach other known unchanged links, and release the selected registration with an explicit warning. Remembered ownership does not authorize deleting the replacement.

Scenarios: [C28-06](../scenarios/commands/c28-library-detach.md#c28-06).

### Destination link absent at detach

Target: finish detaching known unchanged links and release registration; report the already absent destination without blocking. Record current blocking behavior as a gap.

Scenarios: [C28-05](../scenarios/commands/c28-library-detach.md#c28-05).

## Final result

The user retains their edits and the Library returns to a known usable state.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
