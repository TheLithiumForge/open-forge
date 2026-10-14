---
open-forge:
  description: "Reviewed scenario collections with explicit selection and target changes"
  tags: [Memory, Document, CLI, Scenario]
---

# Scenarios

## Reading The Cases

Each stable ID identifies one reviewed source case. The shared actor and goal apply unless a case narrows them. Describe expected behavior before executing it, and keep real state checks separate from text snapshots. Multiple named snapshots can capture different steps or views of one test. Use direct assertions for meaning and effects, not arbitrary strings.

Added cases retain a useful user outcome; improved cases state the revised target explicitly. Neither status claims the current executable passes. Deferred and rejected IDs remain short traceability stubs, not selected specifications. The source pack's proposed output catalogue and contract rewrite were not adopted.

Flow validation is required before new test implementation. Current command contracts define the exact existing interface, while these cases expose desired differences for reconciliation. A missing fixture produces not-run or fixture-unavailable, never a product pass.

## Task 79 coverage

Task 79 changes X02 to X05 and X16 and adds X32 to X50 for completion, identity evidence, safety and batch continuation. Command cases C05-13 to C05-22, C02-13 to C02-20 and C06-14 to C06-24 support changed F03, F05 and F08 and new F29 to F31. These specify current command behavior. The Task record retains accepted form and folder-rename decisions and historical outputs.

## Entries

- [Reviewed command-local scenario collections](commands/_commands.md) - #Memory #Document #CLI #Scenario
- [Cross-command experience scenarios](experience.md) - #Memory #Document #CLI #Scenario #Evergreen
