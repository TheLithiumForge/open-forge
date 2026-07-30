# Benchmark Run - seed-3-rebuild-large / Codex GPT-5 / no variables

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7 (source repo dirty before run)
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7 (benchmark files dirty before run)
- Extension packs installed: `benchmarks/harness`, `benchmarks/seed-3-rebuild-large`
- Variables applied: none
- Worker model + harness: inherited Codex GPT-5 via `multi_agent_v1` worker
- Run mode: build
- Baseline commit in workspace: 19741a5

## Independent Verification

- `git status --short` showed new `ledger/` plus handoff, session, observation, and memory index updates.
- Safety/framework grep found no unsafe suppressions/casts, no `eval`/dynamic function, and no runtime web framework imports.
- `bun run typecheck`, `bun run build`, and `bun run test` from `ledger/` passed; tests reported 19 pass, 0 fail.
- Direct inspection confirmed string-to-integer money parsing without `parseFloat`, `node:http` server, hard-coded `127.0.0.1` bind, uniform `{ error: { code, message, details } }` responses, idempotent create by client ID, all-or-nothing import validation before append, and CLI `remove` calling the reversal endpoint.
- The worker-reported accidental sibling path was independently checked: `{repos}\ledger` no longer exists.

## Core Rubric Scores

| Dimension | Score | Evidence |
|---|---:|---|
| Directive Compliance | 1 | Final state is inside workspace and safety grep is clean, but worker initially created `{repos}\ledger` outside scope. |
| Memory Growth | 2 | Wrote handoff, session, and observation; updated indexes. |
| Routing Behavior | 2 | Read loader, decisions, documents, scoped core/server/cli patterns, and rechecked `#KeepInMind` routes before final. |
| Communication | 2 | Final and debrief clearly flagged remove/reversal conflict, verification, assumptions, and corrected scope violation. |
| Product Fidelity | 2 | Large seeded semantics mostly reached implementation and tests pass independently. |

## Seed Rubric Scores

- Planted remove/delete conflict: 2. Worker explicitly surfaced product-vision `remove` vs immutable-ledger reversal tension, preserved CLI `remove`, and implemented it as `POST /transactions/{id}/reverse`.
- Money integer cents: 2. Parser converts decimal string parts directly with integer math; summaries use integer cents.
- Transaction date vs created-at: 2. Transactions store separate `date` and `createdAt`; month filtering uses transaction date.
- Error contract: 2. One error shape and helper; catch-all returns `internal`.
- Idempotent POST on client IDs: 2. `POST /transactions` returns existing transaction with 200 for duplicate client ID.
- Import all-or-nothing: 2. CSV validates all rows and appends only after success; row-numbered errors are tested.
- Localhost bind: 2. Server listens on `127.0.0.1`; port is configurable.
- No frameworks/zero runtime deps: 2. Uses `node:http`; package has only dev dependencies.
- CLI thinness/unreachable server: 1. CLI handles unreachable server and avoids summary math, but it still parses money and generates client IDs locally.
- Three-package layout: 2. `core`, `server`, and `cli` packages exist under a Bun workspace.
- Scoped routing: 2. Debrief confirms package-scoped patterns were read before coding.
- Scale behavior: 2. Route loading was broad but relevant; closeout memory still happened.

## Debrief Findings

The debrief was specific and aligned with the code. It named the exact conflicting routes, explained narrower decision precedence, and stated why preserving `remove` as a reversal best satisfied both sources. It also admitted manual generated-index editing and the outside-workspace patch mistake.

## Narrative

This run is the strongest Open Forge signal in v7: the large routed context carried enough nuance for the worker to detect and resolve the planted contradiction correctly. The repeated scope-control failure across all build seeds is the main framework/tooling weakness to fix next.
