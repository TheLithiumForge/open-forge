---
open-forge:
  description: The open-forge CLI is installed globally; regenerate index regions with it instead of hand-editing generated regions
  tags: [Extension, Directive, Tooling, Routing]
---

# Index Regeneration

The `open-forge` CLI is installed globally in this environment.

## Axioms

- After adding, moving, or removing routed markdown files, run `open-forge index .` from the workspace root to regenerate index regions.
- Do not hand-edit content between generated-index markers; the CLI owns that region.
- If the CLI is genuinely unavailable, hand-edit as a fallback, keep the edit to exactly what the generator would produce, and report the fallback in the final response.
