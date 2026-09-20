# Stage 5 journeys in plain language

The maintainer authorized starting stage 5 after this explanation. These are expected user outcomes, not claims that the current CLI passes them. [The roster](stage5-journey-roster.json) binds the exact flow sources and main/alternative scenario references. Deferred cases remain deferred.

**F01 — First installation.** Check an empty workspace, preview installation, install, read startup guidance and check health. Existing files survive; repeating installation changes nothing.

**F02 — Add a Skill.** Copy in a Skill folder, index it, find it and read it. Its native format and supporting files remain intact.

**F03 — Write a plain note.** Write Markdown without metadata, index it and read it. The note remains usable and unchanged, with a helpful warning.

**F04 — Copy a Template.** Create and edit a document from a Template, then change the Template. The copied document remains independent.

**F05 — Create nested guidance.** Create a deeply nested note, update its tags and read it. Unambiguous missing parent scopes are created; repeated updates do nothing.

**F06 — Choose the workspace.** Work from different folders and explicitly select a workspace. Results and effects belong only to the selected workspace.

**F07 — Find useful guidance.** Browse, filter by tags or scope, then read a result. Matches and incomplete search coverage are reported accurately.

**F08 — Repair a manual rename.** Rename a file outside Open Forge, find its broken link, choose the replacement and repair it. Only the chosen destination changes.

**F09 — Move guidance.** Preview and apply a move, then follow the links. Navigation and incoming/outgoing links keep working.

**F10 — Remove guidance.** Preview and remove a source, then repeat. Surrounding link text survives and the repeat is harmless.

**F11 — Package lifecycle.** Install a package and dependencies, update it, then remove it. Required dependencies and shared files remain until deliberately removable.

**F12 — Make a custom package.** Create a package, add content, inspect it and install into another workspace. The exact selected source is used and remains unchanged.

**F13 — Handle an occupied destination.** Encounter an existing file, preview replacement and explicitly authorize it. Unrelated content and later user edits remain protected.

**F14 — Approve external writes.** Approve a precise destination once or persistently, then revoke approval. Scope and persistence are respected and reported.

**F15 — Work around unrelated damage.** Perform a valid operation beside malformed unrelated content. Independent safe work continues; missing necessary facts still stop unsafe work.

**F16 — Use a Library.** Attach shared guidance, change source membership, sync and detach. Links follow membership while source files survive.

**F17 — Repair Library destinations.** Delete a link and sync; replace a link with a user file and sync again. Missing links are restored, user files preserved and independent safe work continues.

**F18 — Detach a missing source.** Lose the source folder, inspect, try sync and detach. No source retirements are invented; known destination links can be removed safely.

**F19 — Understand partial completion.** Fail after an effect, inspect actual state and choose a supported continuation. Completed work is reported honestly and recovery evidence retained.

**F20 — Concurrent commands.** Hold one workspace lock, try another write, use another workspace, then retry after release. Lock ownership and retry guidance are accurate.

**F21 — Read different output views.** Compare concise, detailed and JSON results. Facts agree, and useful warnings appear without extra verbosity.

**F22 — Create first, enrich later.** Create a named note without optional metadata, then enrich it. Initial creation works and enrichment updates the same file.

**F23 — Clean recovery leftovers.** Preview cleanup, remove eligible leftovers and repeat. Ambiguous evidence and ordinary workspace content remain untouched.

**F24 — Preserve authored content.** Read content, change metadata and index. Unicode, code samples, line endings and text outside intended edits survive.

**F25 — Customize with an overwrite.** Add an overwrite beside guidance and read it. Context supplies base then companion without a duplicate navigation entry.

**F26 — Install only wanted extensions.** Install Collaboration, then Planning; remove Collaboration. Only selected packages/dependencies are added; Planning and user content remain.
