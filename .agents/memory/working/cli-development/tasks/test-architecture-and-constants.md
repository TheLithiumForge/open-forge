---
open-forge:
  description: Audit symbolic constants, generated-region authority, and composable active-test foundations across the replacement CLI
  tags: [Memory, Working, CLI, Task, Testing, Architecture, Constants, Fixtures, Snapshot, Contextual]
---

# Audit CLI Constants And Test Architecture

## Task State

- State: Planned after the active Mutation Foundation contract slice is accepted
  and fully validated. Reconfirm the exact predecessor before starting.
- Responsible role: Mastermind.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Task source: Maintainer direction recorded on 2026-08-27.

## Expected Outcome

The replacement CLI and its active tests use named, nearest-owner symbolic values
for constant-like contracts and a small composable test foundation for repeated
workspace, document, catalogue, process, and result scenarios. Tests remain cheap,
independently runnable, readable, thorough, and faithful to the behavior each tier
proves. The audit introduces no mocks, fake filesystem, generic utility bag,
universal fixture, or speculative browser abstraction.

## Authority

- [C# Callable Design](../../../../directives/csharp/design.md), including
  nearest-owner symbolic constants and readable cohesive call surfaces.
- [C# Style](../../../../directives/csharp/style.md), including interpolated
  templates and coherent raw multi-line construction.
- [Test Evidence Integrity](../../../../directives/open-forge/testing/evidence-integrity.md).
- [Evidence Tiers](../../../../patterns/testing/evidence-tiers.md).
- [Nearest Shared Scope](../../../../patterns/software/source-locality/nearest-shared-scope.md).
- [CLI Architecture](../../../crystallized/documents/cli/architecture.md).
- The completed [Active-Test Architecture](generic-improvements/active-test-architecture.md)
  remains accepted evidence, not a prohibition on a new audit of later code.

## Preflight Inventory

Before changing source, inventory all authored production and active C# test paths
and classify each finding by ownership and meaning:

1. Repeated or semantically constant strings and numbers: schema versions, marker
   text, policy names, stable IDs, path names, statuses, limits, timeouts, and
   formatting tokens.
2. Direct test literals that should consume an owned contract value versus
   independent literal oracles that must remain separate to detect an accidental
   production-constant change.
3. Duplicated document builders such as metadata frontmatter, Loader and routed
   entrypoints, generated navigation, overwrite companions, catalogues, lifecycle
   documents, and malformed variants.
4. Duplicated real-workspace, process, snapshot, setup, and cleanup behavior,
   including which consumers have exactly identical semantics and which only look
   structurally similar.
5. Expensive, timing-sensitive, hard-to-select, or over-broad tests and detailed
   branch coverage repeated above its cheapest proving tier.
6. Large exact strings or structured results that would be clearer as focused,
   explicitly updated snapshots without hiding safety assertions.

Record the complete candidate ledger before selecting mutations. Duplication is
evidence to inspect, not automatic authority to centralize.

## Generated Navigation Boundary Investigation

Treat marker constants and marker removal as two separate decisions.

- Centralize any retained exact generated-region tokens at the capability that
  owns their syntax; tests should not scatter copies such as
  `<!-- open-forge:generated-index:start -->`.
- Independently evaluate whether Markdig can safely identify one owned `Entries`
  heading and list from the AST, preserve all unrelated authored bytes, distinguish
  managed from authored lists, handle empty or malformed regions, and reject
  ambiguity.
- Do not remove explicit markers merely because a heading and list can be found.
  The replacement must prove ownership, idempotence, bounded replacement,
  compatibility, diagnostics, and recovery for every accepted rooted, detached,
  overwrite, malformed, empty, and adversarial case.
- If AST ownership changes the accepted authored document contract or lifecycle
  compatibility, return to Architecture and contracts before production work.

## Desired Test Foundation

- Unit tests exercise focused methods, classes, or pure stages over explicit
  in-memory inputs and outputs.
- Integration tests exercise one module across the real boundary it owns, usually
  an isolated real filesystem, process, serialization, or package layout.
- EndToEnd tests treat the published CLI as a black box and retain only complete
  critical journeys, public streams, exits, cancellation, and unchanged-state
  evidence.
- Tests use stable descriptive identities and cover accepted happy, invalid,
  boundary, failure, and interruption paths.
- Scenario data is assembled from small named seeds, builders, and factories.
  A metadata document and a metadata-plus-navigation document should share the
  same frontmatter/body primitives rather than duplicate concatenation, while
  malformed evidence keeps the explicit bytes needed to prove rejection.
- Real setup and teardown remain composable and owned. Promote support only to the
  nearest common scope of at least two consumers with identical semantics; keep
  feature-specific policies and richer fixtures local.
- Prefer real boundaries and observable state. Permit a test double only when the
  real boundary is unsafe or cannot be made deterministic at reasonable cost, and
  record that exception locally.
- Use focused snapshots when they improve review of exact strings or large stable
  projections. Keep safety, containment, mutation, receipt, and state-transition
  claims as direct assertions.

## Execution Shape

1. Read-only Preflight and candidate ledger.
2. Freeze any missing generic Directive or Pattern meaning before code depends on
   it; link reusable shapes rather than embedding framework-specific examples in
   generic instructions.
3. Split the accepted findings into small locality-safe Purple slices. Keep
   production constant ownership separate from test-support promotion and from
   any generated-region contract change.
4. For each slice, preserve test identity and behavior, run focused evidence,
   obtain correctness and improvement review, and commit only an accepted boundary.
5. Finish with warning-free Release build, format and diff checks, full managed
   Unit/Integration/EndToEnd evidence, supported Native AOT gates, source/locality
   audits, and an independent architecture review.

## Stop Conditions

- Stop if a shared helper would mix distinct scenario meaning, hide required setup,
  create a generic repository or utility bag, or grant tests broader authority.
- Stop if importing a production constant would make a conformance assertion
  tautological or prevent detection of an accidental protocol change.
- Stop if a snapshot would hide a safety invariant or ordinary test execution
  would mutate its expected output.
- Stop if parser-based generated-region replacement cannot prove one unambiguous
  owned target and exact preservation outside it.
- Stop on a new package, test framework, public behavior, persisted schema,
  platform promise, or browser-specific production abstraction without a separate
  accepted decision.

## Deferred Relationship

The [Browser WebAssembly CLI Playground](../../../emerging/ideas/cli-browser-wasm-playground.md)
may later consume identical seed data, but it does not justify shared code during
this audit. Promotion requires demonstrated semantic reuse after a browser spike.
