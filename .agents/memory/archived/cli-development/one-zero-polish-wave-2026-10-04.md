---
open-forge:
  description: "Historical snapshot: Accepted 1.0 polish selection, current wave horizons, and frozen decisions"
  tags: [Memory, CLI, Task, Contextual, Archived, Historical]
---

# 1.0 Polish Wave

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/one-zero-polish-wave.md` while the live record was shortened to unfinished work. This snapshot preserves earlier plans, decisions and receipts. Read the current record for its remaining review or execution boundary. Historical timing, dispatch and release statements grant no current authorization.

## Current status, 2026-10-04

The [project-control ledger](../../working/cli-development/project-control.md#active-task-ledger) defines
current task state and the next boundary. [Task 70](tasks/task70-existing-workspace-adoption-during-installation.md#current-release-hold-2026-10-02)
records the release hold and separate local reused-worktree correction. Its
dated hold supersedes the earlier noon window, pending hosted gates and
pre-publication claims below. Further publication is not authorized.

[Tasks 73](tasks/task73-layered-adoption-and-installation-choices.md) and
[72](tasks/task72-extension-wizard-terminal-layout.md) are locally accepted in
the isolated onboarding worktree, with exact qualification receipts retained.
Their changes remain uncommitted, unmerged and unpublished. Tasks 34 and 39
remain paused, Task 48 remains deferred, and Task 71's pipeline analysis remains
an open follow-up.

The remaining sections preserve the original selection, frozen decisions and
earlier execution checkpoints. Read their changing release and qualification
claims as historical state. Use the ledger and selected Task for current work.

## Historical 2026-10-02 execution window

The maintainer requested the first-install preservation fixes on a branch by
12:00, with squash commit, push, and release permitted only from 12:00 through
13:00 Europe/Zurich. The candidate is therefore qualified before committing,
using its actual HEAD, complete source-change fingerprint, and immutable built
artifacts. Freeze its complete file tree for exact comparison with the develop
squash. This timing instruction supersedes the earlier requirement to commit
candidate C before local qualification; every qualification gate remains.
The [Task 70 record](tasks/task70-existing-workspace-adoption-during-installation.md)
records all final local gates passing, including the actual published-beta4
upgrade. Root accepts the candidate for exact-tree integration at noon. Hosted
six-host qualification and release/public postchecks remain. No individual
requalification of Tasks 32, 47, 54, or 64 was requested.

## Purpose and authority

This capsule records the maintainer's 2026-10-01 task selection, current wave
horizons, and frozen cross-task decisions. The project-control ledger owns
permanent Task identity, queue state, and ownership. Each Task record owns its
scope, task-local phase and milestones, implementation discoveries, and
acceptance evidence. The wave phases and milestones below are separate from
task-local milestones.

`0.9.0-beta.5` is selected, and version preparation is complete. Root
independently verified that all four version fields across the root and
package-root package files and `Directory.Build.props` equal
`0.9.0-beta.5`. The receipt is
`artifacts/beta5-version-preparation/version-bump-receipt.json` (SHA-256
`E0CF49C8B15E06A4B7147FB5AAB3B575AECAC47CE8FE09CCBA474121DF734A21`). The
command exit was not observed. Version preparation is not a release.
`0.9.0-beta.4` remains public. Tasks 47, 54, 32, and 64 are squash-integrated
into clean local `develop`; beta5 has not been pushed or published. The release
sequence uses the qualified branch HEAD plus source fingerprint and frozen
complete tree under the execution-window amendment above. All local managed, native, package, site, user-journey, and
actual published-beta4-to-C5 upgrade gates must pass before the final Task 70
squash into `develop` as M. The clean `develop` M tree must then exactly equal
the qualified C tree. Only then may Root push `develop`, require the exact-M
six-host hosted Build to pass, fast-forward `main`, publish beta5, and pass public
postchecks. Final combined qualification is completed on C before the Task 70
squash, not on `develop` afterward.

Task 69's beta4 receipt and Task 68's link-validation receipt remain complete
and unchanged. This selection creates no top-level Task ID or Framework
category.

## Selected implementation tasks

All seven tasks in the 1.0 selection completed preflight. The current beta5
horizon is Tasks 32, 47, 54, 64, and 70. Task 70 is the top beta5
qualification priority. Its v9 handoff is sealed and independently verified,
Root source intake is accepted, and local qualification is ready for combined
qualification. Root's local combined qualification passes; hosted qualification remains. Tasks 47,
54, 32, and 64 are independently qualified in all required individual modes
and Root-verified, squash-integrated into clean local `develop`. Final local combined qualification passes; hosted qualification and release remain.
Tasks 39 and 34 are paused, and Task 48 is deferred beyond beta5 and required
before 1.0. Wave-level release acceptance remains outstanding.

| Task         | Canonical task name                                                                                        | Current wave state                                                                                                                                                                                                                                                                                                                                                                                |
| ------------ | ---------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 54           | [Tag trimming for 1.0](tasks/task54-tag-trimming.md)                                                       | Phase 3/3, milestone 2/3; independently qualified in all six modes plus package and squash-integrated into `develop` as `29c22d5f651ebaf9fac11e16ca60049b7c8e762d` on 2026-10-02. The accepted 89-path patch includes 64 numeric-only loading-cost captures. Local combined gates pass; hosted release gates remain; not closed.                                                                  |
| 39           | [Output Audit](../../working/cli-development/tasks/task39-output-audit.md)                                 | Phase 2/3, milestone 1/3; paused by the user. Final checkpoint `r_f4bdc49aa2c4` saved; all workers stopped. No beta5 integration or captures.                                                                                                                                                                                                                                                     |
| 34           | [Interpolated Value Markup](../../working/cli-development/tasks/task34-interpolated-value-markup.md)       | Phase 2/3, milestone 1/3; paused by the user. Final literal-correction checkpoint `r_b68ae7955e36` saved; all workers stopped. No beta5 integration or captures.                                                                                                                                                                                                                                  |
| 47 follow-up | [Default Skill indexing](tasks/task47-default-skill-indexing.md)                                           | Phase 3/3, milestone 2/3; independently qualified in all six modes and package/planning journeys, then squash-integrated into clean `develop` as `27a916dcd1c165f2610dd20b625dbe10beb0491b` on 2026-10-02. Local combined gates pass; hosted release gates remain; not closed. Index reachability only; no new top-level Task ID.                                                                 |
| 64           | [CLI defects and contract drift](tasks/task64-cli-defects-and-contract-drift.md)                           | Phase 3/3, milestone 2/3; independently qualified in all required individual modes and Root-verified, squash-integrated into clean local `develop` as `292ab17bc641f4d7cd3933f976dc2586e78e9899`. Local combined gates pass; hosted release gates remain; not closed.                                                                                                                             |
| 48           | [Scoping for Extension routes](../../working/cli-development/tasks/task48-scoping-for-extension-routes.md) | Phase 2/3, milestone 1/3; frozen at A/B1 after checkpoint `r_1d770f557a5e`; all four children idle. A: 13 files accepted and transferred, 12 tests. B1: five-file candidate, 10 unit and 6 Remove integration tests passed, not transferred. B2/C not started. Deferred beyond beta5 and required before 1.0; corrected Task 64 Init is accepted for later resume. Core-first scope is unchanged. |
| 32           | [Minimal Output Sweep](tasks/task32-minimal-output-sweep.md)                                               | Phase 3/3, milestone 2/3; independently qualified in all required individual modes and Root-verified, squash-integrated into clean local `develop` as `38956e8f77654127f15f98bb33384ddd616444f6`. Local combined gates pass; hosted release gates remain; not closed. Optional Task 32 number clarification remains an assumption, not a blocker.                                                 |

**Priority installation migration:** The user reproduced plain root
`open-forge install` on a project with an existing `.agents`; Root independently
reproduced missing or incomplete native Skill frontmatter. Prior analysis/repro
runs remain `r_81613ce6e3f5` (Astra High) and `r_5ba221ad6529` (Luna QA). The
current release is blocked (exit 5, effects 0). The beta5 user-journey
scenarios are (1) installing into an existing brownfield project with
`.agents`, including missing metadata and entrypoints, and (2) a fresh install,
then copying in a real downloaded Skill folder with its references and support
files, followed by normal CLI checks and a repeat install. The plan is frozen,
and implementation was released by Astra High in `r_d4f375e95ee6` /
`a_19282d527cdb`. The bounded plan
adds automatic migration for missing compatible metadata and entrypoints in
selected standard routes; it preserves authored bytes, native fields, and
ownership, reports migration, and follows normal confirmation/recovery,
dry-run, and idempotence behavior. Strict malformed-input boundaries remain.
Task 70's canonical record is [Existing workspace adoption during
installation](tasks/task70-existing-workspace-adoption-during-installation.md).

**Task 70 Update adoption amendment:** Root accepted adding ordinary Update in
already-managed Framework workspaces, applying the same compatible-metadata and
required-entrypoint adoption in selected standard local routes. Synthetic
earlier-payload proof (SHA-256
`7165DAE3905492FCFF9487B670027F6DD8A06C62E89FF24093B06BFCD20F01D7`) showed a
compatible partial native Skill can be blocked by Install's managed-divergence
guard yet skipped by Update's generated-region safety. Update continues
owned-payload reconciliation. Preserve user ownership and bytes, exclusions
and foreign owners, normal confirmation, dry-run, recovery, and idempotence;
add no flags. The v2/v3 results are historical. The 78-file v2 candidate had
stable hashes and its full Release build succeeded with zero warnings and
errors, but its receipts included CORE 125 passes, TYPE 63 passes
across four classes with an incorrect receipt aggregate, PRESENTATION 28
passes with a prepared-count mismatch at 39, four unexpected UPDATE planning
failures, and three REGRESSION failures plus one pass because the fixture used
the wrong overwrite filename. Two repairs were interrupted and later resumed.
The earlier 102 focused passes apply only to the pre-amendment candidate.
Install, pinned-archive, and exclusions passes remain stage evidence.

The v5 results and v6 run are historical. V5's source-ready receipt SHA-256
was `F4AC090EBB39D4DFC5DD887338588BA009D8AB84F85BA00DC85349AC22803B5F`. V6
recorded 321 focused/hermetic checks and 10 pinned Skill journeys passing.
The current v9 Release build
exited 0 with zero warnings and errors. Root independently checked all 79
source rows, including 76 Task 70 rows, two separate Task 47 prerequisites,
and one tested record. The mismatch count was zero, and the raw build-log
bindings matched. The frozen v9 build receipt SHA-256 is
`3C6424D48641799E6B46DE9E65F969EEC5E4AED1CBD765BE065542F47DBEED49`, and the
inventory SHA-256 is
`3659A8CE09764C7F2D0C35BB90891CEE25A8321373DCF045F8E834E8B69CDDA8`.
Six completed read-only lanes passed 333 hermetic checks with zero failures or
skips: CORE 128, REGRESSION 16, TYPES 65, APPLICATION 77, PRESENTATION 32,
and PROCESS 15. All 10 pinned Skill Creator journeys passed, with 45 process
captures and zero failures or skips.

The independent whole-task review `r_b0d032f1336f` is closed, consuming one
review budget. It recorded five source-derived findings, four P2 and one P3:
F1 (CRLF/BOM preservation outside Entries and the distinction between exact
projection and physical write), F2 (H1-only Skill fallback), F3 (migration
rows require actual non-directory file effects), F4 (user and Framework
ownership carried internally), and F5 (bounded Update help). The grouped
corrections are implemented, and affected v9 checks passed. The 77-source
handoff, patch, and prompt-preservation state are sealed and independently
verified; Root source intake is accepted. The transfer manifest is
`C:/Users/Tedy/.worker-watch/worktrees/a_025cfc30c08d/artifacts/task70/handoff-v9/main-final-transfer-manifest-v9.json`
(SHA-256
`01681D725CC7CB55472D78B39682DB2A521191AC97E57D5292EB02CC991737F7`). Task 70
is locally qualified and ready for Root's combined qualification. Its 333
hermetic checks and 10 pinned Skill Creator journeys passed with zero failures
or skips. Four expected optional CLI Index warnings remain, so the task
evidence is not warning-free. The Release build had zero warnings and errors.
Root has not given final task acceptance, and Task 70 is not integrated into
`develop`. Candidate
C's actual clean commit and all local managed, native, package, site,
user-journey, and published-beta4-to-C5 upgrade gates remain pending. Exact
tree identity, hosted Build, publication, and beta5 release acceptance also
remain pending.

## Beta5 qualification horizon

Tasks 32, 47, 54, and 64 are independently qualified in all required
individual modes and squash-integrated into clean local `develop`. Their phase
3/3 and milestone 2/3 remain until final combined qualification. Task 70's
sealed handoff and accepted source intake are complete; its local qualification
and independent whole-task review are complete. Root's combined qualification
remains pending.
Candidate C must be assembled in the Root worktree with its actual clean C
commit. All local managed, native, package, site, user-journey, and actual
published-beta4-to-C5 upgrade gates must pass before the final Task 70 squash
into `develop` as M. The clean `develop` M tree must exactly equal the
qualified C tree. Only then push `develop`, require a green exact-M six-host
hosted Build, fast-forward `main`, publish beta5, and pass public postchecks.
The relevant public docs and
four beta5 version fields are prepared in the combined candidate but are not
published; `0.9.0-beta.4` remains public. Tasks 39 and 34 remain paused with
saved checkpoints, all workers stopped, and no beta5 integration or captures.
Task 48 remains frozen, deferred beyond beta5, and required before 1.0. Beta4
receipts stay unchanged. See the purpose section for the release boundary and
version receipt.

## Current beta5 readiness

The local `develop` branch is clean at `292ab17bc641f4d7cd3933f976dc2586e78e9899`
after the Task 47, 54, 32, and 64 squash integrations.

- **Task 32:** Sol agent `a_e549c748b513` completed run `r_4f18526b8697`; Root
  independently qualified it in all required individual modes and
  squash-integrated its accepted 107-path patch into `develop` as
  `38956e8f77654127f15f98bb33384ddd616444f6`. The accepted patch has SHA-256
  `2bc128b1f165b7616eb854c8dd3db4d88213b0b1e2413f966e8542f5e7be9c39`. It
  passed 131 focused tests. Managed counts were 3939/2672/263, native counts
  were 2672/263, and managed-on-native was 263. Seventeen declared Windows
  exclusions and green package journeys were included. Task 32 remains at
  phase 3/3, milestone 2/3 pending final combined qualification; it is not
  closed.
- **Task 64:** Sol agent `a_69f4673603e6` completed run `r_b0c6a996c6d5`; Root
  independently qualified it in all required individual modes and
  squash-integrated its accepted 70-path patch into `develop` as
  `292ab17bc641f4d7cd3933f976dc2586e78e9899`. The accepted patch has SHA-256
  `6FE985553D9D490536E1A723C6751FBB8E4BA4148F037F6FE641A2B5A0527C4A`.
  It passed 3904 unit and 2693 integration tests, with 17 declared exclusions.
  Refreshed public counts were 263 each for managed, native, and
  managed-on-native runs. Read-only timestamp changes were reproduced as
  ReFS delayed-delete behavior without running the CLI. Writes-denied
  Doctor/Context controls passed, and exact equality assertions are unchanged.
  Task 64 remains at phase 3/3, milestone 2/3 pending final combined
  qualification; it is not closed.

Root beta5 qualification preparation `a_6ecaa48917f3`, run
`r_a3c37dc96075`, verified the preserved beta4 upgrade fixtures and cache-only
managed and three-win-x64 restore graphs. The final beta5 combined managed,
native, package, site, user-journey, and real beta4 upgrade gates remain.
No release notes are required.

The four-task pre-Task70 baseline run `r_de912d2d7e91`, owned by Luna agent
`a_6ecaa48917f3`, completed on `0.9.0-beta.4` and excludes Task 70. Its full
Release build had zero warnings and errors. It passed 3975 Unit and 2698
Integration tests, with 17 declared operating-system exclusions, and 264
Public tests. `npm run verify` exited 0, including 55 delivery tests, 7
package-layout tests, .NET format, and RepositoryDocumentation. The owner is
sealing the final integrity receipt. This baseline is not final beta5 proof.
Worker Watch infrastructure repair `a_eac947212992`,
run `r_b9ed83553c55`, completed 27 regressions including real Windows sharing
locks and verified a generated build of 42 files. The daemon was not restarted
while workers were active. Archive-only run `r_b5b490308694` by
`a_fcdcd2876079` preserves 18 old trees; no deletion has occurred.

## Historical pre-Task70 integrated-candidate checks

Before Task 70, four independent GPT-6 Luna max checks recorded passes against
the beta4-versioned integrated candidate: Task 54 had 28 tests, Task 32 had
102, Task 47 had 26, and Task 64 had 54. All 210 tests passed with zero
failures or skips. The shared clean Release build exited 0 with zero warnings
or errors. Each run verified all 361 build-closure hashes and unchanged
Git, source, and untracked identities. Source HEAD was
`11a801d0dd5a670c480dfacc0681337caca184f1`. The build-ready hash was
`68ED14D4248F235FC560134F3E054CC2348516D3106EE6E5B8E565BBC232D4BF`. The
Task 70 integration source freeze is released. These checks are historical,
not final beta5 qualification or task-closure evidence. Tasks 32, 47, 54, and
64 are now independently qualified in all required individual modes and
Root-verified, squash-integrated into clean local `develop`. Their existing
phase and milestone labels remain unchanged. The
individual receipts and hashes are recorded in
`artifacts/one-zero-polish/task70-execution-state-reconciliation.json`.

## Frozen qualification receipts

- **Task 54:** Twenty-eight focused tests across five classes passed, with zero
  failures and skips and all exits zero. QA receipt `r_692a527eeb83` resolves
  the initial missing-exit receipt. Fresh Sol 6.1 High review
  `r_3096ec2adf82` reported no material findings; review 1/1, repair 0/1.
  Master final: `r_aa5944a122bd`. The Release build had zero warnings/errors.
  The frozen 25-file patch SHA-256 is
  `38E8FECD1EEEF0E1E8E5917938D6E5590305D8F6E3636D0A87C66F0D98DD9835`.
  Transfer `r_87b2453f2c63` completed; Root independently verified 25/25
  destination hashes, preserving 200 unrelated status/hash entries and both
  real indexes. Receipt: `artifacts/task54/integration-transfer.json`,
  SHA-256 `7EB621E30B9D65660CF825751566D37C87DC0361604B3CBA0C5652C208BCC792`.
  Root accepted Task 54 for beta5 combined qualification and squash-integrated
  it into `develop` as `29c22d5f651ebaf9fac11e16ca60049b7c8e762d` on
  2026-10-02. Final combined gates remain and Task 54 is not closed. See the
  [canonical Task 54 record](tasks/task54-tag-trimming.md).
- **Task 47 follow-up:** The original 13-file transfer `r_ee4eaa84b49a`
  remains valid, with Root verifying all hashes before these receipt-only
  updates. Root accepted the candidate for inclusion in combined qualification
  after focused integration, documentation, and physical checks. The final
  combined managed, native, package, site, and user-journey gates remain. Root
  independently qualified all six modes and package/planning journeys, then
  squash-integrated Task 47 into `develop` as
  `27a916dcd1c165f2610dd20b625dbe10beb0491b` on 2026-10-02. The evidence and
  bounded dispositions are in the [Root qualification addendum](tasks/task47-default-skill-indexing.md#root-qualification-addendum-2026-10-01).
- **Task 32:** Fresh review `r_b85fdd23a797` found no material production
  defect; 99 source hashes matched and 83 text captures had no JSON changes.
  R1 and R2 led to the one grouped repair release `r_5d3f44d6a7ca`, limited to
  three test files; review 1/1, repair 1/1, councils 0. Source evidence has
  102 tests with zero failures/skips and full whitespace/analyzer verification.
  Historical integration `r_572300b52200` verified 96/99 files and stopped
  before builds/tests because three already-reviewed additions were omitted
  from the handoff; this was a transfer omission, not a product defect.
  Replacement `r_7e676fedfbd6` copied those three additions, matched all 99
  hashes, passed two clean Release builds and 102 focused tests, and preserved
  94 unrelated status/hash records. Root accepted this earlier integrated
  candidate for beta5 combined qualification. That earlier transfer receipt is
  historical and does not describe the current 107-path patch. Root
  independently verified and squash-integrated the current patch as
  `38956e8f77654127f15f98bb33384ddd616444f6` on 2026-10-02. Final combined
  gates remain; Task 32 is not closed.

The labels `output audit 49` and `markup 44` map by task name to Tasks 39 and 34. Do not reopen historical Tasks 42 or 49. The spoken reference to Task 42
for minimal output is assumed to mean Task 32, not archived Task 42. Task 44's
Template and Core content work is postponed and is not selected for
implementation.

## Frozen decisions and coordination

- **Task 32:** Retain the current Workspace policy; correct seven command
  overrides; retain 17 meaningful `Next` duplicates caused by severity-filter
  behavior; hide only the healthy generic ExtensionList hint at minimal
  output; put the ExtensionInstall permission alternative before the final
  `Next`. Design is ready. The optional Task 32 number clarification remains
  an assumption, not a blocker.
- **Task 34:** Use explicit literal code fences in authored wording, the same
  display string in text and JSON, and preserve raw machine coordinates.
  Standalone Usage, synopsis, grammar, and command-kind `Next` output remain raw
  in text and JSON. Markup applies inside explanatory authored sentences. This
  is Root's publicly stated compatibility-preserving working assumption, not
  user approval. Integrate semantic output before final markup and captures;
  later families and captures wait for semantic and policy integration. Latest
  master: `r_75f8e3c0ed05`.
- **Task 48:** Use installed Extension-owned scoped copies with matching
  Install/Update lifecycle behavior; retain current declared inventories and
  whole-file receipts; preserve the existing Core-first interpretation. Add
  no JSON schema or origin table. The A/B1 checkpoint is frozen. The corrected
  Task 64 Init is accepted for later resume. Task 48 remains frozen at A/B1,
  deferred beyond beta5, and required before 1.0; preserve the Core-first scope.
- **Task 39:** Preserve Windows file-in-use behavior with typed exceptions and
  OS facts; do not parse strings or add public codes or schema. D04's
  ExtensionRemove dependency `Next` contract intent is intentional and
  unchanged.
- **Task 64, option A:** Use the exact observed recovery-file subject and
  cleanup `--dry-run` review guidance, preserving the current admission
  policy. Optional verified-final coexistence remains deferred and is not a
  blocker for option A.
- **Task 64 recovery decision:** A stored `Blocked` state with no residual path
  uses existing `Incomplete` / `RecoveryUnavailable` with no effects or
  invented recovery-file subject; real residual conflicts remain `Blocked`.
- Task 32 defines the minimal-output policy, Task 39 owns the error-fact audit,
  and Task 34 owns literal markup. Coordinate shared output policy, error
  facts, and literal markup to avoid competing edits. Semantic-output
  integration precedes final markup and captures.
- Task 47 is limited to default Index selection and Skill resource reachability.
  It does not select unrelated route discovery or context-loading changes.
- **Task 54:** Root accepted the transferred frozen candidate for beta5
  combined qualification. Final combined gates remain; Task 54 is not closed.
  Preserve the eight defined tags
  (`LoadNow`, `KeepInMind`, `Core`, `Memory`, `Extension`, `Contextual`,
  `CurrentTruth`, and `Evergreen`), their loading meanings, every `applyTo`
  condition, and archived tags.

## Deferred and unselected work

- Task 53's full loading and scoping audit is postponed.
- Task 44's Template and Core polish, Task 36's partial merging, and Task 63's
  managed-file edit policy are postponed.
- Task 61's maintainer review and the prior reviews for Tasks 66 and 67 are
  postponed. Current beta5 documentation reconciliation is Root-owned in this
  wave. Committed review material remains intact.
- Task 40's broad capture-coverage expansion is postponed. Task 58 remains
  after 1.0.
- Task 37 is not selected by this direction. Its earlier scope decision
  remains pending.

## Ownership and execution boundary

The root Overseer owns shared contracts, integration, final acceptance, and
release coordination. Task workers use separate worktrees. The beta5 horizon
above is current; task-local receipts and evidence remain in their Task
records. Shared-ledger coordination uses separate, disjoint files.

The supplied wave base commit was
`2e0da10a5657c084b2847245775858123e09a5ed`. The current local `develop` HEAD
is `292ab17bc641f4d7cd3933f976dc2586e78e9899`, and the branch is clean after
the four Task integrations. Task worktrees remain separate from the
integration worktree.

Task 70 source authorship used seven disjoint GPT-6 Luna max lanes. Its
GPT-6.1-Sol / high mastermind completed run `r_206aa377b864`; the sealed
handoff and canonical record are verified. See the [Task 70 record](tasks/task70-existing-workspace-adoption-during-installation.md).
Earlier independent GPT-6.1-Sol / high
completion assignments are retained below for provenance. Root owns combined
qualification for Tasks 32, 47, 54, 64, and 70.

| Task | Earlier agent    | Earlier run      |
| ---- | ---------------- | ---------------- |
| 32   | `a_e549c748b513` | `r_cbf5998fca8b` |
| 47   | `a_1bec444bebb4` | `r_54dbdc5b958b` |
| 54   | `a_392d98ddb935` | `r_baced1c9c479` |
| 64   | `a_69f4673603e6` | `r_2c5dc7545819` |

These earlier assignments did not advance task phases or milestones. Tasks 32,
47, 54, and 64 are independently qualified in all required individual modes
and Root-verified, squash-integrated. All remain at phase 3/3, milestone 2/3
pending final combined qualification. Earlier
concurrency-cap receipts remain historical. Each meaningful task retains one
fresh whole-task review, one grouped correction pass, and zero councils. Their
pre-Task70 checks above are historical.
Wave-level qualification and acceptance are outstanding.

## Decision-record closeout

[Task 55 “Alternative workspace root such as .apm”](tasks/task55-alternative-root-decision.md)
is complete at phase 1/1,
milestone 1/1. The maintainer retained `.agents/` as the sole root. Ordinary
coexistence with APM remains product direction, not compatibility
certification. No dedicated APM adapters were selected. The completion
checkboxes record the accepted decision and workspace-file reconciliation,
not external tests.

[Task 65 “Where open Tasks live”](tasks/task65-where-open-tasks-live.md)
is complete at phase 1/1, milestone 1/1. Open
Task records remain in Working Memory even when they are not currently active.
The completion checkboxes record the accepted placement decision and updated
selection views, not a file migration or runtime tests. The broader
[Local Planning review](../../working/local-planning.md) remains open.
