---
open-forge:
  description: Explore whether Decisions need explicit approval and replacement metadata without turning rationale records into a second lifecycle system
  tags: [Memory, Idea, Contextual, Candidate, Decision, Lifecycle, Metadata, History]
---

# Decision Lifecycle Metadata

## Opportunity

Decisions preserve accepted choices and useful rationale. Explicit lifecycle metadata could make approval, replacement, consolidation, and historical navigation easier to inspect, but unnecessary fields would add ceremony to every Decision.

The existing Memory state, current-view, and archive contracts already define whether a Decision is current or historical. Any metadata should clarify that lifecycle rather than compete with it.

## Candidate Metadata

- Approval date and time
- Replacement date and time
- A link to the replacing Decision or current source
- A relationship for partial replacement, consolidation, or several replacement Decisions

## Questions

- Does lifecycle metadata belong in frontmatter, the document body, or both?
- Which timestamp precision and timezone remain useful and deterministic?
- Does replacement always move a Decision to Archived, or can part of its rationale remain current within a narrower scope?
- Should archival be automatic, proposed, or explicitly selected?
- How should partial replacement, consolidation, and multiple replacements remain understandable?
- Which metadata can tools validate without making tools authoritative for Decision meaning?
- Does the benefit justify changing the Decision Template and every installed Decision contract?

## Promotion Signals

Promote a metadata contract only when repeated Decision replacement or consolidation makes approval state or historical relationships difficult to understand from the existing `route`, content, and links.

Prefer the smallest representation that answers the recurring problem. Do not require timestamps merely because they are easy to generate.

## Related Current Sources

- [Decisions entrypoint](../../crystallized/decisions/_decisions.md)
- [Accepted state and synchronization](../../crystallized/documents/framework/truth.md)
- [Memory transitions](../../crystallized/documents/framework/memory/transitions.md)
- [Decision Template](../../../templates/memory/decision.md)
