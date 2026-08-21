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
- Do not implement an unresolved architecture choice. The Mastermind directly
  authors the project foundation, cross-cutting shell and Framework contracts,
  composition boundaries, and integration changes. Delegate only Tasks whose
  architecture, classes or algorithms, dependencies, evidence, and stop
  conditions are closed.
- A local test pass does not override the Architecture. Stop and return to the
  parent context when a Task requires a new dependency, project, shared scope,
  public behavior, wire shape, filesystem guarantee, lifecycle meaning, or
  release boundary.

### Physical Workspace And Projects

- Keep all replacement-specific C# workspace configuration, source, projects,
  tests, and generated C# artifacts below `src/cli/`. Do not add a replacement
  `.slnx`, `global.json`, `NuGet.Config`, `Directory.Build.props`, or
  `Directory.Packages.props` at the repository root.
- Use the accepted `root/`, `core/`, and `tests/` physical boundaries. The root
  executable depends on Core. Core never depends on the root. Keep exactly three
  runnable test projects and one test-support library unless the maintainer
  accepts a later Architecture change.
- Keep solution membership direct. Do not create solution-only folders. Use SDK
  default authored-source globs. Do not list ordinary C# files, disable default
  compile items, or link production source across projects.
- Route every C# binary, intermediate, test, publish, and package output through
  `src/cli/artifacts/`. No project-local `bin/` or `obj/` is accepted.
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
- Leave a command's definitions, binding, request, operation, and concrete result
  at its leaf root. Put supporting source below its narrowest
  `Shared/<Capability>/` path. Promote a complete semantic unit only when another
  real consumer needs identical meaning.
- Match namespaces to physical paths. Do not use aliases, forwarding types,
  sibling-private imports, `Common`, `Utils`, or an undifferentiated `Shared`
  folder to hide ownership.
- Apply the workspace-wide C# design and style Directives to production and tests.
  Treat a class materially above 200 lines as an architecture or locality review
  trigger, not an automatic split rule.

### Construction, Parsing, And Pipeline

- Build the command tree explicitly in one root composition source. Dispatch by
  exact `System.CommandLine.Command` identity through closed
  `CliCommandBinding<TRequest, TResult>` instances. Do not add reflection,
  assembly scanning, runtime registration, service location, shell dependency
  injection, string dispatch, or an untyped operation registry.
- Let `System.CommandLine` own selection, arity, occurrence aggregation, typed
  conversion, unknown symbols, parser diagnostics, and standard help. Read typed
  parse results. Do not create a second parser. Keep any accepted delimiter guard
  limited to the exact syntax fact the library cannot expose.
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
- Keep lexical paths, normalized paths, physical identities, and link targets as
  separate typed facts. Resolve physical paths one existing component at a time
  and prove containment after every link resolution. Block the first external
  transition even when a later target re-enters the root.
- Use managed BCL APIs first. Do not author C, C++, Rust, P/Invoke, native shims,
  helper executables, or platform production projects. Stop at Architecture if a
  critical guarantee cannot be proved.
- Read-only commands create no lock, lifecycle, cache, index, or recovery state.
  Mutation commands form a command-local plan, acquire the real lock when
  required, revalidate expected state, apply bounded effects, verify the result,
  and record accepted lifecycle or recovery facts.
- Shared mutation support provides primitives, not product decisions. Every
  command retains its own plan, ordering, findings, compensation or rollback
  meaning, and result.

### Dependencies And Native AOT

- Use only dependencies and exact versions accepted by the Architecture and
  central package file. Do not add or update a package inside a command Task.
- Keep every runtime feature, serializer, package, and test fixture trimming and
  Native-AOT compatible. Actual publish and execution evidence is required;
  source inspection and project properties are not proof.
- Keep the executable managed and BCL-first. Native AOT describes the output, not
  a license to add native source.

### Test Evidence

- Keep active Unit, Integration, EndToEnd, and TestSupport source under the exact
  `src/cli/tests/` boundaries in the Architecture.
- Give every test a readable display name, one durable feature trait, and one
  evidence trait. Traits refine selection and do not collapse project tiers.
- Unit tests claim only pure or directly callable behavior. Integration tests use
  production modules and real owned OS boundaries. End-to-end tests invoke the
  published executable and prove arguments, streams, exits, cancellation, and
  unchanged bytes.
- Treat `src/cli/tests/preserved/` as candidate evidence. Map a preserved test to
  a current contract before porting it. Do not compile preserved projects or copy
  old fixture architecture wholesale.
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
