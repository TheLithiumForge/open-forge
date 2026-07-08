# Extensions

## Description

This descriptor governs first-party and local Open Forge extension mechanics.

Extensions add optional routed files to an installed workspace without changing the base framework contract.

## Represents

An extension represents optional installable #Core, #Memory, or #Extension material.

An extension can contain workflows, skills, patterns, guidance, directives, memory routes, workspace routes, or support files when those routes belong in the normal Open Forge tree.

Extension payload routes use #Extension plus the route type and useful scope tags when the file format is Open Forge-authored. They must not use #OpenForge; use a reserved load-policy tag only when the extension intentionally adds baseline-loaded material.

## Source Contract

A first-party extension lives under `src/extensions/{extension-id}/`.

It may contain maintainer files such as `extension.json` and `README.md`, but installable runtime content must live under `payload/`.

Only `payload/` is copied when the CLI installs a package-shaped extension. Installed markdown remains runtime truth.

Extension payload route files should use #Extension metadata by default when that metadata does not break a native runtime format. Runtime-native files such as `SKILL.md` keep native metadata and may be indexed with default route tags. #OpenForge is reserved for core framework routes so installed extensions do not become permanent baseline context by accident.

`extension.json` may help CLI list and select bundled extensions. It must not be required by agents at runtime.

## Install Contract

The CLI resolves extension arguments in this order:

1. existing local directory
2. bundled first-party extension id

A local directory with `payload/` installs only `payload/`. A local directory without `payload/` is treated as a direct overlay.

Multiple bundled ids can be installed in one command. Index generation runs once after all selected payloads are copied.

Interactive selection is a convenience for TTY users. Unattended use must have a deterministic id list.

## Sharing Contract

Shared behavior must not be duplicated when a small shared route or shared extension pack is clearer.

Until dependency handling exists, first-party extensions must either:

- be standalone
- group related workflows and shared skills in one pack
- clearly require the user to install another shared pack

The first starter extension uses one pack so shared workflow skills are installed once.

## Why

Extensions keep the default install small while letting Open Forge offer useful workflows, skills, and pattern packs without forcing them on every workspace.
