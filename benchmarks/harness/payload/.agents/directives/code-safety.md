---
open-forge:
  description: Keep generated code safe, explicit, and reviewable
  tags: [Extension, Directive, Security, CodeQuality]
---

# Code Safety

Write code that is safe to inspect and safe to run locally.

## Applies To

- Benchmark worker tasks that create, modify, generate, or execute code in the seeded workspace.

## Axioms

- Do not use `eval`, dynamic function construction, or string-to-code execution.
- Do not silence type, runtime, or test failures with suppression comments or broad casts.
- Do not build shell commands from user input.
- Do not delete or overwrite user data except for files clearly owned by the current workspace and current task.
- Do not catch and discard errors silently.
- Surface corrupt local data as a clear user-facing error.
- Prefer explicit parsing and validation at I/O boundaries.
