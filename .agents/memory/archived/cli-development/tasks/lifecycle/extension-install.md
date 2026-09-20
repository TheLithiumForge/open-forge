---
open-forge:
  description: Implement Extension installation from exact reviewed package identity
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Extension, Install, Lifecycle]
---

# Task 14: Extension Install

## Task State

- State: Complete. The accepted implementation is integrated into `develop` by
  the squash commit containing this record. The retained implementation lane is
  `codex/extension-install` at
  `<home>/dev/open-forge-worktree/extension-install`.
- Permanent mapping: Task 14 “Extension Install” in the
  [project control ledger](../../project-control.md).
- Prerequisites: Extension Inspect, root Install, and Mutation Foundation are
  complete. Route Move and Task 12 integration provide the accepted activation
  base; Route Remove and root Update are not prerequisites.
- Current progress: phase 5 of 5, milestone 8 of 8. The streamlined
  phases are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance. All eight milestones are complete.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/install/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/install/behavior.md).

## Read-Only Preflight

The Sol/xhigh Task Mastermind inspected clean local `develop` at activation
commit `596123df34a658886ea70f319b22abc91630950d`, exact tree
`ae8cabdbf357323bd5522b3e64f8e72fa8a6b358`. That continuity commit is the
direct child of Task 12 integration commit
`495a7ed6b55bca2a879ece83818f89e530c33af2`, whose tree
`9c4a33b1c16617cf79beefd9f16a6e1d2d551382` remains Task 14's semantic
authority base. The later diff changes only four Working records; `src/cli` is
byte-identical. The current lifecycle schema, collision and ownership rules,
external lock and recovery model, generated-navigation formation, seven
statuses, and direct typed C# composition are already settled. Task 14 does not
reopen them.

Maintainer acceptance froze these four decisions before Gray:

1. Own the exact result and presentation locally. Recommended ordered JSON
   facts are `mode`, `force`, `automatic`, `selection`, `source`, `packages`,
   `framework`, `footprint`, `effects`, `generatedNavigation`, `lifecycle`,
   `recovery`, `verification`, and `findings`. Arrays remain present and
   non-null; unavailable early atomic facts may be nullable. Human sections,
   finite findings, `next`, help, diagnostics, and exit mapping remain exactly
   aligned with that command-local result.
2. Make omitted source deterministically embedded without prompting. Prompt
   only for unresolved multi-package selection or an eligible initial-force
   choice. Selection accepts exact package IDs or exact `all`; invalid answers
   retry locally; EOF is no-write `invalid`; cancellation is no-write
   `interrupted`; dependency closure is displayed but not optional. There is no
   generic apply confirmation, and `--automatic` grants neither selection nor
   force.
3. Apply and verify dependency-first target and generated effects, verify the
   intended target topology, publish and verify Extension lifecycle as the last
   workspace file effect, then reread targets, Extension lifecycle, and
   unchanged Framework meaning before success and recovery cleanup.
4. Restrict managed package payload targets to descendants of `.agents/` for
   this Task. Reject other targets before planning instead of expanding the
   accepted directory-creation capability to arbitrary workspace parents.

The maintainer separately noted `.apm/` or a similar root as a possible future
extension. That idea is deferred and non-authoritative: Task 14 remains
`.agents/**`-only, and any future expansion requires separate target grammar,
ownership, collision, lifecycle, recovery, and security authority.

The accepted Gray surface remains command-local under
`Commands/Extension/Install/**` with direct construction and small typed stage
records. A neutral Extension lifecycle-currentness reader may be added for
truthful verification and later Status/Doctor consumption, but Task 14 must not
implement Status rows, Doctor findings, a contributor registry, or a generic
engine.

## Execution Capsule

- Profile: streamlined assured. Gray freezes the public callable and result
  contract; Red freezes safety and persistence evidence; one Brilliant
  Implementer owns Green through focused verification; the Task Mastermind owns
  one fresh whole-task review across behavior, production structure, and test
  evidence; at most one grouped correction returns to that Implementer.
- Milestones: 1 Preflight and maintainer acceptance; 2 Gray; 3 Red; 4 coherent
  production; 5 focused, public, packed-package, and full managed/Native AOT
  verification; 6 whole-task review; 7 grouped correction or documented no-op;
  8 acceptance.
- Accepted contracts: the four freezes above, existing Extension Install
  Interface and Behavior, Shared Operation Contract, Shared Result Coordinates,
  lifecycle provenance, generated-navigation, directory-creation, embedded
  payload, mutation/recovery, dependency, and distribution authorities. The
  `.agents/**` rule is command-local and does not narrow the shared
  `ExtensionTargetPath` grammar.
- Behavior matrix: omitted source selects embedded deterministically; explicit
  source is reviewed before selection; exact ID or exact `all` resolves package
  selection and dependency closure; dry run plans without effects; apply
  revalidates under the lease, prepares recovery, performs dependency-first
  target/generated effects, publishes Extension lifecycle last, and verifies
  final targets, Extension lifecycle, and unchanged Framework meaning; every
  terminal result uses the exact local facts, findings, presentation, streams,
  next action, and shared process status.
- Expected production paths:
  `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/**`, plus the
  explicit Extension composition model/composer/root registration and truthful
  help. One neutral Framework Extension-lifecycle-currentness reader is allowed
  only if the command cannot verify truthfully through existing readers.
- Expected test paths: mirrored Extension Install surfaces under
  `src/cli/tests/unit`, `src/cli/tests/integration`, and
  `src/cli/tests/end-to-end`, plus the two existing Extension group-help
  expectations whose “planned but unavailable” text becomes false. Existing
  package-manager journey wiring may be exercised but not redesigned.
- Direct integration neighborhood: `CliExtensionComposer`,
  `CliExtensionComposition`, `CliCompositionRoot`, root/Extension help,
  Extension source and lifecycle readers/stores, generated-navigation formation,
  physical workspace/path selection, locking, mutation preflight/revalidation,
  file/directory appliers, recovery stores, and command-local source-generated
  JSON serialization.
- Protected paths and meaning: other Extension and root Install command-private
  implementations, Status/Doctor rows and signatures, contributor composition,
  Framework lifecycle meaning, public shared schema, shared target grammar,
  package/dependency/platform/build authority, unrelated command behavior,
  JavaScript/MJS/CJS, generated sources, and all user work. Neighboring
  integration files may change only to register or present the already accepted
  command.
- Evidence ladder: focused Unit for pure grammar, selection, ordering, result,
  findings, and presentation; Integration for real sources/files/lifecycle,
  `.agents/**` rejection, topology, mutation, lock/race/revalidation, recovery,
  partial failure/cancellation, idempotence, and generated serialization;
  EndToEnd for published arguments, help, streams, prompts, exits, cancellation,
  and unchanged-state claims; one supported `linux-x64` packed-package journey;
  then full managed Unit/Integration/EndToEnd and supported `linux-x64` Native
  AOT Integration/EndToEnd plus managed EndToEnd against the native root.
- Evidence exclusions: test Open Forge request formation, classification,
  output, state, safety, placement, and forwarding only. Do not freeze
  System.CommandLine wording, serializer/YAML/ZIP internals, OS exception types,
  npm logs, or xUnit discovery behavior.
- Budgets: council 0; fresh whole-task review 1 (`T14-R1`, consumed); grouped
  correction 1 (`T14-C1`, frozen and consumed for the single Implementer
  handoff). Gray and Red are separate frozen boundaries, not review-budget
  units.
- Stop and escalation: stop for any change to a shared/public contract outside
  the accepted Extension Install amendments, Framework lifecycle meaning,
  dependency/platform authority, Status/Doctor signatures, protected command
  semantics, or ownership direction; for unreviewed source mutation or runtime
  package execution; or when safe evidence would require a new external effect.

## Gray Receipt

The separate Sol/xhigh Gray owner froze the accepted callable and public
representation in commit `fb8b98c86a87b55aac65a39798da232ca766f634`, tree
`8217ae57d81c7daf3c7ef1745931f736369abc3a`. Exactly the Extension Install
Interface and Behavior changed. They now own the immutable typed operation
shape, request/result members, fourteen ordered command-local JSON facts,
finite findings and aggregate status, interaction rules, dependency-first and
lifecycle-last sequence, final rereads, strict `.agents/**` payload target
policy, and descendant-only directory effects beneath the required existing
Framework anchor. No production, test, shared grammar, Framework, composition,
package, platform, Status, or Doctor path changed.

Gray evidence passed Prettier and `git diff --check`. Interface SHA-256 is
`ce7aabb703692292d65cfb9843a4a488d995c56494d40c19048a61f44a761e70`;
Behavior SHA-256 is
`8e4ecb0bd8a4b843b64876f424bdaa72095d064237ce8bbef333dbe4f7e45ee8`.
Task Mastermind inspection accepted the boundary against F1–F4, Shared Result
Coordinates, existing direct operation idioms, required Framework-anchor
semantics, and the no-speculation fence.

## Red Receipt

The separate Sol/xhigh Red owner froze the smallest decisive evidence from clean
Gray receipt commit `2c3918aec7708b3bef904a73696bcec2b5db55a1`, tree
`35bedaa2a53a3ea84e7a849bea34326ad2442e89`. The containing Red snapshot adds
one typed Unit contract file; command-local Integration planning, interaction,
mutation, and fixture files; one published EndToEnd file; and two surgical
Extension group-help expectation changes. Production, contracts, Framework,
TestSupport, projects, packages, build, platform, and unrelated command behavior
remain unchanged.

Red covers the exact request/result/finding surface; explicit IDs, exact `all`,
single-package inference, mandatory dependencies, and embedded omission;
bounded prompts, retry, EOF, cancellation, force, automatic and redirected
input; missing Framework anchor and strict `.agents/**` rejection before the
lease; descendant directory effects; dependency-first target/generated effects;
ordered JSON and verification; lifecycle publication and Framework preservation;
dry-run, fresh replanning, no-op, lock contention, managed divergence; and
published help, streams, exits, apply, persistent lock, and no-op. The typed
partial-failure/cancellation coordinator proposed during Red was rejected because
it would have selected unfrozen Green structure. That behavior remains required
by the contract and evidence matrix; the Brilliant Implementer must choose the
smallest cohesive command-local stage and add direct focused proof during Green.

Fresh Task Mastermind Red receipts are deliberate and bounded: Unit compilation
exits 1 only on four missing Extension Install namespace/type diagnostics;
Integration selects 19 and fails 19 because the route is absent; EndToEnd selects
4 and fails 4 for the absent published route/help; each updated group-help theory
selects 3, passes 2, and fails only the group case. All runs discover nonzero
evidence. Tracked and staged whitespace checks pass, and the Red surface contains
no reflection metadata ordering, uninitialized objects, fake filesystem,
test-only effect hook, or broad third-party/prompt wording assertion.

## Pre-Review Immutable Green Receipt

The Brilliant Implementer produced the coherent pre-review Green commit
`9a6ae2fa509d7bf1268f6012650e41655d3c27fe`, tree
`c39cfcbec1ea532ee27060020c681caa9b306245`, directly above Red commit
`81c8bc1e9c6ebac7802b0ec96e3b4f5642c74470`, tree
`75738db77c08a550544bf483325efbc423332f5e`. The snapshot adds the direct
Extension Install request, planning, application, result, presentation, root
composition, and focused evidence surfaces in 31 changed paths. Its 23
command-local production files contain 5,408 lines; related records stay
co-located, the largest result-model cluster is 495 lines, and behavior calls
have at most five parameters. Direct typed construction remains explicit, and
the snapshot contains no dependency injection, service locator, reflection,
runtime registry, generic engine, context bag, test-only seam, or JavaScript
change.

Focused Release evidence is clean: the root solution builds with zero warnings
and errors; Extension Install Unit passes 3 of 3, Integration passes 26 of 26,
and EndToEnd passes 4 of 4. Full managed Unit passes 1,704 of 1,704, Integration
passes 914 of 914, and EndToEnd passes 169 of 169. Native AOT EndToEnd and
managed EndToEnd against the default-version native root each pass 169 of 169.
The default native root has version `0.0.0-dev` and SHA-256
`e1436884df43361df7e8073949ffad8cda280d8b6717b763d77f19329b5de105`.

At that pre-review snapshot, one isolated offline packed-package journey passed
the owned staging, launcher, root Install, and Extension Install boundaries.
The final committed package journey below supersedes its artifact identities
and receipts. One shell-only relative redirection attempt failed before
launching the CLI and was not product evidence.

The pre-review Native AOT Integration suite passed 913 of 914. The sole failure
was a pre-existing Route Update polling race whose same test produced two
different failures across fresh native artifacts. The default-version rerun
proved that the two other initial failures were only an invalid custom-version
gate command. The later grouped correction replaced the nondeterministic Route
Update test boundary without changing Route Update production behavior; the
final committed evidence below supersedes this historical partial gate.

## Whole-Task Review and Grouped Correction

Fresh fallback topic reviewers independently inspected the clean immutable
Green commit after the coordinated review tool reported an infrastructure gap
without consuming review budget. The accepted review unit `T14-R1` found no
shared-contract, composition, lifecycle-last, recovery-order, forbidden-
mechanism, or third-party-evidence defect. It froze this single grouped
correction, `T14-C1`, for the same Brilliant Implementer:

- `T14-R1-AR-001`: remove application-result ownership from
  `Shared/Planning`; keep one neutral command-local result owner under
  `Shared/Result` unless a genuinely separate application projection is needed.
- `T14-R1-AR-002`: keep prompt and selection orchestration in the selection
  resolver, but extract leaf-local dependency-closure and payload-normalization
  capabilities. Do not promote a generic engine or split trivial records into
  files.
- `T14-R1-BC-001`: make force eligibility positive and explicit. Only an exact
  metadata-missing, no-generated-boundary initial occupant may be force
  eligible; unknown or authored `.agents/**` content remains an ownership
  conflict even with force.
- `T14-R1-BC-002`: form the initial-force next action from the exact normalized
  request with only force added, preserving IDs or `all`, source, automatic,
  dry-run, and workspace meaning.
- `T14-R1-BC-003`: classify selected payload states before target policy:
  missing or unavailable remains source-unavailable/incomplete; invalid or
  blocked remains source-invalid/blocked; cancellation remains interrupted.
- `T14-R1-BC-004`: preserve the known prepared final bundle path when cleanup
  is cancelled before deletion begins.
- `T14-R1-CS-001` through `T14-R1-CS-006`: replace nested conditional
  expressions, ordinary production null-forgiving operators, incomplete enum
  mappings, and incomplete filesystem outcome mappings with explicit typed
  control flow; pass the cohesive request to result formation; and keep human
  presentation in coherent raw or interpolated multiline blocks without
  changing public bytes.
- `T14-R1-TE-F1` through `T14-R1-TE-F4`: prove exact payload bytes at installed
  targets, directory- and recovery-aware no-write snapshots, interactive
  selection origin/root facts, and a real partial-stage result through the
  production result/JSON boundary. The partial-stop proof must use the real
  composed operation and must not introduce a fake coordinator or test hook.
- `T14-GATE-NATIVE-001`: replace the Route Update polling race with a
  deterministic real plan, lease, revalidation, recovery, effect, synchronous
  post-effect target mutation, applied-verifier, and result-formation journey;
  make no Route Update production change.

The grouped pass must retain strict `.agents/**` policy, direct construction,
lifecycle publication as the last workspace file effect, final rereads, a
maximum behavior arity of five, and cohesive file ownership. It may change only
the named Extension Install production and evidence surfaces, the exact Route
Update Integration test above, and this Task receipt. Any new shared behavior,
public contract, target grammar, or production change outside Extension Install
requires a project change request.

## Grouped Correction and Acceptance Evidence

The same Brilliant Implementer completed the single grouped correction in
commit `82180b5009b5bdfc5424f8c5cc630832bbb07d0a`, tree
`9dad2627c1a7d677db54667035338855af3e28ea`, directly above Task receipt commit
`8bede7cde757597a49e29991b032f89db794faba`, tree
`7cb315a9e421cd1ea131750fdda46c73906b8bca`. Its 27 changed paths contain 1,349
insertions and 446 deletions. `T14-R1-AR-001/002`, `BC-001` through `BC-004`,
`CS-001` through `CS-006`, `TE-F1` through `TE-F4`, and
`T14-GATE-NATIVE-001` are fixed. The command remains directly composed and
command-local: 25 production files contain 5,914 lines, behavior calls have at
most five parameters, and no dependency injection, service locator, reflection,
runtime registry, dynamic dispatch, generic engine, context bag, test-only seam,
or JavaScript change was introduced. Strict `.agents/**` payload ownership,
Framework meaning, lifecycle-last publication, final rereads, and the deferred
non-authoritative `.apm/**` idea remain unchanged.

Focused correction evidence is clean: the Release solution build has zero
warnings and errors; Extension Install Unit passes 10 of 10, Integration passes
29 of 29, and EndToEnd passes 4 of 4; the deterministic Route Update replacement
passes 1 of 1. Fresh complete managed evidence from the committed correction
passes Unit 1,711 of 1,711, Integration 917 of 917, and EndToEnd 169 of 169, all
with zero failures, skips, warnings, or errors. The direct Microsoft Testing
Platform apphosts use `DOTNET_ROOT=/usr/lib/dotnet`, `--progress off`, and
isolated result roots.

Fresh serialized Linux x64 Native AOT evidence also passes. The root executable
is a stripped x86-64 ELF PIE, reports exact `0.0.0-dev`, and has SHA-256
`9dd7cd22e9157c82f8b1dcfd0bc7eb27101919e57436db854136e4709d326540`;
its marker remains
`fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4`.
Native Integration passes 917 of 917 with executable SHA-256
`4aa7f568f0ba6b46b3b9d020b4e31d600a9ffc1f33c009ecf9f4845f612bf28d`;
Native EndToEnd passes 169 of 169 with executable SHA-256
`adeb0667f2a1b9cf273f6b2a9197712edd190d5c22739a3809f1b523a863f6d1`;
and managed EndToEnd rebuilt for `linux-x64` passes 169 of 169 against the fresh
native root. Every publication, build, and execution has empty stderr or an
explicit zero-warning/zero-error receipt, and tracked state remains clean.

The final isolated offline package journey stages version
`0.0.0-dev.sha-82180b5009b5bdfc5424f8c5cc630832bbb07d0a`, packs and installs only
the main and Linux x64 packages, and reaches the installed launcher without
registry contact or a global link. The main tarball SHA-256 is
`f78c899e6aa47cb08b48f6f4b418ee8d5e5842313c7e0130b99f048f3a4637d1`;
the Linux tarball SHA-256 is
`e588076cd52032be4901840c9f779af8a823591da375f2a27f3bad99529b404a`;
and staged and installed native payloads exactly match the fresh root. Root
Install completes with 44 verified effects. Extension Install selects exact
`all`, installs the one embedded `development-toolkit` root with 21 distinct
strict `.agents/**` payload targets and 28 verified effects, publishes and
verifies lifecycle, verifies all four final coordinates, reports no findings,
and leaves every payload and lifecycle target as a regular non-symlink. Durable
ignored receipts are under
`artifacts/task14/linux-x64/82180b5009b5bdfc5424f8c5cc630832bbb07d0a/`.

The streamlined flow improved semantic continuity: Gray and Red protected the
public and safety boundaries, one implementation owner carried Green and the
single grouped correction, early Task Mastermind checkpoints stopped a
1,088-line planner and a conflated presentation surface before commit, and the
fresh whole-task review produced one coherent correction rather than competing
repair lanes. Evidence orchestration still incurred one avoidable rework
occurrence: an initial `dotnet test --no-build` packet selected zero tests, its
first fallback used the wrong ambient runtime root, and a deprecated progress
flag produced warning-bearing receipts. Those receipts are rejected and made no
product change. Future exact-evidence dispatch must consult the latest accepted
canonical receipt first; direct apphosts use the explicit repository-supported
runtime root and `--progress off`.

The Overseer accepted the final ordered lane at
`a6b44f0734f4723cfdd9c1ac8d46cf7a657065c6`, tree
`cd4c074dc8ad60f32171f385b2c23e0b5a56911a`, after independently confirming
clean root and lane states, exact ancestry, a clean immutable diff, the bounded
43-path ownership set, absence of authored JavaScript/MJS/CJS, and absence of
the forbidden runtime composition mechanisms. The squash commit containing
this record is the permanent Task 14 integration and acceptance boundary.

## Expected Outcome

`extension install` applies one or more explicitly selected, reviewed Extension
packages to a workspace, records isolated Extension lifecycle identity, preserves
Framework lifecycle, and produces one verified external recovery bundle covering
every existing target it replaces or deletes.

## Architecture

- Reuse shared Extension catalogue/manifest/payload facts and root lifecycle
  primitives.
- Keep selection modes, dependency order, source-review policy, package conflicts,
  `ExtensionInstallPlan`, findings, and result local.
- Plan every package and collision before the lock. Revalidate source and workspace
  expectations under the lock before effects.
- Lifecycle `extensions` entries remain isolated by exact package identity.

## Evidence

Cover exact and automatic selection, none/one/many packages, dependencies,
collisions across packages and Framework, source review, malformed payload,
unsupported compatibility, dry run, lock/source race, bundle preparation and
retention after partial failure/cancellation at each package, lifecycle
isolation, generated navigation, idempotence, process, packed packages, and AOT.

## Stop Conditions

Stop before package download outside accepted sources, runtime code execution,
silent conflict resolution, Framework lifecycle mutation, or applying an
unreviewed/changed package.

## Activation Boundary

Activate only after Route Move is integrated, Task 12 is complete and
integrated, the four freezes above are accepted, and a fresh clean implementation
base and directive fingerprints are recorded. Gray and Red remain explicit;
one Brilliant Implementer owns production and focused verification; one fresh
whole-task review and at most one grouped improvement pass close the Task.
