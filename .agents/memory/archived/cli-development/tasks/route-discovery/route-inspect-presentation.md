---
open-forge:
  description: Bind, execute, and present route inspect once through compact, expanded, JSON, diagnostics, and help surfaces
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Route, Inspect, Presentation, Complete]
---

# Present Route Inspect Through The CLI

## Task State

- State: Complete from exact accepted Presentation Green commit `51c0960` (`Implement route inspect presentation`) plus final no-production-change managed acceptance evidence recorded below.
- Responsible/implementer: Mastermind. Bounded phase delegation is permitted only after this packet and its exact predecessor are closed. Mastermind owns integration, staging, commits, and acceptance.
- Parent: [Implement Route Inspect And Promote Shared Route Facts](route-inspect.md).
- Exact predecessors: profile/result behavior at `c407e24` (`Implement route inspect profile`) and accepted Shell correction at `222ada9` (`Clarify typed shell input ownership`). Presentation must not reopen either accepted boundary.

## Expected Outcome

`route inspect` is bound and registered, its operation executes at most once, and one typed result drives compact human, expanded human, and source-generated JSON projections. Help, examples, related commands, bounded diagnostics, streams, statuses, exits, and raw lexical argument behavior conform to the accepted contracts.

## Authority And Backlinks

- Parent: [route-inspect parent](route-inspect.md).
- Predecessor: [route-inspect profile child](route-inspect-profile.md).
- Meaning: [Route Inspect Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).
- Structure: [CLI Architecture](../../../../crystallized/documents/cli/architecture.md) and [Shared CLI Operation Contract](../../../../crystallized/documents/cli/shared-operation-contract.md).
- Implementation and evidence: [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md), [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md), and [Evidence tiers](../../../../../patterns/testing/evidence-tiers.md).
- Edge inputs: [Replacement CLI Edge-Case Ledger](../../edge-cases.md), especially [CLI-EDGE-004](../../edge-cases.md#cli-edge-004--hostile-process-input-breadth) and [CLI-EDGE-005](../../edge-cases.md#cli-edge-005--raw-lexical-option-edge).

## Execution Provenance

| Stage                                     | Primary owner                     | Supporting agents                                                           | Recorded work                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ----------------------------------------- | --------------------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Activation                                | Mastermind (`openai/gpt-5.6-sol`) | None                                                                        | Adopted exact accepted profile predecessor `c407e24`, protected profile/result behavior, and activated presentation Phase 0.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| Phase 0 exploration and synthesis         | Mastermind (`openai/gpt-5.6-sol`) | Two Explorer agents and two independent Advisor agents                      | Inspected production, evidence, contracts, and pinned parser behavior. One analysis supported an Inspect-local binding and projection adapter; another identified the existing global raw-argument delimiter enforcement as a blocking Shell defect. Mastermind checked the findings against the sources and adopted the focused correction boundary below.                                                                                                                                                                                                                                                                                            |
| Shell correction Red                      | Mastermind (`openai/gpt-5.6-sol`) | One Red Evidence Author, one Reviewer, and one Improvement Reviewer         | Authored the production-free four-file evidence packet. Corrected two test-only compilation issues, restored the contract-correct `route-list.unknown-source` expectation, added positive terminal occurrence evidence, made baselines non-vacuous, and split published delimiter and `--` journeys. Correctness review passed. Local-improvement review produced clearer raw-argument, baseline, ownership, and failure evidence; its final suggestion for message-bearing `Assert.Equal` was not applied because xUnit provides no such overload and the current matrix assertions retain the failing form identity.                                 |
| Shell Green investigation                 | Mastermind (`openai/gpt-5.6-sol`) | One Green Behavior Implementer and one grounded Advisor                     | The four-file Green made every frozen correction test pass, but the accepted published `--depth= --json` regression emitted human instead of JSON because pinned parser arity consumed `--json` as the depth value after raw global scans were removed. Production WIP was isolated without commit. Direct pinned-version evidence and independent advice support the narrow production-free arity supplement below rather than restoring raw global parsing or weakening the published expectation.                                                                                                                                                   |
| Shell correction Red supplement           | Mastermind (`openai/gpt-5.6-sol`) | One Reviewer and one Writing Reviewer                                       | Added one Route List symbol expectation and one direct pinned-parser case while preserving the published expectation and production baseline. Correctness review passed; writing review corrections made public arity, the frozen published expectation, and exact resume boundary explicit.                                                                                                                                                                                                                                                                                                                                                           |
| Shell correction Green                    | Mastermind (`openai/gpt-5.6-sol`) | One Green Behavior Implementer, one Reviewer, and one Improvement Reviewer  | Restored the isolated typed-global Green, added only the accepted depth-symbol arity change, and passed all frozen plus full regression evidence. Correctness review passed. Improvement review identified truthful global-reader ownership for a separate Blue commit, deferred Route List binding-signature cleanup to the authorized generic-improvements audit, and supported a code-local explanation for the exceptional parser arity.                                                                                                                                                                                                           |
| Shell correction Blue                     | Mastermind (`openai/gpt-5.6-sol`) | One Reviewer and one Improvement Reviewer                                   | Moved typed parser extraction from the data record into `CliGlobalInputReader` and documented the exceptional depth token-capture setting without changing behavior. The full frozen evidence passed again; correctness review passed and final improvement review found no material local improvement.                                                                                                                                                                                                                                                                                                                                                |
| Maintainer process update                 | Mastermind (`openai/gpt-5.6-sol`) | None                                                                        | Adopted the durable Working-prose review rule, full-suite beginning/end execution rule, focused authored/affected inner loop, general standard-behavior and edge-triage priority, and parser-before-test-architecture sequence. Existing exact `222ada9` evidence supplies this Task's beginning full-suite baseline.                                                                                                                                                                                                                                                                                                                                  |
| Presentation Gray planning                | Mastermind (`openai/gpt-5.6-sol`) | Two Explorer agents and two grounded Advisor agents                         | Mapped binding, renderer, JSON, help, composition, test, route-family ownership, and invalid-binding context surfaces. Accepted route-family promotion at the second real leaf and a typed invalid-binding context so workspace-invalid Inspect results retain parser-owned source operands. Recorded terminal-mode domain-input handling as non-blocking triage for the authorized parser-remediation branch rather than adding a command-local workaround.                                                                                                                                                                                           |
| Presentation Gray                         | Mastermind (`openai/gpt-5.6-sol`) | One Gray Contract Implementer, one Reviewer, and one Improvement Reviewer   | Added only compile-time route-family, invalid-binding, Inspect binding/rendering/help, required JSON DTO, and source-generation surfaces. Corrected the cardinality-policy context, required complete DTO initialization, and retained accepted physical-layer paths. All behavior entrypoints throw. Correctness review passed and final improvement review found no material Gray improvement.                                                                                                                                                                                                                                                       |
| Presentation Red authoring and correction | Mastermind (`openai/gpt-5.6-sol`) | Four Red Evidence Authors, one Reviewer, and one Improvement Reviewer       | Authored the production-free Unit, Integration, and managed-process evidence packet. First review returned it for exact next-action wording, zero-operation invalid paths, complete JSON layer/DTO coverage, semantically valid fixtures, and contract-bounded escaping/diagnostics. Corrections added independently selectable status and native-form evidence, exact public human wording, typed JSON next parity, complete physical layers and DTO shapes, direct zero-operation paths, valid entrypoint facts, control-safe encoding-agnostic escaping, an independent diagnostic failure root, and mirrored Unit and Integration helper locality. |
| Presentation Red final review             | Mastermind (`openai/gpt-5.6-sol`) | One Reviewer and one Improvement Reviewer                                   | Final correctness review passed after correcting entrypoint local-Axioms applicability and one source-reference kind. Local-improvement review found no material improvement: diagnostic detail is not a stable schema, large files remain cohesive and selectable, helpers occupy their narrowest shared capability, and public failed/interrupted triggers remain intentionally absent. Green has not started.                                                                                                                                                                                                                                       |
| Presentation Green                        | Mastermind (`openai/gpt-5.6-sol`) | Two Green Behavior Implementers, one Reviewer, and one Improvement Reviewer | Implemented one family-owned route group, Inspect binding/composition, contextual invalid-input migration, human/JSON/diagnostic/help presentation, and root registration. Mechanically migrated test call sites while preserving frozen expectations. Review corrections removed the final legacy overload, added ambiguous-route direct-safe wording, eliminated duplicate expanded messages, preserved Unicode scalar boundaries during truncation, made KeepInMind prose event-specific, and marked unavailable related commands truthfully. Final correctness review passed and local-improvement review found no material improvement.           |
| Presentation final acceptance             | Mastermind (`openai/gpt-5.6-sol`) | None                                                                        | From the unchanged production tree at `51c0960`, ran the one final complete managed boundary: warning-free Release build, format, diff, Unit `549/549`, Integration `166/166`, and freshly published managed EndToEnd `36/36`. Presentation is complete; Native AOT remains assigned to the integrated acceptance child.                                                                                                                                                                                                                                                                                                                               |
| Integrated-acceptance locality correction | Mastermind (`openai/gpt-5.6-sol`) | One Reviewer and one Improvement Reviewer                                   | Moved `RouteInspectSymbols`, the materially changed `CliGlobalInput` model cluster, and `RouteTopologyParentRelationship` to their nearest topical `Models/` scopes with matching namespaces and no forwarding or duplicate type. Contracts, tests, parser behavior, and presentation expectations remained frozen. Release build, format, focused Unit `71/71`, and focused Integration `52/52` pass; correctness review passes; the improvement review's three stale-import findings are applied and the post-cleanup build/format pass.                                                                                                             |

## Analysis And Accepted Plan

1. Build the exact `open-forge route inspect <source-reference> [global flags]` symbol tree in the Inspect binding and register it through the explicit root composition. Standard help remains derived from the composed symbols; Inspect owns only its product sections, examples, related commands, and bounded notes.
2. Pass one complete request to one operation and cache one concrete typed result. Renderers never rerun resolution, graph construction, profile formation, status selection, or measurement.
3. Render compact and expanded human views from the same result. Render one complete source-generated JSON graph from the same result, with `--view` accepted as a JSON no-op. Diagnostics are bounded, escaped, redacted as required, and written only to stderr.
4. Preserve the shared process policy for all seven statuses, streams, exits, terminal help/version bypass, and at most one required `Next:` line. Do not let presentation add diagnosis, recommendation, or mutation meaning.
5. Preserve bounded original arguments and the accepted `--` lexical boundary. Raw inspection may not become a second parser or consume option-like workspace/source values as a new syntax system.
6. Use the pinned `System.CommandLine` 2.0.11 parser's native option syntax. Generated help uses the ordinary `--view <value>` form; native `--view=value` and `--view:value` remain accepted without a custom lexical parser. Multi-value options use native repeated occurrences by default and enable multiple arguments per token only when an accepted command contract requires that shape.

### Phase 0 Shell Defect And Accepted Correction Boundary

Phase 0 reached the generic-Shell stop condition before presentation Gray. The
root then added delimiter policies that rejected native attached `--workspace`
forms and native spaced or colon `--view` forms. `CliGlobalInput` also rescanned
original arguments to recover global values and occurrence counts. The delimiter
guard scanned beyond `--`, so the Route List depth policy could misclassify an
option-like Inspect source operand. This conflicted with the pinned-parser rules
in the [CLI implementation Directive](../../../../../directives/open-forge/cli/implementation.md#construction-parsing-and-pipeline)
and would make the accepted Inspect grammar unimplementable without an Inspect
special case. These are proven shared Shell defects, not a reason to reopen
route-inspect profile or result meaning.

The accepted prerequisite correction is deliberately narrower than the later
generic-improvements audit:

1. Read all six global values from typed parse results and their occurrence facts
   from parser-owned `System.CommandLine` occurrence data. Remove every global
   raw-argument scan, including the unused `ReadAvailable` fallback.
2. Remove only the root-owned workspace/view delimiter policies so the pinned
   parser accepts spaced, equals, and colon scalar forms uniformly.
3. Make the existing delimiter guard stop at `--`. Preserve bounded original
   arguments and the route-list `--depth=<value>` delimiter exception because its
   accepted command contract explicitly requires that spelling.
4. Configure only the Route List depth parser symbol as zero-or-one so pinned
   `System.CommandLine` retains a following global option after attached-empty
   `--depth=`. This changes only parser argument-token capture, not the public
   option arity: `route list` still exposes one scalar
   `--depth=<non-negative-integer|all>` value, with repetition and attached-empty
   values invalid. Bare, spaced, and colon forms remain rejected before `--`;
   option-like tokens after `--` remain source operands. The existing raw depth
   spelling reader and typed depth validation preserve that behavior. Broader
   raw-depth remediation remains deferred.
5. Make no Inspect binding, renderer, profile, result, JSON, help, or command-tree
   composition change in this correction. The only root-definition change is
   removing the two root workspace/view policies and, if pinned-parser evidence
   requires it, applying general typed nonempty validation to a value-taking
   global. Do not redesign policy attachment, Route List, or the generic Shell
   pipeline.

Allowed production files for the correction are only
`Shell/Parsing/CliGlobalInput.cs`,
`Shell/Parsing/CliGlobalInputReader.cs`, and
`Shell/Parsing/CliRootDefinitionFactory.cs`, plus the single `--` stop condition
in `Shell/Parsing/CliDelimiterGuard.cs`, plus only the depth-symbol arity in
`Commands/Route/List/RouteListBinding.cs`. Focused evidence may change Shell Unit
tests, the Route List syntax/binding Unit test, pinned-parser Integration tests,
Route List application Integration tests, and the published process test. Route
List request/result/operation/presentation behavior and every route-inspect
production file remain protected.

The correction uses a production-free Red commit followed by the smallest Green
commit. Red proves typed defaults and occurrences for all six global flags; all
three native scalar forms for the two value-taking globals, `--workspace` and
`--view`; invalid attached-empty equals and colon forms for both value-taking
globals; identical composed Route List results across valid forms; published
process behavior; preservation of global scalar repetition and Boolean
idempotence; the unchanged route-list depth exception before `--`; and an
option-like source operand after `--` while the delimiter guard receives policies
aggregated from sibling commands.
Green passes that frozen evidence without changing expectations or fixtures. The
full accepted Route List suite is regression evidence, not authority to redesign
it. This packet covers only the six global flags, the root
`--workspace`/`--view` policies and attached-empty safety, and the guard's `--`
boundary. The production-free arity supplement additionally proves that
attached-empty depth retains zero argument tokens, `--json` retains its own typed
occurrence, and omitted/default, valid, repeated, spaced, colon, and post-`--`
depth behavior remains unchanged. The later authorized audit remains responsible
for every other parser, raw-argument, delimiter, occurrence, multi-value, and
attached-empty case.

### Phase Boundaries

- Phase 0 adopts the exact profile predecessor, records the separately proven Shell defect above, and freezes its correction before presentation Gray.
- The Shell correction used production-free Red and smallest Green commits, each independently reviewed, before presentation work resumed from exact commit `222ada9`.
- The discovered pinned-parser attached-empty consumption conflict returns to one production-free Red supplement before Green resumes. The published expectation remains frozen across this boundary; production WIP remains uncommitted until the supplement is accepted.
- If new renderer or projection call surfaces are required, Gray freezes only those compilable production surfaces with no tests or behavior.
- Red adds the complete affected failing Unit, Integration, and managed EndToEnd evidence without modifying frozen production.
- Green makes the frozen Red evidence pass without changing expectations or fixtures. Blue and Purple receive separate inspected commits only when they mutate permitted surfaces, and every mutating phase updates this Task in the same commit.
- The exact `222ada9` full-suite result is the presentation beginning baseline. During Gray/Red/Green, run only new presentation evidence, fast in-memory tests useful for feedback, and directly affected Shell/List/profile regressions. Run the complete Unit, Integration, and managed published-process suite once after the final presentation change before this child is accepted.

### Presentation Gray Contract

Gray starts from exact planning baseline `3b5078e` and freezes only compilable
callable/model surfaces. It adds no Inspect behavior, tests, command registration,
or operation execution.

The second real route leaf establishes family ownership at `Commands/Route/`:

- `RouteDefinitions.RouteGroup` owns the shared group syntax.
- `RouteBinding.CreateGroup()` owns creation of the one route `Command` instance.
- `RouteHelpSections.CreateGroup()` owns family help.
- `RouteListBinding.CreateSymbols(Command routeGroup)` is a temporary throwing
  overload in Gray. Green implements it, removes the no-argument factory and
  List-owned route-group definition/help, and leaves no forwarding wrapper.
- `RouteInspectBinding.CreateSymbols(Command routeGroup)` attaches only Inspect.
  Root composition later creates one group, passes that exact instance to both
  leaves, registers one branch, and dispatches both bindings by command identity.

The accepted Inspect-local callable surfaces are:

- `RouteInspectBinding.CreateSymbols`, `CreateBinder`, `Bind`, and `Close`.
  Source operands use one parser-owned `Argument<string[]>` with semantic
  cardinality validation; Inspect never scans original arguments.
- `RouteInspectBindingInputPolicy` forms missing, multiple, and workspace-invalid
  typed results without invoking the operation.
- `RouteInspectBindingComponents` groups help, operation, renderer set, and
  optional diagnostics so `Close` remains a two-parameter call.
- `RouteInspectHumanRenderer`, compact renderer, expanded renderer, JSON
  projection/renderer, diagnostic renderer, text escaping, and Inspect help each
  expose one narrow directly callable entrypoint; escaping may expose bounded
  overloads with at most two parameters.
- `RouteInspectJsonDocument` and its property-only presentation graph preserve the
  schema-version-1 envelope and complete selection, identity, reading, five
  measurements, topology, Axioms, observation, condition, and next-action facts.
  `CliJsonContext` registers only that concrete document graph.

Workspace resolution currently reaches a command's invalid-result factory before
binding, while the Inspect contract requires the requested source in its error.
Gray therefore adds property-only `CliInvalidBindingInput` under Shell Composition
Models and a context-based invalid-result delegate. It retains the old Shell path
only as temporary compile compatibility. Red freezes parser-context retention;
Green migrates Shell and Route List mechanically, then deletes the legacy path.
No command receives `CliParseOutcome`, and no domain operation receives parser
types.

All Gray behavior entrypoints throw `NotSupportedException`. New and materially
changed production files remain below 200 lines, and every new callable has at
most four parameters.

### Frozen Presentation Red Matrix

- Unit fixed-result evidence covers binding cardinality, workspace-invalid source
  retention, one operation invocation, compact/expanded/JSON projection parity,
  all seven statuses, primary targets/exits, exact `Next:` presence, tri-state
  facts, source-generated JSON shape, diagnostics, escaping, and Inspect-owned
  help. Existing profile test data may move only to a nearer shared Inspect test
  scope when at least two presentation tests need identical builders.
- Integration evidence covers the one shared route group and exact leaf identity,
  root/group/List/Inspect help truth, real binding/pipeline/serialization, native
  global forms, terminal bypass, workspace-invalid typed JSON without operation
  execution, and selected real workspace journeys. It reuses accepted profile
  semantics rather than rebuilding them in presentation assertions.
- Managed published EndToEnd evidence covers public grammar, group and leaf help,
  compact/expanded/JSON/verbose streams and exits, stable public status journeys,
  option-like operands after `--`, hostile filesystem-safe values, and unchanged
  workspace hashes. NUL/control-only renderer cases remain Unit evidence. No
  test-only public `failed` trigger is added; fixed-result Unit/Integration evidence
  covers failed status while `CLI-EDGE-003` remains honest.
- Existing focused Route List binding, presentation, composed application, parser,
  and published-process cases are affected regressions. The full `501/143/9`
  suites are not repeated in the inner loop; final presentation acceptance runs
  the complete suite once.

## Allowed And Protected Surfaces

### Allowed

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/` for Inspect binding, human/JSON/diagnostic rendering, help, and the presentation call surface; place projection records, interfaces, and property-only presentation types under the nearest topical `Models/` path.
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/` for the promoted route-group definition, one group factory, and family group help proved shared by List and Inspect.
- The narrow Shell Composition invalid-binding context/delegate and its exact pipeline migration required to retain parser-owned command inputs on pre-binding workspace failures.
- Root composition registration for `route inspect` and the existing CLI JSON context registration for the concrete Inspect projection.
- Mirrored Inspect Unit and Integration tests plus managed EndToEnd tests and matching fixtures under `src/cli/tests/`.

### Protected

- Accepted graph, profile, measurement, availability, result, status, and operation behavior. Presentation must consume the predecessor's typed result rather than recompute it.
- Generic Shell semantics beyond the frozen invalid-binding context. Terminal-mode domain-input conflict is recorded for later general parser remediation rather than handled specially in Inspect.
- Route-list result, operation, human/JSON/diagnostic rendering, contracts, and private support. Only route-group ownership, stale family/related help, the symbol-factory handoff, and mechanical invalid-factory adaptation may change; no shared renderer or second command catalogue is created.
- Native AOT acceptance, six-RID delivery, and unrelated commands.

## Requirements

- Expose the exact public grammar with exactly one required source reference and the six accepted global flags. Reject unsupported operation-specific modes.
- Preserve terminal `--help` and `--version` bypass before domain operation. A bare `route` group performs no domain operation.
- Execute the operation at most once and render compact, expanded, and JSON from one typed result. `--view` with `--json` is a no-op.
- Render all seven statuses: `complete`, `attention`, `incomplete`, `invalid`, `blocked`, `failed`, and `interrupted`.
- Keep primary human output on the contract-selected stream, JSON as one complete stdout document for every status, diagnostics on stderr, and human text out of JSON stdout.
- Emit at most one required `Next:` line and preserve the contract's exact presence/absence rules.
- Provide binding-derived help, examples, related commands, and bounded notes without a second command catalogue or help string-replacement pass.
- Escape control, NUL, Unicode, long, and option-like values without leaking raw control output. Preserve source/workspace values and the bounded original-arguments/`--` behavior required by CLI-EDGE-005.
- Keep CLI-EDGE-004's hostile-input breadth honest. This child supplies the warranted public cross-products selected by acceptance; it does not claim untested combinations.
- Keep presentation DTOs and other non-behavioral types out of behavior-owning renderer folders unless they are under that renderer capability's `Models/` child, grouped further when the model set grows beyond roughly five to ten types.
- Do not add a custom parser or raw-argument rescan to distinguish native spaced, equals, or colon option delimiters. Repetition policy may validate typed occurrences, but lexical tokenization remains library-owned.

## Evidence

- Unit evidence uses fixed typed results for compact/expanded/JSON parity, all statuses, stream and next-action rules, escaping, diagnostics bounds, help ownership, operation-once behavior, and JSON no-op view semantics.
- Integration evidence uses the production binding, pipeline, source-generated serialization, and owned streams without rerunning profile behavior.
- Managed EndToEnd evidence invokes the composed executable for exact grammar, terminal bypass, help/examples/related commands, human views, JSON, diagnostics, streams, exits, hostile values, and raw lexical `--` cases.
- Native AOT execution is deferred to the acceptance child. This child does not claim a published native result.
- The Mastermind inspects the complete composition, exact changed paths, protected profile behavior, and evidence before each mutating phase-boundary commit. No pending hash is invented.

### Beginning Full-Suite Baseline

The complete beginning baseline was executed immediately before accepted commit
`222ada9`; its production and test tree is identical to that commit. Commands ran
from `src/cli/` with the pinned .NET 10 SDK on managed Windows x64. The published
process project used a managed `win-x64` publish of `OpenForge.Cli.exe`,
`OPEN_FORGE_CLI_PATH` pointed to its owned publish location, and
`OPEN_FORGE_CLI_EXPECTED_VERSION` was `0.0.0-dev`.

The .NET 10 Microsoft.Testing.Platform mode selected by `global.json` requires
`--project`. The literal build and managed test commands were:

```text
dotnet build OpenForge.Cli.slnx -c Release --no-restore
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --no-progress
```

The published-process setup used the repository-routed artifact location:

```text
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj -c Release --no-restore -r win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH=artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe
OPEN_FORGE_CLI_EXPECTED_VERSION=0.0.0-dev
dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --no-progress
```

The two assignments are test-process environment variables resolved from
`src/cli/`, not additional command arguments. The managed publish and EndToEnd
run were reverified in this exact artifact location after the policy update; the
production and test tree remained identical to `222ada9`.

| Project tier                                      | Result         |
| ------------------------------------------------- | -------------- |
| Unit                                              | `501/501` pass |
| Integration                                       | `143/143` pass |
| EndToEnd against the managed published executable | `9/9` pass     |

The warning-free Release solution build, formatting verification, and
`git diff --check` also passed at this boundary. Native AOT remains assigned to
the acceptance child and is not claimed by this managed published baseline.

## Dependencies

| Dependency                                           | Required state        | Effect                                                                                                                                  |
| ---------------------------------------------------- | --------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| [route-inspect-profile.md](route-inspect-profile.md) | Accepted at `c407e24` | Supplies one typed profile/result meaning                                                                                               |
| Shell global-input correction                        | Accepted at `222ada9` | Supplies typed native global input, bounded `--` behavior, preserved Route List depth semantics, and the exact presentation predecessor |
| Route Inspect Interface and Behavior Contracts       | Unchanged             | Define exact grammar, projections, statuses, streams, and help relationship                                                             |
| CLI-EDGE-004 and CLI-EDGE-005                        | Explicitly carried    | Bound hostile-input breadth and raw lexical regression                                                                                  |

## Stop Conditions

- Stop if rendering recomputes or changes profile behavior, status, availability, graph facts, or next-action policy.
- Stop if generic Shell semantics must change without a separately proven Shell defect and accepted boundary.
- Stop if help needs a second catalogue, JSON needs a second result, diagnostics can alter primary completion, or human text mixes with JSON stdout.
- Stop if raw lexical scanning becomes a second parser, crosses `--`, or changes typed parser ownership.
- Stop if a required hostile cross-product is interpreted as proved without public evidence, or if presentation requires a new architecture, dependency, or unrelated command change.
- Stop if Gray creates a second route group, leaves List as the final family owner, adds a permanent dual invalid-factory API, loses the requested source on workspace failure, or requires any Route List result/presentation behavior change beyond truthful help.

## Progress And Next Action

- Development cycle: Task `Route Inspect Presentation`; Gray completed at `bc300ea`; Red completed at `767c146`; Green completed at `51c0960`; final presentation acceptance completed. Integrated Route Inspect acceptance is now active.
- Current result: The complete production-free Red packet covers binding/cardinality/context, operation invocation bounds, route-family composition/help, compact/expanded/JSON/diagnostic presentation, all statuses and exact public next wording, typed JSON next semantics, complete source-generated DTOs and physical layers, tri-state facts, escaping, native forms, terminal bypass, real workspaces, no-write behavior, and managed process journeys. Unit and Integration helpers moved to mirrored `Inspect/Shared/<Capability>/` ownership.
- Evidence: Release build passes with zero warnings/errors; format and `git diff --check` pass. RouteInspect Unit is `275` total: `237` accepted predecessor/profile cases pass and `38` fail only at throwing Gray group, binding, human, JSON, diagnostic, escaping, or help entrypoints. RouteInspect Integration is `65` total: `43` accepted predecessor and generated-serialization cases pass and `22` fail only because Inspect is absent from composition. Generated serialization is `1/1`. Directly affected Route List Unit is `13/14`, Route List application Integration is `6/7`, and Hosting is `3/5`; only the new truthful Inspect help expectations fail. Against the existing repository-routed managed Gray executable, RouteInspect EndToEnd is `28/28` intentional absent-registration failures; this is Red evidence, not fresh-publish acceptance.
- Review result: Final correctness review passes. Final local-improvement review returns `NO_MATERIAL_IMPROVEMENTS`; it specifically rejects uncontracted diagnostic-schema assertions and unnecessary file splitting. Generic fixture/signature cleanup remains deferred to the authorized generic-improvements branch.
- Green evidence: Release build passes with zero warnings/errors; format and `git diff --check` pass. RouteInspect Unit is `284/284`; Route List Unit is `219/219`; Shell binding is `9/9`; Shell parser is `20/20`; RouteInspect Integration is `65/65`; Route List application Integration is `7/7`; Hosting is `5/5`; pinned parser Integration is `12/12`; and freshly published managed RouteInspect EndToEnd is `28/28`. All changed/new production files are below 200 lines with a maximum of `197`; new callables remain at four or fewer parameters.
- Review result: Final Green correctness review passes. Final local-improvement review returns `NO_MATERIAL_IMPROVEMENTS` after direct evidence for duplicate-free expanded messages, Unicode-safe truncation, typed event wording, direct-safe blocked recovery, and truthful unavailable help.
- Final acceptance evidence: From the unchanged production tree at `51c0960`, the warning-free Release build, format, and `git diff --check` pass; complete Unit is `549/549`; complete Integration is `166/166`; fresh managed `win-x64` publish succeeds; and complete EndToEnd is `36/36` with no skip. This is the Task's single final complete-suite run. Native AOT remains explicitly deferred to the integrated acceptance child.
- Blockers: None. `CLI-EDGE-007` remains triaged to the later general parser-remediation branch and does not authorize an Inspect workaround.
- Next action: Use this accepted presentation boundary as the predecessor for `route-inspect-acceptance.md`; run integrated managed/native, promotion, edge, and Route Discovery closeout gates.

## Completion

Complete. The exact grammar, terminal bypass, one-operation/one-result relationship, compact/expanded/JSON/diagnostic/help projections, seven statuses, streams, exits, next-action bound, escaping, hostile-input cases, and CLI-EDGE-005 regression pass at their assigned tiers. Integrated acceptance is enabled. Move this record to `done/` only after that acceptance completes.
