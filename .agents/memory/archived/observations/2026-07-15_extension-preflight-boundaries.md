---
open-forge:
  description: Historical adversarial extension review of portable paths, topology, indirection, and generated-index preflight
  tags: [Memory, Archived, Observation, AgentLearning, Contextual, Historical, Extension, CLI, Security, Portability, Reliability]
---

# Observation: Extension Safety Lives Beyond The Payload File List

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/observations/2026-07-15_extension-preflight-boundaries.md`.  
Archived because: the verified preflight invariants shipped and remaining lifecycle boundaries were extracted to current planning.  
Current replacement: `.agents/memory/crystallized/decisions/extension-package-boundary.md`, `.agents/memory/crystallized/documents/extensions/architecture.md`, `docs/extensions.md`, installer tests, and `.agents/memory/working/backlog.md`.

Date: 2026-07-15, updated 2026-07-17. Scope: the dependency-aware extension installer, the 18-unit first-party catalogue, targeted integration tests, and independent read-only reviews.

- The first complete implementation already validated manifests, dependency closure, same-path byte collisions, local-block preservation, rollback, and real pack composition; its focused tests were green.
- Adversarial review still found independent boundary failures: an existing target junction could redirect writes outside the workspace, case-only paths could collapse on default macOS or Windows filesystems, a planned file could also be another file's required directory, case-folded `AGENTS.md` could be misclassified, `null` could bypass optional manifest typing, a nested reference named `scripts` could be overclassified as executable, and dry-run could miss malformed index regions.
- Each issue required a different identity: portable case-folded source identity, host target identity, path-prefix topology, existing-component `lstat` checks, strict own-field schema validation, anchored scope classification, and read-only secondary-effect validation.
- The implemented fixes and regression tests show that an install plan is not complete when payload bytes are known. It must also model target topology, target indirection, platform path equivalence, scope effects, and downstream generated artifacts before the first write. Real-path projection through the nearest existing target ancestor also prevents a lexical alias from redirecting installation back into its source package.
- Residual boundary: dry-run validates existing index marker/layout and entrypoint ambiguity but does not yet show the generated-index body diff or include generated index writes in its counts. Rollback is process-local; abrupt-process recovery, ownership, update/remove, and migration remain future lifecycle work.

Promotion candidate: keep the invariant in the extensions-and-CLI decision and future installer tests; promote to a general tooling-safety route only if the same multi-identity pattern recurs outside extensions.

## 2026-07-17 Follow-On

- A second adversarial pass found a transaction-visibility identity: an extension-provided `.gitignore` could pass preflight, hide its sibling outputs after application, and make Git falsely report no reviewable diff. Extension payloads now reject `.git/` and `.gitignore`; repository-control changes are separate reviewed work.
- Git checkpoint pathspecs are literal and target-scoped, while ignore input is NUL-delimited so path magic and newline-bearing legal filenames cannot alter the checked set.
- Core now plans direct managed and derived scoped writes together, preflights links and hardlinks, includes potential generated-index writes in Git visibility, and rolls the Core phase back if index planning or application fails.
- Real OS-temporary command tests cover self-hiding payload rejection, ignored extension output, Core rollback, hard-linked scoped files, and linked route-tree rejection across find, chain, doctor, and index.
