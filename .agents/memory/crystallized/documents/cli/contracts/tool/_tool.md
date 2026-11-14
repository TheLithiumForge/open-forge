---
open-forge:
  description: Route the tool command contracts for external program installation and workspace configuration
  tags: [Memory, Document, CLI, Contract, Tool, CurrentTruth]
---

# tool
`open-forge tool` is a command group that manages reviewed external programs and their workspace configuration.

Install a supported pin, build its index through the external program, then choose its workspace role. Installation leaves every role unchanged. This branch defines the V1 core with an empty production catalogue. Concrete tool definitions and adapters follow separately.

Status: V1 core contracts decided.

## Commands

Bare `open-forge tool` shows text help and exits 0 before workspace work. It runs no wizard or domain operation. Help lists these leaves in this order:

1. `list` - list available definitions and installed tools.
2. `inspect` - explain one exact definition and its requirements.
3. `install` - install and register one pin without choosing a role.
4. `configure` - reconcile a registered tool's settings and desired hosts.
5. `status` - passively check one or all registered tools.
6. `update` - replace a registered pin with one exact supported pin.
7. `remove` - remove one registration and its recognized artifacts.
8. `choose` - assign or clear one workspace role.

## Inputs

Tool and role IDs contain 1 to 128 lowercase ASCII letters, digits or hyphens. The first character is a letter or digit. `none` is reserved for clearing a role and can't identify a tool. IDs are case-sensitive. Versions are full SemVer 2.0 strings without a `v` prefix. Version selection compares exact strings.

The shared [global flags](../shared/global-flags/interface.md) keep their meanings. `--help` and `--version` are mutually exclusive terminal forms. Domain operands and local flags can't accompany them. Other well-formed global flags are no-ops in a terminal form.

Local flags apply only to these commands:

| Flag | Commands | Default and meaning |
| --- | --- | --- |
| `--role <role>` | list | No filter. Filter by the role's required capability |
| `--source <path>` | list, inspect, install, update | Embedded catalogue. Select one exact local manifest or catalogue |
| `--tool-version <exact>` | inspect, install, update | The chosen source's default pin |
| `--host <claude-code\|codex>` | install, configure | Both hosts on install. Recorded hosts on configure. Repetition gives the full desired set |
| `--code-root <path>` | install, configure | `.` on install. Recorded root on configure. One contained workspace root |
| `--allow-download` | install, update | No consent. Approve this invocation's exact artifact descriptors |
| `--allow-session-daemon` | install, configure, update, choose | No new consent. Approve the declared build, cache and process scope |
| `--allow-path <path>` | install, configure, update, remove, choose | No new grants. Save shared workspace destination grants |
| `--automatic` | install, configure, update, remove, choose | Give final confirmation without prompts |
| `--dry-run` | install, configure, update, remove, choose | Preview the same plan and checks without effects |

Reject missing values, repeated singleton values and flags outside this matrix. Repeated Boolean flags are idempotent. Repeated hosts and paths become distinct ordinally sorted sets. Source paths resolve from the process directory. Code roots and grants resolve from the selected workspace. Reserved roles produce no V1 providers in List and can't be activated.

## Results

Every leaf uses the unchanged schema-3 [result coordinates](../shared/result-coordinates/interface.md). A renderer reads one result and never repeats the operation. Detail and format can't change status. Parser errors before binding use text on stderr and exit 4.

The shared [tool operation rules](../../shared-operation-contract.md#tool-operations) define preview, consent, effect ordering, exclusions and result data used by all five writers. The [manifest contracts](../shared/tool-manifest/_tool-manifest.md) define official downloads, exact source selection, private runtimes and the settings and lock split. External search, indexing, ranking and maintenance stay in the external program. This branch adds no execution proxy or scheduler.

## Not decided yet

- Proposed: `open-forge tool exec <tool> -- <args>` runs the pinned binary and its private runtime from the current directory. Arguments stay separate. It adds no PATH entry or symlink. This is a specification proposal and isn't implemented.
- Whether Choose switches agent integrations through declared commands or changes only the role map needs the maintainer's decision.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Route the tool choose interface and behavior contracts](choose/_choose.md) - #Memory #Document #CLI #Contract #Tool #Choose #CurrentTruth
- [Route the tool configure interface and behavior contracts](configure/_configure.md) - #Memory #Document #CLI #Contract #Tool #Configure #CurrentTruth
- [Route the tool inspect interface and behavior contracts](inspect/_inspect.md) - #Memory #Document #CLI #Contract #Tool #Inspect #CurrentTruth
- [Route the tool install interface and behavior contracts](install/_install.md) - #Memory #Document #CLI #Contract #Tool #Install #CurrentTruth
- [Route the tool list interface and behavior contracts](list/_list.md) - #Memory #Document #CLI #Contract #Tool #List #CurrentTruth
- [Route the tool remove interface and behavior contracts](remove/_remove.md) - #Memory #Document #CLI #Contract #Tool #Remove #CurrentTruth
- [Route the passive tool status interface and behavior contracts](status/_status.md) - #Memory #Document #CLI #Contract #Tool #Status #CurrentTruth
- [Route the tool update interface and behavior contracts](update/_update.md) - #Memory #Document #CLI #Contract #Tool #Update #CurrentTruth
