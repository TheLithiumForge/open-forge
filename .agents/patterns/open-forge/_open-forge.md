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
- [Preserve goals, decisions, unresolved ideas, and next actions across long sessions, phase changes, handoffs, and context restoration](continuity-checkpoints.md) - #Pattern #Memory #KeepInMind #Continuity #ContextRestoration #LongRunning
- [Structure current maintenance documents around their source, maintainer contract, and verification](maintenance-contract.md) - #Pattern #Framework #Maintenance #Governance #Documentation
- [Keep canonical root instructions and harness bridges safely replaceable inside one workspace-owned file](managed-root-entry.md) - #Pattern #Framework #Entry #Bridge #Installation #Safety
- [Select workflows from visible routing signals and recommend helpful prior work once without blocking progress](phase-aware-workflow-routing.md) - #Pattern #Workflow #Routing #DevelopmentPhase #Minimalism
- [Decide directive scope from the route before loading it, then treat every loaded directive as binding](route-scoped-directives.md) - #Pattern #Directive #Routing #Scope #Minimalism
- [Keep this repository's default test feedback pure and fast while preserving explicit closure evidence for real OS, Git, CLI, packaging, and benchmark boundaries](tiered-test-evidence.md) - #Pattern #Framework #Testing #Unit #Closure #CI #CLI #Evidence
<!-- open-forge:generated-index:end -->
