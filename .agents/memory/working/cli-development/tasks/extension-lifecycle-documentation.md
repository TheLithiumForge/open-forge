---
open-forge:
  description: Verify public CLI documentation against the integrated executable and reconcile remaining CLI-owned references
  tags: [Memory, Working, Contextual, CLI, Task, Extension, Documentation]
---

# CLI Documentation Verification

## Task State

The user reassigned all public documentation to Task 28, including CLI and setup guides. Task 28 has rewritten the README and every public `docs/` page. It removed the legacy public instructions instead of retaining a second command guide. This record now holds the remaining executable verification and CLI-owned reference work for the user's implementer on a separate branch.

This record is integrated with Task 28 after explicit user authorization. Register a task identity in the [project control ledger](../project-control.md) when activating it. This record does not dispatch an implementer or authorize a behavior change, merge, or publication.

## Outcome

Verify the rewritten public documentation against the actual integrated CLI. Keep current command examples and observable behavior aligned without restoring legacy explanations or treating documentation as executable evidence.

The public rewrite used read-only CLI definitions and contracts from `develop` at `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`, together with the reviewed Framework and Extension source on this branch. No CLI was built or executed for this documentation pass. Select a fresh accepted baseline for verification after integration.

## Starting Sources

- [Public Extension documentation](../../../../../docs/extensions.md)
- [CLI command overview](../../../../../docs/cli.md)
- [Extension command contracts](../../../crystallized/documents/cli/contracts/extension/_extension.md)
- [Consumer permission contracts](../../../crystallized/documents/cli/contracts/shared/workspace-permissions/_workspace-permissions.md)
- [Replacement CLI Architecture](../../../crystallized/documents/cli/architecture.md)
- [Frozen CLI MVP Architecture](../../../crystallized/documents/cli/mvp-architecture.md)
- [Top Architecture](../../../crystallized/documents/architecture.md)
- [First-party catalogue](../../../../../src/extensions/README.md)
- [Catalogue synchronization task](extension-catalogue-synchronization.md)

## Work And Evidence

1. Compare the integrated executable, its command help, accepted contracts, and rewritten public guides. Resolve any behavioral disagreement through the existing authority rules.
2. Verify runnable examples and the exact global options, source-reference grammar, route metadata flags, Extension source separation, Library argument order, status streams, and exit codes. Record the source revision, executable identity, commands, and results.
3. Complete the separate scoped-continuity and embedded-catalogue tasks before making their target behavior or package availability a release claim. Recheck affected documentation after those changes.
4. Reconcile any remaining CLI-owned references to the current Extensions Architecture and retired historical surfaces. Preserve the architecture's narrower responsibility and keep detailed lifecycle behavior in its defining CLI sources.
5. Keep installation and update guidance in public setup documentation. Preserve manual Framework use and optional tooling.
6. Run proportionate public-command and package evidence for the integrated candidate. Documentation checks do not replace executable qualification.

## Reference Repairs Already Prepared

Task 28 corrected the relative path to the CLI-v2 historical record in current Markdown compatibility, Markdown syntax, and path-identity documents. The target remains history. These three path repairs need no further rewrite; preserve them during integration and check that they still resolve.

## Completion

- Public guides contain only the current CLI instructions, with no competing legacy guide.
- CLI descriptions and runnable examples agree with the accepted contracts and integrated executable.
- Package semantics remain in the Extensions Architecture, with lifecycle detail in its defining CLI sources.
- Links, anchors, and relevant examples have supporting evidence.
- Any unresolved behavior or release question remains explicit rather than being settled by a documentation edit.
