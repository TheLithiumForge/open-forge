# Memory Starters

Memory Starters gives useful knowledge a starting shape. There is room to explore an idea before it becomes accepted direction, or to preserve the reason behind a choice once it has been made.

## Contents

The five [Memory Templates](content/.agents/templates/memory/_memory.md) cover different reasons to keep a record:

| Template | Starting point |
| --- | --- |
| [Decision](content/.agents/templates/memory/decision.md) | Record an accepted choice and its rationale |
| [Idea](content/.agents/templates/memory/idea.md) | Keep a possibility open for exploration |
| [Analysis](content/.agents/templates/memory/analysis.md) | Develop reasoning that remains unsettled |
| [Observation](content/.agents/templates/memory/observation.md) | Preserve something noticed in evidence |
| [Handoff](content/.agents/templates/memory/handoff.md) | Preserve a snapshot at a transfer or planned resumption boundary |

Copy the Template that fits, replace its prompts with your project's information, and adapt its metadata to the destination. The destination determines the record's scope and Memory state. A starting shape does not establish acceptance.

## Install

The [Extension installation guide](../../../docs/extensions.md#install-an-extension) covers setup. Preview this package with the CLI, replacing the example paths with your source catalogue and destination workspace. Keep the source checkout separate from that workspace.

```sh
open-forge extension install memory-starters --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

Apply the reviewed plan with the same command without `--dry-run`. Review the resulting files and `.agents/open-forge.lifecycle.json`.

For [manual installation](../../../docs/extensions.md#manual-installation), copy this package's `content/` files into the workspace. Update affected `Entries`, check links, and review the assembled files. This package has no Extension dependencies. Manual copying does not create managed lifecycle state.
