---
open-forge:
  description: Keep reads, writes, generated artifacts, and commands within the authorized workspace and task scope
  tags: [LoadNow, Extension, Directive, Reliability, Workspace, Scope]
---

# Workspace Scope

Keep all work inside the authorized workspace and requested scope.

## Axioms

- Establish the workspace root and the task's read and write boundaries before the first mutation.
- Use an explicit working directory for commands and resolve target paths before consequential file operations.
- Read or write outside the workspace only when the user explicitly authorizes that location and purpose.
- Treat sibling repositories, user-level configuration, shared caches, and external systems as outside scope unless explicitly included.
- Keep generated artifacts, test data, and support files within an authorized workspace location or a tool-owned location whose use is required and non-destructive.
- If an operation crosses the intended boundary, stop, contain or reverse it when safe, and report the scope breach.
