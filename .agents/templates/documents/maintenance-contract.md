---
open-forge:
  description: Maintenance contract template is used when one current document must define what a source or repository surface must preserve and how maintainers verify it
  tags: [Template, Document, Maintenance, Governance, CurrentView]
---

# {Surface} Maintenance Contract

<!--
Template selection:
- Need: One current maintainer-facing document for a source or repository surface's contract and verification boundary.
- Primary question: What must remain true when this surface changes, and how can maintainers verify that it still does?

Use this template only when a source or repository surface has stable maintenance obligations that deserve an independent current document.
The continuing document shape and review expectations belong to the [maintenance contract pattern](../../patterns/open-forge/maintenance-contract.md).
Link to authoritative runtime sources and true counterparts instead of copying their complete contents.
Place relationships beside the contract or verification statement they affect.
Replace this template's frontmatter, title, placeholders, and comments.
-->

## Source

{Link to the canonical source, state the responsibility for which it is authoritative, and identify true counterparts whose alignment creates a maintenance obligation.}

## Contract

{State the smallest complete set of maintainer-level invariants, boundaries, and change obligations.}

### {Distinct Concern}

{Use an optional level-3 subsection only when it clarifies a genuinely distinct concern such as installation, generation, loading, scope, integration, or an external contract.}

## Verification

{Link to automated evidence and state any necessary manual checks. Explain what each check proves rather than repeating the contract.}
