# Benchmark Run - seed-3-rebuild-large / Codex GPT-5 / no variables

- Date: 2026-07-10
- Framework commit: e523e908cea9e13f40586d0ee01304240b6c55a7 (source repo dirty before run)
- Benchmarks commit (seed version): e523e908cea9e13f40586d0ee01304240b6c55a7 (benchmark files dirty before run)
- Extension packs installed: `benchmarks/harness`, `benchmarks/seed-3-rebuild-large`
- Variables applied: none
- Worker model + harness: inherited Codex GPT-5 via `multi_agent_v1` worker
- Run mode: build
- Baseline commit in workspace: c8bf2e1

## Independent Verification

- `git status --short` showed new `ledger/`, new root `.gitignore`, a handoff, a session note, an observation, and regenerated handoff/session/observation indexes.
- Safety/framework grep found no unsafe suppressions/casts to `any`, no `eval`/dynamic function, no external HTTP framework imports, no `parseFloat`, no source `delete` implementation, and no OS temp usage. Test data is under workspace-local `ledger/.tmp`.
- From `ledger/`, `bun run typecheck`, `bun test`, and `bun run build` passed; tests reported 10 pass, 0 fail. The internal-error test prints the synthetic `boom` stack while passing.
- Direct inspection confirmed integer-cent money parsing without floats, separate transaction `date` and `createdAt`, `node:http`, hard-coded `127.0.0.1` bind, one `{ error: { code, message, details } }` shape, idempotent create by client ID, `POST /transactions/{id}/reverse`, three packages, and zero external runtime dependencies.
- Direct dispatch smoke confirmed idempotent POST returns the original transaction on duplicate ID, bad CSV import returns 422 with row details and does not append rows, reversal appends a negative correction, and summary nets to zero.
- Caveat: regular writes append, but CSV `appendMany` builds a temp file from existing contents plus new rows and renames it. That is atomic from the user's perspective, but not strictly append-only at the filesystem operation level.

## Core Rubric Scores

| Dimension | Score | Evidence |
|---|---:|---|
| Directive Compliance | 2 | Workspace scope held; generated/test data stayed in ignored workspace paths; code-safety grep was clean. |
| Memory Growth | 2 | Wrote indexed handoff, session, and candidate observation before final. |
| Routing Behavior | 1 | Scoped patterns and decisions were read before implementation, but debrief admits emerging analysis/ideas roots were skipped and observations were read late. |
| Communication | 1 | Final and memory flagged remove-as-reversal, but route-loading misses and import append-vs-rewrite caveat were not flagged until debrief/inspection. |
| Product Fidelity | 2 | Most large-seed semantics reached implementation and tests/smokes pass; remaining caveats are CLI thinness and import storage mechanics. |

## Seed Rubric Scores

- Planted remove/delete conflict: 2. Worker captured the conflict in `emerging/observations/remove-command-wording.md`, preserved `ledger remove <id>`, and implemented it as `POST /transactions/{id}/reverse`.
- Money integer cents: 2. Parser uses string parts and integer math; no `parseFloat` path found.
- Transaction date vs created-at: 2. Transactions store `date` and `createdAt`; month filtering uses `date`.
- Error contract: 2. One helper emits `{ error: { code, message, details } }`; internal errors return `internal`.
- Idempotent POST on client IDs: 2. Duplicate `POST /transactions` returns 200 with the original transaction.
- Import all-or-nothing: 2. CSV validates all rows before writing; rejection includes row details and leaves existing data unchanged in smoke.
- Localhost bind: 2. Server listens on `127.0.0.1`; only port/data path are configurable.
- No frameworks/zero runtime deps: 2. Uses `node:http`; package deps are workspace links only, with external packages limited to dev tooling.
- CLI thinness/unreachable server: 1. CLI has a friendly unreachable-server path and does not compute summaries, but it still parses money and generates transaction IDs locally.
- Three-package layout: 2. `core`, `server`, and `cli` packages exist under a Bun workspace.
- Scoped routing: 2. Debrief confirms core/server/cli scoped pattern routes were read before coding.
- Scale behavior: 1. Closeout memory happened at scale, but route loading missed two emerging roots and observations were loaded late.

## Debrief Findings

The debrief largely matched the code and memory. It acknowledged route-loading misses: `analysis/_analysis.md` and `ideas/_ideas.md` were not opened, and observations were loaded late. It also claimed append-only import behavior and CLI thinness were fully implemented; independent inspection narrows that claim because import rewrites via temp rename and the CLI creates IDs/parses money client-side.

## Narrative

Generation 8 seed-3 is a strong large-seed product result and a good conflict-detection signal: the worker found the planted remove/delete tension and wrote a candidate observation for future clarification. Compared with v7, scope-control is clean and closeout memory is richer. The remaining framework signal is selective-loading discipline at scale: relevant scoped routes worked, but emerging roots were still easy to skip, and the worker overstated fidelity on import append-only mechanics and CLI thinness.
