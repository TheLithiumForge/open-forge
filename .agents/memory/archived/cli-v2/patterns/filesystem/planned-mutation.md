---
open-forge:
  description: "Historical CLI-v2 source: Run every CLI mutation through complete effect planning, read-only preflight, revalidation, application, verification, and recovery"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Planned CLI Mutation

## Boundary

The accepted [Mutation Execution contract](../../../../memory/crystallized/documents/cli/contracts/mutation-execution.md) is authoritative for request, plan, effect, preflight, application, verification, recovery, preview, and public-evidence semantics. The [Filesystem Effects](../../../../memory/crystallized/documents/cli/contracts/filesystem-effects.md) and [Workspace Recovery](../../../../memory/crystallized/documents/cli/contracts/workspace-recovery.md) contracts own their focused safety guarantees. This Pattern owns the reusable implementation composition and source shape.

Exact TypeScript declarations belong to production source under the [Contract Ownership](../../typescript/contract-ownership.md) Pattern.

## Shape

Every mutating leaf composes the same visible stages:

```text
complete typed request
  -> inspect operation facts
  -> plan every persistent effect
  -> preflight the complete plan
  -> present preview or confirmation
  -> revalidate
  -> apply effects in order
  -> verify effects and operation
  -> recover applied effects when required
  -> run bounded post-processing
  -> typed result
```

Use the [Typed Transition Pipeline](../../typescript/typed-transition-pipeline.md) Pattern so each stage receives only its accepted predecessor and returns only its real typed choices. Dispatch effect data through exhaustive branches to directly imported executors. Do not resolve stages or effects through string-keyed registries.

## Shared And Local Responsibilities

Keep mechanical mutation behavior at CLI mutation scope:

```text
src/cli/mutation/
  effect-kind.ts
  effect-target.ts
  mutation-plan.ts
  mutation-state.ts
  preflighted-plan.ts
  preflight-plan.ts
  apply-plan.ts
  dispatch-effect.ts
  effects/
  recovery/
```

This is a responsibility map, not a requirement to create empty files. Focused modules and demonstrated reuse determine the final split.

Keep operation meaning beside the leaf:

```text
src/cli/commands/route/rebuild/
  rebuild.ts
  inspect-rebuild.ts
  plan-rebuild.ts
  verify-rebuild.ts
  render-rebuild.ts
  rebuild.test.ts
  rebuild.integration.test.ts
```

The operation owns intent, fact inspection, operation-specific eligibility, effect ordering, and complete-success verification. Shared mutation modules own accepted effect mechanics and transitions. A leaf does not hide operation meaning inside a callback-configured universal operation engine.

## Plan And Preflight Composition

The operation planner receives immutable facts and returns ordinary readonly plan data or one typed blocked outcome. It projects all operation-owned persistent effects before application and computes dependent effects from projected state rather than discovering them while writing.

Preflight consumes a raw plan and returns a construction-gated preflighted value or one typed blocked outcome. Application accepts only the preflighted value. Revalidation calls the same focused target and policy checks with fresh observations immediately before their accepted use.

Resolve mutable targets through the [Contained Filesystem Target](contained-filesystem-target.md) Pattern. Lifecycle operations compose the [Managed Reconciliation](managed-reconciliation.md) Pattern. External sources compose the [Reviewed Source Boundary](../commands/reviewed-source-boundary.md). Formatter post-processing composes the [Workspace Formatter Strategy](../workspace/workspace-formatter-strategy.md).

## Application Evidence

Keep application and recovery material private to execution. Record each applied transition in order so verification and reverse recovery consume concrete state rather than reconstructing it from the requested plan.

Project public mutation evidence only after a complete plan exists. The operation result composes the shared projection from one ordered effect collection; renderers derive summaries from that collection rather than maintaining parallel counters or state arrays.

Human and structured presentation use the [Result And Display Boundary](../commands/result-display-boundary.md). Exact effect fields, states, preview behavior, recovery guarantees, and authority remain linked to their CurrentTruth contracts instead of being copied here.

## Review Checks

- Every stage is a direct typed transition visible from the coordinator.
- The operation planner produces every intended persistent effect before application.
- Preflight returns a distinct construction-gated value, and application cannot accept a raw plan.
- Revalidation reuses focused inspectors rather than trusting old observations.
- Effect dispatch is exhaustive and directly navigable.
- Operation-specific intent stays beside the leaf while mechanical effect behavior stays shared.
- Applied-state recording supports verification and reverse recovery without leaking private material into results.
- Public summaries derive from one ordered effect projection.
- Specialized containment, lifecycle, source-review, formatting, display, and recovery concerns compose through their focused Patterns and contracts.
