---
open-forge:
  description: Historical preliminary design for making discovery commands easy to find without merging their meanings
  responsibility: Preserve the historical Discovery-family review without making it current command authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, PreliminaryDesign, Discovery, Help, Capability]
---

# Discovery Family Preliminary Design

## Status

This file was archived from the [active CLI Review Queue](queue.md) on 2026-08-15 after Queue item 12 settled. The current [CLI-D076 Discovery decision](../../../working/cli-release/decision-agenda.md) accepts a prominent manually organized root-help and documentation section, with no `discover` command or executable manifest. This file preserves historical alternatives and evidence and does not define current command authority.

- **Origin:** `.agents/memory/working/cli-release/review/discovery-family.md`
- **Archived because:** Queue item 12 left active review after the maintainer accepted the manually organized Discovery section and rejected a separate command and executable manifest.
- **Current source:** [CLI-D076 Discovery decision](../../../working/cli-release/decision-agenda.md)

The remainder of this file preserves the former review record. Its alternatives
and evidence are historical context, not current fallback designs.

## User Job

An agent entering an unfamiliar workspace should quickly learn:

- Which discovery capabilities exist.
- Which files and routes are available.
- Which operation reports each kind of fact.
- Which relationships were inspected.
- Whether no result is complete, the capability is unavailable, the operation
  was not requested, or the inspection was incomplete.

The desired outcome is not semantic search. It is an interface that makes
inspection easier and safer than proceeding from assumptions.

## Historical Accepted Direction

The following records the accepted direction as it appeared during review. The
linked Decision Agenda item above remains the current source.

Make **Discovery** a prominent category in root help and documentation.
Cross-list the canonical domain operations with concrete questions:

```text
What sources exist or match exact metadata?    find
What routed material exists?                   route list
What is true about this route?                 route inspect
What ordinary links enter or leave a source?  <reference operation>
What context would be selected, in order?      <context path projection>
```

Keep these domain operations canonical. Do not relocate accepted `find`, route
operations, or context projections under a new namespace merely to create visual
symmetry.

Do not add `discover`, a discovery namespace, or an executable discovery
manifest. The accepted command-definition source may generate human or
structured help and completion during Gate 3, but those are presentations of the
same help surface rather than another domain operation.

## Help Availability And Coverage Wording

Help must keep these dimensions separate:

- **Capability availability:** available, unavailable, disabled, or planned in
  the current executable.
- **Operation coverage:** the kinds of facts each command can inspect and the
  relationships it deliberately excludes.
- **Runtime result:** the selected domain operation, not help, reports whether
  inspection completed, was incomplete, or returned no records.

An unavailable capability is not an empty workspace result. Help does not claim
that any workspace domain was inspected. A complete empty result exists only
after the user runs the applicable domain operation.

## Boundaries

The Discovery family does not mean relevance. It provides no fuzzy terms,
semantic ranking, recommendations, generic `related` edge, hidden persistent
index, or claim that one operation inspected every fact type.

Root help, group help, completion, and any structured help should derive from one
accepted command-definition source when Gate 3 defines it. This design does not
choose the registry technology.

## Historical Alternatives And Disposition

### Canonical `discover` Namespace

```text
discover routes
discover references
discover neighborhood
```

This is highly visible but duplicates or displaces the operations that define
each fact. It becomes misleading if the family implies one search model or grows
into a miscellaneous read-only bucket. The maintainer rejected this namespace.

### Help-Only Family

This has no runtime duplication and preserves shallow direct commands. Its risk
is that autonomous agents may not receive or inspect root help and cannot report
which discovery planes they skipped. The maintainer accepted this direction and
requires evidence that manually organized help makes each operation findable.

### Thin Executable Manifest

This would provide one machine-readable first call, but the maintainer rejected
it because it duplicates help, risks drift and false completeness, and creates
another operation without a distinct workspace job. It is not a fallback if
help configuration or testing is difficult.

## Remaining Review Questions

1. Can the selected command library manually group and order the root help
   section without introducing another command path?
2. Which concrete user questions should appear beside each discovery operation?
3. How should unavailable or later capabilities appear without implying that
   the current executable supports them?

## Evidence Needed

Cold-start tests should exercise root help across fixed tasks for sources,
routes, references, and context paths. The help design passes when agents find
each applicable domain operation, understand its relationship boundary, and do
not confuse unavailable, unrequested, incomplete, or complete-empty runtime
states. Record first-attempt success, tool calls, output size, and time. Gate 3
must also prove that human, structured-help, and completion presentations derive
from the same command-definition source.

## Related Evidence

- [Council Synthesis](path-reference-council.md)
- [Route Catalogue](route-catalog.md)
- [Find contract set](../../../crystallized/documents/cli/contracts/find/_find.md)
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
