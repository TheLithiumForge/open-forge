# Open Forge Loader

Read this after `AGENTS.md`. It defines how to select context, follow applicable rules, and maintain the workspace.

## Terms

- `entrypoint` - Markdown file that makes a folder routable. Open Forge uses `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names.
- `entry` - One generated route line under `Entries`, describing a destination rather than containing its contents.
- `description` - Short text that helps a reader decide whether to open a file.
- `responsibility` - Optional sentence stating what a file defines, so an editor can decide what belongs there. It creates no authority or loading behavior.
- `axiom` - Required rule under `Axioms` in this loader or a recognized loaded entrypoint.

## Axioms

### Authority And Inheritance

#### Working With The User

- Respect platform constraints and runtime safety in every action.
- Follow clear user direction when it is safe and allowed. It sets goals, priorities, important choices, and accepted changes. Do not ask the user to confirm it again.
- A request to act allows the routine, reversible, in-scope choices needed to complete it.
- Use the request and accepted context to resolve unclear choices. When an unresolved choice could significantly change the result, scope, risk, cost, external effects, or ability to undo the work, keep it #Contextual and ask before relying on it.
- Match explanation and planning to the request. Unless deeper analysis is requested, begin with the current understanding, one recommendation, and at most one important open choice.
- Continue safe, in-scope work when accepted direction or a stated reversible assumption is enough. Do not stop only to ask whether to continue.
- State assumptions so they can be corrected. Stop when uncertainty, conflict, or an authority boundary could significantly change the work.

#### Sources And Acceptance

- Use a declared external authority for the facts assigned to it.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading rules, or lifecycle.
- Put detail in the narrowest source that defines the question. Higher-level entrypoints and records keep only the summary needed to select that source and link to it.
- Axioms and Directives govern agent behavior within their scope. Accepted documents and other sources define the current requirements, design, or knowledge of their subjects.
- Validation establishes whether evidence supports a claim. Acceptance establishes what may be treated as current within its scope. Moving or restoring a record does not establish acceptance.
- Investigate an apparent #CurrentTruth conflict before changing either source. When accepted direction changes current state, update the source that defines it and preserve useful prior context.

#### How Rules Combine

- Only this loader and recognized loaded entrypoints define active Axioms. A loaded child route inherits its ancestors' Axioms. A child entrypoint adds rules only for its narrower scope.
- Follow loaded Axioms and Directives within their scope. Accepted workspace-specific content replaces corresponding Open Forge defaults only for the same role and within its accepted scope.
- Selected non-binding material may specialize broader material of the same kind. Loaded binding instructions add to one another, not silently override one another. Report unresolved conflicts.

### Routing

#### Terms

- `route` - Navigable path exposed through entrypoints and `Entries`.
- `root route` - Route exposed directly by this loader.
- `scope` - Part of a route that narrows where the following content applies.
- `slug` - Concrete folder name in a route path.
- `managed route` - Route whose declared manager may update known files as a lifecycle action. Management does not create runtime authority.

#### Rules

- An entrypoint lists its direct routes under `Entries`. Select what matters from descriptions, tags, paths, ancestor routes, and explicit links. Read each selected entrypoint before its children.
- Follow relevant branches recursively. Every folder in a route chain has exactly one recognized entrypoint.
- A root route exists only where this loader exposes it. It cannot be scoped or recreated inside another route.
- Any number of routed slugs may narrow a route below its root. They may appear before, between, or after deeper route segments. Each narrows everything that follows it.
- A scope contains only the routes useful there. It need not copy another scope or the installed defaults.
- Put specialized material in the narrowest useful scope. Use workspace-wide placement only for material that applies across the workspace.
- Keep separately selected scopes as separate chains and follow their explicit links. Recheck selection after an important task change.
- When search or a direct link selects a routed file, load its applicable ancestor chain and required context before using it.
- Use entrypoints for navigation. Do not open ordinary file bodies just to discover routes.

#### Management And Customization

- Management affects file lifecycle, not runtime meaning or authority.
- Each manager defines the route shapes it recognizes, preserves their segment order through scopes, and changes only files it owns or can safely identify for the requested lifecycle operation.
- Users may add, move, replace, or remove routes. A familiar slug or tag does not create root behavior or managed status. Other valid routes remain routable.
- Removed defaults stay removed unless the user asks to restore them.
- Generated `Entries` provide navigation, not authority.

#### Overwrites

- A user-owned `{name}.overwrite.md` companion loads immediately after `{name}.md`. It shares the base file's route, scope, and loading behavior. It is not indexed or selected separately.
- Interpret the overwrite as part of its base source, within that source's role and scope. Where they answer the same question differently, the overwrite wins only for the corresponding content. It does not override unrelated sources.

### Tags And Loading

Defined tags keep the meanings below wherever they appear. They control loading or classify content, but do not grant authority. Other tags remain search and routing signals.

Entries stay on demand unless marked #LoadNow or #KeepInMind. These tags act only through loaded parents. Neither activates an unselected ancestor or scope.

Select the scope first, then follow its loading rules. Do not skip a required file because its individual description seems less relevant. Read each source once per loading or refresh pass. A later refresh or a changed source may require another read.

For example, a C# scope may contain a #LoadNow design file and an on-demand Windows scope. Selecting C# loads its design rules, not Windows. A #LoadNow file under Windows loads only after Windows is selected.

#### Defined Tags

- #LoadNow - Read the linked file, in listed order, when a loaded parent exposes it. If it is an entrypoint, apply its child loading rules.
- #KeepInMind - Read exposed continuity context when its parent loads, then refresh it while its scope remains active.
  - Timing: Refresh at task start or resume, after context restoration, and before handoff or closeout.
  - Order: Read the tagged file and its adjacent overwrite. For an entrypoint, apply its child loading rules in listed order.
  - Boundaries: Refresh only active scopes. Do not open file bodies just to discover tagged files.
  - Recheck: Recheck during work only when the active continuity set may have changed.
- #Core - Base routing and loading, workspace orientation, and reusable agent-facing roles.
- #Memory - Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history.
- #Extension - Optional packaged routes, capabilities, integrations, and support files.
- #Contextual - Useful context, not authority by itself. Treat it as unaccepted unless applicable authority establishes acceptance within its scope.
- #CurrentTruth - Accepted current state within its stated scope, subject to user direction, runtime safety, platform constraints, and declared external authorities for the facts they define.
- #Evergreen - Material that must stay aligned with accepted current state. It creates no authority or loading behavior.
  - Update affected #Evergreen sources you are allowed to edit before work depends on them and no later than closeout. Batch related updates when safe.
  - Keep each source aligned with what it represents now. Preserve useful prior context in the appropriate Memory route and report needed updates that are blocked or not authorized.

#### Choosing Loading Tags

- Keep optional content on demand. Follow any stronger loading rule defined by its category.
- Use #LoadNow only when the content is needed whenever its parent loads and missing it would cost more than reading it each time.
- Narrow the scope before making specialized content mandatory. Do not mark a broad parent #LoadNow merely to expose an important descendant.
- Use #KeepInMind only when exposed content also needs refreshing for continuity while its scope remains active. It is not an importance label.

### CLI

Use the relevant commands below when the Open Forge CLI is available. The plain files remain complete without it.

#### Applicable Commands

- `open-forge --help` - Show the complete current command interface
- `open-forge context [<source-reference>...]` - Read startup or selected context
- `open-forge route list [<source-reference>]` - List routed sources and descendants
- `open-forge route inspect <source-reference>` - Inspect one source's route behavior
- `open-forge find [options]` - Find Markdown sources by tags and headings
- `open-forge references <source-reference>` - Inspect direct authored references
- `open-forge index [<source-reference>...]` - Rebuild generated `Entries`
- `open-forge status` - Summarize workspace, context, lifecycle, generated navigation, and recovery
- `open-forge doctor` - Diagnose workspace, routes, references, lifecycle, and recovery without changes

## Entries

- [Required instructions loaded through selected routes](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, tradeoffs, and work situations](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Concise maps to important local and external sources and when to use them](maps/_maps.md) - #LoadNow #Core #Map
- [Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history](memory/_memory.md) - #LoadNow #Memory #OrganicGrowth
- [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Specialized capabilities provided through native SKILL.md packages](skills/_skills.md) - #LoadNow #Core #Skill
- [Copy-ready files for starting independently maintained workspace content](templates/_templates.md) - #Core #Template
