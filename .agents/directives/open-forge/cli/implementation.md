---
open-forge:
  description: Implement the accepted greenfield replacement CLI below src/cli through architecture-owned foundations and closed Tasks
  tags: [LoadNow, Directive, CLI, Implementation, Architecture, Task, CSharp, DotNet, NativeAOT, Filesystem, Testing]
---

# Greenfield CLI Implementation

## Instructions

### Authority And Readiness

- Apply these instructions only to the non-shipping replacement CLI after the
  parent CLI route is selected. Do not use them for the frozen MVP or unrelated
  repository work.
- Read the current CLI Architecture, active Plan, selected parent Task, and active
  leaf Task before changing source. The command contracts define behavior. The
  Architecture defines structure. The Task defines the bounded outcome and
  allowed changes.
- Do not implement an unresolved architecture choice. The Overseer owns the
  project foundation, cross-cutting shell and Framework contracts, composition
  boundaries, and integration changes, working directly or through a bounded
  Task Mastermind. Delegate only Tasks whose architecture, classes or
  algorithms, dependencies, evidence, and stop conditions are closed.
- A local test pass does not override the Architecture. Stop and return to the
  parent context when a Task requires a new dependency, project, shared scope,
  public behavior, wire shape, filesystem guarantee, lifecycle meaning, or
  release boundary.

### Physical Workspace And Projects

- Keep replacement source, projects, and tests below `src/cli/`. Keep the
  replacement `.slnx`, `global.json`, `NuGet.Config`, `Directory.Build.props`,
  and `Directory.Packages.props` at the repository root so ordinary .NET and IDE
  workflows discover one workspace without changing directories.
- Use the accepted `root/`, `core/`, and `tests/` physical boundaries. The root
  executable depends on Core. Core never depends on the root. Keep exactly three
  runnable test projects and one test-support library unless the maintainer
  accepts a later Architecture change.
- Keep solution membership direct. Do not create solution-only folders. Use SDK
  default authored-source globs. Do not list ordinary C# files, disable default
  compile items, or link production source across projects.
- Route every C# binary, intermediate, test, publish, and package output through
  the ignored repository-root `/artifacts/` directory. No project-local `bin/`
  or `obj/` is accepted.
- Let an ordinary non-RID CLI build publish the managed `open-forge-dev` artifact
  used by local EndToEnd evidence. Use the explicit
  `OpenForgeSkipDevelopmentPublish=true` property only when that build does not
  need the artifact. Native evidence selects one supported target RID at build
  time; do not select test executables or expected versions through environment
  variables or arbitrary paths.
- Keep generated source deterministic and explicit. It is the only production
  compile-item exception.

### Source And Dependency Direction

- Keep process arguments, environment, streams, cancellation hookup, and explicit
  command registration in the root host. Keep parser mechanics, invocation,
  pipeline, presentation, output, Framework capabilities, commands, and concrete
  results in Core according to the Architecture.
- Keep Shell free of concrete command dependencies. Keep Framework capabilities
  free of parser symbols, command requests and results, renderers, and process
  writers. Let commands depend on Shell contracts and Framework facts.
- Leave a command's definitions, binding, and behavior-owning composition at its
  leaf root. For new records, interfaces, and property-only classes, and existing
  models materially changed or promoted by the current Task, use a local
  `Models/` folder under the nearest command or capability owner. Group further by
  cohesive topic once roughly five to ten models accumulate. Untouched accepted
  models remain outside a focused Task's migration. Put supporting behavior below
  its narrowest `Shared/<Capability>/` path. Promote a complete semantic unit only
  when another real consumer needs identical meaning.
- Match namespaces to physical paths. Do not use aliases, forwarding types,
  sibling-private imports, `Common`, `Utils`, or an undifferentiated `Shared`
  folder to hide ownership.
- Apply the workspace-wide C# design and style Directives to production and tests.
  Treat a class materially above 200 lines as an architecture or locality review
  trigger, not an automatic split rule.

### Standard Behavior, Exceptions, And Edge Triage

- Default to standard behavior supplied by the pinned runtime, BCL, framework,
  library, compiler, serializer, parser, and test platform. Verify the exact
  pinned version and use its documented callable surface before adding local
  policy or replacement mechanics.
- Add a workaround, compatibility shim, custom parser, raw-token recognizer, or
  replacement of a standard capability only when an explicitly accepted product
  requirement cannot be satisfied by standard behavior. Treat that choice as an
  explicit exception, not an ordinary implementation detail.
- Surface every exception in the active Task and review evidence. Record the
  unmet standard capability, user-visible effect, bounded scope, reason it is
  required, tests, documentation impact, responsible Task or role, and removal or
  re-evaluation condition. Surface the exception to the maintainer before relying
  on it. Document it publicly when callers can observe or depend on it.
- Record unusual inputs and edge cases in the active Task or the [replacement CLI
  edge-case ledger](../../../memory/working/cli-development/edge-cases.md) before
  deciding their disposition. An edge case is evidence for triage, not an
  automatic blocker, defect, or requirement to add special handling. Classify its
  reproducibility, impact, affected surface, governing contract, and whether
  standard behavior already gives a safe result. Accepted safety, public-contract,
  and required-evidence violations remain blockers after triage.
- Solve the general invariant first. Prefer one typed validation or general
  capability that also covers edge cases over a branch for one spelling, payload,
  path, platform, or fixture. Add a special case only after the general solution
  is accepted and evidence proves that an explicitly accepted product requirement
  cannot be satisfied by it. Record the special case as an exception.
- Do not turn a dependency's unusual but safe behavior into local product policy
  merely to make one example look different. Use an already accepted configuration
  or validate the dependency's typed result before considering a workaround.
  Consider an upgrade only through the explicit dependency decision and evidence
  required by the Architecture. The maintainer must explicitly accept any
  resulting product behavior.

### Construction, Parsing, And Pipeline

- Build the command tree explicitly in one root composition source. Dispatch by
  exact `System.CommandLine.Command` identity through closed
  `CliCommandBinding<TRequest, TResult>` instances. Do not add reflection,
  assembly scanning, runtime registration, service location, shell dependency
  injection, string dispatch, or an untyped operation registry.
- Let `System.CommandLine` own tokenization, option delimiters, selection, arity,
  occurrence aggregation, typed conversion, unknown symbols, parser diagnostics,
  and standard help. Read typed parse results and library-owned occurrence facts.
  Do not create a second parser or rescan raw arguments to reinterpret a spelling.
- For the current pinned `System.CommandLine` 2.0.11, accept its native long-option
  value forms: `--option value`, `--option=value`, and `--option:value`. Use the
  ordinary spaced form in generated help. Use that form in examples by default.
  An explicitly accepted command contract may demonstrate another native form
  without changing canonical help. Reverify this rule and its focused parser
  evidence when the pinned package changes.
- Do not add a delimiter guard merely to reject otherwise accepted native syntax.
  A narrowly scoped guard remains permitted only when an explicitly accepted
  public contract requires a distinction the pinned parser cannot expose through
  typed results. Record it as the exception above, keep it out of domain behavior,
  and prove every accepted native and rejected contract form directly.
- For multi-value options, use native repeated occurrences such as
  `--item one --item two` by default. Enable multiple arguments per token only
  when an explicitly accepted command contract requires `--item one two`. Use
  typed library aggregation and conversion; do not split or accumulate raw
  process strings. An explicitly accepted typed comma-list value grammar remains
  valid; parse its one typed value rather than rescanning process arguments.
- Validate the general typed value after parsing, such as a non-negative numeric
  depth or membership in the accepted view values. When occurrence policy matters,
  use library-owned occurrence information. Do not add handling for one raw
  pattern, such as `--depth= --json`, when the general typed validation and parser
  diagnostics already define a safe result.
- Form one immutable process-wide invocation and one complete command-local
  request. Do not pass `ParseResult`, writers, service collections, or unrelated
  context bags into domain capabilities.
- Use directly callable immutable stages for invocation, operation, presentation,
  rendering, output, and completion. Validate each stage before effects. Invoke
  the operation at most once, select one cached concrete renderer, write one
  primary result, and return one fixed process completion.
- Pass output writers and cancellation explicitly. Do not cache ambient console
  state or terminate the process inside Core.

### Results, Help, Diagnostics, And Serialization

- Form one concrete command result before rendering. Keep shared process facts on
  the non-wire result contract and command payloads on concrete result records.
  Serialize only concrete source-generated graphs.
- Derive standard help from the exact composed symbol tree. Add product sections
  from command bindings. Do not maintain a second command catalogue or
  post-process library help through brittle string replacement.
- Keep diagnostics bounded, escaped, redacted, and on stderr. Verbose mode must
  not change operation behavior, result, primary output, status, or exit. JSON
  stdout remains one document.
- Use `System.Text.Json` source generation with reflection disabled. Use one
  source-generated YAML context for accepted metadata models. Do not add dynamic
  resolvers or duplicate command-specific contexts for identical shapes.

### Filesystem And Mutation Safety

- Use real `System.IO` and real owned temporary resources. Do not introduce a fake
  or virtual filesystem abstraction.
- Keep lexical paths, normalized paths, resolved physical paths, and observed
  link targets as separate typed facts. Resolve physical paths one existing
  component at a time and prove containment after every link resolution. Block
  the first external transition even when a later target re-enters the root.
  Revalidate expected state immediately before effects and use ordinary managed
  BCL file operations and atomic replacement. Apply the Architecture's stable-
  workspace and cooperating-process threat boundary; do not claim adversarial
  handle identity from path sampling.
- Use managed BCL APIs first. Do not author C, C++, Rust, P/Invoke, native shims,
  helper executables, or platform production projects. Stop at Architecture if a
  guarantee required by the accepted project boundary cannot be proved. Surface
  that divergence to the maintainer before proposing exceptional machinery.
- Read-only commands create no lock, lifecycle, cache, index, or recovery state.
  Mutation commands form a command-local plan, acquire the real lock when
  required, revalidate expected state, apply bounded effects, verify the result,
  and record accepted lifecycle or recovery facts.
- Shared mutation support provides primitives, not product decisions. Every
  command retains its command-local plan, policy, ordering, findings,
  residual-state reporting, and result. The replacement CLI never automatically
  restores a target, rolls back an effect, or compensates for target effects.

### Dependencies And Native AOT

- Use only dependencies and exact versions accepted by the Architecture and
  central package file. Do not add or update a package inside a command Task.
- Keep every runtime feature, serializer, package, and test fixture trimming and
  Native-AOT compatible. Actual publish and execution evidence is required;
  source inspection and project properties are not proof.
- Keep the executable managed and BCL-first. Native AOT describes the output, not
  a license to add native source.

### Test Evidence

- Run the complete CLI test suite once at the beginning of an implementation Task
  and once at its final acceptance boundary. A complete suite from the exact
  unchanged predecessor may serve as the beginning baseline when its projects,
  executable, environment, counts, and result are recorded.
- During implementation, run the tests authored by the current Task plus every
  directly affected test boundary. Determine affected tests from changed behavior,
  shared types, consumers, composition, serialization, filesystem capabilities,
  and refactors rather than from file names alone.
- Avoid repeatedly running the complete filesystem, Integration, published-process,
  or other expensive suite during inner-loop development. Run only the focused
  affected cases until final acceptance. Fast deterministic in-memory tests may run
  more often when they improve feedback speed.
- Rerun the complete suite before acceptance after the final production, test,
  fixture, composition, or configuration change. A later documentation-only state
  update does not invalidate that executable result. If a correction follows the
  final run, rerun the affected evidence and the complete suite only when the
  correction can affect a broader boundary.
- Keep active Unit, Integration, EndToEnd, and TestSupport source under the exact
  `src/cli/tests/` boundaries in the Architecture.
- Give every test a readable display name, one durable feature trait, and one
  evidence trait. Traits refine selection and do not collapse project tiers.
- Unit tests claim only pure or directly callable behavior. Integration tests use
  production modules and real owned OS boundaries. End-to-end tests invoke the
  published executable and prove arguments, streams, exits, cancellation, and
  unchanged bytes.
- Treat removed or historical tests as candidate evidence only when a current
  Task maps their expectation to a current contract. Put accepted evidence in the
  matching active project. Do not restore preserved projects or copy old fixture
  architecture wholesale.
- Every test owns its mutable workspace, home, temporary files, Git repository,
  cache, process, and artifacts. No parallel test shares mutable state.
- Use snapshots only for stable projections. Assert safety, identity, effects,
  status, stream selection, and exits directly.

### Integration And Acceptance

- Keep implementation, focused evidence, necessary cleanup, and Task-state update
  in one coherent increment. Do not defer known structural debt into the next
  command.
- Inspect the actual changed paths, dependency direction, namespaces, consumers,
  tests, and generated artifacts. Verify focused behavior, affected regressions,
  full project boundaries, and required Native AOT execution before acceptance.
- Commit only accepted coherent boundaries. Keep remote actions, publication, and
  release outside a Task unless its exact delivery boundary authorizes them.
