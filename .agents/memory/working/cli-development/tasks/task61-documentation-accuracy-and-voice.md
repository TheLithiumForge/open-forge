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

**Now:** The earlier documentation pass remains integrated, with its release and
site-verification receipts recorded in Task59. The current documentation
reconciliation is integrated in the verified beta4 candidate, and the site
type-check/build gates are green. [Task 69](task69-next-beta-stabilization-release.md)
records the local qualification, review, clean merge, and pending exact-merged
hosted gate. The maintainer review checkbox above remains unchecked.

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
