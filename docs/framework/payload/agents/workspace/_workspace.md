# Workspace Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/workspace/_workspace.md`.

The workspace category entrypoint defines how agents discover important workspace destinations and provides generated navigation to workspace route files.

## Represents

The workspace category represents the navigation layer between the loader and the destinations that own workspace truth.

It supports a single project, a monorepo, multiple repositories, a document workspace, or a larger collection without imposing one route-file organization.

## Contains

The installed workspace category entrypoint must contain:

- scoped `open-forge:` frontmatter with description and tags
- a title
- one short category description
- compact workspace routing axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated entries do not count toward this limit.

## Workspace Contract

Workspace routes identify important destinations and explain their scope or relevance.

Routes cover the smallest set of destinations needed for reliable discovery. Route coverage is intentional rather than exhaustive.

Workspace route organization is owned by the workspace. A route file can represent one destination or a related group. Nested workspace categories extend routing to any useful depth.

A destination can be a file, folder, project, repository, system, or document set. The routed destination owns detailed truth.

The current request determines which routed files are loaded. Generated entries provide navigation metadata and never define instructions or authority.

## Route Files

Each workspace route file must identify at least one destination and provide enough meaning for an agent to decide when that destination is relevant.

Compact route lists must use this shape:

```md
- {destination} - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Concrete paths use backticks. Symbolic paths may omit backticks when they remain unambiguous.

Route files can use any clear filename. A workspace can keep several related routes in one file, split routes into separate files, or use nested categories. These organizations implement the same route contract.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct workspace route files and direct child workspace category entrypoints. Child entrypoints may use CLI compatibility aliases. The shared formatting governor owns naming, ambiguity handling, marker validation, legacy migration, and regeneration behavior.

## Used By

The loader registry exposes this category's path and meaning. Agents load it when the current request needs workspace routes.

Agents use its generated entries to choose relevant workspace route files, then follow those routes to the destinations that own detailed truth.

## Why

The workspace category gives agents durable orientation without requiring a complete project map.

Its organization scales through ordinary files and nested categories while remaining owned by the workspace rather than by an Open Forge project-layout assumption.

## Alignment Checks

The implementation is aligned when it:

- is named `_workspace.md`
- lives in `{forgePath}/workspace/`
- defines selective workspace routing
- leaves route-file granularity to the workspace
- supports nested categories at any useful depth
- keeps detailed truth at routed destinations
- contains no seeded project-layout assumptions
- places generated navigation last inside the required markers
