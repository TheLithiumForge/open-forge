---
open-forge:
  description: Open Task 36 to design partial file merging by Extensions and to replace the comment guards in authored Markdown with a boundary an agent still reads as an instruction
  tags: [Memory, Working, CLI, Task, Extensions, Markers, Authoring, Contextual, Active]
---

# Task 36 — Extension Partial Merge And Guard Replacement

## Task state

- State: **Open, not started.** Raised by the maintainer on 2026-09-16.
- Owner: Root.
- Two questions in one Task because they share a mechanism: both are about how
  an Extension's content and a user's content coexist in one file.

## Question 1 — partial merging of files by Extensions

Today an Extension owns whole files and whole regions. The maintainer wants the
option of an Extension contributing **part** of a file — the example given was
adding headers — so that one file can carry both authored and Extension-supplied
content without the Extension owning the whole thing.

What has to be decided:

- **What the unit of contribution is.** A heading and its body? A named block?
  A list under a known heading? The Entries region already proves one shape
  works, and its history is the cautionary tale: see the `index` finding where a
  section that extended to end-of-file swallowed authored prose.
- **How ownership is recorded.** The ownership document already distinguishes
  `paths` from `regions`, so a partial contribution is closer to a region claim
  than a path claim. Decide whether regions generalise or whether this needs its
  own concept.
- **What happens on update.** If the user edits inside an Extension's
  contribution, does the update overwrite, skip, or report a divergence? This is
  the same question [Task 33](task33-managed-content-removal.md) and
  [Task 35](task35-removal-and-suppression-model.md) face for removal, and the
  answers should agree.
- **What happens on removal.** Removing an Extension must remove its
  contribution without taking the authored text around it.

## Question 2 — replace the comment guards

The generated-region comment guards are going away; that is already recorded as
accepted in Task 30's findings, where the Prettier churn was closed as "resolved
by deletion, not by a fix". B1 migrated the Entries region to **heading-based**
location. This Task decides what replaces guards everywhere else, including in
`AGENTS.md`-style authored instruction files.

Candidate boundaries, none chosen:

1. **A heading**, as B1 already did for Entries. Proven, parses with Markdig,
   survives formatters. Its weakness is the end-of-section boundary problem
   already seen once.
2. **A fenced block with an info string**, for example a fence tagged
   `open-forge` with the instructions inside.
3. **A pair of thematic breaks (`---`)**, proposed by the maintainer on
   2026-09-25 for `AGENTS.md` and `CLAUDE.md`. The Open Forge section sits
   between two `---` lines. It reads as a clearly separated section in any
   Markdown preview, where HTML comments look like noise, and the text between
   stays plain instructions. Check before choosing it: a `---` directly under a
   line of text makes that line a heading, so each break needs a blank line
   above it. A `---` on the first line of a file starts YAML frontmatter. Other
   content may use `---` too, so the pair probably needs a heading inside, such
   as `# Open Forge`, to identify the region. Frontmatter was also considered,
   but a file has only one frontmatter block and harnesses don't reliably treat
   it as instructions.
4. Something else — the Task should propose alternatives rather than pick from
   these.

**The risk the maintainer flagged is the one to test first, and it is not a
parsing question.** A fenced code block is valid Markdown and easy to locate
with the existing parser — but an agent reading the file may treat fenced
content as an *example* rather than as an instruction it must follow. That would
silently weaken every directive it wraps.

So this cannot be decided on parser convenience. It needs evidence about how
the content is *read*, not just how it is located:

- Write the same instruction in each candidate form and check whether an agent
  actually follows it. More than one model, since this is a behavioural claim
  about readers, not a property of the syntax.
- Note that Open Forge's own `.agents` tree is the test corpus — the workspace
  dogfoods itself, so a form that reads badly will degrade this repository's own
  instructions first.

## Actionable boundary

- Decide question 2 before question 1 if they conflict: the contribution
  mechanism may want the same boundary the guards are replaced with, and picking
  two different ones would be the worst outcome.
- Any boundary must be locatable with the Markdig parser already in use. Do not
  hand-roll a scanner; that is the defect `hand-rolled parsing` recorded.
- A formatter must not be able to break the boundary. Prettier inserting blank
  lines is what killed the comment guards; test the candidate against it.
- Preserve authored text absolutely. The `index` finding — a section boundary
  that ran to end-of-file and deleted a user's appended prose — is the standing
  example of what failure looks like here.

## Acceptance

- A recorded decision on the boundary form, with the agent-readability evidence
  that justified it, including what was tried and rejected.
- A recorded decision on the partial-contribution unit and its ownership
  representation, agreeing with Tasks 33 and 35 on update and removal
  behaviour.
- Guards removed wherever the new boundary replaces them, with the Prettier
  containment in `.prettierignore` reduced accordingly.
- No authored text can be lost by any boundary the decision introduces, proven
  by a test that appends prose in every position around a boundary.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
