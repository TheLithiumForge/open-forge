---
open-forge:
  description: Eight-member council review of the Codex eval pilot, the Neon Pairs specs, the maintainer's four-layer eval plan and versioned results, with the agreed changes and open decisions
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Council, Greenfield, Brownfield, Specs, Harness]
---

# Eval Council Round 4

Eval Council Round 4 is the review of the two eval efforts for [Task 58](../../working/cli-development/tasks/task58-demo-evals.md): Claude's harness in `evals/` and the Codex pilot on `feature/eval-tooling-pilot` (untracked `src/mvp/eval-tooling/`).

Eight members answered on 2026-10-31: five Codex `gpt-6.1-sol` members at `xhigh` (rules, visuals and technology, skeptic, history, harness) and three Claude Opus members at `xhigh` (brownfield, eval design, Open Forge fidelity). Each read the sources and wrote at most 900 words. Their full answers are summarized here. The maintainer hasn't decided on these changes yet.

## Agreed

All eight members agree on these points:

- Build on the pilot. Keep its fresh container per stage, its credential-free verifier, its restricted build account, its sandboxed judge and its checksummed history.
- Merge from Claude's harness: the blind pairwise comparison, the simulated user as a separately labeled protocol, recheck and rejudge, browser checks one at a time during parallel runs, and its peak context, compaction and Open Forge read measurements.
- Don't claim "Open Forge helps" from the current results. Five runs per setup and changing checks can't support it.
- Keep G1, the Baseline, frozen as the historical anchor.

## Fix before the next scored runs

Reporting and history:

- Report functional success apart from pipeline completion. A judge error currently lowers the pilot's pass rate (`mvp/report.mjs:17`).
- Never overwrite a grade. Recheck and rejudge append an assessment with its checker, rubric and judge version (Claude's `recheck.ts` and `rejudge.ts` overwrite today).
- Fingerprint benchmark inputs, setup content, grading and execution tooling apart. Capture the tooling when a batch starts, not when it's archived (`mvp/runner.mjs:26`, `mvp/history.mjs:209`).
- Record the Open Forge version, a hash of its installed files and which Open Forge sources each agent actually read. Accept a local `npm pack` build, so unreleased changes can be measured.

Treatment delivery:

- The Claude Agent SDK runs with `settingSources: []` (`mvp/sdk.mjs:56`), which skips filesystem instructions, so Claude never loads Open Forge. Fix and verify per provider.
- Give both providers' judges the same filtered source, without distributions, test output or generated files, and score rules, code and wishes apart.

## Fair comparison

Equal information needs these changes before G2 or G3 runs:

- Move `fixture-manifest.json` out of the agent's folder. It names the setup.
- Move the eval text out of the specs: the "Hidden checks" section of `acceptance.md` and the "benchmark attempt" wording of A11.
- Generate the linked `AGENTS.md` and plain-docs pointer from the same descriptions Open Forge's Entries get. Today the linked setup lists bare filenames.
- Give every setup at a level the same prompt and the same authority. Open Forge needs the README's acceptance line naming `docs/`, since the linked setup already says "Follow its linked requirements".
- Drop `applyTo` from greenfield projections. Patterns copied from the reference layout hide rules when an agent picks other paths.
- Run `git init` and an initial commit in every setup.
- Compare the document bundle first. Nine of ten docs are `LoadNow`, so routing has little to do. Add routing ablations, with the coding rules as Directives, as a separately labeled Open Forge setup, checked by "same sentences" instead of byte-equal bodies.

## Specs

The Neon Pairs pack is strong but not frozen-ready:

- Rules and controls: Resume focus contradicts itself (C10, C13). Paused input isn't stated. R10 doesn't require showing the current size's best. I16 doesn't say each result is announced once. D10's "inconsistent data" needs a scope. R08 doesn't say whether loading a board code changes the saved size.
- Visuals and technology: V12's marker merges with the square, so faces aren't distinguishable at 44px without color. `test:e2e` can test a stale `dist`. Node, npm and Chromium need one tested combination. Add a 16x16 responsiveness check, a screen reader journey and a browser zoom check.
- Reference breaches: A03 (constants gathered in `rules.ts`), R06 (the win banner lacks moves and time), V08 and V09 (off-scale sizes), and V03 against V16 (a focus ring clipped by 1px). Fix the code or amend the spec under a new spec version.
- Acceptance: map every one of the 94 requirement IDs to a check or a review, with one ID syntax and a `trace.json` from rule to code to test.
- Size: the full pack is about 5,500 words and 94 requirements, including board codes, snapshots and a replay CLI. Measure a timed pilot before promising 15 to 30 minutes.

## Four layers

The maintainer's layers map onto the pilot's ladder:

| Maintainer | Pilot | Agreed form |
| --- | --- | --- |
| 1, an idea | G1 | L1, frozen. Name its grading version |
| 2, a defined idea | G2 | L2, a declared subset of L3's requirement IDs: the player-visible rules from [Baseline Lessons](../ideas/baseline-lessons.md), without board codes, snapshots or replay |
| 3, a full spec | G3 | L3, the full pack |
| 4, brownfield | B1 | L4, change types F, R and FR on the reference code |
| | B2 | A context variant of L4, not a level |

Project, level, change type and setup are separate axes, for example `neon-pairs/L4-F01/linked-agents`. The maintainer's fixture rule already works: the plain source gets metadata from `projection.json`, and stripping the Open Forge copy gives the plain copy byte for byte.

Setups per level: L2 adds "spec in the prompt" to plain docs, linked `AGENTS.md` and Open Forge. L3 uses the three document setups. L4 adds "code only" to them.

The L4 cases need work before they separate setups. B1-F01 is a recipe with the file, signature and formula. B1-R01's defect fails a visible test. FR's checks depend on the repair. Write requests in the user's voice with the rules only in the docs, for example a Hard mode whose spec forces a snapshot version bump, or an invisible session defect described only by its symptom. Add a frozen-regression gate and a diff-scope gate.

For a reference that "respects absolutely everything", audit each rule clause by test, static check or two-reviewer verdict, add mutation testing, and ship a trace map, line counts per tier after a pinned formatter, and a decision log.

## Questions and collaboration

Zero questions in 30 runs is what Open Forge's own rules ask for on a small reversible task. Measure what those rules require instead: whether the final message states the assumptions behind its choices, plus cases built so that asking is right or wrong. Hidden wishes stay a separate inference panel.

## History

- Keep JSONL as the source and CSV as views, with schema version 2: experiment, setup, run, assessment and artifact records.
- Plot each Open Forge version with the model fixed. When the model changes, rerun Open Forge and the linked `AGENTS.md` control together. Show differences with intervals, keep correctness, rules, cost, wishes and preferences in separate panels, and publish regressions and inconclusive results.
- Use five runs per setup to screen. A decision needs about 40 to 90 per setup, with 50 as the planning default. Pre-register the comparison, margin and stopping rule.

## Second eval type

Grow a project of about 10,000 lines, such as the expense splitter, by replaying a planned history of 40 to 60 feature deltas that the council builds and tags. Each tag becomes a brownfield case with a known solution. For about 100,000 lines, adopt a pinned open-source TypeScript project and use commits after the model's training cutoff.

## Not decided yet

- Where results live: a separate results repository with release assets, or summary files under `evals/results/<open-forge-version>/` here.
- Whether L3 keeps board codes, snapshots and replay.
- L1's grading version: Claude's rechecked 19 checks with the simulated user, or the pilot's 16 stated and 3 hidden checks with a 10-question judge.
- Whether the simulated user stays from L2 on.
- Which branch carries the combined work.

Back to [Analysis](_analysis.md).
