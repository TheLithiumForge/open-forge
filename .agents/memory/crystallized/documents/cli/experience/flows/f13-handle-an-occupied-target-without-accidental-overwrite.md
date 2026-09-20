---
open-forge:
  description: "Handle an occupied target without accidental overwrite"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F13: Handle an occupied target without accidental overwrite

**Who:** A maintainer adding a package to an existing project.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

PACKAGES with one eligible unowned ordinary conflict and a distinctive user file backup prepared for comparison.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-09](../scenarios/commands/c21-extension-install.md#c21-09) | The operation is blocked and all targets remain unchanged. |
| 2 | open-forge extension install toolkit --source "$CAT" --force --dry-run | [C21-14](../scenarios/commands/c21-extension-install.md#c21-14) | The exact eligible replacement is visible; no authority is silently added by preview. |
| 3 | After reviewing and authorizing replacement, run open-forge extension install toolkit --source "$CAT" --force --automatic. | [C21-10](../scenarios/commands/c21-extension-install.md#c21-10) | Only the eligible selected replacement is made. |
| 4 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-11](../scenarios/commands/c21-extension-install.md#c21-11) | A repeat is a no-op. |
| 5 | Edit a selected installed file, then repeat install. | [C21-12](../scenarios/commands/c21-extension-install.md#c21-12) | Install preserves the new edit and points to the distinct update operation. |

## Alternatives and recovery

### Other owner or protected path

Do not permit force to override ownership or reserved boundaries.

Scenarios: [X27](../scenarios/experience.md#x27).

### User declines

Keep all original content.

Scenarios: [C21-18](../scenarios/commands/c21-extension-install.md#c21-18).

## Final result

Replacement happens only under the intended explicit authority, and install/update remain distinct.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
