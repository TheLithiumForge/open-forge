---
open-forge:
  description: Measured survey of duplicated implementation across the CLI, covering identical method bodies, the four incompatible text escapers, duplicated constants, and the finding that type placement is already sound
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Duplication, Structure, Refactoring, Escaping]
---

# Implementation Duplication

Four scans run after the phase 1 enum sweep, asking what else is duplicated and
what sits lower than it should. Every figure here is measured against
`OpenForge.Cli.Core` at commit `2f524f49`, not estimated.

[csharp-directives-and-structure.md](csharp-directives-and-structure.md) measured
the _structural_ signature of task-boundable decomposition. This document
measures the _implementation_ that decomposition produced.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## 1. Type placement is already sound

The scan worth reporting first, because it is the negative result.

Every type declared inside a command boundary was checked against every file
that references it. **No leaf command reaches into a sibling leaf at all.**
Eleven types are referenced across a command family boundary:

| Types                                                                                                                                                              | Reading                    | Verdict                                                                                                                      |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `DoctorDiagnosisReader`, `DoctorDiagnosisRead`, `DoctorObservation`, `DoctorRequest`, `DoctorProposalKind`, `DoctorDiagnosis`, `DoctorLibraryRecoveryPresentation` | Repair reads Doctor        | The accepted design. Repair repairs what Doctor finds.                                                                       |
| `FindBinding`                                                                                                                                                      | Shell reads Find           | Composition.                                                                                                                 |
| `FindMetadata`, `FindProjection`                                                                                                                                   | Framework, Route List      | Worth a glance during the naming pass; not a leak.                                                                           |
| `LifecycleObservation`                                                                                                                                             | Extension reads Route Move | **False positive.** Extension's `LifecycleObservation` is an enum member of its own finding code, not the Route Move record. |

A cruder first pass reported 129 hits. That count treats a family-level type used
by its own leaves — `LibraryHumanText` used by `Library/Attach` — as a crossing.
That is correct placement, and the refined figure is 11.

**Nothing needs promoting.** The problem is not where types live; it is that the
same logic was written more than once in the places they live.

## 2. Identical method bodies: 103 groups, ~1,400 duplicated lines

Method bodies were normalised for whitespace **and for the method's own name**,
so the same logic written twice under two names still collides. Bodies shorter
than four lines are excluded.

| Span                            | Groups | Duplicated lines | Owner that should hold it                     |
| ------------------------------- | ------ | ---------------- | --------------------------------------------- |
| Across command families         | 22     | ~348             | A shared owner under `Commands/Shared/`       |
| **Across leaves of one family** | **59** | **~793**         | That family's `Shared/`, which already exists |
| Within one boundary             | 22     | ~270             | That command's own rendering work             |

### The family bucket is the concentration

| Family    | Groups | Duplicated lines |
| --------- | ------ | ---------------- |
| Route     | 39     | 455              |
| Library   | 12     | 242              |
| Extension | 8      | 96               |

**Route Move and Route Remove are near-twins.** Byte-identical `ReadStatus` (31
lines), `ApplyFileAsync` (26), `ApplyDeletionAsync` (19), `DeleteCandidateAsync`
(18), `ReadLayerAsync` (18), `ReadDeletionCatalogueAsync` (16),
`ExecuteHeldSafelyAsync` (15), `ReadAsync` (14), `NotStarted` (13). Route Create
and Route Init share `MatchesIntended`; Route Create and Route Update share
`ValidateSingleton` (24 lines) and `AppendUnchanged`.

**Library Attach, Detach and Sync** share `Validate` (25 lines), `ApplyAsync`
(16) and `Empty` (12). Library Inspect and List share `SelectStatus` (21).

**Extension Install and Update** share `CreatePackageSource` (16). Install,
Remove and Update share `ReadStatus` (14).

Every one of those families already has a `Shared/` parent with other content in
it, so these are deletions against an owner that exists — the same shape as phase
1 item 1, at four times the size.

### The cross-family bucket

Smaller and more varied. The largest are the escapers below, then
`ReadRegionInputs` (Install and Update, 13 lines), `Document` (Doctor and
Library), the binder helpers `ReadMany`/`ReadValues`/`ReadRelinkValues`
(Extension Create, References, Repair), `CreateSingleton` across five bindings,
and `Workspace` across seven JSON projections.

`ValidateCause` appears in five Framework model files; `Location`/`SourceLocation`
in five rendering files.

## 3. Text escaping: one question, four answers

This one is not duplication. It is a **JSON contract divergence**.

Eight types answer "how is text escaped for output":

| Behaviour                                                        | Implementations                                                                                                       |
| ---------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `\\`, `"`, and control characters as `\uXXXX`                    | `CommandTextEscaping` (the existing shared owner), `ContextTextEscaping`, `FindTextEscaping`, `RouteListTextEscaping` |
| The same, plus `\n` `\r` `\t` `\b` `\f` short forms              | `ExtensionTextEscaping`, `ReferencesTextEscaping`                                                                     |
| Control characters only; backslash and quote are **not** escaped | `RouteInspectTextEscaping`                                                                                            |
| `JsonEncodedText.Encode`, which additionally escapes `+ < > &`   | `RouteTextEscaping`                                                                                                   |

So a newline inside a value is emitted as the six characters `\u000a` by
`find --json` and as the two characters `\\n` by `extension list --json`,
and a `<` is emitted unescaped by most commands but as `\u003c` by
`route list --json`. The audit recorded a symptom of this in
[command-output-design.md](command-output-design.md) — _"never emit JSON
escaping in human text"_ — without reaching the cause.

`DiagnosticValueLimit = 240` is declared once beside each of these, seven times.

**Converging them changes output**, which is why it is not a mechanical dedup.

## 4. Constants

| Constant                                               | Copies | Reading                                                                                                    |
| ------------------------------------------------------ | ------ | ---------------------------------------------------------------------------------------------------------- |
| `DiagnosticValueLimit = 240`                           | 7      | One beside each escaper.                                                                                   |
| `"open-forge doctor"`                                  | 6      | Next-action command lines, restated per command.                                                           |
| `"open-forge cleanup"`                                 | 5      | The same.                                                                                                  |
| `"open-forge route init"`, `"open-forge route update"` | 2 each | The same.                                                                                                  |
| `SchemaVersion = 1`                                    | 26     | **Not a duplicate.** Each command versions its own envelope independently, which is the accepted contract. |

The next-action strings are the notable case: every command already owns a
`CommandIdentity` constant — `"doctor"`, `"extension install"` — and the
`"open-forge "` prefix is invariant, so eighteen declarations restate something
already derivable.

## Method

Four scripts, all reading the source tree directly:

- **Enum mappings**: every `Enum.Member => "wire-string"` arm, grouped by
  identical member-to-string signature, classified by whether the copies span a
  command boundary.
- **Method bodies**: every method signature at class-member indentation, body
  extracted by brace matching so expression bodies and block bodies are both
  handled, normalised for whitespace and for the method's own identifier.
- **Constants**: every `internal const` declaration, grouped by exact text.
- **Type placement**: every declared type name against every file's identifier
  set, classified by family and leaf.

The scans are disposable. They are described rather than kept because rerunning
them against a changed tree is cheaper than trusting a stale list.
