# Workspace Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/workspace/_workspace.md`.

The workspace category `entrypoint` defines how agents discover important project locations, understand when to use them, and use generated navigation to workspace route files.

## Represents

The workspace category represents the navigation layer between the loader and the destinations that own workspace truth.

It supports a single project, a monorepo, multiple repositories, a document workspace, or a larger collection without imposing one route-file organization.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Workspace Contract

Workspace routes identify important destinations and explain their contents and relevance.

Routes cover the smallest set of destinations needed for reliable discovery. Route coverage is intentional rather than exhaustive.

Workspace route filenames, grouping, and nesting depth are owned by the workspace. A route file can represent one destination or a related group. Nested workspace categories extend routing to any useful depth.

A destination can be a file, folder, project, repository, system, or document set. The routed destination owns detailed truth.

The current request determines which routed files are loaded. Generated `entries` provide navigation metadata and reserved load policy only. They never define instructions or authority.

## Route Files

Each workspace route file must identify at least one destination and provide enough meaning for an agent to decide when that destination is relevant.

Compact route lists must use this shape:

```md
- {destination} - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Concrete paths use backticks and resolve from the active workspace root unless the route explicitly identifies another base.

Route files can use any clear filename. A workspace can keep several related routes in one file, split routes into separate files, or use nested categories. These organizations implement the same route contract.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct workspace route files and direct child workspace category `entrypoints`. Child `entrypoints` may use CLI compatibility aliases. The shared formatting governor owns naming, ambiguity handling, marker validation, legacy migration, and regeneration behavior.

## Used By

The loader registry exposes this category's path and meaning. Agents load it when the current request needs workspace routes.

Agents use its generated `entries` to choose relevant workspace route files, then follow those routes to the destinations that own detailed truth.

## Why

The workspace category gives agents durable orientation without requiring a complete project map.

Its organization scales through routed files and nested categories while remaining owned by the workspace rather than by an Open Forge project-layout assumption.

## Alignment Checks

The implementation is aligned when it:

- is named `_workspace.md`
- lives in `.agents/workspace/`
- includes `Core` and `Workspace` in scoped `open-forge:` tags
- defines selective workspace routing
- leaves route-file granularity to the workspace
- uses `entries` to select routes relevant to the current request
- supports nested categories at any useful depth
- keeps detailed truth at routed destinations
- contains no seeded project-layout assumptions
- places generated navigation last inside the required markers
- keeps compact workspace routing axioms
- keeps the authored portion between 10 and 40 non-empty lines
