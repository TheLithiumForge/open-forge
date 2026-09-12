---
open-forge:
  description: Recorded language findings that informed the release preparation rewrite
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Markdown language and clarity analysis

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Prepared 2026-09-11 after the substantive Task 28 changes. These are recommendations for user discussion. They have not been applied as a blanket language pass.

The strongest remaining gains come from separating current instructions from historical reference, exposing branches in dense local workflows, and explaining specialist terms at their first useful appearance. The revised portable workflows are already small enough that further compression would often remove useful meaning.

## Coverage And Limits

The inventory covers all 703 existing Git-visible first-party Markdown files, including newly added files and hidden repository directories:

| Group | Files | Treatment |
| --- | ---: | --- |
| Current dogfood and maintenance | 317 | Scan for density and terminology; inspect high-impact examples |
| Public or installable | 54 | Check introductory paths, current versus historical framing, and workflow readability |
| Authored agent sources | 27 | Scan separately; preserve role and runtime contracts |
| Working records | 121 | Inventory; avoid rewriting task history as polished current documentation |
| Emerging records | 37 | Inventory; preserve uncertainty and candidate status |
| Historical or frozen | 143 | Inventory only for this language pass; no historical rewrite |
| Other first-party Markdown | 4 | Inventory |

All files received a mechanical inventory and text scan. The analysis also uses the complete workflow audit and close reading of selected public, maintenance, and local workflow passages. It is not a claim that every paragraph in 703 files received a line-by-line editorial review. Ignored build artifacts, dependency trees, and runtime-generated files outside Git visibility are excluded.

The scan found 44 long-line candidates in current documents and authored agent sources using a 70-word threshold after excluding table rows and generated entry labels. A long line is a navigation aid for inspection, not an error or a readability score. Technical terms were counted to locate families of usage, not to justify global replacement.

Evidence: `markdown-inventory.json`, `markdown-language-candidates.json`, and `workflow-audit.md`.

## L1: Separate Current Instructions From Historical Reference

**Recommendation:** Make the current Extension page a complete current user guide. Keep legacy behavior in a clearly bounded historical section, with links to the existing Legacy CLI reference where it already covers the subject. Remove repeated legacy tutorials that add no retained value. Do not add loader content or a new shipped guide.

**Evidence:** `docs/extensions.md:117` marks the following content as frozen history, but later peer headings such as `Manual Installation` at line 266 and `Create A Local Extension` at line 278 look like standalone current instructions. The manual example uses `payload/`, while current package sources use `content/`. The general header at line 160 calls its example the “current first-party Extension.” The existing warning establishes the intended distinction, but a reader arriving through an anchor can miss it.

**Concrete change:** The current manual section would say:

> Copy the selected package's `content/` files and its dependencies into the workspace. Update affected route indexes and check links in the assembled workspace. Manual copying does not create managed lifecycle state.

Keep the old format named explicitly wherever historical examples remain. Current setup should use the same source and package selection for preview and apply. The catalogue already does this after the substantive correction.

**Boundary:** This is an information-structure change with command-sensitive examples. Preserve the accepted native lifecycle contracts, the still-unreleased status, and genuine legacy distinctions. Verify every retained command and link. Do not imply that rewriting a guide updates CLI behavior.

## L2: Make Workflow Branches Visible

**Recommendation:** Keep the existing Goal, Steps, and Completion contract. Within Steps, separate ordinary execution from conditional coordination or trial behavior with short subheadings and compact input tables. Keep a shared intake definition in its defining source and link to it from consuming recipes.

**Evidence:** `.agents/workflows/review.md:16` contains a 159-word step combining two review paths, budget accounting, coordinator responsibility, snapshot fields, ancestry validation, and topic boundaries. Related detail is repeated in `.agents/workflows/worktree-program-development.md:27`, `program-development.md:21`, `adaptive-development.md:21`, and `development/task-lifecycle.md:21`.

**Concrete opening, before the complete retained field list:**

> Choose ordinary or coordinated review before preparing its inputs. Ordinary review uses the accepted baseline and the explicit changed and untracked target. Coordinated review uses independently owned immutable snapshots and the topic units assigned to each snapshot.

Then separate the two paths and retain every required input, budget rule, protected boundary, and recheck condition. This example introduces the structure; it is not a replacement that drops the remaining requirements.

**Boundary:** Do not flatten the local assured or coordinated profiles into the portable recipe. Do not remove a required field or reduce required review under the banner of readability. Recheck source-to-consumer links after consolidating repeated instructions.

## L3: Explain Specialist Terms Through Their Actual Meaning

**Recommendation:** Use familiar words where they preserve the exact meaning. Where a specialist term carries a real distinction, explain it briefly or link to its definition at first useful use in each standalone source. Avoid a global search-and-replace or a new glossary loaded into every task.

**Evidence:** `.agents/workflows/planning.md:20–21` introduces “archetype,” “golden slice,” “Execution Capsule,” “placement map,” “direct integration neighborhood,” and “evidence ladder” together. The scan finds “horizon” in 24 current source files and “frontier” in 10. Their frequency is not itself a defect; unexplained use at an entry boundary is the issue.

**Concrete candidates:**

| Current phrase | Candidate wording when the meaning matches |
| --- | --- |
| Review horizon | Review scope |
| Unresolved frontier | Decisions that remain unresolved |
| Execution capsule | Compact task context, with the locally defined fields linked |
| Golden slice | The accepted example that later tasks follow |
| Direct integration neighborhood | Directly affected callers, dependencies, and shared components |
| Disposition findings | Record which findings are accepted, rejected, deferred, or fixed |

`semantic`, `projection`, `invariant`, and `receipt` often describe real technical contracts. Keep those terms where precision depends on them. Likewise, preserve the established distinction between a canonical byte source and an authoritative answer.

**Boundary:** Adopt wording family by family after inspecting usage. A change to “owner,” for example, must preserve the distinction between responsibility, decision authority, and managed lifecycle ownership.

## L4: Turn Source Maps Into Readable Relationships

**Recommendation:** Replace long maintenance introductions that list many authorities with a short purpose statement and a source/question table. Keep rationale links in their existing dedicated section.

**Evidence:** `.agents/memory/crystallized/documents/maintenance/payload/agents/loader.md:14` combines structural authority, top-level composition, five routing topics, Markdown representation, and seven decision references in one 121-word paragraph.

**Concrete shape:**

| Source | Question it answers |
| --- | --- |
| Framework Architecture | What structural role does the loader have? |
| Top architecture | How do Core, Memory, and Extensions relate? |
| Routing contracts | How are routes selected, loaded, scoped, and customized? |
| Routed Markdown contract | How is authored and generated content represented? |

Keep the actual links and place the decision links with rationale. This makes each source's slice of truth easier to inspect without copying its contents.

**Boundary:** A table is appropriate for this mapping, not a requirement for every paragraph or document. It must not imply that links combine source authority.

## Separate Semantic Follow-Up

The dogfood Observation Template has evidence-sanitation and conclusion fields absent from its packaged counterpart. Both copies are unchanged from the review baseline. This is a content and role decision, not a spelling fix. A later reconciliation should decide which fields belong in the reusable shape and which continuing rules belong in a Directive or Guidance. Keep that discussion separate from a language-only rewrite.

The native CLI's embedded catalogue and diagnostic gaps also remain implementation work. Clearer language must neither hide those gaps nor change package semantics to silence them.

## Suggested Review Order

Discuss L1 first, then L2, L3, and L4. Apply each accepted recommendation to one coherent family and review its final meaning and navigation before proceeding. Keep diagrams deferred. Preserve historical statements, candidate status, optionality, requirements, authority, and scope throughout.
