---
open-forge:
  description: Replace generic parser rescans and terminal bypass gaps with pinned library facts and typed validation
  tags: [Memory, Working, CLI, Task, Generic, Parser, SystemCommandLine, Evidence, Contextual, Active]
---

# Remediate Parser And Standard Behavior

## Task State

- State: Complete through Purple commit `d445e20` on
  `feature/cli-generic-improvements`; focused acceptance and edge closure are
  recorded without consuming the parent branch's final full gate.
- Implementer: Mastermind. Bounded Gray, Red, Green, and review delegation is
  permitted only from the accepted phase predecessor.
- Parent: [Improve Generic CLI Structure](_generic-improvements.md).
- Pinned dependency: `System.CommandLine` 2.0.11.
- Task source: This file.
- Last updated: 2026-08-22.

## Baseline

Exact `develop`/branch start `bd5d280` with the parent Task's passing managed
Unit `549/549`, Integration `166/166`, and EndToEnd `36/36` beginning baseline.

## Outcome

A single parser invocation produces one library-owned typed result tree. The
parser owns option forms, occurrences, values, operands, and the `--` boundary.
Shared typed validation rejects missing scalar values and terminal modes combined
with domain operands or operation-local options. Route List retains only the
documented `CliDelimiterGuard` for its equals-only depth exception; it no longer
rescans raw arguments to parse or validate depth, and command bindings no longer
receive raw process arguments.

## Authority And Inputs

- [Global Flags Interface](../../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/shared/global-flags/behavior.md).
- [Route List Interface](../../../../crystallized/documents/cli/contracts/route/list/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/route/list/behavior.md).
- [Route Inspect Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).
- [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md),
  especially standard behavior, parser ownership, typed validation, and evidence.
- [CLI-EDGE-005](../../edge-cases.md#cli-edge-005--raw-lexical-option-edge) and
  [CLI-EDGE-007](../../edge-cases.md#cli-edge-007--terminal-modes-with-domain-input).
- [CLI Development Plan](../../plan.md) and
  [Checkpoint](../../../checkpoints/cli-development.md).
- Exact 2.0.11 `ParseResult`, `CommandResult`, `OptionResult`, `ArgumentResult`,
  `SymbolResult`, tokenizer, arity, and parser tests from the upstream v2.0.11 tag.

## Analysis And Accepted Plan

1. `System.CommandLine` normalizes spaced, equals, and colon long-option forms to
   the same typed option/value result. Public typed results do not retain delimiter
   spelling, original positions, or per-occurrence value grouping.
2. `OptionResult.Implicit`, `IdentifierTokenCount`, and `Tokens` expose explicit
   occurrence and value-count facts. `CommandResult.Children`, explicit
   `ArgumentResult` values, and `ParseResult.UnmatchedTokens` can enforce terminal
   composition without scanning raw strings.
3. An `ExactlyOne` scalar option may greedily consume a following option after an
   attached-empty spelling. `ZeroOrOne` plus typed explicit/no-value validation
   preserves the following global and rejects the empty value generally.
4. Global workspace/view custom callbacks and Route List's
   `ReadDepthSpelling`/exception catch are not retained architecture. Workspace
   stays `Option<string?>`; view stays `Option<CliView>` and uses the library's
   `AcceptOnlyFromAmong` validator for exact `compact`/`expanded` tokens. Both use
   `ZeroOrOne` plus shared explicit/no-value facts so a following global is not
   consumed. Depth uses the same facts and retains command-local finite/all value
   mapping.
5. The accepted final state removes `CliBindingParse.OriginalArguments`. Green
   first stops consuming that property while preserving the existing callable so
   frozen tests remain compilable. A separate post-Green callable migration then
   moves the materially changed record to
   `Shell/Composition/Models/CliBindingParse.cs` with namespace
   `OpenForge.Cli.Core.Shell.Composition.Models`, updates `CliCoreApplication`, the
   generic binding delegates, Route List/Inspect binding consumers, and direct
   Unit consumers, and removes the old declaration without a forwarding type.
6. Route Inspect's `Argument<string[]>` aggregation and command-local exactly-one
   cardinality remain: the parser owns token aggregation, while the accepted
   command contract requires zero/several subjects to become one typed invalid
   Inspect result. It is not a raw parser or terminal workaround.
7. POSIX bundling disabled and response-file replacement disabled remain explicit
   parser configuration; neither reparses a token or conflicts with accepted
   behavior.

### Accepted Delimiter Exception

Route List's current contract permits only
`--depth=<non-negative-integer|all>` and explicitly rejects alternate depth
spellings. Version 2.0.11 typed results cannot distinguish spaced, equals, and
colon delimiters, so `CliDelimiterGuard` remains the one narrow lexical exception.

| Exception fact | Accepted record |
| --- | --- |
| Unmet standard capability | Typed results discard delimiter spelling. |
| User-visible effect | Route List accepts equals depth and rejects spaced, colon, or bare depth before `--`; other scalar options retain all native forms. |
| Scope | One `--depth` policy, applied before domain binding and stopped at `--`. |
| Evidence | Direct pinned parser normalization plus Unit, Integration, and published Route List grammar regressions. |
| Owner | This parser Task and Route List contract. |
| Removal condition | Remove the guard if the product contract accepts native depth forms or the pinned parser exposes delimiter provenance through a supported typed API. |

No other raw scan, custom process parser, delimiter policy, or command-binding raw
argument path is accepted.

## Decisions Needed

None. The exact callable architecture, retained exception, behavior boundary, and
evidence are closed. A stop condition returns to Preflight rather than inventing a
new parser mechanism.

## Frozen Gray Architecture

Gray adds callable structure only:

- `Shell/Parsing/Models/CliOptionResultFacts`: immutable explicit-occurrence,
  identifier-count, and value-count facts.
- `Shell/Parsing/CliOptionResultFactsReader.Read<T>(ParseResult, Option<T>)`:
  throwing skeleton that will later read only public parser results.
- `Shell/Parsing/CliTerminalInputValidator.Validate(CliParseOutcome,
  CliGlobalInput)`: throwing skeleton returning an optional existing
  `CliInvalidInput` for terminal/domain composition.

Gray does not wire the new validation surfaces, alter option arity or types,
remove or relocate existing code, add tests, or change runtime behavior.

### Post-Green Callable Migration

After accepted Green behavior no longer reads binding raw arguments, one separate
authorized structural-refinement phase starts from the exact accepted Green commit
once that hash is recorded. Its inspected commit removes
`CliBindingParse.OriginalArguments`, relocates the record to its required `Models/`
path, and mechanically updates production and existing test call sites. Public
behavior, Red expectations, existing assertions, fixtures, and the retained
delimiter guard are frozen. No compatibility constructor, forwarding type, or
replacement raw property is allowed.

## Red Evidence Matrix

| Area | Required failing evidence before Green |
| --- | --- |
| Pinned native forms | Direct 2.0.11 facts prove spaced/equals/colon normalization for workspace, view, and a policy-free depth option. Public Route List still proves its explicit equals-only exception. |
| Occurrences and multi-value | Scalar repetition errors, Boolean occurrence counts, and repeated `Option<string[]>` aggregation use `OptionResult` facts; no raw split or per-token accumulator. |
| Attached-empty values | Workspace, view, and depth attached-empty forms preserve following `--json`/`--verbose`/terminal options and become invalid rather than defaults or consumed values. |
| Typed option facts | Omitted, explicit one-value, explicit no-value, and repeated results expose exact explicit/identifier/value counts. |
| Terminal composition | Root, route group, List leaf, and Inspect leaf retain valid help/version with well-formed globals; in terminal mode, explicit operands, operation-local depth, unmatched tokens, and option-like operands after `--` are invalid before workspace or operation work. Outside terminal mode, option-like operands after `--` remain domain input. |
| Route List depth | Omitted, zero, `Int32.MaxValue`, `all`, negative, overflow, unknown, empty, repetition, and `--` behavior preserve typed command results and no-write behavior. `--depth= --json` is triage evidence for the general no-value fact, not a spelling-specific requirement. |
| Equals-only exception | Unit, Integration, and published-process cases prove equals-valid, bare-invalid, spaced-invalid, colon-invalid, and inspection stopping at `--`. |
| Green raw-consumption boundary | Typed depth/option expectations require no production consumer to read `CliBindingParse.OriginalArguments`, no `ReadDepthSpelling`, and no global custom parser callback. Red replaces the obsolete `RawDepthScanStopsAtTheDelimiter` helper-level test with typed depth-fact and `--` behavior evidence before expectations freeze. The raw property may remain temporarily until the authorized post-Green refinement. |
| Post-Green raw-authority removal | Source/call-site evidence after accepted Green requires the `CliBindingParse.OriginalArguments` property to be absent, the model relocated, all call sites mechanically updated, and no forwarding/compatibility type present. |

Unit owns pure result-fact and terminal-policy cases. Integration owns the exact
composed parser tree and pinned package behavior. Managed EndToEnd owns selected
public terminal conflicts, attached-empty depth/global separation, streams/exits,
and unchanged workspaces. Existing Route List and Inspect application tests remain
directly affected regression boundaries.

## Allowed Surfaces

- `src/cli/core/OpenForge.Cli.Core/Shell/Parsing/` and its topical `Models/`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCommandBinding.cs`,
  `CliCoreApplication.cs`, and `Shell/Composition/Models/` only for removing raw
  binding input and integrating typed validation.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/RouteListBinding.cs` and
  `RouteListBindingInputPolicy.cs` only for typed depth input.
- Root composition only if direct compile-time delimiter-policy plumbing requires
  an equivalent adjustment.
- Matching active Unit, Integration, EndToEnd, edge-ledger, Task, Plan, and
  Checkpoint evidence.

## Protected Surfaces

- All command contracts, result/status/rendering/domain behavior, JSON shapes, and
  filesystem behavior.
- Route Inspect typed cardinality and invalid-result presentation.
- The equals-only Route List depth contract and its one documented guard.
- Test fixture architecture, long signatures including `RouteListBinding.Close`,
  root-host/Core project ownership, dependencies, and unrelated commands.
- Preserved tests and generated `Entries` regions.

## Evidence

### Durable New Test Identities

- Unit `CliOptionResultFactsExposeExplicitOccurrenceAndValueCounts`.
- Integration `PinnedParserNormalizesNativeScalarForms` and
  `PinnedParserAggregatesRepeatedMultiValueOptions`.
- Integration `AttachedEmptyScalarPreservesFollowingGlobal`.
- Unit and Integration `TerminalModesRejectDomainAndLocalInput`.
- EndToEnd `PublishedTerminalModesRejectDomainAndLocalInput` and
  `PublishedAttachedEmptyDepthPreservesJsonInvalidResult`.
- Unit, Integration, and EndToEnd equals-only matrices explicitly include valid
  equals, invalid bare, invalid spaced, invalid colon, and `--` termination cases.

### Exact Focused Commands

Commands run from `src/cli/`:

```bash
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~OpenForge.Cli.Core.UnitTests.Shell.ParserTests|FullyQualifiedName~OpenForge.Cli.Core.UnitTests.Shell.BindingTests|FullyQualifiedName~RouteListDefinitionsSyntaxAndBindingTests|FullyQualifiedName~RouteInspectBindingAndCompositionTests"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~SystemCommandLineBehaviorTests|FullyQualifiedName~CliHostTests|FullyQualifiedName~RouteListApplicationIntegrationTests|FullyQualifiedName~RouteInspectApplicationIntegrationTests"
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~OpenForge.Cli.EndToEndTests.CliProcessTests|FullyQualifiedName~PublishedRouteInspect"
```

From the repository root, every phase runs `git diff --check`.

### Public Scenario

Against the fresh managed publish, `open-forge route inspect --help root --json`
must exit `4`, write no stdout, write one bounded semantic diagnostic to stderr,
perform no workspace or operation work, and preserve workspace hashes. In ordinary
domain mode, `open-forge route inspect --json -- --view` remains one typed invalid
source result rather than a terminal conflict. Route List
`--depth= --json` remains a typed JSON invalid-depth result, proving the general
explicit/no-value invariant rather than preserving one raw spelling branch.

### Phase And Correction Routing

- Gray verification: warning-free build, format, diff, exact callable inspection,
  and targeted correctness/locality review; no tests are added or run as behavior
  evidence.
- Red verification: exact focused Unit and Integration commands compile and fail
  only at frozen skeletons or still-present deviations; the published Red subset
  is run only when the managed executable can express the intended failure safely.
- Green verification: all exact focused commands and the public scenario pass.
- Post-Green callable migration verification: the same focused commands pass;
  source audit proves the raw property and helper are absent with no binding
  raw-argument authority or forwarding type; fresh correctness and locality review
  passes.
- A typed-fact or terminal call-surface defect returns to Gray; an expectation or
  tier defect returns to Red; an implementation defect returns to Green; a raw
  binding contract/locality defect returns to the post-Green migration. A contract,
  dependency, or later-child requirement returns to Preflight.
- Separate Red evidence authoring and Green implementation contexts add value;
  final correctness and local-improvement reviewers inspect each production phase.
  Mastermind retains architecture, integration, and acceptance.

### Final Generic-Branch Gate

The parent Task's [Final Full Gate](_generic-improvements.md#final-full-gate)
records the exact complete managed, local `win-x64` Native AOT, package, format,
diff, and audit commands run after the final generic production change. Parser
inner loops do not consume that final gate early.

### Evidence Boundaries

- Focused Unit filters: `Feature=cli-parser`, Route List syntax/binding, Shell
  binding, and Route Inspect binding/composition.
- Focused Integration filters: `SystemCommandLineBehaviorTests`, `CliHostTests`,
  Route List application, and Route Inspect application.
- Focused managed EndToEnd filters: CLI process, Route List grammar/failure, and
  Route Inspect terminal/cardinality cases against a fresh managed publish.
- Each mutating phase runs warning-free Release build, exact-path format, and
  `git diff --check`. Final generic-branch acceptance, not this inner loop, owns
  the next complete suite and Native AOT decision.
- Fresh correctness and local-improvement reviews inspect each accepted production
  phase and direct consumers.

## Stop Conditions

- Stop if public 2.0.11 results cannot expose explicit no-value, terminal operands,
  or local-option identity without another raw scan.
- Stop if a correction would change a command contract, accept alternate Route
  List depth delimiters, lose typed Inspect invalid results, consume a following
  global, or replace parser diagnostics with a spelling-specific branch.
- Stop on a new dependency, package update, public wire change, test-architecture
  cleanup, long-signature cleanup, or root-host/Core restructuring.

## Progress

- Development cycle: Task `Parser And Standard Behavior Remediation`; compile-only
  Gray is accepted at `ddd2683`; corrected Red is accepted at `68455f6`; Green is
  accepted at `661b89b`; the post-Green callable migration is accepted at `d7056e8`;
  Blue is accepted at `f9f4dbe`; Purple is accepted at `d445e20`; public scenarios
  and focused Task acceptance pass; active-test architecture was later completed at
  `e277227` without reopening parser meaning.
- Accepted Red result: Eight active test files add the complete affected option-fact,
  native-form, attached-empty, terminal-composition, depth, delimiter, cardinality,
  stream, exit, and no-write matrix. Gray production, contracts, dependencies,
  fixtures, generated regions, and later generic-improvement surfaces are unchanged.
- Red evidence: The warning-free Release build, format verification, and
  `git diff --check` pass. Focused Unit is `68` total with `52` pass and `16`
  intentional failures. Focused Integration is `93` total with `52` pass and `41`
  intentional failures. A fresh managed publish drives `57` selected EndToEnd cases
  with `51` pass and `6` intentional failures. The failures are limited to the two
  throwing Gray callables, current raw depth consumption, current custom attached-
  empty parsing, or current terminal-mode bypass. There are no skips.
- Red review: The bounded correctness review initially found that typed authority
  was not disproved and one test asserted an unaccepted internal code. Adversarial
  typed-versus-raw evidence now closes the authority gap, and the code assertion is
  removed. The bounded improvement review removed same-tier empty-depth duplication,
  required one bounded terminal diagnostic, and made source-case helpers exhaustive.
  Final correctness review passes; final local-improvement review returns
  `NO_MATERIAL_IMPROVEMENTS`. Retaining more same-tier duplication was the strongest
  alternative, but it added execution cost without another evidence boundary.
- Correction cycle: Initial Green integration exposed one contradictory Route List
  Unit expectation that required terminal-domain validation both inside
  `CliTerminalValidator` and again through a second direct validator call. The
  accepted architecture requires one shared validation before terminal short
  circuiting, so the corrected expectation now observes the integrated resolution
  directly. The corrected Green review then found that removing the workspace
  callback also removed non-empty validation for a spaced explicit empty token; one
  focused Unit expectation now requires that typed value to remain semantic invalid
  input. These production-free corrections start from `a47f145`, remain grouped in
  the one exceptional correction cycle, and leave the separate direct-validator,
  typed-authority, terminal, and public evidence intact. Green production work was
  isolated before each correction and resumes only from the latest correction
  commit.
- Green result: Seven production files now read public option-result facts, preserve
  native scalar forms and following globals, reject explicit missing or empty scalar
  values through typed semantic validation, and reject terminal/domain conflicts once
  before workspace or operation work. Route List reads typed depth only and retains
  the one equals-only delimiter guard; `ReadDepthSpelling` and global custom parsers
  are removed. `CliBindingParse.OriginalArguments` remains only for the separately
  authorized post-Green migration, and Route List does not consume its value.
- Green evidence: The warning-free Release build, format verification, and
  `git diff --check` pass. Focused Unit is `68/68`, focused Integration is `93/93`,
  and a fresh managed publish drives selected EndToEnd `57/57`, all with zero skip.
  The exact public terminal scenario exits `4`, writes no stdout, and writes one
  bounded stderr diagnostic. Ordinary `route inspect --json -- --view` remains one
  typed invalid source result with empty stderr. The source audit finds raw arguments
  only in two transitional binding-parse constructions and the accepted delimiter
  guard; `ReadDepthSpelling` and `CustomParser` are absent.
- Green review: The first bounded correctness review found the explicit-empty
  workspace regression, which returned to corrected Red, and the local-improvement
  review found an overly broad semantic exception catch. The final Green rejects the
  empty typed value, isolates global-input and terminal-policy exceptions, preserves
  the terminal-conflict identity, and lets typed terminal validation return its
  existing invalid model. Final correctness review passes; final local-improvement
  review returns `NO_MATERIAL_IMPROVEMENTS`. A separately named bare-terminator case
  was the strongest residual counterargument, but the typed tree, omitted-operand,
  unmatched-input, and post-terminator operand evidence shows no defect.
- Post-Green migration result: `CliBindingParse` now contains only `ParseResult` and
  resides at `Shell/Composition/Models/CliBindingParse.cs` with the matching Models
  namespace. The old declaration, `OriginalArguments` property, Route List raw
  parameter, two-argument constructors, and every related production and Unit call
  site are removed or updated without a forwarding or compatibility type. The
  retained `CliParseOutcome.OriginalArguments` remains separate for pinned parser
  evidence and the one accepted delimiter guard.
- Post-Green migration evidence: The warning-free Release build, format verification,
  and `git diff --check` pass. Focused Unit is `68/68`, Integration is `93/93`, and a
  fresh managed publish drives selected EndToEnd `57/57`, all with zero skip. The
  source audit finds one `CliBindingParse` declaration at the required path, one-argument
  construction throughout, and no old property, Route List raw parameter, duplicate,
  forwarding type, `ReadDepthSpelling`, or global custom parser.
- Post-Green migration review: Bounded correctness review passes. The local-
  improvement review found one stale test method name after raw-argument removal;
  the name now states typed parser-fact ownership and the final review returns
  `NO_MATERIAL_IMPROVEMENTS`. Reusing one temporary binding-parse local was the
  strongest alternative, but mutually exclusive branches make it immaterial.
- Blue result: Four production files now name the shared explicit-without-value fact
  once and reuse one immutable omitted-facts instance. Global scalar validation and
  Route List typed depth consume the named fact without changing the Gray model's
  accepted values, parser behavior, raw-authority boundary, or call surfaces.
- Blue evidence and review: The warning-free Release build, format verification, and
  `git diff --check` pass. Focused Unit is `68/68`, Integration is `93/93`, and a
  fresh managed publish drives selected EndToEnd `57/57`, all with zero skip. Bounded
  correctness review passes. The local-improvement review recommends keeping the
  named predicate and immutable cache and returns `NO_MATERIAL_IMPROVEMENTS` for the
  final Blue diff. Per-read reference identity was the strongest counterargument, but
  the sealed get-only model has value-only consumers and no identity contract.
- Purple result: The published version terminal journey now snapshots both the parent
  of the deliberately missing explicit workspace and the separate process current
  directory, then proves both remain byte-unchanged. Arguments, exit, streams,
  version text, missing-workspace assertion, test identity, fixtures, and production
  remain unchanged.
- Purple evidence and review: The warning-free Release build, format verification,
  and `git diff --check` pass. Focused Unit is `68/68`, Integration is `93/93`, and a
  fresh managed publish drives selected EndToEnd `57/57`, all with zero skip. Bounded
  correctness review passes. Snapshot hashes do not claim transient or metadata-only
  mutation, but the existing helper plus the direct missing-directory assertion proves
  the accepted byte/no-create boundary without entering the later test-architecture
  Task.
- Public scenario and acceptance: Exact published `route inspect --help root --json`,
  ordinary `route inspect --json -- --view`, and `route list --depth= --json`
  scenarios pass with their required exits, streams, typed results, and bounded
  diagnostic. The final whole-Task review finds no production, contract, architecture,
  correction-cycle, or evidence gap after current-state instructions are aligned.
  CLI-EDGE-005 and CLI-EDGE-007 are closed for parser remediation. The one correction
  cycle is consumed; no unresolved material finding remains. At parser acceptance,
  the parent generic branch's complete managed and Native AOT gate remained deferred
  until every generic child was complete; it now passes from clean `7871764`.
- Blockers: None.
- Next action: Return to the accepted generic parent. Its final feature gate passes
  from clean `7871764`, and authorized squash-merge into `develop` is next; parser
  remediation remains Complete and its accepted meaning is closed.

## Completion

Complete when pinned evidence and public regressions pass; terminal/domain input is
validated once from typed results; global callbacks, generic Route List raw depth
parsing and validation, and binding raw arguments are gone; the one Route List
delimiter exception is fully documented and bounded; CLI-EDGE-005/007 are updated;
and independent review finds no behavior, dependency, or later-child scope drift.
