---
title: Installation
description: Install Open Forge, choose the routes you need, and change that selection later.
---

# Installation

Setup has three steps: install the CLI, use it to install the Framework into your project, then review and commit the result. The CLI is optional. If you'd rather not use it, skip to [installing the Framework manually](#manually-from-a-clone).

## Quick start

With Node.js 22.18 or later, run:

```sh
npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install
```

The interactive install offers Essentials, Full Core, or Custom, shows the selected plan, and asks before applying it. For a smaller start, choose Essentials. Check the result with `git status` and `git diff`, including the new files, and commit it.

Continue with the [ten-minute guide](ten-minute-guide.md) to try a task, add a rule, and save a useful fact. Installing is a setup step. Your agent doesn't repeat it at startup. The base works without Extensions or a chosen workflow.

## Choose the installed routes

The first interactive install offers three choices:

| Choice     | What it supplies                                                                                                         |
| ---------- | ------------------------------------------------------------------------------------------------------------------------ |
| Essentials | Directives, Patterns, Skills, Emerging and Crystallized Memory, plus Working Memory with its whole directory Git-ignored |
| Full Core  | Every built-in category and Memory state, with no Install-owned Git-ignore entries                                       |
| Custom     | Choose Add, Remove, or Add + Git-ignore for each built-in category and Memory state                                      |

Essentials omits Guidance, Maps, Templates, and Archived Memory. You can add them later. Full Core includes the base categories, not optional Extensions or an agent runtime.

Git-ignored routes remain ordinary Open Forge content. Agents can find and read them, and `index` still lists them. Git-ignore doesn't untrack files already committed to Git. With Essentials, Working Memory stays available for resuming local work while new records stay out of ordinary Git commits.

Custom starts from your current choices in an existing workspace, or Essentials in a fresh one. **Remove** omits supplied defaults and releases their Framework management. Existing files, notes, and overwrite companions stay in place and remain routable. **Add + Git-ignore** installs the route and ignores its whole directory through an Install-owned section of `.gitignore`. Other Git-ignore rules stay yours.

## Configure an existing workspace

To revisit those choices, run:

```sh
open-forge install --configure
```

The wizard offers the presets, with Custom starting from the current choices, and shows the plan before confirmation. It preserves authored files and narrower omissions. It can also restore eligible missing packaged defaults, such as ignored Working scaffolding after a checkout, whether or not the lock file is present. Missing private notes need your own copy or backup.

For a repeatable preview or unattended setup, name the preset explicitly:

```sh
open-forge install --configure --preset essentials --dry-run
open-forge install --configure --preset essentials --automatic
```

Dry-run, automatic, JSON, and redirected requests never ask setup questions. Noninteractive `--configure` requires `--preset`. An ordinary unattended first install keeps Full Core and existing omissions. See the [CLI reference](/guides/cli#setup-choices) for Custom row flags and exact combinations.

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

Full Core supplies 15 Markdown files: `AGENTS.md`, the `CLAUDE.md` bridge, the loader, one entrypoint for each Core category and Memory state, and the `open-forge-cli` Skill. Essentials selects fewer routes, and Custom follows your selection. Records such as Decisions and Checkpoints, and workflow recipes, come from optional [Extensions](#add-extensions-optional) or from you.

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

Naming the preset previews that exact selection without setup questions. The second command shows the plan and asks before it writes. Use `full-core` instead when you want every built-in route. An explicit preset in an already installed workspace also needs `--configure`.

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
blocked-write behavior. `index` only rebuilds generated navigation and does
not convert these hosts.

## 3. Review and commit

Whichever route you took, review the result with `git status` and `git diff`, then commit it. A fresh install adds new files, which `git status` lists and `git diff` alone doesn't show. That's the whole setup.

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
