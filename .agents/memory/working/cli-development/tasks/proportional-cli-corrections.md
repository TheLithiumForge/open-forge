---
open-forge:
  description: Correct confirmed read-only CLI defects and preserve proportionate architecture findings before Mutation Foundation resumes
  tags: [Memory, Working, CLI, Task, Audit, Correctness, Architecture, Proportionality, Contextual, Complete]
---

# Correct Proportional CLI Findings

## Task State

- State: Complete.
- Responsible role: Overseer, with one bounded Task Mastermind for each parallel
  correction lane.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Accepted base: local `develop` commit `bba84b6`.
- Integrated boundary: proportionate guidance `5f9f59e`, Route List correction
  `2cd525d`, and Markdown correction `0d88606`.
- Required predecessor: the proportional-development and CLI threat-boundary
  package committed at feature commit `0f40622`.
- Last updated: 2026-08-27.

## Outcome

Preserve the whole-CLI proportionality audit with stable dispositions, correct
the two confirmed read-only behavior defects, remove only production support
proved to be dead, and return a reviewed green baseline before Mutation
Foundation resumes. Do not turn this correction into a broad cleanup or change
accepted public command meaning beyond the named defects.

## Authority

- [Proportionate Development Directive](../../../../directives/proportional-development.md)
- [CLI Architecture](../../../crystallized/documents/cli/architecture.md)
- [CLI implementation Directive](../../../../directives/open-forge/cli/implementation.md)
- [C# design](../../../../directives/csharp/design.md) and
  [C# style](../../../../directives/csharp/style.md)
- [Review Evidence Directive](../../../../directives/review-evidence.md)
- [Development Plan](../plan.md)

## Accepted Findings And Dispositions

| ID      | Severity | Finding | Disposition and earliest boundary |
| ------- | -------- | ------- | --------------------------------- |
| PCF-001 | Medium   | `RouteListTextEscaping` can append an ellipsis after consuming the complete limit, and raw clamping can split an escape or Unicode scalar. | Resolved at `2cd525d`: configured limits are total UTF-16 output bounds including the truncation marker; complete escape tokens and scalar boundaries are preserved. |
| PCF-002 | High     | The shared Markdown generated-region parser admits canonical visible `Entries` text with trailing heading whitespace, while Extension Inspect fingerprinting requires the exact raw `## Entries` line. | Resolved at `0d88606`: one authoritative parsed fact owns exact region grammar and distinguishes invalid grammar from unavailable parsing; fingerprinting maps that fact without a second grammar. |
| PCF-003 | High     | Earlier `physical identity` wording overstated what managed resolved-path observation proves. | Resolved at `5f9f59e`: the accepted boundary is managed-API-observable links and reparse points in a stable workspace, not inode, file-ID, hidden mount, or adversarial handle identity. |
| PCF-004 | High     | Generated Navigation is a substantial foundation whose public Index consumer is still pending. | Do not remove it during this correction because Index is an accepted direct consumer. Do not broaden it. Reassess its actual retained surface at Index acceptance and promote only meaning proved by additional consumers. |
| PCF-005 | High     | Extension Inspect comparison and result formation carry extensive future-facing lifecycle, fingerprint, and generated-state machinery. | Preserve accepted public behavior now. Before Status or Doctor consumes these facts, run a bounded architecture reduction pass and promote only neutral capabilities with real consumers. |
| PCF-006 | Medium   | Lifecycle validation includes exhaustive coverage states and custom duplicate-property scanning before lifecycle mutation exists. | Mutation Foundation must investigate which facts are required as write authority. Keep source-generated typed schema; treat retained custom scanning as an explicit justified exception. |
| PCF-007 | Medium   | `RouteListDirectoryEnumerator` and `RouteListFilesystemEntryReader` appear to have no production consumer; one has only a direct test. | Resolved at `2cd525d`: Git history and the complete reference graph proved that `SourceCatalogueReader` superseded both helpers; the dead helpers and only orphaned policy/test branches were removed. |
| PCF-008 | Low      | `SourceLinkDestinationResolver` exposes three delegates that References immediately forwards through lambdas. | Preference-only for this wave. Revisit the seam when a real consumer changes; prefer one cohesive capability or concrete Framework services if the forwarding remains. |
| PCF-009 | Medium   | Extension and Lifecycle JSON syntax validators duplicate property-tracking mechanics. | Revisit before lifecycle mutation. Share one narrow mechanism only if strict duplicate rejection remains accepted and both consumers retain identical semantics. |

No native interop, P/Invoke, unsafe code, reflection dispatch, native helper,
service locator, or dependency-injection container was found. The audit concern
is premature or duplicated managed machinery, not an exceptional platform
workaround.

## Execution Capsule

- Profile: Standard correction wave with two independent mutating lanes.
- Review budget: one Sol/xhigh review per lane and one project-level integration
  review. Use one grouped correction pass per review. No council is authorized.
- Correction budget: one bounded repair pass per lane; reopen architecture only
  if accepted public meaning or a cross-lane contract changes.
- Shared invariants: exact public statuses, streams, JSON shapes, source-generated
  serialization, Native AOT compatibility, no-write behavior, and unrelated
  Markdown bytes remain unchanged.
- Integration order: integrate the proportional-development package, then the
  Markdown and Route List corrections in either order after each rebases cleanly
  onto the accepted baseline.

### Lane A — Route List Rendering And Dead Support

- Owns `Commands/Route/List/Shared/Rendering/`, directly mirrored Route List
  presentation tests, the two named filesystem helpers, and their directly
  orphaned tests.
- May promote a scalar-aware escaping primitive only after investigation proves
  another current consumer has identical semantics. Otherwise keep the fix local.
- Must prove exact total bounds, control escaping, quotes and backslashes,
  supplementary Unicode scalars, boundary values, determinism, and unchanged
  ordinary diagnostics.
- Must prove the complete production reference graph before deleting support.

### Lane B — Markdown Generated-Region Consistency

- Owns `Framework/Documents/Markdown/`, its mirrored direct tests, and the
  smallest Extension Inspect fingerprint integration neighborhood needed to
  consume the authoritative fact.
- The accepted grammar requires one exact canonical ATX line `## Entries` with
  no trailing whitespace, one exact ordered marker pair outside code, and the
  final-section/final-non-empty-line conditions in the Extension Inspect
  contract.
- Must preserve absent, valid, malformed, duplicate, reversed, code-contained,
  CRLF, lone-CR, invalid-encoding, and exact-byte fallback behavior.
- Must not add a second parser, raw-token workaround, compatibility path, or new
  dependency.

## Protected Paths And Meaning

- Mutation Foundation worktree and all mutation, recovery, Git, lock, lifecycle
  writer, receipt, and application production paths.
- Public command grammar, statuses, exits, stream selection, JSON schema and
  property order, and ordinary human output not named by PCF-001.
- Extension package identity, catalogue inventory, dependency semantics,
  lifecycle schema, generated Markdown bytes, and repository routing behavior.
- Package versions, project topology, Native AOT RID policy, remotes, and user
  Git configuration.

## Evidence

Each lane requires focused Unit and Integration evidence, a warning-free Release
build of affected projects, format and diff checks, one safe isolated-worktree
public dogfood scenario, primary-owner inspection, and one Sol/xhigh review.

After both lanes integrate, run the complete Release build, Unit, Integration,
and EndToEnd suites; rerun repository doctor and generated-index checks; execute
the affected public commands from the isolated integrated worktree; and obtain
one project-level Sol/xhigh correctness and architecture review.

## Stop Conditions

- Stop and return to the Overseer if a correction changes accepted public
  grammar, result meaning, persisted schema, generated bytes, dependency
  direction, platform support, or mutation safety.
- Stop before introducing a workaround, compatibility path, custom parser,
  native boundary, reflection, unsafe code, or new package.
- Stop deletion when a real production or accepted imminent consumer exists.
- Stop promotion when consumers have similar syntax but different semantic
  authority.

## Acceptance Result

- The integrated Release solution build passes with zero warnings and errors.
- Managed Unit `1053/1053`, Integration `411/411`, and EndToEnd `120/120` pass
  with zero failures or skips.
- Portable `linux-x64` Native AOT root publication and execution pass without a
  distro-specific RID or project workaround. Native AOT Integration is
  `411/411` and Native AOT EndToEnd is `120/120`, with zero skips.
- Native Route List and Extension Inspect dogfood preserve expected public
  status, stream, generated-region, and no-write behavior.
- Format verification exits zero with the known repository-local reference-load
  warnings even after a successful standard restore. `git diff --check` passes,
  the integrated tree is clean, and legacy Doctor reports zero errors plus the
  pre-existing C# `Axioms` warning.
- Both lane reviews and the final integrated Sol/xhigh review have no unresolved
  material finding. The integrated review independently reproduced the complete
  managed and Native AOT suites.
- PCF-004 through PCF-006 and PCF-008 through PCF-009 retain their recorded
  defer or revisit boundaries; this wave did not broaden them.

## Completion

Complete. PCF-001, PCF-002, PCF-003, and PCF-007 are closed in the integrated
tree at `0d88606`; all other findings retain the dispositions above. Complete
managed and Native AOT evidence, public dogfood, no-write checks, and independent
review pass. Mutation Foundation M1 is the next accepted development boundary.
