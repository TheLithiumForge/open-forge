---
open-forge:
  description: Share skills between extensions by name at build time and place them once under skills/ at install time
  tags: [Memory, Idea, Contextual, Candidate, Extension, Skill, CLI]
---

# Extension Skill Sharing

Direction sketched 2026-07-08. Sharing skills between extensions at build time and install time is the truly unsolved gap; no pattern exists yet.

- Extensions are effectively the true structure of the payload being installed, and external users must be able to author their own, so the sharing system must be a general mechanism rather than a first-party convention.
- Candidate mechanism: an extension declares shared skills by name or id; the build copies each shared skill into the extension package; the install places skills under `.agents/skills/` once, deduplicating across selected extensions.
- Skills stay native skill packages (`SKILL.md`) so runtimes keep discovering and invoking them; workflows reference them by path or package name instead of embedding copies.
- Installed files remain runtime truth; manifests may help install and dedupe, but agents must not need them at runtime.
- Open: dedupe and version behavior when two extensions carry different versions of the same shared skill.
- Related: the workflow-redesign idea consumes this by referencing shared skills from `Required Routes`.
