---
open-forge:
  description: "Historical CLI-v2 source: Explicit domain diagnosis, typed findings, completeness, stable ordering, safe repair projection, conflict handling, and rediagnosis semantics"
  responsibility: Define one predictable doctor and repair composition without resolving behavior through diagnostic strings
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Diagnosis And Repair Contract

## Scope

The CLI interface owns the `doctor` and `repair` commands and public result
envelope. Domain contracts own the invariants they diagnose. Mutation and
recovery contracts own application safety. Production TypeScript owns exact
finding unions, codes, and function signatures.

## Guarantees

### Explicit Composition

Doctor is one user experience, not one validation module and not a dynamic
plugin registry.

The root diagnostic coordinator directly imports and calls each accepted
domain:

```text
inspectWorkspace()
diagnoseRecovery(workspace)
diagnoseRoutes(workspace)
diagnoseReferences(routes)
diagnoseFramework(workspace)
diagnoseExtensions(workspace)
```

The calls and their order are visible in one focused coordinator. A diagnostic
code is serialized evidence only. No code string, module name, manifest entry,
or dependency-container key resolves a check or repair function.

When a domain becomes too broad, it composes focused direct functions inside
that domain. New domains require an explicit coordinator edit and a typed
finding-union update.

### Shared Facts

Diagnosis reuses the same invocation facts as read commands:

- Selected workspace and containment observations.
- Durable recovery inspection.
- Authored route inventory and generated-navigation comparison.
- Local-reference inventory.
- Embedded Framework and installed Extension lifecycle inventories.

Consumers do not independently reopen files or parse private variants.
Expected divergence discovered while constructing facts becomes a typed
finding. Unexpected inability to construct required facts becomes explicit
incompleteness or failure.

Initial execution is deterministic and sequential. Parallel diagnosis may be
introduced only after measurements prove it preserves bounded reads, finding
order, error attribution, and the same snapshot semantics.

### Domain Reports

Each domain returns:

- Named domain.
- Completeness state.
- Typed findings in deterministic domain order.
- Internal mechanically safe repair proposals, if any.
- Useful inspected counts and boundaries.

Completeness states distinguish:

| State      | Meaning                                                                                      |
| ---------- | -------------------------------------------------------------------------------------------- |
| Complete   | Every check in the selected domain boundary ran                                              |
| Incomplete | Safe partial facts exist, but a named limit or unavailable source prevented a complete claim |
| Blocked    | The domain boundary could not be inspected safely                                            |

An expected malformed file, stale index, missing target, malformed workspace
state, backup collision, or visible residual is
a finding, not an exception. An unexpected implementation or runtime failure
reaches the outer safety boundary and returns `failed`.

### Finding Shape

Every public finding contains:

- Stable domain-owned code.
- Named severity.
- Legible message.
- Typed subject.
- Domain-specific typed evidence.
- Named resolution class.
- Zero or more typed next actions.

Severities are:

| Severity    | Meaning                                                                        |
| ----------- | ------------------------------------------------------------------------------ |
| Information | Established fact useful to understand state                                    |
| Warning     | A contract is incomplete or degraded but the inspected boundary remains usable |
| Error       | A contract is invalid, unsafe, or unusable                                     |

Resolution classes are:

| Resolution      | Meaning                                                                            |
| --------------- | ---------------------------------------------------------------------------------- |
| Safe repair     | The same report carries a deterministic meaning-preserving repair proposal         |
| Manual decision | Authored meaning, authority, ownership, or destructive intent is required          |
| Blocked repair  | A repair would otherwise be mechanical but current capability or state prevents it |
| Informational   | No repair is required                                                              |

Severity does not choose fixability, result status, or exit code.

Subjects and evidence remain a discriminated domain union. A route finding can
name route and source; a reference finding can name source position and
destination; a recovery finding can name a backup, residual target, Git state,
or lifecycle disagreement. The
common finding does not accumulate universal optional path, line, route,
extension, effect, backup, or Git fields.

### Actions And Suggestions

An action is either:

- A typed CLI invocation using a known operation id and explicit arguments.
- A legible manual instruction.

Display adapters format command invocations. Domain logic does not concatenate
a shell command string and does not quote for a caller's shell.

Suggestions are advisory evidence. They never change finding classification,
select a target, authorize mutation, or execute automatically. A fuzzy or
ambiguous candidate may help manual work but never becomes a safe repair
proposal.

### Stable Ordering

Doctor concatenates complete domain reports in one named order:

1. Workspace and entry.
2. Recovery.
3. Routes and generated navigation.
4. Local references.
5. Framework lifecycle.
6. Extension lifecycle.

Each domain owns a runtime-independent comparator over its actual subject.
Source paths, positions, and named codes are common deterministic keys.
Process locale and asynchronous completion order never affect public ordering.

### Doctor Outcome

Bare doctor performs no writes, creates no backup or temporary material, and
changes no Git or lifecycle state.

| Result      | Condition                                                                                |
| ----------- | ---------------------------------------------------------------------------------------- |
| `success`   | Complete diagnosis with no warning or error findings                                     |
| `attention` | Complete diagnosis with one or more warning or error findings                            |
| `blocked`   | A valid diagnosis could not cover a required boundary safely or reached a declared limit |
| `failed`    | Unexpected execution prevented trustworthy diagnosis                                     |
| `cancelled` | The caller cancelled before completion                                                   |

A blocked result may retain clearly labelled partial findings and counts. It
never describes them as a complete health report.

Human output begins with the outcome and counts, then groups findings by domain
without hiding their stable order:

```text
Health: attention
Checked: 282 routes, 1,679 references
Findings: 2 repairable, 1 manual

ROUTE.GENERATED-STALE
  .agents/memory/archived/ideas/_ideas.md
  Generated Entries differ from authored direct routes.
  Repair: open-forge repair
```

Exact capitalization and decoration belong to the display adapter. JSON uses
registered protocol values.

### Internal Repair Proposals

A safe-repair finding and its internal proposal are produced together by the
domain diagnosis. The public finding code does not retrieve the proposal later.

The repair coordinator directly calls domain planners:

```text
planResidualRepairs(report.recovery)
planRouteRepairs(report.routes)
planReferenceRepairs(report.references)
planFrameworkRepairs(report.framework)
planExtensionRepairs(report.extensions)
```

Each planner receives its typed domain report. It returns typed purposes over
the shared mechanical effects. There is no generic fixer map.

### Repair Planning

Adjacent backups, temporary residuals, lifecycle disagreement, and post-stop
mixed state participate like other findings. They produce a safe proposal only
when one exact restoration or cleanup result is mechanically proven. Ambiguous
evidence remains manual and does not block unrelated safe findings unless the
plans overlap the same target.

Planning proceeds as follows:

1. Diagnose the complete applicable Open Forge surface.
2. Collect every safe repair proposal.
3. Convert proposals into one complete ordered mutation plan.
4. Detect duplicate targets, incompatible expected states, authority overlap,
   and effect-order conflicts.
5. Preflight the complete plan.

Equivalent proposals for the same exact resulting bytes may be coalesced only
through an explicit typed rule that retains every originating finding.
Incompatible proposals block the complete repair before mutation. Order never
decides a conflict.

Manual, informational, and blocked-repair findings produce no effects.

### Preview, Application, And Rediagnosis

`repair --dry-run` returns after complete plan construction and preflight. It
shows selected findings, effects, unchanged findings, and blockers through the
ordinary typed result. It never creates a reusable authorization token.

`repair` applies the preflighted plan through the accepted recovery policy,
verifies the complete operation, then creates a fresh fact snapshot and reruns
all diagnosis.

The final result distinguishes:

- Findings repaired.
- Findings still present.
- New findings.
- Manual decisions.
- Blocked repairs.
- Mutation verification, recovery, and residual evidence.

If no safe repair exists, repair changes nothing and returns `attention` with
the remaining findings. If every repair succeeds and no warning or error
remains, it returns `success`. Application or verification failure remains
`failed` even when rollback succeeds.

## Boundaries

`doctor` is always read-only. `repair` may plan only proposals produced with a
finding whose authored meaning and mechanical correction are proven together.
Suggestions, severity, diagnostic strings, asynchronous completion order, and
planner order never select behavior or resolve conflicting authority.

## Verification

Test each domain diagnosis and planner directly with real focused sources.
Test the coordinator with several real domains to prove ordering,
completeness, proposal conflicts, recovery precedence, and result projection.

Process-level tests prove read-only doctor, JSON cleanliness, dry-run
non-mutation, one applied repair journey, rediagnosis, and interruption
recovery. Snapshots cover complete human and JSON output; semantic assertions
protect read-only and safety invariants.

## Related Current Sources

- [CLI interface](../interface.md)
- [Mutation execution](mutation-execution.md)
- [Workspace recovery](workspace-recovery.md)
- [Route inventory](route-inventory.md)
- [Local references](local-references.md)
