---
open-forge:
  description: Flow summaries and every main-path step mapped to existing scenario evidence
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# Coverage Of All 26 User Flows

Return to the [review](./_cli-experience-coverage.md). **No existing test establishes a complete reviewed flow.** This is a sequence-and-assertion assessment, not a claim that every ingredient is absent. A chain must carry real output state forward and assert the reviewed goal; tests that reset unrelated fixtures cannot be concatenated into coverage.

The step tables map actions to their scenario evidence. Their ratings belong to the **whole linked scenario**, not to the individual step: a shared case can include branches and commands absent from that step. The flow summary and qualification explain which evidence actually applies.

| Flow | Useful existing coverage | What is missing |
| --- | --- | --- |
| [F01: Install and confirm the workspace is ready](#f01) | Install/repeat, separate installed Doctor and context checks. | No carried status → preview → apply → independent inventory → status → context → Doctor → repeat. Existing-host and occupied-force branches are not established. |
| [F02: Paste a native Skill and use it](#f02) | Generic index/list/inspect/context checks; a native Skill exists in fixture data. | No whole native Skill package journey or support-file/asset preservation assertions. Fixture presence is not evidence. |
| [F03: Write a plain note and let indexing help](#f03) | Ordinary fully described content can be indexed; metadata projection has lower-tier checks. | No plain/partial-metadata note → warning-only useful index → immediate retrieval → repeat with identical authored bytes. |
| [F04: Copy a Template and keep the new document independent](#f04) | Generic exact route creation and metadata update command checks. | No template copy with independent destination metadata, later template edit and unchanged copy carried through one sequence. |
| [F05: Create and refine a nested scope](#f05) | Explicit route-init preview/apply/repeat and create/update ingredients. | No create-first nested path with deterministic missing parents, followed by inspect/context on its actual state. |
| [F06: Work in the intended workspace](#f06) | All bindings preserve an explicitly missing workspace; version is workspace-independent. | No empty child versus installed ancestor and malformed cwd B versus explicit healthy A journey. |
| [F07: Find the relevant guidance without loading everything](#f07) | Separate Find tag/heading/content, Route List and Context evidence. | No selection returned by Find carried into inspection/context, nor the complete scope/tag intersection and decoy alternatives. |
| [F08: Repair a link after a manual rename](#f08) | Explicit guarded relink → repeat no-op checks exact body; separate reference diagnostics. | No manual rename → references → Doctor → repair → reference verification on carried state. |
| [F09: Move a route and preserve links in one operation](#f09) | Route Move checks a leaf’s bytes and identity; separate reference/context tests. | No complete move preserving the reviewed authored incoming links, then reading the new identity and checking actual references. |
| [F10: Remove a route without deleting authored meaning](#f10) | Leaf/category removal preserves bounded fixture state; category repeat is tested. | No full reviewed removal → Doctor → harmless absent repeat journey with authored prose and all relevant links verified. |
| [F11: Install, update and remove a package with dependencies](#f11) | Install, source-edited update/repeat and removal/repeat have genuine shorter sequences. | No whole dependency lifecycle with shared ownership, local edits, support files and unrelated content checked together. |
| [F12: Create a custom package, then install from that source](#f12) | Custom extension creation and separate explicit-source installation exist. | No create → inspect generated package → edit real source → install from it → retrieve the installed result sequence. |
| [F13: Handle an occupied target without accidental overwrite](#f13) | Occupied destinations are rejected in selected mutation tests; user content is preserved. | No reviewed collision → inspect → explicit eligible replacement preview/application → later preservation sequence. |
| [F14: Approve an external destination once or persistently](#f14) | Library grant theory carries actual attach into sync/detach; flag persistence and preview are asserted. | No Extension missing-consent → real once/always choice → persisted/revoked scope journey. |
| [F15: Continue past unrelated corruption without ignoring safety](#f15) | Legacy inert malformed files survive installation. | No safe independent action continuing past unrelated malformed routed content, with a genuinely required unavailable input limiting only dependent work. |
| [F16: Attach a Library and synchronize changing source membership](#f16) | Library mixed addition/retirement preview → apply and grant attach → sync/detach subsequences. | No attach → inspect → source additions/edits/removals → inspect/sync → repeat → detach lifecycle. |
| [F17: Recover a missing link and protect a changed destination](#f17) | Missing projection is observed; changed occupants are preserved by whole-operation rejection. | No missing-link restoration chain. Existing sync/detach tests explicitly contradict independent continuation around a changed destination. |
| [F18: Detach after the source folder disappears](#f18) | Detach handles a dangling member while another source remains. | No disappearance of the source root → incomplete list/inspect/sync without guessed retirement → detach, including already absent destinations. |
| [F19: Inspect a partial operation and choose a safe continuation](#f19) | Runner cancellation and lower-tier operation fault tests supply adjacent evidence. | No published after-effect fault → truthful receipt → Doctor → explicit continuation → cleanup preview journey. |
| [F20: Run concurrent work without misleading lock advice](#f20) | Integration verifies real exclusive handle, contention, release and reuse; Cleanup E2E bypasses lock for no-op. | No two live CLI writers, independent workspace, accurate lock advice and successful retry after release. |
| [F21: Read more detail without changing what happened](#f21) | Unavailable-workspace schema/view matrix, healthy Route List full/debug equality, incomplete Status detail fragments. | No full success/incomplete default=minimal and filter comparison with useful, nonduplicated human reports and effect invariance. |
| [F22: Correct an incomplete command in one attempt](#f22) | Help boundaries and explicit metadata create/update cases. | No successful create without optional description/tags followed by enrichment of that same file; current invalid-metadata checks need reconciliation. |
| [F23: Review and remove recovery leftovers](#f23) | Cleanup apply → repeat; separate preview preserves state and foreign/unknown candidates. | No same-workspace damaged-plus-valid cleanup continuation or complete reviewed count/state/report journey. |
| [F24: Keep authored content intact across reading and maintenance](#f24) | Exact simple Find body, bounded simple Index/Repair edits and some repeat checks. | No literal Unicode, whitespace, fences and vocabulary-rich content carried across reads and maintenance with later authored lists preserved. |
| [F25: Customize through an overwrite](#f25) | Context exact base-before-overwrite path order and Route Inspect layer facts. | No author companion → index → list one source → literal context → repeat while preserving both authored files. |
| [F26: Select only needed extensions](#f26) | Embedded catalogue lists source-derived IDs; generic package operations and installed-package Doctor checks. | No specific fresh Core → Collaboration only → Planning plus Workflow Support → template reads → remove Collaboration → Doctor, with exact absent unrelated packages. |

## F01

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f01-install-and-confirm-the-workspace-is-ready.md). **Complete flow: not covered.** Install/repeat, separate installed Doctor and context checks. No carried status → preview → apply → independent inventory → status → context → Doctor → repeat. Existing-host and occupied-force branches are not established.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge status | [C01-01](core-scenarios.md#c01-01): **none** |
| 2 | open-forge install --dry-run | [C03-02](core-scenarios.md#c03-02): **partial** |
| 3 | open-forge install --automatic | [C03-01](core-scenarios.md#c03-01): **partial** |
| 4 | Compare the full created-file and created-directory sets with the install report. | [X08](cross-command-scenarios.md#x08): **partial** |
| 5 | open-forge status | [C01-02](core-scenarios.md#c01-02): **partial** |
| 6 | open-forge context | [C08-01](core-scenarios.md#c08-01): **partial** |
| 7 | open-forge doctor | [C02-01](core-scenarios.md#c02-01): **partial** |
| 8 | open-forge install --automatic | [C03-03](core-scenarios.md#c03-03): **partial** |

**Alternatives:** [C03-04](core-scenarios.md#c03-04) (none), [C03-05](core-scenarios.md#c03-05) (none), [C03-06](core-scenarios.md#c03-06) (none), [C03-08](core-scenarios.md#c03-08) (direct). See the linked scenario gaps; a covered alternative does not cover the main path.

## F02

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f02-paste-a-native-skill-and-use-it.md). **Complete flow: not covered.** Generic index/list/inspect/context checks; a native Skill exists in fixture data. No whole native Skill package journey or support-file/asset preservation assertions. Fixture presence is not evidence.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Copy docs/cli-experience-fixtures/content/native-skill/team-notes to .agents/skills/team-notes. | [X01](cross-command-scenarios.md#x01): **none** |
| 2 | open-forge index | [C05-02](core-scenarios.md#c05-02): **partial** |
| 3 | open-forge route list skills --depth=all | [C11-02](core-scenarios.md#c11-02): **adjacent** |
| 4 | open-forge route inspect .agents/skills/team-notes/SKILL.md | [C12-02](core-scenarios.md#c12-02): **adjacent** |
| 5 | open-forge context .agents/skills/team-notes/SKILL.md | [C08-02](core-scenarios.md#c08-02): **partial** |
| 6 | open-forge index | [C05-01](core-scenarios.md#c05-01): **partial** |

**Alternatives:** [C05-07](core-scenarios.md#c05-07) (none), [X01](cross-command-scenarios.md#x01) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F03

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f03-write-a-plain-note-and-let-indexing-help.md). **Complete flow: not covered.** Ordinary fully described content can be indexed; metadata projection has lower-tier checks. No plain/partial-metadata note → warning-only useful index → immediate retrieval → repeat with identical authored bytes.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Copy docs/cli-experience-fixtures/content/plain-note.md to .agents/guidance/my-note.md. | [X02](cross-command-scenarios.md#x02): **none** |
| 2 | open-forge index | [X02](cross-command-scenarios.md#x02): **none** |
| 3 | open-forge doctor | [X05](cross-command-scenarios.md#x05): **adjacent** |
| 4 | Optionally add a description and tags chosen by the author. | [X02](cross-command-scenarios.md#x02): **none** |
| 5 | open-forge index | [C05-02](core-scenarios.md#c05-02): **partial** |
| 6 | open-forge context guidance/my-note | [C08-02](core-scenarios.md#c08-02): **partial** |

**Alternatives:** [X03](cross-command-scenarios.md#x03) (none), [X04](cross-command-scenarios.md#x04) (none), [X05](cross-command-scenarios.md#x05) (adjacent). See the linked scenario gaps; a covered alternative does not cover the main path.

## F04

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f04-copy-a-template-and-keep-the-new-document-independent.md). **Complete flow: not covered.** Generic exact route creation and metadata update command checks. No template copy with independent destination metadata, later template edit and unchanged copy carried through one sequence.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge route create memory/working/new-note --description "Scenario for adding team notes" --tag=Scenario --template templates/example | [C14-02](authoring-scenarios.md#c14-02): **none** |
| 2 | Fill the new document placeholders and remove irrelevant sections. | [X10](cross-command-scenarios.md#x10): **adjacent** |
| 3 | Edit only templates/example.md. | [X10](cross-command-scenarios.md#x10): **adjacent** |
| 4 | open-forge route update memory/working/new-note --description "Accepted review scenario" --template templates/example | [C15-05](authoring-scenarios.md#c15-05): **none** |

**Alternatives:** [C14-11](authoring-scenarios.md#c14-11) (none), [C14-10](authoring-scenarios.md#c14-10) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F05

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f05-create-and-refine-a-nested-scope.md). **Complete flow: not covered.** Explicit route-init preview/apply/repeat and create/update ingredients. No create-first nested path with deterministic missing parents, followed by inspect/context on its actual state.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge route create guidance/team/new-topic/notes --description "Team topic notes" --tag=Guidance | [C14-09](authoring-scenarios.md#c14-09): **none** |
| 2 | open-forge route update guidance/team/new-topic/notes --tag=Guidance --tag=Team | [C15-02](authoring-scenarios.md#c15-02): **partial** |
| 3 | open-forge route inspect guidance/team/new-topic/notes | [C12-02](core-scenarios.md#c12-02): **adjacent** |
| 4 | open-forge context guidance/team/new-topic/notes | [C08-02](core-scenarios.md#c08-02): **partial** |
| 5 | open-forge route update guidance/team/new-topic/notes --tag=Guidance --tag=Team | [C15-06](authoring-scenarios.md#c15-06): **direct** |

**Alternatives:** [C14-09](authoring-scenarios.md#c14-09) (none), [C13-01](core-scenarios.md#c13-01) (partial), [C13-08](core-scenarios.md#c13-08) (direct). See the linked scenario gaps; a covered alternative does not cover the main path.

## F06

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f06-work-in-the-intended-workspace.md). **Complete flow: not covered.** All bindings preserve an explicitly missing workspace; version is workspace-independent. No empty child versus installed ancestor and malformed cwd B versus explicit healthy A journey.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | From the uninstalled subdirectory, run open-forge status. | [X11](cross-command-scenarios.md#x11): **partial** |
| 2 | From B, run open-forge status --workspace "$WS_A". | [X11](cross-command-scenarios.md#x11): **partial** |
| 3 | open-forge index --workspace "$WS_A" --dry-run | [X11](cross-command-scenarios.md#x11): **partial** |
| 4 | open-forge status --workspace "$MISSING_WS" | [C01-11](core-scenarios.md#c01-11): **partial** |

**Alternatives:** [C05-07](core-scenarios.md#c05-07) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F07

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f07-find-the-relevant-guidance-without-loading-everything.md). **Complete flow: not covered.** Separate Find tag/heading/content, Route List and Context evidence. No selection returned by Find carried into inspection/context, nor the complete scope/tag intersection and decoy alternatives.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge route list --depth=all | [C11-03](core-scenarios.md#c11-03): **adjacent** |
| 2 | open-forge find --tag=Decision --tag=Architecture | [C09-03](core-scenarios.md#c09-03): **adjacent** |
| 3 | open-forge find --tag=Decision --content=headings | [C09-06](core-scenarios.md#c09-06): **adjacent** |
| 4 | open-forge find --tag=Decision --include=guidance | [C09-07](core-scenarios.md#c09-07): **none** |
| 5 | Run open-forge context with one exact source returned by find. | [C08-02](core-scenarios.md#c08-02): **partial** |

**Alternatives:** [C09-05](core-scenarios.md#c09-05) (none), [C09-09](core-scenarios.md#c09-09) (none), [X28](cross-command-scenarios.md#x28) (partial). See the linked scenario gaps; a covered alternative does not cover the main path.

## F08

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f08-repair-a-link-after-a-manual-rename.md). **Complete flow: not covered.** Explicit guarded relink → repeat no-op checks exact body; separate reference diagnostics. No manual rename → references → Doctor → repair → reference verification on carried state.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge references guidance/target | [C10-01](core-scenarios.md#c10-01): **partial** |
| 2 | Rename only target.md to renamed-target.md using the file manager; then run open-forge references guidance/notes --direction=out. | [C10-05](core-scenarios.md#c10-05): **partial** |
| 3 | open-forge doctor | [C02-03](core-scenarios.md#c02-03): **adjacent** |
| 4 | Bind LOCATION, EXPECTED and TARGET from the actual occurrence and the user’s chosen renamed-target.md; preview and apply the exact --relink request. | [C06-05](core-scenarios.md#c06-05): **partial** |
| 5 | open-forge references guidance/notes --direction=out | [C10-03](core-scenarios.md#c10-03): **partial** |
| 6 | open-forge repair --automatic | [C06-01](core-scenarios.md#c06-01): **partial** |

**Alternatives:** [C06-03](core-scenarios.md#c06-03) (partial), [X16](cross-command-scenarios.md#x16) (partial). See the linked scenario gaps; a covered alternative does not cover the main path.

## F09

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f09-move-a-route-and-preserve-links-in-one-operation.md). **Complete flow: not covered.** Route Move checks a leaf’s bytes and identity; separate reference/context tests. No complete move preserving the reviewed authored incoming links, then reading the new identity and checking actual references.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge route move guidance/notes guidance/team/moved-note --dry-run | [C16-04](authoring-scenarios.md#c16-04): **partial** |
| 2 | open-forge route move guidance/notes guidance/team/moved-note | [C16-02](authoring-scenarios.md#c16-02): **partial** |
| 3 | open-forge references guidance/team/moved-note | [C10-01](core-scenarios.md#c10-01): **partial** |
| 4 | open-forge context guidance/team/moved-note | [C08-02](core-scenarios.md#c08-02): **partial** |

**Alternatives:** [C16-05](authoring-scenarios.md#c16-05) (partial), [C16-03](authoring-scenarios.md#c16-03) (none), [C16-11](authoring-scenarios.md#c16-11) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F10

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f10-remove-a-route-without-deleting-authored-meaning.md). **Complete flow: not covered.** Leaf/category removal preserves bounded fixture state; category repeat is tested. No full reviewed removal → Doctor → harmless absent repeat journey with authored prose and all relevant links verified.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge route remove guidance/notes --dry-run | [C17-04](authoring-scenarios.md#c17-04): **partial** |
| 2 | open-forge route remove guidance/notes --automatic | [C17-02](authoring-scenarios.md#c17-02): **direct** |
| 3 | open-forge doctor | [C02-01](core-scenarios.md#c02-01): **partial** |
| 4 | open-forge route remove guidance/notes --automatic | [C17-05](authoring-scenarios.md#c17-05): **opposite** |

**Alternatives:** [C17-07](authoring-scenarios.md#c17-07) (none), [C17-06](authoring-scenarios.md#c17-06) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F11

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f11-install-update-and-remove-a-package-with-dependencies.md). **Complete flow: not covered.** Install, source-edited update/repeat and removal/repeat have genuine shorter sequences. No whole dependency lifecycle with shared ownership, local edits, support files and unrelated content checked together.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge extension list --source "$CAT" | [C18-05](authoring-scenarios.md#c18-05): **partial** |
| 2 | open-forge extension inspect toolkit --source "$CAT" | [C19-03](authoring-scenarios.md#c19-03): **none** |
| 3 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-02](authoring-scenarios.md#c21-02): **none** |
| 4 | open-forge extension remove base --automatic | [C23-05](authoring-scenarios.md#c23-05): **none** |
| 5 | Replace CAT with the pinned next-version source, then preview and apply toolkit update using the same explicit --source. | [C22-03](authoring-scenarios.md#c22-03): **none** |
| 6 | open-forge extension remove toolkit --automatic | [C23-04](authoring-scenarios.md#c23-04): **none** |
| 7 | open-forge extension remove base --automatic | [C23-01](authoring-scenarios.md#c23-01): **partial** |

**Alternatives:** [C22-04](authoring-scenarios.md#c22-04) (none), [C22-05](authoring-scenarios.md#c22-05) (none), [C23-02](authoring-scenarios.md#c23-02) (none), [X30](cross-command-scenarios.md#x30) (partial). See the linked scenario gaps; a covered alternative does not cover the main path.

## F12

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f12-create-a-custom-package-then-install-from-that-source.md). **Complete flow: not covered.** Custom extension creation and separate explicit-source installation exist. No create → inspect generated package → edit real source → install from it → retrieve the installed result sequence.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge extension create toolkit --path "$CAT" --name "Team toolkit" --description "Shared team guidance" --package-version=preview --automatic | [C20-02](authoring-scenarios.md#c20-02): **partial** |
| 2 | Author valid ordinary content under CAT/toolkit/content/.agents/guidance and keep its route requirements explicit. | [X27](cross-command-scenarios.md#x27): **partial** |
| 3 | open-forge extension list --source "$CAT" | [C18-05](authoring-scenarios.md#c18-05): **partial** |
| 4 | open-forge extension inspect toolkit --source "$CAT" | [C19-03](authoring-scenarios.md#c19-03): **none** |
| 5 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-01](authoring-scenarios.md#c21-01): **partial** |

**Alternatives:** [C21-13](authoring-scenarios.md#c21-13) (none), [C20-10](authoring-scenarios.md#c20-10) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F13

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f13-handle-an-occupied-target-without-accidental-overwrite.md). **Complete flow: not covered.** Occupied destinations are rejected in selected mutation tests; user content is preserved. No reviewed collision → inspect → explicit eligible replacement preview/application → later preservation sequence.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-09](authoring-scenarios.md#c21-09): **none** |
| 2 | open-forge extension install toolkit --source "$CAT" --force --dry-run | [C21-14](authoring-scenarios.md#c21-14): **none** |
| 3 | After reviewing and authorizing replacement, run open-forge extension install toolkit --source "$CAT" --force --automatic. | [C21-10](authoring-scenarios.md#c21-10): **none** |
| 4 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-11](authoring-scenarios.md#c21-11): **partial** |
| 5 | Edit a selected installed file, then repeat install. | [C21-12](authoring-scenarios.md#c21-12): **none** |

**Alternatives:** [X27](cross-command-scenarios.md#x27) (partial), [C21-18](authoring-scenarios.md#c21-18) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F14

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f14-approve-an-external-destination-once-or-persistently.md). **Complete flow: not covered.** Library grant theory carries actual attach into sync/detach; flag persistence and preview are asserted. No Extension missing-consent → real once/always choice → persisted/revoked scope journey.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge extension install toolkit --source "$CAT" --automatic | [C21-05](authoring-scenarios.md#c21-05): **none** |
| 2 | On a suitable fresh branch of the same failed state, use a real terminal and choose once. | [C21-07](authoring-scenarios.md#c21-07): **none** |
| 3 | On a separate branch, choose always for the displayed scope, or supply --allow-path docs/team.md for this exact file, or docs for the deliberately selected directory. | [X14](cross-command-scenarios.md#x14): **partial** |
| 4 | Revoke the applicable grant, introduce a real update, then run the update automatically. | [C22-09](authoring-scenarios.md#c22-09): **none** |

**Alternatives:** [C21-14](authoring-scenarios.md#c21-14) (none), [X26](cross-command-scenarios.md#x26) (adjacent), [X14](cross-command-scenarios.md#x14) (partial). See the linked scenario gaps; a covered alternative does not cover the main path.

## F15

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f15-continue-past-unrelated-corruption-without-ignoring-safety.md). **Complete flow: not covered.** Legacy inert malformed files survive installation. No safe independent action continuing past unrelated malformed routed content, with a genuinely required unavailable input limiting only dependent work.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Run the otherwise valid explicit Extension install. | [X12](cross-command-scenarios.md#x12): **adjacent** |
| 2 | Fork with an absent, unreadable or malformed generated ownership lock instead. | [X13](cross-command-scenarios.md#x13): **partial** |
| 3 | Fork with a genuinely unreadable required package source or required recovery boundary. | [X12](cross-command-scenarios.md#x12): **adjacent** |

**Alternatives:** [X13](cross-command-scenarios.md#x13) (partial), [C26-12](library-scenarios.md#c26-12) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F16

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f16-attach-a-library-and-synchronize-changing-source-membership.md). **Complete flow: not covered.** Library mixed addition/retirement preview → apply and grant attach → sync/detach subsequences. No attach → inspect → source additions/edits/removals → inspect/sync → repeat → detach lifecycle.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge library attach team shared-guides --to .agents/guidance/team --automatic | [C26-01](library-scenarios.md#c26-01): **adjacent** |
| 2 | open-forge library inspect team | [C25-01](library-scenarios.md#c25-01): **partial** |
| 3 | Add one eligible file to shared-guides, then run open-forge library inspect team. | [C25-02](library-scenarios.md#c25-02): **partial** |
| 4 | open-forge library sync team --automatic | [C27-02](library-scenarios.md#c27-02): **partial** |
| 5 | Add one other source member and retire a different existing one, then sync again. | [C27-04](library-scenarios.md#c27-04): **partial** |
| 6 | open-forge library detach team --automatic | [C28-01](library-scenarios.md#c28-01): **partial** |

**Alternatives:** [C26-05](library-scenarios.md#c26-05) (none), [C27-09](library-scenarios.md#c27-09) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F17

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f17-recover-a-missing-link-and-protect-a-changed-destination.md). **Complete flow: not covered.** Missing projection is observed; changed occupants are preserved by whole-operation rejection. No missing-link restoration chain. Existing sync/detach tests explicitly contradict independent continuation around a changed destination.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Unlink one registered destination without touching its source, then list Libraries. | [C24-03](library-scenarios.md#c24-03): **partial** |
| 2 | open-forge library sync team --automatic | [C27-08](library-scenarios.md#c27-08): **adjacent** |
| 3 | Replace that link with an ordinary user file, then sync. | [C27-07](library-scenarios.md#c27-07): **opposite** |
| 4 | Deliberately move the user file to a backup outside the registered destination set, then sync again. | [X18](cross-command-scenarios.md#x18): **opposite** |
| 5 | open-forge library inspect team | [C25-01](library-scenarios.md#c25-01): **partial** |

**Step qualification:** X18’s opposite rating concerns continuing while a changed occupant remains. It does not rate the later backup-and-sync action as opposite; that recovery sequence is untested.

**Alternatives:** [C28-06](library-scenarios.md#c28-06) (opposite), [C28-05](library-scenarios.md#c28-05) (adjacent). See the linked scenario gaps; a covered alternative does not cover the main path.

## F18

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f18-detach-after-the-source-folder-disappears.md). **Complete flow: not covered.** Detach handles a dangling member while another source remains. No disappearance of the source root → incomplete list/inspect/sync without guessed retirement → detach, including already absent destinations.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Remove or rename the source root while keeping every destination symlink entry. | [X17](cross-command-scenarios.md#x17): **adjacent** |
| 2 | open-forge library list | [C24-06](library-scenarios.md#c24-06): **adjacent** |
| 3 | open-forge library inspect team | [C25-07](library-scenarios.md#c25-07): **none** |
| 4 | open-forge library sync team --automatic | [C27-09](library-scenarios.md#c27-09): **none** |
| 5 | open-forge library detach team --automatic | [X17](cross-command-scenarios.md#x17): **adjacent** |

**Alternatives:** [C28-05](library-scenarios.md#c28-05) (adjacent), [C28-08](library-scenarios.md#c28-08) (adjacent). See the linked scenario gaps; a covered alternative does not cover the main path.

## F19

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f19-inspect-a-partial-operation-and-choose-a-safe-continuation.md). **Complete flow: not covered.** Runner cancellation and lower-tier operation fault tests supply adjacent evidence. No published after-effect fault → truthful receipt → Doctor → explicit continuation → cleanup preview journey.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Trigger the chosen operation’s after-one-effect failure. | [X09](cross-command-scenarios.md#x09): **adjacent** |
| 2 | Compare actual completed effects, saved settings, ownership publication and retained recovery. | [X09](cross-command-scenarios.md#x09): **adjacent** |
| 3 | Run doctor on that same resulting state, interpreting its actual findings rather than forcing the base fixture’s expected count. | [C02-05](core-scenarios.md#c02-05): **none** |
| 4 | Only if current diagnosis proves an applicable safe-exact repair, preview it. Otherwise retain the evidence and use the explicit user-approved file recovery procedure for this fixture. | [X16](cross-command-scenarios.md#x16): **partial** |
| 5 | Once recovery is no longer needed and eligible leftovers are understood, run open-forge cleanup --dry-run. | [C07-03](core-scenarios.md#c07-03): **partial** |

**Alternatives:** [X09](cross-command-scenarios.md#x09) (adjacent), [X26](cross-command-scenarios.md#x26) (adjacent). See the linked scenario gaps; a covered alternative does not cover the main path.

## F20

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f20-run-concurrent-work-without-misleading-lock-advice.md). **Complete flow: not covered.** Integration verifies real exclusive handle, contention, release and reuse; Cleanup E2E bypasses lock for no-op. No two live CLI writers, independent workspace, accurate lock advice and successful retry after release.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Start one mutation in workspace A and hold its real lock at a known boundary. | [X21](cross-command-scenarios.md#x21): **adjacent** |
| 2 | Run index against A while the first operation holds the lock. | [C05-09](core-scenarios.md#c05-09): **none** |
| 3 | Run an otherwise valid mutation in workspace B. | [X21](cross-command-scenarios.md#x21): **adjacent** |
| 4 | Release the first operation; retry the second while the persistent lock file remains. | [X21](cross-command-scenarios.md#x21): **adjacent** |

**Alternatives:** [X21](cross-command-scenarios.md#x21) (adjacent), [X09](cross-command-scenarios.md#x09) (adjacent). See the linked scenario gaps; a covered alternative does not cover the main path.

## F21

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f21-read-more-detail-without-changing-what-happened.md). **Complete flow: not covered.** Unavailable-workspace schema/view matrix, healthy Route List full/debug equality, incomplete Status detail fragments. No full success/incomplete default=minimal and filter comparison with useful, nonduplicated human reports and effect invariance.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Capture default text and explicit minimal for one success and one incomplete result. | [X20](cross-command-scenarios.md#x20): **partial** |
| 2 | Capture standard, full and debug for those same cases. | [X20](cross-command-scenarios.md#x20): **partial** |
| 3 | Review these fresh captures for repeated explanations and missing useful facts. | [X25](cross-command-scenarios.md#x25): **adjacent** |
| 4 | Capture JSON and detail-filter variants, retaining stdout, stderr and exit separately. | [X20](cross-command-scenarios.md#x20): **partial** |

**Alternatives:** [C02-03](core-scenarios.md#c02-03) (adjacent), [X23](cross-command-scenarios.md#x23) (partial), [X24](cross-command-scenarios.md#x24) (partial). See the linked scenario gaps; a covered alternative does not cover the main path.

## F22

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f22-correct-an-incomplete-command-in-one-attempt.md). **Complete flow: not covered.** Help boundaries and explicit metadata create/update cases. No successful create without optional description/tags followed by enrichment of that same file; current invalid-metadata checks need reconciliation.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge route create guidance/new-note | [C14-07](authoring-scenarios.md#c14-07): **none** |
| 2 | Optionally choose a more useful description and tag. | [X15](cross-command-scenarios.md#x15): **opposite** |
| 3 | open-forge route update guidance/new-note --description "Team operating notes" --tag=Guidance | [C15-01](authoring-scenarios.md#c15-01): **partial** |
| 4 | open-forge route inspect guidance/new-note | [C12-02](core-scenarios.md#c12-02): **adjacent** |

**Alternatives:** [X24](cross-command-scenarios.md#x24) (partial), [C14-09](authoring-scenarios.md#c14-09) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F23

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f23-review-and-remove-recovery-leftovers.md). **Complete flow: not covered.** Cleanup apply → repeat; separate preview preserves state and foreign/unknown candidates. No same-workspace damaged-plus-valid cleanup continuation or complete reviewed count/state/report journey.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | open-forge cleanup --dry-run | [C07-03](core-scenarios.md#c07-03): **partial** |
| 2 | open-forge cleanup | [C07-02](core-scenarios.md#c07-02): **partial** |
| 3 | open-forge cleanup | [C07-01](core-scenarios.md#c07-01): **partial** |

**Alternatives:** [C07-04](core-scenarios.md#c07-04) (adjacent), [C07-06](core-scenarios.md#c07-06) (none), [C07-07](core-scenarios.md#c07-07) (none). See the linked scenario gaps; a covered alternative does not cover the main path.

## F24

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f24-keep-authored-content-intact-across-reading-and-maintenance.md). **Complete flow: not covered.** Exact simple Find body, bounded simple Index/Repair edits and some repeat checks. No literal Unicode, whitespace, fences and vocabulary-rich content carried across reads and maintenance with later authored lists preserved.

| Step | Reviewed action | Linked scenario’s overall rating |
| --- | --- | --- |
| 1 | Read the source through context with body and section projections. | [X23](cross-command-scenarios.md#x23): **partial** |
| 2 | Update only that source’s description. | [C15-01](authoring-scenarios.md#c15-01): **partial** |
| 3 | Index its parent containing surrounding prose and a later authored list. | [X07](cross-command-scenarios.md#x07): **partial** |
| 4 | Repeat the completed maintenance operation. | [X19](cross-command-scenarios.md#x19): **opposite** |

**Step qualification:** X19’s opposite rating concerns repeated Route Remove. Repeated Index and Update have positive no-op evidence; the complete literal-content maintenance sequence is still missing.

**Alternatives:** [X15](cross-command-scenarios.md#x15) (opposite), [X23](cross-command-scenarios.md#x23) (partial). See the linked scenario gaps; a covered alternative does not cover the main path.

## F25

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f25-customize-through-an-overwrite.md). **Complete flow: not covered.** Context exact base-before-overwrite path order and Route Inspect layer facts. No author companion → index → list one source → literal context → repeat while preserving both authored files.

1. Write `.agents/guidance/team-note.overwrite.md` with a distinctive local adjustment.
2. Run `open-forge index`, then list the guidance route.
3. Run `open-forge context guidance/team-note`.
4. Run index again and compare both authored files byte-for-byte.

Related case: [X06](cross-command-scenarios.md#x06) (ordered overwrite selection). No case supplies the missing authored-file/index/list/repeat chain.

## F26

[Reviewed flow](../../../crystallized/documents/cli/experience/flows/f26-select-only-needed-extensions.md). **Complete flow: not covered.** Embedded catalogue lists source-derived IDs; generic package operations and installed-package Doctor checks. No specific fresh Core → Collaboration only → Planning plus Workflow Support → template reads → remove Collaboration → Doctor, with exact absent unrelated packages.

1. Install `collaboration --automatic` through the Extension command and inspect its installed files.
2. Read `.agents/guidance/adaptive-collaboration.md` through context.
3. Install `planning --automatic` and list installed Extensions.
4. Read the User Flow and Scenario Templates through their explicit installed paths.
5. Remove Collaboration automatically and run Doctor.

Related cases: [X27](cross-command-scenarios.md#x27) and [X30](cross-command-scenarios.md#x30) (generic lifecycle evidence). These do not establish this exact package selection.
