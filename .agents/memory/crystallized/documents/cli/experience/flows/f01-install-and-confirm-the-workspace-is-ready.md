---
open-forge:
  description: "Install and confirm the workspace is ready"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F01: Install and confirm the workspace is ready

**Who:** A first-time maintainer who knows normal files and commands, not Open Forge internals.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

W0, with a hand-authored README and no installed Framework.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge status | [C01-01](../scenarios/commands/c01-status.md#c01-01) | The user knows this exact workspace is not installed; nothing has changed. |
| 2 | open-forge install --dry-run | [C03-02](../scenarios/commands/c03-install.md#c03-02) | The intended host sections, content targets and counts are visible, but no installation exists yet. |
| 3 | open-forge install --automatic | [C03-01](../scenarios/commands/c03-install.md#c03-01) | The reviewed installation has actually been created; record the independent inventory. |
| 4 | Compare the full created-file and created-directory sets with the install report. | [X08](../scenarios/experience.md#x08) | A truthful baseline inventory is available; no count is accepted only because it was printed. |
| 5 | open-forge status | [C01-02](../scenarios/commands/c01-status.md#c01-02) | Readiness and startup measures describe the resulting installed state. |
| 6 | open-forge context | [C08-01](../scenarios/commands/c08-context.md#c08-01) | Startup instructions are readable in the declared order. |
| 7 | open-forge doctor | [C02-01](../scenarios/commands/c02-doctor.md#c02-01) | Applicable checks finish on the same installed state. |
| 8 | open-forge install --automatic | [C03-03](../scenarios/commands/c03-install.md#c03-03) | The identical second install is a real no-op. |

## Alternatives and recovery

### Existing AGENTS.md

Preserve authored host content; compare its bounded managed section separately.

Scenarios: [C03-04](../scenarios/commands/c03-install.md#c03-04).

### Occupied target

First stop with no writes. Inspect the force dry run and deliberately choose eligible replacement; never force an unsafe or other-owned path.

Scenarios: [C03-05](../scenarios/commands/c03-install.md#c03-05), [C03-06](../scenarios/commands/c03-install.md#c03-06).

### No interactive confirmation

Explain the missing confirmation; only the user-authorized automatic request resumes.

Scenarios: [C03-08](../scenarios/commands/c03-install.md#c03-08).

## Final result

The workspace is installed, inspectable and repeatable; original unrelated content survives.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
