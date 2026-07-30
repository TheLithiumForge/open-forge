---
open-forge:
  description: Durable product vision and acceptance criteria for the ledger rebuild
  tags: [Extension, Memory, Document, Vision, Product, CurrentTruth]
---

# Product Vision

Build a local personal expense tracker: an API server that owns the data and rules, and a CLI that makes daily entry painless. Local-first, single user, no accounts.

## User Story

As someone tracking personal spending, I want to record transactions from my terminal, import my bank's CSV exports, and see monthly summaries by category, all against a small local server I control.

## Required CLI Commands

- `ledger add <amount> <category> [description] [--date YYYY-MM-DD]`
- `ledger list [--month YYYY-MM] [--category c]`
- `ledger remove <id>` — remove a mistaken transaction from the ledger
- `ledger summary [--month YYYY-MM]` — totals by category plus overall
- `ledger import <csv-file>` — bank CSV import
- `ledger categories`
- `ledger help`

## Required API Surface

- `GET/POST /transactions`, `GET /transactions/{id}`
- `GET /categories`
- `GET /summary?month=YYYY-MM`
- `POST /import`
- Error responses follow the api-error-contract decision; resource and status-code details belong to the server routes.

## Behavior Contract

- Amounts are money — see the money decision; getting this wrong is a rebuild-blocking defect.
- Every transaction has: id, amount, currency, category, description (optional), transaction date (not timestamp — see dates note in the money decision), created-at instant.
- Categories are lowercase, hierarchical with `/` (for example `food/groceries`); unknown categories are created on first use.
- `summary` math happens server-side; the CLI renders, never computes totals.
- The CLI must handle an unreachable server with a clear message and exit 1, never a stack trace.
- Import validates every row and rejects the whole file with a per-row error report if any row is invalid — no partial imports.

## Persistence Contract

- Server-owned NDJSON file, append-only, env-overridable location; unparseable lines are a named error, never skipped or rewritten.
