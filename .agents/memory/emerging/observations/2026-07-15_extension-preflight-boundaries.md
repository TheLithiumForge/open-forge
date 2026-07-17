---
open-forge:
  description: Adversarial extension review found that safe composition requires portable path, topology, indirection, and generated-index preflight beyond byte collision checks
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, Extension, CLI, Security, Portability, Reliability]
---

# Observation: Extension Safety Lives Beyond The Payload File List

Date: 2026-07-15. Scope: the new dependency-aware extension installer, eight first-party packs, targeted integration tests, and two independent read-only reviews.

- The first complete implementation already validated manifests, dependency closure, same-path byte collisions, local-block preservation, rollback, and real pack composition; its focused tests were green.
- Adversarial review still found independent boundary failures: an existing target junction could redirect writes outside the workspace, case-only paths could collapse on default macOS or Windows filesystems, a planned file could also be another file's required directory, case-folded `AGENTS.md` could be misclassified, `null` could bypass optional manifest typing, a nested reference named `scripts` could be overclassified as executable, and dry-run could miss malformed index regions.
- Each issue required a different identity: portable case-folded source identity, host target identity, path-prefix topology, existing-component `lstat` checks, strict own-field schema validation, anchored scope classification, and read-only secondary-effect validation.
- The implemented fixes and regression tests show that an install plan is not complete when payload bytes are known. It must also model target topology, target indirection, platform path equivalence, scope effects, and downstream generated artifacts before the first write. Real-path projection through the nearest existing target ancestor also prevents a lexical alias from redirecting installation back into its source package.
- Residual boundary: dry-run validates existing index marker/layout and entrypoint ambiguity but does not yet show the generated-index body diff or include generated index writes in its counts. Rollback is process-local; abrupt-process recovery, ownership, update/remove, and migration remain future lifecycle work.

Promotion candidate: keep the invariant in the extensions-and-CLI decision and future installer tests; promote to a general tooling-safety route only if the same multi-identity pattern recurs outside extensions.
