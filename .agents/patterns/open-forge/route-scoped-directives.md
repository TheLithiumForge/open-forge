---
open-forge:
  description: Decide directive scope from the route before loading it, then treat every loaded directive as binding
  tags: [Pattern, Directive, Routing, Scope, Minimalism]
---

# Route-Scoped Directives

## Shape

```text
loaded parent Entries
  -> select a directive route from path + description + tags
  -> load that directive entrypoint
  -> inherit ancestor Axioms
  -> read every direct directive entry through #LoadNow
  -> obey every loaded directive
```

- Every direct directive file carries #LoadNow relative to its already-loaded parent.
- A direct file under `.agents/directives/` is workspace-wide because the root directive route is baseline-loaded.
- A narrower rule lives below a positively named child directive `entrypoint`; its visible entry is the applicability decision surface.
- A directive file contains one substantive Axioms section. Loading it through the active directive route chain accepts the already-routed scope; inspecting an inactive example, archive, or source payload does not activate it.
- A child scope adds to loaded ancestors. Conflicts are reported instead of resolved through hidden precedence.
- If a directive should not bind, do not route into it. If its visible selection surface is ambiguous, improve the path or description.

## Review Checks

- Scope can be decided before opening the directive body.
- Generic #LoadNow traversal reads every direct file after its directive parent establishes scope.
- No loaded directive can be silently ignored as inapplicable.
- The same top-to-bottom selection pattern used by guidance, patterns, skills, workflows, workspace, and memory is preserved.
