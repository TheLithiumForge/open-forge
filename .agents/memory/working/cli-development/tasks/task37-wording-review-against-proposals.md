---
open-forge:
  description: Open Task 37 to compare every shipped CLI sentence against the unaccepted G4 output proposals and adopt, merge or reject each on its merits
  tags: [Memory, Working, CLI, Task, Wording, Review, Contextual, Active]
---

# Task 37 — Wording Review Against The Output Proposals

## Task state

- State: **Open, not started.** Raised by the maintainer on 2026-09-16.
- Owner: Root.
- Nature: a sentence-by-sentence review producing per-sentence decisions. It is
  not a rewrite, and it is not permission to change wording in bulk.

## Why this exists

Three long output proposals were written for G4 and only partly consumed. The
[consolidated proposal](task30/phase-4b-g4-output-proposal-consolidated.md)
produced the accepted decisions **C1–C18** — the flags, the four detail levels,
the schema-3 envelope, the severity vocabulary. Those were **structural**.

What was never systematically compared is the **wording**. The proposals carry
per-command transcripts — proposed sentences for all 28 commands — and the
implementation used them as a guide, not as a line-by-line source. So the
shipped text and the proposed text have never been placed side by side.

## The inputs

| Proposal | Size | What it is |
| --- | --- | --- |
| [Fable](task30/phase-4b-g4-output-proposal-fable.md) | 1,859 lines | Shared presentation rules plus per-command transcripts for all 28 commands |
| [Astra](task30/phase-4b-g4-output-proposal-astra.md) | 1,884 lines | A review of the others that also proposes its own wording |
| [Opus](task30/phase-4b-g4-output-proposal-opus.md) | — | The third proposal |
| [Consolidated](task30/phase-4b-g4-output-proposal-consolidated.md) | — | What the maintainer reviewed to produce C1–C18 |

**The maintainer intends to add a further proposal.** Check for it before
starting; if a fourth document is present, it is an input like the rest.

All four are **sealed, unaccepted proposals**. Read them as candidate wording.
Do not edit them, and do not treat a sentence as approved because it appears
there — the accepted wording is what the command catalogues under
`task30-g4/` say, and what the code actually renders.

## The method

Work sentence by sentence, not command by command in prose:

1. Take the shipped sentence — from the code and the captures, not from the
   catalogue, because the catalogue can drift.
2. Put the proposals' sentences for the same situation beside it.
3. Decide: **keep**, **adopt one proposal**, or **merge**. Record which, and
   why, in one line.
4. Where the proposals disagree with each other, say which is better and why.
   That disagreement is the most useful signal in the corpus.

A "keep" is a real outcome and will be the common one. The bar for changing a
shipped sentence is that the replacement is clearly better for a reader who is
stuck — not that it is newer, longer, or more precise about internals.

## Constraints

- **Every change is a frozen-string change.** Each one regenerates captures and
  needs the catalogue row updated in the same change. Batch by command so the
  review and the regeneration stay reviewable.
- The shared message families in `task30-g4/00-conventions.md` are one sentence
  used by many commands. Changing one changes every command that uses it —
  treat family sentences as a separate, higher-bar decision.
- **One code, one situation** binds here. If a proposal's better sentence only
  works because it describes one of two situations a code currently carries,
  that is a finding-code split to propose, not a sentence to adopt.
- The rules that already exist win over a proposal that predates them: the
  accepted C1–C18 decisions, the shared presentation rules, and the
  [minimal-output sweep](task32-minimal-output-sweep.md) if it has ruled by
  then.
- Interpolated command names and arguments are getting code markers in
  [Task 34](task34-interpolated-value-markup.md). Sequence against it — doing
  both at once churns the same sentences twice.

## Acceptance

- Every user-facing sentence has a recorded decision: keep, adopt, or merge,
  with a one-line reason.
- Adopted and merged sentences are implemented, their catalogue rows updated,
  and their captures regenerated and reviewed per situation.
- Every proposal disagreement is recorded with the ruling, so the corpus becomes
  a settled reference rather than three competing drafts.
- All four gates green.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
