---
open-forge:
  description: Implement route removal with dependency, reference, generated-navigation, and recovery integrity
  tags: [Memory, Working, CLI, Task, Route, Remove, Mutation, Contextual]
---

# Implement Route Remove

## Task State

- State: Planned after Route Move.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/remove/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/remove/behavior.md).

## Expected Outcome

`route remove` removes exactly the accepted route unit after proving dependency,
reference, overwrite, descendant, lifecycle, generated-navigation, and external
recovery-bundle boundaries. It preserves unrelated and user-owned content.

## Architecture

- Observation records exact physical source, route descendants, overwrite pair,
  incoming references, generated projections, lifecycle ownership, and recovery-
  bundle state.
- `RouteRemovePlan` explicitly names every file/region effect and its ordering.
- Use shared route/reference facts and mutation primitives. Keep dependency policy,
  refusal reasons, and deletion order local.

## Evidence

Cover simple route, non-empty or dependent route, overwrite pair, incoming
references, managed/unmanaged distinction, dry run, invalid confirmation/write
policy, physical aliases, lock race, generated navigation, proof that no lifecycle
write or ownership release occurs, partial failures and bundle retention, second
run, preservation, human/JSON/help, process exits, and AOT.

Current Remove is positive-unmanaged-only. Framework-aware Route Init targets or
generated regions are trusted claims and block selection, including when they
sit below a user-owned scope entrypoint. `sourceAssetPath` is provenance and does
not grant release authority.

## Preparation Closeout

Initial read-only preparation on clean no-op branch `codex/route-remove` at
exact base `33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray,
Red, or Green change. Later preparation produced the neutral Markdown link-label
projection, now project-accepted and integrated into local `develop` at
`89a35a7876f39123d9538bca24126ff7197b9459`. No public wire, Route Remove
command, or shared mutation behavior was implemented. Route Remove remains
Planned after Route Move and the remaining command-local result freeze.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Remove/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/**`.
- `src/cli/core/OpenForge.Cli.Core/Framework/**`, Route shared surfaces,
  `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  EndToEnd/Native AOT evidence, preceding commands, and program ledgers remain
  protected unless a later packet assigns one exact neutral promotion.
- Decisive evidence must cover binding, leaf/category inventory, positive
  ownership proof, complete workspace references and label-preserving
  detachments, intended topology, dry-run/revalidation, file and directory
  effects, recovery and residual state, typed presentation, process behavior,
  and Native AOT.

### Accepted Parser Prerequisite

Integrated commit `89a35a7876f39123d9538bca24126ff7197b9459`
adds the smallest neutral Markdig-bound typed projection. Every
`MarkdownLinkFact` carries a `Supported` visible label or an `Unsupported`
state. The parser derives supported text only from the existing pinned Markdig
child AST, collapses whitespace deterministically, and fails closed for empty,
incomplete, or unhandled children. It does not scan raw Markdown again or change
the parser pipeline or existing consumers.

Final evidence is Release `0` warnings and `0` errors, parser `31/31`, full Unit
`1410/1410`, and full Integration `614/614`, all with zero skips. Fresh
independent Sol/xhigh review is `ROBUST PASS`. No EndToEnd or Native AOT boundary
was added because this is one internal document fact with no public,
serialization, composition, dependency, or pipeline change.

### Accepted Shared Preparation Decisions

- Use one shared workspace-Markdown discovery and validation primitive over
  validated workspace-relative paths. Its inputs explicitly define includes,
  excludes, and filters. Route Move and Route Remove must not add local Markdown
  scanners.
- Use one shared non-recursive delete-if-empty capability. A caller supplies the
  exact planned directories; application revalidates them and deletes them
  bottom-up only while each directory is empty. It is not a recursive deletion
  engine.
- Use one Framework-plus-Extensions ownership projection from one validated
  lifecycle-file snapshot. Current filename authority remains
  `.agents/open-forge.lifecycle.json` unless a later accepted decision literally
  renames it.
- Every newly created lifecycle document canonically includes every standard
  root key. Framework-created documents use a complete empty Extensions value.
  An existing incomplete file remains untrusted; repair is deferred to Update or
  Doctor rather than hidden inside Route Remove.
- Exact planned directory creation is a normal shared mutation capability
  available to any command. It is not Route-specific and does not imply a
  general filesystem or route-mutation engine.

Route Move proves or promotes these capabilities first when its accepted
behavior is the same. Route Remove consumes the accepted shared meaning after
Move integration and keeps category policy, removal ordering, and refusal
reasons command-local.

### Canonical Lifecycle Predecessor Fulfilled

Canonical lifecycle creation integrated into local `develop` at
`1d404c5cef3f5fd464ca771fc132a657f792f533`. A genuinely absent lifecycle
document now receives the canonical standard root envelope and complete empty
Extensions value when Framework publication creates it. The integrated boundary
passed its focused and full managed, Native AOT dogfood, diff, and independent
review gates.

This fulfilled creation gate does not trust or repair an existing document whose
required Framework or Extensions section is missing, `null`, malformed, or
incomplete. Such existing state remains untrusted and blocks planning; Route Move
and Route Remove must not work around or silently repair it. Update or Doctor
remains the repair owner for existing untrusted documents.

### Remaining Maintainer Authority

- Freeze the exact command-local JSON result graph, findings, finite values, and
  `next` content.

## Deferred Managed-Release Decision

Any later expansion to managed scoped routes requires explicit maintainer
acceptance of:

1. whether the release unit is one target, one managed chain, or one physical
   category;
2. preservation of user-owned scope entrypoints, descendants, and shared
   generated regions;
3. lifecycle publication order and post-remove verification; and
4. repeat and no-op semantics.

Do not infer those decisions from a path prefix or `sourceAssetPath`.

## Stop Conditions

Stop before recursive deletion not explicitly planned, removal of unowned
content, ignored references, unsafe alias traversal, or cleanup of recovery facts
before verified completion. Stop before releasing or deleting any managed scoped
target under the current contract.
