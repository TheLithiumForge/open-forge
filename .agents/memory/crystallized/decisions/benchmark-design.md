---
open-forge:
  description: Benchmark design separates stable tasks, reusable inputs, exact recipes, and durable evidence while limiting review claims to observable traces
  tags: [Memory, Decision, CurrentTruth, Benchmark, Dogfood, Evaluation]
---

# Benchmark Design

## Context

Early Open Forge evaluations mixed tasks, Framework treatments, orchestration, review, and claims inside generation-specific fixtures. That made useful experiments difficult to reuse, exact comparisons difficult to reconstruct, and strong causal language easier to imply than the observable evidence justified.

The benchmark system needed to support practical dogfood across tasks, compositions, treatments, models, and runtimes without becoming a self-authenticating research platform.

## Decision

Open Forge benchmarks separate three stable concepts:

- A scenario is a framework-agnostic task
- A primitive block is one reusable worker-visible input
- A meta-scenario is an exact runnable recipe that selects one scenario, an ordered primitive set, and any bundled Extensions

Stable controls and traps are explicit meta-scenarios. Run-local variants add declared external primitives, bundled Extensions, or runtime surfaces without mutating the stable corpus. Every selected input is frozen before workers launch in fresh external workspaces.

An orchestrator is responsible for scheduling, runtime interaction, trace capture, independent review, and comparison. Worker self-review is preserved as evidence of awareness rather than proof of outcome. Claims remain bounded by the actual observable trace, workspace, delta, checks, and known runtime limits.

Completed run sets may produce append-only durable publications after no worker can be influenced by the result. The plain-file protocol remains understandable and reproducible without the benchmark CLI.

## Rationale

Separating tasks from treatments allows the same scenario and primitive to be reused without rewriting either. Exact meta-scenarios make composition differences inspectable, while on-demand variants preserve experimentation without silently changing stable fixtures.

Freezing prompts, composition, baselines, and review material before execution makes comparison inputs reviewable. Independent outcome review prevents worker confidence from substituting for evidence.

Keeping orchestration outside the worker workspace reduces leakage and preserves a clear trace boundary. Honest unknowns are more useful than reconstructed behavior the runtime did not expose.

## Alternatives And Tradeoffs

- Generation-specific monolithic scenarios make one run easy to describe but make components difficult to reuse or compare
- A second stable variant language would duplicate exact base and trap recipes
- Letting the runner schedule agents or provision provider-native tools would claim control over runtime surfaces it does not control
- Fixed rating ladders, eligibility seals, and causal-study language would add ceremony without proving isolation or causality
- Treating successful validation as behavioral proof would confuse structural checks with agent adherence

The composable design requires more explicit manifests, frozen inputs, and orchestration records. One run remains one observation rather than a general performance claim.

## Consequences

- Scenario, primitive, and meta-scenario reviews answer distinct questions
- Folder hierarchy organizes the corpus while manifest ids preserve identity
- File collisions fail visibly, while deliberate logical conflicts remain valid treatments
- Model comparisons freeze composition; treatment comparisons minimize and declare the composition delta
- Failed or partial runs may remain useful evidence when their state and limits are explicit
- Historical reports remain context rather than the current benchmark contract

## Authoritative Sources

- [Current benchmark system](../../../../benchmarks/)
- [Benchmark overview and operating contract](../../../../benchmarks/README.md)
- [Building-block contract](../../../../benchmarks/building-blocks/README.md)
- [Meta-scenario contract](../../../../benchmarks/meta-scenarios/README.md)
- [Harness contract](../../../../benchmarks/harness/README.md)
- [Current harness implementation](../../../../benchmarks/harness/runner.ts)

## Evidence

- [Accepted evaluation syntheses](../documents/evaluations/_evaluations.md)

## Decision Relationships

- [Product direction](product-direction.md)
- [Source and packaging](source-and-packaging.md)
