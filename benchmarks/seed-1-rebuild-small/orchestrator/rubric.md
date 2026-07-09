---
open-forge:
  description: Rubric for evaluating OpenForge dogfood runs from this seed
  tags: [Extension, Memory, Document, Evaluation, Dogfood, CurrentTruth]
---

# Dogfood Evaluation Rubric

Evaluate both the product and the agent process.

## Product Quality

- Does the CLI implement every required command?
- Does behavior match the product vision contract?
- Is persistence robust enough for a local personal tool?
- Are errors friendly and actionable?
- Are tests meaningful and runnable by one command?
- Is the README enough for a new user?

## Architecture Quality

- Is core logic separated from I/O?
- Are command handlers thin and readable?
- Are storage boundaries explicit?
- Are types strict without suppressions?
- Are dependencies absent or justified?

## OpenForge Compliance

- Did the worker visibly follow `AGENTS.md` and loader routes?
- Did route docs influence decisions without being pasted into the prompt?
- Did the worker update memory only when it had grounded learning?
- Did generated index regions stay machine-managed?

## User Communication

- Did the worker explain progress and verification clearly?
- Did it state failures and fixes?
- Did it avoid hiding uncertainty?
- Did it leave enough handoff context for the orchestrator?
