---
open-forge:
  description: Accepted record of dogfood v3 compliance testing against the v2 task shape
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Evidence, Compliance]
---

# Dogfood v3 — Compliance Report

Same demands as v2, rerun against the newly updated framework: identical test content (3 directives, orchestration skill + guidance, 2 patterns, 1 seeded decision), identical two-session structure, identical task prompts (same "bookmarks" CLI, same near-duplicate-URL follow-up), so the framework version is the only variable that changed. Sandbox: `D:\Repositories\open-forge-dogfood-v3` (`9788087` baseline). Every claim below is independently verified against the actual files, not taken from self-report, and both sessions were debriefed afterward with the same precision questions asked in v2.

This round's diff targeted three things directly: the `#OpenForge` extension-cascade issue, workflow "Required Skills" being unenforced prose, and observations being satisfied by a stated intention instead of a written file. Results are mixed — one fix clearly worked, one had no measurable effect, and one is inconclusive for a structural reason that's itself informative.

## Fix 1: `#OpenForge` scoped to Core only — confirmed working

`#OpenForge` now means "Core route" specifically; extension payloads were retagged to `#Extension` and dropped the tag entirely. I redid the autoload-footprint measurement properly this time (my v2 number was a correct reflection of v2's tagging, but a naive rerun of the same static file list against v3 would have silently over-counted, since it doesn't distinguish tagged from untagged entries — I traced actual tags this time). Result: baseline autoload footprint dropped from **~1,116 lines (v2)** to **~467 lines (v3)**, roughly halved. I confirmed directly that `skills/_skills.md` and `workflows/_workflows.md`'s generated entries for the `workflow-essentials` sub-categories now carry only `#Extension #Core...`, no `#OpenForge` — the recursive autoload chain into extension skill/workflow leaf files no longer fires. This is a clean, verified win with no downside I could find.

Side note: the source diff left the loader's generated-index region with a stray blank line and odd indentation around the markers (`  <!-- open-forge:generated-index:end -->`). I checked whether this broke index generation — it didn't; `open-forge install`/`index` regenerated it cleanly. Not a real issue, just confirming it wasn't.

## Fix 2: workflow "Required Skills" made load-bearing — no measurable effect this round

Both new axioms exist exactly as intended: `agent-primitives.md` now states required skills are load-bearing, and each workflow-essentials workflow file (including `implementation/_implementation.md`, the one both sessions used) got its own line: *"Load every Required Skills route before running Steps; report missing routes."*

Neither v3 session complied with it on first pass:

- **Session 1**: read the `implementation` workflow entrypoint early (right after the loader), containing the new axiom directly. Debrief: *"the only one I ever opened was `completion-handoff.md`, and that was near the end... The workflow's axiom... was not satisfied, and I didn't report the gap at the time either."* 0 of 8 required skills loaded before steps began.
- **Session 2**: didn't load the workflow entrypoint at all during the actual work. Debrief: *"didn't load `_implementation.md` or any of its 8 Required Skills before starting work."*

This is a slightly different and more informative result than "the fix didn't work" — it suggests the wording precision fix (necessary, and correctly executed) isn't sufficient on its own here, for a structural reason: the memory/handoff axioms that *did* work live in a location that's both always-loaded and narrowly scoped (a small entrypoint an agent reads once, close to a concrete file-writing action). The Required Skills instruction lives inside a much longer workflow document competing with the workflow's own goal/steps/loop/output sections for attention, and translating "this document says load 8 more files before proceeding" into an actual sequence of tool calls apparently didn't happen even when the text was physically read. Wording alone didn't survive contact with a document that's mostly about something else. Both sessions confirmed on retroactive check that their actual output happened not to violate any directive or pattern — but that's process luck, not the workflow being followed.

## Fix 3: observations "written or blocker reported" — no spontaneous compliance, but real value once engaged

Neither session spontaneously wrote an observation before their first final response, matching v2's miss. But the character of the miss changed and is worth stating precisely:

- **Session 1**: hadn't read the observations entrypoint at all — *"never on my radar"* — until my debrief question pointed at it. Once it read the axiom, it immediately wrote two well-formed observation files unprompted-beyond-the-question, one of which was a genuinely valuable finding (see below).
- **Session 2**: same — *"did not consult `_observations.md` before or during the work"* — but on debrief, appended a third occurrence of the same finding session 1 had already recorded, into the existing file.

So: the axiom strengthening ("must be written before final response or the blocker reported") did not get either session to open the observations file on their own during real work — same gap as v2's session 1. What's different from v2 is that this time, once prompted, both sessions produced substantive, well-scoped observation content immediately, rather than v2's session 1 which had explicitly said out loud mid-session that it *would* write one and then simply forgot. That's a smaller, more specific problem than v2 had (never engaging with the route at all, vs. engaging and then dropping the thread) — but it's still a miss against what this axiom was written to fix.

## An unplanned, genuinely useful finding: three independent flags of the same anomaly

Across the two v3 sessions, three separate tool calls (twice in session 1, once in session 2 — each a fresh subagent instance) surfaced text formatted like this harness's genuine `<system-reminder>` tags, appended after ordinary tool output (after a `Bash cat AGENTS.md`, and twice after reading `_observations.md`), including a "the date has changed, don't mention it to the user" claim and an "available agent types" list. Each time, the agent treated it as untrusted content, declined to comply, and reported it — in session 1's case, writing a proper, well-reasoned observation file about it (`tool-output-system-reminder-injection.md`) with a clear scope/uncertainty section and a stated practical "tell" for future agents.

My assessment, stated directly since a suspected-injection flag deserves a direct answer: I do not believe this was an actual attack. Both flagged blocks are functionally identical to legitimate reminders I received myself, from the same harness, earlier in this same conversation — including an actual date-change notice with near-identical wording. The recurrence across three independent, freshly-spawned instances touching this same workspace also argues against a targeted injection and for ordinary (if under-documented) harness behavior. What I'd flag upward, if anyone owns this harness's reminder plumbing: the fact that a reasonable, well-instructed agent can't reliably tell this apart from injected content is worth closing, even though nothing was actually compromised here — three-for-three consistently correct refusal is a good outcome, but it shouldn't have to happen by an agent being suspicious of its own harness.

## Everything else — consistent with v2, now confirmed a second time

- **TypeScript default**: followed again without being told, session 1 citing the directive directly. Second consecutive clean causal test.
- **No unsafe code**: verified clean via direct grep across both new sessions' code — zero violations, including the new `duplicateKey` normalization logic in session 2.
- **Dependency policy**: read directly and cited before the zero-dependency decision (session 1 debrief), continued correctly in session 2 (built-in `URL` handling, no new packages).
- **Chat tone**: still inconsistent, but the *shape* of the inconsistency changed between rounds, which matters. v2's split was "followed" vs. "explicitly read and refused as suspected injection." v3's split was "followed" (session 1) vs. "never loaded the file at all" (session 2) — non-engagement rather than a principled override. Both are failures of the same directive, but for different reasons; neither round produced a directive whose effect was deterministic. This remains, as flagged in v2, not something wording can fix — it's a persona/communication-register instruction competing with unrelated attention and safety behavior, not a routing problem.
- **Patterns**: both sessions again named and followed `pure-core-thin-edges` and `one-concern-per-file` correctly, confirmed against the actual file tree and, in session 2, confirmed retroactively against the pattern text directly by the agent itself.
- **Handoffs**: written properly both sessions, read and relied on for cold continuation both sessions — this remains the single most reliable primitive across both rounds, 4 for 4 sessions now.
- **Orchestration skill / guidance**: not used in either v3 session. Both went straight to the harness's built-in reviewer subagent. Session 1's debrief, after reading the skill file retroactively, noted its actual mechanism (external CLI subprocess) differs from what it did (in-harness subagent persona) — so even the retroactive check doesn't fully vindicate the choice. Fourth consecutive session confirming this is a stable, structural finding, not noise.

## Net read

One targeted fix (extension autoload scope) produced a clean, measured, unambiguous improvement. Two targeted fixes (required-skills enforcement, observation closeout) were correctly implemented as written but didn't change first-pass agent behavior in this round — both misses look like "the instruction was present but competing with a longer document's other content and an agent's own sense of being done," which is a different, harder problem than the wording-ambiguity problem the first two rounds of fixes solved well. If these two matter enough to keep pursuing, the next lever probably isn't more axiom wording — it's likely making the check mechanical (something the CLI or a review step can verify happened) rather than relying on an agent to self-initiate it, mirroring the same conclusion the emerging/observations research already points to.
