---
open-forge:
  description: Define tool inspect syntax, exact pins and definition requirements
  tags: [Memory, Document, CLI, Contract, Tool, Inspect, Interface, CurrentTruth]
---

# tool inspect interface
`tool inspect` is a read command that explains one exact tool definition without launching it.

It shows artifacts, destinations, runtime, processes, consent, compatibility, trust limits and readable installed facts. Candidate definitions can be examined without being enabled.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool inspect <id> [--source <path>] [--tool-version <exact>] [global flags]
```

Exactly one tool ID is required. The [group input rules](../_tool.md#inputs) define flags and repetition. The help purpose is "Explain one exact tool definition and its requirements."

## Selection

Use the selected source's default pin unless `--tool-version` gives an exact listed version. The [manifest interface](../../shared/tool-manifest/interface.md) defines source shape, integrity, admission and errors. Inspect reports activation blockers as requirements. It doesn't grant admission or process consent.

## Result

Required `data` fields are `id`, `definition`, `target`, `installed`, `requirements`, `limitations` and `instructions`. `installed` is null only when no install is recorded. Instructions are resolved external command, argument and environment records when the required facts are available. Unavailable values have a cause and aren't invented paths.

Malformed IDs, unknown IDs and absent exact versions are `invalid-input`. Unreadable required source bytes give `incomplete`. A well-formed unreviewed tuple or unavailable adapter revision is `blocked`, exit 5, while readable definition facts stay in the result. A release-supported candidate can complete inspection while reporting its pending activation tests. Shared exits, streams and schema 3 stay unchanged.

## Conformance

Required cases are:

- Source default and exact version selection.
- Release-supported candidates explained without activation.
- Unreviewed tuples and unavailable adapter revisions keep `blocked`, exit 5, with readable explanation.
- Missing source, digest mismatch and manifest identity mismatch.
- Readable settings with missing lock details.
- Rejection of consent flags, side effects and child processes.

The [behavior](behavior.md) defines passive resolution.
