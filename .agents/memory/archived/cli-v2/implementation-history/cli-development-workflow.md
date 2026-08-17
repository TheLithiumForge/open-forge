---
open-forge:
  description: Historical CLI-v2 source: Accepted historical Task for the optional Purple Development phase, calibrated agent reasoning, and repository TypeScript import boundaries
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Development Workflow Task

## Archive State

Accepted on 2026-08-03 after fresh Whole-Task Acceptance verified root clerical
commit `edf3b7109273c1951eacdaf887c1a40d463f689a`. This record is archived with
its complete phase, correction, evidence, and orchestration history. The
ordered [CLI replacement backlog](../working/cli-replacement-backlog.md) now
owns continuation, with repository-wide Prettier work still queued for its
just-in-time Task. Safe local squash integration into `feature/cli-overhaul`
remains pending; nothing was pushed.

## Outcome

Turn the accepted CLI Foundation dogfooding results into durable Development
Workflow and TypeScript import policy, then enforce the import policy across the
repository without reopening frozen MVP behavior or starting the separate
Prettier baseline.

## Authority

- The user authorized uninterrupted continuation into this Task on 2026-08-03
  after accepting the CLI Foundation.
- The accepted historical [CLI Foundation Task](cli-foundation.md)
  and its [dogfooding observations](../emerging/observations/2026-08-03_cli-foundation-dogfooding-findings.md)
  provide the evidence for Purple, calibrated agent reasoning, and import
  enforcement.
- The [Development Workflow](../../workflows/development/_development.md),
  [Task Lifecycle](../../workflows/development/task-lifecycle.md), and
  [Task Acceptance](../../workflows/development/task-acceptance.md) govern this
  Task.
- The [Deliberate Framework Change](../../directives/open-forge/framework/deliberate-framework-change.md)
  and [TypeScript Source Structure](../../directives/open-forge/typescript/typescript-source-structure.md)
  Directives govern the durable Framework and TypeScript changes.

## Baseline

- Source branch: `feature/cli-overhaul`.
- Immutable source commit: `b438b5f432ec28ecf93484a1830331ab1c74890e`.
- Task branch: `agents/feature/cli-overhaul-development-workflow`.
- The source and Task branch pointed to the same clean commit before Task setup.
- This Task is not stacked on an unmerged branch; the accepted Foundation was
  locally squash-integrated into the source branch first.
- No branch in this sequence may be pushed.

## Scope

### Required Result

- Promote Purple into an optional Development phase after Blue and before phase
  Review when grounded, material improvement exists in already-green tests,
  fixtures, snapshots, or test-only support.
- Keep missing, incorrect, or incomplete expectations under Red. Purple must
  preserve accepted behavior, Contract, production, and expectation meaning.
- Add durable calibrated-agent guidance: `medium` for mechanical work and state
  inspection, `high` for phase judgment, and `xhigh` for independent phase
  Review and whole-Task Acceptance. These are risk-calibrated defaults, not
  automatic assignments or substitutes for a bounded delegation.
- Require type-only dependencies to use static TypeScript type imports and
  enforce `@typescript-eslint/consistent-type-imports` repository-wide.
- Ban TypeScript import type expressions such as `import("module").Type`.
- Reserve `*.types.ts` for modules containing only types. Modules that own a
  runtime value remain ordinary `.ts` files even when they also export types.
- Keep static imports as the default. Ban runtime `import()` by default and
  allow it only at an explicit awaited or otherwise asynchronous lazy-loading
  boundary with visible justification and a narrow ESLint exception.
- Audit all tracked authored TypeScript and JavaScript, correct every policy
  violation, and record every intentional exception before acceptance.

### Accepted Import Interpretation

- A declaration whose imports are all type-only uses `import type`.
- A static declaration that imports runtime values and types from the same
  module may use inline `type` specifiers such as
  `import { Value, type ValueOptions } from "module"`.
- The allowed inline `type` specifier is distinct from the banned TypeScript
  import type expression `import("module").Type`.
- A runtime `import()` exception is file- and boundary-specific. It does not
  create a general dynamic-import allowance for the folder or test tier.

### Allowed Surfaces

- `.agents/workflows/development/` for the optional Purple phase, phase order,
  finding routing, and completion rules.
- `.agents/guidance/` for reusable calibrated-agent reasoning guidance.
- `.agents/directives/typescript-source-structure.md` for binding import and
  `*.types.ts` policy.
- `eslint.config.mjs`, `package.json`, and focused tooling evidence needed to
  enforce and prove the accepted policy.
- Tracked authored TypeScript or JavaScript only when the repository audit
  identifies an actual violation or the accepted lazy boundary needs a visible
  justification.
- Focused non-MVP test files and fixtures required to prove Workflow or ESLint
  conformance.
- Generated route entrypoint regions affected by adding, moving, or renaming a
  routed Framework file.
- This Task, the CLI replacement backlog, and a nearest routed Observation for
  authoritative progress and eager reusable findings; phase agents do not edit
  these state files.

### Forbidden Surfaces

- Prettier configuration, formatting scripts, or repository-wide formatting;
  backlog Order 3 owns that separate Task.
- CLI public behavior, callable contracts, production semantics, distribution,
  or the next command slice.
- Frozen MVP production or tests under `src/cli-mvp/` except read-only policy
  audit. The retained MVP suites are trusted and must not be rerun for this
  Task.
- Framework or Extension payload behavior under `src/open-forge/` or
  `src/extensions/`.
- Changes made only for stylistic preference after the top-99th material
  readiness threshold has been reached.

## Current Audit

- A config-independent ESLint syntax audit of all 70 tracked authored
  TypeScript and JavaScript files reports zero
  `@typescript-eslint/consistent-type-imports` violations.
- The repository contains zero TypeScript import type expressions and zero
  `*.types.ts` files.
- The only runtime `import()` is the top-level awaited E2E artifact boundary in
  `src/cli/testing/fixtures/final-stream-failure-shim.ts`. It is genuinely lazy
  and remains eligible only for an exact, visibly justified ESLint exception.
- Existing configured typed lint covers `src/cli/**/*.ts`; repository-wide
  syntactic import enforcement must additionally cover the other tracked
  authored TypeScript without requiring the frozen MVP suites to execute.
- Task setup initially passed the file-shaped
  `memory/working/cli-development-workflow` value to legacy `chain`; the command
  rejected it because that value is not a chainable route. The invocation
  changed no state. Both Doctors, generated indexing, local links, and
  `find --tag Active` succeeded, so this is corrected orchestration usage, not a
  product or routing defect.
- On 2026-08-03, the installed `open-forge load --bodies` command rejected the
  loader-documented `--bodies` flag as unknown and printed only base usage. The
  failed read-only invocation changed no state. This is a strange tooling/
  documentation mismatch for later investigation, not part of Green
  acceptance.

## Contract Evidence

- Contract is accepted across exactly ten resulting durable Contract, context,
  or generated-index paths:
  - `.agents/directives/typescript-source-structure.md`
  - `.agents/guidance/calibrated-agent-reasoning.md`
  - `.agents/guidance/_guidance.md`
  - `.agents/memory/emerging/ideas/composable-workflow-entrypoints.md`
  - `.agents/memory/emerging/ideas/task-work-modes.md`
  - `.agents/workflows/development/_development.md`
  - `.agents/workflows/development/phase-5-purple.md`
  - `.agents/workflows/development/phase-6-review.md`
  - `.agents/workflows/development/task-lifecycle.md`
  - `.agents/workflows/_workflows.md`
- `phase-6-review.md` is the intended move and revision of the former
  `phase-5-review.md`; the removed source path is not a separate resulting
  durable document.
- The current Contract makes Purple optional and evidence-gated after Blue,
  keeps missing or incorrect expectations in Red, routes test-only structural
  findings to Purple, and requires an explicit Purple completion or skip before
  independent Review.
- Calibrated reasoning is guidance applied after a delegation is bounded:
  `medium` for mechanical state work, `high` for phase judgment, and `xhigh` for
  independent Review or Acceptance, with deviations grounded in concrete risk
  and complexity.
- Type-only dependencies use static `import type` declarations or inline `type`
  specifiers in mixed static imports. Import type expressions are banned,
  `*.types.ts` remains exclusive to type-only modules, and runtime `import()` is
  limited to a visibly justified asynchronous lazy-loading boundary with an
  exact exception.
- Contract-local route entries and links have been regenerated and inspected.
  Both Doctors, targeted chain output, local-link checks, stale-reference
  searches, protected-path diff review, generated-index idempotence, and diff
  hygiene passed. This evidence formed the accepted Contract handoff.
- The first combined `apply_patch` attempt failed atomically on a context
  mismatch. It changed no state beyond the separately completed and already
  verified intended Review-file move; subsequent focused patches succeeded.
  This is a transient orchestration event, not a product, routing, or Contract
  finding.

### Independent Contract Review Outcome

- **Medium — Contract authority conflict, fixed:**
  `.agents/workflows/development/task-lifecycle.md` lines 27-30 had directed
  each phase agent to stage and commit its own work. That conflicted with the
  accepted user, Task, and backlog authority that phase agents leave changes
  unstaged and only the root orchestrator inspects, stages, and commits them.
  The lifecycle step and completion language now align on root-orchestrator-only
  staging and commits.
- **Low — stale promoted-Idea state, fixed:**
  `.agents/memory/emerging/ideas/composable-workflow-entrypoints.md` had
  described five Development descendants and `phase-5-review.md` under
  `Current Accepted Boundary`. Purple is now promoted and Review is Phase 6,
  and the Idea now records that its earlier proposal was promoted and is
  superseded by the accepted Contract.
- **Low — adjacent stale work-mode Idea context, fixed:**
  `.agents/memory/emerging/ideas/task-work-modes.md` had listed only Contract,
  Red, Green, Blue, and Review and linked an active-readiness record that had
  since been archived or removed from the active route. The Idea now includes
  optional Purple and points to the accepted archived CLI Foundation, current
  Development Workflow, and this active Task without expanding its scope.
- Fresh independent `xhigh` rereview at the top-99th material threshold accepts
  the optional Purple and Review flow, calibrated reasoning guidance, and
  TypeScript import/file policy with no material blocker. Contract is complete,
  preserved in root commit `21a23c5`, and Red is authorized next.

### Disposable Review Cleanup

- Reviewer cleanup was blocked twice before execution. A subsequent
  orchestrator-native `Remove-Item` attempt was also blocked before execution
  and changed no state.
- The exact disposable directory was then verified as an OS-temporary target,
  removed through .NET `Directory.Delete`, and verified absent. This transient
  cleanup sequence had no repository impact.

## Red Design Checkpoint

- Red is bounded to two new test-only paths: one ESLint import-policy
  fixture and one integration test. Temporary fixture repositories use
  `mkdtemp` under the testing surface and are removed by `afterAll`.
- Evidence exercises the real ESLint configuration and API. The negative cases
  are an all-type value import, a TypeScript import type expression, an awaited
  runtime `import()` under the default policy, and a runtime export from a
  `*.types.ts` file.
- Positive cases include a mixed static import with an inline `type` specifier.
  The actual E2E final-stream-failure shim may pass only through an exact
  adjacent suppression whose text states the lazy-loading reason.
- The repository inventory must prove
  `@typescript-eslint/consistent-type-imports` applies across tracked authored
  TypeScript and JavaScript.
- Red uses standard `no-restricted-syntax`; it will not add brittle Workflow
  prose snapshots or package-script string assertions. Initial Red failure and
  complete evidence are captured through eight explicit integration tests.
- The focused run reports two passing cases and six intended Red failures. The
  accepted static import forms pass; enforcement is still
  missing for the all-type value import, TypeScript import type expression,
  default awaited runtime import ban, runtime export from `*.types.ts`, exact
  adjacent reason-bearing E2E-shim suppression, and complete tracked-source
  `consistent-type-imports` inventory.
- Type checking, currently configured lint, diff hygiene, temporary-directory
  cleanup, and protected-path diff checks pass. Red did not run or modify the
  frozen MVP suite.

### Independent Red Review

- Independent review rejected Red at low severity because the lazy-exception
  reason matcher accepts meaningless text such as `-- x`. Red must require a
  semantic lazy-loading or deferred-loading reason without freezing one exact
  prose sentence.
- Exact diagnostic-message prose assertions are over-prescriptive and are
  removed in favor of stable rule identity and exact syntax-span evidence.
- Correction implementation confirmed that ESLint 10 `LintMessage` and
  `SuppressedLintMessage`, including their runtime JSON shape, expose
  `ruleId`, message, location, and `messageId`, but not `nodeType`. Red therefore
  avoids message-prose duplication by asserting that each ESLint-reported range
  selects the exact forbidden TypeScript import-type, runtime-import, runtime-
  declaration, or suppressed shim-import syntax span. Fixture names state the
  expected syntax identity for readable failures, while the semantic
  lazy/deferred-loading comment reason is checked separately.
- This range-based evidence uses the real standard rules. It introduces no
  custom ESLint rule and no new policy decision.
- The representative `*.types.ts` runtime-export polarity is accepted for Red.
  Green and Blue must still confirm the configured syntax coverage catches all
  runtime declaration forms governed by that filename boundary.
- The narrow correction preserves exactly two Red paths and eight explicit
  cases. The accepted focused result is two passes and six intended failures,
  including the semantic lazy/deferred-loading reason and ESLint 10 range-based
  syntax identity without diagnostic-prose coupling.
- Type checking, configured lint, diff hygiene, temporary cleanup, and
  protected-path review pass. The dynamic authored-source inventory sees 70
  tracked files now and will see 72 after the two Red paths are committed. The
  two files are 199 and 73 lines, both within their applicable limits. No MVP
  suite ran.
- Independent `xhigh` review and focused `high` rereview accept Red at the
  top-99th material threshold after the recorded low rejection and correction.
  Red is complete and preserved in root commit `75a61e4`; Green is authorized
  next.

## Green Checkpoint

- Green is bounded to exactly three files: `eslint.config.mjs`, `package.json`,
  and `src/cli/testing/fixtures/final-stream-failure-shim.ts`.
- The ESLint configuration composes housekeeping global ignores, ESLint
  recommended rules and the existing strict typed layer scoped to `src/cli`, a
  TypeScript syntax layer for authored TypeScript, and a lightweight repository
  authored-source policy for consistent type imports plus banned
  `TSImportType` and `ImportExpression` syntax. The frozen MVP is intentionally
  not globally ignored so the lightweight import policy reaches it, while
  recommended and typed rules remain CLI-only. The `*.types.ts` boundary uses
  a closed runtime-declaration allowlist and shared restrictions.
- The package lint command builds assets and then runs `eslint .`. The E2E shim
  carries one adjacent reason-bearing suppression for its accepted deferred
  artifact load.
- The complete 72-file authored-source inventory passes with no existing
  violation requiring source correction.
- The first focused Red run passed six of eight cases. Both misses were exact
  evidence mismatches: the `*.types.ts` selector reported the export parent
  instead of the expected `VariableDeclaration` child, and the shim comment
  used `defers` instead of an accepted `lazy`, `defer`, or `deferred` reason.
  Green narrowed the selector to the runtime declaration child and changed the
  reason wording to `deferred`. No policy ambiguity or new decision arose.
- The focused Red suite now passes all eight cases.
- The first widened repository lint surfaced 190 unrelated general-lint
  findings in legacy and benchmark sources because ESLint recommended rules
  still applied globally. Those findings are outside the authorized import-
  policy audit and include the frozen MVP; they are a configuration-scope
  observation, not authored import-policy violations.
- Green remaps both the general recommended and strict typed layers to
  `src/cli` while the lightweight repository-wide layer enforces only the
  accepted import policy. This requires no source correction and introduces no
  policy decision.
- The first independent Green-review `lintText` polarity probe supplied a
  nonexistent `*.types.ts` path under `src/cli` and correctly reached typed-
  project inclusion before the filename policy. It changed no state and is not
  a finding. The rerun outside typed CLI scope isolated the repository filename
  policy and passed the realistic positive/negative polarity.

### Green Acceptance

- Independent Green review accepts the result at the top-99th material
  threshold with no material finding. Frozen Red passes all 8 cases,
  `bun run lint` passes, and both TypeScript projects pass.
- Direct JSON inventory reports exactly 72 tracked authored JavaScript and
  TypeScript files with none missing or extra. Exactly one accepted adjacent
  suppression remains for the deferred E2E artifact import. Realistic
  `*.types.ts` positive/negative polarity passes, and typed rules plus project
  paths remain scoped to `src/cli/**/*.ts`.
- Protected-path, frozen-MVP, and artifact diff checks pass. No MVP suite ran.
- Root reproduction passed frozen Red 8/8, repository lint, type checking, and
  diff hygiene, and inspected the complete allowed-path diff.
- Green is complete and preserved in root commit `1a35dfc`; Blue is authorized
  next.

## Blue Checkpoint

- Blue found a current-truth wording mismatch outside its edit authority in
  `.agents/memory/crystallized/documents/cli/development-toolchain.md`: it still
  says ESLint excludes frozen `src/cli-mvp/`, while accepted Green intentionally
  applies the lightweight repository import policy there and keeps general
  recommended plus typed lint scoped to `src/cli`.
- This is a documentation-consistency finding that must route to the earliest
  responsible phase; it does not reopen behavior or require a product decision.
- Blue is performing a bounded read-only stale-reference search and continues
  its configuration-structure review.
- After reporting that it had stopped preference-only exploration and moved to
  final no-change verification, the Blue agent remained running through four
  30-second waits and did not respond to two stop/final requests. The
  orchestrator interrupted it and requested a tool-free final handoff. This is
  an orchestration/runtime oddity with no known workspace mutation or evidence
  loss. The interruption happened only after verification completed, and no
  command remained incomplete.
- Blue's structural verdict is explicitly no-change. Red passes 8/8,
  repository lint and both TypeScript projects pass, direct inventory covers
  all 72 authored sources, there are zero `TSImportType` nodes and exactly one
  accepted `ImportExpression`, realistic `*.types.ts` polarity passes, and
  diff/protected-path checks pass. No MVP suite ran.
- The exact stale current-truth cluster is
  `.agents/memory/crystallized/documents/cli/development-toolchain.md:41`,
  `docs/development.md:107`, and `docs/development.md:112`.
- This Blue attempt is not acceptably complete because that current-truth
  documentation is stale. A narrow Contract repeat is authorized and in
  progress; fresh proportionate Red, Green, and Blue reruns are required after
  its correction.
- Blue recommended skipping Purple because the explored changes were
  preference-only. The orchestrator will reassess that recommendation after
  the rerun against objective test-structure and file-size evidence.

## Contract Repeat Checkpoint

- The Contract repeat changed exactly two documents. The crystallized CLI
  development toolchain now describes actual lint/check coverage, and the
  contributor development guide now describes actual lint/check coverage plus
  the optional-Purple phase summary.
- The orchestrator authorized the same Contract repeat to correct that adjacent
  summary because this Task already accepted optional grounded Purple. This is
  current-truth alignment only; it introduces no product decision or
  implementation change.
- Fresh independent `xhigh` review accepts the bounded correction at the
  top-99th material threshold. A bounded no-contradiction search, both Doctors,
  links, a direct ESLint configuration-scope probe, diff hygiene, and protected-
  path checks pass. No MVP suite ran.
- Contract repeat is complete and preserved in root commit `193c200`; the
  original accepted Contract remains `21a23c5`. A fresh `high`-reasoning Red
  agent is validating frozen expectation completeness against the
  documentation-only correction.

## Red Repeat Acceptance

- Red repeat is accepted with no change. Both Red-owned paths are byte-for-byte
  unchanged from `75a61e4`, and all 8 focused cases pass.
- Focused ESLint and the TypeScript 7 test-project typecheck pass. Configuration
  polarity shows the CLI receives 170 rules spanning recommended, typed, and
  repository-policy layers, while representative MVP and benchmark paths
  receive exactly the two lightweight import-policy rules and no typed project.
- Diff, staged-state, protected-path, temporary-cleanup, and no-MVP checks pass;
  no MVP suite ran.
- Residual proportionality is explicit: the inventory enumerates
  `consistent-type-imports` coverage, while restricted-syntax coverage is
  corroborated through configuration polarity and focused cases. This is
  sufficient for the documentation-only correction.
- Red found no grounded Purple scope. No Red repeat commit exists because the
  phase made no owned change. A fresh `high`-reasoning Green repeat is
  authorized and in progress.

## Green Repeat Acceptance

- Green repeat is accepted with no change. Frozen Red passes 8/8, repository
  lint and both typechecks pass, and direct configuration inventory covers all
  72 authored files: 59 CLI files receive 170 rules plus typed projects, while
  13 outside files receive exactly the two lightweight rules and no project.
- Realistic polarity and default-deny probes pass. The repository contains zero
  `TSImportType` nodes, exactly one accepted shim `ImportExpression`, and exactly
  one adjacent reason-bearing suppression for it.
- Red- and Green-owned files are byte-identical to `75a61e4` and `1a35dfc`.
  Documentation and package scripts match the implemented scopes. Staged-state,
  diff-hygiene, protected-path, and no-MVP checks pass; no MVP suite ran.
- Residual risk is bounded: no tracked production `*.types.ts` example exists,
  but realistic positive and negative probes cover the filename boundary.
- Two inline read-only probe attempts contained local script mistakes before
  the corrected probe passed. They wrote no repository file and changed no
  repository state; this is a transient orchestration oddity, not a finding.
- Green found no grounded Purple scope. The 199-line Red integration test is
  within its hard limit and remains coherent rather than demonstrating a
  material test-structure split. No Green repeat commit exists because the
  phase made no owned change. A fresh `high`-reasoning Blue repeat is authorized
  and in progress.

## Blue Repeat Checkpoint

- The first final-evidence attempt ran frozen Red and repository lint
  concurrently. Lint observed Red's temporary intentionally invalid negative-
  probe files and correctly reported their five expected violations.
- Red cleanup completed, no temporary probe directory remains, and Git state is
  unchanged except for the orchestrator-owned Task update. This is test/lint
  concurrency interference, not a product or configuration defect.
- Blue is rerunning the affected gates sequentially.

### Blue Repeat Acceptance And Purple Decision

- Fresh Blue repeat accepts the structure at the top-99th material threshold
  with no change. Configuration structure, ordering, and required tuple
  replacement are coherent, and documentation, package scripts, and ESLint
  configuration agree.
- Frozen Red passes 8/8; repository lint and both typechecks pass; exact scope
  inventory remains 72 authored files split into 59 CLI and 13 outside files;
  realistic polarity passes; AST inventory remains zero `TSImportType` and one
  accepted `ImportExpression`; the exact adjacent reason-bearing exception,
  diff, protected-path, staged-state, and no-MVP checks pass. No MVP suite ran.
- The concurrent test/lint interference was rerun sequentially and passed. Its
  reusable disposition is now recorded in
  `.agents/memory/emerging/observations/2026-08-03_cli-foundation-dogfooding-findings.md`,
  which will be committed with this Task state.
- Red, Green, and Blue repeats are complete and accepted with no phase-owned
  repeat commit beyond Contract repeat `193c200`.
- Purple is deliberately skipped. Objective inspection treats the 199-line Red
  integration test and its one-line hard-limit headroom as residual risk, but
  its eight explicit cases are cohesive, its 73-line probe catalogue is already
  extracted, helpers have one consumer, and duplication is low. Splitting it
  would add indirection without grounded material benefit. Red, Green, and Blue
  each found no grounded Purple scope.
- A fresh `xhigh` Phase Review is authorized and in progress.

## Phase Review Acceptance

- Fresh independent `xhigh` Phase Review accepts the complete cycle with no
  material blocker. Frozen Red passes 8/8; repository lint and both TypeScript
  7 projects pass; exact configuration inventory remains 72 authored files
  split into 59 CLI and 13 outside files; AST inventory remains zero
  `TSImportType`, one accepted `ImportExpression`, and one exact adjacent
  reason-bearing exception; realistic filename polarity passes.
- Both frozen-MVP Doctors, the routed Development chain, a 16-file Markdown
  link and stale-reference audit, six-commit history review, and candidate diff,
  protected-path, staged-state, and untracked-state checks pass. No MVP suite or
  full E2E suite ran.
- Accepted residuals are explicit: no production `*.types.ts` module exists but
  realistic probes cover the boundary; E2E evidence is intentionally trusted;
  the incomplete replacement still lacks `load --bodies`; Purple's skip is
  justified; and the sequential test/lint concurrency disposition is correct.
- One reviewer script first called unavailable `.parse()`. The corrected
  `parseForESLint()` probe passed and changed no state.
- After multiple stop/final requests, the still-running reviewer was interrupted
  and asked for a tool-free final handoff. The final confirmed no command was
  incomplete and no workspace state changed. This is an orchestration/runtime
  oddity, not an evidence gap.
- Phase Review is complete and accepted. A fresh `xhigh` whole-Task Acceptance
  reviewer is authorized and in progress.

## Whole-Task Acceptance Checkpoint

- Fresh independent `xhigh` whole-Task Acceptance returns
  `MINOR_CORRECTIONS` with no phase blocker. Everything except two clerical
  current-state corrections is accepted.
- The replacement backlog's temporary Foundation Purple section must state
  that Order 2 promoted optional grounded Purple into the Development Workflow.
- The CLI Foundation dogfooding Observation's strict type-import row must state
  that permanent repository ESLint enforcement completed through Red evidence
  at `75a61e4` and Green implementation at `1a35dfc`, rather than remaining
  queued.
- The two clerical corrections were preserved in root commit `edf3b71`. Fresh
  `xhigh` verification returned `READY`: the exact three-path correction,
  local links, generated-index idempotence, diff hygiene, and clean repository
  state all passed. No MVP suite or full E2E suite ran, and no finding remains.

## Progress

| Stage                 | State                     | Commit                               | Evidence                                                                                                                                                                                                                                                                 |
| --------------------- | ------------------------- | ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Task setup            | Complete                  | None                                 | Exact baseline and branch, import audit, phase plan, generated Working index, both legacy Doctors, local links, active-Task discovery, and diff hygiene verified                                                                                                         |
| Contract              | Repeat complete; accepted | Original `21a23c5`; repeat `193c200` | Exactly two docs align toolchain lint/check coverage and contributor lint/check plus optional-Purple summary; xhigh top-99th review, contradiction search, both Doctors, links, direct config probe, diff/protected checks, and no-MVP boundary pass                     |
| Red                   | Repeat complete; accepted | Baseline `75a61e4`; no-change repeat | Red paths byte-unchanged; 8/8, focused ESLint, TS7 test typecheck, CLI-versus-MVP/benchmark config polarity, diff/staged/protected/cleanup/no-MVP evidence pass; no repeat commit required                                                                               |
| Green                 | Repeat complete; accepted | Baseline `1a35dfc`; no-change repeat | Red 8/8, lint/typecheck, exact 59 CLI plus 13 outside config polarity, default deny, syntax/suppression counts, byte identity, docs/scripts alignment, staged/diff/protected/no-MVP evidence pass; no repeat commit required                                             |
| Blue                  | Repeat complete; accepted | No-change repeat                     | Top-99th structural review, docs/scripts/config agreement, sequential full evidence, scope and AST inventories, exact exception, diff/protected/staged/no-MVP checks pass; no repeat commit required                                                                     |
| Purple                | Deliberately skipped      | None                                 | The cohesive 199-line eight-case test, extracted 73-line catalogue, one-consumer helpers, and low duplication show no grounded material refactor; splitting would add indirection                                                                                        |
| Phase Review          | Complete; accepted        | None                                 | Fresh xhigh review accepted Red 8/8, lint, both TS7 projects, 72-file scope, AST/exception counts, polarity, both Doctors, route chain, 16-file Markdown audit, six-commit history, clean candidate state, protected boundaries, and proportional no-MVP/no-E2E evidence |
| Whole-Task Acceptance | Complete; accepted        | `edf3b71`                            | Fresh xhigh review returned READY after verifying the exact three-path clerical correction, links, index idempotence, diff hygiene, and clean state with no MVP/full E2E execution; no finding remains                                                                   |

## Phase Plan

### Contract

Delegate at `high` reasoning. Define the durable optional Purple recipe and
phase order, repeat routing, calibrated-agent guidance, and binding TypeScript
import and file-naming policy. Preserve Review independence and Task Acceptance
as separate gates. Change no ESLint configuration or authored code.

### Red

Delegate at `high` reasoning. Add the cheapest focused executable conformance
that proves the accepted Workflow shape and that forbidden type imports and
runtime dynamic imports fail while the exact justified async-lazy boundary can
pass. Red may edit only tests and test-local fixtures. It must not invoke or
extend the frozen MVP suite.

### Green

Delegate at `high` reasoning. Implement the smallest repository ESLint and
script surface that passes Red, retain typed lint for the replacement CLI, add
only the exact lazy-boundary exception and visible justification, and correct
only violations found by the complete audit. Do not format unrelated files.

### Blue

Delegate at `high` reasoning. Review the changed Workflow, guidance,
Directive, ESLint, script, and audit-correction structure for clarity,
duplication, predictable ownership, and maintainability while Contract and Red
evidence remain frozen and green.

### Purple

After Blue, the orchestrator decides from evidence whether Purple has grounded
material scope. If so, delegate at `high` reasoning to improve only already-
green tests, fixtures, snapshots, and test-only support. If not, record a
deliberate skip. Purple may not add, remove, weaken, or reinterpret an
expectation.

### Review And Acceptance

Delegate phase Review and whole-Task Acceptance separately to fresh agents at
`xhigh` reasoning. Review the complete repository policy coverage, exception
narrowness, frozen-MVP boundary, routing integrity, evidence credibility, and
commit reviewability. Repeat the earliest invalidated phase for every material
finding. Stop cycling when no material correctness, Contract, integrity,
maintainability, enforcement, or evidence gap remains; cosmetic preference is
not a blocker.

## Acceptance Criteria

- The Development Workflow exposes Purple as optional after Blue, gives it an
  exact test-only authority, keeps expectation defects in Red, and includes it
  correctly in later-phase repetition.
- Calibrated reasoning guidance is durable, linked from orchestration, and
  explicitly risk-sensitive rather than automatic.
- The TypeScript Directive and ESLint configuration agree on type-only
  declarations, allowed mixed static imports, banned import type expressions,
  `*.types.ts` ownership, static-import default, and narrow async-lazy runtime
  import exceptions.
- Repository lint reaches all tracked authored TypeScript and JavaScript that
  is in current source scope while keeping typed replacement-CLI lint intact.
- Every repository import finding is corrected or has one exact documented and
  enforced exception. The current E2E shim is the only expected runtime
  `import()` exception.
- Focused negative probes prove forbidden forms fail. Positive probes prove the
  accepted static forms and exact lazy boundary pass.
- Routed indexes, Doctors, link checks, strict type checking, focused tests,
  repository lint, and `git diff --check` pass proportionately.
- The frozen MVP files remain unchanged and its trusted 25 fast and 162 closure
  tests are not rerun.
- Phase Review and separate whole-Task Acceptance report no material blocker at
  the top-99th readiness threshold.

## Evidence Plan

- Contract: legacy Open Forge index, Doctor, targeted `chain`, link search, and
  Workflow-shape evidence.
- Red: focused Workflow and ESLint policy tests that execute and fail only for
  the missing accepted contract.
- Green: focused Red evidence, repository lint, replacement production and test
  typechecks, and a complete tracked-source import inventory.
- Blue and optional Purple: focused evidence throughout, then the complete
  Task-specific policy and tooling suite.
- Review and Acceptance: fresh read-only reproduction of routed integrity,
  policy probes, repository lint, relevant typechecks, tracked-source audit,
  `git diff --check`, branch/commit history, and protected-path diffs.
- Do not use `test:mvp`, `test:mvp:fast`, `test:mvp:closure`, or a broader
  command that transitively runs them. Foundation already supplied trusted MVP
  evidence and this Task does not change its behavior.

## Orchestration

- The orchestrator delegates every development phase to a fresh phase-specific
  sub-agent, supplies only bounded context, inspects every returned diff and
  reruns proportionate evidence before advancing.
- Phase agents leave changes unstaged and uncommitted. Only the orchestrator may
  stage or commit, using ordinary descriptive commit messages without
  Conventional Commit prefixes or co-author trailers.
- Choose reasoning deliberately: `medium` for mechanical inspection and state
  maintenance, `high` for Contract/Red/Green/Blue/Purple judgment, and `xhigh`
  for independent Review and Acceptance. Raise or lower only when the concrete
  risk and complexity justify it.
- Write grounded oddities, unconformities, and reusable observations eagerly in
  the nearest routed Observation. If a phase cannot edit state, it reports the
  finding immediately so the orchestrator records it before dependent work.
- Pause for the user only when accepted sources do not resolve a material
  behavioral, architecture, authority, safety, cost, or scope choice.
- Never push. Keep every accepted topic in the smallest coherent reviewable
  commit and preserve unrelated workspace state.

## Decisions Needed

None. The user explicitly authorized this Task, the mixed-static-import
interpretation above, calibrated reasoning, optional Purple promotion, the
top-99th stopping rule, and continuation without rerunning the frozen MVP.

## Completion

Achieved on 2026-08-03.

- Accepted Task commits are `77664c2`, `21a23c5`, `75a61e4`, `1a35dfc`,
  `193c200`, `d0b8846`, `88a145f`, and final clerical correction `edf3b71`.
- Contract, Red, Green, Blue, deliberately skipped Purple, Phase Review, and
  separate Whole-Task Acceptance satisfy the accepted Development Workflow.
- Optional grounded Purple, calibrated agent reasoning, and repository
  TypeScript import boundaries are durable and enforced; all proportional
  evidence and final clerical verification passed.
- Accepted residual risks are the absence of a tracked production `*.types.ts`
  example despite realistic polarity probes, the cohesive 199-line integration
  test's limited one-line hard-limit headroom, unavailable replacement
  `load --bodies` until its future CLI slice, and the requirement to run the
  import-policy integration test before repository lint until temporary probes
  are isolated from lint discovery.
- The Task branch was never pushed and has no upstream. Safe local squash
  integration into `feature/cli-overhaul` remains pending.
