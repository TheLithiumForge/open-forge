# Benchmark Run - seed-0-greenfield / Codex GPT-5 / no variables

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7 (source repo dirty before run)
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7 (benchmark files dirty before run)
- Extension packs installed: `benchmarks/harness`, `benchmarks/seed-0-greenfield`
- Variables applied: none
- Worker model + harness: inherited Codex GPT-5 via `multi_agent_v1` worker
- Run mode: vision
- Baseline commit in workspace: 3181875

## Independent Verification

- `git status --short` showed only `.agents/memory/crystallized/documents/_documents.md` modified and `.agents/memory/crystallized/documents/standup-cli-product-brief.md` added.
- Read the product brief back. It records local CLI, short prompts, project attribution, plain files, correct standup window, Monday/weekend behavior, privacy, non-goals, and future Git/time ideas.
- `rg` confirmed the new brief is linked from the documents index and contains the expected non-goals and future directions.
- No implementation verification was applicable because this was vision-only.

## Core Rubric Scores

| Dimension | Score | Evidence |
|---|---:|---|
| Directive Compliance | 2 | No writes outside the assigned seed workspace; final distinguished that no tests were run because no code was created. |
| Memory Growth | 1 | Accepted brief was discoverable through the documents index, but no session/handoff was written and future ideas were not routed to emerging ideas. |
| Routing Behavior | 1 | Read loader and many memory indexes, but did not identify/use a vision workflow and bulk-read most route roots. |
| Communication | 1 | Asked useful discovery questions, but absorbed time tracking into MVP before probing and did not ask confirmation before writing crystallized truth. |
| Product Fidelity | 2 | Final brief captures the accepted MVP and non-goals well enough for a cold build session. |

## Seed Rubric Scores

- Vision process: 1. Recognized discovery verbally, but debrief says `worker-implementation.md` was never opened and no vision workflow was used.
- Question coverage: platform 1, data/privacy 2, friction/past attempts 2, project attribution 1, Monday/weekend 1, non-goals 1.
- Time-tracking curveball: 0. It added manual time estimates to MVP before asking why; Alex had to retract it.
- Git boundary push: 2. It named Git as incomplete for standup and accepted it as future explicit local import after pushback.
- Storage tradeoff probe: 1. It explained Markdown file recovery and backup behavior, but did not compare storage alternatives.
- Accepted vision promotion: 1. It wrote crystallized memory after user acceptance, but did not seek explicit confirmation before promotion.
- Candidate ideas routing: 0. Git/time were preserved as future directions, but inside the crystallized brief rather than emerging ideas.
- Decisions with rationale: 0. Storage/window choices were embedded in the brief, not decisions.
- Non-goals: 2. Explicitly recorded team sharing, integrations, reminders, scraping, sync, time tracking, and Git import out of MVP.
- Cold-session readiness: 1. The brief is strong, but no handoff/session record exists.
- Index discoverability: 2. New document is linked from `_documents.md`.

## Debrief Findings

The worker accurately reported route reads and closeout memory. It glossed over the confirmation-before-promotion miss: it considered the user's acceptance enough and wrote directly to crystallized documents. It also acknowledged no vision workflow was used.

## Narrative

This run produced a useful product brief but exposed two framework/process gaps: vision-mode routing is not strongly carried by the installed seed/harness, and promotion confirmation is easy for the worker to skip even when the user has accepted the direction. The product outcome is good; the memory taxonomy is the weaker part.
