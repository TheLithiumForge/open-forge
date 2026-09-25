---
open-forge:
  description: "Correct an incomplete command in one attempt"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F22: Correct an incomplete command in one attempt

**Who:** A maintainer who knows the desired file but not every required option.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

A deterministic route destination should be creatable without optional description and tags. Preserve missing values without inventing meaning; warn and allow later enrichment. Truly missing targets or ambiguous input still need one helpful correction.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

ROUTES with a valid guidance parent and absent new-note.md.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge route create guidance/new-note | [C14-07](../scenarios/commands/c14-route-create.md#c14-07) | Target: create the unambiguous destination with a warning about optional missing metadata. |
| 2 | Optionally choose a more useful description and tag. | [X15](../scenarios/experience.md#x15) | The note remains usable before enrichment. |
| 3 | open-forge route update guidance/new-note --description "Team operating notes" --tag=Guidance | [C15-01](../scenarios/commands/c15-route-update.md#c15-01) | Enrichment updates the same file and navigation. |
| 4 | open-forge route inspect guidance/new-note | [C12-02](../scenarios/commands/c12-route-inspect.md#c12-02) | The resulting source is exactly the one named in the corrected request. |

## Alternatives and recovery

### Need syntax help

Terminal help works without requiring the missing target first.

Scenarios: [X24](../scenarios/experience.md#x24).

### Parent absent

Create through unambiguous missing intermediate scopes under a recognized root. Record any current stop as a gap; explicit parent initialization is a labelled recovery branch, not the desired mandatory workflow.

Scenarios: [C14-09](../scenarios/commands/c14-route-create.md#c14-09).

## Final result

The user reaches the intended result without a chain of avoidable trial-and-error failures.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
