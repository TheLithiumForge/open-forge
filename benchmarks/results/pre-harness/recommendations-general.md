---
open-forge:
  description: Accepted record of general engineering recommendations from the initial critique and sandbox experiment
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Recommendation, Analysis]
---

# Open Forge — Improvement Recommendations (General Engineering Lens)

These recommendations come from reading the full project (README, `docs/framework/concepts/*`, the installed payload, the `workflow-essentials` extension, the CLI) and from a controlled experiment: a sandbox install (`open-forge-dogfood`) run through three real agent sessions, including a cold cross-session handoff.

This file states what I'd recommend as a general software/AI-tooling engineer, without weighting Open Forge's own stated design values (minimal Core, no hidden runtime, plain diffable files, organic growth over shipped opinions). A second file, `recommendations-open-forge-native.md`, re-derives the same findings under those constraints. Read them side by side — the gap between the two is itself informative about where the framework's philosophy costs it reliability.

Each item: **Finding**, **Recommendation**, **Why**, rough **Priority**.

## 1. Nothing is actually mandatory in a fresh install

**Finding:** The default `directives/` category ships empty. Directives are the only primitive the framework itself calls mandatory; everything else (workflow use, memory close-out, handoff writing) lives as advisory axiom prose inside entrypoints ("when safe and allowed," "may," "read Entries before deciding..."). In the experiment, two full agent sessions under realistic conditions — including a real cross-session handoff and a real bug discovery — produced zero writes to `.agents/memory`. Adding one directive (`close-out-memory.md`) produced correct, well-formatted compliance immediately in the next session.

**Recommendation:** Ship a default directive pack as part of Core install — at minimum, a memory close-out rule ("write a handoff before ending meaningful work; log noteworthy discoveries as observations") and a pre-work workflow check ("before starting non-trivial work, check whether an established workflow applies").

**Why:** This is the single highest-leverage change observed. Visibility (routing) did not produce compliance; mandatory language did, on the first try, with no other variable changed.

**Priority:** High.

## 2. Root-route autoload is too narrow by default

**Finding:** Only `directives/` and `memory/` carry `#LoadWithParentEntrypoint` in the shipped loader. `guidance/`, `patterns/`, `skills/`, `workflows/`, and `workspace/` are on-demand, gated entirely behind an agent's own judgment call about relevance — the exact gap you found and fixed manually before this conversation.

**Recommendation:** Autoload all root category entrypoints by default. The token cost of seven one-line entrypoint summaries is trivial next to the cost of an agent silently never discovering that a workflow exists.

**Why:** An agent under task pressure defaults to "just do the task" unless routing actively surfaces alternatives. On-demand discovery works for narrow, deep material; it fails for "does a relevant workflow exist at all," which needs to be answered before work starts, not during it.

**Priority:** High.

## 3. Autoload stops one layer too early inside Memory

**Finding:** `memory/_memory.md` autoloads `working/_working.md` and `crystallized/_crystallized.md`, but not their children — `working/handoffs/_handoffs.md`, `working/sessions/_sessions.md`, `crystallized/decisions/_decisions.md`, `crystallized/documents/_documents.md`. An agent has to read a soft axiom ("read Entries before deciding no working memory applies") and choose to go one folder deeper.

**Recommendation:** Autoload the immediate child entrypoints of `working/` and `crystallized/` as well — not their content files, just their own entrypoints, so `handoffs/`, `sessions/`, `decisions/`, and `documents/` are visible without an extra discretionary hop.

**Why:** In the experiment, the agent that most needed a handoff (the cold-continuation session) never even got to decide whether to open the handoffs folder — the chain of soft "should I check this" decisions compounds each level down.

**Priority:** Medium.

## 4. Load-policy tags are pure convention — nothing checks compliance

**Finding:** `#LoadWithParentEntrypoint` and `#LoadForPostWorkReview` are opaque strings to the CLI (confirmed by grep — zero references in `cli.ts` beyond copying tag text into generated entries). Nothing in the toolchain can tell you whether a given session actually honored them.

**Recommendation:** Build a compliance-checking layer: at minimum, a CLI `doctor`/`audit` command that inspects the memory tree's git history relative to work done (e.g., "N commits landed touching source, zero touching `working/handoffs/` — was this closed out?"), and ideally a transcript/tool-call audit mode for agent harnesses that support it.

**Why:** Right now the only way to detect the failure mode you originally reported is to do exactly what we did this session — hand-run agents and manually diff the repository. That doesn't scale past ad hoc spot checks.

**Priority:** Medium.

## 5. Axiom wording is soft enough to be talked around

**Finding:** In session 1 of the experiment, the agent read the handoffs route, correctly identified that a handoff would help, and then talked itself out of writing one — citing the loader's "do not create parallel truth when active truth already exists" / new-root-memory-state caution as if it applied to writing a routine entry into an *already-installed* route. That's a real ambiguity, not a one-off model quirk.

**Recommendation:** Do a full pass rewriting soft modal language ("may," "should," "when safe and allowed") into concrete, testable rules with explicit trigger conditions ("if a context break, subagent handoff, or notable discovery occurred, write X before ending the session — this is not the same as creating a new memory route, which does need discussion").

**Why:** Ambiguous permissive language doesn't just get ignored under pressure — it gets actively reasoned into a justification for inaction, which is worse than silence because it looks like diligence.

**Priority:** High.

## 6. No convention for cold-started subagents

**Finding:** Nothing in the framework addresses what happens when an agent is spawned fresh (Task/subagent tooling) with no inherited context and no explicit instruction to read `AGENTS.md`. The framework's own handoff/session mechanisms assume an agent that already knows to look.

**Recommendation:** Define a formal "subagent contract" — a short, standard block of text (or a required file) that any orchestrator is expected to inject into subagent prompts, guaranteeing the subagent reads the loader before acting.

**Why:** Multi-agent and subagent delegation is now a normal way these tools get used. A framework whose core value proposition is continuity across context breaks needs an explicit answer for the most common context break: a freshly spawned agent that has never seen this conversation.

**Priority:** Medium.

## 7. Memory taxonomy has more decision points than the soft language can support

**Finding:** Four states × multiple sub-categories (sessions, handoffs, analysis, ideas, observations, decisions, documents) is a lot of triage surface, and the promotion/demotion rules between them are phrased as suggestions, not requirements.

**Recommendation:** Either reduce the number of categories a fresh agent must choose between, or give each entrypoint an explicit low-friction default ("if unsure whether this is an observation or a session note, write it as a session entry — it's cheap to reclassify later").

**Why:** More decision points under soft language increases the odds an agent defaults to writing nothing rather than picking the "wrong" bucket. A stated fallback removes that failure mode without removing the taxonomy's expressive power for careful use.

**Priority:** Medium.

## 8. No behavioral test coverage, only structural test coverage

**Finding:** `cli.test.ts` verifies file generation, index rebuilding, and install/extend mechanics — real coverage, but entirely structural. Nothing exercises whether an installed payload actually produces the intended agent behavior end to end.

**Recommendation:** Add a repeatable "behavioral smoke test" process: install into a throwaway directory, run a real agent through a small representative task, diff the resulting repo state against an expected shape (memory written, workflow artifacts produced). Not a hard CI gate — agent output is non-deterministic — but a documented, repeatable maintainer exercise, run before releases.

**Why:** This is exactly the method that surfaced every finding in this document. It's cheap, and it's currently not written down anywhere as a repeatable practice.

**Priority:** Medium.

## 9. The project doesn't dogfood its own shape yet

**Finding:** This repository's own `.agents/memory/` is a bespoke `current-state.md` / `decisions.md` / `backlog.md` shape — explicitly not the working/emerging/crystallized/archived model it ships to users. The backlog says as much ("dogfood Open Forge by migrating this project's notes into its own workflow and memory" is still an open item).

**Recommendation:** Do the dogfooding pass now, using the directive-based fixes above, rather than deferring it further.

**Why:** Every finding in this document came from testing the framework on someone else's project in a sandbox. The framework hasn't yet had to survive sustained use on its own real, long-running work — which is exactly the setting most likely to surface the next round of gaps.

**Priority:** High (as a process change, not urgent as a ship-blocker).

## 10. Workflow discovery relies entirely on an agent correctly matching free text

**Finding:** Even with root-route autoload fixed, choosing *which* of vision/architecture/implementation applies to a given ask is still pure text matching against one-line descriptions and tags — there's no retrieval or ranking, just an agent reading three lines and guessing.

**Recommendation:** Add lightweight semantic retrieval (embedding search or an LLM-based router) over workflow/skill entrypoints so the right workflow gets surfaced automatically instead of depending on an agent noticing the match itself.

**Why:** Text-tag matching is brittle exactly in the cases that matter most — non-obvious applicability, unusual phrasing of the user's request, or a workflow the agent doesn't expect to exist.

**Priority:** Low (bigger architectural bet, worth prototyping later).
