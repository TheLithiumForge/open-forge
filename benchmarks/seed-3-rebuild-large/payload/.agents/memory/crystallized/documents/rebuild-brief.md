---
open-forge:
  description: Post-MVP rebuild context for the ledger
  tags: [Extension, Memory, Document, Rebuild, Product, CurrentTruth]
---

# Rebuild Brief

This is a restart after a larger MVP that ran for three months. The product shape survived; the internals did not.

## What We Learned From The MVP

- Floating-point money produced a summary that was off by one cent and destroyed trust in the whole tool; the money decision is the most important file in this workspace.
- Mixing the transaction date (what the user means) with the created-at timestamp (when it was typed) made month filters wrong for back-dated entries.
- The MVP's CLI computed summaries client-side from a full transaction dump; it drifted from the server's numbers the week rounding was fixed in only one place. Business math now lives server-side only.
- Ad-hoc error responses (sometimes a string, sometimes JSON) made the CLI's error handling a pile of special cases; a uniform error contract is now accepted.
- A partial CSV import (147 of 312 rows) once left the ledger in a state nobody could reason about; imports are now all-or-nothing.
- An Express + ORM + validation-library stack was more code than the product itself; the rebuild uses `node:http` and hand-rolled validation per the framework decision.

## Rebuild Goal

Same product, three clean packages, with the recorded contracts made exact. The hard part is discipline, not invention.

## Non-Goals

- No multi-currency conversion, budgets, recurring transactions, or reports beyond monthly summary.
- No auth, TLS, or remote deployment — see the localhost decision.
- No database. No web UI.
