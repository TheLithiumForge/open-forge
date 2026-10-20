---
open-forge:
  description: Every changed statement of the 16 shipped files between develop and v2, with its status, destination and reason, as the council received it
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Loader, Council, ChangeMap]
---

# Statement Ledger

Every statement of the 16 shipped files at `origin/develop` (before) mapped to v2 on `council/opt-in-framework` (after). Status: Unchanged, Kept (same behavior, new wording), Partly (core survives, a part dropped), Changed (intentionally different behavior), Moved (lives in another file), CLI (the CLI and its Skill handle it), Dropped (gone from the shipped Core). Line numbers are each file's own. Destinations name the new file and line. Headings are omitted.

## loader.md (src/open-forge/.agents/loader.md) — 2052 → 823 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L3 | kept | Read this after `AGENTS.md`. It defines how to select context, follow applicable rules, and maintain the workspace. | L3, AGENTS.md L5 | The intro now says what the file is and that it works with plain files. AGENTS.md says to follow its reading steps. |
| L7 | kept | - `entrypoint` - Markdown file that makes a folder routable. Open Forge uses `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names. | L29 | Kept: “A folder's catalogue is named `_{folder-name}.md`.” The compatibility names are still recognized by the CLI and listed in its routes reference. Entrypoint is now called catalogue. |
| L8 | kept | - `entry` - One generated route line under `Entries`, describing a destination rather than containing its contents. | L25 | No definition needed. The first bullet of Change what is on says an entry line carries the description, tags and conditions. |
| L9 | kept | - `axiom` - Required rule under `Axioms` in this loader or a recognized loaded entrypoint, inherited by selected descendants. | L19 | “the `## Axioms` of catalogues, which cover everything a catalogue lists.” |
| L17 | kept | - Respect platform constraints and runtime safety in every action. | L20 | “within platform and safety limits” |
| L18 | kept | - Follow clear user direction when it is safe and allowed. It sets goals, priorities, important choices, and accepted changes. Do not ask the user to confirm it again. | L20 | “The user's direction comes first … without asking again” |
| L19 | kept | - A request to act allows the routine, reversible, in-scope choices needed to complete it. State assumptions that affect the result, and continue when accepted direction or a reversible assumption is enough. | L20 | “Take routine, reversible steps inside the request” |
| L20 | kept | - Use the request and accepted context to resolve unclear choices. When an unresolved choice could significantly change the result, scope, risk, cost, external effects, or ability to undo the work, keep it #Contextual and ask before relying on it. | L20 | “ask before a choice that would significantly change the result, scope, risk or cost.” Council 2 cut it, and the chair restored it because it is an authority rule, not a style rule. |
| L21 | dropped | - Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice. | nowhere | A communication preference, not a framework rule. It fits an optional pack such as the Collaboration Extension. |
| L22 | dropped | - Use the fewest words that preserve meaning, readability, and logical progression in communication and authored content. Remove filler and repetition. Keep the context, conditions, and exceptions needed to understand and apply the result. | nowhere | A writing preference. This repository keeps it in its own writing Directive. |
| L26 | kept | - Use a declared external authority for the facts assigned to it. | L21 | Merged: “Accepted means the user agreed, or a source the user named says so.” |
| L27 | partly | - Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules, or lifecycle. | crystallized L14 | Survives only inside Crystallized Memory: “update its record instead of adding a competing one.” The strongest candidate to restore, because it is a core Open Forge principle. |
| L28 | dropped | - Connect related material with relative links to local sources and precise references. Reuse consistent descriptive tags for shared subjects so tag and reference searches can find those connections. Add links and tags only when they help navigation or retrieval. | nowhere | Ordinary authoring practice that capable models follow without being told. |
| L29 | moved | - Put detail in the narrowest source that defines the question. Higher-level entrypoints and records keep only the summary needed to select that source and link to it. | directives L9 | “subfolders narrow them to an area” |
| L30 | kept | - Axioms and Directives govern agent behavior within their scope. Accepted documents and other sources define the current requirements, design, or knowledge of their subjects. | L19, L21 | Rule files and catalogue Axioms are followed. Other content is not accepted just by being read. |
| L31 | kept | - Validation establishes whether evidence supports a claim. Acceptance establishes what may be treated as current within its scope. Moving or restoring a record does not establish acceptance. | L21 | “Reading, linking, saving or tagging a file does not make its content accepted” |
| L32 | partly | - Investigate an apparent #CurrentTruth conflict before changing either source. When accepted direction changes current state, update the source that defines it and preserve useful prior context. | crystallized L14 | Crystallized Memory keeps “update its record instead of adding a competing one.” The explicit investigate first sentence was dropped. |
| L36 | kept | - Only this loader and recognized loaded entrypoints define active Axioms. A loaded child inherits its ancestors' rules and adds only requirements specific to its narrower scope. Do not repeat inherited rules. | L19 | “Follow the rules you read within their stated scope … Narrower rules add to broader ones.” |
| L37 | dropped | - When authoring an entrypoint with no local rules, use `## Axioms` with one entry: `- inherited - No local axioms; loaded ancestor axioms remain active.` Existing missing or empty local sections also add no rules. Never combine the inherited marker with local rules. | nowhere | The marker stays valid where present and route scaffolding still writes it. Agents do not need to know about it. |
| L38 | kept | - Follow loaded Axioms and Directive Instructions within their scope. Adding a child instruction does not cancel an active ancestor rule. Report unresolved conflicts. | L19 | “When two rules conflict, say so instead of picking one.” |
| L39 | kept | - Customize a default by editing or replacing its source, or by adding its overwrite companion. This changes that source's content. It does not grant a separate file precedence over other active rules. | L30 | “write your change in `{name}.overwrite.md` beside it” |
| L40 | dropped | - Selected non-binding material may specialize broader material of the same kind within its accepted scope. | nowhere | Rarely matters. Guidance still says to adapt advice when another approach fits better. |
| L46 | dropped | - `route` - Navigable path exposed through entrypoints and `Entries`. | nowhere | Term removed. The behavior lives in the steps. |
| L47 | dropped | - `root route` - Route exposed directly by this loader. | nowhere | Term removed. The loader's Entries are simply the starting list. |
| L48 | dropped | - `scope` - Part of a route that narrows where the following content applies. | nowhere | Term removed. “Narrower rules add to broader ones” and catalogue conditions carry the meaning. |
| L49 | dropped | - `slug` - Concrete folder name in a route path. | nowhere | Term removed. Ordinary folder names. |
| L50 | cli | - `managed route` - Route whose declared manager may install, update, or remove identified files. Management does not create runtime authority. | CLI SKILL.md L41 | Which files an installer may change is the CLI's concern, covered in the CLI Skill's packages reference. |
| L54 | kept | - An entrypoint lists its direct routes under `Entries`. Select what matters from descriptions, tags, paths, ancestor routes, and explicit links. Read each selected entrypoint before its children. | L11, L13 | Required reads in step 3, description-based choice in step 5, parent before children in both. |
| L55 | kept | - Follow relevant branches recursively. Every folder in a route chain has exactly one recognized entrypoint. | L11, L29 | “then do the same inside each catalogue you read” and the catalogue naming rule. |
| L56 | changed | - A root route exists only where this loader exposes it. It cannot be scoped or recreated inside another route. | L25 | Reversed on purpose. Entries may now link straight to a file anywhere in the workspace, such as `../docs/architecture.md`. |
| L57 | dropped | - Any number of routed slugs may narrow a route below its root. They may appear before, between, or after deeper route segments. Each narrows everything that follows it. | nowhere | Ordinary folders need no rule. |
| L58 | dropped | - A scope contains only the routes useful there. It need not copy another scope or the installed defaults. | nowhere | Obvious once Entries are the switch. |
| L59 | moved | - Put specialized material in the narrowest useful scope. Use workspace-wide placement only for material that applies across the workspace. | directives L9 | “Rule files here apply to the whole workspace, and subfolders narrow them to an area.” |
| L60 | kept | - Keep separately selected scopes as separate chains and follow their explicit links. Recheck selection after an important task change. | L15 | “Start again from step 1 when your working files or goal change.” |
| L61 | kept | - When search or a direct link selects a routed file, load its applicable ancestor chain and required context before using it. | L13 | “Before applying a file you reached through search, read the listed catalogues above it.” |
| L62 | dropped | - Use entrypoints for navigation. Do not open ordinary file bodies just to discover routes. | nowhere | Descriptions on Entries lines make it unnecessary. |
| L66 | cli | - Management determines which files an installer or updater may change. It does not determine their meaning or authority. | CLI SKILL.md L41 | Management ownership is described in the CLI Skill's packages reference. |
| L67 | cli | - Each manager defines the route shapes it recognizes. It preserves their segment order through scopes. It changes only files it owns or can safely identify for the requested lifecycle operation. | CLI SKILL.md L40, CLI SKILL.md L41 | Route shapes the installer recognizes are the CLI's concern. |
| L68 | kept | - Users may add, move, replace, or remove routes. A familiar slug or tag does not create root behavior or managed status. Other valid routes remain routable. | L25, L26 | Add a line to turn something on, remove a line to turn it off. |
| L69 | kept | - Removed defaults stay removed unless the user asks to restore them. For managed files, record intentional removals in the manager's exclusions before updating. | L26 | Stronger: “nothing should bring it back unless the user asks.” `index` now keeps removals instead of re-adding them. Inside other catalogues, lines follow their folder, so a removal there means deleting or moving the file. |
| L70 | kept | - Generated `Entries` provide navigation, not authority. | L21 | Generated or not, an entry line gives no authority by itself. |
| L74 | kept | - A user-owned `{name}.overwrite.md` companion loads immediately after `{name}.md`. It shares the base file's route, scope, and loading behavior. It is not indexed or selected separately. | L30 | “It is read right after its base” |
| L75 | kept | - Interpret the overwrite as part of its base source, within that source's role and scope. Where they answer the same question differently, the overwrite wins only for the corresponding content. It does not override unrelated sources. | L30 | “wins where they differ, for that file only.” |
| L79 | kept | Defined tags keep the meanings below wherever they appear. They control loading or classify content, but do not grant authority. Other tags remain search and routing signals. | L28 | “Every other tag is a search label.” |
| L81 | kept | Entries stay on demand unless marked #LoadNow or #KeepInMind. These tags act only through loaded parents. Neither activates an unselected ancestor or scope. | L11, L13, L28 | On demand by default (step 5). “A required file inside a catalogue needs #LoadNow on that catalogue too.” |
| L83 | kept | Select the scope first, then follow its loading rules. Do not skip a required file because its individual description seems less relevant. Read each source once per loading or refresh pass. A later refresh or a changed source may require another read. | L11 | “Never skip one because its description looks unrelated. Read each file once per pass.” |
| L85 | dropped | For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected. | README.md L9 | The abstract C# and Windows example is gone. The README shows a concrete linked example instead. |
| L89 | kept | - #LoadNow - Read the linked file, in listed order, when a loaded parent exposes it. If it is an entrypoint, apply its child loading rules. | L11, L28 | Step 3, and “Tag an entry #LoadNow when it must always be read.” |
| L90 | kept | - #KeepInMind - Read the tagged context when its parent loads, then refresh it while its scope remains active. | L11, L15, L28 | Read with the required set at the start, then reread before handoff. |
| L91 | kept |   - Timing: Refresh at task start or resume, after context restoration, and before handoff or closeout. | L9, L15 | Start or resume and after restore (step 1), before handoff or finish (the restart line). |
| L92 | kept |   - Order: Read the tagged file and its adjacent overwrite. For an entrypoint, apply its child loading rules in listed order. | L11, L30 | “in listed order, then do the same inside each catalogue”. The overwrite rule covers the adjacent overwrite. |
| L93 | dropped |   - Boundaries: Refresh only active scopes. Do not open file bodies just to discover tagged files. | nowhere | Refresh only active scopes. Dropped as obvious once there are no scopes to track. |
| L94 | dropped |   - Recheck: Recheck during work only when the tagged files in active scopes may have changed. | nowhere | Recheck during work when a tagged file changes. Simplified to start plus before handoff. |
| L95 | dropped | - #Core - Base routing and loading, workspace orientation, and reusable agent-facing roles. | nowhere | No defined meaning any more. Still used as a label. |
| L96 | dropped | - #Memory - Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history. | nowhere | No defined meaning any more. Memory's own catalogue says what it is. |
| L97 | dropped | - #Extension - Optional packaged routes, capabilities, integrations, and support files. | nowhere | No defined meaning any more. Still used as a label. |
| L98 | kept | - #Contextual - Useful context, not authority by itself. Treat it as unaccepted unless applicable authority establishes acceptance within its scope. | L21 | Kept as behavior: content is not accepted until the user or a named source says so. The tag stays as a label. |
| L99 | kept | - #CurrentTruth - Accepted current state within its stated scope, subject to user direction, runtime safety, platform constraints, and declared external authorities for the facts they define. | L21, crystallized L14 | Kept as behavior in the acceptance bullet and in Crystallized Memory. |
| L100 | partly | - #Evergreen - Material that must stay aligned with accepted current state. It creates no authority or loading behavior. | crystallized L14 | Crystallized Memory: “When accepted direction changes a subject, update its record.” |
| L101 | partly |   - Update affected #Evergreen sources you are allowed to edit before work depends on them and no later than closeout. Batch related updates when safe. | crystallized L14 | The update duty survives in Crystallized Memory. The before work depends on them timing was dropped. |
| L102 | partly |   - Keep each source aligned with what it represents now. Preserve useful prior context in the appropriate Memory route. Report needed updates that are blocked or not authorized. | emerging L14, archived L13 | Preserving prior context survives in Emerging and Archived. Report blocked updates was dropped. |
| L106 | kept | - Keep optional content on demand. Follow any stronger loading rule defined by its category. | L13 | Everything not required is opened only when its description fits. |
| L107 | kept | - Use #LoadNow only when the content is needed whenever its parent loads and missing it would cost more than reading it each time. | L28, directives L14 | “#LoadNow when it must always be read.” Directives say which files need it. |
| L108 | moved | - Narrow the scope before making specialized content mandatory. Do not mark a broad parent #LoadNow merely to expose an important descendant. | directives L14 | “Limit it with `applyTo` … A subfolder of rules needs a #LoadNow catalogue, limited the same way.” |
| L109 | kept | - Use #KeepInMind only when work needs the content read again at the defined refresh points. It is not an importance label. | L28 | “#KeepInMind when it must also be reread before a handoff” says exactly what it is for. |
| L113 | kept | Routed Markdown files start with YAML frontmatter. Open Forge fields sit at the root, or under an `open-forge:` key that then holds all of them. When you add metadata, follow `frontmatter` in `.agents/open-forge.json`. A missing value means scoped. For routing, each line under an entrypoint's `Entries` repeats a file's description, tags, and `applyTo` patterns, so you can decide before opening it. | L29 | Shorter: “in the same form as the files around them.” The CLI still reads the setting in `open-forge.json`. |
| L115 | kept | - `description` - Required short text that helps a reader decide whether to open the file. | L29 |  |
| L116 | cli | - `responsibility` - Optional sentence stating what the file defines, so an editor can decide what belongs there. It creates no authority or loading behavior. | CLI SKILL.md L40 | Still valid metadata, documented in the CLI routes reference. |
| L117 | kept | - `tags` - Required list of tag names without `#`. Defined Tags gives the reserved meanings. | L29 |  |
| L118 | kept | - `applyTo` - Optional glob patterns, as a list or a comma-separated string, that filter a file by the files the task works on. Root and scoped declarations are both read. Equal patterns count once, and different patterns are invalid. | L12, L29 | The field is kept. Format details (list or comma string, root and scoped forms, duplicates) are in the CLI routes reference. |
| L119 | kept |   - Filter: A file that would load or refresh does so, and applies, only while one working file matches its patterns and those of every ancestor with `applyTo`. `applyTo` never loads a file by itself. | L12 | “only when the same working file matches its patterns and those of every conditioned catalogue above it.” |
| L120 | kept |   - Patterns: Globs match from the workspace root and are case-sensitive. `*.py` matches root files, `**/*.py` any depth, and `docs/**` a folder. A leading `!` is literal and does not exclude files. | L12 | “case-sensitive globs from the workspace root, such as `**/*.cs` or `docs/**`.” |
| L121 | kept |   - Working files: Files the task investigates, creates, changes, deletes, renames, or reviews, including planned ones. Reading a file for context does not count. When work reaches another file, that file's own conditions apply. `applyTo` never grants or restricts edit permission. | L9, L21 | “A file you only read for background is not a working file” and “gives no permission to edit it.” |
| L122 | kept |   - Unknown files: While the working files are unknown, report conditioned #LoadNow and #KeepInMind entries as pending. | L12 | “treat these entries as pending and resolve them before you touch a matching file.” |
| L126 | kept | Use the relevant commands below when the Open Forge CLI is available. The plain files remain complete without it. | L3, L10 | The intro says it works with plain files, and step 2 falls back to file tools. |
| L130 | cli | - `open-forge --help` - Show the complete current command interface | CLI SKILL.md L25 | In the CLI Skill. |
| L131 | kept | - `open-forge context [<source-reference>...]` - Read startup or selected context | L10 | The one command the loader names, with `--for` and a fallback. |
| L132 | cli | - `open-forge route list [<source-reference>]` - List routed sources and descendants | skills L13, CLI SKILL.md L16 | The Skills catalogue says to read the CLI Skill before any command other than `context`. |
| L133 | cli | - `open-forge route inspect <source-reference>` - Inspect one source's route behavior | skills L13, CLI SKILL.md L16 |  |
| L134 | cli | - `open-forge find [options]` - Find Markdown sources by tags and headings | skills L13, CLI SKILL.md L16 |  |
| L135 | cli | - `open-forge references <source-reference>` - Inspect direct authored references | skills L13, CLI SKILL.md L16 |  |
| L136 | cli | - `open-forge index [<source-reference>...]` - Complete missing metadata and navigation, including generated `Entries` | L29, CLI SKILL.md L16 | `open-forge index` is also named in the frontmatter bullet. |
| L137 | cli | - `open-forge status` - Summarize workspace, context, lifecycle, generated navigation, and recovery | skills L13, CLI SKILL.md L16 |  |
| L138 | cli | - `open-forge doctor` - Diagnose workspace, routes, references, lifecycle, and recovery without changes | skills L13, CLI SKILL.md L16 |  |
| L142 | kept | - [Required instructions loaded through selected routes](directives/_directives.md) - #LoadNow #Core #Directive | L34 | Still #LoadNow. The description says what it is in plain words. |
| L143 | changed | - [Advice for recurring choices, tradeoffs, and work situations](guidance/_guidance.md) - #LoadNow #Core #Guidance | L35 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. |
| L144 | changed | - [Concise maps to important local and external sources and when to use them](maps/_maps.md) - #LoadNow #Core #Map | L36 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. This repository's own Maps keeps #LoadNow because it lists a required map. |
| L145 | kept | - [Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history](memory/_memory.md) - #LoadNow #Memory | L37 | Still #LoadNow. Plain description. |
| L146 | changed | - [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern | L38 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. |
| L147 | changed | - [Specialized capabilities provided through native SKILL.md packages](skills/_skills.md) - #LoadNow #Core #Skill | L39 | #LoadNow removed. The CLI Skill is no longer read at every startup. Step 2 names `context` itself, so a model that never opens the Skill still gets the shortcut. |
| L148 | kept | - [Copy-ready files for starting independently maintained workspace content](templates/_templates.md) - #Core #Template | L40 | Already on demand. Only the description changed. |

## AGENTS.md (src/open-forge/AGENTS.md) — 43 → 56 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L5 | changed | Before starting a task, read `.agents/loader.md`. Use it to select every relevant scope, including nested scopes. Follow the loaded rules throughout the task. | L5 | “Use it to select every relevant scope” became “follow its reading steps”, plus the CLI shortcut. |

## README.md (src/open-forge/.agents/README.md) — new file

New file. Full text is in the worktree.

## directives (src/open-forge/.agents/directives/_directives.md) — 176 → 101 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Required instructions loaded through selected routes tags: [LoadNow, Core, Directive] --- | L1 | Plain description. Tags unchanged, still #LoadNow. |
| L11 | kept | Directives define required agent behavior within a scope. | L9 |  |
| L15 | kept | - Each Directive file in the same folder as an `entrypoint` must carry #LoadNow. Directives are mandatory within their scope. | L14 | “Tag each rule file #LoadNow.” Mandatory within scope is now the loader's step 3. |
| L16 | kept | - Each Directive file keeps its instructions under a non-empty `## Instructions` heading. | L13 | “Put each file's rules under `## Instructions`” |
| L17 | kept | - State each obligation, its conditions, and meaningful exceptions in the instructions. Supporting examples or visuals illustrate those instructions. The obligation must remain understandable from the instructions themselves. | L13 | “with when they apply and their exceptions, so they make sense on their own.” |
| L18 | kept | - Directive files in this root `entrypoint`'s folder apply throughout the workspace. | L9 | “Rule files here apply to the whole workspace” |
| L19 | changed | - A child entrypoint does not need #LoadNow. It loads only when selected. | L14 | Reversed: a subfolder of rules now needs a #LoadNow catalogue, limited by `applyTo` when it matches files. |
| L20 | changed | - Select a child entrypoint before loading its Directive files. Their Instructions apply only within that narrower scope. | L14 | Selection by judgment is replaced by `applyTo` conditions that step 4 evaluates. |
| L21 | moved | - Child Directives add to active parent Directives. A narrower scope does not create higher authority. | loader.md L19 | “Narrower rules add to broader ones.” |
| L22 | moved | - Report any conflict or instruction that cannot be followed, and explain why. | loader.md L19 | “When two rules conflict, say so instead of picking one.” |

## guidance (src/open-forge/.agents/guidance/_guidance.md) — 115 → 72 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Advice for recurring choices, tradeoffs, and work situations tags: [LoadNow, Core, Guidance] --- | L1 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. |
| L11 | kept | Guidance recommends approaches for recurring situations and explains when they fit. It offers adaptable advice, not the binding instructions of a Directive. | L9 | “It is not a rule.” |
| L15 | moved | - Check `Entries` when the work involves a recurring situation or choice that may have Guidance. | loader.md L35 | The trigger now lives in the entry description: “to check before choosing an approach.” |
| L16 | kept | - Each Guidance file explains the situation, recommended approach, reasons, and tradeoffs. | L9 | “with its reasons and tradeoffs” |
| L17 | kept | - Apply Guidance when it fits the work. Adapt it when another approach fits better, and explain the reason when the difference matters to the result. | L13 |  |

## maps (src/open-forge/.agents/maps/_maps.md) — 168 → 78 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Concise maps to important local and external sources and when to use them tags: [LoadNow, Core, Map] --- | L1 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. |
| L11 | kept | Maps point to important local and external sources and explain when they matter. Each destination retains its own detail and authority. | L9 | “Each destination keeps its own detail and authority.” |
| L17 | moved | - Check `Entries` when the work needs an important local or external source. | loader.md L36 | The trigger now lives in the entry description: “checked before relying on it.” |
| L18 | kept | - Each Map route links to one or more destinations and says what they contain and when to use them. | L9, L13 | “saying what each holds and when to use it” |
| L19 | kept | - Local links are relative to the Map file. External sources use their normal URL. | L13 | “Link each source relative to the Map, or by URL” |
| L23 | dropped | - Keep Maps broad. Prefer modules, projects, repositories, systems, or scopes over members and functions unless finer routing clearly helps. | nowhere | Granularity advice. Each workspace decides. |
| L24 | dropped | - Each workspace chooses its own Map filenames, grouping, and nesting depth. | nowhere | Already true: nothing fixes Map layout. |
| L25 | partly | - Maps do not replace Memory or the destination's current information. | L9 | “Each destination keeps its own detail and authority” covers most of it. |

## patterns (src/open-forge/.agents/patterns/_patterns.md) — 363 → 107 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Reusable default shapes for code, files, APIs, documents, and other work tags: [LoadNow, Core, Pattern] --- | L1 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. |
| L11 | kept | Patterns define reusable default shapes that make related work consistent, easy to create, and easy to inspect. | L9 | “so related work looks alike” |
| L17 | moved | - Check `Entries` when the work creates, changes, or reviews something with a visible structure. | loader.md L38 | The trigger now lives in the entry description: “before creating or reviewing structure.” |
| L18 | kept | - Each Pattern defines a concrete shape for code, files, naming, placement, boundaries, APIs, documents, or another inspectable result. | L9 | “one reusable shape for code, files, APIs or documents” |
| L19 | dropped | - Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions, or unsettled candidates into a Pattern. | nowhere | When to create a Pattern is left to the author. |
| L23 | kept | - Show one reusable shape as the main content through a concrete example, tree, table, diagram, or annotated outline of the expected result. A reviewer given the Pattern and real code or files should be able to compare their structure without further explanation. | L14 | “Show the shape as an example, tree, table or outline” |
| L24 | kept | - Add brief applicability notes. Label which parts are required, optional or repeatable, how they nest and relate, and which names, values and orders are fixed or only illustrative. | L14 | “label which parts are required and which are only illustrative” |
| L25 | dropped | - Link related sources for required rules, advice, maps, procedures, and starting files, and say which of them bind the shape. Sources that apply the shape should link back to its Pattern. | nowhere | Back-linking rule for related sources. |
| L26 | dropped | - Keep required behavior, procedures, and substantial choice rationale in their defining sources. Example names and values illustrate the shape unless an applicable requirement fixes them. | nowhere | Where rationale lives. Covered by the loader's rule that each file answers its own role. |
| L27 | kept | - Use examples that are valid for their APIs, formats, and tools. Label intentionally incomplete examples as schematic and identify what is omitted. | L14 | “Keep examples valid, or label them schematic.” |
| L28 | partly | - Treat an applicable Pattern as the default shape in its scope. A justified adaptation is allowed within the authority already granted for the work. Explain material departures before other work depends on them. A justified adaptation is still a departure, so a review reports it separately from conformance. | L13 | “adapt it and explain the difference before other work depends on it.” Report adaptations separately in a review was dropped. |
| L29 | moved | - When an applicable rule or accepted requirement makes a shape mandatory, follow it. Ask only when a departure needs authority that has not been granted. | loader.md L19 | Covered by the loader: rules you read are followed. |

## skills (src/open-forge/.agents/skills/_skills.md) — 174 → 101 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Specialized capabilities provided through native SKILL.md packages tags: [LoadNow, Core, Skill] --- | L1 | #LoadNow removed, so it loads only when step 5 finds that its description fits the task. The description now says when to check it. |
| L11 | kept | A Skill is a native capability package entered through `SKILL.md`. That file defines how to use the Skill and its supporting resources. | L9 |  |
| L15 | changed | - When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start. Respect an intentional omission and use the loader's CLI summary instead. Use the CLI only when it is available; otherwise read and maintain the files directly. | L13, loader.md L10 | “Read `open-forge-cli/SKILL.md` before running `open-forge` commands other than `context`.” The fallback to plain files is in the loader's step 2. |
| L16 | moved | - Check `Entries` when the work may benefit from a Skill. | loader.md L39 | The trigger now lives in the entry description: “before specialized work.” |
| L17 | kept | - Follow the selected `SKILL.md` for metadata, use, instructions, and resource loading. | L13 | “Read the matching `SKILL.md` before using a capability.” |
| L18 | kept | - The active agent runtime controls Skill activation, invocation, installation, and execution. | L9 | “Your agent runtime controls how they are activated and run.” |

## templates (src/open-forge/.agents/templates/_templates.md) — 246 → 94 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Copy-ready files for starting independently maintained workspace content tags: [Core, Template] --- | L1 | Description only. Already on demand. |
| L11 | kept | Templates are reusable starting files. Copy and adapt a Template, then maintain the result independently. | L9 |  |
| L17 | kept | - Check `Entries` when copy-ready starting content would help with a new artifact. | L13 |  |
| L18 | kept | - Choose the most relevant Template. Keep only the sections and detail the destination needs, while preserving its required structure. | L14 | “keep only the sections it needs” |
| L19 | kept | - Replace metadata and placeholders so they describe the destination's ownership, scope, state, authority, and relationships. | L14 | “replace placeholders and metadata” |
| L20 | moved | - Give copied content its own metadata in the workspace's chosen form. Native `SKILL.md` keeps its native metadata. | loader.md L29 | Metadata form is the loader's frontmatter rule: “in the same form as the files around them.” |
| L21 | kept | - Remove the #Template tag unless the result is another Template. Remove package tags that do not describe the destination. | L14 | “remove the #Template tag unless it is another Template” |
| L22 | kept | - Rebase relative links for the destination. | L14 | “fix relative links” |
| L23 | kept | - Later Template changes do not update existing copies. | L14 | “Later Template changes do not update copies.” |
| L24 | dropped | - If the new artifact needs continuing guidance or requirements, link the source that defines them. The Template provides only starting content. | nowhere | Linking continuing requirements from a copy. |
| L28 | dropped | - State the need and primary question each Template answers in its description and source instructions. | nowhere | Authoring advice. |
| L29 | dropped | - Use focused prompts that request necessary content once, in a readable order. Mark optional sections clearly. | nowhere | Authoring advice. |
| L30 | dropped | - Treat generic Templates as fallbacks. Add a specialized Template only when it provides meaningfully different starting content. | nowhere | Authoring advice. |
| L31 | moved | - Users may edit, replace, scope, or remove Templates. | loader.md L25, loader.md L26 | Now the loader's general rule for turning parts on and off. |

## memory (src/open-forge/.agents/memory/_memory.md) — 467 → 163 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history tags: [LoadNow, Memory] --- | L1 | Plain description of what it holds. |
| L11 | kept | Memory evolves with the work. It holds Markdown records for active work, coordination, accepted knowledge, candidates, and history. | L1 | What Memory holds moved into the description and the entry lines. |
| L13 | dropped | Its self-growing structure lets people and agents add useful records and routed scopes without a fixed structural limit. Unselected branches do not load their file contents. Records need not pass through every state. | nowhere | The self-growing framing. “Unselected branches do not load” is the loader's step 5. |
| L19 | dropped | - An accepted record can define current knowledge or decisions. Recording another category's content does not give the record that category's role. | nowhere | A record's role follows from the folder it lives in. |
| L20 | kept | - Keep unsettled conclusions in a #Contextual Memory route. Apply the loader's acceptance rules before treating them as current knowledge. | L14 | “Saving or moving a record does not make it accepted.” |
| L24 | moved | - Save agent communication and coordination only when they must survive the current context. Do not save every conversation. | saving.md L14 | “Save only what another session needs.” |
| L25 | moved | - Save a durable outcome when future work needs the accepted result, its reasoning, a required behavior, or a reusable shape. Otherwise, create no durable record. | saving.md L14 | “Finished work needs no record unless its outcome still matters.” |
| L26 | partly | - Show the source and uncertainty when they affect trust or later acceptance. Distinguish what was stated, observed, or inferred. | L13 | “say where a record came from and how certain it is when that affects trust.” Stated, observed or inferred was dropped. |
| L27 | partly | - Update affected sources before other work depends on the outcome and no later than closeout. Keep useful context and reasoning in Memory. Accepted durable knowledge must reach the source that defines it, not remain only in chat or a #Contextual record. | emerging L14, saving.md L13 | Accepted results reach their defining file through Emerging. The timing moved to the saving policy. |
| L28 | moved | - Before closeout, also preserve unsettled findings worth revisiting and temporary state needed to resume or transfer unfinished work. | saving.md L13 | “Before you pause or hand off work that is not finished, update its record” |
| L32 | dropped | - Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them. | nowhere | Cost and benefit of adding records. |
| L33 | kept | - Move material when its state, scope, or intended use changes. The destination's rules govern its new role. Do not leave a stale or competing copy. | L14 | “When a record's state changes, move it and leave no stale copy.” |
| L34 | moved | - Put accepted instructions for future agent behavior in the source for that role. | emerging L14 | “move the result to the file that should define it” |
| L35 | partly | - Named record categories and Templates are optional. Use an existing state and the narrowest useful scope. A record may live directly under that state. Do not recreate a removed category merely to save a record. | L15 | “Do not recreate a Memory folder that has been removed.” The rest is implied. |
| L36 | partly | - Use an existing suitable record before creating another. Within the authority granted for the work, add an ordinary record or scoped route when needed. Ask when its placement would change accepted meaning or cross an unresolved authority boundary. | L13, loader.md L20 | “Use an existing record before creating another.” Ask when placement changes meaning is covered by the loader's ask-before-significant-choices rule. |
| L37 | moved | - Get explicit user direction before creating a new top-level Memory state. | loader.md L20 | Covered by the loader: ask before a choice that significantly changes the result. |
| L41 | changed | - [Useful history that no longer controls current work](archived/_archived.md) - #Memory #Archived #Contextual #Historical | L19 | Description only. |
| L42 | changed | - [Accepted knowledge that should remain current](crystallized/_crystallized.md) - #LoadNow #Memory #Crystallized #CurrentTruth | L20 | Description only. |
| L43 | changed | - [Useful material that is not accepted yet](emerging/_emerging.md) - #KeepInMind #Memory #Emerging #Contextual #Candidate | L21 | #KeepInMind removed in the payload. Reread before every handoff was a heavy default for candidate material. |
| L44 | changed | - [Temporary memory that helps agents continue or resume active work](working/_working.md) - #LoadNow #Memory #Working #Contextual | L22 | Description only. |

## working (src/open-forge/.agents/memory/working/_working.md) — 127 → 107 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Temporary memory that helps agents continue or resume active work tags: [LoadNow, Memory, Working, Contextual] --- | L1 | Description only. |
| L11 | kept | Working Memory holds temporary context for continuing or resuming active work. | L9 | “Records of unfinished work, so another session can pick it up.” |
| L17 | kept | - Check `Entries` before deciding that no Working Memory applies. | L13 | “read the record of that work if there is one, and check it against the current files.” |
| L18 | moved | - Save the temporary context needed to resume unfinished work. | saving.md L13 | Now its own policy file. |
| L19 | dropped | - An explicitly accepted temporary choice may remain here when its source, scope, and expected expiration are clear. | nowhere | Accepted temporary choices with an expiry. Rare enough to leave to the user. |
| L23 | kept | - Keep Working Memory small, current, and easy to replace. | L14 | “Keep each record small” |
| L24 | kept | - When the active need ends, preserve useful results, then move, archive, consolidate, or prune the record. | L14 | “When the work ends, move useful results to where they belong and retire the record.” |
| L28 | changed | - none - No entries - #Empty | L18 | Entries now list `saving.md`. |

## saving.md (src/open-forge/.agents/memory/working/saving.md) — new file

New file. Full text is in the worktree.

## crystallized (src/open-forge/.agents/memory/crystallized/_crystallized.md) — 120 → 79 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Accepted knowledge that should remain current tags: [LoadNow, Memory, Crystallized, CurrentTruth] --- | L1 | Description only. |
| L11 | kept | Crystallized Memory holds accepted knowledge that should remain current within its scope. | L9 |  |
| L15 | kept | - Check `Entries` before deciding that no Crystallized Memory applies. | L13 |  |
| L16 | kept | - Keep only accepted current knowledge here, using the loader's acceptance rules within the record's scope. | L14 | “Keep only accepted knowledge here.” |
| L17 | moved | - Tags, repetition, and agent confidence do not establish acceptance. | loader.md L21 | “Reading, linking, saving or tagging a file does not make its content accepted” |
| L18 | kept | - Update, split, merge, or reshape existing Crystallized Memory instead of creating a competing current version. | L14 | “update its record instead of adding a competing one” |
| L19 | kept | - Archive or link material that is no longer current. Keep enough context to understand the change. | L14 | “archive or link what it replaces.” |

## emerging (src/open-forge/.agents/memory/emerging/_emerging.md) — 168 → 80 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Useful material that is not accepted yet tags: [KeepInMind, Memory, Emerging, Contextual, Candidate] --- | L1 | #KeepInMind removed in the payload. This repository's own copy keeps it for its authors-findings record. |
| L11 | kept | Emerging Memory keeps useful findings, possibilities, and reasoning that are not accepted yet. | L9 |  |
| L17 | partly | - At each applicable #KeepInMind refresh, check visible `Entries` and save useful candidate material before it is lost. | saving.md L13 | Saving before work is lost is now the saving policy. Candidate capture at every refresh was dropped. |
| L18 | moved | - "Nothing useful to save" is a valid result. Do not turn raw activity into Memory. | saving.md L14 | “Save only what another session needs.” |
| L19 | kept | - Keep uncertainty, source, and scope visible when they affect later use. | L13 |  |
| L23 | dropped | - Refine material while its validity, use, or final destination remains unsettled. | nowhere | Refining unsettled material is what Emerging is for anyway. |
| L24 | kept | - When material becomes accepted, move the durable result to the source that should define it. Preserve useful outcomes and reasoning before archiving or pruning rejected or replaced material. | L14 | “move the result to the file that should define it … keeping useful reasoning.” |
| L25 | dropped | - Revisit repeated or stale material and decide whether to consolidate, promote, archive, or prune it. Review may leave a record unchanged. | nowhere | Periodic review of stale material. |

## archived (src/open-forge/.agents/memory/archived/_archived.md) — 178 → 94 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L1 | changed | --- open-forge: description: Useful history that no longer controls current work tags: [Memory, Archived, Contextual, Historical] --- | L1 | Description only. |
| L11 | kept | Archived Memory keeps useful history without making it current authority. | L9 |  |
| L15 | moved | - Check `Entries` when current work may need archived context. | memory L19 | On demand by its entry description: “History that no longer guides current work.” |
| L16 | kept | - Before archiving, extract anything still current into the sources that define it. Retain the remaining material when its history is useful, or link to it when it belongs elsewhere. | L13 | “move anything still current to the file that defines it” |
| L17 | dropped | - Retain, consolidate, or transform history according to its future value. Archival need not preserve every detail. | nowhere | How much history to keep is the author's call. |
| L18 | kept | - Preserve useful information about where the material came from, why it was archived, and what replaced it. | L13 | “note why the material was archived and what replaced it.” |
| L19 | kept | - Remove metadata from archived material that would keep its former behavioral role active or claim current authority. | L13 | “Remove loading tags from it.” |
| L20 | kept | - Delete retained material only under user direction or accepted retention preferences. | L14 | “Delete archived material only when the user asks.” |
| L21 | kept | - Validate material against current conditions before restoring it to an explicit destination. Establish acceptance before treating it as current knowledge. | L14 | “check it against current conditions and get it accepted again.” |

## CLI SKILL.md (src/open-forge/.agents/skills/open-forge-cli/SKILL.md) — 590 → 590 words

| Old | Status | Old text | Now at | Note |
| --- | --- | --- | --- | --- |
| L8 | changed | Use the optional CLI to read and maintain Open Forge's Markdown, rules, and links. The Skills entrypoint requires this guide at startup. Loading it grants no installation or package-change authority. | L8 | “The Skills entrypoint requires this guide at startup” became “Read this guide before running commands other than `context`.” |
| L29 | changed | ```sh open-forge context skills/open-forge-cli --for src/Order.cs --for web/order.ts ``` | L29 | The example no longer loads the Skill itself as a startup source. |
