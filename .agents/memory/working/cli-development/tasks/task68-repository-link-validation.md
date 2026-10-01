---
open-forge:
  description: Offline validation of current README, public guides, and routed Markdown local paths and anchors
  tags: [Memory, Working, Task, CLI, Contextual, Complete]
---

# Task 68 — Repository link validation

## Outcome

Validate the current repository's README, public guides, and routed current
Markdown local paths and anchors offline with the existing Markdown parser.
Inside the named roots, the two explicit source exclusions cover illustrative
fixtures and unaccepted candidate prose. Historical archives remain outside
the named roots. The checker integrates with `npm verify` and CI. This Task
does not add a remote URL availability checker.

The execution profile is Standard. It has two phases and three milestones:
spec frozen, checker integrated and tested, and the repository gate green.
The exact checker and source-policy packet is frozen below. Return to Root for
any new policy or shared-contract choice; do not invent a checker or broaden
the source scope.

## Frozen specification

Root froze the read-only specification for this Task. It adds no dependency,
project, parser, command, or general validation framework. The checker and
scope APIs are static, and the test and wiring packets run sequentially.

### Packet 1 — checker and source scope

Under
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Documents/RepositoryDocumentation/Shared/`:

- `RepositoryMarkdownScope.cs`
  - `internal static IReadOnlyList<string> Enumerate(string repositoryRoot)`
  - `internal static bool IsPublishedSiteSource(string sourcePath)`
- `RepositoryMarkdownChecker.cs`
  - `internal static Task<IReadOnlyList<string>> CheckAsync(string repositoryRoot, IReadOnlyList<string> sourcePaths, CancellationToken cancellationToken)`

Both classes are `internal static`; helpers remain private. Diagnostic strings
avoid a new result-model hierarchy. `Enumerate` requires an explicit absolute
repository root containing `OpenForge.Cli.slnx`, traverses only the named
recursive roots and immediate Extension children, selects exact `.md` files,
normalizes separators, deduplicates, and sorts with `StringComparer.Ordinal`.

The fixed current-document inventory is:

```text
README.md
docs/**/*.md
src/docusaurus/docs/**/*.md
src/extensions/README.md
src/extensions/*/README.md
.agents/maps/**/*.md
.agents/memory/crystallized/documents/cli/_cli.md
.agents/memory/crystallized/documents/cli/architecture.md
.agents/memory/crystallized/documents/cli/command-contract-set.md
.agents/memory/crystallized/documents/cli/distribution.md
.agents/memory/crystallized/documents/cli/shared-operation-contract.md
.agents/memory/crystallized/documents/cli/contracts/**/*.md
.agents/memory/crystallized/documents/cli/layers/**/*.md
.agents/memory/crystallized/documents/cli/technical-designs/**/*.md
.agents/memory/crystallized/decisions/framework/workspace-state-files.md
```

Missing required files or roots, unreadable enumeration, reparse-point
traversal, and an empty result fail. The fixed named scope includes the public
Docusaurus demos pages. Inside the named recursive roots, the only explicit
source exclusions are `docs/cli-experience-fixtures` (test corpus) and
`docs/extension-candidates` (unaccepted candidate prose). Other
archive/template/payload/runtime-demo trees are excluded because they are
outside the named roots. Do not apply arbitrary directory-name exclusions. A
destination inside an excluded tree is still checked and is never promoted
into source scope.

`IsPublishedSiteSource` recognizes `src/docusaurus/docs/**/*.md` and exactly
`docs/cli.md`, `docs/extensions.md`, and `docs/development.md`.

### Packet 1 — parser, resolver, and failure boundary

Use the existing C# `MarkdownDocumentParser.Parse`, consuming collected links
and images ordered by `Span.Start`, their `RawDestination`, and the use
location. Reference definitions are already resolved by the collector. Use the
existing `CliWorkspace`, `PhysicalPathResolver.ResolveCandidate`, empty
`SourceCatalogue` construction, `SourceLinkDestinationInput`, and
`SourceLinkDestinationResolver`. Do not add another decoder, slugger, Markdown
AST, syntax regex, parser, or resolver.

Read sources as strict UTF8, cache facts only within one invocation, preserve
every link/image occurrence, and emit ordered diagnostics as
`source:line:column: code: destination`, using `1:1` for source-level failures
and no machine-specific absolute paths. Capture the resolver's physical
resolution for directory adaptation: only a contained ordinary directory with
no fragment, confirmed through the existing `LinkTargetReader`, may convert
the resolver's malformed-directory outcome to success.

The existing resolver owns URL decoding, lexical and physical containment, and
canonical heading matching. Check authored path-component casing ordinally
against actual directory entries. HTTP(S), `mailto:`, and protocol-relative
URLs remain outside the offline gate; other unsupported schemes fail.
Docusaurus `/docs/`, `/guides/`, and `/img/` routes delegate to existing site
handling, while relative Markdown paths use the repository checker. Unmatched
or unsupported anchors remain failures or `unverified-anchor` when the existing
Markdown model cannot verify them. Cancellation, read failures, and unsupported
local cases cannot produce a clean result.

### Packet 2 — tests and fixtures

Add only
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Documents/RepositoryDocumentation/RepositoryMarkdownTests.cs`
and
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Documents/RepositoryDocumentation/Shared/RepositoryMarkdownFixtures.cs`.
Use the existing `TemporaryWorkspace` APIs through static fixtures
`CreateLinkCases()` and `CreateScopeCases()`, with no wrapper filesystem,
injected parser, mock resolver, or fixture framework. Keep one
`RepositoryMarkdownTests` class. The repository test enumerates the current
directory, checks the returned inventory, and asserts no diagnostics.

Cover links and images, reference forms, encoded and malformed destinations,
queries, casing and containment, fragments and duplicate slugs, unsupported
Setext/HTML/non-Markdown anchors, code/frontmatter, directory links, delegated
site links, excluded-source destinations, exact scope expectations, missing
members, invalid UTF8, cancellation, stable diagnostic order, and unchanged
fixture hashes. Expected inventories are independently authored and checking
does not write.

### Packet 3 — wiring and documentation

Edit only `package.json` and `docs/development.md`. Add this exact script:

```json
"check:docs": "dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true -- --filter-class OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.RepositoryMarkdownTests --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off"
```

Append `&& npm run check:docs` to `verify`. No workflow edit is needed. The
development guide documents the offline local-target and supported-anchor gate,
source exclusions, destination checking into excluded trees, no external URL
contact, Docusaurus route delegation, and failure of anchors the existing
Markdown model cannot verify.

Packets 1, 2, and 3 are sequential and complete. The checker helper, scope,
fixtures, tests, documentation wiring, and repository gate are integrated.
The exact two stale anchors, `Index behavior` → `interface#dry-run` and
`Route Init behavior` → `interface#verification`, are fixed. The named-root
scope and its two explicit exclusions remain unchanged, and no source beyond
the named roots is silently excluded.

## Plan

1. Freeze the exact current-source scope, exclusions, parser boundary, findings,
   and `npm verify`/CI integration packet at Root.
2. Implement and test the accepted packet, keeping the two explicit source
   exclusions and named-root boundary unchanged.
3. Apply the assigned exact documentation correction, rerun the repository
   gate, and record its result in the release record.

The completed horizon is phase 2/2 with milestone 3/3 — Complete. The exact
source policy and specification remain frozen. The checker helper, scope,
tests, and wiring are integrated, and the repository gate is green. Further
whole-candidate review and release qualification belong to Task 69.

## Execution Capsule

- Current owner: Root; execution via Worker Watch.
- Current boundary: Phase 2/2, milestone 3/3 — Complete; the exact frozen
  specification, checker, tests, wiring, and repository gate are integrated.
- Profile: Standard.
- Review budget: 0 separate units; the release whole-change review covers this Task.
- Council budget: 0.
- Correction budget: 1 grouped correction pass.
- Protected boundaries: the existing Markdown parser, current routed Markdown
  sources, the two explicit source exclusions inside named roots, no arbitrary
  directory-name filtering, `npm verify`, and CI. No remote URL checker.
- Stop condition: Return to Root when the exact packet or a new policy choice is required. Do not choose parser duplication, checker design, or broader source scope here.
- Completion grace: None consumed.

## Current State

**Task 68 “Repository link validation” (phase 2/2): milestone 3/3 — Complete.** The fixed named scope includes the public Docusaurus demos pages; only `docs/cli-experience-fixtures` and `docs/extension-candidates` are explicitly excluded inside named roots, with no arbitrary directory-name filtering. The exact checker, parser/resolver boundary, tests, and `npm verify`/CI wiring are integrated and tested. On beta4, `npm run verify` exited 0, including 13 `RepositoryMarkdownTests` passed with 0 skipped against the current repository inventory. The two stale anchors are fixed, and no source beyond the named roots is silently excluded. Task 69 still owns whole-candidate review and release qualification.
