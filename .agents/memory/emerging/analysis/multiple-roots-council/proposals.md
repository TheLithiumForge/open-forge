---
open-forge:
  description: The five Stage 1 proposals of the multiple roots council, kept as evidence
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Routing, Include, Settings]
---

# Multiple Roots Council Proposals

The five independent Stage 1 proposals, each under its Stage 2 label and lens. Citations point at commit `7c86a8ba3`. The [record](record.md) holds the decisions.

## P1: Contract and compatibility guardian

/root - GPT-6 - high

#### Recommendation

Keep one main root and add explicitly maintained catalogue trees as the meaning of “multiple roots.”
Preserve `route include` as a read-only inclusion operation, with separate `route maintain` and `route unmaintain` commands controlling maintenance.
Keep `mainRoot`, included-file `roots`, maintenance permission, package ownership and loading policy distinct.
Amend Task 85’s internal layout boundary now, but ship additional roots separately after Delivery 1.
Keep existing source identities, the schema-3 JSON envelope and the frozen default-layout byte guarantee.

#### Answers

##### 1. What “multiple roots” means

A second root is a contained, user-owned directory with one designated catalogue. It has no loader, settings file or installation of its own.

Today an included catalogue already provides recursive reading through its Entries. The reader follows those links without scanning neighbouring files. It assigns workspace-path identities to the included sources. Index inventories `.agents`, while route topology explicitly excludes included sources. These boundaries explain the missing capability. **[E1–E3]**

A maintained root adds:

- Discovery of eligible files within its directory.
- Index maintenance of metadata, missing catalogues and generated Entries.
- Route creation, initialization, metadata updates and removal within that boundary.
- Existing reference diagnosis and repair for those maintained files.

For example, a team could keep its knowledge in `docs/team`, add a document there and use Index to update navigation. An ordinary include requires the team to maintain that navigation itself.

Registration never makes every file required reading. The main loader and catalogue rows still control exposure, order, tags and conditions.

##### 2. Extend `route include` or not

Reuse its catalogue model and loading machinery, but keep its command contract unchanged.

Proposed commands:

```text
open-forge route maintain <workspace-catalogue> [--dry-run]
open-forge route unmaintain <workspace-catalogue> [--dry-run]
```

`maintain` registers an existing canonical catalogue and its containing directory for maintenance. Its preview identifies the directory, eligibility rules and registration change. It does not rewrite content. Index previews the subsequent content changes.

`unmaintain` removes that permission. It preserves documents, generated navigation and loader rows. It must work when the catalogue is missing.

Use ordinary `route include` to expose the catalogue to agents. Include and Exclude continue changing only loader membership and include registration. Excluding a maintained root removes its exposure, while maintenance registration remains visible as unlisted. Automatic Index does not traverse it. Explicit maintenance remains possible until `unmaintain`.

This separates two independently useful actions. The current Include contract promises target-byte preservation and limits effects to loader and settings. Extending it with ongoing subtree write authority would make that promise harder to understand. **[E4]**

##### 3. The model

**Settings.** Keep Task 85’s v4 unchanged in purpose. Introduce a later settings version for the additional field. Schematic example:

```json
{
  "schemaVersion": 5,
  "mainRoot": "docs/forge",
  "roots": ["docs/team/_team.md"],
  "maintainedRoots": ["docs/team/_team.md"]
}
```

`mainRoot` selects the installation’s content location. `roots` remains the existing included-file registry. `maintainedRoots` names catalogues whose containing trees permit route maintenance. Absence means empty. Store no copied descriptions, tags, conditions, aliases or loading order.

The two arrays answer different questions. An ordinary include needs only `roots`. Maintenance without loader exposure needs only `maintainedRoots`. Neither array recreates a removed loader row.

Validate canonical paths, duplicates and overlapping maintenance boundaries. Preserve unknown values and property order during rewrites. Preserve existing declarations on unrelated changes. Explicit adoption of the new feature writes its matching version and schema URL. Published schemas remain immutable. Existing settings code already preserves unknown properties through its JSON object model, but does not preserve comments on a changed serialization. **[E5–E6]**

**Identity.**

- Preserve main-root short IDs and fixed native Skills IDs.
- Additional-root sources retain `./<workspace-path>` identities, including the catalogue filename.
- Promotion from an ordinary include to maintained content must not rename its identity.
- Literal paths remain literal. Do not redirect them through the main root.
- Do not introduce root names, `root:id` syntax or precedence between duplicate short IDs.
- Retain explicit ambiguity handling. Reject overlapping or physically aliased maintenance registrations rather than selecting a winner.

The current included reader already supplies `./path`, and the source resolver already reports ambiguous IDs. **[E1, E7]**

**Command behavior.**

| Surface | Additional-root contract |
|---|---|
| Index | Traverse exposed maintained trees automatically. Permit explicit selection of an unlisted maintained tree. Preserve ordinary includes and authored loader-row policy. Never follow a link into another tree as write authority. |
| Route List / Inspect | Show registration, exposure, maintenance boundary and actual source path separately. |
| Route Create / Init / Update / Remove | Accept exact paths within the registered boundary. Apply existing ownership and preservation rules. Reject deleting or moving the root boundary itself until it is unregistered. |
| Route Move | Initially support moves within one root. Reject cross-root moves until their reference and recovery semantics are specified. |
| Context | Follow the loader and catalogue graph. Maintenance registration alone loads nothing. |
| Find | Inventory eligible maintained content, with existing reachability and applicability facts. Ordinary includes remain link-discovered only. |
| References | Use the combined source inventory and literal targets. Preserve the distinction between navigation rows and authored references. |
| Status | Report maintained, listed, unlisted, missing and unavailable facts without conflating registration with loading. |
| Doctor | Diagnose malformed registrations, overlap, missing catalogues and unsafe paths. Do not restore membership or registration automatically. |
| Repair / Cleanup | Reuse existing effect and recovery machinery. Recovery addresses recorded paths independently of current registration. |

Current include reporting already separates registered, listed and missing facts. Preserve that distinction when extending reporting. **[E8]**

**Ownership and permissions.** Maintenance registration is permission for bounded route maintenance, not Framework or Extension ownership. It grants no general package-install permission. Keep `allowInstallPaths` independent.

Initially reject registrations intersecting Library sources, Library projections or whole-file package-owned content. This avoids silently converting package or shared content into ordinary authored material. Package destinations continue mapping through the main root and native Skills only.

**Recovery.** Keep one physical-workspace lease and the existing `.open-forge-state/` recovery store. Revalidate settings, root ancestry and targets under that lease before effects. Registration changes use existing settings mutation recovery. Later Index and route effects use their normal recovery preparation. Preserve historical target paths and never remap them through current roots. No automatic rollback is added. **[E9]**

**Reserved locations.** Reject the workspace root, Git metadata, runtime state, `.agents` controls, native Skills, the main root and any overlapping ancestor or descendant. Reject linked registration directories initially. Native Skill discovery remains confined to its existing location. A second root does not create another host-discovered Skills location.

**JSON.** Keep envelope schema 3, source-location coordinates, field order, nullability, statuses and exits unchanged. New commands receive their own data contracts. Existing commands may add optional maintenance facts only where omission preserves prior meaning. Omit those fields in unchanged default workspaces. Do not repurpose existing `registered` fields. **[E10]**

##### 4. Fit with frozen Task 85

The frozen decision explicitly declined configurable locations beyond the main root. Additional maintained roots therefore require an accepted amendment. They are not already included in Task 85’s promise. **[E11]**

**Change now in Delivery 1:**

1. Keep `WorkspaceLayout` pure and its reader under Settings.
2. Represent content locations internally with their role and identity policy. Retain `SourceRoots` as a deduplicated scan projection, not an authority list.
3. Add one location lookup returning the containing location and relative path. Avoid repeated prefix arithmetic in consumers.
4. Specify `SourcePath`, `CategoryDirectory` and `MapPayloadPath` as primary installation mappings. They must never search additional roots.
5. Keep `IsContentPath` separate from `IsImplicitContentDestination`. The latter admits only the existing primary installation destinations and native Skills.
6. Define `TryGetSourceRelativePath` as the primary short-ID mapping. Additional roots will use workspace-path identity.
7. Preserve one observed layout through sessions, catalogues, plans and under-lease revalidation.

The frozen API already separates membership from permission, but Wave 4’s statement that implicit admission follows content locations needs narrowing before additional locations exist. **[E12]**

Amend Wave 0’s callable contract before dependent work. Wave 1 must preserve established identities when reconstructing catalogues. Waves 2–5 must consume location facts without assuming exactly two locations or deriving authority from inventory membership. Wave 6 retains activation only after all command families support the selected main root.

**Do not change v4 to reserve an unimplemented maintenance field.** Delivery 1 remains first-install main-root selection, with absent `mainRoot` meaning `.agents`. New settings declare v4, unrelated rewrites preserve declarations and no absent default is inserted. **[E11–E12]**

**Later delivery:** add maintenance registration, its settings version, source classification, command support and qualification together. Keep Task 85 Delivery 2’s established-root relocation separate. Registering a second tree must not provide a shortcut around relocation safeguards.

**Contracts requiring explicit updates:**

- Settings schema and configuration semantics.
- Workspace layout, source references, source-universe selection and workspace permissions.
- New Maintain/Unmaintain interface and behavior contracts.
- Include/Exclude documentation explaining their independence from maintenance.
- Every affected route leaf, Index, Context, Find, References, Status, Doctor and Repair.
- Install, Update, Remove, Extension and Library contracts where they define destination admission, preservation or removal scope.
- Generated navigation, sharing, ownership, mutation and recovery technical designs.
- Command-local JSON definitions, help, CLI documentation, shipped README and CLI Skill references.

Shared result coordinates need conformance evidence, not a schema change. Ownership and recovery formats need no new fields for this design. Task 85 already identifies the broader documentation inventory. **[E10, E13]**

##### 5. Risks

The largest compatibility risk is an older executable ignoring the new meaning.

Current settings readers accept unknown schema versions and read only understood keys. They ignore unknown properties. A v4 or v5 declaration therefore cannot force an older CLI to refuse. A minimum-version field would have the same limitation. **[E5]**

Document custom and maintained-root workspaces as unsupported by older CLIs. Do not claim downgrade safety merely because an old command exits successfully. Qualify selected older binaries in disposable fixtures, especially Update, Configure and Exclude.

The default guarantee must remain precise. For Delivery 1, preserve paths, content bytes, ownership and ordinary output against the Task 86 baseline. Allow only the frozen v4 declaration and intentional help, option and wizard changes. Do not serialize empty future collections or normalize existing files opportunistically. **[E11]**

Other material risks are identity churn after promotion, duplicate physical traversal, accidental package permission expansion, automatic reactivation and recovery that depends on registration still existing.

##### 6. Tests

Use production-owned constants and typed definitions for filenames, directories, schema versions, finding codes and environment variables. Give scenario-specific fixture values one local owner. Assert user-facing wording only through reviewed snapshots.

| Tier | Required evidence |
|---|---|
| Unit | Pure layout classification, identity stability, overlap detection, settings version handling, unknown-key retention, no-op serialization, permission separation and concrete JSON projections. |
| Integration | Real owned filesystems proving registration and removal, containment, alias rejection, Index’s bounded writes, preserved outside links, unlisted-root behavior, stale-settings refusal and recovery after partial effects. |
| EndToEnd | Fresh delivered executable performing include → maintain → add document → Index → Context/Find/References → unmaintain. Also custom-main-root coexistence with native Skills, default compatibility and selected older-CLI journeys. |

Test direct byte preservation separately from snapshots. The repository’s snapshot comparer normalizes line endings, so it cannot prove the frozen byte guarantee. Compare supplied pre-operation bytes and exact baseline artifacts, including line endings. Do not compare unrelated generated timestamps as preservation evidence. **[E14]**

Keep detailed rejection matrices below EndToEnd. Run filesystem-sensitive journeys on Windows, Linux and macOS, then the required managed and Native AOT acceptance gates with fresh-artifact receipts. The tier boundaries are mandatory. **[E15]**

##### 7. Work packets and estimate

| Packet | Astra high ownership | Estimate |
|---|---|---:|
| Foundation amendment | Layout roles, mapping semantics, permission distinction and Task 85 plan amendments | 1–2 days |
| Registration | Settings version, codec, validation, Maintain/Unmaintain and focused recovery evidence | 2–3 days |
| Inventory and identity | Catalogue construction, stable external identities, alias handling and read-command integration | 2–3 days |
| Maintenance | Index, route operations, bounded navigation and effect recovery | 3–5 days |
| Reporting and qualification | Status/Doctor, JSON, docs, default bytes, older binaries and delivered journeys | 3–4 days |

Estimate **11–17 engineering days**, including the foundation amendment and later feature. This is additional work, not a replacement estimate for Task 85.

Freeze shared contracts first. Integrate registration and inventory before maintenance. Activate the commands only after the complete lifecycle passes. Parallel implementation can shorten elapsed time after those boundaries are fixed.

#### Risks and what I would not build

I would not build a mount system, multiple loaders, root precedence, secondary short-ID namespaces, per-root settings, per-root locks or package installation into arbitrary roots.

I would not convert existing `roots` strings into objects or treat every include as writable. I would not make registration imply loading, package ownership or implicit installation permission.

I would not combine secondary-root adoption with moving an established main root. Nor would I promise compatibility with older executables that cannot understand the new authority boundary.

This proposal is based on the supplied snapshot. The separate Wave 0 worker’s implementation and released older-binary behavior were not verified. No builds or tests were run.

#### Evidence

Paths below are relative to the repository. `F` means `src/cli/framework/OpenForge.Cli.Framework/Framework`, `O` means `src/cli/operations/OpenForge.Cli.Operations/Commands`, and `C` means `.agents/memory/crystallized/documents/cli`.

- **E1:** `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:15`, `:96` — linked traversal, catalogue recognition and `./path` identity.
- **E2:** `O/Index/Shared/Projection/IndexProjectionReader.cs:67`, `F/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:423`, `:448` — inventory boundary and preservation of authored rows.
- **E3:** `F/Sources/Routing/SourceRouteFactsResolver.cs:26`, `O/Route/Update/Shared/Planning/RouteUpdateSourceSelector.cs:45`, `O/Route/Move/Shared/Planning/RouteMoveSubjectSelector.cs:79` — included-source exclusion and route inventory.
- **E4:** `C/contracts/route/include/interface.md:20`, `:65`, `:69`, `C/contracts/route/exclude/interface.md:20`, `docs/cli.md:939` — current Include/Exclude promises.
- **E5:** `F/Settings/WorkspaceSettingsDefinitions.cs:17`, `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:18`, `:274`, `F/Settings/Models/Document/WorkspaceSettingsDocument.cs:32` — tolerant versions, unknown keys and existing `roots` meaning.
- **E6:** `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:80`, `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:94`, `schemas/v3/open-forge.schema.json:109`, `.agents/directives/software/versioned-configuration-schema.md:15` — rewrite and schema rules.
- **E7:** `F/Sources/Identity/SourceIdentity.cs:14`, `F/Sources/Identity/SourceReferenceParser.cs:26`, `F/Sources/Identity/SourceReferenceResolver.cs:56` — short IDs, literal paths and ambiguity.
- **E8:** `F/Sources/Operational/Shared/Routes/RouteIncludedFileReader.cs:42`, `:147` — registration and exposure facts.
- **E9:** `F/Workspace/Shared/Runtime/WorkspaceRuntimePaths.cs:7`, `F/Mutation/Locking/WorkspaceLockManager.cs:34`, `F/Recovery/Shared/Storage/RecoveryStoreLocator.cs:11`, `C/contracts/route/include/behavior.md:43` — fixed runtime storage and mutation recovery.
- **E10:** `C/contracts/shared/result-coordinates/interface.md:160` — shared and command-local JSON compatibility.
- **E11:** `.agents/memory/emerging/analysis/main-root-council/record.md:44`, `:55` — frozen deliveries, defaults and declined scope.
- **E12:** `.agents/memory/emerging/analysis/main-root-council/plan.md:91`, `:110`, `:118`, `:132`, `:217`, `:236`, `:421` — layout API, transport and permission amendment points.
- **E13:** `.agents/memory/emerging/analysis/main-root-council/plan.md:551` — affected documentation and contracts.
- **E14:** `src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/PlatformSnapshotComparer.cs:18`, `.agents/memory/emerging/analysis/main-root-council/plan.md:545` — byte-comparison requirement.
- **E15:** `.agents/directives/open-forge/testing/evidence-integrity.md:12`, `.agents/patterns/testing/evidence-tiers.md:12` — evidence boundaries.

## P2: Contrarian minimalist

/root - GPT-6 - high

#### Recommendation

Ship one configurable main root with the existing ability to include files and catalogues elsewhere in the workspace. Council 4 explicitly called those included loader entries “multiple roots” and deferred maintaining their contents. [E1] Task 85 should preserve that boundary and prove it works with a nested custom main root. Add explicit acceptance criteria and focused evidence, without adding another settings concept or command. Reconsider writable secondary roots only when a concrete maintenance workflow requires them.

#### Answers

##### 1. What “multiple roots” should mean

A workspace has one main root containing its loader and ordinary Open Forge categories. Native Skills retain their fixed location. The loader can also list independently maintained documents and catalogues elsewhere in that workspace.

That already allows a team to keep engineering context in one folder and service-specific context beside each service. Included catalogues expose their listed descendants without requiring those documents to move. The reader follows Entries and assigns exact `./path` identities. It does not scan adjacent folders. [E2]

**A second maintained root would add write behavior.** Index could discover new files and regenerate navigation there. Route commands could create, update, move or remove its content. Existing includes do not provide that capability. [E3, E4]

The brief establishes a need for multiple locations. It does not establish a need for those additional write capabilities. My recommendation satisfies the former and deliberately leaves the latter unimplemented.

The tradeoff is real. Authors must maintain included catalogues themselves. If that becomes the demonstrated problem, revisit catalogue maintenance as its own bounded feature.

##### 2. Extend `route include` or not

Keep the existing surface:

```sh
open-forge install --main-root docs/forge
open-forge route include docs/team/_team.md --description "Team context"
open-forge route include services/api/_api.md --description "API context"
open-forge route exclude services/api/_api.md
```

The first command is Task 85’s planned surface. The others already express the required reuse model.

Do not accept a directory as shorthand yet. The current command requires one file and already suggests the directory’s catalogue. [E5] A folder operand would save little typing while raising a new question about whether its contents become maintained.

Do not add `--maintain`, `--root`, or a top-level `root` command in Delivery 1. Include currently promises to change only the loader and settings. Quietly widening that promise would be a product change. [E3]

If later evidence justifies automatic catalogue maintenance, an explicit Include option is worth evaluating. Naming it now would prematurely freeze its lifecycle and authority.

##### 3. The model

Keep settings v4 as planned. For example:

```json
{
  "schemaVersion": 4,
  "mainRoot": "docs/forge",
  "roots": [
    "docs/team/_team.md",
    "services/api/_api.md"
  ]
}
```

`mainRoot` selects the main content directory. `roots` remains a registration list of canonical workspace-relative file paths. It holds no directory ownership, loading metadata or write permission.

The loader remains authoritative for loading, order, tags and conditions. Removing a loader row leaves an unlisted registration. Index must never reconstruct that row from settings. This is already the Include contract. [E3]

**Identity and collisions**

- Main-root sources retain stable short IDs through the planned layout mapping.
- Native Skills retain their existing namespace and fixed location.
- Included files and catalogue descendants retain exact `./workspace/path.md` identities.
- Do not invent root aliases, prefix namespaces or precedence between secondary roots.
- Preserve the existing physical-file alias handling. The included reader retains the first encountered parent and row metadata for a physical file. This is existing behavior, not a new root-priority policy. [E2]

Two external catalogues containing similarly named documents therefore do not compete for the same short ID.

**Command behavior**

| Surface | Treatment of included context |
|---|---|
| Index | Preserve included loader rows. Do not scan or rewrite included trees. |
| Route commands | Include and Exclude alter membership. Structural writers stay within the maintained content locations. |
| Context | Follow included catalogue chains, loading tags and inherited conditions. |
| Find | Search included sources and listed descendants without scanning neighbouring files. |
| References | Inspect included Markdown links. Other text formats remain opaque. |
| Status | Report registered includes as listed, unlisted or missing, alongside hand-written rows and context measurements. |
| Doctor | Diagnose missing targets, invalid registrations and unlisted registrations without creating maintenance ownership. |

These behaviors already have implementation support. Task 85 must carry them across the layout change. [E2, E4, E6]

**Ownership, recovery and reserved locations**

Including a document grants no ownership over its bytes. Include’s recovery plan covers its loader and settings changes. Excluding it removes membership and registration while preserving the document. [E3, E7]

Keep one workspace lock and recovery store under `.open-forge-state/`. The code derives both from the physical workspace root. A context location does not need its own lock, recovery directory or installation record. [E8]

Keep settings and ownership controls fixed under `.agents`, and native Skills under `.agents/skills`. Retain Task 85’s restrictions on Git metadata, runtime storage, workspace-root selection, `.agents` descendants and a competing custom-root Skills category. Inclusion must also retain lexical and physical containment checks and control-file exclusions. [E5, E9]

##### 4. Fit with the frozen Task 85 plan

**No new production API or settings field is needed for this interpretation of multiple roots.**

Keep the pure `WorkspaceLayout`, its settings-backed reader and its existing signatures. Its `SourceRoots` collection represents the main content location and native Skills. It must not absorb the include registry. The frozen plan already distinguishes those locations from included sources. [E9]

Make these requirements explicit now:

1. **Wave 0, A1–A3:** State that layout membership and include registration are different facts. Keep v4 limited to `mainRoot`. Complete the already-planned validation of registrations against the effective content locations and fixed reserved paths.
2. **Wave 1, B1–B2:** Preserve external `./path` identities and catalogue aliases. Resolve loader links from the actual loader directory. The current Include formatter assumes a one-level loader through `../`, so nested roots require the planned conversion. [E7]
3. **Waves 2–4, C1, D1, D2 and E1:** Preserve the separation between maintained inventory and included-source traversal. Verify read commands and operational reporting agree.
4. **Wave 5, Q:** Keep Include and Exclude’s write scope unchanged while replacing fixed-root and path-segment assumptions.
5. **Wave 6 and qualification:** Add a custom-root journey with two external catalogues. Keep public activation after all command families support the layout.

These are acceptance clarifications within existing packets, not another implementation wave. The plan already names the relevant conversions. [E10]

Delivery 1 includes the configurable main root, preserved includes, documentation and qualification. Delivery 2 remains relocation of an established main root. Extra maintained roots belong to a separately accepted delivery, if needed.

I would reject a promise of “multiple maintained roots later without rework.” Their ownership and mutation semantics remain undecided. Task 85’s useful preparation is removing scattered path assumptions and preserving clear boundaries. A generic mount registry would freeze guesses.

##### 5. Risks

The most consequential risks are:

- **Accidental write expansion.** Adding included folders to `SourceRoots` could expose them to inventory, generated navigation and implicit destination admission. The frozen plan explicitly ties implicit admission to content locations. [E11]
- **Incorrect rebasing.** Nested main roots break code that assumes the loader is exactly one directory below the workspace. Include has that assumption today. [E7]
- **Silent reactivation.** Treating `roots` as the loading authority could restore deliberately removed rows.
- **Older CLI use.** The current codec accepts integer schema versions and does not read `mainRoot`. A v4 declaration alone cannot make that CLI understand custom layouts. Keep the planned unsupported-version guidance. [E12]
- **Default-layout drift.** Preserve default paths and bytes under the frozen exceptions for settings declarations and intentional interface changes. [E9]

Keeping the current `roots` shape avoids an additional compatibility problem. Replacing strings with objects or reinterpreting registered files as writable directories would add one.

##### 6. Tests

Use the repository’s separate Unit, Integration and EndToEnd projects. Keep each test at the lowest boundary that proves its claim. [E13]

| Tier | Required evidence |
|---|---|
| Unit | Layout mapping and membership. Root-registration validation. Stable main-root IDs and external exact identities. Loader-relative destination formation. Settings declaration and unknown-property preservation. |
| Integration | One command or module against owned real files. Include changes only the selected loader and settings. Index preserves external bytes and authored rows. Included traversal follows listed descendants and conditions without scanning siblings. Status and Doctor expose typed findings. Stale layout observations prevent effects. |
| EndToEnd | One delivered-CLI journey installs a nested custom root, includes two catalogues, reads their context, indexes and excludes one while preserving external bytes. A second journey proves removing a loader row survives Index, Update, Configure and Extension installation. Retain default-layout comparison evidence. |

Exercise the main root, native Skills and external catalogue locations together. Test that short-ID lookalikes remain distinguishable by exact paths. Put alias, containment and missing-target details in focused Integration tests rather than multiplying full journeys.

Apply the maintainer’s rules throughout:

- Reference filenames, folder names, schema keys, finding codes and environment variables through their owners.
- Give invented fixture paths one local fixture owner.
- Assert typed outcomes, identities, effects and independently observed file preservation directly.
- Check exact public wording only through reviewed snapshots.
- Compare before-and-after file bytes directly for preservation. Do not replace that evidence with normalized snapshots.

Existing Include tests are useful scenario sources, but some assert literal rendered rows and substrings. Revise touched tests instead of copying those assertions into Task 85. [E14]

Retain Task 85’s required managed and Native AOT qualification. This proposal does not replace its broader cross-platform layout evidence with the two journeys above. [E10]

##### 7. Plan

Use the existing Astra high implementers and packet ownership.

| Packet | Owner and scope | Incremental estimate |
|---|---|---:|
| Contract clarification | Foundation owner. Amend Task 85 acceptance criteria and the layout description. State that includes remain outside maintained inventory. | 0.5 day |
| Boundary evidence | Existing A2, B2, D2, E1 and Q owners. Add only missing focused cases in their production-mirrored tests. Correct touched literal-text assertions. | 1–1.5 days |
| User journey and explanation | Existing qualification and documentation owners. Add the two-catalogue custom-root journey and explain who maintains each catalogue. | 0.5–1 day |

Allow **2–3 engineering days of incremental work**, with overlap against evidence already budgeted in Task 85. This is a planning estimate, not measured execution time.

Do not restart Wave 0 or create a second foundation owner. The frozen Delivery 1 estimate remains 14–18 engineering days, subject to its existing qualification work. [E10]

#### Risks and what I would not build

- **A maintained flag inside `roots`.** It mixes read registration with write authority and changes a published string-array contract.
- **Automatic folder adoption.** Existing project directories may contain documents with different formatting and navigation conventions. Inclusion does not establish permission to normalize them.
- **Multiple main roots.** The loader, payload placement and host pointers need one unambiguous destination. Adding several would force precedence rules without improving the stated reuse workflow.
- **Global short IDs for external files.** Equal relative names would need namespaces, ambiguity handling or priority. Exact paths already solve identification.
- **Per-root settings, locks or recovery.** Loader and settings changes remain workspace-wide. Splitting their coordination adds failure modes.
- **Package destinations in arbitrary secondary roots.** That expands Extension ownership, Library projection, exclusions and recovery work beyond the demonstrated need.
- **A migration framework for hypothetical roots.** Finish the accepted single-root relocation contract first.

The strongest argument against my recommendation is the manual cost of maintaining external catalogue Entries. If that cost is the maintainer’s actual concern, this proposal is insufficient. The smallest next investigation would be explicit maintenance of one included catalogue tree, with its write boundary defined before expanding every route command.

#### Evidence

Paths are repository-relative. Findings come from static inspection. Live Wave 0 progress and runtime behavior were not verified. No edits, builds, tests or Doctor execution were performed.

- **E1 — Meaning and deferred maintenance:** `.agents/memory/emerging/analysis/route-include-council/record.md:59`, `:205`, `:260`.
- **E2 — Catalogue traversal, physical aliases and exact identities:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Shared/Includes/SourceIncludedEntryReader.cs:60`, `:79`, `:96`.
- **E3 — Include’s contract and bounded effects:** `.agents/memory/crystallized/documents/cli/contracts/route/include/interface.md:20`, `:65`, `:71`. Its sibling `behavior.md:35` preserves loader membership.
- **E4 — Maintained inventory boundary:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Projection/IndexProjectionReader.cs:67`. Route equivalents include `Commands/Route/Update/Shared/Planning/RouteUpdateSourceSelector.cs:42`, `Commands/Route/Move/Shared/Planning/RouteMoveSubjectSelector.cs:79` and `Commands/Route/Remove/Shared/Planning/RouteRemoveSubjectSelector.cs:79` under the same Operations project.
- **E5 — Operand and containment checks:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Shared/Inclusion/Planning/RouteInclusionTargetResolver.cs:15`, `:49`, `:60`.
- **E6 — Read and reporting behavior:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Context/Shared/Graph/ContextGraphBuilder.cs:81`, `Commands/Find/FindOperationFactory.cs:36`, `Commands/References/ReferencesOperationFactory.cs:28`, `Commands/References/Shared/Inspection/ReferencesLayerInspector.cs:54`, and `Commands/Doctor/Shared/Domains/WorkspaceIncludeDoctorInspector.cs:10` under that Operations project. Measurements and registration facts are in `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Operational/Shared/Routes/RouteIncludedFileReader.cs:38`.
- **E7 — Actual mutation targets and fixed-depth link formation:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Shared/Inclusion/Planning/RouteInclusionPlanBuilder.cs:131`, `:178`.
- **E8 — Fixed runtime storage:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Workspace/Shared/Runtime/WorkspaceRuntimePaths.cs:7`. Consumers are `Framework/Mutation/Locking/WorkspaceLockManager.cs:34` and `Framework/Recovery/Shared/Storage/RecoveryStoreLocator.cs:12` under that Framework project.
- **E9 — Frozen layout and delivery boundaries:** `.agents/memory/emerging/analysis/main-root-council/record.md:44`. The sibling `plan.md:84`, `:110`, `:118`, `:120` defines the API and settings behavior.
- **E10 — Existing conversion packets and qualification:** `.agents/memory/emerging/analysis/main-root-council/plan.md:275`, `:337`, `:388`, `:440`, `:530`, `:585`.
- **E11 — Implicit admission follows content locations:** `.agents/memory/emerging/analysis/main-root-council/plan.md:421`.
- **E12 — Current settings behavior:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:48`, `:274`. Root strings are handled in its sibling `WorkspaceSettingsCodec.Roots.cs:16`.
- **E13 — Evidence boundaries:** `.agents/directives/open-forge/testing/evidence-integrity.md:13`, `:30`, and `.agents/patterns/testing/evidence-tiers.md:13`.
- **E14 — Existing scenarios and assertions needing adjustment when touched:** `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Include/RouteIncludeApplicationTests.cs:18`, `:44`, and `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteIncludeProcessTests.cs:38`, `:54`.

## P3: Test and qualification engineer

/root - GPT-6 - high

#### Recommendation

Keep Task 85 Delivery 1 focused on one configurable main root, with native Skills fixed in place. Council 4 already used “multiple roots” to mean included files and catalogues, so a maintained second folder would add a new capability [E1]. Introduce that capability later through `route include <catalogue> --maintain`, with explicit maintenance registration and unchanged plain Include behavior. Amend the foundation now to separate discovery, identity, loading and write authority. Correct the qualification plan before accepting evidence from its command packets.

#### Answers

##### 1. What multiple roots should mean

Distinguish two supported uses.

An **included root** is an existing document or catalogue that agents can read where it lives. Today, Include rejects directory operands, records external files in settings, and changes only the loader and settings. Catalogue traversal follows Entries without scanning adjacent folders [E2–E4].

A **maintained secondary root** would be a registered folder, anchored by its canonical catalogue, whose eligible content participates in Index and route maintenance. For example, Index could discover a newly added document under `docs/team/`, complete its metadata and update its catalogue. Route commands could create, update, move and remove eligible content there.

That maintenance is the additional benefit. Reading the catalogue and its listed descendants already works. Current Index inventories `.agents`, and mutation selectors such as Route Update and Move also start there [E5].

A secondary root would have no separate loader, settings, installation or package lifecycle.

##### 2. Extend `route include` or not

Extend Include with an explicit option:

```text
open-forge route include docs/team/_team.md --maintain --description "Team context"
open-forge route exclude docs/team/_team.md
```

Plain Include retains its current preservation contract. `--maintain` requires an existing canonical folder catalogue and registers its parent as a maintenance boundary. It does not rewrite the tree during registration. A subsequent Index preview shows the proposed maintenance.

Exclude removes the loader row and either registration, without deleting content. Repeating plain Include on an already maintained root preserves that status. Demotion uses Exclude followed by plain Include.

Keep directory operands rejected initially. Reusing the catalogue operand avoids introducing a second entrypoint-selection rule. The existing file and folder rejection behavior is explicit in both code and contract [E2].

##### 3. The model

**Settings.** Keep Delivery 1’s v4 shape unchanged: optional `mainRoot`, plus the existing file registrations in `roots`. In the later delivery, add `maintainedRoots` as canonical catalogue paths under a new schema version.

The two registration lists are mutually exclusive. Promotion moves a catalogue from `roots` to `maintainedRoots` within the same mutation plan. Neither list supplies loading metadata. Loader rows remain authoritative for order, tags and conditions. An absent row remains absent.

This preserves the distinction already established for `roots`. Today, its codec validates external file paths and preserves existing declarations when rewriting [E6].

**Identity.** Keep existing short IDs for main-root content and native Skills. Secondary-root content uses exact `./workspace/path` identities, including the filename. This preserves identities when an included catalogue becomes maintained. Included sources already use this form [E4].

Do not derive unqualified short IDs independently in every root. Two folders can both contain `decisions/architecture.md`. Exact paths avoid that ambiguity without aliases or precedence rules.

**Command behavior.**

| Surface | Proposed treatment |
|---|---|
| Index | Default maintenance covers eligible listed roots. Explicit selection may maintain a registered but unlisted root without restoring its loader row. |
| Route List and Inspect | Show root provenance, registration and loading state. |
| Route mutations | Accept explicit paths inside registered maintenance boundaries. Initially keep moves within one root. Protect the registered root catalogue itself from deletion or relocation. |
| Context | Follow loader and catalogue rows, preserving conditions and physical-file deduplication. |
| Find | Search the selected source universe. Registration alone must not make content active. |
| References | Retain existing link semantics and report literal workspace paths across roots. |
| Status and Doctor | Distinguish included, maintained, unlisted, missing, unsafe and conflicting registrations. Report without repairing. |
| Install, Update and packages | Continue targeting the main root and native Skills. Secondary registration grants no implicit package-install permission. |

**Ownership and recovery.** Maintenance registration grants bounded navigation and metadata maintenance. It does not claim ownership of every file. Existing Framework, Extension, Library, overwrite and permission protections still apply.

Use the same workspace lease and recovery store for every root. Task 86 already derives both locations from the physical workspace under `.open-forge-state` [E7]. Recovery records retain literal target paths.

**Reserved locations.** Reject the workspace root, Git metadata, runtime state, fixed controls, `.agents` and overlaps with the main root or another secondary root. Validate physical aliases as well as lexical paths. Native Skills remain the explicit fixed-location exception. Registration must never turn a linked escape or an overlapping tree into writable content.

##### 4. Fit with the frozen Task 85 plan

The plan already has the right dependency direction. `WorkspaceLayout` is pure, Settings observes it, and mutation plans retain the observation for under-lease validation. It also already changes default selection from one scope to a collection [E8].

Make these amendments now:

- **Wave 0:** Represent content locations with a small typed descriptor containing path and role. Initially instantiate only main content and native Skills. Keep `SourceRoots` as the deduplicated inventory projection. Membership, short-ID derivation and implicit installation permission must remain separate decisions.
- **Wave 0:** Give every new property name, finding code, filename and reserved path one defining source. Freeze how tests consume those definitions.
- **Wave 1:** Preserve the root-neutral syntax and catalogue-carried layout already planned. Do not encode “exactly two roots,” positional root selection or universal prefix stripping into consumers.
- **Waves 2–5:** Stop ancestor traversal at the resolved maintenance boundary. Preserve external includes as read-only. Keep package mapping limited to its existing destinations.
- **Wave 6:** Retain activation last and the default-layout compatibility gate.

Keep v4 limited to `mainRoot`. Do not add an unused secondary-root field now. Validate legacy `roots` against the selected layout without dropping registrations during unrelated rewrites.

The later maintained-root delivery owns registration, secondary-tree inventory, route mutation support, permission rules and its schema change. Existing-workspace relocation remains Task 85 Delivery 2. Adding maintenance authority and moving files are separate changes with different failure evidence.

These amendments reduce predictable rework. They cannot establish that secondary-root support is already implemented.

##### 5. Risks

The highest risk is accidentally promoting every discovered include into writable content. A shared source catalogue must carry enough provenance to prevent that.

Other material risks are identity collisions, overlapping physical trees, lost loader membership, accidental default-root recreation, and treating root registration as package permission.

Older CLIs are a compatibility limit. The current settings definition explicitly tolerates unknown versions, so a schema bump cannot force old executables to refuse a custom layout [E6]. Document unsupported combinations and qualify supported versions. Do not promise protection from a minimum-version field an older CLI ignores.

Default behavior needs its own qualification. Passing custom-root cases does not prove unchanged default paths, files or output.

##### 6. Tests and audit of the frozen plan

The maintainer’s rules apply to both new tests and existing tests modified by Task 85.

**Unit evidence**

Use in-memory inputs for:

- Layout mapping, canonical syntax and reserved-path classification.
- Source identity, root membership and collision decisions.
- Settings decoding and mutation, including absent, lower and higher versions.
- Unknown-property preservation, unchanged declarations and omitted defaults.
- Loading selection, permission policy and generated-navigation planning.
- Typed findings, statuses, planned effects and serialization projections.

No Unit test should create a directory, resolve a real link, acquire a lease or invoke the host. Test physical observations in Integration.

**Integration evidence**

Use production modules with owned temporary resources to prove:

- Settings and layout observation, including no files created by reading.
- Real containment, aliases, overlapping roots and unavailable directories.
- Catalogue traversal, ordering, conditions and deduplication.
- One command’s plan, application, verification and preservation boundary.
- Settings or root changes between planning and application.
- Recovery preparation failure and interruption with truthful residual effects.
- Included-file preservation beside explicitly maintained content.
- Protection of managed files, Skills, controls and package destinations.

Compare independently observed before-and-after bytes for preservation. Assert typed outcomes and exact effect sets. Snapshot the human explanation.

**EndToEnd evidence**

Keep a small set of complete published-process journeys:

1. Fresh custom install, offline host-pointer traversal, Context and Index.
2. Include, remove its loader row, then Index, Update, Configure and Extension Install without reactivation.
3. Maintained-root registration, maintenance, discovery and exclusion with retained files.
4. Default-layout compatibility and one consequential blocked-write journey.

Detailed rejection combinations belong below this tier. PackageEndToEnd is needed when the wrapper or packaged delivery changes, not merely because another root exists.

**Required plan corrections**

- **Q1, high, qualification selection:** Packet Q runs `ti '*RouteInclusion*'`. That misses the actual `RouteIncludePlanningTests`, `RouteIncludeApplicationTests` and corresponding Exclude classes. Select the command scopes and shared inclusion scope explicitly. Record discovered and executed counts. Earliest affected boundary is Wave 5 packet acceptance [E9].
- **Q2, high, assertion compliance:** Existing Include planning tests assert literal rows and prose fragments. The published no-reactivation journey also searches rendered Markdown strings [E10]. Replace prose checks with reviewed snapshots. Parse structural membership through the existing document model where appropriate. Keep direct preservation and state assertions. Moving the same prose into constants does not satisfy the rule.
- **Q3, medium, tier allocation:** A3 combines observation cases with “matching unit and integration tests.” Specify that only pure classification belongs in Unit. Filesystem ancestry, pointer observation and stale-plan checks belong in Integration. The internal Install → Context → Index checkpoint is useful composition evidence, but is not published EndToEnd evidence before public activation [E8].
- **Q4, medium, qualification scope:** The four new published classes should prove journeys, not repeat every command’s branch matrix. Keep command-specific failures in their mirrored Integration scopes. The frozen plan’s broad “all command families” requirement needs this allocation [E11].
- **Q5, high, symbolic definitions:** EndToEnd and TestSupport intentionally have no production reference. Yet TestSupport copies the production data-home variable [E12]. Freeze a small build-time projection of owned public tokens into test support. Keep its generator tracked. Unit and Integration can use accessible defining types directly. Do not add production calls to EndToEnd or duplicate constants manually.
- **Q6, medium, snapshot limits:** The snapshot comparer ignores line endings [E13]. Retain byte comparisons for file preservation. Use reviewed snapshots for exact public wording and serialized output. Test any required line-ending policy separately without literal prose assertions.

Every fixture-specific name also needs one local definition. Expected results must remain independent of the transformation being tested. Reusing owned tokens must not become “expected = actual production mapper.”

Retain the plan’s fresh-artifact requirement, targeted serial snapshot updates, managed and Native AOT gates, and supported-platform qualification. Receipts must identify source, artifact, runtime target, selection counts, failures and skips. A managed pass does not establish native execution, and several execution modes on one host do not establish platform coverage [E11, E14].

##### 7. Work packets and estimate

Each packet includes its production changes, focused tests and reviewed snapshots. Assign one Astra high implementer per coherent packet.

| Packet | Ownership and completion evidence | Estimate |
|---|---|---:|
| Foundation amendment | Existing foundation owner. Layout descriptors, symbolic definitions and settings boundaries. Pure Unit evidence. | 1–2 days |
| Qualification correction | Test-contract projection, focused selections, Q1–Q6 allocation and affected assertion cleanup. Coordinate touched command tests with their owners. | 2–3 days |
| Later registration | Settings schema, Include/Exclude planning and application, root validation and deregistration preservation. | 2–3 days |
| Later source maintenance | Sources, generated navigation, Index and route mutations. Identity, ownership and boundary evidence. | 3–4 days |
| Later readers and reporting | Context, Find, References, Status and Doctor projections. | 1–2 days |
| Later qualification | Published journeys, native/platform receipts and documentation. | 2–3 days |

Allow **3–5 additional engineering days** around Delivery 1 for the amendments and evidence repair, subject to overlap with planned work. Allow **8–12 engineering days** for maintained secondary roots after Delivery 1. These are source-inspection estimates, not measured execution times.

#### Risks and what I would not build

I would not build named mounts, configurable native Skill locations, per-root settings, per-root locks, automatic include promotion, cross-workspace roots or cross-root moves in the first secondary-root delivery.

I would not use source scanning or a passing snapshot suite as proof of mutation safety. I would not replace detailed command evidence with one large lifecycle test.

This audit verified snapshot commit `7c86a8ba35dde2c3d579886346c7183bc29f7fda`. It did not inspect the active foundation worker’s changes. No files were edited, builds or tests run, or Doctor invoked.

#### Evidence

Paths below are repository-relative. Prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `I` = `src/cli/tests/integration/OpenForge.Cli.IntegrationTests`
- `E` = `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests`
- `S` = `src/cli/tests/support/OpenForge.Cli.TestSupport`
- `P` = `.agents/memory/emerging/analysis/main-root-council/plan.md`

| Reference | Supporting locations |
|---|---|
| E1 | `.agents/memory/emerging/analysis/route-include-council/record.md:59`, `:105`, `:132` |
| E2 | `docs/cli.md:939`, `:962`. `.agents/memory/crystallized/documents/cli/contracts/route/include/interface.md:37`, `:71`. `O/Route/Shared/Inclusion/Planning/RouteInclusionTargetResolver.cs:49` |
| E3 | `O/Route/Shared/Inclusion/Planning/RouteInclusionPlanBuilder.cs:83`, `:131`. `F/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:448` |
| E4 | `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:15`, `:79`, `:96` |
| E5 | `O/Index/Shared/Projection/IndexProjectionReader.cs:67`. `O/Route/Update/Shared/Planning/RouteUpdateSourceSelector.cs:42`. `O/Route/Move/Shared/Planning/RouteMoveSubjectSelector.cs:78` |
| E6 | `F/Settings/WorkspaceSettingsDefinitions.cs:17`, `:24`, `:48`. `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:16`, `:94` |
| E7 | `F/Workspace/Shared/Runtime/WorkspaceRuntimePaths.cs:7`. `F/Mutation/Locking/WorkspaceLockManager.cs:34`. `F/Recovery/Shared/Storage/RecoveryStoreLocator.cs:12` |
| E8 | `P:84`, `:110`, `:118`, `:217`, `:236`, `:269`, `:386` |
| E9 | `P:440`. `I/Commands/Route/Include/RouteIncludePlanningTests.cs:14`. `I/Commands/Route/Include/RouteIncludeApplicationTests.cs:13` |
| E10 | `I/Commands/Route/Include/RouteIncludePlanningTests.cs:35`, `:50`, `:131`. `E/PublishedRouteIncludeProcessTests.cs:53`, `:64` |
| E11 | `P:78`, `:530`, `:541`, `:545`, `:549` |
| E12 | `.agents/memory/crystallized/documents/cli/architecture.md:245`. `E/OpenForge.Cli.EndToEndTests.csproj:20`. `F/Filesystem/PhysicalPaths/CurrentUserDataHome.cs:15`. `S/Isolation/TestDataHome.cs:24` |
| E13 | `S/Snapshots/CommandOutputSnapshot.cs:8`, `:16`, `:24`, `:38` |
| E14 | `.agents/directives/open-forge/testing/evidence-integrity.md:13`, `:16`, `:18`, `:25`, `:30`. `.agents/patterns/testing/evidence-tiers.md:12` |

## P4: CLI architect

/root - GPT-6 - high

#### Recommendation

Keep Delivery 1 focused on one configurable main root plus the included root routes that already exist. Council 4 explicitly called those includes “multiple roots”, so the maintainer’s wording does not itself require a new filesystem model [E1]. Preserve `route include` as a read-only inclusion contract. Add explicit maintenance of secondary catalogue trees in a later delivery through `route maintain` and `route unmaintain`, without changing short IDs, package destinations or Delivery 1’s frozen API.

#### Answers

##### 1. What “multiple roots” should mean

Distinguish three things:

| Kind | Meaning |
|---|---|
| Main root | The single home of the loader and installed categories |
| Included root route | An existing file or catalogue listed by the loader |
| Maintained secondary root | A separately registered catalogue tree eligible for Index and route maintenance |

The first two are already the accepted design. Today, inclusion follows catalogue Entries without scanning the containing folder. External sources receive `./workspace/path` identities [E2].

The useful new capability is **maintaining another tree in place**. For example, Index could discover a newly added `docs/team/testing.md`, complete missing routed metadata and update `docs/team/_team.md`. Route commands could create, update and remove eligible content there. Today, Index inventories `.agents`, and Route Create requires an identity derived from that content space [E3].

A secondary root would have no loader, settings store, package installation target or independent lifecycle.

##### 2. Extend `route include` or not

Keep its public meaning unchanged:

```text
open-forge route include docs/team/_team.md --description "Team context"
```

It lists the file and records its inclusion. It does not authorize changing that file or its descendants. The current contract and planner limit changes to loader and settings [E4].

For the later maintenance capability, add:

```text
open-forge route maintain docs/team/_team.md --dry-run
open-forge route maintain docs/team/_team.md
open-forge route unmaintain docs/team/_team.md
```

`maintain` validates and registers an existing canonical catalogue. Registration itself changes only settings. The catalogue’s containing directory bounds maintenance. The first content changes come through an ordinary Index or route preview and application.

`unmaintain` removes that registration, including when the target is missing. It leaves files and loader rows unchanged. Both operations are idempotent.

Keep inclusion and maintenance independent. `route exclude` removes loading membership but does not withdraw maintenance authorization. `unmaintain` withdraws maintenance but does not stop reading an included catalogue.

I would not add `include --maintain`. Descendant discovery and maintenance are a different operation from listing one document. The CLI directive explicitly separates operations when their subjects, effects or lifecycle differ [E5].

##### 3. The model

**Settings.** Leave v4 unchanged. A later published schema adds one optional field. Schematic example:

```json
{
  "schemaVersion": 5,
  "mainRoot": "docs/forge",
  "roots": [
    "CONTRIBUTING.md",
    "docs/team/_team.md"
  ],
  "maintainedRoots": [
    "docs/team/_team.md"
  ]
}
```

`roots` remains a list of included file paths. Its existing meaning must not become “directories to scan” or “permission to write”. `maintainedRoots` contains unique canonical catalogue paths. Absence means empty. Neither list stores descriptions, tags, conditions or precedence.

The loader remains the authority for loading. A maintenance registration without a loader row is valid and unlisted. Registration never recreates a removed row. This preserves the present separation between settings registration and loader membership [E4].

Publish the new shape in the next schema directory. Preserve unknown properties and existing declarations during unrelated rewrites, following the existing codec pattern [E6].

**Identity.** Main-root and native Skill short IDs keep their frozen meaning. Secondary content keeps exact identities such as `./docs/team/testing.md`, including after maintenance registration. A catalogue keeps its filename in that identity.

There are no root aliases, secondary short IDs or search-order resolution. Two trees may both contain `testing.md` without collision. Existing main-root identity collisions remain errors [E7].

Preserve physical alias handling for reading. Reject maintenance registrations that overlap or resolve to the same physical tree. Do not choose a writable owner by registration order.

**Discovery and loading.** Maintain separate facts for:

- The location and identity of a source.
- Its loader or catalogue edges.
- Whether its path is eligible for maintenance.
- Existing ownership and permissions.

This separation matters in the current implementation. `SourceIncludedEntryReader` skips paths already inventoried before constructing included-source metadata. Simply adding secondary directories to inventory could therefore discard the row relationship and conditions that previously governed them [E2]. The later discovery packet must retain those edges when combining inventories.

**Command behavior.**

| Command | Proposed secondary-root behavior |
|---|---|
| Index | Inventory registered trees and maintain eligible routed Markdown. Bare execution follows listed roots. Explicit selection can maintain an unlisted registered catalogue without listing it again |
| Route List / Inspect | Show exact paths, loading membership and maintenance registration separately |
| Route Init / Create | Accept exact destinations inside registered trees. Stop ancestor creation at their registered boundary. Short-ID destinations remain in the primary layout |
| Route Update / Remove | Operate on eligible descendants using current ownership, reference, preview and recovery rules |
| Route Move | Initially support moves within one maintained tree. Refuse moving the registered anchor or crossing root boundaries |
| Context | Follow the existing loader, tags, conditions and catalogue chain. Registration alone adds no required reads |
| Find | Include maintained-tree inventory, including unrouted candidates, as native inventory does. Ordinary includes still cause no directory scan |
| References | Inspect authored links across the combined source universe. Entries remain navigation, and non-Markdown includes remain opaque |
| Status | Report main root, secondary maintenance registrations and listed, unlisted, missing or invalid states separately |
| Doctor | Diagnose unavailable anchors, unsafe paths, overlap and malformed maintenance structure without inferring or restoring registration |

Current Find consumes included row metadata. References excludes Entries links. Status and Doctor already distinguish registration from listing, providing useful extension points [E8].

**Content and ownership.** Registration grants bounded maintenance eligibility, not Framework or Extension ownership. Existing claims, exclusions, overwrite protection and permissions still apply. Do not add secondary trees to implicit package installation permissions.

Require the existing canonical catalogue and routed-file conventions for maintenance. Foreign catalogues remain usable through Include. Do not silently convert conflicting row policy into frontmatter. Preserve authored prose and unrelated metadata. Out-of-tree links remain readable links and never expand the maintenance boundary.

Registration does not change Git ignore rules or make documents private.

**Recovery and reserved paths.** Use the existing workspace lease, expected-state validation and recovery store. Revalidate registrations and physical ancestry before effects. Recovery records retain literal affected paths. Task 86 already fixes the lock and recovery beneath the physical workspace’s `.open-forge-state` directory [E9].

Reject secondary roots overlapping the main root, `.agents`, another secondary root, Git metadata or the runtime directory. Reject workspace-root registration, escapes and physical aliases into reserved locations. Keep native Skills at `.agents/skills`. Secondary trees cannot become another native Skill installation location.

##### 4. Fit with the frozen Task 85 plan

**No callable API change is needed now. No additional settings field belongs in v4.**

The frozen seam already provides the important preparation:

- Pure `WorkspaceLayout`, with settings observation outside Workspace.
- Layout carried through catalogues, sessions and mutation plans.
- Root-neutral logical-path syntax.
- Literal external identities.
- Multiple default selection scopes.
- Under-lease layout revalidation [E10].

Make three clarifications in the execution contract now:

1. `SourceRoots` describes Delivery 1’s primary content locations. Consumers must not infer loading membership or unrestricted mutation authority from membership.
2. `SourcePath`, `CategoryDirectory` and `MapPayloadPath` remain primary-layout mappings. They never choose among included or secondary roots.
3. `IsImplicitContentDestination` remains separate from any future secondary-tree maintenance permission.

These clarify the existing seam. They do not require a speculative mount abstraction.

**Wave changes now:**

- **Wave 0:** retain frozen signatures and v4. Include the three boundary clarifications in foundation acceptance.
- **Wave 1:** preserve included `./path` identities, aliases and conditions through catalogue reconstruction.
- **Waves 2–5:** retain command-specific selection and mutation checks. Confirm custom-loader relative links and native Skills are classified correctly.
- **Wave 6:** qualify several included roots alongside a nested custom main root. Activate Install last, as planned.

These cases fit existing packets B2, C1, D1, D2 and Q [E10].

**Later delivery:** add immutable secondary-root descriptors to the same layout carrier, populated by its Settings reader. Preserve the existing factory and primary mapping methods. Add a narrow maintenance-boundary lookup and extend shared inventory formation. Commands then consume those facts through the carriers already introduced by Delivery 1.

This requires new feature implementation, but no redesign of Delivery 1 behavior or another round of layout plumbing.

Keep Task 85’s Delivery 2 meaning unchanged. It remains relocation of an established main root. Secondary maintenance should be a separate delivery after Delivery 1, with its own acceptance decision [E11].

##### 5. Risks

The largest risk is treating registration, discovery, loading and write authority as the same fact.

Other concrete risks are losing included-row conditions during inventory merging, changing exact identities, scanning an unexpectedly broad directory, and letting secondary registration authorize package writes.

Older CLIs tolerate unknown settings keys and versions. A schema number therefore cannot enforce compatibility [E6]. Document older CLIs as unsupported for the new maintenance capability, just as Task 85 does for custom main roots. Do not claim that a minimum-version property would make them safe.

Preserve default-layout behavior and bytes through the existing Task 85 compatibility gate [E11].

##### 6. Tests

Apply the maintainer’s stricter rules throughout. Folder names, property names, finding codes, environment variables and other owned values come from their defining symbols. Test fixtures own their scenario-specific names. No literal, substring or pattern assertions against user-facing wording. Exact wording uses reviewed snapshots.

**Unit:** in-memory settings parsing and preservation, identity derivation, overlap policy, boundary lookup, selection, no-op plans and typed findings. Prove that maintenance registration does not change short IDs, loader policy or package mapping.

**Integration:** one real boundary per test in an owned temporary workspace. Cover registration and deregistration, physical containment, aliases, missing anchors, discovery merging, Index effects, stale settings under the lease and recovery preparation failure. Compare untouched file bytes directly.

Include a decisive regression where the same catalogue is both included and maintained. Its inherited conditions and row metadata must survive inventory merging.

**EndToEnd:** small complete journeys through fresh delivered executables:

1. Install a custom main root, include two catalogues, read them, then Index without modifying included files.
2. Register maintenance, create and index a descendant, query it, withdraw maintenance and verify further writes refuse.
3. Remove a loader row, then run Index, Update, Configure and Extension installation. It stays unlisted.
4. Preserve default-layout paths, bytes, ownership and output.

Keep detailed invalid-input combinations below EndToEnd. Use the existing separate projects and managed/Native AOT qualification requirements. Run required platform evidence on actual supported hosts. These boundaries follow the repository’s evidence rules [E12].

##### 7. Plan

Each packet has one Astra high implementation owner, including its focused evidence.

| Packet | Ownership | Estimate |
|---|---|---:|
| Delivery 1 seam review | Foundation contract clarifications and existing-packet acceptance cases | 0.5–1 day |
| Later settings and registration | New schema, codec, pure descriptors, Maintain/Unmaintain operations and recovery attribution | 2–3 days |
| Inventory and navigation | Secondary discovery, identity preservation, inclusion edges, Index and metadata boundaries | 2–3 days |
| Route maintenance | Exact-path Init/Create/Update/Move/Remove and bounded ancestor handling | 2–3 days |
| Reading and diagnostics | Context, Find, References, List/Inspect, Status and Doctor integration | 1–2 days |
| Qualification and documentation | Complete journeys, default parity, native/platform gates and contracts | 2 days |

The later capability is approximately **9–13 engineering days**. This is an architectural estimate, not measured implementation evidence. Resolve settings and source semantics before splitting command work. Keep the frozen Delivery 1 estimate and relocation estimate separate.

#### Risks and what I would not build

I would not build a root-name namespace, precedence system, second loader, second settings store, per-root lock, package destination selector or general mount table.

I would not turn existing `roots` entries into writable trees, accept directories through Include, or automatically discover roots from filesystem contents.

I would defer cross-root moves and registered-root relocation. They introduce reference, ownership and recovery work beyond maintaining a bounded tree.

If the maintainer only wants Council 4’s original “multiple roots”, stop after Delivery 1 qualification. The existing Include model already provides that capability.

#### Evidence

Inspection used commit `7c86a8ba35dde2c3d579886346c7183bc29f7fda`. No files were edited, builds or tests run, or Doctor invoked. The in-flight Wave 0 implementation was not present in this snapshot.

- **E1:** `route-include-council/record.md:59` defines the original “multiple roots” meaning.
- **E2:** `SourceIncludedEntryReader.cs:15`, `SourceIncludedEntryReader.cs:74` and `SourceReferenceParser.cs:26`.
- **E3:** `IndexProjectionReader.cs:67` and `RouteCreateTargetPlanner.cs:20`.
- **E4:** `include/interface.md:67`, `RouteInclusionPlanBuilder.cs:83`, `GeneratedNavigationRegionPlanner.cs:153` and `docs/cli.md:939`.
- **E5:** `composable-operation.md:19`.
- **E6:** `WorkspaceSettingsCodec.Roots.cs:16`, `WorkspaceSettingsCodec.cs:18` and `WorkspaceSettingsCodec.cs:84`.
- **E7:** `SourceIdentity.cs:14` and `SourceCatalogueReader.cs:493`.
- **E8:** `FindLayerInspector.cs:51`, `ReferencesLinkExtractor.cs:45`, `RouteIncludedFileReader.cs:42` and `WorkspaceIncludeDoctorInspector.cs:14`.
- **E9:** `WorkspaceRuntimePaths.cs:7` and `RouteInclusionApplication.cs:35`.
- **E10:** `main-root-council/plan.md:84`, `plan.md:177`, `plan.md:275` and `plan.md:440`.
- **E11:** `main-root-council/record.md:44`, `plan.md:530` and `plan.md:587`.
- **E12:** `evidence-integrity.md:11`, `evidence-integrity.md:28` and `evidence-tiers.md:11`.

## P5: User and agent experience designer

/root - GPT-6 - high

#### Recommendation

Keep one main root and one loader, with additional maintained context folders inside the workspace. Reuse `route include` to expose their catalogues, and add `route maintain` and `route unmaintain` to control maintenance explicitly. Preserve `./path` identities for additional folders, so enabling maintenance does not rename existing sources. Make a small amendment to Task 85’s layout boundary now, then deliver additional maintained roots separately. Agents should discover every listed root through ordinary Markdown links without reading settings or needing the CLI.

#### Answers

##### 1. What “multiple roots” should mean

An additional root is a workspace folder whose navigation Open Forge may maintain. It has one catalogue, ordinary documents and optional nested catalogues. It has no separate loader, installation, settings file or runtime store.

Today, an included catalogue already supplies another reading entrypoint. The reader follows its `Entries` without scanning its folder and assigns its sources `./path` identities. Include itself rejects directory operands. Index inventories `.agents` and preserves authored links rather than maintaining the linked folders. Route creation also requires an identity derived from the native source tree. These are implementation boundaries, not merely missing documentation. (E1–E4)

The new capability is therefore concrete:

- Index discovers eligible Markdown and maintains navigation inside `docs/team`.
- Route commands create, initialize, update, move and remove eligible content within that folder.
- Status distinguishes whether the folder is maintained and whether its catalogue is listed.
- Existing documents stay where they are.

If the person only wants agents to read an existing catalogue, ordinary Include remains sufficient.

##### 2. Extend `route include` or not

Keep Include’s existing file operand and preservation promise. Adding recursive maintenance changes its effects substantially. The current contract permits changes only to the loader and settings, while preserving the target’s bytes. The command-design directive also separates operations whose effects differ. (E1, E5)

Add two leaves to the existing Route group:

| Command | Proposed effect |
|---|---|
| `route maintain <folder>` | Register a maintenance boundary and create its canonical catalogue if missing. Preserve an existing catalogue. |
| `route unmaintain <folder>` | Remove that registration. Preserve files and loader rows. |
| `route include <catalogue>` | List the catalogue using the existing description, tags and conditions. |
| `route exclude <catalogue>` | Remove its loader rows and include registration. Leave maintenance registration intact. |

Both new commands support `--dry-run`, deterministic noninteractive execution and verified no-ops. Maintenance registration does not perform a recursive Index implicitly.

A person can preview and register `docs/team`, preview Index’s changes, then include `docs/team/_team.md`. Someone with an existing include can enable maintenance without changing the loader row.

Install keeps Task 85’s single question about the main context folder. Do not add a root-selection wizard. Explain additional folders in `help concepts`, and cross-link the four operations from their command help.

Use plain distinctions: **listed for agents** and **maintained by Index**. Avoid calling everything “managed”, since package ownership is a different fact.

##### 3. The model

**Settings.** Keep `mainRoot` and `roots` unchanged in meaning. Introduce a separate `maintainedRoots` array in the later schema:

```json
{
  "mainRoot": "docs/forge",
  "roots": [
    "CONTRIBUTING.md",
    "docs/team/_team.md"
  ],
  "maintainedRoots": [
    "docs/team"
  ]
}
```

This is a schematic fragment. `roots` still contains included **files**. `maintainedRoots` contains canonical workspace-relative **directories**. Its catalogue name follows the existing canonical naming owner.

Descriptions, tags, conditions and loading order remain in loader rows. Registration never recreates a deleted row. An included catalogue may belong to a maintained root, so validation must permit that combination.

**Identity.** Main-root short IDs remain stable, as Task 85 requires. Native Skills keep their existing IDs. Additional-root sources retain exact identities such as `./docs/team/testing.md`, including after maintenance is enabled or removed. The current include reader already uses this identity form. (E2, E6)

Do not introduce root aliases, qualified short IDs or search-order precedence. Equal basenames in separate folders are harmless because their paths differ. Reject overlapping maintenance boundaries and registrations that resolve to the same physical folder. Preserve existing physical-file deduplication during reading.

**Command behavior.**

| Surface | Proposed behavior |
|---|---|
| Index | Maintain registered folders using existing metadata, navigation, exclusion and ownership rules. Never follow an outgoing catalogue link as permission to write outside its registered boundary. Never restore loader membership. |
| Route Init/Create/Update | Accept exact paths inside additional roots. Stop ancestor scaffolding at the registered root catalogue. |
| Route Move/Remove | Operate on eligible descendants with existing reference and recovery checks. Initially reject moving or deleting the registered root itself and moving content across root boundaries. |
| Context | Follow loader order, conditions and selected catalogue chains. Registration alone adds no startup reads. Explicit source selection retains its documented meaning. |
| Find | Discover maintained content and ordinary included sources. Show exact identities and distinguish unlisted branches from startup context. |
| References | Resolve links across roots using workspace paths. Reading a linked source grants no mutation authority. |
| Route List/Inspect | Show the actual path, catalogue chain, listing state and maintenance state. |
| Status | Name the main root and summarize additional folders with separate listing, maintenance and availability facts. Count each physical document once. |
| Doctor | Diagnose missing catalogues, unsafe or overlapping registrations and stale navigation. Supply a concrete path and next action. Do not repair or re-list automatically. |

Listing and maintenance are independent. Default Index maintains registered folders even when their loader rows are absent. `route unmaintain` stops that maintenance. This distinction must appear in previews and help.

Current Status already separates listed, unlisted and missing includes. Doctor already reports missing targets and unlisted registrations. Extend those facts rather than replacing them with one ambiguous root status. (E7)

**Ownership and recovery.** Maintenance registration does not establish Framework, Extension or Library ownership. It authorizes the maintenance behavior of relevant commands within a bounded folder. It grants no blanket package-install permission.

Keep one workspace lease and operation-local recovery plans with literal target paths. Task 86 already places the lease file and recovery bundles under `.open-forge-state` at the physical workspace root. Additional roots must not change that. (E8)

Keep settings and ownership records in `.agents`, native Skills in `.agents/skills`, and host pointers at the workspace root. Reject roots that overlap the main root, another additional root, `.agents`, Git metadata or the runtime store. Validate lexical and physical containment. Respect existing package ownership boundaries inside otherwise eligible folders.

**Without the CLI.** For a main loader at `docs/forge/loader.md`, an ordinary row can be:

```markdown
- [Team engineering context](../team/_team.md)
```

The agent follows that link and its catalogue chain. An explicitly supplied loading tag or condition has the existing meaning. Neither maintenance status nor settings changes the reading procedure.

Retain the host pointer to the selected loader, relative Markdown destinations, inherited conditions, required reads and missing-file errors. The current loader already defines this procedure and its README fallback. No root manifest should become another required startup read. (E9)

##### 4. Fit with the frozen Task 85 plan

Preserve Delivery 1’s outcome: one configurable main root selected at first installation, with complete command support before public activation. Preserve its refusal to relocate an established installation. (E6)

Amend the foundation before Wave 1 depends on it:

1. **Give source locations an explicit role.** Replace bare entries in `WorkspaceLayout.SourceRoots` with small immutable descriptors containing the path and role. Delivery 1 needs only main content and native Skills. Preserve the single scan of `.agents` in the default layout.
2. **Expose the containing maintenance boundary.** Navigation and scaffolding need the boundary where ancestor traversal stops. They must not infer it from path depth or array position.
3. **Keep short-ID mapping narrow.** Rename or explicitly constrain `TryGetSourceRelativePath` to main-root and native-Skill ID mapping. Adding an inventoried location later must not automatically give its contents short IDs.
4. **Separate content membership from package permission.** `IsImplicitContentDestination` must keep its own main-root/native-Skill policy. It must not become an alias for membership in any source root.
5. **Keep layout pure.** Settings resolves the layout once. Sources and plans consume it. Revalidation retains the existing observation boundary.

These changes refine the frozen API’s two existing location roles. They do not require a mount engine or a configurable policy framework. The plan already transports Layout through catalogues, sessions and mutation plans. (E10)

**Settings v4 needs no additional public field.** Ship only the accepted optional `mainRoot`. Preserve `roots` as file registrations. In particular, its validation must exclude primary content locations without assuming that every future inventoried location is forbidden.

Apply the amendment across the existing waves:

- **Wave 0:** location descriptors, boundary queries and settings validation.
- **Waves 1–2:** identity, catalogue transport and navigation consume those boundaries.
- **Waves 3–5:** commands consume the shared answers without new root-specific path arithmetic.
- **Wave 6:** retain the existing activation gate, output compatibility checks and documentation work.

The later delivery adds `maintainedRoots`, the two commands, additional-root inventory and command eligibility. Publish a new settings schema then rather than changing a shipped v4 schema.

Keep main-root relocation as Task 85’s existing Delivery 2. Additional maintained roots are a separate delivery and do not require relocation to ship first.

##### 5. Compatibility risks

Existing `roots` values must never be promoted into writable folders automatically. Their present meaning is file registration only. (E11)

Older CLIs cannot be made safe merely by increasing `schemaVersion`. The current settings model explicitly reports unfamiliar versions without enforcing them. Document the required CLI version for custom and maintained roots. Do not claim that a minimum-version field protects users of older binaries. (E11)

Preserve these defaults:

- No extra registration means no extra inventory or maintenance.
- Existing includes retain their paths, row metadata and preservation behavior.
- Adding maintenance does not add required reads.
- Removing a loader row does not cause later commands to recreate it.
- Default-layout bytes and ordinary output remain unchanged except the accepted Task 85 changes.

The highest-risk implementation mistake is treating “belongs to a source root” as permission to write, install packages or load instructions.

##### 6. Tests

Follow the repository’s tier boundaries and the maintainer’s stricter literal rules. (E12)

**Unit:** Pure layout classification, root-overlap policy, exact identity preservation, request binding, settings transformations and typed result projections. Prove that short-ID mapping and implicit package permission do not expand with inventory membership.

**Integration:** Exercise one command or capability against owned temporary files. Cover registration, catalogue creation, Index’s bounded changes, ordinary route operations, conditional catalogue traversal, physical aliases, ownership restrictions and stale settings under the lease. Assert unchanged bytes outside planned targets and retained recovery facts after interruption.

**EndToEnd:** Use a small set of complete journeys through the delivered executable:

- Install a custom main root, register and include a second folder, create content, index it, then find and load it.
- Enable maintenance for an existing included catalogue and prove that identities and loading policy remain unchanged.
- Remove its loader row, run maintenance and lifecycle commands, and prove it stays unlisted.
- Stop maintenance and prove that existing catalogue reading still works.
- Repeat the default-layout compatibility journey.

Add an offline acceptance exercise using installed artifacts. Follow both host pointers through the loader and two conditional catalogue branches without the CLI. Compare the ordered reads with Context using independently specified scenario expectations. Repeat without the README.

Production and tests reference the owning definitions for filenames, settings keys, findings, environment variables and other contract values. Scenario-specific values have one fixture owner. Assert semantic facts through typed values and observed effects. Check all exact human wording only through reviewed snapshots, with no literal, substring or pattern assertions.

Run the accepted managed and Native AOT gates on the required platforms. Keep execution receipts tied to fresh artifacts. This proposal claims no execution evidence.

##### 7. Work packets and estimate

One Astra high implementer owns each packet and its corresponding tests.

| Packet | Ownership and acceptance boundary | Estimate |
|---|---|---:|
| Delivery 1 amendment | Workspace layout types, Settings observation and the frozen packet signatures. Foundation owner integrates before dependent waves. | 1–2 days |
| Registration lifecycle | Later schema, Maintain/Unmaintain operations, composition, previews and recovery. | 2–3 days |
| Content operations | Sources, generated navigation, Index and ordinary route commands. Exact identities and bounded writes are the acceptance conditions. | 3–4 days |
| Experience | Status, Doctor, List/Inspect projections, OutputText, help, loader examples and CLI Skill guidance. | 1–2 days |
| Qualification | Complete journeys, offline reading exercise, compatibility checks and platform qualification. | 2–3 days |

Budget **1–2 additional engineering days within Delivery 1**, then **8–12 engineering days for maintained roots**, beyond the frozen Task 85 estimate. These are planning estimates, not measurements.

Freeze registration, identity and permission contracts before splitting implementation. Integrate one complete additional-folder journey before extending the remaining command surfaces.

#### Risks and what I would not build

The main experience risk is confusing reading, maintenance and package ownership. Keep those facts distinct in help, previews and Status.

I would not build multiple loaders, root precedence, root aliases, per-root settings, automatic folder discovery, remote roots, separate locks, automatic adoption of existing includes or package relocation into additional roots.

I would also defer overlapping roots and cross-root moves. They add substantial identity and recovery questions without helping the first useful journey.

#### Evidence

Inspected snapshot: `7c86a8ba35dde2c3d579886346c7183bc29f7fda`. The foundation worker’s live progress was not verifiable from this snapshot. No files were edited and no builds, tests or Doctor runs were performed.

Prefixes below expand from the repository root:

- **F:** `src/cli/framework/OpenForge.Cli.Framework/Framework`
- **O:** `src/cli/operations/OpenForge.Cli.Operations/Commands`
- **M:** `.agents/memory/emerging/analysis`
- **C:** `.agents/memory/crystallized/documents/cli/contracts`

| Reference | Verified locations |
|---|---|
| E1 | `docs/cli.md:939`, `docs/cli.md:962`, `C/route/include/interface.md:20`, `C/route/include/interface.md:69`, `O/Route/Shared/Inclusion/Planning/RouteInclusionTargetResolver.cs:49` |
| E2 | `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:62`, `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:96` |
| E3 | `O/Index/Shared/Projection/IndexProjectionReader.cs:67`, `F/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:448` |
| E4 | `O/Route/Create/Shared/Planning/RouteCreateTargetPlanner.cs:30`, `O/Route/Init/Shared/Planning/RouteInitTargetPlanner.cs:239`, `O/Route/Update/Shared/Planning/RouteUpdateSourceSelector.cs:43` |
| E5 | `.agents/directives/cli/composable-operation.md:23`, `.agents/directives/cli/composable-operation.md:34` |
| E6 | `M/main-root-council/record.md:44`, `M/main-root-council/record.md:48`, `M/route-include-council/record.md:179` |
| E7 | `O/Status/Shared/Aggregation/StatusContextAggregator.cs:15`, `O/Doctor/Shared/Domains/WorkspaceIncludeDoctorInspector.cs:14` |
| E8 | `F/Workspace/Shared/Runtime/WorkspaceRuntimePaths.cs:7`, `F/Mutation/Locking/WorkspaceLockManager.cs:34`, `F/Recovery/Shared/Storage/RecoveryStoreLocator.cs:12` |
| E9 | `.agents/loader.md:11`, `.agents/loader.md:33`, `.agents/loader.md:38` |
| E10 | `M/main-root-council/plan.md:91`, `M/main-root-council/plan.md:110`, `M/main-root-council/plan.md:217`, `M/main-root-council/plan.md:236` |
| E11 | `F/Settings/Models/Document/WorkspaceSettingsDocument.cs:32`, `F/Settings/Models/Document/WorkspaceSettingsDocument.cs:41`, `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:94` |
| E12 | `.agents/directives/open-forge/testing/evidence-integrity.md:12`, `.agents/directives/open-forge/testing/evidence-integrity.md:28`, `.agents/patterns/testing/evidence-tiers.md:11` |
