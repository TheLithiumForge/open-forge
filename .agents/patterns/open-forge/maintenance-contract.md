---
open-forge:
  description: Structure current maintenance documents around their source, maintainer contract, and verification
  tags: [Pattern, Framework, Maintenance, Governance, Documentation]
---

# Maintenance Contract

## Shape

```text
frontmatter
  description                            required metadata
  tags                                   required metadata
  responsibility                         optional edit boundary, no authority grant
# <subject>                              illustrative title
## Source                                required
  canonical source link + responsibility
  true counterparts, when present
## Contract                              required
  smallest complete invariants, boundaries, change obligations
  ### <distinct concern>                 optional, repeatable
  statement + relative supporting link
## Verification                          required
  observable tests or checks + links
  manual verification when no automated check exists
```

The section names, order and relationships are fixed. Subject and concern names are illustrative. Optional Contract subsections fit distinct concerns such as patching, generation, loading, scope, integration or an external contract.

## Applicability And Sources

For current maintenance documents. Positive, closed descriptions are preferable where practical. A negative constraint helps when it closes a concrete risk more precisely.

Optional `responsibility` fits when one stable sentence helps keep future edits local.

- [Deliberate Framework Change](../../directives/open-forge/framework/deliberate-framework-change.md#maintenance-documents) binds the maintenance shape and editing rules.
- [Payload Maintenance](../../memory/crystallized/documents/maintenance/payload/_payload.md#axioms) binds the source-wording boundary.
- [Maintenance contract template](../../templates/documents/maintenance-contract.md) supplies optional starting content.
