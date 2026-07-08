---
open-forge:
  description: Accepted rationale that explains important choices and their consequences
  tags: [OpenForge, Memory, Decision, Rationale, Index, CurrentTruth]
---

# Decisions

Decisions are accepted rationale for important choices that may need to be understood later.

## Axioms

- This `entrypoint` exposes accepted decision records and routes to them.
- Read `Entries` when current work needs rationale for an important choice.
- Treat decisions as current memory within their stated scope unless superseded.
- Decisions explain why a choice was made; the chosen behavior, record, route, or external state belongs to its owning route or system.
- Extract resulting #Core material to matching #Core routes instead of keeping it as decision rationale.
- Keep alternatives, tradeoffs, constraints, and consequences only when they help future work.
- Avoid duplicate rationale; update, split, merge, or link existing decision routes instead.
- Archive or link superseded decisions with enough context to understand what replaced them.
- Load only the decision bodies and child categories relevant to the current request.
- Add child categories when they improve routing, ownership, or clarity.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- `core-primitives.md` - The Core layer installs directives, patterns, guidance, skills, workflows, and workspace with their accepted meanings - #Memory #Decision #CurrentTruth #Core #Primitive
- `do-not-revive.md` - Rejected structures that must not return without a new explicit decision - #Memory #Decision #CurrentTruth #Rejected
- `extensions-and-cli.md` - Extensions add optional routed files into the existing tree; the extend command is a dogfooding MVP; installed files remain runtime truth - #Memory #Decision #CurrentTruth #Extension #CLI
- `loading-reliability.md` - Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from - #Memory #Decision #CurrentTruth #Loading #Routing #Reliability
- `memory-model.md` - Memory is self-growing markdown state with working, emerging, crystallized, and archived states; it records state and never owns behavior - #Memory #Decision #CurrentTruth #MemoryModel
- `product-direction.md` - Open Forge stays markdown-first, human-led, small by default, and optimized for recursive customization - #Memory #Decision #CurrentTruth #Product
- `routing-model.md` - Routing goes through small markdown entrypoints; one recognized entrypoint per folder; the loader exposes only direct root routes - #Memory #Decision #CurrentTruth #Routing
- `routing-surfaces.md` - The entry description is the selection surface; the routed body is the execution recipe - #Memory #Decision #CurrentTruth #Routing #Formatting
- `scope-and-slugs.md` - Framework routes, scope routes, scoped framework routes, and slugs are distinct; placeholders are notation only and slug placement changes meaning - #Memory #Decision #CurrentTruth #Routing #Scope
- `source-and-packaging.md` - Users receive src/open-forge as the payload; docs/framework governs maintainers and is never hidden runtime context - #Memory #Decision #CurrentTruth #Packaging #Governance
- `tags.md` - Accepted tag semantics - load-policy tags create visibility only, layer and route type tags classify, tags stay bare and parseable - #Memory #Decision #CurrentTruth #Tags #Routing
<!-- open-forge:generated-index:end -->
