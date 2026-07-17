---
open-forge:
  description: Accepted record of the open-forge-seed design review against the greenfield-to-brownfield goal, written with Claude Fable 5
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Recommendation, Analysis, Seed]
---

# open-forge-seed — Design Review

**Model note:** reviewed and written by **Claude Fable 5** (`claude-fable-5`).

Reviewed `{repos}\open-forge-seed` in full: README, `extension.json`, and the payload's 30 route files across directives, guidance, memory (5 decisions, 3 documents, 2 analyses, 4 observations, 1 ideas), patterns, workflows, and workspace.

## Verdict

Well designed — genuinely. It hits the stated target ("somewhere between greenfield and brownfield: I tried an MVP, here are the learnings, decisions, and vision") more precisely than I'd have expected the framework's primitives to support, and it closes the loop this whole dogfooding arc has been building toward: the earlier rounds' *observed agent failures* have been converted into *seeded routed knowledge* for future runs. The issues I found are refinements, not structural problems.

## What's right

**The brownfield framing is carried by exactly the right file.** `rebuild-brief.md` is the load-bearing piece: "this seed represents a clean restart after an earlier MVP, not a blank greenfield idea," followed by concrete MVP learnings (the command set was right; pure functions were the readable part; the weak spots were duplicate-URL/tag-normalization/corrupt-JSON/write-safety semantics; command-only READMEs weren't enough) and non-goals that explicitly forbid the classic agent scope-inflation moves (no sync, no plugin framework, no enterprise optimization). This is precisely the "between greenfield and brownfield" posture — enough accumulated truth to constrain, not so much that there's nothing left to build.

**The vision resolves every ambiguity previous rounds' agents flagged.** Across v2–v5, debriefed agents consistently listed the same would-have-asked items: duplicate `add` semantics, URL strictness/normalization, tag normalization, corrupt-file handling, env-var override, locking. `product-vision.md`'s behavior contract answers every single one (merge tags on duplicate, http(s)-only via platform parser, normalize-store, trimmed/lowercased/deduped/sorted tags, never silently overwrite corrupt data, env override, locking documented-not-required). Notably, v5's two sessions *diverged* on duplicate-add (one chose error, one chose merge) precisely because their workspaces lacked this file. The seed is demonstrably built from real observed friction, which is the organic-growth thesis made concrete.

**Truth is distributed to the routes that own it, and the README says why.** Decisions carry rationale, vision carries contract, patterns carry shape, directives carry hard rules, and the README states the experimental discipline outright: "The root prompt given to a worker should not repeat those details. The purpose of this seed is to test whether OpenForge routes carry enough context by themselves." That's the correct falsifiable framing for a dogfood instrument.

**The directives follow the persona-vs-work boundary.** All three (code-safety, scope-control, evidence-first-closeout) govern work product and process — the class that has been 100% reliable across every round. No persona/tone directive was seeded. Whether deliberate or instinctive, it matches the evidence.

**The meta-level is seeded too, and it encodes hard-won lessons.** `dogfood-orchestration.md` is essentially the methodology these five rounds converged on (baseline commit, minimal worker prompt, don't paste route details, independent verification, orchestrator-owned report). `worker-implementation.md`'s "Report Ownership" section and `agent-temptations.md` (CLI frameworks for five commands, schema packages for tiny JSON, premature repository abstractions, README-without-behavior, temp-dir convenience) read like a distilled register of exactly the failure modes observed in real runs. `scope-control.md`'s "do not read prior dogfood implementations" is proper contamination control for comparative experiments — a subtle need I'd have expected to be missed.

**The fallback posture matches the "ask/report when things aren't OK" goal.** `runtime-and-tooling.md` doesn't just decree Bun; it says: if unavailable, "report that clearly," fall back only when continuing beats blocking, and "document the deviation in the final report." That's the report-don't-silently-absorb behavior pattern, encoded in a decision.

## Issues and refinements

1. **The Bun decision quietly bakes in a half-wrong environment observation.** v5a concluded "no Node on this machine"; v5b then found a working Node 26 install (off-PATH, not absent). `runtime-and-tooling.md` chooses Bun as if the environment settled the question. The fallback clause saves it in practice, but consider one sentence acknowledging Node exists off-PATH — otherwise every future seeded run inherits a premise the very next session after it was formed already contradicted. It's also a nice live demo of why the framework's verify-before-promoting axiom for observations exists.
2. **`extension.json` name won't survive becoming a bundled extension.** `"OpenForge Dogfood Seed"` with spaces is fine for local-folder installs, but the CLI's bundled-id path expects lowercase kebab ids. If this ever ships in `src/extensions/`, it needs an id like `dogfood-seed`. Cheap to align now.
3. **Two documents carry near-duplicate spec fragments.** The command list appears in both `product-vision.md` and (implicitly, as "the command set was right") `rebuild-brief.md` — fine — but watch that future edits keep the behavior contract in exactly one place (vision) so the seed doesn't develop the parallel-truth problem the framework itself warns about. Currently OK; flagging the seam.
4. **No seeded handoff.** For a "restart after an MVP" fiction, a `working/handoffs/` note from the fictional MVP session ("here's where the old build stood, why we're restarting clean") would complete the brownfield illusion and give the handoff route a read-side test in seeded runs — currently the seed exercises crystallized/emerging reads but working-memory reads only happen if a run is multi-session.
5. **`agent-temptations.md` mixes two audiences.** Most bullets address the worker (CLI frameworks, schema packages), but two address the orchestrator (report ownership, temp dirs). Splitting worker-facing from orchestrator-facing temptations — or moving the latter into `orchestrator-dogfood.md` guidance — would keep each route's audience clean, per the framework's own scope-wording principle.

## One suggestion beyond the folder itself

This seed is close to being the "sensible defaults / evaluation harness" extension the synthesis report recommended. With the bookmarks-specific content swapped out, its *skeleton* (three work-scoped directives, evidence-first closeout, orchestration + worker + report workflows, an evaluation rubric document) is product-agnostic and would generalize to any project's dogfood runs. Worth considering as the template for a first-party `dogfood-harness` extension once the shape settles.
