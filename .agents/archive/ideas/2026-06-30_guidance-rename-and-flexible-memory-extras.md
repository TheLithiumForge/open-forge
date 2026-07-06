---
open-forge:
  description: Guidance rename completion and flexible placement for memory extras
  tags: [OpenForge, Guidance, Memory, Extension, CLI, Taxonomy]
---

# Guidance Rename And Flexible Memory Extras

## Guidance Rename

Implemented decision: the contextual-judgment primitive is named `guidance/`.

Old names:

- `guidelines/`
- `_guidelines.md`
- #Guideline

Current names:

- `guidance/`
- `_guidance.md`
- #Guidance

`guidance` better describes adaptable judgment, reasoning, and tradeoffs without implying mandatory behavior or human tutorial docs.

The rename must remain reflected in governance descriptors, installed payload files, README structure, CLI tests, generated indexes, and future migration/update behavior.

## Route Template Model

The flexible placement model should be described as route templates.

Template paths may include user-provided slug segments, such as:

```text
memory/crystallized/[scope]/documents/
memory/crystallized/[scope]/decisions/
memory/[scope]/crystallized/documents/
memory/[scope]/crystallized/decisions/
memory/[scope]/crystallized/[scope]/documents/
memory/[scope]/archived/
patterns/[scope]/[route]/
guidance/[scope]/[route]/
workflows/[scope]/[route]/
```

The exact bracket syntax is not yet final. `[scope]`, `[route]`, `[state]`, or another clear named notation can work in documentation and manifests as long as the CLI treats it as a named path parameter. Anonymous placeholders are not useful because the CLI needs a prompt label, validation rule, and scaffold content target.

Template placeholders are not installed into user workspaces. They exist for maintainers, extension authors, CLI prompts, and user-facing explanations. Installation resolves them to concrete folder names.

The CLI should accept concrete slugs supplied by the user and scaffold the resulting routed path when every segment has enough entrypoint meaning to be useful.

This model should apply beyond Memory where useful. Any route template must still resolve to routed folders and files after installation; agents should not need to understand template syntax at runtime.

## Scope Route Model

Scope routes are an extension of the routing model, not a separate hierarchy.

A scope route is a routed folder with a category entrypoint. Its concrete folder name is the slug. The entrypoint owns what that scope means.

There is no required folder named `scope`, `project`, `domain`, or `team`. Those can exist when a user wants typed grouping, but the base model is just a scope route.

Pinned framework segments are stable anchors:

```text
directives/
guidance/
memory/
patterns/
skills/
workflows/
workspace/

memory/working/
memory/emerging/
memory/crystallized/
memory/archived/
```

Users and extensions may insert scope routes around those pinned segments when a routed copy gives clearer ownership.

Zero extra scope is the default:

```text
.agents/memory/crystallized/documents/
.agents/patterns/
.agents/guidance/
.agents/directives/
```

A state-local scope keeps only one state scoped:

```text
.agents/memory/crystallized/mobile-app/
  _mobile-app.md
  documents/
    _documents.md
```

A memory-wide scope groups multiple memory states under one owner:

```text
.agents/memory/mobile-app/
  _mobile-app.md
  working/
    _working.md
  emerging/
    _emerging.md
  crystallized/
    _crystallized.md
  archived/
    _archived.md
```

Multiple scope layers are valid when each inserted folder has meaning:

```text
.agents/memory/customer-facing/mobile-app/
  _mobile-app.md
  crystallized/
    _crystallized.md
    documents/
      _documents.md
```

The same scoping model applies outside #Memory:

```text
.agents/patterns/mobile-app/react/
.agents/guidance/mobile-app/cross-platform-apps/
.agents/directives/mobile-app/database/
.agents/workflows/mobile-app/implementation/
```

These examples do not require `mobile-app` to mean "project." It is the slug for the scope described by `_mobile-app.md`.

## Scoped Framework Copies

When a pinned framework segment is copied under a scope, the copied entrypoint should keep the same framework contract as the unscoped entrypoint unless a local overwrite or explicit edit changes it.

Examples:

```text
.agents/memory/crystallized/_crystallized.md
.agents/memory/mobile-app/crystallized/_crystallized.md
.agents/memory/customer-facing/mobile-app/crystallized/_crystallized.md
```

All three represent the same framework route type in different scopes. If the framework updates the `_crystallized.md` wording, the CLI should be able to update every matching scoped framework route without asking the user to copy and paste the change.

Do not add template, source, or version metadata to installed framework files just to support updates.

Scoped framework routes should be identified from their concrete path and canonical entrypoint filename. A scoped framework route is identifiable when its path matches a known framework route shape, allowing scope routes between pinned framework segments, and the final entrypoint name matches the terminal pinned segment.

Examples:

```text
memory/crystallized/_crystallized.md
memory/mobile-app/crystallized/_crystallized.md
memory/customer-facing/mobile-app/crystallized/_crystallized.md
memory/crystallized/documents/_documents.md
memory/crystallized/mobile-app/documents/_documents.md
memory/customer-facing/mobile-app/crystallized/platform/documents/_documents.md
```

These match framework route shapes such as:

```text
memory/.../crystallized/_crystallized.md
memory/.../crystallized/.../documents/_documents.md
```

The same idea applies to other copied framework entrypoints in valid owners:

```text
workflows/implementation/directives/_directives.md
workflows/mobile-app/implementation/directives/_directives.md
workflows/mobile-app/implementation/skills/_skills.md
```

A user route such as `patterns/mobile-app/react/_react.md` is not automatically a scoped framework route just because it is an entrypoint. It is a user or extension route unless it matches a known framework entrypoint shape.

The CLI does not need hidden versioning to update framework-owned files. It can install the current framework wording over every recognized scoped framework route entrypoint and let git show the diff. Users who manually edited framework files can review the diff and reapply or discard their edits. Users who want durable local changes should prefer sibling files, child routes, or `.overwrite.md` companions.

The update command should still be review-friendly: support dry-run or preview output, preserve `.overwrite.md` companions, regenerate only generated regions through the indexer, and never rename user paths without explicit approval.

## Slugified Category Paths

The route-template model depends on stable path segments.

Open Forge already behaves this way implicitly: root category names are stable, and users can add any routed folder they want below them. The CLI should make this explicit by treating user-created and extension-created category folder names as slugs by default.

Potential slug contract:

- canonical framework roots stay pinned and are not auto-renamed
- user and extension scope segments use stable lowercase kebab-case by default
- display names live in titles, descriptions, or frontmatter, not in folder names
- extension manifests target concrete slug paths or route templates with named slug segments
- the CLI validates or proposes slugs instead of silently inventing unclear paths
- existing user paths are not renamed without explicit user approval
- slug collisions require user review

Examples:

```text
memory/crystallized/mobile-app/documents/
memory/mobile-app/crystallized/documents/
memory/customer-facing/mobile-app/crystallized/documents/
patterns/react-server-components/
guidance/cross-platform-apps/
```

Slugification must remain a routing stability rule, not an authority rule. A slug makes routes predictable for users, agents, CLI updates, and extensions; the category entrypoint still owns the route's meaning.

## Filesystem Slugs Versus Manifest Slugs

Installed and routed material should use concrete filesystem folder names as the source of truth.

Open Forge routing already works through concrete paths, entrypoints, and generated entries. Keeping slugs in the filesystem makes routes visible to humans, agents, `rg`, git diffs, Obsidian-style graph/search tools, and simple CLI operations without requiring a hidden registry.

A JSON-style manifest is still useful for extension and template packaging, but it should not become runtime routing truth.

Good manifest responsibilities:

- declare route templates before install
- define named slug parameters and display labels
- provide scaffold content for missing ancestor category entrypoints
- declare aliases, migrations, compatibility notes, and provenance
- preview what concrete paths the CLI will create

Poor manifest responsibilities:

- override installed route meaning after install
- make agents resolve category identity through a registry
- hide important routing truth away from the files the user reads
- duplicate path slugs in frontmatter unless needed for alias or migration support

Preferred hybrid:

1. Author #Core and #Extension payloads with slug-safe paths directly.
2. Let extension manifests describe templates and installation inputs.
3. Expand templates into routed folders and files during install.
4. Treat installed paths plus entrypoint content as the runtime truth.

In practice, the template may say `[scope]`, but the installed path says `mobile-app`, `billing-api`, or another concrete slug chosen by the user or extension installer.

Example manifest shape to explore:

```json
{
  "routes": [
    {
      "template": "memory/[scope]/crystallized/documents",
      "params": {
        "scope": {
          "label": "Scope",
          "slug": "lowercase-kebab-case"
        }
      },
      "entrypoints": {
        "memory/[scope]": {
          "descriptionTemplate": "{scope} memory routes.",
          "tags": ["Memory", "Index"]
        },
        "memory/[scope]/crystallized": {
          "descriptionTemplate": "{scope} accepted durable memory and current truth.",
          "tags": ["Memory", "Crystallized", "Index", "CurrentTruth"]
        }
      }
    }
  ]
}
```

The exact manifest format is not decided. The important rule is that agents should not need this manifest at runtime; the CLI uses it to create explicit files that become readable local truth.

## CLI Slugged Path Scaffolding

Future CLI work should support generating concrete routed paths from user intent.

Example intents:

- "create a new scope route named Mobile App"
- "add crystallized memory for Billing API"
- "add working, emerging, and crystallized memory routes for Design System"
- "add scoped documents under this memory route"

The CLI should:

- ask for or accept a human display name
- propose a stable slug such as `mobile-app`
- show the concrete paths it will create
- create only filesystem folders and entrypoints
- generate missing ancestor entrypoints only when it has meaningful entrypoint content
- rebuild generated indexes after creation
- detect collisions and ask before reusing or changing an existing route
- preserve existing user paths unless the user explicitly approves a rename

Template placeholders such as `[scope]`, `[route]`, and `[state]` remain CLI, maintainer, extension-author, and documentation notation only. User workspaces receive concrete folders.

This should support both broad route creation and targeted Memory helpers. For example, a user could ask for scoped #Memory and choose which states or extras to create instead of manually spelling every folder path.

Open design questions:

- Should this be one generic scaffold command, a wizard, or both?
- Should common intents such as `scope`, `memory`, or `crystallized-memory` be named presets?
- How much entrypoint text should the CLI ask for versus generate from a reviewed template?
- How should the CLI preview a path tree before writing files?

## CLI Extension Template Authoring

Future CLI work should also support generating extension templates for maintainers.

This is separate from installing an extension. It helps Open Forge maintainers and third-party maintainers create shareable extension source folders with the right structure.

Possible output shape:

```text
src/extensions/{extension-id}/
  extension.json
  payload/
    .agents/
      ...
```

The extension authoring CLI should eventually help create:

- extension metadata
- route templates with named slug parameters
- concrete payload files
- scaffold content for missing ancestor entrypoints
- preview data for install commands
- compatibility and migration notes

This also helps us build Open Forge's own future extensions without hand-writing every package shape.

Open design questions:

- Is an extension template just a route-template package, or a stronger product unit?
- Which manifest fields are required before an extension is installable?
- Should the CLI generate example payload files, empty entrypoints, or both?
- How should extension authors test install, update, and removal locally?

## Loader Terminology And Axiom Consistency

Accepted terms:

- framework route - Open Forge-owned route with stable default meaning
- scope route - local route used to narrow meaning or ownership for routes below it
- scoped framework route - framework route initialized inside a scope route
- slug - concrete folder segment used in a route path
- child route - route exposed by a parent entrypoint

The installed loader should define these terms briefly enough for humans and agents to understand the routing model without reading maintainer docs.

Runtime examples may use `[scope]` notation when they explicitly say it is a placeholder for a concrete slug folder with its own entrypoint. Generated entries and installed paths must stay concrete.

Lower-priority consistency pass: reorder axioms across installed files so they follow a predictable order, such as definition, relevance/loading, routing, authority, lifecycle, conflict/customization, and generated entries. Do this later as its own reviewable pass, because it touches many files.

Future CLI vision work should use the current CLI docs as the honest baseline: today the CLI installs managed files, updates recognized scoped framework routes, and rebuilds indexes; later product work should define scaffold, preview, wizard, extension, and upgrade capabilities from that baseline.

## Flexible Memory Extras

Memory root states should remain pinned:

- `working/`
- `emerging/`
- `crystallized/`
- `archived/`

P14 decision: do not add `references/` for now. Keep `archived/` as a root Memory state. Install `decisions/` under `crystallized/` by default because decisions are accepted rationale, not a separate lifecycle state.

Some child routes are useful as shipped or optional extras, but their best placement depends on the workspace. Examples include archive child routes, tasks, backlog, project records, and workflow outputs.

The CLI should eventually support installing or scaffolding those extras in concrete routed locations when the route meaning allows it. Scope comes from scope routes and their entrypoints.

Examples:

```text
memory/crystallized/documents/
memory/crystallized/decisions/
memory/crystallized/[scope]/documents/
memory/crystallized/[scope]/decisions/
memory/[scope]/crystallized/documents/
memory/[scope]/crystallized/decisions/
memory/[scope]/archived/
```

The goal is not to prescribe these shapes. The goal is to let a user choose where a route belongs while keeping every routed folder explicit, indexed, and understandable. Every visible slug folder in a route template implies a concrete entrypoint after install.

Slug placement changes meaning:

- `memory/crystallized/mobile-app/decisions/` means the `mobile-app` scope exists inside crystallized memory.
- `memory/mobile-app/crystallized/decisions/` means the `mobile-app` scope owns its own Memory states.

Both are valid when their entrypoints make the scope clear. User documentation should explain this difference with examples.

Decisions represent accepted rationale for meaningful choices that may need to be understood later. A decision about a #Core directive, pattern, workflow, or workspace route belongs in #Memory as crystallized rationale; the current behavior itself belongs in the matching #Core route.

Archived memory represents useful history that is no longer current. Archive remains a state, not a child type of crystallized memory. Archive child routes derive scope from slug placement; active routes should not hide stale material as current truth.

Deferred idea: archived files might later use a small origin block with fields such as origin path, archived date, replacement, and reason. Do not require this yet; preserve it only as a future formatting/helper idea.

Deferred CLI idea: support two upgrade modes. A forceful upgrade adds and overwrites all framework-owned files. A softer upgrade updates existing framework-owned files but does not re-add older optional/default files that the user deleted, while still adding genuinely new required framework files. This needs clear ownership rules before implementation.

## External Or Distributed Memory Roots

Idea to explore: allow Memory to live outside `.agents/`, either as a workspace-level sibling such as `.memory/` or as scoped memory roots inside project/package/application folders.

Possible shapes:

```text
.agents/
  loader.md
  memory/

.memory/
  _memory.md
  crystallized/
  emerging/
  working/
  archived/

packages/api/.memory/
  _memory.md
  crystallized/
  emerging/
```

Potential benefit: memory becomes visually separate from agent framework files and can feel more like a human-readable knowledge/documentation place. In monorepos or multi-package workspaces, local `.memory/` roots could keep package-specific memory beside the code it describes instead of recreating every package name under one central memory tree.

Potential risk: memory outside `.agents/` can become invisible to agents unless it is explicitly routed from the loader, workspace routes, or a loaded entrypoint. Distributed memory roots can fragment current truth, make promotion/archive behavior harder, and create competing local histories unless each root states its scope and relationship to broader #Memory.

Current bias: do not move default #Memory outside `.agents/` yet. Keep central `.agents/memory/` as the canonical installed route. Treat external or distributed memory roots as a future user-documentation pattern first, not product behavior. A user can create `.memory/`, `docs/`, or another local knowledge folder whenever they want, but they must route it explicitly through `AGENTS.md`, a workspace route, or another already loaded entrypoint. Later CLI or extension support may make this easier. No implicit filesystem search should be required.

## Boundary To Explore

Pinned routes are framework anchors. They should be few and stable.

Flexible extras are route templates. They can be installed at the root route, under a direct scope route, under a typed grouping route, under a custom `[scope]`, or under any owning route when that placement makes truth ownership clearer.

The CLI must not create meaningless ancestor categories. If a requested target path has missing category entrypoints, the operation must get scaffold content from the user, from an extension manifest, or from a reviewed route template.

## Typed Grouping Question

Typed grouping routes such as `projects/`, `platforms/`, `packages/`, `domains/`, or `teams/` are attractive for some workspaces, but they should not be base assumptions.

Open questions:

- Should typed grouping routes be optional CLI presets instead of framework defaults?
- Should the CLI ask whether an extra belongs at the root route, under a direct scope route, or under a typed grouping route?
- Should typed grouping routes be scope routes with their own entrypoints and no special authority?
- How should single-scope work avoid unnecessary container folders?

Current bias: do not pin typed grouping routes in the base payload. Treat them as optional user-created or extension-created routes when they improve navigation.

## Random Current Truth

Users need a place for accepted current truth that is not obviously architecture, vision, PRD, research, external reference material, or another formal document type.

Possible answers to explore:

- direct files under the owning crystallized route
- a user-created `facts/`, `notes/`, `knowledge/`, or `context/` route
- files under `documents/[scope]/`
- a later `references/` route for research-heavy accepted source summaries and pointers
- a CLI-created custom category with a user-supplied name and description

Current bias: avoid a default vague bucket. If a truth file feels random, the CLI and user docs should encourage the smallest named scope that explains why it exists and how it stays current.

## Evergreen Maintenance

Crystallized memory must avoid becoming stale documentation.

Future user docs or extensions should explain that crystallized files are current truth. When reality changes, update, split, merge, supersede, or archive them instead of appending old notes forever.
