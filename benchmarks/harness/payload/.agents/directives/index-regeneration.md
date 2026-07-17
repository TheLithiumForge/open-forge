---
open-forge:
  description: Use the declared open-forge CLI when the benchmark runtime provides it; otherwise report and bound any manual index fallback
  tags: [Extension, Directive, Tooling, Routing]
---

# Index Regeneration

The benchmark arm or runtime must declare whether an `open-forge` CLI is available. Do not infer a global installation.

## Axioms

- When the declared runtime provides `open-forge`, after adding, moving, or removing routed Markdown files run `open-forge index .` from the workspace root to regenerate index regions.
- Do not hand-edit content between generated-index markers; the CLI owns that region.
- If the CLI is genuinely unavailable, hand-edit only as a bounded fallback, keep the edit to exactly what the generator would produce, and report the missing tool and fallback in the final response.
