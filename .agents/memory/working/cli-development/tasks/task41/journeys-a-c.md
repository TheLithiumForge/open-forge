---
open-forge:
  description: Beta journey scenarios A to C covering first install, extensions, and authoring and indexing, each written as something a user does rather than a command to run
  tags: [Memory, Working, CLI, Task, Subtask, Scenario, Beta, Contextual, Active]
---

# Journeys A–C — install, extensions, authoring

Each scenario is **something a person does**, not a command to run. The runner
performs it in a scratch workspace and answers the questions. A scenario passes
only when a reader who has never seen Open Forge would have known what happened
and what to do next.

Record for every scenario: the exact commands, the full output, the exit code,
and the workspace state afterwards. **A scenario that "worked" but read badly is
a failure.** Note token cost for anything a user runs often.

## A — Arriving for the first time

- **A1. Install into an empty folder.** Run `open-forge install`. Does the output
  say what was placed, where, and what to do next? Is a first-time reader told
  what Open Forge now *is* in this folder?
- **A2. Install where `.agents/` already exists and was authored by hand.** Must
  not clobber authored files. Is the refusal or merge understandable?
- **A3. Install twice.** The second run should be a clean no-op, not a
  re-application. Does it say so in one line?
- **A4. Ask what state the workspace is in.** `open-forge status` straight after
  install. Should read healthy, short, and boring.
- **A5. Read the startup context.** `open-forge context`. Is it the content an
  agent needs, and is it token-sane? Record the token count.
- **A6. Check for problems on a clean install.** `open-forge doctor` must find
  nothing and say so briefly. **Run this only in a scratch workspace.**
- **A7. Ask for help.** `open-forge --help`, then one command's `--help`. Can a
  reader pick their next command from it?

## B — Adding and removing Extensions

- **B1. See what is available.** `open-forge extension list`. Can a reader tell
  what each Extension is for, and which are installed?
- **B2. Install one.** Files appear, ownership is recorded, output names what
  arrived.
- **B3. Install one that depends on another.** The dependency is pulled in and
  the output says so, rather than silently doing it.
- **B4. Install onto a file the user wrote.** Must refuse, name the file, and
  say how to proceed.
- **B5. Update after the source moved on.** `open-forge extension update`.
  Replaced, kept and retired files are each visible.
- **B6. Edit an installed file, then update.** The edit must not be destroyed
  silently. The report must name the file.
- **B7. Remove one.** Files go, ownership is cleaned, an orphaned dependency is
  reported rather than left silently behind.
- **B8. Remove one that another still needs.** Blocked, names the dependent, and
  gives the command that would work.
- **B9. Remove, then check the workspace.** `doctor` and `status` must agree that
  nothing is left over.

## C — Authoring and indexing

This is the journey most likely to be a beta embarrassment, because it is what a
user does **without reading any documentation**.

- **C1. Drop a Markdown file into a routed folder by hand, then index.**
  `open-forge index` must pick it up and add its `Entries` row.
- **C2. The same file with no frontmatter at all.** Indexing must still work.
  The generated metadata should be present but deliberately thin, so the user or
  their agent can complete it. Confirm what the file looks like afterwards, and
  that nothing was invented on the user's behalf.
- **C3. The same file with partial frontmatter.** Only the missing parts are
  filled; anything the user wrote is left alone.
- **C4. Add a skill.** It routes, indexes and appears where a reader expects.
- **C5. Create a new folder with an entrypoint.** It becomes routable and its
  parent lists it.
- **C6. Create a folder with no entrypoint.** The diagnosis must name the folder
  and say what is missing, not just report a broken route.
- **C7. Nest a route several levels deep.** Indexing and `route list` stay
  correct and readable at depth.
- **C8. Inspect what was created.** `open-forge route inspect` on the new file.
- **C9. Index a second time with nothing changed.** Must be a no-op and say so.
- **C10. Rename a file by hand, then index.** Does the workspace end up
  consistent, or does the user need to be told something?
