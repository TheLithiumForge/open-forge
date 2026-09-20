---
open-forge:
  description: Implement one atomic per-file application primitive with exact verified receipts
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Mutation, Filesystem, Atomic]
---

# Implement Atomic File Application

## Task State

- State: Complete. Historical one-change implementation remains recorded at
  `cdfde34` and its final review remains historical evidence. Exact
  production candidate `e7d937f` adds the current matching verified
  recovery-preparation requirement for every existing-target effect and retains
  the preparation-free Create path.
- Parent: [Mutation Foundation](_mutation-foundation.md).
- Responsible role: Overseer-managed Task Mastermind, sequential.
- Last updated: 2026-08-29.

## Accepted Outcome

Commands can apply one already-planned file change while a live workspace lock
lease is held and receive one truthful finite `FileChangeReceipt`. For Replace
and Delete, the applier also requires the matching verified
`RecoveryBundlePreparation` for the complete operation. The applier performs
mechanics only. Commands retain their ordered plan, grouping, policy, recovery,
and result shaping; no ordered generic mutation executor is introduced.

## Accepted Design

### Callable boundary

`FileChangeApplier` is the only application component. It consumes:

- a live `WorkspaceLockLease`;
- one `PlannedFileChange`;
- one `Matched` `FileExpectationValidationResult` whose actual snapshot equals
  the change expectation; and
- a matching verified `RecoveryBundlePreparation` for `Replace`,
  `ReplaceGeneratedRegion`, or `Delete`; `Create` supplies none; and
- a cancellation token.

Its `MutationRevalidator` dependency repeats one exact target check under the
same live lease immediately before the target effect. Its
`FileExpectationValidator` dependency observes the resulting target. The
applier validates lease, selected workspace, logical and physical target,
matched check, change kind, and expected identity before creating a stage.
Foreign, disposed, retargeted, malformed, or incoherent inputs do not affect a
target. An impossible matched-check or recovery-preparation contract is rejected
at the callable boundary; a valid before snapshot is retained for every
reachable pre-effect receipt.

### Create and replacement

Create, Replace, and ReplaceGeneratedRegion all use the same full-document
mechanics after their distinct authority checks. Replace and
ReplaceGeneratedRegion require a matching verified recovery preparation before
staging; Create never does. ReplaceGeneratedRegion does not parse or recompute a
region; its supplied bytes are the complete intended document.

For each one-change invocation the applier:

1. creates one unpredictable adjacent stage in the resolved target directory
   using `Path.GetRandomFileName`, `FileMode.CreateNew`, and ordinary managed
   BCL APIs;
2. writes all intended bytes, closes the stage, and reads it back for exact-byte
   equality;
3. repeats the one-target `MutationRevalidator` check immediately before the
   effect and performs a final cancellation check;
4. calls same-directory `File.Move` with `overwrite: false` for Create and
   `overwrite: true` for Replace and ReplaceGeneratedRegion; and
5. revalidates the intended exact resulting identity and bytes.

There is no copy-delete, in-place write, weaker fallback, retry, forced
durability (`WriteThrough`, `Flush(true)`, or `fsync`), native/unsafe/reflection
path, or deterministic failure-injection seam. If an ordinary supported BCL
platform cannot retain the required full-version old-or-new target semantics,
implementation stops at Architecture for a platform decision.

### Delete

Delete accepts only a matched ordinary-file expectation. It repeats the target
revalidation immediately before `File.Delete` and then verifies `Missing`. A
directory, device, link, or any other non-file state is never deleted. Directory
policy remains command-owned and future; this primitive has no empty-directory
deletion exception.

### Receipts and cleanup

The existing finite `FileChangeReceipt` is retained. `NotStarted` carries the
exact accepted before snapshot, bounded cause, and exactly one neutral reason:
`Cancelled`, `TargetChanged`, `ApplicationFailed`, or `ContractRejected`.
Cancellation maps to `Cancelled`; state or identity drift maps to
`TargetChanged`; staging or ordinary application failure before a target effect
maps to `ApplicationFailed`; and invalid lease, preparation, or callable
contract maps to `ContractRejected`. After a successful synchronous Move/Delete,
cancellation cannot be
reinterpreted as `NotStarted`; the applier verifies resulting state and callers
decide operation interruption.

`Verified` carries the exact verified after snapshot. A successful effect whose
verification observes a mismatch uses `VerificationFailed` and retains that
observation. A successful effect whose verification is unavailable is
`Applied`/`Failed` with no after snapshot. A thrown Move or Delete is
post-observed: exact intended state is `Verified`; unchanged before state is
`NotStarted`/`ApplicationFailed`; any other changed or unavailable state is
`CompletionUnknown`, carrying the observed after snapshot when available.
`CompletionUnknown` is never used for a known pre-effect failure. No receipt
claims rollback, compensation, or recovery.

Cleanup may delete only the exact stage path successfully created by this
invocation. A failed `CreateNew` never cleans a pre-existing lookalike. Reparse,
directory, inaccessible, or otherwise changed stage paths are preserved rather
than followed or broadly removed.

## Behavior Matrix

| Input/effect boundary                                                     | Required result                                   | Target effect                            |
| ------------------------------------------------------------------------- | ------------------------------------------------- | ---------------------------------------- |
| Matched Create, no preparation, and exact stage readback                  | Verified with intended bytes                      | One atomic Move, `overwrite: false`      |
| Matched Replace and matching verified recovery preparation                | Verified with intended full bytes                 | One atomic Move, `overwrite: true`       |
| Matched ReplaceGeneratedRegion and matching verified recovery preparation | Verified with supplied full bytes                 | One atomic Move, `overwrite: true`       |
| Matched ordinary-file Delete and matching verified recovery preparation   | Verified with Missing after                       | One File.Delete                          |
| Replace/Delete without matching verified recovery preparation             | NotStarted/ContractRejected                       | None                                     |
| Cancellation before effect                                                | NotStarted/Cancelled with accepted before         | None                                     |
| Pre-effect state or identity drift                                        | NotStarted/TargetChanged with accepted before     | None                                     |
| Stage or ordinary pre-effect application failure                          | NotStarted/ApplicationFailed with accepted before | None                                     |
| Invalid lease, preparation, or callable contract                          | NotStarted/ContractRejected with accepted before  | None                                     |
| Move/Delete throws and intended state is observed                         | Verified with exact intended after                | Effect completed despite the thrown call |
| Move/Delete throws and before state remains                               | NotStarted/ApplicationFailed with accepted before | No observed target change                |
| Move/Delete throws and after is changed or unavailable                    | CompletionUnknown, observed after if available    | May have happened; no claim              |
| Successful effect with observed intended mismatch                         | VerificationFailed                                | Effect applied; exact mismatch recorded  |
| Successful effect with unavailable verification                           | Applied/Failed with no after snapshot             | Effect applied; verification unavailable |
| Directory/link/device target or unsafe physical identity                  | NotStarted/ContractRejected                       | None                                     |

## Placement And Boundaries

Production locality is:

- `src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Application/FileChangeApplier*.cs`;
- existing mutation models under `Framework/Mutation/Models/Filesystem/`;
- existing lock and validation capabilities under `Framework/Mutation/Locking/`
  and `Framework/Mutation/Validation/`.

Focused real-filesystem evidence is one coherent file at
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Mutation/Application/FileChangeApplierIntegrationTests.cs`.
Existing receipt invariants remain in
`Framework/Mutation/Models/Filesystem/FileMutationContractTests.cs` when a
factory state needs direct proof. No project, package, configuration,
TestSupport, command, lifecycle policy, recovery, Git, output, or public
contract changes are in scope.

## Evidence Matrix

| ID      | Required proof                                                                                                                                                                                    | Tier                  |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- |
| MFA-001 | Create writes exact bytes, returns Verified, and leaves no owned stage residual                                                                                                                   | Integration           |
| MFA-002 | Replace and ReplaceGeneratedRegion use the full-version path and preserve exactly supplied full-document bytes                                                                                    | Integration           |
| MFA-003 | Ordinary-file Delete verifies Missing and never deletes a directory                                                                                                                               | Integration           |
| MFA-004 | Live same-workspace lease and exact matched check are required; disposed, foreign, retargeted, and incoherent inputs are blocked                                                                  | Integration and Unit  |
| MFA-005 | Target replacement/removal after an earlier check and pre-effect cancellation produce no target effect and clean owned stages                                                                     | Integration           |
| MFA-006 | Any reachable ordinary BCL failure maps to a finite receipt without fallback; unrelated bytes and pre-existing lookalikes remain unchanged; receipt factories cover unavailable/ambiguous effects | Integration and Unit  |
| MFA-007 | Stale replay is rejected by immediate revalidation; idempotence remains caller planning                                                                                                           | Integration           |
| MFA-008 | Published `linux-x64` Native AOT Integration executes all four accepted change kinds                                                                                                              | Published Integration |

Use real owned temporary files, handles, links, cancellation, and deterministic
receipt factory cases. Do not add a fake filesystem, product test seam, hostile
ABA/inode/handle machinery, or guessed failure injection. Report portable
fixture limitations explicitly.

## Execution Capsule

- Exact baseline: `56a3fed3707681b0164f0093f509141afcd238d0` on
  `codex/mutation-foundation-contracts`; accepted contract base is `7abde56`.
- Pre-production evidence: warning-free Release build; managed Unit `1101/1101`,
  Integration `437/437`, and EndToEnd `120/120`, all with zero skips.
- Historical focused evidence: FileChangeApplier Integration `12/12` and
  receipt contract Unit `9/9`, all with zero failures or skips; that evidence
  predates the required recovery-preparation integration and did not close that
  later correction. The final current evidence below does.
- Historical final evidence: warning-free Release build; managed Unit
  `1101/1101`, Integration `449/449`, and EndToEnd `120/120`; published
  `linux-x64` Native AOT Integration `449/449`; format and `git diff --check`
  pass. Independent Task Mastermind reproduction passed focused Integration
  `12/12` and receipt Unit `9/9`; final independent review reproduced
  that historical gate and returned `APPROVED`.
- Direct consumers: Lifecycle write plans produce `PlannedFileChange`; future
  mutating commands call this one-change primitive while retaining ordering and
  policy. Child 04 supplies the matching verified
  `RecoveryBundlePreparation`; exact production candidate `e7d937f` completes
  that integration.
- Protected paths: commands, Shell/root, public output and JSON, projects,
  packages, configuration, TestSupport, lifecycle policy, recovery-bundle
  behavior, archived/sealed records, generated navigation, and frozen MVP.
- Full gate: warning-free Release build, format and `git diff --check`, focused
  application Integration and receipt Unit evidence, mutation-foundation Unit /
  Integration, full managed Unit / Integration / EndToEnd, and published
  `linux-x64` AOT Integration.

## Stop Conditions

Stop and report upward if the receipt contract cannot truthfully represent a
reachable state, if application needs command policy or an ordered executor, if
safe replacement requires native/unsafe/reflection or a weaker fallback, if
stage ownership cannot be kept exact, if directory policy enters this primitive,
if a new package/project/configuration or product seam is required, or if
ordinary supported BCL operations cannot prove the accepted full-version target
semantics on a required platform.

## Progress

The one-change applier and its original real-filesystem evidence are historically
recorded at `cdfde34`. Exact production candidate `e7d937f` completes the
matching-preparation integration. The focused suite covers exact
create/replacement/delete behavior,
lease and check coherence, cancellation, stale replay, create collision,
directory and delete replacement races, resolved identity retarget,
pre-existing stage lookalikes, forged contained Missing physical identity, and
deterministic receipt states for unavailable/ambiguous effects and blocks
missing, foreign, mismatched, corrupt, or draft preparation before an
existing-target effect. Create accepts only null preparation. On the current
root-owned Linux host, a deterministic AccessDenied/I/O effect-call
fixture is not honest without a forbidden failure seam, so it is not claimed;
the typed `FileChangeReceipt` paths remain truthful without a fake seam.

Final M1 focused Unit `43/43`, Integration `47/47`, full managed
`1096/458/120`, focused published `linux-x64` Recovery/source-generation Native
AOT `10/10`, and full Native AOT Integration `458/458` pass with zero skips.
Final independent M1 review is `ROBUST`, safe to commit, confidence `0.98`.
