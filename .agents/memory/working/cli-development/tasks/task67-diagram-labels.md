---
open-forge:
  description: Task 67 council redesign of the framework diagram's labels, so what loads when and where content comes from reads at a glance and stays accurate
  tags: [Memory, Working, Task, Documentation, Diagram, Loading, Council, Writing, Contextual, Active]
---

# Task 67: Diagram labels

## Outcome

Requested by the maintainer on 2026-09-28, after reviewing the diagram from
[Task 61](task61-documentation-accuracy-and-voice.md) and the polish in
[Task 66](task66-council-polish.md). The diagram has too many label types:
four loading badges, two source chips, and several notes. A reader has to
study the legend before the picture makes sense.

**Direction from the maintainer:**

- Simplify the labels so the essence reads at a glance, while staying
  completely accurate. The README and its diagram give the essence. The
  documentation then gives more in manageable chunks that invite the reader
  to keep going.
- The Extension source chips could go. A "Ships with Core" label is probably
  unnecessary, because everything the diagram shows ships with Core.
- Separate entrypoints from entries. An entrypoint loads at startup or on
  demand. Entries open on demand unless the user tags them to load.
- Say that the tags have CLI meaning, as far as that is true.
- Run a council with Astra and a Claude agent, then apply what holds up.
- Work on a new branch on top of `task66-council-polish`. No merge into
  `develop` or `main` and no push.

**Preserve / out of scope:** the shipped Framework and Extension files, the
CLI, and the diagram's content beyond its labels and notes.

**Done when:**

- [x] The council has reported, and the synthesis records what changed the
      recommendation.
- [x] The site diagram, the README SVGs, the alt text, and every page that
      uses the loading labels agree.
- [x] The site builds with link checks, and the diagram works at phone width.
- [ ] The maintainer reviews the branch.

## Current State

**Now:** the pitch round with Astra is applied and committed on
`task67-diagram-labels`. The maintainer reviews, merges, and pushes.

**Evidence:** the site builds with broken-link checks and `tsc` passes. The
diagram was checked in the browser at desktop width, in the docs column, at
375 pixels with no horizontal scroll, and in dark mode. Both README SVGs were
regenerated and checked.

**CLI meaning of tags**, checked in `src/cli/`: `context`, `route inspect`,
and `status` compute loading from `#LoadNow` and `#KeepInMind`.
`route create --template` requires `#Template`. `find --tag` searches any
tag. The other defined tags have no CLI behavior beyond search.

## Council on 2026-09-28

Two independent members answered the same brief: OpenAI Codex's
`gpt-6-astra` at high effort, run read-only through Worker Watch, read the
diagram as a first-time reader. A Claude Opus agent checked every label
against the files, the CLI, and both renderings. Neither saw the other's
position.

**Both proposed independently, and applied:**

- Two loading values instead of four, both about entrypoints: "Entrypoint at
  startup" and "Entrypoint on demand". The word "Entrypoint" stays on the
  badge, so a reader doesn't assume a whole category loads.
- No legend, no re-read badge, and no source chips.
- What Extensions add becomes a plain line on each card, such as "Extensions
  add Checkpoints and Handoffs." The Skills card says it starts with
  `open-forge-cli`.
- Emerging's re-read moves into its card: "Tagged `#KeepInMind`, so it's also
  re-read at each refresh point."
- The Memory heading carries its own badge, because the Memory entrypoint
  loads at startup too.
- One caption explains entrypoints and entries: entries open on demand unless
  their line is tagged `#LoadNow` or `#KeepInMind`.

**Where they differed, and the choice:**

- **`AGENTS.md` and the loader.** Astra gave them "At startup". The Claude
  member gave them no badge, because neither is a category entrypoint. They
  now have no badge, and their notes say "Read at startup" and "Read next, in
  full", so no third label is needed.
- **Package names.** Astra kept "from Planning". The Claude member dropped
  them, and the maintainer suggested dropping the Extension markers. The
  cards say "Extensions add …", and the Extension pages name the packages.
- **The CLI sentence.** Astra put the full tag list in prose below the
  figure. The Claude member put one sentence about the loading tags in the
  caption. The caption says the loader and the Memory entrypoint use the same
  tags, and that `open-forge context` lists what they select. The full,
  verified list is a new "Tags and the CLI" section on the Loading and tags
  page. Saying that every tag has CLI meaning would be wrong.

**Found while checking the render:** the full refresh-point list made the
Emerging card tall, which left the Core cards half empty. The list moved to
the caption, and the Emerging card keeps a short note.

**Docs aligned:** Loading and tags (two terms, a "When it's read" column, and
the new CLI section), Core categories, Memory, Your first task (the tree), and
the glossary, which drops "Loaded at startup" and "Entrypoint at startup,
re-read" and adds "Entrypoint on demand".

## Pitch round with Astra on 2026-09-28

The maintainer asked for the diagram to work as a 30-second pitch for someone
who has never seen Open Forge: "you need to explain this to me" answered at a
glance. They also asked for it to show that the files an entrypoint lists open
on demand, on every card, for `AGENTS.md or CLAUDE.md` read automatically, and
for a "wow" that clarifies. They asked the lead to come to terms with Astra.

**Round 1.** Astra and the lead each wrote a design without seeing the other.
Both split every card into the folder's index above and what it links to
below. Both explained "entrypoint" in plain words before the first card and
used the fresh-install figure. Astra's accuracy point changed the wording: an
entry line is read with its index, and only the file it points to waits. So
the lower row says "Linked files", not "Entries on demand".

**Round 2.** Astra accepted dashed outlines for on demand, the figure under
the pitch, the task strip repeating the two styles, and a badge-only Memory
heading. It improved three points for accuracy. The pitch must not hide
tagged loading. The row reads "Linked files: on demand unless tagged". The
footer keeps the rule that tags act only through a parent that was read.

**Round 3.** Astra checked the final text against the files and sent five
corrections, all applied. The important one: "Neither tag opens an index that
wasn't read" was wrong, because the loader's `#LoadNow` tags are what open the
unread category indexes. The rule is that both tags act only through a parent
index that has been read. The alt text also no longer calls Archived a main
folder.

**The result:**

- A pitch line on top: "At startup your agent reads its instructions, the
  index of most main folders, and whatever those indexes mark to load.
  Everything else opens only when a task needs it." Under it, the
  fresh-install figure.
- Two states everywhere: tinted with a solid outline for read at startup,
  unfilled with a dashed outline for on demand. They apply to the top nodes,
  every card's index, every card's linked-files row, and the first two task
  steps.
- Directives shows "Root rules: at startup" tinted and "Scoped rules: when
  their scope is selected" dashed.
- The entrypoint explanation sits inside the fork, between the loader and the
  panels.
