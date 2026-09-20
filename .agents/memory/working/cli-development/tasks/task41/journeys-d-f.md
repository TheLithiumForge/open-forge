---
open-forge:
  description: Beta journey scenarios D to F covering links and repair, Libraries and propagation, and everyday health commands
  tags: [Memory, Working, CLI, Task, Subtask, Scenario, Beta, Contextual, Active]
---

# Journeys D–F — links, Libraries, everyday health

The same rules as [A–C](journeys-a-c.md): perform what a person does, record the
literal output, and judge whether a first-time reader would understand it.

## D — Links break, and getting them back

- **D1. Link two authored files.** `open-forge references` on the target shows
  the incoming link.
- **D2. Rename the target by hand.** The link is now broken. Does anything tell
  the user, or do they have to already suspect it?
- **D3. Ask what is wrong.** `open-forge doctor` names the broken link with its
  file, line and column, and says what to do.
- **D4. Repair it.** `open-forge repair` fixes the link. The output shows the old
  and the new target so the user can see what changed.
- **D5. Move a file with the tool instead.** `open-forge route move` rewrites the
  incoming links itself. Compare the experience with D2 — moving properly should
  obviously be the better path.
- **D6. Break a heading link.** Rename a heading a link points at. The report
  distinguishes a missing file from a missing heading.
- **D7. Create a link that cannot be rewritten safely.** The refusal must be
  explicit; nothing may be silently half-rewritten.
- **D8. Repair with several problems at once.** Are they ordered sensibly, and is
  the summary honest about what it did and did not fix?
- **D9. Repair when there is nothing to repair.** One line, no ceremony.

## E — Libraries and propagation

The maintainer called this out specifically: propagation must work in both
directions, and deletion must be asymmetric.

- **E1. Attach a source folder.** `open-forge library attach`. Links are created
  and the output names what was linked.
- **E2. Edit a file in the source.** The change must be visible through the link.
  Confirm whether this needs a `sync` or is immediate, and whether the user could
  work that out from the output.
- **E3. Delete a file from the source, then sync.** The link must go too. The
  output must say the link was removed and why.
- **E4. Delete a linked file from the destination, then sync.** **The source must
  not be touched.** Confirm the source file still exists, and that sync either
  restores the link or reports the gap. This asymmetry is the point of the
  scenario: destination is a projection, source is the truth.
- **E5. Edit a linked file in the destination.** What happens on the next sync?
  Is the user's edit protected, reported, or lost? Whatever it is, the output
  must say so.
- **E6. Add a new file to the source, then sync.** It appears as a new link.
- **E7. Attach a source that collides with an authored file.** Refused, names the
  file.
- **E8. Detach.** Links go, the source is untouched, and the output says both.
- **E9. Ask about Libraries.** `library list` and `library inspect` after each of
  the above tell a consistent story.

## F — Everyday health and reading the workspace

- **F1. Status on a workspace with real drift.** Stale `Entries`, a changed
  managed file and a missing link at once. Is the report readable, and does it
  lead somewhere?
- **F2. Doctor with several unrelated problems.** Ordered by what matters, not by
  internal domain. Every finding names its subject.
- **F3. Find by tag and by heading.** `open-forge find`. Useful for an agent
  choosing what to read?
- **F4. Read the context at each detail level.** `minimal`, `standard`, `full`.
  Record token cost for each. `minimal` should be genuinely minimal.
- **F5. Clean up recovery bundles.** `open-forge cleanup` after a failed run.
- **F6. Run the common commands in JSON.** `--format json` on status, doctor,
  context, find. Valid schema-3, and scriptable without parsing prose.
- **F7. Check exit codes.** Healthy, warnings, blocked and failed each return the
  documented code.
