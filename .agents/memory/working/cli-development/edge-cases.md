---
open-forge:
  description: Active ledger of deferred replacement-CLI edge cases, owners, risks, and closure conditions
  tags: [Memory, Working, CLI, EdgeCase, Evidence, Contextual, Active]
---

# Replacement CLI Edge-Case Ledger

This is the active ledger for deferred edge cases in the replacement CLI. The
archived CLI-v2 [development edge-case record](../../archived/cli-v2/implementation-history/development-edge-cases.md)
is historical evidence and remains non-authoritative for the replacement.

An item in this ledger is not a current acceptance blocker unless its owning Task
says that it is. Before an owning gate completes, the owning Task must resolve the
item or explicitly accept the remaining risk. This ledger records evidence and
closure conditions; it does not change a command contract or acceptance state.

## Owning Tasks

Use these current Task links when assigning or closing an item.

- [Accept Route List](tasks/route-discovery/done/route-list-acceptance.md)
- [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
- [Implement Index](tasks/read-only/index.md)
- [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
- [Accept And Release The Complete CLI](tasks/delivery/04-release.md)

## Deferred Items

### CLI-EDGE-001 — Legacy routing-tool duplicate-entrypoint reports

- **Current behavior and evidence:** `open-forge-old doctor` and `open-forge-old index`
  report the read-only Task folder as having `_read-only.md`, `index.md`, and
  `references.md` as multiple recognized entrypoints. They also report
  `.agents/memory/crystallized/documents/cli/contracts/index` as
  `_index.md, _index.md`, and the runs emit 502 warnings. The maintainer directed
  these false/duplicate-entrypoint reports to this ledger, so they no longer
  block route-list closeout.
- **Risk:** The legacy router cannot provide clean Doctor or generated-Entries
  evidence for these paths, which can obscure repository-routing validation. This
  is a routing-tool and repository-validation issue, not evidence that route-list
  behavior is incorrect.
- **Owning Task(s):** [Implement Index](tasks/read-only/index.md), with [Complete
  Native CI And Support Floors](tasks/delivery/02-native-ci.md) and [Accept And
  Release The Complete CLI](tasks/delivery/04-release.md) handling delivery and
  release validation as appropriate.
- **Closure condition:** The Index Task records a correction or explicit
  compatibility/validation acceptance for the duplicate-entrypoint reports. The
  applicable delivery or release gate records any remaining waiver and does not
  claim that legacy routing passed without evidence.

### CLI-EDGE-002 — Workspace failure classification

- **Current behavior and evidence:** The current Shell collapses workspace-selection
  failures into `cli.workspace.invalid`. `route list` emits the `invalid-workspace`/`invalid`
  result even though its model can represent
  `workspace-unavailable` and `blocked`.
- **Risk:** Invalid selection, unavailable workspace state, and blocked safety
  state can lose their intended distinction. A shared correction made before the
  meanings are compared could change route-list results or promote incompatible
  semantics.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  must compare identical workspace semantics before any shared correction; [Accept
  Route List](tasks/route-discovery/done/route-list-acceptance.md) owns the
  required route-list regression.
- **Closure condition:** Route inspect records the comparison and the promotion
  decision. If behavior changes, route-list regression evidence proves the chosen
  classification; otherwise the owning Task explicitly accepts the existing
  distinction and its risk.

### CLI-EDGE-003 — Deterministic public `failed` journey

- **Current behavior and evidence:** Unit evidence proves failed-result formation
  and all renderer, status, and stream rules. There is no production-only stable
  external trigger for an unexpected internal failure.
- **Risk:** A public process journey for an unexpected internal failure is not
  demonstrated by a stable external trigger. Do not add a test hook solely to force
  this result.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  and [Accept And Release The Complete CLI](tasks/delivery/04-release.md) at final
  acceptance.
- **Closure condition:** Route inspect and final acceptance decide whether
  additional real trigger evidence exists. They record either that production-only
  public evidence or that no stable trigger exists and the bounded Unit evidence is
  explicitly accepted, without adding a test-only trigger.

### CLI-EDGE-004 — Hostile process-input breadth

- **Current behavior and evidence:** Control-character, NUL, very long Unicode,
  and option-like values have parser, escaping, and selected process evidence. Not
  every cross-product of those inputs is published-process evidence.
- **Risk:** A missing cross-product could expose a difference between managed or
  selected-process behavior and the public published executable. The gap is a
  breadth limitation, not evidence that every hostile input fails.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  and final delivery hardening through [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and [Accept And Release The Complete CLI](tasks/delivery/04-release.md).
- **Closure condition:** The owning Tasks identify the warranted published-process
  cross-products, add or reuse real public evidence for them, and resolve or
  explicitly accept any remaining breadth boundary before their gates complete.

### CLI-EDGE-005 — Raw lexical option edge

- **Current behavior and evidence:** Binding now receives bounded original
  arguments. Attached-empty `--depth= --json` forms a typed JSON invalid result,
  and lexical scanning stops at `--`. Dedicated coverage for every option-like
  workspace or source value remains deferred.
- **Risk:** Future Shell consumers could let raw lexical inspection drift from
  parser-owned typed behavior, especially when a workspace or source value looks
  like an option.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  owns the Shell-consumer regression; [Accept And Release The Complete CLI](tasks/delivery/04-release.md)
  owns the final-acceptance decision on the remaining coverage.
- **Closure condition:** The route-inspect regression preserves the bounded raw
  argument and typed-parse interaction. Final acceptance records dedicated
  coverage or an explicit acceptance of the remaining option-like workspace/source
  values without broadening lexical scanning beyond its accepted boundary.

### CLI-EDGE-006 — Six-RID parity

- **Current behavior and evidence:** Local route-list evidence is `win-x64` only.
  Six-RID proof remains owned by [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and is not inferred from local results.
- **Risk:** A local managed or `win-x64` Native AOT pass does not establish parity
  on the other native RIDs or the declared support floors.
- **Owning Task(s):** [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and [Accept And Release The Complete CLI](tasks/delivery/04-release.md).
- **Closure condition:** Native CI records reproducible evidence for all six native
  RIDs and the support-floor environments. Release consumes that evidence and
  records acceptance or an explicit residual risk before its gate completes.
