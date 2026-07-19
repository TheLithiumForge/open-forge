---
open-forge:
  description: Reusable structural patterns for changing and operating Open Forge without losing its minimal routed design
  tags: [Pattern, Framework, Dogfood]
---

# Open Forge Patterns

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- `continuity-checkpoints.md` - Preserve goals, decisions, unresolved ideas, and next actions across long sessions, phase changes, handoffs, and context restoration - #Pattern #Memory #KeepInMind #Continuity #ContextRestoration #LongRunning
- `phase-aware-workflow-routing.md` - Infer the current development phase and recommend the closest workflow plus an earlier prerequisite only when current truth is insufficient - #Pattern #Workflow #Routing #DevelopmentPhase #Minimalism
- `route-scoped-directives.md` - Decide directive scope from the route before loading it, then treat every loaded directive as binding - #Pattern #Directive #Routing #Scope #Minimalism
- `tiered-test-evidence.md` - Keep this repository's default test feedback pure and fast while preserving explicit closure evidence for real OS, Git, CLI, packaging, and benchmark boundaries - #Pattern #Framework #Testing #Unit #Closure #CI #CLI #Evidence
<!-- open-forge:generated-index:end -->
