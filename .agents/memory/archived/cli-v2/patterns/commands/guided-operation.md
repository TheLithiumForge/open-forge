---
open-forge:
  description: Historical CLI-v2 source: Keep a helpful CLI wizard and deterministic automation path over one request, plan, application, and result contract
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Guided CLI Operation

## Boundary

The accepted [Request Construction contract](../../../../memory/crystallized/documents/cli/contracts/request-construction.md) is authoritative for defaults, selection, prompting, confirmation, preview, cancellation, structured automation, and authority. This Pattern owns the reusable resolver and presentation arrangement that implements those semantics.

## Shape

Keep interaction outside the operation:

```text
interactive presentation       explicit arguments and policies
          |                                  |
          +--------> complete request <------+
                              |
                        named handler
                              |
                    plan, apply, and verify
                              |
                         typed result
```

Use one command-local resolver before the named handler:

```text
validated command values
  + immutable execution context
  + focused interaction presenter
  -> resolve request
  -> ready | invalid | blocked | cancelled
```

The resolver returns a complete semantic request or one typed terminal outcome. It does not return a partially selected request for the handler to repair. Parser flags become focused execution or interaction policy before this boundary; raw flag names and terminal state do not enter the request.

The interactive presenter:

1. Receives typed questions and safe evidence from request resolution.
2. Shows the mechanically known subject, choices, consequences, and documented default.
3. Returns one typed choice without planning or mutating.
4. Uses the same resolver again until the request is complete or terminal.

Plan confirmation follows complete planning and preflight. A changed semantic choice discards that plan and restarts request resolution rather than editing a validated plan in place.

Focused trust and lifecycle interactions remain separate collaborators:

- External content uses the [Reviewed Source Boundary](reviewed-source-boundary.md).
- Managed lifecycle choices use the [Managed Reconciliation](../filesystem/managed-reconciliation.md) Pattern.
- Executable formatter choices use the [Workspace Formatter Strategy](../workspace/workspace-formatter-strategy.md).
- Persistent effects use the [Planned Mutation](../filesystem/planned-mutation.md) Pattern.

## Review Checks

- Interactive and explicit inputs produce the same complete request type.
- The resolver owns incomplete input; the handler receives semantic intent only.
- Each prompt removes one material uncertainty or confirms one inspectable plan.
- Presentation never creates effects, catches failures through another operation path, or changes result semantics.
- Specialized authority boundaries remain separate collaborators rather than wizard modes.
- A changed post-plan choice causes complete resolution and planning to restart.
- Exact selection and authority behavior is linked to the Request Construction contract instead of restated here.
