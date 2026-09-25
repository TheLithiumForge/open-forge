---
open-forge:
  description: "Detach after the source folder disappears"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F18: Detach after the source folder disappears

**Who:** A maintainer retiring a Library whose source is no longer available.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Distinguish dangling and absent links, but allow both to be retired from a known registration. Unavailable source enumeration must not imply source retirements during sync.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

LIB with valid registration, current raw relative symlink targets and applicable permission.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Remove or rename the source root while keeping every destination symlink entry. | [X17](../scenarios/experience.md#x17) | Links are dangling, not absent. |
| 2 | open-forge library list | [C24-06](../scenarios/commands/c24-library-list.md#c24-06) | The registered Library is visible with its source-availability issue. |
| 3 | open-forge library inspect team | [C25-07](../scenarios/commands/c25-library-inspect.md#c25-07) | Full source comparison is unavailable. |
| 4 | open-forge library sync team --automatic | [C27-09](../scenarios/commands/c27-library-sync.md#c27-09) | No retirement is inferred from the unavailable source. |
| 5 | open-forge library detach team --automatic | [X17](../scenarios/experience.md#x17) | The known links are removed using destination identity; no source read or deletion is required. |

## Alternatives and recovery

### A destination entry is also gone

Target: skip the already absent destination with a useful warning and detach remaining known unchanged links.

Scenarios: [C28-05](../scenarios/commands/c28-library-detach.md#c28-05).

### External permission revoked

Source-independent does not bypass consent.

Scenarios: [C28-08](../scenarios/commands/c28-library-detach.md#c28-08).

## Final result

The Library is detached without fabricating source facts or confusing dangling and missing links.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
