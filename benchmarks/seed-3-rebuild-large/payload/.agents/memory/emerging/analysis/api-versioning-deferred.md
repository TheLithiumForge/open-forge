---
open-forge:
  description: Analysis concluding API versioning is deliberately deferred for this rebuild
  tags: [Extension, Memory, Analysis, Api, Candidate, Contextual]
---

# API Versioning: Deferred

## Question

Should the rebuilt API carry a version prefix (`/v1/...`) from day one?

## Evidence

Single consumer (the CLI in the same repo), released together, no external clients, localhost only. Every versioning mechanism costs route noise now against a hypothetical future migration.

## Conclusion (candidate)

No version prefix in this rebuild. The error contract's stable `code` values are the compatibility surface that matters. Revisit only if a second, independently-released consumer appears.

## Limits

If this tool ever grows a web UI shipped separately, this conclusion is void and versioning should be decided before that UI's first release.
