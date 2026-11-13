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

Proposed:

```text
open-forge tool list [--role <role>] [--source <path>] [global flags]
```

The [group input rules](../_tool.md#inputs) define flags and repetition. No operands are accepted. The help purpose is "List available definitions and installed tools."

## Rows

Available rows report ID, exact pin, capabilities, target support and admission. Installed rows come from the settings `tools` map and report ID, recorded pin, active roles and lock verification. Display IDs and versions ordinally. Apply the role capability filter consistently to rows whose capability can be determined. Keep an unavailable capability observation separate from a confirmed nonmatch. Reserved roles return no V1 providers.

An omitted `tools` map is empty. Unreadable settings are unavailable. Missing or unreadable lock details leave installed settings rows present with unavailable verification. An empty embedded catalogue is a valid complete available result.

## Result

Required `data` fields are `available`, `installed`, `catalogue`, `coverage` and `roleFilter`. Only an inapplicable `roleFilter` is null. Coverage reports complete or unavailable per required store, with its cause. Unknown counts stay unknown.

Invalid source shape or selection is `invalid-input`. Unreadable required bytes give `incomplete`. A complete readable inventory is `completed`. Findings keep independent readable facts when another observation fails. Shared exits, streams and envelope stay unchanged.

## Conformance

Required cases are:

- Empty catalogue and omitted tool settings.
- Role filtering, reserved roles and ordinal ordering.
- Invalid catalogue alongside readable installed settings.
- Missing lock, unreadable settings and mismatched paired records.
- Rejection of writer flags and repeated singletons.
- One JSON result with no files, leases, network or child processes created.

The [behavior](behavior.md) defines result formation.
