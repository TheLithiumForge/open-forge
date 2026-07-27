---
open-forge:
  description: Current user-owned overwrite purpose, selection boundary, inherited route behavior, precedence, generated boundary, ownership, and lifecycle
  responsibility: Define how one Markdown companion adjusts its base without becoming an independent route or obscuring a complete replacement
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Overwrite, Customization, Ownership]
---

# Overwrite Customization

## Scope

This document is authoritative for the meaning and lifecycle of Open Forge overwrite companions.

The [loader](../../../../../loader.md#routing) remains authoritative for the compact installed loading and precedence rule. The [Framework Architecture](../architecture.md#recursive-customization) places overwrites among the available customization choices. CLI and Extension documents define only the deterministic operations and package boundaries they own.

## Role

An overwrite is an optional user-owned Markdown companion named `{name}.overwrite.md` beside `{name}.md`.

Use one when the base remains useful and correct except for a small local addition, narrowing, exception, replacement, or explicit disable. The overwrite states the bounded adjustment and the result after applying it.

Typical adjustments include a local rule, example, exception, or interpretation that belongs specifically to the base file's existing subject and scope.

Prefer a local routed file when the meaning stands independently, a scope route when the subject needs narrower placement, and a direct edit, replacement, or removal when the base is fundamentally unsuitable. An overwrite should not make a reader reconstruct the intended model from extensive contradictions.

An overwrite is a customization of one base file. It is not a separate route, primitive, Memory state, Extension mechanism, or generated entry.

## Identity And Loading

The shared filename stem and containing folder identify the pair:

```text
{name}.md
{name}.overwrite.md
```

Whenever the base is loaded, the overwrite is read immediately afterward. It inherits the base route, scope, and loading behavior.

An overwrite is never selected, indexed, or loaded independently. Frontmatter, descriptions, or tags inside it do not create a second routing surface or change the base file's generated entry or load policy.

An overwrite without its base is an orphan. It has no inherited route, scope, or loading behavior and must be repaired, moved to an appropriate independent route, or removed.

## Composition And Precedence

The base remains effective except where the overwrite explicitly changes it.

When both files address the same question incompatibly, the overwrite has final precedence within the base file's scope. State the affected base behavior and its local result clearly enough that no reader must guess which statement applies.

File-local precedence does not grant authority outside the base route. It does not silently override loaded ancestor Directives, another authoritative source, clear user direction, runtime safety, or platform constraints. Conflicts across those boundaries follow the normal authority and conflict contract.

An overwrite may explicitly replace or disable one bounded behavior. It cannot remove the base route or its loading behavior because the base remains the selected file. Replace or remove the base when the complete contract should no longer apply.

## Generated Boundary

Generated `Entries` describe routed sources and never list overwrite companions.

An entrypoint overwrite adjusts only the authored contract associated with its base. It does not own a second generated region. Change navigation by adding, moving, editing, or removing routed sources, then rebuild the base entrypoint's generated `Entries`.

## Ownership And Lifecycle

Overwrite companions belong to the workspace. Framework installation and update preserve them as separate visible files instead of merging them into a base or treating their absence as damage.

Managed Extensions contribute whole files and cannot claim workspace-owned overwrite paths. Packaging or ownership metadata never becomes necessary to understand the overwrite at runtime.

After a base changes, review its overwrite when the adjustment may no longer match the updated wording or behavior. Git preserves a reviewable difference, but it does not prove that the combined result remains coherent.

## Related Current Sources

- [Framework Architecture](../architecture.md)
- [Routing loading and continuity](loading.md)
- [Route scope and inheritance](scope.md)
- [Routing paths and identity](paths.md)
- [Canonical loader](../../../../../loader.md)
- [Loader maintenance contract](../../maintenance/payload/agents/loader.md)
- [CLI MVP Architecture](../../cli/architecture.md)
- [Extensions MVP Architecture](../../extensions/architecture.md)

## Decisions And Rationale

- [Source and packaging](../../../decisions/source-and-packaging.md)
- [Do not revive rejected mutation mechanisms](../../../decisions/do-not-revive.md)
