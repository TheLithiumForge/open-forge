---
open-forge:
  description: Initial blind-reader probe found no factual failures across ten known-answer Framework questions and identified five discovery-cost gaps
  tags: [Memory, Analysis, Contextual, Candidate, Documentation, Evaluation, Comprehension, Agent, Evidence]
---

# Initial Documentation Comprehension Probe

## Question

Can an unfamiliar agent derive the intended Open Forge mental model from `.agents` alone, without design conversation, implementation, tests, public documentation, or repository history?

## Method

One isolated agent received no conversation history and was restricted to files under `.agents`. It began at `.agents/loader.md`, followed the documented routes, and answered ten known-answer questions covering:

1. Product identity and non-goals
2. User direction and agent autonomy
3. Loading order, #LoadNow, and #KeepInMind
4. Human-readable meaning and deterministic tooling
5. The seven Core primitives
6. The four Memory states and transition model
7. Scoped Decisions and Documents
8. Tentative versus accepted direction
9. Current documents, Decisions, Archives, and supersession
10. Multi-project and multi-repository scaling and integration

For every question, the reader reported its answer, confidence, sources used, ambiguity, and discovery difficulty. It was explicitly told that finding a statement somewhere did not by itself prove good communication.

## Results

The reader produced no factual failure.

| Question | Result | Confidence | Main comprehension cost |
|---|---|---|---|
| Identity and non-goals | Correct | High | Vision is several route levels below the loader |
| Direction and autonomy | Correct | High | Full direct-application boundary is deeper than the loader |
| Loading | Correct | High | `complete routed set` may be mistaken for every descendant |
| Tool boundary | Correct | High | No material ambiguity |
| Core primitives | Correct | High | Core also contains routing, which could be mistaken for an eighth primitive |
| Memory states | Correct | High | Per-state capture thresholds require several focused documents |
| Scoped Decisions and Documents | Correct | High | Exact paths required the deepest targeted traversal |
| Acceptance examples | Correct | High | Exceptionally clear once the accepted-state document is found |
| Current truth, rationale, and history | Correct | High | Safe supersession spans several related contracts |
| Multiple scopes and repositories | Correct | Medium | Integration behavior is distributed and partly inferential |

## Strong Communication

- Vision, ACE, and non-goals form one coherent product explanation
- The baseline authority rule is clear enough to support ordinary autonomy without repeated confirmation
- The human-readable and deterministic-tool boundary is consistent across the Principles, top architecture, Framework architecture, and loader
- The seven primitive roles and the four Memory states are distinct once their focused models are selected
- The accepted-state examples make tentative, accepted, and experimental direction unusually concrete
- The current document, Decision, and Archive distinction is conceptually strong

## Discovery And Explanation Gaps

### Scoped Decisions And Documents

The reader answered correctly because [Crystallized Memory](../../crystallized/documents/framework/memory/crystallized.md#scoped-decisions-and-documents) now contains explicit paths. It still ranked this as the hardest exact answer because neither the root Crystallized runtime entrypoint nor the Decisions and Documents entrypoints expose the recursive placement capability.

This is a locality question rather than missing architecture. The general scope model is correct, but a reader should not need to infer every important permitted shape from it.

### Multi-Scope Integration

Support for many projects and repositories is clear. What happens when work deliberately combines two scopes is less complete.

The reader could infer that both branches and their explicit relationships are selected, applicable ancestors remain active, destination authority remains local, and unresolved conflicts are reported. It could not find one concise integration example or a direct statement that no separate scope-merge mechanism exists.

### #KeepInMind Breadth

The loading contract correctly distinguishes global continuity from selected branches. The phrase `complete routed set` can initially sound like every descendant body must be read.

The intended meaning is the complete effective catalogue of routed #KeepInMind results, together with the visible #LoadNow closure each result exposes, rather than indiscriminate traversal of every descendant.

### Acceptance Discoverability

The exact `Consider architecture B` examples are clear but live in a deep conceptual document. This may be appropriate because the loader already carries the compact acceptance rule, but later probes should test whether an installed-source-only reader can apply the distinction without repository conceptual documents.

### Supersession Distribution

The source-role distinction is easy to state. Applying supersession safely requires the current-view, accepted-state, transition, and Archive contracts together.

This may be appropriate separation rather than a defect. A future probe should test a concrete supersession scenario instead of asking only for the conceptual distinction.

## Mechanical Finding

The new [documentation comprehension probe idea](../ideas/documentation-comprehension-probes.md) was initially absent from generated Ideas entries because an index rebuild encountered a transient file-open failure while the blind reader was traversing `.agents`.

The missing route was detected by the reader and repaired after the isolated pass. This demonstrates that comprehension probes can also reveal practical navigation failures, although structural validation remains the authoritative mechanism for detecting them deterministically.

## Current Interpretation

The initial result supports the current architecture. Ten correct answers, nine at high confidence, indicate that the intended model is recoverable from `.agents` without private context.

The result does not prove that the documentation is cheap enough to use. The strongest next improvements concern locality and explicit composition, especially scoped Memory roles, multi-scope integration, and the exact breadth of continuity loading.

## Limits

- One capable agent is not a representative reader population
- The questions named the concepts being sought and therefore reduced route-selection difficulty
- The complete repository `.agents` surface includes maintainer conceptual documents that installed users do not receive
- The probe measured explanation and discovery, not runtime compliance or implementation correctness
- Confidence is self-reported and not a deterministic metric

## Next Experiments

- Repeat the same questions against only the installable `.agents` payload
- Test public onboarding with README plus installed files
- Ask scenario questions without naming the expected Framework concepts
- Repeat selected questions with several isolated readers and compare route paths as well as answers
- Establish a small answer rubric that separates factual correctness, discovery cost, unsupported inference, and confidence

## Related Sources

- [Documentation comprehension probe idea](../ideas/documentation-comprehension-probes.md)
- [Open Forge Vision](../../crystallized/documents/vision.md)
- [Framework Architecture](../../crystallized/documents/framework/architecture.md)
- [Routing loading and continuity](../../crystallized/documents/framework/routing/loading.md)
- [Memory model](../../crystallized/documents/framework/memory/model.md)
- [Accepted state and synchronization](../../crystallized/documents/framework/truth.md)
