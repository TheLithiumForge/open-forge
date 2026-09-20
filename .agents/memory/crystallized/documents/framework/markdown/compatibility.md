---
open-forge:
  description: Current input-only Markdown and filename compatibility accepted by Open Forge tools without becoming canonical authoring syntax
  responsibility: Define the boundary between canonical Open Forge authoring and noncanonical input accepted for migration or interoperability
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Markdown, Compatibility, Migration, CLI]
---

# Markdown Compatibility Boundary

## Scope

This document is authoritative for which noncanonical Markdown and filename forms may be accepted as input without becoming recommended Open Forge authoring forms.

The [canonical syntax](syntax.md) and [routed representation](routes.md) define
what Open Forge authors and emits. The [CLI MVP
Architecture](../../cli/mvp-architecture.md) defines only the frozen old CLI's
retained behavior until that implementation is retired. Most new-CLI
compatibility input remains to be designed. The structural heading and
entrypoint filename inputs below are explicit accepted exceptions.

## Accepted New CLI Structural Headings

The new CLI's `find --heading` predicate and `section:<name>` content projection
use CommonMark ATX and Setext heading nodes from the accepted Markdown parser
configuration. Parser extensions do not add other accepted heading forms unless
a later compatibility decision names them.

This is compatibility input for structural discovery and retrieval. It does not
change canonical authoring:

- Open Forge-authored semantic sections still use ATX headings.
- Parser recognition does not make a heading semantically active.
- Component contracts still define required heading names, levels, order, and
  behavior.
- Generated Open Forge Markdown continues to use canonical ATX syntax.

The CLI preserves authored heading spelling and reports its level, source form
when available, and whether it satisfies canonical Open Forge syntax. Heading
names match complete visible text while ignoring case under the accepted CLI
contract. The parser supplies section boundaries; the CLI does not guess
headings from malformed text that the parser does not represent as a heading
node.

## Generated Guard Migration Input

One top-level canonical ATX `## Entries` heading owns the body from the end of
its heading span to the next top-level heading of level 1 or 2, or EOF. Fenced,
indented-code, quoted, nested-list, Setext, differently cased and differently
leveled lookalikes do not establish this semantic section. Trailing horizontal
heading whitespace and an initial BOM are accepted. Duplicate `## Entries`
headings are diagnosed; no arbitrary first section is selected.

Exact historical generated-index start/end comment lines remain readable inside
the heading-owned Entries body. Index replaces that body with canonical entries,
removing these guards on the first rewrite and remaining byte-stable thereafter.
A lone or reordered guard does not change the heading boundary. Comments outside
the body and quoted examples remain ordinary source bytes. This exception is
input-only; it does not restore the retired marker grammar.

## Accepted New CLI Entrypoint Filenames

The new CLI recognizes these existing entrypoint filenames as
compatibility input:

- `index.md`
- `_index.md`
- `references.md`
- `_references.md`

Canonical authoring still uses `_{folder-name}.md`. The CLI creates only that
canonical filename for a new entrypoint.

Recognition is contextual to the containing folder. For example, `_index.md`
is canonical inside a folder named `index` and a compatibility name elsewhere.
The same rule applies to `_references.md` inside a folder named `references`.

When exactly one canonical or compatibility entrypoint exists in a folder, the
new CLI:

- Treats its automatic source ID as the containing folder ID.
- Reads and validates it as that folder's entrypoint.
- Preserves its filename during indexing and bounded route updates.
- Uses its actual relative path when generated navigation must point to it.
- Treats it as an existing entrypoint during recursive route initialization
  instead of creating a canonical sibling.

The CLI does not rename, copy, or migrate a compatibility entrypoint merely to
normalize its filename. A later explicit move or migration operation requires
its own authority and complete reference plan.

When a folder contains more than one recognized entrypoint, the route is
structurally ambiguous. An exact path may select one file for read-only
inspection, but indexing, route initialization, route mutation, and other
operations that require valid route meaning block until one recognized
entrypoint remains. The CLI never chooses by canonical preference, file age,
generated order, or likely intent.

Compatibility recognition is input-only. Generated source files, Templates,
examples, and newly initialized entrypoints use the canonical filename. A
generated link to a preserved compatibility entrypoint records the actual
destination; it does not make that filename a canonical authoring form.

## Historical CLI-v2 Proposal

Deleted CLI v2 proposed accepting these noncanonical frontmatter forms for
interoperability:

- `rune:` scoped metadata or unscoped `description`, `responsibility`, and
  `tags` in external files
- Semantically equivalent quoted or block string scalars and inline flow,
  multiline flow, or block string sequences accepted by the replacement
  frontmatter contract

These forms did not change canonical output in that proposal. This list is raw
historical input, not an accepted requirement for the new CLI.

## Frozen MVP Compatibility Input

The explicitly invoked frozen old CLI may continue to read these historical
forms until it is retired:

- `index.md`, `_index.md`, `references.md`, and `_references.md` as
  category-entrypoint aliases
- `Skill.md` as a skill-entrypoint alias
- A first suitable body sentence as a generated description fallback when an
  external or local file has no supported metadata description

Legacy backtick routes remain input-compatible:

| Surface             | Example                                                          | Path resolution                                                                                      |
| ------------------- | ---------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| Generated `Entries` | ``- `.agents/skills/example/SKILL.md` - Example skill - #Skill`` | `.agents/` paths resolve from the workspace root; other paths resolve from the containing entrypoint |

The frozen old CLI may also read:

- Angle-bracket route destinations accepted by the current parser
- Legacy generated `Entries` sections without bounded markers when they can be migrated safely

Compatibility behavior is input-only unless a current authoritative source explicitly says otherwise. Open Forge-generated files, Templates, examples, documentation, and new authored content use canonical forms.

## Unsupported Equivalents

Other Markdown equivalents may render correctly for people while remaining unsupported for Open Forge semantics.

Open Forge does not promise to interpret:

- Setext headings as canonical or semantically active sections merely because
  they are structurally recognized
- Reference-style links as route entries
- Alternate list markers as machine-readable route lines
- Renamed semantic headings
- Arbitrary metadata layouts

Setext headings remain noncanonical Markdown input. The accepted new CLI may
discover and retrieve them structurally, but deterministic tools do not treat
them as canonical, semantically active, or valid for a component's required ATX
section unless that component contract explicitly accepts them.

Ordinary prose may use normal Markdown without becoming Framework syntax.

## Tool Boundary

Deterministic tools may parse, validate, generate, migrate, scaffold, or display the accepted Markdown contracts. They do not privately define them.

Tools emit canonical syntax, keep compatibility behavior visibly input-only, diagnose ambiguous machine-meaningful structures, and leave unrelated prose alone. The same files remain understandable and maintainable without those tools.

Most of the new CLI's parser technology and compatibility boundary remains
unsettled. Parser acceptance must not grant Framework meaning to unknown syntax.

## Related Current Sources

- [Canonical Markdown syntax](syntax.md)
- [Routed Markdown representation](routes.md)
- [CLI MVP Architecture](../../cli/mvp-architecture.md)
- [Historical CLI-v2 evidence](../../../../archived/cli-v2/_cli-v2.md)

## Decisions And Rationale

- [Canonical Markdown authoring](../../../decisions/framework/canonical-markdown.md)
