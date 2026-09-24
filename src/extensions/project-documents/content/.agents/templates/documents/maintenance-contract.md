---
open-forge:
  description: "State what a maintained source owns, what must remain true, and how to verify it"
  tags: [Extension, Template, Document, Maintenance, Governance]
---

# {Surface} Maintenance Contract

{
Use when a source has stable maintenance obligations worth defining separately. Do not invent a governance layer for a trivial file.
Replace {prompts}; remove this source guidance and sections that add no value.
Set metadata for the destination, not this Template. Rebase links after copying.
}

## Source

**Maintained source:** {Exact path or link.}

**Responsibility:** {The question or behavior this source owns.}

**Related surfaces:** {Counterparts, generated output, installed copies, or consumers that must stay aligned; omit when none.}

## Contract

{State the smallest complete set of conditions that must remain true. Preserve required conditions, exceptions, and ownership boundaries.}

### {Distinct Concern}

{Optional: a separate installation, generation, loading, scope, compatibility, or external-contract requirement. Remove this subsection unless it earns its place.}

## Verification

| Requirement | Check | Evidence and limit |
| --- | --- | --- |
| {Contract condition} | {Actual project command, inspection, or other check} | {What a result proves and what remains unchecked} |

**Change sequence:** {Only necessary ordering across sources and generated or installed counterparts.}

**Unavailable checks:** {Name a real verification gap and the required follow-up; do not report an intended check as passed.}
