---
open-forge:
  description: Implement bounded route content and metadata update without identity drift
  tags: [Memory, Working, CLI, Task, Route, Update, Mutation, Contextual]
---

# Implement Route Update

## Task State

- State: Completed; M12 is complete.
- Display mapping: Task 3 “Route Update”.
- Current phase and milestone: phase 7/7, 12/12 milestones complete.
- Responsible role: Overseer-managed Route Update Task Mastermind.
- Branch and worktree: `codex/route-update` at
  `/home/tedy/dev/open-forge-worktree/route-update`.
- Accepted base: `5aad04acdaca1bd90b741397a75e6d99f98cf0d9`, exact tree
  `0f56b2ce96c10b8eefba66d3d402de536221e37c`.
- Last updated: 2026-09-02.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/update/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/update/behavior.md).

This Task is the mutable authority for its phase, milestone, findings, evidence,
task-level review budgets, and consumed IDs. The parent Plan owns program order;
the Checkpoint links to current resumption state. Historical preparation and
completed predecessor identities remain evidence rather than mutable state.

## Expected Outcome

`route update` changes only accepted content or metadata of one exact route while
preserving route identity, path, unrelated authored sections, overwrite ownership,
and generated navigation.

## Architecture

- Separate `RouteUpdateObservation`, command-local `RouteUpdatePlan`, content
  transformation, generated projection, apply receipts, and result.
- Reuse strict source reads, Markdown/frontmatter facts, exact route identity,
  lock/revalidation, atomic replacement, and external recovery-bundle support.
  Prepare one verified bundle covering every existing Replace/Delete before the
  first target effect; no-op plans create none.
- Keep update-field policy and preservation rules command-local.

## Evidence

Cover exact ID/path, ambiguity, detached/compatibility forms, no-op, dry run,
accepted fields, unknown or repeated fields, invalid metadata, overwrite pair,
line endings and preservation, identity race, generated navigation, read/write
failures, bundle retention, no unrelated changes, presentation, process, and AOT.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-update` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. The branch was later fast-forwarded without unique history to the
accepted `5aad04a` integration baseline. Route Create, CLI Quality Remediation,
and the pre-Gray callable/public-result freeze are now complete prerequisites.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Update/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/**`,
  including a command-local real-filesystem fixture.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs` and
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`
  remain protected until public integration. `Framework/**`, `Shell/**`, other
  command behavior, projects, contracts, architecture, and generated artifacts
  remain protected throughout this Task.
- Decisive evidence must cover binding, exact target resolution, preservation of
  unsupported content, metadata/body transformation, intended navigation,
  dry-run/no-op, lock/revalidation/recovery, typed result and process behavior,
  and Native AOT.

### Accepted Pre-Gray Freeze And YAML Meaning

- Architecture authority has frozen the exact wire graph, finding/status
  mapping, `next` precedence, callable stages, allowed paths, evidence budget,
  review budget, commit plan, and stop conditions below.
- Parse the complete frontmatter, edit only recognized fields including
  `responsibility`, and preserve unrecognized YAML source exactly as encountered.
  Use parsed-span edits; do not deserialize and reserialize the whole document.
  Promote only the proven-identical Route Create Template selection and body
  extraction mechanism to the nearest honest Route-shared scope.

## Accepted Execution Horizon

| Phase                          | Milestones and boundary                                                                                                                                                                                  |
| ------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1. Activation and architecture | M1 exact baseline, authority, and capsule; M2 accepted paths, wire graph, callables, evidence, reviews, commits, and stops.                                                                              |
| 2. Gray                        | M3 callable/model skeleton plus neutral Template contract compiles warning-free.                                                                                                                         |
| 3. Red                         | M4 frozen Unit evidence; M5 frozen Integration and process evidence.                                                                                                                                     |
| 4. Green                       | M6 observation/patch/Template/planning; M7 application/recovery/verification/result/rendering; M8 protected root/help/public integration.                                                                |
| 5. Blue                        | M9 bounded production-structure audit and correction, or an evidence-backed skip.                                                                                                                        |
| 6. Purple                      | M10 bounded test/evidence-structure audit and correction, or an evidence-backed skip.                                                                                                                    |
| 7. Review and acceptance       | M11 full managed/native/dogfood evidence plus one immutable coordinated-review snapshot; M12 dispositions, one grouped correction, affected rechecks, fresh holistic review, acceptance, and continuity. |

Completed numerators do not regress. A material horizon change requires a new
explicit denominator rather than rewriting this one.

## Accepted Dependency Direction And Placement

```text
Shell binding and presentation
  -> Route/Update policy and orchestration
      -> Route/Shared/Templates neutral Template mechanism
      -> Framework source, Markdown, and YAML facts
      -> Framework generated-navigation projection
      -> Framework mutation and recovery mechanics
```

- Route Update owns field-patch policy, observations, intended bytes, planning,
  findings, results, rendering, and top-level orchestration.
- Existing Framework capabilities remain neutral dependencies. No Framework
  dependency points toward a command.
- Promote only existing-source reference resolution, ordinary routed Markdown
  classification, the exact `Template` tag, overwrite rejection, strict reads,
  frontmatter stripping, and exact body-byte extraction. Create and Update keep
  command-local finding/status adapters.
- Compose the graph visibly in `RouteUpdateOperationFactory.Create` with typed
  dependencies and optional `WorkspaceLockStoreRoot`. Add no DI container,
  service locator, generic Route engine, parameter/service bag, or subprocess.
- Keep `RouteUpdateJsonContext` co-located with the Update JSON renderer. Do not
  centralize command-result source generation or restore the former broad
  `CliJsonContext` pattern.

## Frozen Command Grammar

```text
open-forge route update <source-reference>
  [--description <text>]
  [--responsibility <text>]
  [--tag=<tag>]...
  [--template <template-reference>]
  [--dry-run]
  [global flags]
```

- Require exactly one source operand and at least one metadata or Template
  operation.
- Description, responsibility, and Template are singleton; any second
  occurrence is invalid even when equal.
- Tag occurrences form one complete ordered replacement list. Empty, prefixed,
  noncanonical, and exact duplicate values are invalid.
- Exact empty responsibility removes the field. Whitespace-only nonempty input
  is invalid. Repeated dry-run is idempotent.
- Shared global flags retain existing meaning. Add no alias, wizard, body flag,
  `--force`, `--yes`, generic patch language, inferred Template, or replacement
  mode.

## Frozen Public Result

Schema-v1 JSON uses the exact property order and nullability below. Arrays are
initialized; nullable members represent genuinely unavailable, omitted, or
absent facts.

```text
document {
  schemaVersion : int
  command       : string
  status        : string
  workspace     : workspace | null
  result        : result
  next          : next | null
}

workspace { path : string, selectedBy : string }

result {
  mode           : string
  target         : target
  patch          : patch
  template       : template | null
  plan           : plan
  effects        : effect[]
  unchangedPaths : string[]
  recovery       : recovery
  verification   : string
  findings       : finding[]
}

target {
  requested      : string
  selectedBy     : string | null
  id             : string | null
  path           : string | null
  form           : string | null
  overwritePaths : string[]
}

patch {
  description { requested : bool, before : string | null,
                expected : string | null, state : string }
  responsibility { requested : bool, operation : string,
                   before : string | null, expected : string | null,
                   state : string }
  tags { requested : bool, before : string[] | null,
         expected : string[] | null, state : string }
}

template {
  requested      : string
  id             : string | null
  path           : string | null
  classification : string | null
  bodyByteLength : long | null
  decision       : string
}

plan { completeness : string, safety : string, body : string }
effect {
  path     : string
  kind     : string
  action   : string
  change   { before : string, expected : string }
  preview  : previewHunk[]
  outcome  : string
  residual : string
}
previewHunk { kind : string, before : string, expected : string }
recovery { state : string, residualPath : string | null }
finding { code : string, status : string, target : string | null, cause : string }
next { command : string, reason : string }
```

- Template is null only when omitted. A requested unresolved Template retains
  its requested value and nullable unresolved facts.
- Update effects are Replace-only and always have non-null change facts. Both
  hashes are required because every Update target already exists.
- Preview hunks expose only changed recognized-field syntax, eligible
  whitespace/Template body content, or generated interiors—never unrelated
  authored bytes or recovery contents.
- Effects are unique by physical path. Target metadata, body, and self-region
  changes coalesce into one routed-file replacement; the exposing-parent
  generated-region replacement follows it.
- Unchanged paths are unique ordinal output over only the potential mutation
  neighborhood. A Template is an observation dependency, not a mutation target.

### Finite machine values

| Concept                  | Values                                                                            |
| ------------------------ | --------------------------------------------------------------------------------- |
| Mode                     | `apply`, `dry-run`                                                                |
| Target selection         | `source-id`, `base-path`, `overwrite-path`                                        |
| Target form              | `ordinary-markdown`, `canonical-entrypoint`, `compatibility-entrypoint`           |
| Patch state              | `not-requested`, `unresolved`, `unchanged`, `changed`                             |
| Responsibility operation | `not-requested`, `set`, `remove`                                                  |
| Template classification  | `template`                                                                        |
| Template decision        | `unresolved`, `copied`, `authored-body-protected`                                 |
| Plan completeness        | `not-established`, `incomplete`, `complete`                                       |
| Plan safety              | `not-established`, `safe`, `blocked`                                              |
| Body state               | `not-established`, `preserved`, `template-copied`, `authored-body-protected`      |
| Effect kind              | `routed-file`, `generated-region`                                                 |
| Effect action            | `replace`                                                                         |
| Preview kind             | `metadata-field`, `template-body`, `generated-region`                             |
| Effect outcome           | `planned`, `not-started`, `verified`, `verification-failed`, `completion-unknown` |
| Effect residual          | `none`, `retained`, `unknown`                                                     |
| Recovery                 | `not-required`, `not-created`, `removed`, `retained`, `unknown`                   |
| Verification             | `not-requested`, `verified`, `failed`, `unknown`                                  |

Every enum-to-wire mapping is a named exhaustive switch with a throwing discard.

### Findings, statuses, and next action

| Status        | Finding codes                                                                                                                                                                                                                                                    |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `invalid`     | `route-update.invalid-input`, `.invalid-target`, `.invalid-patch`, `.invalid-template`                                                                                                                                                                           |
| `blocked`     | `.workspace-unsafe`, `.target-unsafe`, `.route-ambiguous`, `.identity-collision`, `.frontmatter-unsafe`, `.metadata-preservation-unsafe`, `.template-unsafe`, `.generated-region-unsafe`, `.workspace-lock-unavailable`, `.target-changed`, `.recovery-conflict` |
| `incomplete`  | `.workspace-unavailable`, `.inspection-incomplete`, `.template-unavailable`, `.projection-incomplete`, `.recovery-unavailable`                                                                                                                                   |
| `attention`   | `.template-body-protected`, `.recovery-artifact-retained`                                                                                                                                                                                                        |
| `failed`      | `.target-changed-during-apply`, `.write-failed`, `.verification-failed`, `.recovery-failed`, `.operation-failed`                                                                                                                                                 |
| `interrupted` | `.interrupted`                                                                                                                                                                                                                                                   |

Binding failure is invalid. After effects begin, unexpected application,
verification, or unknown-recovery failure is failed; cancellation remains
interrupted unless a failure supersedes it. Ordinary precedence is
`blocked > incomplete > attention > complete`. Protected Template attention
requires a complete safe plan or verified application. Recovery
`Failed/Retained` is attention; `Failed/Unknown` is failed. Planned changes alone
never create attention.

Shared process mappings remain complete `0`, attention `2`, incomplete `3`,
invalid `4`, blocked `5`, failed `1`, and interrupted `130`. JSON always emits
one document on stdout. One next action uses this precedence: retained recovery
cleanup; protected-body review; invalid help; lock/changed retry; other
blocked/incomplete Doctor; failed verbose retry; interrupted retry; complete
null. Cleanup wins when both attention findings coexist.

## Frozen Callable Stages

| Stage                    | Callable responsibility                                                                                                                                       |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Binding                  | `Bind(RouteUpdateBindingComponents)` forms occurrence facts, validation, and request.                                                                         |
| Shared Template resolver | `ResolveAsync(RouteTemplateResolutionRequest, CancellationToken)` selects and extracts identical Route Template facts.                                        |
| Target observer          | `ObserveAsync(RouteUpdateObservationRequest, CancellationToken)` establishes catalogue, reference, base/overwrite, exact bytes, frontmatter, and route facts. |
| Metadata patcher         | `Build(RouteUpdateMetadataPatchInput)` forms parsed-span recognized-field edits.                                                                              |
| Body planner             | `Build(RouteUpdateBodyPlanInput)` preserves authored body or copies exact Template bytes.                                                                     |
| Destination planner      | `BuildAsync(RouteUpdateDestinationInput, CancellationToken)` coalesces metadata/body/self-region and validates intended form.                                 |
| Navigation planner       | `BuildAsync(RouteUpdateNavigationInput, CancellationToken)` projects parent/self navigation from intended facts.                                              |
| Plan projector           | `Build(RouteUpdatePlanProjectionInput)` forms comparisons, ordered effects, previews, and result facts.                                                       |
| Plan builder             | `BuildAsync(RouteUpdateRequest, CancellationToken)` owns observation through complete plan or typed stop result.                                              |
| Revalidator              | `RevalidateAsync(RouteUpdatePlan, CancellationToken)` rechecks all expected target, Template, route, and navigation facts.                                    |
| Recovery lifecycle       | Typed prepare/complete methods own one external bundle.                                                                                                       |
| Effect application       | A typed input produces ordered replacement receipts.                                                                                                          |
| Applied verifier         | A typed input verifies effects and the semantic postcondition.                                                                                                |
| Application operation    | Lock, revalidate, recover, apply, verify, cleanup, and form result.                                                                                           |
| Top operation            | One request branches through dry-run, no-op, or application.                                                                                                  |
| Result/rendering         | One typed result feeds human, JSON, diagnostics, and help.                                                                                                    |

Constructors take one to five cohesive typed dependencies. Required-init stage
models replace long parameter lists; parameter/options/service bags are forbidden.

## Parsed-Span Algorithm

1. Read the base through existing strict UTF-8 and snapshot boundaries. Invalid
   UTF-8 or a BOM-obscured frontmatter boundary fails closed.
2. Require one complete Markdown frontmatter boundary and parse the complete YAML
   span with `YamlDocumentParser`.
3. Require one root mapping and exactly one scalar `open-forge` key with a mapping
   value. Detect duplicate recognized keys; preserve unknown entries as opaque
   exact source.
4. Read requested fields from parsed nodes. A missing supplied supported field
   may be added; final description and tags remain mandatory.
5. Use `FrameworkDocumentMetadataEmitter` only as a source-generated canonical
   value oracle. Parse its temporary output and extract requested scalar/sequence
   spelling; never replace the target with that emitted document.
6. Replace existing fields by parsed value/gap spans. Add or remove responsibility
   only through proven member and line boundaries. Ambiguous flow style, comments,
   indentation, or separators block as metadata-preservation-unsafe.
7. Convert YAML UTF-16 character spans to strict UTF-8 byte offsets, apply
   nonoverlapping edits in descending order, and copy untouched slices verbatim.
8. Add a field with the immediately adjacent scoped member's indentation and line
   ending. Block without a deterministic local convention; preserve mixed endings
   elsewhere.
9. Classify the decoded target body with Unicode whitespace. Preserve every byte
   when any non-whitespace exists. Otherwise replace only the eligible body with
   exact Template body bytes after the existing delimiter line ending.
10. Reparse and validate the complete intended Markdown, metadata, and form. Do
    not implement a custom parser, whole-document deserialize/reserialize,
    semantic normalization, or arbitrary repair.
11. Project navigation from intended facts. Coalesce a target self-region with
    the routed-file replacement and order the exposing parent afterward.

## Exact Scope And Paths

| Scope                | Paths and meaning                                                                                                                                                                                         |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Expected production  | `src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/**`.                                                                                                                                               |
| Nearest shared       | `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Templates/**`; only proven-identical Template resolution/body extraction.                                                                          |
| Create adaptation    | `Route/Create/Shared/Planning/RouteCreateTemplateResolver.cs`, `Route/Create/Models/Planning/RouteCreateTemplateResolution.cs`, and directly affected Template tests; preserve exact observable behavior. |
| Unit                 | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Update/**` and directly affected `Route/Shared/Templates/**` tests.                                                                       |
| Integration          | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/**` and directly affected `Route/Shared/Templates/**` tests.                                                              |
| EndToEnd             | `PublishedRouteUpdateProcessTests.cs` and `PublishedRouteUpdateWorkspace.cs`.                                                                                                                             |
| Protected until M8   | `CliCompositionRoot.cs` and `RouteHelpSections.cs`.                                                                                                                                                       |
| Protected throughout | `Framework/**`, `Shell/**`, other command behavior/tests, projects/configuration, contracts, architecture, and generated artifacts.                                                                       |
| Working continuity   | This Task, parent Task, task index, Plan, Overseer Memory, and CLI Checkpoint; do not rewrite sealed history or invent a Task-2 repository record.                                                        |

Expected paths forecast the implementation; a directly required neighboring
integration file still requires reporting. Protected paths remain absolute until
the Overseer changes them.

## Acceptance Matrix And Evidence

| ID  | Boundary                                                                                                                           | Decisive evidence                                                       |
| --- | ---------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| B1  | Operand/flag occurrences, all mappings, findings, statuses, and next actions.                                                      | Frozen Unit plus binding/process tests.                                 |
| B2  | ID/base/overwrite selection; ordinary/canonical/compatibility targets; ambiguity, orphan, detached, and protected kinds.           | Unit plus real-filesystem Integration.                                  |
| B3  | Add/replace/remove/omit recognized fields while preserving unknown YAML, comments, encoding, and line endings; unsafe spans block. | Unit matrix plus byte-level Integration hashes.                         |
| B4  | Create-equivalent Template resolution; eligible copy; exact authored body and overwrite preservation.                              | Differential Create tests plus Update Unit/Integration.                 |
| B5  | Intended target/self/parent projections coalesce and order correctly; responsibility-only avoids generated work.                   | Unit projection plus real-filesystem Integration.                       |
| B6  | Dry-run, no-op, lock/races, recovery, partial failure, verification, and interruption follow the accepted lifecycle.               | Integration with exact receipts and residual facts.                     |
| B7  | Human/JSON order and nulls, all seven statuses, streams, exits, compact/verbose output, and one next action.                       | Unit serialization plus published EndToEnd.                             |
| B8  | Public composition and source generation survive Native AOT; dogfood changes only accepted bytes.                                  | Full managed, fresh native publish/run, and before/after hash manifest. |

Final evidence includes locked restore, warning-free Release build, complete
managed Unit/Integration/EndToEnd, fresh supported `linux-x64` Native AOT root,
Integration, and EndToEnd publication/execution, ELF/version smoke, formatter,
diff check, static architecture searches, and disposable-workspace dogfood.
Receipts bind root, commit/tree, configuration identity, exact command/toolchain,
fresh artifact, selected/discovered/executed counts, failures, skips, warnings,
exit, and limits. Zero execution, skips, warning-bearing output, stale
publication, or wrong-scope negative searches are not passes.

## Execution And Review Capsule

- Current owner: Route Update Task Mastermind supervising one continuous
  Sol/xhigh C# writer, `route_update_writer`, through integrated Green.
- Every C# author or reviewer independently reads the complete current
  `_csharp.md`, `design.md`, and `style.md`; a summary or inherited fingerprint
  is not a substitute.
- Review budget: maximum nine named invocations. One M11 wave reserves
  `T3-M11-CS-01`, `T3-M11-ARCH-01`, `T3-M11-BEH-01`, and
  `T3-M11-EVID-01`; affected targeted rechecks may consume matching `-R1-01`
  IDs after one grouped correction; fresh holistic review is
  `T3-M12-HOL-01`. Coordinator validation and synthesis consume no unit.
- Council budget: zero. Correction budget: one grouped writer-owned cycle.
- Commit plan: `Activated Route Update implementation`; `Shared route Template
resolution between Create and Update`; `Implemented bounded Route Update
planning and application`; `Integrated Route Update into the public CLI`;
  `Separated Route Update production responsibilities`; `Strengthened Route
Update preservation evidence`;
  optional `Corrected Route Update review findings` after accepted findings.

Coherent green commits are immutable snapshots. One Review Mastermind coordinates
one active wave and routes only triggered topics. Reviewers remain read-only and
never disposition. This Task Mastermind revalidates findings, records dispositions,
and gives one grouped repair packet to the original writer. Fresh holistic
Sol/xhigh review remains a separate final gate.

## Current Progress And Risks

- Current result: M1–M12 are complete. M4 and M5 freeze the accepted Unit,
  Integration, and process evidence. M6 planning Green is owner-accepted after
  its grouped correction for C# conformance, cancellation, overwrite
  observation, and snapshot coherence. M7 application, recovery, verification,
  result formation, and command-local rendering are owner-accepted after the
  grouped semantic-verification and human-contract correction. M8 public
  root/help integration, M9 bounded production-structure correction, and M10
  bounded test/evidence-structure correction are owner-accepted. M11 full
  managed/native/dogfood evidence and its coordinated topic review are complete.
  M12 owner-froze the joined review dispositions, completed the two authorized
  grouped corrections, and passed fresh holistic review and acceptance.
- M3 supervision removed two forwarding-only pipeline wrappers and one needless
  content wrapper before evidence. `RouteUpdatePlanBuilder` now directly owns
  observer/destination/navigation/projection, Destination owns Template/metadata/body,
  and Application owns one cohesive preparer plus apply/verify/result/lock.
- Accepted M6 evidence on SDK `10.0.111`: Core, Unit, and Integration
  Release builds pass with zero warnings/errors. The exact Route Update Unit
  selection discovers and executes `61` tests: `56` pass, `5` intentionally
  remain Red only at the M7/M8 human, JSON, diagnostic, and help seams, and `0`
  skip. The exact Integration selection discovers and executes `43` tests: all
  `30` planning cases pass, `13` intentionally remain Red only at the M7
  application, recovery, revalidation, verification, cancellation, and
  top-level-operation seams, and `0` skip. Folder-mode whitespace formatting and
  verification pass for every changed C# path, and `git diff --check` passes.
  Route-shared Template production did not change during M6, so the conditional
  Route Create `24` Unit and `22` Integration equivalence rerun was not triggered.
- Accepted M7 evidence on SDK `10.0.111`: Core, Unit, and Integration Release
  builds pass with zero warnings/errors. The exact Route Update Unit selection
  selects, discovers, and executes `68` tests: `67` pass, only the intentional M8
  help test remains Red, and `0` skip. The exact Integration selection selects,
  discovers, and executes `44` tests: `44` pass and `0` skip. Folder-mode
  whitespace verification, `git diff --check`, protected-path audit, and C#
  conformance searches pass. Route-shared Template production remains unchanged.
- M8 candidate evidence on SDK `10.0.111`: Core, root, Unit, Integration, and
  EndToEnd Release builds pass with zero warnings/errors. The exact Route Update
  Unit selection selects, discovers, and executes `68` tests: `68` pass and `0`
  skip. The exact Integration selection selects, discovers, and executes `44`
  tests: `44` pass and `0` skip. The exact fresh-publish Route Update process
  selection selects, discovers, and executes `4` tests: `4` pass and `0` skip
  under the system .NET 10 runtime. It proves root/group/leaf discovery, the
  frozen grammar and excluded options, every shared status/exit/stream mapping,
  JSON order and nulls, verbose separation, exact apply bytes and navigation,
  no-op convergence, and write-free failure paths. The seven command-local help
  sections expose singleton/repeatable semantics, exact-empty responsibility
  removal, Template authored-body protection, and dry-run/write policy. Folder
  whitespace verification, `git diff --check`, protected-path audit, line-length
  and C# conformance searches pass. Route-shared Template production remains
  unchanged. M8 is owner-accepted.
- Activation C# directive SHA-256 identities are `_csharp.md`
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `design.md` `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and `style.md` `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- M12 is complete. No project blocker is open.
- The accepted M9 packet replaces the material physical-file sharding with the
  frozen named capabilities: metadata layout reading, edit planning, direct
  byte-slice application, and boundary projection; source selection, layer
  observation, and observation boundary projection; recovery preparation,
  completion, and result projection; pure plan equivalence; and one real
  under-lease application pipeline. Metadata and source-selection state lives
  in cohesive Planning models, the application pipeline has one cohesive
  Operation input, the root operation and narrowed application operation are
  non-partial, and focused unresolved/recovery factories live on their owning
  result records. Explicit factory composition constructs every named
  dependency. The exact `19` superseded partial files are deleted. No Framework,
  shared Template, schema, grammar, public behavior, or evidence expectation
  changed. Owner inspection also removed a redundant broad Observation member
  from the metadata-edit input before acceptance.
- Accepted M9 evidence on SDK `10.0.111`: Core, Unit, Integration, and
  EndToEnd Release builds pass with zero warnings/errors. The exact Route Update
  Unit selection selects, discovers, and executes `68` tests: `68` pass and `0`
  skip. The exact Integration selection selects, discovers, and executes `44`
  tests: `44` pass and `0` skip. The exact fresh-development-publish process
  selection selects, discovers, and executes `4` tests: `4` pass and `0` skip
  under the system .NET 10 runtime. Folder-mode whitespace verification,
  `git diff --check`, protected-path/shared-Template audits, deleted-surface and
  prohibited-pattern searches, and the changed-line length check pass. M9 is
  owner-accepted.
- M9 was triggered by physical-file sharding that hid the complete ownership of
  `RouteUpdateMetadataPatcher` (`883` lines across `9` partials),
  `RouteUpdateTargetObserver` (`614`/`4`), `RouteUpdateRecoveryLifecycle`
  (`394`/`4`), and `RouteUpdatePlanRevalidator` (`257`/`2`), plus explicit
  ownership review of `RouteUpdateApplicationOperation` (`278`/`3`) and root
  `RouteUpdateOperation` (`181`/`2`). The candidate resolves those triggers with
  the named narrow capabilities and non-partial orchestrators recorded above;
  no material partial sharding remains on those surfaces.
- M9 inspection keeps `RouteUpdateDefinitions` as one finite command-contract
  policy and `RouteUpdateResult` as the owner of atomic graph validation. Human
  and JSON projections remain cohesive command-local mapping helpers. The
  NavigationPlanner, PlanProjector, AppliedVerifier, ApplicationPreparer,
  Binding, ApplicationResultFactory, and ResultBuilder also remain cohesive in
  their accepted ownership; none gains a forwarding wrapper or generic engine.
- M10 added four focused Unit cases for flow-style YAML addition/removal refusal,
  byte-exact preservation of comments around responsibility removal, and refusal
  when responsibility owns an attached comment. It also added one Integration
  differential case proving that Route Create and Route Update expose identical
  resolved Template selection, source, length, and body-byte facts. No production,
  fixture, public, project, or configuration path changed.
- Accepted M10 evidence on SDK `10.0.111`: Unit and Integration Release builds
  pass with zero warnings/errors. The exact Route Update Unit selection selects,
  discovers, and executes `72` tests: `72` pass and `0` skip. The exact
  Integration selection selects, discovers, and executes `45` tests: `45` pass
  and `0` skip. Project whitespace verification, `git diff --check`, path and
  static-surface checks pass. The M9 EndToEnd `4/4` receipt remains applicable
  because M10 changed only Unit and Integration evidence; M11 will execute the
  complete fresh managed/native/process boundary.
- M10 retained the existing decisive byte-exact coverage for UTF-16-to-UTF-8
  span conversion, mixed endings, Unicode whitespace, self-region coalescing,
  overwrite, and plan revalidation. Recovery failure injection remains deferred
  because no stable accepted seam exists; collision, retained, and unknown
  recovery outcomes remain covered by existing Integration evidence.
- Develop advanced independently through `996c2e17d1142ffb30dc7a2d17df657419566f97`
  (tree `877314d48db49013edc1c4dad1abb545b37caefc`). The merge base remains
  `5aad04acdaca1bd90b741397a75e6d99f98cf0d9`; its overlap is limited to five
  working-continuity paths and contains no Route Update production, test,
  project, dependency, or lockfile change. Integration must reconcile those
  continuity paths after task acceptance rather than merging during M12.
  That develop authority already resolves `T3-M11-ARCH-01-F002` by synchronizing
  the continuity state and `T3-M11-ARCH-01-F003` by recording the Generated
  Navigation dependency. Those findings are accepted-and-fixed-by-develop; this
  branch does not copy or rewrite the overlapping continuity prose.

### M12 Joined Review Disposition And Grouped Correction

- The immutable M11 review packet contains `19` stable findings. Owner
  disposition accepts `15`; accepts `T3-M11-ARCH-01-F002` and
  `T3-M11-ARCH-01-F003` as already fixed by develop `44b31232`; treats
  `T3-M11-BEH-01-F001` as the duplicate of `T3-M11-ARCH-01-F001`; and defers
  only portable interrupted-process proof from `T3-M11-EVID-01-F001` while
  retaining deterministic real failed-process proof in this correction. No
  finding is rejected, reduced to preference, or classified false positive.
- The active grouped correction makes the planning result carriers exact unions;
  validates Template-copied entrypoint navigation and generated representation
  while leaving ordinary Template-only body copies dependency-minimal;
  verifies final overwrite bytes; converts unexpected application-stage faults
  to conservative typed outcomes without inventing receipts; shows compact apply
  previews; normalizes accepted target spellings; and adds real application,
  byte-preservation, race, cancellation, failure, presentation, JSON, and process
  evidence. Framework, shared Template behavior, public schema, and command
  contracts remain unchanged.
- `T3-M11-BEH-01-F006` exposed parser information loss rather than a Route Update
  grammar change. Overseer-authorized exception `CLI-EDGE-005` carries immutable
  typed parse facts together with original arguments. Route Update inspects raw
  spelling only after typed zero-token responsibility facts, recognizes exact
  `--responsibility=` or `--responsibility:`, stops at `--`, and never carries raw
  arguments into its request or domain. The ineffective identifier-token
  workaround is removed; parser selection, occurrence, value, and diagnostic
  ownership do not move.
- Focused correction receipts on SDK `10.0.111` are warning-free: Route Update
  Unit selects/discovers/executes `76` with `76` pass and `0` skip; parser
  Integration selects/discovers/executes `46` with `46` pass and `0` skip; Route
  Update Integration selects/discovers/executes `65` with `65` pass and `0` skip;
  published Route Update EndToEnd selects/discovers/executes `6` with `6` pass and
  `0` skip. The native-root real verification-failure case additionally passes
  `10/10` consecutive focused executions after the evidence fixture establishes
  its monitor before process launch and preserves a `16 MiB` authored-body
  verification window. Mid-application cancellation likewise passes `10/10`
  consecutive focused managed executions with a ready dedicated monitor and a
  `16 MiB` first-effect verification window; no product fault seam is present.
- Owner inspection narrowed generated-navigation planning after the joined repair:
  a Template copy triggers navigation only when the selected target is an
  entrypoint. Focused evidence proves an ordinary Template-only copy has zero
  navigation regions and one target effect, while all five canonical and
  compatibility entrypoint forms retain target-plus-parent projection and ordered
  physical-effect coalescing.
- Candidate authority is repository root
  `/home/tedy/dev/open-forge-worktree/route-update`, committed parent
  `d8b8a33e3a67910fe71f5590f2ed24149c9c38cb`, tree
  `e0b8f85a0a22f023c2a4fe55ae5015cc470f7fa2`, parent
  `a69a586ead1ea22507fb52ef7bfb86a52ca66113`, accepted base
  `5aad04acdaca1bd90b741397a75e6d99f98cf0d9`, and develop authority
  `44b31232fae61bee6f172771ec1b70d123ce09d5`. The SDK is `10.0.111`.
  Configuration SHA-256 identities are `global.json`
  `15fc2962d00bbb3febae9ddb01301f0723ebb743e1cfcd57af3d1fdd3fb640bb`,
  `Directory.Build.props`
  `0bad18b7db5342f56767d54c4a0b66aef2d627d9586e7be9254eeac2351ebc4e`,
  `Directory.Packages.props`
  `2af79560f3e7a11aa53219b609050b3c809ca1a79bd0be01f49ff6b190af7c2f`,
  and `OpenForge.Cli.slnx`
  `49d2d12fc8c456ed9386f5b5f64faa8ba8c92d555f193e96f3454fb07e8ba883`.
- Exact managed commands are
  `dotnet restore OpenForge.Cli.slnx --locked-mode --nologo` and
  `dotnet build OpenForge.Cli.slnx -c Release --no-restore --nologo -p:OpenForgeSkipDevelopmentPublish=true`,
  followed by direct Release executions of
  `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests`,
  `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests`,
  and `artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests`
  with `DOTNET_ROOT=/usr/lib/dotnet`. Restore covers all six projects; build has
  `0` warnings and `0` errors; complete Unit is `1598/1598`, Integration is
  `802/802`, and EndToEnd is `152/152`, all with `0` failures and `0` skips.
- Exact native publication commands are
  `dotnet publish src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime linux-x64 --no-restore --output artifacts/publish/linux-x64/open-forge -p:OpenForgeSkipDevelopmentPublish=true --nologo`,
  `dotnet publish src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --runtime linux-x64 --no-restore --output artifacts/publish/linux-x64/integration -p:OpenForgeSkipDevelopmentPublish=true --nologo`,
  and
  `dotnet publish src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --runtime linux-x64 --no-restore --output artifacts/publish/linux-x64/end-to-end -p:OpenForgeSkipDevelopmentPublish=true --nologo`.
  Managed EndToEnd rebuilt
  with `-p:OpenForgeEndToEndTargetRuntimeIdentifier=linux-x64` passes `152/152`
  against the native root. Direct native Integration passes `802/802`; direct
  native EndToEnd passes `152/152`; both have `0` failures and `0` skips.
  All three artifacts are stripped x86-64 ELF PIE executables. Root version is
  `0.0.0-dev`. Final SHA-256 identities are root
  `b6c5b896d4b7f39b68c73bdb485192ad1621208dcecf6d0f8501745035ba7aa4`,
  root marker
  `fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4`,
  native Integration
  `a2cce98f45c0ec0f1fd7bf9233e1061959568ccec89e53d117f0c1ddeb196e84`,
  and native EndToEnd
  `b68c8ac545159571fb693606d6b280bc6a87527fd11a786296181fee8288be72`.
- Final native dogfood uses exact normalized overwrite reference
  `./.agents/memory/project-alpha/overview.overwrite.md`, exact-empty
  `--responsibility=`, description `M12 overview`, tags `After` and `Memory`,
  and preview/apply/repeat modes. Preview exits `0` and leaves every hash exact.
  Apply exits `0`, verifies only target hash
  `1c305aeb9ad137f1978805bf5bc2aec7859a7c8724ec9b0c4ac77bfd63e7f8c4`
  to `1e727492858e7332e2e7669c6a2cd9aa501b93c7f81f95a96e8ab194fb837494`
  and parent hash
  `f7dc8ae12149bbb4676a39f3ce96dbbb61704f53b64e3b387a79c2464e92edfc`
  to `03705a7c47b798decb4ae4ac356416cf84d03545353b7b6a2581d3a48f98afef`.
  Loader, overwrite, Template entrypoint, Template, authored body, and unrelated
  hashes remain exact; repeat exits `0` with no effects and recovery
  `not-required`. External recovery contains no files; the one persistent lock is
  zero bytes.
  The exact preview/apply/repeat argv is
  `env XDG_DATA_HOME=/home/tedy/dev/open-forge-worktree/route-update/.temp/m12-route-update-dogfood-final/local-data /home/tedy/dev/open-forge-worktree/route-update/artifacts/publish/linux-x64/open-forge/OpenForge.Cli route update ./.agents/memory/project-alpha/overview.overwrite.md --description 'M12 overview' --responsibility= --tag=After --tag=Memory`,
  with `--dry-run` appended for preview, no appended mode for apply, and `--json`
  appended for repeat.
- Owner-correction native dogfood uses
  `env XDG_DATA_HOME=/home/tedy/dev/open-forge-worktree/route-update/.temp/m12-route-update-owner-correction-dogfood/local-data /home/tedy/dev/open-forge-worktree/route-update/artifacts/publish/linux-x64/open-forge/OpenForge.Cli route update ./.agents/memory/project-alpha/overview.md --template=.agents/templates/route.md`.
  Preview with `--dry-run` and apply each exit `0` with exactly one target effect
  and parent reported unchanged. The target hash changes from
  `8072cad561ddac96d2fc26958a2518384c8b45c48d457a8c560861e915c5dc88`
  to `71dcaf07bfdfd17da350be3094935c05501a0ed40c404cca59fe38db256032de`;
  parent, overwrite, loader, Template entrypoint, Template, and unrelated hashes
  remain exact. Repeat with `--json` exits the accepted attention code `2`, has
  no effects, preserves every hash, reports authored-body protection, creates no
  recovery file, and leaves one zero-byte external lock.
- Folder whitespace verification passes with
  `dotnet format whitespace src/cli/core/OpenForge.Cli.Core/Commands/Route/Update --folder --verify-no-changes --verbosity minimal`,
  `dotnet format whitespace src/cli/core/OpenForge.Cli.Core/Shell --folder --verify-no-changes --verbosity minimal`,
  `dotnet format whitespace src/cli/tests/unit/OpenForge.Cli.Core.UnitTests --folder --verify-no-changes --verbosity minimal`,
  `dotnet format whitespace src/cli/tests/integration/OpenForge.Cli.IntegrationTests --folder --verify-no-changes --verbosity minimal`,
  and
  `dotnet format whitespace src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests --folder --verify-no-changes --verbosity minimal`.
  `git diff --check` passes. Exact raw-argument audit
  `rg -n "OriginalArguments" src/cli/core/OpenForge.Cli.Core -g '*.cs'` finds only
  parse ownership, terminal delimiter validation, carrier construction, and Route
  Update binding consumption. Exact protected audit
  `git diff --name-only -- src/cli/core/OpenForge.Cli.Core/Framework src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Templates src/cli/core/OpenForge.Cli.Core/Commands/Route/Create src/cli/root src/cli/*.csproj Directory.Build.props Directory.Packages.props global.json NuGet.Config`
  is empty. Prohibited DI/service-locator/generic-engine and changed
  nested-conditional/null-forgiving searches are empty. The grouped correction is
  owner-accepted for an immutable coherent commit; the later `T3-M12-C2`
  correction and fresh holistic acceptance supersede this pending state.

### T3-M12-C2 Holistic Contract Correction

- The Overseer approved one additional and final grouped-correction budget
  revision after fresh holistic review `T3-M12-HOL-01` reproduced three public
  contract defects on immutable reviewed commit
  `c902f668cbaa9a6ae5fd65cbfb72ed7cb6c4c502`, tree
  `d129f66e4d957fae3e38d0f076818aedae884956`. `T3-M12-C2` accepts
  `T3-M12-HOL-01-F001`, `T3-M12-HOL-01-F002`, and
  `T3-M12-HOL-01-F003` without changing the public schema, grammar, or
  external-effect boundary.
- `T3-M12-HOL-01-F001` now keeps deterministic target validity distinct from
  inspection availability: unknown IDs, missing exact paths, safely known
  detached sources, and orphan overwrites are invalid targets; unavailable
  route facts remain incomplete for ID, base-path, and overwrite-path
  selection; ambiguous or unsafe identity remains blocked.
- `T3-M12-HOL-01-F002` now keeps deterministic missing Template references
  invalid for both Route Create and Route Update while preserving
  post-catalogue disappearance or read unavailability as incomplete. Shared
  differential evidence proves the same resolution state, issue, finding, and
  empty body facts for Create and Update.
- `T3-M12-HOL-01-F003` supersedes only the stale task-local next-action phrase
  above. The accepted precedence is retained-recovery cleanup first;
  protected-body review only for final `attention`; then status-specific
  invalid, blocked, incomplete, failed, or interrupted guidance. Composite
  findings preserve the protected-body observation without allowing it to
  replace a non-attention status action.
- Red-first receipts were exact and non-vacuous: focused Unit selected,
  discovered, and executed `6`, with `2` pass, `4` expected F003 failures, and
  `0` skip; focused Integration selected, discovered, and executed `38`, with
  `30` pass, `8` expected F001/F002 failures, and `0` skip. The corrected
  candidate builds Unit, Integration, and EndToEnd Release with `0` warnings
  and `0` errors. Route Update Unit is `80/80`; Route Update Integration is
  `72/72`; Route Create Integration is `23/23`; published Route Update
  EndToEnd is `6/6`; every run has `0` failures and `0` skips.
- Full managed acceptance restores all projects from the locked graph and builds
  Release with `0` warnings and `0` errors. Complete Unit is `1602/1602`,
  Integration is `809/809`, and EndToEnd is `152/152`; all have `0` failures
  and `0` skips. Fresh `linux-x64` Native AOT publication succeeds for the root,
  Integration, and EndToEnd executables. Managed EndToEnd against the native
  root is `152/152`; direct native Integration is `809/809`; direct native
  EndToEnd is `152/152`; all have `0` failures and `0` skips.
- Final SHA-256 identities are root
  `1aa56906ebf1ad3e58682c651d20c8c1617d2aa01ad924e0a8b2df1901d629a6`,
  version marker
  `fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4`,
  native Integration
  `05fbaadbc5d025ef55c3e601ec171dd78aa4b88627ce04b3966606aa0939eb35`,
  and native EndToEnd
  `e1635b2466b81969afc244893633563aeea284b91bf3ea1e8dee4265a485dbdf`.
  All three executables are stripped x86-64 ELF PIE files; root version remains
  `0.0.0-dev`.
- Native dogfood proves unknown target and missing Template requests each return
  JSON `invalid`, exit `4`, exact invalid finding and help guidance, no effects,
  and no stderr. A valid authored-body Template dry run remains JSON `attention`,
  exit `2`, complete/safe with zero effects and protected-body review guidance.
  Five affected-folder formatter checks, `git diff --check`, protected-path and
  prohibited-surface searches pass. The pre-receipt 11-path patch identity was
  `20701e310b77875726e550dcf292e34c41ed8f261669b3db69ce17ad39d6855f`.

### M12 Final Holistic Acceptance

- Fresh holistic review `T3-M12-HOL-02` inspected immutable commit
  `8a398f2e8e6f50beb730bea2a9c658a1fc923ffa`, tree
  `3bb4a22440b32781376c4b9160d3d72d168da9ef`, parent
  `c902f668cbaa9a6ae5fd65cbfb72ed7cb6c4c502`, and returned `PASS` with
  zero findings. It independently revalidated all three `T3-M12-HOL-01`
  corrections, their full-task interactions, direct consumers, and
  non-tautological evidence.
- The reviewer completely read the current C# directives with SHA-256 identities
  `_csharp.md` `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `design.md` `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and `style.md` `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- The immutable acceptance receipt remains managed Unit `1602/1602`,
  Integration `809/809`, and EndToEnd `152/152`; native Integration `809/809`,
  native EndToEnd `152/152`, and managed EndToEnd against native root `152/152`.
  Every run has zero failures and zero skips. Fresh Native AOT hashes, three
  dogfood scenarios, formatter verification, static audits, and the clean
  worktree were independently confirmed.
- Integration was revalidated against develop
  `996c2e17d1142ffb30dc7a2d17df657419566f97`, tree
  `877314d48db49013edc1c4dad1abb545b37caefc`. The merge base remains
  `5aad04acdaca1bd90b741397a75e6d99f98cf0d9`; overlap remains exactly the five
  develop-owned continuity paths recorded above, with no production, test,
  project, dependency, configuration, or lockfile overlap.
- Portable real interrupted-process proof remains an explicit deferred residual
  under the accepted task boundary. It is not an acceptance finding or blocker.

## Stop Conditions

Stop and return a project change request if implementation changes the accepted
wire/callable graph, needs a Framework API or contract change, changes Create
observable behavior, changes target ID/path/form/overwrite/authored body, guesses
around unpreservable YAML, violates effect coalescing/order, or requires a generic
patch/Route engine, DI container, service locator, custom parser, whole-document
rewrite, hidden subprocess, or central command-result JSON context. Stop if
recovery or evidence cannot truthfully prove the accepted boundary. Runtime
unavailable or unsafe conditions form finite typed results; they do not authorize
guessing.
