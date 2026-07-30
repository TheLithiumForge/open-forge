---
open-forge:
  description: Structure current maintenance documents around their source, maintainer contract, and verification
  tags: [Pattern, Framework, Maintenance, Governance, Documentation]
---

# Maintenance Contract

## Shape

The [governance separation directive](../../directives/deliberate-framework-change.md#axioms) is authoritative for the source-wording boundary.

The [maintenance contract template](../../templates/documents/maintenance-contract.md) provides copy-ready starting content. This pattern remains the continuing shape used when creating or reviewing an instantiated maintenance document.

Organize each current maintenance document as:

1. `Source` - link to the canonical source, state its responsibility, and identify true counterparts.
2. `Contract` - define the smallest complete set of maintainer-level invariants, boundaries, and change obligations.
3. `Verification` - link to the tests or checks that prove the contract remains aligned; state necessary manual verification when no automated check exists.

Add optional frontmatter `responsibility` when one stable sentence will help keep the document's maintenance concerns local. Treat it as a boundary for future edits, not as an authority grant or replacement for the `Source` section.

Place relative links beside the contract or verification statement they support. Include a relationship when it creates a maintenance consequence.

Use optional level-3 subsections inside `Contract` when the source has distinct concerns such as patching, generation, loading, scope, integration, or an external contract. Name each optional subsection after the actual concern and require it to clarify a distinct part of the contract.

State the valid shape as positively and completely as practical. Prefer a closed requirement such as “contains only A and B” over a list of forbidden examples that leaves unintended alternatives open. Use a negative constraint only when it closes a concrete risk more precisely than a positive boundary.

## Review Checks

- The `Source` section identifies the authoritative runtime source through a link and a concise responsibility.
- Every `Contract` statement narrows the valid design space or identifies a real change obligation.
- Relevant counterparts, dependencies, decisions, consumers, and external contracts are linked where they affect maintenance.
- Optional subsections clarify genuinely distinct concerns.
- Historical rationale stays in decisions or archives.
- `Verification` names observable evidence rather than repeating the contract.
