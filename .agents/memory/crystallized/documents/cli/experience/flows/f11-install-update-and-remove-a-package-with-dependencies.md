---
open-forge:
  description: "Install, update and remove a package with dependencies"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F11: Install, update and remove a package with dependencies

**Who:** A maintainer using a toolkit package rather than managing each file manually.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

PACKAGES with toolkit depending on base and a pinned next-version catalogue available for a later manual source swap.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge extension list --source "$CAT" | [C18-05](../scenarios/commands/c18-extension-list.md#c18-05) | The user sees the selected catalogue and package descriptions. |
| 2 | open-forge extension inspect toolkit --source "$CAT" | [C19-03](../scenarios/commands/c19-extension-inspect.md#c19-03) | The dependency and destination facts are visible before installation. |
| 3 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-02](../scenarios/commands/c21-extension-install.md#c21-02) | toolkit and base are installed once with verified effects. |
| 4 | open-forge extension remove base --automatic | [C23-05](../scenarios/commands/c23-extension-remove.md#c23-05) | Removal is blocked because toolkit still requires base; current content survives. |
| 5 | Replace CAT with the pinned next-version source, then preview and apply toolkit update using the same explicit --source. | [C22-03](../scenarios/commands/c22-extension-update.md#c22-03) | The intended updated membership is installed; retired content follows the deliberate keep/prune branch. |
| 6 | open-forge extension remove toolkit --automatic | [C23-04](../scenarios/commands/c23-extension-remove.md#c23-04) | toolkit is removed; base remains and is identified as no longer needed. |
| 7 | open-forge extension remove base --automatic | [C23-01](../scenarios/commands/c23-extension-remove.md#c23-01) | The user deliberately removes the remaining dependency. |

## Alternatives and recovery

### Retired file kept

Keep it and show the relevant prune preview action.

Scenarios: [C22-04](../scenarios/commands/c22-extension-update.md#c22-04).

### User chooses pruning

Delete only eligible retired ownership after the preview.

Scenarios: [C22-05](../scenarios/commands/c22-extension-update.md#c22-05).

### Shared file

Keep it for the remaining owner.

Scenarios: [C23-02](../scenarios/commands/c23-extension-remove.md#c23-02).

### Only available source or version metadata changes

Report the actual current difference without inventing a user edit or treating version strings as ordered compatibility.

Scenarios: [X30](../scenarios/experience.md#x30).

## Final result

The complete package lifecycle preserves dependency and ownership meaning, not just isolated command success.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
