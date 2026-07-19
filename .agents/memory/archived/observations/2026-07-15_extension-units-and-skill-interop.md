---
open-forge:
  description: Historical extension-composition finding about derived payload kinds, native primitives, dependencies, and one installed-path owner
  tags: [Memory, Archived, Observation, AgentLearning, Contextual, Historical, Extension, Skill, CLI, Interoperability, Dependency]
---

# Observation: Compose Packages, Preserve Primitive Identity

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/observations/2026-07-15_extension-units-and-skill-interop.md`.  
Archived because: the content-agnostic dependency model and native-skill interoperability rules were accepted; unresolved ownership lifecycle work moved to planning.  
Current owner or replacement: `.agents/memory/crystallized/decisions/extensions-and-cli.md`, extension documentation and tests, and `.agents/memory/working/backlog.md`.

Date: 2026-07-15. Scope: the first-party catalogue decomposition, dependency selector, native skill validation, and Microsoft APM compatibility review.

- Treating an extension as an exclusive kind such as workflow pack or skill pack creates special dependency rules and prevents narrow on-demand selection. Treating it as the one content-agnostic installable unit lets the same graph compose skill-only, workflow-only, directive-only, mixed, and dependency-only packages.
- Runtime meaning still belongs to installed files. A native `SKILL.md` remains a skill and a routed workflow remains a workflow; manifest dependency edges only guarantee that required files arrive together.
- Deriving catalogue contents from payload paths avoids a second author-maintained inventory that can drift from runtime truth. Dependency-only packs remain visible as packs without installing placeholder files.
- Shared skills compose safely when one extension owns each installed path and workflows reference that extension at install time plus its concrete `SKILL.md` route at runtime. Identical-byte deduplication remains a safety boundary, not an ownership model.
- External skill managers need no adapter when they deploy the same native `.agents/skills/{name}/SKILL.md` shape. The interoperability handoff is to regenerate routing after installation and never let two managers claim the same installed skill path.

Promotion candidate: preserve the package-versus-primitive distinction and one-owner rule in future registry, update/remove, provenance, and third-party extension designs.
