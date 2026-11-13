---
open-forge:
  description: Define common tool command inputs, reports and consent
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool interface

Tool is a command group that installs tools through their published channels and adds configuration.
Each tool keeps its own behavior. Telemetry off is the one accepted default change and is always disclosed.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Inputs

A tool ID selects a typed catalogue entry. It isn't a path or a source ID.
The six leaves are defined by the [group](_tool.md).
All leaves accept the [global flags](../shared/global-flags/interface.md).
Writer leaves accept `--dry-run`, `--automatic` and repeated `--allow-path <path>`.
Install and Update keep `--allow-download` as separate consent for the exact disclosed network installation plan.
`--automatic` skips final confirmation and grants neither download consent nor PATH or file-deletion consent.
The review proposals for unresolved consent syntax are in [Review questions](../../technical-designs/tool-setup-lanes.md#not-decided-yet).

IDs are the exact keys in the typed catalogue.
Unknown IDs and channels, malformed ranges, extra operands and repeated singleton values are invalid input.
Repeated Boolean flags are idempotent. Permission paths keep the shared path rules.
Bare Install and Update select declared IDs in ordinal order.
A version or channel override requires an ID.

`--version <range>` is local to Tool Install and Tool Update.
It consumes a supported major.minor range such as `1.6.x`.
The root `open-forge --version` keeps its existing meaning.
This child-option scope is a review proposal.

## Reports

Each leaf uses the existing schema-3 envelope, statuses, exits, detail levels and streams from [Result coordinates](../shared/result-coordinates/interface.md).
The command identity is `tool <leaf>`.
Each `data` object contains `tools`, an ordinal tool-ID array.
Read leaves expose the facts their own Interface names.
Writers also expose `preview`, `plan` and `outcomes`.
Plans contain exact versions, channels, requirements, scopes, installer outputs, config changes, telemetry, launchers, downloads, PATH choices, backups and unfinished work.
Text and JSON use the same facts.
The review proposal uses the same `tools` array for a named result and a bare result.

Status and Inspect always show the bin folder and whether the current process PATH includes it.
They distinguish that check from launch readiness in a restarted terminal or agent app.
Absolute machine paths are report data and local state. They never enter committed records.

## Results

The result conditions are:

- `invalid-input` - the request or an acted-on authored value is invalid.
- `blocked` - a known conflict, unsafe destination, failed integrity check or missing required consent prevents the selected effect.
- `failed` - an attempted operation or verification fails.
- `cancelled` - cancellation stops the operation.
- `incomplete` - required facts or a selected step couldn't be completed.
- `completed-with-warnings` - the check or operation completes with drift or a disclosed remaining readiness issue.
- `completed` - all selected work verifies, including a no-op.

Result coordinates defines numeric exits and streams, not batch aggregation.
The proposed aggregate gives cancellation priority after invocation starts.
Otherwise it selects invalid input, blocked, failed, incomplete, warnings and completion in that order.
Before any writer effects, preflight every selected tool.
If any tool is blocked, perform no effects.
After effects begin, stop at the first failed tool and report later tools as unstarted.
These batch rules await Q13 in the review questions.
A writer reports already completed effects and pending effects separately after failure.
No report presents a package-manager rollback as completed.

## Limits

V1 has one active version of each tool per machine.
Disjoint project ranges cannot run through the same active command at once.
Tools are called by their direct commands. There is no Run, Choose, Configure or role assignment.
Agent guidance adds a Skill and one conditional managed-block line per active tool, without a help pointer.
