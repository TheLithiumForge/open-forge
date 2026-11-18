---
title: Developer tools
description: Install supported code-search tools with their own installers and set them up for a project in one step.
---

# Developer tools

Open Forge can install a few supported developer tools for you and set them up for your project. It uses each tool's own install command and its own setup, then adds one small change of its own: it keeps archived Open Forge memory out of the tool's index, so search results stay current.

The project lists the tools it uses in `.agents/open-forge.json`. A teammate who clones the project runs one command to install the ones they're missing.

## See what's available

```sh
open-forge tool list
```

Each row shows the tool's ID, how it installs (`npm` or `script`), whether this project uses it and the version on your machine. Today both tools are in the Code search category:

- **CodeGraph** (`codegraph`) installs with npm and adds itself to the coding agents it finds in your project.
- **Codebase Memory** (`codebase-memory`) installs with its own script. Its installer configures every coding agent it finds for your account and adds its folder to your PATH.

## Install a tool

Preview first. Nothing runs and nothing changes:

```sh
open-forge tool install codegraph --dry-run
```

Then run it without `--dry-run`. Open Forge shows the same plan and asks before it does anything:

```text
Open Forge will run these commands to install CodeGraph:
  npm i -g @colbymchenry/codegraph@1.6.2
  codegraph install --yes --location local
      Adds CodeGraph to the coding agents it finds in this project
      CodeGraph keeps its default telemetry. Turn it off with: codegraph telemetry off
Then Open Forge will make these changes:
  codegraph.json           Keep archived Open Forge memory out of the graph
  .agents/open-forge.json  Add codegraph to this project's tools
Continue? [y/N]
```

If npm isn't installed, Open Forge stops before running anything and tells you to install Node.js, which includes npm. For a script install it checks for curl and bash, or for Windows PowerShell on Windows.

If the tool is already installed, Open Forge skips its install command, shows the installed version and still runs the tool's setup, which repairs it. If a command fails, Open Forge stops, shows the command's exit code and the end of its output, and leaves its own changes unmade.

Install works in a folder where Open Forge is installed, because it records the tool in `.agents/open-forge.json`. Run `open-forge install` first in a new project.

## Set up a teammate's machine

After cloning a project that already lists its tools, run:

```sh
open-forge tool install
```

This installs only the listed tools that are missing on your machine. The project is already set up, so it doesn't rerun any tool's setup.

## Update or remove a tool

To update a tool, use its own update command or run `open-forge tool install <id>` again.

To stop using a tool in a project:

```sh
open-forge tool remove codegraph
```

Open Forge removes its own changes first. Then it explains what the tool's own uninstall does and asks whether to run it too. The answer defaults to no, so the tool stays installed unless you say yes.

## What Open Forge doesn't do

Open Forge doesn't edit your PATH, install runtimes such as Node.js or change a tool's own settings. A tool's own installer may do some of these, and the plan says so before you agree. The [command reference](/guides/cli#developer-tools) has every option.
