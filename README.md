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
    _workspace.md             <- workspace contract and generated routes
```

The important thing is not the number of files. The important thing is the routing.

`AGENTS.md` points agents at the loader. The CLI generates the loader's active category entries from category metadata, so agents immediately see where each category lives and what it represents. Category entrypoints then expose their relevant routed files.

## First Thing After Install

Read the files Open Forge installed.

At minimum:

```text
AGENTS.md
.agents/constants.md
.agents/loader.md
.agents/workspace/_workspace.md
.agents/workspace/*.md
```

Make sure they are what you need. If they are not, change them.

The framework files are fair game. They are yours now. The only reason to avoid editing managed defaults is easier future updates.

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
.agents/workspace/repositories.md
.agents/workspace/guides.md
.agents/workspace/local-docs.md
```

Inside those files, point at your actual docs, guides, directives, workflows, repos, tasks, vault folders, or whatever else your workspace needs.

The default install is intentionally small. If you want more specific docs, tasks, project areas, repo maps, or local rituals, add them when they earn their place.

### Frontmatter

Index metadata comes from frontmatter:

```md
---
description: Local documentation routes
tags: [Docs, Workspace]
---
```

Nested metadata also works:

```md
---
open-forge:
  description: Local documentation routes
  tags: [Docs, Workspace]
---
```

The CLI also reads `rune:` metadata in files you add for other tooling. Open Forge-authored files use `open-forge:`.

### Category Entrypoints

Category entrypoints follow one simple rule: a routed folder contains an entrypoint named `_{folder-name}.md`.

```text
.agents/knowledge/
  _knowledge.md
  architecture.md
```

The installed workspace category uses the same shape:

```text
.agents/workspace/
  _workspace.md
  repositories.md
```

The `_{folder-name}.md` file is the category entrypoint. It keeps the short category contract at the beginning and generated navigation at the end.

The CLI also accepts `_index.md`, `index.md`, `_references.md`, and `references.md` as cross-tool compatibility aliases. Open Forge-authored categories always use `_{folder-name}.md`. Keep exactly one recognized entrypoint in each folder.

A new top-level category becomes active when a direct child folder contains its matching entrypoint. The CLI adds its description, tags, and path to the loader automatically. Nested categories become reachable through their parent category's generated entries.

Generated entries look like this:

```md
- `{file}` - {description} - #{tag1} #{tag2} ... #{tagN}
- `{folder/_folder.md}` - {description} - #Index
```

File names are used as-is. Child folders are routed through their own `_{folder-name}.md` category entrypoint. If you number files, the generated entries keep those numbers.

The generated region is explicitly bounded:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The CLI changes only the content between those markers. Category meaning and axioms stay above the region. Detailed rules, recommendations, and user content belong in separate routed files.

`open-forge install` rebuilds generated index regions automatically.

You can also rebuild only generated index regions:

```sh
npx open-forge index
```

The CLI indexes routed files. Agents follow the routes. If your docs live somewhere unusual, declare that place in a workspace route file.

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

Do not edit or overwrite generated index regions. Add or edit files in the indexed folder instead.

That way, later updates are still manageable. You can `git diff` the changed base file, see what Open Forge updated, and decide what to keep.

It is a little artisanal. That is fine. The entire point is to make the system fit the workspace instead of making the workspace cosplay someone else's process.

## Docs

- CLI command details live in [docs/cli.md](docs/cli.md).
- Development, build, and publishing notes live in [docs/dev.md](docs/dev.md).
