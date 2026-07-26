---
open-forge:
  description: Accepted rationale that explains important choices and their consequences
  tags: [LoadNow, Memory, Decision, Rationale, CurrentTruth]
---

# Decisions

Decisions are accepted rationale for important choices that may need to be understood later.

## Axioms

- Read `Entries` when current work needs rationale for an important choice.
- During the repository migration, use the [temporary knowledge-owner question helper](../../working/knowledge-owner-helper.md) when placement is unclear; remove this repository-only helper after normal route descriptions and current contracts make selection sufficiently cheap.
- Decisions preserve why a choice was made and useful historical rationale; the chosen behavior, record, route, or external state belongs to its owning route or system.
- Keep alternatives, tradeoffs, constraints, and consequences only when they help future work.

## Entries

<!-- open-forge:generated-index:start -->
- [Benchmarks compose agnostic scenarios, reusable primitive blocks, and reproducible meta-scenarios under trace-reviewing orchestrators](benchmark-design.md) - #Memory #Decision #CurrentTruth #Benchmark #Dogfood #Evaluation
- [Core uses distinct reusable content roles instead of one generic knowledge bucket, and every new primitive must earn nonduplicative semantics](core-primitives.md) - #Memory #Decision #CurrentTruth #Core #Primitive
- [Do-not-revive list for rejected files, hidden mechanics, directive gates, continuity gaps, and workflow ceremony](do-not-revive.md) - #Memory #Decision #CurrentTruth #Rejected
- [Extensions add optional whole routed files while installed files remain complete runtime truth](extensions-and-cli.md) - #Memory #Decision #CurrentTruth #Extension #CLI
- [Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from](loading-reliability.md) - #Memory #Decision #CurrentTruth #Loading #Routing #Reliability
- [Memory preserves working, emerging, crystallized, and archived state without activating behavior it describes](memory-model.md) - #Memory #Decision #CurrentTruth #MemoryModel
- [Open Forge stays markdown-first, operator-led, small by default, and optimized for recursive customization](product-direction.md) - #Memory #Decision #CurrentTruth #Product
- [Routing goes through small markdown entrypoints; one recognized entrypoint per folder; the loader exposes only direct root routes](routing-model.md) - #Memory #Decision #CurrentTruth #Routing
- [The entry description is the selection surface; the routed body is the execution recipe](routing-surfaces.md) - #Memory #Decision #CurrentTruth #Routing #Formatting
- [Framework routes, scope routes, scoped framework routes, and slugs are distinct; placeholders are notation only and slug placement changes meaning](scope-and-slugs.md) - #Memory #Decision #CurrentTruth #Routing #Scope
- [Users receive src/open-forge as the payload; crystallized maintenance documents govern reviewed source without becoming hidden runtime context](source-and-packaging.md) - #Memory #Decision #CurrentTruth #Packaging #Governance
- [Accepted tag semantics for loading, layers, truth status, Evergreen synchronization, and ordinary classification](tags.md) - #Memory #Decision #CurrentTruth #Tags #Routing
- [Templates are a distinct Core primitive for copy-ready source artifacts whose ownership transfers to independently maintained results](template-primitive.md) - #Memory #Decision #CurrentTruth #Core #Template #Primitive
- [User-facing Open Forge files use positive natural language and compact selection surfaces](user-facing-writing.md) - #Memory #Decision #CurrentTruth #Formatting #Documentation #Routing
- [Accepted workflow shape - Mode, Goal, Required Routes, Constraints, Steps, Loop, Outputs, Completion; generated Entries are containment, Required Routes are cross-tree dependency](workflow-shape.md) - #Memory #Decision #CurrentTruth #Workflow #Routing #Orchestration
<!-- open-forge:generated-index:end -->
