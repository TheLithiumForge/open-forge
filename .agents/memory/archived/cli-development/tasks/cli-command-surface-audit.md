---
open-forge:
  description: Review the complete retained CLI command surface for direct PR-level architecture, design, refactoring, and test-evidence problems
  tags: [Memory, Archived, Contextual, Historical, Complete, CLI, Task, Audit, Architecture, Refactoring, Testing, Review]
---

# Task 10: CLI Command Surface Audit

## Task State

- State: Complete and integrated after reviewing all 28 retained commands and
  accepted Tasks 24–26; seven findings are selected for Task 21.
- Permanent mapping: Task 10 “CLI Command Surface Audit” in the
  [project control ledger](../project-control.md).
- Phase and milestone horizon: phase 3/3, milestone 5/5.
- Selected profile: sequential strategic audit. Root owns meaning and the report;
  two bounded Astra/high reviewers cover disjoint command cohorts in sequence,
  including architecture, callable design, locality/projection and test evidence.
  This replaces the older planned separate coordinator/worktree shape under the
  user's current sequential direction. No implementation or runtime mutation.
- Responsible role: Overseer as semantic owner and report writer, with one
  read-only reviewer active at a time on the same immutable accepted Git base.

This Task record preserves the accepted later audit boundary. The project
control ledger defines permanent identity, queue state, worktree mapping, and
integration state.

## Expected Outcome

Produce one decision-ready, PR-style review of the complete replacement CLI.
Flag material issues a strong reviewer would notice directly, including
Architecture or C# Directive violations, unclear responsibility boundaries,
missed higher-scope or shared refactors, unnecessary per-command file and model
proliferation, near-identical contexts or projections with uncertain authority,
and weak, redundant, wrongly tiered, or behaviorless tests.

This is a strategic first-pass review, not an exhaustive deep scrub. It records
stable findings with exact locations, consequences, correction direction,
responsible scope, dependencies, and proportional evidence. It also records
sound retained boundaries so later remediation does not generalize code merely
because it looks similar.

## Review Boundary

The audit covers the complete `src/cli/` production and test surface on its
frozen base, the current CLI Architecture and contracts, direct composition and
serialization boundaries, package-facing command reachability where relevant,
and the generated or shared sources that define CLI behavior.

The audit specifically examines commands that have accumulated large file
counts or families of similar request, result, fact, projection, context, and
renderer types. It distinguishes duplicated semantic authority from neutral
mechanism, and command-local policy from capabilities that have acquired a real
second consumer.

The audit does not change production code, tests, contracts, Architecture,
packages, generated runtime projections, dependencies, or public behavior. It
does not retest third-party libraries, runtimes, package managers, or framework
facilities. Test findings judge only Open Forge-owned behavior and evidence.

Every C# semantic reviewer must independently read the complete current
`.agents/directives/csharp/_csharp.md`, `design.md`, and `style.md` files and
report fresh SHA-256 fingerprints before reviewing. Reviewer packets must also
include the current CLI Architecture, applicable command contracts, source
locality, review evidence, and testing directives.

## Bounded Route Review Input

Task 10 retains one bounded audit input from Task 16's current route boundary.
`RouteInspectSourceProjectionBuilder` can form a command-local ambiguous-
overwrite state when one overwrite candidate's automatic ID maps to two or more
base sources; `RouteInspectOverwriteResolutionPolicy` then reports
`route-inspect.ambiguous-overwrite` as blocked. Task 10 must review whether this
synthetic overwrite-ambiguity conflict conflicts with the shared exact-pair
semantics or is separately justified by exact producer-observed facts under the
Route Inspect contract. This is one bounded
review input only: it is not a Doctor finding or a new task, does not restore any
removed Doctor route kind, and does not authorize broad search, inference, or
production change in the audit.

## Finding Standard

Each material finding must provide:

- one stable ID, severity, exact file and symbol or range, and reproduced fact;
- the violated authority or concrete maintainability consequence;
- the smallest credible correction direction and the scope responsible for it;
- dependencies, protected meaning, and proportional recheck evidence;
- a classification of defect, candidate improvement, deliberate tradeoff, or
  reviewer preference.

The Overseer rejects duplicates, unsupported taste claims, speculative
frameworks, and test recommendations that exercise third-party behavior. It
must keep candidates as candidates when a second consumer or measurable
consequence is not yet proved.

## Acceptance Evidence

- Every retained command and its direct shared/composition/test neighborhood is
  covered once in the review inventory.
- Top-down architecture, callable design, source locality, responsibility and
  file shape, serialization/projection authority, and test evidence each have a
  recorded disposition.
- The final report separates confirmed defects, candidate refactors, accepted
  boundaries, dissent, and deferred deep-scrub questions.
- Only the Task record, audit report, and generated navigation needed for those
  records change. All executable and public surfaces remain byte-unchanged.
- A later remediation task is scheduled from accepted findings; this audit does
  not quietly implement or broaden them.

## Activation Boundary

Do not activate this Task until Route Move, Route Remove, Root Update, Extension
Install, Extension Update, Extension Remove, Status, Doctor, Repair, and Cleanup
are accepted and integrated or explicitly removed from the retained command
horizon. At activation, freeze the exact command inventory, review budget,
immutable base, protected surfaces, and finding taxonomy before delegation.

## Frozen Execution Capsule

- Outcome: one decision-ready strategic audit of all 28 retained leaf commands
  and their direct shared, composition, serialization and evidence neighborhoods.
  It is a first-pass PR-level review, not a claim to inspect every line. Every
  command receives one primary reviewer and a concrete coverage disposition.
- Profile/applicability: read-only source and evidence review of this local
  developer tool. Potential consumer-file defects are reported with their
  actual contract and recovery consequence; the audit modifies no executable
  behavior or external system. Existing .NET/BCL, typed filesystem, mutation,
  recovery, parsing and generated JSON boundaries remain authoritative.
  Exceptional machinery: none. New generic frameworks are not an audit goal.
- Accepted base: `d5b77fcec6b31d7cc004c27858a24e978237a531`, after Task 26
  integration `aab57058` and its coordination receipt. Task 26's complete
  2837/1570/188 managed, 1570/188/188 native and 300-file differential evidence
  qualifies the unchanged executable predecessor. Branch:
  `codex/cli-command-surface-audit`. No additional worktree is required for
  immutable read-only inspection in this sequential run.
- Owner: root writes this Task, final report and current program coordination.
  One Astra/high reviewer at a time returns bounded findings without edits,
  tests, builds, publication or extra agents. Current live control prose is not
  an immutable source snapshot; reviewers use the named accepted Git objects.
- Inventory: `artifacts/task10-preflight/inventory.json` pins every binding and
  verified contract route, mirrored Unit/Integration neighborhood and public
  discovery class/method counts. Root composition contains exactly 28 concrete
  bindings. Reconcile inventory facts against the frozen source; a file count
  or parameterized row count alone is not a finding.
- T10-R1 cohort: Context, Doctor, Find, Index, References, Status, Route List,
  Route Inspect, Extension List/Inspect and Library List/Inspect (12 commands).
  It owns primary review of Shell/parser/presentation, source/route/reference
  Framework observation and operational read models, root registration and
  read-only composition. Trace mutation-owned facts only as direct context.
  Include the retained exact-pair overwrite-ambiguity question above.
- T10-R2 cohort: root Install/Update, Repair/Cleanup, Route Init/Create/Update/
  Move/Remove, Extension Create/Install/Update/Remove and Library Attach/Sync/
  Detach (16 commands). It owns primary review of mutation/locking/recovery/
  lifecycle/permission/filesystem mechanisms, destination ownership, native
  host/package reachability and mutation composition. Trace read-only facts
  only as direct context. Do not repeat accepted Task 26 manifest work.
- Each cohort covers accepted behavior, dependency direction and responsibilities,
  callable/nullability design, nearest shared/model placement and material
  duplication, serializer/projection authority, and owned test subjects, tiers,
  independent oracles and execution qualification. Retain exactly-three-public-
  journey direction as evidence authority; distinguish command journeys from
  separate Shell/package subjects and theory rows. Do not test upstream tools.
- Findings use stable `T10-R1-1` / `T10-R2-1` identities, priority, classification,
  exact immutable path/symbol, observed fact, violated authority or maintenance
  consequence, smallest correction, protected meaning, dependencies and focused
  recheck. Mark uncertain improvements as candidates; omit unsupported taste.
  Record sound boundaries so similarity does not become automatic promotion.
- Budgets: two bounded cohort reviews maximum, T10-R1 and T10-R2; council zero;
  implementation correction zero because this Task is audit-only. Consumed:
  T10-R1 and T10-R2. Root validated returned findings against current
  source, deduplicates shared consequences and preserves material dissent;
  no additional whole-program fresh review is commissioned by default.
- Allowed durable delta: this Task, one sibling
  `cli-command-surface-audit-report.md`, needed generated navigation, and root-
  owned current program coordination and the existing flow observation.
  Production, tests, schemas, contracts,
  Architecture, dependencies, packages, workflows and public prose are protected.
  Raw preparation and review receipts remain ignored artifacts.
- Evidence/acceptance: validate immutable ancestry, per-command coverage, exact
  finding locators/authority, local report links, generated scope, whitespace,
  protected-path absence and complete added-file accounting. No build, test or
  Native AOT rerun is triggered by these audit-only Markdown changes. Consume
  the unchanged predecessor evidence rather than repeating it.
- Stop conditions: a proposed product decision, external effect or source repair.
  This audit records findings without changing source. The user has already
  delegated continuation of corrections and refactoring; root may select the
  report's routine in-scope corrections under that authority. A consequential
  new product, support, external-effect or safety choice still requires the
  maintainer. No such choice is needed for the selected seven IDs.
- Current boundary: M5 complete. Both cohorts, root disposition, protected
  source/report freeze and exact-tree local integration are accepted.

| Phase | Boundary              | Completed milestones at boundary                                 |
| ----- | --------------------- | ---------------------------------------------------------------- |
| 1/3   | Preflight             | M1 inventory, scope and authority freeze                         |
| 2/3   | Sequential review     | M2 read-only cohort; M3 mutation cohort                          |
| 3/3   | Report and acceptance | M4 reconciled report; M5 protected-source freeze and integration |

## First Cohort Receipt

T10-R1 completed on source `d5b77fcec6b31d7cc004c27858a24e978237a531`,
tree `d5dc23de257987be099da92c45ddaeeba0802bb3`, under Task authority
`9be5af14`. The reviewer personally read the complete current C# trio and
reported SHA-256 fingerprints:

- `_csharp.md`: `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`
- `design.md`: `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`
- `style.md`: `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`

The return identifies exact-pair overwrite handling (T10-R1-1), detailed
command matrices at the public-process tier (T10-R1-2), and one local Library
Inspect callable improvement candidate (T10-R1-3). Root owns their final
disposition in the audit report. No runtime reproduction is claimed. The
review inspected primary paths and selected supporting source and evidence,
not every line. All twelve assigned commands and shared boundaries received
explicit coverage dispositions.

## Second Cohort And Root Disposition

T10-R2 completed its sixteen commands on the same source/tree under Task
authority `9da5f7db396741d46912c7908a5eaba262e815cb`. The Astra/high reviewer
personally read the complete C# trio and reported the same fresh fingerprints
as T10-R1 above. The final return names two behavior defects, one evidence-tier
finding and one local callable candidate. Both review units are complete.

Root independently traced the three behavior conflicts and both callable
boundaries, inspected the cited public test subjects, and validated 135 exact
coverage file locators against immutable Git. The [audit report](cli-command-surface-audit-report.md)
owns all seven IDs and dispositions. Install's relocated embedded-payload
proof remains a separate artifact subject; only its repeated domain-schema
detail is in the evidence correction. Root corrected two locator inaccuracies
(Index recovery helper placement and Update behavior authority). Neither
changes a finding's governing meaning.

The user already authorized completion of remaining corrections and
refactoring with full internal execution authority. Root selected exactly
T10-R1-1 through T10-R1-3 and T10-R2-1 through T10-R2-4 for Task 21. These
restore accepted contracts/rules or simplify identified local callables. They
introduce no product feature, public schema, support-floor change, compatibility
mechanism or external effect. This resolves older pending-finding wording
within current user authority; publication remains separately unauthorized.

## Acceptance And Integration Receipt

Feature `4914c03273a88d4c4440e81ddb6704d7dc0128db` was squash-integrated
onto accepted `d5b77fce` as `fbcec295edb0c523ba2135f484f3e4191252b92c`.
Both have exact tree `3b73cff413f1895ff0512f09eaebff36a35a8624`. All eight
changed paths were frozen by SHA-256 and mode before staging; the sole formerly
untracked report is committed and accounted. The nine protected Git objects
are unchanged. All 28 command names exactly match the frozen inventory; 135
coverage paths resolve at the source base; report/control links and whitespace
pass. Same-worktree Index refreshed the direct Task navigation. Prettier was
applied after Index's generated-region whitespace update. No source build,
test or Native AOT rerun was warranted.

Ignored `artifacts/task10-preflight/acceptance.json` retains source accounting,
feature/integration trees, protected identities and selected finding IDs. The
prose-only completion receipt does not invalidate the accepted source evidence.
Task 10 is complete; Task 21 begins from this current integrated baseline.
