# Dev Workflow

Dev Workflow is a first-party Open Forge extension that installs one workflow: task-driven development through contract-first TDD with a red, green, blue cycle.

It installs:

- the `dev` workflow route

It requires the shared skills from the `workflow-essentials` extension (`workflow-primitives` and `implementation`). Install both:

```sh
open-forge extend --ids workflow-essentials,dev-workflow
```

The installed payload remains the runtime truth. This metadata exists only so the CLI can list and select bundled extensions.
