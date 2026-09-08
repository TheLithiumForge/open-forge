---
open-forge:
  description: Starting structure for what a source must keep true and how maintainers verify it
  tags: [Extension, Template, Document, Maintenance, Governance, CurrentView]
---

# {Surface} Maintenance Contract

{
Template selection:

- Need: One current document that states what a source or repository surface must keep true and how to verify it.
- Primary question: What must remain true when this surface changes, and how can maintainers verify that it still does?

Use this Template only when a source has stable maintenance requirements that deserve their own current document.
Keep continuing shape and review rules in the sources that define them. This Template provides only starting content.
Link to runtime sources and true counterparts instead of copying them.
Place each relationship beside the requirement or check it affects.
Replace the frontmatter, title, and placeholders, then remove this braced guidance.
Add an optional frontmatter responsibility when one stable sentence will help keep future maintenance concerns in this document.
}

## Source

{Link to the exact source, state what it defines, and identify counterparts that must stay aligned.}

## Contract

{State the smallest complete set of conditions, boundaries, and change requirements that maintainers must keep true.}

### {Distinct Concern}

{Use an optional level-3 subsection only for a clearly separate concern such as installation, generation, loading, scope, integration, or an external contract.}

## Verification

{Link to automated evidence and state any necessary manual checks. Explain what each check proves instead of repeating the requirements.}
