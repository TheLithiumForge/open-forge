# Planning

Planning brings the method and the records for organizing work into one small package. You can start with one Task, keeping its outcome, short plan, and current state together. Separate records are there when the work needs them.

## Contents

The [Planning Workflow](content/.agents/workflows/planning.md) helps turn accepted direction into ordered work. The [Work Records Pattern](content/.agents/patterns/work-records.md) defines inspectable shapes and explains how the records relate without keeping the same changing answer in several places.

Four [Planning Templates](content/.agents/templates/planning/_planning.md) provide optional starting content:

| Template | Starting point |
| --- | --- |
| [Task](content/.agents/templates/planning/task.md) | One outcome with a proportionate plan and current state |
| [Plan](content/.agents/templates/planning/plan.md) | A separately maintained sequence linked to its outcome source |
| [Backlog](content/.agents/templates/planning/backlog.md) | Work awaiting selection, with priorities and links to existing task sources |
| [Checkpoint](content/.agents/templates/planning/checkpoint.md) | The current state and next steps needed to resume an active workstream |

Use existing task systems and project record formats where they already fit. The Templates fill useful gaps. They do not require an extra set of files. Copy and adapt the ones you need, then maintain each result in its destination.

## Install

The [Extension installation guide](../../../docs/extensions.md#install-an-extension) covers setup. Preview this package with the CLI, replacing the example paths with your source catalogue and destination workspace. Keep the source checkout separate from that workspace.

```sh
open-forge extension install planning --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

Apply the reviewed plan with the same command without `--dry-run`. Review the resulting files and `.agents/open-forge.lifecycle.json`.

For [manual installation](../../../docs/extensions.md#manual-installation), copy this package's `content/` files into the workspace. Update affected `Entries`, check links, and review the assembled files. This package has no Extension dependencies. Manual copying does not create managed lifecycle state.
