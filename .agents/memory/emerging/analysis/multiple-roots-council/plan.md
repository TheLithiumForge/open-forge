---
open-forge:
  description: The frozen plan for Task 87, routes outside the main root, with the layout and settings v5 API, the inventory merge, command behavior, seven waves of packets and evidence rules
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Routing, Include, Settings, Plan]
---

# Routes Outside The Main Root Plan

The CLI architect of the [multiple roots council](record.md) wrote this plan from the chair decisions and the maintainer's direction. It is frozen for [Task 87](../../../working/cli-development/tasks/task87-routes-outside-main-root.md) and starts after Task 85 Delivery 1.

## Chair amendments

These override packet size, ownership and execution order below without changing the accepted command behavior or frozen API.

1. **Reconcile and split against Delivery 1.** Checked at `a7e25a992` on 2026-10-24. Task 85 already supplies layout transport, included-row retention and the public-token projection, so packet B extends those owners rather than replacing them. The packets below divide the original broad assignments into one coherent outcome each. Run dependent packets sequentially. Only the disjoint Wave 3 pairs may run concurrently, with at most the two assigned writers. Each packet uses its milestone base plus explicitly named prerequisite patches. This keeps intermediate contracts settled and prevents shared-file conflicts. Complete suites run in hosted CI, while local runs select affected classes, report invariants, layer boundaries, affected serialization and the command's published journey.

| Wave | Packets and integration order | Ownership boundary |
| --- | --- | --- |
| 0 | A1 pure layout, directory validation and settings v5, then A2 physical observation and lease revalidation | A1 owns layout models, Workspace route paths, settings definitions/document/codec/schema, pure tests and the token projection pair. A2 owns layout observation models/readers/revalidation and their Unit and Integration tests. |
| 1 | B1 registered inventory and exact identity, then B2 topology, included edges and generated navigation | B1 owns identity/classification, catalogue requests/readers and read sessions. B2 owns included-row merge, route facts/topology, loading/filter selection and generated navigation. Preserve the existing carriers in both. |
| 2 | C1 boundary-aware scaffolding and metadata, then C2 Index and Repair | C1 owns shared route scaffolding, routed maintenance and Route source carriers. C2 owns Index and Repair selection, projection, diagnosis and tests. |
| 3 | D folder Include/Exclude alongside E1 Create, then E2 Init alongside F read commands, then E3 Update, E4 Move and E5 Remove | D owns inclusion and its presentation. Each E packet owns its command subtree and focused tests. E5 also owns top-level Remove dispatch and safety. F owns Context, Find and References. Shared maintenance remains frozen. |
| 4 | G1 route observations, List and Inspect, then G2 Status, Doctor and lifecycle | G1 owns route observations and List/Inspect presentation. G2 owns Status/Doctor, Update reconstruction and the permission audit. |
| 5 | H published qualification and final token audit | Three published journeys, their fixtures/snapshots and the token projection pair. Detailed failure branches remain in Integration. |
| 6 | I1 contracts and designs, then I2 public documentation and shipped CLI Skill | I1 owns current semantic contracts and designs. I2 owns repository guides, the docs site and CLI Skill. The coordinator owns Task state and final acceptance handoff. |

A1 also moves canonical catalogue filename construction into Workspace route paths and updates the existing classifier to reuse it. B1 and C1 consume that owner after integration. This is the direct dependency needed to keep Workspace below Sources. Every packet owns its associated focused tests and snapshots. Other shared-file edits require coordinator reassignment before parallel work resumes.

2. **Preserve primary identities and paired layers during the outside merge.** Physical collapse in B applies to outside and included sources. Existing primary inventory members keep their identities and short IDs, including physical aliases. Overwrite physical paths retain their logical base owner. New outside inventory must not take precedence over an established authored target. Keep candidates, issues and every row while merging outside physical sources. The B2-R1 regressions demonstrated these preservation boundaries under default and custom layouts. This narrows the broad deduplication step below to honor the required Delivery 1 compatibility, without changing outside command behavior or the frozen API.

3. **Carry the retained observation through reference protection.** C2 also owns the narrow layout-observation transport in `O/Shared/ReferenceMaintenance/ReferenceProtectionReader.cs`. Repair already obtains its protection policy through this shared reader, so assigning C1's additive observation there preserves the one-observation rule without rebuilding a second policy in each caller. Keep existing ownership, Library and primary behavior. Outside effect admission still requires the available boundary and physical target checks in the command owner. This changes packet ownership only and adds no permission or public contract.

4. **Keep Repair maintenance rows in effect order.** C2 also owns the narrow parity correction in `R/Repair/Shared/Selection/RepairReportSelector.cs` and its affected reviewed snapshots. The new outside preview exposed existing text maintenance rows placing metadata before created entrypoints while the native effects put those entrypoints first. The report invariant requires the same visible effect order. Reuse the existing rows, vocabulary and fields, and keep application order unchanged. This is a reporting conformance fix, not a new public contract or a broader presentation audit.

5. **Retain readonly outside inputs independently from completion.** C2 also owns the private carrier in `O/Repair/Models/Planning/RepairMaintenancePlan.cs` to retain outside read expectations. Metadata-completion snapshots do not define readonly acquisition or bounded Entries authority. Keep the added expectation array empty for existing primary paths and validate outside inputs through the command's existing maintenance verification. Preserve the C1 seed, protection policy and public result shapes. This narrow ownership addition supports the existing preservation and expected-state contract.

6. **Keep D's shared additions separate from E1.** D also owns the narrow `F/Settings/Shared/Mutation/WorkspaceSettingsChangePlanner.OutsideRoutes.cs` adapter and its focused Unit evidence. Reuse the existing settings observation, byte-change helper and outside-route codec methods rather than reproducing them in Inclusion. D alone may add the needed folder facts to the tracked public-token projection and its owner test while E1 treats that pair as read-only. H retains the later final audit. These ownership additions support the already planned folder Include/Exclude behavior and keep the parallel writers disjoint. Shared scaffolding, maintenance, layout observation and package permissions remain frozen.

7. **Correct the hosted allocation oracle without changing production.** After E1, its writer also owns the focused correction in `U/Commands/Shared/RoutedMaintenance/Metadata/RoutedMetadataCompletionTests.cs`. Build `40150084861` fails on Windows ARM64 because an exact byte-total comparison rejects membership allocating eight fewer bytes than its direct protection control. Keep a meaningful allocation-growth regression check that tolerates fixed runtime measurement variation, with semantic membership checks intact. Do not alter the production protection policy, skip a platform or tune behavior to a measured byte total. This test-only CI correction is disjoint from D and adds no product contract.

8. **Keep Exclude's common observation separate from tree traversal.** The D execution packet's phrase "without reading its content" overstated the frozen requirement below, which permits Exclude to remove a registration and Loader row without traversing the registered tree. Retain the common layout reader and its anchor observations and expected-state checks. Exclude must not scan descendants, acquire route content for operand maintenance, write content or require a missing or unavailable tree to become readable before deregistration. D-S1 is resolved by this packet clarification, with no Framework reader reassignment or public behavior change.

9. **Keep Init entrypoint grammar and correct Create continuation.** E2 also owns the narrow outside ParentMissing action in `R/Route/Create/Shared/Selection/RouteCreateReportSelector.cs`, with its focused Create presentation evidence and only the affected outside captures. Init keeps its exact canonical or existing compatibility entrypoint grammar. The outside directory reference emitted by E1 cannot satisfy that grammar, so the continuation must name the missing canonical anchor. Primary short-ID guidance stays unchanged. This corrects command composition within the accepted behavior and keeps F disjoint from Create and Init.

10. **Apply existing snapshot newline policy to folder journeys.** After E2, its writer also owns the narrow CI correction in `I/Commands/Route/{Include,Exclude}/Route{Include,Exclude}OutputSnapshotTests.Folders.cs` and their affected reviewed captures. Build `40294213320` reports Unix mismatches only in applied full/debug text. The folder journeys omit the escaped-newline handling already used by the existing Include preview while settings serialization uses platform newlines. Reuse the established command snapshot policy and retain independent byte and parsed-state evidence. Do not change production serialization, broad normalization, assertions or platform skips. This is a test-only qualification correction disjoint from F.

11. **Keep Create composable with preserved compatibility entrypoints.** After F, its writer also owns the outside root admission in `O/Route/Create/Shared/Planning/RouteCreateGeneratedNavigationPlanner.cs` and a focused compatibility-root Integration class. E2-S1 found that Init preserves an existing recognized compatibility entrypoint while Create requires the canonical filename, making the suggested continuation unable to converge for that state. Create must still require one existing recognized entrypoint at the registered directory, preserve its name and bounded Entries authority, and refuse missing or ambiguous roots. Loader membership, registration, shared maintenance and primary behavior stay unchanged. This narrow command correction implements the existing generic entrypoint and command-composition contract, with no new target grammar or automatic restoration.

12. **Classify existing Init ambiguity issues through the contracted refusal.** E2 retains the two missing `ReadCatalogueFinding` mappings for `EntrypointAmbiguous` and `EntrypointCompatibilityCollision`. These are existing catalogue values and the Init chain-selection contract already requires their refusal as `RouteAmbiguous`. The correction replaces the unhandled fallback for those invalid primary states with that existing blocked result. It grants no target admission and changes no classification or successful primary behavior. E2-R1 separately keeps the canonical compatibility-name admission exception outside-only. Focused default/custom controls distinguish the refusal correction from admission preservation.

13. **Preserve a stale canonical Loader row during bounded Create.** E2-S1-R2 found that a valid outside compatibility anchor was still blocked by the Loader row naming its missing canonical sibling. When the available registered directory has exactly one recognized anchor, the Create owner may ignore that one missing declared catalogue destination for maintenance admission. Preserve the authored row unchanged and retain all other Loader safety refusals, primary handling and missing or ambiguous root checks. The same two S1 files own this correction and its default/custom controls. This applies the existing separation between loading membership and bounded maintenance, without relisting or restoring content.

14. **Prepare the next destructive packet without a second writer.** E3 Update, E4 Move and E5 Remove retain sequential implementation and integration. While E3 runs, the other assigned worker may perform a bounded read-only E4 preflight and return an artifact with ownership, dependencies and affected tests. It may not edit production, tests, captures or context. Later command packets consume only explicitly named, hashed prerequisite patches and export only their own delta. Protect recognized compatibility entrypoints at a registered directory as anchors alongside its declared canonical catalogue. This preserves the generic-anchor behavior settled in E2 while keeping destructive effects within one selected boundary.

15. **Complete Move within its private composition owners.** E4 preflight found that preserved authored outside Entries need destination projection when their children move. E4 also owns its private destination resolver, navigation selection/reading/observation, plan builder, effect composer and existing planning carriers, with a small command-local row projector if needed. Compose authored row and generated-region intent from one original snapshot into one effect. Shared included merging and navigation policy remain frozen. Required reference replacements outside the selected outside boundary refuse the complete move unless existing authored link scope already excludes them. Existing removal exclusions remain literal and unchanged, matching current Move behavior. Only existing link-scope settings remap. This adds no removal-exclusion migration, permission bypass or public vocabulary.

16. **Use idle capacity for the bounded Remove preflight.** After the E4 read-only report, the same worker may inspect E5 while E3 remains the only writer. Return only an ignored preparation report, without implementation, builds or tests. Focus on top-level dispatch versus final path protection, retained layout authority, authored Entries removal and honest absence verification. E4 implementation still starts after E3 integration, and E5 implementation after E4 integration. This changes preparation order only.

17. **Close Remove dispatch and absence boundaries locally.** E5 also owns the final top-level Path planner guard, private reference/navigation composition and their retained boundary/row carriers. Preliminary dispatch is classification only. The final planner must reject a newly route-eligible outside target rather than fall through after registration drift. Protect registered directories, their ancestors and canonical or recognized root entrypoints even when absent. Exact outside repeat verification requires trusted physical, ownership, row, reference, projection and exclusion evidence. A missing root Path directory does not establish its former entrypoint, so refuse when complete route absence cannot be proved without guessing. Missing exclusion never permits an effect-free success. Preserve independent registrations and refuse required foreign content edits rather than deregistering or rewriting Loader implicitly. Primary absence behavior remains unchanged.

18. **Correct the isolated rename-evidence CI oracle separately.** Hosted qualification found the existing content-deadline cancellation test failing on Intel macOS. The idle second worker may investigate and correct only that Integration test and a directly required local fixture helper alongside E3. Production rename evidence, timing budgets, platform policy and other command files remain protected. Keep real filesystem coverage and deterministic cancellation/deadline evidence without skips, sleeps, retries or relaxed assertions. If the cause is a production contract violation, report it before widening ownership. This is a disjoint qualification correction, not an outside-route behavior change.

19. **Prepare reporting while Move remains the only writer.** After the E3 milestone, the idle worker may perform a bounded read-only G1 preflight while E4 runs. Map existing outside observation and List/Inspect behavior, the remaining maintenance facts, projection/token owners and affected evidence. Return only an ignored report, without code, tests, captures, context edits or execution. E4 and E5 implementation remain sequential, and reporting implementation still follows them. This changes preparation order only and leaves G2 lifecycle and permission work separate.

20. **Freeze the minimal G1 registration coordinate and consumers.** The planned optional `maintenanceRoot` is a canonical workspace-relative directory string on each applicable List row and Inspect data, at every JSON detail. Omit it entirely when no ordinal outside registration matches an already represented logical coordinate. It reports registration only, including for an existing missing-row representation, and grants no availability, loading, ownership or permission. Do not invent sources or registration-only placeholders, and preserve no-operand List selection. Human standard/full/debug may label the coordinate, while minimal text and all primary/file-only output remain unchanged. G1 also owns its private List source resolver/row carrier, Inspect resolution input/graph/source-facts/identity carriers and the narrow PublishedTokens projection/owner pair. The read-only report identifies their exact paths. Shared loading and topology stay frozen, with first-reaching policy distinct from retained row facts.

21. **Prepare G2 without overlapping implementation.** After G1 preflight, the idle worker may perform one bounded read-only G2 audit while Move remains the only writer. Cover retained outside Status/Doctor observations, lifecycle reconstruction and the explicit permission boundary, returning an ignored report only. No source, test, capture or context edits, builds or fixtures are allowed. G2 implementation still follows G1, and G1 follows E4/E5. The documentation closeout must also correct stale existing Move help that limits exact destinations to `.agents`, through its owned wording and reviewed captures, without new syntax or permission.

22. **Isolate the Git-admission qualification correction.** After E4 integration, the idle worker may investigate and correct only `RenameGitFailureIntegrationTests` and a directly required test-local fixture. Build `40327338023` reports Intel macOS Native AOT timing out before the ancestor-read assertions. Keep the real Git and filesystem boundary, positive admission and exact ancestor/leaf counts. Reuse the existing controlled clock seam when the assertion concerns admission rather than wall-clock deadlines. Production timing, cancellation, platform exclusions and all Move source stay protected. A production contract violation requires a concrete report before widening ownership. This is a separate test qualification correction within the Move milestone.

23. **Name Remove's private row and immediate-effect consumers.** E5 also owns `Models/Planning/RouteRemoveReferencePlan.cs`, which contains its reference-document carrier, and `Shared/Application/RouteRemoveEffectApplication.Effects.cs` for immediate physical and control-state admission. Existing `RouteRemovePostRemoveObserver` and `RouteRemovePostRemoveVerifier` may forward the exact retained outside absence scope to their existing verifier. These additions connect the already required one-snapshot row composition and final-boundary checks. They add no public shape, shared foundation, permission or primary absence policy. Require a path-by-path justification for each actual edit and keep all other command and shared owners frozen.

24. **Close the known lifecycle questions without another writer.** While E5 runs, the idle worker may answer only the G2 preflight's open Install transport and Loader ordering questions in a read-only follow-up. Reuse the existing report rather than auditing Status, Doctor or permissions again. Return source evidence, the accepted order requirement and the smallest existing command-private composition seams, with no implementation, tests, fixtures, builds or context edits. E5 remains the only writer and G2 implementation still follows G1.

25. **Split G2 and retain its existing order meaning.** After G1, run G2a Status/Doctor, then G2b lifecycle transport and permission controls. G2b also owns the narrow `InstallLayoutPlanner` correction that retains outside registrations and observations through existing APIs, including typed refusal of a conflicting initial main root. Existing lifecycle clauses preserve authored rows' mutual order, policy and first-reaching included facts while primary rows use their established generated ordering. They do not freeze absolute mixed-row slots. Qualify those invariants through ordinary Install, configure/form conversion and Update before claiming preservation. The three private projection consumers identified by the source-clean follow-up remain review-only unless a concrete failing case requires reassignment. Shared GeneratedNavigation stays frozen, with no new ordering policy or foundation.

26. **Run documentation beside the remaining code.** The maintainer authorized parallel tracks on 2026-10-26 after E5 was committed as `652df370a`. One writer owns G1, then G2a/G2b and H in reviewed packets. The other owns I1 contracts/designs and then I2 documentation for committed behavior, with a short gap list completed after reporting and lifecycle land. This overrides the earlier Wave 3-only concurrency and final-only documentation schedule. Files remain disjoint. The coordinator retains the Task and chair plan, the four List/Inspect contracts during G1, and Status/Doctor output contracts during G2a. The documentation writer owns the other named I documents, including the repository README, docs site and shipped CLI Skill, and may update the existing Concepts help Markdown and its directly affected reviewed captures alongside I2's existing Move help correction. I1's required runtime classes are qualified by the coordinator on the integrated fresh publication, while the prose-only packet checks source support, links, scope and both exact format commands without changing C# files. No pending G1/G2 output or lifecycle preservation claim is presented as delivered. Public fields and package permissions keep their existing owners and frozen meaning. After final review, the guardian alone prepares the beta version bump, tag, release workflow and release records.

27. **Freeze G2a's minimal registration report without inventing certainty.** After G1, Status adds optional `data.outsideRoutes` at every JSON detail, in declaration order. Each item contains canonical directory `path` and directory-observation `state`, using `available`, `missing`, `unsafe` or `unavailable`. Boolean `listed` describes a proven direct Loader row to the declared root catalogue or a recognized entrypoint at that exact directory. It is false only with complete row-absence evidence and omitted when listing is unknown, preserving known registration facts without calling an unreadable Loader unlisted. It never means activation, reachability, anchor validity, ownership or permission. Omit the collection when understood settings contain no registrations or when the registration inventory is unknown, retaining the existing typed unknown-coverage result in the latter case. Human views show a separate outside-route section only when registrations are represented. Primary and file-only output remain unchanged. Missing or unsafe outside inventory uses existing truthful context-inventory diagnosis rather than labelling the workspace or installation absent. Doctor retains its existing public finding/evidence graph and uses a narrow typed outside condition for accurate wording and grounded actions. Unlisted information never claims that no other route can load the content, and unavailable or unsafe observations supply no automatic repair authority. G2a's coordinator-owned contracts and concrete mappings are finalized at its dispatch against G1's integrated adapter. This freezes the planned collection, not an implementation or execution claim.

28. **Complete Inspect row-policy transport in its private consumers.** G1's focused probes expose included-row conditions being ignored by the file-metadata-only applicability consumer and row-authoritative KeepInMind being suppressed when file metadata is incomplete. Assign `RouteInspectSourceApplicabilityEvaluator.cs` and `RouteInspectReadingProfileBuilder.cs` to G1 for narrow consumption of retained row facts through the existing applicability and loading owners. Keep authored metadata separate and preserve first-reaching row policy, primary/native behavior and all unlisted semantics. This adds two private consumers to G1 ownership without changing a public contract or opening shared Sources, topology, loading or `RouteSourceInspector`.

29. **Update the installable CLI Skill with its repository counterpart.** I2 also owns `src/open-forge/.agents/skills/open-forge-cli/SKILL.md` and its `references/{common,discovery,packages,routes}.md`. The existing Skill maintenance contract requires these five canonical payload files and their repository counterparts to stay byte-aligned. This completes the already authorized shipped Skill documentation scope, without changing the Skill's discovery policy, Framework loading rules, package permissions or other payload files. Follow the applicable Framework instructions, check both Skill copies and verify fresh embedded-payload parity. The directly affected help-capture scope remains ten Concepts captures and the single Route Move command-help capture. Any further capture or source scope needs concrete evidence before reassignment.

30. **Keep detached outside-root guidance truthful at minimal detail.** Amendment 20 freezes existing minimal output and excludes the new maintenance-root line there. Newly supported detached outside-root profiles may nevertheless say `none (detached root)` for their absent parent instead of claiming Loader-root ancestry. This is a correction to an existing topology label for the new outside case, without another field, loading claim or permission. Primary, native and file-only output and every pre-existing capture remain unchanged. The four revised new outside text captures retain the original versions as diagnostic evidence, and the complete final gate is freshly qualified.

31. **Split the confirmed G1 review corrections by owner.** The bounded review identifies a List composition defect and an inferred workspace-entry alias defect in the neutral physical partition. One writer corrects only List included-result composition and its named List tests. The other corrects only the owned outside observation/reader transport and its operational Integration controls, importing the exact original G1 patch without metadata changes into the preserved I1/I2 tree. Structural inventory must survive Loader listing, and physical measurement must retain all logical aliases while counting one resolved file once. An unproved physical identity remains unavailable. The second correction is for G2's promised neutral adapter and claims no current Status/Doctor wire change. Shared identity, `RouteSourceInspector`, settings/layout and loading remain frozen. Merge only disjoint deltas against pinned seeds, recheck both findings and qualify the final combined candidate before committing.

32. **Close the frozen G1 documentation gap beside its private coverage correction.** After G1 integration and review, the documentation writer may update only the CLI reference, routing page and paired CLI Skill discovery references for the accepted optional maintenance-root fact. The List writer separately corrects incomplete anchor coverage within amendment 31 ownership. Their files are disjoint, and documentation acceptance still requires the corrected combined source and fresh embedded-payload parity. Status, Doctor and lifecycle claims remain gaps until G2 qualification. This advances the planned I2 gap closure without changing public behavior or release ownership.

33. **Use both writers for disjoint G2 packets after G1 acceptance.** G2a Status/Doctor and G2b lifecycle/permission qualification may start from the same guardian-prepared G1 commit. G2b's retained layout transport depends on the existing foundation, not G2a's reporting implementation. Keep Status/Doctor and the token projection pair exclusively with G2a, and Install transport, lifecycle tests and permission controls exclusively with G2b. Shared foundations and projection seams stay protected under amendment 25. Review both deltas independently, then qualify their combined behavior before H and the remaining documentation closure. This replaces G2's sequential schedule after the initial documentation track completes, without widening product scope.

34. **Close the overseer checklist and use the mechanical runner.** Before the final guardian review request, require an independent Sol high review of the whole branch's code, contracts and documentation against implementation. Check runnable help examples for any new option, run Index with reviewed scope so maintained Entries are current, verify Markdown links and anchors, and manually exercise folder Include, Create, Move, Remove, List and Status with the built CLI in a scratch workspace. H and final I1/I2 closure retain these gates without running Doctor in the development repository. Use the assigned Luna runner for exact mechanical tests, gate reruns and CI evidence, with source/publication binding and evidence-only returns. Its old checkout is not current product evidence. Keep semantic selection, design, diagnosis and acceptance with Sol and the coordinator. The guardian still owns rebase metadata and release preparation.

35. **Carry Doctor's retained outside observation through its private reader.** G2a also owns `O/Doctor/DoctorDiagnosisReader.cs` and `O/Doctor/Models/Observation/DoctorObservation.cs`. The source preflight names the reader, but the draft packet omitted these two necessary transport consumers. Reuse its one layout observation, pass the neutral retained snapshot into the existing Doctor route overload, and retain it for command-owned diagnosis. Preserve current constructors and the empty-registration path. No shared contributor, G1 carrier, new observer or public graph is assigned. The existing Status measurement projector remains reusable and protected.

36. **Forward the existing retained API through the operational contributor.** G2a also owns `F/Sources/Operational/RouteOperationalContributor.cs` and the mechanical interface updates in `U/Framework/OperationalContributors/Models/OperationalContributorCatalogueTests.cs`. Its existing Status/Doctor test implementations are already assigned. The current catalogue exposes the interface, so commands cannot reach G1's private reader without this additive forwarding seam. Preserve the three-argument methods and forward outside observation and retained-snapshot overloads to the same reader. No second observer, catalogue composition, G1 model, public field or policy change is assigned. The source-proven missing consumer unblocks the accepted one-observation contract.

37. **Reconcile the shipped-guidance guard after I2.** After its G2b export, that writer also owns `F/Distribution/Shared/Content/WorkspaceLayoutPayloadGuidance.cs` and `U/Framework/Distribution/Shared/Content/WorkspaceLayoutPayloadGuidanceTests.cs`. Build `40409549854` fails all six platforms because old outside-path anchors no longer occur in I2's layout-independent shipped references. Reconcile obsolete substitutions and their guards in the existing private owner, preserving all active anchors, exact shipped-template admission and truthful default/custom rendering. Do not restore stale outside semantics or weaken the guard to optional success. This disjoint CI-R5 correction changes no public flag, schema, permission or route behavior. Keep the completed G2b source and evidence pinned while exporting a separate two-file delta.

38. **Complete the proven listing and Doctor eligibility consumers.** G2a also owns `F/Sources/Operational/Shared/Routes/RouteOutsideRouteReader.cs`, its `I/Framework/Sources/Operational/RouteOutsideObservationIntegrationTests.cs` regression owner and `O/Doctor/Shared/Maintenance/DoctorMaintenanceReader.cs`. The neutral listing owner may report false for the selected logical Loader only when its existing retained expectation proves absence, while unavailable evidence remains unknown. Doctor passes the same retained layout observation into existing metadata eligibility and carries available registered structural roots into existing chain discovery, independently of Loader membership. Preserve primary roots, recognized compatibility anchors, unavailable and unsafe boundaries, existing ownership and C1/C2 policy. No additional observer, physical read, loading policy, public shape or shared maintenance foundation is assigned. The authored-row diagnosis correction stays in the already assigned result builder, so the private route projection remains unchanged.

39. **Reconcile the independently reproduced payload captures by owner.** Before G2a or CI-R5 integration, the fresh coordinator candidate reproduces four failing `StatusBeforeOutputSnapshotTests` cases and the one custom-guidance mismatch. I2 changed the five shipped Skill files while their dependent captures retained prior content and measurements. G2a may update only payload-derived numbers in existing captures under that Status class, with an independent source-byte and measurement calculation and complete structural diff review. CI-R5 may update only `I/Framework/Distribution/__snapshots__/WorkspaceLayoutPayloadRenderingIntegrationTests/CustomGuidanceMatchesReviewedTemplates/custom-guidance.content.txt`, reviewing every hunk against current canonical guidance and unchanged custom-root mapping. Preserve unrelated fields, wording, captures and all earlier red evidence. Neither assignment permits a blanket refresh or output-as-expectation oracle. The Luna attempt failed snapshot storage admission and is diagnostic, not reproduction. Until that runner boundary is resolved, the coordinator executes only snapshot-bearing classes that need its writable source-local artifact boundary, while Luna runs the remaining exact bound classes.

40. **Keep repository-bound execution evidence within its existing writable boundary.** The combined Luna gate reaches the existing Install Configuration terminal tests, whose transcript sink uses the compiled repository root. Five cases cannot create that artifact directory in the runner's read-only Root checkout. Run that whole class at the coordinator, preserve and exclude its failed runner attempt, retain the three fully passing bound classes and send only the unrun remainder back to Luna. The same narrow executor fallback covers source-local snapshot locks and repository-bound test-evidence sinks without changing source, expectations, platform policy, worker permissions or product behavior. Require complete class membership, exact counts and unchanged bindings when combining accepted evidence, with no duplicate or partially accepted failed class.

41. **Close qualification and documentation beside an independent branch review.** From guardian-prepared `d0ac9bc0d`, H owns the three published compositions, their new fixtures and the final token projection audit. The other writer closes remaining I1/I2 gaps, with the exact current document inventory in its packet. It also owns only the single custom-guidance capture and independently computed payload-dependent Status metric changes. The coordinator retains Task state, this plan and the four frozen Status/Doctor contracts. A fresh Sol high reviewer reads the complete branch against develop `77a24b2c0`, with source read-only and scratch evidence permitted. Initial review does not qualify later H, documentation, navigation or fixes. Require complete unchanged hashes and a final delta recheck before the guardian handoff. This parallel schedule preserves behavior, disjoint source ownership and the guardian's release responsibility.

42. **Close the independent whole-branch findings through bounded owners.** W87-R1 assigns terminating retained included-parent depth resolution and focused disconnected-cycle controls to the List owner. W87-R2 assigns Doctor's reconciliation of retained outside structural rows, preserving real missing or unreadable coverage, to the Doctor owner. W87-R3 assigns only newly introduced copied loading-policy tokens to their existing production owner or published projection. These correct accepted behavior and evidence rules without changing identities, loading policy, public fields or package authority. Preserve completed H and documentation deltas, export each correction separately and qualify the combined candidate. The independent reviewer must recheck all three findings and every later delta before acceptance.

43. **Reconcile the Repair qualification with G2's typed directory diagnosis.** Build `40436391995` reports two C2 Repair expectations that still require an incomplete global diagnosis for an unrelated missing or wrong-kind registration. Assign their semantic trace and focused correction to the Repair test owner after W87-R1. Preserve the accepted distinction between known directory facts and unavailable coverage, safe selected maintenance and actual unsafe boundaries. Do not change production or weaken expectations to match a run without a source-backed contract finding and explicit file ownership. Retain the failed CI evidence and prove exact local effects, preservation and the genuine incomplete-coverage refusal. This adds a bounded qualification packet, not another public behavior or a historical test cleanup.

44. **Retain the original maintenance observation when Repair inventories a filtered view.** CI-R6-Q1 proves that an unrelated missing registration makes discovery use a filtered layout. Repair then passes that inventory to shared maintenance, whose unchanged identity guard rejects the original retained observation and silently omits eligible metadata and missing entrypoints. Assign the command-private `RepairMaintenancePlanner.cs` and focused Repair controls to reconcile the retained discovery facts with the original observation before maintenance composition. Keep Framework inventory filtering, shared identity/authority guards, unavailable-root refusal, authored policy and lease validation unchanged. A further neutral capability requires explicit assignment. This closes the existing one-observation and affected-closure contract, without granting authority from inventory or adding public behavior.

45. **Run loading-token corrections beside Repair and List closure.** After H and W87-R2 are sealed, their writer takes W87-R3's sixteen exact existing test/fixture paths. The raw audit has twenty-five introduced loading-token lines in sixteen distinct files, correcting the initial review's file-count typo. The other writer retains CI-R6-Q1's private Repair planner/tests, then W87-R1-C1's private List builder and new cycle class. Those files are disjoint from W87-R3, including its existing List fixture consumers. Keep the completed H token pair and Doctor delta unchanged, review each export separately and qualify the final combined source. This ownership change advances the maintainer's parallelization direction without broadening the token correction or Task 88 scope.

46. **Separate retained parent facts from finite List presentation validation.** The corrected cycle projection reaches an existing result guard that assumes every emitted known parent precedes its child and that a maximum-depth entrypoint has no children. Detached cycle roots violate those assumptions while retaining valid local facts and finite canonical-row traversal. Assign `RouteListResult.cs` and `RouteListCoverageAndResultTests.cs` alongside the same R1 builder and cycle class. Permit only the established detached-root parent facts and already-emitted return edges, preserving strict descendant ordering/depth, canonical presentation order, complete child coverage, duplicate rejection and ordinary rooted-tree refusals. Validate branched cycles as well as self/two-member cases. This conforms internal validation to the accepted parent/count contract without adding fields, loading authority or another selection rule.

47. **Retain diagnosis for a missing authored outside target.** Final independent publication review W87-F1 proves that the outside-row comparison filter and the local-reference generated-span guard together suppress a missing target inside an unlisted outside catalogue. Assign the Doctor aggregation owner, its local-reference inspector and the existing outside structural Integration class to retain the existing typed missing-target warning and manual decision from the same observed occurrence. Preserve authored rows and conditions, healthy unlisted information, primary generated-navigation policy, loading membership and maintenance authority. Never suggest Index for the proven no-op or grant Repair a new effect. Cover default/custom layouts, missing leaves and catalogue targets, existing healthy and broken-parent controls, exact locations and read preservation. This is an existing diagnosis conformance correction, with no observer, wire field or public contract change. Recheck the corrected source and bound publication independently before acceptance.

48. **Require complete local managed qualification and close the stale default payload fixture.** The maintainer's 2026-10-27 direction supersedes affected-class selection in this plan and the coordinator brief. Before every commit request, run complete managed Unit and Integration suites locally on Windows x64, plus complete EndToEnd when a milestone touches public output or published journeys. Native AOT and the other five platforms remain hosted. A sandbox-blocked fixture is environment-blocked with its exact error, never passed or a product verdict. At `fbf307f92`, full coordinator Unit and Integration pass, while the two default compatibility journeys fail byte comparison after the accepted shipped Skill guidance changes. Assign only `Shared/MainRoot/Fixtures/default-layout.json` to reconcile its reviewed canonical payload bytes and any mechanically dependent ownership hashes. Preserve the schema, path set, unrelated file bytes, text/JSON expectations and assertions. Derive changes from the shipped sources and existing ownership algorithm, not captured output. The coordinator integrates and runs all required complete suites before requesting the correction commit. No production behavior or public contract changes.

## Authority and baseline

Implement frozen `council/decisions.md`, especially R2 through R5. Task 87 follows Task 85 Delivery 1. It does not implement main-root migration or introduce another maintenance command.

This plan was checked against snapshot `7c86a8ba3`. Task 85’s layout types are prerequisites defined in `.agents/memory/emerging/analysis/main-root-council/plan.md:84`, not implemented types in this snapshot. Names explicitly marked **new** below belong to Task 87.

Before dispatch, verify Delivery 1 acceptance, its layout transport, and its build-time public-token projection. Reconcile moved files against that integrated baseline. Preserve the frozen signatures. Each worker receives this entire plan and its packet.

Path abbreviations:

| Prefix | Repository path |
|---|---|
| `F` | `src/cli/framework/OpenForge.Cli.Framework/Framework` |
| `O` | `src/cli/operations/OpenForge.Cli.Operations/Commands` |
| `R` | `src/cli/rendering/OpenForge.Cli.Rendering/Presentation` |
| `T` | `src/cli/output-text/OpenForge.Cli.OutputText` |
| `U` | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests` |
| `I` | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests` |
| `E` | `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests` |
| `S` | `src/cli/tests/support/OpenForge.Cli.TestSupport` |
| `C` | `.agents/memory/crystallized/documents/cli` |

Brace lists below denote individual files, not shell commands.

## Frozen foundation API

### Layout and maintenance boundaries

Keep Task 85’s `WorkspaceLayout.Create(CliWorkspace, string)` and all primary mappings unchanged. `SourceRoots`, `SourcePath`, `CategoryDirectory`, `MapPayloadPath`, `TryGetSourceRelativePath` and `IsImplicitContentDestination` retain Delivery 1 meaning.

Add these internal types and members:

```text
WorkspaceOutsideRoute
    DirectoryPath → string
    CataloguePath → string

WorkspaceMaintenanceBoundary
    Kind → WorkspaceMaintenanceBoundaryKind
    DirectoryPath → string
    AnchorPath → string

WorkspaceMaintenanceBoundaryKind
    Primary, NativeSkills, Outside

WorkspaceLayout.WithOutsideRoutes(IEnumerable<string> directories)
    → WorkspaceLayout

WorkspaceLayout.OutsideRoutes
    → ImmutableArray<WorkspaceOutsideRoute>

WorkspaceLayout.InventoryRoots
    → ImmutableArray<string>

WorkspaceLayout.TryGetMaintenanceBoundary(
    string canonicalPath,
    out WorkspaceMaintenanceBoundary boundary)
    → bool
```

Put the new types beside `WorkspaceLayout` in `F/Workspace/Models/Layout/`. Add **new** `F/Workspace/Shared/Paths/WorkspaceRoutePath.cs`, owning `CanonicalEntrypointPath(string directoryPath)`. Classification and scaffolding must reuse this filename construction.

`InventoryRoots` combines primary `SourceRoots` and registered outside directories. It excludes file registrations in `roots`. It conveys discovery scope only.

Boundary lookup is pure lexical classification. It checks native Skills before the encompassing default primary root. Existing primary route restrictions remain in their command owners. An outside boundary ends at its registered directory and has its canonical catalogue as anchor. Ownership, permissions and physical safety remain separate checks.

Reject duplicate or overlapping registrations using portable path identity, including case aliases. Reject workspace-root, main-root, `.agents`, Git metadata and runtime overlaps in either ancestor direction. Physical observation must repeat these checks against resolved locations.

### Settings v5 and observation

Add `outsideRoutes`, an optional array of canonical workspace-relative **directory** paths. Absence means empty. Values have neither `./` nor trailing separators. Preserve declaration order without assigning precedence.

`roots` continues to contain files. Folder Include does not also insert its catalogue into `roots`.

Extend:

- `WorkspaceSettingsDefinitions` with `OutsideRoutesProperty` and v5 declarations
- `WorkspaceSettingsDocument` with `ImmutableArray<string> OutsideRoutes`
- `WorkspaceSettingsCodec` with **new** partial `WorkspaceSettingsCodec.OutsideRoutes.cs`

Freeze new codec members:

```text
AddOutsideRoute(ReadOnlyMemory<byte> existing, string directoryPath)
    → byte[]?

RemoveOutsideRoute(ReadOnlyMemory<byte> existing, string directoryPath)
    → byte[]?
```

Use existing null-for-no-change behavior. Adding the feature declares v5. Removing its last registration retains an empty array and does not downgrade. Unrelated mutations preserve existing declarations, unknown properties and property order. Publish **new** `schemas/v5/open-forge.schema.json`. Do not rewrite v4.

Extend Task 85’s `WorkspaceLayoutReader`, retaining its signature and one-observation-per-operation model. Add **new** `WorkspaceOutsideRouteRead` and `WorkspaceOutsideRouteReadState` under `F/Settings/Models/Observation/`, exposed through `WorkspaceLayoutRead.OutsideRoutes`.

Each observation carries its descriptor, resolved physical directory when available, and state `Available`, `Missing`, `Unsafe` or `Unavailable`. Append directory and ancestry expectations to existing `Expectations`. `FileExpectation.Directory` already exists at `F/Mutation/Models/Filesystem/Files/FileExpectation.cs:55`.

A missing or unsafe outside directory remains diagnosable without inventing a replacement root. Writes within that boundary require a safe observation. Exclude may remove a canonical registration and loader row without traversing the registered tree. Invalid settings syntax never creates a fallback layout.

`WorkspaceLayoutRevalidator` validates the retained observations under Task 86’s lease. Per-target mutation validation also proves physical containment within the selected boundary.

### Identity, inventory and included edges

Outside content retains exact identities, including catalogue filenames:

```text
./docs/team/_team.md
./docs/team/testing.md
```

Registration and deregistration never change these identities. Short IDs and payload mappings remain primary-only. Outside `SKILL.md` files do not establish another native Skill location.

Extend `SourceCatalogue` with ordered `IncludedRows`, using existing `SourceIncludedEntryRow`. Preserve the Task 85 constructor through an additive overload. All catalogue reconstruction must carry `Layout`, aliases and these rows.

Keep `SourceIncludedEntryReader.Extend` as the merge owner. Its existing implementation only retains alias information from rows at `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:115`. Its `seen` check at line 74 currently prevents inclusion metadata being attached to already-inventoried targets.

The corrected merge must:

1. Deduplicate source inventory by canonical and physical identity.
2. Retain every observed row independently, including unresolved and alias rows.
3. Preserve parent, description, tags, `ApplyTo`, source span and row order.
4. Traverse a linked catalogue even when inventory already contains it, with a separate traversal-visited set.
5. Preserve existing first-reaching-row semantics for effective included metadata.
6. Inspect disconnected registered catalogues without making them loading roots.
7. Preserve all these facts through prospective formation and reconstruction.

Existing `IncludedParentPath`, `IncludedEntry` and `IsIncludedCatalogue` may remain compatibility projections. They must cease to determine maintenance permission. The current proxy at `F/Sources/Routing/SourceRouteFactsResolver.cs:28` must be replaced.

Generated navigation recognizes registered anchors structurally, but loader rows alone establish listing. Existing outside loader rows retain their authored metadata and conditions. Links leaving a maintained boundary remain readable edges and confer no maintenance eligibility.

## Command behavior

| Command | Outside-route behavior |
|---|---|
| `route include <folder>` | Require an existing safe folder. Register it, list its canonical catalogue, and create that catalogue only when missing. Preserve an existing catalogue byte-for-byte. |
| `route include <file>` | Preserve current file behavior, including catalogue files. It grants no new maintenance eligibility. |
| `route exclude <folder>` | Remove the folder registration and matching loader row. Never remove content. Recognize registered folders even when missing. |
| `route exclude <file>` | Remove the file registration and row as today. An independent folder registration remains, possibly unlisted. |
| `index` | Discover registered trees. Bare execution maintains listed roots. Explicit exact selection may maintain an unlisted registered tree without relisting it. |
| `route create` | Accept exact destinations within a registered boundary. Create necessary route chains only inside it. Short destinations remain primary. |
| `route init` | Initialize or restore generic routes inside the boundary. Framework scaffolding retains primary package semantics. |
| `route update` | Update eligible outside sources through existing metadata, ownership and recovery policies. |
| `route move` | Allow moves within one outside boundary. Reject crossing boundaries or moving its directory or catalogue anchor. |
| `route remove` | Remove eligible descendants through existing protection and reference rules. Protect the registered anchor. Exclude owns deregistration. |
| `route list`, `route inspect` | Show exact identity, loading facts and maintenance registration separately. |
| `context` | Follow loader/catalogue edges, tags and conditions. Registration alone adds no required reads. |
| `find` | Search registered inventory, including unrouted candidates. Ordinary file includes still cause no directory scan. |
| `references` | Use the combined universe. Preserve Entries exclusion and opaque non-Markdown handling. |
| `status` | Report outside registrations independently from included-file registrations, listing and availability. |
| `doctor` | Diagnose missing anchors, unsafe mappings, overlap and malformed maintenance structure. Never register or relist automatically. |
| `repair` | Apply existing bounded maintenance to eligible outside sources. Never infer registration or loader membership. |
| `install`, `update` | Preserve registration and authored rows. Package mapping remains primary. No outside ownership is inferred. |
| Top-level `remove` | Dispatch registered descendants through route-aware removal where applicable. Path removal cannot bypass anchor protection. |
| Extension and Library commands | Existing explicit permission and ownership policies apply. Registration adds no implicit destination or grant. |
| `cleanup` | Runtime storage remains workspace-root `.open-forge-state`. Content roots do not alter its scope. |
| Help and command discovery | Explain folder/file Include and Exclude. No additional command or root alias is introduced. |

For a new folder loader row, retain Include’s existing metadata merge rules. When `--description` is omitted, use the generic Route Init scaffold description. Omitted tags and conditions retain existing row values, or their existing empty defaults for a new row. Row metadata never rewrites an existing catalogue.

## Execution and evidence rules

Workers load `.agents/loader.md` and applicable C# and testing instructions. They are not alone in the codebase and must preserve other workers’ changes.

Ownership includes the named production files, their named test classes and associated snapshots. Any additional shared-file edit requires reassignment before parallel work proceeds. Only Wave 3 runs packets concurrently.

Every packet returns its uncommitted diff, exact commands, discovered and executed test counts, failures, skips, limitations and divergences. Zero-test success is a failed gate. Rollback means withdrawing that packet’s diff, not resetting another worker’s tree.

### Shared sandbox build block

Every packet’s **Build in the sandbox** section invokes this complete block. Define it in that worker’s PowerShell session at its worktree root:

```powershell
function BuildInSandbox {
    $task87Root = (Get-Location).Path
    $env:APPDATA = Join-Path $task87Root 'artifacts\nuget-appdata'
    $env:OPENFORGE_DATA_HOME = Join-Path $task87Root 'artifacts\data-home'
    $env:TEMP = Join-Path $task87Root 'artifacts\tmp'
    $env:TMP = $env:TEMP

    New-Item -ItemType Directory -Force -Path @(
        (Join-Path $env:APPDATA 'NuGet'),
        $env:OPENFORGE_DATA_HOME,
        $env:TEMP
    ) | Out-Null

    dotnet restore OpenForge.Cli.slnx --configfile NuGet.Config -p:NuGetAudit=false
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }

    $env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
    dotnet build OpenForge.Cli.slnx -c Release --disable-build-servers -m:1
    if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
}
```

Focused commands below use these helpers, matching Task 85’s runner convention:

```powershell
function tu([string]$class) {
    dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build -- --filter-class "*.$class"
}
function ti([string]$class) {
    dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build -- --filter-class "*.$class"
}
function te([string]$class) {
    dotnet run --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build -- --filter-class "*.$class"
}
```

Check each exit status before continuing.

### Test ownership

Production-owned values must be referenced through their owner. This includes settings keys, filenames, environment variables, command tokens, finding codes and tag vocabulary. Scenario-specific paths belong to focused fixture owners.

Unit and Integration may reference permitted production assemblies. EndToEnd and `S` must use Task 85’s tracked build-time token projection. Extend its input for new public tokens. Do not add production assembly references or copied constants. The projection’s physical artifact names must be verified from the delivered Task 85 implementation, since R5 freezes its requirement but not its filenames.

No literal, substring or pattern assertions of user-facing prose. Exact output belongs in reviewed snapshots. Semantic JSON assertions use projected property and vocabulary owners. Parse document structure for navigation assertions.

Unit stays in memory. Integration exercises one real boundary in an owned temporary workspace. Published journeys remain few. Snapshot normalization does not prove file preservation, so retain direct byte comparisons.

## Waves and packets

### Wave 0, packet A: layout, schema and physical observation

**Own**

- Task 85’s `F/Workspace/Models/Layout/{WorkspaceLayout,WorkspaceMainRootPath}.cs`
- The new layout types and `WorkspaceRoutePath.cs` specified above
- `F/Settings/WorkspaceSettingsDefinitions.cs`
- `F/Settings/Models/Document/WorkspaceSettingsDocument.cs`
- `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs` and new `.OutsideRoutes.cs`
- Task 85’s `WorkspaceLayoutRead.cs`, `WorkspaceLayoutIssue.cs`, `WorkspaceLayoutReader.cs`, `WorkspaceLayoutRevalidator.cs`
- New outside observation types and `schemas/v5/open-forge.schema.json`

**Change**

Implement the frozen API and v5 codec. Preserve v4 and file-root semantics. Observe each registered directory, canonical anchor and physical ancestry. Distinguish missing content from invalid registration. Ensure registration does not modify ownership or implicit permissions.

**Focused tests**

```powershell
tu 'WorkspaceSettingsCodecTests'
tu 'WorkspaceSettingsRootsTests'
tu 'WorkspaceSettingsSchemaTests'
tu 'WorkspaceSettingsWriteTests'
ti 'WorkspaceSettingsReaderIntegrationTests'
```

Add **new** `U/Framework/Workspace/Models/Layout/WorkspaceOutsideRouteTests.cs` and `I/Framework/Settings/Shared/Observation/WorkspaceOutsideRouteReaderIntegrationTests.cs`, then run those classes.

**Acceptance**

Pure tests cover canonical paths, prefix lookalikes, symmetric overlaps, deterministic descriptors and unchanged primary mappings. Filesystem tests cover missing directories, physical aliases, reserved ancestors and changed ancestry under revalidation. Adding and removing the same registration is idempotent. Unrelated settings retain unknown values.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox`, which sets all required worktree-local stores, restores and builds with the exact commands above.

### Wave 1, packet B: combined source inventory and edge preservation

**Own**

Under `F/Sources`:

- `Identity/{SourceIdentity,SourceFormClassifier}.cs`
- `Models/Inventory/{SourceCatalogue,SourceCatalogueRequest,SourceLogicalSource}.cs`
- `Models/Reading/SourceIncludedEntryRead.cs`
- `Inventory/SourceCatalogueReader.cs`
- `Reading/SourceReadSessionReader.cs`
- `Shared/Includes/SourceIncludedEntryReader.cs`
- `Routing/{SourceRouteFactsResolver,SourceRouteTopologyBuilder,SourceLoaderEntriesParser,SourceLoaderDestinationParser,SourceLoaderDeclarationParser}.cs`
- `Loading/{SourceGeneratedDestinationResolver,SourceLoadingClosureResolver}.cs`
- `Selection/SourceUniverseFilterResolver.cs`
- `Models/Selection/SourceUniverseSelectorResolution.cs`

Under `F/GeneratedNavigation`:

- `GeneratedNavigationFormationBuilder.cs`
- `GeneratedNavigationFormationBuilder.{Derivation,ObservedEvidence,Aliases}.cs`
- `GeneratedNavigationRegionPlanner.cs`

**Change**

Implement the catalogue merge contract. Read sessions use `InventoryRoots` while preserving Task 85’s default-selection scope transport. Registered anchors participate in topology independently from listing.

Replace inclusion-as-read-only proxies with boundary classification. Preserve authored outside rows during projection. Do not classify an arbitrary external link as a generated child merely because its target is now inventoried.

**Focused tests**

```powershell
tu 'SourceIdentityTests'
tu 'SourceFormClassifierTests'
tu 'SourceIncludedAliasesTests'
tu 'SourceUniverseFilterResolverTests'
tu 'SourceRouteTopologyBuilderTests'
tu 'GeneratedNavigationFormationBuilderTests'
ti 'SourceCatalogueReaderIntegrationTests'
ti 'IncludedSourcesIntegrationTests'
ti 'SourceRouteFactsResolverIntegrationTests'
ti 'GeneratedNavigationProspectiveFormationIntegrationTests'
```

**Acceptance**

An included catalogue and its conditioned descendants retain identities, aliases, row order and applicability before registration, after scanning and after prospective reconstruction. Test duplicate reachability and cycles. An unlisted registration supplies inventory without activating Context. A file include never scans siblings.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before focused tests.

### Wave 2, packet C: shared maintenance, Index and Repair

**Own**

- `O/Shared/RouteScaffolding/{RouteScaffoldComposer,RouteScaffoldSourceBuilder,RouteScaffoldChainDiscovery}.cs`
- `O/Shared/RouteScaffolding/Models/Discovery/RouteScaffoldDiscovery.cs`
- `O/Shared/RoutedMaintenance/RoutedMaintenancePlanner.cs`
- `O/Shared/RoutedMaintenance/Metadata/{RoutedMetadataCompletionPolicy,RoutedMetadataDefaults}.cs`
- Its `Models/{RoutedMetadataPolicyContext,RoutedMetadataFileFacts}.cs`
- `O/Route/Shared/Models/Source/{RouteSource,RouteSourceDocument}.cs`
- `O/Route/Shared/References/RouteMarkdownCatalogueReader.{Traversal,State}.cs`
- `O/Index/Shared/Projection/IndexProjectionReader.cs`
- `O/Index/Shared/Selection/IndexSelectionResolver.cs`
- `O/Repair/Shared/Planning/{RepairTargetReader,RepairMaintenancePlanner}.cs`
- `O/Repair/Shared/Diagnosis/{RepairCatalogueReader,RepairOperationalContributorFactory}.cs`

**Change**

Use explicit boundaries in chain discovery and completion. Outside roots use generic Workspace metadata defaults, even when their directory names resemble primary categories. Stop ancestor creation at the registered anchor.

Separate structural roots from selected listed roots. Today `IndexProjectionReader.SelectMaintenanceSources` uses structural roots at line 178. Correct both selection and completion so bare Index does not mutate unlisted outside trees. Explicit selection remains possible without loader edits.

Repair shares eligibility and protection rules, without registration inference.

**Focused tests**

```powershell
tu 'RouteScaffoldCompositionTests'
tu 'RouteScaffoldChainTests'
tu 'RoutedMetadataCompletionTests'
tu 'IndexSelectionResolverTests'
ti 'IndexCompletionIntegrationTests'
ti 'IndexEntriesPreservationTests'
ti 'IndexApplicationIntegrationTests'
ti 'RepairCompletionIntegrationTests'
ti 'RepairProtectionIntegrationTests'
```

**Acceptance**

Complete missing descendants and metadata inside the selected boundary. Preserve foreign links, authored prose, ownership, removals and library projections. Explicit maintenance of an unlisted root leaves loader bytes unchanged. Second execution is a no-op.

**Snapshots**

Add outside cases to `IndexBeforeOutputSnapshotTests` and `RepairBeforeOutputSnapshotTests`.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before focused tests.

### Wave 3, packet D: folder Include and Exclude

**Own**

- `O/Route/Shared/Inclusion/Planning/{RouteInclusionTargetResolver,RouteInclusionPlanBuilder}.cs`
- `O/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs`
- `O/Route/Shared/Inclusion/Models/{RouteInclusionRequest,RouteInclusionPlan,RouteInclusionOutcome,RouteInclusionStates}.cs`
- New `Models/RouteInclusionTarget.cs`
- `O/Route/{Include/RouteIncludeDefinitions,Exclude/RouteExcludeDefinitions}.cs`
- `R/Route/Include/Models/RouteIncludeData.cs` and Exclude counterpart
- Their `Shared/Selection/*ReportSelector.cs`, `Shared/Rendering/*DataTextRenderer.cs` and `Shared/Help/*HelpSections.cs`
- `T/Route/{Include/RouteIncludeText,Exclude/RouteExcludeText}.cs`

**Change**

Replace the directory refusal at `RouteInclusionTargetResolver.cs:49` with typed file/folder resolution. Keep existing request `File` properties to avoid unnecessary binding churn.

Plan settings, loader and optional catalogue creation together. Reuse shared scaffold composition, never another command’s private planner. Add `IntendedLayout` to `RouteInclusionPlan`. Preview, lease revalidation, recovery preparation, apply and verification must agree on before and intended layouts.

Add folder-only `targetKind` and `catalogue` result facts. Existing file result fields retain meaning and shape. Folder registration flags describe the folder list.

**Focused tests**

```powershell
tu 'RouteIncludeTests'
tu 'RouteExcludeTests'
tu 'RouteIncludeJsonSerializationTests'
tu 'RouteExcludeJsonSerializationTests'
tu 'RouteInclusionRecoveryTests'
ti 'RouteIncludePlanningTests'
ti 'RouteIncludeApplicationTests'
ti 'RouteExcludePlanningTests'
ti 'RouteExcludeApplicationTests'
ti 'RouteInclusionRecoveryHistoryIntegrationTests'
```

**Acceptance**

Cover existing and missing catalogues, existing rows, explicit re-inclusion, missing registered folders, independent file registration, dry-run, stale settings, recovery-preparation failure and interrupted application. Existing catalogue bytes never change. Exclude never deletes files. Recovery includes every actual changed file.

**Snapshots**

Extend `RouteIncludeOutputSnapshotTests` and `RouteExcludeOutputSnapshotTests` with folder preview, applied, no-op and failure cases. Retain file baselines.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before focused tests.

### Wave 3, packet E: route authoring and destructive commands

**Own**

Under `O/Route`, these existing files:

- `Create/Shared/Planning/RouteCreate{TargetPlanner,TargetInspector,GeneratedNavigationPlanner}.cs`
- `Init/Shared/Planning/RouteInit{TargetPlanner,CurrentStateReader,ProspectivePlanBuilder,PlanningInspector,IntendedChainBuilder}.cs`
- `Update/Shared/Planning/RouteUpdate{SourceSelector,PlanProjector.Effects}.cs`
- `Move/Shared/Planning/RouteMove{SubjectSelector,SubjectSelector.Exposure,SubjectSelector.Boundaries,DestinationProjector,NavigationSourceProjector}.cs`
- `Move/Shared/References/RouteMoveReferenceScanner{,.Projection,.Observation,.Affectedness}.cs`
- `Move/Shared/Application/RouteMovePostMoveObserver.Destination.cs`
- `Remove/Shared/Planning/RouteRemove{SubjectSelector,SubjectSelector.Exposure,SubjectSelector.Boundaries,CategoryAbsencePlanner,PersistencePlanner}.cs`
- `Remove/Models/Planning/RouteRemoveAbsenceScope.cs`
- `Remove/Shared/Application/RouteRemove{EffectApplication.Settings,AppliedVerifier.Persistence}.cs`

Also `O/Remove/Shared/Selection/RemoveTargetResolver.cs` and `O/Remove/Shared/Planning/RemovePathSafety.cs`.

**Change**

Apply the command table through Task 85’s retained `LayoutRead`. Exact destinations use maintenance lookup. Short destinations keep primary mapping. Preserve literal outside paths in settings exclusions, reference rewrites and recovery coordinates.

Protect registered anchors through both route and path removal. Move must compare source and destination boundaries before planning effects. Readable links outside the boundary do not expand its mutation scope.

**Focused tests**

```powershell
ti 'RouteCreatePlanningIntegrationTests'
ti 'RouteCreateNestedIntegrationTests'
ti 'GenericRouteInitSafetyIntegrationTests'
ti 'RouteInitApplicationIntegrityIntegrationTests'
ti 'RouteUpdatePlanningIntegrationTests'
ti 'RouteUpdateApplicationIntegrityIntegrationTests'
ti 'RouteMovePlanningIntegrationTests'
ti 'RouteMoveRevalidationIntegrationTests'
ti 'RouteRemovePersistenceIntegrationTests'
ti 'RouteRemoveRevalidationIntegrationTests'
```

**Acceptance**

Exercise each supported outside operation, anchor rejection, cross-boundary rejection and stale physical mappings. Verify unchanged primary short-ID behavior and native Skills. Removal persistence prevents automatic resurrection without changing registration.

**Snapshots**

Add outside cases to `RouteCreateBeforeOutputSnapshotTests`, `RouteInitBeforeOutputSnapshotTests`, `RouteUpdateBeforeOutputSnapshotTests`, `RouteMoveBeforeOutputSnapshotTests` and `RouteRemoveBeforeOutputSnapshotTests`.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before focused tests.

### Wave 3, packet F: Context, Find and References

**Own**

- `O/Context/Shared/Graph/ContextGraphBuilder.cs`
- `O/Context/Shared/Selection/ContextLoadingClosureResolver.cs`
- `O/Context/Shared/Links/{ContextLinkExpander,ContextLinkedSourceReader}.cs`
- `O/Context/Models/Operation/ContextGraphModels.cs`
- `O/Find/FindOperationFactory.cs`
- `O/Find/Shared/Documents/FindLayerInspector.cs`
- `O/Find/Shared/Application/{FindSourceResolver,FindApplicabilityResolver}.cs`
- `O/Find/Shared/Selection/FindUniverseResolver.cs`
- `O/References/ReferencesOperationFactory.cs`
- `O/References/Shared/Inspection/ReferencesLayerInspector.cs`
- `O/References/Shared/Extraction/ReferencesLinkExtractor.cs`
- `O/References/Shared/Resolution/ReferencesSourceResolver.cs`

**Change**

Consume the combined catalogue and retained edges. Remove assumptions that an inventoried source cannot also have included-row policy. Keep membership, discovery and applicability separate.

**Focused tests**

```powershell
ti 'ContextApplyToIntegrationTests'
ti 'ContextAmbiguousAncestryIntegrationTests'
ti 'FindSourceUniverseIntegrationTests'
ti 'FindApplyToIntegrationTests'
ti 'ReferencesReadSessionIntegrationTests'
ti 'ReferencesApplicationIntegrationTests'
```

**Acceptance**

Compare effective reads and applicability before and after folder registration. Find discovers unrouted registered files. Context does not activate them merely because they are scanned. References retains Entries exclusion and opaque-file behavior.

**Snapshots**

Extend `ContextBeforeOutputSnapshotTests`, `FindBeforeOutputSnapshotTests` and `ReferencesBeforeOutputSnapshotTests`.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before focused tests.

### Wave 4, packet G: reporting and lifecycle integration

**Own**

- `F/Sources/Operational/Shared/Routes/{RouteObservationReader,RouteIncludedFileReader}.cs`
- `F/Sources/Operational/Models/Routes/RouteIncludedFileModels.cs`
- New sibling `RouteOutsideRouteModels.cs` and reader
- `O/Route/List/Shared/Filesystem/{RouteListInventoryRequest,RouteListInventoryReader}.cs`
- `O/Route/List/Shared/Selection/RouteListSelectionResolver{,.Loader,.LoaderIssues,.Source}.cs`
- `O/Route/List/Shared/Application/RouteListIncludedResultBuilder.cs`
- `O/Route/Inspect/Shared/Resolution/RouteInspect{Resolver,ResolutionSupport,SourceProjectionBuilder}.cs`
- `O/Route/Inspect/Shared/Profile/RouteInspectLoadingFactsBuilder{,.Entries,.Topology,.Traversal,.Paths}.cs`
- List `Models/Result/RouteListPresentationFacts.cs` and Inspect `Models/Presentation/RouteInspectJsonFacts.cs`
- `O/Status/Shared/Aggregation/StatusContextAggregator.cs`, `Models/Result/StatusContextModels.cs` and new `StatusOutsideRoutes.cs`
- `O/Doctor/Shared/Domains/WorkspaceIncludeDoctorInspector.cs`
- `O/Update/Shared/Planning/Update{PlanBuilder,IntendedStateBuilder,GeneratedNavigationPlanner}.cs`
- For List, Inspect and Status, their existing `R` data models, report selectors and data renderers, plus corresponding `T` wording owners

**Change**

Project outside registration separately from file includes. Add optional `maintenanceRoot` facts to List and Inspect and an outside-route collection to Status. Preserve existing ownership facts. Doctor reports availability and structural problems through owned finding vocabulary.

Audit Update reconstruction and package navigation for row preservation. Verify `O/Extension/Shared/Permissions/ExtensionDestinationPolicy.cs` and `O/Library/Shared/Permissions/LibraryPermissionScopePlanner.cs` still use primary implicit permissions. Those files are review-only unless the integrated baseline violates R4.

**Focused tests**

```powershell
ti 'RouteListInventorySourceFormRegressionIntegrationTests'
ti 'RouteInspectResolverSourceFormsIntegrationTests'
ti 'DoctorApplicationIntegrationTests'
ti 'DoctorRepairActionableAgreementIntegrationTests'
ti 'UpdateGeneratedNavigationPlannerIntegrationTests'
ti 'Schema3CommandIntegrationTests'
ti 'CliReportInvariantsTests'
ti 'LayerBoundaryTests'
```

**Acceptance**

Distinguish registered/listed, registered/unlisted, missing and unsafe states. Installed and permitted ownership do not arise from registration. Lifecycle updates retain folder registrations and conditioned loader rows. No Presentation-to-Framework dependency is added.

**Snapshots**

Extend `RouteListBeforeOutputSnapshotTests`, `RouteInspectBeforeOutputSnapshotTests`, `StatusBeforeOutputSnapshotTests`, `StatusOperationLifecycleSnapshotIntegrationTests`, `DoctorBeforeOutputSnapshotTests` and `UpdateBeforeOutputSnapshotTests`.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before focused tests.

### Wave 5, packet H: published qualification and token audit

**Own**

- New `E/PublishedOutsideRouteProcessTests.cs`
- `E/PublishedRouteIncludeProcessTests.cs`
- `E/PublishedRouteExcludeProcessTests.cs`
- Task 85’s tracked public-token projection input consumed by `S`
- Their reviewed snapshots
- New focused test data beside `PublishedOutsideRouteProcessTests`

**Change**

Add three published journeys:

1. Custom primary root, folder Include with missing catalogue, create and Index, then Context, Find, References and reporting.
2. Existing conditioned catalogue, registration, manual row removal, bare and explicit Index, then Exclude. Verify preservation and no reactivation.
3. Within-root move and descendant removal, with cross-root and anchor refusals and observable recovery evidence.

Keep detailed failure injection in Integration. Do not repeat every command branch here.

**Focused tests**

```powershell
te 'PublishedOutsideRouteProcessTests'
te 'PublishedRouteIncludeProcessTests'
te 'PublishedRouteExcludeProcessTests'
te 'PublishedSchema3ProcessTests'
te 'PublishedIndexProcessTests'
```

The Release build produces the development publication through `src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj:63`. Verify `PublishedExecutableTarget.Discover()` selects that fresh artifact and version. Report native qualification separately when executed.

**Acceptance**

No production references in EndToEnd or test support. No copied product tokens. Existing no-reactivation assertions comply with R5. Published tests invoke the executable, not internal composition. Capture positive execution counts and publication identity.

**Snapshots**

Add `PublishedOutsideRouteProcessTests` groups and update affected Include, Exclude and Schema3 groups. Review every changed baseline.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before published qualification.

### Wave 6, packet I: contracts, designs and documentation

**Own**

Update the following affected documents together:

- `C/contracts/route/{include,exclude,create,init,update,move,remove,list,inspect}/{interface,behavior}.md`
- `C/contracts/{index-candidate,context,find,references-candidate,status,doctor,repair,install,update,remove,cleanup}/{interface,behavior}.md`
- `C/contracts/shared/{source-references,source-universe-filters,workspace-permissions,result-coordinates}/{interface,behavior}.md`
- `C/contracts/{index-candidate,context,find}/technical-design.md`
- `C/contracts/route/update/technical-design.md`
- `C/architecture.md` and `C/layers/{framework,operations,presentation}.md`
- Task 85’s `C/technical-designs/workspace-layout.md`
- `C/technical-designs/{generated-navigation,forgiving-maintenance,mutation-and-recovery,workspace-permissions,workspace-libraries,lifecycle-provenance,embedded-payload}.md`
- `docs/{cli,extensions}.md` and `README.md`
- `src/docusaurus/docs/cli/{index,flows}.md`
- `src/docusaurus/docs/concepts/{routing,loading-and-tags,customizing,scopes}.md`
- `src/docusaurus/docs/getting-started/{grow-your-framework,greenfield-and-brownfield}.md`
- `src/docusaurus/docs/glossary.md`
- `.agents/skills/open-forge-cli/references/{routes,discovery}.md`
- New `.agents/memory/working/cli-development/tasks/task87-routes-outside-main-root.md`
- Its existing `_tasks.md`, `plan.md` and `project-control.md` navigation

**Change**

Document file versus folder inclusion, schema v5, exact identities, unlisted registrations, boundaries, command coverage, ownership and recovery. Keep primary configuration and outside registration distinct.

Record the final API, snapshot groups and execution receipts. State that older CLIs do not maintain folder registrations. Do not promise downgrade compatibility merely because their settings readers preserve unknown properties.

Do not rewrite frozen council decisions or historical Task 85 evidence.

**Focused verification**

```powershell
ti 'Schema3CommandIntegrationTests'
ti 'CliReportInvariantsTests'
te 'PublishedRouteIncludeProcessTests'
te 'PublishedRouteExcludeProcessTests'
```

Review command examples against the accepted published journeys. Check links and schema references. Do not run Doctor against the development repository.

**Acceptance**

No current document claims that all outside content is read-only or that `roots` contains folders. Every new public fact has a contract owner. Task 87 records tested scope and remaining platform limits.

#### Build in the sandbox

Codex `workspace-write` on Windows. No commits. Leave the diff uncommitted. Run `BuildInSandbox` before final verification.

## Risks and early detection

| Risk | Earliest detection |
|---|---|
| Scanning erases included conditions or changes first-parent behavior | Packet B differential merge tests before command work |
| Structural registration silently relists or maintains an unlisted tree | Packet C bare-versus-explicit Index tests |
| Folder Include edits an existing catalogue or verifies against stale settings | Packet D byte preservation and intended-layout tests |
| Directory names accidentally acquire native category or Skill semantics | Packets B and C classification/default tests |
| Physical aliases escape the registered boundary | Packet A observation tests and packet E under-lease revalidation |
| Registration becomes package permission or ownership | Packet G permission and lifecycle audit |
| Exclude removes the wrong registration kind | Packet D independent file/folder registration cases |
| Snapshot updates conceal content loss | Direct byte comparisons alongside reviewed output |
| Parallel workers alter shared foundations | Integrate A, B and C before Wave 3, then enforce disjoint ownership |
| Task 85 implementation differs from its frozen plan | Pre-dispatch carrier and token-projection verification |

No implementation, build or test execution was performed for this read-only plan.
