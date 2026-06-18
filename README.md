# The Open Forge Methodology

Open Forge is a tiny, agnostic AI agent workflow that embraces the idea of spec-driven development and extends it to real workflows.

The aim is to give you an organically growing system, shaped by your actual needs, that helps turn ideas into docs, tasks, implementation, review, handoff, and learning inside plain repo files.

With the AI shift, code became more of a commodity and reviewing became the usual bottleneck. Open Forge puts the person at the forefront of the work. AI should be one of the tools in a developer's toolbox, not the main driver. From designing and brainstorming to implementing and reviewing, the AI should increase productivity without becoming a maintenance burden.

That also means the framework itself should be easy to review. With all the AI-related security breaches and supply-chain attacks, trusting your tool is not optional. Open Forge starts as plain files that can be read, diffed, changed, and carried forward with minimal initial effort.

The methodology is simple: like in a forge, things need to happen organically. If you force it, you break it.

The core of the methodology is patterns and knowledge routes. Patterns make structure visible. If something respects the local pattern, that is a quiet confidence signal. If it does not, that becomes easier to spot as a warning flag.

An AI agent does not need to know everything, but it needs to know where everything is. Open Forge gives it a loader and index-like files for knowledge routing. Modern agents are smart enough to load only what is needed for the task at hand, provided the workspace tells them where to look.

Open Forge is intentionally a bit more work from the start than other SDD frameworks. It does not ship a complete set of defaults because complete defaults usually assume everyone works the same way. They do not.

The framework is meant to support one project, many projects together, a monorepo, a private document vault or second brain, or whatever shape your work actually has. My use cases will differ from your use cases. Your use cases will differ from the next person's use cases. Optimizing for some imaginary middle ground would quietly harm everyone's projects, just in slightly different ways.

So Open Forge optimizes for customizability and organic growth. It provides only a handful of rules and a scalable architecture that you can craft to your own needs and wishes.

Before asking: yes, you should definitely read the files on initial install and on updates. They are small on purpose, and they become instructions your agents will follow.

## Why Use It

- You want agents to help without taking over the shape of your work.
- You want plain markdown, not a hidden runtime or a vendor-shaped ritual.
- You want the same base to work for one repo, many repos, or a vault.
- You want reviewable chunks of documentation, code, decisions, and handoffs.
- You want local patterns that make drift and weird agent decisions easier to notice.
- You want to grow your own framework primitives instead of inheriting a giant default pack.

## Usage

Install Open Forge into the current folder:

```sh
npx open-forge install
```

Install Open Forge into another folder:

```sh
npx open-forge install {target-folder}
```

## GitHub Release Install

If you prefer the manual path, download the standalone source archive from a GitHub Release.

macOS / Linux:

```sh
curl -L {release-url}/open-forge-src.tar.gz -o open-forge-src.tar.gz
tar -xzf open-forge-src.tar.gz
cp -R open-forge-src/. {target-folder}/
```

Optional checksum:

```sh
curl -L {release-url}/open-forge-src.tar.gz.sha256 -o open-forge-src.tar.gz.sha256
sha256sum -c open-forge-src.tar.gz.sha256
```

Windows PowerShell:

```powershell
Invoke-WebRequest -Uri "{release-url}/open-forge-src.tar.gz" -OutFile "open-forge-src.tar.gz"
tar -xzf open-forge-src.tar.gz
Get-ChildItem -Force .\open-forge-src | Copy-Item -Destination "{target-folder}" -Recurse -Force
```

PowerShell checksum:

```powershell
Get-FileHash .\open-forge-src.tar.gz -Algorithm SHA256
```

The release also includes:

```text
open-forge-src.manifest.json
```

That manifest contains per-file SHA-256 hashes.

## What Gets Installed

Installed into a target repo, Open Forge creates this shape:

```text
AGENTS.md                     <- agent entry block
.agents/
  constants.md                <- root path constants
  loader.md                   <- tells the agent what to read and when
  workspace/
    _workspace.md             <- indexes workspace route files
    local.md                  <- local workspace routes
    open-forge.md             <- default Open Forge routes
  patterns/
    _patterns.md              <- indexes reusable pattern files
    local.md                  <- local patterns preserved for workspace customization
    open-forge.md             <- default Open Forge patterns
  workflows/
    _workflows.md             <- indexes action sequence files
  templates/
    _templates.md             <- indexes artifact skeletons
  observations/
    _observations.md          <- indexes candidate lessons
    archive/                  <- archived observations
  sessions/
    _sessions.md              <- indexes saved chat summaries
    archive/                  <- archived sessions
  handoffs/
    _handoffs.md              <- indexes temporary continuation notes
    archive/                  <- archived handoffs
  skills/
    _skills.md                <- indexes tool and runtime adapters
docs/
  directives/                 <- human-reviewed local rules
  guides/                     <- human-facing guidance
```

The important thing is not the number of files. The important thing is the routing.

`AGENTS.md` points agents at the loader. The loader points them at the indexes. The indexes point them at the relevant files with tags and descriptions. The files tell the agent how this workspace works.

## First Thing After Install

Read the files Open Forge installed.

At minimum:

```text
AGENTS.md
.agents/constants.md
.agents/loader.md
.agents/workspace/_workspace.md
.agents/workspace/*.md
.agents/patterns/_patterns.md
.agents/patterns/*.md
.agents/workflows/_workflows.md
.agents/templates/_templates.md
.agents/observations/_observations.md
.agents/sessions/_sessions.md
.agents/handoffs/_handoffs.md
.agents/skills/_skills.md
```

Make sure they are what you need. If they are not, change them.

The framework files are fair game. They are yours now. The only reason to avoid editing managed defaults is easier future updates.

Some files are seeded for local customization. Open Forge creates them when missing, then preserves them on normal install:

```text
.agents/workspace/local.md
.agents/patterns/local.md
```

## Growing Your Framework

Each file is just markdown. You can edit them directly.

For easier updates, prefer adding local files first, then overwrite files when that keeps behavior clear. Edit managed framework files only when the base file would mislead your workspace or when base plus overwrite would confuse an agent.

This lets you reinstall or update Open Forge later, inspect the diff, and keep your local shape without wrestling every line.

The starter files are anchors, not borders. Add your own files where the local shape needs them.

### Local Shape

Use workspace route files to tell agents where things live and what they mean.

Useful place:

```text
.agents/workspace/
```

Examples:

```text
.agents/workspace/local-docs.md
.agents/workspace/repositories.md
.agents/workspace/guides.md
```

Inside those files, point at your actual docs, guides, directives, workflows, repos, tasks, vault folders, or whatever else your workspace needs.

Use pattern files to describe reusable structure and placement rules.

Useful place:

```text
.agents/patterns/
```

Examples:

```text
.agents/patterns/local.md
.agents/patterns/local-docs.md
.agents/patterns/reviewable-work.md
```

The default install is intentionally small. If you want more specific docs, tasks, project areas, repo maps, or local rituals, add them when they earn their place.

### Frontmatter

Index metadata comes from frontmatter:

```md
---
description: Local documentation patterns
tags: [Docs, Pattern]
---
```

Nested metadata also works:

```md
---
open-forge:
  description: Local documentation patterns
  tags: [Docs, Pattern]
---
```

`rune:` is accepted the same way for cross-tool compatibility.

### Indexes

Indexes follow one simple rule: a folder can contain an index named `_{folder-name}.md`.

```text
.agents/patterns/
  _patterns.md
  open-forge.md
  local-docs.md
```

The same shape is used for workspace routes:

```text
.agents/workspace/
  _workspace.md
  local.md
  open-forge.md
```

Generated entries look like this:

```md
- `{file}` - {description} - #{tag1} #{tag2}
```

File names are used as-is. If you number files, the index keeps those numbers.

Index files are intentionally dull. They should contain a short description and generated entries, not rules or recommendations.

`open-forge install` rebuilds indexes automatically.

You can also rebuild only indexes:

```sh
npx open-forge index
```

The CLI indexes the route files. The agents follow the routes. If your docs live somewhere unusual, declare that place in a workspace route file.

### Overwrites

Any markdown file can have a companion overwrite file:

```text
{name}.md
{name}.overwrite.md
```

Use an overwrite when the changed behavior is something an AI agent can understand and respect while reading both files together.

Edit the base file when the base behavior is wrong for your workspace. Use an overwrite when the base behavior is mostly right, but needs a local addition, narrowing, exception, or disable.

Good overwrite use:

- narrow a rule
- add local examples
- add a local exception
- add a missing section
- disable a small behavior with a clear reason

Bad overwrite use:

- keep two contradictory rules active
- fight the base file section by section
- ask the agent to guess which incompatible model is real

If the behavior is divergent enough that both files together would confuse the agent, it is better to edit `{name}.md`. Remove the section that would cause the problem, then add the replacement behavior in `{name}.overwrite.md`.

Do not use overwrites for generated index files. Add or edit files in the indexed folder instead.

That way, later updates are still manageable. You can `git diff` the changed base file, see what Open Forge updated, and decide what to keep.

It is a little artisanal. That is fine. The entire point is to make the system fit the workspace instead of making the workspace cosplay someone else's process.

## Docs

- CLI command details live in [docs/cli.md](docs/cli.md).
- Development, build, and publishing notes live in [docs/dev.md](docs/dev.md).
