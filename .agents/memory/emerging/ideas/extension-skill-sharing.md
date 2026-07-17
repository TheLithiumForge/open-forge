---
open-forge:
  description: Share skills between extensions by name at build time and place them once under skills/ at install time
  tags: [Memory, Idea, Contextual, Candidate, Extension, Skill, CLI]
---

# Extension Skill Sharing

Direction sketched 2026-07-08. Install-time sharing through content-agnostic dependency extensions was implemented 2026-07-15 and expanded to the first-party architecture, vision, planning, implementation, and quality capabilities. Build-time vendoring, version compatibility, and update/remove ownership remain open.

## Implemented Install-Time Shape

- A shared native skill lives once in its own bundled extension, such as `architecture-capability`, `planning-capability`, or `implementation-capability`.
- Dependent manifests declare bundled extension ids in `dependencies`; the CLI resolves the offline transitive graph in dependency-first order.
- Workflows still name concrete installed `Required Routes`; the manifest is never runtime routing truth.
- Different bytes targeting one path block during preflight. Identical bytes are deduplicated within the install plan.
- Real-pack integration tests prove each advertised pack and the aggregate composition resolve every workflow dependency.
- The catalogue derives each extension's payload kinds, while the selector visibly auto-selects and locks transitive capability dependencies. Dependency-only convenience packs compose existing extensions without duplicate runtime files.
- Directly copied or externally managed native skills remain supported under `.agents/skills/`; `open-forge index` routes them. Microsoft APM's cross-tool skill target uses that same destination, so no adapter or duplicated `.apm/` source tree is required.

## Remaining

- Extensions are the general composition mechanism for first-party and local payloads; dependencies reference extensions rather than inventing a skill-specific dependency field.
- Optional future build-time vendoring may help two independently distributed extensions carry a shared skill, but first-party packages use one canonical owner and dependency edges instead of copying it.
- Skills stay native skill packages (`SKILL.md`) so runtimes keep discovering and invoking them; workflows reference them by path or package name instead of embedding copies.
- Installed files remain runtime truth; manifests may help install and dedupe, but agents must not need them at runtime.
- Open: version solving, persistent ownership, update/remove behavior, provenance across multiple managers, and optional build-time vendoring when two independently distributed extensions need the same shared skill.
- Related: the workflow-redesign idea consumes this by referencing shared skills from `Required Routes`.
