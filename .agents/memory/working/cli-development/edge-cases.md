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
- [Implement Index](tasks/read-only/index-command.md)
- [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
- [Accept And Release The Complete CLI](tasks/delivery/04-release.md)
- [Remediate Parser And Standard Behavior](tasks/generic-improvements/parser-remediation.md)
- [Implement The Find Query Operation](tasks/read-only/find-query-operation.md)

## Deferred Items

### CLI-EDGE-001 — Legacy routing-tool duplicate-entrypoint reports

- **Current behavior and evidence:** The compatibility filenames `index.md`,
  `references.md`, `_index.md`, and `_references.md` made the frozen routing tool
  classify ordinary Task files or the same compatibility entrypoint more than
  once. The Working Tasks now use `index-command.md` and
  `references-command.md`. The current contracts remain staged under
  `index-candidate/` and `references-candidate/` until the replacement `index`
  command validates their final compatibility-name paths.
- **Risk:** The temporary names preserve legacy routing assistance, but the final
  compatibility-name regression remains unproved by the replacement command.
- **Owning Task(s):** [Implement Index](tasks/read-only/index-command.md), with [Complete
  Native CI And Support Floors](tasks/delivery/02-native-ci.md) and [Accept And
  Release The Complete CLI](tasks/delivery/04-release.md) handling delivery and
  release validation as appropriate.
- **Closure condition:** The replacement Index Task validates each final
  compatibility entrypoint by physical identity before the candidate contracts
  move to `contracts/index/_index.md` and
  `contracts/references/_references.md`. The applicable delivery or release gate
  records any remaining limitation without claiming unproved compatibility.

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

- **Current behavior and evidence:** Global and Route List depth values,
  occurrences, and aggregate value counts come from typed parser results. Global
  custom parsers and the raw depth spelling reader are removed. Route List retains
  only its explicitly contracted equals-only delimiter guard, and that inspection
  stops at `--`. Attached-empty workspace, view, and depth values preserve a
  following typed global and become invalid through typed validation.
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
- **Parser-remediation acceptance disposition:** Public 2.0.11 result facts now own
  scalar and multi-value occurrences and aggregate values. The retained delimiter
  guard distinguishes only Route List depth spelling; it does not parse a value,
  count occurrences, cross `--`, or enter command binding. Focused Unit `68/68`,
  Integration `93/93`, and freshly published managed selected EndToEnd `57/57`
  pass. The exact attached-empty depth JSON journey and ordinary option-like
  post-terminator Inspect journey pass without workspace writes.
- **Owning Task(s):** [Implement Route Inspect And Promote Shared Route Facts](tasks/route-discovery/route-inspect.md)
  owns focused option-terminator regression. [Parser And Standard Behavior
  Remediation](tasks/generic-improvements/parser-remediation.md) closes the raw depth
  audit. [Accept And Release The Complete CLI](tasks/delivery/04-release.md) owns final
  residual-risk acceptance.
- **Closure condition:** Met for parser remediation. Route Inspect preserves
  parser-owned global and operand behavior at `--`; generic typed validation removes
  raw depth value authority; and the one documented equals-only delimiter exception
  remains bounded without another spelling-specific workaround. Final release still
  owns broader public input breadth.

### CLI-EDGE-007 — Terminal modes with domain input

- **Current behavior and evidence:** The shared Shell validates parser and delimiter
  results, reads typed global input, resolves terminal conflict policy, and applies
  one typed terminal-input validator before help/version short circuiting, workspace
  selection, command binding, or operation work. Root, group, List, Inspect, local-
  option, unmatched, and option-like post-terminator conflicts are rejected. Other
  well-formed globals remain terminal no-ops.
- **Risk:** The shared Global Flags contract requires domain operands and local
  flags to remain invalid with terminal modes. A command-local workaround would
  duplicate parser policy and could diverge across leaves.
- **Historical triage:** Record as a reproducible general Shell parser-policy
  deviation, not an automatic blocker for valid Route Inspect help/version
  presentation. The general solution must inspect library-owned typed symbol results
  and apply one shared terminal-input invariant; it must not rescan raw arguments.
- **Parser-remediation acceptance disposition:** The general invariant is implemented
  from `CommandResult.Children`, explicit `ArgumentResult` and `OptionResult`
  identities, and `ParseResult.UnmatchedTokens`. Exact public
  `route inspect --help root --json` exits `4`, writes no stdout, writes one bounded
  stderr diagnostic, and performs no workspace work. Focused Unit `68/68`,
  Integration `93/93`, and freshly published managed selected EndToEnd `57/57`
  pass.
- **Owning Task(s):** [Parser And Standard Behavior
  Remediation](tasks/generic-improvements/parser-remediation.md) closes the general
  parser-policy deviation. Route Inspect presentation owns only valid terminal bypass
  evidence and does not claim the conflicting forms.
- **Closure condition:** Met. Focused pinned-parser evidence proves root, group, and
  leaf terminal modes; one general typed validation rejects domain operands,
  operation-specific options, and unmatched input while accepting other well-formed
  globals as no-ops.

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

### CLI-EDGE-008 — Find selector state completeness

- **Current behavior and evidence:** The accepted Find selector schema has no
  not-started or unavailable resolution state. Child 2 therefore reports a
  syntactically valid supplied selector as `invalid` when another Find-local input
  prevents source resolution from starting. It reports an exact path as
  `unsupported` when the path is physically eligible but an unavailable parent
  directory enumeration prevented the sole source catalogue from admitting it.
  The physical fallback classifies the existing finite outcome only; it never
  creates a second catalogue candidate or selected source.
- **Maintainer disposition:** Reuse the existing `invalid` and `unsupported`
  states for Child 2 and keep this edge durable as a possible contract improvement.
  This disposition is accepted for the current non-shipping Find implementation
  and does not authorize a Shell change, second catalogue, guessed source, or
  hidden selector state.
- **Risk:** `invalid` can describe a selector that was not itself invalid, and
  `unsupported` can describe a supported source whose catalogue admission was
  unavailable. Automation cannot distinguish those narrower causes from ordinary
  invalid or unsupported selectors through the current finite selector field.
- **Owning Task(s):** [Implement The Find Query
  Operation](tasks/read-only/find-query-operation.md) records and proves the bounded
  reuse. [Accept And Release The Complete CLI](tasks/delivery/04-release.md) owns
  any later public-schema refinement before shipping.
- **Closure condition:** A later accepted Find contract may add truthful
  `not-started` and `unavailable` selector states, with any required finding,
  status, renderer, JSON, and compatibility changes. Until then, tests preserve
  the current finite-state reuse and no implementation infers a missing source or
  bypasses the neutral catalogue.

### CLI-EDGE-009 — Find identity-unavailable finding reuse

- **Current behavior and evidence:** The neutral catalogue can retain a recognized
  Markdown candidate without an automatic ID, including a root `.agents/SKILL.md`
  or a file named `.agents/.md`. It emits `IdentityUnavailable` and cannot form a
  logical source. The accepted Find finding vocabulary has no identity-unavailable
  code, while every unresolved candidate that could change the result set must
  keep matching coverage incomplete.
- **Maintainer disposition:** Map `IdentityUnavailable` to the existing
  `find.layer-unresolved` incomplete finding for Child 2. This keeps the candidate
  visible, avoids inventing an ID or logical source, and preserves safe unrelated
  matches. Record a dedicated identity finding as a possible contract improvement
  rather than changing the frozen vocabulary during this child.
- **Risk:** `find.layer-unresolved` currently describes orphan or ambiguous
  overwrite relationships in the Interface. Automation cannot distinguish that
  ordinary meaning from unavailable automatic identity through the current
  finding code alone. The finding subject, path, and cause must retain the direct
  identity failure without pretending an overwrite exists.
- **Owning Task(s):** [Implement The Find Query
  Operation](tasks/read-only/find-query-operation.md) owns the bounded mapping and
  Red evidence. [Accept And Release The Complete CLI](tasks/delivery/04-release.md)
  owns any later public finding-vocabulary refinement before shipping.
- **Closure condition:** A later accepted Find contract may add a dedicated
  `find.identity-unavailable` finding and update status, ordering, renderers, JSON,
  and compatibility evidence. Until then, tests preserve the incomplete
  `layer-unresolved` reuse and prove that no ID or source is fabricated.

### CLI-EDGE-010 — Authored prose around the generated `Entries` region

- **Current behavior and evidence:** The frozen indexer requires the generated
  start marker to immediately follow `## Entries`. Moving an authored description
  above that heading lets the current repository index complete, but an ordinary
  description below the heading and before the start marker is rejected even
  though it is outside the marked generated interior.
- **Risk:** The replacement Index command could retain a line-oriented adjacency
  restriction despite already parsing Markdown into an AST, or could overcorrect
  by treating every node under `Entries` as manager-owned and overwrite authored
  explanation.
- **Owning Task(s):** [Implement Index](tasks/read-only/index-command.md).
- **Closure condition:** Index uses its parsed Markdown structure to recognize one
  valid managed region within the `Entries` section, preserves authored nodes and
  bytes outside that region, and proves malformed, duplicate, nested, and
  marker-like code cases. The separate [managed-region
  ideas](../../emerging/ideas/deferred-product-ideas.md#deferred-product-ideas)
  analyze whether marker comments can ever be removed safely.
