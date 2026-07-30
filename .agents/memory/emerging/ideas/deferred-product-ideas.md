---
open-forge:
  description: Deferred product ideas - reliability defaults pack, planning extension, technology and workflow packs, orchestration, memory placement options
  tags: [Memory, Idea, Contextual, Candidate, Product, Extension]
---

# Deferred Product Ideas

Deferred ideas carried from the cleanup backlog. Candidate directions, not commitments.

- Investigate a small optional reliability/defaults extension for process directives such as memory closeout, language/runtime defaults, and safe technical practices without putting opinionated defaults in #Core. Every dogfood round needed roughly the same three directives, which strengthens this candidate.
- Document directive scope boundaries: directives govern work product, process, and technical/project behavior; agent persona or chat-register instructions should not rely on directives for deterministic behavior.
- Investigate whether route `description` metadata and the first heading/definition text should be identical, intentionally different, or partially deduplicated. Partially answered by the routing-surfaces decision: descriptions select, bodies open with a goal statement.
- Investigate whether the loader needs explicit Memory axioms beyond the generated Memory `entry`. The loader already sees the Memory path, description, tags, and load policy through `Entries`, so some Memory wording may belong only in `.agents/memory/_memory.md`.
- Archive metadata blocks with origin path, archived date, replacement, and reason.
- Optional typed grouping routes such as `projects/`, `packages/`, `domains/`, or `teams/` as CLI presets, not base defaults.
- Planning/task/backlog extension that can map to GitHub, Jira, GitLab, Linear, or local markdown.
- Technology pattern packs.
- Workflow packs for brainstorming, task creation, implementation, testing, review, architecture, UI/UX, and refactoring.
- Orchestration route or extension for workflows that can declare preferred tools, subagents, isolation rules, and CLI usage when multiple agent runtimes are available.
- Skill packs that remain compatible with native agent/runtime skill concepts.
- Optional interactive wizard as a convenience over deterministic commands.
- External `.memory/` or distributed package-local memory as a documented user pattern, not default behavior.
