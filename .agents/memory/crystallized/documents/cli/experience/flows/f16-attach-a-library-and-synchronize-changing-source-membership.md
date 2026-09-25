---
open-forge:
  description: "Attach a Library and synchronize changing source membership"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F16: Attach a Library and synchronize changing source membership

**Who:** A maintainer keeping shared guidance linked rather than copied.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

LIB-UNATTACHED: real contained shared-guides, an existing destination route and unoccupied leaves.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge library attach team shared-guides --to .agents/guidance/team --automatic | [C26-01](../scenarios/commands/c26-library-attach.md#c26-01) | Relative file symlinks and a registration exist; source bytes are untouched. |
| 2 | open-forge library inspect team | [C25-01](../scenarios/commands/c25-library-inspect.md#c25-01) | The initial complete inventory and links agree. |
| 3 | Add one eligible file to shared-guides, then run open-forge library inspect team. | [C25-02](../scenarios/commands/c25-library-inspect.md#c25-02) | A source addition is visible without creating its link yet. |
| 4 | open-forge library sync team --automatic | [C27-02](../scenarios/commands/c27-library-sync.md#c27-02) | The added source file now has the correct relative destination link. |
| 5 | Add one other source member and retire a different existing one, then sync again. | [C27-04](../scenarios/commands/c27-library-sync.md#c27-04) | Membership changes are applied while unchanged links survive. |
| 6 | open-forge library detach team --automatic | [C28-01](../scenarios/commands/c28-library-detach.md#c28-01) | Registered links and the registration are removed; the remaining source files stay. |

## Alternatives and recovery

### Initially empty source

A real empty registration may gain links after later source additions.

Scenarios: [C26-05](../scenarios/commands/c26-library-attach.md#c26-05).

### Incomplete source scan

Do not infer retirements or delete links from partial inventory.

Scenarios: [C27-09](../scenarios/commands/c27-library-sync.md#c27-09).

## Final result

Attach, inspect, sync and detach operate on one coherent Library state across the entire flow.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
