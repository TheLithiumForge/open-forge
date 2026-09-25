---
open-forge:
  description: "Read more detail without changing what happened"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F21: Read more detail without changing what happened

**Who:** A user moving from a concise result to troubleshooting or machine consumption.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Capture current executable output from identical pre-state copies. Historical supplied captures are provenance only, not a required flow step or an oracle. Expose actionable warnings at the default detail level.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

VIEW-VARIANTS, with exact identical pre-state copies for each mutating view and unchanged state for read-only views.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Capture default text and explicit minimal for one success and one incomplete result. | [X20](../scenarios/experience.md#x20) | The default is verified to be minimal without a semantic change. |
| 2 | Capture standard, full and debug for those same cases. | [X20](../scenarios/experience.md#x20) | Added detail reveals evidence rather than repeating the same explanation. |
| 3 | Review these fresh captures for repeated explanations and missing useful facts. | [X25](../scenarios/experience.md#x25) | Communication quality is assessed against the actual fixture, not an old capture. |
| 4 | Capture JSON and detail-filter variants, retaining stdout, stderr and exit separately. | [X20](../scenarios/experience.md#x20) | Presentation variants describe the same selected result and preserve counts/unknowns. |

## Alternatives and recovery

### Doctor warning-only output

Show affected sources and a useful next action at the default detail level. A warning count alone is insufficient; richer views may add supporting evidence.

Scenarios: [C02-03](../scenarios/commands/c02-doctor.md#c02-03).

### Requested authored content

Do not remove user content because it resembles forbidden generated vocabulary.

Scenarios: [X23](../scenarios/experience.md#x23).

### Parser failure or terminal help

Do not require a schema-3 domain envelope before binding.

Scenarios: [X24](../scenarios/experience.md#x24).

## Final result

Human and machine consumers see consistent facts at different levels of detail.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.

## Notes

This is an explicit view-comparison fork, not a normal journey with hidden resets. Per-scenario richer exact transcripts remain deferred unless supplied.
