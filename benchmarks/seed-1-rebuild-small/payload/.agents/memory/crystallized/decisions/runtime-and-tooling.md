---
open-forge:
  description: Accepted runtime and tooling choice for the seed implementation
  tags: [Extension, Memory, Decision, Architecture, Tooling, CurrentTruth]
---

# Runtime and Tooling

## Decision

The bookmarks CLI should be implemented as a TypeScript project using Bun as the primary local toolchain.

## Consequences

- Use TypeScript source files.
- Use Bun scripts for install, test, and development where practical.
- Keep the runtime dependency set empty unless a dependency has a clear written reason.
- Prefer standard Web and JavaScript APIs when they are supported by Bun.
- Keep generated build output out of source control unless the implementation explicitly needs it.

## Fallback

If Bun is unavailable in the environment, report that clearly. Use the closest Node.js/npm fallback only when continuing is more useful than blocking, and document the deviation in the final report.

Note: runtimes are sometimes installed but off the default PATH (for example under a version manager). Check for an off-PATH installation before concluding a runtime is absent, and record what was actually found rather than what the PATH suggested.
