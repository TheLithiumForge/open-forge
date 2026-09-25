---
open-forge:
  description: "Copy a Template and keep the new document independent"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F04: Copy a Template and keep the new document independent

**Who:** A maintainer starting a reusable scenario or project note.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

TEMPLATE with a routed templates/example source and a valid memory/working parent.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge route create memory/working/new-note --description "Scenario for adding team notes" --tag=Scenario --template templates/example | [C14-02](../scenarios/commands/c14-route-create.md#c14-02) | The new source has destination metadata and copied starting body. |
| 2 | Fill the new document placeholders and remove irrelevant sections. | [X10](../scenarios/experience.md#x10) | The copied starting file becomes a useful independently maintained document. |
| 3 | Edit only templates/example.md. | [X10](../scenarios/experience.md#x10) | The original Template changes; the copied document does not. |
| 4 | open-forge route update memory/working/new-note --description "Accepted review scenario" --template templates/example | [C15-05](../scenarios/commands/c15-route-update.md#c15-05) | Allowed metadata changes, but existing authored body is kept with the appropriate warning. |

## Alternatives and recovery

### Missing Template

Explain the missing reference before creating anything.

Scenarios: [C14-11](../scenarios/commands/c14-route-create.md#c14-11).

### Destination already differs

Preserve the existing document; do not pretend create is overwrite.

Scenarios: [C14-10](../scenarios/commands/c14-route-create.md#c14-10).

## Final result

The copied document owns its own metadata and content; the Template remains reusable.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
