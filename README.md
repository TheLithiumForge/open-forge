# The Open Forge Methodology

Open Forge is a tiny, agnostic AI agent workflow that embraces the idea of spec-driven development and extends it to real workflows.

The aim is to give you an organically growing system, shaped by your actual needs, that helps turn ideas into docs, tasks, implementation, review, handoff, and learning inside plain repo files.

With the AI shift, code became more of a commodity and reviewing became the usual bottleneck. Open Forge puts the person at the forefront of the work. AI should be one of the tools in a developer's toolbox, not the main driver. From designing and brainstorming to implementing and reviewing, the AI should increase productivity without becoming a maintenance burden.

That also means the framework itself should be easy to review. With all the AI-related security breaches and supply-chain attacks, trusting your tool is not optional. Open Forge starts as plain files that can be read, diffed, changed, and carried forward with minimal initial effort.

The methodology is simple: like in a forge, things need to happen organically. If you force it, you break it.

The core of the methodology is patterns and knowledge routes. Patterns make structure visible. If something respects the local pattern, that is a quiet confidence signal. If it does not, that becomes easier to spot as a warning flag.

An AI agent does not need to know everything, but it needs to know where everything is. Open Forge gives it a loader and index-like files for knowledge routing. Modern agents are smart enough to load only what is needed for the task at hand, provided the workspace tells them where to look.

This is probability-shaping, not mechanical control. Deterministic routes, validators, memory, and review checkpoints make a nondeterministic agent more likely to see and follow the right context; they cannot guarantee that it will. Keep important choices visible, verify consequential work, and treat evidence as stronger than agent self-report.

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

Start in a Git repository, then install Core into the current folder:

```sh
git init
npx open-forge install
```

Review the small Core diff and commit it before adding optional material:

```sh
git status
git diff
git add AGENTS.md .agents
git commit -m "Install Open Forge Core"
```

Install a local extension overlay:

```sh
npx open-forge extend {extension-folder}
```

Select, list, or install bundled first-party extensions:

```sh
npx open-forge extend
npx open-forge extend --list
npx open-forge extend {extension-id}
npx open-forge extend --ids {extension-id},{extension-id}
npx open-forge extend {extension-id} --dry-run
npx open-forge extend --remove {installed-extension-id} --dry-run
```

An extension may contain one skill, one workflow, directives, patterns, guidance, workspace or memory routes, support-only material, any deliberate mix, or only dependencies as a convenience pack. The catalogue derives and shows those contents; interactive selection marks transitive dependencies as required and locks them while needed. Bundled dependencies resolve offline and install automatically. `--dry-run` shows dependency order, every planned file, change status, and baseline/executable scope without writing.

Stable-id extensions are safely updateable and removable. Every bundled package declares an id independent of its source-folder grouping, and the catalogue presents Skills, Workflows, Packs, and Support without changing those keys. Open Forge stores ownership and hashes in transparent, Git-visible `open-forge.extensions.json`, while agents route from installed files. A local source opts into this managed lifecycle by declaring a stable manifest `id`; idless plain overlays and directly installed skills remain unmanaged.

Extensions add whole files through the ordinary route tree; they do not mutate shared Markdown. The CLI is optional: a manual install can copy an extension's `payload/` and update affected generated `Entries` by hand. Manifests, catalogue grouping, and receipts are install metadata only, while installed routed files remain complete agent-readable truth.

Each normal extension invocation starts from a clean target-scoped Git checkpoint and ends by asking you to review and commit the selected dependency closure. Install separate roots in separate commands when you want separate diffs; multi-select and `--ids` intentionally make one combined review unit. Removal changes exactly the requested ids and blocks when retained extensions still depend on them; orphan dependencies are not pruned automatically. Updating or removing an owned entrypoint is also blocked when the final route tree would strand retained descendants; move or remove those descendants in the same plan, or keep another owner for the route host. Catalogue listing and dry runs are read-only and remain available at any time.

Expert users may add `--pro` to `install` or a writing `extend` command to intentionally bypass the Git/Core lifecycle checkpoints. This does not disable dependency, manifest, receipt, ownership, containment, collision, link, index, or rollback safety.

Install Open Forge into another folder:

```sh
npx open-forge install {target-folder}
```

Outside Git, an interactive install recommends `git init` and asks for explicit approval; a non-interactive install stops without writing. This keeps the first Core diff and every later extension transaction independently reviewable by default.

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

Manual archive copying bypasses the CLI checkpoint guard. Start from a clean Git baseline, verify the archive hash or manifest, review the copied Core diff, and commit it before installing optional extensions.

## What Gets Installed

Installed into a target repo, Open Forge creates this shape:

```text
AGENTS.md                     <- agent entry block
.agents/
  loader.md                   <- tells the agent what to read and when
  directives/
    _directives.md            <- binding instructions selected through routes
  guidance/
    _guidance.md              <- contextual guidance for recurring decisions
  memory/
    _memory.md                <- self-growing workspace memory routes
    working/
      _working.md             <- temporary memory for continuing active work
      handoffs/
        _handoffs.md          <- concise transfer notes for context breaks
      sessions/
        _sessions.md          <- raw chronological work records
    emerging/
      _emerging.md            <- candidate memory routes
      analysis/
        _analysis.md          <- structured reasoning before acceptance
      ideas/
        _ideas.md             <- future potential and candidate options
      observations/
        _observations.md      <- agent-noticed findings and learning
    crystallized/
      _crystallized.md        <- accepted durable memory and current truth
      decisions/
        _decisions.md         <- accepted rationale for important choices
      documents/
        _documents.md         <- long-form accepted records and routes
    archived/
      _archived.md            <- historical memory routes
  patterns/
    _patterns.md              <- concrete reusable shapes for inspectable work
  skills/
    _skills.md                <- bounded reusable agent capabilities
  workflows/
    _workflows.md             <- goal-oriented agent workflows
  workspace/
    _workspace.md             <- project locations and when to use them
```

The important thing is not the number of files. The important thing is the routing.

`AGENTS.md` points agents at the loader. The CLI generates the loader's active root-route `entries` from `entrypoint` metadata, so agents immediately see where each root route lives and what it represents. Category `entrypoints` then expose their relevant routed files.

Every direct directive file carries #LoadNow relative to its already-loaded parent. The root directive route is baseline-loaded, so its direct files bind workspace-wide. Put narrower directives below a positively described child directive `entrypoint`; selecting and loading that route establishes scope before ordinary #LoadNow traversal reads its direct files. A direct directive file contains one substantive level-2 `## Axioms` section and no `Applies To` gate. Use guidance, a skill, or a workflow when behavior is optional rather than mandatory.

For non-trivial work, agents use the request, routed current truth, and visible workflow descriptions and tags to select a relevant workflow before opening it, then confirm its Goal. A Goal may include an optional `- helpful before: ...` item. When that work would help and a matching earlier workflow is available, the agent recommends it once without blocking progress; if it is skipped or unavailable, the selected workflow proceeds with explicit assumptions. Phases are wayfinding, not a waterfall. One workflow stays primary, additional workflows become ordered handoffs, and an explicit workflow choice or opt-out wins.

#KeepInMind routes protect long-running work from context loss. At task start or resume, after actual context restoration, before handoff, and before closeout, agents recheck the complete catalogue and keep its follow-ups binding within their owner's authority. They also refresh it when a transition may have changed those follow-ups.

When the CLI is available, one command emits effective startup context in the correct order:

```sh
npx open-forge load --bodies
```

It optionally batches the same plain traversal: the visible transitive #LoadNow chain plus the complete #KeepInMind catalogue, with each user-owned `.overwrite.md` immediately after its base.

Generated paths are concrete and relative to the folder whose `AGENTS.md` selected the loader. A shared submodule does not change those logical paths. For safety, deterministic CLI reads and writes reject a symlinked or junction-mounted route tree that resolves outside the selected target; plain Markdown loading of an explicitly trusted external mount remains a manual trust decision.

## First Thing After Install

Review and read the files Open Forge installed, then commit the base baseline (#Core plus minimum #Memory, with no optional extensions) before installing extensions.

At minimum:

```text
AGENTS.md
.agents/loader.md
.agents/directives/_directives.md
.agents/guidance/_guidance.md
.agents/memory/_memory.md
.agents/memory/working/_working.md
.agents/memory/working/handoffs/_handoffs.md
.agents/memory/working/sessions/_sessions.md
.agents/memory/emerging/_emerging.md
.agents/memory/emerging/analysis/_analysis.md
.agents/memory/emerging/ideas/_ideas.md
.agents/memory/emerging/observations/_observations.md
.agents/memory/crystallized/_crystallized.md
.agents/memory/crystallized/decisions/_decisions.md
.agents/memory/crystallized/documents/_documents.md
.agents/memory/archived/_archived.md
.agents/patterns/_patterns.md
.agents/skills/_skills.md
.agents/workflows/_workflows.md
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
tags: [Doc, Workspace]
---
```

Nested metadata also works:

```md
---
open-forge:
  description: Local documentation routes
  tags: [Doc, Workspace]
---
```

The CLI also reads `rune:` metadata in files you add for other tooling. Open Forge-authored files use `open-forge:`.

### Category Entrypoints

Category `entrypoints` follow one simple rule: a routed folder contains an `entrypoint` named `_{folder-name}.md`.

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

The `_{folder-name}.md` file is the category `entrypoint`. It keeps the short category contract at the beginning and generated navigation at the end.

The CLI also accepts `_index.md`, `index.md`, `_references.md`, and `references.md` as cross-tool compatibility aliases. Open Forge-authored categories always use `_{folder-name}.md`. Keep exactly one recognized `entrypoint` in each folder.

A new top-level category becomes active when a direct child folder contains its matching `entrypoint`. The CLI adds its description, tags, and path to the loader automatically. Nested categories become reachable through their parent category's generated `entries`.

Generated `entries` look like this:

```md
- `{file}` - {description} - #{Tag1} #{Tag2} ... #{TagN}
- `{folder/_folder.md}` - {description} - #Index
```

File names are used as-is. Child folders are routed through their own `_{folder-name}.md` category `entrypoint`. If you number files, the generated `entries` keep those numbers.

Use `scope routes` when a workspace needs extra ownership or meaning. A `scope route` is a `slug` folder with its own `entrypoint`:

```text
.agents/memory/
  _memory.md
  [scope]/
    _[scope].md
    crystallized/
      _crystallized.md
      decisions/
        _decisions.md
      documents/
        _documents.md

.agents/guidance/
  _guidance.md
  [scope]/
    _[scope].md
    cross-platform-apps.md
```

`[scope]` means a real folder name such as `mobile-app`, `billing-api`, or any other concrete `slug`. It is not installed literally.

Every folder in the visible route chain needs its own `entrypoint`; otherwise the parent index cannot route to deeper files. `Entries` list sibling markdown files and direct child `entrypoints`.

A `scoped framework route` is a `framework route` initialized inside a `scope route`, such as `crystallized/_crystallized.md` under `[scope]/`. Open Forge does not require `projects/`, `domains/`, `teams/`, or any other grouping folder. Add those only when they make your routes easier to read.

Route placement changes meaning:

```text
.agents/memory/crystallized/[scope]/decisions/
```

This means `[scope]` is a scope inside crystallized memory.

```text
.agents/memory/[scope]/crystallized/decisions/
```

This means `[scope]` owns its own memory states.

Both shapes are valid when every folder has an `entrypoint` and the `entrypoint` descriptions make the scope clear.

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

Agents read `{name}.md` and then your `{name}.overwrite.md`, so the overwrite has final precedence within that file's scope. Extensions add whole routed files and must not own workspace overwrite files.

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
- Extension mechanics live in [docs/extensions.md](docs/extensions.md).
- Development, build, and publishing notes live in [docs/dev.md](docs/dev.md).
