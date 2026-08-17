---
open-forge:
  description: Contract template is used when one current document must define observable and testable guarantees at a product, API, protocol, or behavioral boundary
  tags: [Extension, Template, Document, Contract, CurrentView]
---

# {Boundary} Contract

{
Template selection:

- Need: One current document for guarantees that a named implementation, provider, or consumer can satisfy or violate.
- Primary question: What may a relying party expect at this boundary, and what evidence proves conformance?

Use this Template only when the boundary has stable observable obligations. Keep implementation structure, mandatory work behavior, reusable shapes, and rationale in their authoritative sources.
Treat the sections below as a responsibility checklist rather than a mandatory schema. Rename, merge, reorder, or remove headings to fit the subject, and use descriptive domain-specific headings when they make the guarantees clearer.
Replace this Template's frontmatter, title, and placeholders, then remove this braced source guidance.
}

## Scope

{Identify the boundary, the question for which this document is authoritative, the implementation or provider that must conform, and the consumers that may rely on it. Link to narrower authoritative contracts instead of duplicating them.}

## Guarantees

{State observable, testable obligations and invariant relationships. Organize distinct concerns under descriptive headings rather than keeping a long undifferentiated list. Leave implementation choices open unless they are themselves part of the contract.}

## Boundaries

{State important exclusions, unsupported states, and behavior consumers must not infer. Keep negative constraints only when they close a real ambiguity or safety risk.}

## Compatibility And Evolution

{Keep this section only when persisted data, external consumers, versioned protocols, or compatibility promises make change semantics material. Define which changes preserve the contract and which require an explicit migration or compatibility decision.}

## Verification

{State the evidence that proves conformance at the appropriate boundaries. Explain what direct, integration, process-level, review, or release evidence demonstrates without duplicating test implementation.}

## Related Current Sources

{Link only to authoritative sources that complete this boundary, and state the distinct question each source answers when the relationship is not obvious.}
