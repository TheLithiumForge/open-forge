---
open-forge:
  description: Accepted current caller-visible contract for shared CLI source references
  responsibility: Define the public source-reference grammar, identity, disambiguation, results, errors, and examples
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Source, Reference, Interface, CurrentTruth]
---

# CLI Source References Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the caller-visible
shared source-reference contract. The CLI is available as a public beta. [CLI Distribution](../../../distribution.md)
records qualified platforms and published versions. [CLI Development](../../../../../../working/cli-development/_cli-development.md)
records current implementation and release work. Command contracts link here for public
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
path. IDs are convenient for files below `.agents/`. Paths provide exact disambiguation when a
workspace contains unusual or colliding structures.

The CLI never guesses whether a reference is an ID or path.

## Accepted Forms

A source reference is one of:

```text
<source-id>
.agents/<path>
./.agents/<path>
./<workspace-path>
```

An exact path keeps its `.agents/` or `./.agents/` prefix. For example,
`.agents/x/y/z` and `./.agents/x/y/z` have the ID `x/y/z`; the derivation rules
below still apply to Markdown suffixes and recognized entrypoints.

The prefix determines the form:

- `.agents/` or `./` means an exact workspace path.
- Every other value means a source ID, even when it resembles another relative
  filesystem path. For example, `src/file.md` is parsed as an ID and will usually
  be unknown. Use `./src/file.md` for exact workspace path input.

Examples:

```text
memory/crystallized/documents
.agents/memory/crystallized/documents/_documents.md
./.agents/memory/crystallized/documents/_documents.md
```

These values identify the same source.

A file outside `.agents/` has no short source ID. Name an active included file
by its exact path, such as `./CONTRIBUTING.md`. Explicit Context selections,
Find, References and Route List use the exact `./path` reference in their
existing `id` field. Context startup and ordinary linked reads keep a null ID. Their `path` field stays workspace-relative.

Included paths to the same physical file share the first retained source and its
catalogue parent. Each listed alias remains accepted as an exact operand.

## Authored Local Destination Spellings

Markdown links use containing-file-relative paths with `/` separators. A local
path that resolves to the same target has two accepted spellings:

1. Index form encodes each path segment as UTF-8, leaving only ASCII letters,
   digits, `-`, `.`, `_` and `~` literal. All percent escapes use uppercase
   hexadecimal digits. Index always writes this form.
2. Literal form keeps Unicode, spaces and punctuation decoded, except `%`, `#`,
   `?`, `<` and `>` remain `%25`, `%23`, `%3F`, `%3C` and `%3E`. A colon in the
   first relative path segment remains `%3A` to avoid URI-scheme interpretation.
   An ampersand opening a recognized CommonMark named entity or decimal or
   hexadecimal character reference is `%26`. Determine this after the required
   URL escapes. Ordinary ampersands and unrecognized or incomplete
   references remain literal. For example, `entity&amp;.md` on disk has literal
   form `entity%26amp;.md` and Index form `entity%26amp%3B.md`. `entity&#65;.md`
   has literal form `entity&%2365;.md`: `%23` already prevents a numeric reference
   from opening, so its ampersand needs no extra escape. These are filename characters, not
   character references to decode while parsing Markdown.
   The surrounding Markdown must be a valid
   [CommonMark destination](https://spec.commonmark.org/0.31.2/#link-destination).
   Enclose paths containing spaces in angle brackets, and use angle brackets
   for other punctuation when needed by that syntax, such as unbalanced
   parentheses. Those brackets are delimiters, not part of the path spelling.

Compare the path spelling exactly after deriving the target's canonical relative
path. Either whole form is accepted without an encoding finding. Do not mix
optional escaping from the two forms. Unnecessary escapes such as `%68ash.md`
for `hash.md`, lowercase escapes and other spellings outside both forms retain
`reference.same-target-encoding`. An admitted Repair correction writes Index
form. A heading correction also uses Index form for its path. Resolution decodes
percent encoding once. Thus `literal%20.md` on disk is `literal%2520.md` in both
forms, while `hash#name.md` is `hash%23name.md` in both. `café(one).md` and
`caf%C3%A9%28one%29.md` are both accepted. `<old guide.md>` and `old%20guide.md`
are both accepted.

Source-reference operands and reported filesystem paths remain literal paths.
These Markdown spelling rules do not change their grammar or identity.
Backslash and control characters remain outside the supported local-path rules.
Other punctuation keeps its literal value inside a valid Markdown destination.
Markdown character references and backslash escapes in hand-written sources
still follow CommonMark. Their decoded spans remain ineligible for automatic
spelling edits when the original bytes do not match the parsed destination.

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
- Accepts `.agents/...`, `./.agents/...` or `./<workspace-path>`. The canonical
  path field omits `./`. Files outside `.agents/` are selected by exact path.
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

An included file outside `.agents/` uses the exact-reference display described
under [Accepted Forms](#accepted-forms). An unlisted local Markdown link target
has no source operand identity. Context keeps its null ID for startup and
ordinary linked reads, and uses the exact reference for explicit included selections. The canonical workspace-relative path remains
required. A filesystem symlink projection at an eligible `.agents/...`
destination retains its canonical destination path and normal destination-derived
automatic source ID.

## Unknown Source Guidance

An unknown source ID names the rejected value and points to
`open-forge route list --depth all` in human text. It does not guess, correct
spelling or change case-sensitive resolution. Invalid input stays on stderr
with exit 4. JSON retains its command-owned next action and findings.

## Errors

- An empty source reference is invalid.
- A path outside `.agents/` must use `./<workspace-path>`. The selected command
  must admit that file as a source.
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

## Reserved Runtime Storage

The fixed `.open-forge-state/` subtree is reserved runtime storage. Exclude it before source reads and traversal, even without Git ignore rules. Direct paths and links into it do not admit its files as sources or writable package destinations.
