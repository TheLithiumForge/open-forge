# Workflow Essentials

Workflow Essentials is a convenience extension pack for on-demand vision, architecture, and implementation work.

It selects:

- `vision-workflow`
- `architecture-workflow`
- `implementation-workflow`

Each workflow then selects its required skill-only capability extension. This pack adds no parallel runtime route of its own; it is an ordinary dependency-only extension.

```sh
open-forge extend workflow-essentials
```

Installed workflow and skill files remain runtime truth. The pack manifest only composes installation.
