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

- The title is followed by `## What behavior is required in this scope?` and a short paragraph describing the folder.
- Frontmatter uses #LoadNow, #Core, and #Directive so the `root route` enters baseline context with its primitive type visible
- When its loader row is enabled, #LoadNow reads its catalogue and applicable rule files in order.
- Sibling Directives under the root apply across the workspace. A child catalogue narrows the scope. It may stay on demand, or carry #LoadNow and applicable file conditions when its rules must load automatically.
- Child Directives add to active parent Directives and report conflicts instead of creating hidden precedence
- Every sibling Directive carries #LoadNow and keeps its instructions under a non-empty level-2 `## Instructions` heading
- Instructions state obligations, conditions, and meaningful exceptions. Supporting examples or visuals are illustrative, and obligations remain understandable from the instructions themselves
- The entrypoint defines the reusable category rules. Each sibling Directive defines its own required behavior
- The category uses `applyTo` to limit file-specific rules and required child catalogues. The [loader](loader.md) defines matching through the full ancestor chain and conflict reporting.

The file-format section of the [workspace guide](README.md) defines catalogue and Instructions formats. The [routed Markdown representation](../../../framework/markdown/routes.md) defines entrypoint and generated-region syntax. Directive-specific meaning remains in the source and current Directive contract rather than in shared Markdown rules.

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for shipped bytes and frontmatter rendering.
- [SourceLoadingClosureResolverTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Loading/SourceLoadingClosureResolverTests.cs) cover required loading and conditions. They do not validate the meaning of Directive prose.
- Review the source for non-empty Instructions, #LoadNow rule files and the distinction between selected and automatically loaded child catalogues. Compare shared source and repository rules outside Entries.
