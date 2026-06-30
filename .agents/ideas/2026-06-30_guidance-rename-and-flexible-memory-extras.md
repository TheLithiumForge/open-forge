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
memory/crystallized/projects/[project]/documents/
memory/[state]/[scope]/decisions/
memory/[state]/[scope]/archived/
```

The exact bracket syntax is not yet final. `[slug]`, `{slug}`, or another clear notation can work in documentation and manifests as long as the CLI treats it as a named user-provided path segment.

Template placeholders are not installed into user workspaces. They exist for maintainers, extension authors, CLI prompts, and user-facing explanations. Installation resolves them to ordinary concrete folder names.

The CLI should accept concrete slugs supplied by the user and scaffold the resulting routed path when every segment has enough category metadata to be meaningful.

This model should apply beyond Memory where useful. Any route template must still resolve to ordinary routed folders and files after installation; agents should not need to understand template syntax at runtime.

## Slugified Category Paths

The route-template model depends on stable path segments.

Open Forge already behaves this way implicitly: root category names are stable, and users can add any routed folder they want below them. The CLI should make this explicit by treating user-created and extension-created category folder names as slugs by default.

Potential slug contract:

- canonical framework roots stay pinned and are not auto-renamed
- user and extension scope segments use stable lowercase kebab-case by default
- display names live in titles, descriptions, or frontmatter, not in folder names
- extension manifests target concrete slug paths or route templates with named slug segments
- the CLI validates or proposes slugs instead of silently inventing unclear folder names
- existing user paths are not renamed without explicit user approval
- slug collisions require user review

Examples:

```text
memory/crystallized/projects/mobile-app/documents/
memory/crystallized/platform/cross-platform/
patterns/react-server-components/
guidance/cross-platform-apps/
```

Slugification must remain a routing stability rule, not an authority rule. A slug makes paths predictable for users, agents, CLI updates, and extensions; the category entrypoint still owns the route's meaning.

## Filesystem Slugs Versus Manifest Slugs

Installed and routed material should use concrete filesystem folder names as the source of truth.

Open Forge routing already works through concrete paths, entrypoints, and generated entries. Keeping slugs in the filesystem makes routes visible to humans, agents, `rg`, git diffs, Obsidian-style graph/search tools, and simple CLI operations without requiring a hidden registry.

A JSON-style manifest is still useful for extension and template packaging, but it should not become runtime routing truth.

Good manifest responsibilities:

- declare route templates before install
- define named slug parameters and display labels
- provide metadata for missing ancestor category entrypoints
- declare aliases, migrations, compatibility, versioning, and provenance
- preview what concrete paths the CLI will create

Poor manifest responsibilities:

- override installed route meaning after install
- make agents resolve category identity through a registry
- hide important routing truth away from the files the user reads
- duplicate path slugs in frontmatter unless needed for alias or migration support

Preferred hybrid:

1. Author #Core and #Extension payloads with slug-safe paths directly.
2. Let extension manifests describe templates and installation metadata.
3. Expand templates into ordinary routed folders and files during install.
4. Treat installed paths plus entrypoint content as the runtime truth.

In practice, the template may say `[project]`, but the installed path says `mobile-app`, `billing-api`, or another concrete slug chosen by the user or extension installer.

Example manifest shape to explore:

```json
{
  "routes": [
    {
      "template": "memory/crystallized/projects/[project]/documents",
      "params": {
        "project": {
          "label": "Project",
          "slug": "lowercase-kebab-case"
        }
      },
      "entrypoints": {
        "memory/crystallized/projects": {
          "description": "Project-scoped crystallized memory routes.",
          "tags": ["Memory", "Index"]
        },
        "memory/crystallized/projects/[project]": {
          "descriptionTemplate": "{project} project memory.",
          "tags": ["Memory", "Project", "Index"]
        }
      }
    }
  ]
}
```

The exact manifest format is not decided. The important rule is that agents should not need this manifest at runtime; the CLI uses it to create explicit files that become readable local truth.

## CLI Slugged Path Scaffolding

Future CLI work should support generating concrete slugged routed paths from user intent.

Example intents:

- "create a new project route named Mobile App"
- "add crystallized memory for Billing API"
- "add working, emerging, and crystallized memory routes for Design System"
- "add project-scoped documents under this memory route"

The CLI should:

- ask for or accept a human display name
- propose a stable slug such as `mobile-app`
- show the concrete paths it will create
- create only ordinary filesystem folders and entrypoints
- generate missing ancestor entrypoints only when it has meaningful metadata
- rebuild generated indexes after creation
- detect collisions and ask before reusing or changing an existing route
- preserve existing user paths unless the user explicitly approves a rename

Template placeholders such as `[project]` remain CLI, maintainer, extension-author, and documentation notation only. User workspaces receive concrete folders.

This should support both broad route creation and targeted Memory helpers. For example, a user could ask for project-scoped #Memory and choose which states or extras to create instead of manually spelling every folder path.

Open design questions:

- Should this be one generic scaffold command, a wizard, or both?
- Should common intents such as `project`, `memory`, or `crystallized-memory` be named presets?
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
- scaffold metadata for missing ancestor entrypoints
- preview data for install commands
- compatibility and migration notes

This also helps us build Open Forge's own future extensions without hand-writing every package shape.

Open design questions:

- Is an extension template just a route-template package, or a stronger product unit?
- Which manifest fields are required before an extension is installable?
- Should the CLI generate example payload files, empty entrypoints, or both?
- How should extension authors test install, update, and removal locally?

## Flexible Memory Extras

Memory root states should remain pinned:

- `working/`
- `emerging/`
- `crystallized/`
- `archived/`

Some child routes are useful as shipped or optional extras, but their best placement depends on the workspace. Examples include documents, references, decisions, scoped archives, tasks, backlog, project records, and workflow outputs.

The CLI should eventually support installing or scaffolding those extras in either global or scoped locations when the route meaning allows it.

Examples:

```text
memory/crystallized/documents/
memory/crystallized/projects/[project]/documents/
memory/crystallized/[scope]/documents/
memory/[state]/[scope]/decisions/
memory/[state]/[scope]/archived/
```

The goal is not to prescribe these shapes. The goal is to let a user choose where a route belongs while keeping every routed folder explicit, indexed, and understandable.

## Boundary To Explore

Pinned routes are framework anchors. They should be few and stable.

Flexible extras are route templates. They can be installed globally, under a project scope, under a custom `[scope]`, or under any owning route when that placement makes truth ownership clearer.

The CLI must not create meaningless ancestor categories. If a requested target path has missing category entrypoints, the operation must get scaffold metadata from the user, from an extension manifest, or from a reviewed route template.

## Projects Question

A `projects/` route is attractive for multi-project work, but it may not belong everywhere by default.

Open questions:

- Should `projects/` be a common optional route under `memory/crystallized/documents/`?
- Should it be allowed under any memory state when the user wants project-scoped working, emerging, crystallized, or archived memory?
- Should the CLI ask whether an extra is global, project-scoped, or custom-scoped instead of shipping a default project taxonomy?
- How should single-project work avoid the awkward difference between `project/` and `projects/`?

Current bias: do not pin `projects/` in the base payload. Treat it as an optional scoped route that user docs and the CLI can offer when useful.

## Random Current Truth

Users need a place for accepted current truth that is not obviously architecture, vision, PRD, research, external reference material, or another formal document type.

Possible answers to explore:

- direct files under the owning crystallized route
- a user-created `facts/`, `notes/`, `knowledge/`, or `context/` route
- scoped files under `documents/{scope}/`
- a dedicated `references/` route for accepted source/resource summaries and pointers
- a CLI-created custom category with a user-supplied name and description

Current bias: avoid a default vague bucket. If a truth file feels random, the CLI and user docs should encourage the smallest named scope that explains why it exists and how it stays current.

## Evergreen Maintenance

Crystallized memory must avoid becoming stale documentation.

Future user docs or extensions should explain that crystallized files are current truth. When reality changes, update, split, merge, supersede, or archive them instead of appending old notes forever.
