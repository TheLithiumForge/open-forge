---
title: Working with the CLI
description: What the open-forge CLI is for, the habits that make it safe, and which command helps with which job.
---

# Working with the CLI

The Framework is plain Markdown, and you can do everything by hand. The CLI covers the parts that are tedious or easy to get subtly wrong by hand: seeing exactly what the rules load, keeping navigation in step with the files, and changing many files safely. This page explains what each command is for. [Everyday flows](flows.md) shows how they fit together, and the [command reference](/guides/cli) lists every option. The [glossary](../glossary.md) defines terms such as route, scope, entrypoint, and `Entries`.

## Three habits

**Preview first.** Every command that changes files accepts `--dry-run`. It reports the plan (what the command would create, replace, move, or delete) and writes nothing. Some previews summarize new files as a count, and `--detail standard` lists each one. Nothing changes until you run the command again without the flag.

**Check the status.** Every result has one status: `completed`, `completed-with-warnings`, `incomplete`, `blocked`, or one of a few others. A warning isn't a failure, and `blocked` means the CLI stopped at a boundary instead of guessing. The first line of a text result summarizes the outcome, and each status has its own exit code, so scripts can tell them apart too.

**Follow the next step.** When something needs attention, the result says what to do next, often as the exact command to run. Do that, then run the original command again so it starts from the workspace's current state.

## Which command for which job

### Inspect the workspace

These commands change nothing.

| Command         | Use it to                                                                                                 |
| --------------- | --------------------------------------------------------------------------------------------------------- |
| `status`        | Get a one-screen summary: startup context, navigation, installed packages, anything needing attention     |
| `context`       | See startup or selected context, filtered by repeated `--for <path>` values                               |
| `route list`    | See how the workspace is organized, one level or the whole tree                                           |
| `route inspect` | Explain a source's route and how supplied paths match its declared and inherited conditions               |
| `find`          | Find by tag or heading, with an additional path filter through `--for`                                     |
| `references`    | See what links to a file and what it links to, before you move or delete it                               |

### Add and organize knowledge

| Command        | Use it to                                                                                                   |
| -------------- | ----------------------------------------------------------------------------------------------------------- |
| `route init`   | Create scope folders and entrypoints; metadata applies only to a missing generic final entrypoint           |
| `route create` | Add a file with optional Template or repeated `--apply-to`; update its parent's `Entries`                   |
| `route update` | Change metadata or file conditions, including clearing `applyTo`                                           |
| `route move`   | Move a file or a category you created and update the links that point to it. Installed files can't be moved |
| `route remove` | Remove a file or a category and remember that you removed it                                                |

### Check and repair

| Command  | Use it to                                                                                      |
| -------- | ---------------------------------------------------------------------------------------------- |
| `index`  | Rebuild the generated `Entries` after you add, rename, or retag files by hand                  |
| `doctor` | Diagnose broken links, stale navigation, and lifecycle problems, without changing anything     |
| `repair` | Apply the safe fixes for broken local links, and finish recovering interrupted Library changes |

### Install, update, and remove

| Command   | Use it to                                                                                         |
| --------- | ------------------------------------------------------------------------------------------------- |
| `install` | Put the Framework into a workspace, and record what was installed so it can be updated later      |
| `update`  | Bring Framework files to the version bundled with the CLI. Every file it replaces is listed first |
| `remove`  | Remove a file, folder, route, package, or Library, and keep it removed through later updates      |
| `cleanup` | Delete the recovery copies that updates and removals keep, once you've checked the result         |

### Extensions and Libraries

Extensions are optional packages of files. A Library is a folder of shared files, such as a team's rules, that the CLI links into the workspace.

| Command group                                               | Use it to                                                                     |
| ----------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `extension list`, `extension inspect`                       | See which packages exist, what each contains, and what it depends on          |
| `extension install`, `extension update`, `extension remove` | Manage optional packages, with the same previews and records as the Framework |
| `extension create`                                          | Start your own package, ready for a catalogue                                 |
| `library attach`, `library sync`, `library detach`          | Link one shared set of files, such as a team's rules, into this workspace     |
| `library list`, `library inspect`                           | See which Libraries are attached and whether their links are current          |

## For agents and scripts

**Your agent can use it too.** The loader lists the main commands, so an agent
with the CLI available can run `context` or `find` instead of opening files one
by one. Repeat `context --for <path>` for every file in the task, including
planned files. If paths are unknown, the conditions of tagged entries stay
pending and `context` returns an incomplete result with exit code `3`. The CLI does not
infer paths from Git or discover code dependencies. A file condition does not
select hidden ancestors or control edit permission.

The base install also ships one Skill, `open-forge-cli`, which gives the agent
this page's advice: which command fits which job, how to preview with
`--dry-run`, and how to read the status. At startup the agent sees only the
Skill's one-line entry in the Skills entrypoint. The Skill file itself opens on
demand, when a task needs the CLI.

**Scripts and CI get structured output.** Add `--format json` for one machine-readable result, and check the exit code. `0` means completed and `2` means completed with warnings. Every other code means the result is incomplete, the input was invalid, or the command failed, was blocked, or was cancelled. The [command reference](/guides/cli#status-and-exit-codes) lists each code. `open-forge doctor` exits with a nonzero code when it finds a warning or an error, so running it in CI catches a broken link or stale navigation before it reaches an agent. A command that asks for confirmation, such as `install`, `update`, or `remove`, needs `--automatic` when nobody is there to answer.

**Every command works on the current folder** unless you pass `--workspace <path>`. Add `--detail standard` or `--detail full` to any command for reasons and evidence.

Next: [Everyday flows](flows.md).
