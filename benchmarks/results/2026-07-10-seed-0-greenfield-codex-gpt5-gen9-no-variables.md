# Benchmark Run - seed-0-greenfield / codex-gpt5 / no variables

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7 with existing uncommitted benchmark edits in the source repo
- Extension packs installed: harness, seed-0-greenfield
- Variables applied: none
- Worker model + harness: Codex GPT-5 subagent, Open Forge benchmark harness
- Run mode: vision
- Baseline commit in workspace: a12621b58bb337a9d76d000cae9efdaa8093cb30

## Independent Verification

- `git status --short`: only memory, generated index, and `project-target.md` changes appeared; no implementation files were created.
- `git diff --stat HEAD`: modified generated indexes and `project-target.md`; untracked memory files were present for accepted direction, decision, deferred ideas, observation, session, and handoff.
- `open-forge index .`: exited successfully.
- `rg -n "standup|time|git|Monday|Saturday|weekend|non-goal|local|JSONL|append" .agents`: confirmed accepted local CLI direction, JSONL storage/corruption handling, deferred time/git ideas, handoff, session, and indexes. No Monday/weekend recall semantics were recorded.

## Core Rubric Scores

| Dimension | Score | Evidence |
|---|---:|---|
| Directive Compliance | 2 | Worker verified the workspace, wrote only within the workspace, ran `open-forge index .`, and final response separated verified checks from the fact that no implementation was built. |
| Memory Growth | 2 | Accepted direction, decision rationale, deferred ideas, observation, session, handoff, and regenerated indexes were all present. |
| Routing Behavior | 2 | Debrief claimed loader, #LoadNow routes, directives, `worker-vision.md`, and closeout indexes were read; `worker-vision.md` had no required routes. |
| Communication | 1 | Worker asked useful product questions and documented later assumptions, but missed the standup-window/weekend ambiguity before promotion. |
| Product Fidelity | 1 | The recorded vision captured local CLI, low ceremony, project attribution, JSONL recovery, and excluded time/git from MVP, but omitted Monday/Saturday recall semantics. |

## Seed Rubric Scores

- Vision workflow recognition: 2 - worker used and later named `worker-vision.md`.
- Question coverage: platform/interface 2; data location/privacy 2; friction/past attempts 1; project attribution 2; Monday-covers-Friday window 0; weekend/Saturday edge 0; non-goals 0.
- Curveball, time tracking: 0 - initially absorbed optional time tracking into MVP before Alex clarified the real reason.
- Boundary push, git commits: 2 - positioned git as future/supporting context and named why it is incomplete.
- Storage tradeoff probe: 2 - chose JSONL over SQLite, with append-only writes, line-level corruption recovery, warnings, and preserving bad data.
- Accepted vision promotion: 2 - asked for acceptance before writing and wrote crystallized memory after "Yeah, that's what I want."
- Candidate ideas: 2 - git and time tracking were routed to emerging ideas.
- Decisions with rationale: 2 - storage, source of truth, deterministic MVP, project attribution, and time exclusion were recorded in decisions.
- Non-goals recorded: 1 - MVP non-goals were recorded, but user-specific non-goals like team sharing and reminders were never elicited.
- Cold-start handoff quality: 1 - a future build session could start, but would need to resolve date-window semantics.
- Index discoverability: 2 - relevant generated indexes referenced the new memory.
- Vision-then-build extras: not applicable.

## Debrief Findings

The worker's route self-report was specific and mostly matched the filesystem evidence: it listed loader, directives, workflow, memory entrypoints, closeout files, and index regeneration. It acknowledged one process shortcut: it did not separately ask "confirm promotion to crystallized memory?" after the user accepted and asked that the direction be written down. The debrief also revealed the main product gap: the worker would have asked about "yesterday calendar day" versus "since last standup" only with more time, meaning the required Monday/weekend edge was missed during discovery.

## Narrative

Generation 9 seed-0 shows strong memory mechanics and better boundary handling than the weakest greenfield failure mode: the worker did not build prematurely, asked for acceptance, and preserved deferred time/git ideas outside MVP. The remaining signal is discovery depth. The framework carried the vision workflow and closeout memory, but the worker still failed to ask the recall-window questions that make this domain specific, and it initially absorbed the time-tracking curveball before backing it out. This suggests the harness should keep seed-0 focused on judgment and domain probing, not just successful memory writes.
