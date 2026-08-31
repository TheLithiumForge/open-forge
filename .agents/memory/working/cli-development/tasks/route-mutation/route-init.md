---
open-forge:
  description: Implement generic and Framework-aware sparse route initialization after root Install
  tags: [Memory, Working, CLI, Task, Route, Init, Mutation, Contextual]
---

# Implement Route Init

## Task State

- State: Complete and squash-integrated at
  `cc5085ce51ca624d07c347b014e036b8c3b7e1b4`, exact tree
  `a1810c4b247bf4997146baebf8a7ca3cf7f794c9`, from reviewed closeout
  `c5801494ac6426add2c64e32cafbba6f0162561a`. The executable evidence candidate
  remains `cb62b19f73afcace163371af9093d877821fa800`, tree
  `be93900d0dc102fcf2d5a351651c0b0134de39a0`.
  Root Install and the required shared foundations are integrated. The complete
  Generic and Framework-aware command, public surface, managed and Native AOT
  evidence, isolated dogfood, and independent review are closed.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/init/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/init/behavior.md).

## Accepted Mode Boundary

Generic mode retains exact-chain initialization and the fixed draft scaffold.
`--framework` accepts one desired concrete route, aligns exact canonical
non-root Framework segments uniquely against the embedded topology, treats
inserted segments as scope labels, and creates only the requested sparse chain.
It is not `install --route`, a blueprint/Template engine, or a `--scope`
placeholder language.

## Expected Outcome

For one exact concrete route target, inspect the current chain, plan every missing
entrypoint, preserve existing routable parents and authored content, apply the
fixed accepted scaffold under lock/revalidation, refresh generated navigation,
verify the resulting chain, and form one concrete result.

## Architecture

- Keep definitions, binding, request, plan, operation, result, and renderers at
  `Commands/Route/Init/`.
- Use route shared identity/topology facts and mutation primitives.
- Keep scaffold selection and route-init policy local.
- Reuse the neutral embedded Framework distribution and trusted root Install
  lifecycle. ID-form scope conversion remains one Route Init value policy; exact
  `.agents/...` targets are never slugged.
- Keep inserted scope entrypoints user-owned. Lifecycle-manage only copied
  canonical Framework assets and derived generated regions, with exact
  `sourceAssetPath` provenance.
- Author exactly one canonical `open-forge` metadata root with `description`,
  `tags`, and optional `responsibility`, and read only that root as Open Forge
  metadata. Treat `rune` and other unrelated YAML as opaque: never consume their
  values as Open Forge metadata, never author them, and preserve their bytes in
  bounded source edits. When both occur, use exactly `open-forge`.
- Reuse the shared parent-first ordinary-BCL directory capability while holding
  the external workspace lease. Generic mode reports missing `.agents` as the
  first ordinary directory-create effect after lease acquisition; Framework
  mode requires an existing trusted Install. Keep directory effects separate
  from file effects and retain verified created directories as reported
  residuals after later failure or interruption.
- Model inspect, plan, apply, verify, and lifecycle effects separately.

The accepted command-local callable freeze is:

```csharp
RouteInitBinding.CreateSymbols(Command routeGroup) -> RouteInitSymbols
RouteInitBinding.Close(RouteInitSymbols, RouteInitBindingComponents)
    -> CliCommandBinding<RouteInitRequest, RouteInitResult>
RouteInitOperationFactory.Create(WorkspaceLockStoreRoot?) -> RouteInitOperation
RouteInitOperation.ExecuteAsync(RouteInitRequest, CancellationToken)
    -> ValueTask<RouteInitResult>
RouteInitPlanBuilder.BuildAsync(RouteInitRequest, CancellationToken)
    -> ValueTask<RouteInitPlanBuild>
RouteInitApplicationOperation.ExecuteAsync(RouteInitPlan, CancellationToken)
    -> ValueTask<RouteInitApplicationOutcome>
RouteInitResultBuilder.Build(RouteInitResultFormation) -> RouteInitResult
```

Keep these concrete and Native-AOT-compatible. Do not add command-local service
locators, fake filesystems, interfaces, or a general Route engine.

## Evidence

Cover full existing, partially missing, completely missing, invalid segments,
collisions, physical aliases, dry run, lock race, generated navigation,
idempotence, external bundle preparation/retention for any existing
Replace/Delete, human/JSON/help/diagnostics, unchanged unrelated bytes, process
exits, and AOT. Cover Framework alignment with zero/multiple/consecutive scopes,
all scope positions, slug rules and exact-path non-slugging, ambiguity,
reordering, root recreation, trusted/outdated Install state, exact embedded
bytes, sparse creation, and lifecycle ownership exclusion for scope files.
Prove the shared directory capability through missing-parent races,
post-verification, retained residuals, and file/recovery non-regression.
Cover generic missing-`.agents` planning as the first ordinary lease-bound
directory effect, cancellation and contention before workspace effects, later
residual reporting, and Framework-mode refusal when trusted Install state is
absent.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-init` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. The implementation branch was then fast-forwarded without history
rewriting to integrated root Install base
`02dab54a7db84b337542fc03bda10858ca07bf3e`; the accepted post-Install freeze
below is the implementation authority before Gray.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Init/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Init/**`.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliYamlContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  `src/cli/core/OpenForge.Cli.Core/Framework/**`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Install/**`, EndToEnd/Native AOT
  evidence, other Route commands, and shared program ledgers remain protected
  integration or predecessor surfaces.
- Decisive evidence must cover binding, generic and Framework planning,
  intended-topology projection, mutation and lock races, recovery and lifecycle,
  typed presentation, public process behavior, and supported Native AOT.

### Accepted Sequencing And Freeze Boundary

- The exact command-local result graph, finding vocabulary, finite values, and
  `next` command/reason content are frozen by the Route Init
  [Interface](../../../../crystallized/documents/cli/contracts/route/init/interface.md).
- Route Init may consume embedded assets and trusted facts only through neutral
  Framework-layer backing functions, never through `Commands/Install/**`. Promote
  only the two accepted smallest shared functions: physical trusted-current
  verification and embedded-asset-to-source projection.
- Freeze and compile Gray before authoring Red evidence. Freeze failing Red
  evidence before Green. Protected composition, serialization-context, and Route
  help integration follows only after command-local behavior is passing.

### Accepted Post-Install Freeze

#### Accepted Decision 1: exact command-local result

Keep the Architecture's schema-v1 envelope exactly
`{ schemaVersion, command, status, workspace, result, next }`. The Route Init
`result` object is fully present in the following property order:

```text
result {
  mode
  scaffold
  target {
    requested
    id
    path
  }
  plan {
    completeness
    safety
  }
  framework {
    inventoryFingerprint
    segments[] {
      path
      role
      sourceAssetPath
    }
  } | null
  entrypoints[] {
    id
    path
    form
    current
    ownership
    metadata {
      description
      descriptionSource
      responsibility
      responsibilitySource
      tags[]
      tagsSource
    } | null
    sourceAssetPath
    outcome
  }
  effects[] {
    path
    kind
    action
    sourceAssetPath
    change {
      before
      expected
    } | null
    outcome
    residual
  }
  unchangedPaths[]
  lifecycle {
    action
    outcome
  }
  recovery {
    state
    residualPath
  }
  verification
  findings[] {
    code
    status
    target
    cause
  }
}
```

`status` and `next` are derived once from ordered findings. They are not
duplicated inside `result`. Collections are immutable and never `null`.
Entrypoints and Framework segments retain first-to-final chain order; effects
retain execution order; tags retain argument order; unchanged paths are unique
and ordinally ordered. Findings use the fixed code order below, then nullable
target and cause in ordinal order.

The target intentionally omits a public operand-form or `selectedBy` field.
`requested`, resolved `id`, and resolved canonical `path` are the accepted
observable requirements; adding public operand-form provenance would be a new
wire fact without a current consumer. This omission is accepted.

Nullable members are limited to unresolved target coordinates, `framework`,
existing-entrypoint `metadata`, metadata `responsibility`, `sourceAssetPath`,
directory `change`, create-change `before`, recovery `residualPath`, finding
`target`, envelope `workspace`, and envelope `next`.

Lifecycle evidence stays bounded to Route Init-owned facts. `effects` contains
only `directory`, `entrypoint`, and `generated-region` effects. `lifecycle`
reports only the command's action and outcome; neither the result nor a dry-run
change exposes the whole lifecycle document, preserved Extension state, root
Install targets, or other unrelated lifecycle bytes. An entrypoint create change
contains that new entrypoint's complete UTF-8 text. A generated-region change
contains only its bounded interior. Directory changes are `null`.

The finite machine values are:

| Coordinate | Values |
| --- | --- |
| `mode` | `apply`, `dry-run` |
| `scaffold` | `generic`, `framework` |
| `plan.completeness` | `not-established`, `incomplete`, `complete` |
| `plan.safety` | `not-established`, `safe`, `blocked` |
| Framework segment `role` | `installed-root`, `managed`, `scope` |
| Entrypoint `form` | `canonical`, `compatibility` |
| Entrypoint `current` | `existing`, `missing` |
| Entrypoint `ownership` | `user`, `framework` |
| `descriptionSource` | `draft`, `explicit`, `embedded` |
| `responsibilitySource` | `default-omitted`, `explicit-omitted`, `explicit`, `embedded` |
| `tagsSource` | `draft`, `explicit`, `mixed`, `embedded` |
| Entrypoint `outcome` | `unchanged`, `planned`, `not-started`, `created`, `verification-failed`, `completion-unknown` |
| Effect `kind` | `directory`, `entrypoint`, `generated-region` |
| Effect `action` | `create`, `replace` |
| Effect `outcome` | `planned`, `not-started`, `verified`, `verification-failed`, `completion-unknown` |
| Effect `residual` | `none`, `retained`, `unknown` |
| Lifecycle `action` | `none`, `preserve`, `publish` |
| Lifecycle `outcome` | `not-requested`, `planned`, `already-current`, `not-started`, `verified`, `verification-failed`, `completion-unknown` |
| Recovery `state` | `not-required`, `not-created`, `removed`, `retained`, `unknown` |
| `verification` | `not-requested`, `verified`, `failed`, `unknown` |

Use this exact finding order and status mapping:

| Status | Finding codes in order |
| --- | --- |
| `invalid` | `route-init.invalid-input`, `route-init.invalid-target`, `route-init.invalid-metadata` |
| `blocked` | `route-init.workspace-unavailable`, `route-init.workspace-unsafe`, `route-init.target-unsafe`, `route-init.route-ambiguous`, `route-init.identity-collision`, `route-init.loader-unsafe`, `route-init.framework-payload-invalid`, `route-init.framework-install-required`, `route-init.framework-update-required`, `route-init.framework-alignment-blocked`, `route-init.metadata-unsafe`, `route-init.generated-region-unsafe`, `route-init.lifecycle-blocked`, `route-init.workspace-lock-unavailable`, `route-init.target-changed`, `route-init.recovery-conflict` |
| `incomplete` | `route-init.framework-payload-unavailable`, `route-init.inspection-incomplete`, `route-init.metadata-incomplete`, `route-init.projection-incomplete`, `route-init.lifecycle-unavailable`, `route-init.recovery-unavailable` |
| `attention` | `route-init.needs-authoring`, `route-init.recovery-artifact-retained` |
| `failed` | `route-init.target-changed-during-apply`, `route-init.write-failed`, `route-init.verification-failed`, `route-init.lifecycle-publication-failed`, `route-init.recovery-failed`, `route-init.operation-failed` |
| `interrupted` | `route-init.interrupted` |

Every finding is exactly `{ code, status, target, cause }`; `complete` has no
finding. Aggregate precedence is `failed`, `interrupted`, `invalid`, `blocked`,
`incomplete`, `attention`, then `complete`.

The exact single-action `next` policy is:

| First applicable condition | `next.command` | `next.reason` |
| --- | --- | --- |
| `complete` | `null` | `null` |
| `invalid` | `open-forge route init --help` | `Correct the named Route Init input, then rerun the request.` |
| `route-init.framework-install-required` | `open-forge install` | `Establish a trusted current Framework installation before rerunning Route Init in Framework mode.` |
| `route-init.framework-update-required` | `open-forge update` | `Update the installed Framework state to the running CLI's embedded inventory before rerunning Route Init.` |
| Workspace lock unavailable or target changed | `open-forge route init` | `Wait for the blocking condition or inspect the changed target, then rerun Route Init from a fresh plan.` |
| Other `blocked` | `open-forge doctor` | `Inspect the blocked workspace, route, identity, lifecycle, generated-region, or recovery boundary before rerunning Route Init.` |
| `incomplete` | `open-forge doctor` | `Inspect the unavailable route, metadata, projection, lifecycle, or recovery facts before relying on this Route Init result.` |
| Retained recovery artifact | `open-forge cleanup` | `Review and remove the reported recovery artifact after confirming the verified Route Init result.` |
| NeedsAuthoring attention | `open-forge route update` | `Author each reported NeedsAuthoring entrypoint before relying on its description or tags.` |
| `failed` | `open-forge route init --verbose` | `Report the failure and retry the same Route Init request with bounded diagnostics.` |
| `interrupted` | `open-forge route init` | `Rerun the same Route Init request.` |

Recovery cleanup wins when both attention conditions coexist.

#### Accepted Decision 2: neutral trusted-current verification

Do not treat `LifecycleStoreReadState.Available` as trusted-current physical
Install state. The current neutral store verifies schema, workspace binding,
source identity, inventory fingerprint shape, target provenance, and persisted
fingerprint shape, but it does not read any lifecycle target. The only current
physical-current implementations are Install-private
`InstallContentIdentity.IsCurrentBaseExact` and
`InstallPreservationVerifier.VerifyAsync`, which Route Init may not import.

Authorize one smallest neutral Framework-lifecycle currentness reader over a
validated `FrameworkLifecycleState`, the running embedded `FrameworkPayload`,
and current contained target reads. It reports typed current, source-mismatch,
changed, missing, unavailable, blocked, and cancelled facts. It applies the
accepted fingerprint policy to source-backed semantic or exact targets, bounded
generated `entries` interiors, and root `AGENTS.md`/`CLAUDE.md` managed blocks;
it validates current `sourceAssetPath` ownership against the running inventory.
Route Init maps those neutral facts to its command-local findings and next
actions. The reader gains no mutation, Update, Install, diagnosis, or result
policy.

The repository's current lifecycle filename is exactly
`.agents/open-forge.lifecycle.json`. Treat its schema-v1 root as one canonical
complete standard envelope: `schemaVersion`, `fingerprintPolicy`,
`workspacePath`, `framework`, and `extensions` are all present in canonical
order and neither standard section is silently assigned missing-state meaning.
When no Extension is installed, `extensions` is the complete canonical empty
state `{ coverage: "complete", packages: [], paths: [] }`, not `null` or an
omitted key. A newly created lifecycle document always writes every standard
root key. Route Init Framework mode never creates this document; it may append
scoped Framework facts only when the existing common envelope and both standard
sections are complete and trusted. A missing, `null`, malformed, unsupported,
or incomplete standard section produces no Route Init write and remains
explicit Update or Doctor work.

The canonical lifecycle correction is integrated at
`1d404c5cef3f5fd464ca771fc132a657f792f533`. The corrected same-read Framework
plan blocks an existing missing, `null`, malformed, or incomplete opposite
Extensions section before semantic no-op classification, so Route Init can use
that plan result as its complete-envelope gate before physical currentness. The
Route Init verifier models this shared truth and does not duplicate or widen the
neutral lifecycle authority.

#### Accepted Decision 3: neutral embedded-source projection

Authorize one narrow embedded-payload-to-source projection. The neutral payload
already exposes exact assets and inventory, and shared source/topology builders
already consume `SourceLogicalSource`, but the only current adapter from a
payload asset to a canonical base source is private inside
`InstallIntendedStateBuilder`. Project only recognized `.agents/...` payload
assets into canonical base sources for one supplied workspace. Install and Route
Init may consume that identical fact; Framework alignment, scope insertion,
scaffold selection, and sparse-chain policy remain Route Init-local. Do not add
a general topology, Template, slug, or mutation engine.

#### Accepted Decision 4: protected integration seams

After Gray and Red establish the accepted command-local boundary, authorize the
minimum protected integration changes: register Route Init below the existing
Route group in `CliCompositionRoot`, register only its JSON presentation graph in
`CliJsonContext`, add only the source-generated YAML types actually required by
the fixed scaffold to `CliYamlContext`, and add its help section to
`RouteHelpSections`. Keep `Commands/Install/**`, other Route command behavior,
the shared envelope, and non-Route help unchanged except for any separately
accepted neutral delegation needed to avoid duplicate lifecycle fingerprint
authority.

### Accepted Evidence Ladder

The task profile is Assured because this is the first public Route
mutation and it combines a new public wire graph, filesystem effects, lifecycle
publication, recovery, concurrency, source-generated serialization, and Native
AOT. The review budget is one independent Sol/xhigh review, no council, and one
grouped correction pass.

| Boundary | Decisive evidence before completion |
| --- | --- |
| Gray | Release compilation of definitions, binding, immutable request/result/presentation models, callable operation stub, exhaustive finite-value mappings, and no production behavior; no tests authored in Gray. |
| Red unit | Required target syntax; singleton and repeatable flag rules; generic and Framework request formation; ID/path validation; rune-aware scope slugging; fixed scaffold bytes and metadata provenance; result invariants, ordering, nullability, finding/status/next mapping, bounded changes, and human/JSON/help/diagnostic projections. |
| Red integration | Real-filesystem chain inspection; compatibility entrypoints; collisions and physical aliases; zero/one/many/consecutive scope alignments; trusted, missing, changed, and outdated Install facts; canonical lifecycle documents with every standard key and complete empty Extensions; rejection of `null`, missing, malformed, or incomplete standard sections; prospective generated navigation; sparse exact embedded bytes; no-op; dry-run parity; lock/revalidation races; directory residuals; file/lifecycle verification; recovery preparation, retention, and deletion. |
| Green focus | Every Red test fails first at the named missing Route Init or neutral callable, then passes without changing frozen test meaning or the accepted contract. |
| Public process | Root composition, streams, all seven exits, JSON-only stdout, diagnostics stderr, exact help, and unchanged neighboring Route commands. |
| Compatibility | Full managed Unit, Integration, and EndToEnd suites; Release build with zero warnings and errors; formatting and generated-output checks. |
| Native AOT | Supported `linux-x64` CLI, Integration, and EndToEnd publication/execution with source-generated JSON/YAML and embedded payload access away from the checkout. |
| Dogfood and review | One isolated generic dry-run/application/no-op journey and one safe Framework dry-run/application/no-op journey when a trusted fixture can be created; own semantic/diff review; one independent Sol/xhigh exact-tip review and at most one grouped repair pass. |

This ladder authorizes the named EndToEnd, Native AOT, neutral Framework, root
composition, serializer-context, and help work only in its ordered implementation
phase. It does not authorize Install-private imports, unrelated Install behavior,
or shared-ledger mutation.

### Gray Closeout

Gray freezes the accepted command surface, immutable request, planning,
application, result, and presentation graphs, exhaustive finite mappings, and
the exact concrete callable stages under `Commands/Route/Init/`. Each
behavior-owning stage fails explicitly through its named Gray
`NotSupportedException`; Gray contains no domain, filesystem, lifecycle,
rendering, or mutation behavior.

Command-local JSON projection, JSON rendering, human rendering, diagnostics, and
help formation are also callable Gray stubs so Red can freeze their observable
behavior without changing production. Protected source-generated context and
Route-group help registration remain later integration work.

The two accepted neutral seams are frozen in new Framework files:

- `EmbeddedFrameworkSourceProjector.Project(CliWorkspace, FrameworkPayload)`
  returns an immutable canonical source projection.
- `FrameworkLifecycleCurrentnessReader.ReadAsync(CliWorkspace,
  FrameworkLifecycleState, FrameworkPayload, CancellationToken)` returns one
  strict typed currentness fact with exact current, source-mismatch, changed,
  missing, unavailable, blocked, or cancelled state. Current and cancelled have
  no path/cause; source mismatch has a bounded cause; target observations retain
  their exact path and bounded cause.

Release compilation is warning-free. Gray changes only new Route Init and the
three new neutral Framework files; it does not import Install-private code or
touch existing LifecycleStore planning, protected composition, serializers,
Route help, neighboring commands, or tests. Its three logical commits were
rebased without conflict onto `6217a34`; `git range-diff` records exact `=`
equivalence, and the resulting Gray tip is
`154398705338ada1caaaf727750df2e30b93c592`.

### Red Evidence Freeze

Red changes only Route Init-owned Unit and Integration evidence plus this Task
record. It does not change production, public composition, serializer contexts,
Route help, neighboring commands, or the accepted contracts. Passing Gray
definition and model assertions are expected; every intended behavior failure
must terminate at its named Route Init or neutral Framework
`NotSupportedException`, never at fixture setup, compilation, or an unrelated
callable.

The Unit packet owns exact grammar and binding normalization; invalid request
and metadata combinations; every finite value, finding/status/ordering and
`next` mapping; immutable result and plan invariants; exact scaffold bytes and
metadata provenance through plan formation; JSON property order, nullability,
bounded changes, human/JSON/help/diagnostic projection; Unicode-rune scope
slugging; embedded-payload source projection; and lifecycle-currentness model
and reader states.

The generic Integration packet owns real-filesystem ID and exact-path chain
inspection; missing and partial ancestors; canonical and compatibility
entrypoints; existing-content preservation; canonical `open-forge` emission and
reading; opaque `rune` preservation and sole `open-forge` selection; complete
and NeedsAuthoring metadata; ordinary and physical collisions; Loader absence,
generated-region replacement, and unsafe boundaries; missing-`.agents`
parent-first directory
effects; dry-run parity; application, no-op, verification, idempotence, recovery,
and unchanged-byte evidence.

The Framework/safety Integration packet owns trusted Install fixture creation;
zero, one, many, and consecutive scope alignment at every position; ID-form
rune-aware slugging and exact-path non-slugging; ambiguous, reordered, nested-root,
and trailing-scope cases; exact embedded bytes and `sourceAssetPath` provenance;
sparse lifecycle publication that preserves complete empty Extensions; missing,
changed, outdated, incomplete, `null`, malformed, or missing-section lifecycle
refusal; physical currentness states; lock contention; revalidation races;
directory residuals; recovery retention/deletion; write, verification, lifecycle,
and cancellation outcomes.

These three packets have disjoint file and fixture ownership. Red contains 65
test declarations and 98 concrete cases. The focused Unit run executes 35 cases:
11 passing Gray contract/model assertions and 24 intended failures at the named
binding, plan, result, presentation, embedded-projection, or lifecycle-currentness
stub. The focused Integration run executes 63 cases: 30 generic and 33 Framework
failures, all at `RouteInitOperation.ExecuteAsync`. Both projects compile in
Release with zero warnings and errors; every intended failure reaches only its
named Gray `NotSupportedException`, and all three packets have zero skips.

### Final Closeout

Route Init is Green and fully proved at exact candidate `cb62b19`, tree
`be93900`. The warning-free Release build passes. Managed Unit `1481/1481` and
Integration `695/695` pass; source-generated serialization is `18/18`; published
Find, Index, and Route Init process evidence passes `14/14`, `5/5`, and `6/6`.
The exact supported `linux-x64` Native AOT publications complete with zero
warnings or errors; Native AOT serialization is `18/18`, full Integration is
`695/695`, and the same published Find, Index, and Route Init suites pass
`14/14`, `5/5`, and `6/6`, all with zero failures and skips.

Detached exact-tip dogfood applies one Generic route, emits canonical flow-style
tags, finds the exact new source, proves Index already-current, and repeats Route
Init as a verified no-op. Parent and target hashes remain unchanged across Find,
Index, and repeat; external lock files are zero-byte; the detached worktree and
temporary workspace are removed without tracked or untracked drift. The broad
repository Find remains truthfully incomplete on historical malformed CLI-v2
documents under fail-closed metadata authority; that existing repair concern is
outside Route Init.

Framework document metadata recognizes and writes only `open-forge`; `rune` is
opaque unrelated YAML. One shared Framework YAML boundary combines a typed
flow/block reader with a source-generated writer that emits canonical flow tags,
and one shared Markdown parser owns the third-party Markdig boundary. Find and
Loader retain only their bounded typed semantic extensions. Route Init has no
command-local YAML writer, generic metadata framework, reflection, dynamic
serializer, or general Route mutation engine.

Fresh independent Sol/xhigh YAML/Markdown-boundary and whole-task reviews both
return `PASS`, with `0.98` confidence, and release exact candidate `cb62b19`.
Future operational aggregate domains will use explicit ordered typed
contributors registered through composition, without reflection, a service
locator, or a dynamic plug-in engine. This records the accepted mechanism only;
it does not choose or change operational-command order.

## Stop Conditions

Stop on ambiguous route alignment, managed-segment reordering, root recreation,
broad subtree ownership, general slug/template machinery, missing trusted root
Install state, lifecycle meaning not defined by accepted contracts, a
command-local directory mechanism, directory rollback or recovery, or behavior
that weakens the accepted external-lease, revalidation, and retained-residual
boundary. Do not move or duplicate the shared external lock identity or merge
its versioned subtree with recovery.
