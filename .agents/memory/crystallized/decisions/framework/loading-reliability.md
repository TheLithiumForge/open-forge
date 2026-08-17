---
open-forge:
  description: Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from
  tags: [Memory, Decision, CurrentTruth, Loading, Routing, Reliability]
---

# Loading Reliability

## Context

Dogfood evaluations repeatedly showed that agents skip context when discovery is conditional, deferred, expensive, or phrased as if a tool will enforce it. Late instructions were especially likely to be missed.

The Framework needed reliable baseline and continuity context without loading every conditional route or pretending that nondeterministic compliance is mechanically guaranteed.

## Decision

Reliability-critical context is read unconditionally and early. Conditional context remains cheap to select, cheap to skip, and cheap to recover from.

Route selection establishes scope before routed content is read. Directives use ordinary #LoadNow traversal after their route establishes scope, without a second applicability gate inside each directive. Continuity makes #KeepInMind entrypoint proactivity target-sensitive while keeping ordinary routed #KeepInMind records globally discoverable; each applicable result follows its visible #LoadNow closure.

Loading instructions address the agent directly. Deterministic tools may batch the same plain-file traversal, but they do not create a separate loading contract or enforce reasoning.

## Rationale

Unconditional root directive loading was the only evaluated mechanism with full compliance across every measured session. Early follow-up loading also improved late actions such as closeout writes, while deferring context until it appeared relevant produced recurring misses.

Visible paths, descriptions, tags, ancestor meaning, and relative links make conditional selection inexpensive without requiring a hidden relevance engine. Reusing one traversal model reduces interpretation overhead.

## Alternatives And Tradeoffs

- Loading everything would reduce discovery risk but destroy relevance-scaled context
- Deferring critical context until an agent judges it relevant reproduces the observed failure mode
- A directive-body `Applies To` gate would ask for a second applicability decision after routing already established scope
- Globally loading every #KeepInMind entrypoint would defeat relevance-scaled selection, while hiding ordinary records behind route-body discovery would create continuity gaps
- Tool-implying phrases such as “is loaded” would overstate mechanical enforcement

Unconditional context has a permanent attention cost, so only reliability-critical material should receive it.

## Consequences

- Baseline directives and continuity entrypoints must remain compact
- Broken route chains are repaired rather than used to weaken the loading contract
- Conditional route descriptions must expose enough purpose or outcome for pre-load selection
- Deterministic CLI assistance remains an optional accelerator over the complete manual traversal; the frozen legacy command is `open-forge-old load --bodies`, while the new interface remains unsettled
- Changes to loading behavior require proportionate dogfood or behavioral evidence

## Authoritative Sources

- [Open Forge loader](../../../../loader.md)
- [Current routing loading contract](../../documents/framework/routing/loading.md)
- [Current Directive contract](../../documents/framework/primitives/directives.md)
- [Loader Maintenance contract](../../documents/maintenance/payload/agents/loader.md)

## Evidence

- [Accepted evaluation syntheses](../../documents/evaluations/_evaluations.md)

## Decision Relationships

- [Routing model](routing-model.md)
- [Routing surfaces](routing-surfaces.md)
- [Tag semantics](tags.md)
