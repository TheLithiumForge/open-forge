---
open-forge:
  description: Individual scenario coverage with exact existing evidence and missing outcomes
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# Library Scenarios

Return to the [review](./_cli-experience-coverage.md). **Status describes E2E evidence for the whole individual scenario**, not current runtime success. Lower-tier evidence is named separately. Deferred and omitted identities remain for accounting and are not accepted test requirements.

## C24-01

**[None registered](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-01)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent ownership gives completed/0, empty libraries and no inventory; snapshot unchanged, no infrastructure. |

**Gap / qualification:** Absent ownership is not readable known-empty ownership. Install legacy-file test invokes list but asserts only exit and preservation.

## C24-02

**[One current](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [HealthyRecordsAreDeterministic:21](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Two seeded registrations give identical repeated JSON/streams/exit; ordered IDs and links, expected mapping and current states; unchanged workspace. |

**Gap / qualification:** Two JSON registrations, not one concise human row.

## C24-03

**[Link missing](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [MissingProjectionIsAttentionWithoutInventory:61](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent destination with present source gives warnings/2 and link-missing code, no inventory, unchanged workspace. |

**Gap / qualification:** No exact finding path or useful action.

## C24-04

**[Link changed](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-04)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ChangedOccupantBlocksWholeSync:63](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Changed ordinary occupant plus independent source addition gives blocked/5, unchanged snapshot and explicitly no new link; source preserved. |
| [ChangedOccupantPreservesEveryProjectionAndRecord:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Changed ordinary occupant plus other link gives blocked/5 and unchanged full snapshot/ownership, preserving other link/source. |

**Gap / qualification:** Changed occupant tested through mutations, not list.

## C24-05

**[No ownership record](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-05)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent ownership gives completed/0, empty libraries and no inventory; snapshot unchanged, no infrastructure. |

**Gap / qualification:** No unavailable-count or known-empty distinction assertion.

## C24-06

**[Source folder missing](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-06)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyRemovesExactAndDanglingLinksBeforeLastRecord:27](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Current and dangling targets become null; libraries array empty, registrationRemoved=true; remaining source preserved, no recovery. |

**Gap / qualification:** Dangling member during detach, not missing-root list.

## C24-07

**[Record invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No malformed ownership list.

## C24-08

**[Record unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No independently denied ownership read.

## C24-09

**[Link blocked](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No unsafe linked ancestry/identity list.

## C24-10

**[Invalid input](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No unsupported positional ID list case.

## C24-S08

**[Outcome 08](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-s08)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred unexpected fault not exercised.

## C24-S09

**[Outcome 09](../../../crystallized/documents/cli/experience/scenarios/commands/c24-library-list.md#c24-s09)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred cancellation not exercised.

## C25-01

**[Current](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [HealthyProjectionHasCompleteInventoryAndDestinationIdentity:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Seeded docs mapping yields one current row, current=true, source/destination paths, equal expected/observed targets and preserved source. |

**Gap / qualification:** No complete identity/count communication.

## C25-02

**[Added source files](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [CompleteInventoryExplainsAdditionRetirementAndMissingProjection:51](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Mixed seeded state yields exactly ordered added, retired, missing rows and destination paths, warnings/2 and unchanged snapshot. |

**Gap / qualification:** Seeded mixed addition state, no attach then source addition.

## C25-03

**[Retired source files](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [CompleteInventoryExplainsAdditionRetirementAndMissingProjection:51](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Mixed seeded state yields exactly ordered added, retired, missing rows and destination paths, warnings/2 and unchanged snapshot. |

**Gap / qualification:** Seeded retirement, no public sequence or sync advice.

## C25-04

**[Missing links](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-04)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [CompleteInventoryExplainsAdditionRetirementAndMissingProjection:51](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Mixed seeded state yields exactly ordered added, retired, missing rows and destination paths, warnings/2 and unchanged snapshot. |

**Gap / qualification:** Missing row asserted, restoration explanation absent.

## C25-05

**[Changed links](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-05)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ChangedOccupantBlocksWholeSync:63](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Changed ordinary occupant plus independent source addition gives blocked/5, unchanged snapshot and explicitly no new link; source preserved. |
| [ChangedOccupantPreservesEveryProjectionAndRecord:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Changed ordinary occupant plus other link gives blocked/5 and unchanged full snapshot/ownership, preserving other link/source. |

**Gap / qualification:** Other mutation commands, not inspect.

## C25-06

**[Empty source](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No enumerable empty source inspection.

## C25-07

**[Source unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No denied/incomplete source inventory.

## C25-08

**[Record invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No malformed ownership inspect.

## C25-09

**[Unknown id](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-09)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [RequiredIdOmissionIsInvalidWithoutObservation:32](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Omitted ID gives invalid/4, empty stdout, Cannot inspect and Library List guidance; full detail includes invalid-ID code; no changes. |

**Gap / qualification:** Omitted operand differs from valid known-absent ID.

## C25-10

**[No ownership record](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-10)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent ownership gives completed/0, empty libraries and no inventory; snapshot unchanged, no infrastructure. |

**Gap / qualification:** Missing ownership only for list.

## C25-11

**[Invalid id](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-11)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [RequiredIdOmissionIsInvalidWithoutObservation:32](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Omitted ID gives invalid/4, empty stdout, Cannot inspect and Library List guidance; full detail includes invalid-ID code; no changes. |

**Gap / qualification:** Omitted ID differs from supplied malformed ID.

## C25-12

**[Blocked mapping](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No unsafe/conflicting mapping inspection.

## C25-S08

**[Outcome 08](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-s08)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred unexpected fault absent.

## C25-S09

**[Outcome 09](../../../crystallized/documents/cli/experience/scenarios/commands/c25-library-inspect.md#c25-s09)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred cancellation absent.

## C26-01

**[Attached inside agents](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-01)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunReportsCompletePlanWithoutEffects:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) | Default-destination preview asserts source/destination, dry-run, recorded=false, permission not-required, no directory/infrastructure or snapshot changes. |
| [ApplyCreatesExactRelativeProjectionAndOwnershipReceipt:26](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) | Public attach to pre-granted docs creates exact relative file link, ordinary parent, exact ownership member; source and authored route prefix preserved, unrelated README unlisted, persistent empty lock, no recovery. |

**Gap / qualification:** Only preview implicit admission or apply pre-granted docs; no routed .agents attach and Entries verification.

## C26-02

**[Attached outside with flag](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Six rows attach/sync/detach by preview/apply. Repeated docs/tools grants persist and keep unrelated setting; preview leaves settings/workspace unchanged. Sync/detach start with public attach. |
| [ApplyCreatesExactRelativeProjectionAndOwnershipReceipt:26](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) | Public attach to pre-granted docs creates exact relative file link, ordinary parent, exact ownership member; source and authored route prefix preserved, unrelated README unlisted, persistent empty lock, no recovery. |

**Gap / qualification:** Grant persistence and mapping tested separately; no single case plus reserved boundary.

## C26-03

**[Permission prompt](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No real terminal once/always choice.

## C26-04

**[Permission required non interactive](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-04)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Six rows attach/sync/detach by preview/apply. Repeated docs/tools grants persist and keep unrelated setting; preview leaves settings/workspace unchanged. Sync/detach start with public attach. |

**Gap / qualification:** Supplies replacement grants; does not assert uncovered automatic denial.

## C26-05

**[Empty source](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No empty-source registration.

## C26-06

**[Duplicate id](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-06)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No repeated identical attach or conflicting remap.

## C26-07

**[Source missing](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No missing-source attach.

## C26-08

**[Destination collision](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-08)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [OccupiedDestinationBlocksWholeAttach:67](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryAttachProcessTests.cs) | Occupied ordinary file yields blocked/5, unchanged full snapshot, no registration/infrastructure and preserved source. |

**Gap / qualification:** Ordinary occupied leaf only; no unowned exact-looking link, precise finding or multi-member plan.

## C26-09

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-09)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Six rows attach/sync/detach by preview/apply. Repeated docs/tools grants persist and keep unrelated setting; preview leaves settings/workspace unchanged. Sync/detach start with public attach. |

**Gap / qualification:** No proposed links/directories or scope explanation assertion.

## C26-10

**[Links unsupported](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-10)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No genuine symlink capability failure and partial-state verification.

## C26-11

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No live attach contention.

## C26-12

**[Record invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No explicit attach despite malformed prior ownership.

## C26-13

**[Interrupted partial](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No after-link cancellation and truthful receipt.

## C26-14

**[Invalid input](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No malformed supplied attach ID.

## C26-S04

**[Outcome 04](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-s04)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred retained-recovery fault absent.

## C26-S09

**[Outcome 09](../../../crystallized/documents/cli/experience/scenarios/commands/c26-library-attach.md#c26-s09)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred cancellation absent.

## C27-01

**[Up to date](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [UnchangedInventoryIsAnEffectFreeNoOp:9](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Seeded current mapping gives completed/0, empty effects, unchanged workspace/source, no infrastructure. |

**Gap / qualification:** No human up-to-date message or public prior-state provenance.

## C27-02

**[Links added](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunThenApplyReconcilesOneAdditionAndRetirement:24](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Connected preview/apply preserves preview state, matches planned/applied effect shapes, removes old link target, creates exact relative new link, retains only new member, preserves source, no recovery. |

**Gap / qualification:** No existing-current-link preservation.

## C27-03

**[Links removed](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunThenApplyReconcilesOneAdditionAndRetirement:24](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Connected preview/apply preserves preview state, matches planned/applied effect shapes, removes old link target, creates exact relative new link, retains only new member, preserves source, no recovery. |

**Gap / qualification:** No sibling sentinel or explicit entry absence.

## C27-04

**[Both](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-04)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunThenApplyReconcilesOneAdditionAndRetirement:24](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Connected preview/apply preserves preview state, matches planned/applied effect shapes, removes old link target, creates exact relative new link, retains only new member, preserves source, no recovery. |

**Gap / qualification:** No unchanged third member/count.

## C27-05

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-05)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunThenApplyReconcilesOneAdditionAndRetirement:24](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Connected preview/apply preserves preview state, matches planned/applied effect shapes, removes old link target, creates exact relative new link, retains only new member, preserves source, no recovery. |

**Gap / qualification:** No independent exact receipt or reviewed public-created fixture.

## C27-06

**[Unknown id](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No valid ownership with unknown selected ID.

## C27-07

**[Changed occupant](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-07)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ChangedOccupantBlocksWholeSync:63](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Changed ordinary occupant plus independent source addition gives blocked/5, unchanged snapshot and explicitly no new link; source preserved. |

**Gap / qualification:** Explicit whole-sync block and no independent addition contradict continuation.

## C27-08

**[Registered link gone](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-08)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [CompleteInventoryExplainsAdditionRetirementAndMissingProjection:51](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryInspectProcessTests.cs) | Mixed seeded state yields exactly ordered added, retired, missing rows and destination paths, warnings/2 and unchanged snapshot. |

**Gap / qualification:** Inspect observes missing link; no sync restoration/warning.

## C27-09

**[Source unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No incomplete/denied scan preserving links.

## C27-10

**[Permission required](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-10)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Six rows attach/sync/detach by preview/apply. Repeated docs/tools grants persist and keep unrelated setting; preview leaves settings/workspace unchanged. Sync/detach start with public attach. |

**Gap / qualification:** Replacement grants, not denied revoked permission.

## C27-11

**[No ownership record](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-11)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent ownership gives completed/0, empty libraries and no inventory; snapshot unchanged, no infrastructure. |

**Gap / qualification:** Missing ownership only for list.

## C27-12

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No live sync contention.

## C27-13

**[Record invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No malformed ownership sync.

## C27-14

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No controlled failure after link effect.

## C27-15

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-15)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No real terminal cancellation.

## C27-S06

**[Outcome 06](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-s06)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred retained-recovery fault absent.

## C27-16

**[A retired registration has no deletable link](../../../crystallized/documents/cli/experience/scenarios/commands/c27-library-sync.md#c27-16)** — selection: improved; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunThenApplyReconcilesOneAdditionAndRetirement:24](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySyncProcessTests.cs) | Connected preview/apply preserves preview state, matches planned/applied effect shapes, removes old link target, creates exact relative new link, retains only new member, preserves source, no recovery. |

**Gap / qualification:** Retires existing dangling link, not already absent destination.

## C28-01

**[Detached](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyRemovesExactAndDanglingLinksBeforeLastRecord:27](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Current and dangling targets become null; libraries array empty, registrationRemoved=true; remaining source preserved, no recovery. |

**Gap / qualification:** No sibling/navigation preservation or source-kept explanation.

## C28-02

**[Detached no links](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-02)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No empty registered link-set detach.

## C28-03

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DryRunReportsExactDeletionWithoutEffects:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Preview asserts selected ID, registrationRemoved=false, one exact-target deletion; unchanged workspace, no infrastructure. |

**Gap / qualification:** Seeded registration, not public attachment provenance.

## C28-04

**[Unknown id](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-04)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No known-absent repeated detach no-op.

## C28-05

**[Registered link gone](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-05)** — selection: improved; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyRemovesExactAndDanglingLinksBeforeLastRecord:27](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Current and dangling targets become null; libraries array empty, registrationRemoved=true; remaining source preserved, no recovery. |

**Gap / qualification:** Dangling link exists; absent destination not exercised.

## C28-06

**[Changed occupant](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-06)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ChangedOccupantPreservesEveryProjectionAndRecord:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryDetachProcessTests.cs) | Changed ordinary occupant plus other link gives blocked/5 and unchanged full snapshot/ownership, preserving other link/source. |

**Gap / qualification:** Whole registration and other links retained, contrary to independent detach.

## C28-07

**[Destination protected](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No protected control/unsafe ancestry detach.

## C28-08

**[Permission required](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-08)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ExplicitRepeatableGrantPersistsAndDryRunWritesNothing:12](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibrarySharedGrantProcessTests.cs) | Six rows attach/sync/detach by preview/apply. Repeated docs/tools grants persist and keep unrelated setting; preview leaves settings/workspace unchanged. Sync/detach start with public attach. |

**Gap / qualification:** Replacement grants, not revocation denial.

## C28-09

**[No ownership record](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-09)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AbsentRecordIsCompleteWithoutInventory:8](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryListProcessTests.cs) | Absent ownership gives completed/0, empty libraries and no inventory; snapshot unchanged, no infrastructure. |

**Gap / qualification:** Missing ownership only for list.

## C28-10

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No live detach contention.

## C28-11

**[Record invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No malformed ownership detach.

## C28-12

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No failure after a real deletion.

## C28-13

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No real terminal cancellation.

## C28-S05

**[Outcome 05](../../../crystallized/documents/cli/experience/scenarios/commands/c28-library-detach.md#c28-s05)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred retained-recovery fault absent.

