---
open-forge:
  description: Define tool inspect syntax, exact pins and definition requirements
  tags: [Memory, Document, CLI, Contract, Tool, Inspect, Interface, CurrentTruth]
---

# tool inspect interface

`tool inspect` is a read command that explains one exact tool definition without launching it.

It shows artifacts, destinations, runtime, processes, consent, compatibility, trust limits and readable installed facts. Show official sources, pinned hashes, signature and attestation requirements and the saved telemetry choice. Show resolved install, private runtime, cache, state and workspace paths, or a cause when unavailable. Candidate definitions can be examined without being enabled. There is no separate open-path command.

Status: V1 core contract decided.

## Syntax

```text
open-forge tool inspect <id> [--source <path>] [--tool-version <exact>] [global flags]
```

Exactly one tool ID is required. The [group input rules](../_tool.md#inputs) define flags and repetition. The help purpose is "Explain one exact tool definition and its requirements."

## Selection

Use the selected source's default pin unless `--tool-version` gives an exact listed version. The [manifest interface](../../shared/tool-manifest/interface.md) defines source shape, integrity, admission and errors. Inspect reports activation blockers as requirements. It doesn't grant admission or process consent.

## Result

Required `data` fields are `id`, `definition`, `target`, `installed`, `telemetry`, `requirements`, `limitations` and `instructions`. `installed` is null only when no install is recorded. `telemetry` is the saved boolean choice, defaulting to `false`, or null when settings cannot be read. Instructions are resolved external command, argument and environment records when the required facts are available. Unavailable values have a cause and aren't invented paths.

Malformed IDs, unknown IDs and absent exact versions are `invalid-input`. Unreadable required source bytes give `incomplete`. A well-formed unreviewed tuple or unavailable adapter revision is `blocked`, exit 5, while readable definition facts stay in the result. A release-supported candidate can complete inspection while reporting its pending activation tests. Shared exits, streams and schema 3 stay unchanged.

## Findings catalogue

Each code names one failed observation. The message carries its exact cause:

| Code                                 | Status                    | Situation                                                        | Message   |
| ------------------------------------ | ------------------------- | ---------------------------------------------------------------- | --------- |
| `tool-inspect.invalid-input`         | `invalid-input`           | A bound operand or local flag is invalid                         | `<cause>` |
| `tool-inspect.workspace-unavailable` | `blocked`                 | The exact workspace cannot be selected                           | `<cause>` |
| `tool-inspect.settings-unavailable`  | `incomplete`              | Settings cannot be safely read or decoded                        | `<cause>` |
| `tool-inspect.lock-unavailable`      | `incomplete`              | The required receipt lock cannot be read                         | `<cause>` |
| `tool-inspect.receipt-missing`       | `incomplete`              | A recorded install has no matching receipt                       | `<cause>` |
| `tool-inspect.receipt-mismatch`      | `completed-with-warnings` | The settings pin differs from the receipt pin                    | `<cause>` |
| `tool-inspect.cancelled`             | `cancelled`               | The read was cancelled                                           | `<cause>` |
| `tool-inspect.read-unavailable`      | `incomplete`              | A required local read failed                                     | `<cause>` |
| `tool-inspect.definition-invalid`    | `invalid-input`           | The definition or exact pin selection is invalid                 | `<cause>` |
| `tool-inspect.pin-not-supported`     | `blocked`                 | The release does not support the exact tuple or adapter revision | `<cause>` |
| `tool-inspect.catalogue-unavailable` | `incomplete`              | Required definition source bytes cannot be read                  | `<cause>` |

## Conformance

Required cases are:

- Source default and exact version selection.
- Release-supported candidates explained without activation.
- Unreviewed tuples and unavailable adapter revisions keep `blocked`, exit 5, with readable explanation.
- Missing source, digest mismatch and manifest identity mismatch.
- Readable settings with missing lock details.
- Rejection of consent flags, side effects and child processes.

The [behavior](behavior.md) defines passive resolution.

## Final V1 requirements

Show the definition's origin and third-party consent disclosure. List each installer-owned entry, its exact command rewrite, optional component, saved or required choice and resolved Open Forge launch path. Explain telemetry, hook behavior and each unavailable path without starting a process.
