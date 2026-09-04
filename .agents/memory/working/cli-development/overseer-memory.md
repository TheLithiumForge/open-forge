---
open-forge:
  description: Active Overseer continuity for the replacement CLI task graph, decisions, agents, worktrees, and observations
  tags: [Memory, Working, Contextual, Active, KeepInMind, CLI, Overseer, Orchestration, Decision, Evidence]
---

# CLI Overseer Memory

## Purpose

Keep the compact project-level state needed to resume and coordinate replacement
CLI development without retaining complete child transcripts. Accepted product
meaning remains in Crystallized contracts and architecture. The [project control
ledger](project-control.md) defines permanent task identities, queue state,
completion grace, and integration mapping. The Plan, Tasks, and Checkpoint
define execution state and evidence; this record retains only the live
orchestration graph, decision frontier, and observations that the Overseer must
carry across parallel lanes.

## Maintainer Authority

- The maintainer alone accepts architectural decisions and feature additions or
  removals. Agents may identify contradictions, alternatives, risks, and
  recommendations, but must stop before changing accepted product meaning.
- Use proportionate native C# and existing Framework capabilities. A workaround,
  general engine, speculative abstraction, or materially stronger safety model
  is a stop condition for discussion.
- Do not push. Keep implementation in isolated feature branches and worktrees;
  local integration occurs only after review and accepted evidence.
- Questions and status discussions do not pause the program. Continue every
  unaffected lane until the maintainer explicitly requests a halt; pause only
  the exact boundary that requires an unresolved maintainer decision.
- Create every new worktree in the designated `open-forge-worktree` directory
  and record its feature-branch name. Existing registered worktrees retain
  their current paths unless a separate safe migration is deliberately
  accepted.

## Current Horizon

The accepted dependency order is:

1. The D0 contract freeze and all four shared foundations are integrated and
   accepted at the current local baseline.
2. Extension Create is complete at `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`.
   Root Install is complete at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`,
   exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`. Route Inspect interactive
   selection is squash-integrated at `fa3db1ee` with exact tree equality to final
   reviewed candidate `37c9360`.
3. Route Init is squash-integrated at `cc5085ce`; Route Create is Complete and
   squash-integrated at `19412d2`, exact tree `2bbba7e`, from reviewed candidate
   `392114a`. It supplies the `RouteCreateJsonContext` and Route-help predecessor
   slices. The separate [CLI Quality Remediation](tasks/cli-quality-remediation.md)
   Task is Complete and squash-integrated at `862cbf2a`, exact tree `571f104f`,
   from accepted implementation candidate `a4ccf19a`, tree `97254e65`, after
   consuming/revalidating those slices and closing the residual findings.
   Review orchestration is integrated at `5aad04ac`; permanent task identity and
   progress follow-up is integrated at `3356eba1`; repository-local npm linking
   is integrated at `128b70b3`; live progression is recorded at `fc7e3c75`, tree
   `af2a8880`. Task 3 “Route Update” is Complete at accepted closeout
   `27df8325`, tree `b68d4349`, and is squash-integrated at `272f5121`, tree
   `702f06d9`.
4. Task 4 “Route Move” is Complete at accepted closeout `631983ea`, tree
   `d6f6fdf7`, and is squash-integrated at `d3d2dc1`, tree `6959b51e`. Task 12
   CLI Architecture Authority Remediation is Complete at phase 5/5, milestone
   6/6. Corrected source `a0e6bc8d`, exact tree `9c4a33b1`, is
   squash-integrated at `495a7ed6`, the same exact tree. The accepted
   adoption-first slice is Task 14 Extension Install, Task 15 Status, and Task
   16 Doctor. Task 14 “Extension Install” is Complete at phase 5/5, milestone
   8/8. Accepted lane `a6b44f07`, tree `cd4c074d`, is squash-integrated at
   `20807781`, tree `4592a139`. Final warning-free managed evidence passes
   `1711/917/169`; Native AOT passes `917/169`; managed-on-native passes `169`;
   and the isolated offline package journey verifies launcher reachability,
   Root Install's 44 effects, and Extension Install's 28 effects. Task 15
   Status is ACTIVE at phase 3/5, milestone 3/8: Gray `dac2aece`/`874bdcae`,
   bounded callable correction `df46fb9c`/`eda74e79`, and accepted Red
   `f633fe1f`/`5dbfe9d7`. A Sol/xhigh Brilliant Implementer was activated at
   `de27fcd6`/`ebdcb4a6`; its early architecture checkpoint found that the two
   lifecycle Status signatures cannot guarantee one common lifecycle-document
   observation without prohibited mutable state. Coherent Green behavior
   remained unmodified while the bounded correction made one stateless
   invocation-local observation explicit. Corrected raw-snapshot Gray
   `b859d0aa`/`8f417c58` is committed with a warning-free Core Release build.
   Corrected Red `d768ca52`/`7a9b785b` is accepted with 14 Unit, 10 Integration,
   and 3 unchanged EndToEnd tests. Resume checkpoint
   `d120d49e`/`28f23430` reapplied the interrupted Implementer's 94-line partial
   `StatusDefinitions` work losslessly; recovery stash `9b51b3f8` remains
   retained while the same Sol/xhigh Brilliant Implementer owns coherent
   production and focused verification. First coherent production seam
   `44104f03`/`71a19f85` passes a warning-free Core Release build, one direct
   Status operation Integration case, and 62 selected lifecycle-store and
   Extension compatibility tests. The other nine selected Status Integration
   cases remain expected Green work at command composition or rendering.
   Immutable seam review found `T15-S1`, a real unresolved-identity
   compatibility change missed by that selection. Corrected Gray
   `e6a89c24`/`9f98c0e4` preserves three raw mechanical failure stages;
   corrected Red `c5726e5c`/`f719db23` fails only the two intended Extension
   mapping rows in its isolated 2/2 Unit selection. The same Brilliant
   Implementer has resumed exact stage-sensitive Green. Task 16 Doctor
   is PREPARED at phase 2/5, milestone 2/8 from the exact Status Gray snapshot;
   activation
   `26e245e4`/`680df032` and accepted Gray `ce627593`/`1f07abb7` are recorded.
   The lane is parked before Red/production until Task 15
   acceptance/integration. No test or completion claim is made. A later Task 16 activation reconnaissance began at
   root `53805940`: a Sol/xhigh Task Mastermind successfully supervised three
   Luna/max Explorer inventories from `/root/doctor_activation_recon`. No
   build, test, source or workspace mutation, artifact, or activation occurred;
   root advanced only through coordination ledgers and is clean at `bbf2d87c`.
   The six-domain, 112-kind Doctor contract is unchanged. Doctor consumes Task
   15's immutable typed views rather than parsing Status output or using direct
   producer fan-in; exact callable shapes remain Status Gray-owned and neutral
   readers remain reusable. The earlier single-owner preparation at `328599a0`
   remains historical context, not current authority. The accepted Status/Doctor
   rule remains complete coverage of the explicit contributor
   inventory at each frozen baseline; every later producer extends that
   inventory and affected evidence before its own acceptance. Task 7's
   Linux/macOS/Windows x64 expansion is Complete at phase 4/4, milestone 7/7:
   accepted lane `a2942781`, tree `fe36fc3f`, is squash-integrated at `e19d429e`
   with the same tree. Its Linux host journey passed; Darwin and Windows have
   stage-and-pack evidence only. ARM, publication, and live link or unlink
   remain unproven and unauthorized. Task 5 Route
   Remove, Task 6 Root Update, Tasks 17–20, Task 10 and conditional Task 21,
   Task 13
   implementation, and Task 22 follow in the project-control order.

Task 5's supervised read-only reconnaissance ran from assigned `develop`
commit `bbf2d87c` to clean `develop` commit `08f2fbb6`: a Sol/xhigh Task
Mastermind supervised three Luna/max Explorers, with no build, test, source,
contract, architecture, workspace, or artifact mutation. Only Doctor
coordination bookkeeping drift was reconciled. The historical
`codex/route-remove` lane is stale. Its durable positive-unmanaged
leaf/category boundary, neutral shared mechanisms, Route Remove-local policy,
and prohibition on sibling-private surfaces remain unchanged. Revalidate
lifecycle/Extension ownership, Task 15 catalogue/views, root composition, help,
serialization, and affected Status/Doctor evidence after Tasks 14–16. Task 6
may do read-only preparation after Route Remove Gray; implementation waits
for Task 5 acceptance.

Tasks 4–6 and 14–20 are the accepted measured trial of the streamlined assured lane. Each
keeps Task Mastermind Preflight plus explicit Gray and Red boundaries, uses one
Brilliant Implementer for Green through verification and the grouped
improvement pass, and folds ordinary Blue and Purple assessment into the Task
Mastermind's closing whole-task review. Compare task yield after every trial
using critical-path time, handoffs, correction cycles, dispositioned findings,
gate failures, integration friction, and any post-acceptance miss.

Root Install owns the closed base Framework installation. Route Init owns
concrete scoped route initialization and reuses the neutral embedded payload and
topology capability. It does not become `install --route`, a blueprint engine,
or a general template/scaffold system.

## Active Lanes

| Lane                          | Responsibility                                                                         | State                                                                                                                                                                                                                                                                                                                                                             |
| ----------------------------- | -------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D0                            | Contract, architecture, Plan, Task, checkpoint, and public-doc freeze                  | Integrated at `38e1498`                                                                                                                                                                                                                                                                                                                                           |
| F1                            | Native Shell question/answer transport and invocation capability                       | Integrated at `e782090` after review                                                                                                                                                                                                                                                                                                                              |
| F2                            | Embedded Framework payload reader and deterministic inventory                          | Integrated at `680915a` after review                                                                                                                                                                                                                                                                                                                              |
| F3                            | Framework lifecycle `sourceAssetPath` provenance                                       | Integrated at `0989356` after review                                                                                                                                                                                                                                                                                                                              |
| F4                            | Shared planned directory-creation mutation effect                                      | Integrated at `33913df` after review                                                                                                                                                                                                                                                                                                                              |
| C1                            | Extension Create                                                                       | Complete at protected public integration `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`                                                                                                                                                                                                                                                                               |
| C2                            | Root Install                                                                           | Complete at local integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`; final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56`, exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`                                                                                                                                                                       |
| C3                            | Route Inspect interactive correction                                                   | Complete and squash-integrated at `fa3db1ee` with exact tree equality to final reviewed candidate `37c9360`                                                                                                                                                                                                                                                       |
| C4                            | Generic and Framework-aware Route Init                                                 | Complete and squash-integrated at `cc5085ce`; reviewed closeout `c580149`, tree `a1810c4`                                                                                                                                                                                                                                                                         |
| M2 preparation                | Init/Create/Update/Move/Remove readiness                                               | Complete on clean no-op branches from `33913dfe`; Route Create is Complete at `19412d2`; QR1 is squash-integrated at `862cbf2a`, exact tree `571f104f`; Route Update is Complete, and later leaf Tasks retain accepted preparation decisions and remaining authority gates                                                                                        |
| Quality remediation           | Accepted first-pass CLI architecture, design, authority, and test-evidence findings    | Complete and squash-integrated at `862cbf2a`, exact tree `571f104f`, from accepted implementation candidate `a4ccf19a`, tree `97254e65`; all thirteen findings/candidates, final `QR-R1-001`, managed `1522/736/146`, native Integration `736/736`, format, diff, and Sol/xhigh review are closed                                                                 |
| Review orchestration          | Opt-in immutable coordinated topic review and permanent task-progress controls         | Complete and integrated at `5aad04ac` plus follow-up `3356eba1`; APM, Open Forge, projection, formatting, protected-path, and C# identity gates pass                                                                                                                                                                                                              |
| npm link shims                | Repository-local managed development CLI linking                                       | Complete and integrated at `128b70b3`; Node `16/16` and exact package, mode, manifest, nonmutation, and protected-path gates pass                                                                                                                                                                                                                                 |
| Route Update                  | Bounded route content and metadata update                                              | Complete at accepted closeout `27df8325`, tree `b68d4349`; immutable behavior `8a398f2e`, tree `3bb4a224`; managed `1602/809/152`, native `809/152`, managed-on-native `152`, dogfood, and holistic review pass                                                                                                                                                   |
| Task 4 Route Move             | Route movement with reference, navigation, lifecycle, and recovery integrity           | Complete at phase 6/6, milestone 12/12; closeout `631983ea`, tree `d6f6fdf7`, is squash-integrated at `d3d2dc1`, tree `6959b51e`; two-update completion grace is consumed and the Task is dequeued                                                                                                                                                                |
| Task 12 authority remediation | Move accepted CLI Architecture detail to its correct authority without behavior change | Complete at phase 5/5, milestone 6/6; corrected source `a0e6bc8d`, exact tree `9c4a33b1`, is squash-integrated at `495a7ed6`, the same exact tree; two-update completion grace is consumed and the Task is dequeued                                                                                                                                               |
| Task 14 Extension Install     | Install reviewed Extension packages with isolated lifecycle and recovery integrity     | Complete at phase 5/5, milestone 8/8; lane `a6b44f07`, tree `cd4c074d`, is squash-integrated at `20807781`, tree `4592a139`; final managed `1711/917/169`, Native `917/169`, managed-on-native `169`, and offline package journey pass; completion grace consumed and the Task is dequeued                                                                        |
| Task 7                        | Complete x64 package graph and package evidence                                        | Complete 4/4, 7/7; `a2942781` → `e19d429e`; Linux journey passed; Darwin/Windows stage+pack only.                                                                                                                                                                                                                                                                 |
| Task 15 Status                | Report complete typed current facts for the frozen producer inventory                  | ACTIVE phase 3/5, milestone 3/8; production seam `44104f03`/`71a19f85`; material review finding `T15-S1`; corrected Gray `e6a89c24`/`9f98c0e4`; corrected Red `c5726e5c`/`f719db23`; exact stage-sensitive Green active; rendering stash reapplied; both recovery stashes retained; no completion claimed                                                         |
| Task 16 Doctor                | Diagnose the complete Status contributor inventory without mutation                    | PREPARED phase 2/5, milestone 2/8 from the exact Status Gray snapshot; activation `26e245e4`/`680df032`; accepted Gray `ce627593`/`1f07abb7`; parked before Red/production until Task 15 acceptance/integration; no tests or completion claimed                                                                                                                   |
| Task 13 native CI             | Prepare minimal build/test CI, manual publish, artifacts, and package targets          | Preparation completed at phase 1/3, milestone 2/6 on candidate `e7689696`, tree `c204c17b`, and is integrated by the commit containing this record from `develop` parent `de40d550`, tree `e6040c49`. The Task is queued behind all retained commands, Task 10, and conditional Task 21 remediation. Linux/macOS/Windows x64 are accepted; ARM remains undecided. |

The Status lane's immutable continuation after `T15-S1` is
`cf00fb86`/`bf69e7c2` → `48309613`/`b8e0ad0f` → `e79a9767`/`17068f0b` →
`2521574d`/`11eb89ee` → `b8ac2dd7`/`453eb1b8`. It covers rendering and
compatibility Green, direct JSON context, plural-source Gray, direct producer
construction authority, and focused plural-source Red. Red selected, discovered,
and executed `1/1/1`; only the expected three-observation versus one-scaffold
oracle failed, while lifecycle completeness/trust and five package/source facts
passed. The same Sol/xhigh Brilliant Implementer is implementing plural
semantics, and independent Route selected-view work is present. No stable
full-producer evidence or completion is claimed.

Task 16's read-only alignment consumes the complete plural Extension view. Its
six Doctor-facing signatures and catalogue shape remain unchanged through
Status `2521574d`, but two Status-only methods changed. After Status acceptance,
reapply 17 C# files and two contract amendments, then add Red that consumes every
supplied source observation without fallback, substitution, reconstruction, or
reread. No Task 16 files, tests, builds, or implementation occurred in this
alignment.

Each mutating lane owns a distinct worktree and feature branch. Shared root
composition, serializer registration, public help, process evidence, Plan, and
Checkpoint are integration-owned unless a Task packet explicitly says otherwise.

## Accepted Observations

- The D0 contract freeze is integrated at `38e1498`. F1, F2, F3, and F4 are
  integrated at `e782090`, `680915a`, `0989356`, and `33913df`, respectively.
  The combined reviewed baseline has a Release build with `0` warnings and `0`
  errors; managed Unit `1284/1284`, Integration `500/500`, and EndToEnd
  `125/125`; Native AOT Integration `500/500` and EndToEnd `125/125`; and zero
  skips in every stated run.
- C3 Route Inspect interaction is complete and squash-integrated at `fa3db1ee`
  with exact tree equality to final reviewed candidate `37c9360`. Root owns Console and redirection facts; Core owns
  the interaction transport and Route Inspect policy. Focused Unit
  `131/131` plus Shell interaction `8/8`, Integration `82/82`, published
  EndToEnd `32/32`, full managed `1284/511/125`, and local `linux-x64` Native
  AOT `511/125` pass with zero skips. Independent review and docs-only rebase
  recheck pass. Direct composition proves interactive behavior; published
  redirected-human and JSON flows prove no prompt. Actual PTY process proof is
  explicitly outside the accepted evidence scope, and no workaround was added.
- The accepted lock-location correction removes bootstrap state. The persistent
  zero-byte lock is external under `LocalApplicationData/OpenForge/locks/v1`,
  keyed by the full SHA-256 of normalized physical workspace identity. Missing
  `.agents` is an ordinary lease-bound directory effect.
- C2 root Install is Complete at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`,
  exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`, from final candidate
  `11994e4d21ddc807b7480afc39ae3612e5a69a56`. Final managed
  `1390/598/136`, supported `linux-x64` Native AOT `598/136`, focused post-rebase,
  and two independent Sol/xhigh review gates pass. Native dry-run dogfood safely
  blocks on the repository's existing generated-region state without effects,
  workspace changes, lifecycle publication, or a new external lock.
- C4 Route Init is Complete and squash-integrated at
  `cc5085ce51ca624d07c347b014e036b8c3b7e1b4`, exact tree
  `a1810c4b247bf4997146baebf8a7ca3cf7f794c9`, from reviewed closeout
  `c5801494ac6426add2c64e32cafbba6f0162561a`. Its executable evidence candidate
  remains `cb62b19f73afcace163371af9093d877821fa800`, tree
  `be93900d0dc102fcf2d5a351651c0b0134de39a0`.
  Release is warning-free; managed Unit `1481/1481`, Integration `695/695`,
  generated serialization `18/18`, and published Find/Index/Route Init `14/5/6`
  pass. Supported `linux-x64` Native AOT serialization `18/18`, full Integration
  `695/695`, and published `14/5/6` pass. Detached dogfood proves canonical flow
  tags, exact scoped Find, Index already-current, verified Route Init no-op,
  unchanged hashes, zero-byte external locks, and clean cleanup. Fresh
  YAML/Markdown-boundary and whole-task Sol/xhigh reviews return `PASS` at `0.98`
  confidence.
- Route Create is Complete and squash-integrated at
  `19412d2a562ae66d1b4642256d854438df75366f`, exact tree
  `2bbba7e216e75809e99213e0ccd155bffe720d1f`, from reviewed candidate
  `392114a03a3c1329eb3ce9410795dcd36419815c`. Restored format, managed
  `1507/720/146`, portable `linux-x64` Native AOT root plus `720/146`, isolated
  dogfood, and Sol/xhigh `RC-R2` evidence pass. Post-integration Release is
  warning-free; focused Unit `27/27`, Integration `48/48`, and published
  EndToEnd `30/30` pass.
- Task 13 preparation is integrated from accepted candidate
  `e76896965e1cce51b7295f899499c5a7d0cce252`, tree
  `c204c17b4bdda87bf43a479879d6ebcf87472283`. Its direct parent is activation
  commit `3c904a23ed6735ca91bb3ca2810156b01a25e5b9`, tree
  `f2a780b8cb1da6ac680b7d7d7586d9732cb20cf6`; that direct-parent comparison
  changes only the Task 13 record. The integrated preparation preserves the
  fresh Sol/xhigh R1–R8 dispositions, accepted Linux/macOS/Windows x64 direction,
  and undecided ARM boundary. Task 7's later platform-expansion closeout is
  accepted in lane `a2942781`, tree `fe36fc3f`, and squash-integrated at
  `e19d429e` with the same tree. It proves the Linux host journey and Darwin and
  Windows stage-and-pack only. It changes no CI, CLI behavior, or release
  authority and does not claim D1, ARM, publication, or live link/unlink.
- Task 4 Route Move is Complete from accepted activation base `272f5121`, tree
  `702f06d9`, through closeout `631983ea`, tree `d6f6fdf7`. Warning-free Release,
  full managed `1701/888/165`, native `888/165`, managed-on-native `165`, focused
  `94/79/13`, shared escaping `5/5`, exact composition `1/1`, disposable dogfood,
  static, format, and immutable whole-task review gates pass with zero skips.
  Mid-read BCL cancellation or unexpected-read fault injection without a
  forbidden seam remains the accepted verification limit.
- Task 12 CLI Architecture Authority Remediation is Complete from accepted
  `develop` base `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
  `6959b51e148af44d59512d8bdd801350d88fc651`, through corrected source
  `a0e6bc8dc25ae9395beae98289f594e1f9f56af2`, exact tree
  `9c4a33b1c16617cf79beefd9f16a6e1d2d551382`, and squash integration
  `495a7ed6b55bca2a879ece83818f89e530c33af2`, the same exact tree. The twelve
  routed authority destinations, 27 Architecture headings, exact result and edge
  coordinates, dependency-version ownership, generated navigation, formatting,
  links, protected manifests, and focused integration re-review pass. No
  executable, package, platform, project, schema, build, runtime-projection, or
  public-document behavior changed.

- A switch that names every declared enum member is not closed over unnamed
  runtime numeric values. The direct modern-C# pattern is a clear switch
  expression with every named arm plus a discard arm that throws
  `ArgumentOutOfRangeException`, with evidence for every named mapping and one
  undefined runtime value. Do not add analyzer, generator, union, reflection, or
  warning-suppression machinery merely to claim stronger exhaustiveness.
- Exact-name BCL manifest-resource access is compatible with the project's
  reflection-free behavior boundary. Reflective type discovery, assembly-wide
  behavioral scanning, and reflective serialization remain prohibited.
- An inventory fingerprint cannot identify which historical embedded asset
  produced one concrete scoped target after that asset is removed, renamed, or
  moved. Required nullable per-target `sourceAssetPath` is the smallest complete
  provenance addition; it does not grant Route Remove lifecycle-release
  authority.
- Current Route Remove remains positive-unmanaged-only. A managed scoped target
  blocks it until the maintainer separately accepts release-unit, preservation,
  shared-region, publication-order, and repeat semantics.

## Open Decision Frontier

Route Move has no open decision. Its third positional operand follows the
accepted standard pinned-parser boundary as shell `cli.parser.invalid`; missing
required operands remain typed Route Move `invalid` results.
Task 12 is complete and integrated. Its accepted future Status/Doctor decision
is one explicit immutable
application-scoped `OperationalContributorCatalogue` built by
`CliCompositionRoot`, with producer-owned typed contributors, narrow Status and
Doctor views, and fresh invocation observations. It adds no dependency
injection, service locator, reflection, runtime registry, generic operational
engine, or ambient registration. Task 12 owns durable architecture placement;
Task 15 Gray owns exact signatures. Doctor consumes Task 15's immutable typed
views rather than parsing Status output or using direct producer fan-in, and
neutral readers remain reusable. Composition alone does not change public
Status or Doctor contracts. Task 14 integration is complete. Task 7's
platform-expansion horizon is also complete at phase 4/4, milestone 7/7, with
the Linux host journey passed and Darwin and Windows limited to stage-and-pack
evidence. ARM, publication, and live link or unlink remain unproven and
unauthorized. Task 15 Status is ACTIVE at phase 3/5, milestone 3/8 with corrected
raw-snapshot Gray and Red accepted; coherent production and focused verification
have resumed under the same Brilliant Implementer. Task 16 Doctor is PREPARED at phase
2/5,
milestone 2/8 from the exact Status Gray snapshot and is parked before
Red/production until Task 15 acceptance/integration.
The exact public Root Install JSON result schema is accepted, implemented, and
frozen, including fully present ordered facts and typed residual values `none`,
`retained`, and `unknown`.
M2 preparation surfaced command-local result and callable frontiers retained in
the Route Mutation Tasks. The maintainer accepted the recorded Create correction,
Init/Update freeze timing, Move lifecycle and neutral-resolver corrections, Init
neutral Framework-layer reuse, and Remove parser projection. Remaining exact
wire/proportionality/ownership/effect choices stay open and must close at the
recorded sequential boundary. Route Create is Complete at `19412d2`, exact tree
`2bbba7e`, from reviewed candidate `392114a`. The
maintainer-approved final result model makes each result and JSON effect
`Change` required while preserving nullable `Change.Before` and schema-v1 wire
compatibility. The candidate owns the `RouteCreateJsonContext` predecessor
slice, with no legacy JSON-context migration, and the Route-help predecessor
slice. Restored format, managed `1507/720/146`, portable `linux-x64` Native AOT
root plus `720/146`, isolated dogfood, and Sol/xhigh `RC-R2` evidence pass. The
separate CLI Quality Remediation Task is Complete and squash-integrated at
`862cbf2a`, exact tree `571f104f`, from accepted implementation candidate
`a4ccf19a`, tree `97254e65`. Task 3 “Route Update” is Complete at phase 7/7,
milestone 12/12, on accepted closeout `27df8325`, tree `b68d4349`; final
executable behavior is `8a398f2e`, tree `3bb4a224`, with fresh holistic review
PASS and the portable real interrupted-process proof deferred.
New architecture or product questions must still be returned to the maintainer
before changing accepted meaning.

## Accepted Interaction Placement

Keep `CliInvocation` and semantic requests free of streams and context objects.
Root composition injects `CliInteractiveSession` only into prompt-capable
operations or their factories. Each relevant binder records only an explicit
command-local policy fact such as `AllowInteractiveSourceSelection`. Generic
binding, unrelated operations, and unrelated requests remain unchanged.

## Accepted Interaction And Creation Policy

- Route Inspect prompts only when standard input and its prompt stream are both
  terminal-capable. One answer may be a one-based candidate number or the exact
  displayed path. Invalid input or end-of-input retains the existing blocked
  collision result; cancellation is interrupted. Use ordinary .NET redirection
  facts only, with no terminal framework.
- Root Install prompts once only for a prompt-capable human application that
  would write, after full preflight and before lock/effects. Dry-run, exact
  no-op, `--automatic`, and JSON never prompt. Refusal, end-of-input, and
  cancellation are no-write interrupted results. A non-prompt-capable human
  application that would write requires `--automatic`; omission is invalid with
  direct rerun guidance.
- A shared directory effect plans every exact missing directory, revalidates the
  missing target and physical parent under the existing workspace lease, calls
  ordinary `Directory.CreateDirectory`, and verifies the exact result. Created
  directories remain after later failure; there is no rollback, compensation,
  recovery bundle, P/Invoke, or hostile same-user creator-identity guarantee.
  Keep the effect separate from byte-bearing file changes.
- An applicable Install or generic Route Init plan includes missing `.agents`
  visibly as its first ordinary directory-create effect after acquiring the
  external workspace lease. Retain and report a verified created `.agents` as
  residual state after a later failure. Lock and recovery catalogues occupy
  separate versioned application-owned subtrees; do not move or duplicate the
  authoritative shared lock identity.
- When two consumers require the same semantics and evidence, promote the
  smallest honest shared capability at their nearest common scope. Similarity
  alone does not justify a generic engine; duplicated identical ownership does
  justify a shared step, pipeline stage, function, or module.
- Extension Create derives its default name by splitting the stable ID on `-`,
  uppercasing the first ASCII letter of each segment, and joining with spaces.
  Its default description is `Open Forge Extension package <stable-id>.`.
  Any existing safely resolved catalogue directory is valid, including an empty
  one; unrelated siblings are ignored, and only `<catalogue>/<id>` participates
  in collision and idempotence.
- Extension Create does not promise an exact question count. Its interactive
  wizard asks for every missing required fact, currently the stable ID and
  catalogue path, with concise guidance. Invalid or blank input can be corrected
  locally while input remains available; end-of-input is a no-write invalid
  result and cancellation is interrupted. Optional metadata uses accepted
  defaults unless explicit flags override it. Keep this flow command-local; do
  not create a general retry framework or arbitrary attempt limit.

## Reporting And Closeout

For every direct child and reported descendant, retain its task name, role,
model, reasoning level, owned worktree/branch, result, review, and commit or
blocker in the relevant Task or checkpoint before integration. Prefer compact
outcome-first updates: important current change, exact evidence, decision or
blocker, and next dependency. Do not discard sound work because an agent omitted
a requested self-identification when the actual model and reasoning can be
verified independently.

Render every progress-bearing Overseer update from the project control ledger
and linked Task records. Show actual task names, permanent IDs, truthful active
phase ordinals and completed milestone counts, and the dynamic Active, Recently
completed, and Queued sections. Advance completion grace only on those Overseer
updates. Beneath every active task, identify each active responsible agent by
canonical name, agent type and repository-relative role file, exact model, and
reasoning effort. Use `unreported` rather than infer unavailable runtime
metadata, and do not present completed helpers as active.
