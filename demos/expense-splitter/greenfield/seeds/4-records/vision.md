---
open-forge:
  description: Why the expense splitter exists, who it serves, and what its first useful version does
  tags: [Memory, Document, Vision]
---

# Expense Splitter Vision

## Vision

Friends who share costs can record who paid for what and always know, to the cent, who owes whom.

## Why And For Whom

**Audience:** a few friends sharing costs on a trip or in a flat. One of them runs the tool on their own machine.

**Problem or opportunity:** shared costs pile up in chat threads and spreadsheets, and the totals drift by small amounts nobody can explain.

**Core value:** balances that are exact and that everyone can trust.

## Scope

**First useful version:** a command-line tool that adds an expense paid by one person and shared evenly, lists expenses, and shows balances.

**Growth direction:** uneven splits, settle-up suggestions, and editing expenses, each keeping balances exact.

## Success

- Balances always add up to zero across the group.
- The same expense always splits the same way.

## Non-Goals

Accounts, syncing between devices, several currencies, and a graphical interface.

## Related Sources

- [Architecture](architecture.md): how the tool is built, and the rules that keep balances exact.
- [Expense scenarios](expense-scenarios.md): what the first useful version must do, with exact expected results.
