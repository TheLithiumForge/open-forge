---
open-forge:
  description: Gate 2 CLI work is testing whether bounded evidence packets, file packs, and one end review reduce agent cost without reducing quality
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Review, Delegation, Efficiency, Evidence]
---

# Observation: Lean Agent Batching May Preserve Quality With Less Rework

Date: 2026-08-15. Scope: the remaining Gate 2 CLI contract definition and
review work. This is an experiment record, not an accepted agent rule.

## Concrete Occurrences

- Queue 06 resolved two Find Interface questions with one Explorer, four
  independent Advisors, two Writers, and one Reviewer. The work produced useful
  decisions and caught real defects, but the helper count and repeated inspection
  were high for one command review.
- The evidence Explorer reached its step limit before completing its assigned
  repository survey. Mastermind already held several required paths and later
  resolved the remaining authority question directly.
- Writers performed useful correction checks, but broad writer-side review can
  duplicate later Mastermind acceptance and independent review.
- The maintainer requested coherent file packs, bounded helper depth, exact
  evidence packets from Mastermind, deferred Gate 3 library work, and one review
  or writing review at the end of a meaningful batch.

## Hypothesis

For well-scoped documentation work whose authority and decisions are already
known, one grounded writer working across the related file pack, followed by one
tightly scoped end review, will use fewer helper calls and less repeated context
while preserving semantic correctness, link health, validation, and useful
independent findings.

Cold isolation may still be worth its cost when independent positions are the
deliverable. Open-ended exploration and councils should not be the default when
Mastermind can provide the exact sources and accepted constraints.

## Trial Protocol

Apply the following protocol to at least two comparable remaining Gate 2 packs:

1. Mastermind supplies exact file paths, accepted decisions, known line evidence,
   allowed changes, forbidden changes, validation, and stop conditions.
2. Mastermind reads or edits small context-heavy changes directly. Use one
   bounded Explorer only when a factual gap remains.
3. Use a council only for a material unresolved tradeoff or an explicit
   maintainer request. Preserve cold isolation only for genuinely independent
   first-round positions.
4. Use one grounded Writer for the complete related file pack after meaning is
   decided. Writer checks stay corrective: allowed-file diff, links, generated
   regions when affected, and whitespace hygiene.
5. Mastermind inspects the actual result and runs focused deterministic
   validation.
6. Run one appropriately scoped Reviewer or Writing Reviewer at the end of the
   meaningful batch. Do not run one review per file or duplicate the same review
   lens by default.

## Evidence To Record

For each trial pack, append:

- pack name and settled decisions;
- helper invocations by role;
- whether delegated repository-wide discovery was needed;
- changed files and correction rounds;
- end-review actionable findings by severity;
- doctor, body-load, link, label, source-ID, and diff results that apply;
- any maintainer-reported quality, speed, or context problem; and
- token usage when the runtime exposes it.

The runtime has not exposed reliable per-helper token counts in this work. Until
it does, helper count, duplicate discovery, correction rounds, elapsed review
turns, and actionable end-review findings are proxies. Do not present them as
exact token savings.

## Initial Comparison Baseline

Queue 06 used eight helper invocations for two maintainer decisions: one
Explorer, four Advisors, two Writers, and one Reviewer. It needed one
post-review correction round. Its final deterministic validation passed and its
Reviewer found three actionable application defects. This is a quality-bearing
but expensive baseline, not evidence that fewer helpers will be better.

## Trial 1: Queue 09–10 Index Contracts

The first lean pack settled three Index decisions across the authoritative
Interface and Behavior plus six synchronized public and program records:
idempotent repeated Boolean write-policy flags, human stdout/stderr allocation,
and `attention` for a dry run with planned changes and a non-blocking finding.

- Helpers: one grounded Writer and one end-of-pack Reviewer.
- Discovery and diversity: no Explorer, Advisor, or council was needed because
  Mastermind supplied the exact authoritative files and the maintainer resolved
  the three product choices directly.
- Scope: eight changed files in one authoring pass.
- Correction rounds: zero after review.
- End review: no actionable findings in the changed hunks or nearby authority.
- Deterministic evidence: 594 unique Find/Index labels and references, 12 exact
  accepted-decision invariants, 161 scoped local links and anchors,
  `git diff --check`, `open-forge-old doctor`, and body loading all passed.
- Token evidence: unavailable; no exact token-saving claim is made.

This is materially fewer helper invocations per settled decision than the Queue
06 baseline and did not produce a review or validation regression. One trial is
not enough for promotion; run at least one more comparable pack.

## Trial 2: Queue 13–15 Discovery Capabilities

The second pack settled Route List defaults and output, ordered Context paths,
direct References, shared non-global Source Universe Filters, the two-view
boundary, external no-fetch facts, and the temporary References compatibility
staging. It created two command contract sets and one shared contract set,
reconciled Find, synchronized public/program records, archived settled review
evidence, and regenerated navigation.

- Helpers: six independent Advisors across two maintainer-requested council
  rounds, three bounded Writers for separable file groups, and one end Reviewer.
- Discovery: no Explorer or repository-wide delegated rediscovery. Mastermind
  supplied the exact evidence packets and resolved the frozen-tool staging fact
  directly from observed validation failure.
- Scope: 39 affected file paths in one coordinated capability pack, including
  new contracts, four archived review records, synchronized program/public
  records, and regenerated indexes.
- Correction rounds: one Mastermind correction pass before review for contract
  integration and staging details; one post-review correction for stale
  validation counts.
- End review: one actionable stale-evidence finding; no command-semantic,
  authority, routing, or link finding. The evidence records were corrected
  without a second review.
- Deterministic evidence: 45 unique command source IDs, 594 temporary Find/Index
  labels and references, 1,270 local links and anchors across 80 relevant active
  and archived CLI files, Global Flags linkage for all 10 leaf commands, five
  focused batch invariants, `git diff --check`, `open-forge-old doctor`, and body
  loading all passed.
- Tooling evidence: frozen `open-forge-old` double-counted the final
  `references/_references.md` compatibility shape. The accepted public command
  was staged under `references-candidate/`, and `CLI-MIG-C006` now preserves the
  required new-CLI physical-identity regression.
- Token evidence: unavailable; no exact token-saving claim is made.

This pack used more helpers than Trial 1 because the maintainer explicitly asked
for two council rounds. It still settled substantially more independent product
boundaries than the Queue 06 baseline, with no Explorer, no duplicated reviewer,
and one bounded post-review correction.

## Promotion Assessment After Two Trials

The provisional promotion threshold is met:

- both trials used materially fewer helper invocations per settled decision than
  Queue 06's eight helpers for two decisions;
- neither trial left a high-severity finding or validation regression;
- Trial 1 needed zero and Trial 2 needed one post-review correction round; and
- the maintainer reported better speed/iteration preferences without reporting a
  loss of decision quality or maintainability.

The evidence supports proposing the lean packet/batch/end-review technique for
the authoritative agent sources. This Observation does not apply that change by
itself. Preserve it as provenance and obtain maintainer acceptance for the exact
agent-file wording and scope.

## Promotion Threshold

Propose a durable agent-rule change only after at least two comparable lean
batches show:

- materially fewer helper invocations and less duplicate discovery than the
  Queue 06 baseline;
- no uncorrected high-severity finding and no validation regression;
- no more than one post-review correction round per batch; and
- no maintainer-observed loss of decision quality, clarity, or maintainability.

If evidence is mixed, refine the packet or batch boundary and continue observing.
If quality declines, reject the technique rather than promoting efficiency by
assertion. If the threshold is met, propose the smallest matching changes to the
authoritative agent sources and keep this Observation as provenance.

## Trial 3: Queue 29 Lifecycle Integration

Queue 29 integrated one accepted lifecycle direction across the Framework
Install/Update contracts, six Extension command sets, Status, Doctor, the
Decision Agenda, overview Documents, the CLI Pattern, public documentation, and
generated navigation.

- Helpers: one grounded Writer for the accepted contract pack and one fresh
  Reviewer session. The Reviewer needed one continuation after reaching its tool
  limit; this was one review lens, not a second review.
- Discovery: Mastermind supplied the exact accepted packet, target files,
  authority boundary, allowed work, forbidden program records, and validation.
  Separate Explorers later inspected Queue 30, workflow gaps, and architecture
  evidence; they did not rediscover Queue 29 semantics.
- Scope control: the Writer produced the complete contract pack but also updated
  program/review records that its packet had forbidden. Mastermind inspected the
  actual changes instead of accepting the summary and retained only changes that
  matched the now-accepted program transition. This is evidence that allowed-path
  diff inspection remains necessary even when meaning is closed.
- End review: two high findings and one medium finding. The high findings were
  inconsistent single-package manifest-ID inference under automatic mode and
  omitted dry-run apply-event boundaries. The medium finding was stale Queue 28
  authority wording in the contextual packet.
- Correction: one focused Mastermind correction pass updated the affected
  Extension selection rules, six mutation Behaviors, the shared Pattern, D091,
  and the contextual authority wording. No second independent review was added.
- Token evidence: unavailable. The Reviewer tool-limit continuation and Writer
  scope deviation are qualitative cost signals, not exact token measurements.

This trial supports one writer plus one end review for a broad accepted prose
pack, but it narrows the hypothesis: Mastermind must verify the allowed-path diff
and should expect one focused semantic correction pass when the pack spans many
independent command surfaces. A writer summary is not acceptance evidence.
