---
open-forge:
  description: Open Task 78 to make the documentation, CLI help and every wizard clear, easy to explain and simple to use, through a UX audit and the changes it supports
  tags: [Memory, Working, Task, CLI, Documentation, Site, Wizard, Prompts, Help, UserExperience, Wording, Contextual, Active]
---

# Task 78: Clear docs and wizards

## Outcome

A newcomer can explain Open Forge, each command and each wizard step in one plain sentence after reading it once. The documentation site, `README.md`, CLI help, results, errors and every interactive wizard use the same plain terms, lead with what the reader needs, and show the next step.

The maintainer asked on 2026-10-12: "Go thro the documentation and cli wizards and make sure we are - clear - easy to explain, and actually good ux / ui. Both you claude and sols need to use some ux/ux skills to make sure we offer a simple to use, streamlined, easy to underatand ui, wizard, docs." They named Sol 6.1 at xhigh for the workers.

Status: Task 78 “Clear docs and wizards” (phase 3/3): milestone 6/6 — complete with local search and the Allow once default, every gate passed on the Task 80 stack at `0b6a259d0`; integrates as one squash with Tasks 79 and 80.

## Relationship to open Tasks

Tasks [61](task61-documentation-accuracy-and-voice.md), [66](task66-council-polish.md) and [67](task67-diagram-labels.md) delivered documentation accuracy, polish and diagram simplification, integrated in `a8bbf4677`. Their maintainer reviews remain open. Task 78 builds on that content and does not reopen their decisions. [Task 76](../../../archived/cli-development/tasks/task76-simple-wizards-and-honest-change-wording.md) decisions D1 to D11 stay accepted. Tasks 34, 37 and 39 stay paused or unselected. Task 78 may resolve findings they recorded, and records any overlap at closeout.

## Decision boundary

The maintainer's request accepts wording, layout and documentation changes that make the same behavior clearer. The Overseer may also accept presentation-only wizard changes, such as a recommended marker, a progress line or clearer focused detail. Proposals that change filesystem effects, JSON shapes, flags, defaults, exit codes or which questions a wizard asks go to the maintainer with a recommendation.

## Execution capsule

- **Profile.** Standard. Public wording, help and prompt presentation change in a beta, with no new data, effect, dependency or wire shape unless the maintainer accepts one.
- **Base.** `feature/task78-clear-docs-and-wizards` from develop `c76f93066`.
- **UX method.** A shared UX review kit (personas, the one-sentence test, Nielsen's heuristics, clig.dev, wizard practice, Diátaxis and plain language, severity and finding format), kept with the session's scratch material. The Overseer reviews the docs site visually with the `frontend-design` skill.
- **Evidence tooling.** The audit uses a Release build of develop with `OPENFORGE_DATA_HOME` redirected into scratch folders. A scratch ConPTY capture tool reuses the end-to-end terminal harness to record each wizard screen in key and line mode.
- **Workers.** Worker Watch, `gpt-6.1-sol` at `xhigh`, as the maintainer named.
- **Closeout.** Independent review of the integrated branch, docs and help examples updated, `index`, link and anchor checks, a manual check of the changed wizards with the built CLI, and the managed and Native AOT gates for changed CLI source.

## Phases and milestones

| Phase       | Milestone | Outcome                                                                   |
| ----------- | --------- | ------------------------------------------------------------------------- |
| 1 Audit     | M1        | UX kit, capture tool and six read-only audit lanes started                |
|             | M2        | Audit reports returned and synthesized with the Overseer's own review     |
|             | M3        | Findings triaged, accepted changes frozen as slices, maintainer questions |
| 2 Implement | M4        | Accepted slices implemented and integrated                                |
| 3 Qualify   | M5        | Independent review and one grouped correction pass                        |
|             | M6        | Gates, site build and manual checks pass, ready for maintainer review     |

## Audit lanes

| Lane | Surface                                                                         |
| ---- | ------------------------------------------------------------------------------- |
| A1   | Install and Configure wizard, Install help, the first result and first `status` |
| A2   | Every other wizard and confirmation                                             |
| A3   | Command-line help, read-command results, errors and `Next:` hints               |
| A4   | README, intro, getting started, demos, site navigation and homepage             |
| A5   | Concept pages, glossary, Extension pages and diagrams                           |
| A6   | `docs/cli.md`, the site CLI pages and the shipped CLI Skill                     |

## Overseer findings

- **O-1 Stale startup numbers on the homepage (S1).** The framework map says "A fresh install reads 12 files at startup, about 9.6k estimated tokens, including the CLI guide". The base section shows "~5.7k tokens loaded at startup". A fresh Full Core install with this build reports `12 sources, about 7.3k tokens` from `open-forge context skills/open-forge-cli`, and Essentials reports `10 sources, about 6.8k tokens`.
- **O-2 No search on the documentation site (S2).** The navbar has no search box. Readers must browse the sidebar to find a command or term.
- **O-3 The homepage opens with the densest picture (S2).** The complete framework map follows the hero directly. A newcomer sees every category, badge and loading rule before the short explanation of what Open Forge does for them.
- **O-4 Default 404 page (S3).** `/docs/intro` returns Docusaurus's generic "Page Not Found" text without a link to the docs or getting started.

## Audit results

The six lanes reported 13 S1, 48 S2 and 9 S3 findings with real CLI captures. The reports and captures are kept outside the repository with the session's scratch material. The decisions below carry their accepted content.

The most serious problems:

- Install's `Next:` rerun command drops the workspace, preset, metadata form and routes, so following it literally installs a different setup, possibly in another folder (A1-1, A3-1).
- Documented journeys break on the recommended Essentials setup, because it leaves out Templates, Maps and, in Custom, possibly Directives. The Decision flow, the ten-minute guide's first rule, the brownfield demo and the starter-only packages fail or silently install nothing (A4-05, A4-11, A5-1, A6-1).
- Two Extension lifecycle defects: adding `development-toolkit` after `planning` blocks on apply after a successful preview, and the toolkit on Essentials leaves a record that `extension update` cannot read (A6-2, A6-10).
- Results that contradict themselves: a cancelled or blocked Extension install says Entries were updated, a removal preview names one of two packages, `Previous content: git diff` is offered as recovery for untracked files, and Library recovery accepts selections its planner rejects (A2-1, A2-2, A2-3, A2-15).
- Help reads like a contract: Install help is 105 lines with its first example at line 73, seven commands have no examples, and Route Remove help contradicts its behavior (A3-3, A3-2).

## Accepted dispositions

The Overseer accepted these within the maintainer's direction on 2026-10-12. Text in quotes is the intended meaning, and slices may refine wording under the Writing Standard.

- **Next commands.** Every printed `Next:` or retry command is complete and runnable for the intended request. It adds `--automatic` only when the reason is missing confirmation. Text and JSON carry the same command. The JSON `next.command` value may change, its schema does not.
- **Install wizard.** The plan and receipt recap the resolved setup. The review shown before `Apply these changes? [y/N]` no longer says `No files were changed.` A first successful install ends with how the agent uses the files and a next step. Line mode shows option consequences and how to cancel. Short stage captions, plain focused details, a recommended cue on Essentials, and guidance on the metadata question. No change to which questions are asked.
- **Help.** One template for every command: purpose, Usage, Examples, Behavior, Arguments, Options, Details, Automation, Related commands. Root help starts with a first-run sequence that names the preset in preview and apply, then groups commands by intent. Examples never wrap into different shell commands. Every existing fact stays available.
- **Errors and layout.** Invalid input names the bad value and the help command. Context errors stop repeating unavailable counts. Cancellation before any effect reads as a cancellation, not an error. Tables and line-mode text wrap at spaces, with a long path on its own line.
- **Results.** Honest Extension cancellation, removal and dependency wording. Recovery advice names the retained recovery bundle. Update adoption counts stay the same from preview to result. Library recovery asks for one step, matching its one-entry limit. Repair reasons and leave-unchanged results use plain words.
- **Restoring a left-out category.** Docs and CLI hints use `open-forge route init <category> --framework`, previewed first. `open-forge install --configure` stays the interactive way to change several choices. Both produce the same settings.
- **Documentation.** The tutorial recommends Essentials and Root keys and includes fallbacks for left-out categories. Full Core supplies 19 Markdown files. No unqualified startup token figure. One accurate description of Git-ignored Working Memory. Sidebar: Start here, Try a demo, How-to guides, Understand Open Forge, Using the CLI, Extensions, Reference. Homepage leads with a three-command try-it block and moves the full map behind a disclosure. Every package page has an Install section. The glossary gains everyday terms with examples. `docs/cli.md` gets a command index and one section per operation.
- **Information ownership.** Help gives immediate syntax and common examples. `docs/cli.md` is the complete reference. The site CLI overview is a short task map. Flows hold complete procedures. The CLI Skill holds agent command selection and offline first-use guidance.

## Maintainer questions

The maintainer answered on 2026-10-13 and 2026-10-14.

1. **Permission prompt default.** Key mode focuses `Allow always`, so Enter saves a lasting grant for every Extension and Library. Task 76 deferred this. Recommendation: focus `Allow once` and keep the numbered order. Decided: the maintainer left the choice to the Overseer, who chose to focus `Allow once`, so Enter never saves a lasting grant by accident. Implemented in `381fab775`.
2. **Unattended default preset.** Decided: unattended installs keep Full Core, and `--preset essentials|full-core|custom` always selects another preset. The wizard keeps starting on Essentials.
3. **Documentation search.** Decided: add `@easyops-cn/docusaurus-search-local` because it is free (MIT) and fully local. The index is built with the site and searched in the browser. Its optional `open-ask-ai` peer is not installed.
4. **Templates in Essentials.** Decided: no for now. Essentials stays small and the skip stays visible in the CLI and docs.

## Implementation slices

| Slice | Scope                                                                                     |
| ----- | ----------------------------------------------------------------------------------------- |
| X1    | Extension lifecycle defects, then Extension wording (A5-1, A5-2, A2-1, A2-2, A2-6, A2-14) |
| C1    | Install wizard, plan, result and next action                                              |
| C2    | Help for every command                                                                    |
| C3a   | Shared layout and prompts, invalid input, Context errors, cancellation rendering          |
| C3b   | Route, Repair and Update results                                                          |
| D1    | README, intro, getting started, demos, sidebar, footer, homepage and 404 page             |
| D2    | Concepts, glossary, Extension pages and diagrams                                          |
| D3    | `docs/cli.md`, the site CLI pages, flows and the CLI Skill, then a final sync             |
| X2    | Library links unreadable on Windows                                                       |

## Slice receipts

- **D1, D2, D3** integrated as `02ec9375f`, `3065cda70` and `1fc640213` after the Overseer's prose review and a browser check of the homepage, map, phone width and dark mode. D3 replayed 244 commands across 26 scratch repositories. Its final sync with the new CLI text follows the CLI slices.
- **X1** fixed A6-2 and A6-10. `ExtensionInstallTopologyBuilder` folded a dependency host's `Entries` into package delivery while the effect planner skipped already installed dependencies, so the host had no effect and failed the unchanged-target check. `ExtensionInstallTargetInspector` discarded a package whose payload exclusions had emptied, so Essentials recorded the toolkit's dependency on Scenarios without a Scenarios record. Completed-update rows now require verified effects, a fully excluded package keeps an empty-path record, and Update names the damaged package. Four failing-first Integration cases and one Unit test cover them. Worker run: 353 Unit, 498 Integration, 20 EndToEnd and 7 architecture tests passed in the sandbox.
- **X2 decision.** Attached Library files cannot be read on Windows because `LibraryMapping` builds link targets with `/` and the applier creates them unchanged. The accepted designs required that raw target. The Overseer accepted a bounded correction on 2026-10-13: mapping identity stays portable, on-disk targets use the platform separator, ownership comparisons ignore the separator, records and JSON stay unchanged, and legacy Windows links are reported as unreadable with exact detach and attach commands instead of a new mutation effect.
- **C1, C2, C3a, C3b, X1b, X2, X3** integrated through `f3f933d49` to `c606c2eee`. The Overseer walked the Install wizard with the built CLI and sent two follow-ups: plain Full Core detail, no repeated cues and an honest closing line (C1b), and compact two-column help that cut Install help from 137 to 74 lines (C2b). Two merge workers resolved the shared prompt API between C1 and C3a (R1) and made the integrated branch green (R2), which also restored Entries effect rows in minimal Extension Install output. X3 finished the excluded-package journey: a package whose files are all excluded is recorded without its files, and the result names the restore and update steps.
- **Independent review.** Two Sol reviewers covered code, contracts and safety (5 findings, 4 S1) and user experience and documentation (8 findings, 2 S1). Both S1 groups concerned next commands that still dropped request facts, a Repair layer violation and removal previews that suggested a blocked dependency removal. One grouped correction pass per review (FX1, FX2), merged by R3, fixed all of them and swept every other next command. The docs were synced twice with the built CLI (D4, D5).
- **Cross-platform snapshots.** The WSL gate on `5213422f4` failed 1 Unit and 79 Integration tests because retry commands quote a Windows path but not a simple Unix path, while snapshots normalized the path after rendering. The fix makes snapshot normalization platform-neutral rather than changing product quoting (R4).

## Decisions taken during implementation

- **Next commands.** Retry and next commands are formed in typed request facts and quoted with the shared helper. A preview's next step is the complete apply request for the same selection. Dependency removals are offered only after the selected removal is applied.
- **Excluded Extension files.** A package whose every file is excluded is recorded without files, so dependency records stay valid. Partial exclusions are named in plans and receipts.
- **Windows Library links.** See the X2 decision above.
- **Help.** Leaf pages list their own options and summarize global options in one line. The exit-code table is a short footer. Details never repeat Behavior.

## Final qualification

The final branch commit `1c5f2e360` passed every local gate on 2026-10-13:

- Windows `test:built` after `build:native`: Unit 4,682, Integration 3,093 with 17 declared platform exclusions, managed EndToEnd 331, native Integration 3,093, native EndToEnd 331 and EndToEnd against the native CLI 331.
- WSL Ubuntu (`gate:wsl`): Unit 4,682, Integration 3,074 passed with 36 declared skips, EndToEnd 298 passed with 33 declared skips.
- Site type check and production build with broken links and anchors failing, Prettier on all 112 changed files, ESLint, and the repository Markdown gate inside Integration.
- The Overseer walked the fresh Install, Configure, the excluded-package journey, Extension removal previews and the error paths with the built and native CLIs, and checked the homepage, Installation, glossary and framework map in a browser at desktop and phone width and in dark mode.

## Follow-ups

- `RouteMoveGitIgnoreIntegrationTests` (`bin` case) failed once while the WSL gate loaded the machine and passed three times alone. Its five-second Git deadline is sensitive to CPU contention. It predates Task 78.
- The four maintainer questions above remain open.
