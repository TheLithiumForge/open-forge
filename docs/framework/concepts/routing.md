# Routing

## Description

This descriptor governs the shared routing model across Open Forge.

Routing lets agents discover relevant context without loading the entire workspace.

## Represents

A route identifies a destination and explains its relevance.

A category groups related routes under one category `entrypoint`. Open Forge-authored categories use `_{category}.md`; the CLI may recognize compatibility aliases for external tools.

A category `entrypoint` combines stable category meaning with generated navigation.

## Ownership

The loader contains the generated registry of active root routes and their one-line purposes.

Each category `entrypoint` owns its installed meaning, boundaries, authority, and generated `entries`.

Each framework descriptor governs its corresponding installed category file.

A routed destination owns its detailed truth.

The installed payload boundary is governed by `docs/framework/concepts/payload-boundary.md`.

Layer boundaries are governed by `docs/framework/concepts/layers.md`.

Directive, pattern, guidance, skill, and workflow semantics are governed by `docs/framework/concepts/agent-primitives.md`.

## Category Contract

Every active category `entrypoint` must define:

- what the category represents
- what its routed files represent
- when the category is relevant
- the authority of its contents
- how nested categories extend it
- its final generated index region

An installed category becomes active when its folder contains exactly one recognized category `entrypoint`. An Open Forge-authored category also requires an approved framework descriptor before it enters the payload.

The CLI must generate a loader `entry` for each direct active root route under `.agents/`.

The category `entrypoint` must expose a one-line description that provides enough meaning for an agent to decide whether to load the category without opening it first. Open Forge-authored categories use scoped frontmatter. Local categories may use supported metadata or their first body description.

Category placement and descriptions expose positive scope. Tags add compact scope signals such as domain, work type, topic, technology, and artifact.

Tags must not be the only indication of workspace-wide or mandatory behavior. Tags never create authority. Reserved load-policy tags create only the loading behavior defined in this concept and in the installed loader.

Reserved load-policy tags are #LoadNow and #KeepInMind. Layer tags such as #Core, #Memory, and #Extension are classification signals only. Built-in route type tags use singular PascalCase, such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace.

A primitive category may extend its own type recursively at any depth. Workflows may also own mixed local bundles of directives, patterns, guidance, and skills. Other category types reference root primitives instead of embedding mixed local scopes.

Within a recursively selected non-directive category, material in a narrower positive scope is preferred over broader material of the same primitive when safe and allowed. The category `entrypoint` owns any additional precedence rules for its contents.

Directive routes use scope without hidden precedence. Every direct directive file carries #LoadNow, so loading a directive `entrypoint` reads its direct files through the ordinary loaded-parent rule. Direct files under the baseline-loaded root directive route bind workspace-wide. Selecting and loading a positively described child directive route establishes the narrower scope before its direct #LoadNow entries are read. Child directives add to loaded ancestors; conflicts are reported rather than resolved through silent override. Directive bodies contain Axioms and no second applicability gate. Reading an inactive archive, example, or source payload for inspection does not activate that directive route.

Axioms of loaded ancestor `entrypoints` apply to all routes below them. A child `entrypoint` adds only what is specific to its scope and does not restate ancestor axioms. A missing or empty local Axioms section, or one declared as `inherited` or `none`, means no local additions; none of those forms disables an ancestor axiom.

## Route Contract

A route must identify:

- where the destination is
- what the destination represents
- enough context to determine relevance

Routes are navigation. They never replace the destination's detailed truth.

Generated route metadata never defines instructions, behavior, or authority. Only reserved load-policy tags may affect loading.

Generated category `entries` may point to direct markdown files, direct child category `entrypoints`, and standard `SKILL.md` files inside skills routes.

Complete workflow recipes carry exactly one primary development-phase tag for cheap wayfinding. Phase tags help infer the current state from the request and routed truth; they do not create physical phase routes, mandatory chronology, or authority over the workflow Goal.

## Load Tags

#LoadNow reads an entry relative to its already-loaded parent.

When an `entrypoint` is loaded, agents read each generated `entry` tagged #LoadNow, in listed order. If the target is a category `entrypoint`, only that `entrypoint` is read first; that child `entrypoint`'s own `entries` then apply the same routing contract. A hidden descendant never becomes visible merely because it carries #LoadNow.

#KeepInMind loads standing follow-up context as a complete routed catalogue.

At task start or resume, after actual context restoration, before handoff, and before closeout, agents read or recheck every routed #KeepInMind result and treat its follow-up instructions as binding within the authority of the owning content. They also refresh it at a transition when its follow-ups may have changed. When the CLI is available, `open-forge load --bodies` provides the complete effective baseline; plain traversal of generated route trees remains the fallback. A broken chain is a routing defect to repair, not permission to silently omit the result.

Load-policy tags do not create authority, scope, precedence, or a write requirement. #LoadNow autoload follows visible loaded-parent chains. #KeepInMind is the deliberate catalogue-wide continuity exception because lost follow-up context is most costly across long sessions and context restoration.

The loaded target still gets its meaning from its category and authored content.

## Path Contract

Generated `Entries` and authored `Required Routes` use `- [Description](relative/path.md) - #Tags`. Each link destination is concrete and resolves relative to the Markdown file containing it. A loader link therefore starts from `.agents/loader.md`, while a category or workflow link starts from that category `entrypoint` or workflow file.

The active workspace root is the directory whose `AGENTS.md` selected the loader. Resolving a Markdown link does not infer a different logical root from Git or submodule boundaries, and a relative destination that uses `..` must still resolve inside the active workspace.

CLI route arguments remain workspace-relative because they are command inputs rather than Markdown links. For example, `open-forge find --route .agents/workflows/dev/_dev.md` resolves from the selected target. CLI output may likewise use workspace-relative route identity. Security-sensitive CLI lookup, validation, indexing, installation, and extension writes require every consumed or mutated route to remain physically below that target and reject an externally resolving symlink or junction. A plain Markdown agent may follow an explicitly trusted external mount, but that is outside the CLI trust boundary.

## Scoped Routes

A `framework route` is an Open Forge core route with stable default meaning.

A `scope route` is a local route used to narrow meaning or ownership for routes below it. A `scope route` is created with a concrete `slug` folder and its own `entrypoint`.

A `scoped framework route` is a `framework route` initialized inside a `scope route`. It keeps the framework contract inside that scope unless a local edit or overwrite changes it.

A `slug` is the concrete folder segment used in a route path. Use `child route` when the relationship to a parent `entrypoint` matters. Use `slug` when the folder segment or route-template placeholder matters.

Every folder in a visible route chain needs an `entrypoint`. A deep file below a folder without an `entrypoint` is not reachable through generated routing.

A skill folder is different: the generated route points to its standard `SKILL.md`, and files below that folder are loaded only when the skill makes them relevant.

Route patterns may use placeholders such as `[scope]`, `[route]`, or `[state]` before install:

```text
memory/crystallized/documents/
memory/[scope]/crystallized/documents/
memory/crystallized/[scope]/documents/
memory/[scope]/crystallized/[scope]/documents/
```

The first pattern is an unscoped `framework route`. The others insert `scope routes` before, after, or between pinned `framework route` segments.

Installed workspaces contain concrete `slugs` only:

```text
memory/mobile-app/_mobile-app.md
memory/mobile-app/crystallized/_crystallized.md
memory/mobile-app/crystallized/documents/_documents.md
```

Open Forge does not pin typed grouping folders such as `projects/`, `domains/`, `teams/`, or `platforms/`. They are user-created `scope routes` when useful.

`Slug` placement changes meaning. A `slug` below a state route scopes material inside that state. A `slug` above a state route owns its own state routes. Both are valid when `entrypoints` make the scope clear.

Agents route through concrete paths and `entrypoint` content, not template syntax.

The CLI may identify `scoped framework route` `entrypoints` by known path shape and canonical `entrypoint` filename. It must not require hidden template or version metadata in installed framework files for this.

## Load Contract

Agents load routing layers in this order:

1. Load the loader; its authored axioms bind immediately.
2. Read generated `entries` tagged #LoadNow in listed order. Bind each loaded `entrypoint` before traversing its own `Entries`.
3. Read the complete routed #KeepInMind catalogue; each result binds within its owner's authority and remains active as follow-up context.
4. Let the current request select other relevant `entries` by path, description, and tags, binding each selected route as it is loaded.
5. Follow selected routes to the destinations that own detailed truth.
6. At context restoration, handoff, closeout, or a transition that may have changed follow-ups, recheck the complete routed #KeepInMind catalogue and perform its required actions.

Whenever a Markdown file has a user-owned `.overwrite.md` companion, read it immediately after the base. The overwrite inherits the base route, is never an independent generated entry, and has final precedence within that file's scope.

The CLI `load --bodies` command may batch the same plain traversal by emitting the loader, transitive visible #LoadNow closure, and complete #KeepInMind catalogue without entering on-demand parents. It adds each existing `.overwrite.md` after its base. The `chain` command may inspect inherited context for one route in loader-to-target order, adding an overwrite after its base and a skill's `SKILL.md` before an internal target. These commands automate traversal but do not define runtime meaning; installed files remain complete without them.

## Why

This model keeps the loader small, gives every category one authoritative meaning, supports recursive routing, and avoids a duplicated central registry.

Deterministic routing, validation, and visible memory increase the probability that a nondeterministic agent sees and follows the right context. They do not execute instructions or mechanically guarantee compliant behavior; review and evidence remain necessary.

## Alignment Checks

Routing is aligned when:

- the loader generates `entries` for direct active root routes only
- loader `entries` provide enough meaning to route by relevance
- every loader category has an installed `entrypoint`
- every Open Forge-authored category has a matching framework descriptor
- every category owns its detailed meaning in its `entrypoint`
- installed files provide enough meaning without governance descriptors
- generated `entries` remain navigation metadata plus reserved load policy
- reserved load-policy tags affect loading only
- #LoadNow reads visible baseline routes by default
- #KeepInMind reads the complete routed catalogue at task entry or resume, actual context restoration, handoff, closeout, and transitions where its follow-ups may have changed
- category placement and descriptions keep scope visible
- tags provide compact scope and classification signals
- layer tags and route type tags remain classification signals unless a loaded `entrypoint` defines more
- paths or descriptions expose mandatory and workspace-wide scope without relying on tags alone
- direct directive files carry #LoadNow; active directive routes settle scope before bodies are read, root direct files bind workspace-wide, and selected child direct files bind within their visible positive scope
- directive bodies add no second applicability gate, child directive scopes do not silently override ancestors, and inactive archive, example, or source inspection does not activate a directive
- workflow descriptions and tags support selection, phase tags remain non-waterfall wayfinding, and routed Goals own execution and completion
- generated `Entries` and authored `Required Routes` use concrete containing-file-relative Markdown links
- loader links resolve relative to `.agents/loader.md`
- CLI `--route` arguments and reported route identities remain workspace-relative
- repository and submodule boundaries do not change the logical active workspace root
- deterministic CLI reads and writes reject routes that escape the target through a symbolic link or junction
- `framework routes`, `scope routes`, `scoped framework routes`, and `slugs` have distinct meanings
- `scope routes` are concrete `slug` folders with `entrypoints`
- every folder in a visible route chain has its own `entrypoint`
- `scope routes` can appear before, after, or between pinned route segments
- installed workspaces do not contain route-template placeholders
- `scoped framework route` `entrypoints` can be identified by path shape and canonical `entrypoint` filename
- routed destinations own detailed truth
- skills route through their standard `SKILL.md` and keep their resources skill-owned
- nested categories use the same contract at every depth
- ancestor axioms apply within loaded route chains without restatement
- absent, empty, inherited, and none local Axioms declarations add nothing and never cancel ancestor axioms
- optional batched loading mirrors the visible #LoadNow closure plus complete effective #KeepInMind traversal
- optional deterministic chain inspection emits loader, ancestors, skills, targets, and user-owned overwrites in load order without defining runtime truth
- #LoadNow nested autoload exists only through loaded parent `entrypoints`
- standing #KeepInMind follow-ups remain complete even across context loss or a changed active route chain
- mixed local primitive bundles are limited to workflows
- workspace overwrites load immediately after their base files and have final precedence within that file's scope
- default Open Forge core `entrypoints` use #LoadNow
- default `memory/emerging/` and `memory/emerging/observations/` `entrypoints` use #KeepInMind
- narrower selected non-directive scopes take safe preference within the same primitive, while selected directive scopes add binding instructions and report conflicts
