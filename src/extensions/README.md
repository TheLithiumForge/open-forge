# Open Forge Extensions

We built these Extensions to give you more starting points as you make Open Forge your own. Each package is optional, so you can bring in what helps your work and adapt it to your workspace.

## Current Catalogue

| Package | Contents | Dependencies |
| --- | --- | --- |
| [Project Documents](project-documents/README.md) | Vision and Architecture Workflows, with four document Templates | None |
| [Memory Starters](memory-starters/README.md) | Five Memory Templates | None |
| [Planning](planning/README.md) | A planning Workflow, the Work Records Pattern, and four planning Templates | None |
| [Development](development/README.md) | Development, Debugging, and Review Workflows | None |
| [Orchestration](orchestration/README.md) | One Workflow for dependent tasks, recovery, and integration | Planning, Development |
| [Development Toolkit](development-toolkit/README.md) | A bundle of Project Documents, Memory Starters, Planning, and Development | All four included packages |

Choose a focused package for the work at hand, or use Development Toolkit to bring its four dependencies in together. Orchestration is available separately when coordination across tasks would help.

Use what fits, adapt it as your project changes, and remove content that adds no value. Installed Workflows stay optional.

## Installation

You can install a package with the CLI or copy the files yourself. The [Extension installation guide](../../docs/extensions.md#install-an-extension) covers both approaches.

These commands let you browse the catalogue and preview a package before adding it to your workspace. Replace the example paths with your source catalogue and destination workspace, keeping the source checkout separate from that workspace.

```sh
open-forge extension list --available --source /path/to/open-forge/src/extensions --workspace /path/to/project
open-forge extension install development-toolkit --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
open-forge extension install orchestration --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

After reviewing the preview, apply the same command without `--dry-run`. The guide also covers updates, removal, and permissions. Managed state lives in `.agents/open-forge.lifecycle.json`. It does not become agent context.

For [manual installation](../../docs/extensions.md#manual-installation), copy the `content/` files from the selected package and its dependencies into the workspace. A bundle has no separate payload to copy. Update affected `Entries`, check links in the assembled workspace, and review the changes. Manual copying does not create managed lifecycle state.

## Source Shape

Each Extension adds complete files. A package contains `extension.json`, an optional `README.md`, and workspace-relative files under `content/`. A package containing only dependencies may omit `content/`.

Routed content follows the roles and scopes of its destination in the Framework. Native formats and support files keep the meaning defined by their consumers. The installed files remain usable without the package manifest or CLI.

The manifest identifies the package and declares its dependencies. Dependencies resolve transitively before their dependents. The [package format](../../docs/extensions.md#package-format) defines the accepted representation. [Create an Extension](../../docs/extensions.md#create-an-extension) walks through assembling one.

## Content And Verification

Each reusable file has one package source. Dependency packages provide shared files instead of competing copies. Runtime relationships use ordinary links and explicit Workflow steps because manifest dependencies describe installation only.

Keep native formats such as `SKILL.md` in their native form. Routed Open Forge files normally carry `Extension`, their role, and useful topic tags. Add eager-loading tags only when their loading cost is deliberate.

Check each dependency closure for conflicting destinations and missing links, and verify that its metadata and Workflow sections are valid. Check route indexing after assembly. Verify installation and removal in a separate workspace with a CLI built from the same source tree.

Keep packaged Templates and the Work Records Pattern aligned with their repository copies. Local Workflow profiles may differ where repository-specific rules justify it.

Adding a package to this catalogue requires evidence of useful outcomes and clear decisions about dependencies and runtime boundaries. A successful local experiment alone does not establish that it belongs in the catalogue.
