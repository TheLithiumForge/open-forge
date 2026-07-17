# Dev Workflow

Dev Workflow installs one mixed delivery workflow: shape a slice, implement, test, review or refactor, retest, diagnose and fix regressions, then close out.

Choose it for full iterative delivery. Use `implementation-workflow` for one focused implementation pass or `testing-workflow` when the task is evidence-only.

It installs:

- the `dev` workflow route

It supports test-first and implement-first entry paths. The workflow selects the path from the risk, evidence, and repository conventions rather than treating one order as universal.

It depends on the skill-only `implementation-capability` and `quality-capability` extensions. The selector marks both as required and the CLI installs them first.

```sh
open-forge extend dev-workflow
```

The installed payload remains the runtime truth. This metadata exists only so the CLI can list and select bundled extensions.
