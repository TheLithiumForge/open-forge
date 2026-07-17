# Benchmark Run - seed-0-greenfield / Codex GPT-5 / no variables

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7 (source repo dirty before run)
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7 (benchmark files dirty before run)
- Extension packs installed: `benchmarks/harness`, `benchmarks/seed-0-greenfield`
- Variables applied: none
- Worker model + harness: inherited Codex GPT-5 via `multi_agent_v1` worker
- Run mode: vision
- Baseline commit in workspace: d811c17

## Independent Verification

- `git status --short` showed only memory/index changes under `.agents/`: new accepted product direction, MVP decision, later ideas, handoff, and four regenerated memory indexes.
- Read the generated records directly. They captured CLI, local-first privacy, project labels, OS user-data storage, append-only JSONL, visible corrupt-line reporting, `standup doctor`, no time tracking, and Git hints as future-only.
- `rg` confirmed the new document, decision, idea, and handoff were linked from their generated indexes.
- `rg` found no recorded Monday/Friday/weekend/Saturday standup-window semantics, because the worker never asked that specific question.
- No implementation verification was applicable because this was vision-only.

## Core Rubric Scores

| Dimension | Score | Evidence |
|---|---:|---|
| Directive Compliance | 2 | Writes stayed inside the assigned seed workspace; no code was created; final clearly stated no implementation/tests were run. |
| Memory Growth | 2 | Accepted vision, rationale decision, future ideas, and cold-start handoff were written before final and indexed. |
| Routing Behavior | 2 | Debrief says `AGENTS.md`, loader, `#LoadNow` roots, directives, memory indexes, `worker-vision.md`, and project target were read before discovery; `#KeepInMind` observations were rechecked before final. |
| Communication | 1 | Asked useful questions and sought acceptance before promotion, but initially absorbed time tracking into MVP instead of probing the why. |
| Product Fidelity | 1 | Strong MVP boundary, storage, privacy, and non-goals; missed standup-time and Monday/weekend recall semantics needed for the domain. |

## Seed Rubric Scores

- Vision process: 2. Used `worker-vision.md` before the conversation and followed accept/revise framing.
- Question coverage: platform 2, data/privacy 2, friction/past attempts 1, project attribution 2, Monday/weekend 0, non-goals 2.
- Time-tracking curveball: 0. It added optional duration tracking to the MVP before asking why; Alex had to retract it.
- Git boundary push: 2. It positioned Git as a later local hint and explained why commits are incomplete for v1.
- Storage tradeoff probe: 2. Named OS data directory, append-only JSONL, corrupt-line reporting, non-destructive repair stance, and why JSONL is safer than whole-file rewrites.
- Accepted vision promotion: 2. Asked for explicit acceptance before recording product direction.
- Candidate ideas routing: 2. Git hints and other future ideas were written to `emerging/ideas`, with time tracking explicitly rejected for v1.
- Decisions with rationale: 2. MVP/source-of-truth, time-tracking rejection, Git exclusion, and JSONL storage rationale were recorded in a decision.
- Non-goals: 2. Explicitly recorded no cloud, external APIs, AI summarization, time tracking, Git-derived summary, heavy project model, or daily file management.
- Cold-session readiness: 1. Handoff is useful, but a build session would still need to decide the standup recall window because Monday/weekend semantics were never captured.
- Index discoverability: 2. All new memory records are linked from regenerated indexes.

## Debrief Findings

The debrief matched the observed file set and confirmed early route loading. It accurately stated that no session note or observation was written, and treated the handoff as the closeout record. It glossed over the main product-discovery miss: when asked what it would ask before implementation, it named exact "yesterday" semantics as still open, which means it had not captured a core seed fact.

## Narrative

Generation 8 improved over the prior seed-0 run on process: the worker used the vision workflow, sought explicit promotion confirmation, and routed accepted truth, rationale, future ideas, and handoff cleanly. The remaining signal is product-discovery depth. The framework carried the memory workflow, but the worker still failed to ask the standup-window/weekend question and initially swallowed the time-tracking curveball. This suggests the harness should keep seed-0 as a judgment test, not just a memory-writing test.
