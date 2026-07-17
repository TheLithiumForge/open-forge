---
open-forge:
  description: What each framework area needs to reach S and S++ quality - measured claims first, automatic measurement second
  tags: [Memory, Idea, Contextual, Candidate, Quality, Benchmark, Roadmap]
---

# Rating Ladder: The Path To S And S++

Captured 2026-07-09 from the post-hardening review. Current ratings in parentheses. The through-line: every S means "the claim is measured, not asserted"; every S++ means "the measurement is automatic and survives without the maintainer". The benchmark seeds are the instrument for most of these.

## Core Routing (A)

- S: a published, versioned routing spec a third party could reimplement from docs alone, plus route-chain integrity validation (every folder reachable, every entry resolvable, zero dead routes) running in CI.
- S++: the contract proven by a second independent runtime or tool implementation (the `rune:` compatibility promise made real), with a conformance test suite both implementations pass.

## Load Policy (B+)

- S: two consecutive seeded benchmark generations with near-100% #LoadNow chain compliance and #KeepInMind closeout compliance across multiple models.
- S++: full compliance made cheaper than partial for any agent via first-party `open-forge dump --follow-required`, plus a measured per-tier token budget that stays flat as workspaces grow.

## Memory (A-)

- S: the observations rework shipped and evidenced - a seeded round where an agent spontaneously detects recurrence and proposes a promotion a human accepts; the self-growth loop actually closing.
- S++: demonstrated long-horizon value - a workspace through dozens of sessions where crystallized memory measurably changes agent behavior (fewer rediscoveries, correct decision citations) versus a memoryless baseline.

## Workflows (B-)

- S: the redesign accepted, applied to workflow-essentials and the seeds, and a benchmark round showing Required Routes loading at eight-of-eight levels without harness babysitting, across deterministic, iterative, and goal-seeking workflows.
- S++: orchestration proven - the v6 orchestrator-worker pattern completing a multi-workflow goal with clean handoffs, where the audit trail alone (named workflows, completion checklists) reconstructs what happened without reading transcripts.

## Extensions (B-)

- S: the skill-sharing mechanism designed and shipped (declare by name, build copies, install dedupes into `skills/`), plus update and remove lifecycle.
- S++: a third-party author ships a working extension without asking the maintainer anything.

## Governance (B)

- S: the concepts-versus-descriptors sweep done, plus CI that mechanically verifies alignment checks against installed files, turning "aligned when" from prose into a gate.
- S++: governance that provably cannot drift - descriptor changes fail CI until the payload matches in the same commit.

## CLI And Tests (B+)

- S: `open-forge dump` shipped from the variable-dump-tool seed evidence, and extension lifecycle covered by real temp-directory install tests.
- S++: the benchmark harness runs as CI on payload changes - every framework edit gets a compliance score before merge; regression-tested behavior, not just regression-tested text.

## Shortest Path To The Biggest Jump

Accept the workflow redesign, migrate the extension and seeds, run one benchmark generation, and let the numbers say whether the redesign holds.
