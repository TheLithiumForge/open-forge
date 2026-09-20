---
open-forge:
  description: Correct false Doctor coverage and source-identity failures before changing its presentation
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Doctor, Refactoring]
---

# Doctor Correctness

## Frozen Stage D1

Task 27 continuation, COMPLETE phase 3/3, milestone 3/3, from local develop
`6b6f054b`, exact tree `939a3f1cfc52db9613b872d49cddd26ab83dcfc4`.
Root works directly in `<temp>/open-forge-cli-refactor-sequential`, branch
`codex/cli-doctor-correctness`. The preceding native-delimiter stage is complete
and squash-integrated. Its source candidate `5bef9a24` passes all six suites:
3,232 Unit, 1,745 Integration, and 114 public tests in each applicable mode.

The user's accepted [plan](cli-dogfood-follow-up-plan.md) requires accurate
behavior before presentation changes. D1 corrects four reproduced contradictions:

1. A complete present empty Extension section is trusted, not an untrusted error.
2. Complete exact Extension source/bridge inspection has no unconditional missing
   manifest-universe limitation. Real unavailable sources and bridge facts remain
   incomplete or blocked; no filesystem manifest scan is added.
3. Generated-region projection targets only Loader/recognized entrypoints.
   Ordinary leaves remain inputs for child metadata and local references. Missing,
   malformed and unsafe eligible hosts retain their existing diagnosis.
4. The installed source identity `embedded catalogue`, already emitted and
   persisted by Extension Install/Update, identifies embedded assets. It is not
   an explicit filesystem path. Retain the recorded identity for exact package
   and bridge joins; explicit filesystem sources retain containment/disjointness
   checks. Reuse one embedded read if both default inspection and recorded
   packages require it. No lifecycle migration, inferred source or fallback.

Persisted generated-region baselines can also become stale after legitimate
Extension/Index changes. That shared lifecycle comparison issue is D2, still
under investigation. D1's embedded-package tests assert the Extension domain;
they do not bless or hide the known Framework findings. Full presentation and
candidate deduplication follow correctness. This stage changes no rendering,
wire shape, catalogue kind, finding severity, mutation or ownership policy.

## Applicability And Evidence

Read-only public diagnosis can misdirect users and conceal incomplete checks.
The consequence is incorrect reported status or advice; this stage itself has no
workspace effects. Git reverses implementation. Tests own temporary workspaces
and locks, compare hashes across diagnosis, and exercise actual Install-produced
state. Existing interruption, malformed-input, missing-source, containment,
coverage and lifecycle trust controls stay frozen. No new malicious same-user
process or concurrency guarantee is selected.

Reuse typed lifecycle trust, existing source readers, resource catalogue,
source-form classification and generated-navigation projection. BCL exact ordinal
identity and ordinary collection filtering are sufficient. Exceptional machinery:
none. No new parser, dependency, schema, shared scope or native interop.

Fresh public producer-to-consumer journeys are the decisive evidence for the
actual bug class: Framework Install to Doctor, and embedded Extension Install to
Doctor. Freeze their three failing cases before implementation. Existing lower
tier lifecycle/source/route tests retain malformed and unavailable controls.
The initial public test file was authored before this capsule was recorded; it
has not been built or run, and no implementation has changed. This capsule now
freezes the profile before qualification and all subsequent evidence edits.

Material shared observation/public-composition changes trigger the complete
managed and Linux Native AOT gate. Required changed-C# formatting/style applies.
After Red, make the four corrections, adapt only the obsolete blanket-horizon
assertion, run the complete gate and direct native journeys, inspect the diff,
update contracts only to clarify these already accepted invariants, and squash
this verified set into local develop. No remote operation.

The first Red run proves the fresh-Framework failure (expected exit 0, actual 3),
but Extension fixture disposal masked its two domain assertions: the narrower
Framework-only helper does not own installed Extension outputs. Reuse the existing
Extension Install workspace/lock/recovery cleanup helper. No expected outcome or
production changes. Re-run Red before treating those two cases as behavioral proof.

## D1 Implementation And Qualification

Corrected Red is frozen at `7aaba4af`: all three cases fail at their intended
assertion (fresh Framework exit 3 instead of 0; installed embedded source
unavailable instead of available). No cleanup failure remains. Production then
implements the four bounded corrections above. The source identity is centralized
at its existing embedded producer and reused by Inspect/cancellation reporting.
The existing region-eligibility predicate is reused without a second classifier.

Managed Unit 3,232 and Integration 1,745 pass. The public Update journey exposes
one old expectation of unconditional Doctor incompleteness. It now expects
`attention`/exit 2 with complete coverage for the intentionally modified managed
file. Its Framework finding, Update action, Status, force-prune, repeat, effect
and unchanged-workspace assertions remain fixed. The old hand-authored Doctor
fixture retains its genuine unavailable bridge limitation; only the obsolete
blanket manifest limitation assertion is removed.

Managed direct checks over four preserved installed workspaces confirm fresh
Framework is complete/exit 0. Development, Toolkit and all packages have complete
Extension coverage and only the separately recorded Framework generated-baseline
findings. Required changed-C# whitespace/style checks pass before the final
Update expectation adaptation. The complete native gate will rerun the final
managed and native candidate. No complete final gate is claimed yet.

Final selected public Doctor/Update qualification passes 9/9 with zero skips;
this includes all three frozen fresh-install cases. Both C# checks pass for the
final Update expectation. The initial full public run was 116 pass/1 obsolete
expectation, now covered by this focused correction and the pending full gate.
Source/test diff review finds no new mutation or parser behavior. The new Red
file remains byte-identical to `7aaba4af`.

The installed CLI indexed the task route successfully. Its incidental whitespace
normalization in the unrelated Delivery entrypoint was restored. A read-only
presentation baseline captures all 28 native leaf help screens: all exit 0 with
empty stderr and maximum line width at most 80. This is coverage evidence for
the upcoming wording review, not a claim that help wording is already clear.

## D1 Qualified Closeout

Candidate `c8d786ed92d11de79361c6e5139151fa223d3d1d` passes all six canonical
managed/native suites: 3,232 Unit, 1,745 Integration in managed and native modes,
and 117 public tests in all three execution modes. Zero failures/skips.
Receipts: `artifacts/task27-doctor-correctness/native-{build,tests}.log`,
`native-qualification.json`, and delivery reports `reports-V1s3Yj`.
Native CLI SHA-256:
`a54a18bf6292b7c3829dddd8f5e2726d51a95bacda5007fbe2a9e860670eb53d`.
The global npm-linked CLI is refreshed to this exact candidate.

Four native Doctor checks over preserved fresh Framework/Development/Toolkit/all
installations match the intended D1 outcomes and preserve both workspace and
external-state hashes. Fresh Framework is complete/exit 0. Embedded package
Extension domains are complete with no false findings; the separately recorded
Framework generated-baseline issue remains visible. Required C# checks pass.
New regression assertions remain byte-identical to corrected Red `7aaba4af`.

Final source review verifies exact recorded source joins, unchanged explicit-path
checks, no source fallback, shared region eligibility and preservation of real
unavailable/malformed facts. No finding kind, JSON field, default filter,
mutation, ownership or recovery functionality changes. The [presentation audit](cli-presentation-audit.md)
records the full surface pass and pending proposals. The user must approve
public diagnostic-kind retirement or changed default visibility before those
are implemented. This closeout accompanies authorized local squash integration.

## Durable Qualification Summary

Generated logs and machine reports mentioned above are disposable. This tracked
summary, committed regression sources and ordinary build scripts retain the
required result and reproduction path. Exploratory task scripts are not build
inputs or a substitute for committed regressions.

- Qualified source: `c8d786ed92d11de79361c6e5139151fa223d3d1d`; local squash: `ef7bdb3b`.
- Native target: `linux-x64`; version: `0.0.0-dev.sha-c8d786ed92d11de79361c6e5139151fa223d3d1d`.
- Native CLI SHA-256: `a54a18bf6292b7c3829dddd8f5e2726d51a95bacda5007fbe2a9e860670eb53d`.
- Gate result: 3,232 Unit; 1,745 Integration in both modes; 117 public in all three modes; zero failures/skips.
- Toolchain: .NET SDK 10.0.111, Node 24.19.0, npm 11.17.0, Linux x64.
- Reproduce from that source commit with repository dependencies restored:
  `npm run build:native -- --sha`, then `npm run test:built`.
  These tracked scripts generate fresh outputs and validate all six suites.
- Required regression sources: `src/cli/tests/`; build/test orchestration:
  `scripts/delivery/`. No required helper exists only in `artifacts/`.

These are recorded past results. Deleting outputs discards raw receipts and
binaries; rerun the commands before claiming fresh execution evidence.
