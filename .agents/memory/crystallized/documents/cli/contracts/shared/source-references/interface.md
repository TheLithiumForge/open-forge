---
open-forge:
  description: Accepted current caller-visible contract for shared CLI source references
  responsibility: Define the public source-reference grammar, identity, disambiguation, results, errors, and examples
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Source, Reference, Interface, CurrentTruth]
---

# CLI Source References Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the caller-visible
shared source-reference contract. It does not ship yet; implementation and
executable proof remain pending Gate 5. Command contracts link here for public
grammar, identity, disambiguation, results, errors, and examples instead of
defining a second source grammar.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
classification, resolution, invariants, safety, result formation, and
conformance. The [Shared Result Coordinates](../result-coordinates/interface.md)
define the shared result envelope and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines the BCL-first filesystem and
physical-identity, runtime, and evidence boundaries. This file remains the
caller-visible source-reference authority rather than creating an implementation
contract.

## Purpose

Commands accept either a short automatic source ID or an exact workspace-relative
`.agents` path. IDs are convenient. Paths provide exact disambiguation when a
workspace contains unusual or colliding structures.

The CLI never guesses whether a reference is an ID or path.

## Accepted Forms

A source reference is one of:

```text
<source-id>
.agents/<path>
./.agents/<path>
```

An exact path keeps its `.agents/` or `./.agents/` prefix. For example,
`.agents/x/y/z` and `./.agents/x/y/z` have the ID `x/y/z`; the derivation rules
below still apply to Markdown suffixes and recognized entrypoints.

The prefix determines the form:

- `.agents/` or `./.agents/` means an exact workspace path.
- Every other value means a source ID, even when it resembles another relative
  filesystem path. For example, `src/file.md` is parsed as an ID and will usually
  be unknown. Use `.agents/...` for path input.

Examples:

```text
memory/crystallized/documents
.agents/memory/crystallized/documents/_documents.md
./.agents/memory/crystallized/documents/_documents.md
```

These values identify the same source.

Source-reference operands have no general path form outside `.agents`. A
specific command may define a separate external-source operand with its own
grammar.

## Automatic Source IDs

Every supported file under `.agents` receives an automatic ID derived from its
workspace-relative path. The ID is calculated per invocation. It is not stored
in frontmatter or a registry.

Examples:

| Path                                                                 | Automatic ID                                              |
| -------------------------------------------------------------------- | --------------------------------------------------------- |
| `.agents/loader.md`                                                  | `loader`                                                  |
| `.agents/memory/_memory.md`                                          | `memory`                                                  |
| `.agents/memory/crystallized/_crystallized.md`                       | `memory/crystallized`                                     |
| `.agents/memory/crystallized/documents/architecture.md`              | `memory/crystallized/documents/architecture`              |
| `.agents/skills/experience-design/SKILL.md`                          | `skills/experience-design`                                |
| `.agents/skills/experience-design/references/map-user-experience.md` | `skills/experience-design/references/map-user-experience` |

Derivation rules:

1. Start from the canonical workspace-relative path below `.agents`.
2. Use `/` between ID segments on every operating system.
3. Remove the `.md` suffix from Markdown files.
4. For a recognized entrypoint, use its containing folder ID instead of its
   filename.
5. For `SKILL.md`, use the containing Skill folder ID.
6. Keep a meaningful extension for supported non-Markdown resources.
7. Preserve exact case, spaces, and Unicode.
8. Reject `.` and `..` ID segments.

An automatic ID identifies a source. It does not make the source routed,
indexed, authoritative, or managed.

## Entrypoints

Recognized entrypoint filenames do not appear in source IDs. For example:

```text
.agents/memory/_memory.md
```

has the ID:

```text
memory
```

The same rule applies to accepted compatibility entrypoint filenames when the
Framework recognizes them as input. The generated or reported ID uses the
folder path, not `_memory`, `index`, `_index`, `references`, or `_references`.

If one folder contains more than one recognized entrypoint, exact path input can
identify a file for inspection, but the folder's route identity remains
ambiguous. A command that needs valid route meaning blocks until the structural
problem is fixed.

## Quoting Spaces And Special Characters

Source IDs and paths may contain spaces or Unicode. Quote the complete operand
in the shell:

```text
open-forge context "memory/project alpha/documents"
open-forge context ".agents/memory/project alpha/documents/_documents.md"
```

Single quotes are also valid in shells that support them:

```text
open-forge context 'memory/project alpha/documents'
```

Quotes are shell syntax. They are removed before the CLI receives the value and
are not part of the ID or path. Documentation uses double quotes by default
because they work across more common shells. Windows `cmd.exe` users must use
double quotes.

The CLI does not implement another quote language after argument parsing.

## Path Resolution

An exact path:

- Resolves from the selected workspace.
- Accepts `.agents/...` or `./.agents/...` and reports `.agents/...` canonically.
- Uses `/` in documented CLI input on every operating system.
- Must remain lexically and physically inside the selected workspace.
- Must identify a supported source kind for the selected command.
- Does not become another workspace or external source.

Backslash input, case compatibility, filesystem aliases, and exact physical
identity are realized within the [CLI Architecture](../../../architecture.md)
BCL-first filesystem boundary. Canonical output uses workspace-relative `/`
paths; the implementation may not add compatibility or alias behavior that
changes the public identity rules.

A filesystem symlink projection at an eligible `.agents/...` destination is
identified and reported under that canonical destination path. It retains the
normal automatic source ID derived from the destination path; the source root
named by the separate Library record does not replace that ID.

## ID Resolution

The CLI derives source IDs from the current workspace and matches the complete
input value exactly. It does not apply case correction, fuzzy matching, synonym
matching, percent decoding, or semantic search.

An ID may resolve to:

- One source: continue.
- No source: return invalid input and a useful next action.
- Several sources: use the collision behavior below.

Generated `Entries` help validate and order routed sources. They are not the only
source of ID identity and cannot hide a supported file that exists in the
workspace graph.

## Collisions And Disambiguation

Users may create structures that derive the same ID. For example:

```text
.agents/guidance/style.md
.agents/guidance/style/_style.md
```

Both derive:

```text
guidance/style
```

The CLI does not choose by file kind, generated order, modification time,
directory depth, or likely intent.

### Interactive Use

An interactive command may show every candidate and ask the user to choose:

```text
The source ID `guidance/style` matches more than one source.

1. .agents/guidance/style.md
2. .agents/guidance/style/_style.md

Choose a source, or rerun the command with its exact path.
```

The choice completes only this source reference. It does not grant write,
overwrite, delete, force, or ownership authority.

### Non-Interactive And JSON Use

A non-interactive or JSON invocation returns `blocked` with every candidate
path. It does not prompt or choose a default.

The exact path is the normal disambiguation mechanism:

```text
open-forge context .agents/guidance/style.md
```

Do not add `id:`, `route:`, `path:`, `@file`, or similar qualifier syntax unless
later evidence shows exact paths are insufficient.

## Overwrite Companions

A base and adjacent overwrite companion share one logical source ID:

```text
.agents/guidance/style.md
.agents/guidance/style.overwrite.md
```

Automatic ID:

```text
guidance/style
```

Selecting the ID, base path, or overwrite path selects the complete logical
source. Commands read or inspect the base first and the overwrite second. The
overwrite is never returned as an independent source.

An orphan overwrite has no valid inherited route. Its exact path may be reported
as broken evidence, but commands must not treat it as a valid standalone source.

## Command Use

Every command operand that identifies existing `.agents` content accepts either
an ID or exact path unless that command contract states a narrower requirement:

```text
open-forge context <source-reference...>
open-forge route inspect <source-reference>
open-forge index <source-reference...>
open-forge route update <source-reference>
```

Creation commands may also accept an ID or exact path when the selected command
defines one unambiguous target shape. If an ID could map to several valid target
shapes, creation blocks and asks for an exact path.

External Extension catalogues, formatter executables, workspace roots, and
other filesystem values use their own purpose-specific operands. They are not
source references. A Workspace Library ID is likewise a separate management
identity and is never a source-reference operand.

## Result Display

Every result that emits or identifies a resolved source shows both identities:

```text
ID:   memory/crystallized/documents/architecture
Path: .agents/memory/crystallized/documents/architecture.md
```

Structured results expose the same automatic ID and canonical path as separate
fields. A source with an ID collision keeps its path and reports the ambiguous
ID state.

Only content reached through an `AUTHORED LOCAL MARKDOWN REFERENCE` outside
`.agents` has no automatic Open Forge ID. Human output reports `ID: none`;
structured output uses a null ID. The canonical workspace-relative path remains
required. A filesystem symlink projection at an eligible `.agents/...`
destination retains its canonical destination path and normal destination-derived
automatic source ID.

## Errors

- An empty source reference is invalid.
- A path outside `.agents` is invalid for this operand type.
- A path that escapes the selected workspace is blocked.
- An unknown ID or missing path is invalid.
- An ambiguous ID is blocked unless an interactive choice resolves it.
- A source kind unsupported by the selected command is invalid.
- A structurally ambiguous route blocks commands that require valid route
  meaning even when an exact file path exists.

Every error reports the original reference, its interpreted form, the cause, and
a useful next action.

## Related Sources

- [Shared CLI Contract Set](../_shared.md)
- [Behavior Contract](behavior.md)
- [CLI Architecture](../../../architecture.md)
- [Global CLI Flags Interface Contract](../global-flags/interface.md)
- [Context Interface Contract](../../context/interface.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Framework path rules](../../../../framework/routing/paths.md)
- [Framework overwrite rules](../../../../framework/routing/overwrites.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
