---
open-forge:
  description: One worked scenario, filled in from a real defect, showing the template at the level of detail the work needs
  tags: [Memory, Working, CLI, Task, Subtask, Scenario, Example, Contextual, Active]
---

# Worked example

Historical worked example from the earlier Scenario template. The current [Scenario Collection template](../../../../../templates/planning/scenario-collection.md) replaces that starting shape.

Filled from a defect found on 2026-09-17, so the shape is shown on something
real rather than on an invented happy path. Expected output here is what the
command **should** print; what it prints today is recorded under **Open**.

### index.authored-file-without-metadata — a file written by hand

**Wants.** I wrote a Markdown file into a folder Open Forge manages, and I want
it to show up where that folder lists its contents.

**From.** A clean install. One file at `.agents/guidance/my-notes.md` with a
heading and a paragraph and no frontmatter at all.

**Does.**

    open-forge index

**Ends.** status `completed-with-warnings` · exit `2` · stream `stdout`

**Result.**

*minimal*

    Updated the Entries section in 1 of 20 files.
      .agents/guidance/_guidance.md  1 -> 2 entries
      Warning  .agents/guidance/my-notes.md  Description is missing
             .agents/guidance/my-notes.md has no description, so its entry reads from its file name.
    Next: open-forge route inspect .agents/guidance/my-notes.md

*standard* — as minimal.

*full* — adds the finding code and the resolved route for the new entry.

*debug* — adds the metadata actually read from the file.

*json* — `status` is `completed-with-warnings`; `effects` carries the rewritten
entrypoint; `findings` carries one warning whose `subject.path` is the authored
file; `counts.entriesSectionsStale` reaches zero.

**Why.** A user writing a file by hand is the normal way content arrives, not an
error. The tool knows the file's path and heading, which is enough to list it,
so it should list it and say what it could not determine. Refusing the whole
rebuild over one thin header makes the tool feel broken at the exact moment a
user is trying to use it.

What would make this wrong: inventing a description the user did not write,
silently leaving the file unlisted, or reporting the missing metadata as though
the file could not be read.

**Open.** **Today this does none of that.** `index` returns exit 3,
`The Entries sections could not be rebuilt completely. Nothing was changed.`,
and the file is never listed. One thin header blocks all 20 sections. The
warning says `The frontmatter of <path> could not be read completely.`, which is
untrue — there is no frontmatter to fail at reading, and the same sentence is
used when frontmatter is present and merely missing `tags`.

Two decisions sit behind this, and both are the maintainer's:

1. Should `index` continue past a file whose metadata is thin, listing what it
   can? The G4 catalogue currently specifies the opposite, so this is a change
   to accepted design rather than a bug fix.
2. Does "no metadata yet" get its own finding code, separate from "metadata
   could not be read"? They are different scenarios sharing one code today,
   which is why one of them is described by a false sentence.

**Notes.** The shape of the counts matters here. If `index` lists the file, the
entry text has to come from somewhere; reading it from the file name is a
decision worth stating rather than assuming.
