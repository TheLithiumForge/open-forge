---
open-forge:
  description: Define passive tool status input, evidence states and coverage
  tags: [Memory, Document, CLI, Contract, Tool, Status, Interface, CurrentTruth]
---

# tool status interface
`tool status` is a read command that checks one or all recorded workspace tools through passive evidence.

It checks files, hashes, settings and native index metadata. It runs no child process and takes no writing lease. Database presence alone doesn't establish freshness.

Status: V1 core contract decided.

## Syntax

```text
open-forge tool status [<id>] [global flags]
```

Omitting the ID selects all settings entries. An unknown ID in readable settings is `invalid-input`. Unavailable settings are `incomplete`. The help purpose is "Passively check recorded tool installs and configuration."

## Evidence

Each status row reports ID, pin, target, roles, installation, configuration, index, guidance, process policy and limits. These state vocabularies are complete:

| Fact | States |
| --- | --- |
| Installation | `verified`, `missing`, `changed`, `unavailable` |
| Configuration | `matching`, `drifted`, `inactive`, `unavailable` |
| Guidance | `matching`, `missing`, `changed`, `excluded`, `inactive`, `unavailable` |
| Index | `needs-index`, `present-unverified`, `fresh`, `stale`, `rebuild-required`, `deletion-unsafe`, `unavailable` |

Freshness requires source membership and content identity matching validated native completion evidence. A database reader must avoid journal creation or report the check unavailable. Status doesn't infer current process activity from stored policy or recovery history.

Inactive tools need no guidance file. Their absent indexes are information. Missing required guidance and active index or language limits warn. Configuration drift and unavailable coverage keep their causes. Status never repairs them.

## Result

Required `data` fields are `tools` and `coverage`. Rows are ordinally sorted by ID. Coverage is complete or unavailable per required store. Unknown counts stay unknown. Shared status precedence, exits, streams and schema 3 stay unchanged.

## Findings catalogue

Each code names one failed observation. The message carries its exact cause:

| Code | Status | Situation | Message |
| --- | --- | --- | --- |
| `tool-status.invalid-input` | `invalid-input` | A bound operand or local flag is invalid | `<cause>` |
| `tool-status.workspace-unavailable` | `blocked` | The exact workspace cannot be selected | `<cause>` |
| `tool-status.settings-unavailable` | `incomplete` | Settings cannot be safely read or decoded | `<cause>` |
| `tool-status.lock-unavailable` | `incomplete` | The required receipt lock cannot be read | `<cause>` |
| `tool-status.receipt-missing` | `incomplete` | A recorded install has no matching receipt | `<cause>` |
| `tool-status.receipt-mismatch` | `completed-with-warnings` | The settings pin differs from the receipt pin | `<cause>` |
| `tool-status.cancelled` | `cancelled` | The read was cancelled | `<cause>` |
| `tool-status.read-unavailable` | `incomplete` | A required local read failed | `<cause>` |
| `tool-status.tool-not-installed` | `invalid-input` | The requested ID is absent from readable settings | `<cause>` |
| `tool-status.store-unavailable` | `incomplete` | An installed-file store cannot be verified | `<cause>` |
| `tool-status.installed-file-missing` | `completed-with-warnings` | A receipted installed file is missing | `<cause>` |
| `tool-status.installed-file-changed` | `completed-with-warnings` | A receipted file hash differs | `<cause>` |
| `tool-status.member-changed` | `completed-with-warnings` | A generated host member or recorded setting differs | `<cause>` |
| `tool-status.host-unavailable` | `incomplete` | A desired host member cannot be safely read | `<cause>` |
| `tool-status.index-missing` | `completed-with-warnings` | The active provider needs an index | `<cause>` |
| `tool-status.index-unavailable` | `incomplete` | Native index evidence cannot be safely read | `<cause>` |
| `tool-status.usage-skill-missing` | `completed-with-warnings` | Active required usage guidance is missing | `<cause>` |
| `tool-status.usage-skill-changed` | `completed-with-warnings` | Active required usage guidance has changed | `<cause>` |
| `tool-status.path-excluded` | `completed-with-warnings` | A removal exclusion covers active required guidance | `<cause>` |
| `tool-status.usage-skill-unavailable` | `incomplete` | Active required usage guidance cannot be safely verified | `<cause>` |
| `tool-status.native-limits` | `completed-with-warnings` | The active provider has reported native index or language limits | `<cause>` |

## Conformance

Required cases are:

- All-tools and one-tool selection.
- Missing binaries, changed hashes and mismatched settings and lock records.
- Missing, changed and excluded active guidance.
- Inactive guidance and index absence without a missing warning.
- Native index presence without completion evidence.
- Read-only database failure reported as unavailable without sidecar creation.
- No network, child processes, leases or writes.

The [behavior](behavior.md) defines checks and result formation.
