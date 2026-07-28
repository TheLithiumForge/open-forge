# Quality Workflows

Optional Open Forge workflows for evidence-backed review, testing, behavior-preserving refactoring, and root-cause debugging.

It installs or selects:

- review, refactoring, and debugging workflow routes
- `testing-workflow`
- the shared `quality-capability` required by all four workflows

Review is read-only unless fixes are explicitly requested. Testing distinguishes running existing checks from authoring tests, and no workflow weakens evidence merely to make a check pass.

Install it with:

```sh
open-forge extend quality-workflows
```

Install only `testing-workflow` when test work is all that is needed. Installed payload files remain runtime truth. The manifest only composes installation.
