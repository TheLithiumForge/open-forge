---
title: Everyday flows
description: Complete CLI procedures with previews, review pauses, application, and observable results.
---

# Everyday flows

Each flow gives commands in order, with a review pause before each change. Run
the apply block only after reviewing its preview. The [command reference](/guides/cli)
has every option, and the [glossary](../glossary.md) defines the terms. Shell
blocks use `\` for line continuation. In PowerShell, put a wrapped command on
one line or use a backtick.

Start with the base Framework. Later flows use Documents from Project Documents,
and Decisions and their Template from Planning. `development-toolkit` installs
both. Essentials leaves Templates out, so restore that category before installing
Planning or the toolkit when their Templates are wanted.

## Set up a workspace

Use this when adding Open Forge to a new or existing project.

1. Preview the selected setup.

   ```sh
   open-forge install --preset essentials --dry-run
   ```

   Review the setup recap and the files and settings it will change. Naming
   Essentials keeps the preview and apply on the same selection. It includes private Working contents
   and a shared entrypoint. Use `--detail standard` for per-file reasons. Existing
   `AGENTS.md` or `CLAUDE.md` content is kept when the Open Forge section is added.
   In an existing `.agents` tree, also review planned Skill migrations. See
   [installation](../getting-started/installation.md#with-the-cli) for adoption rules.

2. Apply that setup and inspect the result.

   ```sh
   open-forge install --preset essentials
   open-forge status
   git status
   git diff
   ```

   Check that `.agents/loader.md`, settings, and the ownership lock exist. Status
   shows context, installed Extensions, and anything needing attention. Plain
   interactive Install offers Minimal, Essentials, Full Core, or Custom instead. New
   private Working records stay out of ordinary Git commits.

3. If your agent tool does not read `AGENTS.md` on its own, ask it to read that
   file first. Then give your agent a normal task. No Extension or selected
   workflow is required.

The installation record lets later updates distinguish supplied files from yours.

## Use documents you already have

Use this when the project already keeps rules or knowledge in its own files.

1. Preview one include.

   ```sh
   open-forge route include CONTRIBUTING.md --description "Contribution rules to follow" --tag LoadNow --dry-run
   ```

   The preview shows the loader line it would write and the settings change.
   Leave the tag off when agents should open the file only when its
   description fits the task, and add `--apply-to "src/**"` to limit it to
   matching work.

2. Apply it, then check what agents read.

   ```sh
   open-forge route include CONTRIBUTING.md --description "Contribution rules to follow" --tag LoadNow
   open-forge context
   ```

   The file stays where it is and nothing is written to it. A Markdown file
   with its own `## Entries` brings its listed files along.

3. Change or remove it later.

   ```sh
   open-forge route include CONTRIBUTING.md --apply-to "src/**"
   open-forge route exclude CONTRIBUTING.md
   ```

   Running Include again changes only the options you pass. Exclude removes the
   line and leaves the file alone.

## Change the installed routes

Use this to add a category, change sharing choices, or restore missing packaged
scaffolding. To restore only one left-out built-in category, use
[`route init <category> --framework`](/guides/cli#route-init), previewed with
`--dry-run` first. `open-forge install --configure` is the interactive way to
change several setup choices.

1. Preview this repeatable change to the current selection.

   ```sh
   open-forge install --configure --preset custom \
     --route guidance=add --route memory/working=git-ignore --dry-run
   ```

   Look for Guidance being added and Working's contents being ignored while its
   entrypoint stays shared. Custom starts from the current choices, so other rows
   keep their selection. On Full Core, Guidance is already present.

2. Apply the reviewed request and check the route and sharing policy.

   ```sh
   open-forge install --configure --preset custom \
     --route guidance=add --route memory/working=git-ignore --automatic
   open-forge route inspect guidance
   git diff
   ```

   Check `.agents/guidance/_guidance.md` and the Working rules in `.gitignore`.
   Index leaves private child files out of shared Entries. Find and explicit
   Context requests can still read them locally. Files already tracked by Git
   stay tracked.

Custom Remove omits supplied defaults and releases their Framework management.
Existing files, notes, and overwrite companions remain routable. Explicit
configuration can restore eligible missing defaults after a checkout, with or
without a lock file, while preserving authored files and narrower omissions.
Missing private notes need your own copy or backup. See
[installation](../getting-started/installation.md#configure-an-existing-workspace)
and [setup choices](/guides/cli#setup-choices).

## Choose an Extension when you need one

Use this for Decisions, project documents, or development workflows. Start from
an installed base Framework. This example adds `development-toolkit`, which
includes Project Documents, Planning, Flows and Scenarios, Development, and their
Workflow Support dependency. The [catalogue](../extensions/index.md) explains
each package.

1. Restore Templates before installing the toolkit on Essentials.

   ```sh
   open-forge route init templates --framework --dry-run
   ```

   Review the restoration plan, then apply it. On Full Core, this can be a no-op.

   ```sh
   open-forge route init templates --framework
   ```

2. Discover the package and preview its installation.

   ```sh
   open-forge extension list --available
   open-forge extension inspect development-toolkit
   open-forge extension install development-toolkit --dry-run
   ```

   Review selected packages, dependencies, destinations, and file changes.

3. Apply and verify the supplied routes and Template.

   ```sh
   open-forge extension install development-toolkit
   open-forge extension list --installed
   open-forge find --tag Template
   open-forge doctor
   ```

   Continue when `templates/planning/decision` is listed and the Documents and
   Decisions entrypoints exist under `.agents/memory/crystallized/`.

You can choose a smaller package, such as `planning`, instead. Restore Templates
before its installation too. With no IDs, interactive Extension Install offers
a package picker. For unattended apply, supply the IDs and `--automatic` after
reviewing the preview. `--source <path>` selects a separate local package or
catalogue. If Planning is already installed without its starters, restore
Templates first, then preview and apply its update using the
[restore and update procedure](../extensions/index.md#if-your-setup-leaves-out-templates). See
[Extension Install](/guides/cli#extension-install).

## Find out why an agent ignored a rule

Use this when a written rule seems absent from the agent's context. The example
assumes `directives/frontend/components` is an existing rule you wrote.

1. Compare startup context with the rule's conditions.

   ```sh
   open-forge context --for src/Order.cs --for web/order.ts
   open-forge route inspect directives/frontend/components \
     --for src/Order.cs --for web/order.ts
   open-forge find --for web/order.ts --tag Directive
   ```

   Include every working file, including planned files. One supplied path must
   satisfy the source's whole inherited condition chain. `applyTo` only filters,
   so a matching rule also needs a loading tag or explicit selection. A missing
   `LoadNow` tag or an unselected scope is a common cause.

2. Load the scope you selected and verify that the rule is returned.

   ```sh
   open-forge context directives/frontend/components \
     --for src/Order.cs --for web/order.ts --additions-only
   open-forge route inspect directives/frontend/components --matching-files
   ```

   The command shows what the rules select, not what a model actually read.
   Inspect can explain a nonmatch without activating its automatic children.
   Matching-files lists current matches without supplying working paths. See
   [scan scope and completeness](/guides/cli#current-matching-files).

3. If metadata is wrong, preview a correction with
   [`route update`](/guides/cli#route-update), review it, then apply the same
   request without `--dry-run`. Check Context again with the complete file set.
   Preserve unrelated tags and conditions when choosing the patch.

Without known paths, exposed conditioned `LoadNow` and `KeepInMind` entries stay
pending. Context returns incomplete, exit `3`. Other conditioned entries stay on
demand. Neither command globally selects hidden ancestors or discovers code
dependencies. Add related caller or test paths when they are part of the work.
A file condition controls applicability, not permission to edit related files.

## Record something new

Use this after making a decision or finding a convention worth keeping. This
example starts from the base Framework and uses Planning's Decision Template.

1. Make the Template available before installing Planning.

   ```sh
   open-forge route init templates --framework --dry-run
   ```

   Review the plan, then restore the category.

   ```sh
   open-forge route init templates --framework
   open-forge extension install planning --dry-run
   ```

   Review the package plan, then apply it.

   ```sh
   open-forge extension install planning
   open-forge find --tag Template
   ```

   Continue when `templates/planning/decision` is listed. If Planning is already
   installed without its starters, use the
   [restore and update procedure](../extensions/index.md#if-your-setup-leaves-out-templates).

2. Search for an existing answer and preview a new record if needed.

   ```sh
   open-forge find --tag Decision
   open-forge route create memory/crystallized/decisions/money-in-cents \
     --template templates/planning/decision \
     --description "Every amount is a whole number of cents" \
     --tag Memory --tag Decision --dry-run
   ```

   If a Decision already covers this question, update it instead. Otherwise,
   review the new file, its metadata, and its parent Entries change.

3. Apply the reviewed creation, then open and fill in the new file's body.

   ```sh
   open-forge route create memory/crystallized/decisions/money-in-cents \
     --template templates/planning/decision \
     --description "Every amount is a whole number of cents" \
     --tag Memory --tag Decision
   open-forge doctor
   ```

   Check `.agents/memory/crystallized/decisions/money-in-cents.md` and its entry
   in `_decisions.md`. The Template supplies starting content. The new record
   is maintained independently, giving agents one current answer to find.

## Reorganize without breaking links

Use this when a file belongs in a narrower scope. Here `billing` is a Document
you wrote in the Documents category supplied by Project Documents. If the category
is missing, follow the reviewed Extension flow with `project-documents` as the
package ID. Restore Templates before installing it on Essentials too, since its
workflow guides link to the supplied Document Templates.

1. Inspect incoming links and preview the destination scope.

   ```sh
   open-forge references memory/crystallized/documents/billing --direction in
   open-forge route init memory/crystallized/documents/payments --dry-run
   ```

   Review the scope plan, then create it.

   ```sh
   open-forge route init memory/crystallized/documents/payments
   open-forge route move memory/crystallized/documents/billing \
     .agents/memory/crystallized/documents/payments/billing.md --dry-run
   ```

2. Review the move's links and Entries changes, then apply it.

   ```sh
   open-forge route move memory/crystallized/documents/billing \
     .agents/memory/crystallized/documents/payments/billing.md
   open-forge doctor
   ```

   Check that the old Billing path is absent, the new file exists, and incoming
   links point to it. The destination parent must exist before planning a move.
   Route Move refuses files installed by the Framework or an Extension.

To delete instead, preview and review [`route remove`](/guides/cli#route-remove),
then apply the same request. It records the removal and turns supported incoming
links into plain text. Move and Remove scan supported workspace Markdown, retain
tracked-file coverage, and include private `.agents` routes. Ignored files outside
`.agents` stay unchanged. See the [reference scan rules](/guides/cli#reference-scans-for-moves-and-removals)
for Git exclusions, fallback behavior, and debug counts.

## Keep the workspace healthy

Use this after manual edits, a merge, or a Status warning. When Git retains the old file's committed content, Index and Repair work in either order. Without Git, run Repair first while old Entries still record the file's metadata.

1. Diagnose the current workspace and preview safe repairs.

   ```sh
   open-forge status
   open-forge doctor
   open-forge repair --automatic --dry-run
   ```

   Review the selected link corrections and any unresolved findings.

2. Apply the reviewed safe repairs, then preview navigation changes.

   ```sh
   open-forge repair --automatic
   open-forge index --dry-run
   ```

   Review missing metadata, new entrypoints, and Entries changes. Automatic Repair selects only currently
   safe corrections. To choose a target yourself, use interactive Repair or
   the [explicit relink procedure](/guides/cli#repair) as a separate reviewed task.
   When Repair cannot ask in the current session, it lists copyable
   `--relink ... --automatic` commands for the possible targets.
   Interactive Repair explains candidate targets and lets you leave a link
   unchanged. Remaining broken links stay in the result. For Library recovery,
   choose one step or leave all recovery steps out, then review the plan.

3. Apply the navigation plan and verify the result.

   ```sh
   open-forge index
   open-forge doctor
   ```

   Check the corrected destinations and rebuilt Entries. Repair refreshes navigation affected by its corrections. Index completes unrelated new notes and folders. Follow any reported target choice or manual edit before checking Doctor again. See [After you edit files by hand](/guides/cli#after-you-edit-files-by-hand) for the full procedure. Agents use Entries to discover files. Keep
   shared Entries current after manual edits. Private routes intentionally omit
   their child files from shared Entries. Find can still discover those local files.

Repair keeps managed files protected. Follow their reported managing Update or authoring action. A partial result states what changed and what remains. Copyable commands retain the selected workspace. A Git timeout asks you to retry before editing a possibly renamed link.

## Take a Framework update

Upgrade the CLI first when you want a newer bundled Framework.

1. Preserve your current files with version control or a separate backup, then
   preview the update.

   ```sh
   git status
   open-forge update --dry-run
   ```

   Review every replaced or restored file. Update replaces edited managed files
   and retains their previous bytes in recovery data. Keep lasting customization
   in an [overwrite companion](../concepts/customizing.md#overwrite-companions),
   which Update does not touch. Compatible Skill metadata and navigation additions
   preserve existing Skill content and keep those files user-owned.

2. Apply the reviewed update and inspect the files and recovery location.

   ```sh
   open-forge update
   git diff
   open-forge status
   ```

   Check the changed files and the reported recovery bundle before deleting it.
   The bundle contains previous bytes. `git diff` compares tracked changes with
   Git's saved content and cannot recover unsaved or untracked content. Keep a
   separate backup of prior changes you need to preserve.

3. Preview cleanup only after checking the update.

   ```sh
   open-forge cleanup --dry-run
   ```

   Review the exact recovery bundles and drafts selected for deletion, then apply.

   ```sh
   open-forge cleanup
   open-forge status
   ```

   Verify the selected recovery copies are gone. Cleanup applies directly.

## Drop what you don't use

Use this when a default category or Extension no longer fits. A CLI removal
records an exclusion, so later updates keep it removed.

1. Preview the chosen removal. This example removes Templates.

   ```sh
   open-forge remove .agents/templates --dry-run
   ```

   Review deleted files, detached links, and the exclusion in
   `.agents/open-forge.json`. On Essentials, an already omitted category may
   need no file deletion.

2. Apply and check the removed category and settings.

   ```sh
   open-forge remove .agents/templates
   open-forge status
   ```

   Verify Templates is absent and its removal is recorded. To remove an installed
   Extension instead, preview `open-forge remove orchestration --kind extension --dry-run`,
   review it, then apply without `--dry-run`.

Removing `development-toolkit` leaves its dependency packages installed. Apply
that removal before previewing further dependency removals. An Extension Remove
preview's next command applies the selected removal. The applied result supplies commands for
the dependencies. Preview each further removal before applying it. A
dependency cannot be removed while a retained package needs it. See
[Extension Remove](/guides/cli#extension-remove).

## Share rules across repositories

Use this when repositories share team rules or a knowledge base. Keep the shared
files in a contained folder, such as a Git submodule. This example assumes
`vendor/team-rules/.agents/guidance/team/review.md` is a reviewed source file.
Its destination needs an existing route.

1. Restore Guidance first if Essentials left it out.

   ```sh
   open-forge route init guidance --framework --dry-run
   ```

   Review, then apply the restoration. Full Core may need no changes.

   ```sh
   open-forge route init guidance --framework
   open-forge route init guidance/team --dry-run
   ```

   Review the scope plan, then create it and preview attachment.

   ```sh
   open-forge route init guidance/team
   open-forge library attach team-rules vendor/team-rules \
     --git-ignore true --allow-path .gitignore --dry-run
   ```

2. Review each projected link and ignore-file change, then attach the folder.

   ```sh
   open-forge library attach team-rules vendor/team-rules \
     --git-ignore true --allow-path .gitignore --automatic
   open-forge library inspect team-rules
   ```

   Check the relative link at `.agents/guidance/team/review.md` and its Entries
   listing. Source files stay in the shared folder. This example keeps local
   links out of ordinary Git commits because the source folder is the versioned
   copy. Interactive Attach offers that choice with No as the default. Use
   `--git-ignore false` to leave ignore rules unchanged.

3. After adding or removing a source file, preview and review synchronization.

   ```sh
   open-forge library sync team-rules --dry-run
   ```

   Apply the reviewed plan and check the links.

   ```sh
   open-forge library sync team-rules
   open-forge library inspect team-rules
   ```

   Sync remembers the sharing choice and updates exact owned ignore rules as
   files come and go. Already committed links stay tracked.

4. To stop sharing, preview Detach, review its link removals, then apply it.

   ```sh
   open-forge library detach team-rules --dry-run
   ```

   ```sh
   open-forge library detach team-rules
   open-forge library list
   ```

   Check that projected links and registration are gone and the shared source
   files remain. Authored ignore rules and other Libraries survive. Each projected
   file has its own ignore rule, so unrelated files in the destination stay visible.

These commands need `.gitignore` permission when changing it. The explicit Attach
grant above is saved for later operations. Add destination grants when files land
outside `.agents`. Library Detach and removal release their owned ignore rules.
See [Library permissions](/guides/cli#keep-projected-links-out-of-git).

If an interactive operation asks to change paths outside `.agents`, key mode
starts focused on `Allow once`. Press Enter to approve only this run, or
deliberately choose `1. Allow always` to save the displayed grant. Line mode
accepts `always`, `once`, or `cancel`, and an empty line cancels.

On Windows, Inspect, List, Doctor, or Sync may report unreadable legacy links
whose on-disk targets use forward slashes. Follow the reported detach command,
remove only that Library ID from `removedLibraries` in `.agents/open-forge.json`,
then use the reported attach command with the same options. Preview and review
each operation first. Source files stay intact. See
[recreate legacy Windows links](/guides/cli#recreate-legacy-windows-links).

## Automate it

Use this to check workspace health in CI or another tool.

1. Run a read-only diagnosis with structured output.

   ```sh
   open-forge doctor --format json
   ```

2. Check its exit code and inspect findings before using the result. `0` means
   completed. A nonzero code needs attention, so CI can fail on warnings or errors
   such as broken links and stale Entries. JSON returns one structured result.
   See [status and exit codes](/guides/cli#status-and-exit-codes).

Next: [Command reference](/guides/cli#command-index).

## Work in separate worktrees

Each directory has its own `.open-forge-state/` lock and recovery store. A sandbox needs workspace write access. A leftover lock file does not mean a command is running.

Use `open-forge status --detail full` or `open-forge doctor --detail full` to inspect workspace and legacy recovery stores. Preview `open-forge cleanup --dry-run` before removing evidence. Eligible local cleanup can proceed when the legacy store is unavailable. See [worktrees and sandboxes](/guides/cli#worktrees-sandboxes-and-the-runtime-folder).
