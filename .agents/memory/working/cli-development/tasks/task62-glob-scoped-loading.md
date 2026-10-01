---
open-forge:
  description: Implement optional applyTo file conditions, loading rules, Entries display, and CLI context filtering through accepted execution packets
  tags: [Memory, Working, Task, Framework, Loading, Frontmatter, CLI, Contextual, Complete]
---

# Task 62: Glob-scoped loading

## Outcome

Add an optional file condition that makes relevant routed context load when a
task works on matching paths. Keep its rules simple enough to follow without
the CLI. The CLI should compute the same selection through `context --for`.

## Accepted direction

The maintainer clarified this scope on 2026-09-29:

- Use the key `applyTo`. Accept it at the root of YAML frontmatter and under
  `open-forge:`, including root `applyTo` beside an existing scoped metadata block.
- Support `context --for <path>` to pre-filter context for the task's files.
- Define loading and scope rules first, then optional-field support in the CLI,
  then filtering on the selected commands.
- Explain related edits outside a glob without making agents guess whether
  they may edit a necessary caller, dependency, test, or context source.
- Update documentation at the beginning, during delivery, and at completion.
- Keep alternative roots, placing the Framework inside `.apm/`, multi-root
  behavior, and APM integration in the separate restored
  [Task 55](task55-alternative-root.md).
- The subsequent maintainer acceptance authorized all recommendations, detailed action planning, and parallel implementation.

These requirements are implemented and squash-integrated into `develop`, then
promoted to `main`.
The full managed and Windows Native AOT gates passed. Root-level `description`, `tags`, and
`responsibility` were not accepted by this clarification.

## Current state

The maintainer accepted all recommendations on 2026-09-29 and authorized
parallel implementation. The command, Framework and documentation slices are
integrated, reviewed and verified. Commit
`00e3ba0294679163d95b42a81295091254debfb7` includes the squash and beta 2
version preparation and is pushed to `develop` and `main`. The documentation
deployment passed; [Task 59](task59-beta-2-release.md) records the completed
release and published-package/site verification. The [execution packet](task62/_task62.md)
records frozen decisions, detailed steps, ownership, dependencies and verification.
The [analysis](../../../emerging/analysis/glob-scoped-loading.md) is frozen as
accepted design input. Subsequent discoveries and decisions belong in the
execution packet and affected current contracts.

The 2026-09-28 folding of Task 55 is superseded. Its permanent ID is restored
separately. Task 62 does not choose a Framework root or require APM integration.

## Accepted delivery sequence

The execution packet expands this accepted sequence into owned slices.

1. **Define the rules and initial documentation.** Use the accepted analysis
   decisions and examples. Record the accepted semantics and behavior
   matrix. Prepare the Loader text and Framework Markdown/loading contracts
   before dependent code. Keep future behavior labeled until it ships.
2. **Support the optional field.** Inspect current metadata readers, source
   models, authoring commands, generated Entries, and their direct consumers.
   Freeze one parsing and matching contract. Add root and scoped input,
   validation, authoring, and the agreed Entries projection. Preserve files
   without `applyTo` and unrelated frontmatter. Update syntax documentation and
   examples alongside this work.
3. **Add deterministic selection.** Implement repeatable `context --for <path>`
   using the accepted route traversal and inherited conditions. Add the agreed
   `find --for` discovery and `route inspect --for` explanation surfaces.
   Update command contracts, help, and examples in the same changes. Do not
   filter Index or hide Doctor's structural checks.
4. **Verify the complete experience.** Cover the analysis scenario table through
   public CLI tests, including new files, mixed scopes, links, context-source
   edits, unknown paths, overwrites, and unconditioned behavior. Try the same
   C# and TypeScript examples through plain Markdown and the CLI. Run the
   repository's applicable build and test checks after inspecting their current
   instructions. Review text and JSON output and generated Entries.
5. **Finish documentation and acceptance.** Reconcile the shipped Loader,
   Framework contracts, CLI help, public guides, and examples with demonstrated
   behavior. Remove obsolete proposals from current instructions, retain useful
   rationale, and report remaining limitations. Obtain acceptance of the result.

Documentation is part of every stage. It is not deferred to the final stage.
An implementation packet must inspect current source paths and verification
commands rather than reuse the older analysis's pre-migration code paths.

State: Complete.

Task 62 “Glob-scoped loading” (phase 3/3): milestone 6/6.

The [final receipts](task62/execution.md#final-gate-receipts) record 9331 passing
test executions across six modes, the native CLI smoke journey, documentation
checks and the pre-existing repository Doctor findings.

## Completion criteria

- [x] The maintainer accepts one coherent loading, metadata, and Entries contract.
- [x] Plain Markdown and CLI selection follow the same declared scope boundaries.
- [x] Related work outside a glob has a clear rule and verified examples.
- [x] Optional-field parsing, authoring, indexing, inspection, and selected
      filtering commands are implemented with compatible unconditioned behavior.
- [x] Documentation is updated before dependent work, alongside changes, and
      verified against the completed result.
- [x] Public-interface evidence covers the accepted behavior matrix.

## Related work

- [Task 53](task53-loading-and-scoping-audit.md) audits current loading. Coordinate
  shared Loader wording with the verified Task 62 rules before integrating either task.
- [Task 54](task54-tag-trimming.md) shares the Entries and metadata surface.
- [Task 55](task55-alternative-root.md) owns alternative roots and APM integration.
- [Task 36](task36-extension-merge-and-guards.md) supplies context about readable
  Markdown boundaries. Native harness adapters remain separate work.
