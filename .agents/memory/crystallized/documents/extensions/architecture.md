---
open-forge:
  description: Current Extension concept, package composition, installed interpretation, and boundary with lifecycle tools
  responsibility: Define how optional Extension packages compose complete files without changing their Framework meaning or making lifecycle tools necessary for interpretation
  tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, Extension, Composition, ACE]
---

# Open Forge Extensions Architecture

## Role

An Extension is an optional installation and ownership unit for complete files. Its identity is independent of the kind of content it carries. A package may contain one kind of content, a useful combination, support files, or only dependencies.

Extensions add routed content and supporting files. Routed content uses ordinary Framework `routes` and retains its destination's role, scope, loading behavior, and authority. Native formats and supporting files retain the meaning defined by their consumers. Lifecycle tools may manage files, but packaging does not create another runtime primitive or `root route`.

## Sources And Scope

This document defines the Extension concept, package composition, and the boundary between installed content and lifecycle tools. Related sources define their own detail:

| Source | Question it answers |
| --- | --- |
| [Top Architecture](../architecture.md#framework-composition) | How do Core, Memory, and Extensions compose? |
| [Framework Architecture](../framework/architecture.md) | How are installed routes, roles, scope, loading, authority, and Memory interpreted? |
| [Extension command contracts](../cli/contracts/extension/_extension.md), including [package layout](../cli/contracts/extension/_extension.md#package-layout) | What package representation and managed operations do the current CLI contracts define? |
| [Consumer permission contracts](../cli/contracts/shared/workspace-permissions/_workspace-permissions.md) | Which permission grants are required for files consumed by other tools? |
| [First-party catalogue](../../../../../src/extensions/README.md) | Which packages exist, what do they contain, and how are their source files arranged and installed? |

The current command contracts define implementation-specific manifest rules, dependency resolution, lifecycle records, update and removal behavior, and operational safety requirements. They do not define the runtime meaning of installed Framework content.

## Architectural Invariants

These constraints apply independently of the implementation:

1. Extensions remain optional.
2. Explicit user selection precedes installation.
3. Installed files carry the Extension's complete runtime meaning.
4. Extension metadata is not required for agent interpretation.
5. Routed Extension content uses ordinary `routes` and relationships.
6. Packages contribute whole files. They do not inject private changes into shared Markdown.
7. Every installed managed path has explicit ownership.
8. Dependency edges compose installable units without redefining runtime relationships.
9. Manual installation through ordinary files remains possible.
10. Installation effects are reviewable before they become trusted context.
11. Removal protects user changes, shared ownership, dependencies, and route reachability.
12. Catalogue and source organization do not determine authority.

## Package Composition

Package identity identifies an installation unit. Its source location and subject do not determine the meaning or scope of its installed files.

Dependencies connect installable packages. They arrange for required content to be assembled, while installed files express their runtime relationships through ordinary links and instructions. For example, a Workflow package may depend on a package that provides a Skill. The installed Workflow still identifies the Skill it uses; the package dependency is not an agent instruction.

A convenience pack may declare only dependencies, contribute coordinated files, or combine both. Shared content should have one package source and be reused through dependencies instead of copied into competing packages.

## Reusable Content And Project Context

Generic first-party capabilities provide useful methods and starting shapes. The workspace supplies the project facts and accepted requirements through its selected scopes. A development recipe can explain how to investigate, implement, and verify a change while the project's sources define its technologies, commands, conventions, and evidence requirements.

Templates leave the subject's facts as removable prompts. Existing project records and formats can provide the same information without a parallel set of files. Each copied result is maintained independently within its destination's scope and role.

The [Framework change instructions](../../../../directives/open-forge/framework/deliberate-framework-change.md) bind repository authors to this boundary. A capability may specialize its stated subject; package identity does not make it authoritative over other project concerns.

## Installed Interpretation

An installed Directive remains required within its loaded scope. A Workflow remains an optional recipe. Memory retains its destination's state and authority. Packaging does not change any of these roles.

The `#Extension` tag identifies optional package provenance and composition. It does not activate content or create authority. Generated `Entries` expose installed routes; the destination files define their meaning.

Links resolve relative to the installed file in the assembled workspace. References within a package should resolve within its content. References to Framework files or declared dependencies may require the combined installation.

Agents interpret the installed files through the Framework and the native formats those files use. They do not need the package manifest, catalogue, dependency metadata, lifecycle records, or CLI for that interpretation.

## Manual Installation And Managed Ownership

Manual installation copies the content of the selected package and its dependencies into the workspace, updates affected `Entries`, and reviews the assembled files and relationships. The result remains usable without the CLI. Manual copying does not by itself establish managed ownership.

Lifecycle tools reduce manual work through package discovery, dependency planning, navigation maintenance, and supported installation, update, and removal operations. Installation and updates remain user-directed setup and maintenance concerns.

Managed operations act through explicit ownership and protect user changes, shared files, required dependencies, and retained route reachability. Matching bytes alone do not authorize a manager to adopt an existing file. [Overwrite companions](../framework/routing/overwrites.md) belong to the workspace and remain outside Extension ownership.

Lifecycle metadata supports management of files. It does not grant those files authority or replace their content as the source of runtime meaning.

## Native Formats And External Managers

Keep native formats such as `SKILL.md` with their own metadata and resources. Routing a native package does not require wrapping or rewriting it as another Framework category.

Users and external tools may install native content independently. Exposing that content through ordinary routes does not transfer its ownership to Open Forge. Two managers must not claim the same installed path.

An externally installed capability does not automatically satisfy a package dependency. Such substitution would need an explicit identity, compatibility, and ownership model. Routing the capability does not create that model.

## Evidence And Evolution

Verify package contents and their assembled dependencies against the relevant Framework and lifecycle contracts. Distinguish successful installation and structural checks from evidence that the content helps its users. Claims about efficiency, reliability, or reusable value need evidence for the actual method and conditions.

Optional packages must not turn Core into a universal methodology. Continued distribution should be justified by useful outcomes, clarity, maintenance cost, and evidence of use. Evaluate each capability by how it helps achieve its stated goal.

The [Extensions Evolution candidate](../../../emerging/ideas/extensions-overhaul.md) keeps questions about distribution, compatibility, migrations, richer dependencies, ownership across workspaces or managers, and catalogue governance contextual. Those questions do not establish additional package or runtime capabilities.

## Decisions And Rationale

The [Extension Package Boundary Decision](../../decisions/extensions/extension-package-boundary.md) records why optional whole files, explicit ownership, and complete installed interpretation were chosen.

The [focused package decision](../../decisions/extensions/focused-extension-packages.md) records the accepted catalogue split, convenience bundle, and Experience Design removal. The catalogue remains the defining source for the package inventory.

## Historical Context

The [frozen MVP architecture record](../../../archived/extensions-mvp-architecture.md) preserves the former implementation's package, ownership, lifecycle, safety, recorded verification, and limitations. That history may inform later decisions. It does not define current behavior.
