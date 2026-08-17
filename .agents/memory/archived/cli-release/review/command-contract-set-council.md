---
open-forge:
  description: Historical council evidence for command-contract placement, authority, entrypoints, and temporary migration traceability
  responsibility: Preserve the historical Queue item 01 reasoning without making it current Pattern authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, Council, Analysis, Command, Contract, Authority, Identity]
---

# Command Contract Set Council

## Status

This file was archived from the [active CLI Review Queue](queue.md) on 2026-08-15 after Queue item 01 settled. The maintainer accepted the current [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md) for locality and authority. This council remains historical context and does not define that Working Document.

- **Origin:** `.agents/memory/working/cli-release/review/command-contract-set-council.md`
- **Archived because:** Queue item 01 left active review after its accepted locality and authority outcome was integrated into the current Working Document.
- **Current source:** [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md)

The remainder of this file preserves the former review record. Its reasoning,
recommendations, and dissent are not current authority.

## Historical Maintainer Direction After Council

The maintainer accepted these outcomes after reviewing the council findings:

- Related behavior, Interface, Behavior, Technical Design, tests, fixtures, and
  focused support stay together in the narrowest command or component scope.
  Artifact type alone must not split related files into distant categories. This
  locality rule belongs in both a Directive and a Pattern.
- Final command-contract placement follows the public command path. The current
  `index-candidate/` path is temporary only because additive source identity and
  the frozen CLI must both remain valid.
  The new CLI must handle the `index/_index.md` case, after which the scope can
  return to `commands/index/`.
- The direct-command contract-set shape remains. A real `route inspect` grouped
  pilot must prove group and leaf entrypoint behavior before grouped placement
  becomes a permanent Pattern rule.
- Permanent requirement IDs are rejected. Existing I/B/T labels may remain only
  as temporary Working reconciliation aids in the current candidates, ledger,
  and Checkpoint. Remove them before any contract moves to Crystallized, and do
  not assign such labels to other files.
- CLI source identity remains the path-derived source ID or exact `.agents/...`
  path defined by the shared source-reference contract.

After this council, the maintainer authorized preparing every remaining split
candidate before detailed review. The current unsplit Working command contracts
continue to control meaning until each candidate is reviewed and an explicit
maintainer cutover names its replacement. This resolves the migration-authority
question without accepting the surrounding council analysis or completing the
Pattern's later detailed review. Archived and temporary sources remain raw input.

## Decision Frame

The council examined whether the detailed Pattern is ready to become the
continuing default for:

- Command-local Interface, Behavior, and optional Technical Design placement.
- Direct and grouped command scopes.
- Shared-contract placement.
- Entrypoint responsibility.
- Migration authority and explicit cutover.
- Stable local requirement identifiers.

The criteria were authority clarity, lossless migration, locality,
discoverability, review cost, traceability, scalability, and the smallest
permanent structure that preserves the accepted contract roles.

## Council's Inherited Constraints

The council did not reopen these accepted system-level directions:

- Every public operation has a distinct product-facing Interface Contract and
  technology-neutral Behavior Contract.
- Technical choices remain separate and cannot redefine public or behavioral
  guarantees.
- Related command files use command-local routed scopes, while genuinely shared
  contracts use the nearest useful common scope.
- Migration remains additive. The mixed source stays authoritative until an
  explicit reviewed cutover names its replacements.
- Fact-level traceability, continuing Patterns, and copy-ready Templates remain
  required. The council initially treated stable requirement identity as an
  inherited constraint; the maintainer later rejected it as a permanent system
  and retained the current labels only as temporary migration aids.

## Verified Current Facts

- The Pattern is routed under active Patterns, carries no Candidate or
  Contextual status, and therefore presents as an applicable default shape.
- Queue item 01 remains under maintainer review. No detailed acceptance event for
  the Pattern is recorded.
- The Find and Index candidate scopes instantiate the three-file split and keep
  the mixed command files authoritative.
- `commands/index-candidate/` is a recorded temporary staging exception. The
  public command remains `index`, and final placement is not settled by that
  exception.
- The additive `route` candidate now supplies a real grouped-command example.
  Its parent and leaf entrypoints remain non-authoritative pending detailed
  review.
- Current candidate identifiers cover normative guarantees, lifecycle/status
  facts, scenarios, and verification obligations. Find also preserves an
  intentional Behavior-ID gap.
- Requirement IDs such as `FIND-I-001` are distinct from path-derived CLI source
  IDs.

## Independent Perspectives

### Governance And Authority

This lens recommended revision before acceptance. The active Pattern status can
make unreviewed detailed clauses look accepted. It favored either a reduced
active Pattern containing only already accepted invariants or explicit
candidate treatment for the remaining detail. It also requested maintainer
acceptance in the cutover rule, navigation-only entrypoint wording, and a
bounded staging-path exception.

### Information Architecture And Locality

This lens retained the command-local three-file shape. It found direct-command
locality and nearest-parent sharing well calibrated, but asked the Pattern to
distinguish a group router from a leaf contract-set entrypoint. It treated exact
command-path placement as a final-state rule and requested a real grouped pilot
before relying on the schematic example as proof.

### Traceability And Verification

This lens retained stable IDs but requested a stronger grammar and lifecycle:
stable semantic scope rather than physical path, explicit distinction from
source IDs, append-only allocation, no reuse or gap filling, and split/merge or
replacement mappings. It warned that IDs on status, examples, and verification
can inflate counts and make structural completeness look semantic.

### Radical Simplifier

This lens proposed one contract file per leaf with Interface, Behavior, and
Traceability sections, plus separate technical sidecars only when needed. It
treated per-leaf folders, entrypoints, and separate Interface and Behavior files
as potentially avoidable ceremony. This alternative would reopen the accepted
command-local file shape in `CLI-D071`; it remains material dissent rather than
a compatible cleanup.

## Shared Ground

All perspectives agreed that:

- Interface and Behavior meaning must remain distinct.
- Technical Design is optional, subordinate, and never a source of public or
  technology-neutral guarantees.
- Candidate routability, completeness, identifiers, or generated Entries never
  create authority.
- Cutover must remain explicit, additive, reviewed, and reversible until the
  authority change.
- Shared contracts require real consumers and explicit links rather than
  hypothetical reuse or copied definitions.
- Stable IDs are useful only when their ownership, lifecycle, and relationship
  to evidence are clear.

## Material Dissent

### File And Folder Minimum

Three grounded lenses retained the separate routed contract-set shape. The
simplifier preferred one contract file per command. Choosing the simpler file
would require revisiting `CLI-D071`, not merely editing this Pattern.

### Identifier Coverage

One view keeps IDs on every independently reviewable contract, scenario, and
evidence obligation. The stricter view assigns IDs only to normative atomic
facts and lets scenarios and evidence cite the facts they cover. The current
Pattern does not resolve that boundary.

### Grouped Commands

The proposed nested command-path shape is coherent, but no real grouped
candidate proves that a group entrypoint remains navigation-only without empty
contract ceremony. The maintainer may accept the conceptual rule now or require
a grouped pilot first.

## Decision-Relevant Findings

### 1. Authority State Needs Resolution

The Pattern is active while its detailed review item is unaccepted. The
maintainer must choose whether to accept the detailed Pattern, split accepted
invariants from candidate detail, or keep the complete Pattern contextual until
review finishes.

### 2. Placement Must Separate Final Identity From Staging

The public command path should control final command-local placement. A temporary
candidate path should be allowed only when the migration ledger records its
reason, non-authority, intended final identity, and exit condition.

### 3. Group And Leaf Entrypoints Need Distinct Rules

A group entrypoint should route child operations and describe group help or
namespace behavior. A leaf entrypoint should route one contract set and state its
lifecycle. Neither should receive empty contracts or duplicate command detail
merely because a folder exists.

### 4. Council Finding Before Decision: Stable-ID Boundary

The Pattern should define whether IDs belong only to normative facts or also to
status, scenario, and verification records. It should also define stable scope,
append-only allocation, gaps, retirement, replacement, split and merge mapping,
and the prohibition on deriving requirement identity from a temporary path.

### 5. Cutover Wording Is Too General

`reviewed cutover` should name the actual boundary: complete fact mapping,
resolved conflicts, lossless and authority review, maintainer acceptance,
updated links, regenerated navigation, and verification. The Pattern should not
embed this program's rollback commit as a universal rule.

## Council Recommendation Before Maintainer Decision

Revise rather than accept the Pattern as written. Preserve the accepted
command-local Interface, Behavior, optional Technical Design, shared-contract,
and additive-migration model. Before acceptance:

1. Resolve the Pattern's current active authority state.
2. Make command-path matching a final-placement rule with a ledger-controlled
   staging exception.
3. Separate group-router and leaf-entrypoint responsibilities.
4. Decide whether stable IDs should remain a permanent contract mechanism.
5. Make maintainer acceptance explicit in cutover.

The one-file simplification remains a valid alternative only if the maintainer
wants to reopen `CLI-D071`.

## Remaining Decision Frontier

The locality, final-path, grouped-pilot, permanent-ID, and migration-authority
questions are resolved by the maintainer direction above. Detailed review of the
Pattern remains open. That review must determine whether its current wording
expresses only the accepted direction or still contains unaccepted detail.

## Evidence Needed For Later Choices

- Detailed review of the real grouped `route` candidate and its group-help
  boundary.
- Verification that current temporary I/B/T labels are removed before the
  contracts move to Crystallized.
- A new-CLI move and compatibility test proving that `index/_index.md` is handled
  once and the final `commands/index/` placement remains routable.
- A cutover rehearsal that proves zero unmapped facts, unresolved conflicts,
  duplicate definitions, missing references, or authority ambiguity.

## Related Sources

- [Review Queue](queue.md)
- [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md)
- [Contract Migration Ledger](../contract-migration-ledger.md)
- [CLI Decision Agenda](../../../working/cli-release/decision-agenda.md)
- [CLI Release Plan](../../../working/cli-release/release-plan.md)
