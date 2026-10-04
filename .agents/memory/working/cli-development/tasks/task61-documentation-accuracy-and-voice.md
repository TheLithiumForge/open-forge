---
open-forge:
  description: Task 61 accuracy and voice pass over the documentation site, the README, and the repository guides, with a diagram that separates Core from Extensions and startup from on-demand loading
  tags: [Memory, Working, Task, Documentation, Site, Diagram, Writing, Contextual, Active]
---

# Task 61 — Documentation accuracy and voice

## Outcome

Requested by the maintainer on 2026-09-27, after reviewing the site from
[Task 57](../../../archived/cli-development/tasks/task57-onboarding-and-demos.md). Every page of the documentation
site, the README, and the guides under `docs/` states only what the shipped
files, the Extension packages, the Framework and CLI documents, and the CLI
itself support. The diagram and the wording make two distinctions explicit
everywhere: what ships with Core versus what an Extension adds, and what loads
at startup versus what opens on demand.

**Direction from the maintainer:**

- Name Extension content as examples with their source, for example "a
  Checkpoint or a Handoff", never as if the base install contained it.
- Loading an entrypoint doesn't load what it lists. The diagrams and the prose
  must say so.
- Keep the friendly, approachable voice, moved slightly toward an engineering
  README. A reader who has never seen Open Forge must be able to follow it.
- Check claims against the repository's own knowledge, not against what the
  documentation assumed. Multiple agents may be used.

**In scope:** `src/docusaurus/docs/**`, the homepage components, the framework
diagram and its README SVGs, `README.md`, `docs/cli.md`, `docs/extensions.md`,
`docs/development.md`, `src/extensions/README.md`, and the demo READMEs and
checklists.

**Preserve / out of scope:** shipped Framework and Extension content, demo app
code and seed records, and page structure and URLs.

**Done when:**

- [x] Every page has had an accuracy pass against its sources, with corrections
      and unresolved contradictions recorded below.
- [x] The diagram shows Core versus Extension content and startup versus
      on-demand loading, on the site and in the README SVGs.
- [x] The site builds with broken links and anchors failing, and type-checks.
- [ ] The maintainer reviews the result.

## Current State

**Now:** On 2026-10-02 the maintainer extended this isolated follow-up to
autoload the CLI usage Skill, cover every command's flags and useful cases,
and explain the product as Markdown, rules, and links. This new bounded
horizon is complete at phase 2/2, milestone 2/2. The source and dogfood
Skill, startup read rule, maintenance contract, public explanation, and
diagram are aligned and verified. The broader maintainer review remains
open; this receipt does not close the entire Task or qualify a release.
The previous completed documentation horizon below remains historical evidence.

The 2026-10-03 continuation reached local acceptance in Root's isolated
`docs/onboarding-and-presets` worktree from local develop `de54c1d`.
[Task 73's dated capsule](../../../archived/cli-development/tasks/task73-layered-adoption-and-installation-choices.md#active-execution-capsule-2026-10-03)
records the single ten-minute guide, restoration behavior, accepted installer
choices and combined qualification at that stage. Tasks 72 and 73 subsequently
shipped in beta6 after squash integration into develop and all six hosted
platforms passed. [Task 70](../../../archived/cli-development/tasks/task70-existing-workspace-adoption-during-installation.md#beta-6-release-complete-2026-10-04)
retains the completed integration, publication and public checks. Startup reading
respects an intentionally omitted CLI Skill. Help covers all 29 command paths
and six globals; the site build and formatting passed. Earlier worktree and
qualification statements below describe their dated horizons. The broader
Task 61 maintainer review remains open.

The new scope explicitly includes `src/open-forge/.agents/skills/`, its
repository counterpart, the Skills Maintenance contract, and affected
public loading descriptions and diagrams. It changes authored instructions,
not the CLI parser, loading-tag semantics, runtime Skill activation, or
Extension wizard behavior. Use the existing Skills Axiom to require the
native Skill at startup; do not introduce a second Skill metadata format.
The exact flag guide earns its baseline cost through the maintainer's
explicit request; report the measured increase. No release, remote action
or integration was selected for that earlier bounded documentation horizon.
The later combined release is complete in Task 70; the primary agent retains
the separate broader maintainer-review boundary here.

The maintainer also requested a reminder after completion about an as-yet
unrecalled concept to promote or showcase. Do not invent that concept.
The reminder is included in the final handoff; the simplification direction is accepted
independently of that pending idea.

**Continuation verification and applicability:**

- Both Skill copies match byte-for-byte, keep valid native frontmatter, and
  expose the exact flag sets of all 29 current command paths plus six global
  flags. Best-use cases cover initial batches, full working-file sets,
  selected chains, additions-only reads, bounded links, discovery, and
  reviewed changes. The existing native-metadata contract is unchanged.
- The two focused managed embedded-payload checks and all 13 repository
  documentation checks pass from the final worktree build. The actual Windows
  x64 Native AOT CLI publishes and executes successfully. Its owned fresh
  workspace passes install preview with no writes, automatic install, exact
  installed Skill hash comparison, a 12-source startup batch with no duplicates,
  additions-only selection, depth-one linked context, and doctor.
- Both affected trees have current generated indexes. The source payload and
  fresh installed fixture pass doctor. Repository doctor stops with exit 5
  on 12 existing `reference.target-alias` findings involving historical
  `beta-follow-ups` links; no broad repair is selected.
- Startup reads 12 files and estimates 9,618 tokens, including 3,531 for the
  complete CLI guide. All 15 base files estimate 10,288 tokens. These are the
  CLI method's per-file character count divided by four and rounded up, not
  tokenizer measurements. Plain context/status still derive an 11-file
  loading-tag closure; explicit read instructions remain the agent's duty.
  Public numbers and both generated SVGs reflect this distinction.
- Site type checking and the production build pass. Browser inspection verifies
  the startup Skill row and all seven compact CLI tables at phone width without
  page overflow. The viewport is reset, and the local preview is preserved.
  All 18 changed public/payload inputs match the build's source copy by SHA-256.
- One coherent Writing Reviewer pass returned two meaning corrections:
  preserve both working paths in the continuation example, and require a CLI
  upgrade only when a newer bundled Framework is wanted. Both are corrected
  and inspected by the primary owner. Required-read wording is aligned across
  the introductions, glossary, loading pages, and diagram.
- The Skill Creator Python validator was attempted but its local Python
  environments lack PyYAML. Prepared js-yaml validates native metadata instead;
  exact parity and flag coverage are checked separately. No dependencies are
  downloaded. Formatting and whitespace checks pass.
- Focused managed and native evidence is selected for changed payload prose
  and its installed representation. No parser, serializer, dependency,
  resource identity, or lifecycle implementation changes; the complete
  release gate and six-platform qualification remain outside this horizon.

Continuation evidence is in `artifacts/verification/skill-*.json` and
`skill-*.log`, with the owned installed fixture in
`artifacts/verification/skill-workspace/` and the native executable in
`artifacts/verification/native-cli/`. The source remains uncommitted in
`docs/cli-simplification`; no integration, push, or publication occurs.

### Completed First Simplification Horizon

**Now:** Task 61 “Documentation accuracy and voice” (phase 2/2): milestone
2/2. The bounded documentation simplification follow-up is complete in its
local worktree. The earlier maintainer review remains open, and this does not
close the whole Task or accept a release. The maintainer
requested a simpler introduction and clearer CLI flag
discovery on 2026-10-02 after a successful demonstration. This follow-up puts
setup and a normal first task before the taxonomy, gives the README and site
a short task-flow diagram, makes the full map optional, and lists each
command's flags in the compact CLI overview. The separate wizard issue is
queued as [Task 72](../../../archived/cli-development/tasks/task72-extension-wizard-terminal-layout.md).

The bounded horizon has two milestones: complete the public prose pack,
then verify flags, examples, links, formatting, and the rendered site. The
primary agent owns this work on `docs/cli-simplification` from
`634ab07051e670e3f9bcc1b6603ac39a91f4febc`. Scope is the README, introduction,
installation and first-task pages, CLI overview and flows, and the CLI
reference introduction. Shipped payloads, CLI behavior, detailed command
contracts, existing diagram assets, other tasks, and the other agent's
checkout remain outside this follow-up. No integration or publication is
selected.

**Verification:**

- The compact overview covers every command-specific flag in 29 command rows,
  compared with the prepared beta5 executable's help. The globally installed
  beta1 executable was not used as the current interface authority.
- The existing managed repository documentation gate passes all 13 checks.
  Site type checking and the production build pass, with broken links and
  anchors failing the build. Formatting and `git diff --check` pass.
- Ten documented CLI steps pass in a fresh owned example workspace: install
  preview and apply, context, finding rules, doctor, index preview, and
  Extension list, inspect, preview and apply. Install preview writes nothing.
- Browser inspection verifies the expandable map and installation details,
  links into those details, and the short diagram and flag tables at phone
  width without page overflow. The default viewport is restored afterward.
- All seven changed public source files are compared by SHA-256 with the
  production build's source copy. Prepared dependencies are local to that
  verification copy because sharing a dependency directory between worktrees
  caused the initial static-rendering failure. No dependencies were downloaded.
- Task navigation is regenerated. The broader workspace `doctor` exits 1,
  reporting archived-reference ambiguity, historical missing targets,
  unindexed support files, and installed Extension state/version warnings.
  These remain outside this documentation follow-up. Directory paths in the
  new wizard task are plain source pointers, not unsupported reference links.

Evidence is retained under `artifacts/verification/`, including the flag
inventory, copied managed test runner, and example-command JSON results.
`artifacts/site-build.log` records the successful build, and
`artifacts/site-verification/` contains its exact public prose inputs and
rendered site. The local preview is available on port 4387 while its serve
process remains running. Changes remain uncommitted in the isolated worktree.

**Earlier baseline:** The earlier documentation pass remains integrated, with its release and
site-verification receipts recorded in Task59. The current documentation
reconciliation is qualified and deployed with beta4, and the site type-check/
build gates are green. [Task 69](../../../archived/cli-development/tasks/task69-next-beta-stabilization-release.md)
records the completed qualification, publication, and documentation receipt.
The maintainer review checkbox above remains unchecked.

**Shipped content changed:** the `open-forge-cli` Skill said to read "the
status that ends each result", but text output has no status line. It now
points to the exit code and `--format json`, and says that confirming commands
need `--automatic` in a noninteractive shell. Both copies are updated, and the
integration snapshot refresh recorded in [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md) covers
the new bytes.

**Diagram decisions:**

- Four loading badges replace three: "Loaded at startup" (the loader,
  `AGENTS.md`, and root Directives), "Entrypoint at startup", "Entrypoint at
  startup, re-read" (Emerging, `KeepInMind`), and "On demand" (Templates,
  Archived).
- Solid chips mark what ships with Core (`open-forge-cli`). Dashed chips mark
  what an Extension adds, with the Extension's name. Extension categories
  carry no loading tag, so only their one-line entry is visible at startup.

## Council on 2026-09-28

At the maintainer's request, a council reviewed the README and the site's
first-contact pages, following the Open Forge council workflow. It had two
independent members: OpenAI Codex's `gpt-6-astra` at high effort, run read-only
through Worker Watch, read as an outsider. A Claude Opus agent checked
accuracy and structure. Neither saw the other's position, and the synthesis
did not count votes.

**Both found independently, and applied:**

- The standalone loading summaries on the homepage, in the README, and in the
  diagram legend left out that entries tagged `LoadNow` or `KeepInMind` load
  too.
- "Record Decisions for every change" overstated the Planning package, which
  defines a Decision as an important accepted choice.
- The Extensions overview carried catalogue history a newcomer doesn't need.

**One member found, verified, and applied:**

- Setup review now uses `git status` as well as `git diff`, because a fresh
  install adds untracked files that `git diff` doesn't show.
- The first-task page has a concrete starting prompt.
- The adoption guide installs the Development Toolkit after previewing it,
  and separates Directives (rules) from Patterns (shapes).
- The diagram's Directives card says root rules load at startup, the refresh
  note lists all four refresh points, and the homepage tree lists `CLAUDE.md`.
- The introduction and the loading page lead with what a newcomer asks first,
  and the glossary defines "harness".
- Checking the regenerated SVG found a chip wider than its card. The generator
  now wraps an Extension's name onto a second line inside the chip.

**Declined, with the reason:**

- Both members proposed replacing the README's opening sentence with a more
  mechanical one. The current opening is the model example in the accepted
  [Project Voice](../../../crystallized/documents/maintenance/project-voice.md)
  guide, so it stays unless the maintainer changes that guide.
- One member proposed moving the diagram below setup. The maintainer asked for
  it near the top of the README.
- One member proposed cutting "early feedback named Decisions". The maintainer
  is the source of that feedback.

## Findings for follow-up

These are the original review observations, retained for provenance.
[Task 64](../../../archived/cli-development/tasks/task64-cli-defects-and-contract-drift.md#historical-source-state)
records the accepted behavior and wording fixes that shipped in beta4, plus
later recovery-diagnostic work. Use its current state and
[Project Control](../project-control.md#active-task-ledger) to determine what
remains open. This list is not a current defect inventory, and the broader
maintainer review of Task 61 remains pending.

The reviewers found these while checking the documentation. They are outside a
documentation pass, so nothing here was changed. The pages describe what the
CLI actually does today.

### CLI behavior

- `find --include=<source> --content=metadata` returns `incomplete` for every
  match.
- With a Library link under `.agents`, `update --dry-run` is blocked as
  crossing a link, and the message blames a nonexistent `Entries` section in
  `open-forge.lock.json`. `status` reports the workspace as current.
- `extension update --all --dry-run` says "Nothing to do" but lists `Entries`
  sections it would update. A real `extension update` reports the
  `use-workflow` catalogue's `Entries` as updated when the file doesn't change.
- `route move` returns `incomplete` when a file that links to the moved file
  also contains an unrelated broken link.
- After `maps/_maps.md` is deleted, `doctor` also flags the unedited
  `loader.md` as changed.
- While any recovery bundle is kept, `route create`, `init`, `update`, `move`,
  and `remove` are blocked. `doctor` reports no problems, and the blocked
  command's next step points to `doctor`, not `cleanup`.
- `update` returns `completed` (exit 0) when it keeps a recovery bundle, where
  the update contract lists `completed-with-warnings`.

### CLI wording

- `extension remove --dry-run` prints "Removed ..." without "No files were
  changed."
- `remove <path> --dry-run` prints ".agents/open-forge.json created" although
  nothing was written.
- The `library detach` and `remove --kind library` dry runs print "Would the
  Entries section of ...", with the verb missing.
- On a fresh install, `extension list` reports "no ownership record" although
  the lock file exists.
- `update --help` and `extension update --help` describe `--force` as
  replacing changed or restoring missing content. The contracts, and the help's
  own Authority section, say it adds nothing.
- `extension create --help` says supplying both required values avoids
  prompts. Create still asks for confirmation.

### Contract contradictions

- The Extension install and create contracts say there is no generic or second
  confirmation. The CLI confirms, and needs `--automatic` when noninteractive.
- The `route remove` contract contradicts itself about managed content. A real
  run removed an Extension-owned Template and released its claim.
- The Extensions Architecture says removal protects user changes. The remove
  contract and the CLI delete edited final-owner files after saving a recovery
  bundle.
- The update contract still calls the command "non-shipping".
- The Extension list contract's example output shows the retired
  `memory-starters` package.
- Help wrapping (terminal width, 80 columns when redirected) is defined only in
  code, not in a contract.
- `library attach` into a Framework category, such as `.agents/guidance/`, is
  blocked as another managed domain, while `route init` edits the same `Entries`.
  The contract doesn't say whether that is intended.

### Questions for the maintainer

- Keeping a direct edit through updates, which `removedFiles` allows as an
  undocumented side effect, is now [Task 63](task63-keeping-edits-through-updates.md).
- Resolved on 2026-09-27: contributors need Node.js 22.18 or later, as
  `package.json` requires, unless a change depends on a newer API. The
  development guide says so.
- The CLI `install` always creates `CLAUDE.md`, while the manual route treats
  it as optional.
- The tag syntax wording differs slightly between the Markdown syntax contract
  and `docs/cli.md`.
- The Checkpoints `KeepInMind` gap is recorded in
  [Task 53](task53-loading-and-scoping-audit.md).

### Environment

Windows Application Control blocked `open-forge-dev.exe` at times on this
machine. Some reviewers verified against the contracts instead. The CLI
reviewer ran the same build's `OpenForge.Cli.dll` through `dotnet`, which
Application Control allowed.
