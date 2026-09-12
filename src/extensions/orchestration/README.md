# Orchestration

Orchestration gives you a method to come back to when the work spans several tasks. Its one optional Workflow, [Managed Delivery](content/.agents/workflows/managed-delivery.md), helps coordinate dependencies and bring the results together. If work is interrupted, the recipe also covers how to resume it.

The package depends on [Planning](../planning/README.md) for its planning method and Work Records Pattern, and [Development](../development/README.md) for implementation and review.

Coordination and task execution have separate responsibilities, and one person or agent can handle both. A separate integration role is useful only when combining results needs dedicated attention. You can work through the method sequentially.

Parallel work requires authorization and suitable isolation in the environment you use. The package provides the method. Scheduling and permissions remain outside it. It has no model roster, fixed agent hierarchy, or APM dependency.

## Use

You can preview the package and its dependencies with the [Extension installation guide](../../../docs/extensions.md#install-an-extension). Replace the example paths with your source catalogue and destination workspace, keeping the source checkout separate from that workspace.

```sh
open-forge extension install orchestration --source /path/to/open-forge/src/extensions --workspace /path/to/project --dry-run
```

Apply the reviewed plan with the same command without `--dry-run`. For [manual installation](../../../docs/extensions.md#manual-installation), copy the `content/` files from this package, Planning, and Development into the workspace, then update affected `Entries`, check links, and review the assembled files. Manual copying does not create managed lifecycle state. The [catalogue](../README.md) describes package composition and verification.

The installed recipe is `.agents/workflows/managed-delivery.md`. Select it when cross-task coordination adds value. Installation does not authorize parallel execution, commits, integration, or publication.

## Evidence

Judge this Workflow by the work it helps you deliver: how it coordinates tasks, preserves what you need to resume, and verifies authorized integration. Claims about efficiency or reliability gains need evidence from the workspaces and conditions where those gains are expected.
