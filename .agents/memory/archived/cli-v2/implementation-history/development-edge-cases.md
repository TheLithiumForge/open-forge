---
open-forge:
  description: Historical CLI-v2 status edge case recorded during deleted implementation work
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Development Edge Cases

This record belonged to deleted CLI-v2 implementation work. It is retained as
historical evidence and does not define the new CLI.

## CLI Status

### Git Trace Before A Non-Repository Diagnostic

- Trigger: the caller enables Git tracing, such as `GIT_TRACE=1`, and runs
  `status` outside a Git repository.
- Observed behavior: Git writes trace lines before its ordinary
  `not a git repository` diagnostic, so Open Forge reports Git inspection as
  `failed` instead of `not-repository`.
- Disposition: accepted on 2026-08-04 as a user-debug environment edge case.
  Normal environments were unaffected, and no further correction was planned.
