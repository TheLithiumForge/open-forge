---
open-forge:
  description: Current maintenance contract for the installable Directives Core category entrypoint
  responsibility: Preserve installed Directive activation, scope, authority, source alignment, and deterministic validation boundary
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Directive]
---

# Directives Category Maintenance Contract

## Source

[`src/open-forge/.agents/directives/_directives.md`](../../../../../../../src/open-forge/.agents/directives/_directives.md) is the canonical installed Directives entrypoint. The repository [Directives entrypoint](../../../../../../directives/_directives.md) dogfoods the same authored contract and may add local generated entries.

The [current Directive contract](../../../framework/primitives/directives.md) defines the coherent activation, authority, scope, and file model.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Directive so the `root route` enters baseline context with its primitive type visible
- The entrypoint keeps the one-pass model: select a Directive route, then load and follow every sibling Directive exposed through #LoadNow
- Sibling Directives under the root apply across the workspace. A selected child entrypoint sets the narrower scope before its sibling Directives load
- Child Directives add to active parent Directives and report conflicts instead of creating hidden precedence
- Every sibling Directive carries #LoadNow and has exactly one non-empty level-2 `## Instructions` section
- The entrypoint defines the reusable category rules. Each sibling Directive defines its own required behavior

The [routed Markdown representation](../../../framework/markdown/routes.md) defines entrypoint and generated-region syntax. Directive-specific meaning remains in the source and current Directive contract rather than in shared Markdown rules.

## Verification

- Directive validation tests require sibling-file #LoadNow, non-empty Instructions, active primitive classification, and rejection of retired applicability gates
- Core installation tests verify that the root category installs, indexes, and remains baseline-loaded
