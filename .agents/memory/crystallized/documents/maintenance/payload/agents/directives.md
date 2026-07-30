---
open-forge:
  description: Current maintenance contract for the installable Directives Core category entrypoint
  responsibility: Preserve the installed Directive activation, scope, authority, source alignment, and deterministic validation boundary
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Directive]
---

# Directives Category Maintenance Contract

## Source

[`src/open-forge/.agents/directives/_directives.md`](../../../../../../../src/open-forge/.agents/directives/_directives.md) is the canonical installed Directives entrypoint. The repository [Directives entrypoint](../../../../../../directives/_directives.md) dogfoods the same authored contract and may add local generated entries.

The [current Directive contract](../../../framework/primitives/directives.md) defines the coherent activation, authority, scope, and file model.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Directive so the `root route` enters baseline context with its primitive type visible
- The authored entrypoint preserves the one-pass model: select a Directive route, then load and obey every direct file exposed through #LoadNow
- Root direct files are workspace-wide; a selected child entrypoint establishes narrower positive scope before its direct files load
- Child Directives add to active ancestor Directives and surface conflicts instead of inventing narrower precedence
- The entrypoint requires one substantive level-2 `## Axioms` section and #LoadNow on every direct Directive file
- The source contains the minimum reusable category contract and no opinionated Directive content

The [routed Markdown representation](../../../framework/markdown/routes.md) defines entrypoint and generated-region syntax. Directive-specific meaning remains in the source and current Directive contract rather than in shared Markdown rules.

## Verification

- Directive validation tests require direct-file #LoadNow, substantive Axioms, active primitive classification, and rejection of retired applicability gates
- Core installation tests verify that the root category installs, indexes, and remains baseline-loaded
