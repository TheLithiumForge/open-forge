---
title: Installation
description: Install the Open Forge CLI, then use it (or a plain copy) to add the Framework to your project.
---

# Installation

Setup has two steps: install the CLI, then use it to install the Framework into your project. The CLI is optional. If you'd rather not use it, skip to [installing the Framework manually](#manually-from-a-clone).

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

Both routes install the same Framework files. The CLI also records what it installed, so it can update those files later.

### With the CLI

From your project:

```sh
open-forge install --dry-run
open-forge install
```

The dry run lists what will be written. Nothing changes until you run the second command.

### Manually, from a clone

No CLI needed. The Framework is complete in [`src/open-forge`](../../../open-forge/). Clone the repository and copy it into your project:

```sh
git clone https://github.com/TheLithiumForge/open-forge.git
cp -R open-forge/src/open-forge/.agents /path/to/your-project/
cat open-forge/src/open-forge/AGENTS.md >> /path/to/your-project/AGENTS.md
```

If your harness reads `CLAUDE.md`, append `open-forge/src/open-forge/CLAUDE.md` to your project's `CLAUDE.md` as well. If the project already has a `.agents/` folder, merge the two by hand instead of copying over it.

## 3. Review and commit

Whichever route you took, review the result with `git diff` and commit it. That's the whole setup.

Installing and updating are things you do when maintaining the workspace. Agents don't repeat them at startup.

:::tip[Read the files once]

The installed files are short, and they become instructions your agents follow. Read them once now, and again after each update. They're also yours: add, adapt, replace, or remove the defaults as your needs change.

:::

## Add Extensions (optional)

Once the Framework is in place, you can add optional packages. Preview first:

```sh
open-forge extension list --available
open-forge extension install development-toolkit --dry-run
```

The [Extensions](../extensions/index.md) section explains what each package installs and what every file is for.

**Next:** [Your first task](first-task.md).
