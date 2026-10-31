---
open-forge:
  description: Open Task 90 so installing an Extension also adds the built-in routes it places files into, instead of recording the Extension with its files skipped when a preset left those routes out
  tags: [Memory, Working, CLI, Task, Extensions, Routes, Presets, Install, Contextual, Active]
---

# Task 90 - Extensions install the routes they need

Task 90 makes `open-forge extension install` add the built-in routes an Extension needs, so an Extension works on every preset.

Today the smaller presets leave routes out, and an Extension that places files there installs as a record with no files. For example, on a fresh Essentials workspace, `open-forge extension install collaboration --automatic` reports that it recorded the collaboration Extension without its files, because Guidance and Templates are left out. It then asks the user to run `route init guidance --framework`, `route init templates --framework` and `extension update collaboration`. A user who picked the recommended preset ends up with an Extension that does nothing.

Recorded on 2026-11-01 at the maintainer's request, after the Task 58 evals needed exactly those extra commands to build their Helpful setup. Not started.

## Which Extensions break

Each Extension places files into these routes:

| Extension | Routes it places files into |
| --- | --- |
| Collaboration | `guidance`, `templates` |
| Core Templates | `templates` |
| Scenarios | `templates` |
| Workflows | `skills`, `templates` |
| Development, Orchestration | `skills` |
| Planning | `directives`, `patterns`, `skills`, `templates`, `memory/crystallized/decisions`, `memory/emerging/analysis`, `memory/emerging/ideas`, `memory/working/checkpoints` |
| Project Documents | `skills`, `templates`, `memory/crystallized/documents` |
| Observations and Handoffs | `templates`, `memory/emerging/observations`, `memory/working/handoffs` |

Essentials leaves out Guidance, Maps, Templates and Archived Memory, so Collaboration, Core Templates and Scenarios install no files there, and Planning, Project Documents, Workflows and Observations and Handoffs lose their templates. Minimal leaves out more, so most Extensions lose most of their files.

## Outcome

- Installing an Extension adds each missing built-in route it places files into, the same way `route init <route> --framework` does, then installs the Extension's files.
- The plan names every route it adds before writing, and `--dry-run` shows it.
- A route the user removed on purpose stays removed. The install then stops, or skips those files and says so, instead of quietly restoring it. Which of the two needs a decision.
- Removing an Extension doesn't remove a route that other content or another Extension still uses.

## Done when

- [ ] Each Extension declares the routes it needs, or the CLI derives them from its files.
- [ ] Installing an Extension on Minimal, Essentials and Full Core installs all of its files, with the added routes listed in the plan.
- [ ] A route the user excluded keeps its exclusion, with the behavior above decided and tested.
- [ ] `extension remove` keeps routes still in use.
- [ ] The installation guide and the Extension docs describe the behavior.

## Related

- [Task 48](task48-scoping-for-extension-routes.md) covers scoping for routes an Extension creates. This task covers the built-in routes an Extension needs.
- [Task 58](task58-demo-evals.md) found the problem while building its Helpful setup.
