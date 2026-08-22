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
- **Resolution/promotion decision:** Route-inspect resolution consumes an already
  selected exact `CliWorkspace`; it does not create or promote another workspace
  classifier. The existing Shell collapse and residual public-classification risk
  remain outside shared route facts. The unchanged route-list unavailable-workspace
  Integration journey passes after promotion.
- **Route-inspect acceptance disposition:** The comparison is complete. Final
  managed and native Integration include the unchanged Route List unavailable-
  workspace journey. Route Inspect accepts one already selected workspace, so no
  shared correction is truthful in this slice. The Shell classification risk is
  explicitly accepted for Route Inspect and remains visible to final release.
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
- **Route-inspect acceptance disposition:** No stable production-only trigger was
  found. Bounded result-formation, human, JSON, status, exit, and next-action Unit
  evidence is accepted, centered on
  `BoundedFailureFormsFailedWithoutOperationTrigger`. No test-only production hook
  exists. Public failed-process evidence remains an explicit final-release gap.
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
- **Route-inspect acceptance disposition:** Managed and native public evidence
  covers one exact filesystem-safe space/Unicode/emoji source and one option-like
  source after `--`, both with unchanged hashes. NUL, C0 controls, truncation, and
  very long text remain Unit renderer evidence because those values are not all
  valid OS path/process inputs. No untested cross-product is claimed; remaining
  breadth stays with delivery hardening and final release.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  and final delivery hardening through [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and [Accept And Release The Complete CLI](tasks/delivery/04-release.md).
- **Closure condition:** The owning Tasks identify the warranted published-process
  cross-products, add or reuse real public evidence for them, and resolve or
  explicitly accept any remaining breadth boundary before their gates complete.

### CLI-EDGE-005 — Raw lexical option edge

- **Current behavior and evidence:** Global values and occurrences now come from
  typed parser results, and delimiter enforcement stops at `--`. Route List still
  retains its explicitly contracted raw `--depth=` spelling reader; its
  attached-empty case preserves a following typed global through the documented
  parser-symbol arity exception. Dedicated coverage for every option-like
  workspace or source value remains deferred.
- **Risk:** Future Shell consumers could let raw lexical inspection drift from
  parser-owned typed behavior, especially when a workspace or source value looks
  like an option.
- **Resolution/promotion decision:** Promoted route parsers consume only the bound
  typed source reference and never rescan process arguments. The route-list
  attached-empty depth EndToEnd journey passes after promotion. The reviewed Purple
  regression also proves that route-list lexical scanning stops at `--`; the
  resolution/promotion child is closed.
- **Route-inspect acceptance disposition:** Final managed/native evidence preserves
  parser-owned typed globals and the `--` boundary for List and Inspect, including
  an option-like Inspect operand after the terminator. The raw Route List depth
  exception and broader option-looking workspace/source audit remain assigned to
  the authorized generic parser-remediation branch; Route Inspect adds no lexical
  workaround.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  owns focused option-terminator regression. The maintainer-authorized generic
  parser-remediation branch owns the remaining raw depth audit before test
  architecture; [Accept And Release The Complete CLI](tasks/delivery/04-release.md)
  owns final residual-risk acceptance.
- **Closure condition:** Route Inspect preserves parser-owned global and operand
  behavior at `--`. The generic remediation applies general typed validation and
  removes or explicitly reaccepts the remaining raw depth exception before test
  architecture, without adding another spelling-specific workaround.

### CLI-EDGE-007 — Terminal modes with domain input

- **Current behavior and evidence:** The shared Shell validates global input and
  then short-circuits `--help` or `--version` before command binding. A valid leaf
  may therefore accept and ignore a domain operand or operation-specific option in
  terminal mode. Valid root, group, and leaf terminal bypass is covered; the
  conflicting domain-input forms are not claimed as conforming.
- **Risk:** The shared Global Flags contract requires domain operands and local
  flags to remain invalid with terminal modes. A command-local workaround would
  duplicate parser policy and could diverge across leaves.
- **Triage:** Record as a reproducible general Shell parser-policy deviation, not
  an automatic blocker for valid Route Inspect help/version presentation. The
  general solution must inspect library-owned typed symbol results and apply one
  shared terminal-input invariant; it must not rescan raw arguments.
- **Owning Task(s):** The maintainer-authorized generic-improvements branch owns
  parser remediation before test architecture. Route Inspect presentation owns
  only valid terminal bypass evidence and must not claim the conflicting forms.
- **Closure condition:** Focused pinned-parser evidence proves root, group, and
  leaf terminal modes; one general typed validation rejects domain operands and
  operation-specific options while accepting other well-formed globals as no-ops.

### CLI-EDGE-006 — Six-RID parity

- **Current behavior and evidence:** Local route-list evidence is `win-x64` only.
  Six-RID proof remains owned by [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and is not inferred from local results.
- **Route-inspect local evidence:** The final corrected tree publishes and executes
  the local `win-x64` Native AOT root with public EndToEnd `36/36`, the native
  Integration runner `166/166`, and the native EndToEnd runner `36/36`. This is
  explicitly one-RID evidence and does not change the delivery owner or closure
  condition.
- **Risk:** A local managed or `win-x64` Native AOT pass does not establish parity
  on the other native RIDs or the declared support floors.
- **Owning Task(s):** [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and [Accept And Release The Complete CLI](tasks/delivery/04-release.md).
- **Closure condition:** Native CI records reproducible evidence for all six native
  RIDs and the support-floor environments. Release consumes that evidence and
  records acceptance or an explicit residual risk before its gate completes.
