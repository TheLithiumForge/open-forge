---
open-forge:
  description: Define tool list syntax, role filtering and independent available and installed rows
  tags: [Memory, Document, CLI, Contract, Tool, List, Interface, CurrentTruth]
---

# tool list interface

`tool list` is a read command that lists available tool definitions and recorded installs for the selected workspace.

A broken catalogue still permits readable installed rows. A missing lock doesn't hide a settings entry or make it a verified install.

Status: V1 core contract decided.

## Syntax

```text
open-forge tool list [--role <role>] [--source <path>] [global flags]
```

The [group input rules](../_tool.md#inputs) define flags and repetition. No operands are accepted. The help purpose is "List available definitions and installed tools."

## Rows

Available rows report ID, exact pin, origin, capabilities, target support and admission. Installed rows come from the settings `tools` map and report ID, recorded pin, active roles and lock verification. Show Origin as its own column. Group first-party before third-party, then sort IDs and versions ordinally. Read installed origin from its stored definition. If it is unavailable, keep the row with null origin and sort it last. Apply the role capability filter consistently to rows whose capability can be determined. Keep an unavailable capability observation separate from a confirmed nonmatch. Reserved roles return no V1 providers.

An omitted `tools` map is empty. Unreadable settings are unavailable. Missing or unreadable lock details leave installed settings rows present with unavailable verification. An empty embedded catalogue is a valid complete available result.

## Result

Required `data` fields are `available`, `installed`, `catalogue`, `coverage` and `roleFilter`. Only an inapplicable `roleFilter` is null. Coverage reports complete or unavailable per required store, with its cause. Unknown counts stay unknown.

Invalid source shape or selection is `invalid-input`. Unreadable required bytes give `incomplete`. A complete readable inventory is `completed`. Findings keep independent readable facts when another observation fails. Shared exits, streams and envelope stay unchanged.

## Findings catalogue

Each code names one failed observation. The message carries its exact cause:

| Code                              | Status                    | Situation                                     | Message   |
| --------------------------------- | ------------------------- | --------------------------------------------- | --------- |
| `tool-list.invalid-input`         | `invalid-input`           | A bound operand or local flag is invalid      | `<cause>` |
| `tool-list.workspace-unavailable` | `blocked`                 | The exact workspace cannot be selected        | `<cause>` |
| `tool-list.settings-unavailable`  | `incomplete`              | Settings cannot be safely read or decoded     | `<cause>` |
| `tool-list.lock-unavailable`      | `incomplete`              | The required receipt lock cannot be read      | `<cause>` |
| `tool-list.receipt-missing`       | `incomplete`              | A recorded install has no matching receipt    | `<cause>` |
| `tool-list.receipt-mismatch`      | `completed-with-warnings` | The settings pin differs from the receipt pin | `<cause>` |
| `tool-list.cancelled`             | `cancelled`               | The read was cancelled                        | `<cause>` |
| `tool-list.read-unavailable`      | `incomplete`              | A required local read failed                  | `<cause>` |
| `tool-list.catalogue-invalid`     | `invalid-input`           | The selected catalogue is malformed           | `<cause>` |
| `tool-list.catalogue-unavailable` | `incomplete`              | Required catalogue bytes cannot be read       | `<cause>` |

## Conformance

Required cases are:

- Empty catalogue and omitted tool settings.
- Role filtering, reserved roles and ordinal ordering.
- Invalid catalogue alongside readable installed settings.
- Missing lock, unreadable settings and mismatched paired records.
- Rejection of writer flags and repeated singletons.
- One JSON result with no files, leases, network or child processes created.

The [behavior](behavior.md) defines result formation.

## Final V1 requirements

Every available row includes the definition's `first-party` or `third-party` origin. Installed rows use the verified stored definition and report unavailable origin with its cause if it cannot be read. Human output groups first-party rows before third-party rows, with unavailable rows last, and includes an Origin column. Within each group, sort by ID and exact version. JSON keeps the independent arrays and adds the origin fact to each row.
