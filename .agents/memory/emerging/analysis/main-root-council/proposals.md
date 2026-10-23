---
open-forge:
  description: The six Stage 1 proposals of the main root council, kept as evidence
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Layout, Install, Settings]
---

# Main Root Council Proposals

The six independent Stage 1 proposals, each under its Stage 2 label and lens. Citations point at commit `d024f5087`. The [record](record.md) holds the decisions.

## P1: User and agent experience designer

/root - GPT-6.1 - high

#### Recommendation

Make the install choice a **context folder**, defaulting to `.agents`. Keep settings, ownership and native Skills in `.agents`, and Task 86 runtime state in `.open-forge-state`. Deliver fresh custom-root installation first, with every existing command supporting that layout. Deliver reviewed relocation next, keeping Task 85 open until it ships. Agents must reach the correct loader through readable root pointers without needing the CLI.

#### Answers

##### 1. What the plan gets wrong today

Path prefixes used below are expanded in Evidence.

- **The #6 prerequisite is complete.** Generic `./<workspace-path>` parsing exists at `F/Sources/Identity/SourceReferenceParser.cs:26`. Include, Exclude and Help definitions exist at `O/Route/Include/RouteIncludeDefinitions.cs:9`, `O/Route/Exclude/RouteExcludeDefinitions.cs:9` and `O/Help/HelpDefinitions.cs:7`. Convert these consumers instead of scheduling their creation.
- **There is no authoring-guide member to migrate.** `F/Sources/Identity/SourceLogicalPath.cs:15` defines the human README. `O/Help/HelpOperation.cs:64` obtains file formats from that bundled README. Delete `AuthoringGuidePath` and `authoring.md` from the proposed API and packets.
- **The user-facing language changed.** Status says “listed”, “unlisted”, “missing” and “hand-written”, not on/off (`T/Status/StatusWording.cs:8`). Preserve that vocabulary.
- **Installation already has distinct setup and metadata choices.** The fresh interactive default is Minimal (`O/Install/Shared/Configuration/InstallSetupResolver.cs:63`). “Root keys” means metadata placement (`T/Install/InstallSetupText.cs:8`). The Install contract still says Essentials and omits Minimal from its syntax (`C/contracts/install/interface.md:76`, `:109`). Reconcile that drift rather than copying it into new help.
- **The native CLI Skill is another startup surface.** It directs agents to `.agents/README.md`, and its common reference still describes only `.agents/...` exact paths (`src/open-forge/.agents/skills/open-forge-cli/SKILL.md:10`, `references/common.md:21`). Fix these instructions alongside host pointers.
- **Task 86 changes the recovery dependency.** Runtime paths are workspace-fixed. Recovery observations carry store locations, and the old `ReadAsync(workspace)` becomes local-only. Migration must consume the appropriate combined observation and preserve Task 86’s Install-specific blocking rules (`L:75`, `:235`, `:297`).
- **322 is an audit count, not an edit budget.** The old plan explicitly includes comments and pass-through consumers (`P:29`). It does not justify 322 independent edits.

This is a focused code and contract audit, not verification of every census entry.

##### 2. The smallest correct design

Ship one configurable knowledge directory and one fixed native Skills directory. Keep `roots` as the existing registration list for included files. Do not introduce a public mount system.

The first delivery must support custom-root Context, discovery, Index, routes, packages, Update, Remove, Repair and observations. Fresh installation is not complete if the next maintenance command recreates `.agents/loader.md`.

Defer relocation, combined ownership translation and interruption handling to the next delivery. This changes sequencing, not the accepted outcome. Until relocation ships:

- A different root on an installed workspace fails before effects.
- Same-root configuration continues normally.
- Help states that this release chooses the folder during first installation.
- Errors never recommend an unavailable relocation command or manual settings edits.

Keep relocation’s real-file inventory, collision refusal, private-file protection, byte verification and recovery evidence. Cut generic continuation, automatic rollback, arbitrary prose rewriting and mixed Library projection support.

##### 3. The layout seam

Freeze one immutable `WorkspaceLayout` under `F/Workspace/Layout/`. Keep it separate from `CliWorkspace`, whose physical identity also anchors Task 86.

The foundation exposes:

```text
WorkspaceLayoutResolver.ReadAsync(workspace, cancellation)
    -> WorkspaceLayoutRead

WorkspaceLayoutRead
    SettingsRead, Layout, State, Issues

WorkspaceLayout
    MainRoot, LoaderPath, HumanGuidePath
    NativeSkillsRoot, NativeSkillsCataloguePath
    CategoryDirectory(categoryId)
    MapPayloadPath(packagePath)
    TryGetSourceRelativePath(path)
    IsMainRootPath(path), IsNativeSkillPath(path)
```

`State` distinguishes ready, uninstalled, invalid, mismatch and unavailable. Issues carry exact paths and causes.

Resolve once at operation entry. Carry the layout through `SourceCatalogue`, `SourceReadSession` and command planning facts. Status and Doctor share the same observation. Rendering receives concrete paths through Operations results and never reads settings.

Keep fixed control paths in their existing owner and runtime paths in Task 86’s owner. Package `.agents/...` coordinates remain portable package coordinates. Map them before destination permission checks and ownership publication.

Source IDs remain `memory/crystallized` and `skills/example`. Exact paths remain literal. `./docs/forge/...` selects a custom-root file. `.agents/...` never redirects.

Absent `mainRoot` follows the existing `.agents` path, with unchanged ordering, bytes and ordinary output. Explicit compatibility exceptions are the new wizard question, advertised help and newly created v4 settings. Existing settings declarations remain preserved. Default snapshots must compare actual output without hiding root paths.

##### 4. Settings and validation

Publish v4 with optional `"mainRoot": "docs/forge"`. Absence means `.agents`. Leave published v1–v3 schemas unchanged.

Accept a canonical workspace-relative directory. CLI input may normalize separators and a leading `./`. Reject empty or null values, absolute paths, traversal, control characters, Git metadata, runtime storage and control locations. Permit `.agents` as the default exception, but reject custom descendants of `.agents`. Validate physical ancestry and refuse escaping or ambiguous aliases.

For the first delivery, require a missing or empty custom destination. This avoids treating an existing documentation tree as newly managed content. Include existing documents through `route include`.

Validate `roots` against the selected main root, native Skills and reserved paths. Do not retain today’s blanket rejection of every `.agents/...` path (`F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:16`).

When settings and files disagree:

- Report the configured loader and conflicting observed paths.
- Read only the selected loader, with an incomplete or mismatch finding where appropriate.
- Block layout-dependent writes.
- Never discover a replacement loader silently.
- Preserve independent Help and Task 86 recovery inspection and cleanup.

A fresh install may establish an absent layout. A missing loader in an established workspace is not a fresh-install invitation.

Older CLIs accept unknown keys and do not reject newer schema numbers (`F/Settings/WorkspaceSettingsDefinitions.cs:17`, `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:18`). They therefore ignore `mainRoot`. Schema v4 cannot provide a retrospective safety barrier. Document the actual minimum release, warn against older CLI maintenance, and retain plain-file startup.

##### 5. Recommendations for every open question

| Plan section 8 question | Recommendation |
|---|---|
| Incomplete #6 | Closed by current source. Integrate the released Include, Exclude and Help implementations. |
| Default bytes versus v4 | Preserve existing defaults and declarations. Name wizard, help and new-file schema exceptions explicitly. |
| Partial migration | Retain recovery evidence and report exact residuals. Block blind retries. No replay engine. |
| Authored ignore rules | Preserve their bytes. Block relocation when currently private files would lose protection. Report the affected paths and require a reviewed correction. |
| Library registration straddles the move | Block relocation. Offer narrower registrations with concrete source/destination guidance. Do not invent lock schema v2 for this feature. |
| Library source inside the old root | Pin the source subtree. Preserve its bytes and location. Block overlap with movable or managed content when separation is ambiguous. Explain any retained old-root subtree in the preview. |
| Custom-root `skills` collision | Reserve the native `skills` identity. Reject a second routed Skills category. |
| Older CLI | State minimum support in custom-root guidance and documentation. Do not claim old binaries will refuse safely. |
| Competing loaders or edited settings | Require an explicit, evidence-backed source layout. Never choose the nearest or newest loader. |
| Old paths in prose | Rebase supported parsed links and managed pointers. List residual literals for review without rewriting prose automatically. |

The Library restriction follows the current single-destination-root mapping (`F/Libraries/LibraryPathIdentity.cs:10`). Recovery handling must preserve Task 86’s distinction between historical artifacts and current-operation requirements.

##### 6. The user’s experience

**Install question**

Ask before presenting destination-dependent route choices:

> Where should Open Forge keep its context files?  
> Context folder: `.agents`

Supporting text:

> A folder inside this workspace, such as `docs/forge`. Settings stay in `.agents`. Skills, when selected, stay in `.agents/skills`.

Keep the accepted question and use “context folder” as its label. Do not call it “multiple roots” in the wizard. That phrase does not tell users whether they are moving files or including existing documents.

An explicit `--main-root` supplies the answer. Preview, JSON and unattended execution never prompt. Existing-workspace configuration retains its current root unless explicitly changed. Preserve the existing preset and metadata defaults.

The review shows the selected folder, loader, actual Skill destination and root pointers. A relocation review additionally shows source → destination, retained subtrees, ignore changes and blockers. Cancellation creates nothing.

**Help and status**

Use this option description:

> `--main-root <directory>` — Choose the workspace folder for Open Forge context files. Defaults to `.agents`.

Add relocation wording only when it ships.

Extend `help concepts` with “Workspace layout”. Explain `mainRoot`, included `roots`, fixed controls, native Skills and runtime state. Keep Help workspace-independent, as its current embedded README implementation allows (`O/Help/HelpOperation.cs:64`).

For a custom root, Status starts with:

```text
Context folder: docs/forge
Loader: docs/forge/loader.md
```

Show controls and Skills in detailed output. Preserve Task 86’s separate runtime reporting. Reuse existing loader coordinates and add only the structured layout facts consumers need. Avoid new default-layout noise.

**Root pointers and agent startup**

Render the existing AGENTS managed block with the selected loader:

> Before you start or resume a task, read `docs/forge/loader.md` and follow it until the task is done.

Keep the existing Context shortcut and file-tool fallback. Add a short minimum-CLI-version note only for custom layouts.

Keep CLAUDE as the existing bridge:

```text
@AGENTS.md
@docs/forge/loader.md
```

Do not duplicate policy there. Preserve all bytes outside recognized managed regions. Update the maintenance contract that currently requires the literal default import (`C/../maintenance/payload/CLAUDE.md:17`).

The agent then follows the selected loader’s relative links. Native Skills appear through `../../.agents/skills/_skills.md` when selected. Included documents retain their workspace paths. Working-file globs stay workspace-relative. Neither the directory name nor the presence of files activates a route.

Custom installation must also render correct operational examples in the README and CLI Skill. A fixed `.agents/README.md` instruction would undermine otherwise correct pointers. Keep README deletion and `help concepts` fallback working.

**Docs site**

Add one custom-folder walkthrough to Installation. Link it from routing, customization, CLI flows and the glossary. Update `docs/cli.md`, the root README and Skill references. Keep default examples labeled as defaults instead of rewriting every example.

The walkthrough must show the resulting tree, both startup pointers, a first rule under the custom root, an included document, and the fixed folders. Later add a separate relocation walkthrough.

##### 7. Implementation plan

Freeze the API above and the behavior matrix before assigning Astra high implementers. One foundation owner controls shared signatures. Each packet owns its mirrored tests and a bounded file list in a separate worktree. Split large command families by leaf, not by production versus tests.

| Wave | Packets and ownership | Required evidence |
|---|---|---|
| 0 | Foundation owner: new layout files, settings document/codec, v4 schema, shared source and permission contracts | Validation, declaration preservation, default mapping, Task 86 reservation |
| 1 | Separate source identity and source-reading packets | Stable IDs, literal paths, overwrites, custom loader and native Skills |
| 1 | Separate payload/navigation packet | Default bytes, mapped payload, frontmatter forms, relative links, private Entries |
| 2 | Install packet: binding, request, setup resolver and planning | Fresh custom install, occupied destination, same-root configure, changed-root refusal |
| 2 | Install presentation packet after result shapes freeze | Question, cancellation, narrow terminal, line input, preview/JSON/automatic no-prompt behavior |
| 3 | Separate route, Extension and Library leaf packets | Custom destinations, fixed Skills, actual ownership, unchanged included files |
| 3 | Separate Update, Remove, Repair and observation packets | No recreated default loader, correct paths, mismatch handling, retained runtime state |
| 4 | Documentation/payload prose packet | Pointer preservation, custom examples, README deletion, link checks and Help parity |
| 4 | Integration owner | Published custom install → Context → Index → Update journey and default regression comparisons |
| Follow-up | Migration planner, translation and application packets, integrated sequentially | Ignored/untracked/binary content, pinned sources, straddling refusal, privacy and interruption boundaries |

Before expanding Wave 3, prove one integrated custom Install → Context → Index slice.

Require three equivalent reading journeys: loader-only, Context-only and Help-led. Include conditions, overwrites, native Skills and an unlisted registration. Repeat without README and without the CLI. Verify removal of a loader row stays removed across maintenance.

Complete the repository’s managed and supported Native AOT gates after integration. Record actual platform coverage. No builds or tests were run for this proposal.

Estimate **12–16 engineer-days for the first delivery**, then **5–8 for relocation**. Three lanes may reduce the first delivery to roughly 7–10 working days. These are planning estimates, contingent on the integrated source/layout slice.

#### Risks and what I would not build

The main risks are stale agent instructions, private notes becoming shareable, and old CLIs operating against the default path. Test those directly.

I would not build arbitrary mounts, movable controls, a second Skills location, loader discovery, a new relocation command, a replay journal or automatic prose replacement.

Do not promise that relocation is recoverable through Git alone. Ignored and untracked files require verified copies and recovery evidence.

The repository commit identity was supplied by the brief. Git verification was blocked by its ownership check. No configuration was changed.

#### Evidence

Prefixes used in citations:

- **F** — `src/cli/framework/OpenForge.Cli.Framework/Framework`
- **O** — `src/cli/operations/OpenForge.Cli.Operations/Commands`
- **T** — `src/cli/output-text/OpenForge.Cli.OutputText`
- **C** — `.agents/memory/crystallized/documents/cli`
- **P** — `.agents/memory/emerging/analysis/route-include-council/main-root-plan.md`
- **L** — `.agents/memory/emerging/analysis/lock-scope-council/plan.md`

Additional anchors:

- Accepted outcome and relocation: `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:13`, `:23`.
- Fixed runtime boundary: the same Task file at `:38`, and `L:85`.
- Existing source-root coupling: `F/Sources/Reading/SourceReadSessionReader.cs:35`, `F/Sources/Identity/SourceIdentity.cs:14`.
- Payload rendering coupling: `F/Distribution/Shared/Content/WorkspacePayloadRenderer.cs:29`.
- Current host pointers: `src/open-forge/AGENTS.md:5`, `src/open-forge/CLAUDE.md:3`.
- Removable guide: `src/open-forge/.agents/README.md:3`.
- Current installation narrative: `src/docusaurus/docs/getting-started/installation.md:23`, `:25`.
- Current agent-entry explanation: `src/docusaurus/docs/concepts/routing.md:29`.
- Open-question inventory: `P:466`.

## P2: Contrarian minimalist

/root - GPT-6 - high

#### Recommendation

Ship configurable roots for first installation, with complete support for subsequent maintenance, and defer moving existing installations. This deliberately narrows the accepted release scope, so Task 85 must retain migration as unfinished work unless the maintainer accepts the split. Keep fixed controls, native Skills, stable source IDs and literal exact paths. Introduce one immutable layout, without a general mount system or new ownership format. Budget 14–18 engineering days after Task 86, rather than presenting this as a small installer flag.

#### Answers

##### 1. What the plan gets wrong today

- **The prerequisite is complete.** Include, Exclude and Help have command definitions. `SourceReferenceParser` already accepts `./<workspace-path>`. Delete the obsolete prerequisite in the plan’s opening caveat. [E1]
- **The census is not an implementation estimate.** Repeating the plan’s search finds 334 production C# files, versus 322 originally. The plan itself says these are audit hits, including comments and pass-through models. Neither count proves how many files need edits. [E2]
- **The separate authoring guide does not exist.** Remove `AuthoringGuidePath`, the proposed `authoring.md` destination and its documentation packet. Formats live in the removable README, which Help reads from the embedded payload. [E3]
- **Include is now concrete code to convert.** Its planner classifies external paths using `.agents/`, reads the fixed loader and constructs a fixed-root catalogue. Its resolver assumes the category occupies the second path segment. A nested root breaks that arithmetic. [E4]
- **Included-source identity already has an implementation.** It uses `./path` internally. Preserve that representation while retaining the public promise that external files have no short ID. Do not introduce a nullable identity rewrite merely because the old plan’s table says `null`. [E5]
- **Native Skills need more than an external loader link.** The linked-destination resolver currently rejects `.agents` destinations. Index separately maintains native Skill catalogues. Under a custom root, Skills must retain that maintenance behavior instead of becoming ordinary read-only includes. [E6]
- **Package coordinates and installed paths are currently entangled.** Payload validation uses source-path validation. Frontmatter rendering only transforms destinations beginning `.agents/`. Mapping destinations without addressing these consumers would produce incorrect custom installations. [E7]
- **The proposed resolver placement needs correction.** A `Workspace` capability that reads Settings adds an upward dependency. Keep the layout value below Sources and put settings-backed resolution with Settings observation. The architecture already identifies reversed Framework dependencies as debt. [E8]

Task 86 supplies the fixed runtime subtree, lease and recovery-location contracts. Reserve `.open-forge-state/` lexically and physically. Never relocate it or rewrite historical bundles. Consume Task 86’s distinction between historical warnings and current-state blockers, including Install’s retained state checks. Do not carry forward a blanket “recovery artifacts exist” veto. These are requirements from its frozen plan, not verified implementation in this snapshot. [E9]

##### 2. The smallest correct design

The first release should support an existing project receiving its **first Open Forge installation**, not only an empty repository.

Allow a custom destination that is absent or empty. Existing native Skills may remain in `.agents/skills`. Preserve today’s default-root adoption behavior. Defer adoption of a populated custom destination because `route include` already handles existing project documents.

After installation, every ordinary command must work with the selected root. Update, Remove, Repair, Index, route operations, Extensions and Libraries are release requirements. A custom installation that cannot survive its next update is unfinished.

For an established installation:

- Omitting `--main-root` keeps its current root.
- Supplying the same root allows ordinary Install or Configure behavior.
- Supplying another root refuses before writes, including with `--configure`.
- The message says moving an installed workspace is unsupported in this release.

Cut migration inventory, copied-file verification, old-root deletion, incoming-link rewriting, exclusion translation and migration interruption handling from this release. Also cut combined multi-owner publication added solely for migration. Existing ownership APIs already publish each manager’s actual paths. [E10]

Keep the current lock schema. Libraries retain their literal source and destination mapping. Extensions map portable package coordinates to actual installed destinations before exclusions, permissions, collisions and ownership publication.

This is a proposed scope amendment. The accepted task explicitly promises moving existing workspaces, so an install-only release cannot honestly close that promise. [E11]

##### 3. The layout seam

Freeze this foundation:

| Surface | Contract |
|---|---|
| `WorkspaceLayout` | Immutable value under `Framework/Workspace/Models/Layout`, containing `Workspace`, `MainRoot`, `LoaderPath` and `HumanGuidePath` |
| Fixed locations | Existing owners retain settings, ownership, host filenames and Task 86 runtime paths |
| `WorkspaceLayout.Create(workspace, canonicalMainRoot)` | Pure construction |
| `WorkspaceLayoutReader.ReadAsync(workspace, cancellationToken)` | Settings observation owns resolution and returns the settings snapshot, layout and typed issues |
| Layout operations | `CategoryDirectory`, `MapPayloadPath`, `TryGetSourceRelativePath`, `IsContentPath` |
| Source handoff | `SourceCatalogueRequest` accepts Layout, `SourceCatalogue` retains it, sessions expose the same instance |

Resolve once at operation entry. Pass that observation through existing planning records. Revalidate its settings and physical expectations under the existing lease. Status and Doctor share the same observation across their contributors. Renderers receive projected paths and findings, never a settings reader.

Use two explicit content locations: the main root and native Skills. Deduplicate them for the default layout. Do not introduce configurable mounts or a public mount table.

`CategoryDirectory("skills")` remains `.agents/skills`. Other categories resolve beneath `MainRoot`. Short IDs remain root-relative. Exact `.agents/...` and `./...` operands stay literal.

Separate root-neutral path syntax from layout membership and mutation permission. Relaxing the former must not broaden the latter.

Default rendering is an identity transformation. Preserve existing bytes, ordering, omitted properties and output snapshots. The declared exceptions are newly created v4 settings and intentional option, wizard and help changes. A literal promise of unchanged *all* output would contradict adding the requested question. [E2, E12]

##### 4. Settings and validation

Publish v4 without modifying v1–v3. Add optional `mainRoot`, defaulting to `.agents`. Preserve existing declarations and unknown properties during unrelated rewrites. Do not insert `"mainRoot": ".agents"` automatically. Continue reading understood keys regardless of declared version. [E12]

Accept a canonical, portable, workspace-relative directory. CLI input may normalize separators, leading `./` and a trailing slash. Persist canonical `/` paths.

Reject:

- Empty, null, wrongly typed, absolute, drive-qualified or traversing values.
- The workspace root, Git metadata and `.open-forge-state`.
- Custom roots inside `.agents`, with `.agents` itself the default exception.
- Non-directory targets, unsafe linked ancestry and physical aliases into reserved locations.

Do not add a second path parser. Reuse existing portable-path and physical-resolution capabilities.

Validate `roots` against the selected content locations and fixed reservations. Native Skills remain a content category with stable IDs, even though their catalogue sits outside a custom main root. They should not acquire ordinary external-include semantics.

When an established workspace’s selected loader is missing, or managed pointers and ownership evidence identify another layout, report the exact disagreement. Read commands never fall back. Content-writing commands stop. In this release, recovery means restoring the correct setting or missing files, not invoking an unimplemented migration command.

Older CLIs accept newer versions and ignore unknown keys. They can continue assuming `.agents`, so v4 does **not** provide downgrade protection. Document the first supporting CLI version and unsupported older-CLI mutation. Do not add a pretend minimum-version field that old binaries also ignore. [E12]

##### 5. Recommendations for every open question

| Plan section 8 row | Recommendation |
|---|---|
| Partial #6 snapshot | Closed by current code. Convert its actual consumers. |
| Byte-identical defaults versus v4 | Preserve existing behavior and bytes, with the explicit schema/help/wizard exceptions above. |
| Partial migration lacks replay state | Defer migration. Add no journal or generalized continuation API. |
| Authored ignore rules expose moved files | Preserve authored bytes. In the later migration release, block moves whose destination privacy is not established. |
| Library registration straddles the moved folder | Later migration must block it and request narrower registrations. No per-file remapping or lock schema v2. |
| Library source inside the old root | Later migration should initially block this case. Do not build pinned islands inside an otherwise moved tree. Sources remain untouched. |
| Custom-root `skills` collision | Reserve the short-ID namespace for native Skills. Reject a second routed Skills category. |
| Older CLIs ignore `mainRoot` | Explicit compatibility documentation and offline startup support. No claim of enforced refusal. |
| Competing loaders or hand-edited settings | Report evidence, never guess a source root. First release offers no relocation repair. |
| Old paths in arbitrary prose | No repository-wide replacement. Render owned shipped paths during installation. Future migration rewrites only supported parsed links and managed regions. |

The Library restrictions preserve its current single-destination mapping and source-protection contract. Authored ignore text is already outside the owned Library ignore section. [E13]

##### 6. The user’s experience

Ask during interactive first installation:

> Where should Open Forge keep its context files?  
> Default: `.agents`

Supporting text:

> Settings and native Skills stay in `.agents`. This choice does not move an existing installation.

Ask before route choices, because their previews depend on the answer. An explicit flag suppresses the question. Dry runs, JSON and unattended execution never ask. An omitted choice means `.agents`.

Install preview names the context directory, fixed controls, native Skills and host-pointer changes. Status shows the actual loader and, for custom layouts, the selected main root. Keep ordinary default status output unchanged.

Render `AGENTS.md` to name the selected loader. Preserve Claude’s native `@AGENTS.md` bridge and render its loader import. Replace only recognized managed spans, preserving surrounding authored bytes. The current payload already separates these forms. [E14]

The custom loader links to `.agents/skills/_skills.md` using a relative path. Its rows remain the authority for loading. A removed row stays removed.

A new agent must reach the correct files from either host entry without the CLI or README. Update custom-rendered README examples and bundled Skill references where they name knowledge paths. Never globally replace `.agents`, because some occurrences name fixed controls or native Skills.

Add one layout section to `help concepts`, which remains usable outside a workspace. Update installation, routing, customization, CLI and glossary pages. Explain `mainRoot` versus included `roots`, fixed Skills, the compatibility boundary and the migration limitation.

##### 7. Implementation plan

Use Astra high implementers in separate worktrees after Task 86 integrates. One foundation owner retains shared API authority. Each packet owns its focused tests and matching output projections. Do not separate production and tests into different implementation owners.

Brace alternatives below mean separate packets. Freeze exact file manifests against the integrated Task 86 base before dispatch.

| Wave | Packet ownership | Required evidence |
|---|---|---|
| 0 | Layout model, reader, settings codec/document, v4 schema and shared contracts | Defaults, canonicalization, reserved locations, version tolerance, mismatch findings |
| 1 | `Sources/Identity` and directly affected identity models | Stable IDs, literal paths, Skills namespace, no widened permissions |
| 1 | `Sources/Inventory` and catalogue models | Two content locations, physical containment, unchanged default inventory |
| 1 | Separate `Sources/{Reading,Routing,Loading,Selection}` packets | Nested custom root, includes, conditions, overwrites, ordering |
| 2 | `GeneratedNavigation`, then `Sources/Sharing` | Listed and unlisted rows, private-route filtering, fixed Skills catalogue |
| 2 | `Distribution` and owned payload path rendering | Default byte identity, custom paths, both frontmatter forms, fixed package coordinates |
| 2 | Install binding, setup inputs, configuration and planning in sequential packets | Fresh custom install, preview parity, no prompts, root-change refusal, authored ignore preservation |
| 3 | Separate `Route/Shared`, scaffolding, Index and route-leaf packets | CRUD, Include/Exclude, literal paths, foreign-file preservation, Skill maintenance |
| 3 | Extension permissions, then separate lifecycle leaves | Mapped exclusions, collision checks, actual ownership, update/remove |
| 3 | Library boundary/permission helpers, then affected leaves | Literal mappings, source preservation, custom-root projection maintenance |
| 3 | Separate `{Update,Remove,Repair}` packets | No recreated default loader, correct removals, rename evidence, retained user files |
| 4 | Workspace observation, Status, Doctor and composition | Consistent paths and mismatch reports, default snapshots, Task 86 runtime isolation |
| 4 | Documentation packet | Correct examples, links, managed pointers, Help/README parity and CLI Skill references |
| 5 | Integration owner | Published custom-root lifecycle and complete managed/AOT qualification |

Complete one Install → Context → Index vertical slice before multiplying command conversions.

The documentation owner covers shared source and permission contracts, changed command semantics, one workspace-layout design, public CLI docs, relevant site pages and payload counterparts. Update only documents whose promises change.

Final journeys must include nested custom roots, native Skills, included files, unlisted registrations, deleted README, Update and Remove, stale settings, custom Install interruption and offline startup. Keep existing default snapshots as regression oracles. Test Task 86’s reservation and runtime location through the integrated feature without reimplementing its lock tests.

Estimate **14–18 engineering days**, approximately **7–10 elapsed working days with three lanes**. This is a planning estimate. Migration needs a separate estimate after its deliberately restricted supported cases are accepted.

#### Risks and what I would not build

The main risk is a surviving `.agents` assumption that writes to the wrong place. The second is silently weakening native Skills or privacy behavior while treating them as external includes.

I would not build relocation, arbitrary mount configuration, movable controls, symlink-based root aliases, Library per-file mappings, generalized recovery replay, automatic prose rewriting or a new ownership schema.

Existing users receive no relocation benefit in the first release. If moving this maintainer’s current workspace is the acceptance criterion, the proposed split is insufficient and migration must remain in the release.

#### Evidence

Paths use these repository prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `C` = `.agents/memory/crystallized/documents/cli`
- `P` = `.agents/memory/emerging/analysis/route-include-council/main-root-plan.md`
- `L` = `.agents/memory/emerging/analysis/lock-scope-council/plan.md`

Evidence references:

- **E1:** `P:15`. `F/Sources/Identity/SourceReferenceParser.cs:26`. `O/Route/Include/RouteIncludeDefinitions.cs:10`. `O/Route/Exclude/RouteExcludeDefinitions.cs:10`. `O/Help/HelpDefinitions.cs:17`.
- **E2:** `P:29`, `P:34`, `P:188`, `P:424`. The recorded census command returned 334.
- **E3:** `F/Sources/Identity/SourceLogicalPath.cs:15`. `O/Help/HelpOperation.cs:64`. `src/open-forge/.agents/README.md:74`.
- **E4:** `O/Route/Shared/Inclusion/Planning/RouteInclusionPlanBuilder.cs:49`. `O/Route/Shared/Inclusion/Planning/RouteInclusionTargetResolver.cs:55`.
- **E5:** `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:98`.
- **E6:** `F/Sources/Loading/SourceGeneratedDestinationResolver.cs:112`. `O/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs:11`.
- **E7:** `F/Distribution/Models/FrameworkPayloadAsset.cs:48`. `F/Distribution/Shared/Content/WorkspacePayloadRenderer.cs:29`. `O/Extension/Install/Shared/Planning/ExtensionInstallPayloadNormalizer.cs:45`.
- **E8:** `C/architecture.md:445`.
- **E9:** `L:75`, `L:85`, `L:278`, `L:297`, `L:353`.
- **E10:** `F/Ownership/WorkspaceOwnershipStore.cs:12`, `:43`, `:73`. `O/Install/Models/Planning/InstallMigrationPlan.cs:6` describes adoption, not relocation.
- **E11:** `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:13`, `:23`.
- **E12:** `F/Settings/WorkspaceSettingsDefinitions.cs:17`. `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:18`. `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:94`. `.agents/directives/software/versioned-configuration-schema.md:15`.
- **E13:** `F/Libraries/LibraryPathIdentity.cs:10`. `C/technical-designs/workspace-libraries.md:50`, `:76`, `:94`. `O/Install/Shared/Configuration/InstallIgnoreSection.cs:54`.
- **E14:** `src/open-forge/AGENTS.md:5`. `src/open-forge/CLAUDE.md:3`.

Read-only source inspection only. No files changed, builds or tests run, or Doctor command executed. Post-Task 86 behavior and the effort estimate remain unverified.

## P3: Test and qualification engineer

/root - GPT-6 - high

#### Recommendation

Deliver custom-root installation first, with every existing command working against that layout. Make relocation a separately qualified follow-up, while keeping Task 85 open until its accepted migration outcome ships. Preserve the default through explicit byte comparisons and unchanged snapshots. Reuse Task 86’s fixed runtime storage and qualify the result on all six supported platform targets. Treat the old plan’s file census as an audit list, not a mandatory edit list.

#### Answers

##### 1. What the plan gets wrong today

- **Its prerequisite is complete.** Generic `./<workspace-path>` parsing, Include, Exclude and Help now exist. Include already has a published-process journey proving no reactivation through Index, Update, Configure and Extension installation. Extend this evidence rather than recreate it. [E1]
- **The authoring file is obsolete.** `SourceLogicalPath` has no `AuthoringGuidePath`. Help extracts “Writing workspace files” from the embedded README. Remove the proposed member, migration treatment and authoring-file documentation packets. Keep Help’s package lookup independent of installed layout. [E2]
- **Some locations moved.** `WorkspaceSettingsCodec.Roots.cs` lives under `Settings/Shared/Serialization`. It still rejects every `.agents/` registration. That rule must distinguish selected knowledge, native Skills and fixed controls. [E3]
- **The census is not an implementation estimate.** Repeating its exact search finds 334 production C# files, against the plan’s 322. Some hits are legitimate package coordinates or fixed controls.
- **Skills need more than destination mapping.** Index traverses native Skill catalogues and their descendants. Preserve that maintenance with a loader outside `.agents`, including reference catalogues and unlisted branches. [E4]
- **The proposed sequencing is not green by construction.** It changes shared signatures before composition, separates production from test ownership, and leaves substantial output work until late. Each callable change needs its complete caller closure and focused evidence before integration.
- **Task 86 changes the baseline.** Runtime paths become workspace-local. Recovery observations distinguish local and legacy stores. Unrelated history stops vetoing Route and Extension mutations, while Install retains its filtered state checks. Do not reintroduce a blanket recovery-history veto through layout validation. [E5]

##### 2. The smallest correct design

Use one main knowledge root and one fixed native Skills location. Do not create a general mount registry, persistent source-ID registry or new relocation command.

Keep package paths portable. Map installed destinations once, preserve actual paths in ownership, and leave source IDs root-relative. Keep settings, ownership, Skills and Task 86 runtime storage fixed.

I recommend two deliverables:

1. **Choosing a root:** fresh installation, complete reading and maintenance, package lifecycles, permissions, diagnostics, host pointers and documentation.
2. **Moving a root:** complete filesystem inventory, ownership translation, link rebasing, privacy checks, verified deletion and interruption reporting.

This separates the largest data-loss risk from the useful installation feature. It changes the accepted delivery sequence, so the chair must explicitly accept it. Until relocation ships, changing an established root must refuse without effects. It must not suggest that editing settings moves files.

Cut the arbitrary five-file packet limit and separate test-authoring wave. Keep packets cohesive and small through clear ownership. Defer generalized migration continuation, per-file Library remapping and new ownership schema fields.

##### 3. The layout seam and default proof

Freeze an immutable `WorkspaceLayout` under Framework’s workspace capability. Its callable surface should cover:

```text
WorkspaceLayoutResolver.ReadAsync(workspace, cancellationToken)
    → WorkspaceLayoutRead

WorkspaceLayout
    MainRoot, LoaderPath, HumanGuidePath, MemoryArchivePath
    NativeSkillsRoot, NativeSkillsCataloguePath
    SettingsPath, OwnershipPath
    CategoryDirectory(id)
    MapPayloadPath(packagePath)
    TryGetSourceRelativePath(path)
    IsImplicitContentDestination(path)
```

`WorkspaceLayoutRead` carries the observed settings, layout, issues and readiness state. It does not change `CliWorkspace` identity or own Task 86 runtime paths.

Resolve at operation entry. Pass the same observation through catalogue, read session and planning facts. Revalidate it under the mutation lease. Rendering receives command-owned path facts and performs no settings reads.

Keep `.agents/...` exact operands literal. `./docs/forge/...` selects an exact custom-root path. Bare values remain IDs. These parsing rules already exist and need layout-aware identity beneath them. [E1]

**Default acceptance has three independent parts:**

- Keep existing default snapshots unchanged during structural waves.
- Compare explicit `.agents` and omitted-root scenarios using independent expected paths, selected bytes, ordering, effects, statuses and exits.
- Compare the integrated candidate against the post-Task-86 baseline on identical default fixtures. Check file bytes and output separately.

Allow only reviewed changes for new help/options, the wizard question and new v4 declarations. Preserve existing declarations and omit an unsolicited `"mainRoot": ".agents"`.

Do not normalize `.agents` and custom roots into the same snapshot token. The existing comparer ignores line endings, so it cannot alone prove exact output bytes. Add direct byte assertions where byte preservation is promised. Reuse the existing tree snapshot for no-write assertions, but do not compare its timestamps across independently created workspaces. [E6]

##### 4. Settings and validation

Publish v4 without modifying v1–v3. Absence means `.agents`. Reject null, empty and non-string `mainRoot`. Preserve unknown values, existing declarations and property order on unrelated rewrites. Existing code already distinguishes declaration preservation from creating new settings. [E3]

Use the existing portable-path validator for canonical stored values. It already handles traversal segments, forbidden characters, device names, trailing dots/spaces and Unicode normalization. Normalize documented CLI separator forms before validation. [E7]

Additionally reject:

- The workspace root, Git metadata and `.open-forge-state`.
- Custom roots overlapping fixed controls or native Skills, including descendants of `.agents`.
- Files where directories are required.
- Escaping links, ambiguous aliases and unsafe ancestry.
- A custom-root `skills` category competing with native Skills identity.

Validate registrations in `roots` against the selected layout. Native Skills remain a known category, not an ordinary foreign include. Preserve unlisted registrations and literal external paths.

For established workspaces, a missing selected loader, competing legacy loader or inconsistent ownership produces a mismatch. Readers name the disagreement without silently falling back. Content writers stop. Status and recovery inspection remain available. Cleanup must retain Task 86’s store-specific safety rules rather than require a healthy knowledge tree.

**Older CLIs are not safely excluded by v4.** The current reader accepts unknown keys and versions. Such a CLI ignores `mainRoot` and retains `.agents` assumptions. Document the first supporting release and prohibit claims of mixed-version compatibility. Qualify representative old-reader and old-writer behavior in disposable fixtures with a pinned beta.17 executable. The new release cannot retrofit a refusal into old binaries. [E3]

##### 5. Recommendations for every section 8 question

| Question | Recommendation |
|---|---|
| Partial implementation of #6 | Close this prerequisite. Reuse its current tests. |
| Byte-identical default versus v4 | Use a narrow exception ledger for new declarations and advertised interaction/help. Do not bulk-refresh snapshots. |
| Partial migration without replay state | Preserve recovery evidence and block blind retries. No automatic rollback or inferred resume. |
| Authored ignore rules | Preserve their bytes. Translate only proven managed sections and sharing records. Block before effects if destination privacy cannot be established. |
| Library registration straddles the move | Block only registrations whose resulting mappings cannot fit the existing single destination-root contract. Name the registration and affected paths. Require narrower reattachment. |
| Library source inside old root | Pin the source subtree and preserve its bytes. Rebase affected projections and loader links. Block ambiguous overlap rather than move the source implicitly. |
| Custom-root `skills` collision | Reserve the namespace for `.agents/skills`. |
| Older CLI ignores `mainRoot` | State the minimum supported version in help and custom host guidance. Keep CLI-free startup functional. |
| Competing loaders or hand-edited settings | Require explicit, evidence-backed source selection. Never guess by searching for loaders. |
| Old paths inside arbitrary prose | Rewrite supported parsed links and managed pointer regions. Report remaining literals without rewriting prose globally. |

The Library mapping restriction is real: every mapping derives from one source root, one destination root and a suffix. [E8]

Privacy qualification must include nested ignore files, negations, ignored untracked Working notes and absent excluded paths. Use Git’s actual ignore evaluation in owned fixtures. If the implementation cannot prove protection, refuse before copying or deleting. The current Install writer provides a bounded managed-section boundary to preserve. [E9]

##### 6. The user’s experience

Ask:

> Where should Open Forge keep its context files?  
> Directory relative to this workspace [.agents]:

Explain briefly that settings and native Skills remain in `.agents`. Show the selected loader path in the preview. Dry runs, JSON and unattended invocations never ask.

Custom-root Status should identify the main root and loader, while retaining useful distinctions between listed, unlisted and missing includes. Preserve default output shape unless a change is explicitly approved.

Render only recognized managed regions in root `AGENTS.md` and `CLAUDE.md`. Preserve surrounding text. Default rendering must return existing bytes. The shipped files currently name the loader directly, including Claude’s import, so both require qualification. [E10]

Add layout guidance to `help concepts`, installation documentation and the docs site. Show stable IDs, exact custom paths, fixed Skills, version requirements and the supported relocation boundary.

Qualify an agent’s startup without the CLI: follow each host file to the selected loader, native Skills and an included conditional catalogue. Repeat with README removed. Compare the ordered required-read manifest with Context output. Do not claim this proves identical reasoning by every agent host.

Update CLI Skill examples where necessary. Any shipped prose change that alters default installed bytes must appear as an explicit documentation delta, not disappear inside a compatibility normalization.

##### 7. Implementation and qualification plan

One foundation owner retains layout, settings semantics and ownership publication. Assign separate Astra high implementers only after that API is frozen. Each packet owns its mirrored tests, affected snapshots and command-local presentation. Shared files have one writer.

| Wave | Packet ownership | Required evidence |
|---|---|---|
| 0 | Layout resolver/models, settings codec and v4 schema. One owner. Establish post-Task-86 baseline. | Default/custom mapping, invalid settings, version preservation, controls, runtime reservation and no-write resolution. |
| 1 | Source identity, inventory, sessions and loading, with complete direct caller/composition changes. | Stable IDs, literal operands, overwrite ordering, includes, aliases, missing loader and default parity. |
| 2 | Separate navigation/Index and payload-mapping packets. Route shared planning follows their accepted APIs. | Native Skill maintenance, unlisted routes, frontmatter forms, payload byte parity and destination permissions. |
| 3 | Disjoint Route, Extension and Library packets. Split individual leaves only after family helpers settle. | Command effects, foreign-file preservation, mapped ownership, fixed sources, relative links and unchanged default snapshots. |
| 4 | Framework lifecycle packet, then Install option/wizard activation. Separate Status/Doctor presentation and documentation packets. | Complete custom-root lifecycle, mismatch refusals, managed pointers, no-prompt behavior and published journeys. |
| 5 | Relocation inventory/planning, then application/recovery. Sequential ownership of Install integration. | Round trips, privacy, Library constraints, collisions, stale plans, interruption and honest residuals. |

Do not expose custom-root installation while downstream consumers still assume `.agents`. Integrate signature changes with every required caller. The first connected slice must prove Install → Context → Index before repeating the pattern.

**Focused evidence**

Keep detailed branches in Unit or Integration tests. Add a small published-process set covering:

- Default, `docs/forge`, and a nested root containing spaces and Unicode.
- Minimal, Essentials and selected Custom installation.
- Both frontmatter forms.
- Context, Find, References, Index and route mutation.
- Include/Exclude with foreign bytes unchanged and no reactivation.
- Extension install/update/remove and Library attach/sync/detach.
- Framework update/remove, status and recovery cleanup.
- Preview, refusal and verified no-op without new runtime state.
- CLI-free startup and README removal.

Relocation additionally requires default → custom → custom → default, ignored/untracked/binary files, overwrites, empty directories, stationary sources and historical recovery paths. Inject failures around copying, verification, ownership, pointers, settings and deletion. Assert that originals are never deleted before destination verification.

**Sandbox execution**

This council is read-only, so no tests can run here. Future workspace-write workers can run focused managed tests after dependencies and writable artifact locations are prepared.

Apply Task 86’s amendment: redirect `TEMP` and `TMP` beneath `artifacts/tmp` before starting test processes. Its older statement that temporary-workspace tests necessarily require the overseer is superseded by that amendment. All three suites initialize a temporary data home, so even a unit filter needs this setup. Validate the actual temporary location rather than assuming the environment worked. [E5, E11]

Use the existing Release build with disabled build servers and prepared dependencies. Focused execution follows:

```text
dotnet run --project <owned-test-project> -c Release --no-build --
  --filter-class <owned-class> --minimum-expected-tests 1
  --fail-warns on --fail-skips on
```

A build using `OpenForgeSkipDevelopmentPublish=true` does not refresh the executable used by EndToEnd tests. Require a fresh development publication before those tests. Hand unavailable ACL, link, terminal, native-toolchain or cross-platform checks to the overseer. Never weaken assertions to make a sandbox pass. [E11]

**Wave and release gates**

At each integrated wave, run the complete managed suite and supported Native AOT gate required by repository policy. Leaf iterations use focused tests.

Before shipping, run `build:native` and `test:built` on matching Windows, Linux and macOS hosts, both x64 and arm64. Six execution modes on Windows are not six-platform evidence. Record source identity, executable identity, runtime mode, discovered/executed counts, warnings, exclusions and failures. [E12]

Core custom-root journeys must execute on all platforms. Windows ACL/share-denial evidence and Unix permission evidence remain platform-specific. Use Task 86’s cross-platform lock holder. Do not inherit the current F20 Windows-only skip into the new custom-root concurrency journey.

**Snapshot and documentation ownership**

Update snapshots only for reviewed behavior changes, serially and by owning class. Keep update mode disabled during ordinary tests. Add a few custom-root output cases rather than duplicating every default snapshot.

The documentation owner covers shared contracts, lifecycle/package contracts, workspace-layout design, host guidance, CLI Skill references, `docs/cli.md`, README and relevant site pages. Run repository documentation checks, site build, Help tests and payload parity before closeout.

Estimate **11–15 engineer-days** for the complete fresh-install feature and **7–10 additional days** for relocation. Allow roughly **8–12 elapsed working days** with three lanes and available platform runners. Re-estimate after the first connected slice.

#### Risks and what I would not build

The main risks are stale executable evidence, broadened permissions, private files becoming visible, duplicate Skills identity and snapshots concealing wrong paths.

I would not build a generic mount framework, per-file Library remapping, automatic migration replay, arbitrary prose rewriting, another test project or a new platform harness.

No builds, tests or Doctor runs were performed. Task 86 integration, sandbox capability and platform success remain qualification requirements, not observed results.

#### Evidence

Repository-relative paths below identify the inspected sources.

- **E1:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Identity/SourceReferenceParser.cs:26`, `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Include/RouteIncludeDefinitions.cs:9`, `src/cli/operations/OpenForge.Cli.Operations/Commands/Route/Exclude/RouteExcludeDefinitions.cs:9`, `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteIncludeProcessTests.cs:37`.
- **E2:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Sources/Identity/SourceLogicalPath.cs:15`, `src/cli/operations/OpenForge.Cli.Operations/Commands/Help/HelpOperation.cs:64`.
- **E3:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:16`, same file `:94`, `src/cli/framework/OpenForge.Cli.Framework/Framework/Settings/WorkspaceSettingsDefinitions.cs:18`.
- **E4:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs:11`.
- **E5:** `.agents/memory/emerging/analysis/lock-scope-council/plan.md:16`, same file `:75`, `:278`, `:295`.
- **E6:** `src/cli/tests/support/OpenForge.Cli.TestSupport/Snapshots/CommandOutputSnapshot.cs:16`, same file `:38`, `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/PublishedProcess/PublishedWorkspaceTreeSnapshot.cs:53`.
- **E7:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Filesystem/Shared/Paths/PortableWorkspacePath.cs:12`.
- **E8:** `src/cli/framework/OpenForge.Cli.Framework/Framework/Libraries/LibraryPathIdentity.cs:10`, `.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md:76`.
- **E9:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Install/Shared/Configuration/InstallIgnoreSection.cs:42`.
- **E10:** `src/open-forge/AGENTS.md:5`, `src/open-forge/CLAUDE.md:4`.
- **E11:** `src/cli/tests/support/OpenForge.Cli.TestSupport/Isolation/TestDataHome.cs:91`, `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/TestProcessIsolation.cs:13`, `.agents/memory/emerging/analysis/lock-scope-council/plan.md:47`.
- **E12:** `.github/workflows/build.yml:75`, `scripts/delivery/README.md:77`, `scripts/delivery/platform-skips.ts:54`, `.agents/directives/open-forge/cli/implementation.md:325`.

## P4: CLI architect

/root - GPT-6 - high

#### Recommendation

Keep one configurable content root, fixed native Skills, fixed controls, and Task 86’s fixed runtime directory. Put the layout boundary in Framework source discovery and payload projection, then carry resolved facts through existing command plans. Deliver installation and complete operation of custom-root workspaces first, followed by relocation in a second PR within Task 85. Reject Library migrations that cannot preserve today’s registration model. Budget 20–27 engineer-days for the complete outcome, with 14–18 for the first PR.

#### Answers

Citations use the directory prefixes defined under Evidence.

##### 1. What the plan gets wrong today

The architecture remains broadly sound, but its baseline and packet estimates need revision.

- **The missing #6 prerequisite is complete.** Exact `./workspace/path` references exist at `F/Sources/Identity/SourceReferenceParser.cs:26`. Include and Exclude share `O/Route/Shared/Inclusion/Planning/RouteInclusionPlanBuilder.cs:44`. Help reads the bundled README at `O/Help/HelpOperation.cs:64`. These need adaptation, not implementation.
- **`AuthoringGuidePath` and `authoring.md` should disappear from the proposal.** Current source constants contain only the loader and human guide at `F/Sources/Identity/SourceLogicalPath.cs:8`. File formats come from the README.
- **Some path neutralization already happened.** `SourceLayer` accepts workspace-relative paths. `SourceLogicalIdentity` represents included sources using `./path`, rather than a nullable identity. Preserve that representation instead of redesigning the models. See `F/Sources/Models/Inventory/SourceLayer.cs:20` and `SourceLogicalIdentity.cs:19`.
- **Include traversal and physical aliases now matter.** `F/Sources/Shared/Includes/SourceIncludedEntryReader.cs:62` resolves linked rows and retains aliases. Layout propagation must survive its catalogue reconstruction at line 126.
- **The single default selection scope is an overlooked seam.** A custom layout has content and native Skills in separate directories. Keeping one scope would omit Skills from default selections. Change `SourceReadSession.DefaultSelectionScope` and its filter consumers together. See `F/Sources/Models/Reading/SourceReadSession.cs:24` and `F/Sources/Selection/SourceUniverseFilterResolver.cs:129`.
- **Library movement is more constrained than “straddling.”** Every destination is `destinationRoot + source-relative suffix`. Even a registration entirely under `.agents` can be unrepresentable after relocation. See `F/Libraries/Models/Observation/LibraryMapping.cs:37`.
- **Task 86 changes the recovery foundation.** Consume `WorkspaceRuntimePaths` and recorded recovery-store locations. Do not relocate runtime state, rewrite historical bundles, or restore blanket historical-recovery vetoes. Install’s specific mixed-state checks remain. See `LOCK:75`, `LOCK:239`, and `LOCK:278`.

Rerunning the plan’s census finds **334 production C# files**, versus its 322. Only 84 reference the selected movable-root constants directly. Neither count is an edit estimate. Literal paths, package coordinates, comments, and neutral helper calls need different treatment.

##### 2. The smallest correct design

Separate three coordinates:

1. **Workspace paths** remain literal, including settings, ownership receipts, Library records, includes, and recovery evidence.
2. **Source IDs** derive from the selected content root, with `skills/...` reserved for native Skills.
3. **Package paths** retain portable `.agents/...` coordinates and map when delivered.

This preserves the existing package format and ownership schema. Framework and Extension receipts already record actual installed paths and regions at `F/Ownership/Models/Document/WorkspaceOwnershipDocument.cs:17`.

The first PR must support the complete lifecycle of a newly installed custom-root workspace. That includes Context, Find, References, navigation, route mutations, Update, Remove, Repair, Extensions, Libraries, and observations. Shipping only an installer would create workspaces that subsequent commands mishandle.

Relocation can follow. Until it ships, an installed workspace requesting a different root must receive an explicit unsupported-operation result. Same-root Configure remains available. This is a proposed delivery split, not completion of Task 85’s accepted relocation outcome.

Cut these from the first PR:

- Relocation inventory, copy/delete sequencing, combined ownership translation, and interruption handling.
- New ownership fields.
- General mount configuration.
- Generic continuation or rollback.
- Separate test-only implementation packets.

Keep stable IDs, literal exact paths, native Skills, preserved loader membership, contained writes, and actual-path receipts.

##### 3. The layout seam

Introduce three files:

- `F/Workspace/Models/Layout/WorkspaceLayout.cs`
- `F/Workspace/Models/Layout/WorkspaceLayoutRead.cs`
- `F/Workspace/Shared/Layout/WorkspaceLayoutResolver.cs`

Freeze this foundation surface:

| Surface | Contract |
|---|---|
| `WorkspaceLayout` | Immutable workspace identity and validated `MainRoot` |
| Derived paths | `LoaderPath`, `HumanGuidePath`, `MemoryArchivePath`, fixed native Skills paths |
| Mapping | `SourcePath`, `CategoryDirectory`, `MapPayloadPath`, `TryGetSourceRelativePath` |
| Membership | `IsMainRootPath`, `IsNativeSkillPath`, `IsImplicitContentDestination` |
| Resolver | `ReadAsync(workspace, cancellationToken)` returns settings observation, layout, state, and issues |
| Discovery | `SourceCatalogueRequest(layout, logicalRoots)` |
| Reading | `SourceReadSessionReader.ReadAsync(layout, cancellationToken)` |
| Identity | `SourceIdentity.DeriveId(path, layout)` |
| Classification | `SourceFormClassifier.TryClassify(path, layout, out form)` |

Do not add settings state to `CliWorkspace`. Do not duplicate fixed control constants inside a large layout object.

Resolve once when an operation establishes its facts. Reuse that settings snapshot. Carry the layout on `SourceCatalogue`, and expose it through `SourceReadSession`. Status and Doctor contributors receive the same observation. Rendering receives paths and grouping facts from results. It never resolves settings.

The existing reader’s `.agents` scan and scope construction are the first connection point at `F/Sources/Reading/SourceReadSessionReader.cs:35`. Root composition already constructs this reader explicitly at `H/CliCompositionRoot.cs:51`.

Use root-neutral lexical helpers for parent and filename arithmetic. Keep membership and permission checks explicit. Changing `IsCanonicalSource` to accept everything would silently weaken several policies.

Discovery scans the selected root and native Skills, deduplicating the default overlap. Native Skills remain managed sources with stable IDs and catalogue maintenance. They must not become ordinary foreign includes merely because their loader link contains `..`. Today, that requires changes to both destination resolution and direct-child navigation recognition at `F/Sources/Loading/SourceGeneratedDestinationResolver.cs:83` and `F/GeneratedNavigation/GeneratedNavigationFormationBuilder.cs:162`.

Default compatibility uses identity path mapping and unchanged bytes. Do not serialize an absent `mainRoot` during unrelated writes. Preserve ordering, IDs, loader rows, and established output. Explicit exceptions are new settings declarations, help, and the requested wizard question. Custom host, guide, and bundled CLI Skill rendering must leave default rendered bytes unchanged.

##### 4. Settings and validation

Publish `schemas/v4/open-forge.schema.json`. Leave v1–v3 immutable.

`mainRoot` is optional. Absence means `.agents`. Null, empty, and non-string values are invalid. Settings store canonical workspace-relative directories with `/`.

Reuse `PortableWorkspacePath` for portable names, Unicode normalization, forbidden characters, and device-name rejection. CLI input may normalize separators and leading `./` before validation. See `F/Filesystem/Shared/Paths/PortableWorkspacePath.cs:12`.

Permit `.agents` as the default exception. Otherwise reject:

- The workspace root.
- Git metadata.
- Overlap with the fixed `.agents` control and native Skills tree.
- Overlap with `.open-forge-state`.
- Files, unsafe ancestry, escaping links, and physical aliases.

Validate `roots` against the resolved layout. They remain literal external files. Do not silently convert a registration into managed content when a proposed root would envelop it.

New settings declare v4. Existing rewrites retain declared versions, `$schema`, unknown keys, and ordering, following the current preserving writer at `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:94`.

A missing selected loader, conflicting managed pointer, or contradictory ownership evidence produces a layout finding. Never fall back to another loader. Read commands may report trustworthy selected-root observations. Content-writing commands stop. Once relocation exists, Configure may resolve a mismatch only from explicit, unambiguous source evidence. Runtime-only Cleanup retains Task 86’s independent storage rules.

**Older CLIs are not safely excluded by v4.** They accept unknown versions and ignore unknown keys. They can still assume `.agents` and perform harmful maintenance. Document the minimum supporting CLI release and preserve offline startup. Do not describe schema versioning as an enforcement mechanism. See `F/Settings/WorkspaceSettingsDefinitions.cs:18`.

##### 5. Recommendations for every section 8 question

| Plan question | Recommendation |
|---|---|
| Partial #6 snapshot | Remove the prerequisite. Adapt the released Include, Exclude, Help, and exact-path implementation. |
| Byte-identical default versus v4 | Preserve existing settings declarations and default payload rendering. Review new-file v4 declarations and advertised interaction changes explicitly. |
| Partial migration | Reuse prior-state recovery. Report partial effects and block blind retries. Add no replay engine. |
| Authored ignore rules | Preserve authored bytes. Translate owned sections. Refuse relocation when currently ignored private content would become exposed or protection cannot be established. |
| Straddling Library registrations | Test whether every intended destination is representable by one new destination root with unchanged source suffixes. Block otherwise and provide exact reattachment instructions. |
| Library sources inside the old root | Keep sources fixed. Initially block relocation where a source intersects movable content. Arbitrary pinned islands complicate stable IDs and navigation. Already stationary Skills subtrees need no move. |
| Custom `skills` collision | Reserve the namespace. Reject `<mainRoot>/skills` as a competing category. |
| Older CLIs | Document their real incompatibility. Keep host startup usable without a CLI. |
| Competing loaders or edited settings | Report disagreement. Never infer the source by searching for a plausible loader. |
| Old paths in prose | Rewrite supported parsed links and recognized managed content only. Report remaining literals and incoming references. Never globally replace `.agents`. |

The Library-source restriction is deliberately narrower than the old plan’s pinning proposal. It avoids introducing partially movable managed trees.

For supported relocation, inventory ignored, untracked, binary, and empty-directory content. Verify destination bytes before source deletion. Translate exclusions even for absent paths, sharing records, owned ignore sections, and actual ownership paths. Keep explicit permission grants literal. Rebase existing include rows without restoring unlisted registrations. Preserve historical recovery paths.

##### 6. The user’s experience

Ask during interactive initial installation:

> Where should Open Forge keep its context files?  
> Directory: `.agents`

Explain immediately below:

> Settings and native Skills stay in `.agents`. Runtime files stay in `.open-forge-state`.

`--main-root docs/forge` supplies the answer. Preview, JSON, automatic, and unattended execution never prompt. With no supplied root, fresh unattended installation uses `.agents`. Existing installations retain their configured root.

The current prompt gate already distinguishes these modes at `O/Install/Shared/Configuration/InstallSetupResolver.cs:40`.

Custom-root Status should identify the content directory, loader, controls, native Skills, and runtime directory. Default-root reports retain their existing shape. Findings name the actual path.

Render only the recognized Open Forge portions of root `AGENTS.md` and `CLAUDE.md`. Preserve user text. The custom Claude pointer becomes `@docs/forge/loader.md`. An agent follows that pointer, reads the loader, and reaches Skills through the loader’s relative catalogue link. It does not need to interpret settings.

Keep `help concepts` workspace-independent. Explain the default and custom layouts there. The README remains removable, and Help continues deriving file formats from the bundled guide at `O/Help/HelpOperation.cs:64`.

The documentation packet covers installation, routing, customization, source references, Extensions, Libraries, status, and relocation limitations. Update the site and CLI Skill guidance together. Prefer IDs and explicitly marked default-layout examples over scattered hard-coded paths.

##### 7. Implementation plan

Freeze the foundation above after Task 86 lands. One owner retains shared APIs, settings shapes, and composition. Each packet belongs to one Astra high implementer in its own worktree and includes its focused tests. Workers preserve other owners’ changes. Braced command names below are separate packets.

| Wave | Packet ownership | Required evidence |
|---|---|---|
| 0 | Layout files, `WorkspaceSettingsDocument`, settings codec partials, definitions, v4 schema | Default/custom paths, invalid settings, older/newer versions, reserved runtime paths |
| 1 | Source identity helpers, catalogue request/reader, inventory model validation | Stable IDs, exact paths, overwrite pairing, collisions, contained aliases |
| 1 | Reading, Routing, Loading, Selection, included-entry reader | Two default scopes, Skills identity, included traversal, literal legacy paths |
| 2 | Generated navigation, `SourceSharing`, Index Skill traversal | Preserved membership/order, private navigation, Skills maintenance, foreign files unchanged |
| 2 | Distribution projection and content rendering | Package coordinates versus installed paths, default byte parity, custom pointers and guide |
| 3 | Route `{Create,Init,Update,Move,Remove,List,Inspect}` and one shared Include/Exclude packet | Custom-root operations, protected controls, unchanged exact-path behavior |
| 3 | Extension normalization, destination policy, `{Install,Update,Remove}` consumers | Mapping before exclusions/collision checks, actual-path receipts, native Skills |
| 3 | Library boundary, destination and permission policies, affected command consumers | Literal sources and destinations, source protection, relative links, owned ignore rules |
| 4 | Install setup/configuration and `{Update,Remove,Repair}` | Complete lifecycle, mapped exclusions, no recreated legacy loader |
| 4 | Workspace/source operational observations, `{Context,Find,References,Status,Doctor}`, affected presentation | Actual paths, mismatch results, default snapshots |
| 5 | Documentation and integrated journeys | Offline startup, README deletion, no reactivation, package lifecycle |
| 6 | Separate relocation PR under `O/Install/Shared/MainRootMigration` | Inventory, representability, privacy, interruption, stale-plan and source-edit checks |

Concrete route entry points include `RouteCreateTargetPlanner.cs:23`, `RouteMoveDestinationProjector.cs:72`, `RouteInitTargetPlanner.cs:211`, and the shared inclusion planner. Extension mapping starts at `ExtensionInstallPayloadNormalizer.cs:45`. Library consumers start at `LibraryConsumerBoundaryReader.cs:26` and `LibraryDestinationPolicy.cs:67`.

Keep production changes and their tests under the same owner. Split large rows into command-local packets before dispatch, using exact caller manifests. Do not assign separate workers to Include and Exclude’s shared planner.

The documentation owner handles:

- New `C/technical-designs/workspace-layout.md`, architecture summaries, shared source-reference and permission contracts.
- Affected command contracts and `workspace-libraries.md`.
- `docs/cli.md`, `docs/extensions.md`, root README, and relevant `src/docusaurus/docs` pages.
- Managed host rendering, guide and CLI Skill variants, and Help wording.

Estimates include implementation, focused tests, documentation, and integration:

- **Custom-root installation and full operation: 14–18 engineer-days.**
- **Relocation: another 6–9 engineer-days.**
- **Complete outcome: 20–27 engineer-days.**

The old 22–25-day estimate remains plausible. Its quarter-day packets and half-day allowance for all presentation work are less credible than its total.

Run the existing managed and supported Native AOT gates after integration, with published journeys on Windows, Linux, and macOS. No builds, tests, or Doctor execution were performed for this proposal.

#### Risks and what I would not build

The largest risks are losing native Skills from selection, treating portable package paths as installed paths, granting authority through relaxed path validation, and moving private content without preserving ignore protection.

I would not build configurable control storage, arbitrary mounts, Library per-file remapping, automatic source relocation, generic rollback, or recovery-history rewriting.

The exact changed-file count remains unverified until the shared signatures are implemented. The census establishes breadth, not a promise that a small helper change will finish the work.

#### Evidence

Directory prefixes:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `H` = `src/cli/root/OpenForge.Cli/Composition`
- `C` = `.agents/memory/crystallized/documents/cli`
- `LOCK` = `.agents/memory/emerging/analysis/lock-scope-council/plan.md`

Primary anchors:

- Accepted layout outcome: `record.md:173`.
- Original inventory and open questions: `main-root-plan.md:28` and `main-root-plan.md:466`.
- Fixed runtime contract: `Task 86 plan:75`.
- Source discovery seam: `SourceReadSessionReader.cs:35`.
- Library mapping limit: `LibraryMapping.cs:37`.
- Library source and ownership contract: `workspace-libraries.md:35`.
- Owned ignore boundary: `O/Install/Shared/Configuration/InstallIgnoreSection.cs:42`.
- Literal ownership receipts: `F/Ownership/Models/Document/WorkspaceOwnershipDocument.cs:43`.
- Current permission boundary: `C/contracts/shared/workspace-permissions/behavior.md:16`.

## P5: Contract and compatibility guardian

/root - GPT-6 - high

#### Recommendation

Implement one configurable knowledge directory alongside the existing included-file `roots`. Keep settings, ownership and native Skills under `.agents`, and consume Task 86’s fixed workspace runtime paths unchanged. Ship complete custom-root operation support before exposing the install choice. Migration can follow in a separate PR, but Task 85 remains incomplete until reviewed relocation ships. Treat older CLIs as unsupported for custom-root workspaces because settings v4 cannot make released readers refuse them.

#### Answers

##### 1. What the plan gets wrong today

- **The PR #6 prerequisite is complete.** `SourceReferenceParser` already recognizes generic `./` paths. Include, Exclude and Help definitions exist. Remove the plan’s baseline caveat. [E1]
- **`AuthoringGuidePath` is obsolete.** Current source identity exposes only the loader and human guide. The formats live in README and `help concepts`. Do not recreate `authoring.md` or add it to the layout API. [E2]
- **Several inventory paths need correction.** Settings codecs now reside under `Settings/Shared/Serialization`, observation under `Shared/Observation`, and mutation under `Shared/Mutation`. The census is an audit list, not 322 required edits. [E3]
- **Included-file identity needs more precision.** An included file has no automatic short ID, but explicit Context selections, Find, References and Route List already put `./path` in their public `id` field. Startup and ordinary linked reads retain null IDs. Preserve that distinction. [E4]
- **Settings v4 is unrelated to JSON output schema 3.** The latter freezes field order, presence, nullability and shared values. The plan needs explicit output compatibility requirements. [E5]
- **Library representability is broader than straddling.** A registration can lie entirely beneath the moved folder and still resist translation through its single destination root. [E6]
- **Task 86 changes recovery integration.** Reserve `.open-forge-state` lexically and physically. Consume its local and legacy store observations without rewriting historical paths. Do not reinstate blanket historical-recovery vetoes. Install retains its specific state-based checks. [E7]

##### 2. The smallest correct design

Keep one immutable layout, one resolver, one payload destination mapping, and the existing command architecture. Distinguish three coordinates explicitly:

1. Package paths, which retain `.agents`.
2. Actual workspace paths, which follow the layout.
3. Source IDs, which remain stable.

Do not replace every `.agents` literal. Fixed controls, package coordinates, native Skills and default examples remain legitimate.

I recommend two delivery PRs. The first provides fresh installation and complete maintenance of custom-root workspaces. The second adds relocation. This changes sequencing, not the accepted outcome. Between them, changing an established root must fail with a truthful explanation. Documentation must not advertise relocation before it exists.

Keep migration’s complete filesystem inventory, private and untracked files, overwrite companions, collision checks, copied-byte verification, ownership translation, literal historical recovery paths and interruption reporting. These protect files Git may not contain.

Cut a generalized mount framework, arbitrary Library projection maps, automatic rollback, replay journals and source-format changes. Keep production, focused tests and local rendering with one packet owner rather than creating separate test and presentation workstreams.

##### 3. The layout seam

Freeze `WorkspaceLayout` under `Framework/Workspace`, with models under `Models/Layout` and resolution under `Shared/Layout`.

The foundation surface should provide:

| Surface | Frozen responsibility |
|---|---|
| `WorkspaceLayoutResolver.ReadAsync(workspace, cancellationToken)` | Read fixed settings once and return layout observations |
| `WorkspaceLayout` | Workspace, main root, loader, README, archive, fixed controls and native Skills paths |
| `CategoryDirectory(id)` | Map categories, including the fixed Skills exception |
| `MapPayloadPath(path)` | Translate package destinations exactly once |
| `TryGetSourceRelativePath(path, out relative)` | Establish source identity without granting mutation permission |
| Membership predicates | Main content, native Skills, guides and implicit content destinations |
| `WorkspaceLayoutRead` | Settings snapshot, optional layout, state and typed issues |

Use `Ready`, `Uninstalled`, `Mismatch`, `Invalid` and `Unavailable`. Issues carry exact paths and causes.

Resolve at operation entry. Carry the layout through existing catalogue, read-session and planning records. Status and Doctor contributors share that observation. Rendering receives operation-owned facts, without reading settings or depending on Framework. Revalidate under Task 86’s lease.

`memory/crystallized` remains the same ID beneath either main root. Native packages retain `skills/example`. `.agents/foo` remains literal, and custom exact paths require `./docs/forge/foo`. Discovery does not activate routes. [E4]

For default compatibility, freeze fixtures against the **integrated Task 86 baseline**:

- Default payload mapping and rendering are identity transformations.
- Preserve ordering, selected bytes, generated rows, ownership paths and existing text/JSON.
- Omit optional layout output for an ordinary default workspace.
- Keep default-created settings at v3 unless explicit main-root configuration requires v4.

That last point deliberately rejects the plan’s blanket new-file v4 exception. Separate the latest readable version from the default emitted shape. Otherwise “byte-for-byte unchanged” is false. New help, the install question and intentional documentation edits need individually reviewed exceptions.

##### 4. Settings and validation

Publish `schemas/v4/open-forge.schema.json`. Leave published v1–v3 schemas and lock schema v1 unchanged.

`mainRoot` is optional. Absence means `.agents`. Explicit null, an empty string and other types are invalid. Validate the understood key regardless of the declared schema version.

Reuse `PortableWorkspacePath` for canonical portable syntax. Authored settings use forward slashes, NFC text, no leading `./`, no trailing slash, no empty or traversal segments, no absolute/drive/UNC path, no forbidden characters and no reserved device names. [E8]

Reject:

- The workspace root.
- Git metadata and reserved control/runtime locations.
- `.agents` descendants as custom roots.
- Physical aliases into those locations.
- Non-directory roots and unsafe ancestry.

Allow exactly `.agents` as the default exception. A missing root is valid for installation planning. Filesystem existence and migration collisions remain observation questions, not JSON schema questions.

Validate `roots` after resolving the effective layout. Entries remain literal files outside the main and native Skills source namespaces, excluding controls and runtime storage. Preserve their order and unlisted registrations. Keep explicit `allowInstallPaths` literal. Implicit destination admission follows the main root and native Skills, subject to all existing ownership and containment checks.

| Observation | Required result |
|---|---|
| Valid settings and agreeing files | Use the selected layout |
| Established workspace lacks selected loader | Report that path and block layout-dependent writes |
| Recorded ownership or managed pointers disagree | Report both locations and block |
| Competing former and selected loaders | Report mismatch without choosing silently |
| Invalid or unreadable settings | Report the real failure without using the reader’s synthetic defaults |
| Fresh workspace without installation evidence | Install may establish the requested layout |

Configure may resolve disagreement only from an explicit, unambiguous plan. Help remains workspace-independent. Cleanup retains access to Task 86’s fixed recovery stores.

Ordinary settings rewrites preserve declarations, unknown values and property order. They do **not** preserve comments or formatting today. Do not promise otherwise. Explicit main-root configuration owns an intentional v4 declaration change and must show it in preview. [E3]

Older CLI behavior follows from current code:

| Workspace | Older reader behavior |
|---|---|
| v4 with default layout | Reads understood keys and can report an unfamiliar version |
| v4 with custom root | Ignores `mainRoot` and still scans `.agents` |
| v4 with newly permitted known-key values | May reject those values under older validation |

There is no reliable old-writer interlock. Unknown keys and versions are intentionally tolerated. An old CLI may produce incomplete observations or plan against the wrong layout. Document the first supporting release and require it for custom-root CLI use. Do not claim the runtime lock bridge provides semantic compatibility. [E3, E9]

##### 5. Recommendations for every open question

| Plan question | Recommendation |
|---|---|
| Incomplete #6 | Closed. Use the implemented generic exact-path and inclusion behavior |
| Byte-identical default versus v4 | Retain v3 default emission. Write v4 through explicit main-root configuration |
| Partial migration without replay | Retain recovery evidence and report residuals. Block blind retries |
| Authored ignore rules | Preserve bytes. Translate only proven owned rules and sharing records. Block when destination privacy cannot be demonstrated |
| Straddling Library registrations | Block unless every translated destination is representable by one destination root with unchanged source suffixes |
| Library sources inside the old root | Pin source trees and preserve registrations. Block ambiguous overlaps |
| Custom-root Skills collision | Reserve `skills` for the native mount. Reject a competing root category |
| Older CLIs | State the minimum supported release. Preserve plain-file startup |
| Competing loaders or hand-edited settings | Never infer a fallback. Require recorded evidence for Configure |
| Literal paths in prose | Rewrite supported parsed links and managed regions. Report remaining references for manual review |

The Library check must test representability, not just whether a registration straddles the boundary. For example, destination root `.` plus suffix `.agents/guidance/a.md` cannot become `docs/forge/guidance/a.md` merely by changing the destination root. Offer reviewed detach/reattach guidance using narrower source roots where possible. Do not silently change sources or introduce lock schema v2. [E6]

##### 6. The user’s experience

Ask:

> Where should Open Forge keep its context files?  
> Directory relative to this workspace [.agents]:

Explain beside the question that native Skills and settings stay under `.agents`. Explicit input suppresses that question. Preview, JSON, automatic and redirected invocations never prompt. Fresh unattended installation defaults to `.agents`. Configure defaults to the current root.

`--main-root` is an Install option. An established workspace needs `--configure` to change it. Supplying its current root is an ordinary no-op. Preserve the selected root in retry commands.

Custom-root Status should show the main directory and loader separately from fixed controls and runtime storage. Keep default output unchanged. Add optional command-local `data.layout` for custom or mismatched layouts. Define its fields once in the owning contract. Keep the shared JSON envelope at schema 3. Layout relocation must not reuse Install’s existing adoption `data.migrations` rows with a different meaning. [E5, E10]

Root `AGENTS.md` names the selected loader. Root `CLAUDE.md` retains `@AGENTS.md` and imports that loader directly. Rewrite only recognized managed regions. The custom loader links relatively to stationary Skills and included documents. An agent starting without the CLI follows those files normally, including when README is absent. [E11]

Help and the docs site must distinguish “main root” from included-file `roots`. Keep default examples and add one complete custom-root journey.

##### 7. Implementation plan

Freeze the API, version policy, mismatch behavior and JSON additions before parallel work. Each Astra high packet gets a separate worktree, exact ownership and corresponding tests. Workers preserve other packets’ edits. Shared API changes return to the foundation owner.

| Wave | Packets and ownership | Required evidence |
|---|---|---|
| 0 | Foundation owner: layout, settings, schema, shared contracts and composition seam | Validation matrix, version tolerance, unknown-key preservation, default bytes, runtime reservations |
| 1 | Separate packets for source identity/inventory, loading/includes, and payload projection | Stable IDs, literal operands, overwrite pairing, included-ID projections, identity mapping |
| 2 | Separate packets for navigation/sharing and Install setup/pointers | Custom Install → Context → Index, offline startup, private routes, no-prompt behavior |
| 3 | One packet per Route leaf and per lifecycle leaf, after shared route support | Actual destinations, no reactivation, foreign-file hashes, default snapshots |
| 3 | Separate Extension and Library shared packets, followed by their leaf packets | Mapped ownership, literal grants, source preservation, projection representability |
| 4 | Install relocation planner and application packets, integrated sequentially | Full inventory, source edits, collisions, copy verification and interruption boundaries |
| 5 | Status/Doctor observations, documentation batches and integration qualification | Text/JSON parity, mismatch reporting, published-process journeys |

Each command packet owns its matching result, rendering and OutputText changes. Avoid a late sweep that leaves paths correct internally but wrong in output.

The documentation packet must cover these current authorities. `C` means the CLI document directory identified in Evidence.

- **Shared CLI:** `C/architecture.md`, affected layer documents, `command-contract-set.md`, `shared-operation-contract.md`, and shared source-reference, source-universe and workspace-permission contract pairs. Keep result-coordinate guarantees unchanged and reference them.
- **Commands:** Install, Update, Remove, Repair, Cleanup, Index, Context, Find, References, Status, Doctor and Help. Update all nine Route leaves, all Extension leaves and all five Library leaves where layout meaning or output changes. Include their contract catalogues and affected technical designs.
- **Technical designs:** new `workspace-layout.md`, plus generated navigation, lifecycle provenance, embedded payload, forgiving maintenance, workspace permissions, workspace libraries and mutation/recovery.
- **Framework authorities:** Framework architecture, routing paths and scope, native Skills, and the top architecture’s affected summary.
- **Payload maintenance:** AGENTS, CLAUDE, loader, README and Skills contracts, plus affected catalogue summaries.
- **Shipped and dogfood material:** host pointers, loader, README, CLI Skill and its `common`, `discovery`, `routes` and `packages` references. Preserve default installed bytes wherever wording need not change.
- **Public material:** root README, `docs/cli.md`, `docs/extensions.md`, concepts help, and site installation, grow-your-framework, greenfield-and-brownfield, concepts index, routing, loading-and-tags, customizing, CLI index/flows and glossary.
- **Acceptance records:** relevant experience flows/scenarios, Task 85 and its replacement execution plan. Preserve historical council records as history.

Use focused unit and filesystem integration tests per packet. Final qualification includes default/custom/default relocation, custom-root Update without recreating the old loader, source-reading parity, removed-row preservation, pinned older-CLI experiments in disposable fixtures, and supported managed/AOT platform gates.

Estimate **15–21 engineering days**, including documentation and qualification. A fresh-install-first PR is approximately **10–14 days**, followed by **5–7 days** for relocation. These estimates assume Task 86 is integrated and exclude new Library mapping or replay machinery.

#### Risks and what I would not build

The largest compatibility risk is an older writer interpreting a custom workspace as `.agents`. The largest migration risks are private files losing ignore protection and Library destinations that cannot retain their recorded mapping.

I would not build old-CLI traps, forwarding loaders, automatic settings discovery, arbitrary mount configuration, Library source relocation, broad prose replacement or generalized recovery replay.

This was a read-only source review. No builds, tests or Doctor runs were performed. Runtime compatibility and platform claims still require the proposed qualification.

#### Evidence

Prefixes below are repository-relative:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `R` = `src/cli/rendering/OpenForge.Cli.Rendering/Presentation`
- `C` = `.agents/memory/crystallized/documents/cli`

**E1:** `F/Sources/Identity/SourceReferenceParser.cs:26`, `O/Route/Include/RouteIncludeDefinitions.cs:9`, `O/Route/Exclude/RouteExcludeDefinitions.cs:9`, `O/Help/HelpDefinitions.cs:20`.

**E2:** `F/Sources/Identity/SourceLogicalPath.cs:8`, `.agents/memory/crystallized/documents/maintenance/payload/agents/loader.md:50`.

**E3:** `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.cs:48`, `:84`, `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:16`, `:94`, `F/Settings/Models/Observation/WorkspaceSettingsRead.cs:21`.

**E4:** `C/contracts/shared/source-references/interface.md:53`, `:68`, `F/Sources/Identity/SourceIdentity.cs:14`.

**E5:** `C/contracts/shared/result-coordinates/interface.md:160`, `R/Shared/Rendering/CliJsonRenderer.cs:24`.

**E6:** `F/Libraries/LibraryPathIdentity.cs:10`, `F/Libraries/Models/Observation/LibraryMapping.cs:37`.

**E7:** `.agents/memory/emerging/analysis/lock-scope-council/plan.md:75`, `:94`, `:278`, `:297`.

**E8:** `F/Filesystem/Shared/Paths/PortableWorkspacePath.cs:12`, `:37`.

**E9:** `.agents/directives/software/versioned-configuration-schema.md:15`, `F/Settings/WorkspaceSettingsDefinitions.cs:18`, `F/Sources/Reading/SourceReadSessionReader.cs:35`.

**E10:** `O/Install/Models/Planning/InstallMigrationPlan.cs:6`, `C/contracts/install/interface.md:882`, `O/Install/Shared/Configuration/InstallSetupResolver.cs:26`, `:40`.

**E11:** `src/open-forge/AGENTS.md:5`, `src/open-forge/CLAUDE.md:3`, `.agents/memory/crystallized/documents/maintenance/payload/CLAUDE.md:17`.

Accepted scope: `.agents/memory/emerging/analysis/route-include-council/record.md:173`, `:228`, `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:13`, `:38`.

## P6: Migration and safety engineer

/root - GPT-6 - high

#### Recommendation

Ship root selection for new installations first, after Task 86, and defer moving existing workspaces to a separate delivery. Keep migration in Task 85’s accepted outcome, but do not call that outcome complete after installation support ships. The current move plan leaves important gaps around interrupted publication, continuous ignore protection and file metadata. Retain fixed controls, native Skills and runtime storage, with one resolved layout governing all content consumers. Do not release migration until a tested recovery procedure can restore an interrupted move without overwriting subsequent user edits.

#### Answers

##### 1. What the plan gets wrong today

- **The prerequisite caveat is obsolete.** Generic `./<workspace-path>` references now exist. Include, Exclude and Help definitions are present. The old plan should stop treating these as future dependencies. (E1)
- **There is no separate authoring guide.** `SourceLogicalPath` defines the loader and README, but no `AuthoringGuidePath`. The loader directs authors to the README’s file-format section or `help concepts`. Remove the proposed member and authoring-file packets. (E2)
- **The census is an audit, not an edit budget.** Repeating its expression returns 334 C# files in this snapshot, versus 322 in the plan. Package coordinates, fixed controls and examples must not all be converted into runtime paths. (E3)
- **Reading has acquired an included-source branch.** `ReadIncludedAsync` extends the ordinary catalogue. Both branches must carry the same layout. Preserving only the main catalogue would lose included-source behavior. (E4)
- **Skills maintenance must remain functional.** Index now traverses rooted Skill catalogues through a fixed `.agents/skills` assumption. A custom loader must retain that capability without activating an unlisted Skills route. (E5)
- **Library compatibility is narrower than the plan suggests.** One destination root plus unchanged source suffixes determines every projection. Even a registration whose projections all move can be unrepresentable afterward. (E6)
- **Byte verification is not metadata preservation.** Current snapshots contain bytes and expectations. Staging writes a new ordinary file. These mechanisms do not establish preservation of executable modes or access restrictions. (E7)
- **The proposed ignore ordering is unsafe.** The plan copies files before publishing ignore changes. An interruption can leave private copies exposed at the destination. (E8)

Task 86 supplies fixed `.open-forge-state/` paths, local recovery creation, explicit local and legacy observations, and recorded deletion locations. Use those APIs unchanged. Its blanket-history changes do not abolish Install’s state-based recovery boundary. Install still distinguishes mixed prior/intended state from historical evidence. (E9, E10)

##### 2. The smallest correct design

The first delivery should support `install --main-root` in a workspace without an established Open Forge layout. Require an empty or absent content destination. Existing project documents remain usable through Include.

That delivery must still convert every consumer needed to maintain a custom installation. Update, Remove, Repair, Index, routes, Extensions and Libraries cannot remain default-root-only. A custom installation that cannot subsequently update safely is incomplete.

For an established workspace, a different `--main-root` must fail before effects until migration ships. Same-root Configure remains available. This is an explicit staging recommendation that changes delivery order, not the accepted final outcome.

Cut the generic mount framework, separate authoring guide, broad compatibility scaffolding and separate production/test owners. Defer combined ownership publication until migration actually needs it.

The later move must satisfy these conditions:

1. **Inventory physical entries.** Include ignored, untracked, hidden, binary and overwrite files, plus empty directories. The source catalogue is insufficient because it skips unrecognized files. Pin controls, Skills, runtime storage and registered Library sources. (E11)
2. **Reject ambiguous moves.** Block occupied destinations, equal-byte collisions, nested roots, case aliases, nested repositories, unsupported links and incomplete inventory. Returning to `.agents` permits only verified pinned occupants. Invalid ownership blocks planning. Ordinary user-owned files do not.
3. **Translate only proven relationships.** Remap actual Framework and Extension receipts, absent exclusions, link-ignore paths and owned sharing records. Keep permission grants literal. Preserve `roots` paths and unlisted registrations. Rebase supported Markdown destinations and managed host regions without broad text replacement.
4. **Preserve data throughout application.** Under Task 86’s lease, revalidate the inventory and prepare recovery before effects. Protect both locations before copying private content. Verify destination bytes and supported metadata before each source deletion. Recheck sources immediately before deletion.
5. **Make interruption recoverable.** Verify new content before publishing pointers and settings. Delete the old loader last. Retain exact recovery evidence and report partial effects. A fresh invocation must inspect current state, never assume equal bytes authorize continuation.

The staged ignore changes and restoration procedure need explicit contracts before implementation. Existing immutable bundles are evidence, not replay instructions. (E12)

##### 3. The layout seam

Freeze one immutable `WorkspaceLayout` under Framework’s Workspace capability. Keep `CliWorkspace` as physical workspace identity.

Its minimal surface is:

```text
WorkspaceLayout
  Workspace
  MainRoot
  LoaderPath
  HumanGuidePath
  NativeSkillsRoot
  SettingsPath
  OwnershipPath
  CategoryDirectory(categoryId)
  MapPayloadPath(packagePath)
  TryGetSourceRelativePath(path)
  IsImplicitContentDestination(path)

WorkspaceLayoutResolver.ReadAsync(workspace, cancellationToken)
  → WorkspaceLayoutRead(settings, layout, state, issues)
```

Use the states Ready, Uninstalled, Mismatch, Invalid and Unavailable. Runtime paths remain owned by Task 86’s `WorkspaceRuntimePaths`.

Resolve once at operation entry. Pass the result through planning facts, `SourceCatalogue` and `SourceReadSession`. Status and Doctor share one observation. Rendering receives concrete paths through Operations-owned results and performs no layout lookup.

There are two fixed content locations, not arbitrary mounts: the selected main root and native Skills. Discovery does not activate either. The loader remains authoritative.

Package `.agents/...` coordinates map to the main root, except Skills and reserved controls. Exact workspace operands stay literal. Source IDs remain root-relative, with `skills` reserved for native Skills.

For the default layout, preserve existing bytes, ordering, selected reads and ordinary output. Do not insert `mainRoot` during unrelated rewrites. Explicit exceptions are the new install question, advertised help changes and newly created settings declaring v4.

##### 4. Settings and validation

Publish v4 without modifying v1–v3. Add optional `mainRoot`, defaulting to `.agents`.

Accept a canonical workspace-relative directory with `/` separators. CLI input may normalize backslashes and leading `./`. Reject null, empty values, absolute paths, traversal, empty segments and unsupported portable names.

Reject Git metadata, runtime storage, control paths and custom roots beneath `.agents`. Exact `.agents` remains the default exception. Require safe physical containment and ordinary directory ancestry below the resolved workspace. Configure additionally rejects overlapping source and destination trees.

Validate `roots` against the selected root and fixed protected locations. The present blanket rejection of `.agents/...` is no longer the right membership rule. Preserve the native Skills catalogue’s special route semantics. (E13)

When settings and files disagree:

- Never fall back to another loader.
- Read commands name the selected path and report disagreement.
- Content writers stop.
- Status and recovery inspection remain available.
- Cleanup retains Task 86’s independent storage scope.
- Configure may resolve a mismatch only from one evidence-backed source layout.

Preserve existing declarations, unknown values and property order on settings rewrites. New settings use v4. Published older CLIs tolerate unfamiliar schema versions and ignore unknown keys, so v4 cannot make them refuse safely. They may still operate using `.agents` assumptions. Require the first supporting release for custom-root workspaces and state that older writers are unsupported. (E14)

##### 5. Recommendations for every open question

| Plan question | Recommendation |
| --- | --- |
| Partial #6 snapshot | Close this prerequisite. The relevant interfaces now exist. |
| Byte-identical default versus v4 | Preserve existing workspace bytes and ordinary output. Permit the explicit new-install and help differences above. |
| Partial migration without replay state | Defer migration until restoration is tested. Retain immutable evidence and block blind retries. Do not invent automatic rollback. |
| Authored ignore rules | Preserve authored bytes, including nested ignore files. Verify destination protection before copying and retain source protection until deletion. Block when protection cannot be established. |
| Library registration straddles the move | Reject it with the exact conflicting mappings. Do not split registrations or silently reattach them. |
| Library source inside the old root | Pin it. Block if pinning intersects the loader, controls, required moved content or a projection effect. Explain why the old directory remains. |
| Custom-root `skills` conflict | Reserve the ID and category for `.agents/skills`. Reject a competing category. |
| Older CLI ignores `mainRoot` | Document the supporting release in custom-root guidance. Do not claim schema versioning or Task 86’s lock bridge prevents old-layout writes. |
| Competing loaders or edited settings | Report both paths. Require consistent recorded evidence before selecting a migration source. |
| Literal paths in prose | Rewrite parsed supported links and recognized managed regions only. Report remaining literals for review. |

Strengthen the Library rule beyond “straddling.” For every recorded suffix `p`, the intended destination must still equal `newDestinationRoot/p`.

For example, a registration rooted at `.` with suffix `.agents/directives/team.md` cannot become `docs/forge/directives/team.md` merely by changing its destination root. Changing the suffix would also change the source file being projected. Block that case without introducing per-file mappings or a new ownership schema. (E6)

Extensions are different. Their existing ownership stores actual destination paths and regions, so those receipts can be translated while preserving owner IDs, versions and shared claims. Map package destinations before exclusion, collision and permission checks. Moving user files must never create ownership claims. (E15)

##### 6. The user’s experience

Ask during interactive installation:

> Where should Open Forge keep its context files?  
> Default: `.agents`

Explain beside the choice that settings remain in `.agents`, Skills remain in `.agents/skills`, and runtime state remains in `.open-forge-state`.

Preview, JSON and unattended execution never ask. A custom-root preview shows the selected loader, fixed locations and affected paths. During the first delivery, an existing-workspace root change reports that relocation is not yet supported and makes no changes.

Custom-root Status identifies the selected root and loader. Keep default-root output unchanged. Preserve the current “listed” and “unlisted” vocabulary. (E16)

Rewrite only recognized Open Forge regions in root `AGENTS.md` and `CLAUDE.md`. They must name the actual loader and remain sufficient when the CLI is unavailable. The selected loader links relatively to native Skills and external includes. Deleting its README must retain the existing `help concepts` fallback.

Update installation, customization, routing, CLI flows, glossary and CLI Skill documentation. Explain `mainRoot` separately from included `roots`. Examples must show that `.agents` still exists with a custom root.

For the later move, require these interruption outcomes:

| Interruption point | What the user sees |
| --- | --- |
| Before content effects | Original layout remains selected. Any prepared bundle is reported accurately. |
| During copying | Original files remain. Partial destination paths and recovery location are visible. |
| During control publication | Settings, ownership and host disagreement is reported. Content writes stop. |
| During source deletion | The selected new loader and remaining old paths are identified separately. |
| After subsequent user edits | Changed paths block restoration. Neither copy is overwritten automatically. |

Abrupt termination cannot promise a final message. The next invocation must reconstruct observations from files and retained evidence. Cleanup deletes recovery artifacts, not restores the workspace. An offline agent can follow the currently published pointer, but cannot diagnose every mixed CLI state. Do not claim otherwise.

##### 7. Implementation plan

Freeze the layout API above after Task 86 lands. One foundation owner owns layout, settings and shared composition changes. Each separate Astra high implementer owns a bounded production packet and its mirrored tests in an isolated worktree. Split command families into leaf packets instead of assigning overlapping directory-wide ownership.

| Wave | Packet ownership | Required evidence |
| --- | --- | --- |
| 0 | Layout model/resolver, settings codec and v4 schema | Canonical paths, physical containment, version tolerance, default no-op bytes |
| 1 | Separate identity/inventory and loading/include packets | Stable IDs, literal exact paths, fixed Skills, ordered included reads |
| 2 | Distribution, navigation/privacy and Index packets | Custom Install → Context → Index slice, unchanged default payload, no route reactivation |
| 3 | One packet per route leaf, then separate Extension and Library packets | Custom-root lifecycle, exclusions, permissions, collisions, native Skills maintenance |
| 4 | Install/configuration, Update, Remove and Repair packets | Fresh custom install, mismatch refusal, interrupted installation, no recreated legacy loader |
| 5 | Status/Doctor projections and command-local output packets | Actual paths, default snapshots, no observation effects, Task 86 store independence |
| 6 | Documentation and published-process qualification | Offline startup, deleted README, help examples, managed and supported Native AOT gates |
| Later | Migration inventory/planning, then application/recovery packets | Full round trips and interruption recovery |

Keep shared root composition with the foundation owner throughout. Rendering changes travel with their command packet when practical. Documentation packets should contain a coherent small page set, with one owner coordinating terminology and examples.

The migration release gate includes:

- Default → custom → another custom → default.
- Ignored and untracked private notes, binary files, overwrites and empty directories.
- Owned and authored ignore behavior throughout interruption.
- Library source hashes, raw link identities and unrepresentable mapping rejection.
- Extension receipts, shared claims, absent exclusions and literal permission grants.
- Failures during copying, publication and deletion, including edits after interruption.
- Historical bundles unchanged and runtime storage never moved.

Estimate **12–16 engineering days** for custom installation and complete lifecycle support. Allow **7–10 additional days** for bounded migration after its recovery and metadata contracts are closed. These are planning estimates, not demonstrated throughput.

#### Risks and what I would not build

Ignored and untracked content makes Git an incomplete recovery boundary. Local recovery also remains vulnerable to deletion of the workspace or its runtime directory.

I would not build arbitrary mounts, automatic rollback, general filesystem migration, per-file Library remapping, whole-repository text replacement, silent loader fallback or old-CLI compatibility tricks.

The decisive uncertainty is whether existing recovery primitives can support a clear restoration procedure for every intermediate layout and ignore state. That uncertainty justifies deferring moves.

This was read-only source review. No files changed, builds or tests ran, or Doctor executed. Task 86 behavior is taken from its frozen plan, not claimed as implemented.

#### Evidence

Prefixes below identify repository-relative paths:

- `F` = `src/cli/framework/OpenForge.Cli.Framework/Framework`
- `O` = `src/cli/operations/OpenForge.Cli.Operations/Commands`
- `C` = `.agents/memory/crystallized/documents/cli`
- `P` = `.agents/memory/emerging/analysis/route-include-council`
- `L` = `.agents/memory/emerging/analysis/lock-scope-council`

**E1:** `F/Sources/Identity/SourceReferenceParser.cs:26`, `O/Route/Include/RouteIncludeDefinitions.cs:9`, `O/Route/Exclude/RouteExcludeDefinitions.cs:9`, `O/Help/HelpDefinitions.cs:7`.

**E2:** `F/Sources/Identity/SourceLogicalPath.cs:8`, `src/open-forge/.agents/loader.md:38`, `src/open-forge/.agents/README.md:74`.

**E3:** `P/main-root-plan.md:29`. Its census expression was rerun read-only and returned 334.

**E4:** `F/Sources/Reading/SourceReadSessionReader.cs:35`, `F/Sources/Reading/SourceReadSessionReader.cs:51`.

**E5:** `O/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs:11`, `O/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs:52`.

**E6:** `F/Libraries/Models/Observation/LibraryMapping.cs:37`, `F/Libraries/Models/Identity/LibraryRegistration.cs:24`, `C/technical-designs/workspace-libraries.md:77`.

**E7:** `F/Mutation/Models/Filesystem/Files/FileStateSnapshot.cs:17`, `F/Mutation/Application/FileChangeApplier.Staging.cs:21`.

**E8:** `P/main-root-plan.md:339`, `P/main-root-plan.md:341`.

**E9:** `L/plan.md:75`, `L/plan.md:239`, `L/plan.md:278`, `L/plan.md:295`.

**E10:** `O/Install/Shared/Recovery/InstallRecoveryCatalogueReader.cs:52`.

**E11:** `F/Sources/Inventory/SourceCatalogueReader.cs:187`, `P/record.md:182`.

**E12:** `C/technical-designs/mutation-and-recovery.md:169`, `C/technical-designs/mutation-and-recovery.md:432`.

**E13:** `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:16`.

**E14:** `F/Settings/WorkspaceSettingsDefinitions.cs:17`, `F/Settings/Shared/Serialization/WorkspaceSettingsCodec.Roots.cs:94`.

**E15:** `F/Ownership/Models/Document/WorkspaceOwnershipDocument.cs:17`, `O/Extension/Install/Shared/Planning/ExtensionInstallPayloadNormalizer.cs:45`.

**E16:** `src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusWording.cs:8`.

Accepted outcome and delivery scope: `P/record.md:177`, `.agents/memory/working/cli-development/tasks/task85-configurable-main-root.md:12`.
