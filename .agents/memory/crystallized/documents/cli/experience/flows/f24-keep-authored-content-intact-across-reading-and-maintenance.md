---
open-forge:
  description: "Keep authored content intact across reading and maintenance"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F24: Keep authored content intact across reading and maintenance

**Who:** A maintainer using Unicode, code samples and mixed line endings.

**Execution:** See the separate [run record](../../../../../archived/beta-preparation/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

LITERAL-CONTENT with a valid source, distinctive body bytes and a parent list that needs rebuilding.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | Read the source through context with body and section projections. | [X23](../scenarios/experience.md#x23) | Authored content is available without global UI-word filtering or escape corruption. |
| 2 | Update only that source’s description. | [C15-01](../scenarios/commands/c15-route-update.md#c15-01) | The body and unrelated metadata survive. |
| 3 | Index its parent containing surrounding prose and a later authored list. | [X07](../scenarios/experience.md#x07) | Only the contract-defined managed list span changes. |
| 4 | Repeat the completed maintenance operation. | [X19](../scenarios/experience.md#x19) | The repeat is genuinely byte-identical where the operation promises no changes. |

## Alternatives and recovery

### Source ID or path contains spaces

Shown references and next commands retain exact identity with correct argv quoting.

Scenarios: [X15](../scenarios/experience.md#x15).

### No final newline

Do not silently add one during read-only output comparison or outside-span mutation.

Scenarios: [X23](../scenarios/experience.md#x23).

## Final result

Navigation and metadata can evolve without erasing or rewriting authored meaning.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
