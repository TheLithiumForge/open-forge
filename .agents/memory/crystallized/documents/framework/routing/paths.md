---
open-forge:
  description: Current path contract for containing-file-relative Markdown destinations, workspace-relative CLI `routes`, concrete `slugs`, logical workspace identity, and deterministic containment
  responsibility: Define how Open Forge `route` paths resolve across authored Markdown and deterministic tool interfaces
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Paths, Workspace, CLI, Containment]
---

# Routing Paths And Identity

## Scope

This document is authoritative for the path bases and identities used by authored Open Forge `routes` and deterministic `route` interfaces.

The [routed Markdown contract](../markdown/routes.md) defines canonical line and filename syntax. The [CLI MVP Architecture](../../cli/architecture.md) owns current implementation details and safety mechanisms.

## Authored Markdown Destinations

Generated `Entries` and other semantic `route` references use concrete Markdown links whose destinations resolve relative to the file containing the link.

A loader `entry` therefore resolves from `.agents/loader.md`. A category `entry` resolves from its `entrypoint`. A workflow dependency resolves from the workflow file that declares it.

Semantic `entry` destinations identify files. They remain one whitespace-free path, percent-encode unsafe characters, and omit query strings or fragment anchors. Ordinary explanatory links may target headings or external URLs.

An authored `route` may use `..` when needed, but its local destination remains inside the active workspace. External systems are reached through an explicit routed document that describes the relationship rather than by making the generated `route` tree escape its workspace.

## CLI Route Identity

CLI `route` arguments and reported `route` identities are workspace-relative because they are command inputs and outputs rather than links embedded in another Markdown file.

For example:

```text
open-forge find --route .agents/workflows/dev/_dev.md
```

The selected target establishes the `route` base. The CLI does not reinterpret a containing Markdown file as the base for a command-line `route`.

## Active Workspace

The active workspace root is the directory whose canonical workspace entry selected the loader.

Git repositories, nested repositories, submodules, and mounted source trees do not silently replace that logical root. One Open Forge environment may deliberately route several repositories beneath the same selected workspace.

## Concrete Paths

Installed workspaces contain concrete `slugs` and filenames. Placeholders in `route` templates are documentation, Template, Extension, or planning notation only.

Every intermediate folder that participates in a visible generated `route` path has one recognized `entrypoint`. Component-owned internal resources that are not exposed as `route` levels remain under their component's own navigation contract.

## Deterministic Containment

Security-sensitive deterministic reads, validation, indexing, installation, and managed writes keep every consumed or mutated local `route` physically below the selected target.

The current CLI rejects lexical traversal, physical symlink or junction escapes, unsafe aliases, and other path identities that could make a visible `route` operate outside its target. The [CLI MVP Architecture](../../cli/architecture.md#containment-and-identity) is authoritative for the complete current safety model.

Ordinary Markdown may describe or link an explicitly trusted external location, but such a relationship remains outside the trust boundary for managed local `routes`.

## Related Current Sources

- [Routing model](model.md)
- [Route scope and inheritance](scope.md)
- [Loading and continuity](loading.md)
- [Overwrite customization](overwrites.md)
- [Routed Markdown representation](../markdown/routes.md)
- [CLI MVP Architecture](../../cli/architecture.md)

## Decisions And Rationale

- [Routing model](../../../decisions/routing-model.md)
- [Scope and slugs](../../../decisions/scope-and-slugs.md)
- [Routing surfaces](../../../decisions/routing-surfaces.md)
