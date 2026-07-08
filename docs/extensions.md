# Open Forge Extensions

Extensions are optional installable payloads that add files to the normal Open Forge route tree.

They do not create another framework root. They add #Core, #Memory, or #Extension material where those files naturally belong.

## Source Shapes

A local overlay is copied as-is:

```text
my-extension/
  .agents/
    patterns/
      react/
        _react.md
        components.md
```

A local or bundled extension package can keep maintainer metadata outside the installed payload:

```text
my-extension/
  extension.json
  README.md
  payload/
    .agents/
      skills/
        implementation/
          SKILL.md
```

When `payload/` exists, only `payload/` is installed.

Bundled first-party extensions use:

```text
src/extensions/{extension-id}/
  extension.json
  README.md
  payload/
    .agents/
      ...
```

`extension.json` is for CLI list/select display. Installed markdown remains runtime truth.

Extension payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` should keep native metadata and may be indexed with default route tags. Do not use #OpenForge in extension payloads; it is reserved for core framework routes. Use a reserved load-policy tag only when the extension intentionally adds baseline-loaded material.

## Install

```sh
open-forge extend
open-forge extend --list
open-forge extend --select [target]
open-forge extend --ids <id[,id...]> [target]
open-forge extend <extension-source-or-id> [target]
```

The CLI resolves a local folder first. If no local folder exists and the value is a valid bundled id, it installs the bundled first-party extension.

Interactive selection requires a TTY. Use `--ids` for scripts, CI, or unattended installs.

## Sharing

Avoid duplicating shared workflow or skill behavior.

For now, use one of these shapes:

- put shared files and related workflows in one extension pack
- create a shared extension pack and install it beside dependent packs
- keep a workflow pack standalone by shipping the minimal skills it needs

The current CLI does not resolve dependencies. First-party starter workflows therefore begin as one pack: `workflow-essentials`, which contains vision, architecture, implementation, and shared workflow skills.

## Tests

CLI extension tests should install into OS temp folders and inspect the resulting files and generated indexes.

Workflow tests are future work. They should exercise real installed payloads, not only source files.
