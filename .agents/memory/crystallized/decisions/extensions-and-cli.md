---
open-forge:
  description: Extensions add optional routed files through offline dependency-safe install plans while installed files remain runtime truth
  tags: [Memory, Decision, CurrentTruth, Extension, CLI]
---

# Extensions And CLI

Accepted decisions extracted from the design sessions and idea notes on 2026-07-06.

- #Extension is optional installable material added on top of #Core and #Memory. Extension is the content-agnostic installation unit, not another runtime primitive: one extension may contain a skill, workflow, directives, other routed material, a deliberate mix, or only dependencies as a convenience pack.
- Extensions should add files into the existing routed structure instead of creating another framework root.
- Extension payload routes use #Extension plus route type and scope tags by default when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` keep native metadata. Extension content stays relevance-routed and uses a load-policy tag only when baseline loading is deliberately chosen.
- The `extend` command installs local overlays, local extension packages with `payload/`, and bundled first-party extensions.
- Bundled first-party extensions live under `src/extensions/{id}/payload` in the CLI package.
- Extension manifests may describe display metadata, versions, and bundled extension dependencies. A dependency id may reference any bundled extension shape. Dependency-only bundled packs select closures without dummy runtime files. Dependency resolution is transitive, offline, dependency-first, and limited to packages already shipped with the CLI; it never fetches from a registry or network.
- The bundled catalogue derives payload content labels from files. Interactive selection shows the entire catalogue, automatically marks transitive dependencies as required, locks them while needed, and releases orphaned requirements. This display and selection state never replaces installed runtime routes.
- `extend --dry-run` previews resolved order and create/update/unchanged counts without writes.
- Installation preflights every resolved source file. Different bytes targeting one path are a blocker; identical bytes are deduplicated. In-process payload or index failures roll back applied files and generated regions.
- Direct local overlays remain unmanaged. The installer does not persist ownership, a lock, or hidden runtime state.
- Shared first-party skills have one canonical skill-only extension owner; dependent workflows name both the extension dependency at install time and the concrete installed Required Route at runtime. Native skills installed directly or by an external manager may coexist under `.agents/skills/`; reindexing routes them without taking ownership of their contents.
- Final extension design still needs route templates, trust/provenance receipts, compatibility/version solving, crash-recovery journaling, install/update/remove behavior, and richer authoring helpers.
- Installed files remain runtime truth. Manifests may help install and migrate, but agents should not need hidden manifests to route at runtime.
