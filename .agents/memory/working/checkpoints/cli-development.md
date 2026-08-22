---
open-forge:
  description: Current state and next action for the greenfield replacement CLI development program
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Architecture, Plan, Task, Contextual]
---

# CLI Development Checkpoint

## Goal

Build the complete replacement CLI from an accepted top-down architecture and a
detailed hierarchical Task set, then deliver every retained command and release
boundary without restoring the removed local architecture.

## Current State

- The final removed implementation is preserved at Git commit `4b873de`.
- Architecture and delegation governance is committed at `aa7d178`.
- The old production tree and root C# workspace were removed at `40ba03e`.
- Candidate tests are quarantined under
  `src/cli/tests/preserved/route-list-v1/` and are not active projects.
- The complete greenfield CLI Architecture, active Plan, and initial 57-file
  hierarchical Task set are authored; the current route-inspect decomposition adds
  five ordered child Task records.
- The scoped .NET 10 workspace, six-project graph, exact package graph, and
  artifacts routing exist entirely below `src/cli/`; restore and topology checks
  pass.
- The command-free Core and thin root host are committed at `2662f50`; active
  Foundation evidence and six-RID CI scaffolding are committed at `7a601cc`.
- Foundation acceptance passed with 33 Unit, 20 Integration, and 4 EndToEnd cases,
  current `win-x64` Native AOT root and test executable runs, a clean package
  audit, on-disk boundary audits, and a final independent review with no findings.
- Route-list command-local definitions, binding surface, request, selection, row,
  provenance, finding, coverage, result, operation call surface, preserved-test
  disposition, and complete evidence matrix are frozen. The earlier route-list
  contract increment passed a warning-free build with 46 Unit cases; independent
  code and writing reviews passed.
- The completed route-list selection/Loader increment accepts source-reference,
  exact ID/path, catalogue identity, adjacent overwrite, strict Loader
  region/destination, Loader-root selection, and selection-level physical-alias
  behavior. The complete warning-free Release build, 152 Unit cases, and 59
  Integration cases pass; final correctness and local improvement reviews pass.
- The completed route-list filesystem increment inventories only proven-contained
  sources through deterministic real-OS traversal, strict UTF-8 and generated-YAML
  metadata reads, typed physical/read findings, finite aliases, active cycles, and
  cancellation retention. The complete warning-free Release build, 214 Unit cases,
  and 69 Integration cases pass; final correctness and local improvement reviews
  pass.
- The completed route-list topology increment forms immutable authored route
  relationships, selected finite or complete depth, canonical rows, coverage,
  status, findings, and next actions without filesystem access. The warning-free
  Release build, 243 Unit cases, and 80 Integration cases pass.
- The completed route-list presentation increment adds route-list request binding,
  typed invalid depth/workspace results, one inventory/selection/topology/result
  operation coordinator, compact and expanded human output, a dedicated
  source-generated JSON DTO projection, bounded verbose diagnostics, route/group/list
  help, root composition, and managed plus published-process evidence. JSON follows
  the contract with a top-level envelope and result fields `selection`,
  `requestedDepth`, `effectiveDepth`, `coverage`, `findings`, and `rows`; finite
  depths are numbers and `all` is a string.
- The route-list slice is accepted for local development and integrated by squash
  commit `edca509` (`Establish and accept route list`). Before this planning
  update, exact merged-tree equality was verified: branch
  `feature/cli-route-inspect` and `develop` both resolve to the exact accepted
  route-list tree at `edca509`; no route-inspect production or test tree is
  preserved at that baseline. The exact-commit verification passed format, the
  warning-free Release build, 259 Unit cases, 84 managed Integration cases, 7
  managed EndToEnd cases, the published `win-x64` Native AOT root, 84 native
  `win-x64` Integration cases, and 7 native `win-x64` EndToEnd cases, all without
  a skip. The package/vulnerability audit, scoped restore, `git diff --check`,
  and no-write workspace-hash evidence remain clean. All executable and code
  route-list acceptance gates are green. No shared route facts were promoted
  before route inspect.
- Route-inspect planning is committed at `37d2e70`. Its contracts/evidence child
  has an accepted production-only Gray surface under
  `Commands/Route/Inspect/`: command definitions plus topical Operation,
  Resolution, Profile, and Result models. Focused formatting and the full Release
  solution build pass with zero warnings/errors; targeted correctness and local
  improvement reviews pass. Gray contains no tests, inspection behavior,
  promotion, binding, rendering, serialization, or composition.
- Resolution/promotion Phase 0 and Gray are accepted at exact commit `ea4c1dd`
  (`Freeze shared route fact contracts`). The reviewed Task-local Gray extension
  defines candidate route-family source, Loader-parser, overwrite, catalogue, and
  topology facts plus a fact-only Inspect resolution handoff and throwing callable
  skeletons. It contains no behavior, List migration, or completed promotion.
- The initial reviewed Red packet is committed at `fcf78d0` with 187 Unit cases and
  24 real-workspace Inspect Integration cases. Green review exposed omitted Loader
  safety, required-ancestor availability, and List unsupported-form regression
  requirements. The first supplemental Red packet is committed at `ea95b27`. A
  second production-free supplement adds unreadable/malformed Loader route-state
  evidence and three root/suffix source-form parity regressions. Focused whitespace
  formatting and the warning-free Release solution build pass. The final
  production-free supplement adds two-case real-workspace Red evidence for the
  expectation that selecting either exact base candidate leaves the overwrite pair
  ambiguous. The unchanged full Unit run is 446 total: 276 pass and 170 intentional
  failures rooted in shared throwing skeletons. The full Integration run is 118
  total: 88 pass, including all 84 accepted baseline cases and four supplemental
  List regressions, while 30 Inspect cases fail only because
  `RouteInspectResolver.ResolveAsync` throws `NotImplementedException`.
- The accepted resolution/promotion Green is committed at `334f2ba` (`Implement
  shared route resolution`) from final Red commit `fcf6124`. Shared route grammar,
  source facts, metadata, catalogue, overwrite,
  Loader parsing, and authored topology now have List and Inspect consumers;
  obsolete List-private implementations and superseded evidence are removed or
  moved without promoting command policy. Inspect-local safe inventory and source
  resolution distinguish routed, detached, unrouted, unresolved, ambiguous,
  overwrite, cancellation, and containment outcomes. Format verification and
  `git diff --check` are clean; the Release build has zero warnings/errors. The
  reviewed Purple route-list delimiter regression raises Unit evidence to 443/443
  and proves lexical scanning stops at `--`; the accepted Green boundary records
  118/118 Integration cases and 7/7 process EndToEnd cases against the built
  Release executable. Correctness and local-improvement reviews pass, and all
  changed/new production files are under 200 lines. Resolution/promotion and
  profile formation are complete; presentation is active.
- Resolution/promotion Purple is committed at `a54f4e0` (`Complete shared route
  resolution`). Read-only profile Preflight adopts that exact baseline and freezes
  one resolution invocation, one accepted graph/fact input, inspect-local profile
  and result builders, ordered generated visibility intersected with authored
  topology, all typed reading reasons, independent loading/Axioms availability,
  strict UTF-8 round-trip measurement, protected presentation boundaries, and the
  complete Unit/real-OS Integration Red matrix. Its no-change evidence carries
  into Gray; no profile production behavior has changed.
- Profile Gray is committed at `d53e5f7` (`Freeze route inspect profile
  contracts`). Reviewed Red adds 47 Unit and 12 real-OS Integration cases while
  leaving Gray production unchanged. The warning-free Release build passes. Full
  Unit is 490 total with 449 pass and 41 intentional failures caused by the Gray
  skeleton; full Integration is 130 total with 118 pass and 12 intentional
  failures caused by the Gray skeleton;
  accepted Inspect resolution remains 30/30. Correctness reviews pass after
  generated-marker/tag, possible-resolution, measurement, cancellation, snapshot,
  diagnostic, and low-arity fixture corrections.
- The maintainer authorized the post-route-inspect sequence: squash the accepted
  route-inspect branch into `develop`; create one generic-improvements branch;
  first remediate parser/raw-argument/special-edge deviations using standard
  library behavior and general typed validation; then improve reusable test
  fixtures, long signatures, and the root-host/Core architecture; accept and
  squash that branch into `develop`; only then start the next product Task.
- Green implementation exposed one impossible Red assertion: generic
  `RouteInspectFact<bool>` represents not-applicable through its state and reason,
  while unused value storage remains `false`, not runtime `null`. Green production
  WIP was isolated and the first correction committed at `dec7b3f`.
- Resumed Green exposed two further impossible Red assertions: unavailable generic
  value-type facts have the same default-value representation, and the
  determinism case dereferenced topology counts that the Behavior Contract makes
  not-applicable for an ordinary routed leaf. Those corrections committed at
  `bd8a5b7`.
- The next real-OS Green run exposed a missing line break between valid generated
  frontmatter and body in the profile fixture, plus one Unit expectation that
  collapsed the frozen independent inherited/local Axioms model despite matching
  Integration evidence. Those corrections committed at `2016100`.
- Resumed cross-layer Green then exposed two remaining conflicting expectations:
  one Unit count omitted a visible ancestor `#LoadNow` sibling required by the
  selected-closure contract, and one Integration assertion contradicted the
  frozen routed-leaf local-Axioms `NotApplicable` value. Those corrections
  committed at `e1884ff`.
- Fresh Green review then exposed one inherited-Axioms expectation that listed a
  Loader with no `Axioms` section as a contributor, contrary to the accepted
  contributor-only provenance contract. That correction committed at `868a2c1`.
- Final Green improvement review then exposed measurement/reading fixtures whose
  assertions require root startup membership but whose Loader declared no
  `#LoadNow` edge. Loader root topology establishes route exposure, not task-start
  loading. That correction committed at `e8dba19`.
- Profile Green is accepted at `c407e24` (`Implement route inspect profile`). One
  resolver result drives one typed profile/result with Loader-owned startup
  loading, selected and narrow closures, reading reasons, five exact physical
  measurements, topology, effective Axioms provenance, availability, cancellation,
  conditions, observations, status, and next action. Targeted formatting and
  `git diff --check` pass; Release build has zero warnings/errors; Unit is 490/490,
  Integration is 130/130, and accepted Inspect resolution is 30/30. All changed
  production files remain below 200 lines and new callables remain at four or
  fewer parameters. Final correctness review passes and final improvement review
  finds no material improvement.
- The completed profile record remains in its current route because the authorized
  `open-forge index` routing action reproduced the existing `CLI-EDGE-001`
  duplicate-entrypoint report outside this Task. Integrated acceptance owns the
  later link-safe move and generated-index refresh; presentation is not blocked.
- Presentation Phase 0 inspected the actual binding, composition, rendering,
  serialization, parser, and evidence surfaces. It proved that root-owned
  workspace/view delimiter guards and raw global-input scans conflict with the
  pinned-parser rules in the CLI implementation Directive, while the delimiter
  guard crosses `--` and can misclassify an option-like Inspect source operand. A
  narrow correction is accepted before presentation Gray: typed parse results
  own all six global values, parser-owned data supplies occurrences, root
  workspace/view delimiter policies are removed, and the explicitly contracted
  Route List depth exception stops at `--`. The planned focused Red freezes
  nonempty values across valid spaced/equals/colon forms and invalid
  attached-empty forms for `--workspace` and `--view`. The correction starts with
  production-free Red from exact activation commit `6a3c860`; every broader
  parser audit remains in the maintainer-authorized generic-improvements branch.
- Initial Shell Red is accepted at `41e46d5`. Green investigation exposed a
  pinned-parser interaction hidden by the removed raw global scan:
  `--depth= --json` assigns `--json` to an exactly-one depth symbol and therefore
  removes the typed JSON occurrence. The accepted published Route List result
  must remain JSON. Green WIP is isolated; a production-free supplement now owns
  the narrow zero-or-one depth token-capture evidence before the one-line
  RouteListBinding correction. Public depth validation and every broader
  raw-depth remediation remain unchanged and deferred, respectively.
- The depth-arity Red supplement is accepted at `36b1cfe`. The resulting
  five-file Shell Green passes a warning-free Release build, `501/501` Unit,
  `143/143` Integration, and `9/9` managed published-process EndToEnd cases.
  Formatting, `git diff --check`, raw-helper audit, file-size limits, and final
  correctness review pass. Broader Route List signature and parser remediation
  remain in the authorized later audit.
- Shell Green is accepted at `d53a44b`. The reviewed Blue cleanup moves typed
  extraction into `CliGlobalInputReader` and documents the exceptional depth
  token-capture setting. The warning-free Release build, `501/501` Unit,
  `143/143` Integration, freshly published `9/9` EndToEnd, format, diff, and final
  reviews pass again. No broader parser or Route List signature work was admitted.
- Shell correction Blue is accepted at `222ada9` (`Clarify typed shell input
  ownership`). Its `501/501` Unit, `143/143` Integration, and `9/9` freshly
  published EndToEnd result is the presentation Task's complete beginning
  baseline. The maintainer's updated execution rule now limits the presentation
  inner loop to new and directly affected evidence and requires the complete suite
  once more only after the final presentation change. The generic-improvements
  sequence remains parser remediation first, then test architecture, then
  signature and root-host/Core work. The active presentation Task's
  [Beginning Full-Suite Baseline](../cli-development/tasks/route-discovery/route-inspect-presentation.md#beginning-full-suite-baseline)
  records the projects, managed environment, published `win-x64` executable,
  variables, commands, counts, and results.
- Compile-only Presentation Gray is complete and reviewed from exact planning
  baseline `3b5078e`. It freezes route-family ownership, typed invalid-binding
  context, Inspect binding/rendering/help, required JSON projection models, and
  source-generation registration with all behavior entrypoints throwing. Release
  build, format, diff, focused Route List syntax `5/5`, focused serialization
  `2/2`, file-size, arity, correctness, and local-improvement checks pass. No full
  suite was repeated under the focused inner-loop rule.
- Presentation Gray is committed at `bc300ea`. The production-free Presentation
  Red matrix is complete and accepted after corrections for exact public
  next-action wording, typed JSON next semantics, zero-operation invalid paths,
  complete overwrite-layer and required DTO coverage, semantically valid fixed
  fixtures, contract-bounded escaping and diagnostics, and mirrored helper
  locality. The warning-free Release build, format, and diff checks pass.
  RouteInspect Unit is `237` pass plus `38` intentional Gray failures;
  Integration is `43` pass plus `22` intentional absent-registration failures;
  generated serialization is `1/1`. Directly affected List and Hosting failures
  are limited to new Inspect help truth. The existing managed Gray executable
  produces `28/28` intended RouteInspect process failures at absent registration.
  Final correctness review passes and local-improvement review finds no material
  improvement. Green had not started at that Red evidence boundary.
- Presentation Red is committed at `767c146` (`Freeze route inspect presentation
  evidence`). Presentation Green is active from that exact predecessor with all
  Red expectations and fixtures frozen.
- Presentation Green implementation and focused review are complete. One
  family-owned route group now composes List and Inspect; Inspect binding,
  contextual invalid input, human/JSON/diagnostic/help projection, and root
  registration are implemented; temporary List/Shell compatibility paths are
  deleted. Release build, format, and diff pass. Focused Unit passes with
  RouteInspect `284/284`, Route List `219/219`, Shell binding `9/9`, and parser
  `20/20`; focused Integration passes with RouteInspect `65/65`, Route List
  application `7/7`, Hosting `5/5`, and pinned parser `12/12`; freshly published
  managed RouteInspect EndToEnd is `28/28`. Final correctness review passes and
  local-improvement review finds no material improvement. The Green packet is
  ready to commit before the one final complete acceptance suite.
- Presentation Green is committed at `51c0960` (`Implement route inspect
  presentation`). Final presentation acceptance verification is active from that
  exact production tree.
- Presentation final acceptance passes from the unchanged `51c0960` production
  tree: warning-free Release build, format, diff, Unit `549/549`, Integration
  `166/166`, fresh managed `win-x64` publish, and EndToEnd `36/36`, with no skip.
  Presentation is Complete. Integrated Route Inspect acceptance is Active for
  local Native AOT, promotion/architecture audits, edge dispositions, and Route
  Discovery closeout.
- Integrated acceptance native execution passes: the `win-x64` Native AOT root
  drives `36/36` managed public EndToEnd cases, the published native Integration
  runner passes `166/166`, and the published native EndToEnd runner passes
  `36/36`. Package/vulnerability and initial dependency/promotion audits are
  clean. The locality audit found three newly or materially changed state carriers
  outside their nearest `Models/` scopes, so acceptance has returned to one
  behavior-neutral locality correction with contracts and expectations frozen.
- The bounded locality correction is complete: the three state carriers now
  occupy their nearest topical `Models/` scopes with matching namespaces and no
  forwarding or duplicate type. Release build, format, focused Unit `71/71`, and
  focused Integration `52/52` pass; fresh correctness review passes; the local
  improvement review's stale imports are removed. The corrected final tree also
  republishes and executes the Native AOT root with public EndToEnd `36/36`, the
  native Integration runner `166/166`, and the native EndToEnd runner `36/36`.
  Integrated acceptance then resumed final matrix, promotion, edge, and closeout
  recording.
- The final contract matrix, C1-C6 source dispositions, eight-candidate promotion
  audit, architecture checks, exact verification commands, and CLI-EDGE-002
  through CLI-EDGE-007 dispositions are now recorded. Every executable gate passes
  or has its explicit bounded residual. Fresh final acceptance and writing review
  were active before lifecycle closeout.
- Final independent correctness and writing reviews pass after the candidate,
  edge-range, native-surface, command, and lifecycle-provenance corrections.
  Route Inspect and Route Discovery are Complete from production correction
  `9c690b4`. The `CLI-EDGE-001` routing/index waiver leaves completed records and
  generated regions in place without hand editing.

## Current Step

Squash-merge `feature/cli-route-inspect` into `develop`, verify exact tree equality,
then create the authorized generic-improvements branch and begin parser-remediation
Preflight before test architecture.

## Route-List Closeout Decision

The maintainer's closeout/waiver decision authorized commit/squash integration
after the local route-list evidence. The accepted state is integrated by squash
commit `edca509` (`Establish and accept route list`). The two legacy-router errors are routed to [CLI-EDGE-001 — Legacy
routing-tool duplicate-entrypoint reports](../cli-development/edge-cases.md#cli-edge-001--legacy-routing-tool-duplicate-entrypoint-reports)
and are not route-list blockers. Exact merged-tree equality and the route-list
evidence above are recorded for the current route-inspect baseline.

## Accepted Decisions

- All replacement-specific C# material is scoped below `src/cli/`.
- Every new route-inspect record, interface, and property-only class, and every
  existing model materially changed or promoted by this slice, belongs under its
  nearest topical `Models/` path. Untouched accepted CLI types are outside this
  migration.
- The initial physical boundaries are `root/`, `core/`, and `tests/`.
- The project graph contains one executable, one Core library, three runnable test
  projects, and one test-support library.
- The Mastermind authors architecture, cross-cutting callable contracts, and the
  actual route-free foundation.
- Smaller implementers receive only closed Tasks after the relevant foundation is
  accepted.
- Tests remain under `src/cli/tests/`; preserved tests are candidate evidence,
  not architecture authority.

## Blockers

- No Route Discovery or integration blocker remains. `CLI-EDGE-007` is triaged to
  the authorized general parser-remediation branch and does not authorize a
  command-local workaround.
- `CLI-EDGE-002` through `CLI-EDGE-005` have accepted Route Inspect dispositions;
  their recorded residuals remain with generic remediation, delivery, or release.
- The deferred `route init` Framework-shape blocker remains a later,
  non-blocking decision and does not affect route discovery.

## Resume

Read:

1. [CLI Architecture](../../crystallized/documents/cli/architecture.md)
2. [CLI Development Plan](../cli-development/plan.md)
3. [Route Inspect](../cli-development/tasks/route-discovery/route-inspect.md)
4. [Replacement CLI Edge-Case Ledger](../cli-development/edge-cases.md)
5. [Program Architecture Directive](../../../directives/program-architecture.md)
6. [Architectural Perspectives](../../../guidance/architectural-perspectives.md)
7. The selected parent and its post-completion sequence

Then verify exact tree equality after the squash merge, create the authorized
generic-improvements branch, and begin parser-remediation Preflight.
