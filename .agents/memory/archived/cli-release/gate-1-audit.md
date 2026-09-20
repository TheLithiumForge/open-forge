---
open-forge:
  description: Integrated Gate 1 evidence audit, provenance, current truth, job model, and unresolved CLI proposal map
  tags: [Memory, Archived, CLI, Release, Gate, Audit, Evidence, Contextual, Historical]
---

# Gate 1 CLI Evidence Audit

## Status

This is contextual audit evidence, not accepted CLI design. Gate 1 inspection is
complete. No CLI implementation, architecture, command taxonomy, cleanup, or
Rune work was accepted by this audit.

## Maintainer Corrections

The maintainer later established these current boundaries:

- `src/cli-mvp/` is completely frozen. Do not modify, build, test, repair, or
  otherwise exercise it during new-CLI development. Use only the
  `open-forge-old` executable when repository routing assistance is needed.
- Deleted CLI-v2 material and every raw report or handoff artifact are raw input.
  No job, command, flag, library, safety rule, test strategy, or architecture
  carries forward without current evidence and maintainer acceptance.
- The new CLI is an optional agent-first Framework accelerator with a
  predictable human maintenance surface. It has no target command count.
- .NET Native AOT is the implementation direction. Detailed choices remain open
  and must be fully Native AOT and trimming compatible.
- Thin wrappers do not implement Framework behavior. npm is the first wrapper.
- The root `package.json` remains an ecosystem-neutral orchestration layer.
- Rune is entirely outside this effort.
- Deleted CLI-v2 knowledge is preserved under
  `.agents/memory/archived/cli-v2/` for later inspection.

These corrections supersede contrary classifications below. The dated audit
provenance remains useful evidence of the inspected state.

## Provenance

- Audit date: 2026-08-10.
- Repository branch: `feature/redesign`.
- Inspected commit: `9feac1d2607e716c6415522ddef463867e764a66` plus the existing working tree.
- The working tree already contained 55 changed or untracked entries before
  these records were added. Several files had distinct HEAD, index, and worktree
  states. Audit claims therefore describe the inspected worktree unless a commit
  is named explicitly.
- Raw ignored evidence under `.temp/redesing-raw-data/` contains 199 files.
- No Gate 1 file was treated as authoritative merely because its own prose says
  `accepted`, `approved`, `final`, or `CurrentTruth`.
- No restore, compilation, Native AOT publication, wrapper publication, or
  implementation test run was used to call the supplied prototype working.

## Specialist Coverage

| Area                                      | Specialist role     | Result                                                                                         |
| ----------------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------- |
| Raw proposals and competing analyses      | Analyst             | Complete source and command inventory; evidence independence qualified                         |
| Native handoff and prototype              | Architect           | All 187 handoff-package files inspected; source-present versus verified separated              |
| Current implementation and tooling        | Explorer            | MVP, residual CLI build files, package, generated output, tests, and missing surfaces mapped   |
| Current Documents                         | Explorer            | CLI, public, development, source-map, and Framework links audited                              |
| Decisions                                 | Analyst             | All CLI Decisions and cross-route CLI rationale classified                                     |
| Directives, Patterns, and reuse artifacts | Explorer            | Language-neutral, Bun-specific, and deleted-v2 dependencies separated                          |
| Working, Emerging, and Archived Memory    | Explorer            | Lifecycle state and stale operational narration mapped                                         |
| Repository-wide references                | Explore             | Current, historical, generated, candidate, and stale hits classified                           |
| User and agent jobs                       | Experience designer | Job model derived independently of command names                                               |
| Framework constraints                     | Contract builder    | Accepted semantics separated from MVP implementation gaps                                      |
| Final audit challenge                     | Acceptance reviewer | Useful readiness gaps found; verdict was stage-misaligned because no design claimed acceptance |
| Meta-reconciliation                       | Analyst             | Genuine gaps separated from reviewer misunderstanding                                          |

The roles were capable enough for Gate 1 exploration. The missing integration
was an exhaustive durable per-file matrix and authority reconciliation, which
the records in this route now own. Additional Native, release, security, and
performance specialists belong in Gate 3 or Gate 5 after their decision inputs
are accepted.

## Current Repository Truth

1. `build.ts` builds `src/cli-mvp/cli.ts`, not a replacement CLI.
2. `package.json` maps both `open-forge` and `open-forge-old` to `dist/cli.mjs`.
3. The built source identifies itself as `open-forge-old`.
4. Only `src/cli/build/{embedded-assets,generate-build-module,node-shebang}.ts`
   remain from CLI v2.
5. The generated embedded-assets module is not consumed by the MVP executable.
6. The package now includes an adjacent Framework payload. Packaged Extension
   catalogue behavior remains unproven and is irrelevant to the frozen boundary.
7. Current `check` scripts call a missing `typecheck` package script.
8. MVP closure tests refer to missing `tests/support/index.ts`.
9. No current root CI or release workflow exists.
10. CLI-v2 Documents, Decisions, Directives, Patterns, plans, and implementation
    records have now been moved into the historical CLI-v2 archive.
11. The old MVP does not implement current target-sensitive `#KeepInMind` loading
    or the eventual fail-closed metadata and overwrite contract.
12. Current Framework semantics remain authoritative independently of the CLI.
13. Current source and the locally exposed `open-forge-old` executable both
    validate direct Directive `## Instructions`. The earlier divergence was resolved.
14. `open-forge.extensions.json` is divergent from the installed workspace: 9
    declared files match, 11 template files have digest mismatches, and
    `.agents/workflows/development.md` is missing while a user-owned routed
    Development subtree now occupies `.agents/workflows/development/`.

## Source And Authority Map

| Source                               | Responsibility                    | Authority                                | Independence                      | Gate 1 disposition          |
| ------------------------------------ | --------------------------------- | ---------------------------------------- | --------------------------------- | --------------------------- |
| Current loader and Framework sources | Runtime Framework meaning         | Current                                  | Primary                           | Keep authoritative          |
| `src/cli-mvp/`                       | Frozen old executable behavior    | Current implementation truth for old CLI | Primary source                    | Preserve until migration    |
| `docs/cli.md`                        | Public MVP behavior               | Current but partially stale              | Primary documentation             | Mark legacy/reconcile later |
| Deleted CLI-v2 knowledge archive     | Former replacement direction      | Historical raw input                     | Primary historical rationale      | Inspect only when useful    |
| Raw CLI analyses                     | Competing proposals and critiques | Contextual                               | Mixed                             | Preserve alternatives       |
| v4 master review                     | Compilation of reports            | Contextual                               | Duplicate                         | Index only                  |
| v4 change log                        | Summary                           | Contextual                               | Duplicate                         | Index only                  |
| Native handoff                       | Concrete design/prototype package | Contextual and unverified                | Derivative implementation handoff | Raw input for Native design |
| Native wrappers/providers            | Packaging examples                | Speculative                              | Not evidence                      | Defer                       |
| Rune sources                         | Separate future effort            | Outside scope                            | Not release evidence              | Ignore                      |

## Evidence Quality

- The master review embeds standalone reports and adds no corroboration.
- The change log is a derivative index.
- Eight of eleven critique perspectives inherited earlier findings; agreement
  was often propagation, not replication.
- The 16-leaf redesign depends mainly on one trace and estimated unimplemented
  command output.
- The minimal counter-case corrects material evidence errors but assumes baseline
  context bodies were already loaded for its path-projection comparison.
- The v4 reports did not inspect the deleted TypeScript implementation, tests,
  build, or package configuration.
- The handoff explicitly records that restore, compilation, tests, AOT publish,
  binary execution, and wrapper parity were not performed.
- Source existence and test-source existence are not working evidence.
- The current Extension receipt is not proof of healthy managed state; a dry-run
  Extension operation blocks on modified or missing receipt-owned content.

## Current Framework Constraints

- Plain Markdown remains semantically complete without the CLI.
- The CLI may accelerate and validate but may not privately determine meaning.
- Recursive scope selection and flexible routed scope placement are essential.
- Tagged entrypoints use target-sensitive `#KeepInMind` selection.
- Ordinary routed `#KeepInMind` files remain globally recoverable.
- `Axioms` exist only in the loader and recognized entrypoints.
- Checkpoints and Handoffs remain the standard distinct resumability roles.
- Overwrites are paired, visible, non-independent, and final only for the base
  file's question within its scope.
- Generated `Entries` are derived navigation.
- Authored metadata and generated-region integrity must eventually fail closed.
- Existing user-owned content must not be silently removed or claimed.
- Stable capabilities outrank temporary CLI spelling in Framework sources.
- The frozen CLI remains behaviorally behind current target-sensitive loading,
  but the exposed executable accepts the current direct Directive shape.

## Historical Audit Job Model

This table is raw Gate 1 analysis. It does not define the new CLI's jobs. Product
and capability discovery must restart from current Framework use with independent
lenses and maintainer discussion.

| Job                                | CLI need                       | Plain-file alternative               | Primary value                       |
| ---------------------------------- | ------------------------------ | ------------------------------------ | ----------------------------------- |
| Workspace identity and orientation | Useful                         | Inspect loader and repository        | Prevent wrong-root work             |
| Scoped context assembly            | Necessary accelerator          | Traverse routes manually             | Highest context/token saving        |
| Route and source discovery         | Necessary accelerator          | Search paths, descriptions, and tags | Fast literal retrieval              |
| Inspection and explanation         | Useful projection              | Read ancestors and links             | Provenance without full bodies      |
| Validation                         | Necessary                      | Manual structural review             | Deterministic correctness gate      |
| Authoring                          | Useful, not proven essential   | Edit Markdown directly               | Reduce mechanical errors            |
| Generated-index maintenance        | Necessary mechanism            | Edit generated Entries manually      | Preserve derived navigation         |
| Repair and mutation                | Useful with separate authority | Deliberate file edits                | Safety and repeatability            |
| Diagnostics                        | Cross-cutting                  | Inspect evidence manually            | Actionable failure recovery         |
| Automation and JSON                | Necessary cross-cutting        | Shell composition                    | Stable agent and CI behavior        |
| Framework installation             | Necessary but infrequent       | Copy/reconcile files manually        | Safe initial lifecycle              |
| Extension lifecycle                | Potentially necessary          | Copy optional packages manually      | Ownership and update safety         |
| CLI installation/update            | Distribution concern           | Package manager                      | Availability, not Framework meaning |

## Command Proposal Inventory

### Frozen MVP

`install`, `extend`, `index`, `load`, `find`, `chain`, `doctor`, `create`.

### Former 19-Leaf Replacement

`status`, `context`, `find`, `doctor`, `repair`, `create`, `install`,
`route list`, `route inspect`, `route init`, `route rebuild`,
`extension list`, `extension inspect`, `extension add`, `extension update`,
`extension remove`, `completion install`, `completion remove`, and
`completion script`.

### 16-Leaf Jobs Redesign

`enter`, `read`, `rules`, `list`, `show`, `doctor`, `repair`, `create`, `init`,
`reindex`, `install`, four Extension lifecycle leaves, and `completion script`.

### Minimal Counter-Case

`load`, `find`, `doctor --fix`, plus one-time Framework installation.

### v4 17-Leaf Proposal

`status`, `context`, `find`, `check`, `fix`, `create`, `move`, `remove`,
`apply`, `install`, `route init`, five Extension leaves, and
`completion script`.

### Native Handoff

The catalogue resembles v4 but changes `route init` to `route create`. Only
`status`, `context`, `find`, and `check` have source-present command handlers.

## Material Conflict Register

- Product: minimal accelerator versus lifecycle kernel versus full operator.
- Context: `load`, `context`, `enter`, or `read`.
- Validation: `doctor` or `check`.
- Repair: `doctor --fix`, `repair`, `fix`, or targeted mutations only.
- Routes: separate group versus projections; `init` versus `create`.
- Index: public operation, automatic postcondition, or safe repair.
- Orientation: separate `status` versus folded result.
- State: stateless complete output versus receipts and `--since`.
- Workspace: exact CWD versus upward discovery and opt-out.
- Mutation: targeted operations versus generic move/remove/apply.
- Recovery: Git-first/backups versus lock and journal system.
- Formatting: CLI orchestration versus external ownership.
- Completion: script only versus profile lifecycle.
- Distribution: native archives, NuGet, npm, PyPI, Homebrew, WinGet, Scoop.
- Technology: TypeScript lineage versus .NET Native AOT or another candidate.

## Provisional Gate 1 Recommendation

The evidence currently favors discussing a bounded deterministic lifecycle
kernel before either the three-command extreme or a 17-19-leaf workspace
operator. It also favors stateless context with path projections, read-only
validation separated from mutation, and deferring generic structural mutations.
This is an audit recommendation for discussion, not an accepted Decision.

## Deferred Verification

If .NET Native AOT remains a live Gate 3 candidate, independently verify restore,
compile, tests, AOT publish, binary startup, representative commands, trimming,
source generation, package size, startup, platform behavior, and wrapper parity.
Do not reuse the supplied prototype before those checks.
