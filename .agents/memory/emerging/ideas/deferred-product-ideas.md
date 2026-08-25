---
open-forge:
  description: Deferred product ideas for reliability evaluation, route presets, provider adapters, formatting, technology packs, orchestration, and memory placement
  tags: [Memory, Idea, Contextual, Candidate, Product, Extension]
---

# Deferred Product Ideas

Open product candidates after the 2026-07-18 pruning pass. These are options, not commitments. The complete pre-pruning record is `.agents/memory/archived/ideas/2026-07-18_deferred-product-ideas-snapshot.md`.

- Optional typed grouping routes such as `projects/`, `packages/`, `domains/`, or `teams/` as CLI presets, not base defaults.
- Technology pattern packs.
- Add another Workflow only where real use shows that a distinct repeatable recipe materially improves an outcome beyond native agent capability and the six `development-toolkit` Workflows.
- Optional communication, surgical-change, implementation, design, brainstorming, testing, and review content should ship only through the Core role that matches its semantics and only after real use shows that it earns another default or Extension.
- Explore a Skill that preserves the current chat as a local Session record.
- [Composable Workflow entrypoints](composable-workflow-entrypoints.md) that keep a generic recipe usable while exposing opt-in agent, isolation, execution-policy, and runtime variants through ordinary routing.
- [Optional task work modes](task-work-modes.md) ranging from a lightweight linked backlog to sprint planning, with declared authority and open interoperability with external trackers.
- Optional structured deliberation capability for consequential choices, potentially using several independent perspectives and an explicit synthesis without replacing user authority.
- Post-initial CLI candidates such as token-budget filters, approximate token
  counts, startup-budget warnings, intent-to-route previews, additional
  Template authoring assistance, and stronger explicitly named upgrade modes.
- External `.memory/` or distributed package-local memory as a documented user pattern, not default behavior.
- **AST-based Index region boundaries:** Analyze whether the replacement Index
  command should find one ordered generated-marker pair anywhere inside the
  parsed `## Entries` section instead of requiring the start marker immediately
  after the heading. Authored explanation before or after the managed region
  should remain untouched. Evidence should cover nested headings, marker-like
  code examples, duplicate or missing markers, multiple `Entries` headings,
  line-ending preservation, dry-run parity, and idempotent apply.
- **Marker-free generated navigation:** Separately analyze whether the Markdown
  AST can make generated marker comments unnecessary. Keep markers unless a new
  ownership rule can identify exactly which AST nodes Index owns without risking
  an authored list, and can migrate existing workspaces while preserving bytes,
  formatting, and malformed-region safety.
