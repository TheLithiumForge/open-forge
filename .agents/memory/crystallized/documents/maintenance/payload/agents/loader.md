---
open-forge:
  description: Current maintenance contract for the installable Open Forge loader and its dogfood counterpart
  responsibility: Preserve the loader's authored contract, source and dogfood alignment, generated boundary, and verification
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Framework, Loader, Routing]
---

# Loader Maintenance Contract

## Source

[`src/open-forge/.agents/loader.md`](../../../../../../../src/open-forge/.agents/loader.md) is the canonical installed Framework loader. The repository [loader](../../../../../../loader.md) dogfoods the same authored contract with its locally generated root `entries`.

The [Framework Architecture](../../../framework/architecture.md#canonical-entry) is authoritative for the loader's structural role. The [top architecture](../../../architecture.md#framework-composition) is authoritative for the relationship among Core, Memory, and Extensions. The [Open Forge Routing scope](../../../framework/routing/_routing.md) defines the detailed `route`, scope, loading, path, and [overwrite](../../../framework/routing/overwrites.md) contracts. The [routed Markdown contract](../../../framework/markdown/routes.md) is authoritative for its canonical authored and generated representation. The [routing model](../../../../decisions/framework/routing-model.md), [routing surfaces](../../../../decisions/framework/routing-surfaces.md), [scope and slugs](../../../../decisions/framework/scope-and-slugs.md), [tags](../../../../decisions/framework/tags.md), [loading reliability](../../../../decisions/framework/loading-reliability.md), [typed authority terminology](../../../../decisions/framework/authoritative-source-terminology.md), and [source and packaging](../../../../decisions/framework/source-and-packaging.md) decisions preserve accepted rationale.

## Contract

### Responsibility and shape

- The loader remains the first Open Forge reading procedure after the workspace entry. It works with plain files and needs no repository-only documents or CLI.
- Its introduction explains that its Entries list what agents use. The authored shape is one Axioms section with three groups, in order: `Read before you work`, `Use what you read` and `Change what agents use`, followed by Entries.
- The loader keeps the full reading procedure, rule inheritance, acceptance, conditions, loading tags, off switch, dependencies and missing-file rules.
- The file-format section of the optional [workspace guide](README.md), also printed by `open-forge help concepts`, carries file formats, catalogue naming, metadata forms, scope examples and vocabulary. The [CLI Skill](skills.md) carries command discovery and maintenance procedures. Category behavior stays in its own catalogue.

### Reading and applicability

- Reading starts by identifying every working file, including planned files and both rename paths. Background reads do not enlarge that set. Start, resume, restored context and changed goals or working files trigger a fresh pass.
- When available and allowed, `context` receives one `--for` per working file and supplies required context. The reader still selects relevant on-demand material and follows explicit reading instructions. A missing or failed CLI, or a link it cannot follow, falls back to file tools.
- Conditions are checked before required reads. One working file must satisfy an entry's condition and every conditioned ancestor. Patterns within one condition are alternatives.
- Globs are case-sensitive and workspace-relative. Root-only patterns, recursive patterns, folder patterns and a literal leading `!` remain explained. Conditions neither select a branch nor grant edit permission. Unknown working files leave conditioned required entries pending until resolved.
- Required entries are read in listed order through loaded catalogues, once per pass, regardless of their individual descriptions. Other entries are selected by relevance. Search and direct links require the applicable catalogue chain before use. Separately selected branches stay separate.
- #LoadNow and #KeepInMind act only through loaded parents. #KeepInMind adds rereads at start, resume, context restoration and handoff or closeout. During work, reread only when active material may have changed. Specialized required reads need an unbroken, appropriately scoped catalogue chain.

### Using context

- Loaded catalogue Axioms and rule-file Instructions bind within their stated scope. Narrower rules add to broader rules without outranking them. Conflicts and instructions that cannot be followed are reported with reasons.
- User direction remains bounded by platform and safety limits. Routine reversible work proceeds within the request. Significant unresolved choices require user judgment, and assumptions affecting the result remain visible.
- Replies match the request's depth. The shared defaults keep the current understanding, one recommendation and at most one important open choice, with concise wording that retains needed conditions and exceptions.
- Acceptance comes from the user or a source the user named for the question. Evidence, reading, links, saving, tags, repetition and confidence do not establish acceptance or edit permission. #Contextual and #CurrentTruth retain their scoped state meanings.
- Each source answers its own question. Links do not transfer authority, scope, loading or lifecycle. Accepted non-binding advice may specialize broader material of the same kind.
- Conflicting accepted sources are investigated before editing. Accepted changes reach the source that defines current state and preserve useful context.
- #Evergreen creates an update duty without authority or loading. Affected editable sources are updated before dependent work and by closeout. Related updates may be batched safely, and blocked updates are reported.

### Enabling and customization

- Loader rows may include a catalogue or any workspace text file by a containing-file-relative link. A foreign file needs no Open Forge frontmatter because the row carries its description, tags and conditions. Included Markdown catalogues expose their Entries. The loader points to `route include` for CLI assistance.
- Removing a loader row disables its branch without deleting files or canceling accepted requirements or host-loaded instructions. Background reading does not reactivate rules. Removed rows stay absent until the user asks to restore them.
- Dependencies are removed or replaced before a branch is disabled. Missing listed files or required dependencies are errors. Empty Entries are valid.
- Other catalogues follow their folder contents. Keeping a file out requires moving or deleting it. Managed removals are also recorded in exclusions before updates.
- Before authoring workspace context, catalogues or metadata, the loader sends agents to the file formats in `README.md`, to `open-forge help concepts` when that page was deleted, and otherwise to the shape of a neighbouring file. It names README by path, not by link, so deleting the guide leaves no broken link.
- An adjacent overwrite follows its base with the same role, scope and loading conditions. It wins only for corresponding content and is neither listed nor selected independently.

### Distribution and baseline

- Source and repository authored loader content stay aligned. Locally maintained Entries may differ in descriptions, tags and selected roots. The shipped rows require Directives and Memory, while Guidance, Maps, Patterns, Skills and Templates stay on demand.
- Entries preserve authored root links, order and intentional omissions. They may include foreign leaves or catalogues and must not flatten nested catalogues.
- Measured at `a438be5bd`, the shipped loader has 29 non-empty lines before `## Entries`, excluding that heading and its body. The full file has 37 non-empty lines, including the Entries heading and seven rows. Count any line containing a non-whitespace character, including headings and list items. This is a review baseline, not a ceiling.

## Verification

- [SourceLoaderParserTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Routing/SourceLoaderParserTests.cs) cover ordered declarations, valid empty Entries and malformed input.
- [SourceLoadingClosureResolverTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Loading/SourceLoadingClosureResolverTests.cs) cover loading order, deduplication, continuity and conditioned ancestry.
- [ContextApplicationIntegrationTests](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Context/ContextApplicationIntegrationTests.cs) cover startup order, selected chains, overwrites and read-only context results.
- Use the [payload integrity checks](../_payload.md#verification) for shipped bytes. Compare source and repository text before Entries, measure the baseline again and inspect intentional row differences.
- Manually trace the reading procedure with a foreign leaf, a conditional catalogue, an overwrite, an omitted row and an empty list. Verify the file formats remain available through Help with the README removed. Automated loading tests do not prove that an agent follows prose instructions.
