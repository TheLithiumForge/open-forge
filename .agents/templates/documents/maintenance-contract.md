---
open-forge:
  description: "State what a maintained source defines, what must remain true, and how to verify it"
  tags: [Extension, Template, Document, Maintenance, Governance]
---

# {Surface} Maintenance Contract

{
Use when a source has stable maintenance obligations worth defining separately. Do not invent a governance layer for a trivial file.
Keep only obligations and checks that affect maintenance. Replace {prompts}, then remove this guidance and unused optional sections.
}

## Source

**Maintained source:** {Exact path or link.}

**Responsibility:** {The question this source answers or the behavior it defines.}

**Related surfaces:** {OPTIONAL: Counterparts, generated output, installed copies, or consumers that must stay aligned.}

## Contract

{State the smallest complete set of conditions that must remain true. Preserve required conditions, exceptions, and ownership boundaries.}

### {Distinct Concern}

{Optional: a separate installation, generation, loading, scope, compatibility, or external-contract requirement. Remove this subsection unless it earns its place.}

## Verification

| Requirement          | Check                                                | Evidence and limit                                |
| -------------------- | ---------------------------------------------------- | ------------------------------------------------- |
| {Contract condition} | {Actual project command, inspection, or other check} | {What a result proves and what remains unchecked} |

**Change sequence:** {OPTIONAL: Necessary ordering across sources and generated or installed counterparts.}

**Unavailable checks:** {OPTIONAL: Real verification gaps and required follow-up. Do not report an intended check as passed.}
