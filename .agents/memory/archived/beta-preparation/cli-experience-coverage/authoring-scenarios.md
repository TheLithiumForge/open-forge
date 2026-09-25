---
open-forge:
  description: Individual scenario coverage with exact existing evidence and missing outcomes
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# Route And Extension Scenarios

Return to the [review](./_cli-experience-coverage.md). **Status describes E2E evidence for the whole individual scenario**, not current runtime success. Lower-tier evidence is named separately. Deferred and omitted identities remain for accounting and are not accepted test requirements.

## C14-01

**[Created](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-01)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | Creates exact target bytes, bounded parent bytes, preserves loader, checks target kind and repeat no-op. |

**Gap / qualification:** Exact destination/parent bytes, bounded navigation and repeat no-op are asserted; no independent Template or missing-metadata branch.

## C14-02

**[Created from template](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-02)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [TemplateResolutionCopiesOnlyExactClassifiedBody:19](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/RouteCreatePlanningIntegrationTests.cs): Exact classified Template body is copied without frontmatter.

## C14-03

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [JsonDryRunIsReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | JSON completed/dry-run target, parent listing and two effects; RunWithoutWrites snapshot and no-lock check. |

**Gap / qualification:** Exact preview target/parent/effects and preservation are checked, but no Template source exists in this fixture, so Template preservation is not exercised.

## C14-04

**[Already matching](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-04)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | Creates exact target bytes, bounded parent bytes, preserves loader, checks target kind and repeat no-op. |

**Gap / qualification:** The second invocation proves matching-content no-op and byte stability; it follows an apply in the same method.

## C14-05

**[Missing description](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-05)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [InvalidMetadataUsesSharedStatusStreamAndExitWithoutWrites:109](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateProcessTests.cs) | Missing-description exit/stream/Next wording and write-free helper boundary. |

**Gap / qualification:** The current EndToEnd test asserts missing description is invalid and required, opposite the reviewed optional-metadata target.

## C14-06

**[Missing tag](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-06)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C14-07

**[Missing both](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-07)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C14-08

**[Invalid target](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C14-09

**[Create an unambiguous descendant with absent intermediate scopes](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-09)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Integration planning asserts missing/ambiguous parents block before effects; no EndToEnd evidence supports automatic intermediate-scope creation.

**Separate lower-tier evidence:**

- Integration [PlanningBlocksMissingOrAmbiguousParents:123](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/RouteCreatePlanningIntegrationTests.cs) — **opposite target**: Current planner blocks missing/ambiguous parents before effects.

## C14-10

**[Exists with different content](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C14-11

**[Template unknown](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MissingTemplateReferencesAreInvalidForCreateAndUpdate:79](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateTemplateIntegrationTests.cs): Shared Create/Update template resolver rejects unknown references.

## C14-12

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C14-13

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [FinalVerificationRejectsPostApplicationTargetChange:97](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/RouteCreateApplicationIntegrationTests.cs): Lower-tier final verification rejects a target changed after application.

## C14-14

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C14-S04

**[Outcome 04](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-s04)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete recovery-requiring public create plan is covered.

## C14-S05

**[Outcome 05](../../../crystallized/documents/cli/experience/scenarios/commands/c14-route-create.md#c14-s05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PostCatalogueTemplateReadFailureRemainsIncomplete:105](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateTemplateIntegrationTests.cs): Lower-tier shared template read failure remains incomplete.

## C15-01

**[Description changed](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyThenNoOpIsOneExactJourney:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) | Description/tag byte replacement, bounded parent rewrite, persistent lock and exact repeat no-op. |

**Gap / qualification:** Exact description replacement and bounded parent bytes are asserted, but the report is only checked for a generic Updated headline, not explicit old/new fields.

## C15-02

**[Tags replaced](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyThenNoOpIsOneExactJourney:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) | Description/tag byte replacement, bounded parent rewrite, persistent lock and exact repeat no-op. |

**Gap / qualification:** Exact final tag bytes are asserted, but ordered replacement is coupled to description and not separately reported.

## C15-03

**[Responsibility removed](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-03)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ResponsibilityHasNoNavigationDependency:42](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdatePlanningIntegrationTests.cs): Responsibility-only patch schedules no generated navigation work.

## C15-04

**[Template applied](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-04)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [EligibleTemplateBodyAppliesAndVerifies:279](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrationTests.cs): Eligible Template body applies and protected no-op transition is verified.

## C15-05

**[Template body protected](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [EligibleTemplateBodyAppliesAndVerifies:279](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrationTests.cs): Eligible Template body applies and protected no-op transition is verified.

## C15-06

**[No change](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-06)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [ApplyThenNoOpIsOneExactJourney:38](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) | Description/tag byte replacement, bounded parent rewrite, persistent lock and exact repeat no-op. |

**Gap / qualification:** Repeat invocation asserts requested values already present, no-op wording and unchanged snapshot; no standalone preseeded-only test.

## C15-07

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-07)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [JsonDryRunIsReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) | JSON completed/dry-run target and nonempty changes/effects through write-free helper. |

**Gap / qualification:** JSON dry-run, target, change list, effects and no writes are checked; future-tense human output is not.

## C15-08

**[No patch](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-08)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C15-09

**[Unknown source](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MissingTargetReferencesAreInvalidAndWriteFree:155](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateTargetObservationIntegrationTests.cs): Unknown/missing route references are invalid and write-free.

## C15-10

**[An ambiguous ID requires an exact path](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-10)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [AmbiguousTargetRefusesWrites:85](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateProcessTests.cs) | Ambiguous physical target yields blocked exit/error and write-free helper snapshot. |

**Gap / qualification:** Ambiguous physical target is blocked and write-free, but exact candidate paths and the reviewed human ambiguity wording are not asserted.

## C15-11

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ExistingWorkspaceLeaseBlocksApplication:339](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrationTests.cs): Real integration lease blocks before revalidation/recovery/writes.

## C15-12

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MidApplicationCancellationRetainsExactProgress:528](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrationTests.cs): Cancellation after a first effect retains exact progress.

## C15-13

**[Cancel before any persistent effect](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [CallerCancellationIsTypedAndWriteFree:388](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrationTests.cs): Pre-effect caller cancellation is typed and write-free.

## C15-S05

**[Outcome 05](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-s05)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete recovery-requiring public update plan is covered.

## C15-S06

**[Outcome 06](../../../crystallized/documents/cli/experience/scenarios/commands/c15-route-update.md#c15-s06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PostCatalogueTemplateReadFailureRemainsIncomplete:105](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateTemplateIntegrationTests.cs): Lower-tier shared template read failure remains incomplete.

## C16-01

**[Leaf move](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LeafApplyPreservesBytesAndConsumesTheOldIdentity:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) | Moves leaf and overwrite bytes, rewrites README link, preserves lifecycle, and rejects old-ID repeat. |

**Gap / qualification:** Leaf source/overwrite bytes and old identity are checked, but old/new parent Entries are not independently asserted.

## C16-02

**[Leaf move with rewritten links](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LeafApplyPreservesBytesAndConsumesTheOldIdentity:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) | Moves leaf and overwrite bytes, rewrites README link, preserves lifecycle, and rejects old-ID repeat. |

**Gap / qualification:** README rewrite and moved bytes are checked; complete affected-link inventory and every authored reference are not asserted by the EndToEnd method.

**Separate lower-tier evidence:**

- Integration [ReferencePlanningUsesExactSpansAndCompleteFileCoalescing:430](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMovePlanningIntegrationTests.cs): Reference planning uses exact spans and complete file coalescing.

## C16-03

**[Category move](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [CategoryApplicationPreservesResourcesAndRelativeReferences:107](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMoveApplicationIntegrationTests.cs): Category move preserves resources and relative references.

## C16-04

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-04)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [JsonDryRunIsReadOnly:10](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) | JSON completed/dry-run source/destination/moved data and write-free helper boundary. |

**Gap / qualification:** Dry-run identity and moved data are checked through the process helper, but complete reference/navigation plan contents are not.

**Separate lower-tier evidence:**

- Integration [DryRunAndApplyShareOneExactObservationAndPlan:298](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMovePlanningIntegrationTests.cs): Dry-run/apply planning share one observation and no-write plan.

## C16-05

**[Destination exists](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-05)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [OccupiedDestinationRefusesWrites:84](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) | Occupied destination yields blocked error and write-free helper snapshot. |

**Gap / qualification:** Occupied target and write-free rejection are checked. The assertion checks already-exists prose, not that the actual destination path is named.

## C16-06

**[Destination inside source](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [SelectedPhysicalSourcePlansWhenRemainingGuardsPass:16](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/Interaction/RouteMoveInteractionApplicationIntegrationTests.cs): Interactive physical source selection can plan the selected source.

## C16-07

**[Self move](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-07)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C16-08

**[Managed source](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ProjectionLeafCannotBeMutatedEvenWithoutRegistration:12](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/LibraryProjectionMoveGuardIntegrationTests.cs): Projection leaf is guarded even without a registration.

## C16-09

**[Source not found](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-09)** — selection: retained; E2E coverage: **adjacent**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LeafApplyPreservesBytesAndConsumesTheOldIdentity:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveProcessTests.cs) | Moves leaf and overwrite bytes, rewrites README link, preserves lifecycle, and rejects old-ID repeat. |

**Gap / qualification:** Only the repeat after a successful move proves an old source ID is then absent; no fresh missing-source fixture is used.

## C16-10

**[Ambiguous source prompt](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [SelectedPhysicalSourcePlansWhenRemainingGuardsPass:16](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/Interaction/RouteMoveInteractionApplicationIntegrationTests.cs): Interactive physical source selection can plan the selected source.

## C16-11

**[Reference scan incomplete](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RevalidationRejectsEveryStaleObservation:18](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMoveRevalidationIntegrationTests.cs): Stale volatile move facts are reobserved and rejected.

## C16-12

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C16-13

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ConcreteFailureLeavesPriorEffectAndAllLaterEffectsNotStarted:325](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMoveApplicationIntegrationTests.cs): Concrete later failure leaves prior receipts and later effects not started.

## C16-14

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [CancellationLeavesEveryEffectNotStarted:379](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMoveApplicationIntegrationTests.cs): Move cancellation leaves heterogeneous effects not started.

## C16-S04

**[Outcome 04](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-s04)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete recovery-requiring public move plan is covered.

## C16-15

**[Ownership cannot establish an unmanaged subject](../../../crystallized/documents/cli/experience/scenarios/commands/c16-route-move.md#c16-15)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ProjectionLeafCannotBeMutatedEvenWithoutRegistration:12](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/LibraryProjectionMoveGuardIntegrationTests.cs): Projection leaf is guarded even without a registration.

## C17-01

**[Leaf removed](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LeafDryRunAndApplicationPreserveLifecycleAndSurroundingBytes:53](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) | Leaf dry-run detachedLinks/no-write snapshot, apply removes source/overwrite/entry, detaches label and preserves lifecycle. |

**Gap / qualification:** Target/overwrite absence and removed-source prose are checked; parent-entry removal is asserted in output, not by comparing actual bounded parent bytes or all neighbors.

**Separate lower-tier evidence:**

- Integration [LeafApplicationDetachesIncomingLinkAndPreservesLifecycle:17](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs): Leaf removal detaches incoming link and preserves lifecycle.

## C17-02

**[Leaf with detached links](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-02)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LeafDryRunAndApplicationPreserveLifecycleAndSurroundingBytes:53](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) | Leaf dry-run detachedLinks/no-write snapshot, apply removes source/overwrite/entry, detaches label and preserves lifecycle. |

**Gap / qualification:** Dry-run records detached link and apply preserves visible label while removing the route.

**Separate lower-tier evidence:**

- Integration [LeafApplicationDetachesIncomingLinkAndPreservesLifecycle:17](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs): Leaf removal detaches incoming link and preserves lifecycle.

## C17-03

**[Category removed](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [CategoryApplicationProjectionAndRepeatAreStable:103](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) | Category tree/resources/navigation deletion and repeated missing-source invalid result. |

**Gap / qualification:** Category/children/resource absence is asserted, but complete effect enumeration, preserved neighbors and incoming reference detachment are not all independently checked.

**Separate lower-tier evidence:**

- Integration [CategoryApplicationRemovesEveryContainedItem:49](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs): Category removal deletes complete contained inventory.

## C17-04

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-04)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [LeafDryRunAndApplicationPreserveLifecycleAndSurroundingBytes:53](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) | Leaf dry-run detachedLinks/no-write snapshot, apply removes source/overwrite/entry, detaches label and preserves lifecycle. |

**Gap / qualification:** Dry-run no-write state and detachedLinks are asserted in JSON; complete future-tense text and every planned effect are not.

**Separate lower-tier evidence:**

- Integration [LeafApplicationDetachesIncomingLinkAndPreservesLifecycle:17](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs): Leaf removal detaches incoming link and preserves lifecycle.

## C17-05

**[Source not found](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-05)** — selection: improved; E2E coverage: **opposite**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [CategoryApplicationProjectionAndRepeatAreStable:103](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveProcessTests.cs) | Category tree/resources/navigation deletion and repeated missing-source invalid result. |

**Gap / qualification:** EndToEnd repeat asserts exit 4/source-not-found for verified absence, opposite the revised harmless no-op target.

**Separate lower-tier evidence:**

- Integration [CategoryApplicationRemovesEveryContainedItem:49](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs) — **opposite target**: Category removal deletes complete contained inventory.
- Integration [OrdinaryAbsenceProofRetainsEveryLegacyBoundaryFact:205](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveReferenceIntegrationTests.cs) — **opposite target**: Lower-tier absence proof retains boundary facts.

## C17-06

**[Managed source](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C17-07

**[Unsafe link detach](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-07)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection and lacks the concrete offending Markdown form.

## C17-08

**[Ambiguous source prompt](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [DeclinedConfirmationCancelsWithoutWrites:44](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/Interaction/RouteRemoveInteractionApplicationIntegrationTests.cs): Declined confirmation cancels without writes.

## C17-09

**[Reference scan incomplete](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [UnsupportedIncomingTransformationIsWriteFree:85](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveReferenceIntegrationTests.cs): Unsupported incoming transformation is rejected before writes.

## C17-10

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ContendedWorkspaceLockIsWriteFree:16](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveRevalidationIntegrationTests.cs): Held workspace lock is write-free.

## C17-11

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [LaterTargetRaceStopsNewEffectsAndRetainsRecovery:209](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs): Late target race retains prior effects/recovery.

## C17-12

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [DeclinedConfirmationCancelsWithoutWrites:44](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/Interaction/RouteRemoveInteractionApplicationIntegrationTests.cs): Declined confirmation cancels without writes.

## C17-S04

**[Outcome 04](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-s04)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete recovery-requiring public remove plan is covered.

## C17-13

**[Ownership cannot establish an unmanaged subject](../../../crystallized/documents/cli/experience/scenarios/commands/c17-route-remove.md#c17-13)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [OwnershipBoundariesAreWriteFree:280](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemovePlanningIntegrationTests.cs): Unknown ownership boundaries are write-free.

## C18-01

**[None installed](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedExplicitPackageIsExactAndReadOnly:62](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) | Explicit local package source kind/path/ID and source-byte preservation with no writes. |

**Gap / qualification:** Explicit available-only source with empty ownership is checked, but the default two-section Installed none presentation is not.

## C18-02

**[One installed](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedDefaultReportsBothSections:37](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) | Trusted installed development-toolkit and embedded available IDs in JSON; write-free helper. |

**Gap / qualification:** One trusted installed and available package IDs are asserted; matching marker and descriptions are not.

## C18-03

**[Installed only](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [AvailableOnlyDoesNotInferOrRequireLifecycle:202](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/ExtensionListApplicationIntegrationTests.cs): Available-only source remains useful without inferred lifecycle.

## C18-04

**[Available only](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-04)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedExplicitPackageIsExactAndReadOnly:62](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) | Explicit local package source kind/path/ID and source-byte preservation with no writes. |

**Gap / qualification:** Available-only package ID/source bytes are checked; useful description rendering is not.

## C18-05

**[Explicit source](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-05)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedExplicitPackageIsExactAndReadOnly:62](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListProcessTests.cs) | Explicit local package source kind/path/ID and source-byte preservation with no writes. |

**Gap / qualification:** Checks package source kind, one local-toolkit ID and preserved source bytes. Does not assert selected source path or displayed versions.

## C18-06

**[No ownership record](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [UnavailableOwnershipIsInformational:227](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/ExtensionListApplicationIntegrationTests.cs): Unavailable ownership remains informational rather than empty installation.

## C18-07

**[Source unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-08

**[Installed source missing](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [AvailableOnlyDoesNotInferOrRequireLifecycle:202](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/ExtensionListApplicationIntegrationTests.cs): Available-only source remains useful without inferred lifecycle.

## C18-09

**[Installed source unavailable](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-10

**[Installed source invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-11

**[Installed source blocked](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-12

**[Installed files changed](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [CompactComparisonRetainsObservedChangesWithoutWrites:26](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Inspect retains observed changed relation without writes.

## C18-13

**[Installed files missing](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RetirementAndMissingContentUseObservedMembership:671](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Retirement and missing content use receipt membership.

## C18-14

**[Installed target blocked](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-15

**[Installed target unavailable](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-15)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-16

**[Installed files unavailable](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-16)** — selection: not added; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Not added in the reviewed collection; umbrella has no distinct selected fixture.

## C18-17

**[Source invalid](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-17)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-18

**[Source blocked](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-18)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-19

**[Invalid input](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-19)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C18-S07

**[Outcome 07](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-s07)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete unexpected failure mechanism is supplied.

## C18-S08

**[Outcome 08](../../../crystallized/documents/cli/experience/scenarios/commands/c18-extension-list.md#c18-s08)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete cancellation signal boundary is supplied.

## C19-01

**[Installed matches](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedComparisonUsesCurrentAndIntendedContent:40](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) | Two theory rows assert changed/unchanged relation, selected package source, no writes and update next action. |

**Gap / qualification:** Theory unchanged row proves current/intended equality and JSON source identity; dependency/version/file detail completeness is not.

## C19-02

**[Installed changed and retired](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedComparisonUsesCurrentAndIntendedContent:40](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) | Two theory rows assert changed/unchanged relation, selected package source, no writes and update next action. |

**Gap / qualification:** Changed relation is covered, but the EndToEnd fixture has no retired path/receipt membership.

**Separate lower-tier evidence:**

- Integration [RetirementAndMissingContentUseObservedMembership:671](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Retirement and missing content use receipt membership.

## C19-03

**[Available not installed](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PackageUnavailableRetainsInstalledFacts:401](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Unavailable package source retains installed facts.

## C19-04

**[Installed source missing](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-04)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MissingExplicitSourceRemainsTheOnlySource:154](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Missing explicit source is not replaced by embedded facts.

## C19-05

**[Newer available](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C19-06

**[Dependency cycle](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-06)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [DependencyFailuresRetainOnlySafeClosureFacts:306](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Cyclic/incomplete dependency closure retains safe facts.

## C19-07

**[Unknown id](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InvalidStableIdProducesTypedResult:189](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Invalid stable ID keeps typed report shape.

## C19-08

**[No ownership record](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-08)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [UnknownOwnershipDoesNotGateOrAdopt:642](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Uninterpretable ownership does not adopt matching files.

## C19-09

**[Ambiguous source](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [AmbiguousSourceAndIdentityRemainUnselected:272](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Ambiguous source/duplicate identity remains unselected.

## C19-10

**[Invalid input](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InvalidStableIdProducesTypedResult:189](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs): Invalid stable ID keeps typed report shape.

## C19-S08

**[Outcome 08](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-s08)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete unexpected failure mechanism is supplied.

## C19-S09

**[Outcome 09](../../../crystallized/documents/cli/experience/scenarios/commands/c19-extension-inspect.md#c19-s09)** — selection: deferred; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Deferred in the reviewed collection; no concrete cancellation signal boundary is supplied.

## C20-01

**[Created](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedAutomaticApplyConvergesWithoutWorkspaceLifecycleOrRecoveryEffects:25](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) | Scaffold manifest/content creation, metadata defaults, repeat no-op, catalogue/workspace/lifecycle/recovery preservation. |

**Gap / qualification:** Default manifest and content directory are asserted; exact output location and distinction of file versus directory effects are not independently checked.

## C20-02

**[Created with metadata](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedJsonPreservesExactResultOrderAndStdoutIsolation:87](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) | Prompt-free JSON dry-run schema/order/paths and manifest name/description/version/dependency order with no writes. |

**Gap / qualification:** Only preview inspects supplied dependency order and manifest property names; it does not assert supplied descriptive values or an applied manifest preserving them.

## C20-03

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-03)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedJsonPreservesExactResultOrderAndStdoutIsolation:87](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) | Prompt-free JSON dry-run schema/order/paths and manifest name/description/version/dependency order with no writes. |

**Gap / qualification:** Preview asserts exact proposed paths and unchanged file hashes; explicit absence of directories/lock infrastructure is not checked in this method.

## C20-04

**[Already present](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-04)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedAutomaticApplyConvergesWithoutWorkspaceLifecycleOrRecoveryEffects:25](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) | Scaffold manifest/content creation, metadata defaults, repeat no-op, catalogue/workspace/lifecycle/recovery preservation. |

**Gap / qualification:** Second apply reports Nothing to do and catalogue bytes remain unchanged.

## C20-05

**[Destination has other content](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [DivergentDestinationsAreBlocked:126](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateScaffoldIntegrationTests.cs): Divergent/colliding scaffold destinations are blocked without overwrite.

## C20-06

**[Missing id non interactive](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PromptsOnlyForMissingFacts:101](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateOperationIntegrationTests.cs): Interactive create prompts only for missing required facts.

## C20-07

**[Prompted id and path](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [DirectAndInteractiveFlowsAreEquivalent:19](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateInteractionIntegrationTests.cs): Direct and interactive missing-fact flows resolve equivalently.

## C20-08

**[Invalid id](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [NameAndDependencyValidationIsLocal:136](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateOperationIntegrationTests.cs): ID/name/dependency validation is local.

## C20-09

**[Catalogue unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InvalidCatalogueParentsAreNoWrite:50](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateCatalogueIntegrationTests.cs): Missing/file-valued catalogue parents are no-write invalid.

## C20-10

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [LaterEffectFailureRetainsAppliedManifest:108](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateStagingIntegrationTests.cs): A real later effect failure retains the first create effect.

## C20-11

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [CancellationIsInterruptedWithoutWrites:278](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateOperationIntegrationTests.cs): Create cancellation is interrupted before effects.

## C20-12

**[End of input before a required answer](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [EndOfInputIsInvalidWithoutWrites:194](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateOperationIntegrationTests.cs): Create end-of-input is invalid and catalogue unchanged.

## C20-13

**[Descriptive metadata does not trigger dependency lookup](../../../crystallized/documents/cli/experience/scenarios/commands/c20-extension-create.md#c20-13)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedJsonPreservesExactResultOrderAndStdoutIsolation:87](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateProcessTests.cs) | Prompt-free JSON dry-run schema/order/paths and manifest name/description/version/dependency order with no writes. |

**Gap / qualification:** E2E JSON carries arbitrary dependencies without applying them; only Integration explicitly asserts no source lookup.

**Separate lower-tier evidence:**

- Integration [OverridesPreserveTextAndOrderDependencies:42](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/ExtensionCreateOperationIntegrationTests.cs): Create preserves nonblank metadata/dependency order without source lookup.

## C21-01

**[Single package](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedApplyAndNoOpUseExactStreamsExitsAndJson:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) | Explicit toolkit install verifies JSON schema/selection/package/verification, exact bytes, source preservation, repeat empty effects and recovery not-required. |

**Gap / qualification:** Exact installed payload and source bytes are checked. Unrelated workspace preservation and ownership are not independently enumerated; reported verification is not its own oracle.

## C21-02

**[With dependencies](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-02)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InstalledDependencyIsNotASecondMutationTarget:234](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Trusted installed dependency is disabled/untouched.

## C21-03

**[Select from source prompt](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [SelectionPromptRetriesLocallyAndConsumesNoConfirmation:10](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Install selection prompt retries IDs and applies after final confirmation.

## C21-04

**[No selection non interactive](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-04)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedUnattendedSelectionIsRequired:23](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) | Redirected/no-ID request exits invalid with selection-required finding and no stdout. |

**Gap / qualification:** Unattended omitted selection exits invalid with selection-required finding and no stdout.

## C21-05

**[Permission required non interactive](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [NonInteractiveSelectionIsInvalidAndDoesNotConsumeInput:52](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Automatic/redirected omitted selection is invalid and does not prompt.

## C21-06

**[Permission prompt always](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ExplicitAlwaysIsRememberedAndRepeatedInstallDoesNotPrompt:13](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Always grant persists and repeat does not prompt.

## C21-07

**[Permission prompt once](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [AllowOnceAppliesWithoutWritingAbsentOrMalformedSettings:53](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Once applies without writing absent/malformed settings.

## C21-08

**[Allow path flag](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ExplicitGrantPersistsAcrossCommandsAndDryRunCannotWrite:150](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Explicit grant persists across commands; dry-run cannot write.

## C21-09

**[Existing file without force](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [EligibleInitialForceIsTheOnlyApplyPrompt:140](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Force prompt and final confirmation authority are separated.

## C21-10

**[With force](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [EligibleInitialForceIsTheOnlyApplyPrompt:140](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Force prompt and final confirmation authority are separated.

## C21-11

**[Already installed](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-11)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedApplyAndNoOpUseExactStreamsExitsAndJson:47](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) | Explicit toolkit install verifies JSON schema/selection/package/verification, exact bytes, source preservation, repeat empty effects and recovery not-required. |

**Gap / qualification:** Repeat has empty effects and identical workspace/source snapshots, but the reviewed already-installed/no-op explanation is not asserted.

## C21-12

**[Changed since install](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ManagedDivergenceRemainsUpdateOwned:353](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallMutationIntegrationTests.cs): Force does not reconcile managed divergence; update owns it.

## C21-13

**[No content directory](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PayloadOutsideContentReportsAttention:22](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallContentBoundaryIntegrationTests.cs): No-content package reports content-directory attention.

## C21-14

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [DryRunDoesNotPromptForInitialForce:280](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Install dry-run reports force prerequisite without prompting.

## C21-15

**[Source unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-15)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C21-16

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-16)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [LockContentionIsNoWriteBlocked:294](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallMutationIntegrationTests.cs): Live install contention blocks every effect.

## C21-17

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-17)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [TargetChangedAfterVerifiedEffectRetainsProgressAndRecovery:33](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallMutationIntegrationTests.cs): Install later-target change retains verified progress/recovery.

## C21-18

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-18)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PromptCancellationIsInterrupted:110](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Selection cancellation is interrupted and write-free.

## C21-S03

**[Outcome 03](../../../crystallized/documents/cli/experience/scenarios/commands/c21-extension-install.md#c21-s03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InstalledDependencyIsNotASecondMutationTarget:234](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallInteractionIntegrationTests.cs): Trusted installed dependency is disabled/untouched.

## C22-01

**[Up to date](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-01)** — selection: retained; E2E coverage: **direct**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) | Source-only edit replaces target, publishes/rechecks recovery, source stays unchanged by CLI and repeat is no-op. |

**Gap / qualification:** Repeat after source update asserts up-to-date headline, empty effects and unchanged workspace/source/recovery.

## C22-02

**[Files replaced](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-02)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp:34](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionUpdateProcessTests.cs) | Source-only edit replaces target, publishes/rechecks recovery, source stays unchanged by CLI and repeat is no-op. |

**Gap / qualification:** Source-edited target bytes and retained recovery file are checked; exact replacement communication and recovery payload are not verified.

## C22-03

**[New version with new files](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ReplacesChangedAndRestoresMissingTargets:18](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateMutationIntegrationTests.cs): Update replaces changed/restores missing current targets with recovery.

## C22-04

**[Retired kept](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-04)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PruneDeletesEligibleRetiredTargetOnly:77](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateMutationIntegrationTests.cs): Prune deletes only eligible retired target and releases absent ownership.

## C22-05

**[Retired pruned](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [PruneDeletesEligibleRetiredTargetOnly:77](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateMutationIntegrationTests.cs): Prune deletes only eligible retired target and releases absent ownership.

## C22-06

**[All packages](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [AutomaticModeDoesNotBroadenOmittedSelection:140](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateSafetyIntegrationTests.cs): Automatic mode does not broaden omitted selection.

## C22-07

**[Select prompt](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InteractiveSelectionIsFrozenAcrossRevalidation:19](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateInteractionIntegrationTests.cs): Interactive update selection is frozen across revalidation.

## C22-08

**[No selection non interactive](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-08)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [AutomaticModeDoesNotBroadenOmittedSelection:140](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateSafetyIntegrationTests.cs): Automatic mode does not broaden omitted selection.

## C22-09

**[Permission required](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-09)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RevocationBlocksSelectedLifecycleWithoutReadingPermissionAsOwnership:259](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallPermissionIntegrationTests.cs): Revocation blocks lifecycle without treating permission as ownership.

## C22-10

**[Ownership unknown](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MissingSelectedSourceCoverageIsIncompleteWithoutFallback:98](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateSafetyIntegrationTests.cs): Missing selected source coverage is incomplete without fallback.

## C22-11

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [NormalDryRunPlansChangedCurrentContentWithoutMutation:63](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdatePlanningIntegrationTests.cs): Update dry-run plans changed content without mutation.

## C22-12

**[Source unreadable](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MissingSelectedSourceCoverageIsIncompleteWithoutFallback:98](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateSafetyIntegrationTests.cs): Missing selected source remains incomplete without fallback.

## C22-13

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [LockContentionBlocksEveryEffect:135](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateMutationIntegrationTests.cs): Update lock contention preserves source/workspace.

## C22-14

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-14)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RecoveryPrecedesDeletionAtTheApplicationBoundary:31](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateRecoveryOrderingIntegrationTests.cs): Recovery preparation precedes deletion.

## C22-15

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c22-extension-update.md#c22-15)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [ComposedSelectionCancellationIsInterrupted:60](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateApplicationInteractionIntegrationTests.cs): Selection cancellation is interrupted.

## C23-01

**[Single package](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-01)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DefaultRemovalPreservesUnownedContentAndRepeatsAsNoOp:54](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) | Toolkit target removed, unowned neighbor/source preserved, repeat reports no files and snapshots remain equal. |

**Gap / qualification:** Target absence, source/unowned-note preservation and removal explanation are checked; selected ownership claim removal is not independently read.

## C23-02

**[Shared file kept](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-02)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [SharedOwnerReleasePrecedesFinalOwnerDeletion:74](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemoveApplicationIntegrationTests.cs): Shared owner release precedes final-owner deletion.

## C23-03

**[Missing file released](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-03)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [MissingManagedTargetCanBeReleasedSourceIndependently:188](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemovePlanningIntegrationTests.cs): Missing managed target can release ownership independently of source.

## C23-04

**[Orphaned dependency](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-04)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RetainedDependentBlocksCompletePlan:112](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemovePlanningIntegrationTests.cs): Retained dependent blocks complete removal plan.

## C23-05

**[Dependent blocks](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-05)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RetainedDependentBlocksCompletePlan:112](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemovePlanningIntegrationTests.cs): Retained dependent blocks complete removal plan.

## C23-06

**[Select prompt](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-06)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [InteractiveSelectionIncludesDependentsAndIsFrozen:19](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemoveInteractionIntegrationTests.cs): Interactive removal selection includes dependents and freezes selection.

## C23-07

**[No selection non interactive](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-07)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C23-08

**[Not installed](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-08)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [DefaultRemovalPreservesUnownedContentAndRepeatsAsNoOp:54](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) | Toolkit target removed, unowned neighbor/source preserved, repeat reports no files and snapshots remain equal. |

**Gap / qualification:** Repeat proves no-op after removal but does not place similarly named unowned content at the former managed destination or independently assert known-absent claims.

## C23-09

**[Dry run](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-09)** — selection: retained; E2E coverage: **partial**.

| Existing published-process evidence | What its assertions establish |
| --- | --- |
| [JsonPreviewAndApplySharePlanAndDeleteChangedFinalOwner:102](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionRemoveProcessTests.cs) | Changed final-owner dry-run/apply plan parity, verified delete, retained recovery, source/unowned preservation and next cleanup. |

**Gap / qualification:** JSON preview/apply parity and changed-owner deletion are checked; complete future-tense human explanation and full settings/recovery inventory preservation are not.

## C23-10

**[Permission required](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-10)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** No owned EndToEnd method exercises this reviewed starting state and outcome; fixture/helper capability alone is not coverage.

## C23-11

**[Lock held](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-11)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [HeldWorkspaceLockBlocksEveryEffect:77](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemoveSafetyIntegrationTests.cs): Held workspace lock blocks every removal effect.

## C23-12

**[Write failed partial](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-12)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [RecoveryPrecedesDeletionAtTheApplicationBoundary:152](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemoveLockSafetyIntegrationTests.cs): Recovery preparation precedes deletion.

## C23-13

**[Cancelled](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-13)** — selection: retained; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** The cited lower-tier test is related, but no EndToEnd process test uses the reviewed starting state and outcome.

**Separate lower-tier evidence:**

- Integration [SelectionCancellationIsInterrupted:82](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemoveInteractionIntegrationTests.cs): Removal selection cancellation is interrupted.

## C23-S05

**[Outcome 05](../../../crystallized/documents/cli/experience/scenarios/commands/c23-extension-remove.md#c23-s05)** — selection: improved; E2E coverage: **none**.

No matching published-process assertion identified.

**Gap / qualification:** Missing target release is adjacent, but no unreadable required-file fixture is run through the public process.

**Separate lower-tier evidence:**

- Integration [MissingManagedTargetCanBeReleasedSourceIndependently:188](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Remove/ExtensionRemovePlanningIntegrationTests.cs): Missing managed target can release ownership independently of source.

