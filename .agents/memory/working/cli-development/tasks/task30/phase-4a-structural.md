---
open-forge:
  description: Task 30 structural and parser follow-up for generated Entries, Axioms, and authored document handling
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Structural, Parsing]
---

# Task 30 — Phase 4a Structural And Parser Follow-up

## Status

Items 1 and 3 are complete in [B1](07-b1-heading-entries.md), with passing
managed/native qualification and reviewed same-commit contracts. The accepted direction migrates Entries to headings
and automatically removes retired guards inside those sections. Items 2, 4 and 5
remain deferred pending Framework decisions; B1 does not implement them. The
accepted two-file state model is unchanged. The former stop-before-G4 boundary
is superseded; G4 is complete.

## Evidence source

- [Structural requirements and markers](../../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md)
- [Hand-rolled parsing](../../../../emerging/analysis/cli-experience-audit/hand-rolled-parsing.md)
- [Lifecycle baselines and architecture](../../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md)
- [Repository dogfood and configuration](../../../../emerging/analysis/cli-experience-audit/repository-dogfood-and-configuration.md)

The source records found that generated-region markers churn with Prettier,
the loader still has a stale marker-based path, and several Markdown/YAML
surfaces hand-walk formats already represented by Markdig or YamlDotNet. The
front-matter fence symptom and strict unknown-SKILL-key symptom were rechecked
against current code and are fixed; B1 now closes the selected Entries/Axioms parser duplication.

## Actionable boundary

1. **Complete in B1.** Replace the generated `Entries` comment-guard contract with the accepted
   heading-based region lookup, or record a maintainer decision to retain the
   guards. The current direction is deletion: migrate existing files safely,
   prove repeated `index` runs are byte-stable, and remove the Prettier
   containment that exists only for the old shape.
2. **Deferred.** Decide the `Axioms` contract for absent and empty sections against the
   current canonical Markdown decision. If inheritance is accepted, remove the
   nonempty placeholder requirement and update the reader, templates, and
   diagnostics together. Do not invent a placeholder to satisfy a parser.
3. **Complete in B1.** Consolidate or explicitly bound the Route Inspect Axioms parser and the
   generated-region parser around the existing Markdig AST. Compare headings,
   fences, BOMs, and trailing whitespace with focused tests before deleting a
   helper.
4. **Deferred.** Give the TypeScript agent tooling one shared path/region grammar or record
   why its regex parser is a deliberately separate format boundary. It must not
   silently diverge from the C# CLI for the same authored document.
5. **Deferred.** Keep loader/AGENTS maintenance content out of the runtime loader when the
   accepted Framework placement is settled; update the relevant directive,
   template, and shipped payload in one change.

## Acceptance

- Existing `.agents` documents and the shipped payload survive migration with
  no lost content or duplicate headings.
- `index` is idempotent and no longer churns solely because Prettier ran.
- Valid headings, BOMs, CRLF/LF input, empty/absent Axioms, and malformed input
  have explicit tests and diagnostics.
- The chosen parser owner is recorded; no second parser is retained merely as
  an unexamined compatibility path.
- The change is reviewed as a structural/document contract change before
  implementation. G1 lock/state behavior and public JSON/exit contracts are
  unchanged unless separately accepted.

## Stop conditions

Stop before source mutation if heading-based Entries, Axioms inheritance, a
parser dependency, or loader-content relocation is not accepted by the
relevant Framework/CLI owner. Keep the finding in this subtask rather than
silently fixing a contract at implementation time.

## Managed Host Heading Direction

After B1 and local integration, the maintainer requested avoiding legacy files
and markers, confirmed automatic removal of retired Index guards, and suggested
using a heading such as `# Open Forge` to replace the AGENTS managed section's
body. Index cleanup is already implemented and qualified in B1.

AGENTS currently uses one exact ordered `open-forge:start` / `open-forge:end`
comment pair. `FrameworkContentIdentity.ReadManagedBlock` also serves CLAUDE,
whose managed body contains the native `@AGENTS.md` and `@.agents/loader.md`
imports. This is current behavior, not a decision to retain comments indefinitely.

Prepare one focused heading-boundary contract before implementation. The proposed
shape is a unique top-level `# Open Forge` section, with replacement confined to
its body and ending at the next top-level heading or end of file. Preserve all
user-authored content outside it and keep CLAUDE's native imports intact.
Migration must preserve text after an old end marker even when that text has no
heading; simply deleting the comments would make that text part of the managed
section on the next update. Resolve this existing-workspace boundary in the
contract instead of assuming the comments can be removed without consequence.

The maintainer's direction is recorded; the exact heading grammar, safe legacy
conversion and CLAUDE treatment are not frozen. No managed-host parser, payload,
root entry, bridge, or maintenance contract was changed in this prose correction.
This format decision is separate from deferred item 5's content-placement choice
and does not require a renderer rewrite.
