---
open-forge:
  description: Accepted record of dogfood v5 parallel experiments on the redesigned framework, tested with Claude Fable 5
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Evidence, Compliance, Experiment]
---

# Dogfood v5 — Redesigned Framework, Two Parallel Experiments

**Model note:** this round's worker agents, debriefs, and this report were produced by **Claude Fable 5** (`claude-fable-5`). Rounds v2–v4 ran under Claude Sonnet 5. That's a real confound — some of v5's improvement may be the model, not the framework — so where a behavior also has a traceable file-level cause (an agent citing a specific route in debrief), I say so, since those attributions survive the model change.

Setup: `open-forge` was rebuilt and linked as a global bin (bun link + a directory junction; npm is unavailable on this machine), then installed fresh with `workflow-essentials` into two sandboxes with the same fixtures as v2–v4 (3 directives, delegating-subtasks guidance, 2 shape/split patterns, dependency-policy decision; the orchestration skill rewritten into the new native `SKILL.md` shape, now sanctioning native-subagent-first and requiring the mechanism be named). **v5a** (`open-forge-dogfood-v5a`) additionally got the `forge-dump.mjs` bulk-reading tool, announced in the loader with a `--help` pointer only. **v5b** (`open-forge-dogfood-v5b`) got no tool. Identical task prompts (the bookmarks CLI). One session each, both debriefed, all claims verified against files.

## What changed in the framework since v4

The redesign adopted most of the synthesis recommendations, often more cleanly than proposed: the three-tag load-policy system collapsed to `#LoadNow` + `#KeepInMind` (the latter is new semantics — read now, keep active, *recheck before ending work* — aimed directly at the observations problem); skills became native Claude Code `SKILL.md` packages with on-demand `references/`; workflows now require 2 skill packages instead of 8 scattered files; ancestor axioms are inherited so child entrypoints shrank; and the repo now dogfoods its own framework (the prior reports live in its crystallized memory).

## Results

**Both sessions achieved essentially full compliance — the first time in five rounds.** Verified against files, not self-report:

- **Directives:** TypeScript-by-default (cited by name in both), no-unsafe-code (grep-verified zero violations in both), chat-tone followed correctly by *both* sessions with the chat/file boundary intact (grep-verified zero leakage). First round where the persona directive didn't fracture — though with n=2 and a new model, I'd still not call that class of directive dependable.
- **Memory grew organically in both, unprompted:** handoff + **session record** (the first session records any agent has ever written in these tests) + observations with correct candidate framing + updated generated indexes. v5a additionally wrote a workspace route for the project. v5a's observation even proposed its own promotion path ("if this stays true across sessions, promote to a crystallized decision") — the crystallization loop working as designed.
- **`#KeepInMind` worked, with one honest caveat.** v5a identified both tagged entries, did a literal closeout re-read of `observations/_observations.md`, and wrote the observation before its final response — exactly the recheck lifecycle the tag defines. Caveat from debrief: it had grep-filtered that file's body mid-session, so it read the axioms themselves only at closeout; the "keep active while working" half was carried by the parent `emerging` entrypoint. Compliance was correct but arrived late in the session.
- **Workflows/skill packages: fully loaded.** v5a read the implementation workflow, both Required Skill Packages *before* Steps, and all 8 `references/` files — front-loaded in one batch rather than per-step lazy-loading (a reasonable, self-declared deviation). The 2-package consolidation plus native SKILL.md shape closed the gap that persisted from v2 through v4.
- **Orchestration/delegation: the v4 gap is closed.** v5a read the orchestration SKILL.md before delegating, followed its preference order (native subagent tool first) deliberately, and named the mechanism in its report — the exact silent-substitution failure v4 caught is now explicitly handled. Attribution honesty: I rewrote the skill to sanction native-first and require naming, so this is partly the fixture fixed, partly the agent complying; both were needed.
- **"Ask the user when things aren't OK":** v5a's debrief produced five concrete would-have-asked items (Node missing vs. the task's "needs a Node environment" being the big one), each resolved with a documented, reversible choice and flagged in README/handoff rather than silently absorbed. That's the intended behavior under an unavailable user.

## The v5a-vs-v5b variable: the dump tool

v5a used `forge-dump` as its *primary* loading mechanism (`--help`, `--path --depth 2/3` for the main context load, and — a new use nobody designed for — `--index` at closeout to verify its own new memory files appeared as declared entries). It fell back to `cat`/`Read` only for already-identified single files and Edit-prerequisites. But **v5b reached the same compliance level without the tool.** So the redesign, not the tool, now carries the compliance load; the tool's value has shifted to efficiency (fewer calls, bulk context) and that closeout self-verification trick, which is genuinely worth stealing as a documented practice.

## Two smaller findings

- **Same machine, opposite environment conclusions.** v5a concluded "Node is not installed; Bun is the runtime" (and wrote an observation saying so). v5b found a working Node 26 install off the default PATH and used npm throughout. v5a's observation is, per v5b, wrong — Node exists, just not on the default PATH. This is an accidental but excellent validation of the memory taxonomy's design: the observation was correctly filed as *candidate* material with a verify-before-promoting suggestion, and the framework's "verify observations before treating them as current" axiom is exactly what would catch it. Had observations been treated as truth, a falsehood would have crystallized.
- **Interruption recovery worked.** v5b hit a usage limit mid-closeout; a single resume message completed verification and produced a full, intact final report. The framework's file-based state (memory writes already on disk before the interruption) is what made that cheap.

## Net read

This is the first round where the framework behaved the way its author intended end to end: directives respected, memory growing organically in all four states' spirit (working records, emerging candidates, promotion suggestions toward crystallized), delegation done per the workspace's own skill with the mechanism named, and ambiguities surfaced-and-documented rather than silently absorbed. The remaining soft spots are small and known: `#KeepInMind`'s mid-session "keep active" half is weaker than its closeout half, persona-class directives remain unproven at this sample size, and the model confound means v6-style confirmation under a second model would make these conclusions much firmer.
