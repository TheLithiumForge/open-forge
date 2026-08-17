# Developing Open Forge

This guide describes the current repository transition. Start with the
[README](../README.md) when you want to use the Framework.

## Enter The Workspace

Read `AGENTS.md` and `.agents/loader.md` before changing Open Forge. Use the
[Sources Of Truth map](../.agents/maps/sources-of-truth.md) to locate the source
that defines each affected question.

Current responsibilities are:

| Source                                | Responsibility                                                |
| ------------------------------------- | ------------------------------------------------------------- |
| `src/open-forge/`                     | Installable Framework wording                                 |
| `src/extensions/`                     | First-party Extension packages                                |
| `.agents/`                            | Repository dogfood, current knowledge, rules, and active work |
| `src/cli-mvp/`                        | Frozen legacy CLI source, build support, and tests            |
| `src/open-forge-cli/`                 | Non-shipping replacement CLI source                           |
| `.agents/memory/archived/cli-v2/`     | Deleted CLI-v2 raw historical input                           |
| `.agents/memory/working/cli-release/` | Active new-CLI evidence and gate state                        |

## CLI Transition

The TypeScript MVP is frozen. Do not modify, build, test, repair, or otherwise
exercise its source or tests while developing the new CLI. Use
`open-forge-old` only when repository routing assistance is needed, such as:

```sh
open-forge-old load --bodies
open-forge-old index
open-forge-old doctor
```

CLI v2 was deleted. Its former Documents, Decisions, Directives, Patterns,
plans, and implementation records live under
`.agents/memory/archived/cli-v2/`. They are raw input, not accepted design.

The new CLI direction is:

- One canonical .NET Native AOT executable
- Optional agent-first acceleration over a complete Markdown Framework
- Boring, explicit, predictable behavior for agents and occasional human use
- Native AOT and trimming compatibility as hard implementation constraints
- Thin package-manager wrappers that do not implement Framework behavior
- npm as the first wrapper
- The root `package.json` retained as an ecosystem-neutral orchestration layer

The command surface, architecture, libraries, tests, safety model, native
artifacts, and wrapper placement remain subject to explicit design and
maintainer acceptance.

Rune is outside the current release effort.

## Current Repository Tooling

The root `package.json` contains transitional Bun and TypeScript scripts for the
frozen MVP and repository build. They are not replacement CLI implementation or
a replacement release gate. The non-shipping native CLI toolchain remains
separate from this frozen support. Package wrappers do not exist yet.

Do not treat `dist/` or `.temp/` as authored authority. Do not edit generated
output manually.

For Framework-only changes, use proportionate checks and the available legacy
routing commands. Review the complete Git diff before closeout:

```sh
open-forge-old index
open-forge-old doctor
git diff --check
```

Do not run frozen MVP tests or builds as part of new-CLI development.

## New CLI Evidence

No prototype is working merely because source or test files exist. A native CLI
claim requires reproducible restore, compilation, focused tests, Native AOT
publication, actual binary execution, and wrapper evidence appropriate to the
accepted slice.

Design tests from observable risk and independent evidence needs. Do not inherit
the deleted CLI-v2 test volume or tier structure automatically.

## Documentation

When accepted direction changes, update every source that answers a distinct
affected question. Preserve useful old reasoning in the appropriate historical
route instead of leaving competing current descriptions.

Follow the repository [Writing Directive](../.agents/directives/writing.md),
[Writing Standard](../.agents/memory/crystallized/documents/maintenance/writing.md),
and [Dictionary](../.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md).
