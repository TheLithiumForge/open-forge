# Planning Workflows

Optional Open Forge workflows for turning uncertain intent into options, an executable plan, and one authoritative task.

It installs or selects:

- the planning and task-creation workflow routes
- `brainstorming-workflow`
- the shared `planning-capability` required by all three workflows

Each workflow uses exactly one authoritative task source declared by the user or workspace for status, ownership, acceptance, and completion. If none is declared, it defaults to the current user task; an agent never promotes an artifact merely because it exists. Durable decisions and rationale stay with their established owners and may be linked from, but not duplicated into, the task source.

Install it with:

```sh
open-forge extend planning-workflows
```

Install only `brainstorming-workflow` when exploration is all that is needed. Installed payload files remain runtime truth; the manifest only composes installation.
