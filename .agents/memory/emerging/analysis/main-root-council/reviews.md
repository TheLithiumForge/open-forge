---
open-forge:
  description: The six Stage 2 peer reviews and rankings of the main root council
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Layout, Install, Settings]
---

# Main Root Council Reviews

Each member reviewed all six proposals, which were labelled P1 to P6 without lenses. The [record](record.md) holds the averaged rankings and the decisions.

## P1: User and agent experience designer

/root - GPT-6.1 - high

#### Rankings

Each column ranks all six proposals independently. **1 is best.**

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 4 | 4 | 6 | 1 | 6 |
| P2 | 3 | 1 | 5 | 3 | 3 |
| P3 | 5 | 5 | 4 | 6 | 1 |
| P4 | 6 | 2 | 2 | 5 | 2 |
| P5 | 2 | 6 | 3 | 4 | 5 |
| P6 | 1 | 3 | 1 | 2 | 4 |

P1 remains strongest on the actual installation conversation, metadata terminology, host pointers and agent instructions. Its architecture and migration analysis are less complete.

P2 provides the cleanest foundation placement and release boundary. P3 provides the strongest qualification plan. P4 contributes useful source-selection and Library analysis, although one claimed selection failure is incorrect. P5 is especially precise about public IDs, JSON compatibility and older readers, but its separate default-v3 emission policy adds unnecessary behavior. P6 identifies the most consequential omissions in the old migration plan and explains interrupted states honestly.

All six correctly reject an installer-only implementation that leaves maintenance commands assuming `.agents`.

#### Errors

For the code citations below:

- **F** = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- **O** = `src/cli/operations/OpenForge.Cli.Operations/Commands`

**P4 overstates the effect of the single default selection scope.** It says keeping one scope would omit Skills from default selections. With no selectors, the resolver calls `SelectAll`. With exclusions but no includes, it starts with every catalogue source. The scope affects unresolved candidates and diagnostic coverage, not whether already discovered Skill sources enter that source set. Multiple content scopes are still necessary, but the proposed regression should target missing candidates and findings.

Evidence: `F/Sources/Selection/SourceUniverseFilterResolver.cs:105`, `:120`, `:128`, and `F/Sources/Models/Inventory/SourceCatalogue.cs:193`, `:246`.

**P5’s same-root “ordinary no-op” wording is too broad.** Supplying the current root should mean no relocation. It cannot make the entire Install request a no-op. Configure can still change selected routes and metadata form. Preserve ordinary planning and report its actual effects.

Evidence: `O/Install/Shared/Configuration/InstallSetupResolver.cs:78`, `:102`, `:117`.

**P1 and P2 leave the Library compatibility gate incomplete.** Blocking straddling registrations does not cover every unrepresentable move. P4, P5 and P6 correctly identify the additional case. A destination is constructed from one destination root and an unchanged suffix. A suffix containing `.agents/...` cannot generally become `docs/forge/...` by changing that root. This is an omission in their proposed gate, rather than a false description of the current mapping.

Evidence: `F/Libraries/Models/Observation/LibraryMapping.cs:37`.

**P1, P3, P4, P5 and P6 need a foundation-placement correction.** Their settings-reading resolver under Workspace introduces the upward dependency P2 identifies. The architecture places Workspace below Sources and the state capabilities. Keep the pure layout there, but place settings-backed resolution and its Settings-bearing result with Settings observation.

Evidence: `.agents/memory/crystallized/documents/cli/architecture.md:445`. This is a proposed architecture conflict, not a claim that the resolver already exists.

I verified several important claims that should **not** be called errors:

- The repeated census returns 334 files.
- P5’s distinction between included-source IDs and startup null IDs matches the implementation and contract.
- P6’s metadata concern is supported. Snapshots record bytes and expectations, while staging creates a new file.
- P5’s default-v3 emission is an explicit alternative policy. I disagree with it, but it is not a mistaken report of current code.

Evidence: `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:98`, `O/Context/Shared/Graph/ContextGraphBuilder.cs:87`, `F/Mutation/Models/Filesystem/Files/FileStateSnapshot.cs:17`, `F/Mutation/Application/FileChangeApplier.Staging.cs:21`.

#### Best combination

**Use P2’s architecture and delivery boundary.** Ship fresh custom-root installation with complete maintenance support. Keep relocation as an explicit unfinished part of Task 85. An established workspace requesting another root must receive a truthful refusal until relocation ships.

Put the pure immutable layout below Sources. Resolve fixed settings once through Settings observation. Pass the layout into catalogue, reading and planning consumers. Keep runtime storage with Task 86. Preserve literal workspace paths, stable source IDs and portable package coordinates.

**Use P4’s native Skills analysis, with the selection correction above.** Native Skills must retain identity, navigation, maintenance and diagnostic coverage. A relative loader link alone does not establish those behaviors. Test discovery, selection, malformed candidates and Index separately. Do not turn Skills into ordinary foreign includes.

**Use P1’s installation experience.** Ask:

> Where should Open Forge keep its context files?  
> Context folder: `.agents`

Explain that the value is workspace-relative and that settings and selected Skills remain under `.agents`. Ask before destination-dependent route choices. Keep metadata “Root keys” clearly separate.

Explicit input supplies the answer. Preview, JSON and unattended execution never prompt. Existing configurations retain their current root. Show the actual loader and fixed locations in the review without expanding ordinary default Status output.

**Use P1’s startup treatment and P5’s retry discipline.** Render the selected loader in the existing AGENTS managed block. Keep CLAUDE as `@AGENTS.md` plus the selected loader import. Preserve surrounding user bytes. Update custom README and CLI Skill instructions so they do not send an agent back to `.agents/README.md`.

Retry commands must retain the chosen root. An agent must reach the loader and its required reads without parsing settings or having the CLI installed. Keep README removal supported.

**Use P5’s public-output precision.** Preserve existing included-source ID projections. Keep settings schema v4 separate from JSON envelope schema 3. Add layout facts only where needed, through command-owned results. Do not repurpose Install’s adoption migration rows for relocation.

I would retain the majority’s version policy: new settings declare v4, existing declarations survive ordinary rewrites, and absent `mainRoot` is not inserted. List new-file declarations, help, wizard and intentional prose changes as explicit compatibility exceptions. P5’s dual emission policy makes the feature harder to explain without protecting users from old writers.

**Use P6’s migration safety conditions and P4’s representability test.** Protect both source and destination before copying private files. Keep source protection until deletion completes. Establish the supported metadata-preservation boundary before promising lossless movement. Refuse unsupported cases rather than claiming byte equality proves complete preservation.

For Libraries, require every intended destination to remain expressible through one destination root and unchanged suffixes. Initially block source trees intersecting movable content, following P2 and P4. Already stationary native Skills sources can remain stationary. Avoid arbitrary pinned islands in the first relocation implementation.

Require a tested restoration procedure that detects subsequent user edits. Recovery bundles remain evidence, not an automatic replay engine. P6 correctly distinguishes what a running process can report from what the next invocation must discover after abrupt termination.

**Use P3’s qualification plan and P4’s packet discipline.** Each implementer owns a coherent production change, focused tests and affected presentation. Integrate complete caller changes with shared signatures. Prove Install → Context → Index before multiplying conversions, and expose the option only after the full lifecycle works.

Keep direct byte assertions alongside snapshots. The snapshot comparer deliberately ignores line endings, so snapshots alone cannot establish every byte-preservation claim (`src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/CommandOutputSnapshot.cs:16`).

Use P4’s 14–18 engineer-day first-delivery estimate as the planning baseline. Re-estimate relocation after P6’s privacy, metadata and restoration boundaries are closed. None of these estimates is measured throughput.

#### What changed my mind

I would change four parts of P1.

First, I would move settings-backed resolution out of Workspace, following P2.

Second, I would replace my straddling-only Library restriction with the representability test from P4, P5 and P6. I would initially block movable Library-source intersections rather than promise arbitrary pinned subtrees.

Third, P6 showed that privacy must hold during the move, not merely in the final layout. The old plan copies at line 339 and publishes ignore changes at line 341. That ordering can leave private destination copies unprotected after interruption. See `.agents/memory/emerging/analysis/route-include-council/main-root-plan.md:339`.

Finally, P3 and P5 made the compatibility evidence more precise. Default byte comparisons, existing public ID projections and JSON shape preservation need explicit acceptance tests.

I still support the staged delivery and P1’s “context folder” wording. This review used source inspection only. No files changed, builds or tests ran, or Doctor executed.

## P2: Contrarian minimalist

/root - GPT-6 - high

#### Rankings

Each column ranks all six proposals independently, with 1 best. I favor fewer mechanisms, complete maintenance support and evidence that exposes consequential failures.

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 5 | 4 | 6 | **1** | 4 |
| P2, mine | 3 | **1** | 5 | 4 | 3 |
| P3 | 4 | 5 | 4 | 2 | **1** |
| P4 | 6 | 2 | 2 | 5 | 2 |
| P5 | 2 | 6 | 3 | 3 | 5 |
| P6 | **1** | 3 | **1** | 6 | 6 |

- **P6 wins accuracy and open questions.** Its Library representability, copy-before-ignore and metadata findings materially change the migration boundary.
- **P2 wins design narrowly.** It keeps settings-backed resolution above the low-level layout value and rejects unnecessary migration machinery. Its Library rule needs correction.
- **P1 wins experience.** “Context folder” avoids confusion with metadata “Root keys”. It also catches the existing Minimal-versus-Essentials documentation drift.
- **P3 wins planning.** Complete caller changes, direct byte assertions and explicit executable provenance improve verification without requiring another production subsystem.
- **P4 supplies the strongest implementation map**, but overstates the selection-scope failure.
- **P5 contributes valuable compatibility precision**, but its separate default-emitted settings version adds policy that does not earn its cost.

#### Errors

Here, `F` means `src/cli/framework/OpenForge.Cli.Framework/Framework`.

**R1 — P4 overstates what the single default scope does.**

P4 says keeping one scope would omit Skills from default selections. With no selectors, the resolver calls `Catalogue.SelectAll()`. With exclusions but no includes, it starts from all catalogue sources. The scope does not remove successfully discovered Skills.

The real problem is narrower. Scope containment controls otherwise unrepresented candidates and certain diagnostics. A second scope is still needed to preserve that coverage.

Evidence: `F/Sources/Selection/SourceUniverseFilterResolver.cs:105`, `:120`, `:128`, and `F/Sources/Models/Inventory/SourceCatalogue.cs:180`, `:193`, `:246`.

**R2 — P1 and P2 give an incomplete Library migration guard.**

Blocking registrations that straddle the moved folder is insufficient. A registration wholly inside the old root can also be impossible to translate.

For destination root `.` and source suffix `.agents/guidance/a.md`, changing only the destination root cannot produce `docs/forge/guidance/a.md`. Changing the suffix would also change the source. P4, P5 and P6 correctly require representability with unchanged source suffixes.

This is an omission in my P2 recommendation, not evidence that its fresh-install design is broken.

Evidence: `F/Libraries/Models/Observation/LibraryMapping.cs:37`, `:40`, and `F/Libraries/Models/Identity/LibraryRegistration.cs:24`.

**R3 — Several proposed resolver placements conflict with the accepted dependency direction.**

P4 and P5 explicitly place a settings-reading resolver inside Workspace. P1 follows the old Workspace/Layout placement. That creates an upward dependency from a foundational capability into state observation. Keep the immutable layout there, but place settings-backed reading above it.

This is a proposed architecture defect, rather than a false description of current executable behavior. The architecture orders Workspace below Sources and state capabilities, and identifies existing reverse edges as debt.

Evidence: `.agents/memory/crystallized/documents/cli/architecture.md:447`, `:456`, `:464`.

I found no contradiction in P1’s Minimal-default claim, P3’s snapshot limitation, P5’s public included-ID distinction or P6’s metadata finding.

P5’s v3 default-emission policy is an explicit design disagreement. I would reject it, but would not label it a factual error.

#### Best combination

**Ship the first-install boundary from P2 and P4, supported by P6’s migration findings.** An existing project may receive its first Open Forge installation in an absent or empty custom directory. Every ordinary maintenance command must then work. Existing installations retain their root. A requested change refuses before effects until relocation ships. Task 85 remains open for its accepted relocation outcome.

**Use P2’s dependency placement and P4’s three-coordinate distinction.** Keep one immutable layout separate from physical workspace identity. Settings observation resolves it once. Catalogues, sessions and plans carry it. Package coordinates remain portable, workspace paths remain literal and source IDs remain stable. Renderers receive concrete command-owned facts.

Keep fixed controls and Task 86 runtime storage in their existing owners. Do not introduce arbitrary mounts, new ownership fields or a mutable global root.

**Use P2’s native Skills treatment, with P4’s scope observation corrected.** Scan the main root and native Skills as two explicit content locations. Preserve Skills identity, catalogue maintenance and loader-controlled activation. Carry both scopes where candidate and diagnostic selection needs them. Do not classify native Skills as ordinary foreign includes.

**Take P5’s compatibility precision, without its dual emission policy.** New settings declare v4. Existing declarations remain preserved during unrelated rewrites. Validate the settings read state before trusting its document, because invalid and unavailable reads currently carry synthetic defaults.

Keep the shared output envelope at schema 3. Add command-local layout fields only where their absence preserves prior meaning. Preserve the distinction between `./path` IDs for explicit included selections and null IDs for startup or ordinary linked reads.

Evidence: `F/Settings/Models/Observation/WorkspaceSettingsRead.cs:21`, `.agents/memory/crystallized/documents/cli/contracts/shared/source-references/interface.md:68`, and `.agents/memory/crystallized/documents/cli/contracts/shared/result-coordinates/interface.md:160`.

P5’s separate default-emitted v3 buys preservation of a newly created declaration while adding another version-selection rule. The explicit v4 exception used by the other proposals is smaller and easier to explain.

**Take P1’s user experience.** Ask for a “context folder” before destination-dependent choices. Preserve the current preset and metadata defaults. Explain that settings and selected Skills remain in `.agents`. Show the custom folder and loader prominently, with fixed locations in detailed output.

Render both host pointers and correct the bundled CLI Skill’s knowledge-path instructions. Keep Help workspace-independent and README optional. Describe relocation only when it exists. Do not suggest manual settings edits as a substitute.

**Take P3’s implementation discipline and P4’s bounded ownership.** Every shared signature change includes its direct callers and composition changes. My P2 table placed composition too late. Fix that before dispatch.

Each Astra high packet owns production, focused tests and local output. One owner retains shared APIs. Prove a custom Install → Context → Index slice before expanding command conversions. Keep default snapshots unchanged during structural work, then review the small declared schema, help and wizard differences.

Use direct byte assertions where exact bytes matter. The existing snapshot comparer deliberately ignores line endings, so snapshots alone cannot establish that promise.

Evidence: `src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/CommandOutputSnapshot.cs:16`.

Retain the 14–18 engineering-day estimate for the first release, then revise it after the connected slice. Do not freeze a migration estimate before its supported cases are closed.

**For later relocation, use P6’s safety sequence and P4’s restricted source policy.** Check every Library mapping for representability. Initially refuse moves that require keeping arbitrary Library source islands inside otherwise movable content.

Establish destination ignore protection before copying private files, retaining source protection until deletion. Define supported metadata preservation or refuse unsupported cases. Verify content before deleting originals. Keep historical bundles literal and provide a tested restoration procedure that stops on subsequent edits.

These conditions justify deferring migration. They do not justify building a general filesystem migration engine.

#### What changed my mind

Three findings changed my proposal.

First, **Library representability is the actual invariant**, not straddling. P4, P5 and P6 exposed a case my rule missed.

Second, **migration privacy must hold throughout copying**. The old plan creates destination files at line 339 and publishes ignore changes at line 341. P6 correctly identifies the interruption window. Final-state privacy checks are insufficient.

Third, **verified bytes do not establish preserved file metadata**. The current snapshot stores expectations and bytes, while staging creates a new file without an explicit source-mode or access-policy transfer. P6’s concern is supported by `F/Mutation/Models/Filesystem/Files/FileStateSnapshot.cs:17` and `F/Mutation/Application/FileChangeApplier.Staging.cs:114`.

P3 also persuaded me to require direct byte comparisons and integrate composition with each callable change.

I still favor first-install support before relocation. The new evidence makes that split more compelling.

This was read-only source review. No builds, tests or Doctor execution were performed.

## P3: Test and qualification engineer

/root - GPT-6 - high

#### Rankings

Each column ranks all six proposals independently, with 1 best.

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 4 | 5 | 5 | 1 | 5 |
| P2 | 2 | 1 | 6 | 2 | 4 |
| P3, mine | 6 | 4 | 4 | 5 | 1 |
| P4 | 5 | 2 | 3 | 4 | 2 |
| P5 | 3 | 6 | 2 | 6 | 6 |
| P6 | 1 | 3 | 1 | 3 | 3 |

P6 finds the most consequential migration gaps: privacy during interruption, metadata preservation and Library representability. P2 offers the cleanest dependency direction and smallest useful layout API. P1 best connects installation, help and agent startup, including the existing Minimal preset drift.

P3 remains strongest on qualification, sandbox constraints and keeping production changes with their tests. Its investigation missed important design constraints, so I rank it last for ACCURACY. P4 contributes valuable source-selection details but misstates one consequence. P5 provides excellent contract distinctions, but its separate default-v3 emission policy adds complexity, and its wave table delays qualification until after relocation despite proposing two independently deliverable PRs.

#### Errors

1. **P4 overstates what the single default selection scope does.** “Keeping one scope would omit Skills from default selections” is contradicted by the selection implementation. With no selectors, it calls `SelectAll`. With exclusions only, it starts from every catalogue source. The scope instead affects unresolved candidates and projected issues, including unavailable directories. See `SourceUniverseFilterResolver.cs:105`, `SourceCatalogue.cs:193` and `SourceCatalogue.cs:246`. Multiple scopes still matter. Tests must prove that errors and incomplete observations survive across both content locations.

2. **My P3 overstates native Skill maintenance by including “unlisted branches” without qualification.** Current traversal requires the Skill to belong to the rooted path set. Otherwise it returns no catalogue paths. That set originates from loader-rooted Skills and their topology descendants. See `IndexSkillCatalogueTraversal.cs:23` and `IndexSkillCatalogueTraversal.cs:50`. The requirement is to preserve rooted maintenance while preserving deliberate omissions, not to maintain every discovered Skill branch indiscriminately.

I would also correct the proposed Workspace-owned settings resolver in P1, P3, P4 and P5. Settings already depends on Workspace, so that placement introduces a reverse dependency. P2 avoids it. This is a design defect, rather than a false claim about existing behavior. See `WorkspaceSettingsReader.cs:7` and `architecture.md:445`.

#### Best combination

**Use P2’s foundation, with P4’s source propagation details and P5’s contract precision.** Put an immutable, pure `WorkspaceLayout` below Sources. Keep the settings-backed reader and its observation result with Settings. Resolve once per operation, carry the same layout through catalogue reconstruction and sessions, then revalidate before effects. Keep package coordinates, installed paths and source identities distinct. Preserve included sources’ existing `./path` identity representation and literal operands.

Keep two explicit content locations: the selected main root and fixed native Skills. Preserve fixed settings, ownership and Task 86 runtime locations. Do not introduce a mount system.

**Use the shared two-delivery recommendation, with P2’s complete first-install boundary.** The first delivery supports first installation into an existing project, using an absent or empty custom destination, followed by the complete maintenance lifecycle. Preserve current default-root adoption. An established installation may retain its root and undergo ordinary configuration. Changing its root refuses without effects until relocation ships. Task 85 remains open between deliveries.

**Use P3’s default-compatibility proof, with P5’s output-contract distinction.** Establish the baseline after Task 86. Compare exact file paths, bytes, ownership records, managed host regions, text and JSON. Preserve output omission and ordering. Keep JSON output schema 3 distinct from settings v4.

Retain the accepted, explicit exceptions for newly created v4 settings and intentional option, wizard and help changes. I would not adopt P5’s additional default-v3 emission policy. Preserve existing declarations and unknown settings values during unrelated rewrites.

Existing snapshots ignore line endings, so passing them cannot establish byte identity. Add direct byte comparisons for generated artifacts and preserve existing default snapshots as independent oracles. Approve snapshot changes individually and update only targeted captures, serially. See `CommandOutputSnapshot.cs:24`.

**Use P1’s installation and startup experience.** Ask “Where should Open Forge keep its context files?” with `.agents` as the default. Explain that settings and native Skills stay in `.agents`. Keep this separate from metadata “Root keys”. Preserve actual preset behavior, including interactive Minimal, verified at `InstallSetupResolver.cs:63`.

Update managed `AGENTS.md` and `CLAUDE.md` regions, native CLI Skill instructions, help and site documentation together. Qualify startup without the CLI and Help fallback without the installed README. Report mismatches without silently selecting another root. Document that older CLIs may ignore `mainRoot` or reject newly permitted known-key values. Do not promise enforced downgrade protection.

**Use P6’s migration safety and the Library constraint identified by P4, P5 and P6.** Protect both locations before copying private content. Verify bytes and the supported metadata contract before deleting originals. Test interruption at every publication boundary, including recovery when users subsequently edit files.

Current staging writes a new file and checks bytes. It does not establish preservation of executable modes or access restrictions. See `FileChangeApplier.Staging.cs:21`. Preserve supported metadata explicitly or refuse cases that cannot satisfy the contract.

Check representability for **every** affected Library registration. An entirely moving registration can fail because its source-relative suffix remains fixed. See `LibraryMapping.cs:37`. Pin registered source trees and block ambiguous overlaps. Preserve authored ignore bytes, literal permission grants, unlisted registrations and historical recovery paths. Do not add arbitrary projection maps or a replay engine.

**Use P3’s ownership and qualification discipline, with P2’s smaller foundation packets.**

| Wave | Ownership and exit evidence |
|---|---|
| 0 | One foundation owner handles layout, settings and validation. Freeze the API and default baseline. |
| 1 | Source packets integrate sequentially where signatures overlap. Each includes its complete caller changes and focused tests. Cover identity, includes, aliases, both selection scopes and Skill maintenance. |
| 2 | Separate navigation and payload packets. Then command-family packets own production, rendering, tests and snapshots together. |
| 3 | Complete lifecycle and observation consumers. Activate the install option last. Finish documentation and qualify the first delivery before relocation begins. |
| 4 | Relocation planning, then application and recovery. Keep shared Install ownership sequential. Qualify interruption and round trips separately. |

A sandboxed worker can run focused managed tests when dependencies are prepared and all required writes remain permitted. Redirect temporary storage into the worktree before starting tests. Even test-process initialization creates temporary directories, as shown by `TestDataHome.cs:94`. End-to-end execution requires a freshly built development executable. Hand off unavailable permissions, link capabilities or native toolchains as explicit qualification gaps.

Every integration wave needs the required managed and host-native gates, with nonzero executed counts and classified skips. Final acceptance must execute custom-root journeys on Windows, Linux and macOS, across the six supported RIDs. The six `test:built` modes run on **one host**, not six platforms. See `scripts/delivery/README.md:77` and `.github/workflows/build.yml:75`.

Use P4’s **20–27 engineering-day** combined estimate as the planning range. Do not reduce it merely because migration becomes a second delivery.

#### What changed my mind

P2 convinced me that my settings-backed resolver belonged above the pure Workspace model. P6 convinced me that privacy must hold throughout migration, and that byte verification alone cannot prove preservation. The Library implementation disproved my implicit assumption that only straddling registrations need special attention.

P4 exposed another necessary regression test, but code inspection changed its purpose: preserve unresolved candidates and diagnostics across both roots.

This review is based on read-only inspection. No builds or tests were run.

## P4: CLI architect

/root - GPT-6 - high

#### Rankings

Each column ranks all six proposals independently, with 1 best.

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 5 | 6 | 6 | 1 | 6 |
| P2 | 1 | 1 | 5 | 4 | 2 |
| P3 | 4 | 4 | 4 | 5 | 1 |
| P4, mine | 6 | 2 | 2 | 6 | 3 |
| P5 | 2 | 3 | 3 | 2 | 5 |
| P6 | 3 | 5 | 1 | 3 | 4 |

P2 finds the cleanest dependency boundary. P5 is most precise about public identity and output contracts. P6 supplies the strongest migration safety analysis. P1 gives the clearest user experience. P3 has the most credible integration and verification sequence.

P4 remains useful for identifying source consumers and Library constraints, but its selection-scope claim is wrong. Its resolver placement also needs correction.

#### Errors

Paths below use these repository-relative prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `C` = `.agents/memory/crystallized/documents/cli`

**P4 incorrectly says one default selection scope would omit Skills from default selections.** With no selectors, `SourceUniverseFilterResolver` calls `SelectAll`. With exclusions only, it starts from every catalogue source and removes excluded sources. Selection scopes govern unresolved candidates and scoped diagnostics, rather than filtering healthy sources already selected.

Evidence: `F/Sources/Selection/SourceUniverseFilterResolver.cs:105`, `:120`, `:128`, and `F/Sources/Models/Inventory/SourceCatalogue.cs:193`, `:246`.

The plural-scope change remains justified for complete diagnostic coverage across the main root and native Skills. Its acceptance tests must exercise invalid candidates and unavailable directories, rather than claim healthy Skills disappear.

**P1, P3, P4, P5 and P6 place settings-backed resolution inside Workspace.** This is a proposed dependency conflict, rather than a false description of existing behavior. Workspace belongs below Sources and the state/effect capabilities. A resolver there that returns `WorkspaceSettingsRead` also imports a settings observation containing a mutation snapshot. P2 correctly separates the pure layout value from settings-backed observation.

Evidence: `C/architecture.md:447`, `:464`, `F/Settings/Models/Observation/WorkspaceSettingsRead.cs:28`, `:34`, and `F/Settings/Shared/Observation/WorkspaceSettingsReader.cs:18`.

I would not classify P5’s default-v3 emission policy or the proposals’ differing Library restrictions as factual errors. They are deliberate design choices.

#### Best combination

**Ship P2’s dependency boundary, P4’s coordinate separation, and P3’s delivery discipline.**

First deliver custom-root installation with complete subsequent maintenance. Require a missing or empty custom destination, as P1 and P2 recommend. Do not expose the choice while Update, Remove, Repair, Cleanup, navigation, routes, Extensions or Libraries still assume the default layout. Existing-workspace relocation follows in a second PR. Task 85 remains open until that accepted outcome ships.

Keep one immutable `WorkspaceLayout` in `F/Workspace/Models/Layout/WorkspaceLayout.cs`. Put `WorkspaceLayoutReader` and its settings-backed observation under Settings. Resolve once for an operation and carry the observed layout through its existing plan. Do not make Sources read Settings or change `CliWorkspace` identity.

The narrow seam has three connected parts:

- **Identity and discovery:** `SourceIdentity.cs`, `SourceLogicalPath.cs`, `SourceWorkspaceRelativePath.cs`, `SourceLogicalIdentity.cs`, `SourceCatalogueRequest.cs`, `SourceCatalogueReader.cs`, `SourceReadSessionReader.cs`, `SourceReadSession.cs` and `SourceIncludedEntryReader.cs`. Extend default diagnostic scopes through `SourceUniverseFilterRequest.cs` and `SourceUniverseFilterResolver.cs`.
- **Navigation:** `SourceLoaderDestinationParser.cs`, `SourceGeneratedDestinationResolver.cs`, `GeneratedNavigationFormationBuilder.cs` and `O/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs`. Native Skills remain managed sources under `.agents/skills`, with stable IDs and catalogue maintenance.
- **Distribution:** `FrameworkPayloadAsset.cs`, `EmbeddedFrameworkSourceProjector.cs` and `WorkspacePayloadRenderer.cs`. Keep embedded package coordinates distinct from installed destinations. Help continues reading formats from the embedded README.

These files establish the seam. They are not the complete edit list. The existing scan and included-catalogue extension are visible at `F/Sources/Reading/SourceReadSessionReader.cs:35` and `:55`.

**Keep P4’s three coordinate systems and P5’s public identity precision.**

Workspace paths remain literal. Package paths remain portable. Short source IDs derive from the selected content locations. `.agents/...` exact operands never redirect.

Included files retain their existing `./path` identities in explicit Context selections, Find, References and Route List. Startup and ordinary linked reads retain their existing null-ID behavior. This is already documented and implemented, rather than a new output design. See `C/contracts/shared/source-references/interface.md:68` and `O/Context/Shared/Selection/ContextRequestedSourceResolver.cs:73`.

Route Include/Exclude must share the converted inclusion planner and target resolver. Route Create, Init and Move must consume layout-aware destinations. Extension mapping must happen before exclusions, permissions, collisions and ownership publication. Library sources and registrations remain literal. Relevant entry points include:

- `O/Route/Shared/Inclusion/Planning/RouteInclusionPlanBuilder.cs:49`
- `O/Route/Shared/Inclusion/Planning/RouteInclusionTargetResolver.cs:55`
- `O/Extension/Install/Shared/Planning/ExtensionInstallPayloadNormalizer.cs:45`
- `O/Library/Shared/Planning/LibraryConsumerBoundaryReader.cs:26`
- `O/Library/Shared/Planning/LibraryDestinationPolicy.cs:67`

**Use P2’s settings policy with P5’s failure handling and output safeguards.**

Publish settings v4. Absence of `mainRoot` means `.agents`. Reject malformed values, reserved locations, physical escapes and conflicting Skills identities. Preserve existing declarations and unknown values on ordinary rewrites. Treat newly created v4 settings, help and the wizard question as explicit default-compatibility exceptions.

I would not adopt P5’s separate default-v3 emission policy. The existing writer already distinguishes new declarations from preserved authored declarations at `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:94`.

Invalid settings must not silently select the reader’s synthetic default document. Missing selected content must produce a layout diagnosis, with no fallback to another loader. Keep JSON output schema 3 and existing field semantics. Do not repurpose Install’s adoption `migrations` rows for relocation.

**Use P4’s Library representability test and conservative source boundary.**

For every recorded suffix, the intended destination must remain expressible through one destination root. Checking only whether a registration straddles the move is insufficient. This follows directly from `F/Libraries/Models/Observation/LibraryMapping.cs:37`.

Initially block relocation when Library sources intersect movable content. Leave already stationary sources alone. Do not add pinned islands, per-file mappings or a new ownership schema to this feature.

**Adopt P6’s continuous privacy requirement and P3’s verification method.**

Protect both source and destination before copying private content. Retain source protection until deletion completes. The old plan explicitly copies first and publishes ignore changes afterward at `.agents/memory/emerging/analysis/route-include-council/main-root-plan.md:339` and `:341`.

Verify nested ignores and negations using Git in isolated fixtures. Preserve authored ignore text outside managed sections.

Define supported metadata preservation before relocation ships. Current byte snapshots and staging do not establish preservation of executable modes or access restrictions. See `F/Mutation/Models/Filesystem/Files/FileStateSnapshot.cs:7` and `F/Mutation/Application/FileChangeApplier.Staging.cs:114`. Refuse unsupported cases before effects. Do not promise general filesystem migration.

Consume Task 86’s fixed runtime paths, local and legacy recovery observations, and Install-specific recovery boundary unchanged. No replay journal or automatic rollback.

**Use P1’s wording and offline startup, supplemented by P5 and P6.**

Ask “Where should Open Forge keep its context files?” Explain that Skills and settings stay under `.agents`. Ask before route choices. Configure defaults to the current location. Preview, JSON and unattended modes never prompt.

Render the selected loader in `AGENTS.md`. Preserve both Claude imports, including `@AGENTS.md`, while changing its loader import. The current two-import bridge is at `src/open-forge/CLAUDE.md:3`.

Custom Status should identify the context folder and loader. Keep fixed controls and runtime details available without expanding ordinary default output. Update the README, CLI Skill references, contracts and docs site together. Interrupted relocation guidance must describe observed residuals and recovery evidence without implying an offline agent can repair mixed state.

**Use P3’s cohesive packets and raise the migration allowance.**

Freeze the foundation after Task 86. Each shared-signature packet owns its direct callers, composition changes and focused tests. Establish one complete custom-install journey before parallel command-family work. Keep local rendering and contract changes with the behavior owner.

Qualify default bytes separately from snapshots that normalize line endings. Cover no-reactivation, included aliases, diagnostic scopes, Framework lifecycle, Extension receipts, Library mappings, privacy and interruption. Published-process evidence must identify the executable and actual platform coverage.

Budget **14–18 engineer-days for complete custom-root operation, then 7–10 for relocation**, including documentation and qualification. That is **21–28 engineer-days after Task 86**. The 334-file search census is an audit surface, not a defensible changed-file estimate.

#### What changed my mind

P2 changed where I would resolve layout. P5 changed how explicitly I would freeze public included-file IDs and separate settings versioning from output schema.

P6 convinced me that privacy must hold throughout migration and that byte preservation cannot stand in for metadata preservation. P3 strengthened the integration plan by requiring complete caller changes and tests in each packet.

My own scope claim did not survive code inspection. The defect to prevent is lost diagnostic coverage, not automatic loss of healthy Skills from default selection.

## P5: Contract and compatibility guardian

/root - GPT-6 - high

#### Rankings

Ranks are within each column, with 1 best.

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 5 | 6 | 5 | 1 | 5 |
| P2 | 2 | 1 | 6 | 5 | 3 |
| P3 | 4 | 3 | 2 | 3 | 1 |
| P4 | 6 | 2 | 4 | 6 | 2 |
| P5 | 3 | 4 | 3 | 2 | 6 |
| P6 | 1 | 5 | 1 | 4 | 4 |

P6 finds the most consequential migration omissions. P2 offers the cleanest dependency boundary. P3 supplies the strongest qualification plan. P1 gives the clearest installation and startup experience.

P4 identifies valuable implementation seams, but overstates the selection-scope defect. P5 is strongest on contracts and output compatibility. Its additional version-writing policy and late placement of observation and documentation work weaken the design and delivery plan.

#### Errors

**P4 incorrectly says one default selection scope would omit Skills from default selections.** With no selectors, the resolver calls `SelectAll`. With exclusions but no includes, selection starts from every catalogue source. The default scope affects otherwise unselected candidates and scoped diagnostics, rather than determining which discovered sources exist in the selection.

Evidence: `SourceUniverseFilterResolver.cs:105`, `SourceUniverseFilterResolver.cs:120`, `SourceCatalogue.cs:193`, `SourceCatalogue.cs:246`.

Plural default scopes remain justified for complete candidate and diagnostic coverage.

**P5’s statement that supplying the current root is “an ordinary no-op” is too broad.** Only the root change should be a no-op. Existing Configure behavior can still select a preset, change routes and resolve frontmatter. Short-circuiting the entire invocation would introduce a compatibility regression.

Evidence: `InstallSetupResolver.cs:60`, `InstallSetupResolver.cs:78`, `InstallSetupResolver.cs:104`. The flag itself is proposed, so this is a faulty compatibility promise, not an observed implementation bug.

I found no further definite code contradiction in the key claims checked. In particular, P6’s metadata concern is supported by the byte-oriented `FileStateSnapshot.cs:17`. P5’s Library representability concern follows directly from `LibraryMapping.cs:37`.

#### Best combination

**Delivery boundary: P2 and P3.** Ship fresh custom-root installation with complete maintenance support first. Require an absent or empty custom destination initially, while preserving existing default-layout adoption behavior. Reject established-root changes until the relocation PR ships. Keep Task 85 open until relocation meets its accepted outcome.

**Layout and dependencies: P2, with P4’s caller inventory.** Put the immutable layout value below Sources, under Workspace. Put settings-backed resolution in Settings observation. Compose ownership and managed-pointer agreement at the operation boundary. Most proposals, including P5, place settings-reading resolution inside Workspace. P2 avoids reversing the documented dependency direction in `architecture.md:447`.

Carry one observation through catalogues, read sessions and plans. Preserve native Skills as native sources. Add plural default scopes for the actual coverage problem identified above. Keep package coordinates, installed paths and source IDs distinct.

**Settings and default compatibility: P1–P4 and P6, supplemented by P5.** Publish immutable schema v4 with optional `mainRoot`. Absence means `.agents`. New settings files declare v4. Preserve existing declarations during unrelated rewrites. Explicit root configuration owns any intentional declaration change.

This requires a narrowly documented exception to default byte compatibility for newly created settings declarations. Reject P5’s separate default-v3 emission policy. All other default payload mapping, ordering, ownership paths and unaffected output remain unchanged against the integrated Task 86 baseline. Review help and wizard changes separately.

Reuse portable path validation. Reject null, empty, non-string, noncanonical, reserved and unsafe physical locations. Validate understood keys regardless of declared version. Validate included-file `roots` against the effective source namespaces. Hand editing settings moves nothing. Invalid settings, contradictory ownership, competing loaders or mismatched pointers must block dependent writes without silently selecting `.agents`.

**Older CLIs: P5’s precise distinction.** Older readers ignore `mainRoot`, but may also reject newly permitted values in familiar keys. The existing `roots` validator rejects `.agents/` paths regardless of a future layout, as shown in `WorkspaceSettingsCodec.Roots.cs:16`.

Neither v4 nor Task 86’s lock bridge supplies an old-writer interlock. Document the first supporting release and unsupported older-CLI maintenance. Qualify representative old binaries in disposable fixtures before making runtime claims.

**IDs and JSON: P5.** Preserve stable short IDs and literal exact-path operands. Preserve the existing distinction between explicit included-file `./path` IDs and null IDs in startup or ordinary linked reads.

Keep JSON output schema 3. Add only contract-defined command-local layout facts where needed. Omit them for ordinary default workspaces. Preserve existing field order, presence and nullability. Do not reuse Install adoption migration rows for relocation. Settings rewrites preserve unknown values and property order, but current serializers do not preserve comments or formatting.

**Migration: P6, with P4 and P5’s Library test.** Keep Task 86’s runtime location and historical recovery paths fixed. Inventory physical entries, including ignored, untracked, binary and overwrite files and empty directories.

Protect both locations before copying private content. Keep source protection until deletion. Define supported metadata preservation and block unsupported cases. Verify destinations and recheck sources before deletion. Require a tested restoration procedure that refuses to overwrite subsequent edits. Recovery bundles remain evidence rather than executable replay instructions.

Require every translated Library destination to remain representable by one destination root with unchanged source suffixes. Straddling alone is an insufficient test. For the first relocation release, adopt P2 and P4’s conservative refusal for Library sources intersecting the moved tree. This explicitly narrows the accepted pinning design. It must provide actionable guidance and leave registrations and source bytes untouched.

**Experience: P1, supplemented by P5.** Ask where context files belong before route choices. Explain that settings and native Skills stay under `.agents`. Explicit input suppresses the question. Preview, JSON and unattended execution never prompt.

Custom-root Status identifies the selected loader and fixed controls. Root AGENTS and CLAUDE point directly to the selected loader. Offline startup must reach Skills and included files without the CLI or README. The shipped CLI Skill must also stop assuming every exact path begins with `.agents`.

**Delivery and qualification: P3 and P4.** Freeze the API, compatibility exceptions and mismatch contract first. Give each packet one owner for production code, focused tests and command output. Assign shared Include/Exclude planning together. Integrate complete caller changes and an Install → Context → Index journey before widening parallel work.

Move Status, Doctor and first-release documentation ahead of the first release gate. P5’s wave table incorrectly leaves these after migration despite proposing an earlier release.

Use P3’s direct byte assertions because the snapshot comparer ignores line endings. Cover default and custom layouts independently, no-write behavior, source identity, non-reactivation, offline startup, old-binary behavior and supported managed/AOT platforms. Use P4’s **20–27 engineering days** as the provisional full-delivery estimate.

**Contracts and documents: P5’s inventory.** Update the accepted architecture and affected layer records, command-contract set, shared operation contract, source-reference, source-universe and permission contracts. Define layout additions without weakening result-coordinate guarantees.

Update Install and every affected navigation, lifecycle, Route, Extension and Library contract and technical design. Include generated navigation, payload mapping, provenance, permissions, Libraries and mutation/recovery.

Cover Framework routing and Skills authorities, payload AGENTS/CLAUDE/loader/README contracts, shipped and dogfood guidance, CLI Skill references, repository documentation, concepts help and the affected site installation, customization, routing and CLI flows. Update acceptance scenarios, Task 85 and its execution plan. Keep historical council records unchanged.

#### What changed my mind

- P2 convinced me to move settings-backed resolution out of Workspace.
- I withdraw P5’s default-v3 writing policy in favor of an explicit new-file v4 exception.
- P6 established that final ignore correctness and copied-byte verification are insufficient migration guarantees.
- I now favor initially blocking intersecting Library source trees over implementing pinned islands.
- P3 and P4 convinced me that P5’s schedule was optimistic and its first-release gate was incomplete.

This was a read-only source review. No builds, tests or Doctor runs were performed.

## P6: Migration and safety engineer

/root - GPT-6 - high

#### Rankings

Ranks are independent within each column. **1 is best.**

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
|---|---:|---:|---:|---:|---:|
| P1 | 4 | 4 | 5 | 1 | 4 |
| P2 | 1 | 1 | 6 | 6 | 3 |
| P3 | 5 | 3 | 4 | 4 | 1 |
| P4 | 6 | 2 | 2 | 5 | 2 |
| P5 | 2 | 6 | 3 | 3 | 6 |
| P6 | 3 | 5 | 1 | 2 | 5 |

The proposals agree substantially. Lower ranks often reflect missing precision rather than an unsafe overall direction.

- **ACCURACY:** P2 identifies important implementation details and the dependency problem. P5 precisely distinguishes public exact-path IDs from short IDs. P6 adds verified migration hazards. P4 identifies a real selection seam but overstates its effect.
- **DESIGN:** P2 offers the cleanest dependency placement. P4 gives the most useful source integration detail. P5’s separate default-v3 emission policy adds complexity to preserve a narrow compatibility claim.
- **QUESTIONS:** P6 best addresses continuous privacy, metadata and interruption. P4 provides the clearest restricted Library migration boundary. P2’s blanket refusal of sources inside the old root is broader than necessary.
- **EXPERIENCE:** P1 provides the strongest install wording, host imports, Skill guidance and release-specific errors. P6 best explains interrupted moves.
- **PLAN:** P3 best connects implementation to existing evidence and complete caller changes. P4 has credible integration boundaries and estimates. My P6 leaves too much presentation work late and does not freeze enough recovery mechanics for its later migration packet.

#### Errors

Paths below use:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `C` = `.agents/memory/crystallized/documents/cli`

**R1 — P4 overstates the single-scope failure.**

P4 says retaining one default selection scope would omit Skills from default selections. The current resolver returns `Catalogue.SelectAll()` when there are no selectors. With exclusion-only selectors, it starts from every catalogue source. Valid Skills already present in that catalogue therefore remain selected.

The scope affects unresolved candidates and diagnostic coverage. That still needs correction for two content locations, but the tests should target the actual failure.

Evidence: `F/Sources/Selection/SourceUniverseFilterResolver.cs:105`, `:120`, `:128`, and `F/Sources/Models/Inventory/SourceCatalogue.cs:193`, `:246`.

**R2 — P1, P3, P4, P5 and my P6 propose a dependency cycle.**

These proposals place settings-backed resolution under Workspace. Settings already depends on Workspace. Adding the reverse dependency would contradict the documented direction that places Workspace below Sources and state capabilities.

This is a defect in the proposed placement, not an existing implementation bug. Keep the pure layout value in Workspace. Put the settings-backed reader and its settings-bearing result above that value, as P2 recommends.

Evidence: `F/Settings/Shared/Observation/WorkspaceSettingsReader.cs:7`, `:18`, and `C/architecture.md:445`.

I would not count P5’s v3 default-emission recommendation as a factual error. It explicitly proposes different policy. I reject it because it complicates the implementation without protecting custom workspaces from older writers.

Several distinctive claims survived checking:

- P1 correctly identifies the Minimal wizard default and stale Essentials contract wording. `O/Install/Shared/Configuration/InstallSetupResolver.cs:63`, `C/contracts/install/interface.md:76`.
- P3 correctly notes that output snapshots ignore line endings. `src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/CommandOutputSnapshot.cs:24`.
- P5 correctly preserves public `./path` IDs and the independent JSON envelope version. `C/contracts/shared/source-references/interface.md:68`, `C/contracts/shared/result-coordinates/interface.md:160`.
- P6 correctly identifies byte-only staging and the old plan’s copy-before-ignore ordering. `F/Mutation/Application/FileChangeApplier.Staging.cs:21`, `.agents/memory/emerging/analysis/route-include-council/main-root-plan.md:339`, `:341`.

#### Best combination

**Ship the installation-first boundary shared by all six proposals.** Support the first Open Forge installation in an existing project, with an absent or empty custom destination. Complete every maintenance and package lifecycle before exposing the option. Preserve default-root adoption. Reject a different root on an established installation until relocation ships. Task 85 remains open.

**Use P2’s dependency placement and P4’s coordinate separation.** Keep literal workspace paths, portable package coordinates and stable source IDs distinct. Resolve one immutable layout per operation and carry it through catalogues, sessions and plans. Include native Skills as managed content with normal maintenance behavior. Correct P4’s scope explanation and preserve candidate and diagnostic coverage across both locations.

**Use P5’s output precision.** External files have no short ID, but several existing public results use `./path` in their `id` field. Preserve that behavior. Settings v4 does not require changing the schema-3 JSON envelope. Optional command-local layout information is permitted when omission preserves previous meaning. The contract explicitly allows that at `C/contracts/shared/result-coordinates/interface.md:165`.

Write new settings as v4 and preserve existing declarations during ordinary rewrites. Treat the wizard question, new help and new-file declarations as explicit compatibility exceptions. Do not build separate default and custom schema-emission machinery.

**Use P1’s user experience.** Call the choice a “context folder” while retaining the accepted question. Ask before destination-dependent route choices. Preserve Claude’s `@AGENTS.md` bridge and render its loader import. Update custom-rendered CLI Skill instructions as well as host pointers. The shipped Skill currently directs agents to `.agents/README.md`, so host changes alone are insufficient. See `src/open-forge/.agents/skills/open-forge-cli/SKILL.md:10`.

**Use P6’s safety conditions with P4’s restricted Library support.** For the first relocation release:

- Preserve fixed controls, native Skills and Task 86 runtime storage.
- Inventory actual filesystem entries, including ignored and untracked content.
- Reject collisions and unsupported links before effects.
- Keep Library sources stationary. Initially refuse sources intersecting movable content. Do not implement arbitrary pinned islands.
- Prove that every translated Library destination remains representable by one destination root and unchanged source suffixes.
- Protect both source and destination throughout copying and deletion.
- Define supported metadata preservation before claiming a safe move.
- Verify destination content and revalidate the source before deletion.
- Retain recovery evidence and refuse blind retries.

The Library representability test comes from P4, P5 and P6. The implementation derives both the destination and source from the same suffix, so changing suffixes to repair destination mapping also changes source identity. See `F/Libraries/Models/Observation/LibraryMapping.cs:37`.

**Close one recovery gap before migration implementation.** Install’s current recovery filter treats any `Third` entry as history. An intermediate ignore-file state or subsequent user edit can therefore remove a bundle from its blocking set. The existing mixed-state check alone cannot establish that a move is complete or safe to retry. See `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:63`.

Keep Task 86 unchanged. Define migration-specific observation and restoration behavior that names residual paths and preserves later edits. Do not turn unrelated historical bundles back into global write blockers. Until that contract is closed and tested, defer relocation.

**Use P3’s delivery discipline and P4’s estimating restraint.** Integrate shared signature changes with their callers and focused tests. Keep implementation, tests and command-local presentation together. Establish the post-Task-86 baseline, then prove Install → Context → Index before expanding the conversion.

Reuse the existing no-reactivation journey at `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteIncludeProcessTests.cs:37`. Add direct byte comparisons where snapshots cannot prove the promise. Qualify custom-root lifecycle and interrupted relocation separately. P4’s 20–27 engineering-day total is a more credible provisional budget than treating migration as a small final packet.

#### What changed my mind

P2 changed my placement recommendation. My P6 put a settings-backed resolver too low in Framework. I would now keep only the pure layout value in Workspace.

P4 changed my initial Library migration boundary. I favor refusing source trees that intersect movable content for the first release. Conditional pinning needs more navigation and identity work than my proposal made explicit.

P5 sharpened the compatibility contract. “No short ID” must not become “null public ID everywhere,” and settings v4 must remain separate from the JSON output version.

P3 strengthened my default-preservation plan. Existing snapshots are useful but cannot alone prove byte identity.

I still recommend deferring relocation. The checked `Third`-state behavior makes the unresolved recovery boundary more concrete.

This review used read-only source inspection. No builds, tests or Doctor runs were performed.
