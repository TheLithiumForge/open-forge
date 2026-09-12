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

Prefer a local routed file when the meaning stands independently, a routed scope when the subject needs narrower placement, and a direct edit, replacement, or removal when the base is fundamentally unsuitable. An overwrite should not make a reader reconstruct the intended model from extensive contradictions.

An overwrite is a customization of one base file. It is not a separate `route`, primitive, Memory state, Extension mechanism, or generated `entry`.

## Identity And Loading

The shared filename stem and containing folder visibly identify the pair:

```text
{name}.md
{name}.overwrite.md
```

Whenever the base is loaded, the overwrite is read immediately afterward. It inherits the base `route`, scope, and loading behavior. A valid pair stays adjacent in the same folder, so the final visible local adjustment can be inspected with its base.

An overwrite is never selected, indexed, or loaded independently. Frontmatter, `descriptions`, or tags inside it do not create a second routing surface or change the base file's generated `entry` or load policy.

An overwrite without its base is an orphan. It has no inherited `route`, scope, or loading behavior and must be repaired, moved to an appropriate independent `route`, or removed. Tools and review fail closed when the pair or its identity cannot be established.

The frozen MVP currently reports an orphan overwrite as a warning rather than enforcing this failure boundary. The [MVP Architecture](../../cli/mvp-architecture.md#metadata-and-overwrite-integrity) records that temporary liability without changing the required pair semantics.

## Composition And Precedence

Interpret an overwrite as part of its base source, within that source's role and scope. The base remains effective except where the overwrite explicitly changes it.

When both files address the same question incompatibly, the overwrite has final precedence within the base file's scope. State the affected base behavior and its local result clearly enough that no reader must guess which statement applies.

File-local precedence does not grant authority outside the base `route`. It does not silently override loaded ancestor Directives, another authoritative source, clear user direction, runtime safety, or platform constraints. Conflicts across those boundaries follow the normal authority and conflict contract.

An overwrite may explicitly replace or disable one bounded behavior. It cannot remove the base `route` or its loading behavior because the base remains the selected file. Replace or remove the base when the complete contract should no longer apply.

## Generated Boundary

Generated `Entries` describe routed sources and never list overwrite companions.

An `entrypoint` overwrite is interpreted within the base entrypoint's inherited authority and scope and adjusts only its authored contract. It does not own a second generated region. Change navigation by adding, moving, editing, or removing routed sources, then rebuild the base `entrypoint`'s generated `Entries`.

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
- [CLI MVP Architecture](../../cli/mvp-architecture.md)
- [Extensions Architecture](../../extensions/architecture.md)

## Decisions And Rationale

- [Source and packaging](../../../decisions/framework/source-and-packaging.md)
- [Extension package boundary](../../../decisions/extensions/extension-package-boundary.md)
