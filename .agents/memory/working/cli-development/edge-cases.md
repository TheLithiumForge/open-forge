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
- [Implement Context](tasks/read-only/context.md)
- [Accept Find Presentation And The Complete Command](tasks/read-only/find-presentation-acceptance.md)
- [Implement Doctor](tasks/operations/doctor.md)
- [Implement Repair](tasks/operations/repair.md)

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

### CLI-EDGE-006 — Native delivery scope beyond `linux-x64`

- **Current accepted boundary:** D1 is thin. Current required native evidence is
  `linux-x64` build and smoke, packed install and invocation, and checksums. Past
  `win-x64` or other local results remain historical evidence and do not create a
  current support claim.
- **Deferred expansion:** Additional RIDs, signatures, SBOM, provenance, OIDC
  attestation, and support-floor matrices require a later explicit maintainer
  decision. They are not missing current D1 evidence and are not inferred from
  managed or single-host results.
- **Risk:** Documentation or release automation could revive the former six-RID
  and support-floor promise without an accepted cost, runner, package, evidence,
  and maintenance decision.
- **Owning Task(s):** [Complete Native CI And Support Floors](tasks/delivery/02-native-ci.md)
  and [Accept And Release The Complete CLI](tasks/delivery/04-release.md) preserve
  the current thin boundary. A future program change owns any expansion.
- **Closure condition:** Met for current D1 when reproducible `linux-x64` native
  build/smoke, packed installation/invocation, and checksums pass and release
  claims no broader RID, signature, SBOM, provenance, OIDC, attestation, or
  support-floor coverage. Any broader claim reopens this item only through an
  explicit maintainer decision and updated Architecture, Plan, Tasks, and
  evidence.

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

### CLI-EDGE-011 — Context rejects valid native Skill metadata

- **Severity/category:** Material correctness; source-form metadata and Context
  closure.
- **Disposition:** Closed on accepted local candidate `deb3f14`. Context now
  consumes the form-aware `SourceAuthoredMetadataFacts`; valid and malformed
  routed Skill Integration is `2/2`, published process is `3/3` across the valid,
  malformed, and distinct-path cases, and the full managed/Native AOT gates pass.
  Repository dogfooding no longer reports the valid
  `experience-design/SKILL.md` while retaining the 65 expected repository
  metadata findings.
- **Original behavior and evidence:** Repository dogfooding reported routed
  `.agents/skills/experience-design/SKILL.md` as
  `context.closure-unavailable` even though its native `name` and `description`
  frontmatter is valid. The shared authored-metadata capability recognizes
  `SourceDocumentForm.Skill`, but Context builds its graph through a parser that
  accepts only Open Forge metadata. It then treats every routed non-entrypoint
  source without complete Open Forge metadata as having unknown global-continuity
  membership.
- **Consequence:** A valid native Skill makes otherwise safe startup Context
  coverage incomplete. This is a false positive and prevents trustworthy Context
  dogfooding on a canonical workspace.
- **Smallest credible correction:** Make Context consume the accepted form-aware
  routed authored-metadata facts, preserving native Skill description and its
  source-contract classification without inventing Open Forge tags. Add focused
  Unit, real-workspace Integration, and published-process regression evidence for
  a routed valid Skill and malformed Skill metadata.
- **Earliest invalidated boundary:** Context graph metadata formation before
  global-continuity selection.
- **Owning Task(s):** [Implement Context](tasks/read-only/context.md) owns the
  behavior. A proposed pre-Mutation-Foundation dogfooding correction slice should
  reopen only this bounded boundary; final release owns any residual disposition.
- **Closure condition:** A valid routed `SKILL.md` contributes its native authored
  metadata without a closure finding, malformed required Skill metadata still
  fails closed with exact source evidence, and complete Context behavior remains
  unchanged for ordinary Open Forge Markdown.

### CLI-EDGE-012 — Compact findings omit known affected sources

- **Severity/category:** Material experience and agent-efficiency defect; human
  presentation.
- **Disposition:** Closed on accepted local candidate `deb3f14`. Focused compact
  Unit evidence is `3/3` for distinct Context/Find paths and Context escaping and
  bounds; published Context and Find path evidence passes. Repository dogfooding
  reports all 65 Context and 66 Find metadata findings with a source coordinate
  and zero known-source `subject=none` rows.
- **Original behavior and evidence:** Compact Context repeated only
  `context.closure-unavailable` and its cause even though each typed finding has a
  subject and path. Compact Find printed `subject=none` for
  `find.frontmatter-unavailable` even though the same typed finding retains a
  source identity and path. JSON preserved the paths, but ordinary compact output
  did not satisfy the accepted requirement to name the affected source or
  boundary when known.
- **Consequence:** Dozens of distinct findings appear identical, cannot be acted
  on without rerunning JSON or expanded output, and make correct fail-closed
  behavior look like an undifferentiated CLI failure.
- **Smallest credible correction:** Keep policy local to each command renderer and
  include one bounded escaped source coordinate selected from the finding's
  existing typed subject, source, or path evidence. Do not change finding
  formation, ordering, status, JSON, or diagnostics. Add focused compact-output
  Unit and published-process evidence for several same-code findings on distinct
  files.
- **Earliest invalidated boundary:** Context and Find compact human rendering.
- **Owning Task(s):** [Implement Context](tasks/read-only/context.md) and [Accept
  Find Presentation And The Complete
  Command](tasks/read-only/find-presentation-acceptance.md). A proposed
  pre-Mutation-Foundation dogfooding correction slice should close both local
  renderers together; final release owns any residual disposition.
- **Closure condition:** Every compact finding names one known affected source or
  boundary, distinct paths remain distinguishable, escaping and bounds remain
  exact, and structured results and semantic statuses are unchanged.

### CLI-EDGE-013 — Read-only next actions name an unavailable replacement Doctor

- **Severity/category:** Transitional integration gap; next-action truthfulness.
- **Current behavior and evidence:** Incomplete Context, Find, and Extension
  results correctly form their contracted `open-forge doctor` next action, but
  the replacement root does not yet register Doctor. The frozen CLI may expose a
  legacy Doctor under the same command spelling, but it does not diagnose the
  replacement semantic facts demonstrated by this dogfooding pass.
- **Consequence:** The public advice is contractually final but not currently
  executable against the replacement CLI, and the legacy command can return a
  reassuring result without explaining the replacement finding.
- **Smallest credible correction:** Do not add a temporary alias, legacy fallback,
  partial Doctor, or alternate next command that would drift from the accepted
  contracts. Treat the action as a known non-shipping program dependency, avoid
  blindly executing it during interim dogfooding, and verify the exact advice
  when complete Doctor lands.
- **Earliest invalidated boundary:** Cross-command composition and current
  executable command availability; individual result formation remains correct.
- **Owning Task(s):** [Implement Doctor](tasks/operations/doctor.md), followed by
  final release acceptance. [Implement Repair](tasks/operations/repair.md) owns
  only admitted exact repair actions after Doctor and must not become a temporary
  diagnostic substitute.
- **Closure condition:** The replacement executable registers complete Doctor,
  each producer's next action reaches a diagnosis that explains its unavailable
  facts with useful source-specific advice, and published managed and Native AOT
  journeys prove the cross-command path.

### CLI-EDGE-014 — The repository contains live metadata migration evidence

- **Severity/category:** Repository conformance debt and valuable Doctor
  dogfooding evidence; not a false CLI status.
- **Current behavior and evidence:** A default-universe tag Find reports 66
  archived CLI-v2 files with unavailable semantic frontmatter. Their unquoted
  `description` values contain a colon followed by a space and are malformed YAML.
  Context reports 60 of those routed archived sources plus five routed historical
  handoffs whose frontmatter still uses unsupported top-level `tags` and
  `description`. Route List remains complete because its topology facts are
  independently available. Default Extension List separately reports incomplete
  installed coverage because `.agents/open-forge.lifecycle.json` is absent; an
  available-only request remains complete, so that Extension result is expected.
- **Consequence:** Find cannot prove complete tag matching and Context cannot prove
  global `#KeepInMind` membership. Correct fail-closed results therefore keep the
  replacement CLI from returning complete startup Context on its own repository.
- **Smallest credible correction:** Preserve the exact occurrence set until its
  diagnostic value is consumed if the maintainer chooses the Doctor-dogfooding
  route. Doctor should identify each malformed or noncanonical source and provide
  accurate manual advice without inventing authored values. Then perform one
  separate reviewed repository-content migration: quote the intended archived
  descriptions and move the five handoffs to canonical `open-forge` metadata,
  followed by generated-navigation and Context/Find verification.
- **Repair boundary:** The accepted Repair contract excludes authored metadata.
  These occurrences are not eligible for `repair --automatic` without a separate
  maintainer-approved contract change. Doctor may diagnose and advise; the
  repository correction remains an explicit authored-content migration.
- **Owning Task(s):** [Implement Doctor](tasks/operations/doctor.md) owns the
  diagnosis and advice evidence. The replacement CLI program Task owns scheduling
  the later manual repository migration, and final release owns complete
  self-dogfood verification.
- **Closure condition:** Doctor reports the exact live failures with correct
  classification and useful non-fabricating advice; the subsequent reviewed
  migration removes the malformed and unsupported metadata; generated navigation
  is reconciled when needed; and the intended Context and Find requests complete
  without weakening fail-closed behavior.

### CLI-EDGE-015 — Route Inspect help describes implemented Context as planned

- **Severity/category:** Material help accuracy; cross-command public
  presentation.
- **Disposition:** Closed on accepted local candidate `deb3f14`. Unit and
  published help evidence name available
  `open-forge context [source-reference...]`, contain no unavailable Context
  wording, retain unavailable Doctor, and pass under managed and Native AOT
  EndToEnd execution.
- **Original behavior and evidence:** Route Inspect help said Context was
  unavailable or planned even though Context is implemented, accepted, and
  registered in the same replacement executable.
- **Consequence:** Help gives stale command-selection advice and understates the
  replacement CLI's current read-only capability.
- **Smallest credible correction:** Update the Route Inspect help section to name
  the available Context command using the accepted command grammar. Add focused
  help and published root-composition regression evidence; do not change Route
  Inspect or Context domain behavior.
- **Earliest invalidated boundary:** Route Inspect product help.
- **Owning Task(s):** Route Inspect presentation through the replacement CLI
  program Task. A proposed pre-Mutation-Foundation dogfooding correction slice
  should close this one-file truthfulness defect; final release owns any residual
  disposition.
- **Closure condition:** Managed and Native AOT help no longer calls implemented
  Context planned or unavailable, and the named command is registered and
  executable in the same artifact.
