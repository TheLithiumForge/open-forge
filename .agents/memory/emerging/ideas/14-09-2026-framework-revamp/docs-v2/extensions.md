# Open Forge Extensions

An Extension is a package of files for your workspace: instructions, advice, reusable shapes, workflows, templates, native skills, and supporting files. Pick the packages that help, then adapt what they install as your needs change.

An Extension is a way to distribute files, nothing more. Routed content keeps the role and scope of wherever it lands. Native capabilities and support files follow the tools that use them. The manifest helps with installation. The installed files carry the meaning an agent needs.

## Choose a package

The [first-party catalogue](../src/extensions/README.md) has five focused packages:

| Package             | What it helps you do                                                                                  |
| ------------------- | ----------------------------------------------------------------------------------------------------- |
| `project-documents` | Establish project direction and architecture with workflows and document starters                     |
| `memory-starters`   | Start useful analysis, decision, handoff, idea, and observation records                               |
| `planning`          | Plan work and keep its records clear with a Workflow, a Pattern, and four starters                    |
| `development`       | Develop, debug, and review changes using your project's rules and tools                               |
| `orchestration`     | Coordinate dependent work through Managed Delivery. Includes Planning and Development as dependencies |

A sixth package, `development-toolkit`, bundles the first four through dependencies and contributes no files of its own. Choose a focused package when you need only that part, or the Toolkit when the whole set is useful.

The CLI embeds the first-party catalogue at build time. Leave out `--source` to use those packages. No source checkout is needed. Each build includes the current package files and reads their IDs and dependencies from the manifests. Dependency resolution stays inside the selected catalogue and never fetches from a registry.

```sh
open-forge extension list --available
open-forge extension inspect orchestration
```

To inspect or install local changes, add `--source /path/to/open-forge/src/extensions`, replacing the path with your catalogue. A local source must be separate from the target workspace, including after links resolve. Keeping the Open Forge checkout beside your project works. Placing the catalogue inside that project does not. An explicit source selects that catalogue alone.

## Install a package

Set up the base Framework first. From your project, preview the package and its dependencies:

```sh
open-forge extension install orchestration --dry-run
```

To preview from a local catalogue instead:

```sh
open-forge extension install orchestration \
  --source /path/to/open-forge/src/extensions \
  --dry-run
```

Review the proposed files, then repeat the command without `--dry-run`. If you are running from another directory, add `--workspace /path/to/project` to both commands. Check the diff before adopting the content.

Several package IDs can go in one request. `--all` selects every package in the chosen source. `--automatic` disables prompting. It does not select packages or authorize overwrites for you.

Installation records managed ownership in `.agents/open-forge.lifecycle.json`. Later operations use it to tell package files from workspace changes. The record supports file maintenance. It does not become agent context and grants the content no authority.

## Update and remove

Inspect installed packages and preview an update from the embedded catalogue. For a local catalogue, add `--source`:

```sh
open-forge extension list --installed
open-forge extension update orchestration --dry-run
```

```sh
open-forge extension update orchestration \
  --source /path/to/open-forge/src/extensions \
  --dry-run
```

A normal update preserves locally changed files, missing files, and retired content that needs a deliberate choice. Use the result to decide which differences to keep:

| Option              | What it selects                                                    |
| ------------------- | ------------------------------------------------------------------ |
| `--force`           | Eligible changed or missing paths that the current package expects |
| `--prune` on update | Eligible retired content that is no longer in the package          |
| `--automatic`       | Non-interactive execution with the choices already supplied        |

Preview those options before applying them. Each has a specific boundary. None is a general permission to overwrite the workspace.

To remove a package, inspect the plan first:

```sh
open-forge extension remove orchestration --dry-run
```

Removal releases the package's ownership. It keeps shared files, deletes safe unchanged files whose final owner is removed, and normally preserves changed files as unmanaged content. `--prune` also selects eligible changed files for deletion. The operation protects packages that retained dependents still need and does not require the original catalogue.

The [CLI guide](cli.md#extensions) covers the command surface. When an operation reports an incomplete change or retained recovery files, follow its reported next action. Managed changes preserve recovery evidence where required. They do not promise automatic rollback.

## Customize installed content

Edit an installed file when your workspace needs something different. Managed updates recognize the divergence. For a small adjustment, an adjacent `{name}.overwrite.md` can hold the local change without editing the base.

An overwrite shares the base's role, scope, and loading behavior. It takes precedence only for the corresponding base content. It stays workspace-owned and is not a separately indexed route.

Templates work differently: copy one, adapt it, and maintain the result independently. Updating the Template does not update copies made from it.

## Manual installation

The CLI is optional. To install a package by hand:

1. Review its manifest and the content of its dependencies.
2. Copy the files under each selected package's and dependency's `content/` into the target workspace, preserving existing changes and shared files. A dependency-only bundle has no files to copy.
3. Keep affected entrypoints and `Entries` aligned with the assembled files. Follow the destination routes' rules and check links from the installed locations.
4. Review the diff and accept the result.

Manually copied files are ordinary Framework content. Copying does not create managed ownership, so later CLI operations must not assume the CLI installed them.

## Create an Extension

Start with a repeatable need and a small coherent set of files. Put shared content in one package and use dependencies when another package needs it. Installed files should express their working relationships through ordinary links and instructions.

The CLI can scaffold a package in an existing catalogue directory:

```sh
open-forge extension create team-practices \
  --path /path/to/catalogue \
  --name "Team Practices" \
  --description "Shared review and documentation practices" \
  --dry-run
```

Apply the reviewed command without `--dry-run`, then add complete files under `content/`. Create writes the manifest and an initial `content/.agents/` directory. It does not install the package. `--path` selects the catalogue parent. `--workspace` does not redirect creation.

Use `--dependency <stable-id>` for each dependency and `--package-version <text>` to choose the version. Supply the package ID and destination explicitly in scripts. A manifest records dependency IDs without proving they exist. Installation resolves them from the selected source.

## Package format

A package holds a manifest, optional documentation, and files arranged as they will appear in the target workspace:

```text
team-practices/
  extension.json
  README.md
  content/
    .agents/
      guidance/
        team-reviews.md
```

A minimal manifest:

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

Add complete files, including entrypoints where new routed folders need them. Write links for the installed layout. The assembled Framework and declared dependencies may supply destinations outside the package.

Native Skill packages keep their own `SKILL.md` metadata and resources. Support files for another tool keep that tool's format. Routing them does not transfer ownership from an external manager to Open Forge.

### Files used by other tools

Files outside `.agents/`, such as an agent definition another tool consumes, need exact consumer grants in `.agents/open-forge.permissions.json` for managed installation. An interactive apply can ask whether to remember a missing grant. Dry runs, JSON output, redirected input, and automatic mode report missing grants without saving approval.

`--force` and `--prune` do not bypass those grants. Keep one manager per installed path. Installing a capability through another tool does not by itself satisfy an Open Forge package dependency.

## Check the assembled result

Preview installation into a separate workspace with the intended Framework and dependencies. Inspect the installed content, links, loading behavior, and any existing files the operation would touch. With the CLI available, run `open-forge doctor` and review the diff.

Structural checks show whether the package fits together. Trying its workflows, templates, or capabilities on representative work shows whether it earns its place. Keep those two kinds of evidence distinct when describing a package.

The [Extensions Architecture](../.agents/memory/crystallized/documents/extensions/architecture.md) covers composition principles. The [Extension contracts](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md) hold the exact managed-operation rules.
