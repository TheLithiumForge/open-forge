---
title: Working with the CLI
description: What the open-forge CLI is for, the habits that make it safe, and which command helps with which job.
---

# Working with the CLI

The Framework is plain Markdown, and you can do everything by hand. The CLI exists for the parts that are tedious or easy to get subtly wrong by hand: seeing exactly what an agent will load, keeping navigation in step with the files, and changing many files safely. This page explains what each command is for. [Everyday flows](flows.md) shows how they fit together, and the [command reference](/guides/cli) lists every option.

## Three habits

**Preview first.** Commands that change files accept `--dry-run` and print the complete plan: what it would create, replace, move, or delete. Nothing is written until you run it again without the flag.

**Read the status line.** Every result ends in one status: `completed`, `completed-with-warnings`, `incomplete`, `blocked`, and a few more. A warning isn't a failure, and `blocked` means the CLI refused rather than guessed. Each status has its own exit code, so scripts can tell them apart too.

**Follow the next step.** When something needs attention, the result says what to do next, often as the exact command to run. Do that, then run the original command again so it starts from the workspace's current state.

## Which command for which job

### Look around: nothing changes

| Command         | Use it to                                                                                             |
| --------------- | ----------------------------------------------------------------------------------------------------- |
| `status`        | Get a one-screen summary: startup context, navigation, installed packages, anything needing attention |
| `context`       | See exactly what an agent loads at startup, or what selecting a scope adds                            |
| `route list`    | See how the workspace is organized, one level or the whole tree                                       |
| `route inspect` | Find out why one file loads or doesn't: its route, its scope, and its loading behavior                |
| `find`          | Find files by tag or heading, such as every Decision or every file tagged `Frontend`                  |
| `references`    | See what links to a file and what it links to, before you move or delete it                           |

### Add and organize knowledge

| Command        | Use it to                                                                                                   |
| -------------- | ----------------------------------------------------------------------------------------------------------- |
| `route init`   | Create a scope: a folder and its entrypoint, or a whole missing chain of them                               |
| `route create` | Add a file with correct frontmatter, optionally starting from a Template. Its parent's `Entries` update too |
| `route update` | Change a file's description, tags, or responsibility without hand-editing the frontmatter                   |
| `route move`   | Move a file or a category and update the links that point to it                                             |
| `route remove` | Remove a file or a category and remember that you removed it                                                |

### Keep it healthy

| Command  | Use it to                                                                                      |
| -------- | ---------------------------------------------------------------------------------------------- |
| `index`  | Rebuild the generated `Entries` after you add, rename, or retag files by hand                  |
| `doctor` | Diagnose broken links, stale navigation, and lifecycle problems, without changing anything     |
| `repair` | Apply the safe fixes for broken local links, and finish recovering interrupted Library changes |

### Install, update, and remove

| Command   | Use it to                                                                                    |
| --------- | -------------------------------------------------------------------------------------------- |
| `install` | Put the Framework into a workspace, and record what was installed so it can be updated later |
| `update`  | Bring Framework files to the current version. Every file it replaces is listed first         |
| `remove`  | Remove a file, folder, route, package, or Library, and keep it removed through later updates |
| `cleanup` | Delete the recovery copies that updates and removals keep, once you've checked the result    |

### Extensions and Libraries

| Command group                                               | Use it to                                                                     |
| ----------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `extension list`, `extension inspect`                       | See which packages exist, what each contains, and what it depends on          |
| `extension install`, `extension update`, `extension remove` | Manage optional packages, with the same previews and records as the Framework |
| `extension create`                                          | Start your own package, ready for a catalogue                                 |
| `library attach`, `library sync`, `library detach`          | Link one shared set of files, such as a team's rules, into this workspace     |
| `library list`, `library inspect`                           | See which Libraries are attached and whether their links are current          |

## For agents and scripts

**Your agent can use it too.** The loader lists the main commands, so an agent with the CLI available can run `context` or `find` instead of opening files one by one. The base also ships an `open-forge-cli` Skill with this page's advice in agent form: which command fits which job, previewing with `--dry-run`, and reading the status. It loads only when a task needs it.

**Scripts and CI get structured output.** Add `--format json` for one machine-readable result, and use the exit code: `0` is completed, `2` is completed with warnings, and higher codes mean the result needs attention. `open-forge doctor` exits with code `2` or higher when it finds a warning or worse, so running it in CI catches a broken link or stale navigation before it reaches anyone's agent.

**Every command works on the current folder** unless you pass `--workspace <path>`, and `--detail standard` or `--detail full` add reasons and evidence to any result.

Next: [Everyday flows](flows.md).
