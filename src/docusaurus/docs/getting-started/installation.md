---
title: Installation
description: Install Open Forge in your project, review its changes, and try it with your agent.
---

import Tabs from "@theme/Tabs"
import TabItem from "@theme/TabItem"

# Installation

Setup has three steps: install the CLI, use it to install the Framework into your project, then review and commit the result. The CLI is optional. If you'd rather not use it, skip to [installing the Framework manually](#manually-from-a-clone).

## Quick start

With Node.js 22.18 or later, run:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

For a first try, choose the recommended Essentials setup. Keep Root keys unless your workspace already declares another metadata form. Before asking for confirmation, the plan shows your setup: the folders it adds, the ones whose contents stay out of Git, the ones it leaves out, and the metadata form. Check the result with `git status` and `git diff`, including the new files, before committing it.

Already have `AGENTS.md` or `CLAUDE.md`? Install adds a marked Open Forge section and keeps your existing text. It also creates the selected files under `.agents/`. To experiment without touching your checkout, [try it in a separate clone](#try-it-and-undo-a-trial).

Continue with the [ten-minute guide](ten-minute-guide.md) to try a task, add a rule, and save a useful fact. Installing is a setup step. Your agent doesn't repeat it at startup. The base works without Extensions or a chosen workflow.

## Try it and undo a trial

Save your current work before installation. Use a Git commit or a separate backup for tracked changes, and copy untracked or ignored files you need to preserve. Keep the install plan and compare the result with that baseline using `git status` and `git diff`.

For a trial, clone your local repository into a separate folder and install there. The clone contains committed files. Copy any additional local files needed to run the project, then use the quick-start commands in the clone. Your original checkout stays unchanged.

To undo an installation in the original checkout, compare the install plan with your saved baseline. Restore the tracked files changed by the trial and remove only the files it added. Keep any work you want before removing trial files. Do not delete an existing `.agents/` tree wholesale. Git cannot recover files or changes you never saved.

For individual managed-file removals or conflicts, read the [Install reference](/guides/cli#install) and [Remove reference](/guides/cli#remove).

## Choose the installed routes

The first interactive install offers three choices:

| Choice     | What it supplies                                                                                                                  |
| ---------- | --------------------------------------------------------------------------------------------------------------------------------- |
| Essentials | Directives, Patterns, Skills, Emerging and Crystallized Memory, plus Working Memory with private contents and a shared entrypoint |
| Full Core  | Every built-in category and Memory state, with no Install-owned Git-ignore entries                                                |
| Custom     | One list of every built-in category and Memory state, where you mark each one to add, add and Git-ignore, or leave out            |

Essentials omits Guidance, Maps, Templates, and Archived Memory. You can add them later. Full Core includes the base categories, not optional Extensions or an agent runtime.

Essentials keeps new Working records out of ordinary Git commits and leaves `_working.md` eligible for sharing. Already tracked files stay tracked. Generated `Entries` omit private contents. You can find and read local records with the CLI or your normal file tools. Commit the entrypoint and `.agents/open-forge*.json` files to share the route and its settings. The same sharing policy applies to any Custom route marked `~`. Git-ignore controls ordinary sharing, not local access.

Custom shows one list, starting from your current choices in an existing workspace, or Essentials in a fresh one. Each row names a category, marks its action, and explains what it supplies. The focused detail explains what the action does.

Press `+`, `~` or `-` to set the focused row, or space to move it to the next mark, then press Enter. Without key input, the numbered list explains each choice. Type edits such as `2+ 9~` and press Enter on an empty line to continue. It says `Type cancel to stop without changes.` Cancelling before application leaves files and settings unchanged.

- **`-` leave out** omits supplied defaults and releases their Framework management. Existing files, notes, and overwrite companions stay in place and remain routable.
- **`~` add, keep contents out of Git** installs the route, records its sharing policy, then ignores its contents with an exception for its entrypoint. Other Git-ignore rules stay yours.

The plan that follows says what happens to each existing file, such as `Entries would be updated` or `settings would be updated`. Only a whole-file swap says `replaced`.

## Choose the frontmatter form

Choose root fields or an `open-forge:` mapping for your workspace. Both are readable in every workspace. New files follow `.agents/open-forge.json`. A missing `frontmatter` setting means scoped.

<Tabs groupId="frontmatter-form">
<TabItem value="root" label="Root" default>

```yaml
---
description: C# rules
tags: [Directive, LoadNow]
applyTo: ["**/*.cs"]
---
```

</TabItem>
<TabItem value="scoped" label="Scoped">

```yaml
---
open-forge:
  description: C# rules
  tags: [Directive, LoadNow]
  applyTo: ["**/*.cs"]
---
```

</TabItem>
</Tabs>

Root keys are the recommended first choice unless settings already declare another preference. Choose the scoped form when other tools already use top-level frontmatter keys in your files. Both forms remain readable. `--frontmatter root` or `--frontmatter scoped` supplies the choice without its question. The [CLI reference](/guides/cli#setup-choices) defines metadata precedence and unattended combinations, and [metadata and file conditions](/guides/cli#frontmatter-and-metadata) explains how fields are read.

## Configure an existing workspace

To change an installed setup, follow [Change your setup choices](grow-your-framework.md#change-your-setup-choices). The how-to covers the wizard, changing only the metadata form, and previewing a preset change.

<details>
<summary>Other installation methods and existing-workspace details</summary>

## 1. Install the CLI

The routes below go from least to most setup.

### From npm

Needs Node.js 22.18 or later.

```sh
npm install -g @thelithiumforge/open-forge@beta
```

### From a GitHub release

No Node.js needed. Download `open-forge-<version>-<platform>.tar.gz` from the [releases page](https://github.com/TheLithiumForge/open-forge/releases). The platform is one of `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`, `win-x64`, or `win-arm64`.

Check the archive against the release's `SHA256SUMS`, then extract it. On Linux, for example:

```sh
sha256sum --check --ignore-missing SHA256SUMS
tar -xzf open-forge-<version>-linux-x64.tar.gz
```

The archive holds a single `open-forge` executable (`open-forge.exe` on Windows) and its license. Put it on your `PATH`, or keep it in your repository (for example as `tools/open-forge`) and run it as `./tools/open-forge`.

### From source

Clone the repository and follow the [local setup guide](/guides/development#link-the-native-cli-locally) to build and link the CLI.

## 2. Install the Framework

Full Core supplies 19 Markdown files: `AGENTS.md`, the `CLAUDE.md` bridge, the loader, the Core and Memory entrypoints, the `open-forge-cli` Skill, and its four supporting guides. The guides are `discovery.md`, `routes.md`, `packages.md`, and `common.md` under `.agents/skills/open-forge-cli/references/`, opened when needed. Essentials selects fewer routes, and Custom follows your selection. Records such as Decisions and Checkpoints, and workflow recipes, come from optional [Extensions](#add-extensions-optional) or from you.

The CLI records what it manages so it can update those files later. A manual copy supplies Full Core, which you can adapt by hand.

New installations use visible managed boundaries in `AGENTS.md` and
`CLAUDE.md`. Keep your own instructions before or after the managed section.
For example:

```md
# Project instructions

Keep project-specific instructions here.

# Open Forge

Before starting a task, read `.agents/loader.md`.
Use it to select every relevant scope, including nested scopes.
Follow the loaded rules throughout the task.

**End of Open Forge managed section.**

# More project instructions

Keep additional project-specific instructions here.
```

### With the CLI

From your project:

```sh
open-forge install --preset essentials --dry-run
open-forge install --preset essentials
```

Naming the preset previews that exact selection without setup questions. The second command asks about frontmatter unless a flag or declared preference supplies it, then shows the plan and asks before it writes. Use `full-core` instead when you want every built-in route. Add `--frontmatter root` or `--frontmatter scoped` to select the form explicitly. An explicit preset or form in an already installed workspace also needs `--configure`.

An existing category entrypoint does not prevent first installation. If a file
already occupies a bundled path in a selected route, such as `.agents/guidance/_guidance.md`, Install
preserves its authored content in `_guidance.overwrite.md` and installs the
Framework base. Existing overwrite content is kept after it. The base gets
rebuilt navigation, while the overwrite remains yours. An existing native
Skill at a bundled `SKILL.md` path also stays yours. Neither case requires
`--force`.

If your project already has `.agents/`, install can adopt its existing native
Skills by filling missing metadata and adding navigation while preserving Skill
bodies and support files. The plan shows the migration paths. No special
migration flag or `--force` is needed for safe adoption. When the managed
Framework still matches the embedded payload, run `open-forge install` again
after adding a downloaded Skill to fill missing metadata or navigation.

After upgrading the CLI in an already managed project, run
`open-forge update --dry-run` and then plain `open-forge update`. Update
reconciles older or edited Framework content it owns and also adopts compatible
native Skills. It does not establish management, so use the install commands
above for a first install into an unmanaged project, even when `.agents/`
already exists.

### Manually, from a clone

No CLI needed. The Framework is complete in [`src/open-forge`](../../../open-forge/). Clone the repository and copy it into your project:

```sh
git clone https://github.com/TheLithiumForge/open-forge.git
cp -R open-forge/src/open-forge/.agents /path/to/your-project/
cat open-forge/src/open-forge/AGENTS.md >> /path/to/your-project/AGENTS.md
```

If your harness reads `CLAUDE.md`, append `open-forge/src/open-forge/CLAUDE.md` to your project's `CLAUDE.md` as well. If the project already has a `.agents/` folder, merge the two by hand instead of copying over it.

If an existing host still uses the legacy `<!-- open-forge:start -->` and
`<!-- open-forge:end -->` pair, you do not need to migrate it by hand. Run
`open-forge update --dry-run` to preview the conversion, then
`open-forge update --automatic` to apply it under the normal update rules.
Content outside the recognized managed section stays unchanged. See the
[CLI reference](/guides/cli#update) for the complete boundary, recovery, and
blocked-write behavior. `index` maintains routed metadata and navigation and
does not convert these hosts.

## 3. Review and commit

Whichever route you took, review the result with `git status` and `git diff`, then commit it. A fresh install adds new files, which `git status` lists and `git diff` alone doesn't show. That's the whole setup.

The first successful CLI install also explains the next agent step. If your agent tool does not read `AGENTS.md` on its own, ask it to read that file first, then give it a normal task.

Installing and updating are things you do when maintaining the workspace. Agents don't repeat them at startup.

:::tip[Read the files once]

The installed files are short, and they become the instructions your agents are asked to follow. Read them once now, and again after each update. They're also yours: add, adapt, replace, or remove the defaults as your needs change.

:::

</details>

## Add Extensions (optional)

Extensions are optional packages of more files, such as Planning for Decisions and Checkpoints, or Development for review and debugging workflows. Their files carry no loading tags, so at startup an installed Extension adds only its one-line entries to the entrypoints that already load. Once the Framework is in place, preview a package before you install it:

```sh
open-forge extension list --available
open-forge extension install development-toolkit --dry-run
```

The [Extensions](../extensions/index.md) section explains what each package installs and what every file is for.

**Next:** [Ten-minute guide](ten-minute-guide.md).
