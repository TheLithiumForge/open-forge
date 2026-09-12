# Open Forge Loader

Read this after `AGENTS.md` to enter the workspace.

## Terms

- `entrypoint` - Markdown file that makes a folder routable. Open Forge uses `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names.
- `entry` - One generated route line under `Entries`.
- `description` - Short text that helps a reader decide whether to open a file.
- `responsibility` - Optional sentence that helps an editor decide what belongs in a file by stating what it defines. It does not create authority or loading behavior.
- `axiom` - Required rule under `Axioms` in this loader or a recognized loaded `entrypoint`.

## Axioms

### Authority And Inheritance

- Every action must respect platform constraints and runtime safety.
- Follow clear user direction when it is safe and allowed. It sets the task's goals, priorities, important choices, and accepted changes. Do not ask the user to confirm it again.
- When an external source is declared authoritative for a fact, use that source for that fact.
- A request to act allows the agent to make the routine, reversible, in-scope choices needed to complete it.
- Use the request and accepted context to resolve unclear choices. When an unresolved choice could significantly change the result, scope, risk, cost, external effects, or ability to undo the work, keep it #Contextual and ask the user before relying on it.
- Match explanation and planning to the request. Unless the user asks for deeper analysis, begin with the current understanding, one recommendation, and no more than one important unresolved choice.
- Continue through safe, in-scope work when accepted direction or a stated reversible assumption is enough. Do not stop only to ask whether to continue.
- State assumptions so they can be corrected. Stop when uncertainty, conflict, or an authority boundary could significantly change the work.
- Let each source answer one clear question. Link to related sources instead of repeating their detail. A link does not change either source's authority, scope, loading, or lifecycle.
- Only this loader and recognized loaded `entrypoints` can define active `Axioms`. A loaded child `route` inherits its ancestors' Axioms. A child entrypoint adds only rules for its narrower scope.
- Follow loaded Axioms and Directives within their scope. Accepted workspace-specific content replaces corresponding Open Forge defaults for the same role, within its accepted scope. Report unresolved conflicts.
- Selected non-binding material for a narrower scope may specialize broader material of the same kind. Loaded binding instructions add to one another. They do not silently override one another. Report conflicts.
- Validation establishes whether evidence supports a claim. Acceptance establishes which knowledge or decisions may be treated as current within their scope. Restoring or moving a record does not establish acceptance by itself.
- Investigate an apparent #CurrentTruth conflict before changing either source. When accepted direction changes current state, update the source that defines it and keep useful prior context.

### Routing

#### Terms

- `route` - Navigable path exposed through `entrypoints` and `Entries`.
- `root route` - Route exposed directly by this loader.
- `slug` - Concrete folder name in a route path.
- `managed route` - Route whose declared manager may update known files as a lifecycle action. Management does not create runtime authority.

#### Rules

- An `entrypoint` lists its direct `routes` under `Entries`. Use descriptions, tags, paths, ancestor routes, and explicit links to decide what matters. Read an entrypoint before its children.
- Follow relevant branches recursively. Every folder in a route chain has exactly one recognized `entrypoint`.
- A `root route` exists only where this loader exposes it. It cannot be scoped or recreated inside another route.
- Any number of routed `slugs` may narrow a route below its root. A slug may appear before, between, or after deeper route segments. It narrows everything that follows it.
- A scope may contain only the `routes` useful there. It does not need to copy another scope or the installed defaults.
- Put specialized material in the narrowest useful scope. Use workspace-wide placement only when the material applies across the workspace.
- Each scoped `entrypoint` chooses the loading its contents justify. Keep entries on demand by default. Use #LoadNow only when missing the content would cost more than reading it whenever its parent loads. Use #KeepInMind only when that exposed content also needs refreshing to preserve continuity while its scope remains active.
- Keep separately selected scopes as separate chains and follow their explicit links. Recheck selection after an important task change. Do not load file bodies only to discover routes.
- Management affects file lifecycle only. It does not change runtime meaning or authority.
- Each manager defines the route shapes it recognizes, keeps their segment order through scopes, and changes only files it owns or can safely identify.
- A familiar `slug` or tag does not create root behavior or managed status. Users may add, move, replace, or remove routes. Other valid routes remain routable.
- Removed defaults stay removed unless the user asks to restore them.
- Generated `Entries` provide navigation only. Entries without a loading tag stay on demand.
- A user-owned `{name}.overwrite.md` companion loads immediately after `{name}.md`. It shares the base file's route, scope, and loading behavior. It is not indexed or selected separately.
- Interpret an overwrite as part of its base source, within that source's role and scope. When they answer the same question differently, the overwrite wins only for the corresponding content. It does not override unrelated sources.

### Tags And Loading

Defined tags keep the meanings below wherever they appear. Tags change what loads, when it loads, or how it is classified. They do not create authority. Other tags remain search and routing signals.

#LoadNow and #KeepInMind operate through loaded parent routes. Neither tag activates an otherwise unselected ancestor or scope.

#### Defined Tags

- #LoadNow - Read this `entry` in listed order when an already-loaded parent exposes it. If it is an `entrypoint`, apply the same rule to its `Entries`.
- #KeepInMind - Read exposed continuity context when its parent loads, then refresh it while its scope remains active.
  - Timing: Refresh at task start or resume, after context restoration, and before handoff or closeout.
  - Order: Read the tagged file and its adjacent overwrite. For an `entrypoint`, apply its child loading rules in listed order.
  - Boundaries: Refresh only active scopes. Do not load file bodies only to discover tagged files.
  - Recheck: Recheck during work only when the active continuity set may have changed.
- #Core - Base routing and loading, workspace orientation, and reusable agent-facing roles.
- #Memory - Self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history.
- #Extension - Optional packaged routes, capabilities, integrations, and support files.
- #Contextual - Useful context. Treat it as unaccepted unless applicable authority establishes acceptance within its scope.
- #CurrentTruth - Accepted current state within its stated scope, below user direction, runtime safety, platform constraints, and declared external sources.
- #Evergreen - Material that must stay aligned with accepted current state. It creates no authority or loading behavior.
  - Update affected #Evergreen material you may edit before work depends on it and no later than closeout. Batch related updates when safe.
  - Keep it aligned with what it represents now. Preserve useful prior context in the right Memory `route` and report blocked updates.

### CLI

Use each relevant command below when the Open Forge CLI is available. The plain files remain complete without the CLI.

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

<!-- open-forge:generated-index:start -->
- [Required instructions loaded through selected routes](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, tradeoffs, and work situations](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Concise maps to important local and external sources and when to use them](maps/_maps.md) - #LoadNow #Core #Map
- [Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history](memory/_memory.md) - #LoadNow #Memory #OrganicGrowth
- [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Specialized capabilities provided through native SKILL.md packages](skills/_skills.md) - #LoadNow #Core #Skill
- [Copy-ready files for starting independently maintained workspace content](templates/_templates.md) - #Core #Template
- [Repeatable Markdown recipes for reaching a defined goal](workflows/_workflows.md) - #LoadNow #Core #Workflow
<!-- open-forge:generated-index:end -->
