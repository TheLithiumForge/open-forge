---
open-forge:
  description: Define one starting situation, the actions a person takes, and the result they should get
  tags: [Extension, Template, Scenario]
---

# {Scenario Name}

{Copy and adapt. State the actor, goal, starting state, actions, and expected result once each. Adapt headings as needed. Replace metadata and prompts, rebase links, and remove this guidance and unused optional sections.}

## Who

{The role, knowledge or constraint that changes this interaction. A fictional persona is unnecessary.}

## Goal

{What the person wants to accomplish.}

## Starting Point

{Reproducible relevant state, existing content, permissions and required inputs.
Name the fixture and any preparation that is not yet possible.}

## Steps

{Actions in order, including inputs and choices. Say where this scenario ends.}

## Expected Result

{What succeeds or why the person cannot continue. What changes, what stays unchanged,
and what they should understand. State the user outcome, not implementation steps.}

### What The Person Sees

{OPTIONAL: Add communication detail beyond the Expected Result, such as a supporting output rule or stable message identity. Bind variable values to this situation. Keep proposals distinct from current behavior.}

### Other Outputs

{OPTIONAL: Reference the format/detail-specific contract and meaningful parity or differences. An undefined variant is deferred, not assumed identical.}

## Supporting Contract

{Link the sources that support this expectation, with their revision when useful. State any intentionally different target and its acceptance status. Existing source references are optional when the user request itself establishes the outcome.}

## Verification

{Independent observations of effects, preservation and completion. Link actual run
records separately. No execution means not run. A matching message is not proof of a state change. Check quantities against independently known fixture facts, not only against the last captured output.}

## Open Questions

{OPTIONAL: Unresolved decisions that could change this expectation.}
