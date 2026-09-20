---
open-forge:
  description: "Find the relevant guidance without loading everything"
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F07: Find the relevant guidance without loading everything

**Who:** A caller searching by tags and headings before reading full content.

**Execution:** See the separate [run record](../../../../../working/cli-experience-run.md). Inclusion records a reviewed user outcome, not a passing implementation.

## Starting point

SEARCH with independently known tag sets, headings, included scopes and decoys.

## Flow

Run the steps on the actual resulting state. A linked scenario supplies a verification boundary, not permission to reset the workspace between steps.

| Step | Action | Scenario | State passed forward |
| ---: | --- | --- | --- |
| 1 | open-forge route list --depth=all | [C11-03](../scenarios/commands/c11-route-list.md#c11-03) | The available source IDs and scope boundaries are visible. |
| 2 | open-forge find --tag=Decision --tag=Architecture | [C09-03](../scenarios/commands/c09-find.md#c09-03) | The intersection is returned, not the union. |
| 3 | open-forge find --tag=Decision --content=headings | [C09-06](../scenarios/commands/c09-find.md#c09-06) | Requested headings add useful content without changing the tag match set. |
| 4 | open-forge find --tag=Decision --include=guidance | [C09-07](../scenarios/commands/c09-find.md#c09-07) | The effective universe is explicit and bounded. |
| 5 | Run open-forge context with one exact source returned by find. | [C08-02](../scenarios/commands/c08-context.md#c08-02) | The selected result can actually be read using the displayed identity. |

## Alternatives and recovery

### Complete no match

Say none matched the query; do not imply an unavailable scan.

Scenarios: [C09-05](../scenarios/commands/c09-find.md#c09-05).

### One unreadable eligible source

Keep known matches and incomplete coverage.

Scenarios: [C09-09](../scenarios/commands/c09-find.md#c09-09).

### Any rather than all

Additional query matrix: use --require=any on an identical copy and compare the independently computed union; this is an added variant, not the all scenario repeated.

Scenarios: [X28](../scenarios/experience.md#x28).

## Final result

The caller can locate and read the intended material with an honest search boundary.

## Verification

Observe the resulting files and links independently of printed output. Record actions, stdout, stderr, exit, preserved bytes, and separate outcome/state/communication verdicts in a run record. Carry real state between steps; fork only at an explicit alternative.
