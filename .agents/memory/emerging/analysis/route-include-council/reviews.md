---
open-forge:
  description: The eight anonymous Stage 2 peer reviews and rankings of the route include council
  tags: [Memory, Analysis, Contextual, Candidate, Framework, CLI, Council]
---

# Council 4 Reviews

Each member reviewed and ranked the seven other proposals without knowing their authors. Proposal labels are expanded to the member's perspective here. The [record](record.md) holds the aggregated rankings and the decisions.

## The CLI Product Designer (Proposal D)

### 1. Critiques

**The CLI Architect (Proposal A).** Its strongest contribution is the concrete implementation path toward one shared source inventory, loading graph, and bounded loader-editing mechanism. Its config nevertheless duplicates descriptions, tags, and conditions that the loader controls, adding avoidable drift and reconciliation states.

**The Unix Minimalist (Proposal B).** It offers the strongest minimal config design, preserves published schemas, and distinguishes registration from activation clearly. Its main gap is reading parity, because it does not specify how Context-only and help-led agents obtain the effective workspace-local authoring rules.

**The Model-Behavior Realist (Proposal C).** It gives the clearest answer to reading parity through `context --authoring`, local authoring content in help, and shared read-trace fixtures. Its root plan unnecessarily places recovery state inside the custom content root, while today’s [recovery storage](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs) is external and keyed by physical workspace identity. Duplicated config metadata and its in-place v2 schema amendment also need correction.

**The Contrarian (Proposal E).** Its strongest contribution is disciplined protection of existing documents, including preservation of handwritten entries and separate reporting of Framework-only and effective startup measurements. Making the requested #7 conditional on another demand-validation gate changes the maintainer’s stated delivery plan, while its #6 schema amendment also conflicts with the repository’s versioning rule.

**The Contract Guardian (Proposal F).** It defines unusually complete machine-facing results, including unavailable facts, intended preview state, verified effects, and existing result-envelope conventions. Its most serious flaw is replacing the complete loading policy on repeated Include, with omitted tags and conditions becoming empty lists, which can silently remove required loading or broaden applicability.

**The Brownfield Team Lead (Proposal G).** Its concrete adoption session and uncoached undo exercise make the first-use promise testable. However, Include refuses to register an existing handwritten entry while Exclude operates only on registered entries, creating two visibly similar relationships with different command behavior.

**The Migration And Safety Engineer (Proposal H).** It provides the strongest migration design, especially for ignored notes, private navigation, stable mutation locks, historical recovery paths, and Library projections. Its proposed in-place settings and lock-schema changes conflict with the existing [schema Directive](../../../../../.agents/directives/software/versioned-configuration-schema.md), which says, “Once a version ships, its schema document stops changing.”

### 2. Rankings

```text
RANKING CONCEPT:
1. The Unix Minimalist (Proposal B)
2. The Migration And Safety Engineer (Proposal H)
3. The Contract Guardian (Proposal F)
4. The CLI Architect (Proposal A)
5. The Model-Behavior Realist (Proposal C)
6. The Contrarian (Proposal E)
7. The Brownfield Team Lead (Proposal G)
RANKING SURFACE:
1. The Unix Minimalist (Proposal B)
2. The CLI Architect (Proposal A)
3. The Migration And Safety Engineer (Proposal H)
4. The Model-Behavior Realist (Proposal C)
5. The Contract Guardian (Proposal F)
6. The Contrarian (Proposal E)
7. The Brownfield Team Lead (Proposal G)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Migration And Safety Engineer (Proposal H)
3. The Brownfield Team Lead (Proposal G)
4. The CLI Architect (Proposal A)
5. The Unix Minimalist (Proposal B)
6. The Contract Guardian (Proposal F)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Unix Minimalist (Proposal B)
3. The Contract Guardian (Proposal F)
4. The CLI Architect (Proposal A)
5. The Brownfield Team Lead (Proposal G)
6. The Model-Behavior Realist (Proposal C)
7. The Contrarian (Proposal E)
```

### 3. The name

Recommend **`route include`**, paired with **`route exclude`**:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--clear-tags]
  [--clear-apply-to]
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>
  [--dry-run]
  [global flags]
```

A new loader row requires a description. Registering an existing matching handwritten row does not. Omitted fields preserve existing policy, while explicit clearing is visible and deliberate.

The name states the effect and remains distinct from creating files, initializing catalogues, and installing packages. Keep one operand in #6 so each description and loading policy clearly belongs to one source.

The runner-up is **`route attach` / `route detach`**, which reads naturally beside Library commands. Include wins because Attach would suggest the filesystem projection that this feature deliberately avoids.

### 4. Steal list

1. **The CLI Architect (Proposal A): Shared source observation.** Promote the existing linked-source reading mechanics into a common capability so Context, discovery, diagnostics, and measurements cannot develop different inventories.
2. **The Unix Minimalist (Proposal B): Paths-only registration with new schema versions.** Keep loading policy in the loader and preserve the schemas that existing files already reference.
3. **The Model-Behavior Realist (Proposal C): Three-path parity fixtures.** Delete the README, then compare actual reads and authored output across file-only, Context-only, and help-led agents.
4. **The Contract Guardian (Proposal F): Separate intended state from verified effects.** Structured output should distinguish what a preview proposes from what an interrupted apply actually accomplished.
5. **The Brownfield Team Lead (Proposal G): Uncoached adoption and undo.** Ask unfamiliar users to expose an existing document, identify every write, and undo the inclusion without deleting the source.
6. **The Migration And Safety Engineer (Proposal H): Preserve privacy, lock identity, and historical recovery coordinates during relocation.** Moving context must not expose ignored records, permit concurrent writers, or reinterpret old recovery targets.

### 5. Most dangerous idea

The Contract Guardian (Proposal F)’s complete replacement of loading policy on repeated Include is the most dangerous idea.

A description-only change could remove `LoadNow` or erase an existing `src/**` condition. That makes an ordinary maintenance request alter when rules apply. Preserve omitted fields and require explicit clearing, consistent with the existing Route Update convention.

### 6. Convergence

All eight proposals independently choose `route include` and `route exclude`. They also agree that inclusion uses existing content without copying it or acquiring ownership, and that Libraries currently have a narrower containment boundary than “anywhere on this computer.”

All eight retain the loader as the activation authority. Config-only registrations stay off, handwritten loader entries remain usable, and maintenance must not restore a removed row.

All eight keep included content outside automatic Index rewriting in #6. They also move authoring rules into an independent `authoring.md`, make the README an optional guide, and preserve native Skill discovery locations.

Most retain fixed `.agents` control files during root relocation. A and C instead introduce workspace-root settings discovery. Most plan #7 directly, while E makes it conditional on further demand evidence.

### 7. Open conflicts

**Paths-only registration versus duplicated metadata.** B, E, F, and G store paths, while A and C also store loading metadata. Recommend paths only. The loader already supplies the effective description, tags, conditions, and order, so another authored representation adds reconciliation work without improving offline use.

**Fixed control paths versus workspace-root settings discovery.** B, E, F, G, and H retain `.agents/open-forge.json` and its lock file, while A and C introduce a workspace-root settings location. Recommend fixed controls for #7, explicitly describing it as configurable context storage. Removing `.agents` entirely can be a separate change with its own compatibility contract.

**Bundled authoring guidance versus effective local authoring guidance.** Most proposals provide bundled explanations or point toward the installed reference, while C explicitly returns local authoring content and its overwrite companion. Recommend C’s approach for authoring rules, with bundled content used only when no workspace is selected. General concept explanations can remain workspace-independent.

Schema versioning is not an open preference under current rules. New shapes need new version directories unless the maintainer explicitly changes that rule.

### 8. Revised position

I would now publish new settings schemas for both PRs and preserve existing authored schema declarations unless explicitly migrated. Unknown-key tolerance remains a reader compatibility policy, not permission to change published schemas.

I would adopt `context --authoring` and make workspace-aware help return the same effective local authoring reference, including its overwrite companion. The acceptance evidence would compare actual reading and authoring across all three agent paths after README removal.

I would retain a paths-only registry and fixed control locations, while adopting H’s migration requirements for ignored files, privacy records, lock identity, literal grants, and historical recovery paths.

I would also preserve existing References semantics. The [CLI reference](../../../../../docs/cli.md) excludes Entries navigation from authored-reference counts. Inclusion exposure belongs in route inspection unless a separately specified output explicitly distinguishes it.

Finally, I would make handwritten-entry adoption, omission-preserving metadata updates, explicit clearing, and interruption ordering precise before implementation. Registration should precede activation, and exclusion should remove activation before registration.

## The CLI Architect (Proposal A)

### Critiques

**The Unix Minimalist (Proposal B).** Its strongest contribution is a path-only registration list with loading policy kept entirely in the loader. Its default Route List also emits inactive registrations, which mixes configuration inventory with the command’s documented enumeration of exposed roots. Put inactive registrations behind a separate selector or in Status.

**The Model-Behavior Realist (Proposal C).** It gives the clearest answer to reading parity through `context --authoring`, local authoring overrides and a shared three-path evaluation. Its #6 schema change modifies published v2, and its #7 statement that custom roots contain recovery state conflicts with the current external recovery store.

**The CLI Product Designer (Proposal D).** Its concrete help text, operation boundaries and explanation of remaining exposure after Exclude make the feature understandable to newcomers. Keeping both additions at schema version 2 contradicts the repository’s [versioning rule](../../../../../.agents/directives/software/versioned-configuration-schema.md), which requires a new version for a new shape even when readers tolerate unknown keys.

**The Contrarian (Proposal E).** It makes a strong case for read-only inclusion, a small registry and early investigation of competing catalogue ancestry. Its central recommendation makes #7 conditional on new demand evidence, so it does not fully deliver the maintainer’s chosen horizon. It also proposes editing the published v2 schema.

**The Contract Guardian (Proposal F).** It supplies the most complete machine-facing surface, distinguishes settings versions from result versions and reports partial effects explicitly. Its workspace-independent help and conditional authoring reference do not specify how a Context-only agent retrieves the local authoring rules, leaving a central parity requirement unresolved.

**The Brownfield Team Lead (Proposal G).** Its short adoption session and batch examples make the user benefit unusually concrete. However, Include refuses to register an existing handwritten row, while Exclude operates only on registered rows, leaving no command path to bring that existing configuration under registration. Batch support also introduces more policy and validation than the other proposals need initially.

**The Migration And Safety Engineer (Proposal H).** It provides the strongest migration design, especially for ignored files, private navigation, Library projections, lock identity and historical recovery paths. It nevertheless proposes editing published settings-v2 and lock-v1 schemas while adding new layout meaning, so its storage evolution needs correction before implementation.

### Rankings

```text
RANKING CONCEPT:
1. The CLI Product Designer (Proposal D)
2. The Migration And Safety Engineer (Proposal H)
3. The Unix Minimalist (Proposal B)
4. The Contract Guardian (Proposal F)
5. The Model-Behavior Realist (Proposal C)
6. The Contrarian (Proposal E)
7. The Brownfield Team Lead (Proposal G)
RANKING SURFACE:
1. The Contract Guardian (Proposal F)
2. The Unix Minimalist (Proposal B)
3. The Migration And Safety Engineer (Proposal H)
4. The Contrarian (Proposal E)
5. The CLI Product Designer (Proposal D)
6. The Model-Behavior Realist (Proposal C)
7. The Brownfield Team Lead (Proposal G)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Migration And Safety Engineer (Proposal H)
3. The Brownfield Team Lead (Proposal G)
4. The CLI Product Designer (Proposal D)
5. The Contract Guardian (Proposal F)
6. The Unix Minimalist (Proposal B)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Unix Minimalist (Proposal B)
3. The Brownfield Team Lead (Proposal G)
4. The Contract Guardian (Proposal F)
5. The CLI Product Designer (Proposal D)
6. The Model-Behavior Realist (Proposal C)
7. The Contrarian (Proposal E)
```

### The name

Recommend:

```text
open-forge route include <workspace-file>
  --description <text>
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>
  [--dry-run]
  [global flags]
```

Include exposes existing content, Create creates content and Init creates catalogue structure. Exclude removes exposure while preserving files, keeping it distinct from Remove.

Start with one operand. Register an identical handwritten row without rewriting it, and reject conflicting policy rather than quietly turning Include into another metadata editor.

The runner-up is top-level `include` with the same operands. It reads naturally, but putting the operation under Route makes related commands easier to discover.

### Steal list

1. **The Unix Minimalist (Proposal B) — Path-only registrations.** Keeping descriptions, tags, conditions and order exclusively in loader rows removes an unnecessary source of drift.
2. **The Model-Behavior Realist (Proposal C) — Three-path parity evaluation.** The same ordered-read and authoring fixtures should cover plain-file use, Context-only use and concepts-help-led use, including local overrides.
3. **The Contrarian (Proposal E) — Resolve competing ancestry before sharing the reader.** The current linked-entry reader retains the first parent, which can change inherited conditions when overlapping sources are introduced.
4. **The Contract Guardian (Proposal F) — Explicit machine results for previews and partial application.** Scripts should distinguish intended effects, verified effects and unavailable facts without interpreting human wording.
5. **The Brownfield Team Lead (Proposal G) — A short adoption-and-undo exercise.** Asking unfamiliar users to expose existing knowledge, identify every write and undo the inclusion directly tests the feature’s value.
6. **The Migration And Safety Engineer (Proposal H) — Migrate privacy policy with content.** Ignored and untracked notes, owned ignore rules and sharing receipts must move together so relocation cannot expose private navigation.

### Most dangerous idea

Putting recovery state beneath the movable main root, as C proposes, risks making recovery depend on the migration it must help diagnose.

The current [recovery path implementation](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs) stores bundles outside the workspace and keys them by physical workspace identity. Preserve that location and interpret historical manifest targets literally, as H specifies. A new `mainRoot` must not change where an earlier operation wrote.

### Convergence

All eight proposals recommend `route include` and `route exclude`, distinguish inclusion from creation and initialization, and reject “link” as overloaded terminology.

All eight preserve handwritten loader entries, keep the loader authoritative for activation, and prevent Index from rewriting included project content. All distinguish the maintainer’s computer-local Library aspiration from today’s workspace-contained implementation.

All eight move mandatory authoring instructions out of the removable README into a separate shipped reference, without introducing an authoring Directive.

All eight preserve `.agents` as the default and separate native Skill discovery from context storage. Six retain existing control-file locations. Most prefer registrations without duplicated loading metadata and use explicit Configure migration.

There is no agreement on schema evolution, batch operands, Include’s metadata-update behavior, authoring retrieval through the CLI or relocation of control files.

### Open conflicts

1. **Registration-only Include versus policy-updating Include.** Prefer registration-only Include and a path-only `roots` array. Adopt identical existing rows, reject conflicting supplied metadata and keep ordinary loader edits authoritative. This gives the command one small operation without adding patch and clear semantics.

2. **Fixed `.agents` controls versus workspace-root configuration and relocated ownership.** Prefer fixed settings and ownership locations for #7, with the remaining `.agents` directory stated plainly. This delivers configurable knowledge storage without also introducing configuration discovery and control-store migration. Use explicit Configure migration rather than adding Relocate unless its distinct lifecycle proves necessary.

3. **Bundled-only topic help versus local-aware authoring retrieval.** Prefer C’s local-aware design. `context --authoring` should return the installed reference and applicable overwrite, while help should use that same source in an installed workspace. Bundled fallback belongs outside installed workspaces, and a missing local prerequisite should remain an error.

Published-schema immutability is a repository constraint rather than another preference to settle. Unknown-key tolerance and versioned schemas serve different purposes.

### Revised position

I would change my proposal in these ways:

- Replace metadata-bearing registration objects with path strings and narrow Include to registration.
- Retain the existing control-file locations for #7, avoiding an additional bootstrap migration.
- Add explicit local authoring retrieval through Context and adopt C’s three-path acceptance fixtures.
- Keep inactive registrations out of default Route List results, exposing them through `--includes`.
- Preserve the existing References counting rule. C and H correctly retain Entries as navigation, which the [current reference](../../../../../docs/cli.md) excludes from authored-reference counts.
- Make H’s privacy migration, unchanged workspace lease identity and literal historical recovery paths explicit acceptance conditions.
- Publish new schema versions for changed settings and any changed ownership shape, while documenting that version increments cannot prevent older writers from ignoring unfamiliar fields.

## The Contract Guardian (Proposal F)

### Critiques

**The CLI Architect (Proposal A).** Its strongest contribution is the concrete plan to make every command consume shared source observations, rather than extending Context’s isolated reader. Duplicating descriptions, tags and conditions in config creates avoidable drift even though the loader remains authoritative.

**The Unix Minimalist (Proposal B).** Its strongest contribution is a small registration model with loading policy stored only in the loader. Adding inactive registrations to ordinary Route List results blurs the existing distinction between exposed topology and registration inventory.

**The Model-Behavior Realist (Proposal C).** Its strongest contribution is addressing all three reading paths through `context --authoring`, workspace-local authoring content and observed parity fixtures. Placing recovery state inside the custom root changes an established external-storage boundary without explaining how existing recovery history remains accessible.

**The CLI Product Designer (Proposal D).** Its strongest contribution is clear newcomer-facing help that distinguishes Include, Create, Init and Exclude through their effects. Keeping both additions inside published schema v2 conflicts with the repository’s explicit schema-immutability rule.

**The Contrarian (Proposal E).** Its strongest contribution is resisting accidental ownership and keeping included content outside automatic maintenance. Making #7 conditional on further demand conflicts with the maintainer’s selected delivery horizon, even though its conditional implementation plan is useful.

**The Brownfield Team Lead (Proposal G).** Its strongest contribution is a realistic adoption journey, including batching related documents and testing whether newcomers can undo their changes. Its refusal to register existing handwritten rows, combined with Exclude handling only registered entries, introduces a provenance distinction that users cannot infer from otherwise identical loader lines.

**The Migration And Safety Engineer (Proposal H).** Its strongest contribution is the most complete migration design, covering ignored notes, private navigation, stable locks, historical recovery paths and Library projections. Its proposal to extend published settings and lock schemas in place undermines the otherwise careful compatibility story.

The schema issue is substantive. The current [configuration Directive](../../../../../.agents/directives/software/versioned-configuration-schema.md) says, “Once a version ships, its schema document stops changing.” Unknown-key tolerance governs readers, not schema publication.

### Rankings

```text
RANKING CONCEPT:
1. The CLI Product Designer (Proposal D)
2. The Migration And Safety Engineer (Proposal H)
3. The Unix Minimalist (Proposal B)
4. The CLI Architect (Proposal A)
5. The Contrarian (Proposal E)
6. The Brownfield Team Lead (Proposal G)
7. The Model-Behavior Realist (Proposal C)
RANKING SURFACE:
1. The Unix Minimalist (Proposal B)
2. The CLI Architect (Proposal A)
3. The Migration And Safety Engineer (Proposal H)
4. The Brownfield Team Lead (Proposal G)
5. The CLI Product Designer (Proposal D)
6. The Contrarian (Proposal E)
7. The Model-Behavior Realist (Proposal C)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Migration And Safety Engineer (Proposal H)
3. The Brownfield Team Lead (Proposal G)
4. The Unix Minimalist (Proposal B)
5. The CLI Product Designer (Proposal D)
6. The CLI Architect (Proposal A)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Unix Minimalist (Proposal B)
3. The CLI Product Designer (Proposal D)
4. The Brownfield Team Lead (Proposal G)
5. The CLI Architect (Proposal A)
6. The Contrarian (Proposal E)
7. The Model-Behavior Realist (Proposal C)
```

### The name

Recommend:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]
```

Require a description when creating a loader row. Registering an existing matching row preserves its metadata, and conflicting supplied metadata blocks with a precise edit instruction. Keep `route exclude` as the inverse that preserves source files.

Use **include** as the noun. It is easier to guess than “inclusion” and describes using existing content without implying copying or ownership. The distinction from Create and Init is observable before execution.

The runner-up is `route add <workspace-file> [options]`. It is familiar, but “add” remains ambiguous beside commands that create files and routing structure.

### Steal list

- **The CLI Architect (Proposal A) — Shared source observations.** Context, discovery, navigation and diagnostics should consume the same observations so registration cannot create inconsistent command behavior.
- **The Unix Minimalist (Proposal B) — Path-only config.** Recording paths without duplicating loading metadata gives each editable fact one clear home.
- **The Model-Behavior Realist (Proposal C) — Explicit authoring retrieval.** `context --authoring` should return the installed authoring reference and its overwrite companion.
- **The Model-Behavior Realist (Proposal C) — Three-path behavior fixtures.** Compare actual reads and authored results across plain-file, Context-only and help-led agents after removing the README.
- **The Migration And Safety Engineer (Proposal H) — Privacy-aware migration.** Inventory ignored and untracked content directly, then migrate sharing records and owned ignore rules together.
- **The Migration And Safety Engineer (Proposal H) — Stable recovery coordinates.** Keep locks keyed to physical workspace identity and preserve historical recovery targets as their original workspace-relative paths.

### Most dangerous idea

Putting recovery state inside the relocated content root, as C proposes, would make the migration affect the mechanism needed to recover from its own failure.

The current [mutation design](../../../../../.agents/memory/crystallized/documents/cli/technical-designs/mutation-and-recovery.md) deliberately uses an external application-data store, with lock and recovery keys derived from physical workspace identity. Preserve that boundary in #7. Moving context should not also relocate recovery storage or reinterpret old manifests.

### Convergence

All eight proposals choose `route include` and `route exclude`. They distinguish using existing content from creating files, initializing catalogues and deleting content.

They also agree that loader membership controls activation, config alone cannot restore a removed loader row, handwritten entries remain supported, and inclusion grants no ownership of the target.

All eight move authoring instructions into a separate local reference before making the README removable. They retain native Skill locations and reject the idea that arbitrary context directories provide harness discovery.

Most favor path-only registration and fixed `.agents` control locations. Most place explicit migration under Configure. Every proposal treats root support as structural work rather than a constant replacement, although E challenges its priority.

There is less convergence on repeated Include updates, published schema versions and how authoring content reaches an agent that uses only Context.

### Open conflicts

1. **Path-only registry versus metadata-bearing records.** B, D, E and G favor strings, while A and C duplicate loading metadata and H wraps paths in objects. Recommend path strings with description, tags, conditions and effective ordering only in the loader. Registration should not preserve a second policy that must later be reconciled.

2. **Fixed controls versus workspace-root settings discovery.** B, D, E, G and H retain fixed controls, while A and C introduce workspace-root settings with legacy-location handling. Recommend fixed `.agents/open-forge.json` and lock locations for #7. State plainly that configurable content storage does not eliminate the small control directory.

3. **Bundled explanations versus workspace-local authoring authority.** Several proposals expose bundled help while directing file readers to installed authoring instructions. Recommend C’s explicit Context retrieval and local authoring resolution when a workspace is selected, with bundled material used outside installed workspaces. A missing required local reference must be reported, not silently replaced by bundled defaults.

### Revised position

I would adopt **include** as the noun and require descriptions only for new rows. Registering an existing handwritten row should preserve its policy, with conflicting changes requiring an explicit edit rather than clearing omitted fields.

I would add C’s `context --authoring` and its three-path acceptance fixtures. Concepts help would distinguish bundled explanation from installed authoring authority and direct workspace authors to the actual local files.

I would incorporate H’s migration requirements for ignored content, privacy rules, physical-workspace lock identity, historical recovery paths and Library links. Recovery storage would remain external.

I would use reviewed `install --configure --main-root` migration rather than require a new top-level Relocate command. Its contract must enumerate movement, reference rewriting, ownership publication and interruption behavior before implementation.

Finally, I would add the payload-maintenance contracts identified by A and B, publish new schema versions, and make the first connected migration fixture include private notes, an Extension, a Library, overwrites and a disabled loader branch.

## The Brownfield Team Lead (Proposal G)

### Critiques

**The CLI Architect (Proposal A).** Its strongest contribution is the concrete plan to give every reader one shared source model, including the necessary navigation changes. Its config unnecessarily duplicates descriptions, tags, and conditions that the loader owns, creating avoidable drift and reconciliation work.

**The Unix Minimalist (Proposal B).** It offers the cleanest combination of path-only registration, explicit handwritten-entry adoption, immutable schema versions, and stable bootstrap locations. Its main gap is authoring parity, because Context-only agents have no specified way to obtain the required authoring reference through Context.

**The Model-Behavior Realist (Proposal C).** It most directly addresses the three agent experiences through `context --authoring`, workspace-local authoring content, and shared behavioral fixtures. Its serious flaw is extending published schema v2 in place, which contradicts the repository’s explicit schema policy.

**The CLI Product Designer (Proposal D).** Its worked help text makes the distinction between including a file, creating content, and initializing catalogues especially clear. Its References proposal leaves navigation-edge reporting insufficiently distinguished from authored-reference counts, and its in-place v2 schema changes violate the published-schema policy.

**The Contrarian (Proposal E).** It usefully challenges implementation cost and preserves separately labelled Framework-only and effective startup measurements. Its main weakness is making the planned #7 conditional on fresh demand, which changes the maintainer’s selected horizon, while also proposing an incompatible schema-maintenance practice.

**The Contract Guardian (Proposal F).** It gives unusually precise result contracts and separates relocation into a coherent operation with explicit effects. Its dangerous choice is making omitted tags and conditions empty on repeated Include, which can unexpectedly remove required loading or broaden applicability.

**The Migration And Safety Engineer (Proposal H).** It provides the strongest migration analysis, particularly for ignored notes, private navigation, workspace lock identity, historical recovery paths, and Library projections. Its principal flaw is proposing edits to published settings and lock schemas without establishing new schema versions.

### Rankings

```text
RANKING CONCEPT:
1. The CLI Product Designer (Proposal D)
2. The Contract Guardian (Proposal F)
3. The Unix Minimalist (Proposal B)
4. The Migration And Safety Engineer (Proposal H)
5. The CLI Architect (Proposal A)
6. The Model-Behavior Realist (Proposal C)
7. The Contrarian (Proposal E)
RANKING SURFACE:
1. The Unix Minimalist (Proposal B)
2. The Contract Guardian (Proposal F)
3. The CLI Architect (Proposal A)
4. The Contrarian (Proposal E)
5. The Migration And Safety Engineer (Proposal H)
6. The CLI Product Designer (Proposal D)
7. The Model-Behavior Realist (Proposal C)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Migration And Safety Engineer (Proposal H)
3. The CLI Product Designer (Proposal D)
4. The Unix Minimalist (Proposal B)
5. The CLI Architect (Proposal A)
6. The Contract Guardian (Proposal F)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Unix Minimalist (Proposal B)
3. The Contract Guardian (Proposal F)
4. The CLI Product Designer (Proposal D)
5. The CLI Architect (Proposal A)
6. The Contrarian (Proposal E)
7. The Model-Behavior Realist (Proposal C)
```

### The name

Recommend **`route include`**, paired with **`route exclude`**:

```text
open-forge route include <workspace-file>
  --description <text>
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>
  [--dry-run]
  [global flags]
```

“Include” describes exposing existing content. “Exclude” clearly reverses exposure while preserving files. Neither overlaps with Create’s content creation or Init’s catalogue creation.

Start with one file per request and require a useful description for a new row. Preserve existing policy when registering a matching handwritten entry.

The runner-up is `route add <workspace-file>`, but its ambiguity beside `route create` makes it weaker. The concept rankings are close because all seven reviewed proposals reach essentially the same naming conclusion.

### Steal list

- **The CLI Architect (Proposal A):** Build one shared inventory and exposure graph for all readers, because inconsistent discovery would undermine otherwise correct loader semantics.
- **The Unix Minimalist (Proposal B):** Store only registered paths in config, because loading policy already has an authoritative home in the loader.
- **The Model-Behavior Realist (Proposal C):** Compare actual ordered reads and authored results across loader-only, Context-only, and help-led sessions, because textual consistency alone cannot prove behavioral parity.
- **The CLI Product Designer (Proposal D):** Report remaining exposure after Exclude, because removing one direct entry does not necessarily make a document unreachable through another catalogue.
- **The Migration And Safety Engineer (Proposal H):** Migrate ignored files and privacy rules together, because preserving bytes while exposing private filenames would still be a serious regression.
- **The Migration And Safety Engineer (Proposal H):** Preserve workspace lock identity and interpret historical recovery paths literally, because changing content storage must not split concurrency control or reinterpret old recovery evidence.

### Most dangerous idea

The Contract Guardian (Proposal F)’s replacement semantics for repeated Include are the most dangerous operational choice.

It states that omitted tags and conditions become empty lists. Repeating a contribution-guide inclusion while changing its description could remove `LoadNow`. Repeating a conditioned `LoadNow` inclusion without its condition could make specialized rules apply throughout the workspace.

Preserve unspecified fields. Require explicit clearing or a clearly identified replacement operation, with the before-and-after loading policy visible in the preview.

### Convergence

All eight proposals recommend `route include` and `route exclude`, distinguish reuse from authority, and preserve existing Library containment.

All eight retain the loader as the activation authority. A config-only registration stays off, handwritten loader entries remain valid, and maintenance must not resurrect deleted rows.

All eight keep included documents independently maintained. Ordinary files remain leaves, recognized catalogues expose Entries, foreign activation metadata stays uninterpreted, and directories do not imply recursive adoption.

All eight move authoring instructions into an independent `authoring.md` reference, make the README removable, and avoid a shipped authoring Directive.

All eight preserve default-root behavior, require explicit relocation, retain harness-native Skill locations, and reject mechanical replacement of every `.agents` occurrence.

Six retain the existing `.agents` control locations. A and C instead introduce workspace-root settings discovery for custom layouts.

### Open conflicts

1. **Path-only registry versus metadata snapshots.** B, D, E, and F store paths without duplicated loading policy, while A and C retain metadata snapshots. Recommend path-only registration, with descriptions, tags, conditions, and ordering defined exclusively by loader rows.

2. **Fixed bootstrap controls versus relocated settings.** B, D, E, F, and H retain `.agents` controls, while A and C introduce a second settings location. Recommend fixed controls for #7 and state plainly that a custom context root does not eliminate `.agents` entirely.

3. **Workspace authoring reference versus bundled explanation alone.** C explicitly makes Context and concept help return the installed authoring reference, while most others primarily describe a bundled topic and a separate conditional file read. Recommend C’s explicit retrieval approach, with local overrides respected and missing required local files reported.

Schema immutability is already settled by repository policy, rather than an open preference. It says, “Once a version ships, its schema document stops changing.” Optional additions and tolerant older readers do not override that requirement. [Schema Directive](../../../../../.agents/directives/software/versioned-configuration-schema.md)

### Revised position

I would simplify my initial surface to one file per Include request, require a meaningful description for new entries, and postpone batching until its metadata semantics are demonstrated.

I would allow an identical handwritten row to become registered without rewriting it. Conflicting metadata would require explicit resolution, and unspecified fields would remain unchanged.

I would add C’s `context --authoring` path and require help-led authoring to use the installed reference when available. Merely linking to `authoring.md` does not satisfy Context-only parity, since the current CLI follows loading tags rather than prose-required reads.

I would adopt H’s migration requirements for ignored and untracked files, privacy-preserving navigation, unchanged lock identity, literal historical recovery paths, and affected Library links.

I would retain path-only registration, new schema versions, fixed control locations, native Skill placement, and explicit migration. I would also preserve References’ existing exclusion of Entries from authored-reference counts.

## The Unix Minimalist (Proposal B)

### Critiques

**The CLI Architect (Proposal A).** Its strongest contribution is the concrete plan to move linked-source reading into a shared Framework graph consumed by every command. Its main weakness is duplicating descriptions, tags and conditions in config as “last-requested metadata”, creating drift that a path-only registry avoids.

**The Model-Behavior Realist (Proposal C).** It gives the strongest treatment of reading parity, including local authoring content, `context --authoring` and observed tests across three agent paths. Its proposed in-place v2 schema extension conflicts with the repository’s explicit schema immutability rule.

**The CLI Product Designer (Proposal D).** Its newcomer-facing command explanation is particularly clear, and its path-only config keeps loading policy in one place. Its most serious flaw is treating optional additions as justification for modifying published v2, confusing tolerant reading with schema publication policy.

**The Contrarian (Proposal E).** Separating Framework-only measurements from effective startup measurements is a useful compatibility safeguard. Its decision to condition #7 on further demand reopens the maintainer’s stated plan, and its v2 schema change also needs correction.

**The Contract Guardian (Proposal F).** It provides the strongest command-level contract detail, including structured results, exact-path interpretation and a separate relocation operation. Repeating Include replaces the complete loading policy, so omitting previously supplied tags or conditions can unintentionally change required reading.

**The Brownfield Team Lead (Proposal G).** Its practical adoption session and all-or-nothing batches make the feature’s value and written effects concrete. However, Include leaves an existing handwritten entry unregistered, so an explicit inclusion request can fail to deliver the requested config tracking.

**The Migration And Safety Engineer (Proposal H).** It has the strongest migration design, covering ignored notes, private navigation, workspace lock identity, historical recovery paths and Library projections. Its documentation plan nevertheless changes published settings and lock schemas in place, including while adding ownership layout provenance.

### Rankings

```text
RANKING CONCEPT:
1. The CLI Product Designer (Proposal D)
2. The Migration And Safety Engineer (Proposal H)
3. The CLI Architect (Proposal A)
4. The Brownfield Team Lead (Proposal G)
5. The Contrarian (Proposal E)
6. The Contract Guardian (Proposal F)
7. The Model-Behavior Realist (Proposal C)
RANKING SURFACE:
1. The Contract Guardian (Proposal F)
2. The Brownfield Team Lead (Proposal G)
3. The CLI Architect (Proposal A)
4. The Contrarian (Proposal E)
5. The CLI Product Designer (Proposal D)
6. The Migration And Safety Engineer (Proposal H)
7. The Model-Behavior Realist (Proposal C)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Migration And Safety Engineer (Proposal H)
3. The Brownfield Team Lead (Proposal G)
4. The CLI Product Designer (Proposal D)
5. The Contract Guardian (Proposal F)
6. The CLI Architect (Proposal A)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Contract Guardian (Proposal F)
3. The CLI Architect (Proposal A)
4. The CLI Product Designer (Proposal D)
5. The Brownfield Team Lead (Proposal G)
6. The Model-Behavior Realist (Proposal C)
7. The Contrarian (Proposal E)
```

### The name

Recommend:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]
```

Require a description for a new row. Adopt an existing handwritten row without duplicating it or changing unspecified policy. Use `route exclude <workspace-file> [--dry-run] [global flags]` as the non-destructive inverse.

Include describes using existing content and remains distinct from Create and Init. The runner-up is `route attach <workspace-file>`, which fits the reuse family but risks suggesting Library-style filesystem projection.

### Steal list

- **The CLI Architect (Proposal A):** Promote linked-source reading into the shared graph so Context, discovery and diagnostics cannot develop different interpretations.
- **The Model-Behavior Realist (Proposal C):** Require the same ordered-read and authoring fixtures across loader-only, Context-only and concepts-led use.
- **The Contrarian (Proposal E):** Retain separately labelled Framework-only and effective startup measurements so expanded coverage does not silently redefine existing figures.
- **The Contract Guardian (Proposal F):** Give relocation its own operation because moving an installed knowledge tree has different effects and recovery obligations from changing setup preferences.
- **The Brownfield Team Lead (Proposal G):** Make adoption an observed end-to-end exercise that checks exactly what was written and proves exclusion leaves existing documents untouched.
- **The Migration And Safety Engineer (Proposal H):** Preserve workspace-based lock and recovery identities while testing ignored files, privacy rules and shareable Entries through migration.

### Most dangerous idea

Treating unknown-key tolerance as permission to modify published schemas would weaken a stable compatibility promise.

C, D, E and H propose extending v2 in place, and H also proposes changing the published lock schema. The repository’s [configuration rule](../../../../../.agents/directives/software/versioned-configuration-schema.md) explicitly says, “Once a version ships, its schema document stops changing.” Publish new schema versions while preserving forgiving readers. Neither approach prevents an older executable from mishandling a custom-root workspace.

### Convergence

All eight proposals independently choose `route include` and `route exclude`. They distinguish package distribution, local file projection and reuse in place, while acknowledging today’s narrower Library containment boundary.

They also agree that loader Entries control activation, config must not resurrect deleted rows, handwritten links remain valid, and inclusion grants no ownership or automatic editing permission. Every proposal keeps included content outside Index’s automatic authoring changes.

All move authoring instructions into a separate shipped reference, reject a shipped authoring Directive and make the README an optional guide. They preserve `.agents` as the default and keep Skills at harness-native locations.

Most favor registration without duplicated loading metadata and fixed control-file locations. Schema publication, bootstrap relocation and delivery of local authoring rules remain unresolved.

### Open conflicts

**Fixed control paths versus workspace-root configuration.** A and C introduce a workspace-root settings anchor, while most others retain `.agents` controls. Recommend fixed controls for #7, with an explicit statement that configurable knowledge storage does not eliminate `.agents` entirely.

**Configure relocation versus a dedicated Relocate operation.** Most proposals put migration under `install --configure`, while F separates it. Recommend `relocate <directory>` for existing installations, leaving the installation option responsible for fresh layout selection. Migration moves user content and changes references, receipts and privacy paths, which warrants a distinct operation contract.

**Bundled guidance versus workspace-local authoring truth.** Several proposals embed defaults or point to authoring instructions, while C explicitly serves the local reference. Recommend local authoring content and its overwrite when a workspace is selected, bundled content outside a workspace, and an explicit `context --authoring` projection. Verify actual file reads, since shared source text alone does not prove agent parity.

### Revised position

I would add C’s explicit authoring projection and local-source selection to the guide design, then require its three-path parity fixture.

I would adopt A’s shared graph implementation boundary and E’s separately labelled startup measurements.

For #7, I would use F’s dedicated relocation operation and incorporate H’s detailed requirements for ignored and untracked content, private navigation, unchanged lock identity, historical recovery coordinates and interrupted publication. A settings/layout disagreement would block writes until an explicit resolution plan succeeds.

## The Model-Behavior Realist (Proposal C)

### Critiques

**The CLI Architect (Proposal A):** Its strongest contribution is a source-backed implementation path that makes reading commands share one graph while retaining explicit write boundaries. Its config nevertheless duplicates descriptions, tags, and conditions that remain authoritative in the loader, creating avoidable synchronization work. Its concepts topic also points toward authoring instructions without clearly supplying the complete grammar itself.

**The Unix Minimalist (Proposal B):** The path-only registry gives the clearest separation between registration and activation, and preserving handwritten policy during adoption makes Include predictable. Its main gap is authoring access: it does not establish how a Context-only agent retrieves the separate reference, or how Concepts alone provides complete authoring instructions.

**The CLI Product Designer (Proposal D):** Its command explanation, examples, and error messages make the distinction between Include, Create, and Init especially approachable. Keeping schema version 2 conflicts with the repository’s explicit rule that published schemas stop changing, regardless of tolerant readers. It also adds navigation edges to References without identifying that change to the current authored-reference contract.

**The Contrarian (Proposal E):** Its strongest contribution is disciplined protection of existing documentation, including registration maintenance during proven rename repairs. Making #7 conditional on further demand changes the maintainer’s stated delivery plan rather than simply designing it safely. Its proposed v2 schema extension also needs correction.

**The Contract Guardian (Proposal F):** It supplies the most concrete structured-result design, including unavailable facts, partial effects, and the distinction between settings versions and result-envelope versions. Its most serious flaw is treating repeated Include as complete policy replacement, with omitted tags and conditions becoming empty lists. A request intended to update a description could therefore remove required loading or broaden applicability.

**The Brownfield Team Lead (Proposal G):** Its concrete adoption session and atomic batch behavior make the intended benefit easy to understand and verify. However, Include leaves an existing handwritten row unregistered while Exclude operates only on registered entries, so a successful Include request does not establish a reliably reversible relationship.

**The Migration And Safety Engineer (Proposal H):** Its migration design is strongest on actual preservation requirements, particularly ignored notes, private navigation, historical recovery paths, and unchanged workspace lock identity. It still proposes modifying published v2 schemas, and its documentation inventory names some new contract sets without enumerating their exact files. Those gaps need correction before its otherwise strong migration plan becomes executable.

The schema objection follows the repository’s [versioned configuration Directive](../../../../../.agents/directives/software/versioned-configuration-schema.md), which explicitly requires a new version for a new shape.

### Rankings

```text
RANKING CONCEPT:
1. The Unix Minimalist (Proposal B)
2. The CLI Product Designer (Proposal D)
3. The Migration And Safety Engineer (Proposal H)
4. The Contract Guardian (Proposal F)
5. The CLI Architect (Proposal A)
6. The Contrarian (Proposal E)
7. The Brownfield Team Lead (Proposal G)
RANKING SURFACE:
1. The Unix Minimalist (Proposal B)
2. The CLI Architect (Proposal A)
3. The Migration And Safety Engineer (Proposal H)
4. The Contrarian (Proposal E)
5. The CLI Product Designer (Proposal D)
6. The Contract Guardian (Proposal F)
7. The Brownfield Team Lead (Proposal G)
RANKING GUIDE:
1. The Migration And Safety Engineer (Proposal H)
2. The Brownfield Team Lead (Proposal G)
3. The Unix Minimalist (Proposal B)
4. The Contract Guardian (Proposal F)
5. The CLI Product Designer (Proposal D)
6. The CLI Architect (Proposal A)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Unix Minimalist (Proposal B)
3. The Contract Guardian (Proposal F)
4. The CLI Architect (Proposal A)
5. The CLI Product Designer (Proposal D)
6. The Brownfield Team Lead (Proposal G)
7. The Contrarian (Proposal E)
```

### The name

Recommend **`route include`**, with **`route exclude`** as its inverse:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]

open-forge route exclude <workspace-file>
  [--dry-run]
  [global flags]
```

Require a description for a new row. Register an existing matching handwritten row without changing its policy, and block conflicting metadata. Start with one target per request so metadata assignment remains unambiguous.

Include describes using existing content and fits beside Create and Init without changing either operation. Exclude describes removing exposure while preserving files.

The runner-up is **`route add <workspace-file>`**, but “add” does not tell newcomers whether the command creates content or exposes it.

### Steal list

- **The Unix Minimalist (Proposal B):** Store only canonical paths in `roots`, leaving every loading-policy field in the loader and eliminating metadata drift.
- **The CLI Architect (Proposal A):** Make reading commands consume one shared source graph with a separate write policy, preventing consistent discovery from accidentally granting maintenance rights.
- **The Migration And Safety Engineer (Proposal H):** Migrate ignored and untracked files together with sharing records and owned ignore rules, because preserving file bytes alone does not preserve privacy.
- **The Migration And Safety Engineer (Proposal H):** Keep workspace lock identity unchanged and historical recovery paths literal, so relocation neither creates a second concurrency boundary nor changes old recovery evidence.
- **The Brownfield Team Lead (Proposal G):** Render a dedicated authoring topic from the same authored source as the installed reference, making complete format instructions discoverable without another maintained copy.
- **The Contract Guardian (Proposal F):** Define structured results for unavailable facts and partial effects explicitly, so scripts receive the same honest outcome as people reading text.

### Most dangerous idea

The Contract Guardian (Proposal F)’s complete-policy replacement on repeated Include is the most dangerous choice.

Optional flags normally invite omission when users are changing something else. Re-registering a required rule with an updated description could erase `LoadNow`, while supplying `LoadNow` without the previous condition could broaden required reading. A preview helps detection, but preserving omitted policy is the safer command contract. Clearing a field should require an explicit action.

### Convergence

All eight proposals choose Include and Exclude under `route`, preserve loader authority, leave included documents independently maintained, move authoring instructions out of the removable README, and retain native Skill discovery locations.

They consistently distinguish the reuse mechanisms by delivery rather than authority. They also acknowledge that today’s Library containment is narrower than arbitrary sharing across one computer.

Most agree that external catalogues remain authored and read-only to Index in #6. Config-only registrations stay inactive, and maintenance must not resurrect deleted loader rows.

Five proposals use string-only registration arrays. Six retain settings and ownership controls under `.agents`. Seven use explicit Configure-based migration rather than a new relocation command.

Most propose workspace-independent concepts help. The unresolved detail is whether it contains the complete authoring specification or merely points elsewhere.

Most treat #7 as planned follow-up work. The Contrarian (Proposal E) instead recommends requiring further demand before funding it.

### Open conflicts

1. **Path registry or policy mirror.** Choose B’s path-only `roots` or A’s records containing description, tags, and conditions. Recommend the path registry, because the loader already owns those fields and registration needs no second policy representation.

2. **Fixed controls or relocated controls.** Choose H’s stable `.agents` control locations or A’s workspace-level settings anchor with relocated ownership. Recommend fixed controls for #7, explicitly describing the feature as configurable content storage. Moving controls adds bootstrap and recovery work without being necessary to relocate knowledge or preserve native Skills.

3. **Complete Concepts or another required help step.** Choose H’s embedded authoring specification or G’s separate `help authoring` topic. Recommend complete authoring instructions inside Concepts, sourced from the same authored reference. A separate topic can remain a convenience, but Concepts must not require readers to guess that essential syntax is elsewhere. Workspace-specific instructions and overrides still require the current loader and local reference.

### Revised position

I would adopt B’s path-only registry and H’s fixed-control layout. I would publish new schema versions for both PRs rather than extend published v2, while retaining tolerant readers and explicitly documenting unsupported older writers.

I would make the authoring reference explicitly selectable through Context’s normal source operand, keeping it outside automatic startup, instead of adding a dedicated `--authoring` flag. Concepts would embed the complete shipped reference and direct workspace authors to its local version and overrides.

I would add H’s privacy, lock-identity, historical-recovery, and Library-projection requirements to the migration contract. I would retain the shared ordered-read and authoring fixtures across all three agent paths, including a run with the README deleted.

I would keep #7 in the planned sequence and use `--main-root` consistently in installation and migration syntax.

## The Migration And Safety Engineer (Proposal H)

### Critiques

**The CLI Architect (Proposal A).** Its strongest contribution is the concrete plan for one shared source graph across reading, discovery and diagnostics, with an explicit boundary preventing Index from writing included content. Its main weakness is storing descriptions, tags and conditions in config while declaring the loader authoritative for those same values, creating avoidable drift.

**The Unix Minimalist (Proposal B).** It offers the cleanest registration model, preserves published schemas and carefully separates package coordinates from installed destinations. Its main gap is authoring consistency: moving instructions into a local file does not establish how a Context-only agent or bundled Concepts reader receives that same installed content.

**The Model-Behavior Realist (Proposal C).** Its strongest contribution is the explicit authoring retrieval path and the proposed comparison of loader-only, Context-only and help-led behavior. Its root design unnecessarily places recovery state inside the configurable content root, departing from the existing external store and adding migration risk without explaining the benefit.

**The CLI Product Designer (Proposal D).** Its example help, corrective errors and distinction between inclusion, creation and initialization are especially clear for newcomers. Its compatibility reasoning is flawed: unknown-key tolerance does not justify changing a published schema, and reporting Entries edges through References changes an existing contract.

**The Contrarian (Proposal E).** It draws a strong boundary around borrowed content and identifies the concrete danger of assigning shared sources to their first discovered parent. Its main weakness is reopening whether #7 should happen, when the maintainer has already requested that later PR, rather than concentrating on a complete deliverable within that direction.

**The Contract Guardian (Proposal F).** It gives the most precise structured-result contract, including unavailable facts, partial effects and separation of settings versions from output versions. Its most serious flaw is making repeated Include replace the complete loading policy, so omitted conditions can unexpectedly broaden a previously scoped required read.

**The Brownfield Team Lead (Proposal G).** Its strongest contribution is a realistic adoption session that makes the written files and reversal behavior inspectable, with batching serving a concrete user journey. Its refusal to register an existing handwritten row creates a surprising asymmetry: Include can report it unchanged, yet Exclude cannot undo that exposure.

### Rankings

```text
RANKING CONCEPT:
1. The CLI Product Designer (Proposal D)
2. The Unix Minimalist (Proposal B)
3. The CLI Architect (Proposal A)
4. The Contract Guardian (Proposal F)
5. The Contrarian (Proposal E)
6. The Model-Behavior Realist (Proposal C)
7. The Brownfield Team Lead (Proposal G)
RANKING SURFACE:
1. The Unix Minimalist (Proposal B)
2. The Contract Guardian (Proposal F)
3. The Brownfield Team Lead (Proposal G)
4. The Contrarian (Proposal E)
5. The CLI Product Designer (Proposal D)
6. The CLI Architect (Proposal A)
7. The Model-Behavior Realist (Proposal C)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Brownfield Team Lead (Proposal G)
3. The Unix Minimalist (Proposal B)
4. The CLI Architect (Proposal A)
5. The Contract Guardian (Proposal F)
6. The CLI Product Designer (Proposal D)
7. The Contrarian (Proposal E)
RANKING ROOT:
1. The Unix Minimalist (Proposal B)
2. The Brownfield Team Lead (Proposal G)
3. The CLI Product Designer (Proposal D)
4. The Contract Guardian (Proposal F)
5. The CLI Architect (Proposal A)
6. The Contrarian (Proposal E)
7. The Model-Behavior Realist (Proposal C)
```

### The name

Recommend:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]
```

Require a description when creating a new loader row. Preserve existing metadata when registering an identical handwritten row, and block conflicting metadata rather than turning registration into another update operation. Use `route exclude <workspace-file>` as the non-deleting inverse.

Include states the effect plainly and fits beside Create and Init without changing their subjects. The runner-up is `route add <workspace-file>`, but “add” leaves creation versus exposure unclear.

### Steal list

1. **The CLI Architect (Proposal A): one shared source graph.** Context, Find, References, Status and Doctor should observe the same exposure edges, with write eligibility carried separately.
2. **The Unix Minimalist (Proposal B): path-only registration and new schema versions.** This avoids duplicated loading policy while respecting the repository’s published-schema contract.
3. **The Model-Behavior Realist (Proposal C): three-path behavioral verification.** Compare actual reads and authored results through loader-only, Context-only and help-led use after deleting the optional README.
4. **The Contract Guardian (Proposal F): explicit structured outcomes.** Nullable unavailable facts and separately reported partial effects make automation safer without inventing another result system.
5. **The Brownfield Team Lead (Proposal G): the complete adoption session.** Verify that unfamiliar users can expose existing knowledge, identify every written file and undo exposure without touching the sources.
6. **The Migration And Safety Engineer (Proposal H): preserve workspace identity through relocation.** Keeping lock and recovery identity independent of the content root protects existing operations and recovery history.

### Most dangerous idea

The Contract Guardian (Proposal F)’s complete-policy replacement on repeated Include could broaden loading through omission.

Suppose an existing rule has `LoadNow` and `applyTo: src/**`. Repeating Include with its description and `--tag LoadNow`, but omitting `--apply-to`, would make it required for every task. A preview makes that visible, but registration should not require users to reconstruct the complete existing policy to preserve it.

Adopt an identical row unchanged. Handle policy changes through deliberate loader editing or a separately specified update operation.

### Convergence

All eight proposals independently choose `route include` and a non-deleting `route exclude`, preserve loader authority for activation, and reject interpreting registration as ownership. They also preserve handwritten loader entries and keep included content outside Index’s automatic authoring boundary.

All eight move required authoring instructions out of the removable README, provide a plain-file authoring reference and avoid a shipped authoring Directive.

All eight distinguish native Skill discovery from catalogue exposure, retain `.agents` as the default and reject blind replacement of every `.agents` occurrence. They recognize that current Library containment is narrower than unrestricted reuse anywhere on the computer.

Most retain fixed control-file locations. Most use Configure for migration, while F proposes Relocate. Schema versioning, metadata storage and authoring retrieval remain substantive disagreements.

### Open conflicts

1. **Path-only registry versus metadata-bearing registrations.** B, D, E, F and G store paths, while A and C also store loading metadata. Recommend path strings with loading policy exclusively in the loader, and publish new schemas as B, F and G propose. The [configuration Directive](../../../../../.agents/directives/software/versioned-configuration-schema.md) explicitly says, “Once a version ships, its schema document stops changing.” Tolerant readers and immutable published schemas solve different problems.

2. **Installed authoring truth versus bundled explanation.** C explicitly retrieves installed authoring content, while most proposals leave help bundled and authoring as a prose prerequisite. Recommend C’s explicit Context authoring path, with help rendering the same installed reference when a workspace is selected and bundled content otherwise. Test customized references and overwrite companions, rather than proving only that shipped copies initially match.

3. **Fixed controls versus relocated bootstrap configuration.** B, D, E, F and G retain `.agents` controls, while A and C introduce workspace-level configuration discovery. Recommend fixed controls for #7 and explicit disclosure that moving knowledge does not eliminate `.agents`. Keep migration under `install --configure --main-root`, avoiding an additional top-level operation unless its distinct lifecycle earns one.

### Revised position

I would correct my schema recommendation. Publish v3 for #6 and v4 for #7 instead of extending published v2. Unknown-version tolerance remains necessary, but it neither permits changing an immutable schema nor protects custom-root workspaces from older writers.

I would replace single-field registration objects with path strings, following B. Include would register matching handwritten rows without rewriting them, and Exclude would work for direct handwritten exposure as well as registered exposure.

I would add C’s explicit authoring retrieval and behavioral fixture, including a customized local reference and overwrite companion. The acceptance condition should be matching effective rules and valid authored files across the three reading paths.

I would retain fixed control paths, native Skills and workspace-keyed recovery storage. I would also make A’s shared source graph an early implementation boundary and use G’s newcomer adoption session alongside mechanical verification.

## The Contrarian (Proposal E)

### Critiques

**The CLI Architect (Proposal A).** Its strongest contribution is a concrete plan to move linked-source observation into shared Framework code so discovery, loading and diagnosis use one graph. Its config duplicates descriptions, tags and conditions while declaring the loader authoritative, creating avoidable drift and reconciliation work.

**The Unix Minimalist (Proposal B).** Its path-only registry, immutable schema versions and explicit separation of content, controls and native discovery make the state model clear. Its default Route List mixes inactive registrations with navigable sources, weakening the existing topology-report boundary when a separate projection would preserve it.

**The Model-Behavior Realist (Proposal C).** It addresses reading parity most directly through `context --authoring`, workspace-local authoring text and shared behavioral fixtures. It nevertheless duplicates loading policy in config and proposes modifying the published v2 schema. Its root design also places recovery state under the custom content root, unnecessarily expanding migration beyond the current external, workspace-keyed recovery store. [Recovery storage](../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Recovery/Shared/Storage/RecoveryBundlePathIdentity.cs)

**The CLI Product Designer (Proposal D).** Its newcomer-facing explanation and proposed help text make Include, Create, Init and Exclude easy to distinguish. Its most serious flaw is retaining schema version 2 for both new shapes and editing that published schema, contrary to the repository’s explicit versioning rule.

**The Contract Guardian (Proposal F).** Its typed results, distinction between settings and result versions, and dedicated Relocate operation give callers unusually clear contracts. However, repeating Include replaces the complete loading policy, so omitted tags or conditions can erase an existing required read or scope restriction.

**The Brownfield Team Lead (Proposal G).** Its concrete adoption session shows how the feature helps an existing repository, and batch operands can reduce repetitive commands. Refusing to register an existing handwritten row means Include cannot complete the same registration request once that row already exists, introducing a provenance distinction users must learn.

**The Migration And Safety Engineer (Proposal H).** It supplies the strongest root-migration design, especially for ignored notes, privacy rules, physical workspace identity, recovery history and Library projections. It correctly treats migration as more than moving Markdown files. Its schema plan still changes published settings and lock schemas without new versions, which conflicts with the repository’s rule that “Once a version ships, its schema document stops changing.” [Versioning directive](../../../../../.agents/directives/software/versioned-configuration-schema.md)

### Rankings

```text
RANKING CONCEPT:
1. The CLI Product Designer (Proposal D)
2. The Migration And Safety Engineer (Proposal H)
3. The Contract Guardian (Proposal F)
4. The CLI Architect (Proposal A)
5. The Brownfield Team Lead (Proposal G)
6. The Model-Behavior Realist (Proposal C)
7. The Unix Minimalist (Proposal B)
RANKING SURFACE:
1. The Unix Minimalist (Proposal B)
2. The Contract Guardian (Proposal F)
3. The Brownfield Team Lead (Proposal G)
4. The CLI Architect (Proposal A)
5. The Migration And Safety Engineer (Proposal H)
6. The Model-Behavior Realist (Proposal C)
7. The CLI Product Designer (Proposal D)
RANKING GUIDE:
1. The Model-Behavior Realist (Proposal C)
2. The Migration And Safety Engineer (Proposal H)
3. The Brownfield Team Lead (Proposal G)
4. The CLI Product Designer (Proposal D)
5. The Contract Guardian (Proposal F)
6. The CLI Architect (Proposal A)
7. The Unix Minimalist (Proposal B)
RANKING ROOT:
1. The Migration And Safety Engineer (Proposal H)
2. The Contract Guardian (Proposal F)
3. The Unix Minimalist (Proposal B)
4. The Brownfield Team Lead (Proposal G)
5. The CLI Product Designer (Proposal D)
6. The CLI Architect (Proposal A)
7. The Model-Behavior Realist (Proposal C)
```

### The name

Recommend **`route include`**, with this grammar:

```text
open-forge route include <workspace-file>
  [--description <text>]
  [--tag <tag>]...
  [--apply-to <glob>]...
  [--dry-run]
  [global flags]
```

Require a description for a new loader row. An identical existing row can be registered without supplying its metadata again. Preserve unspecified policy and block conflicting adoption rather than silently changing it.

Use `route exclude <workspace-file>` as the non-deleting inverse. These names explain the effect while keeping creation, initialization and deletion distinct.

The runner-up is `route add <workspace-file>`. It is familiar but needs more explanation beside `route create`, so Include remains stronger.

### Steal list

- **The CLI Architect (Proposal A):** Promote linked-source observation into the shared Framework inventory so every consuming command uses the same graph.
- **The Unix Minimalist (Proposal B):** Store only registered paths in config, leaving loading metadata exclusively in Entries.
- **The Model-Behavior Realist (Proposal C):** Add `context --authoring` and resolve workspace-local authoring instructions for help so CLI convenience does not substitute bundled defaults for local rules.
- **The Contrarian (Proposal E):** Preserve existing Framework-only measurements and add a separately labelled effective startup estimate, keeping the coverage change visible.
- **The Contract Guardian (Proposal F):** Give relocation its own operation, separating a consequential migration from ordinary installation configuration.
- **The Migration And Safety Engineer (Proposal H):** Translate owned ignore rules and compare shareable Entries before and after migration so moving private records cannot expose their filenames.

### Most dangerous idea

The Contract Guardian (Proposal F)’s complete-policy replacement on repeated Include is the most dangerous everyday behavior. A caller changing a description could remove `LoadNow`, while a caller supplying a loading tag but omitting the old condition could broaden a specialized rule to the whole workspace.

Preserve unspecified fields. Require an explicit clearing action or direct loader edit to remove existing policy, and show the complete before-and-after row in preview.

### Convergence

All eight proposals choose `route include` and `route exclude`, keep the loader authoritative for activation, preserve handwritten loader entries and distinguish inclusion from ownership.

All recognize that current Libraries are workspace-contained rather than arbitrary machine-wide sources. All keep included content outside automatic metadata completion and catalogue rewriting in #6.

All move mandatory authoring instructions out of the removable README into a separate plain-file reference. They agree on keeping `--help` for command syntax and adding concepts help.

All retain `.agents` as the default, preserve native Skill discovery locations and require explicit migration rather than treating a config edit as a file move.

Most favor path-only registration and fixed control-file locations. A and C instead duplicate loading metadata and introduce a workspace-level settings anchor for custom roots.

### Open conflicts

1. **Path-only registry versus mirrored loading policy.** Choose the path-only registry from B, F and G. Mirrored metadata adds drift without helping loader-only agents. New shapes must receive new schema versions under the existing directive, independently of the forgiving reader’s compatibility behavior.

2. **Conditional authoring reference alone versus explicit CLI authoring retrieval.** Choose C’s explicit retrieval. A Context-only agent needs a supported way to obtain the same local reference and overwrite that a file-reading agent follows. Concepts help should use that local source when a workspace is selected and the bundled source outside a workspace, clearly identifying which it returned.

3. **Relocate content only versus relocate settings and maintenance storage too.** Choose fixed bootstrap settings and ownership paths, as in B, D, F, G and H. Keep locks and recovery history anchored to physical workspace identity. Moving control storage introduces discovery, compatibility and recovery questions that the requested content-root feature does not need.

### Revised position

I would publish settings schema v3 for #6 and v4 for #7, preserving earlier schemas unchanged. Unknown-key tolerance and immutable published schemas are separate requirements.

I would require meaningful descriptions for new entries rather than defaulting them to paths. I would adopt C’s authoring retrieval and shared reading fixtures, including a workspace override and a deleted README.

I would preserve the current References contract explicitly. `Entries` are navigation and remain excluded from authored-reference counts, while route inspection explains exposure edges. [Current reference behavior](../../../../../docs/cli.md)

For #7, I would adopt H’s privacy, recovery-history and layout-provenance safeguards, with any new lock shape versioned properly. I would also prefer F’s dedicated Relocate command over adding migration effects to Configure.

I would retain the recommendation that #7 should not block the first stable release, while giving the chair a concrete implementation threshold: proceed once a complete custom-root journey preserves loading, ownership, privacy and native discovery.
