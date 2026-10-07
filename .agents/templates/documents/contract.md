---
open-forge:
  description: Contract template is used when one current document must define observable and testable guarantees at a product, API, protocol, or behavioral boundary
  tags: [Extension, Template, Document, Contract, CurrentView]
---

# {Boundary} Contract

{
Use for stable observable obligations: what may a relying party expect at this boundary, and what proves conformance? Keep implementation structure, mandatory work behavior, reusable shapes, and rationale in their authoritative sources.
Treat the sections below as a responsibility checklist rather than a mandatory schema. Rename, merge, reorder, or remove headings to fit the subject, and use descriptive domain-specific headings when they make the guarantees clearer.
State each guarantee once with its necessary conditions. Replace frontmatter, title, and prompts, then remove this guidance and unused optional sections.
}

## Scope

{Identify the boundary, the question for which this document is authoritative, the implementation or provider that must conform, and the consumers that may rely on it. Link to narrower authoritative contracts instead of duplicating them.}

## Guarantees

{State observable, testable obligations and invariant relationships. Organize distinct concerns under descriptive headings rather than keeping a long undifferentiated list. Leave implementation choices open unless they are themselves part of the contract.}

## Boundaries

{State important exclusions, unsupported states, and behavior consumers must not infer. Keep negative constraints only when they close a real ambiguity or safety risk.}

## Compatibility And Evolution

{OPTIONAL: When persisted data, external consumers, protocols, or compatibility promises make change semantics material, define compatible changes and those needing a migration or compatibility decision.}

## Verification

{State the evidence that proves conformance at the appropriate boundaries. Explain what direct, integration, process-level, review, or release evidence demonstrates without duplicating test implementation.}

## Related Current Sources

{Link only to authoritative sources that complete this boundary, and state the distinct question each source answers when the relationship is not obvious.}
