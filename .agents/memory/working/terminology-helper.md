---
open-forge:
    description: Temporary repository-only vocabulary map for keeping role-bearing Open Forge terms consistent during document and source migration
    tags: [Memory, Working, Helper, Contextual, Temporary, Terminology, Migration]
---

# Temporary Terminology Helper

## Status

This is a repository-only migration helper, not a permanent glossary or separate authority owner. Use it when wording depends on a person's or system's role in Open Forge.

The intended user experience is that these meanings become obvious through consistent natural usage in current documents, runtime entries, and public documentation. Remove this helper when those owners make the distinctions sufficiently clear.

## Role Terms

| Term             | Use when                                                                                                                         |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| User             | A person or group owns, adopts, installs, customizes, or consumes an Open Forge environment                                      |
| Operator         | The active work relationship matters: this role establishes goals, priorities, consequential tradeoffs, and accepted direction   |
| Maintainer       | The role changes the Open Forge distribution, repository contracts, source payload, tooling, tests, or release surfaces          |
| Agent            | An AI system or runtime investigates, suggests, challenges, executes, verifies, or preserves context within applicable direction |
| Contributor      | A person or agent changes a shared project, while product ownership or active operating authority is not the point               |
| Owner            | A file, route, system, person, or group has responsibility or authoritative meaning for a stated subject                         |
| Person or people | No Open Forge-specific authority or responsibility needs to be distinguished                                                     |

The same person may be a user, operator, maintainer, and contributor. Select the term for the responsibility expressed by the sentence rather than the person's permanent identity.

## Usage Checks

- Use `operator` for active decisions and direction, not as a more technical synonym for every user
- Use `user` for ownership, adoption, customization, and product-facing capability
- Use `maintainer` when a requirement exists because Open Forge itself is being changed or distributed
- Use `owner` only with a clear subject; ownership may describe semantic authority, maintenance responsibility, or possession, so the sentence should identify which one matters
- Use ordinary person-centered language when the domain distinction adds no useful precision

## Related Helpers And Current Owners

- [Knowledge owner helper](knowledge-owner-helper.md)
- [Open Forge Principles](../crystallized/documents/principles.md)
- [Top Open Forge Architecture](../crystallized/documents/architecture.md)
- [Open Forge Framework Architecture](../crystallized/documents/framework/architecture.md)
- [Historical knowledge-role boundary analysis](../archived/analysis/2026-07-26_knowledge-role-boundaries.md)
