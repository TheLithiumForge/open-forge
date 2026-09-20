---
open-forge:
  description: "Historical CLI-v2 source: Accepted historical replacement CLI Foundation Task with preserved phase, commit, evidence, and acceptance history"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Foundation Task

## Archive State

Accepted on 2026-08-03 by Whole-Task Acceptance repeat 2 at clean branch state
`5f1cf44e60b1d182a574ab7d46ae96fa0bc58b40`. This record is archived with its
complete audit history. The accepted Foundation is replaced as active working
state by the ordered
[CLI replacement backlog](../working/cli-replacement-backlog.md) and the next
just-in-time Task created from the locally integrated baseline.

## Outcome

Deliver the strict, buildable, portable replacement CLI foundation that later
commands can extend without redefining the parser, result, presentation,
toolchain, test, or embedded-asset boundaries.

## Authority

- The user authorized the replacement implementation sequence on 2026-08-03
  and required all development phases and integrity review to be delegated.
- The [CLI scope](../crystallized/documents/cli/_cli.md) routes to accepted
  architecture, interface, contracts, toolchain, and frozen MVP behavior.
- The [Development Workflow](../../workflows/development/_development.md)
  governs phase ownership, commits, review repetition, and Task acceptance.
- The [TypeScript Source Structure](../../directives/open-forge/typescript/typescript-source-structure.md)
  and [CLI Interface Consistency](../../directives/open-forge/cli/cli-interface-consistency.md)
  Directives remain binding.

## Baseline

- Source branch: `feature/cli-overhaul`.
- Immutable source commit: `6d87744660c287188def80319400295552fa9f17`.
- Task branch: `agents/feature/cli-overhaul-cli-foundation`.
- The branch was clean before Task creation.
- `bun run test:ci` passed at the Task baseline with 25 fast tests and 162
  closure tests in 95.4 seconds.
- Available runtimes are Bun 1.3.14, Node.js 26.3.1, and Deno 2.9.4.

## Scope

### Required Result

- Install and lock the accepted replacement dependencies and development tools.
- Add strict production and test TypeScript projects and typed read-only ESLint.
- Establish the accepted direct, integration, end-to-end, snapshot-update, and
  complete check script surface while retaining an explicit frozen-MVP path.
- Generate and inject deterministic build identity and embedded Framework and
  first-party Extension assets.
- Add `src/cli/cli.ts`, `src/cli/main.ts`, Commander composition, parser-level
  help, version, parse, and unexpected-failure results.
- Add shared named result values, human and JSON display selection, final
  stream writing, and semantic status-to-exit behavior.
- Produce one self-contained `dist/cli.mjs` artifact that runs through the
  accepted Node.js, Bun, and Deno invocation boundaries.

### Allowed Surfaces

- `package.json` and the dependency lockfile.
- `build.ts`, TypeScript projects, ESLint configuration, and focused repository
  scripts required by the accepted toolchain.
- Replacement source and colocated evidence under `src/cli/`.
- Focused replacement test-runner support under `tests/` when CLI-local
  placement cannot express the repository script boundary.
- Exact current documentation whose toolchain authority transfers to source.

### Forbidden Surfaces

- Frozen production or test files under `src/cli-mvp/`.
- Domain behavior for any of the nineteen public leaves.
- Framework or Extension payload behavior under `src/open-forge/` or
  `src/extensions/`.
- Legacy dispatch, aliases, fallback, or translation inside the replacement.
- Task state, backlog state, or another phase's owned files from a phase agent.

## Progress

| Stage                          | State                                                       | Commit                       | Evidence                                                                                                                                                                 |
| ------------------------------ | ----------------------------------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Task setup                     | Complete                                                    | `4e30d1e`                    | Branch, backlog, Task record, archived readiness, baseline suite, and Doctor established                                                                                 |
| Contract                       | Complete                                                    | `498592d`                    | Strict typecheck, typed lint, build, Node syntax, Deno check, and frozen MVP fast evidence passed                                                                        |
| Red cycle 1                    | Rejected                                                    | `d3950c7`                    | Review found missing final-write, parser token/alias/value, suggestion, and minimum-runtime evidence                                                                     |
| Green cycle 1                  | Superseded                                                  | `f74f056`                    | Passed incomplete Red evidence; repeat required after corrected Red                                                                                                      |
| Blue cycle 1                   | Superseded                                                  | `8f52cfe`                    | Structurally valid and byte-identical, but repeat required by phase ordering                                                                                             |
| Phase Review cycle 1           | Rejected from Red                                           | None                         | Confirmed two high and two medium evidence/behavior findings; no Contract or Blue blocker                                                                                |
| Red repeat 1                   | Complete                                                    | `079eb93`                    | Added final-write, raw-token/parser-policy, bounded-suggestion, and exact Node/Bun/Deno minimum-runtime evidence                                                         |
| Green repeat 1                 | Superseded                                                  | `0da8b05`                    | Passed incomplete Red repeat 1; second correction required                                                                                                               |
| Blue repeat 1                  | Superseded no-op                                            | `0da8b05`                    | Structurally valid, but repeat required by phase ordering                                                                                                                |
| Phase Review repeat 1          | Rejected from Red                                           | None                         | Found canonical single-dash and separate-empty-value gaps plus incomplete async write-failure evidence                                                                   |
| Red repeat 2                   | Complete                                                    | `4b6872c`                    | Completed single-dash, empty-value, async writer, listener-cleanup, and filterable runtime-tag evidence                                                                  |
| Green repeat 2                 | Complete                                                    | `8887f01`                    | Corrected exact single-dash classification and separate-empty workspace values across all frozen evidence                                                                |
| Blue repeat 2                  | Complete                                                    | `6d7d6e4`                    | Extracted root parse policy into a focused module with unchanged behavior and complete evidence                                                                          |
| Phase Review repeat 2          | Rejected from Red                                           | None                         | Confirmed combined stream-failure escape, exact-token self-suggestions, locale-dependent packaging order, and raw direct-test control values                             |
| Contract repeat 1              | Complete                                                    | `a447c0f`                    | Accepted guarded final-write, union-typed final-stream, fixture-scope, and one-artifact distribution boundaries; deferred debug and additional channels as Ideas         |
| Red repeat 3                   | Complete                                                    | `65e5fd8`                    | Added exact-token suggestion, guarded paired stream-failure, minimal distribution, union-output, shared-fixture, and filterable runtime evidence                         |
| Green repeat 3                 | Complete                                                    | `aa5a8b2`                    | Guarded final writes, truthful suggestions, and the minimal embedded distribution pass all frozen evidence                                                               |
| Blue repeat 3                  | Complete, deliberate no-op                                  | `aa5a8b2`                    | Corrected Green is already focused; no phase code commit was warranted                                                                                                   |
| Phase Review repeat 3          | Rejected from Red                                           | None                         | Found an inexact npm inventory assertion and duplicated shared test values; no review commit was created                                                                 |
| Red repeat 4                   | Complete                                                    | `0650754`                    | Proves one exact seven-path npm inventory and centralizes repeated semantic and arbitrary test values at their common support scopes                                     |
| Green repeat 4                 | Complete, deliberate no-op                                  | `80304bc`                    | Frozen Red repeat 4 passes against the unchanged corrected production at `aa5a8b2`; no phase code commit was warranted                                                   |
| Blue repeat 4                  | Complete, deliberate no-op                                  | `ba14d6f`                    | Corrected production at `aa5a8b2` is already focused; no phase code commit was warranted                                                                                 |
| Phase Review repeat 4          | Accepted                                                    | None                         | No blocking, optional, or new finding remains; independent complete evidence passed and no phase code commit was created                                                 |
| Whole-Task Acceptance          | Rejected from Contract/current-truth alignment              | None                         | Medium blocking stale current documentation contradicts the implemented, buildable Foundation candidate; no acceptance commit was created                                |
| Contract repeat 2              | Complete                                                    | `a126b0b`                    | Aligned seven current documents with the implemented, buildable Foundation candidate without claiming complete replacement delivery                                      |
| Red repeat 5                   | Complete                                                    | `365e80e`                    | Added one explicit integration test that runs the real build entrypoint from an isolated empty working directory and rejects only named stale guidance                   |
| Green repeat 5                 | Complete                                                    | `cd5e6fb`                    | Corrected the stale missing-source guidance with one focused production-line change and passed the complete repeated evidence surface                                    |
| Blue repeat 5                  | Complete, deliberate no-op                                  | `632c2ab`                    | Corrected production at `cd5e6fb` remains cohesive; no phase code commit was warranted                                                                                   |
| Phase Review repeat 5          | Rejected from Contract                                      | None                         | One low blocking transition-residue category remains in development-toolchain metadata and three linked Decision statements; no review commit was created                |
| Contract repeat 3              | Complete                                                    | `c4b1e61`                    | Aligned three current authority surfaces across the four remaining transition-residue occurrences without changing the accepted toolchain direction                      |
| Red repeat 6                   | Complete, deliberate no-op; chain invalidated from Contract | `abf93bf`                    | Existing evidence passed unchanged, but the authority audit found one residual current architecture sentence; no Red change was warranted and Green repeat 6 was stopped |
| Green repeat 6                 | Superseded before start                                     | None                         | Did not advance because Red repeat 6 returned the chain to Contract                                                                                                      |
| Blue repeat 6                  | Superseded before start                                     | None                         | Did not advance because the repeated chain returned to Contract before Green                                                                                             |
| Phase Review repeat 6          | Superseded before start                                     | None                         | Did not advance because the repeated chain returned to Contract                                                                                                          |
| Contract repeat 4              | Complete                                                    | `3e04529`                    | Aligned the one residual current architecture passage with the accepted source-owned executable-value boundary; no new decision or expectation was introduced            |
| Red repeat 7                   | Complete, deliberate no-op                                  | `a5db8b2`                    | Fresh evidence and authority audits passed unchanged with no gap or residue; no phase code commit was warranted                                                          |
| Green repeat 7                 | Complete, deliberate no-op                                  | `e770f1a`                    | Corrected production remains complete and focused against unchanged frozen evidence; no production, test, documentation, or phase code change was warranted              |
| Blue repeat 7                  | Complete                                                    | `1337e92`                    | Removed the sole runtime ESM dependency cycle across five focused parser production paths while preserving the existing parser-results surface and complete behavior     |
| Phase Review repeat 7          | Superseded before start                                     | None                         | Did not advance because Blue repeat 7 returned the chain to Contract and Red before fresh Review                                                                         |
| Contract repeat 5              | Complete                                                    | `6448109`                    | Moved the unchanged `Blocked mutation` and `Attention` rows into their existing Help And Error Behavior table without changing semantics                                 |
| Red repeat 8                   | Complete, deliberate no-op                                  | `8ff5813`                    | Fresh evidence passed unchanged after the semantic-neutral Contract correction; no expectation or test mutation was warranted                                            |
| Green repeat 8                 | Complete, deliberate no-op                                  | `f58c936`                    | Production at `1337e92` remains the smallest correct implementation against the repeated Contract and unchanged frozen Red evidence                                      |
| Blue repeat 8                  | Complete, deliberate no-op                                  | `b573512`                    | Production at `1337e92` remains structurally focused with zero dependency cycles or missing type-only imports; no production change was warranted                        |
| Purple repeat 1                | Complete                                                    | `1fc7047`                    | Centralized the duplicated Bun subprocess and failure support in one test-only helper without changing behavior, Contract, production, or expectations                   |
| Phase Review repeat 8          | Accepted                                                    | `728c511`                    | Independent Contract-through-Purple review found no blocking, residual, optional, or phase-invalidating finding                                                          |
| Whole-Task Acceptance repeat   | Rejected from Contract                                      | `1281ef2`                    | Broader whole-Task scan found three blocking ownership and duplication categories; the Task is not accepted or ready to close                                            |
| Contract repeat 6              | Complete                                                    | `32450ac`                    | Removed source-owned declaration copies, linked their exact owners, and truthfully labeled future schematic examples without changing meaning                            |
| Red repeat 9                   | Complete, deliberate no-op                                  | `c28091a`                    | Fresh evidence passed unchanged after semantic-neutral Contract ownership alignment; no expectation or test change was warranted                                         |
| Green repeat 9                 | Complete, deliberate no-op                                  | `b3690ea`                    | Production remains byte-identical at `1337e92` and correct against the aligned Contract and unchanged frozen Red evidence                                                |
| Blue repeat 9                  | Complete                                                    | `d3f6d02`                    | Established `RootTokenControl` as the focused production owner for dash, long-option, and terminator controls without changing behavior                                  |
| Purple repeat 2                | Complete                                                    | `8fa61d5`                    | Consolidated CLI test fixtures and support across the exact 16-file test-only scope without changing behavior or expectations                                            |
| Phase Review repeat 9          | Rejected from Red                                           | None                         | Four final blocking categories remain; Contract repeat 6 remains valid and Red is the earliest invalidated phase                                                         |
| Red repeat 10                  | Complete                                                    | `e4dba63`                    | Froze complete three-shape root parser usage behavior and an independent ordered Contract inventory across eight Red-owned files                                         |
| Green repeat 10                | Complete                                                    | `c69e999`                    | Root parse failures now return all three valid root usage shapes through the existing help-data owner                                                                    |
| Blue repeat 10                 | Complete                                                    | `18cf026`                    | Unified root command metadata ownership and completed `RootTokenControl` consumer coverage without changing behavior                                                     |
| Blue repeat 11                 | Complete                                                    | `d74c42e`                    | Corrected exactly two `parser-results.ts` consistent-type-import violations without changing behavior or runtime exports                                                 |
| Purple repeat 3                | Complete                                                    | `c66c7ad`                    | Consolidated test-support ownership across the exact 17-path behavior-neutral scope after accepted Blue repeat 11                                                        |
| Phase Review repeat 10         | Accepted                                                    | None                         | Top-99th material-readiness review accepted the complete corrective chain with no blocker; no review code commit was created                                             |
| Whole-Task Acceptance repeat 2 | Complete, accepted                                          | This acceptance-state commit | Fresh independent review accepted the clean `5f1cf44` candidate at the top-99th material-readiness threshold; closure evidence and routed residuals are recorded below   |

## Decisions Needed

No user decision blocks the repeated cycle. Red repeat 6 remains recorded as a
deliberate evidence no-op whose authority audit returned the chain to Contract
before Green. Contract repeat 4 has now corrected the one residual current
architecture passage in
`.agents/memory/crystallized/documents/cli/architecture.md`. Package metadata,
the lockfile, configuration, scripts, and focused source are consistently
authoritative for exact executable values, while the development-toolchain
document defines their relationships and quality, portability, and
compatibility obligations.

Red repeat 7 and Green repeat 7 completed as deliberate no-ops. Blue repeat 7
then removed the sole runtime ESM dependency cycle without changing the frozen
callable surface or executable behavior. The cycle
`parser-results.ts -> parse-result-message.ts -> parser-results.ts` is now
absent, and existing imports from and exports of `parser-results.ts` remain
valid.

Whole-Task Acceptance repeat rejected the Task from Contract at clean review
state `1281ef2`; it is not accepted or ready to close. Contract repeat 6 is now
complete at `32450ac`, and Red repeat 9 completed as a deliberate no-op at
`c28091a`. Green repeat 9 completed as a deliberate no-op at `b3690ea`, and
Blue repeat 9 completed its focused parser-control owner at `d3f6d02`. Purple
repeat 2 completed the behavior-preserving test-tree cleanup at `8fa61d5`.
Phase Review repeat 9 rejected the chain from Red while leaving Contract repeat
6 accepted and valid. Red repeat 10 completed at `e4dba63`, Green repeat 10
completed at `c69e999`, and Blue repeat 10 completed at `18cf026`. Purple repeat
3's 17-path delta is paused and preserved after its strict audit returned the
chain to Blue repeat 11, completed at `d74c42e` for exactly two pre-existing
type-import corrections. Purple repeat 3 completed its one authorized rerun at
`c66c7ad`. Phase Review repeat 10 accepted the complete corrective chain and
authorized Whole-Task Acceptance repeat 2, which accepted the Task. No user
decision blocked the final review.

During this Task, the user accepted the following temporary development
direction until a focused Workflow and TypeScript-conventions Task promotes it
to its durable owners:

- An import declaration that imports only types uses `import type`.
- A `*.types.ts` module is reserved for a module containing types only.
  `src/cli/parser/parse-issue.ts` correctly remains a normal runtime module
  because it owns runtime discriminant constants as well as their types.
- Purple follows Blue and owns only tests, fixtures, snapshots, and test-only
  helpers. It may improve their structure while Contract, production, accepted
  expectations, and observable behavior remain unchanged. A missing or
  incorrect expectation returns to Red rather than being invented in Purple.
- Grounded oddities, unconformities, and observations are written eagerly when
  discovered rather than deferred to a handoff. A phase agent that cannot edit
  the appropriate record reports the finding immediately so the orchestrator
  records it before dependent work continues.
- Blue repeat 11 owns exactly the two recorded production type-import fixes.
  Purple repeat 3 completed its preserved implementation and review rerun after
  Blue completed.
- Phase Review repeat 10 and Whole-Task Acceptance repeat 2 used the accepted
  99th-percentile material-readiness stopping rule. They must still reject a
  material correctness, Contract, integrity, portability, ownership, or
  maintainability defect, but must not create another cycle for cosmetic or
  speculative improvement.
- The retained frozen-MVP evidence of 25 fast and 162 closure tests is accepted
  and trusted. It must not be rerun again within Foundation.
- Permanent consistent-type-import ESLint enforcement and the rule that dynamic
  `import()` is reserved for genuinely asynchronous lazy boundaries are
  accepted for the next Workflow and TypeScript-conventions Task, not this
  Foundation scope. Ordinary dependencies use static imports.

After Foundation acceptance, the orchestrator locally squash-integrates the
Task branch onto the unambiguous original branch, `feature/cli-overhaul`,
without pushing. Work then continues just in time on a new Task and branch with
one focused Workflow and TypeScript-conventions Task, followed by a separate
repository-wide Prettier-baseline Task. The formatting baseline uses
`useTabs: false`, `tabWidth: 2`, `trailingComma: "all"`, `semi: true`,
`singleQuote: false`, `printWidth: 100`, and `endOfLine: "lf"`. No repository
formatting sweep belongs in Foundation.

Whole-Task Acceptance had rejected the delivery from Contract/current-truth
alignment because accepted current documentation still described the
replacement as paused, absent, or only future-buildable even though the
Foundation candidate is implemented and buildable. Contract repeat 2
corrected that material documentation finding in seven current documents
without claiming that the nineteen-leaf replacement is complete, shipped,
published, or ready for cutover.

The complete current-truth audit also found two related authority areas beyond
the locations initially named by Whole-Task Acceptance: the workspace source
map and the CLI development-toolchain route. Their authoritative
`sources-of-truth.md`, `development-toolchain.md`, and `_cli.md` surfaces were
included in the same alignment without introducing a new product decision.

The stale implementation copy in the `build.ts` missing-source fallback is now
resolved. Red repeat 5 froze the boundary by rejecting the two named stale
phrases without prescribing exact replacement wording, and Green repeat 5
replaced them with current source-specific guidance. Blue repeat 5 is complete
as a deliberate no-op at orchestration-state baseline `632c2ab`, with corrected
production at `cd5e6fb`. Phase Review repeat 5 found no issue in those corrected
surfaces and rejected only the remaining Contract transition residue described
above.

Phase Review repeat 4 had accepted the complete repeated cycle with no
blocking, optional, or newly observed phase finding and authorized the fresh
whole-Task reviewer. Red repeat 4 proves the exact npm inventory and
centralizes the two reusable test values at their nearest common support
scopes. Green repeat 4 passed that frozen evidence as a deliberate no-op at
orchestration-state and implementation baseline `80304bc`, while the corrected
production remains the accepted implementation from `aa5a8b2`. Blue repeat 4
completed as a deliberate no-op at orchestration-state baseline `ba14d6f`;
neither no-op warranted a phase code commit.

The user accepted one guarded write lifecycle that contains a callback failure
and its paired error event, truthful optional suggestions that never repeat the
rejected token, shared deterministic fixtures at their nearest common
test-support scope, and one union-typed final output with at most one non-empty
stream.

The accepted Foundation distribution candidate is one `dist/cli.mjs` whose
generated modules embed the Framework and first-party Extension payloads.
Standalone payload copies, archives, manifests, checksums, and raw Framework or
Extension duplication in the npm payload leave the current scope. Removing
that unneeded artifact family resolves its locale-ordering finding without
inventing a comparator contract. This does not declare the complete replacement
shipped or ready for publication.

Global debug diagnostics and additional distribution channels remain deferred
Emerging Ideas for brownfield dogfooding after integrated cutover. They neither
block nor expand the authorized corrective cycle. Blue repeat 3 completed as a
deliberate no-op at the corrected Green implementation
`aa5a8b24bcb3e838bfbc716179fb84d6e696dadc`. The Review findings do not reopen
those deferred Ideas or the accepted Contract and production direction.

## Evidence

- Baseline commit: `6d87744660c287188def80319400295552fa9f17`.
- Baseline `git diff --check`: passed.
- Baseline `bun run test:ci`: 187 tests passed, zero failed.
- Task setup commit: `4e30d1e` (`Start replacement CLI foundation`).
- Contract commit: `498592d` (`Define replacement CLI foundation contracts`).
- Contract evidence rerun by the orchestrator: `typecheck`, `lint`, `build`,
  Node syntax, Deno check, and 25 frozen MVP fast tests passed.
- Red commit: `d3950c7` (`Specify replacement CLI foundation behavior`).
- Red evidence rerun by the orchestrator: typecheck and lint passed; direct
  evidence produced 7 passes and the intended 12 implementation-boundary
  failures; integration produced 6 passes and the intended single
  placeholder-elimination failure; end-to-end produced 3 passes and the
  intended 8 implementation failures across Node.js, Bun, and Deno; all 25
  frozen MVP fast tests passed.
- Contract production surfaces remained unchanged during Red, and Red changed
  only replacement tests and test support.
- Green commit: `f74f056` (`Implement replacement CLI foundation behavior`).
- Green evidence rerun by the orchestrator: strict typecheck and typed lint;
  19 direct, 7 integration, and 11 end-to-end tests; Node syntax; Deno check;
  and 25 frozen MVP fast tests all passed.
- Green changed only production files under `src/cli/`, preserved every Red
  test byte-for-byte, removed the explicit placeholder, implemented no domain
  leaf, used no type or lint suppression escape hatch, and kept every
  replacement production TypeScript module below 200 lines.
- Blue commit: `8f52cfe` (`Split CLI build packaging into focused modules`).
- Blue evidence rerun by the orchestrator: strict checks and all 37 replacement
  tests, Node syntax, Deno check, 25 frozen MVP fast tests, and a 54-file
  package dry run passed. The package excludes the build composition/helpers.
- Blue reduced the 256-line root build script to 52 lines and added focused
  51-, 13-, 51-, and 96-line helpers. CLI bundle, source manifest, deterministic
  archive, and archive-checksum hashes remained byte-identical to Green, and
  no callable contract, Red evidence, payload, dependency, or behavior changed.
- Phase Review cycle 1 rejected the chain from Red. High findings were an
  uncontained final stream-write failure and raw-token parser handling that
  lets alias-inline, missing-value, and delimiter forms bypass accepted policy.
  Medium findings were incomplete bounded option/short-form suggestion
  evidence and unproved Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 floors.
- The orchestrator independently reproduced root help for `--ws=foo`, the
  wrong result for `--ws=foo --help`, missing-value misclassification for
  `--workspace --json`, JSON presentation from the operand in `-- --json`, an
  invented `--hv` suggestion, and an uncaught final-write stack with exit 1.
- Review found no independent Contract or Blue defect. Contract remains frozen;
  the workflow restarts at Red and repeats Green, Blue, and phase Review.
- Red repeat commit: `079eb93` (`Strengthen replacement CLI foundation evidence`).
- Red repeat evidence rerun by the orchestrator: strict typecheck and lint
  passed; direct produced 22 passes and 6 intentional parser-policy failures;
  integration passed 7 tests; end-to-end produced 14 passes and 2 intentional
  final-write failures; all 25 frozen MVP fast tests passed.
- Pinned binary probes and unchanged-artifact version/parse journeys passed on
  Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0. The Red repeat changed exactly
  four new test files and no production, Contract, configuration, Task,
  documentation, dependency, or frozen-MVP surface.
- Green repeat commit: `0da8b05` (`Harden replacement CLI parsing and final output`).
- Green repeat evidence rerun by the orchestrator: strict checks; 28 direct,
  7 integration, and 16 end-to-end tests; Node syntax; Deno check; exact
  minimum-runtime lanes; and all 25 frozen MVP fast tests passed.
- Green repeat changed six production-only Foundation paths, preserved all Red
  evidence byte-for-byte, implemented no domain behavior, used no suppression
  escape hatch, and kept every production TypeScript module below 200 lines.
- Blue repeat completed as an explicit no-op at corrected Green commit
  `0da8b05`; no empty or cosmetic commit was created.
- Blue repeat audited 27 production/build modules: 23 are at most 100 lines,
  four are 101–200 lines, and none exceed 200. Strict checks, 28 direct,
  7 integration, 16 end-to-end, exact minimum-runtime lanes, Node/Deno checks,
  25 MVP fast tests, package dry run, copied payloads, manifest/archive/checksum
  consistency, and frozen-evidence hashes passed with a clean unchanged tree.
- Phase Review repeat 1 rejected from Red. It found untested canonical
  single-dash spellings and separate empty workspace values, plus missing
  callback/error-event/listener-cleanup evidence for final writes.
- The orchestrator independently reproduced fabricated `--help` corrections
  for `-workspace`, `-yes`, and `-xyz`, and inapplicable-global results for
  separate empty `--workspace`/`--ws` values while inline-empty forms correctly
  produced missing-option-value. Reviewer probes confirmed the final-writer
  implementation already contains callback and emitted-error failures with
  exit 4, safe streams, redaction, and listener cleanup; Red evidence must make
  those guarantees durable.
- The user clarified that every single-dash token remains invalid. An
  unambiguous missing-dash spelling may report its one real canonical option;
  a cluster or unrelated token fails as unknown without a fabricated
  correction. Red repeat 2 encodes that distinction.
- Red repeat 2 commit: `4b6872c` (`Complete replacement CLI foundation
edge-case evidence`), staged and committed by the orchestrator after the
  phase agent left exactly seven test/test-support paths unstaged.
- Red repeat 2 evidence rerun by the orchestrator: strict typecheck and lint;
  38 direct passes with 8 intentional parser failures; 7 integration passes;
  28 end-to-end passes; and 25 MVP fast passes. Runtime filters independently
  selected 9 Node, 7 Bun, and 7 Deno cases with no failures.
- The E2E writer evidence now covers synchronous throws, callback-delivered
  errors, emitted error events, redaction, semantic exit 4, safe stream claims,
  and listener cleanup on Node, Bun, and Deno. Deno `eval` did not reliably
  propagate Node-compatible `process.exitCode`; the retained executable shim
  uses the supported `deno run` boundary with read permission limited to the
  candidate artifact.
- From `4b6872c` forward, only the orchestrator stages and commits. Earlier
  phase-agent commits remain intact and reviewable; history is not rewritten.
- Phase Review should distinguish deliberate wire-format/named-value contract
  assertions from direct behavioral tests that restate protocol control
  strings instead of importing the named values.
- Green repeat 2 commit: `8887f01` (`Correct replacement CLI root token
validation`), staged and committed by the orchestrator after the phase agent
  left exactly two parser production files unstaged.
- Green repeat 2 evidence rerun by the orchestrator: strict checks; 46 direct,
  7 integration, and 28 end-to-end tests; current and exact minimum runtime
  lanes; Node syntax; Deno check; and all 25 MVP fast tests passed. Frozen Red,
  toolchain, Contract, build, writer, payload, MVP, and domain surfaces did not
  change.
- Blue repeat 2 commit: `6d7d6e4` (`Separate CLI root parse policy`), staged
  and committed by the orchestrator after the phase agent left one modified
  coordinator and one new classifier module unstaged.
- Blue repeat 2 reduced the root invocation coordinator from 126 to 46 lines;
  the focused parse-policy classifier is 89 lines. No production/build module
  exceeds 200 lines. Strict checks, all 81 replacement tests, runtime filters,
  exact minimum lanes, 25 MVP fast tests, and the 54-file package dry run
  passed with frozen Contract and evidence surfaces unchanged.
- Observation for phase Review: source-package manifest and tar enumeration use
  locale-dependent `localeCompare`. Repeated builds are stable on the current
  host, but cross-locale byte determinism has not been proved. Blue did not
  change ordering because doing so could change artifact bytes and exceeded
  behavior-neutral refactor authority.
- Phase Review repeat 2 rejected from Red with four findings: a callback error
  followed by a late stream error event escapes after listener cleanup; exact
  documented root tokens self-suggest while temporarily returning unknown;
  manifest/tar ordering depends on the process locale; and behavioral direct
  tests restate raw protocol control values instead of named values.
- The orchestrator independently reproduced the material stream failure with
  exit 1 and a `secret-late-event` stack, `status --json` suggesting `status`,
  distinct `en`/`sv`/`tr`/`da` Unicode collation orders, and the cited raw
  control literals. Existing strict checks and 81 replacement tests remain
  green because Red does not yet encode these combined/boundary guarantees.
- Review otherwise re-proved the single-dash policy, aliases and value forms,
  runtime tags and filters, exact minimum lanes, phase ownership, orchestrator-
  only commits from `4b6872c`, clean/unpushed state, and deferred post-format
  checksum lifecycle.
- During the hold, the user required accepted clarification to be documented
  eagerly. The [parser result contract](../crystallized/documents/cli/contracts/parser-results.md)
  now requires optional truthful corrections, excludes an identical candidate,
  and states the exact single-dash classification. The [tiered test Pattern](../../patterns/open-forge/cli/bun/tiered-test-slice.md)
  now requires shared deterministic fixtures for reused arbitrary values and
  imports from production for protocol and control values, with only scoped
  named-inventory and built-process interoperability assertions as exceptions.
  This documentation alignment did not start Red repeat 3.
- Integrity observation for the corrective review: `CliCompletion` permits
  simultaneous non-empty stdout and stderr even though current display adapters
  effectively select one stream. A future adapter could therefore complete the
  stdout write and then fail on stderr, leaving partial public output. This
  observation does not change the Contract or authorize a redesign during the
  hold.
- The user resolved the hold and authorized the corrective sequence on
  2026-08-03. Contract repeat 1 commit `a447c0f` (`Align CLI foundation output
and distribution contracts`) accepts one union-typed final stream, the
  guarded callback-plus-paired-event lifecycle, nearest-common-scope fixtures,
  truthful optional suggestions, and one embedded `dist/cli.mjs` artifact.
- The orchestrator independently audited the `DisplayOutput` and
  `CliCompletion` consumers and the npm payload boundary. The typed consumers
  remain aligned, while the existing build-only standalone copies,
  archive/manifest/checksum generation, and raw `src/open-forge` and
  `src/extensions` npm inclusions are identified production surfaces for the
  authorized Red-to-Green correction rather than accepted release artifacts.
- Contract repeat 1 verification passed strict `typecheck`, typed `lint`,
  generated `index`, Open Forge `Doctor`, and `git diff --check`. The npm dry-run
  audit independently confirmed the current duplicate raw payload that Green
  must remove after Red makes the accepted package boundary executable.
- Current development guidance now matches the declared Bun 1.3.14 repository
  toolchain and describes the one-artifact build, npm payload, and source-release
  boundary.
- Global debug diagnostics are preserved in the [debug diagnostics Idea](../emerging/ideas/cli-debug-diagnostics.md)
  for brownfield dogfooding after the complete replacement is available.
  Bundler-generated chunks, ecosystem-native packages, and Bun or direct
  executables are preserved separately in the [distribution channels Idea](../emerging/ideas/cli-distribution-channels.md)
  for evaluation only after integrated cutover.
- The locale-dependent standalone-package ordering finding is resolved by
  removing the unneeded standalone artifact family from the accepted release,
  not by selecting an otherwise undiscussed path comparator.
- Red repeat 3 commit: `65e5fd8d7d9759aa8b0059fc9e40c629084a694b`
  (`Complete replacement CLI foundation corrective evidence`).
- Red repeat 3 evidence rerun by the orchestrator: strict typecheck and typed
  lint passed; direct evidence produced 47 passes and exactly two intentional
  failures for exact command and canonical-option self-suggestions;
  integration passed 7 tests; end-to-end produced 25 passes and exactly 11
  intentional failures comprising two distribution boundaries, six paired
  callback/error-event cases on current Node.js, Bun, and Deno, and three
  lifecycle cases at the exact supported runtime floors.
- Runtime-filter evidence selected 8 passes, 3 intentional failures, and 25
  filtered cases for Node.js; 6 passes, 3 intentional failures, and 27
  filtered cases each for Bun and Deno. All 25 frozen MVP fast tests and
  `git diff --check` passed.
- Exact Node.js 22.12.0 cannot load the TypeScript final-stream-failure shim by
  default. The existing floor launcher therefore requires `--no-warnings` and
  `--experimental-strip-types` for that executable test boundary.
- Green repeat 3 commit: `aa5a8b24bcb3e838bfbc716179fb84d6e696dadc`
  (`Correct CLI foundation output and distribution behavior`).
- Green repeat 3 implemented one guarded final-write lifecycle using the
  imported `node:timers` callback, cancellation, and callback/error-event
  settlement boundary; excluded exact suggestion candidates; removed the
  custom archive, manifest, checksum, and build-helper family; and reduced the
  npm package to the accepted embedded artifact and public package material.
- Green repeat 3 evidence rerun by the orchestrator: strict typecheck and typed
  lint passed; direct evidence passed 49 tests, integration passed 7 tests, and
  end-to-end evidence passed 36 tests with zero failures.
- Runtime-filter evidence selected 11 passes with 25 filtered cases for
  Node.js, and 9 passes with 27 filtered cases each for Bun and Deno. Paired
  callback/error-event checks also passed at the exact Node.js 22.12.0, Bun
  1.3.0, and Deno 2.8.0 floors. All 25 frozen MVP fast tests passed.
- The build bundled 29 modules into the single 190346-byte `dist/cli.mjs`
  artifact. The npm dry run contained exactly 7 accepted entries totaling
  72417 packed bytes and 265086 unpacked bytes.
- `git diff --check` passed. Frozen Red evidence and Contract sources remained
  unchanged through Green repeat 3.
- Blue repeat 3 completed as a deliberate no-op at the corrected Green
  implementation `aa5a8b24bcb3e838bfbc716179fb84d6e696dadc`. No phase code commit
  was created. The 92-physical-line writer is one focused single-settlement
  lifecycle, the 37-line suggestion module contains a direct exact-candidate
  guard, and the 35-line build entrypoint owns only the single-artifact build
  boundary. The remaining candidates were cosmetic or would add abstraction
  without another consumer.
- Blue repeat 3 audited 28 authored production and tooling modules by physical
  lines: 25 are at most 100 lines, three are 101–200 lines, and none exceed 200.
  The earlier handoff counts used nonblank lines. No generic helper was
  introduced, and frozen Red tests and fixtures remained outside Blue's
  production-only authority.
- Blue repeat 3 evidence passed strict typecheck and typed lint, 49 direct,
  7 integration, and 36 end-to-end tests. Runtime filters selected 11 Node.js
  cases with 25 filtered and 9 Bun and 9 Deno cases with 27 filtered each.
  Paired final-stream checks at the exact Node.js 22.12.0, Bun 1.3.0, and Deno
  2.8.0 floors, all 25 frozen MVP fast tests, the exact single-artifact build
  and seven-entry npm package boundary, Node syntax and Deno checks, and
  `git diff --check` also passed.
- Contract, frozen Red, and corrected Green files remained byte-identical
  during Blue repeat 3. The Blue audit produced no new oddity, mismatch, or
  unconformity to record.
- Phase Review repeat 3 rejected the chain from Red without creating a review
  commit. Its medium blocking finding is that the distribution end-to-end
  evidence asserts required inclusions and selected exclusions, but neither
  exactly one npm report nor exact sorted equality with the accepted seven
  package paths. An arbitrary eighth package path could therefore pass.
- Its low but blocking finding under the user's explicit no-duplication rule is
  that the `safeFailure` semantic fixture is duplicated in
  `final-stream-failure.e2e.test.ts` and `minimum-runtimes.e2e.test.ts`, while
  the arbitrary direct-test token `stats` is duplicated in
  `parser-results.test.ts` and `main.test.ts` instead of the shared Foundation
  fixture.
- Review otherwise found the Contract, production behavior, structure, phase
  ownership, commit boundary, and evidence credible. Strict typecheck and
  typed lint passed; all 49 direct, 7 integration, and 36 end-to-end tests
  passed. Runtime filters passed 11 Node.js cases with 25 filtered, and 9 Bun
  and 9 Deno cases with 27 filtered each. Exact Node.js 22.12.0, Bun 1.3.0,
  and Deno 2.8.0 lanes, all 25 frozen MVP fast tests, Node syntax, Deno check,
  Open Forge Doctor, and `git diff --check` passed. Previously recorded build
  artifact and seven-entry package figures remain valid.
- Read-only probes on the current Node.js runtime and Node.js 22.12.0 confirmed
  that a real `Writable` reports a write failure in the order callback error,
  paired `error` event, then `setImmediate`. This supports but does not replace
  the cross-runtime executable evidence.
- Red repeat 4 commit: `06507543818e353f92ef0dbd6d46770ae23624ca`
  (`Tighten CLI foundation review evidence`).
- Red repeat 4 requires exactly one npm dry-run report and compares its package
  paths with the accepted seven-path inventory using order-independent exact
  equality. `SafeEmergencyFailureStderr` now lives in the common end-to-end
  runtime support, and `FoundationUnknownCommandToken` now lives in the common
  direct Foundation fixture. The corrective audit found no other nearby
  exact-scope duplication.
- A readonly tuple used by the exact inventory matcher required a mutable
  sorted copy. This is a nonmaterial test-authoring observation and does not
  alter the accepted package contract.
- The orchestrator reran strict typecheck and typed lint; 49 direct, 7
  integration, and 36 end-to-end tests; runtime filters selecting 11 Node.js
  cases with 25 filtered and 9 Bun and 9 Deno cases with 27 filtered each;
  exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 floors; both distribution
  cases; all 25 frozen MVP fast tests; and `git diff --check`. All passed.
- Red repeat 4 changed exactly seven test or test-support paths:
  `src/cli/e2e/distribution.e2e.test.ts`,
  `src/cli/e2e/final-stream-failure.e2e.test.ts`,
  `src/cli/e2e/minimum-runtimes.e2e.test.ts`,
  `src/cli/e2e/supported-runtime-fixture.ts`, `src/cli/main.test.ts`,
  `src/cli/parser/parser-results.test.ts`, and
  `src/cli/testing/fixtures/foundation-cli-input.ts`. Production and frozen
  Contract sources remained byte-identical.
- Green repeat 4 completed as a deliberate no-op at orchestration-state and
  implementation baseline `80304bcd7e09c6f04eb9bd75bed6ced9dbb49244`.
  Frozen Red repeat 4 exposed no production defect, the corrected production
  remains `aa5a8b24bcb3e838bfbc716179fb84d6e696dadc`, and no phase code commit
  was created.
- Green repeat 4 evidence passed strict typecheck and typed lint, 49 direct,
  7 integration, and 36 end-to-end tests. Runtime filters selected 11 Node.js
  cases with 25 filtered, and 9 Bun and 9 Deno cases with 27 filtered each.
  Exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 lifecycle and parity cases
  passed.
- The Green repeat 4 build bundled 29 modules into the single 190346-byte
  `dist/cli.mjs` artifact. The npm dry run returned exactly one report with
  the accepted seven paths, totaling 72417 packed bytes and 265086 unpacked
  bytes.
- All 25 frozen MVP fast tests, `node --check dist/cli.mjs`,
  `deno check dist/cli.mjs`, and `git diff --check` passed. The final working
  tree was clean.
- Since corrected Green `aa5a8b2`, only Task state and Red repeat 4's seven
  test or test-support paths changed. Frozen Red repeat 4 matches `0650754`,
  accepted Contract sources match `a447c0f`, and the frozen MVP and Framework
  or Extension payloads match the Task baseline. Removed build helpers remain
  absent and unreferenced from replacement production, build, and package
  surfaces; historical `open-forge-src` references remain only inside the
  intentionally frozen MVP.
- During Green repeat 4, one intermediate `git status` briefly marked two Red
  files modified despite an empty content diff. Refreshing the index cleared
  the marks; repeated status and byte comparisons were clean and confirmed no
  filesystem mutation. No staging, commit, or push occurred in the phase.
- Blue repeat 4 completed as a deliberate no-op at orchestration-state baseline
  `ba14d6fccdb02f085f76e3641d21447549343e45`. Corrected production remains
  `aa5a8b24bcb3e838bfbc716179fb84d6e696dadc`, and no phase code commit was
  created.
- The Blue repeat 4 physical-line audit counted the final writer at 92 lines,
  the suggestion module at 44 lines, and `build.ts` at 38 lines. Across 28
  authored non-test, non-E2E, non-fixture production and tooling modules under
  `src/cli`, 25 are at most 100 physical lines, three are 101–200 lines, and
  none exceed 200. The earlier smaller suggestion and build figures counted
  nonblank lines.
- The final writer remains one focused single-settlement lifecycle, exact
  suggestion exclusion remains local to its confidence policy, and the build
  entrypoint owns only generation, output cleanup, bundling, shebang insertion,
  and reporting. Blue found no material duplication, generic helper,
  dependency-direction issue, or responsibility split. Removed archive,
  manifest, and tar helpers remain absent and have zero active references.
- Blue repeat 4 evidence passed strict typecheck and typed lint, 49 direct,
  7 integration, and 36 end-to-end tests. Runtime filters selected 11 Node.js
  cases with 25 filtered, and 9 Bun and 9 Deno cases with 27 filtered each.
  Exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 floor journeys passed within
  those lanes, as did all 25 frozen MVP fast tests.
- The Blue repeat 4 build bundled 29 modules into the single 190346-byte
  `dist/cli.mjs` artifact with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  Node syntax, Deno check, and `git diff --check` passed. The npm dry run
  returned exactly one report with the accepted seven paths, totaling 72417
  packed bytes and 265086 unpacked bytes.
- Blue repeat 4 integrity aggregates matched exactly: corrected Green
  implementation
  `5f957126c0c9ab471fb4a7b99d482af6acecab8197da03954a0d2ffea185aa2f`,
  frozen Red repeat 4
  `5ccace011026b19c3f74be6ebc7d8b6e2f16fbe398ebeeae249085e23e11c2fe`,
  accepted Contract
  `65c9baec951dd9dae0feccebe488a92f1ed51dde6e0b7c348ddb19e539cb5f82`,
  and frozen MVP plus payloads
  `59e13deab1c165f67499e7daa6f7c19ff5e93d237a671e93d4f86bb4ccaa0355`.
- Parallel filtered end-to-end invocations contend over the shared
  `dist/cli.mjs` build and caused one Windows `EPERM` during shebang rewriting.
  Serial runtime lanes passed completely, so this is verification-orchestration
  contention rather than a product defect. Future filtered lanes must run
  serially unless each receives an isolated distribution output.
- The transient Git-index observation from Green repeat 4 did not recur across
  repeated Blue status checks. The final tree was clean, and Blue staged,
  committed, and pushed nothing.
- Phase Review repeat 4 accepted the complete Contract-through-Blue cycle with
  no blocking, optional, or newly observed finding. It created no phase code
  commit and authorized the fresh read-only whole-Task Acceptance review.
- The prior review blockers are resolved. Distribution evidence requires
  exactly one npm report and order-independent exact equality with the seven
  accepted paths, so an eighth, duplicate, or missing path fails.
  `SafeEmergencyFailureStderr` and `FoundationUnknownCommandToken` each have
  one owner at their nearest common test-support scope and imported consumers.
  No improper exact-scope duplication remains; raw values are limited to named
  inventory tests or built-process interoperability assertions. Generated test
  declarations remain limited to tagged end-to-end runtime matrices, while
  direct and integration declarations are explicit.
- The independent review reconfirmed the single-dash and truthful-suggestion
  boundaries, the union-typed single final stream, guarded single-settlement
  writer lifecycle and sanitized exit 4, exact embedded distribution, absence
  of the removed archive, manifest, and tar helpers, frozen boundary integrity,
  focused production structure, orchestrator-only Git history, clean unpushed
  branch, and ordinary commit messages without co-author trailers.
- Phase Review repeat 4 independently passed strict typecheck and typed lint;
  49 direct, 7 integration, and 36 end-to-end tests; serial runtime filters of
  11 Node.js, 9 Bun, and 9 Deno cases; all three exact-floor lanes at Node.js
  22.12.0, Bun 1.3.0, and Deno 2.8.0; both distribution cases; and all 25 frozen
  MVP fast tests. Node syntax, Deno check, Open Forge Doctor, and
  `git diff --check` also passed.
- The independently built `dist/cli.mjs` is exactly 190346 bytes with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report containing the accepted seven
  paths, totaling 72417 packed bytes and 265086 unpacked bytes.
- The residual verification constraint is unchanged: parallel filtered
  end-to-end lanes contend over the shared Windows `dist/cli.mjs`, so runtime
  filters must run serially unless they receive isolated outputs. Broader
  operating-system, filesystem, and future domain-operation coverage remains
  outside the Foundation slice rather than an unrecorded Foundation blocker.
- Fresh Whole-Task Acceptance rejected the Task from Contract/current-truth
  alignment without creating an acceptance commit. The medium blocking finding
  is limited to stale current documentation that still describes replacement
  implementation, buildability, test placement, runtime-floor ownership, and
  removed build products as future or paused state. The related stale
  `build.ts` scaffold error is low severity, but belongs in the same repeated
  alignment cycle.
- The whole-Task reviewer passed every other acceptance check: the delivered
  Foundation outcome is complete within its declared scope; accepted Contract,
  Red evidence, Green behavior, and Blue structure agree; parser, final-output,
  runtime, distribution, safety, and frozen-MVP boundaries remain intact; the
  branch diff and commits remain reviewable; recorded residual risks remain
  accurate; and no unimplemented domain leaf is presented by the artifact.
- Whole-Task validation passed `bun run check`, including strict typecheck,
  typed lint, 49 direct tests, 7 integration tests, and 36 end-to-end tests.
  Serial runtime filters selected and passed 11 Node.js, 9 Bun, and 9 Deno
  cases. Exact-floor journeys passed at Node.js 22.12.0, Bun 1.3.0, and Deno
  2.8.0.
- Retained MVP validation passed all 25 fast tests and all 162 closure tests.
  Both requested Open Forge Doctor validations passed. Node syntax and Deno
  checks of the built artifact also passed.
- The independently rebuilt `dist/cli.mjs` remained exactly 190346 bytes with
  SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven paths,
  totaling 72417 packed bytes and 265086 unpacked bytes.
- Whole-Task history inspection found ordinary descriptive commit messages,
  no co-author trailers, and the required orchestrator-only commit boundary.
  The review baseline was clean, the Task branch has no configured upstream,
  and nothing was pushed. Whole-Task Acceptance itself staged and committed
  nothing.
- Contract repeat 2 commit:
  `a126b0b194a325c0c2f76ecfa6e1185ca266a147` (`Align CLI Foundation current
documentation`).
- Contract repeat 2 aligned exactly seven current documents:
  `docs/development.md`,
  `.agents/memory/crystallized/documents/cli/architecture.md`,
  `.agents/memory/crystallized/documents/cli/contracts/runtime-compatibility.md`,
  `.agents/memory/crystallized/documents/cli/mvp-architecture.md`,
  `.agents/memory/crystallized/documents/cli/development-toolchain.md`,
  `.agents/memory/crystallized/documents/cli/_cli.md`, and
  `.agents/workspace/sources-of-truth.md`.
- The aligned documentation now describes the implemented, buildable
  Foundation, source-owned scripts and runtime floors, single embedded build
  artifact, and separate frozen MVP while preserving the incomplete-command,
  unpublished, and no-cutover boundaries.
- The Contract audit included the additionally discovered workspace source map
  and CLI toolchain/route current truth under their existing authority. They
  required no new product or architecture decision.
- Contract repeat 2 validation passed both generated-index checks, both Open
  Forge Doctors, documentation-link and package-script audits, the build and
  all four Foundation probes, and the exact npm package inventory. The stale
  current-language audit found only the `build.ts` missing-source message
  already routed to Red and Green; `git diff --check` passed.
- Red repeat 5 commit:
  `365e80e3387b4b50cd976aa74d8ef3a572cd2933` (`Reject stale replacement build
guidance`).
- Red repeat 5 added exactly one 35-line integration test. It creates and
  verifies an isolated empty working directory, executes the real `build.ts`,
  rejects the named `implementation is paused` and `restore its scaffold`
  phrases without requiring exact new wording, and removes the temporary
  directory in a `finally` boundary.
- The focused Red repeat 5 run produced zero passes and the one intentional
  failure. The full integration suite produced 7 passes and that same one
  intentional failure, while all 49 direct, 36 end-to-end, and 25 frozen MVP
  fast tests passed. Strict typecheck, typed lint, and `git diff --check` also
  passed.
- Bun includes both the throwing source excerpt and the error message in
  stderr for this build failure. The stale production source copy itself must
  therefore change during Green rather than relying on different surrounding
  error rendering. Contract repeat 2 documentation remains frozen at
  `a126b0b194a325c0c2f76ecfa6e1185ca266a147`.
- Green repeat 5 commit:
  `cd5e6fb8183a9669501763ac0331c6e15a26e706` (`Correct replacement build source guidance`).
- Green repeat 5 changed exactly one production line in `build.ts`: the
  missing-source failure now identifies the inaccessible required replacement
  entry source instead of describing the implementation as paused or asking
  the maintainer to restore a scaffold.
- The focused Red repeat 5 integration test passed. The complete check passed
  49 direct, 8 integration, and 36 end-to-end tests. Serial Node.js, Bun, and
  Deno runtime filters and their exact supported-floor journeys passed, as did
  both distribution cases, all 25 frozen MVP fast tests, the build, Node
  syntax, Deno check, both Open Forge Doctors, and `git diff --check`.
- The rebuilt `dist/cli.mjs` remains exactly 190346 bytes with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven paths,
  totaling 73144 packed bytes and 267010 unpacked bytes; the increase reflects
  the aligned public documentation rather than an artifact change.
- Contract repeat 2, frozen Red repeat 5, corrected production, frozen MVP,
  and Framework or Extension payload integrity checks passed. The retained MVP
  closure suite was not rerun because Green changed only the replacement
  build-entry error copy and left the frozen MVP byte-identical. Green repeat 5
  produced no new oddity, mismatch, or unconformity.
- Blue repeat 5 completed as a deliberate no-op at orchestration-state baseline
  `632c2ab2aaba3508fb1746f9fd29b453c5b04fb7`. Corrected production remains
  `cd5e6fb8183a9669501763ac0331c6e15a26e706`, and no phase code commit was
  created.
- The 38-physical-line build entrypoint remains one cohesive boundary for
  generation, output cleanup, bundling, shebang insertion, and reporting.
  Across 28 authored replacement production and tooling modules, 25 are at
  most 100 physical lines, three are 101–200 lines, and none exceed 200. Blue
  found no material responsibility split, duplication, generic helper,
  dependency-direction defect, stale build guidance, or active reference to
  removed archive, manifest, checksum, or tar helpers.
- Blue repeat 5 evidence passed strict typecheck and typed lint, 49 direct,
  8 integration, and 36 end-to-end tests. Serial Node.js, Bun, and Deno runtime
  filters and their exact supported-floor journeys passed, as did both
  distribution cases and all 25 frozen MVP fast tests.
- The Blue build reproduced the single 190346-byte `dist/cli.mjs` artifact with
  SHA-256 `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven paths,
  totaling 73144 packed bytes and 267010 unpacked bytes. Node syntax, Deno
  check, both Open Forge Doctors, and `git diff --check` passed.
- Contract repeat 2, frozen Red repeat 5, corrected production, frozen MVP,
  and Framework or Extension payload integrity checks passed during Blue. No
  stale reference, new oddity, mismatch, or unconformity was found. The prior
  shared-distribution serial verification constraint did not recur because the
  filtered runtime lanes were run serially; the constraint remains applicable
  to future shared-output verification.
- Phase Review repeat 5 rejected the Contract-through-Blue chain from Contract
  without creating a review commit. Its single low-severity but blocking
  category is transition residue in four locations: the CLI development
  toolchain document's frontmatter responsibility, both its linked toolchain
  Decision's preimplementation-ownership statement and future-migration
  Consequence, and the linked testing-architecture Decision's
  preimplementation-ownership statement.
- The accepted authority is unchanged: `package.json`, the lockfile,
  configuration, scripts, and focused source own exact executable values. The
  development-toolchain document owns their semantic relationships and
  compatibility obligations, while the Decisions preserve why those
  relationships were selected. Contract repeat 3 is authorized to audit the
  companion metadata and body once and perform the smallest coherent alignment
  rather than patching only the reported lines.
- Phase Review repeat 5 found no other blocking, optional, or newly observed
  issue. Contract behavior, frozen Red evidence, corrected Green production,
  Blue structure, embedded distribution, runtime portability, retained MVP,
  phase ownership, ordinary commit history, the orchestrator-only Git boundary,
  the clean unpushed branch, and all previously recorded residual constraints
  remained conforming.
- Phase Review repeat 5 independently passed strict typecheck and typed lint;
  49 direct, 8 integration, and 36 end-to-end tests; serial runtime filters of
  11 Node.js cases with 25 filtered and 9 Bun and 9 Deno cases with 27 filtered
  each; exact-floor journeys at Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0;
  both distribution cases; and all 25 frozen MVP fast tests. The build, Node
  syntax check, Deno check, both Open Forge Doctors, and `git diff --check`
  also passed.
- The review build reproduced the single 190346-byte `dist/cli.mjs` artifact
  with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven paths,
  totaling 73144 packed bytes and 267010 unpacked bytes.
- Contract repeat 3 commit:
  `c4b1e6113ab8c074515d35732cfcbb677406c4e3` (`Align CLI toolchain authority
with implemented source`).
- Contract repeat 3 aligned exactly three current files and the four reported
  transition-residue occurrences: the development-toolchain document's
  frontmatter responsibility, two statements in the CLI development-toolchain
  Decision, and one statement in the CLI testing-architecture Decision.
- The accepted authority did not change. Package metadata, the lockfile,
  configuration, scripts, and focused source remain authoritative for exact
  executable values; documentation retains their semantic relationships and
  quality, portability, and compatibility obligations; the Decisions preserve
  why that relationship was selected. No new decision arose.
- Contract repeat 3 validation passed both generated-index checks, both Open
  Forge Doctors, documentation-link validation, the stale transition-language
  audit, the exact changed-scope audit, and `git diff --check`.
- Red repeat 6 completed as a deliberate no-op at orchestration-state baseline
  `abf93bf`. It changed, staged, committed, and pushed nothing, and encountered
  no execution oddity.
- `bun run check` passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases with 25 filtered, and 9 Bun
  and 9 Deno cases with 27 filtered each, including the exact Node.js 22.12.0,
  Bun 1.3.0, and Deno 2.8.0 supported floors. Both distribution cases and all
  25 frozen MVP fast tests passed.
- The Red repeat 6 build reproduced the single 190346-byte `dist/cli.mjs`
  artifact with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven paths,
  totaling 73144 packed bytes and 267010 unpacked bytes.
- Node syntax, Deno check, both Open Forge Doctors, and `git diff --check`
  passed. Both repository and installable-payload index runs produced no diff.
  Frozen Red evidence matches `365e80e`, corrected production matches
  `cd5e6fb`, and the frozen MVP and Framework or Extension payloads match the
  Task baseline. Contract-to-state differences contained only the Task record.
- Red repeat 6 stopped phase advancement from Contract because
  `.agents/memory/crystallized/documents/cli/architecture.md:401-403` still
  assigns exact tools, options, scripts, runtime floors, and their future
  production transition to the development-toolchain document. This conflicts
  with the already accepted source-owned executable-value boundary. It is one
  companion current architecture sentence, not a missing executable
  expectation, and no Red test should encode prose ownership.
- Green repeat 6, Blue repeat 6, and Phase Review repeat 6 were superseded
  before starting when Red repeat 6 returned the chain to Contract.
- Contract repeat 4 commit:
  `3e04529be2174940b2ee2a8a363bb9c5beac4177` (`Align CLI architecture with
source-owned toolchain values`).
- Contract repeat 4 aligned exactly one passage in one current file,
  `.agents/memory/crystallized/documents/cli/architecture.md`. It now states
  that package metadata, the lockfile, configuration, scripts, and focused
  source are authoritative for exact executable values, while the CLI
  Development Toolchain document defines their relationships and quality,
  portability, and compatibility obligations.
- The companion audit found no equivalent residue in the architecture,
  development-toolchain document, linked Decisions, CLI authority map, or
  workspace source map. The correction introduced no new decision, executable
  expectation, or test need.
- Contract repeat 4 validation passed both index runs, both Open Forge Doctors,
  the architecture-link audit, the stale authority-language audit, continuity
  reload, the exact one-file scope check, and `git diff --check`.
- Red repeat 7 completed as a deliberate no-op at orchestration-state baseline
  `a5db8b2`. It created no phase code commit and changed, staged, committed, and
  pushed nothing.
- The Red repeat 7 authority audit confirmed that Contract repeat 4 changed
  only the one documentation-ownership passage. Existing evidence already
  exercises package scripts, test tiers, strict configuration, runtime floors,
  build output, and package inventory. Exhaustive stale-authority search found
  no residue, and no executable expectation or test gap remains.
- `bun run check` passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime lanes passed 11 Node.js cases, 9 Bun cases, and 9 Deno cases,
  including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 supported
  floors. All 25 frozen MVP fast tests passed.
- The Red repeat 7 build reproduced the single 190346-byte `dist/cli.mjs`
  artifact with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven paths,
  totaling 73144 packed bytes and 267010 unpacked bytes.
- Node syntax and Deno check passed. Repository and installable-payload index
  runs passed without a diff, both Open Forge Doctors reported no problem, and
  `git diff --check` passed. Frozen Red evidence, corrected production,
  baseline MVP and payloads, and Contract or current-truth integrity
  comparisons found no change beyond the Task record.
- The Red repeat 7 worktree and index finished clean with no upstream and no
  execution oddity.
- Green repeat 7 completed as a deliberate no-op at orchestration-state
  baseline `e770f1a`. It changed no production, test, or documentation path,
  created no phase code commit, and staged, committed, and pushed nothing.
- Green repeat 7 passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases with 25 filtered, 9 Bun cases
  with 27 filtered, and 9 Deno cases with 27 filtered, including the exact
  Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 supported-floor journeys. All 25
  frozen MVP fast tests passed.
- The Green repeat 7 build bundled 29 modules into the single 190346-byte
  `dist/cli.mjs` artifact with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  The npm dry run returned exactly one report with the accepted seven files,
  totaling 73144 packed bytes and 267010 unpacked bytes.
- Node syntax and Deno checks passed. Four Node artifact probes passed: help,
  human version, and JSON version exited 0, while the structured unknown-command
  parse result exited 2. Repository and installable-payload index runs produced
  no diff, both requested Open Forge Doctors reported no problem, and worktree,
  cached, and `git diff --check` validations passed.
- Corrected production and evidence match `cd5e6fb`, Contract repeat 4 matches
  `3e04529`, frozen Red repeat 7 matches `a5db8b2`, and the baseline MVP and
  Framework or Extension payloads remain unchanged. The worktree and index
  finished clean.
- Green repeat 7 observed a pre-existing low-severity formatting defect in
  `.agents/memory/crystallized/documents/cli/interface.md`: the Help And Error
  Behavior table occupies lines 1409-1417, lines 1419-1422 are prose, and the
  `Blocked mutation` and `Attention` pipe rows are orphaned at lines 1423-1424.
  The semantics remain clear, and the defect does not affect production,
  frozen evidence, or Green correctness. It is not a Green blocker, but it
  requires Contract or phase Review disposition before final acceptance.
- Blue repeat 7 commit:
  `1337e92075fe6c8eec7946a009ec7ac5b62ea965` (`Remove the parser result
dependency cycle`).
- Blue repeat 7 changed exactly five production paths:
  `src/cli/parser/parse-issue.ts`,
  `src/cli/parser/parse-result-message.ts`,
  `src/cli/parser/parse-root-invocation.ts`,
  `src/cli/parser/parser-results.ts`, and
  `src/cli/parser/root-parse-issue.ts`. It moved parse-issue declarations to
  their focused owner and reduced the sole runtime ESM cycle
  `parser-results.ts -> parse-result-message.ts -> parser-results.ts` to zero.
  Existing imports from and exports of `parser-results.ts` remain valid.
- Blue repeat 7 changed no test, fixture, snapshot, current document, Contract,
  or Task path. The phase agent staged, committed, and pushed nothing; the
  orchestrator created the one production commit after verification.
- Focused parser evidence passed 33 tests. The complete check passed 49 direct,
  8 integration, and 36 end-to-end tests. Serial runtime filters passed 11
  Node.js, 9 Bun, and 9 Deno cases, including the exact Node.js 22.12.0, Bun
  1.3.0, and Deno 2.8.0 supported floors. All 25 frozen MVP fast tests passed.
- Node syntax, Deno check, the Foundation artifact probes, both repository and
  installable-payload index runs, both Open Forge Doctors, protected-surface
  integrity comparisons, worktree and cached-diff inspection, and
  `git diff --check` passed.
- Before Blue, the build bundled 29 modules into a 190346-byte `dist/cli.mjs`
  artifact with SHA-256
  `63dda2fc7740c3b188592c2013bf606fb38fdc4477440ad524c01843cf48c066`.
  After the focused module split, it bundles 30 modules into the single
  190478-byte artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
- The npm dry run returned exactly one report with the accepted seven files,
  totaling 73172 packed bytes and 267142 unpacked bytes.
- Blue repeat 7 confirmed the pre-existing Help And Error Behavior orphan rows
  as a low clerical current-document defect and found exact Bun subprocess and
  failure-function duplication in `src/cli/testing/run-tests.ts:42-57` and
  `src/cli/testing/update-snapshot.ts:37-52`. The Workflow returned first to
  Contract repeat 5 for the semantic-neutral row move and now runs Red repeat
  8, Green repeat 8, and Blue repeat 8 verification before Purple repeat 1
  owns the test-support correction. Phase Review repeat 7 was superseded
  before start; fresh phase Review repeat 8 and repeated Whole-Task Acceptance
  remain pending.
- Contract repeat 5 commit:
  `64481098ddaa3682010fe6687d6cba62fa1b7a1f` (`Repair the CLI behavior table`).
- Contract repeat 5 changed exactly one current document,
  `.agents/memory/crystallized/documents/cli/interface.md`, with two additions
  and two deletions. It moved the unchanged `Blocked mutation` and `Attention`
  rows into the existing Help And Error Behavior table without changing their
  wording, meaning, order, or the callable surface.
- Contract repeat 5 passed the documentation-link audit, both repository and
  installable-payload index runs, both Open Forge Doctors, the exact-diff scope
  audit, and `git diff --check`. It introduced no new question. Red repeat 8
  subsequently completed as a deliberate no-op because Contract semantics and
  the callable surface did not change.
- Red repeat 8 completed as a deliberate no-op at orchestration-state baseline
  `8ff5813`. It changed no expectation, test, fixture, snapshot, production,
  or documentation path and created no phase code commit.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. The retained MVP fast suite passed 25 tests, and its
  closure suite passed 162 tests.
- The Red repeat 8 build reproduced the single 190478-byte `dist/cli.mjs`
  artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned exactly one report with the accepted seven files,
  totaling 73172 packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, both repository and installable-payload index runs,
  both Open Forge Doctors, Foundation probes, protected-surface integrity
  comparisons, and `git diff --check` passed. Red repeat 8 found no new
  observation, oddity, mismatch, or unconformity.
- Green repeat 8 completed as a deliberate no-op at orchestration-state
  baseline `f58c936`. Production at `1337e92` remains the smallest correct
  implementation, and the parser's public re-exports remain preserved. No
  production, expectation, test, fixture, snapshot, or documentation change
  was warranted.
- The structural audit counted 25 production modules and 45 runtime edges with
  zero dependency cycles. The type-only import audit found zero violations,
  and no production file exceeds 200 physical lines.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. All 25 retained MVP fast tests passed. The 162-test MVP
  closure result was inherited from unchanged Red repeat 8 and proportionately
  omitted from this production-only no-op verification.
- The Green repeat 8 build reproduced the single 190478-byte `dist/cli.mjs`
  artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned exactly the accepted seven files, totaling 73172
  packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, Foundation probes, both repository and
  installable-payload index runs, both Open Forge Doctors, protected-surface
  integrity comparisons, and `git diff --check` passed. Green repeat 8 found
  no new observation, oddity, mismatch, or unconformity.
- Blue repeat 8 completed as a deliberate no-op at pre-observation
  orchestration-state baseline `b573512`. Production remains at `1337e92`;
  the structural audit found no production issue and warranted no change.
- The audit counted 25 production modules and 45 runtime edges with zero
  dependency cycles and zero missing type-only imports. Twenty-two production
  modules are at most 100 physical lines, three are 101–200 lines, and none
  exceed 200 lines.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. All 25 retained MVP fast tests passed. The 162-test MVP
  closure result was inherited from unchanged Red repeat 8.
- The Blue repeat 8 build reproduced the single 190478-byte `dist/cli.mjs`
  artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned exactly the accepted seven files, totaling 73172
  packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, Foundation probes, both repository and
  installable-payload index runs, both Open Forge Doctors, protected-surface
  integrity comparisons, and `git diff --check` passed.
- During Blue repeat 8 verification, PowerShell `if (git diff --quiet)` tested
  the command's empty output rather than its exit status and produced a false
  index-drift alarm. Immediate status, `.agents` diff, stat, and
  `git diff --check` inspection proved the repository clean. This was local
  verification-script behavior, not repository drift; silent command guards
  must inspect explicit `$LASTEXITCODE` instead.
- Purple repeat 1 commit:
  `1fc7047` (`Consolidate CLI test script support`).
- Purple repeat 1 added exactly `src/cli/testing/testing-script.ts` and changed
  exactly `src/cli/testing/run-tests.ts` and
  `src/cli/testing/update-snapshot.ts`. The shared test-only owner reduced the
  duplicated `Bun.spawn` implementation from two copies to one and the
  duplicated `throw new Error(message)` failure function from two copies to
  one. The fixture-local failure helper remains separate at its narrower
  scope.
- The refactor changed no behavior, Contract, production, expectation,
  fixture, or snapshot. Both before and after it, the complete check passed 49
  direct, 8 integration, and 36 end-to-end tests. Two focused test-runner cases
  and five snapshot integration cases passed, and an invalid tier retained
  exit status 1.
- Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. All 25 retained MVP fast tests passed.
- The Purple repeat 1 build remained byte-identical as the single 190478-byte
  `dist/cli.mjs` artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned exactly the accepted seven files, totaling 73172
  packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, both repository and installable-payload index runs,
  both Open Forge Doctors, protected-surface integrity comparisons, and
  `git diff --check` passed. Purple repeat 1 found no new observation beyond
  the already-recorded loader finding.
- Phase Review repeat 8 accepted the Contract-through-Purple chain at clean
  orchestration state `728c511`. The independent read-only review found no
  blocking, residual, optional, or phase-invalidating finding and invalidated
  no completed phase.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. The retained MVP fast and closure suites passed 25 and 162
  tests respectively.
- The review build reproduced the single 190478-byte `dist/cli.mjs` artifact
  with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned one report with exactly the accepted seven files,
  totaling 73172 packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, Foundation probes, both Open Forge Doctors,
  disposable-copy repository and installable-payload index runs,
  protected-surface integrity comparisons, commit-history inspection, and
  `git diff --check` passed.
- The structural review confirmed 25 production modules with no dependency
  cycle, no module over 200 physical lines, and no missing type-only import.
  Test declarations conform to the accepted explicit direct and integration
  policy and the generated, runtime-tagged end-to-end matrix exception. Purple
  repeat 1 remains an exact behavior-preserving test-support refactor.
- Current documentation matches the implemented Foundation candidate. Commit
  subjects use ordinary descriptive language and contain no co-author trailer.
  The known `load --bodies` gap remains pending only within its scoped future
  work and does not block Foundation acceptance. Durable Purple-phase and
  TypeScript-convention workflow promotion and the repository-wide Prettier
  baseline remain queued after Foundation.
- Phase Review repeat 8 found no new observation, oddity, mismatch, or
  unconformity. Whole-Task Acceptance repeat is authorized as a fresh,
  independent whole-Task review.
- Whole-Task Acceptance repeat rejected the Task from Contract at clean review
  state `1281ef2`. The Task is not accepted or ready to close. The broader
  whole-Task scan disagrees with Phase Review repeat 8 because it inspected the
  complete current Contract and test tree beyond the narrower repeated-phase
  change surfaces.
- Final blocking category 1 was Contract ownership: the current interface
  document retained future-transfer language, manually copied five implemented
  declaration blocks, and did not truthfully distinguish two future schematic
  examples from source-owned declarations.
- Contract repeat 6 commit:
  `32450ac` (`Align CLI interface with source-owned contracts`).
- Contract repeat 6 changed only
  `.agents/memory/crystallized/documents/cli/interface.md`, with 38 additions
  and 109 deletions. It removed the five implemented declaration blocks and
  linked their exact owners in `src/cli/result/result.ts`,
  `src/cli/result/workspace-reference.ts`, and
  `src/cli/result/process-completion.ts`. Ownership is now stated in present
  tense.
- The correction preserved schema version 1, all statuses and message levels,
  all 23 operation identifiers, workspace-selection values, result fields, and
  exit codes. `FrameworkPresenceState` and `DoctorMessageCode` are now
  explicitly schematic. No value, meaning, semantic behavior, or callable
  surface changed.
- Source-derived declaration audit, documentation links, stale-language
  search, both repository and installable-payload index runs, both Open Forge
  Doctors, and `git diff --check` passed. No question arose.
- During Contract repeat 6 verification, the first link-audit wrapper
  mistakenly invoked a missing nested `powershell` executable from the
  already-running PowerShell host. It did not run or change state. The direct
  rerun in the current PowerShell host succeeded; this was transient
  verification orchestration, not repository behavior.
- Red repeat 9 completed as a deliberate no-op at orchestration-state baseline
  `c28091a`. The source-derived semantic inventory preserved schema version 1,
  all statuses and message levels, all 23 operation identifiers,
  workspace-selection values, result fields, and exit codes. Test declarations
  remain explicit except for the accepted generated, runtime-tagged end-to-end
  matrix. No expectation, test, fixture, snapshot, production, or Contract
  change was warranted.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. All 25 retained MVP fast tests passed; the 162-test MVP
  closure result was inherited from unchanged earlier evidence.
- The Red repeat 9 build reproduced the single 190478-byte `dist/cli.mjs`
  artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned exactly the accepted seven files, totaling 73172
  packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, Foundation probes, both Open Forge Doctors,
  disposable-copy repository and installable-payload index runs,
  protected-surface integrity comparisons, and `git diff --check` passed.
- During Red repeat 9 verification, `git fsck --no-progress` exited 0 with no
  corruption but listed many dangling commits, trees, and blobs in the shared
  object database, consistent with the iterative, reset, and review history.
  References, worktree, index, and evidence are unaffected. This is
  repository-maintenance residue only; this Task will not run GC or prune.
- Green repeat 9 completed as a deliberate no-op at orchestration-state
  baseline `b3690ea`. Production remains byte-identical at `1337e92`; no
  production, expectation, test, fixture, snapshot, or Contract change was
  warranted.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. All 25 retained MVP fast tests passed; the 162-test MVP
  closure result was inherited from unchanged Red repeat 9 evidence.
- The Green repeat 9 build reproduced the single 190478-byte `dist/cli.mjs`
  artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`.
  The npm dry run returned one report with exactly the accepted seven files,
  totaling 73172 packed bytes and 267142 unpacked bytes.
- Node syntax, Deno check, Foundation probes, disposable-copy repository and
  installable-payload index runs with 318 and 21 entries respectively, both
  Open Forge Doctors, protected-surface integrity comparisons, and
  `git diff --check` passed. The runtime graph remains 25 production modules
  and 45 runtime edges with zero cycles, zero type-only import violations, and
  no module over 200 physical lines.
- Parser public re-exports, guarded final writing, parser and display-output
  behavior, truthful suggestions, and accepted single-dash handling remain
  correct. Green repeat 9 found no new observation, oddity, mismatch, or
  unconformity.
- Final blocking category 2 was Blue-to-Purple parser controls. Blue repeat 9
  commit `d3f6d02` changed exactly
  `src/cli/parser/root-token-analysis.ts`. The exported `RootTokenControl` now
  owns one raw dash, derives the long-option prefix, and aliases the option
  terminator; four production sites consume it and no raw `"--"` remains in
  production. Behavior, Contract, and tests are unchanged.
- Before and after Blue repeat 9, the complete check passed 49 direct, 8
  integration, and 36 end-to-end tests. Serial runtime filters passed 11
  Node.js cases, 9 Bun cases, and 9 Deno cases, including the exact Node.js
  22.12.0, Bun 1.3.0, and Deno 2.8.0 supported floors. All 25 retained MVP fast
  tests passed.
- The artifact changed from 190478 bytes with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`
  to 190799 bytes with SHA-256
  `1dc8e1aa7a301914db6c4b456418c5600900b3e281e48544f9d94ed956f770a3`,
  a 321-byte increase from the focused named owner. The npm dry run still
  returned exactly the accepted seven files, now totaling 73246 packed bytes
  and 267463 unpacked bytes.
- Node syntax, Deno check, Foundation probes, both repository and
  installable-payload index runs, both Open Forge Doctors, `git fsck`,
  protected-surface integrity comparisons, and `git diff --check` passed. The
  runtime graph remains 25 production modules and 45 runtime edges with zero
  cycles. Blue repeat 9 found no new observation, oddity, mismatch, or
  unconformity.
- Purple repeat 2 commit:
  `8fa61d5` (`Consolidate CLI test fixtures and support`).
- Purple repeat 2 changed exactly 14 test or test-support files:
  `src/cli/build/build-entrypoint.integration.test.ts`,
  `src/cli/build/generate-build-module.integration.test.ts`,
  `src/cli/e2e/distribution.e2e.test.ts`,
  `src/cli/e2e/final-stream-failure.e2e.test.ts`,
  `src/cli/e2e/foundation.e2e.test.ts`,
  `src/cli/e2e/minimum-runtimes.e2e.test.ts`,
  `src/cli/e2e/supported-runtime-fixture.ts`,
  `src/cli/foundation-elimination.integration.test.ts`,
  `src/cli/parser/root-token-boundaries.test.ts`,
  `src/cli/testing/fixtures/final-stream-failure-shim.ts`,
  `src/cli/testing/run-tests.ts`, `src/cli/testing/testing-script.ts`,
  `src/cli/testing/update-snapshot.integration.test.ts`, and
  `src/cli/testing/update-snapshot.ts`. It added exactly
  `src/cli/testing/fixtures/final-stream-failure.ts` and
  `src/cli/testing/testing-paths.ts`, for 16 test-only paths total.
- The behavior-preserving consolidation reduced arbitrary `"stats"` copies
  from 7 to 0, repository-root computations from 6 to 1, CLI-root computations
  from 3 to 1, SHA-256 owners from 2 to 1, minimum-runtime tag literals from 3
  to 0 through `RuntimeId`, `npx` executable copies from 2 to 1, `--yes` copies
  from 3 to 1, arbitrary `"workspace"` copies from 8 to 1, and the raw direct
  test terminator from 1 to 0 through `RootTokenControl.optionTerminator`.
  Final-stream delivery discriminants fell from 21 to 4 definitions, and
  stream discriminants fell from 20 to 2.
- `testing-paths.ts` uses portable typed `import.meta.url` with
  `fileURLToPath`. The focused final-stream fixture owns runtime values, types,
  guards, and its shim path. Boundary-local `isRecord` guards remain local,
  and the distinct `expectJson` variants are intentional.
- The first-pass Bun-only path derivation and stale distribution consumer were
  corrected before completion. They were Purple-local refactor omissions, not
  product, Contract, behavior, or expectation defects.
- The complete check passed 49 direct, 8 integration, and 36 end-to-end tests.
  Serial runtime filters passed 11 Node.js cases, 9 Bun cases, and 9 Deno
  cases, including the exact Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0
  supported floors. All 25 retained MVP fast tests passed.
- The Purple repeat 2 artifact remained byte-identical at 190799 bytes with
  SHA-256
  `1dc8e1aa7a301914db6c4b456418c5600900b3e281e48544f9d94ed956f770a3`.
  The npm dry run returned exactly the accepted seven files, totaling 73246
  packed bytes and 267463 unpacked bytes.
- Node syntax, Deno check, Foundation probes, repository and
  installable-payload index runs with 318 and 23 entries respectively, both
  Open Forge Doctors, protected-surface integrity comparisons, and
  `git diff --check` passed. Purple repeat 2 found no new issue, observation,
  oddity, mismatch, or unconformity.
- Phase Review repeat 9 rejected the corrective chain from Red. Contract repeat
  6 remains accepted and valid; Whole-Task Acceptance repeat 2 is not
  authorized.
- Final blocking category 4 is behavior-neutral Purple test ownership and
  helper consolidation. `src/cli/foundation-elimination.integration.test.ts:33`
  restates the production-owned `.test.ts` suffix instead of using
  `TestFileSuffix[TestTier.direct]`. Ordinary end-to-end setup in the
  foundation, minimum-runtime, and final-stream evidence also repeats
  `--version` and `--json` instead of consuming their production flag owners.
- The Tiered Test Pattern raw-wire exception applies only to a named
  built-boundary assertion, not ordinary test setup. Purple is the earliest
  affected phase for this category, which Purple repeat 3 must correct.
- The exact additional Purple companion inventory includes identical direct
  test `invoke(argv) -> main(createFoundationCliInput(argv))` helpers at
  `main.test.ts:154`, `root-token-boundaries.test.ts:172`,
  `single-dash-boundaries.test.ts:82`, `suggestion-boundaries.test.ts:120`, and
  `workspace-empty-value.test.ts:65`; these belong in the shared Foundation
  fixture. `foundation.e2e.test.ts:105-109` and
  `final-stream-failure.e2e.test.ts:139-149` duplicate the undefined-runtime
  guard.
- `supported-runtime-fixture.ts:27` owns `candidateArtifactPath`, while
  `foundation.e2e.test.ts:17` rebuilds that path and
  `distribution.e2e.test.ts:21` rebuilds its `dist` parent.
  `update-snapshot.integration.test.ts:9-15,49-50` rebuilds `src/cli` instead
  of consuming `cliRoot`. Deno `run --quiet` prefixes are duplicated between
  normal and shim arrays in `supported-runtime-fixture.ts:51-57` and
  `minimum-runtimes.e2e.test.ts:58-59`.
- Four tests also compose identical Bun-script execution through
  `runProcess(process.execPath, ["run", ...], cwd)`:
  `build-entrypoint.integration.test.ts:25`,
  `generate-build-module.integration.test.ts:35-41`,
  `update-snapshot.integration.test.ts:73-75`, and the frozen-reference journey
  in `foundation.e2e.test.ts:91-96`. One test-only `runBunScript` equivalent
  beside `runProcess` is therefore a demonstrated shared capability.
- These companion corrections are behavior-neutral Purple work. The three
  boundary-local `isRecord` guards remain local, and the two semantically
  different `expectJson` helpers remain separate. The obvious exact
  helper-clone inventory is complete and forms the final Purple repeat 3 scope.
- Final blocking category 2 at Phase Review repeat 9 was unified production
  root metadata, whose earliest affected phase was Blue, followed by Purple.
  At discovery, `root-command-metadata.ts:4-10` repeated direct root command
  tokens already owned by `OperationId`; `root-help.ts:19-21,31-33` restated the
  `open-forge [command] [flags]` usage, help, version, direct operations, and
  JSON flag; `parse-root-invocation.ts:25` repeated the usage; and
  `command.ts:19` duplicated the root summary from `root-help.ts:17`.
- The Named Values and predictability requirements call for one focused
  production root-metadata owner consumed by registration, parsing, and help.
  Blue repeat 10 completed that production correction; Purple repeat 3 must
  import those owners in tests.
- Final blocking category 1 is the parser-result expectation and behavior.
  The accepted Parser Result Contract requires `CliParseData.usage` to contain
  the complete valid invocation shapes for the nearest parser path.
  `root-help.ts:18-22` owns three root shapes, but root parse failures use a
  separate one-element `RootUsage` at
  `parse-root-invocation.ts:25,44`. Direct tests import and freeze only
  `createRootHelpData().usage[0]`, so they miss this divergence.
- Red repeat 10 is the earliest required phase for the missing expectation,
  followed by Green repeat 10, Blue repeat 10, and Purple repeat 3 for
  implementation structure and test ownership. The Contract is explicit, so
  no user decision is required.
- Red repeat 10's initial delta correctly made behavior assertions expect all
  three root usage shapes, but independent review found its shared expectation
  `FoundationRootUsage = createRootHelpData().usage` would become vacuous after
  Green and Blue unify the production owner: deleting or reordering a shape
  could change actual and expected values together.
- Red repeat 10 corrected the vacuity blocker and completed at commit
  `e4dba63` (`Freeze complete root parser usage expectations`). The accepted
  delta changed seven existing Red-owned test or fixture files:
  `src/cli/parser/parser-results.test.ts`,
  `src/cli/parser/root-token-boundaries.test.ts`,
  `src/cli/parser/single-dash-boundaries.test.ts`,
  `src/cli/parser/suggestion-boundaries.test.ts`,
  `src/cli/parser/workspace-empty-value.test.ts`,
  `src/cli/presentation/complete-cli-result.test.ts`, and
  `src/cli/testing/fixtures/foundation-cli-input.ts`; it added
  `src/cli/parser/root-help.test.ts` as the eighth Red-owned path.
- All 14 shared-fixture behavior sites and the raw parse-data case now require
  all three root usage shapes. The new focused test independently freezes the
  ordered three-shape Contract inventory. It uses the scoped exact-syntax
  exception only where needed and consumes `GlobalFlag` for help and version,
  so production deletion or reordering cannot change actual and expected
  collections together.
- The isolated independent inventory passed 1 test. The targeted parser and
  presentation evidence produced 10 passes and the expected 23 intentional
  failures across 33 tests; the complete direct tier produced 27 passes and the
  same 23 intentional failures across 50 tests. Every failure was only the
  missing help and version usage shapes. Typecheck, typed lint, and
  `git diff --check` passed.
- Red repeat 10 changed no production, Contract, configuration, or snapshot.
  Independent review accepted the corrected delta with no remaining Red
  finding.
- Green repeat 10 commit:
  `c69e999` (`Return complete usage for root parser failures`). It changed
  exactly `src/cli/parser/parse-root-invocation.ts`, with five additions and two
  deletions. It removed the incomplete `RootUsage` collection and made the
  invalid-result funnel consume `createRootHelpData().usage`. No import edge
  changed, and the delta contains no Blue refactor.
- The targeted evidence passed all 33 tests. `bun run check` passed 50 direct,
  8 integration, and 36 end-to-end tests, including the supported runtime
  floors at Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0. All 25 retained MVP
  fast tests passed. Ten focused built-Foundation end-to-end cases passed
  across Node.js, Bun, and Deno.
- Human and JSON failure probes produced the exact exit status 2, expected
  stream selection, and all three valid usage shapes. The artifact is now
  190780 bytes with SHA-256
  `148e24ca5b7af2af964923892805e3e4d798c35d8c789cd71c16b6013246dad6`.
  The npm dry run returned exactly the accepted seven files, totaling 73238
  packed bytes and 267444 unpacked bytes.
- Node syntax, Deno check, strict typecheck, typed lint,
  protected-surface-delta inspection, and `git diff --check` passed.
  Independent review accepted Green repeat 10 with no finding. It produced no
  new observation, oddity, mismatch, or unconformity.
- Blue repeat 10 commit:
  `18cf026` (`Unify root command metadata ownership`). It changed exactly six
  production files: `src/cli/command.ts`,
  `src/cli/parser/command-suggestions.ts`,
  `src/cli/parser/parse-root-invocation.ts`,
  `src/cli/parser/root-command-metadata.ts`,
  `src/cli/parser/root-help.ts`, and
  `src/cli/parser/root-parse-issue.ts`.
- `RootCommandDefinition` in `root-command-metadata.ts` is now the unified
  owner for the executable name, root summary, usage, commands, and examples.
  Registration, parsing, suggestions, and help consume that owner. Direct root
  token copies fell from 7 to 0, raw root invocation forms from 6 to 0, root
  summaries from 2 to 1, and the raw parser long-option prefix from 1 to 0.
  The compatibility alias `RootCommandMetadata` is the same array reference as
  `RootCommandDefinition.commands`.
- The `BuildIdentity.name` versus fixed executable edge remains resolved by
  the accepted parser-results, interface, and development-toolchain semantics:
  the distributed package, binary, root grammar, and installed command are
  explicitly `open-forge`.
- `BuildIdentity` remains injected for version-result data and fixtures, not as
  an alternate executable-name Contract. Command registration may therefore
  consume the metadata-owned executable name without an undocumented product
  choice. Root token control remains algebraic: one raw option prefix derives
  the long-option prefix, and the option terminator aliases that derived value.
- Blue repeat 10's owner exposes concrete Purple repeat 3 imports; this
  is not an additional Blue blocker. `src/cli/command.test.ts:26` currently
  compares the registered name to `FoundationBuildIdentity.name`, and
  `src/cli/parser/suggestion-boundaries.test.ts:41` restates a raw root-summary
  fragment. Purple repeat 3 must consume the new root metadata owner.
- `src/cli/testing/fixtures/foundation-cli-input.ts:9` retains the compact
  fixture literal `open-forge`. Purple repeat 3 must decide it within the
  existing fixture-deduplication scope, likely deriving it from
  `RootCommandDefinition.executable` when behavior remains neutral. The
  independent root-help Contract inventory remains an intentional raw-syntax
  exception; result and global-flag definition inventories also remain
  intentional.
- Final blocking category 3 at Phase Review repeat 9 was the missed
  `RootTokenControl` production consumer. At discovery,
  `src/cli/parser/root-parse-issue.ts:84` then constructed corrections with the
  raw template `` `--${token.slice(1)}` `` instead of consuming
  `RootTokenControl.longOptionPrefix`. Blue repeat 9 correctly established the
  owner, but its recorded claim that the owner covered all production
  long-option control sites was false and overstated because this consumer was
  missed.
- Blue repeat 10 corrected this consumer through
  `RootTokenControl.longOptionPrefix`. Existing single-dash expectations and
  behavior remain protected; Purple repeat 3 only imports the production owner
  where test setup restates the control.
- Focused Blue evidence passed 31 tests. The complete check passed 50 direct,
  8 integration, and 36 end-to-end tests. Serial runtime filters passed 11
  Node.js cases, 9 Bun cases, and 9 Deno cases, including the exact Node.js
  22.12.0, Bun 1.3.0, and Deno 2.8.0 floors. Retained MVP fast and closure
  suites passed 25 and 162 tests.
- The Blue repeat 10 artifact is 191296 bytes with SHA-256
  `5e7d65130b0a42646fd6e537c82ea8bf37c8ae77b1661ef9b8ce471676811e29`.
  The npm dry run returned exactly the accepted seven files, totaling 73321
  packed bytes and 267960 unpacked bytes.
- Node syntax, Deno check, Foundation probes, and `git diff --check` passed.
  The runtime graph contains 25 production modules and 49 runtime edges with
  zero cycles; the type-only import audit found zero violations, and none of
  the six changed production files exceeds 92 physical lines. Independent
  xhigh review accepted Blue repeat 10 with no finding or new observation.
- Purple repeat 3 was authorized for the already recorded behavior-neutral
  test-tree follow-ons using the new production owners; its later strict audit
  paused the preserved delta and returned the chain to Blue repeat 11 as
  recorded below.
- Purple repeat 3 commit `c66c7ad` (`Consolidate CLI test support ownership`)
  completed the frozen scope across exactly 17 test or test-support paths, with
  130 additions and 104 deletions:
  `src/cli/build/build-entrypoint.integration.test.ts`,
  `src/cli/build/generate-build-module.integration.test.ts`,
  `src/cli/command.test.ts`,
  `src/cli/foundation-elimination.integration.test.ts`,
  `src/cli/main.test.ts`, `src/cli/parser/root-token-boundaries.test.ts`,
  `src/cli/parser/single-dash-boundaries.test.ts`,
  `src/cli/parser/suggestion-boundaries.test.ts`,
  `src/cli/parser/workspace-empty-value.test.ts`,
  `src/cli/e2e/distribution.e2e.test.ts`,
  `src/cli/e2e/final-stream-failure.e2e.test.ts`,
  `src/cli/e2e/foundation.e2e.test.ts`,
  `src/cli/e2e/minimum-runtimes.e2e.test.ts`,
  `src/cli/e2e/supported-runtime-fixture.ts`,
  `src/cli/testing/update-snapshot.integration.test.ts`,
  `src/cli/testing/fixtures/foundation-cli-input.ts`, and
  `src/cli/testing/run-cli-process.ts`. `testing-paths.ts` remains unchanged.
- Achieved reductions are: suffix copies 1 to 0; ordinary raw version and JSON
  controls 10 to 0 while retaining one named built-boundary raw-version
  assertion; direct invoke clones 5 to 0 through one owner; runtime guards 2 to
  0 through one owner; artifact or `dist` rederivations 2 to 0;
  update-snapshot `src/cli` rederivations 5 to 0 through `cliRoot`; Deno
  `run --quiet` tuples 4 to 1; Bun wrappers 4 to 0 through one `runBunScript`;
  ordinary executable or summary restatements 2 to 0; and the fixture's raw
  `open-forge` copy 1 to 0.
- The nearest owners are the Foundation fixture, `run-cli-process`, and the
  supported-runtime fixture. One deliberate test improvement replaces the
  suggestion test's raw leading-summary fragment with absence of the complete
  authoritative `RootCommandDefinition.summary`, preserving the intent while
  avoiding a brittle derived prefix. Independent review approved this
  improvement.
- Purple repeat 3's clean pre-edit baseline was orchestration state `bdca3a6`.
  Typecheck, typed lint, 50 direct tests, and 8 integration tests passed, but
  the unfiltered end-to-end lane twice crossed the existing 5-second per-test
  timeout: the npm dry-run package inventory reached 5010 milliseconds, and
  the first Deno final-stream callback reached 5007 milliseconds with the
  child closing late and exit code -1. Remaining Deno cases and all exact-floor
  minimum-runtime cases passed.
- This occurred before any Purple repeat 3 mutation and is timing/environment
  verification evidence, not introduced behavior. The baseline artifact was
  191296 bytes with SHA-256
  `5e7d65130b0a42646fd6e537c82ea8bf37c8ae77b1661ef9b8ce471676811e29`.
  Purple subsequently reran focused and complete serial evidence.
- During Purple repeat 3's strict audit, explicit
  `bunx eslint src/cli --rule '@typescript-eslint/consistent-type-imports:error'`
  found two pre-existing production violations in
  `src/cli/parser/parser-results.ts`. Lines 11-14 import `CliMessageCode` through
  a runtime import even though it is type-only locally, and line 66 uses the
  forbidden inline type
  `import("../result/result.ts").OperationId` even though `OperationId` is
  already imported at line 5 for runtime and type use.
- The configured lint does not enable this rule, so earlier zero type-only
  violation claims were false and overstated; they describe the limited prior
  audit, not current verified truth. Blue repeat 11 commit `d74c42e` (`Correct
CLI parser type imports`) changed exactly
  `src/cli/parser/parser-results.ts`, with two additions and five deletions. It
  uses `import type { CliMessageCode, CliParseIssue }` and the existing
  `OperationId` type while preserving the runtime re-export.
- Explicit strict-rule runs for `parser-results.ts` and all `src/cli` now report
  zero consistent-type-import violations. Production and test typechecks and
  `git diff --check` passed. The artifact remained byte-identical at 191296
  bytes with SHA-256
  `5e7d65130b0a42646fd6e537c82ea8bf37c8ae77b1661ef9b8ce471676811e29`.
  Independent high review accepted Blue repeat 11 with no finding.
- The correction changes no behavior or Contract and needs no user decision.
  Broad suites and frozen MVP evidence were proportionately not rerun. Purple
  repeat 3 then completed its preserved 17-path delta at `c66c7ad`. Permanent
  configured enforcement remains assigned to the next Workflow and
  TypeScript-conventions Task.
- Purple repeat 3's explicit strict consistent-type-import audit remained at
  zero violations. `bun run check` passed 50 direct, 8 integration, and 36
  end-to-end tests in 35.4 seconds. The pre-edit npm inventory and Deno callback
  timeout crossings did not recur.
- The artifact remained byte-identical at 191296 bytes with SHA-256
  `5e7d65130b0a42646fd6e537c82ea8bf37c8ae77b1661ef9b8ce471676811e29`.
  Frozen MVP evidence was not rerun, as explicitly decided. Independent xhigh
  review accepted Purple repeat 3 at or above the 99th-percentile material-
  readiness threshold with no material finding. The corrective cycle stops;
  cosmetic or speculative improvement must not create another phase cycle.
- Phase Review repeat 10 accepted the completed chain at the top-99th
  material-readiness threshold with no blocker. Contract repeat 6 remains
  accepted; Red repeat 10 has a nonvacuous independent inventory; Green repeat
  10 provides complete behavior; Blue repeats 10 and 11 own unified root
  metadata and strict type imports; and Purple repeat 3 completed the exact
  17-path test-support scope. The review created no code commit; commit
  `5f1cf44` records its accepted orchestration state.
- The authored TypeScript graph contains exactly one dynamic `import()`: an
  awaited, genuinely lazy end-to-end shim boundary. Inline TypeScript import
  type expressions number zero.
- `bun run check` passed 50 direct, 8 integration, and 36 end-to-end tests in
  32.1 seconds. Frozen MVP suites were not rerun; their trusted evidence remains
  25 fast and 162 closure tests. Node syntax, Deno check, both Open Forge
  Doctors, protected-surface integrity comparisons, the exact seven-file npm
  inventory, and `git diff --check` passed.
- The artifact remains 191296 bytes with SHA-256
  `5e7d65130b0a42646fd6e537c82ea8bf37c8ae77b1661ef9b8ce471676811e29`.
  The runtime graph remains 25 production modules and 49 runtime edges with
  zero cycles. Git integrity and history inspection passed. The reviewed Task
  branch state was clean, unpushed, had no upstream, and contained ordinary
  descriptive commits without co-author trailers.
- Later scoped work remains: permanent ESLint and dynamic-import guidance;
  durable Purple-phase and reasoning-level Workflow promotion; the
  repository-wide Prettier baseline; and implementation of `load --bodies`.
  Dangling Git objects remain recorded maintenance residue only.
- Whole-Task Acceptance repeat 2 accepted the clean `5f1cf44` candidate under
  the same material-readiness stopping rule. The Foundation is ready to close
  and locally integrate; cosmetic or speculative improvement does not create
  another corrective cycle.
- The accepted branch is 86 commits ahead of immutable baseline
  `6d87744660c287188def80319400295552fa9f17`, remains unpushed with no upstream,
  and contains ordinary descriptive commit messages without co-author trailers.
  Protected MVP, Framework, and Extension payload trees are unchanged.
- Acceptance retained the Phase Review evidence of 50 direct, 8 integration,
  and 36 end-to-end tests without rerunning the frozen MVP suite. The single
  artifact is 191296 bytes with SHA-256
  `5e7d65130b0a42646fd6e537c82ea8bf37c8ae77b1661ef9b8ce471676811e29`;
  npm reports exactly seven files totaling 73321 packed and 267960 unpacked
  bytes. Git integrity, diff hygiene, documentation, Task state, backlog, and
  Observation records passed acceptance.
- Accepted residual work is routed rather than blocking Foundation: the next
  just-in-time Task owns optional Purple-phase promotion, reasoning calibration,
  ESLint type-import enforcement, and async-lazy-only dynamic imports; a
  separate following Task owns the repository-wide Prettier baseline;
  `load --bodies` remains assigned to the later context/loading slice. Recorded
  parallel-build contention and timeout crossings remain nonrecurring
  orchestration risks. Dangling Git objects remain maintenance residue only.
- This acceptance-state commit records the final clerical acceptance state and
  archive move. Nothing is pushed.
- Independent Phase Review repeat 9 evidence remained green: `bun run check`
  passed 49 direct, 8 integration, and 36 end-to-end tests; serial runtime
  filters passed 11 Node.js, 9 Bun, and 9 Deno cases, including the exact
  Node.js 22.12.0, Bun 1.3.0, and Deno 2.8.0 floors; and retained MVP fast and
  closure suites passed 25 and 162 tests. The current artifact is 190799 bytes
  with SHA-256
  `1dc8e1aa7a301914db6c4b456418c5600900b3e281e48544f9d94ed956f770a3`,
  and npm returned exactly seven files totaling 73246 packed bytes and 267463
  unpacked bytes.
- Node syntax, Deno check, both Open Forge Doctors, disposable-copy repository
  and installable-payload index runs, protected-surface integrity comparisons,
  `git diff --check`, type-only import audit, and Git integrity passed. The
  runtime graph remains 25 production modules and 45 runtime edges with zero
  cycles. Phase Review repeat 9 found no other defect or question.
- The known `load --bodies` limitation and dangling Git objects remain scoped
  observations only. `git fsck --no-progress` reports no corruption, and refs,
  history, worktree, index, and evidence remain intact; no GC or prune belongs
  in this Task.
- For the historical rejected Whole-Task Acceptance repeat, Contract was the
  earliest invalidated phase. Its planned corrective sequence was Contract
  repeat 6, Red repeat 9, Green repeat 9, Blue repeat 9, Purple repeat 2, Phase
  Review repeat 9, and Whole-Task Acceptance repeat 2.
- The following figures are historical evidence from that rejected repeat, not
  current candidate truth: the complete check passed 49 direct,
  8 integration, and 36 end-to-end tests; serial runtime filters passed 11
  Node.js, 9 Bun, and 9 Deno cases at the exact supported floors; and retained
  MVP fast and closure suites passed 25 and 162 tests. The build reproduced the
  single 190478-byte `dist/cli.mjs` artifact with SHA-256
  `f8edb2ae43a7199b1c7a763dcf0dabec63037287e69552ae5d839507b72b544d`,
  and npm returned one report with exactly seven files, 73172 packed bytes, and
  267142 unpacked bytes.
- In that historical rejected repeat, Node syntax, Deno check, Foundation
  probes, both Open Forge Doctors, protected-surface integrity comparisons,
  runtime-graph inspection, Git `fsck`, commit-history inspection, and
  `git diff --check` passed. The
  runtime graph contains 25 production modules and 45 runtime edges with zero
  cycles; no production module exceeds 200 physical lines, and the type-only
  import audit found zero violations. The disposable-copy repository and
  installable-payload index runs remained byte-identical. The first wrapper was
  blocked before execution by shell
  safety policy and created no state; an explicit verified operating-system
  temp retry with .NET cleanup succeeded and left the target absent.
- That historical reviewer found no other defect. The known `load --bodies`
  gap remains pending only in its already scoped future work and is not part of
  these Foundation rejection categories.
- Phase Review repeat 9 identified and corrected the orchestration-record
  contradiction between that historical block and the current review. Its
  rejection is Red-first because of the missing complete-usage expectation.
  Current candidate evidence is the 190799-byte `dist/cli.mjs` artifact with
  SHA-256
  `1dc8e1aa7a301914db6c4b456418c5600900b3e281e48544f9d94ed956f770a3`
  and the exact seven-file npm payload totaling 73246 packed bytes and 267463
  unpacked bytes.
- On 2026-08-03, the user accepted Purple as the active Task's test-refactor
  and improvement phase after Blue. Purple may change only tests, fixtures,
  snapshots, and test-only helpers while preserving Contract, production,
  accepted expectations, and observable behavior. Missing or incorrect
  expectations return to Red.
- The same accepted temporary direction requires `import type` for type-only
  imports and reserves `*.types.ts` for modules containing only types.
  `src/cli/parser/parse-issue.ts` remains correctly named because its
  discriminant constants are runtime values. Grounded observations must be
  written eagerly at discovery; a phase without write authority reports them
  immediately for orchestrator recording rather than deferring them to its
  handoff.
- The accepted post-Foundation sequence first locally squash-integrates this
  Task branch onto `feature/cli-overhaul` without pushing, then starts a new
  just-in-time Task and branch for permanent type-import enforcement and
  async-lazy-only dynamic-import guidance. A separate repository-wide
  Prettier-baseline Task follows using `useTabs: false`, `tabWidth: 2`,
  `trailingComma: "all"`, `semi: true`, `singleQuote: false`,
  `printWidth: 100`, and `endOfLine: "lf"`. Foundation contains no formatting
  sweep.
- During the mandatory post-compaction Open Forge refresh,
  `bun run open-forge load --bodies` failed with `Unknown option "--bodies"`
  and usage limited to `open-forge [command] [flags]`. This is expected
  brownfield dogfooding evidence of the deliberately incomplete Foundation,
  not a new Foundation gap; the plain routed files remain authoritative.
- Commit messages use ordinary descriptive language without Conventional
  Commit prefixes or co-author trailers.

## Completion

- Contract, Red, Green, Blue, Purple, and phase Review satisfied their Workflow
  completion conditions through separate agents.
- A separate fresh agent accepted the complete Task after Phase Review repeat
  10 at clean branch state `5f1cf44`.
- Strict type checking, linting, replacement tests, trusted retained MVP
  evidence, build verification, and the applicable runtime matrix passed.
- The replacement artifact exposes only accepted Foundation behavior and no
  fabricated domain implementation.
- This archived Task preserves every accepted commit, evidence result,
  residual risk, and final acceptance state. Nothing was pushed.
