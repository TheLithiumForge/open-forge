---
open-forge:
  description: Users receive `src/open-forge/` as the payload while repository-only Maintenance contracts govern reviewed source without becoming hidden runtime context
  tags: [Memory, Decision, CurrentTruth, Packaging, Governance]
---

# Source And Packaging

## Context

Open Forge needs rich repository-only design and Maintenance material while promising that an installed workspace remains complete in the files users receive. Dogfood also needs to test the same payload without making repository governance a hidden dependency.

## Decision

`src/open-forge/` is the installable Framework payload. Its `AGENTS.md` is the canonical root instruction contract. Minimal harness bridges may import it and preload another canonical Framework entry such as the loader without duplicating policy.

Every runtime contract required to understand or use an installed workspace is expressed in the payload itself. Repository-only current documents and Maintenance contracts explain design, govern reviewed sources, and define verification, but they are not required runtime context.

The root `.agents/` tree dogfoods the payload and may add visibly repository-specific routes. Shared semantics stay aligned unless an intentional local difference is documented.

Users own installed Framework files. Durable customization uses ordinary sibling files, child routes, or visible overwrite companions rather than hidden version or source metadata.

## Rationale

Separating installed completeness from repository governance keeps the Framework human-readable and tool-optional while allowing maintainers to preserve deeper cross-source obligations.

Dogfooding the same payload exposes migration and usability failures. Visible local additions let the repository evolve without pretending every project needs its Maintenance and design system.

## Alternatives And Tradeoffs

- Shipping repository Maintenance documents would enlarge user context and expose implementation governance as runtime policy
- Keeping required behavior only in governance would create hidden runtime context
- A separately maintained dogfood Framework could drift from the payload
- Hidden source or version metadata would make updates easier to identify but weaken inspectability and manual completeness

Maintaining dogfood and source representations still requires deliberate synchronization and verification.

## Consequences

- Framework source remains concise, explicit, and easy to diff
- Maintenance contracts point to canonical installed sources and proportionate verification
- Harness bridges preserve workspace-owned content outside the managed Open Forge block
- Provider-native imports may make required baseline context deterministic without becoming another policy source
- Build and package behavior must deliver the complete payload without repository-only dependencies
- Normal updates respect user removal and customization unless restoration is requested

## Authoritative Sources

- [Framework distribution and dogfood contract](../../documents/framework/architecture.md#distribution-and-dogfood)
- [Payload Maintenance scope](../../documents/maintenance/payload/_payload.md)
- [Installable Framework source](../../../../../src/open-forge/)
- [Current build implementation](../../../../../build.ts)
- [Current overwrite contract](../../documents/framework/routing/overwrites.md)

## Decision Relationships

- [Product direction](../product/product-direction.md)
- [Extension package boundary](../extensions/extension-package-boundary.md)
- [Routing model](routing-model.md)
- [Templates as a Core primitive](template-primitive.md)
