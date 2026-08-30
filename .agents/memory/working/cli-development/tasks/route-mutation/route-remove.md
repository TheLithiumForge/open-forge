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
Red, or Green change. Preparation resumed after an authorized fast-forward to
accepted `develop` `02dab54a7db84b337542fc03bda10858ca07bf3e` and formed only the
branch-local neutral parser prerequisite through Gray, focused Red, and Green.
This range is not integrated or project-accepted; the Route Mutation parent
therefore retains the historical no-commit preparation closeout until its
integration-owned record is updated. No public wire, Route Remove command,
integration, or shared mutation behavior was implemented. Route Remove remains
sequenced after integrated Route Move and the remaining result-authority gate.
After canonical lifecycle creation integrated into local `develop`, this feature
branch was cleanly rebased without semantic conflict onto
`1d404c5cef3f5fd464ca771fc132a657f792f533`.

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

The frozen Gray surface adds the smallest neutral Markdig-bound typed projection
that associates each link fact with its visible label or an unsupported-label
state. The label belongs to `MarkdownLinkFact`; it is not a sibling fact that a
consumer must correlate by spans. Focused Red evidence requires non-empty plain
visible text from the existing parsed child tree and an explicit unsupported
state when the complete label cannot be projected. Do not parse raw Markdown
again. Gray access failed explicitly at the new label property until the
separately authorized Green. Green now projects only the supported Markdig child
AST and fails every unhandled or incomplete label closed as unsupported. Existing
parser consumers and the Markdig pipeline remain unchanged.

Preparation commits are:

- Gray contract: `051ec9630d1e3a4af2b80897e7d9ec452af23a3e`;
- Gray invariant correction: `b9ca6051c10d315b9b769765b65b781dabda158a`;
- initial focused Red evidence: `9ae6a7765eb8e8cb60a972295a1bf096f983d41b`;
- explicit Gray failure correction:
  `bf5017b8a94b79be0f7c99e42b79d60fd0a9298e`;
- finite pinned-Markdig Red matrix correction:
  `c95fe6f11b8f1560a98e11809dd7f204badd8f2b`;
- Green projection:
  `aabd82645fc0bc96ec7af7ada01ce5da8ef9dc4f`.

Authorized locked restore and the focused Release Unit build passed with zero
warnings and errors. The exact parser-class Red run executed 31 cases: 15 passed,
16 failed, and zero skipped. Every failure terminated only at
`MarkdownLinkFact.Label` with the named `NotSupportedException`; no setup,
compilation, parser, or unrelated failure occurred.

At Green, the full Release solution build passed with zero warnings and errors,
the frozen parser evidence passed 31/31, the full Unit suite passed 1407/1407,
and the full Integration suite passed 598/598, all with zero skips. Whitespace
format verification and `git diff --check` passed. No EndToEnd or Native AOT
boundary was added because the change is one internal parser fact with no public,
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
  root key and one complete empty Extensions value. An existing incomplete file
  remains untrusted; repair is deferred to Update or Doctor rather than hidden
  inside Route Remove.
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
retains its affected managed, Native AOT, and review evidence.

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
