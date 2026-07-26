---
open-forge:
    description: Temporary repository-only vocabulary map for keeping role-bearing Open Forge terms consistent during document and source migration
    tags: [Memory, Working, Helper, Contextual, Temporary, Terminology, Migration]
---

# Temporary Terminology Helper

## Status

This is a repository-only migration helper, not a permanent glossary or separate authoritative source. Use it when wording depends on a person's or system's role in Open Forge.

The intended user experience is that these meanings become obvious through consistent natural usage in current documents, runtime entries, and public documentation. Remove this helper when those authoritative sources make the distinctions sufficiently clear.

## Role Terms

| Term             | Use when                                                                                                                         |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| User             | A person or group owns, adopts, installs, customizes, or consumes an Open Forge environment                                      |
| Operator         | The active work relationship matters: this role establishes goals, priorities, consequential tradeoffs, and accepted direction   |
| Maintainer       | The role changes the Open Forge distribution, repository contracts, source payload, tooling, tests, or release surfaces          |
| Agent            | An AI system or runtime investigates, suggests, challenges, executes, verifies, or preserves context within applicable direction |
| Contributor      | A person or agent changes a shared project, while product responsibility or active operating authority is not the point          |
| Person or people | No Open Forge-specific authority or responsibility needs to be distinguished                                                     |

The same person may be a user, operator, maintainer, and contributor. Select the term for the responsibility expressed by the sentence rather than the person's permanent identity.

## Authority Terms

| Term | Use when |
|---|---|
| Authoritative source | The type is unknown or irrelevant, and one file, route, system, person, or group authoritatively expresses the subject |
| Authoritative document | A current document authoritatively explains an accepted concept |
| Authoritative route | A routed file or entrypoint authoritatively expresses the applicable Framework or workspace meaning |
| Authoritative system | An external system authoritatively contains source code, issue state, product data, or another live subject |
| Responsible person or role | A human responsibility or accountability is meant rather than semantic authority |

Use the most specific natural term that the sentence supports. `Authoritative source` is the generic fallback, not a phrase that every sentence must repeat.

Retain `ownership` when possession or managed lifecycle is the actual subject, including user ownership of installed files, Extension ownership of managed bytes, and ownership transfer during Template instantiation.

## Usage Checks

- Use `operator` for active decisions and direction, not as a more technical synonym for every user
- Use `user` for ownership, adoption, customization, and product-facing capability
- Use `maintainer` when a requirement exists because Open Forge itself is being changed or distributed
- Use a typed authoritative-source term for semantic authority
- Use `responsible person` or `responsible role` for human accountability
- Use `ownership` only when possession or managed lifecycle is the intended meaning
- Use ordinary person-centered language when the domain distinction adds no useful precision

## Related Helpers And Authoritative Sources

- [Knowledge role helper](knowledge-role-helper.md)
- [Open Forge Principles](../crystallized/documents/principles.md)
- [Top Open Forge Architecture](../crystallized/documents/architecture.md)
- [Open Forge Framework Architecture](../crystallized/documents/framework/architecture.md)
- [Authoritative source terminology decision](../crystallized/decisions/authoritative-source-terminology.md)
- [Historical knowledge-role boundary analysis](../archived/analysis/2026-07-26_knowledge-role-boundaries.md)
