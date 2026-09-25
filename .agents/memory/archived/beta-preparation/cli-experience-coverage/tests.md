---
open-forge:
  description: All 112 published-process methods with what they assert and what they leave open
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# Existing E2E Method Inventory

Return to the [review](./_cli-experience-coverage.md). All 112 methods were read with their relevant fixture setup. The 100 Facts and 63 rows across 12 Theories declare 163 executions. No tests were run for this audit. Lines refer to the audited worktree. Repeated methods or theory rows are not distinct user flows.

## CliProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedVersionIsExactAndWorkspaceIndependent:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) — 1 Fact | Exact version stdout and empty stderr from another cwd, including an explicit absent workspace; both workspaces remain unchanged and absent path is not created. | Does not prove normal commands resolve explicit workspaces or execute a multi-command flow. |
| [PublishedRootAndRouteFamilyHelpExposeAvailableCommands:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) — 1 Fact | Root, Route family and selected leaf help expose asserted commands and options without workspace effects. | Not all command help, executable corrections, or terminal interaction. |
| [PublishedParserFailureUsesFixedInvalidExitAndStandardError:103](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) — 1 Fact | Unknown flag and conflicting terminal modes produce invalid/4 on stderr with empty stdout. | No aggregate correction journey or subsequent successful corrected invocation. |
| [PublishedProcessCancellationKillsAndDrainsOwnedChild:121](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) — 1 Fact | The test process runner cancels, requests child-tree termination and preserves workspace state. | Not CLI signal handling, after-effect cancellation, truthful partial receipts, or recovery. |

## PublishedCleanupProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [HelpIsTerminalAndPerformsNoWorkspaceRecoveryLeaseOrWriteWork:11](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupProcessTests.cs) — 1 Fact | Help succeeds against missing workspace with exact usage fragments; full fixture snapshot equal and no lock. | Help only. |
| [ApplicationRemovesEligibleItemsPreservesBlockedAndUnknownThenRepeatsAsNoOp:39](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupProcessTests.cs) — 1 Fact | Deletes one verified bundle and one incomplete draft, names them; preserves unknown same-bucket file/nested sentinel and malformed FOREIGN-bucket archive hashes; repeat while lease held reports no recovery to remove and full snapshot unchanged. | Not two bundles; initial apply does not assert workspace source snapshot equality. Damaged archive is foreign, so no same-bucket damaged+valid continuation evidence. |
| [DryRunJsonIsDeterministicTypedContingentAndLeaseFree:94](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupProcessTests.cs) — 1 Fact | Two identical dryruns same JSON, exact selected bundle+draft order and would-be-removed outcomes; full recovery/workspace snapshot equality; no lock infrastructure. | No human future-tense footer, two-bundle case, same-bucket damaged candidate or eligible-work lock contention. |

## PublishedContextProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedContextDefaultStartupClosureIsOrderedAndReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) — 1 Fact | Metadata projection four-source startup delimiters in order, KeepInMind tags, no projects branch, no source hash changes. | Does not execute default authored-content projection or compare output content bytes. |
| [PublishedContextJsonRetainsExpandedGraph:35](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) — 1 Fact | Selected guide additions-only metadata with follow-links=all yields exact ancestor/base/overwrite/linked/README sequence; local followed/external unchecked facts; no next, no source hash changes. | No default one-source closure, body bytes, paths/section/headings projections or one-hop cutoff. |
| [PublishedContextFragmentFailureIsIncompleteAndReadOnly:85](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) — 1 Fact | Missing followed fragment produces exit3/incomplete and context.fragment-missing with exact from/destination/resolved path/unfollowed; source hashes unchanged. | Fragment absence differs from missing/unreadable file; does not assert remaining readable content is actually returned. |

## PublishedDoctorGeneratedNavigationProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [FreshExtensionNavigationIsNotFrameworkDrift:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs) — 4 rows | Four inline cases: install Framework plus development/development-toolkit, then status/Doctor JSON exits0/completed; Doctor Framework and route finding sets empty; diagnosis source hashes unchanged. | Does not validate startup measurements or distinct installed package/dependency counts; install stage checked by exit only. |
| [NavigationAndAuthoredDriftKeepTheirOwnDiagnosis:33](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs) — 1 Fact | Install, add valid note, index, Doctor exits0; remove generated entry, Doctor exits2 with stale-region finding; reindex, append authored guidance edit, Doctor exits2 with managed-changed AND partial-lifecycle findings; diagnostic hashes unchanged. | Explicit partial-lifecycle finding on intentional edit may disagree with reviewed no-imaginary-failed-update target. Does not prove default useful action or absence of competing diagnoses for stale-only phase. |
| [DuplicateGeneratedHeadingsRemainDiagnosed:67](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs) — 1 Fact | Duplicate Entries headings produce nonzero Doctor with generated-region-duplicate; diagnostics preserve workspace hashes. | Specific malformed generated region, not malformed YAML plus independent broken links. |

## PublishedDoctorInstallationProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [FreshFrameworkHasCompleteCoverage:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorInstallationProcessTests.cs) — 1 Fact | Install then Doctor JSON exit0/completed; all six categories complete with no limitations; source hashes unchanged. | No default-text headline/footer checks; installation relies on exit success. |
| [InstalledEmbeddedPackagesHaveCompleteExtensionCoverage:31](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorInstallationProcessTests.cs) — 2 rows | Two embedded packages installed; Doctor Extensions category complete with no limitations and diagnosis hashes unchanged. | Does not assert Doctor exit in this method or changed owned package/library drift. |

## PublishedDoctorProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [HelpIsReachableWithoutWorkspaceInspection:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs) — 1 Fact | Doctor help succeeds without creating missing workspace or infrastructure; source hashes unchanged. | Help only. |
| [JsonJourneyRetainsSixDomainsAndReadOnlyResult:28](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs) — 1 Fact | Doctor JSON exit2/completed-with-warnings, six ordered categories, Workspace complete, informational extension.ownership-observation; source hashes unchanged and no infrastructure. | Hand-built loader/route without ownership. Does not assert all category coverage or ownership unknown values, and is not an info-only success fixture. |
| [InvalidGrammarAndBlockedWorkspaceUseErrorStream:46](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs) — 1 Fact | Unexpected positional token returns4; selected regular file returns5; stdout empty and stderr nonempty; source hashes unchanged/no infrastructure. | Unsupported flag, explicit nonexistent healthy-parent fallback, and exact diagnostics not asserted. |

## PublishedEmbeddedPayloadProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [RelocatedArtifactReachesEmbeddedPayload:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedEmbeddedPayloadProcessTests.cs) — 1 Fact | Relocated published artifact performs an install preview; embedded asset count and planned loader are visible without lock or workspace writes. | No application after relocation; expected count derives from source payload fixture, not independent full created inventory. |
| [RelocatedArtifactReachesEmbeddedExtensions:31](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedEmbeddedPayloadProcessTests.cs) — 1 Fact | Relocated published artifact lists embedded package IDs matching the source catalogue without mutation. | Package discovery is not selected installation, dependency isolation, removal, or F26. |

## PublishedExtensionCreateProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedHelpIsTruthfulAndReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) — 1 Fact | Create help is reachable and write-free. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedAutomaticApplyConvergesWithoutWorkspaceLifecycleOrRecoveryEffects:25](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) — 1 Fact | Scaffold manifest/content creation, metadata defaults, repeat no-op, catalogue/workspace/lifecycle/recovery preservation. | Default manifest and content directory are asserted; exact output location and distinction of file versus directory effects are not independently checked. |
| [PublishedJsonPreservesExactResultOrderAndStdoutIsolation:87](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) — 1 Fact | Prompt-free JSON dry-run schema/order/paths and manifest name/description/version/dependency order with no writes. | Only preview inspects supplied dependency order and manifest property names; it does not assert supplied descriptive values or an applied manifest preserving them. Preview asserts exact proposed paths and unchanged file hashes; explicit absence of directories/lock infrastructure is not checked in this method. |

## PublishedExtensionInspectProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedHelpIsTerminalAndReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) — 1 Fact | Help bypasses missing workspace, exposes grammar and preserves explicit source. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedComparisonUsesCurrentAndIntendedContent:40](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) — 2 rows | Two theory rows assert changed/unchanged relation, selected package source, no writes and update next action. | Theory unchanged row proves current/intended equality and JSON source identity; dependency/version/file detail completeness is not. Changed relation is covered, but the EndToEnd fixture has no retired path/receipt membership. |

## PublishedExtensionInstallProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedHelpIsTruthfulAndReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) — 1 Fact | Install help is reachable and write-free. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedUnattendedSelectionIsRequired:23](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) — 1 Fact | Redirected/no-ID request exits invalid with selection-required finding and no stdout. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedApplyAndNoOpUseExactStreamsExitsAndJson:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) — 1 Fact | Explicit toolkit install verifies JSON schema/selection/package/verification, exact bytes, source preservation, repeat empty effects and recovery not-required. | Exact installed payload and source bytes are checked. Unrelated workspace preservation and ownership are not independently enumerated; reported verification is not its own oracle. Repeat has empty effects and identical workspace/source snapshots, but the reviewed already-installed/no-op explanation is not asserted. |

## PublishedExtensionListProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedHelpExposesCompleteContract:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) — 1 Fact | Group/list grammar, installed/available/source options and read-only result boundary. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedDefaultReportsBothSections:37](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) — 1 Fact | Trusted installed development-toolkit and embedded available IDs in JSON; write-free helper. | One trusted installed and available package IDs are asserted; matching marker and descriptions are not. |
| [PublishedExplicitPackageIsExactAndReadOnly:62](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) — 1 Fact | Explicit local package source kind/path/ID and source-byte preservation with no writes. | Explicit available-only source with empty ownership is checked, but the default two-section Installed none presentation is not. Available-only package ID/source bytes are checked; useful description rendering is not. |

## PublishedExtensionRemoveProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [RemovedPruneIsAnUnknownOption:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) — 1 Fact | Retired prune option is rejected before writes; target remains. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedHelpIsReachableWithoutWorkspaceInspectionOrWrites:27](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) — 1 Fact | Remove help grammar/sections and no lock infrastructure. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [DefaultRemovalPreservesUnownedContentAndRepeatsAsNoOp:54](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) — 1 Fact | Toolkit target removed, unowned neighbor/source preserved, repeat reports no files and snapshots remain equal. | Target absence, source/unowned-note preservation and removal explanation are checked; selected ownership claim removal is not independently read. Repeat proves no-op after removal but does not place similarly named unowned content at the former managed destination or independently assert known-absent claims. |
| [JsonPreviewAndApplySharePlanAndDeleteChangedFinalOwner:102](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) — 1 Fact | Changed final-owner dry-run/apply plan parity, verified delete, retained recovery, source/unowned preservation and next cleanup. | JSON preview/apply parity and changed-owner deletion are checked; complete future-tense human explanation and full settings/recovery inventory preservation are not. |

## PublishedExtensionUpdateProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedHelpIsReachableWithoutWorkspaceInspectionOrWrites:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) — 1 Fact | Update help grammar is reachable without workspace effects. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) — 1 Fact | Source-only edit replaces target, publishes/rechecks recovery, source stays unchanged by CLI and repeat is no-op. | Source-edited target bytes and retained recovery file are checked; exact replacement communication and recovery payload are not verified. |
| [PublishedUpdateBlocksWorkspaceOverlappingSourceWithoutEffects:104](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) — 1 Fact | Workspace-overlapping source is blocked before effects; workspace/source/lock/recovery stay unchanged. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |

## PublishedFindProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedFindBareUsesCurrentDirectoryAndDefaultMinimalDetail:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindProcessTests.cs) — 1 Fact | Bare find exits0, three nonempty lines, docs/guide named and no workspace/selection decorations; filesystem tree snapshot equal. | No exact full ordered match inventory, routed/unrouted decoys or body-word false-positive check. |
| [PublishedFindCompactFilteringReturnsMatch:33](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindProcessTests.cs) — 3 rows | Three delimiter variants tag Architecture+heading Architecture exit0 and contain docs row; workspace echoed, tree unchanged. | Does not assert only docs, tag-only two-match set, AND across two tags, fenced-heading decoy, include/exclude. |
| [PublishedFindJsonProjectsMatchedContent:52](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindProcessTests.cs) — 1 Fact | Combined tag+heading query JSON has exactly docs and exact base body part text; tree unchanged. | One tag plus heading, not two-tag AND/OR; no headings projection or returned-ID followup. |

## PublishedIndexProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedRepeatedDryRunIsExactAndReadOnly:11](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) — 1 Fact | One explicit stale entrypoint preview names one list and old/new line fragments; stdout no-files-changed; workspace hashes unchanged/no lock infrastructure. | No full default-root scan, numeric before/after count assertion, independent stale sibling, external recovery or directory snapshot. |
| [PublishedApplyPreservesOutsideBytesAndConverges:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) — 1 Fact | One explicit entrypoint apply exactly equals expected generated document including authored prefix; repeat says current and hashes unchanged; persistent lock checked. | No full initial workspace preservation assertion, trailing authored list, mixed line endings, or unrelated stale sibling. |
| [PublishedInvalidJsonUsesTypedStdoutAndSharedExit:76](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) — 1 Fact | Traversal source .agents/../private.md returns4, typed index.invalid-source on stdout, no private-name leak, hashes unchanged/no infrastructure. | Not unknown existing scope, folder resolution, malformed-child continuation or contention. |

## PublishedInstallProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [LegacyFilesAreNeitherReadNorDeleted:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) — 1 Fact | Install/update/status/Doctor/extension-list/library-list each exit0 while malformed legacy lifecycle/library files remain byte-identical; new ownership file exists. | These are intentionally unrelated legacy files, not malformed active Markdown or ownership lock; no full workspace comparison after mutations. |
| [PublishedJsonDryRunIsReadOnly:29](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) — 1 Fact | Fresh install JSON mode dry-run/completed and nonempty effects; workspace hashes unchanged; external lock infrastructure absent. | No exact plan target/count/host-section set, human future tense, directory/settings/recovery snapshot. |
| [PublishedApplyConvergesToVerifiedNoOp:49](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) — 1 Fact | Automatic apply exits0, reports verification, installed Framework path set matches 12 expected payload paths; repeat says already current and source hashes unchanged. | First apply does not compare each installed byte to source, host sections, unrelated file bytes or human counts. Snapshot omits external recovery and directory entries. |
| [PublishedRedirectedHumanWriteIsInvalidAndSilent:73](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedInstallProcessTests.cs) — 1 Fact | Bare install in redirected process exits4, stdout empty, actionable automatic command, no unanswered prompt; workspace unchanged and no lock infrastructure. | No interactive reject branch; no directory/permissions snapshot. |

## PublishedLibraryAttachProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [DryRunReportsCompletePlanWithoutEffects:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) — 1 Fact | Default-destination preview asserts source/destination, dry-run, recorded=false, permission not-required, no directory/infrastructure or snapshot changes. | No proposed link/directory assertions or external grant. |
| [ApplyCreatesExactRelativeProjectionAndOwnershipReceipt:26](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) — 1 Fact | Public attach to pre-granted docs creates exact relative file link, ordinary parent, exact ownership member; source and authored route prefix preserved, unrelated README unlisted, persistent empty lock, no recovery. | Pre-seeded grant outside .agents; no new grant persistence or populated routed navigation. |
| [OccupiedDestinationBlocksWholeAttach:67](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) — 1 Fact | Occupied ordinary file yields blocked/5, unchanged full snapshot, no registration/infrastructure and preserved source. | No precise collision finding, unowned exact-looking link or multi-member no-partial-apply assertion. |

## PublishedLibraryDetachProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [DryRunReportsExactDeletionWithoutEffects:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) — 1 Fact | Preview asserts selected ID, registrationRemoved=false, one exact-target deletion; unchanged workspace, no infrastructure. | Seeded attachment, no public provenance or human explanation. |
| [ApplyRemovesExactAndDanglingLinksBeforeLastRecord:27](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) — 1 Fact | Current and dangling targets become null; libraries array empty, registrationRemoved=true; remaining source preserved, no recovery. | Source root exists; no sibling/navigation sentinel; null target weaker than explicit absence. |
| [ChangedOccupantPreservesEveryProjectionAndRecord:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) — 1 Fact | Changed ordinary occupant plus other link gives blocked/5 and unchanged full snapshot/ownership, preserving other link/source. | Opposite continuation target C28-06. |

## PublishedLibraryInspectProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [HealthyProjectionHasCompleteInventoryAndDestinationIdentity:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) — 1 Fact | Seeded docs mapping yields one current row, current=true, source/destination paths, equal expected/observed targets and preserved source. | No independent expected-target literal, full identity/count assertions or human explanation. |
| [RequiredIdOmissionIsInvalidWithoutObservation:32](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) — 1 Fact | Omitted ID gives invalid/4, empty stdout, Cannot inspect and Library List guidance; full detail includes invalid-ID code; no changes. | Omitted ID differs from malformed supplied ID or valid absent ID. |
| [CompleteInventoryExplainsAdditionRetirementAndMissingProjection:51](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) — 1 Fact | Mixed seeded state yields exactly ordered added, retired, missing rows and destination paths, warnings/2 and unchanged snapshot. | No public attach/source-edit sequence, restoration advice or follow-up sync. |

## PublishedLibraryListProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) — 1 Fact | Absent ownership gives completed/0, empty libraries and no inventory; snapshot unchanged, no infrastructure. | No unavailable counts or distinction from readable known-empty ownership. |
| [HealthyRecordsAreDeterministic:21](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) — 1 Fact | Two seeded registrations give identical repeated JSON/streams/exit; ordered IDs and links, expected mapping and current states; unchanged workspace. | No single concise human row or next action. |
| [MissingProjectionIsAttentionWithoutInventory:61](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) — 1 Fact | Absent destination with present source gives warnings/2 and link-missing code, no inventory, unchanged workspace. | No exact finding path or useful action; state seeded, not public attach then unlink. |

## PublishedLibrarySharedGrantProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) — 6 rows | Six rows attach/sync/detach by preview/apply. Repeated docs/tools grants persist and keep unrelated setting; preview leaves settings/workspace unchanged. Sync/detach start with public attach. | No real once/always prompt, denial, exact-file/sibling distinction or grant then content failure; apply focuses on settings. |

## PublishedLibrarySyncProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [UnchangedInventoryIsAnEffectFreeNoOp:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) — 1 Fact | Seeded current mapping gives completed/0, empty effects, unchanged workspace/source, no infrastructure. | No human up-to-date explanation or public sync then repeat. |
| [DryRunThenApplyReconcilesOneAdditionAndRetirement:24](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) — 1 Fact | Connected preview/apply preserves preview state, matches planned/applied effect shapes, removes old link target, creates exact relative new link, retains only new member, preserves source, no recovery. | No unchanged third member/count, sentinel, initial attach/inspect/repeat/detach. Null target alone is weaker than explicit entry absence. |
| [ChangedOccupantBlocksWholeSync:63](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) — 1 Fact | Changed ordinary occupant plus independent source addition gives blocked/5, unchanged snapshot and explicitly no new link; source preserved. | Opposite continuation target C27-07. |

## PublishedReferencesProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedReferencesDefaultIsBothExpandedAndReadOnly:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedReferencesProcessTests.cs) — 1 Fact | Default references exits2, subject and outgoing markers/destination names shown, unwanted decorations absent, source hashes unchanged. | Despite title, no incoming-row assertion; no generated Entries exclusion or exact location checks. |
| [PublishedReferencesOutgoingPreservesDirectFacts:33](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedReferencesProcessTests.cs) — 1 Fact | Out-only JSON exact eight destination occurrences and base/overwrite layers; external unchecked/no resolvedPath, missing and fragment-missing distinguished; hashes unchanged. | Does not assert incoming not-requested/absent fields, precise locations or absence of network requests. |
| [PublishedReferencesIncomingFiltersPreserveSelectorEvidence:62](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedReferencesProcessTests.cs) — 1 Fact | In-only include alpha/exclude beta yields exactly one alpha incoming occurrence and exact scanned universe alpha; filter values retained; hashes unchanged. | Single-file include, not subtree expansion; location/content detail unasserted. |

## PublishedRepairProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [HelpSucceedsWithoutWorkspaceInspectionOrWrites:13](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) — 1 Fact | Help succeeds with missing workspace and no write infrastructure/state changes. | Help only. |
| [AutomaticJsonDryRunPreviewsSafeExactWithoutEffects:37](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) — 1 Fact | Safe ./guide.md rewrite plus separate ambiguous missing.md: JSON/text preview says one repair/one choice, planned effect, four candidates, exact counts, warning code; source/lock/recovery state unchanged. | Dryrun only; does not apply safe work alongside ambiguous link. Does not assert actionable next command. |
| [ExplicitRelinkPreservesContentAndConvergesToNoOp:104](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRepairProcessTests.cs) — 1 Fact | Measured occurrence relink changes missing.md to replacement.md; exact full source equality, visible label and unrelated binary preserved; reports done, no remaining, recovery removed and actual no recovery artifacts; automatic repeat empty/no-op with full snapshot equality. | One ASCII occurrence; no same-spelling decoy, wrong expected-destination guard, Unicode-scalar column, prior manual rename or Doctor/reference followup. |

## PublishedRouteCreateProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [JsonDryRunIsReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) — 1 Fact | JSON completed/dry-run target, parent listing and two effects; RunWithoutWrites snapshot and no-lock check. | Exact preview target/parent/effects and preservation are checked, but no Template source exists in this fixture, so Template preservation is not exercised. |
| [ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) — 1 Fact | Creates exact target bytes, bounded parent bytes, preserves loader, checks target kind and repeat no-op. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [InvalidMetadataUsesSharedStatusStreamAndExitWithoutWrites:109](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) — 1 Fact | Missing-description exit/stream/Next wording and write-free helper boundary. | The current EndToEnd test asserts missing description is invalid and required, opposite the reviewed optional-metadata target. |

## PublishedRouteInitProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [GenericDryRunApplyAndRepeatNoOpFormOneRealJourney:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInitProcessTests.cs) — 1 Fact | Single docs scope JSON preview/no writes; apply verified, final file exists with supplied description and heading; repeat empty effects/unchanged entrypoints and source hashes unchanged. | No multi-segment generic chain, parent Entries oracle, placeholder meaning, final-only metadata versus ancestors or full target bytes. |
| [FrameworkDryRunApplyAndRepeatNoOpPreserveOwnership:43](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInitProcessTests.cs) — 1 Fact | Installed Framework then sparse memory/mobile-app/working preview/apply; framework scaffold property, final file exists, verified; repeat empty effects/unchanged entrypoints/hashes. | Does not independently assert ownership graph despite title, scaffold exact bytes, unrelated categories or intermediate scope/authoring semantics. |
| [FrameworkWithoutTrustedInstallIsBlocked:77](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInitProcessTests.cs) — 1 Fact | Framework init on uninstalled workspace exits5 on stderr, installation-required text and install dryrun next; source hashes unchanged and no lock infrastructure. | No directory inventory; no implicit installed files detectable by file snapshot. |

## PublishedRouteInspectProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [JsonResultExposesCompleteTypedGraph:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInspectProcessTests.cs) — 1 Fact | Root selection/path exact and base+overwrite layers in order, schema3/completed/no next, source hashes unchanged. | Despite title, no inherited route chain, role, loading conditions or contribution/byte/token measurement assertions. |
| [ExactPathAttentionStatusHasNoInventedNext:43](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInspectProcessTests.cs) — 1 Fact | Exact colliding leaf path succeeds with warning exit2 and exact ID/path text, no Next, source hashes unchanged. | Does not assert non-unique warning reason or distinguish measurements from other candidate. |
| [BlockedCollisionUsesExactNextWording:61](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInspectProcessTests.cs) — 1 Fact | Ambiguous ID in redirected session exits5, explains multiple sources and exact-path next command; source hashes unchanged. | Not terminal choice branch; no alternative-order reversal, all candidates, or no blended measurement assertion. |

## PublishedRouteListProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedRouteListEmitsStructuredReadOnlyResult:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteListProcessTests.cs) — 1 Fact | Default JSON completed/depth1, contains workspace-defined row, source hashes unchanged. | Does not assert complete topology, depth-boundary rows, usable ID followup or native Skill membership. |
| [PublishedRouteListExactPathUsesZeroDepthCompactView:35](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteListProcessTests.cs) — 1 Fact | Exact entrypoint depth0 accepted, root shown, root/child and path/status/coverage decorations absent; hashes unchanged. | Checks one excluded child substring, not whole output or all descendants; no subtree, depthall or empty fixture. |
| [PublishedRouteListInvalidDepthUsesTypedJson:52](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteListProcessTests.cs) — 1 Fact | Negative depth gives exit4/invalid-input, route-list.invalid-depth and no next, source hashes unchanged. | Does not assert accepted depth forms in diagnostic or execute same selected-source command variant. |

## PublishedRouteMoveProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [JsonDryRunIsReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) — 1 Fact | JSON completed/dry-run source/destination/moved data and write-free helper boundary. | Dry-run identity and moved data are checked through the process helper, but complete reference/navigation plan contents are not. |
| [LeafApplyPreservesBytesAndConsumesTheOldIdentity:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) — 1 Fact | Moves leaf and overwrite bytes, rewrites README link, preserves lifecycle, and rejects old-ID repeat. | Leaf source/overwrite bytes and old identity are checked, but old/new parent Entries are not independently asserted. README rewrite and moved bytes are checked; complete affected-link inventory and every authored reference are not asserted by the EndToEnd method. |
| [OccupiedDestinationRefusesWrites:84](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) — 1 Fact | Occupied destination yields blocked error and write-free helper snapshot. | Occupied target and write-free rejection are checked. The assertion checks already-exists prose, not that the actual destination path is named. |

## PublishedRouteRemoveProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [HelpAndInvalidInputHaveExactPublicBoundaries:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) — 1 Fact | Help grammar/exit mapping and missing operand stream; missing workspace remains absent. | Focused command boundary; no complete reviewed flow. See the scenario mappings and fixture limits for omitted branches. |
| [LeafDryRunAndApplicationPreserveLifecycleAndSurroundingBytes:53](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) — 1 Fact | Leaf dry-run detachedLinks/no-write snapshot, apply removes source/overwrite/entry, detaches label and preserves lifecycle. | Target/overwrite absence and removed-source prose are checked; parent-entry removal is asserted in output, not by comparing actual bounded parent bytes or all neighbors. Dry-run no-write state and detachedLinks are asserted in JSON; complete future-tense text and every planned effect are not. |
| [CategoryApplicationProjectionAndRepeatAreStable:103](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) — 1 Fact | Category tree/resources/navigation deletion and repeated missing-source invalid result. | Category/children/resource absence is asserted, but complete effect enumeration, preserved neighbors and incoming reference detachment are not all independently checked. EndToEnd repeat asserts exit 4/source-not-found for verified absence, opposite the revised harmless no-op target. |

## PublishedRouteUpdateProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [JsonDryRunIsReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) — 1 Fact | JSON completed/dry-run target and nonempty changes/effects through write-free helper. | JSON dry-run, target, change list, effects and no writes are checked; future-tense human output is not. |
| [ApplyThenNoOpIsOneExactJourney:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) — 1 Fact | Description/tag byte replacement, bounded parent rewrite, persistent lock and exact repeat no-op. | Exact description replacement and bounded parent bytes are asserted, but the report is only checked for a generic Updated headline, not explicit old/new fields. Exact final tag bytes are asserted, but ordered replacement is coupled to description and not separately reported. |
| [AmbiguousTargetRefusesWrites:85](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) — 1 Fact | Ambiguous physical target yields blocked exit/error and write-free helper snapshot. | Ambiguous physical target is blocked and write-free, but exact candidate paths and the reviewed human ambiguity wording are not asserted. |

## PublishedSchema3ProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [EveryPublishedBindingSupportsBothFormatsAtEveryDetail:39](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedSchema3ProcessTests.cs) — 28 rows | 28 command rows each inspect an unavailable workspace in both output formats and four explicit detail levels. Verifies schema-3 field order, stream rules, exit/status, stable facts across detail levels, and no workspace/catalogue/lock creation. | 224 process invocations are still 28 declared theory executions. Missing-workspace outcomes only; not every successful, warning, partial or mutation result, default detail, or actionable human prose. |

## PublishedShellBoundaryProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedRouteListAcceptsOptionLikeSourceAfterTerminator:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Option-looking operand after -- reaches domain source validation; no writes. | Only the selected binding and invalid subject. |
| [PublishedRouteListUsesNativeDepthForms:41](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 4 rows | Native depth option forms and missing-value binding are asserted through typed results. | Does not establish complete route discovery or all invalid values. |
| [PublishedAttachedEmptyDepthPreservesJsonInvalidResult:87](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Attached empty --depth= produces invalid/4 typed JSON with one route-list.invalid-depth finding on --depth, empty stderr and unchanged workspace. | One invalid scalar value, not a full route discovery case. |
| [PublishedRouteListRejectsRepeatedDepthOccurrencesAsParserError:109](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Repeated depth is a parser error: invalid/4, empty stdout, nonempty stderr and unchanged workspace despite JSON requested. | No domain envelope or subsequent corrected command is expected or exercised. |
| [PublishedTerminalModesRejectDomainAndLocalInput:133](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 2 rows | Help/version ordering relative to supplied options and workspace effects is asserted. | Parser boundaries only, not all user correction cases. |
| [PublishedTerminalModesAcceptWellFormedGlobalNoOpOptions:161](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 2 rows | Help/version ordering relative to supplied options and workspace effects is asserted. | Parser boundaries only, not all user correction cases. |
| [InspectVersionBypassesWorkspaceAndOperation:184](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Version bypasses missing workspace and operation with exact output. | Version only. |
| [OptionLikeOperandAfterTerminatorRemainsDomainInput:211](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Inspect receives an option-looking operand after -- as domain input and reports unknown source without writes. | Not a successful route lifecycle or general correction flow. |
| [TerminalModesRejectSourceInputBeforeWorkspaceAndOperation:238](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 4 rows | Help/version with local source input are rejected before workspace observation and operation effects. | Terminal flags only; no interactive prompt or corrected normal command. |
| [AutomaticIsNotACommandMode:287](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Route Init rejects unsupported --automatic as a parser failure. | Does not prove a supported automatic write or prompt. |
| [ThirdPositionalOperandIsShellInvalidWithoutDomainEffects:306](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Extra Route Move positional operand is invalid without domain effects. | Does not test correcting and then moving the same state. |
| [NativeGlobalScalarDelimitersReachTheCommand:329](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 3 rows | Space, equals and colon scalar forms bind; JSON identifies minimal detail, deduplicated ordered filters and the selected root. | One healthy Route List case, not content filtering or all view outcomes. |
| [RetiredPresentationFlagsAreRejected:353](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 3 rows | Three retired flags are rejected with stderr, invalid exit and no domain envelope or changes. | No new user-flow behavior. |
| [DebugAddsOnlyDiagnosticsToFullDetail:368](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedShellBoundaryProcessTests.cs) — 1 Fact | Healthy Route List full/debug JSON differs only in detail metadata; debug alone adds bounded stderr diagnostics. | Not default=minimal, human usefulness, warning/partial outcomes, or all commands. |

## PublishedStatusProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [PublishedRootAndStatusHelpExposeTheDirectLeafInImplementedOrderWithoutWorkspaceInspection:13](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) — 1 Fact | Root/status help succeeds against a nonexistent selected workspace; expected leaf order, no created path, unchanged workspace/recovery snapshot. | Help only; no ordinary uninstalled or missing-workspace status. |
| [PublishedStatusHumanJourneyUsesSemanticStreamExitAndPreservesWorkspaceAndRecoveryBytes:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) — 1 Fact | Minimal and standard status return 3 with installed text, incomplete recovery-draft path and unfinished-command finding; detail-specific cost headings; workspace and recovery bytes unchanged. | Artificial draft, no valid bundle. Startup numbers themselves, cleanup action and truthful ownership unavailability not asserted. |
| [PublishedStatusJsonJourneyEmitsOneSchemaDocumentAndPreservesWorkspaceAndRecoveryBytes:78](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) — 1 Fact | Schema 3 status incomplete, single draft candidate of incomplete integrity, exit3/stdout; workspace and recovery preserved. | No healthy/read-denied/changed-source mixed fixture or per-measurement availability. |

## PublishedUpdateProcessTests

| Method and declared executions | Existing assertions | Limits relative to reviewed flows |
| --- | --- | --- |
| [HelpInvalidSingletonAndBooleanRepetitionJourney:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedUpdateProcessTests.cs) — 1 Fact | Help lists modes; repeated --workspace exits4 without writes; repeated Boolean automatic/dryrun accepted and JSON flags/mode exact with no next. | No unknown-flag case or actual changed/missing mixed plan. |
| [RedirectedConfirmationAutomaticDryRunApplyRepeatJourney:74](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedUpdateProcessTests.cs) — 1 Fact | After one real loader edit, force without automatic exits4/no writes; force preview planned effects/no writes; force apply reports verified nonempty effects; repeat has empty effects and unchanged workspace hashes. | Apply trusts report verification; does not read restored loader bytes or prior bundle payload. No missing target, no ordinary human preview. |
| [OrdinaryUpdateAndPruneRetainReviewableRecovery:156](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedUpdateProcessTests.cs) — 1 Fact | Edited loader status flags changed; Doctor six categories and update action. Normal update verified with retained existing recovery path. Synthetic retired ownership + force/prune deletes actual retired file; repeat no effects/source changes. | No exact replacement byte oracle or recovery contents; retired target injected into lock rather than two real versions; no default keep-retired branch or unowned-neighbor assert. |

