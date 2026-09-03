---
open-forge:
  description: Implement route move with reference, overwrite, generated-navigation, and recovery integrity
  tags: [Memory, Working, CLI, Task, Route, Move, Mutation, Contextual]
---

# Implement Route Move

## Task State

- Permanent identity: Task 4 “Route Move”.
- State: Complete and squash-integrated into local `develop` by the commit
  containing this record. Accepted branch closeout is
  `631983ea1ec7d7ad5f5fc3999f938ba5f445ed81`, tree
  `d6f6fdf7d1caf62a9ac68582609c7282921f557c`.
- Activation base: accepted local `develop`
  `272f5121ccb792a8ca1eb9871235006665d8fb30`, exact tree
  `702f06d90cb646389d3082f0e95fc1d2d7f40faa`.
- Profile: Streamlined assured trial.
- Horizon: Six phases and twelve fixed milestones.
- Current progress: Phase 6/6, milestone 12/12 complete.
- Current-state suffix: complete and integrated.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/move/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/route/move/behavior.md).
- Current owner: None; the Route Move Task Mastermind, dedicated Gray and Red
  owners, continuous Brilliant Implementer, whole-task reviewer, and Integration
  Mastermind have returned ownership.

## Expected Outcome

`route move` relocates one accepted unmanaged route unit to one validated
destination while preserving route semantics, overwrite pairing, managed
references, generated navigation, and recoverability. Lifecycle evidence is
read and revalidated only; Move does not create, update, adopt, release, or
otherwise mutate lifecycle state.

A repeat request against the consumed old source is `invalid` because the
source no longer exists. A fresh request that observes retained partial state
may plan the remaining safe effects and converge; saved-plan replay,
provenance-based resume, rollback, and compensation are not part of the
contract.

## Maintainer Parser Disposition

The maintainer superseded the earlier repeated-operand expectation during active
milestone 7. A missing source or destination still selects Route Move and forms
its typed `invalid` result. A third positional operand is unmatched shell input
with semantic identifier `cli.parser.invalid`:
the shared parser returns exit `4`, writes no stdout, writes a nonempty diagnostic
to stderr, and stops before workspace selection, Route Move binding/domain
execution, locking, or any persistent effect. It forms no Route Move result,
status, finding, JSON envelope, or `next` action. Parser diagnostic wording is
not frozen.

Ordinary help and version invocations retain their successful terminal
short-circuits. Unknown symbols and other unmatched input retain the existing
shared terminal-invalid policy. Milestone-7 public evidence must prove the
parser/domain distinction through the published executable without asserting
third-party diagnostic text or internals. This disposition changes authority
and the contradictory public oracle only; production remains unchanged.

## Accepted Preflight Capsule

Read-only Preflight completed against the accepted activation base. It closed
the public wire graph, command-local stages, shared callable seams, behavior
matrix, evidence ladder, and stop conditions without production or test
mutation. Route Init, Route Create, Route Update, trusted fresh lifecycle output
with complete empty Extensions, exact planned-directory creation, explicit
typed root composition, and command-local JSON serialization are integrated.

Move must supply four narrow neutral capabilities already accepted by the Route
Move/Remove preparation boundary:

1. Adapt the existing source-link destination input from the misleading
   `LayerCanonicalPath` member to one required validated slash-separated
   workspace-relative Markdown `SourceCanonicalPath`. Preserve existing
   `.agents` behavior and update References and Context consumers; do not add a
   parallel optional member or a Move-local resolver.
2. Add one shared real-filesystem Route Markdown catalogue over explicit
   workspace-relative included roots, excluded paths, and supported-file
   filters. Move requests root `.` with no semantic exclusions and strict UTF-8.
   Coverage is explicitly complete, incomplete, blocked, or interrupted.
3. Add one read-only lifecycle ownership projection that validates Framework
   and Extensions, cross-section collisions, claims, and revalidatable file
   identity from one exact lifecycle snapshot. A claim on any selected layer or
   category item blocks the whole move; lifecycle bytes never change.
4. Add one lease-bound, nonrecursive, bottom-up delete-if-empty directory
   capability. It revalidates one exact contained ordinary empty directory,
   deletes only that directory, verifies absence, and reports residual or
   unknown state without recovery bytes, recursion, restoration, or rollback.

Exact planned directory creation remains a directly reusable accepted shared
capability. No new project meaning or package is required.

## Frozen Public And Callable Boundary

The request carries `Workspace`, one nonempty `SourceReference`, one nonempty
`DestinationTarget`, and `Mode` (`apply` or `dry-run`). The schema-v1 JSON
envelope is ordered as `schemaVersion`, `command`, `status`, `workspace`,
`result`, and `next`; `command` is `route move`.

The command-local result graph is ordered as:

1. `mode`
2. `source`
3. `destination`
4. `subject`
5. `ownership`
6. `references`
7. `generatedNavigation`
8. `plan`
9. `effects`
10. `unchangedPaths`
11. `recovery`
12. `verification`
13. `findings`

All arrays are initialized, identity collections are unique and
deterministically ordered, and unknown finite values throw. Effects use the
semantic kinds `directory`, `moved-file`, `reference-source`, and
`generated-region`; actions are `create`, `replace`, and `delete`. Their
before/expected facts distinguish `missing`, `file`, and `directory`; only file
facts carry a lower-case 64-character SHA-256. Outcomes are `planned`,
`not-started`, `verified`, `verification-failed`, or `completion-unknown`, with
residual `none`, `retained`, or `unknown`.

Command stages are explicit typed Route Move subject resolution, category
inventory, destination resolution, reference planning, generated-navigation
planning, plan projection/building, plan revalidation, application, applied
verification, operation, result building, and rendering. Shared stages are the
Route Markdown catalogue, lifecycle ownership reader, source-link destination
resolver adaptation, and directory deletion applier. Behavioral constructors
and calls normally accept one to three parameters and never exceed five absent
a genuine data/serialization boundary.

## Finite Findings, Status, And Next

Every machine code is prefixed `route-move.`.

- `invalid`: command-local invalid input, missing source or destination, invalid
  source, subject, or destination, or source not found. Shell parser failures do
  not create Route Move findings.
- `blocked`: unsafe workspace/source/category/destination/reference/generated
  region; ambiguous route or overwrite; identity collision; ownership
  unavailable or claimed; missing destination parent; occupied destination;
  self/inside-source move; lock unavailable; target changed; recovery conflict.
- `incomplete`: workspace unavailable; incomplete category inventory, reference
  coverage, projection, recovery, or inspection.
- `attention`: retained recovery artifact only, after all effects verify.
- `failed`: target changed during apply; write, verification, recovery, or
  operation failure.
- `interrupted`: interrupted.

Status precedence preserves a concrete failure or interruption event, then
pre-operation invalidity, then blocked, incomplete, attention, and complete.
Status/exit/stream mappings are complete `0/stdout`, attention `2/stdout`,
incomplete `3/stdout`, invalid `4/stderr`, blocked `5/stderr`, failed
`1/stderr`, and interrupted `130/stderr`; JSON is always one stdout document.

`next` precedence is retained recovery → `open-forge cleanup`; missing
destination parent → `open-forge route init`; other invalid → Route Move help;
retryable lock or target drift → Route Move; other blocked → Doctor; incomplete
→ Doctor; failed → Route Move with verbose diagnostics; interrupted → Route
Move; complete → `null`. Gray freezes the exact reason text with these commands.

## Safety And Effect Boundary

- Resolve cheap source, destination, category, containment, and alias safety
  before whole-workspace scans.
- Validate both lifecycle sections from one exact snapshot before planning and
  revalidate the same expectation and claim set under the held workspace lease.
- Discover every supported workspace Markdown file. Generated Entries interiors
  are excluded only from authored-reference authority; they are not hidden from
  physical coverage.
- Rewrite only authoritative destination spans. Preserve labels, fragments,
  prose, encoding, unrelated bytes, and slash separators; deduplicate shared
  definition spans. An applicable link without an exact writable span makes
  coverage incomplete.
- Reobserve the complete plan under the lease and perform a full semantic
  postscan. Dry run produces the same plan without lock, recovery, directory,
  file, generated-region, or lifecycle effects.
- Before the first target effect, prepare one external recovery ZIP containing
  every existing-target file that a replace, generated-region replacement, or
  delete effect may change.
- Apply destination directories parent-first, destination files in ordinal path
  order, coalesced replacement sources outside the moved subject, old source
  files deepest-first then ordinally, and exact old directories deepest-first
  through nonrecursive delete-if-empty.
- There is no rollback. Every effect boundary reports verified, retained, or
  unknown residual state. Retained recovery is attention only after the applied
  result is otherwise verified.

## Behavior Matrix

- Grammar: missing source or destination is a typed Route Move `invalid` result;
  a third positional operand is shell `cli.parser.invalid` with exit `4`, stderr
  only, and no Route Move result or workspace/domain execution; repeated
  `--dry-run` is idempotent; ordinary help/version bypass operation; the
  consumed old source is invalid.
- Selection: ID, base path, or overwrite path may select a leaf; a category
  requires its exact accepted entrypoint. Compatibility entrypoints are
  retained. Loader, root, native/resource, orphan, ambiguous, and colliding
  selections are rejected.
- Category: inventory includes every directory, entrypoint, routed/unrouted
  Markdown file, native source, resource, and overwrite layer. One unsafe item
  stops the complete operation.
- Ownership: complete trusted empty Framework and Extensions prove unmanaged;
  any selected claim blocks. Missing, malformed, stale, conflicting, or
  incomplete ownership blocks, and lifecycle bytes remain unchanged.
- Destination: same-parent and cross-route moves are allowed only under one
  existing exact parent and complete noncolliding layout. Missing parent,
  occupied/aliased target, self/inside-source move, or overwrite/layout collision
  blocks.
- References: cover incoming, outgoing, and required internal links, including
  outside `.agents`, overwrite layers, definitions, Unicode/spaces, fragments,
  and same-file coalescing. External targets and unaffected internal links stay
  byte-identical. Unsafe references block; unsupported applicable syntax makes
  coverage incomplete.
- Generated navigation: plan old parent, new parent, Loader, and moved
  entrypoint regions from intended membership. Unchanged projections emit no
  effect; unsafe markers block; no hidden Index is introduced.
- Application: lock and revalidation precede effects; every partial-effect and
  recovery boundary is observable; verification failure is failed; unknown
  recovery is failed; cancellation retains any stronger concrete result.
- Presentation: all graph members, arrays, nulls, finite values, finding order,
  streams, exits, compact effect reporting, and verbose invariance are tested.
  Renderers never rescan the workspace.

## Paths And Integration Neighborhood

Expected command-local production and mirrored evidence paths are
`src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/**`,
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Move/**`, and
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/**`.
Shared Route Markdown discovery belongs under
`Commands/Route/Shared/References/**`; combined lifecycle ownership belongs
under `Framework/Lifecycle/Ownership/**` with models under
`Framework/Lifecycle/Models/Ownership/**`; directory deletion belongs under
`Framework/Mutation/Application/**` with filesystem models; source-path
resolution changes the existing Framework Sources reference model/resolver and
their direct References/Context callers.

Later public integration may change root composition, Route group/root help,
`RouteHelpSections`, and focused published EndToEnd fixtures. Serialization stays
in a command-local `RouteMoveJsonContext`; the central Shell `CliJsonContext`
remains protected.

Protected meaning includes every unrelated command, Route Remove, lifecycle
writers/schema, recovery semantics except direct consumption, recursive
deletion, packages/configuration/projects absent an exact compile fact, command
contracts and CLI architecture, and program ledgers outside explicitly delegated
activation/closeout updates. Generated Navigation is consumed unchanged.

No DI container, service locator, context/dependency bag, generic Route mutation
engine, fake filesystem, duplicate Markdown parser or reference resolver,
command-global JSON context, speculative abstraction, rollback, or compensation
is allowed.

## Evidence Ladder

- Gray freezes the public graph, every finite value, exact `next` text, and the
  four neutral callable surfaces without behavior.
- Red freezes complete affected failing evidence for every behavior-matrix row
  before production.
- Unit evidence covers binding, graph/finite mappings, next/status policy,
  subject/destination decisions, exact rewrite spans/coalescing, effect order,
  and result formation.
- Real-workspace Integration evidence covers discovery inside/outside `.agents`,
  strict UTF-8, Unicode/spaces/definitions/aliases, lifecycle snapshot states,
  category inventory, generated projection, lock/revalidation, recovery, every
  effect boundary, delete-if-empty, and lifecycle byte identity.
- Published EndToEnd evidence covers help/grammar, typed missing-operand and
  consumed-source invalid results, shell-owned extra-operand failure, leaf
  dry-run/apply, category plus external references, JSON/streams/exits,
  no-write, and verbose behavior.
- Full managed and supported `linux-x64` Native AOT gates are mandatory because
  the task changes public composition, serialization, and shared filesystem
  foundations. Final evidence also includes format, diff, protected static
  searches, ELF/version/hash checks, and disposable dry-run/apply/old-source
  dogfood with a workspace hash manifest.

The immutable activation baseline passed warning-free Release build; managed
Unit `1602/1602`, Integration `809/809`, and EndToEnd `152/152`; native
Integration `809/809`, native EndToEnd `152/152`, and managed-on-native
EndToEnd `152/152`, all with zero failures or skips. Red freezes Route Move's
minimum expected focused counts after discovery.

## Flow, Budgets, And Measurement

Explicit Gray is accepted. Its corrected 54-path production boundary freezes
the complete command-local request/result graph, finite findings and exact
next-action text, all planning/application/binding/rendering stage callables,
and the four neutral shared seams. Red exposed and the original Gray owner
closed one application/recovery testability gap before Red acceptance: the
boundary now carries an explicit operation identity, ordered heterogeneous
effect receipts, recovery preparation and exact-deletion lifecycle, effect
application, five-capability application orchestration, complete finite machine
mappings, and the leaf/category item invariant. The accepted boundary contains
no Route Move behavior, tests, public wiring, or configuration change.
Independent Release compilation completed with zero warnings and zero errors;
focused whitespace and diff gates are clean.

Explicit Red is accepted as a 15-path test-only boundary: six Unit files, seven
Integration files, and two published EndToEnd files. The frozen nonzero floors
are Unit `88` (`49` graph/mapping facts green and `39` intended callable-stub
failures), Integration `70/70` intended callable-stub failures, EndToEnd
`13/13` failures at the unavailable public command, and source-link
compatibility `2/2` green, all with zero skips. Every Unit and Integration
failure reaches one real Route Move or accepted shared callable; no accepted
failure is rooted only in test setup or cleanup. Red covers the ten behavior
rows, all named and undefined finite mappings, leaf-empty/category-complete
items, direct binding, human/JSON presentation, real recovery/application and
directory boundaries, complete planning/revalidation, and public process
behavior.

The Red cycle rejected one zero-test filter before evidence, corrected three
symbolic-link cleanup failures before accepting the Integration floor, and
added direct Unit binding evidence after the Task Mastermind found the initial
public-only grammar boundary insufficient for the evidence ladder. All three
test projects compile with zero warnings and zero errors; focused formatting,
diff, protected-path, and production-path audits are clean. The resulting Green
handoff is one continuous Brilliant Implementer boundary.

Green milestone 5 is accepted across four neutral responsibilities and eleven
production paths. The workspace Markdown catalogue, one-snapshot lifecycle
ownership reader, exact nonrecursive directory deletion applier, and required
mutation revalidation support compile warning-clean. Focused Integration proves
shared foundations `13/13`, directory deletion `3/3`, and source-link
compatibility `2/2`, all with zero skips. Large initial implementations were
split into cohesive local partials before acceptance; no Route Move policy
entered the shared capabilities. Two command-local work-in-progress paths stay
outside the foundation commit and remain owned by the same Brilliant
Implementer for milestone 6.

Green milestone 6 is accepted through immutable commits `ffd13b62`,
`b27698a2`, `3469983a`, and `2df178d1`. One Brilliant Implementer completed the
command-local plan, revalidation, application, semantic postscan, operation,
and result-building boundary; the Task Mastermind kept wider gates and review.
Every changed Route Move behavioral method is at most fifty lines, every
behavioral call surface has at most five typed inputs, and the accepted larger
files retain one named cohesive responsibility. The boundary adds no DI,
service locator, generic mutation engine, fake filesystem, duplicate parser or
resolver, central JSON context, rollback, recursive deletion, or speculative
shared abstraction.

The grouped M6 correction accepted and repaired findings R1–R12: same-snapshot
reference/generated coalescing, semantic lease revalidation, live post-effect
observation, authoritative Markdown facts, moved-entrypoint navigation
projection, partial-progress and recovery preservation, caller-cancellation
meaning, explicit conditional flow, injected navigation exposure, truthful
null control flow, exact-path destination input, and post-coalescing recovery
targets. One approved neighboring change contains unexpected recovery-bundle
draft/final readback failures inside the existing shared store contract while
preserving the exact observed residual path. No deterministic real-filesystem
trigger exists for a mid-read BCL cancellation or unexpected read exception
without a forbidden seam; complete control-flow inspection plus adjacent real
cancellation/retention evidence is the accepted residual verification limit.

Truth-preserving evidence corrections replaced impossible fixture or framework
representation assumptions without weakening Open Forge-owned assertions. The
authored generated tag is `#Route`, raw text without an applicable CommonMark
link fact is complete, and moved-entrypoint navigation is changed whenever the
real projection changes stale metadata. The application fixture now consumes
the real `RouteMovePlanBuilder`, real lifecycle ownership, and typed navigation
projection; its Loader entries and effects are deterministically Archive before
Guidance with `#Route` metadata. Post-verification recovery deletion cancelled
before candidate selection is `interrupted` with unknown recovery and no
invented residual path; a positively observed retained artifact remains a
truthful residual. The final evidence identity names that exact outcome.

Fresh M6 evidence is shared-foundation/planning/revalidation/application
`13/45/8/8`, aggregate Integration `77/77`, and warning-free Debug Core, Unit,
and Integration builds. Unit executes `88` Route Move cases: `83` pass and the
only five failures are the protected milestone-7 JSON and human renderer stubs;
there are no skips. Focused format, diff, prohibited-surface, conditional,
method-span, and call-surface gates are clean. The immutable M6 review accepted
all behavior, structure, and evidence corrections before milestone advancement.

Milestone 7 is accepted in immutable commit `ce8671f5`, tree `b0bc5525`.
Explicit typed root composition now exposes Route Move through command-local
human, diagnostic, and source-generated JSON presentation. Selected-subject
ownership and unresolved path-origin projection preserve the complete public
graph without changing the accepted operation. The shipped-binary fixture uses
the public install and Route setup journey rather than production internals.

One neutral Route mutation text-escaping capability preserves complete encoded
tokens and exact unbounded bytes for Init, Create, Update, and Move. Cohesive
Route, Extension, and standalone composers keep the root assembler explicit;
state-only composition outputs reside in the local `Models/` scope. The
boundary adds no DI, service locator, generic command factory, context bag,
registry, reflection, command-global JSON context, or speculative public API.

Published-process evidence proved that the frozen repeated-destination oracle
contradicted the accepted shared parser boundary. The maintainer disposition
above superseded only that oracle: extra positional input is shell
`cli.parser.invalid`, while missing operands and consumed-source repeat remain
Route Move typed `invalid`. Fail-before selected `1/1` and failed at the stale
Route Move status assertion; corrected evidence is domain invalid `3/3`, shell
invalid `1/1`, and the full published Route Move class `13/13`.

Fresh milestone-7 evidence is Unit Route Move `94/94`, shared escaping `5/5`,
and exact composition `1/1`; Integration Route Move `77/77`, Route Create
composition `2/2`, and Route List help `1/1`; managed published Route Move
`13/13` plus root/Route help `1/1`; and Native AOT published Route Move `12/12`
plus direct parser-boundary execution. Release solution, E2E publication, and
Native AOT publication are warning-free. Whitespace, diff, static protected-
surface, ELF, version, and artifact checks are clean. The final whole-snapshot
review passed the accepted behavior, C# design, root topology, locality,
test-quality, JSON/AOT, and authority-consistency lenses with no surviving
material finding.

Milestone 8 consumed the one fresh whole-task review budget across behavior and
wire meaning, C# production structure, explicit root composition, total Route
Move locality, test quality and tier independence, and command-local JSON/AOT.
The reviewed candidate was the exact 53-path/51-status-entry snapshot over
`16bd8215`: tracked binary diff SHA-256
`df231fcd0a100799492b9b2496cf2db912aa8da78dd7e8a41a68105df2b420ef`
and 18-file untracked manifest SHA-256
`65f1008c2864eed1f1a90264e6370890c163de5939892a210fb5312bc35c1e18`.
The final reviewer returned `PASS` after rechecking both identities. Commit
`ce8671f5` is byte-identical to that reviewed candidate; Git records 52 changed
paths because the neutral escaping promotion is represented as one rename.
It produced four stable findings: R1 placed three state-only composition models
beside behavior; R2 retained one stale plural Route help oracle; R3 could
truncate an escaped diagnostic inside an encoded token; and R4 duplicated the
same escaping policy across four Route mutation commands. Their classifications
are two production-structure findings, one test-evidence finding, and one
behavior finding. No council or second whole-task reviewer was used.

Milestone 9 applied one grouped correction packet through the same Brilliant
Implementer. The composition outputs moved to the local `Models/` scope, every
stale help oracle adopted the exact singular unavailable operation, bounded
escaping became scalar/token safe, and the byte-identical four-consumer policy
moved to one neutral Route-shared capability. The same packet reconciled the
maintainer's parser authority through Interface, Behavior, Task, and split
published evidence only; production retained the standard shared parser path.
No wrapper, compatibility alias, generic renderer, heuristic, reflection,
catch-all operand, shared parser mutation, or expansion to Route List/Inspect
was introduced. All four findings and the contradictory parser oracle are
fixed; none is rejected, duplicated, deferred, or preference-only.

Milestone 10 focused correction verification is green. Shared escaping executes
`5/5`, the real Move diagnostic consumer `1/1`, Init presentation `5/5`, Create
presentation `4/4`, Update presentation `13/13`, Move presentation `5/5`, exact
composition/help facts `2/2`, and the corrected published parser split
`3/3 + 1/1`. Aggregate managed receipts are Unit Route Move `94/94`,
Integration Route Move `77/77`, and published Route Move `13/13`. The supported
`linux-x64` Native AOT publication is warning-free; ELF/version/help checks,
published Route Move `12/12`, and direct third-operand exit/stream execution are
green. Whitespace, diff, parser-workaround, DI/service-locator, central-JSON,
and removed-duplicate static checks are clean. The final immutable-snapshot
recheck confirms R1-R4 and parser authority remain fixed. The one grouped
correction budget is consumed.

Milestone 11 is green on immutable committed HEAD `aa44b39c`, tree
`75fb5107`. The .NET SDK is `10.0.111`; locked restore covers all six projects.
Configuration SHA-256 identities are `global.json`
`15fc2962d00bbb3febae9ddb01301f0723ebb743e1cfcd57af3d1fdd3fb640bb`,
`Directory.Build.props`
`0bad18b7db5342f56767d54c4a0b66aef2d627d9586e7be9254eeac2351ebc4e`,
`Directory.Packages.props`
`2af79560f3e7a11aa53219b609050b3c809ca1a79bd0be01f49ff6b190af7c2f`,
and `OpenForge.Cli.slnx`
`49d2d12fc8c456ed9386f5b5f64faa8ba8c92d555f193e96f3454fb07e8ba883`.
The Release solution build has zero warnings and zero errors. Complete managed
Unit is `1701/1701`, Integration is `886/886`, and EndToEnd is `165/165`, all
with zero failures and zero skips.

Fresh supported `linux-x64` Native AOT publication succeeds with zero warnings
and zero errors for root, Integration, and EndToEnd. Artifact SHA-256 identities
are root
`685e91c253815ad48f8b83fe2ce6556e9a46d554160dfdcd04e3ae9deaa20d85`,
version marker
`fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4`,
Integration
`68cc1186c71c68c93764d40e386912da23535e150b9a4011f4cdf97fc85cd5b9`,
and EndToEnd
`1899211d6ff10af39dbfb74ae366fb6a4bf264ec6ca8d51df08b9adde99556fb`.
All three executables are stripped x86-64 ELF PIE files and root reports
`0.0.0-dev`. Native Integration is `886/886`, native EndToEnd is `165/165`,
and managed EndToEnd against the native root is `165/165`, all with zero
failures and zero skips.

Disposable native dogfood uses the shipped Install, existing final Guidance
route, public Archive initialization, and public Route Create. The first setup
attempt redundantly initialized Guidance and truthfully stopped `invalid` with
exit `4` before mutation; this was a stale dogfood setup assumption, not a
product or contract failure. The corrected setup required no source change.
Dry run and apply exit `0`; old-source repeat exits `4` with exact
`route-move.source-not-found`. Setup/dry manifests are both
`499c58e53427ba4cec6688c93a7e902b6ae276ecb9ed6e689259ef8087208cfd`;
apply/repeat manifests are both
`43484911bd97a39a5b160097f3c7334f2786149a3913e968b99e8874045f20f8`.
Source and destination bytes share SHA-256
`3a07126bae4a550a04d273bd1bde04cce86e0b5f32e2caeffb65a43c739de547`;
lifecycle stays
`be314e861a95d7e692746c4fa8857ed033769059e42039e507cef7b2f7f36d10`.
Recovery storage is empty, the one external lock is zero bytes, the disposable
workspace is removed, and the repository remains clean.

Folder whitespace, full-range diff, removed-duplicate, parser-workaround,
DI/service-locator, central-JSON, and unimplemented-surface checks are clean.
Route Move source generation remains command-local in `RouteMoveJsonContext`;
root composition remains 102 lines with a 33-line assembler and cohesive Route,
Extension, and standalone composers.

The streamlined trial reached M6 acceptance about eight hours and thirty-three
minutes after activation. It used four semantic ownership transitions through
Gray, Red, the continuous Brilliant Implementer, and Task-Mastermind review;
mechanical gate workers are excluded. The grouped Green correction found nine
behavior issues, three production-structure issues, and one test-evidence
identity issue; all are fixed, none is deferred, and no M6-owned full-gate
failure survives. The only neighboring integration expansion was the approved
shared recovery-store containment. At M6 acceptance no post-acceptance M6 miss
was known because the cancellation and evidence-identity findings had been
corrected before that milestone. The later develop-integration findings recorded
below supersede the zero-miss conclusion without changing the historical M6
receipt. The closeout comparison with Route Create and Route Update follows
below.

## Milestone 12 Closeout

The accepted implementation candidate is commit `ce8671f5`, tree `b0bc5525`.
It is byte-identical to the one whole-task review snapshot: 51 status entries
and 53 actual paths over `16bd8215`, tracked binary-diff SHA-256
`df231fcd0a100799492b9b2496cf2db912aa8da78dd7e8a41a68105df2b420ef`,
and 18-file untracked-manifest SHA-256
`65f1008c2864eed1f1a90264e6370890c163de5939892a210fb5312bc35c1e18`.
The same independent branch reviewer returned `PASS` after the grouped
correction and found no surviving issue in that immutable branch snapshot. That
historical `PASS` remains an exact receipt; the fresh develop-integration review
later found the four post-closeout misses recorded below. Milestone 9 is exactly
the single grouped branch R1-R4 plus parser-authority correction. Milestone 10
is exactly the focused managed, Native AOT, public-process, whitespace, diff,
and protected-static receipt set recorded above. No milestone was skipped or
inferred from a later full gate.

The fresh develop-integration review accepted four post-closeout misses:

- Integration R1, high behavior/reference fidelity: moved sources or targets
  were canonicalized unconditionally even when the authored fragment-only or
  supported noncanonical relative literal still resolved to the intended
  post-move target.
- Integration R2, high safety/behavior: a real filesystem name that could not
  be represented as one canonical workspace-relative path was silently skipped,
  allowing a false `complete` Markdown catalogue.
- Integration R3, medium public-contract/test-evidence: standard help advertised
  destination IDs even though Route Move accepts only one exact destination
  path.
- Integration R4, medium production structure: the behavior-owning
  `SourceWorkspaceRelativePath` validator lived in a `Models/` source.

The grouped integration correction preserves a raw destination only after the
shared source-link authority resolves it from the intended post-move source to
the intended post-move target, blocks unrepresentable filesystem names with
`MarkdownPathUnsafe`, freezes exact-path-only published help, and moves the path
validator unchanged to the neutral Sources identity scope. Focused Release
builds are warning-free; corrected Route Move evidence is Unit `94/94`,
Integration `79/79`, and published EndToEnd `13/13`, all with zero skips.

Activation at `1c65089b` began at 2026-09-02 21:35:12 Europe/Zurich. Immutable
milestone-11 state `715bf245` was committed at 2026-09-03 09:25:53, eleven hours,
fifty minutes, and forty-one seconds later; Task closeout completed at about
twelve hours. Five distinct semantic roles participated: Task Mastermind, Gray
owner, Red owner, continuous Brilliant Implementer, and one whole-task reviewer.
The task-level flow used seven semantic transfers: Preflight to Gray, Gray to
Red, Red to implementation, implementation to review, review to grouped repair,
repair to immutable re-review, and re-review to closeout. One milestone-7 writer
context was replaced before mutation after bounded observability failed; no
partially authored semantic state changed owners.

The Green inner loop contained one grouped M6 correction plus its cancellation
follow-up before M6 acceptance. The formal whole-task review consumed one
grouped M7 correction cycle. Across the recorded branch M6 and M7
review/correction evidence, ten behavior findings, five production-structure
findings, and two test/evidence findings were accepted and fixed; none of those
recorded findings was rejected, deferred, or reduced to preference. The fresh
integration review then identified the four post-closeout misses above. Counting
them in the same required categories yields twelve behavior findings, six
production-structure findings, and three test/evidence findings accepted and
fixed across branch work and integration. Truth-preserving fixture/oracle
corrections stayed inside their implementation loops. The branch final full gate
had no product failure. Its one failed dogfood setup attempt was a stale setup
assumption and was corrected without source mutation. Integration friction now
also includes the four accepted semantic corrections above in addition to the
shared recovery-store containment, Route escaping promotion, explicit root
composition decomposition, and maintainer parser-authority disposition.

Route Create used the earlier Assured lane with distinct Gray/Red/Green,
protected integration, two named review gates, and one continuous implementer;
its final independent review found no material issue. Route Update used seven
phases, separate ordinary Blue and Purple stages, a coordinated review budget of
up to nine named invocations, 19 immutable-review findings, multiple Green and
final grouped corrections, and a second holistic acceptance pass. Route Move's
streamlined lane reduced ordinary review roles and transfers while retaining
explicit Gray, explicit Red, one continuous implementation context, one broad
fresh review, one grouped repair, and full native/public acceptance. It did not
materially eliminate correction work or critical-path time for this larger
move/recovery boundary: late fixture-parity and callable-structure discoveries
still dominated rework.

Trial disposition is **retain with adjustments**. Keep the six-stage shape and
the Task-Mastermind-owned wide gates. For the next comparable task, move the
whole changed-callable span/input audit ahead of the first Green commit, inspect
manual fixture plans against the real production builder before Green, freeze
public fixture construction during Preflight, require bounded child
observability, and preserve immutable milestone snapshots before broad review.
Continue using parallel Luna/max workers for exact mechanical receipts while one
semantic author owns the live boundary. Decide retention from quality and
rework, not token use: the streamlined lane used fewer review roles than Route
Update but produced four post-closeout misses, including two high-severity
behavior/safety defects. Its early literal-preservation, filesystem-coverage,
public-help, model-placement, and fixture gates need to move forward to reduce
correction latency.

Integration was audited read-only against clean local `develop`
`83902c8849bc98e44812b33175c5122421171e8b`, tree
`4b8324b2e9e0cb4c876ef9a5db293968b37a9192`. The merge base remains the
activation base `272f5121`; before this Task-only closeout commit, `develop` is
13 unique commits left and Route Move is 22 unique commits right. Develop
changes 64 paths and Route Move changes 157 paths from the merge base. Their
only four changed-path intersections are `overseer-memory.md`, `plan.md`,
`project-control.md`, and `tasks/_tasks.md`. The merge-tree audit finds four
changed-in-both continuity records with eight textual conflict hunks, no
`CONFLICT` diagnostic, and no overlapping CLI source, test, project, package,
configuration, or lockfile path. Three expected command-local escaping-file
deletions are one-sided and do not conflict. Integration must preserve develop's
newer queue/continuity authority while applying Task 4 completion, accepted
contracts, implementation, and evidence.

Proportional Open Forge self-inspection selected the exact Task path and ID and
confirmed that the source is routed and safe. Context returned its two parent
routes and the complete Task heading inventory. Both commands truthfully
reported incomplete loading coverage because pre-existing handoff and archived
sources lack loading metadata. The shipped CLI advertises Doctor as planned but
unavailable and exposes no Doctor or workspace Format command, so no false-green
Doctor receipt is recorded; milestone 11's warning-free build and folder
whitespace receipt remain the applicable formatting evidence. This existing
workspace-wide metadata condition does not originate in Route Move and does not
invalidate its command evidence.

No known Integration R1-R6 issue survives the grouped correction or its final
evidence. The corrected integrated candidate passes locked restore and a
warning-free Release solution build; managed Unit is `1701/1701`, Integration
is `888/888`, and EndToEnd is `165/165`, all with zero skips. Fresh supported
`linux-x64` Native AOT root, Integration, and EndToEnd publications are
warning-free; native Integration is `888/888`, native EndToEnd is `165/165`,
and managed EndToEnd against the native root is `165/165`. Corrected focused
evidence is Route Move `94/79/13`, shared escaping `5/5`, and exact composition
`1/1`.

The accepted folder-whitespace command exits cleanly without a warning or
format finding. Diff and protected-surface scans are clean. Disposable native
dogfood proves dry-run no-write, verified byte-preserving apply, consumed-source
`route-move.source-not-found`, unchanged lifecycle, empty recovery, and one
persistent zero-byte lock. The first native Integration execution observed the
adjacent Route Update polling oracle complete before its asynchronous mutation;
that exact test then passed `1/1` and the serial full native rerun passed
`888/888`. This timing-sensitive evidence residual is outside Route Move and no
Route Move gate failed.

Parser diagnostic wording remains deliberately unfrozen. The inability to
deterministically inject a mid-read BCL cancellation or unexpected exception
without a forbidden seam remains the documented verification limit; adjacent
real evidence and complete control-flow review are accepted. The Task introduces
no remote state and requires no package or dependency integration.

Integration applied the completed 151-path task delta without taking stale
branch continuity blobs, then reconciled the checkpoint, Overseer memory, Plan,
project-control ledger, Task index, and Route Mutation parent against current
`develop`. Task 4 is `RECENTLY_COMPLETED` on its completion-bearing update with
0/2 subsequent progress updates consumed. The current Task 12 and adoption-slice
priority remains intact, as does Route Remove before root Update inside the M2
dependency order. Task-owned budgets, findings, milestone detail, and flow
metrics remain in this Task record rather than being copied into program ledgers.

The fixed milestones are:

1. Preflight capsule accepted.
2. Worktree activated on the accepted base.
3. Explicit Gray public and neutral callable surfaces frozen.
4. Complete failing Red evidence frozen.
5. Shared neutral capabilities implemented and focused-green.
6. Command-local planning and operation implemented and focused-green.
7. Public composition, rendering, and focused managed/native verification green.
8. Fresh whole-task review completed across behavior, production structure, and
   test/evidence quality.
9. One grouped accepted improvement packet applied.
10. Focused correction verification green.
11. Full managed/native/static/dogfood gate green.
12. Task closeout committed and returned for integration.

Maximum whole-task review budget is one. A second reviewer is allowed only for
one later named distinct material risk. Council budget is zero. Correction
budget is one grouped cycle. Preflight discovery and explicit Gray/Red owners do
not consume the whole-task review budget. No separate ordinary Blue or Purple
owner is justified; the Task Mastermind's fresh whole-task review owns those
lenses.

Measure critical-path time from activation through closeout, semantic
agent/context handoffs, correction cycles, findings and dispositions split by
behavior/production-structure/test-evidence, focused and full-gate failures,
integration friction, and post-acceptance misses. Compare quality and rework
with Route Create and Route Update; do not decide the trial from token count or
raw finding count.

The active efficiency split keeps every delegated semantic owner on targeted
evidence for its owned boundary. The Task Mastermind owns wider task gates and
uses supervised Luna/max workers in parallel for exact pre-decided build, test,
format, static-output, artifact-inspection, and large-output summarization work.
Those workers do not choose product, architecture, test meaning, or commands,
and they never concurrently mutate the active semantic owner's files.

## Stop Conditions

Stop and return a project change request if exact Markdown bytes cannot be
preserved without a second parser, intended-source Generated Navigation cannot
represent the moved category without the old member, one lifecycle snapshot
cannot validate both ownership sections, directory effects require recursion,
rollback, or a generic mutation engine, or accepted public behavior must be
weakened. Stop locally on a protected-path expansion, new dependency/project,
unresolved physical-containment alias, or evidence that the one continuous
Brilliant Implementer boundary is unsafe.
