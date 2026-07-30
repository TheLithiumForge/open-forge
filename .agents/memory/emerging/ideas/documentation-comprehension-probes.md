---
open-forge:
  description: Test Open Forge documentation with isolated known-answer questions that reveal whether unfamiliar agents can discover and understand intended behavior
  tags: [Memory, Idea, Contextual, Candidate, Documentation, Evaluation, Comprehension, Agent]
---

# Documentation Comprehension Probes

## Opportunity

Open Forge documentation can be internally correct while remaining difficult for a first-time reader to discover or apply. A blind comprehension probe can expose logical and navigational gaps that ordinary consistency reviews miss.

The initial trigger was a Crystallized Memory scope rule that existed in the general routing model but was not concrete where Decisions and Documents were explained. A reader without prior Framework knowledge could therefore miss valid paths such as `memory/crystallized/mobile-app/decisions/`.

## Current Understanding

A probe gives an isolated agent a deliberately bounded documentation surface and asks questions whose intended answers are already known.

The agent receives no design conversation, repository history, implementation, tests, or maintainer explanation beyond the selected surface. For each question it records:

- Its answer
- Confidence
- Sources it discovered
- Ambiguity, contradiction, or inference required

A wrong answer indicates missing or conflicting meaning. A correct answer that requires excessive search indicates a routing, locality, example, or explanation problem. Consistently cheap correct answers provide evidence that the documentation communicates its design.

This complements structural validation. It evaluates reader understanding rather than Markdown validity, route reachability, or implementation behavior.

## Possibilities

### Small Known-Answer Probe

Start with approximately ten questions covering identity, authority, loading, tooling boundaries, Core roles, Memory states, recursive scopes, acceptance, current truth, and multi-project scaling.

### Bounded Reader Surfaces

Run separate probes against surfaces such as:

- The installed `.agents` environment
- Public README plus installed files
- One selected conceptual scope with its routed dependencies
- One isolated runtime entrypoint and only the context it exposes

Each surface tests a different promise and should have its own answer key.

### Repeated Independent Readers

Use several isolated agents or models to distinguish one reader's mistake from a recurring documentation weakness. Compare both answer correctness and the routes each reader needed.

### Maintainer And CLI Assistance

If the method remains useful, maintain an internal question bank and answer rubric. Future CLI or evaluation tooling could assemble bounded contexts, collect answers, compare expected concepts, and report uncertain or conflicting interpretations without making model output a deterministic product guarantee.

## Evidence

- The [initial documentation comprehension probe](../../archived/analysis/2026-07-27_documentation-comprehension-probe.md) recovered all ten intended answers and exposed several discovery-cost gaps that later routing changes addressed
- The scoped Decisions and Documents rule was technically present in the routing architecture but needed concrete paths in the Crystallized state document before its intended use became obvious
- Current review already validates structure, links, source alignment, and implementation behavior, but none of those checks proves that an unfamiliar reader forms the intended mental model
- Open Forge explicitly depends on capable agents understanding routed human-readable files, so comprehension is part of the product behavior rather than only documentation polish

## Open Questions

- Which questions are stable enough to become a reusable baseline without freezing current implementation details?
- Where should internal answer keys and scoring guidance live?
- How should correctness, discovery cost, confidence, and unsupported inference be scored separately?
- How many independent readers provide useful signal without creating excessive evaluation cost?
- How can documentation improve around repeated failures without becoming repetitive or overfitted to one test set?
- Which probes belong to the Framework, CLI, Extensions, public documentation, and installed-runtime boundaries?

## Promotion Signals

Promote this idea when repeated isolated probes identify actionable gaps that normal review missed and a stable small question set can be maintained without forcing one documentation shape.

A stronger evaluation contract becomes justified when the same method works across several bounded surfaces and produces comparable evidence over time.

## Related Records And Sources

- [Current evaluation syntheses](../../crystallized/documents/evaluations/_evaluations.md)
- [Knowledge role helper](../../crystallized/documents/maintenance/helpers/knowledge-roles.md)
- [Framework routing model](../../crystallized/documents/framework/routing/model.md)
- [Documentation quality roadmap](rating-ladder.md)
