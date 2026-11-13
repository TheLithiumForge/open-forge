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

Proposed:

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
