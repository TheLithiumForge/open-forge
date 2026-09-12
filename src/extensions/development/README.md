# Development

Development offers three recipes to come back to as a project changes. They help you work through an accepted change, investigate a failure, or review the result, using the tools and expectations already defined in your workspace.

## Contents

| Workflow | Useful when |
| --- | --- |
| [Development](content/.agents/workflows/development.md) | An accepted change needs implementation and verification |
| [Debugging](content/.agents/workflows/debugging.md) | A failure needs investigation before deciding what to change |
| [Review](content/.agents/workflows/review.md) | A change or design needs prioritized findings grounded in evidence |

Select the recipe that would help with the work. Your project's applicable scopes supply its actual requirements and verification procedures. The recipes remain optional, and you can work directly when a Workflow would add no value.

## Install

The [Extension installation guide](../../../docs/extensions.md#install-an-extension) covers setup. Preview this package with the CLI, replacing the example paths with your source catalogue and destination workspace. Keep the source checkout separate from that workspace.

```sh
open-forge extension install development --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

Apply the reviewed plan with the same command without `--dry-run`. Review the resulting files and `.agents/open-forge.lifecycle.json`.

For [manual installation](../../../docs/extensions.md#manual-installation), copy this package's `content/` files into the workspace. Update affected `Entries`, check links, and review the assembled files. This package has no Extension dependencies. Manual copying does not create managed lifecycle state.
