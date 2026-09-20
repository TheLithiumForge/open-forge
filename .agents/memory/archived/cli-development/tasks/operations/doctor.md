---
open-forge:
  description: Implement diagnosis domains, severity, recommendations, and complete doctor projections
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Doctor, Diagnosis]
---

# Task 16: Doctor

## Task State

- State: Complete and squash-integrated. The accepted isolated Task-owned
  worktree used branch `codex/doctor-implementation`, at commit
  `a48a16cd80102331bca6d6cb3498160f37eb6b3f`, tree
  `90e66b0562fea90c2aa2fed9bd573c59676124bd`. Local `develop` integrates that
  exact tree at `59276c3bd764be4601ea92acbf4742b9bfa86837` without duplicating
  the equivalent prose cleanup already present there. Full managed, public,
  supported `linux-x64` Native AOT, managed-on-native, focused, and structural
  acceptance gates are complete.
- Permanent mapping: Task 16 “Doctor” in the
  [project control ledger](../../project-control.md).
- Current progress: phase 5 of 5, milestone 8 of 8. The five streamlined phases
  are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance. Preflight, Status acceptance and
  integration, Doctor activation, the post-Status Gray reconciliation, Red,
  coherent production, focused verification, the fresh whole-task review, and
  all committed Gray amendments, coherent production, `T16-R2`, grouped
  correction `T16-C2`, fresh whole-task evidence, acceptance, and integration
  are complete.
- Incremental completeness: the accepted Doctor catalogue diagnoses the complete
  explicit contributor inventory frozen by Task 15. A healthy supported
  observation may have zero findings, while an unavailable or unsafe producer
  observation still reports its own honest coverage boundary. Task 17 closed
  the accepted bridge-registration observation. No Extension observation
  horizon remains, and no later command owns a Doctor producer obligation.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/doctor/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/doctor/behavior.md).

## Outcome And Profile

Task 16 implements `doctor` as one deterministic read-only diagnosis over the
complete explicit producer inventory accepted through Task 15. It consumes the
six producer-owned Doctor views, retains all six domains even when coverage is
incomplete or blocked, and forms deterministic typed findings, resolution
lanes, next actions, human projections, and JSON without mutation or command
dispatch.

The selected profile is streamlined assured. Preflight is complete. A separate
Gray owner freezes the callable and public representation, and a separate Red
owner later freezes the smallest decisive behavior evidence. One Brilliant
Implementer will own coherent production through focused verification. The
Task Mastermind then owns one fresh whole-task review across behavior,
production structure, and test/evidence quality, followed by at most one
grouped correction by that same Implementer and final acceptance.

## Activation And Accepted Architecture

Preflight revalidated the clean integrated base and accepted authorities by
SHA-256. This record changes during the reconciliation and therefore has no
self-referential content hash. The starting queued-record blob was
`b45b357df9db1f4698717b0d5f21b3e3ff7ccab2c75fab83da082c2ff7baa754`.

- Doctor Interface and Behavior before the earlier whole-task-review Gray
  amendment:
  `e50474712e46736c6bc5afec150aae60315ba1429ade18076be88298c2a78061`
  and
  `a6504e978a0c638cbbe63db333ecb1286382eaf9e7201bb4a08a5b3fb37b8d75`;
- Status Gray Interface and Behavior:
  `b06c1c5c82b7230c311128ed176cc7ca9a926483549878bd83f9143f03b70559`
  and
  `97bcea04ad3589a88db0a3fe49e4252abd086cc06763176d664c0cfb37bf7ca7`;
  and
- current C# root, design, and style Directives:
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.

The exact integrated base is commit
`bb3a03f64fed6e84def7a6d5e0cefe12f9f75f7d`, tree
`41669500d8e2a49a03412975c8fd00f7635a4b6a`, on branch
`codex/doctor-implementation`. The accepted Status lane is commit
`0c19b7053ef2b8c48898cfebadedff0c5702bf34`, tree
`8ff2ac4ac0f5985863a9aaf85911c2deb6152980`; correction candidate
`f2dcdcae8b7c2cc9522b5ce7df250c73a55347db`, tree
`b6f83beb50cefe56a0aae7d9a4019dab08c19aea`. The accepted Status result and
content are represented in the integrated base. The immutable
`OperationalContributorCatalogue` exposes six concrete typed
properties for workspace/entry, recovery/residuals, routes/metadata/overwrites/
generated navigation, local references, Framework lifecycle, and Extension
lifecycle. Each property exposes one narrow `ReadDoctorAsync(CliWorkspace,
CancellationToken)` view. Doctor invokes each contributor exactly once per
invocation and joins only those six fresh typed views. The integrated Extension
view uses the current plural immutable `Sources` observations: one materialized
observation per distinct recorded source, with the nullable embedded source
first and remaining sources in ordinal order. Doctor consumes the complete list
without fallback, substitution, dictionary reconstruction, or rereading.

The catalogue is not dependency injection, a service locator, reflection,
assembly scanning, a runtime or dynamic registry, enumerable provider
discovery, a generic operational engine, ambient registration, a context or
service bag, or command-output parsing. Doctor neither fans directly into the
underlying readers nor imports another command's private `Shared/**`. Producer
views and neutral fact models remain under their Framework producers. Status
Gray owns and protects the exact callable signatures during this preparation
window.

The current unreleased schema-v1 catalogue contains exactly 108 kinds: 21
workspace, 4 recovery, 22 route, 28 local-reference, 14 Framework, and 19
Extension kinds. Fixed domain order is workspace, recovery, routes, local
references, Framework lifecycle, then Extension lifecycle. Finding order is
domain, stable kind, typed subject, then canonical location. Coverage, severity,
resolution, provenance, candidates or exact proposal, and typed next action
remain independent facts.

## Execution Capsule

- Milestones: 1 Preflight and activation; 2 Gray; 3 Red; 4 coherent production;
  5 focused, public, full managed, and Native AOT verification; 6 fresh
  whole-task review; 7 grouped correction or documented no-op; 8 acceptance.
- Applicability: Doctor observes user-owned Markdown, lifecycle and external
  recovery evidence but creates no persistent state. Ordinary defects,
  malformed input, unavailable files, interruption, safe alias detection, and
  accidental workspace change are in scope. A malicious same-user actor and
  transient namespace swapping remain outside the accepted threat model.
  Repeating the command against unchanged bytes is the recovery path because
  there are no effects to undo.
- Standard and exceptional machinery: the pinned .NET runtime, ordinary BCL
  filesystem and hashing APIs, existing source-generated serializers, the six
  accepted producer views, and real test boundaries provide the required
  capability. No dependency change, workaround, compatibility shim, native
  bridge, reflection path, unsafe code, fake filesystem, or other exceptional
  machinery is accepted.
- Accepted contracts: Doctor Interface and Behavior, Shared CLI Operation,
  Shared Result Coordinates, the Status Gray callable boundary, and the
  producer-owned workspace, source, route, reference, lifecycle, Extension,
  ownership, generated-navigation, and recovery fact authorities. Gray owns
  exact Doctor-local callable signatures and the command-local result and JSON
  graph without changing those meanings.
- Behavior matrix: exact CWD or explicit workspace selection; terminal help and
  version; all six domains retained in fixed order; complete, incomplete, and
  blocked coverage; independent information, warning, and error severity; six
  resolution lanes; all 108 stable kinds; bounded typed subjects, evidence,
  provenance, candidates, proposals, and next actions; complete, attention,
  incomplete, invalid, blocked, failed, and interrupted results; one typed
  result feeding compact, expanded, verbose, and JSON presentation; stable
  same-invocation ordering; and byte-for-byte no-write behavior.
- Expected Gray paths:
  `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/**` and exact Doctor Interface
  or Behavior amendments required to freeze its command-local JSON graph.
  Gray may consume but not change producer-owned operational contracts.
- Expected later test paths: mirrored Doctor paths under
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/**` and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/**`, plus a small
  published Doctor journey under
  `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/**` and directly affected
  root composition, help, serialization, and Status regression expectations.
- Direct integration neighborhood: the six operational contributors and their
  Doctor views; shared workspace/result/status/output contracts; source,
  routing, metadata, generated-navigation, local-reference, lifecycle,
  Extension, ownership, payload, and recovery fact models; concrete
  source-generated serialization; and later root standalone composition, help,
  and command registration.
- Protected paths and meaning: all Status production and test paths; every
  producer-owned operational contributor/view; every other command's private
  `Shared/**`; lifecycle publication and mutation; recovery preparation,
  deletion, target inspection, activity, and lock behavior; shared public
  schemas and semantic status/exit/stream mapping; package, dependency,
  platform, project, build, release, and repository authority; generated
  source; npm/TypeScript/package-manager paths; and all user work. A neighboring
  path changes only when accepted Doctor-local representation directly requires
  it and the Task Mastermind approves the expansion.
- Evidence ladder: Gray uses a warning-free Release Core compile, exact
  contract and finite-value checks, formatting, diff validation, and forbidden
  dependency-direction scans. Later Red selects Unit evidence for finite
  mapping, aggregation, ordering, status, rendering, and JSON; Integration for
  real owned workspaces and each producer boundary with byte-for-byte no-write
  checks; EndToEnd for public grammar, streams, exits, repeatability, and
  unchanged bytes; then complete managed and supported `linux-x64` Native AOT
  gates because composition, shared contributors, and serialization change.
- Evidence exclusions: test only Open Forge-owned request formation, diagnosis,
  classification, projection, output, state, safety, placement, and public
  reachability. Do not test or freeze System.CommandLine, STJ, YamlDotNet,
  Markdig, ZIP, BCL, OS, xUnit, or test-platform internals. Zero-test, stale
  `--no-build`, skipped, warning-bearing, partially loaded, or wrong-scope
  negative receipts cannot prove a gate.
- Budgets: council 0; the accepted whole-task review boundaries are consumed;
  grouped corrections `T16-C1` and `T16-C2` are complete. Gray and Red remain
  separate frozen boundaries, not review-budget units. Prior read-only
  reconnaissance consumed no review unit.
- Current implementation owner: none. Sagan's Task Mastermind boundary and
  Curie's Brilliant Implementer boundary are complete; the accepted lane and
  local integration are clean.
- Stop and escalation: begin production only from the immutable Gray and Red
  boundaries; stop before changing public or cross-task meaning, producer or catalogue
  ownership direction, shared lifecycle/source/reference/recovery identity,
  dependency or platform authority, protected paths, or accepted result strength;
  before any external, destructive, publishing, or remote effect; or when
  truthful evidence would require an unaccepted seam, workaround, or shared
  abstraction.

## Gray Boundary Receipt

Gray froze the accepted Doctor-local callable and public representation in
immutable source commit `ce62759348400a8e6a8094f4a5704950bf9ac0ac`, tree
`1f07abb7dd447705f8c225953c26aad998a68bc6`, based on activation commit
`26e245e4116de07d9d922ea055a57848b047c28d`, tree
`680df0320115a1d8a58a068a90b8c598215c376e`. The boundary contains 17 new
contract-only C# files under `Commands/Doctor/**` plus exact Doctor Interface
and Behavior amendments. It does not implement diagnosis, aggregation,
rendering, serialization, root composition, or tests.

`DoctorOperation(OperationalContributorCatalogue)` exposes
`ExecuteAsync(DoctorRequest, CancellationToken) -> ValueTask<DoctorResult>` and
fails immediately with explicit `NotSupportedException` until production is
implemented. It performs no contributor read. The ordered `DoctorObservation`
record freezes exactly the six existing Doctor views without copying their
facts, and the catalogue constructor dependency freezes explicit composition
without DI, location, registration, or enumerable discovery. The accepted
Status lane is commit `0c19b7053ef2b8c48898cfebadedff0c5702bf34`, tree
`8ff2ac4ac0f5985863a9aaf85911c2deb6152980`; correction candidate
`f2dcdcae8b7c2cc9522b5ce7df250c73a55347db`, tree
`b6f83beb50cefe56a0aae7d9a4019dab08c19aea`. The accepted Status result and
content are represented in the fresh integrated base; the catalogue and all
six contributor interface blobs remain
byte-identical to that base.

The original Gray result boundary contained all six fixed domains and exact 112
finding kinds as a historical pre-disposition receipt. It contained three
coverage values, three severities, six resolution lanes, typed subjects,
evidence, provenance, candidates, exact proposals with affected boundary, and
typed actions. Lifecycle and source-availability facts reuse the shared
operational enums. `ReadOnly` is intrinsically `true` and `ChangesMade` is
intrinsically `false`. Resolution and severity counts use an explicit shared
value state plus nullable nonnegative value, so unavailable and not-applicable
never become zero.

The schema-v1 JSON graph preserves shared envelope order and exact Doctor-local
field order, presence, nullability, arrays, finite machine values, and
discriminator-specific applicability. Every array and object is present;
nullable facts remain present and become `null` only when unavailable or
inapplicable. The final Doctor contract hashes are Interface
`e50474712e46736c6bc5afec150aae60315ba1429ade18076be88298c2a78061`
and Behavior
`a6504e978a0c638cbbe63db333ecb1286382eaf9e7201bb4a08a5b3fb37b8d75`.

Independent focused evidence after all corrections passed:

- .NET SDK `10.0.111` Release Core forced rebuild with `--no-restore`,
  development publish disabled, 0 warnings, and 0 errors; fresh assembly
  SHA-256
  `1d49d9113eca854dced96d0901300dd07fa638bdceecfa6fbe9966d6cf40cc80`;
- `dotnet format whitespace . --folder --include src/cli/core/OpenForge.Cli.Core/Commands/Doctor --verify-no-changes` and
  `git diff --check`;
- historical exact finding counts `21/4/25/28/14/20 = 112`, matching the then
  unchanged public catalogue; and
- manual callable, JSON, nullability, locality, protected-path, and forbidden
  registry, service, reflection, dynamic, generic-engine, and command-private
  import inspection.

An initial Gray execution attempted an unplanned package restore despite the
offline packet. The Task Mastermind stopped that exact process immediately and
confirmed only ignored worktree-local build intermediates and no Git change.
The accepted receipt above comes only from the later explicit `--no-restore`
forced rebuild. No test, remote, publication, staging, or destructive effect
occurred.

## Red Boundary Receipt

Red freezes only Doctor-owned tests and test-local fixtures over Gray commit
`24a702efa02fb11d25036cd85f3c7816bf207ded`, tree
`25f636883f827db94bf3e7e5c8a5262c6de59f1b`. Production, contracts, shared test
support, projects, packages, and other commands remain unchanged.

The Unit boundary contained three direct facts: the operation read all six
typed contributors once in fixed order and retains all six domains; every
supplied plural Extension-source observation contributes without first-source
fallback; and one literal 112-pair oracle froze the complete finite kind order
and exact wire names while requiring undefined values to fail closed. Its one
239-line support fixture keeps the six typed contributor doubles and their
shared data together; splitting that single seam would create several tiny
fixture fragments. The operation and mapping test files remain below the
200-line review trigger.

Integration contains exactly three real owned-workspace journeys: a
representative six-domain JSON result, a blocked selected-workspace result that
still retains all domains, and successful byte-for-byte repeatability with
unchanged workspace, lock, lifecycle, and recovery boundaries. End-to-End
contains exactly three public journeys: help without workspace inspection,
one read-only six-domain JSON invocation, and one bounded invalid/blocked stream
and exit journey. The tests assert only Open Forge-owned behavior; they do not
test a parser, serializer, runtime, operating system, test runner, or other
third-party contract.

Fresh Red evidence under .NET SDK `10.0.111` and MSBuild `18.0.11` is:

- before the mapping test was added, a fresh `--no-restore` Unit build and
  direct test-app run selected and executed three intermediate tests: the two
  unchanged operation facts failed with the expected Gray
  `NotSupportedException`, while a later-removed redundant catalogue fact
  passed;
- the final Unit source fails its fresh `--no-restore` build with exactly two
  expected `CS0117` errors for the intentionally absent
  `DoctorDefinitions.ReadFindingKind` callable and zero warnings;
- the fresh `--no-restore` Integration build has zero warnings and errors, and
  its direct filtered run selects, executes, and genuinely fails all three
  tests because the public Doctor command is not composed; and
- the fresh `--no-restore` End-to-End build and direct filtered run select,
  execute, and fail all three tests against the uncomposed published command.

The corrected Integration assembly SHA-256 is
`ff50b4fc78623222570ba9c6b6acd5e354fea90d624eb4f7bc77f2b8c168d5fb`;
the End-to-End assembly and published CLI SHA-256 values are
`79e442a14167dbfc80237d0ec30e4ff5e4c2b930c8b205c3d8ccf4d9597dc222`
and `4450c4552ac44a4e463db6c9adad89a39d803da47801801019ee08c02045bd61`.
Doctor-path whitespace verification and `git diff --check` pass. No file was
staged or committed while this pre-commit receipt was formed.

## Coherent Production Receipt

One Brilliant Implementer completed the coherent production boundary across 34
production paths: five existing root or Doctor paths changed and 29 Doctor-local
paths were added. No frozen Red test, Doctor contract, producer view, Status
path, project, package, dependency, JavaScript, or TypeScript path changed.

The command binds the exact operand-free Doctor grammar and reads each of the
six explicit `OperationalContributorCatalogue` properties exactly once in the
accepted fixed order. The command-local aggregation preserves all six domain
reports, deterministic finding and action order, independent coverage,
severity, resolution, counts, and typed limitations. It consumes every supplied
materialized Extension source observation without fallback or reread. Safe
lifecycle absence short-circuits inapplicable payload, source, ownership, and
target classification without hiding supplied facts from a non-absent flow.

The implementation keeps six explicit domain inspectors with narrow helpers for
Framework managed targets, Extension sources, and Extension managed targets. It
does not introduce dependency injection, a service locator, reflection,
enumerable or runtime discovery, a generic operational engine, a service or
options bag, a producer reread, rendered Status parsing, a lock, mutation,
recovery preparation, or command dispatch. An early 210-line Framework
inspector and 256-line Extension inspector were refactored by cohesive
responsibility before verification. The final largest Doctor-local production
file is 168 lines; no changed production file exceeds 200 lines.

Compact human output retains workspace selection, status, aggregate and
per-domain coverage and counts, six domain identities, limitations, typed
findings and subjects, resolution, candidate cardinality, proposal identity,
and lossless action kind, operation, and command coordinates. Expanded output
adds action reasons, evidence, provenance, locations, candidate basis, and the
complete exact-proposal coordinates. JSON uses one source-generated command-
local context and preserves the accepted schema-v1 graph and finite vocabulary.
Root composition places Doctor after Status and before Context with direct
typed construction and truthful command and help text.

Focused evidence under .NET SDK `10.0.111` is fresh after the final production
corrections:

- Doctor Unit selected and executed 3 tests: 3 passed, 0 failed, 0 skipped;
- Doctor Integration selected and executed 3 tests: 3 passed, 0 failed, 0
  skipped;
- published-process Doctor End-to-End selected and executed 3 tests: 3 passed,
  0 failed, 0 skipped;
- the operational-catalogue Unit regression passed 1 of 1;
- Status composition Integration passed 4 of 4;
- Extension lifecycle operational-contributor Integration passed 1 of 1; and
- the final Release root build completed with 0 warnings and 0 errors.

The exact final managed development publication hashes are apphost
`4450c4552ac44a4e463db6c9adad89a39d803da47801801019ee08c02045bd61`,
version marker
`fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4`,
Core assembly
`80d54b191638c7836948deb4153835854a20bd7fbb76fafe74ff9b7538a9505b`,
root assembly
`43f057579d0166cafbd1826b109ea80dcabf4b4f3368d9c4a5192c0929a90caf`,
and flat publication manifest
`e7f9fd9364313b4ebb0176857ea9290e3b6f043836d253ab235730c47a5366ac`.
Manual compact and expanded published journeys both returned all six domains,
counts, and actions without writes; terminal `doctor --version` bypassed
workspace inspection.

Targeted folder whitespace verification over all Doctor and changed root
composition paths completed silently, `git diff --check` passed, prohibited-
pattern and protected-path scans found no matches, exactly six
`ReadDoctorAsync` call sites remain in Doctor, and the temporary diagnostic
fixture was removed. Full managed and supported `linux-x64` Native AOT gates
remain for post-review acceptance.

## Whole-Task Review And Pending Gray Amendments

Fresh whole-task review `T16-R1` consumed four material findings against the
immutable coherent-production commit `4db37674f7d5b416e359e99e5143bb6f284df3e7`,
tree `9f2ed3c4d8eb530a9c511a7b9a03e820efcd9077`. The accepted findings and
dispositions are:

- `T16-R1-F1` accepted: only 52 of 112 finding kinds had production emission
  paths; the remaining kinds lacked honest mapped producer facts.
- `T16-R1-F2` accepted: lifecycle absence was not safely proved in all cases and
  route diagnosis could be suppressed by that unsafe absence.
- `T16-R1-F3` accepted: the focused evidence was insufficient and included
  false-green results.
- `T16-R1-F4` accepted: candidate basis evidence was absent.

The six following producer-fact clarifications were separate first-Gray clauses,
not additional `T16-R1` findings:

1. Framework and Extension partial lifecycle is a finite mixed-current-state
   observation within one exact trusted declared managed subject or set, while
   more specific managed missing and changed findings remain.
2. Framework partial recovery requires verified structured Framework-owned
   recovery provenance and plan attribution; the first Gray text retained a stop
   condition until the recovery schema could supply it.
3. Extension bridge-registration retains an exact trusted declared and owned
   registration target identity first; its observed state may be missing,
   unreadable, or inconsistent, and content is inspected only when the target is
   readable.
4. Extension unmanaged-like-content requires an exact contained readable
   supported manifest signature in the bounded scan universe without trusted
   ownership for that identity, and remains informational only.
5. The four local-reference candidate bases are exact producer-observed facts;
   every applicable basis is retained, and a basis that current typed facts
   cannot establish is omitted rather than inferred.
6. Recovery attribution remains a neutral producer fact beside recovery models
   and is consumed in lifecycle diagnosis only when verified, without a Doctor
   enum, presentation dependency, or generic bag.

Six topic-specific correction preflights completed against the current production
and contract surfaces. The first Gray amendment was committed as
`1d8624d3435e566ab3413eb91a48106c0eb25d0f`, tree
`a32c2366d5ba01e58ce356af4ff031fccdd34d99`, with parent
`4db37674f7d5b416e359e99e5143bb6f284df3e7`; it remains immutable and preserves
the four `T16-R1` findings and the six separate producer-fact clauses.

The user-authorized second Gray amendment now freezes one public schema-v1 shape.
The product remains unreleased: the discriminator remains exactly `1`, with no
v2, dual reader, or compatibility layer. Every final manifest requires immutable
typed attribution with a finite producer, finite operation, and typed subject
`{kind, identity}`; all seven current writers and future Tasks 5, 6, 17, 18, and
19 must supply it; and command, GUID, path, filename, or ordered-entry values
never infer it. Existing schema-1 finals lacking valid attribution fail closed as
malformed/unattributed and remain preserved. Unknown versions are unsupported.
Drafts remain exact-name/path-only incomplete facts; observers do not inspect or
use draft bytes for attribution. Only a semantically verified current-v1 final
exposes attribution. Cleanup keeps its same-workspace lease, re-enumeration, and
final semantic revalidation; attribution is not deletion authority. Repair's
future writer supplies its own exact producer, operation, and subject. Doctor
Framework partial recovery follows the exact neutral same-workspace comparison
rule recorded below; Framework attribution and generic mixed state alone never
prove it.

The durable schema-v1 vocabulary now admits only the exact producer/operation
pairs and `workspace` subject kind in the Mutation And Recovery Technical Design
table. Each writer uses the selected `CliWorkspace.PhysicalRoot`, normalized by
`WorkspaceIdentity.NormalizePhysicalPath`, and the required non-null identity is
`WorkspaceIdentity.Key(...)`. Unknown, null, malformed, or cross-combined values
are malformed with no fallback. The current writer inputs all carry this trusted
workspace identity, so no project-change request was required. Draft observers
remain path-only and do not inspect or use draft bytes for attribution.

A subsequent overseer correction narrowed `framework.partial-recovery` to a
non-historical comparison. For one semantically verified same-workspace final
with verified Framework attribution, a neutral producer compares current
ordinary target state for that bundle's ordered existing-target entries with
each recorded exact prior and intended state. A prior match is a safely
observable ordinary file contained by the workspace with the exact recorded
prior length and lowercase SHA-256. An intended match is the same exact
ordinary-file comparison against the recorded intended state, or safely proven
absence when the intended state is absence; absence is not unavailable. The
finding is emitted only when at least one entry matches prior, at least one
other matches intended, every compared entry is safely observable, and no entry
is third or unknown. All-intended is a no-finding state compatible with a
completed historical operation; all-prior is a no-partial-finding state
compatible with an unapplied or fully restored operation. Unavailable, unsafe,
non-ordinary, third, unknown, or mismatched state yields incomplete or blocked
coverage or another exact finding, never partial recovery. The finding describes
mixed current state relative to recovery evidence and never claims recovery
occurred.
The producer may expose finite comparison states or bounded evidence only; no
payload bytes enter Doctor output. Each verified Framework-attributed final is
evaluated independently in deterministic catalogue order; entries from separate
bundles are never ranked, selected as a winner, or combined.

The second amendment changes only the eight mutation-owned prose paths recorded
in this packet. Final authority content SHA-256 values, excluding this
self-referential Task record, are:

- Mutation and Recovery Technical Design:
  `afe9db18a95de4860e378d70350c7a93125bacbbd449cdadf05c01c70b409a23`.
- Repair Interface and Behavior:
  `92ea7c6a888149f22a4e473aac492ba042fd249368f16651704f6968239b453b` and
  `7a33b333f78a822092fdbe39ef0e9c00c76b18daa4814130e7996021cb08bb89`.
- Cleanup Interface and Behavior:
  `fc7ef7da44b0d8d9d45bb33ac30f90b1e213a2ca5e3ca574fbd2b0d6b8e8a361` and
  `d653daf7f92060142dd9e831e823283c43557bf03887397918d8b6f6d2cd4067`.
- Doctor Interface and Behavior:
  `1c3c482386375bb23f4105ad9c38898ad3ea72cbe8728d1f3abb95d06c440c71` and
  `56e81aacd3976c77d4064721439068c8cf6cafcf0a5e2983a3c3a11fea4e2433`.

The second amendment was committed as
`074a050c6c60baa67cb12c8ce29693aea5b473c2`, tree
`73d6069ab7c25b3eb3c8fb45d1775acc93bb3272`, with parent
`1d8624d3435e566ab3413eb91a48106c0eb25d0f`; it remains immutable. At that
historical boundary, phase and milestone remained 4/5 and 4/8 while grouped
correction `T16-C1` was pending. The final accepted state is recorded below.

## Current Doctor-v1 Disposition

Under the accepted unreleased schema-v1 horizon, the current Doctor catalogue
contains exactly 108 kinds: 21 workspace, 4 recovery, 22 route, 28
local-reference, 14 Framework, and 19 Extension. The current catalogue removes
`route.child-missing`, `route.cycle`, and `route.overwrite-ambiguous` without
aliases, migration, fallback, or synthetic producers; the schema discriminator
remains exactly `1`. The historical `112`-kind and `52`-emission receipts above
remain historical facts and are not current completeness claims.

The accepted producer-backed subset supplies emission paths for all 108 retained
catalogue kinds. Task 17 closed the accepted set-valued
`extension.bridge-registration` observation by extending the typed contributor
views and Doctor; the Extension domain has no remaining observation horizon.
Task 18 owns Extension Remove only and has no installed-manifest scan or Doctor
producer obligation. Route Remove is complete with its accepted
positive-unmanaged removal boundary. Route Inspect's projection-derived synthetic
overwrite-ambiguity conflict remains command-local and is one bounded Task 10
audit input, not a Doctor finding or a new task.

The Task 18 activation base still contains the now-unreleased
`ExtensionUnmanagedLikeContent` enum member, wire mapping, and 109-count mapping
expectation. Task 18 removes that narrow orphaned executable shape before its
acceptance. This is conformance to the corrected catalogue, not a new Doctor
producer, observation horizon, compatibility path, or schema change.

## Current Extension Producer Closure

Task 17 froze the exact declared and owned bridge-registration role, target
identity, and observed state, then extended the accepted typed contributor views
and Doctor. The resulting `extension.bridge-registration` observations are
producer-owned and remain bounded to ordinary generated-navigation facts.
Task 18 does not own an installed-manifest observation universe and introduces
no scanner, storage, registry, compatibility, lifecycle-schema, or Doctor
behavior. Neither task may use legacy `open-forge.extensions.json`,
package-source manifests, broad `.agents` recursion, payload/path/byte
resemblance, Framework bridges, static CLI composition, dependency injection,
or a runtime registry as a substitute for producer facts. Static composition
remains wiring only.

The final pre-release completeness gate has an honest emission path for all 108
finding kinds. This correction does not revise the historical `T16-R1-F1`
receipt: only 52 of 112 finding kinds had production emission paths at that
earlier boundary. It does not weaken any other Doctor behavior. Task 17 is
complete and dequeued; Task 18 is a separate lifecycle command task with no
Doctor horizon.

The incremental amendment's historical authority content SHA-256 values,
excluding this self-referential Task record, were:

- Doctor Interface and Behavior:
  `f00393b476ff2e54e07498027cea313a562a4e4d49a1a4e44c168267e74cc18d` and
  `9665c166a184d3de5e00b511f13bb76f669d22ba11c7ee99f6904e05f74d7830`.
- Extension Update and Extension Remove task records:
  `8290f13cc081bf3194d3a0229c27c1475ef738ac1a0be0000aff0182a15bcf76` and
  `82dd32d113abc8a2a5e1fcf6d26d323c2f8634517ab14d1ed32cc8838358aad5`.
- Project control ledger:
  `e827360cf90518d65940115470a4d398d6d87394648a7b96a582852a32f63152`.

## Acceptance And Integration Closeout

Fresh whole-task review `T16-R2` found eight material issues (`F1`–`F8`). Sagan
revalidated every finding and supervised Curie's one grouped `T16-C2`
correction. The immutable accepted correction chain is:

- `1e5abb672c577f246fc18acc417ff3b49f8445a1`, tree
  `3358722eea919e2baace5f46a267269f6b955e5e`, for the 28-path C# correction
  plus the byte-identical sealed restart handoff;
- `e0f23e2c23f966981a21ab04d0bb6addd5e4cfb0`, tree
  `9d792059cf62331d3bd2f001bf8fad269b945843`, for the stale neutral source-enum
  evidence; and
- `a48a16cd80102331bca6d6cb3498160f37eb6b3f`, tree
  `90e66b0562fea90c2aa2fed9bd573c59676124bd`, for the exact two-path Route List
  consumer correction exposed by the full Integration gate.

No immutable commit was amended. The final Doctor lane and index are clean.
Exactly three simple public Doctor End-to-End journeys remain. The four exact
handoff commands passed `3/3`, `3/3`, a warning-free End-to-End Release build,
and `3/3`. Formatting, static, protected-path, callable-shape,
prohibited-pattern, and line-length checks passed. The Route List regression
selection passed `8/8`; its complete suites passed Unit `137/137`, Integration
`82/82`, and EndToEnd `18/18`.

Fresh final acceptance under SDK `10.0.111` passed the Release solution build
with zero warnings and errors; managed Unit `1768/1768`, Integration `937/937`,
and EndToEnd `175/175`; supported `linux-x64` Native AOT root, Integration, and
EndToEnd publication; native Integration `937/937` and EndToEnd `175/175`; and
managed-on-native Doctor `3/3` and complete EndToEnd `175/175`. Every test gate
had zero failures and skips. Accepted native artifact SHA-256 identities are:

- root: `5d148821fd4fb4d16c17b588f43b30d1e2f642886d96ce919999ec3b6db5c20f`;
- version marker: `fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4`;
- Integration: `48a65690ceb9787e9e3bd72c942256901dd4cc8009b1ff6d922873b65639b686`;
  and
- EndToEnd: `dc7d17c66aefbf74a3fe4eac63c4cf82506dc40b92d845da6797a857a0cbd85e`.

All three executables are stripped x86-64 ELF PIE files. The grouped 28-path C#
inventory hash is
`24fde84c837d2aaf0ca471e3247dcb527d805fff040543b01a9b966e673bf065`;
its content aggregate is
`13f369666fdeb209c94bcf0b5cda9d03f8d746d67d440f47988c5f97a3ecaca4`.
The sealed handoff hash is
`e6f232cbfa6abbbfb4d8b9a6a57b01a170e74d0f28d0fee6624ac259541f4880`.

Doctor commit `e602e2c1990930338286f938ffb3eb747f31daaa` and `develop` commit
`f8e542a96c9f55679a8435f941bdd15ae67c0a78` have identical stable patch ID
`35f5ded05af2e778b6934f8e253cec7f7a7cb835` for the isolated prose cleanup.
Squash integration `59276c3bd764be4601ea92acbf4742b9bfa86837` therefore has exact accepted
tree `90e66b0562fea90c2aa2fed9bd573c59676124bd` without duplicating those three
paths. Its post-integration Release solution build passed with zero warnings and
errors, and the exact public Doctor selection passed `3/3` with zero failures
and skips. Task 16 is complete at phase 5/5, milestone 8/8.

## Read-Only Preparation And Activation Reconnaissance

The earlier Luna/max single-owner preparation against `develop` commit
`328599a006ef206fd82004e778296c2cac2bc10c`, tree
`56a24a7a50550702eae13bcbfaed9e9ead2a19f3`, remains historical context. Its
workflow limitation and later correction remain in the [trial
results](../../../../emerging/observations/2026-09-03_supervised-luna-preparation-trial-results.md);
they are not current authority for Task 16.

A later read-only activation reconnaissance began at root commit `53805940`. A
Sol/xhigh Task Mastermind successfully supervised three Luna/max Explorer
inventories from `/root/doctor_activation_recon`. No build, test, source or
workspace mutation, artifact, or activation occurred; the root advanced only
through coordination ledgers and is clean at `bbf2d87c`.

The reconnaissance recorded the then six-domain, 112-kind Doctor contract as
historical context: workspace/entry; recovery/residual; routes, metadata,
overwrites, and generated navigation; local references; Framework lifecycle;
and Extension lifecycle. The accepted current disposition is recorded above.
Doctor consumes Task 15's immutable typed views; it does not parse
rendered Status output or fan directly into producers. Exact callable shapes
remain Status Gray-owned, and neutral readers remain reusable. Doctor must not
import another command's private `Shared/**` or introduce dependency injection,
a service locator, reflection, a runtime registry, a generic operational
engine, or mutation.

Status acceptance and integration are complete in the fresh
`codex/doctor-implementation` base above. Gray commit `24a702efa02fb11d25036cd85f3c7816bf207ded`
restores its exact Doctor contract amendments there, including the plural
immutable Extension source observations. The Task Mastermind accepted the
post-Status Gray reconciliation and the Red boundary above; coherent production
is next.

## Expected Outcome

`doctor` evaluates accepted diagnostic domains from shared observed facts,
produces deterministic findings, severity, availability, and safe recommendations,
and performs no repair or cleanup.

## Architecture

- Keep command source at `Commands/Doctor/` with local
  `Shared/{Domains,Aggregation,Rendering}/`.
- Each domain receives typed facts and returns typed diagnostic findings. It does
  not reread another domain's files or parse Status output.
- Domain registration is explicit and statically ordered. No reflective discovery,
  diagnostic plug-in runtime, or universal rule expression language.
- Recommendations name accepted commands and exact safe next operations only.

## Requirements

Cover all contract diagnostic domains, unknown/unavailable facts, severity and
status precedence, deduplication without provenance loss, compact/expanded/JSON,
diagnostics, help, no writes, cancellation, and next actions. A healthy
workspace may have zero findings for supported observations, but the current
horizon remains honestly `incomplete` with exit `3` while both accepted
Extension deferrals remain unavailable; the final release can be complete only
after those horizons close.

## Evidence

Unit domain tests from fixed facts; Integration fixtures generated by actual
producer commands plus malformed/unsafe states; deterministic full-workspace
diagnosis; unchanged snapshots; process streams/exits; AOT; and Status regressions.

## Stop Conditions

Stop before applying a recommendation, inferring facts not observed by shared
readers, hiding unavailable domains, or introducing dynamic rule loading.
