---
open-forge:
  description: Current canonical Markdown syntax for Open Forge metadata, headings, lists, links, code literals, tags, and semantically named sections
  responsibility: Define the canonical Markdown syntax used to represent Open Forge contracts
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Markdown, Authoring, Syntax]
---

# Canonical Markdown Syntax

## Scope

This document is authoritative for the canonical Markdown notation used when Open Forge assigns meaning to authored structure.

It defines notation, not the behavior of routes, Framework primitives, Memory states, or deterministic tools. The [routed Markdown contract](routes.md) defines the shared representation of routed files, and each component's authoritative source defines its own semantic sections and behavior. Ordinary prose remains ordinary Markdown unless a Framework contract assigns additional meaning to part of it.

## Design Goals

Open Forge Markdown is designed to be:

- Readable in raw text
- Understandable without the CLI or a private database
- Cheap for agents to scan and navigate
- Stable and reviewable in Git
- Explicit enough for deterministic validation
- Connected through normal relative links
- Canonical where syntax carries Framework meaning

Open Forge uses a preferred authoring form for each machine-meaningful construct. Frontmatter has two supported forms, and each workspace chooses which one writers use. Other [compatibility input](compatibility.md) does not become preferred output.

## Canonical Forms

Open Forge-authored files use:

- ATX headings using `# Heading`, `## Heading`, and deeper levels as needed
- Blank-line separation using `\n\n` between paragraphs, headings, lists, and fenced blocks
- Hyphens using `- item` for unordered lists
- Sequential decimal markers using `1. item`, `2. item`, and `3. item` for ordered lists
- Triple-backtick fences using ` ``` ` before and after code blocks
- Inline Markdown links using `[label](destination)` for routes and clickable references
- YAML frontmatter using `---` before and after the metadata block when indexed metadata is required
- Bare tags using `#Tag` where tags appear in prose or route metadata

Machine-readable sections use only their declared line shape. For this repository's maintained prose, the [Open Forge Writing Standard](../../maintenance/writing.md) defines detailed prose guidance; it does not add an installed Markdown contract.

## Visible Source And Control Markers

Open Forge keeps agent-facing meaning visible in raw and rendered Markdown.
HTML comments do not carry instructions, selection guidance, behavioral
requirements, or other authored meaning.

Canonical managed root hosts use one top-level ATX heading and one standalone
strong paragraph as their boundaries:

```md
# Open Forge

managed instructions or imports

**End of Open Forge managed section.**
```

The heading and footer are ordinary Markdown blocks, but the managed-host
contract assigns them boundary meaning. Markdown between them remains ordinary
instructions or imports. Boundary recognition is structural, so fenced code,
quoted blocks, nested lists, and inline examples do not claim host ownership. An
incomplete heading and footer pair does not extend ownership to the end of the
file.

Existing exact `<!-- open-forge:start -->` and `<!-- open-forge:end -->` lines
remain input-only compatibility for managed hosts. They are not canonical
output. The generated `Entries` contract separately retains exact historical
guard comments as input-only migration data. Index does not emit them.

Generated Entries use heading boundaries, with no comment tokens.

Templates keep removable source guidance in visible multiline `{...}`
placeholders. Ordinary prose uses visible Markdown rather than another comment
or renderer-specific concealment mechanism.

## Frontmatter

Routed Markdown uses leading YAML frontmatter in root or scoped form. Root form places Open Forge fields at the YAML root. Scoped form places them under an `open-forge:` mapping. New metadata follows the [workspace setting](../../cli/shared-operation-contract.md#frontmatter-form).

Scoped form:

```yaml
---
open-forge:
  description: Current architecture of an example system
  responsibility: Define the accepted structure, relationships, and boundaries of the example system
  tags: [Memory, Document, CurrentTruth, Architecture]
---
```

Root form:

```yaml
---
description: Current architecture of an example system
responsibility: Define the accepted structure, relationships, and boundaries of the example system
tags: [Memory, Document, CurrentTruth, Architecture]
---
```

Each block:

- Appears at the start of the file
- Provides one natural-language `description`
- May provide one natural-language `responsibility`
- Provides a YAML list of tags without `#` prefixes

Every workspace reads both forms. Any explicit `open-forge` key reserves the scoped location, whatever its value, and never falls back to root fields. A mapping supplies the complete Open Forge `description`, `responsibility`, and `tags`. Root fields beside it belong to another tool and are not merged. A null, sequence, or alias value retains its existing Missing or Malformed result. Without that key, root fields supply Open Forge metadata for an admitted routed source. Root fields alone do not make a file a source.

Both forms use the same field-value grammar and diagnostics. Unknown root keys are ignored and preserved. Canonical emission uses single-line or indented multiline text values and an inline tag list. Repository payload sources stay authored in scoped form. Delivery renders them in the workspace's form.

Deterministic tools interpret this metadata shape without promising arbitrary YAML support.
The [compatibility boundary](compatibility.md) separates current canonical
syntax, frozen-MVP compatibility, and historical CLI-v2 proposals.

Canonical output and compatible input are distinct. The new CLI must accept or
reject noncanonical value forms through an explicit compatibility decision. A
YAML implementation does not make arbitrary YAML part of the Framework.

Authored frontmatter must be semantically complete: a non-empty natural-language `description` must describe the routed file accurately enough to select or skip it, and tags must accurately classify its loading, role, state, scope, or useful topic. Tools and review fail closed when required authored metadata is missing, malformed, ambiguous, or semantically inconsistent with the routed source rather than inventing meaning from filenames or bodies.

The root Install contract defines bounded initial and verified-managed-base
adoption. Install completes only selected missing compatible required metadata
in prospective source bytes before its existing navigation projection. Managed
adoption first verifies authored managed payload identity and may update only
the generated navigation required by those scoped sources. This does not relax
the shared parser, Index selector, or projector, and it does not change how
other operations handle incomplete metadata. The [Install Interface](../../cli/contracts/install/interface.md#initial-workspace-adoption)
defines the candidate boundary, preservation rules, and permitted derivations.

Beta 6 shipped this adoption boundary, and [Task 70](../../../../archived/cli-development/tasks/task70-existing-workspace-adoption-during-installation.md)
retains its release evidence. Releases before beta 6, including beta 4, predate
this enforcement. The [MVP
Architecture](../../cli/mvp-architecture.md#metadata-and-overwrite-integrity)
records the frozen MVP's temporary liability without changing the canonical
contract.

The `description` helps a reader decide whether to open the file. It provides enough purpose, trigger, or outcome to select or skip the route before loading its body. It is natural and suggestive rather than a repeated formula.

The optional `responsibility` helps an editor decide what belongs in the file. It states the stable boundary of what the file defines and guides edits after the file is opened. It creates no authority or loading behavior. Change it only through a deliberate redefinition, split, or merge. Move content with an independent responsibility to another authoritative source and link to it.

Use `responsibility` only when it adds a useful boundary beyond the route and description. Category entrypoints normally do not need it because their route, definition, primary question, and generated entries already express their responsibility.

Direct-load files that are never indexed do not need Open Forge metadata unless another tool or contract requires it. Standard files such as `SKILL.md` keep the metadata required by their active runtime.

Native `SKILL.md` frontmatter remains governed by its native contract.
Install may complete a missing required native name or description under the
bounded initial or verified-managed-base adoption rule. It preserves existing
native fields, leaves an absent optional `license` absent, and does not wrap the
Skill in `open-forge:` metadata or add generated `Entries` to it. The Install
contract defines its permitted derivations and revalidation boundary.

### File Conditions

An optional `applyTo` condition narrows a routed source to tasks that work on
matching files. This section defines its syntax. The [loading
contract](../routing/loading.md#file-conditions) defines its effect.

`applyTo` has the same meaning at the YAML frontmatter root and under
`open-forge:`. Readers inspect both locations, including a root declaration
beside an existing `open-forge:` block. A declaration is one quoted string or
a list of quoted strings. A string is an expression that produces one or more
atomic patterns. A list entry is one atomic pattern. New declarations use a
quoted-string list in the workspace's form. Updates preserve authored
locations and update or clear both equivalent declarations.

Scoped form:

```yaml
---
open-forge:
  description: C# design rules
  tags: [Directive, CSharp]
  applyTo: ["**/*.cs", "**/*.csproj"]
---
```

Root form:

```yaml
---
description: C# design rules
tags: [Directive, CSharp]
applyTo: ["**/*.cs", "**/*.csproj"]
---
```

In a string expression, split at top-level commas, meaning commas outside
`{...}` and `[...]`. Inside an expression, `\,` is a literal comma and `\\`
is a literal backslash. Decoding happens before brace parsing, so `\,` escapes
only the list separator. Write `[,]` for a literal comma inside a brace
alternative, as in `{a[,]b,c}`. Trim white space around each fragment and drop
empty fragments. An expression with no fragments fails with `Empty`. A list entry is
trimmed as one atomic pattern. Do not split it at commas or decode `\,`. An
entry that is empty after trimming fails with `Empty`. A string containing
`**/*.cs, **/*.csproj` is therefore equivalent to a two-item list containing
those atomic pattern texts. Brace shorthand is not equivalent to its expansion,
so `{src,test}/**/*.cs` and `src/**/*.cs, test/**/*.cs` are different atomic
pattern sets even when they match the same paths.

An absent field adds no condition. Empty strings, empty lists, nulls, and
non-string values are invalid. If both locations declare `applyTo`, readers
normalize each declaration to a set of distinct atomic pattern texts,
comparing those texts with ordinal, case-sensitive equality. Equivalent sets
represent one condition. Different sets make the metadata conflicting and
invalid, and readers do not union the sets or choose one location.

Atomic patterns use slash-separated, workspace-relative paths. A leading `/`
or drive prefix fails with `AbsolutePath`. A `.` or `..` segment fails with
`Traversal`. Empty segments, including a trailing `/`, fail with `Empty`.
Backslashes and control characters fail with `UnsupportedSyntax`. A leading
`!` is an ordinary character, as in POSIX globbing, APM and VS Code. It does
not negate the pattern.

`**` as a whole segment matches zero or more path segments. `**` inside a
longer segment, such as `a**b` or `***`, behaves as `*`. `*` matches any run of
characters within one segment, including an empty run and a leading dot. `?`
matches exactly one character within one segment.

`[...]` matches one character from a set. Ranges use `a-z`. `[!...]` and
`[^...]` negate the set. A `]` directly after `[`, `[!`, or `[^` is a literal
member. A `-` at the start or end is literal. Classes never match `/`. Bracket
forms such as `[*]`, `[?]`, `[[]`, `[{]`, and `[,]` express literal
metacharacters. An unterminated class fails with `UnsupportedSyntax`.

`{a,b}` expands to alternatives before segments are parsed, so an alternative
may contain `/`, as in `{src,test}/**/*.cs`. Several groups multiply. Empty
alternatives are allowed, and commas inside a class within a group do not split
it. A group with one alternative is that alternative. Nested groups, unbalanced
braces, and more than 1000 total alternatives fail with `UnsupportedSyntax`.
Every expanded alternative must pass the path rules above. Matching succeeds
when any alternative matches.

Matching is anchored at the workspace root and is ordinal and case-sensitive on
every host. Dot-prefixed names match normally. Working paths are matched
lexically and need not exist. Use `**/*.py`, not `*.py`, for any depth.

## Tags

Tags add compact loading, type, state, scope, topic, or search signals. They do not replace readable scope in paths and descriptions.

Open Forge-authored tags:

- Stay bare in Markdown, such as #Core or #CurrentTruth
- Omit the `#` prefix inside frontmatter lists
- Begin with a letter
- Use singular PascalCase names by default
- Contain only letters, numbers, and internal hyphens
- Appear only when they improve selection, classification, loading, or retrieval

The [loader](../../../../../loader.md#defined-tags) is authoritative for reserved tag meanings. Ordinary tags remain routing and search signals unless a loaded source explicitly defines more.

Install may add the `Workspace` search tag only when a new required ordinary
completion needs classification. It does not synthesize loading, behavior,
authority, or state tags. The Install Interface defines this narrow exception.

## Headings And Semantic Sections

ATX headings are the canonical section syntax. A contract that names a heading also establishes its exact heading level, spelling, and order.

For example:

```md
## Axioms

- Keep the routed contract explicit.
```

CommonMark Setext headings may be discovered and selected structurally by the
new CLI. They do not become canonical Open Forge authoring or equivalent
semantic sections merely because the parser represents them as headings. Parser
extensions do not add other public heading forms without a later compatibility
decision.

The component source that requires a named section defines its meaning. Markdown syntax alone does not make `Axioms` binding or another heading semantically active.

## Links And Code Literals

Use relative Markdown links for routed destinations and related repository documents. Resolve each local destination from the file containing the link.

Use backticks for:

- Commands and flags
- Code literals
- Defined Open Forge terms when precision matters
- Concrete paths discussed as text rather than used as destinations

Use normal words when their ordinary English meaning is intended. Keep tags bare so Markdown-aware agents, search, graph, and future retrieval tools can recognize them.

Prefer anchored relative links to repeated explanations when another authoritative source already contains the meaning.

## Related Current Sources

- [Routed Markdown representation](routes.md)
- [Markdown compatibility boundary](compatibility.md)
- [Historical CLI-v2 evidence](../../../../archived/cli-v2/_cli-v2.md)
- [Framework Architecture](../architecture.md)
- [Open Forge Writing Standard](../../maintenance/writing.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Canonical Markdown authoring](../../../decisions/framework/canonical-markdown.md)
- [User-facing writing](../../../decisions/framework/user-facing-writing.md)
- [Tag semantics](../../../decisions/framework/tags.md)
