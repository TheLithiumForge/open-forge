---
open-forge:
  description: Align public documentation, accept all six targets, and prepare an authorized release
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Distribution, Documentation, Release]
---

# Task 22: Final Documentation, Acceptance, and Release

## Delivery Simplification Follow-Up

The approved [Task 13 continuation](02-native-ci.md#delivery-simplification-continuation)
owns the shared-script, synchronized version and pipeline-only publication
changes begun on 2026-09-12. Its current capsule supersedes the older version,
workflow and main-only release constraints below for that bounded change. This
Task retains its completed historical evidence; no shipping release or remote
execution is authorized by the follow-up.

## Task State

- State: Complete and squash-integrated under the accepted local-only horizon.
  Tasks 21, 27 and Task 7 are complete. Publication is excluded from this horizon.
- Current phase and milestone: phase 3 of 3, milestone 6 of 6.
- Current-state suffix: M6 complete. Accepted feature `6d370632` is integrated
  into local `develop` at `3bf03e0e`, exact tree `33c98766`.
  No remote operation, foreign-host execution or publication is claimed.
- Permanent mapping: Task 22 “Final Documentation, Acceptance, and Release” in
  the [project control ledger](../../project-control.md).
- Responsible roles: Mastermind and maintainer.
- Parent: [CLI Delivery](_delivery.md).

## Accepted Local Completion Boundary

On 2026-09-10 the user accepted Linux execution plus static review of all
supported platform jobs, prohibited remote Git/GitHub operations and publication,
and instructed squash integration into local `develop`. Local Git is the source
of truth. This replaces the earlier hosted and release prerequisites for current
completion; it does not establish foreign-host execution or a shipping release.

The existing three-phase, six-milestone horizon retains M1 through M3. M4 is
Task 13's accepted local platform qualification, M5 is acceptance of the prepared
local source and exact evidence, and M6 is local squash integration. The approved
candidate consumes `c2eb60b3` runtime receipts and the reviewed reporter correction
`696c56b7`; later state-only changes do not require another full runtime gate.
Both documentation reviews remain accepted. Release version selection,
publication and public registry verification remain outside this local horizon.
The pre-release and release sections below describe a future separately
authorized release, not outstanding work within this local completion.

## Expected Outcome

Public documentation, package metadata, command help, schemas, the complete
accepted six-target package graph (`linux-x64`, `linux-arm64`, `osx-x64`,
`osx-arm64`, `win-x64`, and `win-arm64`), and release
notes describe the exact product. One separately authorized main-only release
publishes the verified native and packed artifacts and passes public smoke tests.
Current package implementation covers six targets; matching-host receipts remain
incomplete outside Linux x64.

## Local Acceptance Execution Capsule

- Base: `fae17b29` on `codex/cli-delivery`. Root accepted the nine bounded findings
  and eleven-path correction scope in
  `artifacts/task22-preparation/author/preflight.md`. The packet includes full
  README, Extensions and development reads plus targeted CLI-guide inspection;
  it is not a new whole-product behavior audit.
- Profile: bounded public documentation and help correction for a private CLI.
  Accepted operation behavior, syntax and schemas remain frozen. One stale help
  assertion receives independent Red before its text correction. No new command,
  permission behavior, compatibility path or public journey is introduced.
- Owner: root owns authority, five C# help/composition files, one Unit assertion,
  the `package.json` `cli:dev` value, Git and runtime evidence. The continuous
  Astra/high writer owns only `README.md`, `docs/cli.md`, `docs/extensions.md`,
  and `docs/development.md` under the accepted packet. Task 13 owns CI source
  and workflows, with disjoint writes and serialized full runtime effects.
- Expected changes: current 28-command examples, six-target delivery status,
  Extension `content/` and current lifecycle/manifest/grants, five Library
  commands, the existing bounded Library Repair exception, truthful related
  command availability, and same-worktree development invocation.
- Protected: Framework and Extension source, README positioning and principles,
  frozen MVP, dependency/package graph, all operational C# behavior and tests
  except the one named help assertion, CI-owned paths, global installation,
  unrelated dirty worktrees and the user-owned Task 28.
- Evidence ladder: freeze source/help baseline and accepted findings; qualify
  the changed Route Inspect assertion as Red; commit evidence separately; apply
  help/script and public docs in coherent isolated commits; focused Unit/help,
  source/style/format/link checks; one fresh coherent review; then consume the
  combined final managed/Linux native/package gates with Task 13. Review current
  contracts and previous Task 10/21/27 range evidence for final acceptance, and
  explicitly retain any uncovered boundary. Do not duplicate full builds.
- Review budget: two bounded coherent review passes. `T22-R1` covers public
  writing and the help/evidence delta; it accepted `8adfd5d1` with no findings.
  `T22-R2` covers the later forty-file Crystallized execution-status correction
  only; it accepted isolated `d6d7564e` with no findings. One grouped correction/recheck
  `T22-C1` remains unused; council zero. The second pass is added because root
  found explicit obsolete implementation-pending claims after the initial
  eleven-path public draft. This is a distinct durable-authority boundary, not
  a repeat review of the accepted public guides or operational behavior.
- Horizon: phase 1/3 owns M1 frozen preflight and M2 aligned source. Phase 2/3
  owns M3 reviewed local evidence and M4 accepted local platform qualification.
  Phase 3/3 owns M5 acceptance of the prepared local candidate and M6 local squash
  integration. The current completion boundary above supersedes the earlier
  hosted/release gate; no publication or foreign-host execution is claimed.
- Local evidence: Red `ad0ce9e9` fails exactly the stale Doctor help assertion.
  Green `99e67a52` changes only five C# help/composition files and `cli:dev`.
  All 24 selected Route Inspect/List presentation cases pass without skips;
  four fresh same-worktree help surfaces and actual npm script forwarding pass.
  The six-file scoped whitespace gate passes. Raw evidence, exact source hashes
  and the personally read C# Directive fingerprints are recorded under
  `artifacts/task22-preparation/root/`. This is focused evidence only; the
  complete final candidate still requires the combined gate.
- Durable status correction: root froze forty Crystallized documents under
  `artifacts/task22-preparation/authority-status/`. Only obsolete execution-state
  wording changes: all command implementation is locally accepted; six-target
  proof and release remain pending; current Task records own exact evidence.
  No grammar, result schema, algorithm, invariant or verification requirement
  changes. T22-R2 verified all forty paths, eighty before/after fingerprints and
  thirty-five new links, and accepted the isolated status correction.
- Public draft: `8adfd5d1` commits the four documents. Root verified their exact
  frozen bodies and inspected the resulting changes. The writer reports 66
  local links/anchors passing, scoped format/diff passing, and thirteen protected
  README sections byte-identical. Those are targeted writing checks, not whole-
  product acceptance.
- Historical checkpoint before local-only acceptance: M3 complete, phase 2/3;
  T22-R1 and T22-R2 accepted without findings.
  The exact `c2eb60b3` runtime candidate passed all 6,421 managed/Linux native
  executions, package layout seven, original fixture one, and actual installed
  native journey one. Task 13's canonical summary and execution chain bind all
  source/runtime identities and the separately accepted reporter-only correction
  `696c56b7`. Public/help/operational inputs remain unchanged. M4 requires
  Task 13's actual all-six matching-host evidence.
  No remote execution, upload, registry publication,
  global refresh, release version selection or final maintainer acceptance is
  implied. Prepare all concrete local results before requesting external effects.

## Pre-Release Acceptance

1. Reconcile every command contract with help, docs, implementation, and evidence.
2. Run the proportional full managed and native gates, consume Task 13's evidence
   for every accepted native host and Task 7's six-target package-graph journeys, then verify
   packed installation/invocation, checksums, unchanged-state, mutation/recovery,
   and public smoke evidence.
3. Review the complete Git range from the greenfield baseline, current
   Architecture, dependencies, generated sources, the current native/checksum
   workflow, wrappers, and public documentation.
4. Resolve every blocker. Preserve explicit residual risk and unsupported boundary.
5. Obtain maintainer acceptance for the exact version and artifact manifest.

## Release

Release only from the accepted main commit through a separately authorized,
atomic workflow. Verify registry contents, the complete accepted six-target package
graph and checksums, package installation and launcher reachability for the
accepted platform artifacts, direct native execution, version/help,
representative read-only and mutation journeys, and public links. Do not rebuild
artifacts after acceptance. Do not present a platform subset as the complete
graph.

Do not claim additional RIDs, signatures, SBOM, provenance, OIDC attestation, or
support-floor coverage. Each requires a later explicit maintainer decision and
its own updated Architecture, Plan, Task, documentation, and evidence boundary.

## Closeout

Update current Architecture, contracts, Sources Of Truth, public docs, development
guide, release records, and the exact complete six-target package support statement.
Consolidate accepted Task outcomes and evidence. Archive or prune temporary
Plan/Task/Checkpoint detail.
Record follow-up work separately; do not hide it in a completed release.

## Stop Conditions

Stop before partial publication, non-main release, artifact rebuild, undocumented
contract deviation, unresolved high-severity finding, a missing accepted target
artifact or package, failed package reachability or native smoke, broader
unsupported delivery claim, or absent maintainer acceptance and exact publication
authorization.
