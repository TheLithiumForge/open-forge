---
open-forge:
  description: Task 31 removal of duplicated CLI implementation, converging the four text escapers on one owner and placing constants by the scope that owns them
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Duplication, Refactoring, Escaping, Constants]
---

# Implementation Duplication Removal

## Task State

- State: **M2 and M4 complete. M1 partly complete and partly stopped** — see
  [Phase 3.5 as executed](#phase-35-as-executed). **M3 moved into G4**: it
  changes output, and the single escaper is part of the presentation contract
  rather than a standalone refactor.
- Authority: The user's 2026-09-11 direction after the duplication survey — write
  the method work as a task, converge escaping on one method, and place constants
  by scope with a directive clause to match.
- Responsible role: Root, direct sequential implementation.
- Task source: Task 31 "Implementation Duplication Removal", registered in
  [CLI Development Tasks](_tasks.md).
- Evidence base:
  [Implementation Duplication](../../../emerging/analysis/cli-experience-audit/implementation-duplication.md)
  holds the measurements. This record owns the work.
- Predecessor: none. Phase M1 depends on nothing.
- Last updated: 2026-09-11.

## Problem And Expected Outcome

The CLI carries **103 groups of byte-identical method bodies across roughly 1,400
lines**, **eight text escapers giving four incompatible answers**, and constants
restated in up to seven places. Type _placement_ is already sound — no leaf
command reaches into a sibling — so this is entirely about logic written more
than once in the right folders.

At completion: each duplicated body has one owner at the narrowest scope that
covers its consumers; one escaper exists; every constant sits at the scope that
owns it; and the C# style directive states the constant rule so the next task
does not re-derive it.

## Milestones

### M1 — Family-level method bodies. 59 groups, ~793 lines. Behaviour-preserving

Each family already has a `Shared/` parent holding other content, so each move is
a deletion against an owner that exists.

| Family    | Groups | Lines | Concentration                                                                                                                                                                                                                                                                                                                                             |
| --------- | ------ | ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Route     | 39     | 455   | Move and Remove are near-twins: `ReadStatus` 31 lines, `ApplyFileAsync` 26, `ApplyDeletionAsync` 19, `DeleteCandidateAsync` 18, `ReadLayerAsync` 18, `ReadDeletionCatalogueAsync` 16, `ExecuteHeldSafelyAsync` 15, `ReadAsync` 14, `NotStarted` 13. Create/Init share `MatchesIntended`; Create/Update share `ValidateSingleton` 24 and `AppendUnchanged` |
| Library   | 12     | 242   | Attach/Detach/Sync share `Validate` 25, `ApplyAsync` 16, `Empty` 12. Inspect/List share `SelectStatus` 21                                                                                                                                                                                                                                                 |
| Extension | 8      | 96    | Install/Update share `CreatePackageSource` 16. Install/Remove/Update share `ReadStatus` 14                                                                                                                                                                                                                                                                |

Do one family per commit, largest first. Route alone is more than half the work.

**Order caveat.** Do Route and Library first. The Extension family touches
lifecycle-adjacent planning that G1 rewrites, so defer those 8 groups until G1
has landed rather than deduplicating code that is about to change.

### M2 — Cross-family method bodies. 22 groups, ~348 lines. Behaviour-preserving

These need a new owner under `Commands/Shared/`, in the shape phase 1 established
with `RecoveryWireVocabulary` and `SourceWireVocabulary`. The notable ones are
`ReadRegionInputs` (Install, Update), `Document` (Doctor, Library), the binder
helpers `ReadMany`/`ReadValues`/`ReadRelinkValues` (Extension Create, References,
Repair), `CreateSingleton` across five bindings, `Workspace` across seven JSON
projections, `ValidateCause` across five Framework models, and
`Location`/`SourceLocation` across five renderers.

### M3 — One text escaper. **Behaviour-changing**

**Decided:** there is to be **one** escaping method. Either `JsonEncodedText` is
the implementation, or the single owner uses it internally. The other seven types
are deleted.

This changes `--json` output for the commands that do not currently match the
chosen behaviour, so it is the one milestone that is not a silent refactor:

| Current behaviour                            | Commands affected                                |
| -------------------------------------------- | ------------------------------------------------ |
| `\\`, `"`, control as `\uXXXX`               | `CommandTextEscaping`, Context, Find, Route List |
| ...plus `\n` `\r` `\t` `\b` `\f` short forms | Extension, References                            |
| Control only, no backslash or quote          | Route Inspect                                    |
| `JsonEncodedText.Encode`                     | Route                                            |

- Capture a characterization baseline **before** the change, using the method in
  [CLI Experience Remediation](cli-experience-remediation.md). The output diff is
  the deliverable evidence, not an incidental check.
- Route Inspect is the one that is arguably _wrong_ today rather than merely
  different: it does not escape backslash or quote at all.
- `DiagnosticValueLimit = 240`, declared once beside each escaper, moves with it.

### M4 — Constant placement, and the directive clause

**Decided placement:**

- A constant used across the whole CLI belongs in a **global constants static
  class**.
- A constant owned by one module or command belongs in that command's
  **`<CommandName>Constants`**.
- A constant used only inside one class is declared **at the top of that class**.

Apply it to the measured cases: the 18 next-action command lines
(`"open-forge doctor"` six times, `"open-forge cleanup"` five, the two route
lines twice each), which every command can compose from the `CommandIdentity` it
already owns; and the 7 `DiagnosticValueLimit` copies, which travel with M3.

`SchemaVersion = 1` appears 26 times and **stays**: each command versions its own
envelope independently, which is the accepted contract. Do not "fix" it.

**Add the rule to [the C# style directive](../../../../directives/csharp/style.md)**
so the next task inherits it rather than re-deriving it.

## Phase 3.5 as executed

Run 2026-09-11 against `a3cb5080`. Four commits, all behaviour-preserving and
evidenced twice: unit parity at 18 failures of 3365 **diffed by failing test
name**, and a rebuilt throwaway characterization harness — 297 captures over 15
read-only commands in 5 views across the three `charbase` seeds, 20 binding
invocations, and 32 help surfaces, proven self-stable by two runs diffing to
zero. Every commit diffed **byte-identical**.

| Commit     | What                                                                                          | Measured           |
| ---------- | --------------------------------------------------------------------------------------------- | ------------------ |
| `14b85c48` | One status-collapse owner in `CliStatusDefinitions`                                           | 11 sites, −238/+50 |
| `d0f8e70d` | Option readers to `CliOptionResultFactsReader`; `ValidateSingleton` to `Route/Shared/Binding` | 8 sites            |
| `c459e878` | `LibraryId.TryCreate` beside `LibraryId.Create`                                               | 3 sites            |
| `59d17e45` | M4 — 88 command-line literals composed from `CommandIdentity`                                 | 35 files           |

**The scans were rebuilt rather than trusted, and they disagreed with this
record's figures in both directions.** Two findings matter more than the counts:

- **The status collapse was invisible to a body-identity scan.** Eleven sites
  declare the same precedence — `Failed, Interrupted, Invalid, Blocked,
Incomplete, Attention`, else `Complete` — in five different spellings, so no
  two bodies matched textually. Two of them (Route Create, Route Init) _threw_
  where nine returned `Complete`; the throw is unreachable because no finding
  code in any of those commands maps to `Complete`, and Route Create enforces
  that as a model invariant. Scan for the _policy_, not the body.
- **M4 was 88 literals across 21 command lines, not 18 across 4.** The style
  directive's constant clause already existed and the code already violated it.

### Stopped, and why

**Route Move and Route Remove are the same command twice plus a destination.**
Measured at file level after renaming `Move`→`Remove`: **73 paired files, 20
byte-identical (1,559 lines), 33 more at ≥70% (3,014 lines)** — against this
record's "39 groups, 455 lines" for the whole Route family.

It does not lift as "a deletion against an owner that exists".
`RouteMoveSubjectSelector` is 211 identical lines but reaches
`RouteMoveFindingCode`, `RouteMoveResultFormation`, `RouteMoveRequest` and
`RouteMoveBoundary`. The finding-code enums are **107 members against 98**, and
the difference is exactly the destination codes — `DestinationOccupied`,
`SelfMove`, `DestinationInsideSource`, `MovedFile` versus `RemovedFile`. Those
ship in `--json`. Sharing the selector means abstracting over a published
contract, which no characterization diff can vouch for.

Recorded as a candidate for its own task. It is **not** G4 work — G4 does not
rewrite these files — so deferring costs no rework.

Also left, each for a stated reason rather than by omission:

- **Context's `ReadValues`** omits the try/catch its five twins have, so it
  throws where they return empty. Converging is a fix, not a refactor.
- **Extension Create's `ValidateSingleton`** refuses in different words —
  _"accepts exactly one nonblank value"_ — and is user-visible text.
- **Library `SelectStatus`** (Inspect, List) ranks only four statuses. That is
  not a bug: `Failed` and `Interrupted` reach those results through `Event(...)`,
  which bypasses the collapse. Converging needs a reachability proof.
- **Library `Validate`, `ApplyAsync`, `PlanState`** are the Move/Remove shape in
  miniature — identical bodies over per-command plan types.
- **`ReadRegionInputs`** (Install, Update) deferred with G1, as agreed.
- **The three escaper groups** are M3 and belong to G4.

Remaining within-family duplication, rescanned at `59d17e45`: Route 284L,
Library 75L, Extension 21L, Cleanup 16L, Repair 14L, References 9L. The Extension
21L stays deferred until G1 lands.

### One operational note

Extending the harness to cover binding, `route update <x> --tag a --tag b` was
assumed to be a refusal. It is a valid repeatable option, and it **wrote to the
`pristine` and `populated` seeds**. Restored from the payload at
`src/open-forge/.agents/`, then _proved_ restored: the read-only capture set
returned byte-identical to the pre-3.5 baseline across all 257 captures. The
harness now runs binding cases against a disposable copy. **Seeds under
`open-forge-test/charbase/` are shared with G7 — never invoke a mutating command
against them directly.**

## Verification

- M1, M2 and M4 are behaviour-preserving. Prove it the way phase 1 did: a
  throwaway characterization capture across the three seeded workspaces, diffed
  to zero on command output.
- M3 changes output by design. Its diff is reviewed, not required to be empty,
  and the reviewed diff belongs in this record at closeout.
- Unit suite parity against the recorded baseline of 18 pre-existing failures.
- Expect tests that assert the duplication. Phase 0 met that four times and phase
  1 met it four more; budget for rewriting them as part of each milestone.

## Boundaries

- Do not touch the lifecycle baseline or permissions subsystems — G1 of
  [CLI Experience Remediation](cli-experience-remediation.md) deletes them.
- Do not collapse folders. That is postponed to phase 7 of that task.
- The 22 within-one-boundary groups are **out of scope here**. They are a
  command's own rendering work and belong with G4's renderer rewrites.
- Stop and return to the maintainer if a "duplicate" turns out to answer two
  different questions that coincide today. The human-versus-wire vocabularies in
  phase 1 were exactly that case.
