---
open-forge:
  description: Calibrate development rigor, safety, and complexity to the project's real consequences before adding exceptional machinery
  tags: [LoadNow, Core, Directive, Development, Design, Safety, Simplicity, Platform, Risk, Decision]
---

# Proportionate Development

## Instructions

### Understand The Project Before Designing The Solution

- At the beginning of development analysis or implementation, establish the kind of project,
  its users, the data and systems it can affect, its operating environment, how
  reversible its changes are, and the realistic consequence of failure. Use
  accepted product context instead of assigning criticality from a technical
  category alone.
- State the supported threat and failure boundary when it changes architecture.
  Distinguish ordinary defects, interruption, crashes, accidental concurrency,
  malformed input, and dependency failure from a malicious actor who already
  controls the same machine, account, process, repository, or credentials. Do
  not silently turn a local development tool into an adversarial security
  boundary.
- Identify the practical recovery boundary. Account for version control,
  backups, atomic platform operations, regeneration, reruns, and manual repair
  only where they actually exist. Do not claim that one mechanism protects data
  it does not contain, such as uncommitted work that Git cannot restore.
- Reassess the profile when evidence changes the affected data, trust boundary,
  compatibility promise, public exposure, reversibility, or likely harm. Do not
  preserve an early low-risk or high-risk classification after it stops fitting.

### Prefer The Smallest Complete Design

- Default to the simplest design that completely satisfies the accepted product
  need and its realistic safety boundary. Complexity must earn its ongoing cost
  through a concrete requirement, plausible failure, or demonstrated reuse.
- Use the current language, compiler, runtime, standard library, framework,
  operating system, and accepted dependencies to their full documented
  capability. Prefer their ordinary idioms and supported extension points over
  recreating parsers, transactions, schedulers, object systems, filesystems,
  dependency injection, platform abstractions, or other general machinery.
- Keep product policy local and promote only a demonstrated identical capability
  or an accepted neutral foundation. Do not build a global abstraction merely
  because several local implementations look similar.
- Simplicity does not excuse preventable data loss. Apply the mechanisms required
  by the accepted safety boundary, including exact-target validation,
  preservation of unrelated state, atomic or reversible operations, and effect
  verification.
  When several designs satisfy that boundary, choose the simplest one. Weakening
  an accepted safety requirement needs explicit maintainer acceptance and a
  recorded residual risk.

### Treat Exceptional Machinery As A Decision

- Do not add a workaround, compatibility shim, native bridge, custom substitute,
  reflection path, unsafe block, platform reimplementation, or undocumented
  dependency behavior as an ordinary implementation detail.
- Verify the exact supported platform or dependency surface before concluding
  that standard behavior is insufficient. When an accepted requirement cannot
  be met normally, tell the user immediately before implementation. State the
  unmet requirement, realistic consequence, simplest standard alternative,
  exceptional option, portability and maintenance costs, evidence boundary, and
  recommendation.
- Require explicit maintainer acceptance before relying on exceptional machinery
  that changes architecture, portability, dependencies, security, data safety,
  compatibility, or long-term maintenance. Keep the exception at the narrowest
  boundary and record when it can be removed or reconsidered.
- Do not disguise an exception behind a generic helper or an overstated safety
  claim. If the project accepts a narrower threat model or guarantee, state it
  plainly in the architecture and tests.

### Keep Analysis And Evidence Proportionate

- Tie proposed work to a normal user journey, a plausible consequential failure,
  or an invariant required by an accepted product guarantee. Name that connection
  before expanding implementation, testing or review. A technically possible
  input or state alone does not justify additional work.
- Let standard libraries own the languages and mechanisms they implement. For
  example, read a file, let the accepted library parse its format, and consume
  the parsed values the product needs. Validate product requirements on those
  values. Do not rescan syntax or add special handling merely to cover obscure
  spellings, unusual dependency behavior or every feature of the format.
- Keep users responsible for supplying meaningful input within the supported
  contract. Use the existing bounded error path for unsupported or malformed
  input. Add accommodation only when a real use case or accepted guarantee
  needs it; do not make the product interpret arbitrary user intent.
- Investigate likely and consequential failures first. Record unusual cases for
  triage; do not promote every theoretical edge case into a requirement,
  blocker, abstraction, or test.
- Test behavior the project owns, including the internal invariants needed to
  deliver it. Use ordinary dependency behavior without retesting the dependency
  or operating system. Artificial private states and exhaustive language-feature
  combinations need a demonstrated connection to the supported product.
- Choose planning depth, delegation, tests, review, compatibility work, and
  platform coverage from novelty, consequence, and reversibility. File count or
  the presence of filesystem, concurrency, persistence, or security terminology
  does not by itself require the highest-assurance process.
- Match effort to the delivery stage and current goal. During a bounded beta
  closeout, prioritize working journeys and material regressions. Defer optional
  architecture polish and low-impact edge cases. Existing preparation or time
  already spent does not make a finding necessary for release.
- Resolve routine out-of-scope edge cases through these rules. Ask the user only
  when a real product choice or consequential tradeoff remains; do not turn an
  exotic example into an unnecessary decision for them.
- Explain residual risk honestly. Do not market ordinary safeguards as absolute
  guarantees, and do not reject a proportionate design merely because a more
  privileged or malicious actor could defeat it.

Use the [Adaptive Design And Delivery Guidance](../guidance/adaptive-design-delivery.md)
to select an execution profile, the [Program Architecture Directive](program-architecture.md)
for cross-cutting placement, and the [Source Locality Directive](source-locality.md)
for promotion decisions. Language-specific Directives add their normal idioms
and constraints without changing this proportionality boundary.
