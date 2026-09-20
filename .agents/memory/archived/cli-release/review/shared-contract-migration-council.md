---
open-forge:
  description: Historical council synthesis for staging shared CLI contracts without source-ID collisions or semantic loss
  responsibility: Preserve historical shared-contract migration reasoning and cutover boundaries without making it current authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, Council, Analysis, Contract, Migration, Shared]
---

# Shared Contract Migration Council

## Status And Authority

This file was archived from the [active CLI Review Queue](queue.md) on 2026-08-15 after Queue items 20 and 21 settled. The current [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md) and [CLI Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md) remain the accepted shared contracts. Authority cutover is handled elsewhere. This council preserves migration history and does not revise or replace those sources.

- **Origin:** `.agents/memory/working/cli-release/review/shared-contract-migration-council.md`
- **Archived because:** Queue items 20 and 21 left active review after the maintainer accepted the shared contracts and the migration council no longer represented an unresolved review unit.
- **Current sources:** [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md), [CLI Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md), and [CLI-D071 migration direction](../../../working/cli-release/decision-agenda.md)

The remainder of this file preserves the former council record. Its migration
recommendations, alternatives, and unresolved staging questions are historical
context, not current authority.

## Question

How can the two authoritative mixed shared contracts be staged as separate
Interface and Behavior files without changing their authority, automatic source
identity, accepted meaning, consumer links, or unresolved boundaries?

The council evaluates the proposal against these criteria:

- authority and lifecycle remain visible;
- every source fact, example, link, condition, exception, and unknown survives;
- shared material stays at the narrowest scope justified by real consumers;
- routing remains simple and entrypoints do not become competing contracts;
- staging is additive, reversible, and free of new requirement-label debt.

## Inputs And Constraints

- The [Global CLI Flags](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md) and [CLI Source References](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md)
  files are the authoritative mixed contracts. They define accepted Gate 2
  meaning, but neither contract ships yet.
- The [CLI Command Contracts](../../../crystallized/documents/cli/contracts/_contracts.md) route identifies shared
  interfaces as links beside command contracts. The [CLI Command Contract Set
  Working Document](../../../crystallized/documents/cli/command-contract-set.md)
  places a genuinely shared contract at the nearest useful common scope and
  keeps Interface, Behavior, and optional Technical Design responsibilities
  distinct.
- The current [CLI Contract Migration Ledger](../contract-migration-ledger.md)
  requires additive staging, exact fact-level destinations, marked
  cross-layer reminders rather than competing definitions, preserved
  requirement strength and uncertainty, and no destructive cutover before
  mapping and review are complete.
- The shared source-reference contract derives automatic IDs from the current
  workspace-relative path. A recognized entrypoint uses its containing folder ID.
  A flat `commands/global-flags.md` beside a `commands/global-flags/_global-flags.md`
  entrypoint would therefore create the same path-derived source ID while both
  files exist. The same collision applies to `source-references`.
- The migration keeps public contract meaning in Interface, deterministic
  technology-neutral mechanics and conformance in Behavior, and does not add a
  Technical Design file for either shared contract.
- No new permanent or temporary Interface, Behavior, or Technical Design
  requirement IDs are allocated by this record or by the proposed shared
  candidates.

## Independent Council Lenses

These lenses use the same authoritative inputs but keep their reasoning separate.
They are evidence and recommendations, not votes or acceptance.

### Authority And Information Architecture

The authority boundary is the first constraint. The ledger keeps each flat mixed
source authoritative until an explicit reviewed cutover. The source-reference
rules make the staging collision a source-identity fact, not merely a naming
preference: a direct candidate entrypoint would share the flat file's folder ID,
while a child under `commands/shared/` has a distinct path-derived ID during
staging.

The information-architecture recommendation is therefore to stage the two
contract sets under `commands/shared/`, keep the flat files as the only
authoritative definitions, and make the route entrypoints navigational. The
intended direct final paths can be considered only after the flat sources are
removed, unless maintainer review changes that topology.

### Lossless Semantic Accounting

Each authoritative file mixes public input and result meaning with deterministic
resolution, error, and verification obligations. A heading-only move can appear
complete while losing a default, a no-op, a repetition rule, an exception, an
example, a link, or an explicit unknown. The migration ledger must therefore
name the exact destination file and heading for every source heading and then
reconcile every fact within that heading.

The accounting recommendation is to make Interface the sole detailed owner of
caller-visible grammar, defaults, observable results, and public errors. Behavior
must cover deterministic resolution and conformance by linking to those
Interface headings, not by restating them. No label system is needed to prove
coverage; the exact source-to-destination ledger and review evidence do that.

### Locality, Simplicity, And Conformance

The locality rule supports a common scope because these contracts have multiple
real command consumers. Splitting by responsibility keeps each question clear
without separating the shared subject into unrelated artifact categories. The
conformance boundary also favors route-only entrypoints, no empty contract
facts, no Technical Design sidecars, and explicit links across Interface and
Behavior.

This lens distinguishes temporary migration scaffolding from permanent topology.
`commands/shared/` is a useful staging boundary for the collision, but a
permanent extra route segment may add path and navigation cost after the flat
sources are gone. The simplicity recommendation is therefore temporary shared
staging followed by the direct final paths, subject to maintainer review.

## Council Common Ground

The three lenses agree that:

- The two flat mixed files remain authoritative during staging.
- The migration must add candidates before removing, moving, or replacing an
  authoritative source.
- The staged tree must separate Interface and Behavior without adding Technical
  Design.
- `_shared.md`, `_global-flags.md`, and `_source-references.md` are routing
  entrypoints. They may expose route status and `Entries`, but contain no
  contract facts.
- Every source heading and every fact within it needs an exact destination in
  the migration ledger. A link-only cross-layer reminder must not become a
  second detailed definition.
- The current global-flag, source-reference, path, collision, overwrite, error,
  result, verification, and related-source meaning must not be weakened,
  strengthened, or silently filled in during the split.
- No new permanent or temporary I/B/T labels are needed or allowed for these
  candidates.
- Existing command-consumer links continue to target the authoritative mixed
  files until an explicit cutover.
- A recommendation, candidate route, generated `Entries` line, or complete map
  does not create authority or acceptance.

## Material Disagreement

### Permanent `shared/`

The permanent-`shared/` position treats the two contracts as genuinely shared
and keeps their common scope visible after migration. It minimizes a later move
and makes the shared relationship apparent in the filesystem. This position is
consistent with the nearest-shared-scope reasoning when multiple real consumers
need the same meaning.

### Temporary staging and direct final paths

The temporary-staging position treats `shared/` as a collision-avoidance measure
only. It keeps the current flat sources authoritative while the candidates are
reviewed, then returns each contract to its direct named path after the old flat
source is removed. This keeps the permanent route shallower and avoids retaining
migration scaffolding.

### Boundary of the disagreement

The source-ID collision during simultaneous flat and direct-entrypoint presence
is factual. Whether `shared/` remains after that collision ends is a topology and
lifecycle choice. The execution direction below recommends temporary staging and
direct final paths, but this contextual file does not turn that recommendation
into a permanent Pattern or a cutover decision.

## Maintainer/Execution Direction

The execution recommendation carried into maintainer review is additive staging
under `commands/shared/` while the flat mixed sources remain authoritative. The
recommended tree is:

```text
commands/
  shared/
    _shared.md
    global-flags/
      _global-flags.md
      interface.md
      behavior.md
    source-references/
      _source-references.md
      interface.md
      behavior.md
```

The recommendation has these boundaries:

- Do not add a Technical Design file.
- Keep all three routing entrypoints free of contract facts. They route the
  sibling contract files and may state only the route's lifecycle and authority
  status needed for navigation.
- Treat `commands/global-flags/` and `commands/source-references/` as the
  intended final cutover locations only after the old flat sources are removed,
  unless maintainer review changes that direction.
- Allocate no new permanent or temporary I/B/T IDs.
- Record exact source-file and destination-heading coverage in the migration
  ledger. The exhaustive heading map below is the council's proposed coverage
  shape for that ledger; it does not itself accept the candidates.
- Leave existing consumer links on the authoritative mixed sources until the
  explicit cutover. Do not make staged candidates the authority by changing
  links early.

## Source-Heading Coverage Ledger

The source-heading column below preserves every heading from both authoritative
files, including the two nested collision headings. Destination paths are the
recommended staged paths under `commands/shared/`. The destination headings are
structural assignments for the ledger, not candidate contract prose.

The three entrypoints have no contract-fact destination. Their route identity,
lifecycle, and generated `Entries` belong to routing metadata only. Where one
source heading spans both contract roles, the table assigns each part once:
Interface owns the public meaning, and Behavior owns technology-neutral
resolution or conformance that links back to Interface.

### Global CLI Flags

| Authoritative source heading                                                                                                   | Staged destination file and heading                                                                                                                                                                                                               | Coverage boundary                                                                                                                                                                                                                                            |
| ------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [`global-flags.md`](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md) — `# Global CLI Flags` | `commands/shared/global-flags/_global-flags.md` — route identity only; `commands/shared/global-flags/interface.md` — `# Global CLI Flags Interface Contract`; `commands/shared/global-flags/behavior.md` — `# Global CLI Flags Behavior Contract` | Preserve the source identity and contract-set scope. The entrypoint receives no flag fact.                                                                                                                                                                   |
| [`global-flags.md`](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md) — `## Status`          | `commands/shared/global-flags/_global-flags.md` — `## Status And Authority`; candidate status blocks in `interface.md` — `## Status And Authority` and `behavior.md` — `## Status And Boundary`                                                   | Preserve lifecycle and authority metadata without presenting a candidate as current authority.                                                                                                                                                               |
| `global-flags.md` — `## Meaning Of Global`                                                                                     | `interface.md` — `## Meaning Of Global`; `behavior.md` — `## Shared Flag Invariants`                                                                                                                                                              | Interface owns the public definition of global scope. Behavior covers one shared registration, applicability, explicit no-op behavior, and authority boundaries by link rather than a second definition.                                                     |
| `global-flags.md` — `## Accepted Flags`                                                                                        | `interface.md` — `## Accepted Flags`                                                                                                                                                                                                              | Preserve the complete flag table, spellings, values, and meanings in one public owner.                                                                                                                                                                       |
| `global-flags.md` — `## \`--workspace <path>\``                                                                                | `interface.md` — `### \`--workspace <path>\``; `behavior.md`—`## Workspace Resolution`                                                                                                                                                            | Interface owns input form, selection meaning, reporting, and examples. Behavior owns exact-path resolution, current-directory fallback, relative-value base, normalization, directory precondition, and no-discovery rules, linked to the Interface heading. |
| `global-flags.md` — `## \`--json\``                                                                                            | `interface.md` — `### \`--json\``; `behavior.md`—`## Structured Presentation`                                                                                                                                                                     | Interface owns structured-result and stdout observables. Behavior owns one typed result, no rerun, prompt suppression, and diagnostic-stream conformance, without choosing the later schema.                                                                 |
| `global-flags.md` — `## \`--view=<compact\|expanded>\``                                                                        | `interface.md` — `### \`--view=<compact\|expanded>\``; `behavior.md`—`## Human Presentation`                                                                                                                                                      | Preserve the values, expanded default, compact/expanded boundaries, JSON no-op, authored-content preservation, and the rule that view changes no operation semantics.                                                                                        |
| `global-flags.md` — `## \`--verbose\``                                                                                         | `interface.md` — `### \`--verbose\``; `behavior.md`—`## Diagnostic Presentation`                                                                                                                                                                  | Interface owns the diagnostic option and observable non-effect. Behavior preserves bounded diagnostics, stderr conditions, redaction boundaries, and unchanged status/effects.                                                                               |
| `global-flags.md` — `## \`--help\``                                                                                            | `interface.md` — `### \`--help\``; `behavior.md`—`## Terminal Informational Modes`                                                                                                                                                                | Interface owns selected-path help behavior. Behavior preserves no workspace resolution, no domain execution, group handling, and accepted no-op composition.                                                                                                 |
| `global-flags.md` — `## \`--version\``                                                                                         | `interface.md` — `### \`--version\``; `behavior.md`—`## Terminal Informational Modes`                                                                                                                                                             | Interface owns distributed-version output. Behavior preserves no workspace/domain execution and the single-version wrapper relationship without deciding later distribution details.                                                                         |
| `global-flags.md` — `## Composition`                                                                                           | `interface.md` — `## Composition`; `behavior.md` — `## Composition And Terminal Modes`                                                                                                                                                            | Preserve composition, terminal-mode ordering, mutual exclusion, invalid domain input, and no-op rules without inventing precedence.                                                                                                                          |
| `global-flags.md` — `## Errors`                                                                                                | `interface.md` — `## Errors`; `behavior.md` — `## Error Conformance`                                                                                                                                                                              | Interface owns the public invalid, blocked, and no-op conditions. Behavior ensures validation and reporting follow those conditions without adding alternatives.                                                                                             |
| `global-flags.md` — `## Verification Requirements`                                                                             | `behavior.md` — `## Verification Requirements`                                                                                                                                                                                                    | Preserve every required evidence dimension, including registration, workspace selection, rendering, no-op, stream, terminal-mode, repetition, and wrapper-parity coverage.                                                                                   |
| `global-flags.md` — `## Related Sources`                                                                                       | `interface.md` and `behavior.md` — `## Related Sources`                                                                                                                                                                                           | Preserve every related-source link in the role-appropriate file. Links remain relationships, not copied definitions or authority changes.                                                                                                                    |

### CLI Source References

| Authoritative source heading                                                                                                                       | Staged destination file and heading                                                                                                                                                                                                                                             | Coverage boundary                                                                                                                                                                                          |
| -------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`source-references.md`](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md) — `# CLI Source References` | `commands/shared/source-references/_source-references.md` — route identity only; `commands/shared/source-references/interface.md` — `# CLI Source References Interface Contract`; `commands/shared/source-references/behavior.md` — `# CLI Source References Behavior Contract` | Preserve source identity and contract-set scope. The entrypoint receives no source-reference fact.                                                                                                         |
| [`source-references.md`](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md) — `## Status`               | `commands/shared/source-references/_source-references.md` — `## Status And Authority`; candidate status blocks in `interface.md` — `## Status And Authority` and `behavior.md` — `## Status And Boundary`                                                                       | Preserve lifecycle and authority metadata without making the staged files authoritative.                                                                                                                   |
| `source-references.md` — `## Purpose`                                                                                                              | `interface.md` — `## Purpose`; `behavior.md` — `## Reference Invariants`                                                                                                                                                                                                        | Interface owns the public distinction between automatic IDs and exact `.agents` paths. Behavior preserves the no-guessing and disambiguation boundary by link.                                             |
| `source-references.md` — `## Accepted Forms`                                                                                                       | `interface.md` — `## Accepted Forms`; `behavior.md` — `## Reference Classification`                                                                                                                                                                                             | Interface owns the three accepted forms and prefix rule. Behavior owns classification without treating path-like ID text as a filesystem path.                                                             |
| `source-references.md` — `## Automatic Source IDs`                                                                                                 | `interface.md` — `## Automatic Source IDs`; `behavior.md` — `## ID Derivation`                                                                                                                                                                                                  | Interface owns the observable derivation rules and examples. Behavior owns per-invocation derivation, recognized-entrypoint handling, exact case/Unicode preservation, and rejection boundaries.           |
| `source-references.md` — `## Entrypoints`                                                                                                          | `interface.md` — `## Entrypoints`; `behavior.md` — `## Entrypoint Identity`                                                                                                                                                                                                     | Preserve recognized-name omission from IDs, folder identity, ambiguous-entrypoint blocking, and exact-path inspection behavior.                                                                            |
| `source-references.md` — `## Quoting Spaces And Special Characters`                                                                                | `interface.md` — `## Quoting Spaces And Special Characters`; `behavior.md` — `## Input Normalization`                                                                                                                                                                           | Preserve shell quoting examples, quote removal, Windows guidance, and the rule that the CLI adds no second quote language.                                                                                 |
| `source-references.md` — `## Path Resolution`                                                                                                      | `interface.md` — `## Path Resolution`; `behavior.md` — `## Exact Path Resolution`                                                                                                                                                                                               | Interface owns accepted prefixes, canonical reported paths, source-kind requirement, and workspace boundary. Behavior owns resolution from the selected workspace and lexical/physical containment checks. |
| `source-references.md` — `## ID Resolution`                                                                                                        | `interface.md` — `## ID Resolution`; `behavior.md` — `## ID Resolution`                                                                                                                                                                                                         | Interface owns exact-match outcomes and the role of generated `Entries`. Behavior owns current-workspace enumeration, exact comparison, and no case/fuzzy/semantic fallback.                               |
| `source-references.md` — `## Collisions And Disambiguation`                                                                                        | `interface.md` — `## Collisions And Disambiguation`; `behavior.md` — `## Collision Handling`                                                                                                                                                                                    | Preserve every candidate, the prohibition on heuristic choice, and exact-path disambiguation.                                                                                                              |
| `source-references.md` — `### Interactive Use`                                                                                                     | `interface.md` — `### Interactive Use`; `behavior.md` — `### Interactive Use` under `## Collision Handling`                                                                                                                                                                     | Interface owns the user-visible candidate choice and its authority limit. Behavior owns completion of only the source-reference choice.                                                                    |
| `source-references.md` — `### Non-Interactive And JSON Use`                                                                                        | `interface.md` — `### Non-Interactive And JSON Use`; `behavior.md` — `### Non-Interactive And JSON Use` under `## Collision Handling`                                                                                                                                           | Preserve blocked non-interactive/JSON behavior, every candidate path, and exact-path next action without prompting or default selection.                                                                   |
| `source-references.md` — `## Overwrite Companions`                                                                                                 | `interface.md` — `## Overwrite Companions`; `behavior.md` — `## Overwrite Resolution`                                                                                                                                                                                           | Preserve one logical ID, base-then-overwrite read order, independent physical inspection, orphan behavior, and the prohibition on independent overwrite results.                                           |
| `source-references.md` — `## Command Use`                                                                                                          | `interface.md` — `## Command Use`; `behavior.md` — `## Operand Resolution`                                                                                                                                                                                                      | Preserve the shared operand contract and the boundary for command-specific external-source operands. Behavior applies the common resolution without granting write or ownership authority.                 |
| `source-references.md` — `## Result Display`                                                                                                       | `interface.md` — `## Result Display`; `behavior.md` — `## Result Construction`                                                                                                                                                                                                  | Preserve paired ID/path reporting, null ID for linked external content, canonical workspace-relative paths, and ambiguous-ID visibility.                                                                   |
| `source-references.md` — `## Errors`                                                                                                               | `interface.md` — `## Errors`; `behavior.md` — `## Error Conformance`                                                                                                                                                                                                            | Preserve invalid, blocked, ambiguous, unsupported, escaped, and structurally ambiguous cases together with original reference, interpretation, cause, and next action.                                     |
| `source-references.md` — `## Verification Requirements`                                                                                            | `behavior.md` — `## Verification Requirements`                                                                                                                                                                                                                                  | Preserve all identity, parsing, path, collision, overwrite, result, and repeatability evidence requirements.                                                                                               |
| `source-references.md` — `## Related Sources`                                                                                                      | `interface.md` and `behavior.md` — `## Related Sources`                                                                                                                                                                                                                         | Preserve every related-source link in the role-appropriate file. No related link becomes a copied source-reference definition.                                                                             |

This map is exhaustive at the source-heading level: 14 headings from
`global-flags.md` and 18 headings from `source-references.md`, including
`Interactive Use` and `Non-Interactive And JSON Use`, are represented. The
ledger must continue below this level by accounting for every paragraph, list,
table row, code example, link, requirement strength, condition, exception, and
unknown within each mapped heading.

## Cross-Layer Link-Only Rules

- Interface is the detailed owner of caller-visible spelling, grammar, defaults,
  accepted forms, observable results, public errors, examples, and non-goals
  taken from the authoritative source.
- Behavior links to the corresponding Interface heading for those facts. It
  defines only deterministic resolution, invariants, safety boundaries, result
  formation, recovery where the source defines it, and conformance evidence. It
  must not restate a second detailed flag or source-reference contract.
- The route entrypoints contain only route identity, lifecycle/authority status,
  sibling responsibilities, and generated `Entries`. `_shared.md` routes the two
  child scopes; each child entrypoint routes its Interface and Behavior files.
  None of these entrypoints defines a flag, source grammar, result, error, or
  verification fact.
- Existing consumers continue to link to the authoritative mixed files. From a
  future staged global-flags contract file, the relative link to the old source
  is `../../global-flags.md`. From a future staged source-references contract
  file, it is `../../source-references.md`. These path forms are link guidance,
  not a request to change consumers now.
- A consumer may link to a shared contract instead of copying its definition.
  The link does not change authority, loading, routing, responsibility, or
  lifecycle.
- The `Related Sources` sections preserve navigation relationships only. They do
  not promote the staged files, and they do not move a source fact across the
  Interface/Behavior boundary.
- At cutover, consumer links may be updated to the named direct final contract
  files only in the explicit reviewed change that removes the old flat sources
  and names the replacements.

## Preserved Unknowns

The split must carry these unknowns forward rather than deciding them:

- The exact shared structured-result schema and compatibility rules remain Gate 3
  work.
- The exact `--verbose` diagnostic fields and redaction rules remain open until
  real CLI failures provide evidence.
- Exact distributed-version and thin-wrapper mismatch behavior remains later
  distribution design.
- Backslash input, case compatibility, filesystem aliases, and exact physical
  identity for source paths remain Gate 3 parser/filesystem decisions.
- A command-specific external-source operand may have its own grammar; the
  shared source-reference grammar does not decide that grammar.
- Maintainer review may keep `commands/shared/` permanently or may retain the
  recommended temporary staging/direct-final transition.
- No cutover date, acceptance event, final link update, or old-source removal
  event is established by this council record.

## Likely Loss Traps

- Treating a complete heading map as proof of complete fact coverage and failing
  to reconcile a rule, table row, example, link, or nested condition inside it.
- Moving a mixed heading wholly to Interface or wholly to Behavior and losing the
  other layer's meaning, especially per-flag resolution, output, errors, and
  verification obligations.
- Weakening or strengthening defaults, no-op behavior, terminal-mode behavior,
  repetition rules, collision outcomes, overwrite framing, containment rules, or
  error status.
- Turning a preserved unknown into a new product choice, implementation choice,
  stream assignment, schema, or requirement label.
- Putting contract facts in `_shared.md`, `_global-flags.md`, or
  `_source-references.md`, or allowing generated `Entries` to look authoritative.
- Leaving a detailed definition in both Interface and Behavior instead of using a
  link-only reminder.
- Adding a Technical Design file because a destination folder exists, even though
  no technical design is requested here.
- Allocating new permanent or temporary I/B/T IDs, or treating path-derived
  source IDs as requirement IDs.
- Updating consumer links to staged candidates before cutover, or deleting the
  flat authority before mapping, review, and explicit replacement naming.
- Creating a direct final entrypoint while its flat source still exists and
  thereby reintroducing the source-ID collision the staging path avoids.
- Losing the source-relative link depth when moving from `commands/` into
  `commands/shared/`, or leaving a broken link in the staged contract set.

## Verification Checklist

- [ ] Compare the heading inventory with both authoritative files. Confirm all
      14 `global-flags.md` headings and all 18 `source-references.md` headings,
      including both nested collision headings, appear in the map.
- [ ] For every mapped heading, reconcile each paragraph, list item, table row,
      code sample, related link, requirement strength, default, condition, exception,
      and unknown to one exact destination file and heading in the migration ledger.
- [ ] Confirm Interface owns the complete public meaning and Behavior links to it
      for shared facts instead of creating competing definitions.
- [ ] Confirm the proposed tree has exactly the seven listed files, no Technical
      Design file, and no contract facts in any routing entrypoint.
- [ ] Confirm the flat mixed sources remain present and authoritative while the
      staged files are reviewed. Confirm no existing consumer link moves early.
- [ ] Derive staged source IDs from their physical paths and confirm they do not
      collide with the flat source IDs. Do not create either direct final entrypoint
      until its flat source has been removed.
- [ ] Confirm no new permanent or temporary I/B/T identifier appears in the
      candidates, ledger, or this record.
- [ ] Validate every current and planned cross-layer link from its actual file
      location. Check the staged `../../global-flags.md` and
      `../../source-references.md` forms before creating those files.
- [ ] Preserve every unknown listed above verbatim in substance and do not move
      it into an invented Technical Design decision.
- [ ] Before any cutover, require complete ledger coverage, resolved conflicts,
      independent authority/loss/locality review, explicit maintainer acceptance,
      updated consumer links, removed flat sources, regenerated navigation where
      applicable, and source-identity/link/contract verification.
- [ ] Preserve that this council round created only this prose file. Later
      authorized candidate preparation and ledger updates are separate changes. The
      council file is not an entrypoint, so it does not require a generated
      `Entries` update.

## Hard Stops

Stop staging or cutover if any of these conditions occurs:

- A source heading, fact, example, link, condition, exception, or unknown has no
  exact ledger destination.
- Interface and Behavior contain competing detailed definitions, or an
  entrypoint contains contract facts.
- A candidate is treated as authoritative, a consumer link is changed early, or
  the flat source is removed before explicit reviewed cutover.
- A direct final entrypoint coexists with its flat source and the source-ID
  collision remains.
- A new permanent or temporary I/B/T ID, Technical Design file, product choice,
  implementation choice, or schema is needed to complete the migration.
- Any unresolved Gate 3 or distribution boundary is silently decided during the
  split.
- A relative link, generated route, source ID, or preserved semantic rule fails
  verification.

No destructive cutover is authorized by this contextual council record.

## Related Sources

- [Review Queue](queue.md)
- [CLI Command Contracts](../../../crystallized/documents/cli/contracts/_contracts.md)
- [Global CLI Flags contract set](../../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md)
- [CLI Source References contract set](../../../crystallized/documents/cli/contracts/shared/source-references/_source-references.md)
- [CLI Contract Migration Ledger](../contract-migration-ledger.md)
- [CLI Command Contract Set Working Document](../../../crystallized/documents/cli/command-contract-set.md)
