# Open Forge Extensions

Extensions are optional packages of workspace content: instructions, advice, reusable shapes, workflows, templates, native skills, and supporting files. Pick the packages that help your work, then adapt what they install as your needs change.

An Extension is a way to distribute files. Routed content takes the role and scope of the route it lands in. Native capabilities and support files follow the tools that use them. The package manifest helps manage installation, but the installed files carry everything an agent needs to understand them.

## Choose a package

The [first-party catalogue](../src/extensions/README.md) describes every package and its dependencies.

| Package ID | Name | Use it to |
| --- | --- | --- |
| `core-templates` | Core Templates | Create directives, guidance, patterns, skills, templates, maps, and memory records from adaptable starting files. |
| `collaboration` | Collaboration | Explore ideas, compare alternatives, and clarify decisions through discussion and a brainstorming template. |
| `project-documents` | Project Documents | Write and maintain project documentation, including vision, architecture, principles, and maintenance requirements. |
| `planning` | Planning | Record ideas, investigate questions, preserve decisions, and organize tasks, plans, backlogs, and checkpoints. |
| `scenarios` | Flows and Scenarios | Describe user journeys and expected outcomes, organize scenarios, and record what happened when they were tried. |
| `observations-and-handoffs` | Observations and Handoffs | Save useful observations and prepare a clear snapshot for another person, agent, or session to resume work. |
| `development` | Development | Implement changes, investigate defects, and review results using the project's existing tools and conventions. |
| `orchestration` | Task Coordination | Coordinate related tasks, assign responsibilities, manage dependencies, and verify combined results. |
| `development-toolkit` | Development Toolkit | Install Project Documents, Planning, Flows and Scenarios, and Development together. |
| `workflows` | Workflow Support | Find and follow installed workflows, or create your own using the supplied template. |

Core Templates is a good first pick when you want to write your own workspace content. It has no dependencies and isn't part of Development Toolkit. When a more specialized starter already fits the work, use that instead.

Project Documents, Planning, and Development each depend on Workflow Support. Task Coordination retains the ID `orchestration` and depends on Development, Observations and Handoffs, and Planning. Workflow Support installs once through those dependencies.

Development Toolkit contains dependencies only. It includes Flows and Scenarios directly, so Toolkit users keep the scenario starters that used to come with Planning. Core Templates, Collaboration, Observations and Handoffs, and Task Coordination remain separate choices. Neither Flows and Scenarios nor Observations and Handoffs requires other extensions.

Workflow methods live under `.agents/skills/use-workflow/references/`, grouped by the package that supplies them. Agents reach that tree through the Skill, not through the loader's root routes, so name it explicitly when you rebuild its `Entries`:

```sh
open-forge index .agents/skills/use-workflow/references/_references.md
```

The CLI has the first-party catalogue built in. Leave out `--source` to use it. You don't need a checkout of this repository. Each build embeds the current package files and reads package IDs and dependencies from their manifests. Dependencies resolve only within the selected catalogue, and nothing is fetched from a registry.

```sh
open-forge extension list --available
open-forge extension inspect orchestration
```

To inspect or install local changes, add `--source /path/to/open-forge/src/extensions`, with the path pointing at your source catalogue. A local source must be separate from the target workspace, including after links are resolved. An Open Forge checkout next to your project works. A source catalogue inside that project doesn't. An explicit source selects that catalogue alone.

## Install an Extension

Set up the base Framework first. From your project, preview the selected package and its dependencies:

```sh
open-forge extension install orchestration --dry-run
```

To preview the package from a local source catalogue instead:

```sh
open-forge extension install orchestration \
  --source /path/to/open-forge/src/extensions \
  --dry-run
```

Review the proposed files, then run the command again without `--dry-run` to apply it. When it runs interactively, it shows the plan and asks before applying it. If you are running from another directory, add `--workspace /path/to/project` to both commands. Check the resulting diff before adopting the content.

You can select several package IDs in one request. `--all` selects all packages in the chosen source. `--automatic` disables prompting. It does not select packages or authorize overwrites for you.

Installation records managed ownership in `.agents/open-forge.lock.json`, so later operations can tell package files apart from your own changes. The lock file exists for file maintenance only. It isn't agent context, and it gives the content no authority.

## Update and remove

Inspect installed packages and preview an update from the embedded catalogue. To use a local catalogue instead, pass its exact path with `--source`:

```sh
open-forge extension list --installed
open-forge extension update orchestration --dry-run
```

For an update from a local source catalogue:

```sh
open-forge extension update orchestration \
  --source /path/to/open-forge/src/extensions \
  --dry-run
```

Normal updates replace changed owned files and restore missing owned files. Retired content is preserved unless `--prune` selects its deletion. Use the result to review which differences to keep:

| Option              | What it selects                                                    |
| ------------------- | ------------------------------------------------------------------ |
| `--force`           | Eligible changed or missing paths that the current package expects |
| `--prune` on update | Eligible retired content that is no longer in the package          |
| `--automatic`       | Non-interactive execution with the choices already supplied        |

Preview these options before applying them. Each selects something specific, and none of them is a general permission to overwrite the workspace.

To remove a package, first inspect the plan:

```sh
open-forge extension remove orchestration --dry-run
```

Removal releases the package's ownership and keeps files that another package still owns. It deletes eligible files whose last owner is being removed, including files you edited, so review the preview and save any customizations you need first. An installed package that depends on it blocks removal unless you remove that package too. You don't need the original source catalogue.

The selected IDs are recorded in `removedExtensions` in `.agents/open-forge.json`, so later installation, updates and dependency resolution keep them removed. To reinstall a package, explicitly clear its ID and any covering path exclusions, then install it. The root command `open-forge remove orchestration --kind extension` provides the same package operation.

The [CLI guide](cli.md#extension-operations) covers the command surface. When an operation reports an incomplete change or retained recovery files, follow its reported next action. Managed changes preserve recovery evidence where required. They do not promise automatic rollback.

## Moving from the earlier package layout

Revision `0.4.0` separates two sets of files from their former packages:

| Former package | New package | Installed-path change |
| --- | --- | --- |
| Planning | Flows and Scenarios (`scenarios`) | Four starters move from `templates/planning/` to `templates/scenarios/`. |
| Orchestration | Observations and Handoffs (`observations-and-handoffs`) | Memory category paths stay the same. Their starters move from `templates/orchestration/` to `templates/observations-and-handoffs/`. |

Orchestration's display name becomes Task Coordination. Its ID remains `orchestration`. Existing user records and copies made from Templates keep their own content and locations.

Fresh installations use the new layout directly. Existing installations need a reviewed ownership transition. Installing a new package over a path still owned by the former package can report a conflict, even when the bytes match. Adding a dependency does not transfer ownership.

1. Inspect installed packages and preview the affected updates. Record the packages you want to keep and preserve local edits before removing anything.
2. Preserve records beneath categories that will be removed. Removal may refuse to delete their required entrypoint. Where the current scope permits it, use a reviewed `route move` to hold an unmanaged record under another suitable parent in the same Memory state, then restore it after installation. Keep an independent backup and preserve any scoped rules it needs. A move must not silently change its meaning.
3. Preview removal of the affected old packages, their installed dependents, and the shared dependencies being replaced. The verified path replaces the complete old dependency set. For the old Toolkit plus Orchestration, that set is `development-toolkit`, `orchestration`, `planning`, `development`, `project-documents`, and `workflows`. Select only installed packages, account for other dependents, and preserve needed edits before applying. Removal can delete edited files. It has no `--prune` flag.
4. Inspect retained files and reported recovery bundles. Reconcile edited or retained paths explicitly. Never delete user records or rewrite ownership files to force adoption. After checking the preserved work and completed removal, preview `cleanup --dry-run` and apply `cleanup` for recognized recovery data when safe. A retained bundle can block reinstallation.
5. Clear the `removedExtensions` entries for packages you intend to reinstall, including their dependencies, and any covering path exclusions. Preview and install the desired packages from the new catalogue. Restore held records and deliberate customizations, update links to moved starters, rebuild affected Entries, and run Doctor.

Normal update may retain retired starter paths. Review these before removing them. Do not leave two independently changing copies of the same starter. Updating or removing a starter never updates or removes the artifacts created from it.

Don't keep and update the shared Workflow Support catalogue partway through this transition. That has produced an install block reporting that the catalogue changed after planning, so it isn't a supported migration path. The complete replacement sequence above was tested with an independently written Handoff, which came through byte for byte. A workspace with further edits or dependents still needs its own review of the plan.

This is a manual migration boundary, not a promise of automatic conversion. Review actual ownership and reported plans for the installed revision.

## Customize installed content

Edit an installed file when your workspace needs a different version. Managed updates recognize that divergence. For a small adjustment, an adjacent `{name}.overwrite.md` can hold the corresponding local change without editing the base.

An overwrite shares the base's role, scope, and loading behavior. It takes precedence only for the corresponding base content. It stays workspace-owned and is not a separately indexed route.

A Template works differently: copy and adapt it, then maintain the result independently. Updating the Template does not update copies made from it.

## Manual installation

The CLI is optional. To install a package by hand:

1. Review its manifest and the content of its dependencies.
2. Copy the files under each selected package's and dependency's `content/` into the target workspace, preserving existing changes and shared files. A dependency-only bundle has no files of its own to copy.
3. Keep affected entrypoints and `Entries` aligned with the assembled files. Follow the destination routes' rules and check links from the installed locations.
4. Review the diff and accept the result.

Manually copied files are usable Framework content. Copying them does not create managed ownership, so later CLI operations must not assume they were installed by the CLI.

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

Apply the reviewed command without `--dry-run`, then add complete files under `content/`. Create writes the manifest and an initial `content/.agents/` directory. It does not install the package into a workspace. Its `--path` selects the catalogue parent. `--workspace` does not redirect creation.

Use `--dependency <stable-id>` for each dependency and `--package-version <text>` to choose the version. Supply the package ID and destination explicitly in scripts. Creating a manifest records dependency IDs without proving they are available. Installation resolves them from the selected source.

## Package format

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

### Files used by other tools

Files outside `.agents/`, such as an agent definition consumed by another tool, require exact consumer grants in `.agents/open-forge.json` for managed installation. An interactive apply request can ask whether to remember a missing grant. The staged settings change is saved only during confirmed application. Dry runs, JSON output, redirected input, and automatic mode report missing grants without saving approval unless explicit `--allow-path` authority supplies them.

`--force` and `--prune` do not bypass those grants. Keep one manager per installed path. Installing a capability through another tool does not by itself satisfy an Open Forge package dependency.

## Check the assembled result

Preview installation into a separate workspace with the intended Framework and dependencies. Inspect the installed content, links, loading behavior, and any existing files the operation would affect. With the CLI available, use `open-forge doctor` and review the diff.

Structural checks show whether the package fits together. Trying its workflows, templates, or capabilities on representative work shows whether it earns its place. Keep those two kinds of evidence clear when describing a package.

See the [Extensions Architecture](../.agents/memory/crystallized/documents/extensions/architecture.md) for composition principles and the [Extension contracts](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md) for exact managed-operation rules.
