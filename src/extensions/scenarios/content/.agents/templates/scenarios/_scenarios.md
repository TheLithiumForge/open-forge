---
open-forge:
  description: Plan user flows and expected outcomes, group scenarios, and record actual runs
  tags: [Extension, Template, UserFlow, Scenario, ScenarioCollection, RunRecord]
---

# Flow And Scenario Templates

## What should happen, and what happened when it was tried?

Use these starters to plan an experience and check it.

`User Flow` connects scenarios. `Scenario` defines an expected outcome. `Scenario Collection` groups cases, and `Run Record` preserves actual observations. A collection does not automatically define an ordered flow.

Use these starters for products, services, or other work. They do not require Planning, Development, or an execution plan.

## Axioms

- Specify expected results before recording actual results.
- Keep proposed expectations distinct from accepted requirements and observed behavior.
- Connect flow steps through their real resulting state. Do not silently reset the starting conditions between steps.
- Keep missing inputs and unrun checks explicit.

## Entries

- [Record what happened during one run without replacing the intended expectation](run-record.md) - #Extension #Template #Scenario #RunRecord
- [Group related scenarios without creating another copy of their contracts](scenario-collection.md) - #Extension #Template #ScenarioCollection
- [Define one starting situation, the actions a person takes, and the result they should get](scenario.md) - #Extension #Template #Scenario
- [Connect scenarios from a person's starting state to a meaningful goal](user-flow.md) - #Extension #Template #UserFlow
