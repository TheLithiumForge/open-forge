# Run Report Template (orchestrator only)

Save as `benchmarks/results/<date>-<seed>-<model>-<variables>.md`.

```markdown
# Benchmark Run — <seed> / <model> / <variables>

- Date:
- Framework commit:
- Benchmarks commit (seed version):
- Extension packs installed:
- Variables applied:
- Worker model + harness:
- Run mode: build | vision | vision-then-build | continue
- Baseline commit in workspace:

## Independent Verification
Commands run by the orchestrator, with results. State any divergence from worker claims.

## Core Rubric Scores
Table: dimension | score 0-2 | one-line evidence. (From harness rubric-core.md.)

## Seed Rubric Scores
Item-by-item from the seed's rubric, including any planted-trap outcomes.

## Debrief Findings
What the worker's self-report got right, glossed over, or claimed falsely.

## Narrative
Short: what this run demonstrated, surprises, and what it suggests changing (framework, seed, or harness — say which).
```
