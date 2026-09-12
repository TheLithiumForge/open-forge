# Development Toolkit

Development Toolkit brings our four focused packages together in one selection. It gives you a set of starting points for shaping a project and carrying out its work, while each package remains available on its own.

## Included Packages

| Package | Contents |
| --- | --- |
| [Project Documents](../project-documents/README.md) | Vision and Architecture Workflows, with four document Templates |
| [Memory Starters](../memory-starters/README.md) | Five Templates for useful knowledge and transfer records |
| [Planning](../planning/README.md) | A planning Workflow, the Work Records Pattern, and four planning Templates |
| [Development](../development/README.md) | Development, Debugging, and Review Workflows |

Together, these packages provide six optional Workflows, the Work Records Pattern, and thirteen Templates. The bundle declares those dependencies and has no `content/` files of its own. [Orchestration](../orchestration/README.md) is available separately when coordination across tasks would help.

The recipes provide a method. Your project's applicable scopes supply its technologies, tools, conventions, and required checks. Existing task systems and record formats stay in use where they already fit.

Templates provide starting content only. Choose one that helps, copy it, and adapt it to its destination. From there, maintain the result independently.

## Install

You can preview the bundle and its dependencies before adding them to a workspace. The [Extension installation guide](../../../docs/extensions.md#install-an-extension) explains the setup. Replace the example paths with your source catalogue and destination workspace, keeping the source checkout separate from that workspace.

```sh
open-forge extension install development-toolkit --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

After reviewing the preview, install it:

```sh
open-forge extension install development-toolkit --source /path/to/open-forge/src/extensions --workspace /path/to/project
```

Review and commit the resulting files and `.agents/open-forge.lifecycle.json`. Each dependency remains a separate package with its own file ownership.

For [manual installation](../../../docs/extensions.md#manual-installation), copy the four dependencies' `content/` files into the workspace. The bundle has no separate payload to copy. Update affected `Entries`, check links, and review the assembled files. Manual copying does not create managed lifecycle state.
