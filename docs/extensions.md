# Open Forge Extensions

Extensions package useful additions to a workspace: instructions, advice, reusable shapes, workflows, templates, native skills, and supporting files. Choose the packages that help your work, then adapt the installed content as your needs change.

An Extension is a way to distribute files. Routed content keeps its destination's role and scope; native capabilities and support files follow the tools that use them. A package manifest helps manage installation, but the installed files contain the meaning an agent needs.

## Choose A Package

The [first-party catalogue](../src/extensions/README.md) offers five focused packages:

| Package | What it helps you do |
| --- | --- |
| `project-documents` | Establish project direction and architecture with workflows and document starters |
| `memory-starters` | Start useful analysis, decision, handoff, idea, and observation records |
| `planning` | Plan work and keep its records clear with a Workflow, a Pattern, and four starters |
| `development` | Develop, debug, and review changes using your project's rules and tools |
| `orchestration` | Coordinate dependent work through Managed Delivery; includes Planning and Development as dependencies |

The sixth package, `development-toolkit`, bundles the first four through dependencies. It contributes no files of its own. Choose a focused package when you need only that part, or the Toolkit when the whole set is useful.

The CLI can use its embedded catalogue or a local package source. Use `--source` to select the exact source you want to inspect or install. Local dependency resolution stays within that source; it does not fetch missing packages from a registry.

```sh
open-forge extension list --available --source /path/to/open-forge/src/extensions
open-forge extension inspect orchestration --source /path/to/open-forge/src/extensions
```

Replace example paths with your own. A local source must be separate from the target workspace, including after resolving links. Keeping the Open Forge checkout beside your project works; placing the source catalogue inside that project does not.

## Install An Extension

Set up the base Framework first. From your project, preview the selected package and its dependencies:

```sh
open-forge extension install orchestration \
  --source /path/to/open-forge/src/extensions \
  --dry-run
```

Review the proposed files, then repeat the command without `--dry-run` to apply it. If you are running from another directory, add `--workspace /path/to/project` to both commands. Check the resulting diff before adopting the content.

You can select several package IDs in one request. `--all` selects all packages in the chosen source. `--automatic` disables prompting; it does not select packages or authorize overwrites for you.

Installation records managed ownership in `.agents/open-forge.lifecycle.json`. This lets later operations distinguish package files from workspace changes. The record supports file maintenance and does not become agent context or grant authority to the content.

## Update And Remove

Inspect installed packages and preview an update from the same source:

```sh
open-forge extension list --installed
open-forge extension update orchestration \
  --source /path/to/open-forge/src/extensions \
  --dry-run
```

Normal updates preserve locally changed files, missing files, and retired content that needs a deliberate choice. Use the result to decide which differences to keep:

| Option              | What it selects                                                    |
| ------------------- | ------------------------------------------------------------------ |
| `--force`           | Eligible changed or missing paths that the current package expects |
| `--prune` on update | Eligible retired content that is no longer in the package          |
| `--automatic`       | Non-interactive execution with the choices already supplied        |

Preview those options before applying them. They have specific boundaries; none is a general permission to overwrite the workspace.

To remove a package, first inspect the plan:

```sh
open-forge extension remove orchestration --dry-run
```

Removal releases its trusted ownership. It retains shared files, deletes safe unchanged files whose final owner is removed, and normally preserves changed files as unmanaged content. `--prune` also selects eligible changed files for deletion. The operation protects packages still needed by retained dependents and does not require the original source catalogue.

The [CLI guide](cli.md#extension-operations) covers the command surface. When an operation reports an incomplete change or retained recovery files, follow its reported next action. Managed changes preserve recovery evidence where required; they do not promise automatic rollback.

## Customize Installed Content

Edit an installed file when your workspace needs a different version. Managed updates recognize that divergence. For a small adjustment, an adjacent `{name}.overwrite.md` can hold the corresponding local change without editing the base.

An overwrite shares the base's role, scope, and loading behavior. It takes precedence only for the corresponding base content. It stays workspace-owned and is not a separately indexed route.

A Template works differently: copy and adapt it, then maintain the result independently. Updating the Template does not update copies made from it.

## Manual Installation

The CLI is optional. To install a package by hand:

1. Review its manifest and the content of its dependencies.
2. Copy the files under each selected package's and dependency's `content/` into the target workspace, preserving existing changes and shared files. A dependency-only bundle has no files of its own to copy.
3. Keep affected entrypoints and `Entries` aligned with the assembled files. Follow the destination routes' rules and check links from the installed locations.
4. Review the diff and accept the result.

Manually copied files are usable Framework content. Copying them does not create managed ownership, so later CLI operations must not assume they were installed by the CLI.

## Create An Extension

Start with a repeatable need and a small coherent set of files. Put shared content in one package and use dependencies when another package needs it. Installed files should express their working relationships through ordinary links and instructions.

The CLI can scaffold a package in an existing catalogue directory:

```sh
open-forge extension create team-practices \
  --path /path/to/catalogue \
  --name "Team Practices" \
  --description "Shared review and documentation practices" \
  --dry-run
```

Apply the reviewed command without `--dry-run`, then add complete files under `content/`. Create writes the manifest and an initial `content/.agents/` directory. It does not install the package into a workspace. Its `--path` selects the catalogue parent; `--workspace` does not redirect creation.

Use `--dependency <stable-id>` for each dependency and `--package-version <text>` to choose the version. Supply the package ID and destination explicitly in scripts. Creating a manifest records dependency IDs without proving they are available; installation resolves them from the selected source.

## Package Format

A package contains a manifest, optional package documentation, and files arranged as they will appear in the target workspace:

```text
team-practices/
  extension.json
  README.md
  content/
    .agents/
      guidance/
        team-reviews.md
```

A minimal manifest is:

```json
{
  "id": "team-practices",
  "name": "Team Practices",
  "description": "Shared review and documentation practices",
  "version": "0.1.0",
  "dependencies": []
}
```

Use a stable package ID and list dependencies as unique, sorted IDs. The manifest accepts these five fields. A package may provide one content role, a useful combination, or only dependencies. A dependency-only package may omit `content/`.

Add complete files, including entrypoints where new routed folders require them. Write links for the installed layout. The assembled Framework and declared dependencies may supply destinations outside the package itself.

Native Skill packages retain their own `SKILL.md` metadata and resources. Support files for another tool retain that tool's format. Routing them does not transfer ownership from an external manager to Open Forge.

### Files Used By Other Tools

Files outside `.agents/`, such as an agent definition consumed by another tool, require exact consumer grants in `.agents/open-forge.permissions.json` for managed installation. An interactive apply request can ask whether to remember a missing grant. Dry runs, JSON output, redirected input, and automatic mode report missing grants without saving approval.

`--force` and `--prune` do not bypass those grants. Keep one manager per installed path. Installing a capability through another tool does not by itself satisfy an Open Forge package dependency.

## Check The Assembled Result

Preview installation into a separate workspace with the intended Framework and dependencies. Inspect the installed content, links, loading behavior, and any existing files the operation would affect. With the CLI available, use `open-forge doctor` and review the diff.

Structural checks show whether the package fits together. Trying its workflows, templates, or capabilities on representative work shows whether it earns its place. Keep those two kinds of evidence clear when describing a package.

See the [Extensions Architecture](../.agents/memory/crystallized/documents/extensions/architecture.md) for composition principles and the [Extension contracts](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md) for exact managed-operation rules.
