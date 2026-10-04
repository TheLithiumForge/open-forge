---
title: Working with the CLI
description: Start with the everyday commands, see the available flags, and find the full reference when you need it.
---

# Working with the CLI

The `open-forge` command helps you inspect your project's context and maintain its Markdown files. The [ten-minute guide](../getting-started/ten-minute-guide.md) puts the everyday commands into one walkthrough. This page lists the interface, and the [full reference](/guides/cli) defines each command's behavior.

## Start with these commands

| When you want to                               | Run                               |
| ---------------------------------------------- | --------------------------------- |
| Read the tag-selected startup routes           | `open-forge context`              |
| Find a rule                                    | `open-forge find --tag=Directive` |
| Check links and navigation                     | `open-forge doctor`               |
| Preview navigation updates after editing files | `open-forge index --dry-run`      |

The first three only read files. Run `open-forge index` after reviewing its preview to update the generated `Entries`, the lists that help agents find files. You don't need to run these commands before every task.

To discover the interface as you go:

```sh
open-forge --help
open-forge context --help
open-forge extension install --help
```

This page lists the available flags without all their rules. [Everyday flows](flows.md) gives worked examples, and the [command reference](/guides/cli) defines arguments, defaults, combinations, and limitations. Use the help from your installed executable if its version differs from these docs.

## Flags you'll use often

These global options work across commands:

| Flag                         | What it does                                                                                                                                                  |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--workspace <path>`         | Use one exact project directory instead of the current directory                                                                                              |
| `--format <text\|json>`      | Choose readable text or one structured result. Default: `text`                                                                                                |
| `--detail <level>`           | Choose `minimal`, `standard`, `full`, or `debug`. Default: `minimal`. Each level adds reasons and evidence. `debug` also writes diagnostics to standard error |
| `--detail-filter <severity>` | List `error`, `warning`, `info`, or `all` severities. Repeat to combine them. Status and counts stay the same                                                 |
| `--help`                     | Show the selected command's help and exit                                                                                                                     |
| `--version`                  | Show the executable version and exit                                                                                                                          |

Commands that change files also offer `--dry-run`. Commands that prompt accept `--automatic` for noninteractive use. It skips prompts and uses the command's supported automatic choices. It adds no force, prune, or path permission. The command tables below show where each is available.

For example, get more detail about another project:

```sh
open-forge doctor --workspace /path/to/your-project --detail standard
```

## Three habits

**Preview first.** Every command that changes files accepts `--dry-run`. It reports the plan (what the command would create, replace, move, or delete) and writes nothing. Some previews summarize new files as a count, and `--detail standard` lists each one. Nothing changes until you run the command again without the flag.

**Check the status.** Every result has one status: `completed`, `completed-with-warnings`, `incomplete`, `blocked`, or one of a few others. A warning isn't a failure, and `blocked` means the CLI stopped at a boundary instead of guessing. The first line of a text result summarizes the outcome, and each status has its own exit code, so scripts can tell them apart too.

**Follow the next step.** When something needs attention, the result says what to do next, often as the exact command to run. Do that, then run the original command again so it starts from the workspace's current state.

## Which command for which job

### Inspect the workspace

These commands change nothing.

Each row lists the command-specific flags in addition to the global options above. A source is a file or folder exposed by Open Forge's navigation. You can name it by an ID such as `guidance`, or an exact path such as `.agents/guidance/_guidance.md`. See [source references](/guides/cli#source-references) for the grammar.

| Command and purpose                                                                             | Available flags                                                                               |
| ----------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `status`<br />Summarize context, navigation, installed packages, and anything needing attention | Global options only                                                                           |
| `context [<source>...]`<br />Read startup or selected context                                   | `--for`, `--additions-only`, `--content`, `--follow-links`                                    |
| `route list [<source>]`<br />See how the workspace is organized                                 | `--depth=0`, `--depth=1`, or `--depth=all` (any nonnegative integer is valid)                 |
| `route inspect <source>`<br />Explain a source's route and file conditions                      | `--for`, `--matching-files`                                                                   |
| `find`<br />Find sources by tag or heading                                                      | `--tag`, `--heading`, `--require`, `--within`, `--content`, `--include`, `--exclude`, `--for` |
| `references <source>`<br />See direct incoming or outgoing links                                | `--direction`, `--include`, `--exclude`                                                       |

Repeat `--for <path>` for every file the task works on, including planned files. It filters context and search results. It doesn't select a scope by itself. `find` combines tags and headings with `--require=all` (the default) or `--require=any`. `references --direction=in|out|both` defaults to `both`, and its include/exclude filters apply only to incoming links.

For `find`, `--within` selects where tags and headings match: `document`, `frontmatter`, `body`, or `section:<heading>`. `--content` selects what matched sources return: `metadata`, `frontmatter`, `headings`, `body`, or `section:<heading>`. Each accepts one comma-separated list. These regions keep tag and heading searches focused. For arbitrary text, use your editor or a text-search tool.

Check specific paths with `route inspect <source> --for <path>`, or preview the files that match right now with `route inspect <source> --matching-files`. If the full condition chain is unrestricted, the preview answers `all files` without scanning. Otherwise it uses Git's file inventory, and falls back to a `.gitignore`-aware walk of the workspace when Git can't provide one. The [command reference](/guides/cli#current-matching-files) has the details.

### Add and organize knowledge

| Command and purpose                                                                        | Available flags                                                                                           |
| ------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------- |
| `route init <source>`<br />Create a route chain or add an omitted Core category            | `--framework`, `--description`, `--responsibility`, `--tag`, `--apply-to`, `--dry-run`                    |
| `route create <source>`<br />Add a Markdown file and list it in its parent's `Entries`     | `--description`, `--responsibility`, `--tag`, `--apply-to`, `--template`, `--dry-run`                     |
| `route update <source>`<br />Change metadata, file conditions, or an eligible empty body   | `--description`, `--responsibility`, `--tag`, `--apply-to`, `--clear-apply-to`, `--template`, `--dry-run` |
| `route move <source> <path>`<br />Move a file or category you created and update its links | `--dry-run`                                                                                               |
| `route remove <source>`<br />Remove a file or category and record the removal              | `--dry-run`, `--automatic`                                                                                |

Repeat `--tag <tag>` and `--apply-to <glob>` for multiple values. `--template <source>` copies starting content from a source tagged `Template`. `--clear-apply-to` removes a file condition and cannot be combined with `--apply-to`. On `route init`, metadata applies only to a missing generic final entrypoint, and cannot be combined with `--framework`. Installed files can't be moved with `route move`.

To add an omitted canonical Core category, preview `open-forge route init patterns --framework --dry-run`, then run the same command without `--dry-run`. `--framework` restores the selected category's packaged contents. See [growing your framework](../getting-started/grow-your-framework.md#add-an-omitted-category) for scope and preservation rules, or the [full reference](/guides/cli#initialize-a-route-chain).

### Check and repair

| Command and purpose                                                                             | Available flags                                                     |
| ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| `index [<source>...]`<br />Rebuild `Entries` after adding, renaming, or retagging files by hand | `--dry-run`                                                         |
| `doctor`<br />Diagnose broken links, stale navigation, and installation problems                | Global options only                                                 |
| `repair`<br />Fix local links or recover interrupted Library changes                            | `--dry-run`, `--automatic`, `--relink <source> <expected> <target>` |

### Install, update, and remove

| Command and purpose                                                                 | Available flags                                                             |
| ----------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| `install`<br />Install or configure built-in Framework routes and record management | `--configure`, `--preset`, `--route`, `--dry-run`, `--automatic`, `--force` |
| `update`<br />Bring managed Framework files to the version bundled with the CLI     | `--dry-run`, `--automatic`, `--prune`, `--force`                            |
| `remove <target>`<br />Remove a file, folder, route, package, or Library            | `--kind`, `--allow-path`, `--dry-run`, `--automatic`                        |
| `cleanup`<br />Delete recovery copies once you've checked the result                | `--dry-run`                                                                 |

First interactive Install offers Essentials, Full Core, or Custom. Use `install --configure` to revisit an existing setup. `--preset essentials|full-core|custom` selects it explicitly. With Custom, repeat `--route <id>=<add|remove|git-ignore>` to override rows. Remove keeps existing content and releases only its Framework management. Git-ignored routes still load and don't untrack files already in Git. [Installation](../getting-started/installation.md#choose-the-installed-routes) compares the choices, and the [reference](/guides/cli#setup-choices) lists IDs and combinations.

Noninteractive Configure requires a preset. Dry-run, automatic, JSON, and redirected requests never ask setup questions. Ordinary unattended first Install keeps Full Core and existing omissions. Explicit configuration restores eligible missing defaults while preserving authored files and narrower omissions. It cannot recover missing private notes.

Upgrade the CLI first when you want a newer bundled Framework. Ordinary `update` replaces edited managed files and keeps their previous content in a recovery bundle. `--prune` allows eligible retired files to be deleted. `--force` is accepted by update but adds no authority. On install, it allows eligible existing files to be replaced. Review these plans before applying them.

### Extensions and Libraries

Extensions are optional packages of files. A Library is a folder of shared files, such as a team's rules, that the CLI links into the workspace.

| Command and purpose                                                               | Available flags                                                                                      |
| --------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `extension list`<br />List installed or available packages                        | `--installed`, `--available`, `--source`                                                             |
| `extension inspect <id>`<br />See a package's files and dependencies              | `--source`                                                                                           |
| `extension install [<id>...]`<br />Install optional packages                      | `--source`, `--all`, `--force`, `--allow-path`, `--dry-run`, `--automatic`                           |
| `extension update [<id>...]`<br />Reconcile installed packages                    | `--source`, `--all`, `--force`, `--prune`, `--allow-path`, `--dry-run`, `--automatic`                |
| `extension remove [<id>...]`<br />Remove installed packages                       | `--allow-path`, `--dry-run`, `--automatic`                                                           |
| `extension create [<id>]`<br />Start your own package                             | `--path`, `--name`, `--description`, `--package-version`, `--dependency`, `--dry-run`, `--automatic` |
| `library list`<br />List attached Libraries                                       | Global options only                                                                                  |
| `library inspect <id>`<br />Inspect a Library's files and links                   | Global options only                                                                                  |
| `library attach <id> <source-root>`<br />Link a shared folder into this workspace | `--to`, `--git-ignore <true\|false>`, `--allow-path`, `--dry-run`, `--automatic`                     |
| `library sync <id>`<br />Refresh a Library's links                                | `--allow-path`, `--dry-run`, `--automatic`                                                           |
| `library detach <id>`<br />Remove links and registration, keeping source files    | `--allow-path`, `--dry-run`, `--automatic`                                                           |

`--source <path>` selects one local package or catalogue. `--all` selects all eligible packages from that source. With no package IDs, interactive install, update, or remove offers a selection prompt. For a script, supply IDs or the supported `--all` flag as well as `--automatic`.

`extension create` needs an ID and `--path <catalogue-path>`. Repeat `--dependency <id>` for dependencies. `--allow-path <path>` grants permission for writes outside `.agents`, where supported. `library attach --to <directory>` chooses the projection directory. Extension update follows the same force and prune rules as Framework update.

Interactive Library Attach also asks whether to Git-ignore its projected links,
defaulting to No. Use `--git-ignore true` to keep those local links out of ordinary
Git commits, or `--git-ignore false` to skip the question and leave ignore rules
unchanged. Automatic, JSON, redirected and dry-run omission means false. When
opting in unattended, grant `.gitignore` with `--allow-path .gitignore` if needed.
The rules cover exact links, including root destinations and shared folders.
Sync maintains them; removal releases owned entries while preserving authored
rules and other Libraries. The [Library flow](flows.md#share-rules-across-repositories)
shows the choice, and the [reference](/guides/cli#keep-projected-links-out-of-git)
defines permission and cleanup details.

## For agents and scripts

**Your agent can use it too.** The loader lists the main commands, so an agent
with the CLI available can run `context` or `find` instead of opening files one
by one. Repeat `context --for <path>` for every file in the task, including
planned files. If paths are unknown, any conditioned `#LoadNow` or `#KeepInMind`
entry that a loaded parent exposes stays pending, and `context` returns an
incomplete result with exit code `3`. Other conditioned entries stay on demand.
The CLI does not infer paths from Git or discover code dependencies. A file
condition does not select hidden ancestors or control edit permission.

Selecting Skills ships one Skill, `open-forge-cli`, and the Skills entrypoint
requires reading it at startup. It lists every command's flags, explains when
to use them, and recommends batching entrypoint reads with `context`.
Loading the guide leaves CLI installation and Skill invocation to your runtime.

For an initial batch that includes the guide, run
`open-forge context skills/open-forge-cli`. The default output includes
frontmatter and body. Add all known working paths with repeated `--for`.
Each batch includes the selected sources' ancestor entrypoints and required
child context. Once startup is loaded, batch the relevant source IDs with
`--additions-only` to avoid repeating the startup set. It requires an explicit
source and removes only startup context, not other sources read earlier.
Plain `context` follows loading tags. Explicit read instructions in returned
files still apply.
Use `--follow-links=1` when a selected source's linked explanations are useful.

**Scripts and CI get structured output.** Add `--format json` for one machine-readable result, and check the exit code. `0` means completed and `2` means completed with warnings. Every other code means the result is incomplete, the input was invalid, or the command failed, was blocked, or was cancelled. The [command reference](/guides/cli#status-and-exit-codes) lists each code. `open-forge doctor` exits with a nonzero code when it finds a warning or an error, so running it in CI catches a broken link or stale navigation before it reaches an agent. A command that asks for confirmation, such as `install`, `update`, or `remove`, needs `--automatic` when nobody is there to answer.

**Every command works on the current folder** unless you pass `--workspace <path>`. Add `--detail standard` or `--detail full` to any command for reasons and evidence.

Next: [Everyday flows](flows.md).
