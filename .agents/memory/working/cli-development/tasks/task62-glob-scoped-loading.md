---
open-forge:
  description: Open Task 62 to investigate an applies-to glob, like GitHub Copilot's applyTo, as a loading condition, and whether description and tags may also sit at the root of frontmatter
  tags: [Memory, Working, Task, Framework, Loading, Frontmatter, Tags, CLI, Investigation, Contextual, Active]
---

# Task 62 — Glob-scoped loading and root-level frontmatter

## Outcome

Recorded at the maintainer's request on 2026-09-27. Decide whether Open Forge
should support a file-glob condition on routed sources, similar to the
`applyTo` property in GitHub Copilot's instruction files, and whether
`description`, `tags`, and such a field may appear at the root of the
frontmatter as well as under the `open-forge:` scope.

**Direction:** the maintainer does not want Copilot-style instruction files:
routing already does more. The interest is the glob condition itself. It could
add a third way to narrow loading, next to scopes and the `LoadNow` and
`KeepInMind` tags: a route loads progressively, and only when the files the
task works on match its glob.

## Questions

1. **Semantics.** What does a glob mean for a routed source? For example, an
   entry that is on demand unless the task touches a matching file, or a
   `LoadNow` entry that loads at startup only when a match is known. Decide
   who evaluates the match: the agent from the task's files, a harness that
   supports the field natively, or the CLI when given paths. Decide how it
   combines with scopes and inheritance, and what "no match" means when the
   working set isn't known yet.
2. **Generated `Entries`.** Project the glob into the parent's generated entry
   line, next to the tags, so an agent sees the condition without opening the
   file. Choose the format and keep it parseable.
3. **Root-level frontmatter.** The [canonical syntax](../../../../memory/crystallized/documents/framework/markdown/syntax.md#frontmatter)
   currently requires `description`, `responsibility`, and `tags` under
   `open-forge:`. Investigate accepting them, and the glob field, at the root of
   the frontmatter too. Other tools read root-level keys such as
   `description` and `applyTo`, so a shared root key could give some harnesses
   native support without extra code. Weigh that against collisions with other
   tools' keys and the fail-closed metadata rule. Keep the
   [compatibility boundary](../../../../memory/crystallized/documents/framework/markdown/compatibility.md)
   explicit about canonical output versus accepted input.
4. **CLI support.** `find` and `context` could accept file paths and return the
   sources whose globs match, and `context` could state where each piece of
   context came from: the source path, the scope, and the matched glob.
5. **Harness compatibility.** Build a matrix of which agents and editors read
   which frontmatter keys and glob fields, for example GitHub Copilot's
   `applyTo` and similar fields in other rule formats. Verify each entry
   against its vendor's current documentation. Don't rely on memory.

## Related work

- [Task 53](task53-loading-and-scoping-audit.md) audits what loads at startup.
  A glob condition changes that model and should be decided alongside it.
- [Task 54](task54-tag-trimming.md) trims tags, and would share the Entries
  format with a projected glob.
- [Task 55](../../../archived/cli-development/tasks/task55-alternative-root.md) investigates an `.apm` root and APM
  frontmatter. Root-level keys are the same question from another direction.
- [Task 36](task36-extension-merge-and-guards.md) notes that harnesses don't
  reliably read frontmatter, which matters for any claim of native support.

## Actionable boundary

- Investigation only. No Framework, Extension, or CLI change until the
  decisions below are accepted.
- Public research into vendor documentation is part of the investigation. It
  authorizes reading, not installing tools or changing external systems.

## Done when

- [ ] A recorded decision to accept, reject, or defer the glob field, with the
      semantics, the key name, root versus scoped placement, the Entries
      projection, and the evidence behind each choice.
- [ ] A harness compatibility matrix with sources.
- [ ] If accepted, a follow-up implementation Task covering the Framework
      contract, the loader wording, generated `Entries`, and the CLI.

## Current State

**Now:** analysis started on 2026-09-28, awaiting the maintainer's choices. The
[Glob-Scoped Loading Analysis](../../../emerging/analysis/glob-scoped-loading.md)
recommends an optional `applyTo` list in the `open-forge:` block, projected into
the entry line as ``applies to `glob` `` code spans. It triggers opening an
entry before work on a matching file, and never activates an unselected scope.
The CLI support centers on `context --for <path>`. Native harness support would
come later through generated pointer files, since almost every tool reads glob
rules only from its own folder. Antigravity reads them from `.agents/rules/`,
and several tools already read Skills from `.agents/skills/`. It also compares
the glob keys of eleven tools, from official documentation.

**Choices for the maintainer:** the entry-line form (labeled segment or marker
tag), the key name (`applyTo` or `applies-to`), and whether root-level
`description` and `applyTo` are accepted as compatible input.

## Folded in on 2026-09-28

From [Task 55](../../../archived/cli-development/tasks/task55-alternative-root.md), following its
[review](../../../emerging/analysis/open-task-review/task55-alternative-root.md):

- **Keep `.agents/` as the only root,** recorded as a Framework Decision. APM
  handles only some primitive types and compiles instructions into files that
  always load, and it already places Skills under `.agents/skills/`.
- **APM interoperability.** Accept root-level `description` and `applyTo` as
  input, which this Task already covers, and consider an optional export of
  `.apm/instructions/*.instructions.md` pointer files for glob-scoped routes.
- **One prototype worth running.** In a scratch workspace, install one APM Skill
  package into an Open Forge workspace. Confirm that `open-forge index` lists
  the deployed Skill and that `doctor` handles APM's files under `.agents/`
  sensibly.
