---
open-forge:
  description: Focused Extension split, retained payload identity, Experience Design removal, and source verification limits
  tags: [Memory, Working, Contextual, Framework, Review, Extension]
---

# Extension Package Split Review

## Scope

Baseline: `930c1ae1562cd463f29f0d5e3eb23b4d2562a658`. The user explicitly authorized the reduced split and Experience Design removal. The [decision](../../../crystallized/decisions/extensions/focused-extension-packages.md) records the accepted rationale; the [catalogue](../../../../../src/extensions/README.md) defines the resulting package contents.

The full-context source author prepared the exact source moves, manifests, and READMEs. The primary owner reviewed the descriptions and package mapping, updated public and current local context, and independently checked the assembled sources. This review does not claim fresh-context independence or executable CLI qualification.

## Result

Twenty-three payload files move from Toolkit to focused packages without changing their bytes or installed paths. Managed Delivery remains unchanged in Orchestration. Four Experience Design files are removed from the source catalogue, along with the four matching repository Skill files and their local route entry.

| Selected package | Own payload files | Payload files with dependencies | Files including base Framework |
| --- | ---: | ---: | ---: |
| Project Documents | 7 | 7 | 30 |
| Memory Starters | 6 | 6 | 29 |
| Planning | 7 | 7 | 30 |
| Development | 3 | 3 | 26 |
| Orchestration | 1 | 11 | 34 |
| Development Toolkit | 0 | 23 | 46 |

All packages together contribute 24 unique payload files, making 47 files with the base Framework. Template counts in public prose count thirteen leaf Templates; the three scoped Template entrypoints are additional payload files.

## Verification

Static checks verify all six manifests and sorted unique dependencies, acyclic dependency closures, unique installed-path ownership, content links and anchors against only the base and declared dependencies, generated entry metadata, Workflow section order, and Template metadata. Each closure is checked separately so unrelated packages cannot supply a missing dependency.

Every retained payload is byte-identical to its baseline file at the same installed path. All thirteen local Template leaves, the planning entrypoint, and Work Records match their package sources. Existing local Workflow profiles and specialized Task/Plan Templates are preserved. Public and changed Memory links resolve, and the new Decision and review are exposed through their parent indexes. The original branch logic review remains unchanged.

## Limits And Follow-Up

This is source assembly and content verification. No CLI implementation, contracts, tests, build, installation, lifecycle operation, or executable qualification changed or ran. Base Framework content is unchanged.

The [catalogue synchronization task](../../cli-development/tasks/extension-catalogue-synchronization.md#package-ownership-transition) covers embedded distribution, old Toolkit ownership, user changes, removed defaults, retired Skill files, dependency-only bundle behavior, and managed update/removal verification. Local lifecycle receipts are not rewritten by this source change.

[Task 28](../../cli-development/tasks/source-framework-review.md) remains on the review branch. No merge to `develop` or release is authorized.
