---
open-forge:
  description: Route the strip contracts that turn routed documents back into plain Markdown, shared by the strip command and uninstall
  responsibility: Route strip's public Interface and Behavior without adding uninstall or removal policy
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Uninstall, Mutation, CurrentTruth]
---

# Strip Command Contract Set

`strip` removes Open Forge metadata, generated `Entries` and links between documents, and can flatten routed documents into one folder. [`uninstall`](../uninstall/_uninstall.md) offers the same parts through `--strip` and `--flatten`. These contracts define the parts once for both commands.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Plan, revalidate, apply and verify one deterministic strip plan with recovery data for interrupted writes](behavior.md) - #Memory #Crystallized #CLI #Command #Contract #Strip #Behavior #CurrentTruth
- [Select which Open Forge parts to strip from routed documents and where flattened documents go](interface.md) - #Memory #Crystallized #CLI #Command #Contract #Strip #Interface #CurrentTruth
