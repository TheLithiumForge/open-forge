---
open-forge:
  description: Historical CLI-v2 source: Reconcile managed files through explicit current, missing, changed, retired, shared, delete, keep, and ownership-release states
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Managed Reconciliation

## Boundary

The accepted [Managed Lifecycle contract](../../../../memory/crystallized/documents/cli/contracts/managed-lifecycle.md) is authoritative for desired state, exclusions, ownership, checksums, decision meanings, eligibility, and automation behavior. This Pattern owns the reusable fact, decision, presentation, and replanning arrangement.

## Shape

Keep desired management evidence separate from current filesystem state:

```text
manager catalogue
  + managed records and exclusions
  + current exact files
  -> typed reconciliation facts
  -> complete decision set
  -> planned mutation
```

Model each meaningful relationship as a named typed state rather than one changed boolean. The reconciliation module projects those facts into the exact decisions permitted by the lifecycle contract. It does not let a presenter infer ownership or deletion eligibility.

Keep one decision row structurally explicit:

```text
Decision             Subject              Evidence
<literal decision>   <logical identity>   <short reason>
```

The presenter receives the complete typed alternatives for each row, toggles only among them, and returns the chosen decision set. It derives no meaning from color, icons, or list position. Any changed decision invalidates the old plan and causes complete replanning and preflight.

Keep compatible shared ownership directly navigable from each manager record. The planner queries the complete owner set before it proposes a content effect; the application layer never discovers another owner during deletion.

## Placement

Keep fact inspection, decision projection, presentation data, and plan construction as separate focused modules beside the lifecycle domain. Share generic plan application only through the mutation boundary.

## Review Checks

- Desired management, persisted choice, current presence, current bytes, and ownership are separate facts.
- The presenter consumes permitted decisions rather than inventing lifecycle meaning.
- Literal decision text remains legible without decoration.
- Shared ownership is available before effect planning.
- A changed decision triggers a new complete plan.
- Exact decision and automation semantics are linked to the Managed Lifecycle contract instead of restated here.
