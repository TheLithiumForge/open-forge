---
open-forge:
  description: Complete the already-accepted Route Inspect interactive source-collision selection
  tags: [Memory, Working, CLI, Task, Route, Inspect, Interaction, Correction, Contextual]
---

# Complete Route Inspect Interactive Collision Selection

## Task State

- State: Complete in reviewed integration candidate
  `d9f1f350426642410bcee734c6ba90e1221b5f54` over exact local baseline
  `c3642df16a1de073ddd81f0daac3a59c47018f1c`; local `develop` integration
  remains with the Overseer.
- Parent: [Route Discovery](_route-discovery.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/inspect/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/route/inspect/behavior.md).

## Outcome And Profile

- Profile: Standard, with a deliberately frozen Gray request/factory seam and
  complete Red evidence because root integration consumes that seam and the
  finite interaction matrix must not be inferred from the Green implementation.
- Review budget: one independent whole-task correctness review (`C3-REV-001`).
- Council budget: zero.
- Correction budget: one grouped cycle (`C3-COR-001`) when a material finding is
  accepted.
- Current owner: C3 Integration Mastermind.
- Completed boundary: accepted command-local behavior, Root-owned terminal
  composition, published redirected/JSON evidence, full managed and local
  `linux-x64` Native AOT evidence, safe dogfood, and independent integration
  review plus recheck.
- Next boundary: Overseer integration into local `develop`.

## Execution Capsule

### Proportionality and platform applicability

- Route Inspect is a read-only local Markdown-workspace command. This correction
  can affect only terminal interaction and the selected in-memory source; it
  creates no workspace, Git, lock, cache, lifecycle, recovery, or persistent
  state. A defect is reversible by rerunning the command or selecting an exact
  path, while existing Git remains a user-owned recovery boundary for unrelated
  repository content.
- Ordinary malformed input, end of input, cancellation, stream failure, and
  redirected execution are inside the supported failure boundary. A malicious
  same-user process controlling the terminal or workspace is not a security
  boundary for this local CLI.
- The integrated BCL-only `CliInteractiveSession`, `TextReader`, `TextWriter`,
  cancellation tokens, ordinal comparison, and invariant numeric parsing are
  sufficient. Exceptional machinery: none. No prompt dependency, PTY/terminal
  framework, retry engine, reflection, native interop, or compatibility shim is
  justified.
- Shared neutral meaning remains in `Shell/Interaction/**`. Candidate formation,
  prompt text, answer validation, one-answer policy, and collision outcomes stay
  local to Route Inspect.

### Accepted behavior and invariants

| Request and collision state | Session use | Required outcome |
| --- | --- | --- |
| Unique ID or exact path | Never | Preserve existing resolution and result behavior. |
| Ambiguous ID, human policy, prompt-capable session, valid one-based number | Once | Select that displayed path, retain the requested ID, record `interactive`, and continue with the existing graph/profile/result pipeline. |
| Ambiguous ID, human policy, prompt-capable session, exact displayed path | Once | Same result as the matching numbered choice. |
| Ambiguous ID, invalid answer or end of input | Once | Retain the existing blocked collision, every ordered candidate, and exact-path next action. |
| Ambiguous ID, cancellation while asking | Once at most | Form `interrupted` while retaining already known collision facts. |
| Ambiguous ID, JSON policy | Never | Retain the existing blocked collision. |
| Ambiguous ID, non-prompt-capable session | Never | Retain the existing blocked collision. |
| Interactive source choice whose route remains ambiguous | Once | Preserve `interactive` source identity but remain blocked by route ambiguity. |

Prompts contain every canonical candidate in strict ordinal order with one-based
numbers, use only the injected prompt writer, and never reach primary stdout.
There is no default, trimming-based alias, fuzzy selection, retry, qualifier, or
selection heuristic.

### Paths, dependencies, and integration

- Expected production: `Commands/Route/Inspect/Models/Operation/RouteInspectRequest.cs`,
  `RouteInspectBinding.cs`, `RouteInspectOperationFactory.cs`, and focused
  `Commands/Route/Inspect/Shared/Interaction/**` or
  `Shared/Resolution/**` collaborators.
- Expected evidence: mirrored Route Inspect Unit and Integration paths, reusing
  real `StringReader`, `StringWriter`, and owned temporary workspaces.
- Task-owned state: this Task only.
- Protected: `Shell/Interaction/**`, `root/OpenForge.Cli/**`, generic Shell
  binding/invocation/pipeline, JSON context and schemas, help, renderers, other
  Route commands, Framework source semantics, and every existing finite result
  value.
- Integrated neighborhood: `CliHost` owns `Console.In`, stderr, and both
  redirection facts; `CliCompositionRoot` derives prompt capability, constructs
  the native session, and injects it only into Route Inspect. The explicit host
  overload remains noninteractive. Published-process evidence proves redirected
  human and JSON noninteraction.

### Evidence ladder

- Beginning baseline from this worktree: warning-free Release build; focused
  Shell interaction and Route Inspect Unit `110/110`; focused Route Inspect
  Integration `70/70`.
- Gray: Release compile and exact callable-surface inspection.
- Red: complete focused Unit/Integration interaction matrix, with every intended
  failure reaching only the missing command-local behavior.
- Green and improvement: focused Unit/Integration plus directly affected Route
  Inspect binding, resolution, profile, result, and presentation regressions.
- Acceptance: locked offline-prepared restore, warning-free Release build,
  format and diff checks, complete managed Unit/Integration/EndToEnd, supported
  local `linux-x64` Native AOT root/Integration/EndToEnd execution, one safe
  isolated noninteractive dogfood scenario, and `C3-REV-001`.
- Direct composed/injected-session Integration evidence proves prompt-capable
  behavior. Published evidence proves redirected-human and JSON noninteraction.
  An actual terminal-process proof is explicitly outside this acceptance scope:
  the existing capture harness redirects stderr, and no PTY, `script`, native,
  or platform-specific harness is justified or accepted.

### Stop conditions

Stop and return to the Overseer before changing accepted prompt grammar,
interaction capability, result/schema/rendering meaning, shared Shell behavior,
root composition, or another command; before adding retry/default/qualifier
policy, a dependency, terminal framework, reflection, or native interop; or if
the existing graph cannot be reused after one selected candidate.

## Progress, Findings, And Corrections

- Preflight: no product or architecture divergence found. The correction remains
  read-only, BCL-only, command-local, and able to reuse the existing catalogue,
  projection, route-fact, profile, result, and rendering flow.
- Gray: `RouteInspectRequest` now carries the binder-owned explicit human-policy
  Boolean, while `RouteInspectOperationFactory.Create(CliInteractiveSession)`
  freezes the only integration seam and fails explicitly until Green. Existing
  parameterless composition remains noninteractive and unchanged.
- Gray evidence: warning-free Release solution build and `git diff --check`
  pass. No protected Shell, root, serialization, help, renderer, or other Route
  path changed.
- Red: one focused binding assertion proves human requests allow interaction and
  JSON requests do not. Ten Integration cases freeze valid number and path
  selection, one-answer invalid/EOF behavior, cancellation with retained
  collision facts, JSON and redirected noninteraction, stderr/stdout separation,
  ambiguous-route preservation, unambiguous-source noninteraction, exact status,
  selection, observation, next-action, and no-write meaning.
- Red evidence: warning-free Release solution build; focused binding Unit
  `14/14` passes; focused interaction Integration fails `10/10`, and every case
  terminates only at the named Gray `NotSupportedException` in
  `RouteInspectOperationFactory.Create(CliInteractiveSession)`. No setup,
  compilation, environment, or unrelated behavior failure remains.
- Green: a focused Route Inspect selector now owns prompt formation, invariant
  one-based and exact-ordinal answer validation, and the one-answer policy. The
  async collision resolver passes the already-formed collision selection once,
  retains it through cancellation or an unusable answer, and reuses the existing
  catalogue, projection, route-fact, profile, result, and rendering pipeline for
  a valid choice. The parameterless factory remains noninteractive.
- Green evidence: warning-free Release solution build; focused interaction
  Integration `10/10`; affected Route Inspect and Shell-interaction Unit
  `110/110`; affected Route Inspect Integration `80/80`; format made no changes;
  and `git diff --check` passes.
- Red correction: the first Green run exposed two test-oracle defects rather
  than production defects. The compact renderer's established sentence is
  `source ID; interactive selection; requested ...`, and the deliberately
  duplicate route fixture correctly produces more than one distinct
  `AmbiguousRoute` condition. Evidence now asserts the full established
  selection sentence and the intended invariant that route ambiguity remains,
  without constraining unrelated condition cardinality.
- Acceptance: locked restore completed against the prepared empty offline
  source, the warning-free Release solution build published the managed
  development root, and the complete managed suites passed Unit `1235/1235`,
  Integration `491/491`, and EndToEnd `125/125`, all with zero skips. The local
  `linux-x64` Native AOT root published and passed the managed process suite
  `125/125`; the Native AOT Integration and EndToEnd executables passed
  `491/491` and `125/125`, also with zero skips.
- Formatting: the inherited `DOTNET_ROOT=/home/tedy/.dotnet` initially made
  `dotnet format` load stale SDK 9 analyzer assets even though `/usr/bin/dotnet`
  selected SDK 10.0.111. A command-local `DOTNET_ROOT=/usr/lib/dotnet` correction
  loaded the actual SDK 10 analyzers and verified all 843 files without changes.
  No repository workaround or configuration change was added.
- Dogfood: the fresh native root ran Route List and Route Inspect against this
  isolated feature worktree. Route Inspect resolved `directives/csharp` by its
  automatic ID and truthfully reported the repository's existing incomplete
  loading facts. Git status and diff remained clean after execution.
- Review `C3-REV-001`: independent Sol/xhigh review of `e782090c..0ae392ae`
  found no material issue. Its focused confirmation passed Unit `22/22` and
  interaction Integration `10/10`; static tracing confirmed policy and prompt
  capability gates, exact-path and unique-ID bypass, one prompt/read,
  ordinal/invariant parsing, cancellation fact retention, physical containment,
  graph reuse, and protected-surface preservation. A dedicated injected-session
  exact-path noninteraction test was assessed nonmaterial because the typed
  source-path branch cannot reach the ID-collision selector and existing
  exact-path evidence covers its behavior.
- Integration: the four command-local commits map onto current baseline as
  `2e087fcf`, `af0b6206`, `60146fb5`, and `017f81ee`; Root composition and
  published-process evidence are `d9f1f350`. `CliHost` is the smallest mandatory
  neighboring seam because only Root owns ambient Console and redirection facts.
  Generic invocation/request models, Shell interaction behavior, JSON schemas,
  help, renderers, unrelated commands, and other Route production remain
  unchanged.
- Integrated evidence: local-cache-only locked restore used one empty local
  NuGet source with audit disabled; Release build passed with zero warnings and
  errors. Focused Unit passed Route Inspect `131/131` and Shell interaction
  `8/8`; focused Integration `82/82`; and published Route Inspect EndToEnd
  `32/32`. Full managed Unit `1284/1284`, Integration `511/511`, and EndToEnd
  `125/125` passed. Local `linux-x64` Native AOT root version smoke passed;
  Native AOT Integration `511/511` and EndToEnd `125/125` passed. Every stated
  run had zero skips. SDK-10 format and diff checks passed, and native dogfood
  left Git status and content diff unchanged.
- Integration review `C3-INT-REV-001`: independent Sol/xhigh review returned
  `PASS` with no material findings. After the docs-only rebase onto `c3642df1`,
  all 19 C3 path blobs remained byte-identical, all five commit patch IDs
  remained identical, focused evidence passed again, and the same reviewer
  returned `PASS_RECHECK`. The accepted deferred real-terminal process proof is
  an evidence-scope residual only, not a product limitation or workaround.

## Outcome

When a prompt-capable human request uses a non-unique automatic source ID, Route
Inspect lists the exact candidate paths on stderr, asks once, and accepts either
the one-based displayed number or one exact displayed path. Invalid input or end
of input retains the existing blocked collision; cancellation is interrupted.
JSON and redirected requests retain the blocked exact-path behavior and never
prompt.

This is a post-completion conformance correction. It does not reopen Route
Inspect profile, measurement, loading, topology, rendering, or promotion meaning.

## Ownership

- Production: `Commands/Route/Inspect/**` only.
- Evidence: focused Route Inspect Unit and Integration tests.
- Protected: `Shell/Interaction/**`, root composition/help, shared serialization,
  other Route commands, and all existing result schemas and finite values.
- Integration owns the exact root composition delta and published-process
  noninteractive evidence.

## Evidence

Cover ordinal candidate display, one valid number, one exact displayed path,
blocked invalid answer/end of input, interrupted cancellation, `interactive`
selection method, retained `attention` and exact-path next action, ambiguous-route
blocking, no stdout prompt text, and no session call for JSON or redirected
input. Re-run directly affected Route Inspect regressions.

## Stop Conditions

Stop before adding a qualifier syntax, selection heuristic, prompt library,
general retry/default policy, renderer/schema change, or any feature addition or
removal not accepted by the maintainer.
