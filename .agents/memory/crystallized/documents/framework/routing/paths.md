---
open-forge:
  description: Current path contract for containing-file-relative Markdown destinations, concrete `slugs`, logical workspace identity, and deterministic containment
  responsibility: Define how Open Forge authored `route` paths resolve and constrain deterministic local access
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Paths, Workspace, CLI, Containment]
---

# Routing Paths And Identity

## Scope

This document is authoritative for the path bases and identities used by authored Open Forge `routes` and deterministic `route` interfaces.

The [routed Markdown contract](../markdown/routes.md) defines canonical line and filename syntax. The [CLI MVP Architecture](../../cli/mvp-architecture.md) owns current implementation details and safety mechanisms.

## Authored Markdown Destinations

Generated `Entries` and other semantic `route` references use concrete Markdown links whose destinations resolve relative to the file containing the link.

A loader `entry` therefore resolves from `.agents/loader.md`. A category `entry` resolves from its `entrypoint`. A workflow dependency resolves from the workflow file that declares it.

Semantic `entry` destinations identify files. They remain one whitespace-free path, percent-encode unsafe characters, and omit query strings or fragment anchors. Ordinary explanatory links may target headings or external URLs. Local ordinary links may leave `.agents` while remaining inside the selected workspace.

An authored `route` may use `..` when needed, but its local destination remains inside the active workspace. External systems are reached through an explicit routed document that describes the relationship rather than by making the generated `route` tree escape its workspace.

Ordinary Markdown heading fragments use the GitHub-compatible slug of visible
ATX heading text, including Unicode and deterministic numeric suffixes for
duplicate headings. A heading whose visible text cannot be established from
the supported inline syntax remains unverified instead of being guessed. This
fragment rule does not make ordinary links part of route selection or loading.

## Historical CLI-v2 Route Proposal

Deleted CLI v2 proposed that `route` arguments and reported identities use natural routed
segments rather than Markdown filenames:

```text
open-forge find workflows/dev
open-forge route inspect directives/cli-interface-consistency
```

`loader` identifies `.agents/loader.md`. Each later segment is the exact
routed file stem or child folder slug. Entrypoint filenames, `.agents`, and
`.md` remain representation details. Route identities are case-sensitive, use
`/`, and have no alternative path or extension form.

The proposal reports the canonical workspace-relative source beside the route
identity. A Markdown destination still resolves from its containing file; the
tool never interprets a natural route identity as that relative destination.

The proposed positional `<path>` operands that address Open Forge content
use one separate grammar. They begin with `.agents` or `./.agents` beneath the
selected workspace and normalize to the same canonical `.agents/...` path. A
command with no path operand owns its fixed default internally.
Other filesystem inputs use purpose-specific value grammars rather than
reinterpreting an Open Forge content path. This complete section is raw input,
not an accepted path contract for the new CLI.

## Historical CLI-v2 Workspace Proposal

CLI v2 proposed that the active workspace root is the exact directory supplied through
`--workspace`, or the exact current working directory when that flag is
omitted. The canonical workspace entry and loader are inspected state beneath
that root. They do not select, redirect, or discover it.

Git repositories, nested repositories, submodules, mounted source trees,
`AGENTS.md` prose, and nested `.agents` directories do not silently replace
that logical root. A caller selects another Framework source or Extension
payload by changing the workspace, then uses the same workspace-relative `.agents/...` content
path grammar. The new CLI's workspace discovery behavior remains unsettled.

## Concrete Paths

Installed workspaces contain concrete `slugs` and filenames. Placeholders in `route` templates are documentation, Template, Extension, or planning notation only.

Every intermediate folder that participates in a visible generated `route` path has one recognized `entrypoint`. Component-owned internal resources that are not exposed as `route` levels remain under their component's own navigation contract.

## Deterministic Containment

Security-sensitive deterministic reads, validation, indexing, installation, and managed writes keep every consumed or mutated local `route` physically below the selected target.

The current CLI rejects lexical traversal, physical symlink or junction escapes, unsafe aliases, and other path identities that could make a visible `route` operate outside its target. The [CLI MVP Architecture](../../cli/mvp-architecture.md#containment-and-identity) is authoritative for the complete current safety model.

Ordinary Markdown may describe or link an explicitly trusted external location, but such a relationship remains outside the trust boundary for managed local `routes`.

## Related Current Sources

- [Routing model](model.md)
- [Route scope and inheritance](scope.md)
- [Loading and continuity](loading.md)
- [Overwrite customization](overwrites.md)
- [Routed Markdown representation](../markdown/routes.md)
- [CLI MVP Architecture](../../cli/mvp-architecture.md)
- [Historical CLI-v2 evidence](../../../archived/cli-v2/_cli-v2.md)

## Decisions And Rationale

- [Routing model](../../../decisions/framework/routing-model.md)
- [Scope and slugs](../../../decisions/framework/scope-and-slugs.md)
- [Routing surfaces](../../../decisions/framework/routing-surfaces.md)
