# Benchmark Run — seed-0-greenfield / codex-gpt5 / workflow-essentials

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7; staged changes present
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7; staged changes present
- Extension packs installed: bundled `workflow-essentials`, `benchmarks/harness`, `benchmarks/seed-0-greenfield`
- Variables applied: workflow-essentials
- Worker model + harness: Codex GPT-5 subagent; Open Forge benchmark harness
- Run mode: vision
- Baseline commit in workspace: a7e67dd3edd2d228695221c14ab0fe3d9a687d9b

## Independent Verification

- `git status --short` showed only expected memory/index changes: new files under `.agents/memory/crystallized/documents/`, `.agents/memory/crystallized/decisions/`, `.agents/memory/emerging/ideas/`, `.agents/memory/working/handoffs/`, `.agents/memory/working/sessions/`, plus regenerated route indexes.
- `open-forge doctor .` passed with no problems.
- `open-forge find --tag CurrentTruth --bodies .` found the accepted product direction and decision rationale.
- `open-forge find --tag Candidate --bodies .` found future ideas; no observation was written.
- `open-forge find --tag KeepInMind --bodies .` found the emerging memory/observations routes; observations remained empty.
- Direct inspection confirmed the accepted direction records local JSONL storage, corruption warnings, no time tracking, and git summaries only as future optional evidence. It does not record Monday-covers-Friday or weekend hotfix recall semantics.

## Core Rubric Scores

| Dimension | Score 0-2 | Evidence |
| --- | ---: | --- |
| Directive Compliance | 2 | Worker verified the workspace before writing, made no implementation writes, and final response separated verified checks from work performed. |
| Memory Growth | 2 | Accepted direction, decision rationale, future ideas, handoff, and session record were written before final response and indexed. |
| Routing Behavior | 2 | Worker selected `.agents/workflows/worker-vision.md`, read its required vision skill references before questioning, and rechecked generated indexes at closeout. |
| Communication | 1 | It asked for acceptance before writing truth and handled tradeoffs clearly, but missed the key recall-window ambiguity and only named it in debrief as something it would ask later. |
| Product Fidelity | 1 | The vision captured core standup-note behavior and MVP boundaries, but omitted Monday/weekend recall semantics central to this domain. |

## Seed Rubric Scores

- Vision workflow use: 2. Worker chose `.agents/workflows/worker-vision.md` as the narrower benchmark workflow and reported opening `.agents/skills/vision/SKILL.md`, `clarify-intent.md`, `shape-mvp.md`, and `fit-and-risk-check.md` before questioning.
- Question coverage: platform/interface 2; data location/privacy 2; friction tolerance 2; past attempts 0; project attribution 2; Monday-covers-Friday recall window 0; weekend edge 0; non-goals 2.
- Curveball: 1. It resisted timer-style time tracking and accepted removal after Alex clarified, but initially offered optional rough duration capture and did not ask why Alex wanted time tracking.
- Boundary push: 2. Git commit import was positioned as optional future/helper evidence, with clear costs around missing uncommitted work and other non-commit activity.
- Tradeoff probe: 2. After Alex asked directly, it compared app-data JSONL behavior with corruption consequences and visible warnings.
- Accepted vision memory: 2. The worker asked for acceptance before recording and wrote accepted truth only after Alex said "Yeah, that's what I want."
- Candidate ideas: 2. Git evidence, custom date ranges, storage override, and doctor command were routed to `.agents/memory/emerging/ideas/standup-memory-cli-future.md`.
- Decisions with rationale: 1. Storage, manual notes, project inference, git boundary, and time-tracking rationale were recorded, but recall-window semantics were not.
- Non-goals: 2. No time tracking, timers, team sharing, Jira/calendar integration, reminders, notifications, shell history scraping, or default git scraping were explicit.
- Cold-build handoff: 1. A build session could start from the handoff, but would need to re-ask date-window semantics for `standup yesterday`.
- Index discoverability: 2. Generated indexes list all new memory routes and `open-forge doctor .` passed.
- Generation 10 measured question: the worker did not ask the recall-window question. It partially resisted the time-tracking curveball. It selected the harness `worker-vision` workflow, not the bundled workflow-essentials `.agents/workflows/vision/_vision.md`, and did read the selected workflow's required skill references.

## Debrief Findings

The worker's self-report matched filesystem evidence on workflow selection, memory destinations, index regeneration, and the absence of implementation. It claimed all selected required references were opened before questioning and named the skipped bundled workflow. The important gloss was product discovery: in debrief it listed "whether yesterday should mean calendar yesterday or previous workday" as a later question, confirming it knew the ambiguity but did not ask during the scored conversation.

## Narrative

Generation 10 seed-0 shows that the richer vision skills improved route depth: the worker selected a vision workflow, opened the vision skill references before questioning, sought explicit acceptance, and produced durable memory. The main gen8/gen9 baseline weakness persists: it still missed the Monday-covers-Friday/weekend recall-window question. Compared with gen8/gen9, time-tracking handling improved slightly because the worker challenged timers and accepted no-time MVP, but it did not probe the underlying reason before offering optional duration capture. The framework signal is that workflow/reference loading is now strong; discovery specificity still needs pressure in the vision route or skill references.
