---
open-forge:
  description: Anchoring code comments to Open Forge sources by ID or path plus heading, so an agent reading code can reach the documentation that governs it
  tags: [Idea, Memory, Emerging, Contextual, Candidate, CLI, References, Navigation]
---

# Code To Context References

## The idea

A comment in source code that names an Open Forge source, so an agent — or a
person — reading the code can reach the documentation that governs it:

```csharp
// @open-forge ./.agents/memory/crystallized/documents/cli/architecture.md#threat-boundary
internal sealed class WorkspaceLockStore { … }
```

The reference is an **ID or path plus a heading**, not a line number. Headings
survive edits; line numbers do not, and a reference that silently rots is worse
than none.

This originated as part of `rune` — semantic search over Open Forge — and its
companion `glyph`, an LSP-based code search. Both are far out. The question is
whether the anchoring half is worth having in Open Forge now, independent of
either.

## Why it may be worth doing early

**It works with no tooling at all.** A comment carrying a resolvable ID already
lets any agent reading the file find the source, because the ID _is_ the path.
That is the whole minimum viable version, and it costs one convention.

**It closes the gap that made implementers skip Open Forge.** A task record does
not carry an instruction to load the framework
([taxonomy-and-adoption.md](../../emerging/analysis/cli-experience-audit/taxonomy-and-adoption.md)),
and neither does a source file. An agent opening `WorkspaceLockStore.cs` has no
signal that a threat-boundary document governs it. A comment is the only place
that signal can live, because it travels with the code.

**It inverts the navigation direction.** Everything in the framework today points
outward from `.agents/` — `Entries`, links, maps. Nothing points _inward_ from
the code. That is the missing half of a system whose premise is that context and
work stay connected.

## What the CLI would add

The convention alone is useful. Three small things make it substantially more so,
and each is independently shippable:

- **Resolution.** `open-forge context <ref>` already accepts an ID; accepting
  `id#heading` and returning only that section is a small extension of the
  existing `--content section:<name>` capability.
- **Reverse lookup.** `open-forge references <source> --from-code` scans tracked
  source files for `@open-forge` comments naming that source, so a document can show
  what code depends on it. This is the same scan `references` already performs
  over Markdown, pointed at a wider file set.
- **Validation.** A `doctor` finding for an `@open-forge` comment whose target or
  heading no longer exists. This is the piece that makes the convention
  trustworthy — and without it the references rot exactly like line numbers.

Note that all three reuse machinery that exists. None requires semantic search,
which is what keeps this separable from `rune`.

## Open questions

### The marker name

`@open-forge`. Decided.

`@rune` reads better and is shorter, but it names a tool that may never be
released. A convention carrying an unreleased product's name is a bet on that
release, and if the bet fails every comment in every workspace is named after
nothing. Do **not** add it as an alias either: an alias for a name that does not
exist is a second thing to explain and a second thing to support.

If `rune` ships later it reads the same comments — the marker is a prefix, not a
protocol, and adding a second accepted prefix at that point costs one line in the
scanner. The option stays open by doing nothing now.

### Reference shape — clickable wins

The most useful form is a **workspace-root-relative path with a heading**, not an
ID:

```
// @open-forge ./.agents/memory/crystallized/documents/cli/architecture.md#dependency-direction
```

Because editors and terminals make that clickable, and an agent resolves it with
no tooling at all. An ID (`memory/crystallized/documents/cli/architecture`) is
shorter and route-aware but not clickable and not resolvable without the CLI.

Accept both — the hybrid normalizer already in the backlog makes them the same
input. Recommend the clickable form in the convention, because the whole point is
that the reference works before any tool does.

### Validation is a plain scan

Finding broken references needs no semantic search: `grep` for the prefix, resolve
each target, check the heading exists. That is small enough to add to `doctor`
independently of anything else in this idea, and it is what stops the references
rotting the way line numbers would.

### Remaining questions

- **Which files are scanned.** All tracked files matches how `references` already
  behaves; a `scan` glob in `open-forge.json` is more controllable.
- **Heading stability.** Headings move less than line numbers but still move. The
  validation finding is what makes the heading half acceptable — without it, do
  not ship headings.
- **Interop with `rune` later.** If it ships, it reads the same comments; nothing
  needs deciding now — a prefix is not a protocol.

## Priority

**Not now.** Recorded because it is coherent and because the hybrid-ID
normalization already in the backlog is a prerequisite: a code comment will be
written as `memory/crystallized/documents/cli/architecture.md` or as
`.agents/memory/…`, and both must resolve. That task lands regardless, so this
idea gets cheaper after it.

Revisit once the CLI is releasable and the state and view work is done.
