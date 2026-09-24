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

### Responsibility And Shape

- The loader is the first Open Forge `entrypoint` after the canonical workspace entry.
- It contains only the terms and `Axioms` needed to enter, route, and understand an installed Framework. It also contains compact CLI help and a generated list of direct `root routes`.
- Its authored sections remain, in order, the introduction, `Terms`, one grouped `Axioms` section, and `Entries`.
- The `Axioms` groups remain, in order, `Authority And Inheritance`, `Routing`, `Tags And Loading` with `Defined Tags`, and `CLI` with `Applicable Commands`.
- It lets each source answer one clear question and links to related sources instead of repeating their detail. A link does not change authority, scope, loading, or lifecycle.
- Its short grouped lists keep related operational rules adjacent.
- Category-specific behavior stays in routed sources. Workflow execution, Memory transitions, primitive details, Extension mechanics, and complete CLI behavior do not move into the loader.
- The installed loader remains understandable without repository governance, source history, or the CLI.

### Authority And Inheritance

- Role and scope determine authority. File order alone does not.
- The loader distinguishes evidence validation from acceptance and preserves scoped acceptance when classifying contextual material. The [accepted-state contract](../../../framework/truth.md#acceptance) defines the distinction.
- Platform constraints and runtime safety bound every action. Clear user direction sets task goals, priorities, important choices, and accepted changes. A declared external source remains authoritative for the facts assigned to it.
- Explanation and planning match the request. Unless the user asks for deeper analysis, the first response gives the current understanding, one recommendation, and no more than one unresolved important choice.
- The agent continues when accepted direction or a stated reversible assumption makes progress safe. It states assumptions and stops when uncertainty, conflict, or an authority boundary could significantly change the work.
- Only the loader and recognized loaded `entrypoints` define active `Axioms`. Child routes inherit ancestor Axioms and add only rules specific to their narrower scope. Authored scopes without local rules use the inherited marker; missing or empty local sections remain valid input.
- Clear direction is not reconfirmed. A request to act allows routine, reversible, in-scope choices needed to complete the task. The user decides unresolved choices that could significantly change the result, scope, risk, cost, external effects, or ability to undo the work.
- Apparent #CurrentTruth conflicts are investigated before either source changes. Accepted changes update the source that defines current state and keep useful prior context.
- Loaded Axioms and Directive Instructions apply within their scope. Editing or replacing a default, or adding its overwrite companion, changes that source. A separate file does not gain precedence over other active rules. Unresolved conflicts are reported.
- Narrower selected non-binding material may specialize broader material of the same kind. Loaded binding instructions add to one another and report conflicts instead of silently overriding one another.

### Routing And Loading

- The loader defines an `axiom` only as a mandatory instruction under an `Axioms` heading in the loader or a recognized loaded `entrypoint`, keeps general file terms in `Terms`, and defines only four routing terms locally: `route`, `root route`, `slug`, and `managed route`.
- `description` helps a reader decide whether to open a file and remains the pre-load route-selection surface. Optional `responsibility` helps an editor decide what belongs in the file by stating what it defines. It creates no authority or loading behavior.
- The routing section separates short `Terms` and universal `Rules`.
- A `root route` exists only where the loader exposes it; it cannot be scoped or recreated inside another `route`.
- Any number of routed `slugs` may narrow a route below its root. A slug may appear before, between, or after deeper route segments and narrows everything that follows it.
- A scope contains only the routes useful there. It does not need to copy another scope or the installed defaults.
- Specialized material uses the narrowest useful scope. Workspace-wide placement is reserved for material that applies across the workspace
- Each scoped entrypoint keeps content on demand by default. #LoadNow is justified only when omission costs more than reading on each parent load. #KeepInMind also requires a need to read exposed content again at the defined refresh points. Scope alone implies no loading tag
- Selection uses visible paths, descriptions, tags, ancestor routes, and explicit links. It follows relevant branches recursively, keeps separately selected scopes as separate chains, and rechecks them after an important task change. It does not load file bodies only to discover routes.
- Scoping preserves deeper `route` order and meaning. Manager-declared `route` segments retain their order through every scope.
- A familiar `slug` or tag alone creates neither root behavior nor managed status.
- Each declared manager identifies the `route` shapes it recognizes and changes only files it owns or safely identifies.
- Users may add, move, replace, or remove `routes`. Any `route` outside a manager's declared shapes remains generically routable. Record intentional removal of managed files in the manager's exclusions before updating; excluded defaults stay absent until restoration is explicitly requested.
- Generated `Entries` remain navigation metadata. Detailed meaning comes from the routed destination or the authoritative source it identifies.
- Loading and tags change visibility, timing, or classification without creating authority.
- The loader remains authoritative for the meanings of #LoadNow, #KeepInMind, #Core, #Memory, #Extension, #Contextual, #CurrentTruth, and #Evergreen. Its #Memory definition identifies self-growing Markdown state without moving Memory mechanics into the loader. Undefined tags remain routing and search signals.
- #LoadNow and #KeepInMind operate through loaded parent routes for both entrypoints and other files. Neither activates an unselected ancestor or scope. Read the tagged context when its parent loads, then refresh it at the defined boundaries while its scope remains active. Base files precede their adjacent overwrites, and entrypoints apply their child loading rules in listed order.
- Every recovered result retains the authority and scope established by its `route` and content.
- A user-owned `{name}.overwrite.md` is not an independent `route` and loads immediately after its base.
- It is interpreted as part of the base source, within that source's role and scope, and inherits its route and loading behavior. It is neither indexed nor selected independently. Its precedence applies only to corresponding base content and does not override unrelated authority.

### Deterministic Assistance

- CLI commands automate the complete plain-file contract and never become prerequisites for ordinary inspection.
- The loader names the compact CLI surface useful for entering and checking a workspace: `--help`, `context`, `route list`, `route inspect`, `find`, `references`, `index`, `status`, and `doctor`.
- `index` rebuilds generated `Entries` after routed files or route metadata change. `doctor` diagnoses workspace, routes, references, lifecycle, and recovery without changes after Framework structure changes and before closeout.
- Complete CLI help, installation lifecycles, unimplemented commands, and component internals stay outside the loader.

### Distribution And Generation

- Source and dogfood authored loader content remain identical.
- Source and CLI Maintenance define which `entrypoints` Open Forge ships, updates, restores, and validates. Extension contracts define their own managed files. Management remains a lifecycle relationship rather than a runtime `route` type.
- The heading-owned `Entries` body is derived locally and may differ when the installed `root routes` differ.
- Generated loader `entries` expose direct active root `entrypoints` only. They never flatten nested `routes`.
- Generated links resolve relative to `.agents/loader.md`; CLI `route` identities remain workspace-relative.
- The authored loader stays within 35 to 80 non-empty lines unless an accepted Framework change explicitly revises that review budget.

## Verification

- The frozen `git show c4428a90:src/cli-mvp/cli.closure.test.ts` remains legacy-only evidence for source and dogfood authored parity. It does not verify replacement command behavior.
- Replacement [`CliProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/CliProcessTests.cs) verifies root and route-family help plus the read-only `route list` interface.
- Replacement [`PublishedContextProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextProcessTests.cs) verifies `context` help, startup and selected context, and read-only results.
- Replacement [`PublishedRouteInspectProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInspectProcessTests.cs) verifies `route inspect` help, route facts, and read-only results.
- Replacement [`PublishedFindProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindProcessTests.cs), [`PublishedReferencesProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedReferencesProcessTests.cs), and [`PublishedIndexProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedIndexProcessTests.cs) verify search, references, and generated-navigation interfaces.
- Replacement [`PublishedStatusProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedStatusProcessTests.cs) and [`PublishedDoctorProcessTests.cs`](../../../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorProcessTests.cs) verify status and doctor help, result streams, and read-only behavior.
- Review the source and dogfood authored line count, exact alignment, and unchanged generated Entries whenever the loader contract changes.
