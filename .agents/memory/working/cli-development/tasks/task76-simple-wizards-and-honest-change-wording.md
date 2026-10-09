---
open-forge:
  description: Open Task 76 to replace the Install Custom loop with one marked list, give every wizard one clear visual language, and make plans say how existing files change instead of calling every edit a replacement
  tags: [Memory, Working, Task, CLI, Install, Prompts, Wizard, Wording, Contextual, Active]
---

# Task 76: Simple wizards and honest change wording

## Outcome

Every interactive wizard is quick to read and easy to answer. Install's Custom step becomes one list that shows every built-in route with its current choice, and the user changes a row with one key. Plans and results describe what actually happens to each existing file, so adding an entry, a section or a setting is never reported as replacing the file.

The maintainer reported both problems on 2026-10-09 after trying the beta9 Install flow. On Custom, they said: "the easiest and best way to do it would have been to put a list like `[ ] - {entryname} - {add, remove, add+remove}` And in the square brackets you just put + / - / ~ or * and that would be it". On the plan, they said: "it said it will replace some of my files, that is absolutely incorrect, it just modified/appended to some of my files. Which should be the message."

Status: Task 76 “Simple wizards and honest change wording” (phase 2/2): milestone 8/8 — complete, release authorized by the maintainer for after 19:30 on 2026-10-09.

## Reproduced problems

On an Essentials workspace, `install --configure --preset custom --route guidance=add --route memory/archived=git-ignore --dry-run --detail standard` printed:

```text
Would install the Open Forge Framework into <workspace>, replacing 4 existing files.
Frontmatter: root
  .agents/guidance/_guidance.md         would be created
  .agents/loader.md                     would be replaced
  .agents/memory/_memory.md             would be replaced
  .agents/memory/archived/_archived.md  would be created
  .agents/open-forge.json               would be replaced
  .agents/open-forge.lock.json          would be replaced
  .gitignore                            would be replaced
  Would create 2 files and 2 directories under .agents.
  Would create 2 directories.
```

The JSON for the same plan classifies those rows as `section replaced` for both entrypoints and `.gitignore`, `setting replaced` and `record replaced`. Only the generated `Entries`, the Git-ignore section, the settings and the ownership record change. The text renderer maps every `Replace` action to `would be replaced`, the headline counts them as replaced files, the confirmation asks `Replace the 4 existing files listed above?`, and the directory count is printed twice.

The Custom step loops through two questions per route. `Choose a route to change, or finish selection` lists `Finish selection` and each route as `guidance — Add`, with the same retention sentence on every row, then `Choose how to configure guidance` offers Add, Remove or Add + Git-ignore and returns to the list.

## Accepted decisions

The maintainer accepted D1 to D3 on 2026-10-09. The Overseer resolved D4 to D9 within that direction. Reversible defaults are marked.

- **D1 One Custom list.** Custom shows one list of the ten built-in rows with their current marks. `+` adds the route, `~` adds it and keeps its contents out of Git, and `-` leaves it out. The user moves with up and down, presses `+`, `~` or `-` to set the focused row, or presses space to cycle `+`, `~`, `-`. Enter accepts the whole list and Escape cancels. The list starts from the current choices in an installed workspace and from Essentials in a fresh one, with `--route` overrides applied. Rows set by `--route` are locked and say so.
- **D2 Shared marks.** `*` keeps one meaning in every list: included because another choice needs it. Install's Custom list uses only `+`, `~` and `-`. Multi-selection uses the action's mark for a chosen row (`+` to install or update, `-` to remove), `*` for a row included by a dependency, and `[ ]` for a row that is not chosen. The `[x]` mark is retired.
- **D3 Honest change wording.** Each plan and result row names the actual change. `replaced` is reserved for a whole file whose previous content is set aside, such as an occupant replaced by `--force`. The headline counts only those replacements.
- **D4 Row layout.** A list row shows its mark, its label and a short summary clipped to the terminal width. Details for the focused row appear below the list. Row numbers appear only where digit keys choose a row, which is single selection and every line-mode list. The position line appears only when the list does not fit in the view.
- **D5 Line mode for the Custom list.** Line mode prints the numbered list and accepts one or more edits such as `3-` or `2+ 9~`, separated by spaces or commas. It reprints the list after each valid line. An empty line accepts the list, and `cancel` or end of input cancels. This differs from other line prompts, where an empty line cancels, because the list always holds a complete answer and the plan review and `[y/N]` confirmation still follow. An invalid edit repeats the rule and leaves the list unchanged.
- **D6 Focused details for Custom.** A focused `-` row in an installed workspace says that leaving it out keeps existing files and notes, which stay routable, and that Open Forge stops managing its defaults. A focused `~` row says that new files stay on this machine and the entrypoint is still shared through Git. The retention sentence no longer repeats on every row.
- **D7 Change vocabulary.** Plans and results use these labels. The preview form uses `would be` or `would`.

  | Change | Done label |
  |---|---|
  | New file or directory | `created` |
  | New `AGENTS.md` or `CLAUDE.md` | `created with an Open Forge section` |
  | Section added to an existing host file | `Open Forge section added, your content was kept` |
  | Open Forge section in a host file refreshed | `Open Forge section updated` |
  | Generated `Entries` in an existing entrypoint or the loader | `Entries updated` |
  | Install's Git-ignore section added or changed | `Open Forge Git-ignore rules added` or `updated` |
  | Existing `.agents/open-forge.json` | `settings updated` |
  | Existing ownership record | `ownership record updated` |
  | Configure frontmatter conversion | `metadata moved to root keys` or `metadata moved under open-forge:` |
  | Missing metadata completed in an existing file | `metadata completed, your content was kept` |
  | Whole-file replacement | `replaced`, plus `(your previous file is in the recovery bundle)` only when the bundle was retained |

- **D8 Install headlines.** A fresh Install keeps `Installed the Open Forge Framework into <workspace>.`, adding `, replacing <N> existing files` only for whole-file replacements. Configure on an installed workspace reads `Changed the Open Forge setup in <workspace>.` and `Would change the Open Forge setup in <workspace>.` One summary line counts created files and directories and updated existing files, for example `Would create 2 files and 2 directories under .agents, and update 5 existing files.` `Nothing that already exists would be changed.` appears only when it is true.
- **D10 One confirmation pattern.** Every plan confirmation asks `Apply these changes? [y/N]`. When the plan deletes, replaces or removes something, the question names it: `Apply these changes, including deleting 3 files? [y/N]`, `Apply these changes, including replacing 2 existing files? [y/N]`, `Apply these changes, including replacing 2 existing files and deleting 3 files? [y/N]` or `Apply these changes, including removing 4 links? [y/N]`, with correct singular forms. The count covers only that destructive part, and the question still approves the whole plan shown above it. Extension Create keeps `Create these files? [y/N]`. Extension Install's separate authority question before its plan becomes `Allow replacing the <N> existing files listed above? [y/N]`. This supersedes the earlier `Replace the <N> existing files listed above?` and `Delete the <N> files listed above?` questions (reversible default).
- **D11 Prompt layout corrections.** Wrapping breaks at spaces, and legends and control lines wrap only between items. Rows without a mark render `> 1. Label`. Every key-mode list puts `up/down move` first and separates controls with three spaces. Line mode prints the legend directly under the question. The root frontmatter example becomes `description: and tags: at the top level`, so it fits at 80 columns.
- **D9 Boundaries.** JSON effect `kind` and `action` values, `--route` and `--preset` grammar, prompt capabilities, cancellation status and exit codes, and every filesystem effect stay unchanged. The wording changes reach every mutation command whose plan or result calls a partial edit a replacement. The audit in slice R1 names those cases.

## Mockups

Key mode, fresh workspace, Custom:

```text
Choose what Open Forge sets up
+ add   ~ add, keep contents out of Git   - leave out

> [+] directives           Rules agents must follow
  [-] guidance             Advice for recurring choices
  [-] maps                 Pointers to important sources
  [+] patterns             Reusable shapes for code and documents
  [+] skills               Packaged agent capabilities
  [-] templates            Copy-ready starter files
  [~] memory/working       Notes for active work
  [+] memory/emerging      Findings not accepted yet
  [+] memory/crystallized  Accepted knowledge
  [-] memory/archived      Completed and historical records

up/down move   + ~ - set   space next   enter done   esc cancel
```

Line mode, after the list:

```text
Type a row number and a mark, such as 2+ or 7~. Press Enter on an empty line to continue.
>
```

Extension install multi-selection:

```text
Choose Extensions to install
+ install   * needed by your choice

> [+] planning        Plans, tasks and decisions
  [*] workflows       Needed by planning
  [ ] scenarios       User flows and run records
  [ ] core-templates  Installed

space choose   a all   n none   up/down move   enter next   esc cancel
```

Configure plan for the reproduced request:

```text
Would change the Open Forge setup in <workspace>.
Frontmatter: root
  .agents/guidance/_guidance.md         would be created
  .agents/loader.md                     Entries would be updated
  .agents/memory/_memory.md             Entries would be updated
  .agents/memory/archived/_archived.md  would be created
  .agents/open-forge.json               settings would be updated
  .agents/open-forge.lock.json          ownership record would be updated
  .gitignore                            Open Forge Git-ignore rules would be updated
  Would create 2 files and 2 directories under .agents, and update 5 existing files.
```

## Audit dispositions

R1 audited 32 prompts and 9 change-label families at `be285bcc5`. Its report is input to S2 and S3. The Overseer recorded these dispositions. Each implementer revalidates a finding against current code before acting on it.

- **Accepted, wording.** W-1 permission wording, including descriptions in line mode, after verifying who can use a saved path. W-2 line-mode guidance for an invalid answer only. W-7, W-17 and W-18 questions `Choose Extensions to install`, `to update` and `to remove`, and correct dependency notices, including the reversed `Also removing <dependent>, needed by <dependency>`. W-10 to W-12 and C-8 Repair prompts. W-13 and W-14 Extension Create text prompts, without a cancellation hint. W-16 under D10. W-19 Library attach. W-21 to W-24 route disambiguation, naming the action. W-26 preset summaries without the word `routes`.
- **Accepted, labels.** C-1 and C-5 Update, C-2 root remove, C-3 Route Update, C-4 Extension Install ownership row, C-6 Extension Update Entries, C-7 Route Move preview. C-9 and W-3 to W-6, W-8, W-9, W-20 and W-25 under D10. The plan rows above each question show the recorded removal, so root `remove` uses the shared pattern too.
- **Rejected.** W-2 key-mode control footer for `[y/N]`, which adds clutter to a familiar convention. W-15, because `Create these files? [y/N]` is already clear. W-27, because the metadata question and labels were accepted in Task 75 and the examples now explain them.
- **Deferred to the maintainer.** W-1 behavior: the permission prompt focuses `Allow always`, so Enter saves a lasting grant. Focusing `Allow once` would be safer but changes behavior.

## Execution capsule

- **Profile.** Standard, with one independent review of the integrated branch and one grouped correction pass. The work changes public prompt behavior and wording in a beta, with no new data, filesystem effect, dependency or wire shape.
- **Applicability check.** Local developer tool. Effects stay inside the existing Install, Extension and other command plans, which this Task does not change. Recovery comes from Git and the unchanged recovery bundles. The existing `CliPrompts` key and line modes, viewport renderer and report selectors are sufficient. Shared foundations reused: `CliPrompts`, `CliSelectionFrameRenderer`, the Install report selector and each command's existing effect facts. Exceptional machinery: none.
- **Evidence.** Unit tests for the list primitive in key and line modes, the marks, layout at normal and small viewports, and the wording selectors. Integration snapshots for Install, Configure and any audited command whose text changes. EndToEnd ConPTY journeys for the Custom list and the Extension picker, plus line-mode fallback. The shared prompt change triggers the complete managed and Native AOT gate at closeout.
- **Workers.** Worker Watch with `gpt-6.1-sol` at `xhigh`, as the maintainer named for this session. Mutation slices run one at a time in their own worktrees from the committed tip of `feature/task76-simple-wizards`. A read-only audit may run beside them. The Overseer integrates each returned diff, runs the gates that need per-user stores and commits accepted slices on the feature branch.
- **Stop conditions.** A slice stops and reports when it needs a new public behavior beyond D1 to D9, a dependency, a wire-shape change or a protected path, or when a decision cannot be implemented as written.

## Slices

| Slice | Owner | Outcome | Milestone |
|---|---|---|---|
| P1 | Overseer | Shared prompt and Install contract amendments | M2 |
| R1 | Read-only worker | Audit of every wizard and every plan or result that calls a partial edit a replacement, with current text and proposed wording | Input to M6 |
| S1 | Worker | Mark list primitive, shared marks and row layout, and the Install Custom list | M3, M4 |
| S2 | Worker | Install and Configure change wording, headline and summary, the D10 confirmation pattern for Install, and the shared change labels | M5 |
| S3 | Worker | Accepted R1 change labels for the other commands, and the D10 confirmations | M6 |
| S4 | Worker | Accepted R1 prompt wording for the other wizards | M6 |
| S5 | Overseer or worker | Public docs, CLI Skill, flows and scenarios | M7 |
| S6 | Overseer | Review, corrections, managed and Native AOT gates, manual check | M8 |

## Review and corrections

One independent review of `b406db059..077ec7308` returned four major findings and one minor finding. All were accepted and fixed in `490a4c272` and `fa832ef62`.

- **A-1** Install labelled a preserved-content edit, such as completing metadata in an existing native Skill, as replaced. It now reads `metadata completed, your content was kept`.
- **A-2** Update, Extension Update and Extension Install final confirmations did not name whole-file replacements. Confirmation facts now carry replacement and deletion counts, including the combined `including replacing <N> existing files and deleting <K> files` form.
- **A-3** Update counted unstarted replacements. Counts now follow planned or verified outcomes.
- **B-1** The replaced label promised a recovery bundle that a successful Install removes. The clause now appears only when the bundle is retained.
- **B-2** Live command contracts still described the old wording. They now match the code.

Integration found further defects that focused slice evidence missed: a dropped effect row for migrated paths, unfinished Route Move link rewrites removed from the text, misplaced Remove text captures, a reversed remove-direction dependency detail, and stale journey assertions. The [focused evidence observation](../../../../emerging/observations/2026-10-09_focused-worker-evidence-misses-shared-output-guards.md) records the pattern.

## Closeout

- Final feature tip `fd8f7bc97`. Managed Windows: Unit 4,539 passed, Integration 2,977 passed with 17 declared platform skips, EndToEnd 330 passed.
- Linux, from a WSL clone of `fd8f7bc97`: Unit 4,539 passed, Integration 2,967 passed with 30 skips, EndToEnd 297 passed with 33 skips. Every skip reason is declared in `scripts/delivery/platform-skips.ts`.
- Native AOT: recorded with the release receipt.
- Manual check with the built CLI: the reproduced Configure request, a fresh Install onto an existing `AGENTS.md` and `.gitignore`, and a real `route move` that rewrote both README links and moved the Entries line.
- Docs: CLI reference, installation and growing pages, CLI Skill, scenarios C03-20 and C03-21, refreshed Entries, and a passing docs link check.
- Deferred to the maintainer: focusing `Allow once` instead of `Allow always` in the permission prompt.

## Current state

Complete. Squash integration into `develop` and the beta10 release follow the authorization the maintainer gave on 2026-10-09.
