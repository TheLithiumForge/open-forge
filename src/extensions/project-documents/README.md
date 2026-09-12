# Project Documents

Project Documents helps when a project's direction or structure needs a clear explanation. We paired two optional Workflows with four document Templates, so you can work through the ideas and shape what is worth keeping.

## Contents

The [Vision Workflow](content/.agents/workflows/vision.md) helps clarify a subject's purpose and first useful version. The [Architecture Workflow](content/.agents/workflows/architecture.md) works through its structure and relationships.

The [Document Templates](content/.agents/templates/documents/_documents.md) provide starting pages for the resulting explanations:

| Template | Starting point |
| --- | --- |
| [Vision](content/.agents/templates/documents/vision.md) | The project's purpose, value, and direction |
| [Principles](content/.agents/templates/documents/principles.md) | The principles that guide the project |
| [Architecture](content/.agents/templates/documents/architecture.md) | The system's structure and relationships |
| [Maintenance Contract](content/.agents/templates/documents/maintenance-contract.md) | What a source maintains and how to keep it correct |

Use existing project sources when they already hold the answer. Create a separate document when a durable current view would help. Copy any Template you need, adapt it to the subject, and maintain the result independently.

## Install

The [Extension installation guide](../../../docs/extensions.md#install-an-extension) covers setup. Preview this package with the CLI, replacing the example paths with your source catalogue and destination workspace. Keep the source checkout separate from that workspace.

```sh
open-forge extension install project-documents --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

Apply the reviewed plan with the same command without `--dry-run`. Review the resulting files and `.agents/open-forge.lifecycle.json`.

For [manual installation](../../../docs/extensions.md#manual-installation), copy this package's `content/` files into the workspace. Update affected `Entries`, check links, and review the assembled files. This package has no Extension dependencies. Manual copying does not create managed lifecycle state.
