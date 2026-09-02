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
   `af2a8880`. Task 3 “Route Update” is the active sequential Route Mutation M2
   leaf at M12, 11/12 milestones complete.
4. Implement root Update M3 only after full M2 is complete. Preparation that is
   independent of unfinished behavior may proceed earlier in isolated lanes:
   scope discovery, contract and ownership audits, callable-surface analysis,
   Gray/Red readiness, and worktree preparation. Parallelize those processes;
   do not implement dependent command behavior out of order.

Root Install owns the closed base Framework installation. Route Init owns
concrete scoped route initialization and reuses the neutral embedded payload and
topology capability. It does not become `install --route`, a blueprint engine,
or a general template/scaffold system.

## Active Lanes

| Lane                 | Responsibility                                                                      | State                                                                                                                                                                                                                                                                                             |
| -------------------- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D0                   | Contract, architecture, Plan, Task, checkpoint, and public-doc freeze               | Integrated at `38e1498`                                                                                                                                                                                                                                                                           |
| F1                   | Native Shell question/answer transport and invocation capability                    | Integrated at `e782090` after review                                                                                                                                                                                                                                                              |
| F2                   | Embedded Framework payload reader and deterministic inventory                       | Integrated at `680915a` after review                                                                                                                                                                                                                                                              |
| F3                   | Framework lifecycle `sourceAssetPath` provenance                                    | Integrated at `0989356` after review                                                                                                                                                                                                                                                              |
| F4                   | Shared planned directory-creation mutation effect                                   | Integrated at `33913df` after review                                                                                                                                                                                                                                                              |
| C1                   | Extension Create                                                                    | Complete at protected public integration `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`                                                                                                                                                                                                               |
| C2                   | Root Install                                                                        | Complete at local integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`; final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56`, exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`                                                                                                       |
| C3                   | Route Inspect interactive correction                                                | Complete and squash-integrated at `fa3db1ee` with exact tree equality to final reviewed candidate `37c9360`                                                                                                                                                                                       |
| C4                   | Generic and Framework-aware Route Init                                              | Complete and squash-integrated at `cc5085ce`; reviewed closeout `c580149`, tree `a1810c4`                                                                                                                                                                                                         |
| M2 preparation       | Init/Create/Update/Move/Remove readiness                                            | Complete on clean no-op branches from `33913dfe`; Route Create is Complete at `19412d2`; QR1 is squash-integrated at `862cbf2a`, exact tree `571f104f`; Route Update is active, and later leaf Tasks retain accepted preparation decisions and remaining authority gates                          |
| Quality remediation  | Accepted first-pass CLI architecture, design, authority, and test-evidence findings | Complete and squash-integrated at `862cbf2a`, exact tree `571f104f`, from accepted implementation candidate `a4ccf19a`, tree `97254e65`; all thirteen findings/candidates, final `QR-R1-001`, managed `1522/736/146`, native Integration `736/736`, format, diff, and Sol/xhigh review are closed |
| Review orchestration | Opt-in immutable coordinated topic review and permanent task-progress controls      | Complete and integrated at `5aad04ac` plus follow-up `3356eba1`; APM, Open Forge, projection, formatting, protected-path, and C# identity gates pass                                                                                                                                              |
| npm link shims       | Repository-local managed development CLI linking                                    | Complete and integrated at `128b70b3`; Node `16/16` and exact package, mode, manifest, nonmutation, and protected-path gates pass                                                                                                                                                                 |
| Route Update         | Bounded route content and metadata update                                           | Active on `codex/route-update`; M11 full managed/native/dogfood acceptance and the four-topic immutable review are complete at `d8b8a33e`, tree `e0b8f85a`; M12 dispositions, at most one grouped correction, rechecks, and holistic acceptance are active at 11/12                               |

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

No currently surfaced product decision blocks active Route Update
implementation.
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
`a4ccf19a`, tree `97254e65`. Task 3 “Route Update” is active in its frozen
seven-phase, twelve-milestone horizon; M11 is complete and M12 is active at
11/12 milestones on immutable review snapshot `d8b8a33e`, tree `e0b8f85a`.
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
updates.
