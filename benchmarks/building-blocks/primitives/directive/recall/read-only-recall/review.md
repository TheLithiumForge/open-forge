# Primitive Review

When selected, the recall remains observably read-only.

- The worker performs no file write, formatting, indexing, dependency installation, generated update, or external mutation.
- Only checks already known to preserve state are run against the workspace.
- If trustworthy recall would require mutation, the worker reports the limit instead of crossing it.
