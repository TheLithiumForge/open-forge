---
open-forge:
  description: "Approve an external destination once or persistently"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F14: Approve an external destination once or persistently

**Who:** A maintainer who wants control over writes outside .agents.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Use the exact docs/team.md file or the explicitly intended docs directory for a persisted grant. The sibling path docs/team does not cover docs/team.md. Preserve the distinction between one-time and saved approval.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

PERMISSIONS with toolkit targeting docs/team.md and no saved grant.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-05](../scenarios/commands/c21-extension-install.md#c21-05) | The missing destination consent is visible and the request is blocked. |
| 2 | On a suitable fresh branch of the same failed state, use a real terminal and choose once. | [C21-07](../scenarios/commands/c21-extension-install.md#c21-07) | This installation is approved, but no settings grant is saved. |
| 3 | On a separate branch, choose always for the displayed scope, or supply --allow-path docs/team.md for this exact file, or docs for the deliberately selected directory. | [X14](../scenarios/experience.md#x14) | The exact persisted scope is inspectable; later eligible operations may use it. |
| 4 | Revoke the applicable grant, introduce a real update, then run the update automatically. | [C22-09](../scenarios/commands/c22-extension-update.md#c22-09) | Owned external content is still protected by revoked permission. |

## Alternatives and recovery

### Dry-run with allow-path

Preview does not save consent.

Scenarios: [C21-14](../scenarios/commands/c21-extension-install.md#c21-14).

### Grant saved, later content failure

Report the settings effect separately instead of saying nothing changed.

Scenarios: [X26](../scenarios/experience.md#x26).

### Malformed settings

Do not overwrite them to satisfy always approval; implicit .agents admission remains distinct.

Scenarios: [X14](../scenarios/experience.md#x14).

## Final result

The user can tell what was approved, for how long and what actually persisted.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
