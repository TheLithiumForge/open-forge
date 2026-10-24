---
open-forge:
  description: The five Stage 2 peer reviews and rankings of the multiple roots council
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Routing, Include, Settings]
---

# Multiple Roots Council Reviews

Each member reviewed all five proposals, which were labelled P1 to P5 without lenses. The [record](record.md) holds the averaged rankings and the decisions.

## P1: Contract and compatibility guardian

/root - GPT-6 - high

#### Rankings

Each column ranks all five proposals independently, with 1 best.

| Proposal | MEANING | MODEL | FIT | TESTS | PLAN |
|---|---:|---:|---:|---:|---:|
| P1, my proposal | 4 | 2 | 3 | 4 | 4 |
| P2 | 1 | 4 | 1 | 3 | 1 |
| P3 | 5 | 5 | 4 | 1 | 3 |
| P4 | 2 | 1 | 2 | 2 | 2 |
| P5 | 3 | 3 | 5 | 5 | 5 |

**P2 best matches the established request.** Council 4 explicitly defined included root routes as the maintainer’s “multiple roots” idea. It separately deferred writing their navigation. P2 avoids treating that deferred capability as already requested. Its incremental plan fits the active foundation work.

**P4 gives the strongest future model.** It identifies a concrete discovery problem that the other proposals mostly describe abstractly. It also explains how the frozen layout carrier can support later additions without changing its signatures now.

**P1 is strongest on explicit JSON and settings compatibility**, but introduces foundation types before demonstrating that Delivery 1 needs them. My proposal also understates the significance of the original meaning of “multiple roots.”

**P3 provides the strongest evidence audit.** Its test-selection and literal-assertion findings check out. Its mutually exclusive registries and combined Include/maintenance operation introduce avoidable compatibility questions.

**P5 has useful operational and offline-reading details.** Its independent maintenance policy is clearer than my listed-only Index policy. However, folder registration with catalogue creation adds another effect family, and replacing `SourceRoots` elements now causes more foundation churn than necessary.

#### Errors

The central current-code claims largely check out. The following are compatibility corrections and distinctions between proposed behavior and existing behavior.

**1. P3 overstates “unchanged plain Include behavior.”**

Moving a promoted catalogue out of `roots` changes a fact consumed by ordinary Include and reporting. Today, Include reads `RegisteredBefore` directly from `settings.Document.Roots` and sets `RegisteredAfter` for an external include. Its settings codec adds the file to that array.

P3 must either retain inclusion registration independently or explicitly change the registration and result contracts. Mutual exclusion is not merely a new optional setting. It also needs a defined response when an older CLI adds the catalogue back to `roots`.

Evidence: `RouteInclusionPlanBuilder.cs:146`, `WorkspaceSettingsCodec.Roots.cs:24`.

**2. P1, P3 and P4 propose a new automatic Index selection policy.**

Their listed-only maintenance rule must not be presented as preservation of current selection. Current Index derives structural roots from inventory and selects maintenance sources beneath those roots. Loader-row preservation is a separate step.

Thus, keeping a root unlisted does not inherently mean keeping its files unmaintained. P5’s explicit maintenance registration can remain effective independently of loader membership. I now prefer that separation.

This is a proposed-policy correction, not a claim that those proposals falsely described a running secondary-root implementation.

Evidence: `GeneratedNavigationFormationBuilder.Derivation.cs:58`, `IndexProjectionReader.cs:178`, `GeneratedNavigationRegionPlanner.cs:166`.

**3. Every identity-preservation proposal needs a command-specific JSON qualification.**

The included-source model assigns an internal automatic identity of `./path`. Context nevertheless constructs included graph sources with `id: null` and passes that nullable ID into projections.

Preserving `./path` selection does not establish that every command should serialize `id` as `./path`. Promoting a source into inventory could accidentally change existing output even while its selectable path remains stable.

Evidence: `SourceIncludedEntryReader.cs:98`, `ContextGraphBuilder.cs:87`, `ContextProjectionBuilder.cs:68`.

**4. P3’s concrete qualification findings are valid.**

The frozen `*RouteInclusion*` integration filter misses the Include and Exclude command classes. It does match the shared recovery-history class, so the defect is incomplete coverage, not necessarily zero execution.

The cited literal row and prose assertions also exist. These require correction when touched under Task 85’s stricter rules.

Evidence: `plan.md:440`, `RouteIncludePlanningTests.cs:14`, `RouteIncludePlanningTests.cs:35`, `RouteInclusionRecoveryHistoryIntegrationTests.cs:22`.

#### Best combination

**Ship P2’s Delivery 1 scope, with P4’s boundary clarifications and P3’s evidence corrections.**

One configurable main root plus existing external includes satisfies the recorded meaning. Prove a nested custom loader, fixed native Skills and two external conditional catalogues together. The existing Include formatter constructs destinations with a fixed `../` prefix, so correct rebasing is a concrete requirement, not optional future preparation.

Evidence: `route-include-council/record.md:59`, `RouteInclusionPlanBuilder.cs:185`.

Keep the frozen callable API. Clarify that:

- `SourceRoots` supplies inventory locations, not loading policy or unrestricted write permission.
- Primary source mapping and package mapping remain primary-layout operations.
- Additional inventory must never widen implicit package-install permission.
- Settings observation and under-lease revalidation remain operation-wide.

These distinctions already have places in the frozen API. Typed secondary-root descriptors can arrive with their real consumer.

**If maintained secondary trees are separately accepted, use P4 and P1’s independent commands and registries.**

Use `route maintain <catalogue>` and `route unmaintain <catalogue>`. Require an existing canonical catalogue initially. Registration changes settings only. Keep `roots` as included-file registration and add `maintainedRoots` in a later settings schema. Do not remove inclusion history when maintenance starts.

Adopt **P5’s independent maintenance policy**. Default Index may maintain explicitly registered trees even when unlisted. Exclude changes reading exposure. Unmaintain stops maintenance. Neither operation silently performs the other.

Adopt **P4’s discovery requirement**. Retain catalogue edges and inherited conditions independently of inventory membership. The current reader records a row but skips constructing included-source metadata when its path is already inventoried. That makes a simple union of root inventories insufficient.

Evidence: `SourceIncludedEntryReader.cs:72`, `FindApplicabilityResolver.cs:68`.

Retain **P1’s compatibility controls**:

- No reinterpretation of existing registration fields.
- Stable literal paths and command-specific nullable IDs.
- Schema-3 envelope invariants unchanged.
- Optional command data omitted when no new fact exists.
- Direct default-layout byte comparisons, with only the frozen exceptions.
- No assertion that a settings version prevents older executables from acting.
- Existing ownership protections, one workspace lease and literal recovery paths.

The reader returns unfamiliar integer versions without rejecting them. Documentation and qualification must state the older-CLI limitation honestly. `WorkspaceSettingsCodec.cs:274`.

Use **P3’s tier allocation and corrected selections**, plus **P5’s offline reading exercise**. Resolve access to production-owned tokens for EndToEnd tests without adding runtime product dependencies. Keep generated token support narrowly scoped if needed.

Budget current work using P2’s incremental packets. Keep the later maintenance estimate separate, with P4’s ownership sequence and explicit registration, discovery, mutation, reporting and qualification gates.

#### What changed my mind

P2 and P4 corrected my framing. I had treated maintained trees as the natural completion of “multiple roots.” The recorded decision already uses that name for includes and explicitly defers catalogue maintenance.

I no longer require new location descriptors in Wave 0. The existing carrier and separate mapping methods provide enough preparation if their semantics are clear.

P4 exposed the concrete risk of losing include conditions when inventories merge. That deserves an explicit regression, not just a general identity test.

P5 changed my preferred Index policy. Independent maintenance registration should remain effective when loading membership is removed.

P3 identified actionable qualification defects I missed. Those repairs are more immediately valuable than expanding the foundation for an unaccepted feature.

All checks were static and read-only. No runtime compatibility or live Wave 0 implementation was verified.

## P2: Contrarian minimalist

/root - GPT-6 - high

#### Rankings

Each column ranks all five proposals independently, with 1 best. I favour the smallest complete delivery and assess proposed later maintenance separately.

| Proposal | MEANING | MODEL | FIT | TESTS | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 4 | 3 | 4 | 5 | 4 |
| P2 | 1 | 2 | 2 | 4 | 3 |
| P3 | 3 | 4 | 3 | 1 | 1 |
| P4 | 2 | 1 | 1 | 3 | 2 |
| P5 | 5 | 5 | 5 | 2 | 5 |

**P2** best challenges the premise. Council 4 already called included loader entries “multiple roots” and deferred maintenance outside the main root. Its weakness is incomplete qualification analysis. My proposed allowance did not account explicitly for the test-selection and symbolic-definition problems P3 found.

**P4** supplies the strongest model and fit. It preserves the frozen API, separates reading from maintenance, and identifies a concrete inventory-merging hazard. Its later feature remains optional. I would soften its assurance that future work needs no further layout plumbing.

**P3** wins tests and plan through actionable findings against the actual qualification packets. Its immediate location descriptors remain speculative. Its mutually exclusive registration arrays and promotion rules also add transitions that independent reading and maintenance registrations would avoid.

**P1** gives a coherent later maintenance design. However, it amends the foundation before demonstrating that writable secondary trees are required. Its additional rejection rules and older-binary qualification need a clearer connection to supported user journeys.

**P5** has the best offline experience exercise. Its folder operand, catalogue creation and automatic maintenance of unlisted roots create the largest new behavior surface. Those are deliberate proposals, but they require more acceptance decisions than the maintainer’s stated reuse goal establishes.

#### Errors

**R1 — Low, terminology, P1 and P4:** Calling Include itself a “read-only” operation or contract is inaccurate. It preserves included content, but plans loader and settings writes, takes the workspace lease and prepares recovery.

The implementation establishes this at:

- `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Shared/Inclusion/Planning/RouteInclusionPlanBuilder.cs:131`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:35`
- `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Shared/Inclusion/Application/RouteInclusionApplication.cs:65`

Use “Include preserves the target and changes only membership and registration.” Both proposals explain the effects correctly in their detailed sections, so this does not invalidate their designs.

I found no other material contradiction in the proposals’ shared account of current behavior. Their different registration schemas, command names and treatment of unlisted maintained trees are proposed changes, not factual errors.

Two important claims survived checking:

- **P4’s inventory warning is real.** `SourceIncludedEntryReader` seeds its seen-path set from the existing inventory, then skips those paths before attaching `IncludedParentPath` and `IncludedEntry`. Find consumes those fields for row metadata. See `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Shared/Includes/SourceIncludedEntryReader.cs:33`, `:74`, `:98`, and `src/cli/operations/OpenForge.Cli.Operations/Commands/Find/Shared/Documents/FindLayerInspector.cs:51`.
- **P3’s test-selection warning is real.** The frozen packet selects `*RouteInclusion*`, while the main classes are named `RouteIncludePlanningTests`, `RouteIncludeApplicationTests` and corresponding `RouteExclude*` classes. See `.agents/memory/emerging/analysis/main-root-council/plan.md:440`, `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Include/RouteIncludePlanningTests.cs:14`, and `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Exclude/RouteExcludePlanningTests.cs:10`.

These checks were static. I did not run tests or inspect the live foundation worker.

#### Best combination

**Ship P2’s scope, with P4’s explicit boundaries, P3’s qualification corrections and P5’s offline evidence.**

1. **Keep Delivery 1’s product scope from P2.** Deliver one configurable main root, fixed native Skills and several included files or catalogues. Add no maintained-root registry or commands. This matches the original meaning at `.agents/memory/emerging/analysis/route-include-council/record.md:59` and its explicit maintenance deferral at `:205`.

2. **Take P4’s foundation clarifications.** Keep the frozen signatures and v4 shape. Define `SourceRoots` as inventory locations, not loading membership or general write authority. Keep primary package mapping, short-ID mapping and implicit destination permission narrow. These distinctions fit the existing API and its explicit statement that membership grants no mutation permission at `.agents/memory/emerging/analysis/main-root-council/plan.md:100` and `:118`. Do not introduce location descriptors merely to prepare an unaccepted feature.

3. **Make P4’s metadata hazard an acceptance case.** Preserve catalogue edges, inherited conditions, row metadata and exact identities through reconstruction. Test the already-required main-root/native-Skill layout alongside external includes. If maintained secondary inventory is later accepted, require a separate regression proving that promotion preserves included-row policy. Adding paths to inventory is insufficient. Route topology also explicitly filters out included sources at `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Routing/SourceRouteFactsResolver.cs:26`.

4. **Adopt P3’s concrete qualification repair.** Correct packet Q’s selectors and record discovered and executed counts. Replace touched prose assertions with reviewed snapshots and structural assertions. Existing tests demonstrably inspect literal rows and explanation fragments at `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Include/RouteIncludePlanningTests.cs:35` and `:131`. Retain direct before-and-after byte comparisons because `src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/CommandOutputSnapshot.cs:24` ignores line endings.

   Resolve the owned-token boundary before claiming test compliance. TestSupport duplicates the data-home variable at `src/cli/tests/support/OpenForge.Cli.TestSupport/Isolation/TestDataHome.cs:24`, whose production owner is `src/cli/framework/OpenForge.Cli.Framework/Framework/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:15`. EndToEnd has no production assembly reference at `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj:20`. P3’s narrow build-time projection is a credible solution. Keep it limited to required public tokens and preserve independent expected outcomes.

5. **Take P5’s offline acceptance exercise.** Follow installed host pointers through a nested custom loader and two conditional included catalogues. Compare the ordered reads with independently specified expectations and Context. Repeat without the README. This proves the agent-facing result without adding runtime concepts.

6. **Keep P4’s later maintenance design as a contingent option.** If automatic catalogue upkeep becomes an accepted need, prefer independent `maintain` and `unmaintain` operations over P3’s promotion between mutually exclusive arrays. Start with an existing canonical catalogue. Do not also add P5’s catalogue-creation behavior in that first increment. Preserve `./path` identities, package permissions and one workspace lease.

I would not yet choose whether bare Index maintains unlisted secondary trees. P1, P3 and P4 gate automatic maintenance by listing. P5 does not. That disagreement shows why maintenance is a separate product decision, not a small extension already implied by “multiple roots.”

Keep the current foundation owner and existing packets. Correct qualification before their acceptance gates. Revise P2’s incremental estimate after bounding the token-sharing work and affected test cleanup. No proposal provides measured execution evidence that justifies a firmer estimate.

#### What changed my mind

P3 convinced me that acceptance needs more than the focused cases I proposed. The frozen Include/Exclude selector misses its central tests, and “use owned constants” needs an actual solution at the black-box test boundary.

P4 changed my view of the main implementation risk. I focused on accidentally expanding write authority. Inventory expansion can also discard the row relationships that supply metadata and conditions.

P1 and P4 also made separate maintenance commands more persuasive if that capability is later accepted. Reading and maintenance can be withdrawn independently, so separate operations express a useful distinction.

My scope recommendation remains unchanged. The code and original council record still support shipping one configurable main root with existing includes before committing to writable secondary roots.

## P3: Test and qualification engineer

/root - GPT-6 - high

#### Rankings

Each column ranks all five proposals independently. Lower is better.

| Proposal | MEANING | MODEL | FIT | TESTS | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 3 | 2 | 3 | 5 | 3 |
| P2 | 1 | 3 | 1 | 3 | 2 |
| P3, mine | 5 | 5 | 4 | 1 | 4 |
| P4 | 2 | 1 | 2 | 2 | 1 |
| P5 | 4 | 4 | 5 | 4 | 5 |

**P2** best establishes what the maintainer already requested and what Delivery 1 actually needs. **P4** supplies the strongest later maintenance design and identifies a concrete discovery-merging hazard.

**P3** has the strongest qualification audit, including a wrong test filter and existing prohibited assertions. Its mutually exclusive registries and early foundation changes are weaker than the alternatives.

**P1** clearly separates authority and lifecycle, but proposes broader registration restrictions and less specific evidence repair. **P5** contributes the clearest offline agent experience. Its automatic catalogue creation and maintenance of unlisted roots introduce additional behavior that needs separate justification.

#### Errors

**R1, low, terminology, P1 and P4: Include is not a read-only operation.** It preserves the included target, but applies loader and settings changes. Application acquires a lease, prepares recovery and applies file changes. Both proposals describe these effects correctly elsewhere, so this is misleading terminology rather than a broken design.

Evidence: `RouteInclusionPlanBuilder.cs:131`, `RouteInclusionApplication.cs:35`, `RouteInclusionApplication.cs:65`.

**R2, medium, compatibility, P3: Mutually exclusive registries do not preserve existing inclusion-registration semantics without additional changes.** My proposal removes a maintained catalogue from `roots` while it remains included. Today, `roots` records files registered by Include. Reporting derives registration exclusively from that list and counts loader rows outside it as hand-written. The proposed transition therefore changes observable registration meaning. It could be implemented, but my preservation claim understates that change.

Evidence: `WorkspaceSettingsDocument.cs:32`, `RouteIncludedFileReader.cs:43`, `RouteIncludedFileReader.cs:62`.

I found no further code contradictions in the key claims inspected. P5’s broader default maintenance policy is a proposed behavior change, not a factual error. P4’s claim that later work needs no further layout plumbing remains an architectural expectation, not demonstrated implementation evidence.

No builds or tests were run. The active foundation worker’s changes remain unverified.

#### Best combination

**Ship P2’s Delivery 1 scope, with P4’s boundary clarifications and P3’s qualification corrections.**

Council 4 explicitly called included loader entries “multiple roots” and deferred Index maintenance of external catalogues to a later explicit opt-in. That supports completing the existing main-root plan without treating writable secondary trees as an already accepted requirement. See `record.md:59` and `record.md:205`.

Keep the frozen factory and callable signatures. Clarify that inventory membership, loading, short-ID mapping and package permission answer different questions. The plan already separates `IsContentPath` from `IsImplicitContentDestination`, carries Layout through catalogues and plans, and introduces multiple selection scopes. New root descriptors need not interrupt Wave 0. See `plan.md:91`, `plan.md:217` and `plan.md:236`.

For a separately accepted maintenance delivery, use **P4 and P1’s independent operations**:

```text
route maintain <existing-canonical-catalogue>
route unmaintain <existing-canonical-catalogue>
```

Keep `roots` and `maintainedRoots` independent, with catalogue paths allowed in both. Exclude changes listing. Unmaintain changes maintenance authority. Preserve `./path` identities, primary package destinations and one workspace lease. Initially require an existing catalogue, reject overlapping roots and defer cross-root moves.

Prefer P4’s default Index behavior: listed maintained roots are automatic targets, while an unlisted registered root requires explicit selection. This is closer to current automatic selection, which derives maintenance scope from loader root paths. See `IndexProjectionReader.cs:178`.

The qualification package should combine these strengths:

1. **P3’s actual plan repairs.** Packet Q selects `*RouteInclusion*`, while important classes are named `RouteIncludePlanningTests`, `RouteIncludeApplicationTests` and `RouteExclude…`. Select Include, Exclude and shared inclusion scopes explicitly. Require discovered and executed counts. Existing literal row and prose assertions must become reviewed snapshots or typed structural assertions. See `plan.md:440`, `RouteIncludePlanningTests.cs:14` and `RouteIncludePlanningTests.cs:35`.

2. **P4’s decisive promotion regression.** Test a catalogue that is both included and inventoried for maintenance. The reader seeds `seen` from inventory, then skips already-seen paths before attaching `IncludedEntry` and `IncludedParentPath`. Find consumes those fields for row metadata and conditions. Merely expanding inventory can therefore bypass existing metadata handling. The reader retains a separate row record, so the test must prove that consumers preserve the relationship, not merely that a row was parsed. See `SourceIncludedEntryReader.cs:33`, `SourceIncludedEntryReader.cs:72` and `FindLayerInspector.cs:51`.

3. **P3’s strict tier allocation.** Unit covers pure mapping, settings transformations and selection policy. Integration covers real containment, observation, command effects and stale-plan refusal. EndToEnd covers a small set of complete published journeys. Do not duplicate rejection matrices at the process boundary.

4. **P5’s offline exercise.** Follow both installed host pointers through a nested main loader, native Skills and two conditional external catalogues. Compare ordered reads with independently specified expectations. Repeat without the README.

5. **P1 and P3’s preservation evidence.** Compare untouched files against independently captured bytes. Snapshot exact public wording. The existing snapshot comparer ignores line endings, so unchanged snapshots alone cannot establish byte preservation. See `CommandOutputSnapshot.cs:24`.

Retain P3’s owned-value access problem, but freeze the solution before mandating a generator. TestSupport currently repeats the production data-home variable while deliberately avoiding production references. A narrow generated projection is an option. It must not become a general framework or replace independent expected outcomes. See `TestDataHome.cs:17` and `CurrentUserDataHome.cs:15`.

Use **P2’s existing packet ownership now** and **P4’s staged implementation packets later**. Add the qualification corrections to the relevant owners rather than starting another foundation effort. Re-estimate after that bounded audit. Keep managed, Native AOT and platform receipts separate.

#### What changed my mind

- I no longer consider new location descriptors necessary before Delivery 1 proceeds. P2 and P4 show that the frozen layout transport already provides useful preparation.
- I prefer separate Maintain and Unmaintain operations over my Include modifier. They allow maintenance to stop without removing and reconstructing loading policy.
- I withdraw my mutually exclusive registration lists. Inclusion and maintenance are independent facts.
- P4 exposed a more concrete regression than my general identity concerns: inventory promotion can bypass included-source metadata attachment. That deserves explicit Integration evidence.
- I retain my test-filter, literal-assertion and snapshot-limit findings. Peer review strengthened their importance without justifying a broader EndToEnd suite.

## P4: CLI architect

/root - GPT-6 - high

#### Rankings

Ranks are within each column, with 1 best and no ties.

| Proposal | MEANING | MODEL | FIT | TESTS | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 3 | 2 | 3 | 5 | 4 |
| P2 | 1 | 3 | 1 | 3 | 1 |
| P3 | 5 | 5 | 4 | 1 | 2 |
| P4, mine | 2 | 1 | 2 | 4 | 3 |
| P5 | 4 | 4 | 5 | 2 | 5 |

**P2 best matches the demonstrated requirement.** Council 4 explicitly identifies included loader entries as the maintainer’s “multiple roots” idea. It separately defers writing navigation into included trees. P2 preserves that distinction without restarting the foundation. `record.md:59`, `record.md:205`.

**P4 has the strongest concrete model warning.** Inventory, inclusion edges and maintenance eligibility must remain distinct. The included-source reader skips already inventoried paths before assigning their included parent and row metadata. Merely broadening inventory can therefore lose information needed by consumers. `SourceIncludedEntryReader.cs:74`.

**P1 is the strongest alternative maintained-root specification.** It separates registration, loading, package permissions and recovery clearly. Its immediate foundation amendment is less justified than P2’s acceptance clarifications.

**P3 provides the best evidence audit.** Its test-selection and literal-assertion findings are real. Its product model is weaker because mutually exclusive registrations and Exclude-based demotion couple maintenance changes to loading changes.

**P5 gives the clearest user journey and offline-reading acceptance exercise.** However, replacing `SourceRoots` strings with descriptors breaks an explicitly frozen signature. Catalogue creation and automatic maintenance of unlisted roots also add decisions beyond the demonstrated need. These are proposed changes, not factual errors. `plan.md:97`.

#### Errors

**P3’s demotion recipe is incomplete.** Exclude followed by plain Include does not preserve the previous loading policy automatically. Exclude removes the row. A subsequent Include requires a description, defaults omitted tags and conditions to empty, and appends a new external row. The recipe must explicitly preserve and restore metadata and position, or use an independent maintenance operation. Otherwise demotion can change required reading, applicability and order. `RouteInclusionPlanBuilder.cs:69`, `RouteInclusionPlanBuilder.cs:178`, `include/behavior.md:31`.

**P1 and P4 use imprecise “read-only” wording for Include.** Include preserves the included content, but the operation itself mutates loader and settings, prepares recovery and acquires the workspace lease. The accurate phrase is “inclusion without maintaining the target”. Both proposals describe those effects elsewhere, so this is a terminology correction rather than a hidden design defect. `RouteInclusionPlanBuilder.cs:131`, `RouteInclusionApplication.cs:35`.

I found no other material contradiction in the proposals’ descriptions of current behavior. Their different command surfaces, registration shapes and Index defaults are design disagreements.

P3’s qualification findings are supported:

- Packet Q selects `*RouteInclusion*`, which does not match the main `RouteIncludePlanningTests` and `RouteIncludeApplicationTests` class names. `plan.md:440`, `RouteIncludePlanningTests.cs:14`, `RouteIncludeApplicationTests.cs:13`.
- Existing tests assert rendered rows and prose substrings. They cannot serve unchanged as examples for the maintainer’s new rules. `RouteIncludePlanningTests.cs:35`, `PublishedRouteIncludeProcessTests.cs:53`.
- TestSupport duplicates the production data-home variable while deliberately lacking a production dependency. That needs a concrete solution, not merely an instruction to import constants. `TestDataHome.cs:17`, `CurrentUserDataHome.cs:15`.

#### Best combination

**Ship P2’s scope now, with P3’s evidence corrections, P4’s boundary checks and P5’s offline journey.**

1. **Preserve the frozen API and settings v4. From P2 and P4.**  
   Keep one main root, fixed native Skills and external included catalogues. Do not add a maintenance registry, command or location-descriptor API in Delivery 1. The plan already transports Layout through catalogues, sessions and mutation plans, separates root-neutral syntax from membership, and preserves external identities. That is sufficient preparation for the accepted outcome. `plan.md:217`.

2. **Make the distinctions explicit in foundation acceptance. From P1 and P4.**  
   `SourceRoots` is inventory input, not loader membership or unrestricted write permission. Primary path mapping stays primary. Included files retain exact identities. Package admission remains independent. Keep the current workspace-wide runtime location. The frozen plan already states that membership grants no mutation permission. `plan.md:118`, `WorkspaceRuntimePaths.cs:7`.

3. **Prove the actual multiple-location journey. From P2 and P5.**  
   Install a nested custom main root. Include two external catalogues, with inherited conditions and similarly named descendants. Exercise Context, Find, References, Status and route inspection. Run Index and lifecycle commands while comparing external bytes and loader membership. Follow both installed host pointers without the CLI and repeat without the README. This checks the product promise without adding writable trees.

4. **Repair qualification before accepting packet evidence. From P3.**  
   Select Include, Exclude and shared inclusion scopes explicitly. Record discovered and executed counts. Replace touched prose assertions with reviewed snapshots. Keep identity, effect and preservation assertions direct. Put physical containment and stale-observation cases in Integration, with complete delivered journeys in EndToEnd. Resolve shared symbolic tokens through a small data-only build projection where necessary, without importing production behavior into EndToEnd. Its current project reference to the CLI is build-only. `OpenForge.Cli.EndToEndTests.csproj:20`.

5. **Retain secondary maintenance as a candidate design. From P1 and P4.**  
   If the maintainer later requests automatic maintenance of external catalogues, prefer separate Maintain and Unmaintain operations with independent registration. Preserve included identities and loading edges. Resolve inventory merging and route eligibility together. Current route topology excludes sources carrying `IncludedEntry`, so adding a root descriptor alone cannot implement the feature. `SourceRouteFactsResolver.cs:26`. Do not freeze this later command surface now.

Use P2’s existing packet ownership. Add P3’s qualification corrections to those packets, with one owner for the cross-project token contract. Allow approximately **3–5 incremental engineering days**, reduced by evidence already budgeted in Task 85. Keep relocation as Delivery 2. Do not include a maintained-root implementation in that estimate.

#### What changed my mind

P2 persuaded me to make deferral firmer. My P4 proposal correctly identified maintenance as a new capability, but still recommended its later implementation too readily. I would now retain it as a candidate until the maintainer identifies that workflow.

P3 showed that “follow the testing rules” was insufficient. The class filter, existing assertions and TestSupport dependency boundary require explicit corrections before implementation evidence is accepted.

I also withdraw P4’s categorical assurance that later maintenance needs no further layout plumbing. The frozen carrier is a good foundation. Static inspection cannot guarantee the eventual API once maintenance semantics are settled.

This review used static inspection only. No files were edited and no builds, tests or Doctor runs were performed.

## P5: User and agent experience designer

/root - GPT-6 - high

#### Rankings

Rank 1 is best. Each column ranks all five proposals independently.

| Proposal | MEANING | MODEL | FIT | TESTS | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 3 | 2 | 3 | 5 | 3 |
| P2 | 1 | 3 | 1 | 4 | 2 |
| P3 | 5 | 5 | 4 | 1 | 4 |
| P4 | 2 | 1 | 2 | 2 | 1 |
| P5, mine | 4 | 4 | 5 | 3 | 5 |

- **P2** best identifies the immediate requirement. It preserves the established meaning of multiple roots without adding commands or maintenance authority.
- **P4** provides the strongest optional maintenance design. It identifies a concrete loading-condition hazard and separates registration, inventory, loading and ownership. Its work packets make those boundaries reviewable.
- **P1** has a thorough model and compatibility treatment. Its immediate foundation changes are less compelling than P4’s narrower clarifications.
- **P3** provides the strongest evidence audit. Its combined Include operation and destructive demotion sequence make the user experience harder to explain and preserve.
- **P5** contributes the clearest offline-agent journey and useful Status terminology. I now consider its mandatory API changes premature. Its content-operations packet also combines too many responsibilities.

#### Errors

**P3’s demotion recipe is incomplete.** Exclude followed by plain Include does not preserve the former row automatically. Exclude removes that row. A subsequent Include requires a description, and omitted tags and conditions become empty when no existing row remains. The user must capture and resupply the complete policy. Otherwise demotion either fails or changes loading behavior. See `RouteInclusionPlanBuilder.cs:69` and `RouteInclusionPlanBuilder.cs:178`. P1 and P4’s independent Unmaintain operation avoids that problem.

**P1 overstates where included sources are excluded.** The explicit exclusion is in route-fact resolution, which filters sources carrying `IncludedEntry`. The topology builder itself excludes only loaders and overwrite companions. This is a caller boundary, not a universal topology guarantee. That distinction matters when introducing a combined inventory. See `SourceRouteFactsResolver.cs:26` and `SourceRouteTopologyBuilder.cs:71`.

I found no other material contradiction in the checked claims about current Include behavior, exact identities, tolerant settings versions or workspace runtime storage. Differences over future command names, registration shape and automatic Index selection are design choices.

Two findings affecting the rankings are supported by code:

- **P4’s inventory-merging warning is valid.** The include reader records a row, then skips constructing its included source when that path was already inventoried. Context currently attaches the linked parent and condition through the newly included sources. Simply adding directories to inventory could therefore change loading semantics. See `SourceIncludedEntryReader.cs:72` and `ContextGraphBuilder.cs:81`. This is a future integration hazard, not evidence that the current default layout already fails.
- **P3’s qualification corrections are valid.** The frozen `*RouteInclusion*` class filter misses classes named `RouteIncludePlanningTests` and `RouteIncludeApplicationTests`. Those tests also contain prohibited literal-row and prose-substring assertions. See `plan.md:440`, `RouteIncludePlanningTests.cs:14` and `RouteIncludePlanningTests.cs:35`.

Verification was static against `7c86a8ba35dde2c3d579886346c7183bc29f7fda`. No builds, tests or Doctor execution were performed.

#### Best combination

**Ship P2’s Delivery 1 scope, with P4’s boundary clarifications, P3’s evidence corrections and P5’s offline experience checks.**

Council 4 explicitly identifies included loader entries as the maintainer’s “multiple roots” idea. It separately defers writing navigation into included project folders. That supports completing and qualifying the existing model before introducing maintenance registration. See `route-include-council/record.md:59` and `record.md:205`.

For Delivery 1:

1. **Keep P2’s command surface.** Install chooses one main root. Include and Exclude expose existing files and catalogues elsewhere. Keep directory operands rejected and retain their catalogue guidance. Do not add maintenance options, commands or settings.
2. **Use P4’s narrow foundation clarification.** Preserve the frozen callable API. State that `SourceRoots` supplies primary inventory locations, primary mapping methods never search included trees, and content membership grants no general write or package-install authority. The plan already transports Layout and introduces multiple selection scopes. A new descriptor type is not necessary to establish those distinctions now. See `plan.md:97`, `plan.md:118` and `plan.md:217`.
3. **Use P5’s experience language and offline acceptance.** Explain “main context folder” and “included project catalogues”. Show actual paths and listed, unlisted or missing states. Keep one install question. An agent follows the host pointer, loader and ordinary catalogue links without consulting settings. Test the same conditional branches with and without the CLI, including the README-deleted case.
4. **Apply P3’s concrete test corrections.** Fix class selection, retain execution counts, replace literal prose assertions with reviewed snapshots, and keep preservation checks as direct byte comparisons. The snapshot comparison deliberately ignores line endings. See `CommandOutputSnapshot.cs:16`.
5. **Resolve P3’s symbolic-definition boundary narrowly.** TestSupport deliberately has no production reference, while it duplicates the production data-home variable. Fix the ownership path without weakening black-box EndToEnd tests. P3’s build-time projection is a candidate implementation, not a reason to introduce a general generator before comparing smaller options. See `CurrentUserDataHome.cs:15`, `TestDataHome.cs:19` and `architecture.md:248`.

Use P2’s existing-owner approach for immediate work. Keep its 2–3 day allowance provisional until the affected assertion and token-sharing work is counted.

**If maintained secondary trees are accepted later, use P4 as the base.** Take P1’s explicit separation of maintenance permission from package ownership and recovery. Register an existing canonical catalogue through Maintain. Unmaintain removes only that registration. Keep `roots` and `maintainedRoots` independent, preserve exact `./path` identities, and preserve loader-row policy during inventory merging.

Prefer that catalogue operand over my folder operand and automatic catalogue creation. It names the exact document that establishes the boundary and keeps registration separate from content generation.

For the new secondary-root capability, adopt P4’s listed-root default for bare Index, with explicit selection available for an unlisted registered tree. Keep current native-root behavior unchanged. Explain this selection rule directly in help.

Use P4’s separate inventory/navigation and route-maintenance packets. Add P5’s offline journey and P3’s qualification corrections to their acceptance criteria. Keep main-root relocation separate.

#### What changed my mind

P2 and P4 changed my interpretation of the request. I treated maintained folders as the necessary meaning of multiple roots. The earlier decision explicitly gives that name to included loader entries.

P4 also changed my view of the immediate architecture work. Carrying Layout and preserving authority boundaries is sufficient preparation for Delivery 1. Replacing `SourceRoots` with descriptors now would freeze optional feature choices too early.

I retain my recommendation for independent maintenance commands if that feature is accepted. I would now use catalogue operands, preserve loader policy during promotion and demotion, and make that policy preservation the decisive acceptance test.
