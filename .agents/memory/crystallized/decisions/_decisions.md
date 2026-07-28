---
open-forge:
  description: Accepted rationale that explains important choices and their consequences
  tags: [LoadNow, Memory, Decision, Rationale, CurrentTruth]
---

# Decisions

Decisions are accepted rationale for important choices that may need to be understood later.

## Axioms

- Read `Entries` when current work needs rationale for an important choice
- Use the repository-only [knowledge-role helper](../documents/maintenance/helpers/knowledge-roles.md) when placement is unclear; user-facing descriptions and contents must remain understandable without it
- Decisions preserve why a choice was made and useful historical rationale; the chosen behavior, record, route, or external state is expressed by its authoritative route or system
- Keep one coherent choice or tightly coupled decision cluster in each record. Split unrelated choices, consolidate compatible overlap, and move exact current specifications to the sources that implement or document them.
- Consolidate, reshape, or link overlapping Decisions when their accepted rationale is compatible, and archive or link rationale behind a replaced choice; surface material divergence or competing accepted rationale for discussion instead of merging it silently
- Keep alternatives, tradeoffs, constraints, and consequences only when they help future work

## Entries

<!-- open-forge:generated-index:start -->
- [Open Forge states role and authority relationships directly, names semantic authority by source type, and reserves ownership for possession or managed lifecycle](authoritative-source-terminology.md) - #Memory #Decision #CurrentTruth #Terminology #Authority #Documentation
- [Benchmark design separates stable tasks, reusable inputs, exact recipes, and durable evidence while limiting review claims to observable traces](benchmark-design.md) - #Memory #Decision #CurrentTruth #Benchmark #Dogfood #Evaluation
- [Open Forge uses one canonical authoring form wherever Markdown carries Framework meaning while treating compatibility syntax as input-only](canonical-markdown.md) - #Memory #Decision #CurrentTruth #Framework #Markdown #Authoring #Syntax #Compatibility
- [Core uses distinct reusable content roles instead of one generic knowledge bucket, and every new primitive must earn nonduplicative semantics](core-primitives.md) - #Memory #Decision #CurrentTruth #Core #Primitive
- [Extensions deliver optional whole files through existing routes, and installed files carry complete runtime meaning without package metadata or the CLI](extension-package-boundary.md) - #Memory #Decision #CurrentTruth #Extension #Package #RuntimeBoundary
- [Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from](loading-reliability.md) - #Memory #Decision #CurrentTruth #Loading #Routing #Reliability
- [Memory preserves working, emerging, crystallized, and archived state without activating behavior it describes](memory-model.md) - #Memory #Decision #CurrentTruth #MemoryModel
- [Open Forge stays markdown-first, user-directed, small by default, and optimized for recursive customization](product-direction.md) - #Memory #Decision #CurrentTruth #Product
- [Small recursive `entrypoints` preserve local scope while one recognized `entrypoint` and explicit `root routes` remove routing ambiguity](routing-model.md) - #Memory #Decision #CurrentTruth #Routing
- [Entry descriptions support pre-load selection while routed bodies provide complete role-specific meaning](routing-surfaces.md) - #Memory #Decision #CurrentTruth #Routing #Formatting
- [Accepted universal scoping rules, root boundary, `managed route` relationship, and concrete `slug` behavior](scope-and-slugs.md) - #Memory #Decision #CurrentTruth #Routing #Scope
- [Users receive `src/open-forge/` as the payload while repository-only Maintenance contracts govern reviewed source without becoming hidden runtime context](source-and-packaging.md) - #Memory #Decision #CurrentTruth #Packaging #Governance
- [Open Forge separates tags for loading, Framework composition, truth status, synchronization, and ordinary classification](tags.md) - #Memory #Decision #CurrentTruth #Tags #Routing
- [Templates are a distinct Core primitive for copy-ready source artifacts whose ownership transfers to independently maintained results](template-primitive.md) - #Memory #Decision #CurrentTruth #Core #Template #Primitive
- [User-facing Open Forge files use positive natural language and compact selection surfaces](user-facing-writing.md) - #Memory #Decision #CurrentTruth #Formatting #Documentation #Routing
- [Workflows use one predictable goal-oriented recipe shape while phases remain non-waterfall wayfinding and dependencies stay distinct from containment](workflow-shape.md) - #Memory #Decision #CurrentTruth #Workflow #Routing #Orchestration
<!-- open-forge:generated-index:end -->
