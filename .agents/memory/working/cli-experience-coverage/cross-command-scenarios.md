---
open-forge:
  description: Individual scenario coverage with exact existing evidence and missing outcomes
  tags: [Memory, Working, CLI, Testing, Evidence, Contextual]
---

# Cross-command Scenarios

Return to the [review](./_cli-experience-coverage.md). **Status describes E2E evidence for the whole individual scenario**, not current runtime success. Lower-tier evidence is named separately. Deferred and omitted identities remain for accounting and are not accepted test requirements.

## X01

**[Paste a native Skill and index it unchanged](../../crystallized/documents/cli/experience/scenarios/experience.md#x01)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No E2E imports a native Skill then indexes, inspects and reads it while preserving support files and repeating unchanged. Unused fixture capability does not count.

## X02

**[Index a plain note without rewriting it](../../crystallized/documents/cli/experience/scenarios/experience.md#x02)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No published-process case accepts a plain note with warning-only metadata feedback, preserves its bytes, indexes it and repeats without churn.

**Separate lower-tier evidence:**

- Integration [RealMetadataFailuresKeepMissingAndUnsafeMeaningsDistinct:82](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexProjectionIntegrationTests.cs): Real plain child yields MetadataIncomplete; invalid UTF8 child yields MetadataUnsafe, preserved source hashes. Projection only; no useful index application or source retrieval. Do not treat MetadataIncomplete finding alone as proof of opposite end-to-end behavior.

## X03

**[Index partial metadata without changing authored values](../../crystallized/documents/cli/experience/scenarios/experience.md#x03)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No published-process case preserves partial metadata and body while continuing with only the relevant optional-field warnings.

## X04

**[Preview indexing a plain note without metadata edits](../../crystallized/documents/cli/experience/scenarios/experience.md#x04)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No plain-note preview checks useful proposed navigation, warning-only feedback and no implicit scaffolding.

## X05

**[Do not treat missing, partial, malformed and unreadable metadata as one problem](../../crystallized/documents/cli/experience/scenarios/experience.md#x05)** — selection: improved; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedContextFragmentFailureIsIncompleteAndReadOnly:85](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) | A missing fragment is incomplete and read-only. |
| [NavigationAndAuthoredDriftKeepTheirOwnDiagnosis:33](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs) | Install, add valid note, index, Doctor exits0; remove generated entry, Doctor exits2 with stale-region finding; reindex, append authored guidance edit, Doctor exits2 with managed-changed AND partial-lifecycle findings; diagnostic hashes unchanged. |

**Gap / qualification:** Missing-fragment context and authored/navigation drift are related diagnosis evidence, not the required absent/partial/malformed/read-denied metadata distinction with independent continuation.

**Separate lower-tier evidence:**

- Integration [UnclosedChildMetadataNamesTheActualLeafAndDoesNotMutate:14](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexApplicationIntegrationTests.cs): Unclosed child YAML blocks selected root, exact leaf/location diagnostic and no effects/workspace/recovery changes. No independent second stale branch; does not decide reviewed safe-independent-work continuation.
- Integration [RealMetadataFailuresKeepMissingAndUnsafeMeaningsDistinct:82](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexProjectionIntegrationTests.cs): Real plain child yields MetadataIncomplete; invalid UTF8 child yields MetadataUnsafe, preserved source hashes. Projection only; no useful index application or source retrieval. Do not treat MetadataIncomplete finding alone as proof of opposite end-to-end behavior.

## X06

**[Subtract startup context without hiding newly selected material](../../crystallized/documents/cli/experience/scenarios/experience.md#x06)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedContextJsonRetainsExpandedGraph:35](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) | Expanded additions-only context asserts ordered newly selected paths including overwrite placement, with metadata-only output. |

**Gap / qualification:** No full startup-to-additions journey with literal readable bodies, full closure and startup subtraction independently checked.

## X07

**[Preserve authored prose around the index-managed list](../../crystallized/documents/cli/experience/scenarios/experience.md#x07)** — selection: improved; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedApplyPreservesOutsideBytesAndConverges:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) | Index preserves the fixture’s authored prefix, applies navigation and repeats without changes. |

**Gap / qualification:** No adversarial later authored lists and both sides of the first generated Entries region across the reviewed maintenance journey.

## X08

**[Count actual files and directories after install](../../crystallized/documents/cli/experience/scenarios/experience.md#x08)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedApplyConvergesToVerifiedNoOp:49](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) | Install applies the expected fixture payload path set and repeats without writes. |
| [RelocatedArtifactReachesEmbeddedPayload:10](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedEmbeddedPayloadProcessTests.cs) | Relocated binary install preview source asset count equals 12 payload paths+2 hosts; loader effect present; hashes unchanged/no infrastructure. |

**Gap / qualification:** Does not independently reconcile printed file/directory counts with the complete actual inventory, separate host/control/payload, or count directories strictly below .agents.

## X09

**[Tell the truth about interruption before and after effects](../../crystallized/documents/cli/experience/scenarios/experience.md#x09)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedProcessCancellationKillsAndDrainsOwnedChild:121](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) | Owned process cancellation and draining are covered by the runner. |

**Gap / qualification:** No actual CLI Ctrl-C before versus after effects, partial receipt, saved settings or truthful unknown/recovery state. Integration write-failure tests are a different fault boundary.

**Separate lower-tier evidence:**

- Integration [PartialWriteFailure:148](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateBeforeOutputSnapshotTests.cs): Windows real sharing denial on loader: earlier Guidance bytes are restored, loader keeps edited bytes, first effect Verified, overall failed, recovery Retained. No ZIP payload or ownership before/after assertion; no follow-up journey. Snapshot incorrectly attributes denied subject to ownership file; this is not an ownership-publication fault.
- Integration [PartialWriteFailure:90](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallBeforeOutputSnapshotTests.cs): Windows real denial on memory entrypoint after earlier install effects: loader exists, blocked bytes unchanged, ownership absent, earlier Verified effect, blocked effect NotStarted, retained existing recovery bundle. No complete earlier-effect inventory, recovery payload validation or published-process continuation.
- Integration [Cancelled:124](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateBeforeOutputSnapshotTests.cs): Scripted confirmation rejection returns Interrupted with unchanged workspace hashes. Before-effect interaction rejection, not an actual terminal signal.

## X10

**[Keep a copied Template independent](../../crystallized/documents/cli/experience/scenarios/experience.md#x10)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges:47](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | Route Create checks exact new bytes, parent navigation and repeat no-op. |

**Gap / qualification:** No copy of a real Template, destination metadata edit, later source-template edit and demonstrated copy independence.

**Separate lower-tier evidence:**

- Integration [TemplateResolutionCopiesOnlyExactClassifiedBody:19](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/RouteCreatePlanningIntegrationTests.cs): Exact classified Template body is copied without frontmatter. 

## X11

**[Keep failures in another workspace out of this request](../../crystallized/documents/cli/experience/scenarios/experience.md#x11)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedVersionIsExactAndWorkspaceIndependent:12](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) | Explicit absent workspace does not affect version or create that path. |
| [EveryPublishedBindingSupportsBothFormatsAtEveryDetail:39](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedSchema3ProcessTests.cs) | 28 command variants against explicit missing workspace, four details/two formats: fixed exit/status, schema and key coordinates stable, correct streams, missing paths absent, workspace hashes preserved, no infrastructure. |

**Gap / qualification:** Terminal version bypass is not a normal read/write explicit-workspace A/B selection journey or refusal of ancestor fallback.

## X12

**[Ignore unrelated corrupt old records, but not required safety facts](../../crystallized/documents/cli/experience/scenarios/experience.md#x12)** — selection: improved; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LegacyFilesAreNeitherReadNorDeleted:9](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) | Install/update/status/Doctor/extension-list/library-list each exit0 while malformed legacy lifecycle/library files remain byte-identical; new ownership file exists. |

**Gap / qualification:** Install preserves malformed inert legacy files, but no E2E proves useful continuation past malformed routed Markdown unrelated to a valid package action, with required-input boundaries kept distinct.

## X13

**[Treat an unusable ownership lock as unknown claims, not a universal stop](../../crystallized/documents/cli/experience/scenarios/experience.md#x13)** — selection: improved; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Library List preserves workspace and reports an absent ownership record without inventory. |
| [JsonJourneyRetainsSixDomainsAndReadOnlyResult:28](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs) | Doctor JSON exit2/completed-with-warnings, six ordered categories, Workspace complete, informational extension.ownership-observation; source hashes unchanged and no infrastructure. |
| [PublishedStatusHumanJourneyUsesSemanticStreamExitAndPreservesWorkspaceAndRecoveryBytes:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) | Minimal and standard status return 3 with installed text, incomplete recovery-draft path and unfinished-command finding; detail-specific cost headings; workspace and recovery bytes unchanged. |

**Gap / qualification:** No same-command matrix separating denied, malformed, unusable, empty and foreign ownership, with safe continuation and no invented claims.

**Separate lower-tier evidence:**

- IntegrationSafety [UnwritableOwnershipDoesNotInvalidateVerifiedContentEffects:12](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateOwnershipSafetyIntegrationTests.cs): Directory at ownership-file path remains; Update completes with Verified content, OwnershipObservation finding and restored loader. No assertion of failed publication attempt, exact restored bytes, prior-claim preservation or later command behavior. Adjacent unusable-target evidence, not full publication-failure scenario.

## X14

**[Distinguish one-time consent, persistent consent and ownership](../../crystallized/documents/cli/experience/scenarios/experience.md#x14)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Library grant flags persist scopes and preserve unrelated settings; preview does not save them. |

**Gap / qualification:** No real terminal once/always choice, later revoked-permission rejection, exact-file versus sibling scope, or force-versus-ownership distinction in one journey.

**Separate lower-tier evidence:**

- Integration [ExplicitAlwaysIsRememberedAndRepeatedInstallDoesNotPrompt:13](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Always grant persists and repeat does not prompt. 
- Integration [AllowOnceAppliesWithoutWritingAbsentOrMalformedSettings:53](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Once applies without writing absent/malformed settings. 
- Integration [ExplicitGrantPersistsAcrossCommandsAndDryRunCannotWrite:150](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Explicit grant persists across commands; dry-run cannot write. 
- Integration [RevocationBlocksSelectedLifecycleWithoutReadingPermissionAsOwnership:259](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Revocation blocks lifecycle without treating permission as ownership. 

## X15

**[Keep a copyable next action complete and relevant](../../crystallized/documents/cli/experience/scenarios/experience.md#x15)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [RequiredIdOmissionIsInvalidWithoutObservation:32](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Missing Library Inspect operand explains the failure and suggests Library List without writes. |
| [PublishedComparisonUsesCurrentAndIntendedContent:40](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) | After inspecting an explicit custom --source and --workspace, the changed-content row asserts exactly open-forge extension update toolkit --dry-run as the suggested command, omitting the selected source and workspace. |
| [InvalidMetadataUsesSharedStatusStreamAndExitWithoutWrites:109](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | Missing-description exit/stream/Next wording and write-free helper boundary. |
| [PublishedUnattendedSelectionIsRequired:23](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) | Redirected/no-ID request exits invalid with selection-required finding and no stdout. |

**Gap / qualification:** Some missing-input guidance is useful, but Extension Inspect explicitly requires a suggested update that omits its custom source and workspace; it is not a complete identity-preserving replay command. Missing optional metadata also triggers a required correction in Route Create.

## X16

**[Judge partial repair within the actual repair scope](../../crystallized/documents/cli/experience/scenarios/experience.md#x16)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AutomaticJsonDryRunPreviewsSafeExactWithoutEffects:37](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) | Safe ./guide.md rewrite plus separate ambiguous missing.md: JSON/text preview says one repair/one choice, planned effect, four candidates, exact counts, warning code; source/lock/recovery state unchanged. |
| [ExplicitRelinkPreservesContentAndConvergesToNoOp:104](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) | Measured occurrence relink changes missing.md to replacement.md; exact full source equality, visible label and unrelated binary preserved; reports done, no remaining, recovery removed and actual no recovery artifacts; automatic repeat empty/no-op with full snapshot equality. |

**Gap / qualification:** Repair preview distinguishes safe and guided work and an explicit repair repeats as no-op. No actual selected safe application alongside an independently unresolved issue plus unsafe-required-boundary contrast.

## X17

**[Detach known links when their source folder is unavailable](../../crystallized/documents/cli/experience/scenarios/experience.md#x17)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyRemovesExactAndDanglingLinksBeforeLastRecord:27](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Detach removes a current and dangling member while preserving source bytes. |

**Gap / qualification:** Source root remains available. No source-root disappearance followed by list, inspect, sync without guessed retirement, then detach of known links.

## X18

**[Preserve a changed Library destination while continuing safe work](../../crystallized/documents/cli/experience/scenarios/experience.md#x18)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ChangedOccupantBlocksWholeSync:63](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Existing tests require whole-operation blocked/5, no independent new link, and preservation of other links plus registration. |
| [ChangedOccupantPreservesEveryProjectionAndRecord:47](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Existing tests require whole-operation blocked/5, no independent new link, and preservation of other links plus registration. |

**Gap / qualification:** Reviewed target requires preserving the changed user file while applying independent safe sync/detach work and releasing appropriate claims.

## X19

**[Make a second identical operation genuinely a no-op](../../crystallized/documents/cli/experience/scenarios/experience.md#x19)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedApplyConvergesToVerifiedNoOp:49](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) | Several actual apply-to-repeat sequences establish verified no-op outcomes and preservation. |
| [ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges:47](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | Several actual apply-to-repeat sequences establish verified no-op outcomes and preservation. |
| [PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp:34](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) | Several actual apply-to-repeat sequences establish verified no-op outcomes and preservation. |
| [DefaultRemovalPreservesUnownedContentAndRepeatsAsNoOp:54](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) | Several actual apply-to-repeat sequences establish verified no-op outcomes and preservation. |
| [CategoryApplicationProjectionAndRepeatAreStable:103](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) | After actual category removal, repeated JSON and text removal explicitly return invalid/4 with source-not-found and no effects. |
| [RedirectedConfirmationAutomaticDryRunApplyRepeatJourney:74](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedUpdateProcessTests.cs) | After one real loader edit, force without automatic exits4/no writes; force preview planned effects/no writes; force apply reports verified nonempty effects; repeat has empty effects and unchanged workspace hashes. |
| [PublishedApplyPreservesOutsideBytesAndConverges:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) | One explicit entrypoint apply exactly equals expected generated document including authored prefix; repeat says current and hashes unchanged; persistent lock checked. |
| [ExplicitRelinkPreservesContentAndConvergesToNoOp:104](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) | Measured occurrence relink changes missing.md to replacement.md; exact full source equality, visible label and unrelated binary preserved; reports done, no remaining, recovery removed and actual no recovery artifacts; automatic repeat empty/no-op with full snapshot equality. |
| [ApplicationRemovesEligibleItemsPreservesBlockedAndUnknownThenRepeatsAsNoOp:39](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupProcessTests.cs) | Deletes one verified bundle and one incomplete draft, names them; preserves unknown same-bucket file/nested sentinel and malformed FOREIGN-bucket archive hashes; repeat while lease held reports no recovery to remove and full snapshot unchanged. |
| [GenericDryRunApplyAndRepeatNoOpFormOneRealJourney:9](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInitProcessTests.cs) | Single docs scope JSON preview/no writes; apply verified, final file exists with supplied description and heading; repeat empty effects/unchanged entrypoints and source hashes unchanged. |

**Gap / qualification:** Useful no-op sequences exist for other commands, but Route Remove explicitly rejects the already-absent category after successful removal, contrary to the reviewed harmless repeated-removal target. Library repeated-absence detach remains untested.

## X20

**[Keep detail and JSON views about the same outcome](../../crystallized/documents/cli/experience/scenarios/experience.md#x20)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [EveryPublishedBindingSupportsBothFormatsAtEveryDetail:39](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedSchema3ProcessTests.cs) | All bindings have unavailable-workspace view checks; healthy Route List full/debug JSON facts match except detail metadata. |
| [DebugAddsOnlyDiagnosticsToFullDetail:368](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) | All bindings have unavailable-workspace view checks; healthy Route List full/debug JSON facts match except detail metadata. |
| [PublishedStatusHumanJourneyUsesSemanticStreamExitAndPreservesWorkspaceAndRecoveryBytes:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) | Minimal and standard status return 3 with installed text, incomplete recovery-draft path and unfinished-command finding; detail-specific cost headings; workspace and recovery bytes unchanged. |

**Gap / qualification:** No complete success/incomplete/default/filter matrix on the reviewed states, all mutation outcomes, useful human communication or unknown-versus-zero coverage.

## X21

**[Allow one live writer per workspace without mistaking a lock file for a writer](../../crystallized/documents/cli/experience/scenarios/experience.md#x21)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplicationRemovesEligibleItemsPreservesBlockedAndUnknownThenRepeatsAsNoOp:39](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupProcessTests.cs) | Deletes one verified bundle and one incomplete draft, names them; preserves unknown same-bucket file/nested sentinel and malformed FOREIGN-bucket archive hashes; repeat while lease held reports no recovery to remove and full snapshot unchanged. |

**Gap / qualification:** No two live CLI writers, release/retry and independent-workspace test. Cleanup holds a lease only for a no-op. Lower-tier lock-manager and lease tests prove genuine locking mechanics separately.

## X22

**[Do not confuse external links with unsupported or broken local links](../../crystallized/documents/cli/experience/scenarios/experience.md#x22)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedContextJsonRetainsExpandedGraph:35](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) | HTTPS link is external-unchecked and not followed; a missing fragment yields incomplete read-only context. |
| [PublishedContextFragmentFailureIsIncompleteAndReadOnly:85](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) | HTTPS link is external-unchecked and not followed; a missing fragment yields incomplete read-only context. |
| [PublishedReferencesOutgoingPreservesDirectFacts:33](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedReferencesProcessTests.cs) | Out-only JSON exact eight destination occurrences and base/overwrite layers; external unchecked/no resolvedPath, missing and fragment-missing distinguished; hashes unchanged. |

**Gap / qualification:** No complete same-case matrix of unsupported local targets, missing files, missing fragments and proof of zero network access.

## X23

**[Keep literal content literal, including whitespace and Unicode](../../crystallized/documents/cli/experience/scenarios/experience.md#x23)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedFindJsonProjectsMatchedContent:52](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindProcessTests.cs) | Find JSON checks exact simple matched content. |
| [ExplicitRelinkPreservesContentAndConvergesToNoOp:104](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) | Measured occurrence relink changes missing.md to replacement.md; exact full source equality, visible label and unrelated binary preserved; reports done, no remaining, recovery removed and actual no recovery artifacts; automatic repeat empty/no-op with full snapshot equality. |
| [PublishedApplyPreservesOutsideBytesAndConverges:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) | One explicit entrypoint apply exactly equals expected generated document including authored prefix; repeat says current and hashes unchanged; persistent lock checked. |

**Gap / qualification:** No adversarial literal Unicode/whitespace/body-vocabulary fixture carried through reading and bounded maintenance. Preservation hashes alone do not prove content output is literal.

## X24

**[Discover commands and correct mistakes without hidden work](../../crystallized/documents/cli/experience/scenarios/experience.md#x24)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedRootAndRouteFamilyHelpExposeAvailableCommands:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) | Help/version and parser stream/exit boundaries have strong focused evidence. |
| [PublishedParserFailureUsesFixedInvalidExitAndStandardError:103](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) | Help/version and parser stream/exit boundaries have strong focused evidence. |
| [PublishedVersionIsExactAndWorkspaceIndependent:12](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) | Help/version and parser stream/exit boundaries have strong focused evidence. |
| [PublishedRootAndStatusHelpExposeTheDirectLeafInImplementedOrderWithoutWorkspaceInspection:13](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) | Root/status help succeeds against a nonexistent selected workspace; expected leaf order, no created path, unchanged workspace/recovery snapshot. |
| [HelpIsReachableWithoutWorkspaceInspection:10](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs) | Doctor help succeeds without creating missing workspace or infrastructure; source hashes unchanged. |
| [HelpSucceedsWithoutWorkspaceInspectionOrWrites:13](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) | Help succeeds with missing workspace and no write infrastructure/state changes. |
| [HelpIsTerminalAndPerformsNoWorkspaceRecoveryLeaseOrWriteWork:11](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupProcessTests.cs) | Help succeeds against missing workspace with exact usage fragments; full fixture snapshot equal and no lock. |
| [HelpInvalidSingletonAndBooleanRepetitionJourney:9](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedUpdateProcessTests.cs) | Help lists modes; repeated --workspace exits4 without writes; repeated Boolean automatic/dryrun accepted and JSON flags/mode exact with no next. |

**Gap / qualification:** No complete aggregate-error correction then successful execution, with source/workspace/quoted operands retained across all command families.

## X25

**[Add warning detail without repeating the same instruction](../../crystallized/documents/cli/experience/scenarios/experience.md#x25)** — selection: improved; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [EveryPublishedBindingSupportsBothFormatsAtEveryDetail:39](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedSchema3ProcessTests.cs) | Structural detail-mode consistency and diagnostic stream separation are asserted. |
| [DebugAddsOnlyDiagnosticsToFullDetail:368](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) | Structural detail-mode consistency and diagnostic stream separation are asserted. |
| [PublishedStatusHumanJourneyUsesSemanticStreamExitAndPreservesWorkspaceAndRecoveryBytes:38](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) | Minimal and standard status return 3 with installed text, incomplete recovery-draft path and unfinished-command finding; detail-specific cost headings; workspace and recovery bytes unchanged. |

**Gap / qualification:** These assertions do not review usefulness or detect repeated explanations in complete human transcripts. No reviewed multi-view golden transcript for these flows.

## X26

**[Test grant persistence independently from later content failure](../../crystallized/documents/cli/experience/scenarios/experience.md#x26)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Successful explicit settings persistence and unsaved preview are checked. |

**Gap / qualification:** No retained grant followed by content failure, versus a failed settings write, with separate truthful effects.

**Separate lower-tier evidence:**

- Integration [ExplicitGrantPersistsAcrossCommandsAndDryRunCannotWrite:150](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Explicit grant persists across commands; dry-run cannot write. 
- Integration [TargetChangedAfterVerifiedEffectRetainsProgressAndRecovery:33](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallMutationIntegrationTests.cs): Install later-target change retains verified progress/recovery. 

## X27

**[Keep unrelated owners and support files through a full package lifecycle](../../crystallized/documents/cli/experience/scenarios/experience.md#x27)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp:34](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) | Extension update preserves source and repeats; removal preserves unowned content and repeats. |
| [DefaultRemovalPreservesUnownedContentAndRepeatsAsNoOp:54](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) | Extension update preserves source and repeats; removal preserves unowned content and repeats. |
| [PublishedApplyAndNoOpUseExactStreamsExitsAndJson:47](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) | Explicit toolkit install verifies JSON schema/selection/package/verification, exact bytes, source preservation, repeat empty effects and recovery not-required. |

**Gap / qualification:** No complete dependency lifecycle with shared owners, support files, unowned content, source preservation and native Skill optional metadata in one carried-forward state.

**Separate lower-tier evidence:**

- Integration [SharedOwnerReleasePrecedesFinalOwnerDeletion:74](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemoveApplicationIntegrationTests.cs): Shared owner release precedes final-owner deletion. 

## X28

**[Tell a zero result from an unknown or not-requested result](../../crystallized/documents/cli/experience/scenarios/experience.md#x28)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent Library ownership omits inventory; shared unavailable-workspace envelopes retain stable facts across details. |
| [EveryPublishedBindingSupportsBothFormatsAtEveryDetail:39](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedSchema3ProcessTests.cs) | Absent Library ownership omits inventory; shared unavailable-workspace envelopes retain stable facts across details. |
| [ExplicitRelinkPreservesContentAndConvergesToNoOp:104](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) | Measured occurrence relink changes missing.md to replacement.md; exact full source equality, visible label and unrelated binary preserved; reports done, no remaining, recovery removed and actual no recovery artifacts; automatic repeat empty/no-op with full snapshot equality. |
| [PublishedReferencesOutgoingPreservesDirectFacts:33](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedReferencesProcessTests.cs) | Out-only JSON exact eight destination occurrences and base/overwrite layers; external unchecked/no resolvedPath, missing and fragment-missing distinguished; hashes unchanged. |
| [PublishedContextFragmentFailureIsIncompleteAndReadOnly:85](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) | Missing followed fragment produces exit3/incomplete and context.fragment-missing with exact from/destination/resolved path/unfollowed; source hashes unchanged. |

**Gap / qualification:** No complete zero-versus-unknown-versus-not-requested count matrix or deduplicated human limitations for the reviewed states.

## X29

**[Report verified content separately from failed ownership publication](../../crystallized/documents/cli/experience/scenarios/experience.md#x29)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No published-process fault after verified content effects but before ownership publication, followed by later commands that avoid inventing ownership.

**Separate lower-tier evidence:**

- IntegrationSafety [UnwritableOwnershipDoesNotInvalidateVerifiedContentEffects:12](../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateOwnershipSafetyIntegrationTests.cs): Directory at ownership-file path remains; Update completes with Verified content, OwnershipObservation finding and restored loader. No assertion of failed publication attempt, exact restored bytes, prior-claim preservation or later command behavior. Adjacent unusable-target evidence, not full publication-failure scenario.

## X30

**[Do not claim a local edit when only the intended source changed](../../crystallized/documents/cli/experience/scenarios/experience.md#x30)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp:34](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) | Source payload is changed and applied, and comparison checks current/intended content. |
| [PublishedComparisonUsesCurrentAndIntendedContent:40](../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) | Source payload is changed and applied, and comparison checks current/intended content. |

**Gap / qualification:** No same-scenario contrast with separately user-edited local bytes, source/version-label-only change, truthful distinction and preserved edits.

