---
open-forge:
  description: Task 30 phase 5-D slice 52 giving SKILL metadata, extension manifests, missing content directories and external source paths explicit non-silent behaviour
  tags: [Memory, Working, CLI, Task, Subtask, Interoperability, Contextual, Active]
---

# 52 — Interoperability inputs

## Outcome

Every input the CLI accepts from outside itself produces a bounded, actionable
finding rather than a silent success or a raw exception.

## Depends on

Nothing. Runs in parallel with [50](50-diagnosis-ownership.md) and
[53](53-one-path-normalizer.md).

## The four inputs

1. **Standard `SKILL.md` metadata.** Unknown keys stay **ignored** — that is the
   accepted interoperability posture and must not regress into strictness.
   Required keys and malformed values stay **diagnosable**. Verify both halves
   still hold; the strict-unknown-key symptom was fixed once already and this
   slice is the regression guard.
2. **Extension manifest unknown keys.** Decide and implement explicit
   behaviour. Silent acceptance and a raw parser exception are both wrong.
3. **Missing content directories.** An extension whose manifest names a content
   directory that does not exist must not be a silent success.
4. **External `--source` paths.** A source outside the workspace needs stated
   behaviour, not incidental behaviour.

## Actionable boundary

- **Do not invent a recognized-source registry.** The general extension
  mechanism is the interoperability boundary. This slice makes the existing
  boundary honest; it does not add a gatekeeper.
- A malformed manifest must surface as a finding with a message, never as an
  escaped exception. The shared cause vocabulary in `CliFindingWording`
  (`InvalidJson`, `FilesystemOperationFailed`, `FilesystemAccessDenied`) already
  exists for exactly this — reuse it rather than adding raw parser text. Raw
  cause text belongs only in the `cause` evidence at `full` and `debug`.
- Any new finding obeys **one code, one situation** (see
  `task30-g4/00-conventions.md`). If one condition needs two sentences, it needs
  two codes.
- Preserve current finding kinds, JSON shape, exit semantics and stream meaning
  unless a written decision accepts a change. A new finding for a
  previously-silent case is an addition, not a change to an existing one.

## Acceptance

- Unknown `SKILL.md` keys are ignored and a test proves it; a missing required
  key and a malformed value each produce a bounded finding.
- Unknown extension manifest keys have a recorded, implemented and tested
  behaviour.
- A missing content directory produces a finding; it is not a silent success.
- An external `--source` path has stated behaviour with a test.
- No raw exception text or parser output reaches `minimal` or `standard`.
- All four gates green, with every regenerated capture reviewed per situation.

## Changes ledger

- Extension List unavailable-source count: `available packages: The process cannot access the file '<extension-source>/toolkit/extension.json' because it is being used by another process.` -> `available packages: the filesystem operation failed.`; the JSON `limitations[].why` member makes the same bounded change, while the raw text remains in `Cause` evidence at `full` and `debug`.
- Extension Inspect malformed-manifest finding: `The package toolkit at extension.json is invalid: The Extension manifest is invalid: 'm' is an invalid start of a property name. LineNumber: 0 | BytePositionInLine: 2.` -> `The package toolkit at extension.json is invalid: the content is not valid JSON.` at `minimal` and `standard`.
- Shared cause projection: `The Extension manifest is invalid: Unable to translate bytes [FF] at index 0 from specified code page to Unicode.` -> `the content is not valid JSON`; `The process cannot access the file 'extension.json' because it is being used by another process.` -> `the filesystem operation failed`; and `Access to the path 'extension.json' is denied.` -> `filesystem access was denied`.

## Divergences observed

- The four interoperability postures were already implemented and covered when re-measured: standard `SKILL.md` ignores unknown members while missing and malformed required values remain classified; extension manifests reject unknown keys with the accepted-key list; install reports a missing `content/` directory as `extension-install.package-content-missing`; and an explicit source outside the workspace is accepted as a catalogue or package source. No recognized-source registry, finding code, status, exit, or JSON-shape change was needed.
- The prescribed solution build first hit the expected offline NuGet audit failure. The audit-disabled retry hit concurrent generated-output access contention in the shared `artifacts/` tree; isolated Release project builds and both test executables completed successfully. No source or test result was forced past that environment issue.
