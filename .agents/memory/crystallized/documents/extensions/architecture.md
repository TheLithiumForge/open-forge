---
open-forge:
  description: Current Open Forge Extensions MVP package semantics, composition, runtime boundary, ownership lifecycle, safety properties, and liabilities
  responsibility: Define the accepted current Extensions MVP architecture and distinguish it from candidate replacement direction
  tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, Extension, MVP, Composition, ACE]
---

# Open Forge Extensions MVP Architecture

## Status And Scope

Open Forge Extensions are a dogfooded MVP whose long-term architecture remains intentionally open. This document is authoritative for the coherent current view of:

- What an extension means
- How extension packages compose Framework content
- Current source, identity, dependency, ownership, and lifecycle boundaries
- Runtime behavior after installation
- Proven invariants worth preserving
- MVP liabilities

The [top architecture](../architecture.md#framework-composition) is authoritative for the composition relationship among Core, Memory, and Extensions. The [Framework Architecture](../framework/architecture.md) is authoritative for every runtime route, primitive, authority, and Memory meaning used by installed extension files. The [CLI MVP Architecture](../cli/architecture.md) is authoritative for the deterministic implementation that currently discovers, plans, installs, validates, and removes packages.

This document describes the present MVP without declaring that its manifest schema, catalogue, grouping, lifecycle, or CLI integration is the final Extensions design. Open design questions remain contextual in the [Extensions overhaul candidate](../../../emerging/ideas/extensions-overhaul.md) until a replacement architecture is accepted.

## Core Proposition

An extension is an optional content-agnostic installation and ownership unit.

It may contribute:

- A skill
- A workflow
- Directives
- Guidance
- Patterns
- Templates
- Workspace routes
- Memory routes
- Support files
- A deliberate combination
- Only dependencies as a convenience pack

Extension is not a runtime primitive. After installation, every file retains the ordinary meaning of its destination `route`. A Skill remains a Skill, a Directive remains binding in its loaded scope, a Workflow remains a routed recipe, and Memory retains its state and authority.

The package delivers and optionally manages files. It does not create another agent interpretation layer.

## Architectural Invariants

The following constraints define the intended Extensions boundary beyond the current implementation:

1. Extensions remain optional
2. Explicit user selection precedes installation
3. Installed files remain complete runtime truth
4. Extension metadata never becomes necessary for agent interpretation
5. Extension content uses ordinary `routes` and relationships
6. Packages add whole files rather than injecting private mutations into shared Markdown
7. Every installed managed path has explicit ownership
8. Dependency edges compose installable units without redefining runtime relationships
9. Manual plain-file installation remains possible
10. Installation effects are reviewable before they become trusted context
11. Removal protects user changes, shared ownership, dependencies, and route reachability
12. Catalogue and source organization do not determine authority

## Current MVP System

The MVP combines six elements:

```text
Bundled or local source package
  -> manifest identity and dependency graph
    -> payload of complete target-relative files
      -> CLI plan and safety preflight
        -> assembled workspace routes
          -> optional ownership receipt
```

The first three elements define extension content and composition. The CLI MVP Architecture is authoritative for current planning and application. Installed workspace files are authoritative for runtime meaning. The receipt records only managed lifecycle state.

## Source Packages

The current first-party package lives directly beneath [`src/extensions/`](../../../../../src/extensions/):

```text
src/extensions/
  development-toolkit/
    extension.json
    README.md
    payload/
```

A normal managed package has:

```text
{package-folder}/
  extension.json
  README.md
  payload/
    ... target-relative complete files ...
```

A dependency-only pack may omit `payload/`. A local source may use the complete package shape, a plain `payload/` directory, or a direct overlay whose contents map directly into the target.

The source folder helps maintainers browse packages. It does not determine install identity, dependency semantics, or runtime meaning.

## Identity And Manifest

The current `extension.json` manifest may declare:

- `id`: stable lowercase managed identity
- `name`: display name
- `description`: catalogue selection text
- `version`: descriptive package version
- `dependencies`: bundled extension identities required by the package

Every bundled first-party package declares an id. A local source may declare an id to opt into managed lifecycle. An idless local source remains unmanaged.

The stable id is intentionally independent from source location and content type. Moving a package between catalogue groups does not change its identity.

The current version field is descriptive. The MVP has no compatibility solver, version range semantics, or migration contract.

Unknown manifest fields are rejected so misspelled ownership or dependency declarations do not silently alter behavior.

## Payload

The payload contains complete files at their intended workspace-relative paths.

Open Forge-authored routed files normally carry:

- `#Extension`
- Their Framework primitive or Memory classification
- Useful scope and topic tags

Standard runtime formats such as `SKILL.md` retain their native metadata and resource conventions.

`#Extension` identifies optional package provenance and composition. It creates no authority, loading, or runtime behavior. Reserved loading tags are used only when the installed file deliberately belongs in baseline or continuity context.

Markdown links resolve relative to the file in the assembled workspace. A same-package link should resolve in the isolated payload. A link to Core or a declared dependency may resolve only after complete assembly.

Generated `entries` expose installed files but do not own their meaning.

## Runtime Boundary

An installed extension disappears as a runtime abstraction.

Agents use:

- Installed Markdown
- Framework `entrypoints`
- Native `SKILL.md` packages
- Relative links
- Tags
- Declared external sources

They do not need:

- `extension.json`
- Source catalogue grouping
- Package README files
- Dependency metadata
- The ownership receipt
- The CLI

This is the most important Extensions invariant. Packaging may become more sophisticated without making installed workspaces dependent on a private package runtime.

## Dependency And Composition Model

Dependencies connect installable packages. They do not replace runtime links.

The current resolver:

- Uses stable extension ids
- Resolves bundled dependencies offline
- Traverses dependencies transitively
- Installs dependencies before dependents
- Rejects unknown ids and cycles
- Deduplicates the resolved closure

A workflow package may depend on a skill package to ensure the skill arrives. The installed workflow still links to the concrete `SKILL.md` route it needs because the dependency edge is not agent context.

Dependency-only packs select a useful closure without installing placeholder runtime files.

The current MVP permits local packages to depend on bundled ids. It does not resolve arbitrary local-to-local graphs, remote packages, registries, or capability providers.

## Current Catalogue

The [extension catalogue README](../../../../../src/extensions/README.md) and manifests are authoritative for the live package list and descriptions.

The pre-release catalogue contains one deliberately small mixed package. It proves that one package model can assemble ordinary Workflows, a native Skill, and Templates without introducing a runtime Extension abstraction. It does not prove that the current package boundary or content should remain after the Extensions overhaul.

Optional catalogues must not turn Open Forge Core into the author's universal methodology. A package earns continued distribution through reusable value, clarity, maintenance cost, and evidence of use.

Earlier first-party package identities were removed before a stable release and are not aliases. Files already installed from them remain ordinary workspace content. Existing receipts remain sufficient for explicit preview and removal without retaining the old source packages.

## Manual And Managed Installation

The plain-file installation contract is:

1. Copy payload files into the target workspace
2. Rebuild or manually update affected generated route entries
3. Review the assembled files and relationships

After that, the extension is fully usable without the CLI.

The current CLI adds:

- Catalogue discovery and selection
- Dependency resolution
- Complete plan construction
- Portability and containment validation
- Collision and ownership checks
- Generated-index maintenance
- Git review checkpoints
- Managed update and removal
- Rollback after handled failures

The convenience layer is significant, but it remains an implementation over the plain-file contract.

## Managed Ownership

Stable-id installations are recorded in `open-forge.extensions.json` at the workspace root.

The current receipt records:

- Explicitly requested roots
- Installed dependencies
- Descriptive versions
- Owned payload paths
- Content digests
- Shared owner sets

The receipt allows the CLI to distinguish:

- A file it may safely reconcile
- A file modified after installation
- A path shared by several managed packages
- An existing unowned file
- A package dependency that remains required

Generated `Entries` bodies are excluded from authored ownership identity because the CLI may legitimately rebuild them around an extension-owned `entrypoint`.

An unmanaged overlay or externally installed skill remains outside the receipt. Routing it does not transfer ownership.

## Update And Removal

Reinstalling a stable id currently acts as managed reconciliation.

The CLI verifies recorded bytes, plans new payload effects, updates owned files, and removes dropped files only when their recorded contents still match and no owner remains.

Removal:

- Acts only on explicitly requested ids
- Refuses to break retained dependents
- Preserves modified owned files by blocking
- Preserves shared files while another owner remains
- Refuses to remove an `entrypoint` that would strand retained routed descendants
- Does not automatically prune orphaned dependencies

This behavior is safety-oriented but still lacks an explicit versioned update or migration model.

## Safety Boundary

Extension safety is larger than validating payload bytes.

The current plan accounts for:

- Portable cross-platform path identity
- Source and target topology
- Lexical and physical containment
- Symlinks, junctions, and hard links
- File and parent-directory collisions
- Existing unowned paths
- Shared managed ownership
- Git visibility
- Generated-index side effects
- Route reachability after removal

Payloads cannot claim `.git/`, `.gitignore`, the ownership receipt, or workspace-owned `.overwrite.md` files. The [overwrite contract](../framework/routing/overwrites.md) owns why these companions remain outside Extension ownership.

Different bytes targeting one portable path are a conflict. Identical managed bytes may share owners only through explicit compatible plans. Byte equality alone is not permission to adopt an existing file.

The [CLI MVP Architecture](../cli/architecture.md) is authoritative for how these checks are implemented and transacted.

## Skills And External Managers

Open Forge does not need to own every skill.

An external manager or user may install a complete native skill under `.agents/skills/{skill-name}/`. Regenerating the skills index makes it routable without wrapping or rewriting it.

Two managers must not claim the same installed path. The current MVP treats external installation as distinct-path additive interoperability.

An externally installed skill does not automatically satisfy an extension dependency id. Supporting substitution would require an explicit future capability or satisfaction model with clear identity, compatibility, and ownership semantics.

## Packs

A pack is an extension selected primarily for composition convenience.

It may:

- Declare only dependencies
- Add coordinated files
- Combine both when the complete unit has independent value

A pack should not duplicate payloads already owned by narrower packages. Dependency edges are the preferred composition mechanism when the desired files already have canonical owners.

Source placement is catalogue presentation, not a special dependency engine or runtime primitive.

## Current Verification

The current extension system is verified at several levels:

- Pure tests cover dependency selection, manifest validation, path identity, ownership receipts, and collision rules
- Closure tests use real subprocesses, filesystems, and Git repositories for installation, update, removal, rollback, and review checkpoints
- Catalogue integration tests verify the single advertised package, its isolated complete installation, owned lifecycle, direct Workflows, native Skill, Template routes, and dogfood parity
- Packaged-layout tests verify discovery from built and npm-style package layouts
- Framework validation checks assembled routes and links after installation

The primary implementation test sources are linked from the [CLI MVP Architecture](../cli/architecture.md).

These tests validate deterministic packaging and lifecycle behavior. They do not prove that every catalogue package improves agent outcomes.

## MVP Liabilities

### Architecture Is Coupled To The CLI

Manifest parsing, catalogue discovery, dependency resolution, ownership, and lifecycle behavior currently live inside the CLI monolith. Extensions have no independently expressed domain implementation.

### Source Model Is Local And Closed

The resolver understands bundled packages and local sources. It has no source-provider boundary, registry protocol, remote trust model, provenance verification, or reproducible third-party fetch contract.

This limitation is safe for the MVP. The overhaul must decide whether remote distribution belongs in Open Forge at all before designing it.

### Versions Do Not Govern Compatibility

Versions are descriptive. There is no compatibility negotiation among packages, the Framework, the CLI, or agent runtimes.

### Update Has No Migration Semantics

Reinstallation reconciles whole files, but packages cannot declare migrations, compatibility transitions, or required user decisions. This is insufficient for long-lived third-party packages.

### One Root Receipt

The receipt is transparent and effective for the current manager, but its ownership model assumes one Open Forge lifecycle authority at the workspace root. Multi-repository environments, nested scopes, submodules, or several package managers may require more explicit ownership boundaries.

### Dependency Identity Is Package-Specific

Dependencies require exact bundled ids. The MVP cannot express compatible alternatives, provided capabilities, optional dependencies, conflicts, or external satisfaction.

Adding those concepts without a demonstrated need would recreate package-manager complexity, so the overhaul must justify each one.

### Catalogue Governance Is Immature

The consolidated first-party catalogue is intentionally small but still has no accepted stability, deprecation, support, or quality policy. Continued distribution must be justified by actual reusable value rather than the existence of the package.

### Scope-Aware Installation Is Incomplete

Payload paths can target deep `routes`, but the package model does not yet provide a complete user-facing design for selecting a scope, initializing missing scope `entrypoints`, or explaining how ownership composes across many repositories and submodules.

### Recovery Is Process-Local

Handled failures roll back, while abrupt process or machine failure relies on Git. There is no persistent recovery journal or workspace mutation lock.

## Candidate Direction

The [Extensions overhaul candidate](../../../emerging/ideas/extensions-overhaul.md) collects open design questions and candidate direction for source, trust, packages, compatibility, composition, scope, lifecycle, catalogue, components, and migration. Keeping that material in Emerging Memory prevents it from appearing as accepted current Extensions architecture.

## Non-Goals

Extensions are not:

- Another `root route` or interpretation model
- A new runtime primitive
- A provider-specific plugin runtime
- A hidden instruction database
- A reason to make Core large
- Automatic authority over local files
- Automatic satisfaction of external capabilities
- A remote registry by default
- Permission to mutate shared Markdown invisibly
- A substitute for ordinary relative links and routes

## Related Current Views

- [Open Forge architecture](../architecture.md)
- [Framework Architecture](../framework/architecture.md)
- [CLI MVP Architecture](../cli/architecture.md)
- [Current extension user contract](../../../../../docs/extensions.md)
- [Current first-party catalogue](../../../../../src/extensions/README.md)

## Decisions And Rationale

- [Extension package boundary](../../decisions/extension-package-boundary.md)

## Historical Context

These archived records preserve earlier observations and exploration. They may inform future redesign, but they do not govern the current Extensions architecture:

- [Historical package and skill interoperability observation](../../../archived/observations/2026-07-15_extension-units-and-skill-interop.md)
- [Historical extension preflight observation](../../../archived/observations/2026-07-15_extension-preflight-boundaries.md)
- [Historical extension skill-sharing exploration](../../../archived/ideas/extension-skill-sharing.md)
