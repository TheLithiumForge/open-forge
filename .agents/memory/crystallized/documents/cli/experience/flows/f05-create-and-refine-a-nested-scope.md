---
open-forge:
  description: "Create and refine a nested scope"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F05: Create and refine a nested scope

**Who:** A maintainer organizing team-specific guidance.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Review adjustment

Create a clearly identified descendant under a recognized route without an avoidable parent-creation ritual. If the CLI needs an explicit init today, record the gap and continue a separately labelled recovery branch. Do not invent a new root category.

This desired behavior is pending flow validation before test implementation. A current CLI disagreement is recorded as evidence, not copied into the expectation.

## Starting point

ROUTES with guidance present and guidance/team absent.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge route create guidance/team/new-topic/notes --description "Team topic notes" --tag=Guidance | [C14-09](../scenarios/commands/c14-route-create.md#c14-09) | Target: create the missing intermediate scope entrypoints and new leaf under the recognized guidance root, then list the leaf. |
| 2 | open-forge route update guidance/team/new-topic/notes --tag=Guidance --tag=Team | [C15-02](../scenarios/commands/c15-route-update.md#c15-02) | The supplied tag list replaces the old list in order. |
| 3 | open-forge route inspect guidance/team/new-topic/notes | [C12-02](../scenarios/commands/c12-route-inspect.md#c12-02) | The source has the intended inherited scope and identity. |
| 4 | open-forge context guidance/team/new-topic/notes | [C08-02](../scenarios/commands/c08-context.md#c08-02) | The selected context respects the new chain. |
| 5 | open-forge route update guidance/team/new-topic/notes --tag=Guidance --tag=Team | [C15-06](../scenarios/commands/c15-route-update.md#c15-06) | The identical patch is a no-op. |

## Alternatives and recovery

### Parent missing at create

The target is creation through unambiguous missing intermediate scopes. If the current executable stops, record that gap, then run `open-forge route init guidance/team/new-topic` and retry create as a labelled recovery branch. An ambiguous parent or unknown root still needs clarification.

Scenarios: [C14-09](../scenarios/commands/c14-route-create.md#c14-09), [C13-01](../scenarios/commands/c13-route-init.md#c13-01).

### Framework scaffold without installation

Stop with a relevant installation action, not an implicit install.

Scenarios: [C13-08](../scenarios/commands/c13-route-init.md#c13-08).

## Final result

The scope and leaf are discoverable, metadata changes are intentional, and repetition does not churn files.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
