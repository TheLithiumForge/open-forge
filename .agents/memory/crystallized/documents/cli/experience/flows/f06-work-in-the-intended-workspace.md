---
open-forge:
  description: "Work in the intended workspace"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F06: Work in the intended workspace

**Who:** A maintainer moving between repositories and subdirectories.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

TWO-WORKSPACES with healthy A, malformed B, and an uninstalled subdirectory inside A.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | From the uninstalled subdirectory, run open-forge status. | [X11](../scenarios/experience.md#x11) | The selected current directory is not replaced silently by A. |
| 2 | From B, run open-forge status --workspace "$WS_A". | [X11](../scenarios/experience.md#x11) | The explicit A workspace is reported and echoed at minimal. |
| 3 | open-forge index --workspace "$WS_A" --dry-run | [X11](../scenarios/experience.md#x11) | Only A is inspected for the preview. |
| 4 | open-forge status --workspace "$MISSING_WS" | [C01-11](../scenarios/commands/c01-status.md#c01-11) | A nonexistent explicit path is blocked without a nearest-parent fallback. |

## Alternatives and recovery

### User selects B intentionally

Report B’s actual defect. Do not mix A’s healthy measures with B’s paths.

Scenarios: [C05-07](../scenarios/commands/c05-index.md#c05-07).

## Final result

Every result and potential effect belongs to the directory the user selected.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
