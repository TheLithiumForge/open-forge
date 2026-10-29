---
title: Share with your team
description: The three levels Open Forge shares context at, from one workspace, to one folder across several repos through a Library, to packages any project installs through an Extension.
---

import ThemedImage from "@theme/ThemedImage";
import useBaseUrl from "@docusaurus/useBaseUrl";

# Share with your team

Open Forge shares context at three levels: one workspace, a Library across several repos, and Extensions for any project. Each level reaches further than the one inside it, and you can stop at any of them.

<ThemedImage
alt="Three levels of sharing, drawn as rings. The inner ring is one repo, your workspace: the .agents context folder for agent context and outside routes such as docs for your own documents. The middle ring covers several repos through Libraries: a shared folder in the workspace, such as a Git submodule, linked into place. The outer ring covers people, teams, projects and organizations through Extensions: packages you install in any project."
sources={{
    light: useBaseUrl("/img/integration-levels-light.svg"),
    dark: useBaseUrl("/img/integration-levels-dark.svg"),
  }}
style={{ width: "100%", maxWidth: 560 }}
/>

| Level     | Reach                           | What you share                                                               |
| --------- | ------------------------------- | ---------------------------------------------------------------------------- |
| Workspace | One repo                        | The main root for agent context, and outside routes for your team's own docs |
| Library   | Several repos                   | One shared folder, such as a Git submodule, linked into place                |
| Extension | People, teams, projects or orgs | Packages of files that any project installs and then adapts                  |

## Workspace

Everyone who clones the repository gets the main root and its outside routes, so agents and people read the same rules. Your team writes its standards in `docs/` the way it already does, and you decide which of them are accepted sources. [Your own docs](your-own-docs.md) covers registering the folder.

Working records can stay on each machine. Essentials keeps new Working records out of normal Git commits and leaves `_working.md` eligible for sharing, as [Installation](installation.md#choose-the-installed-routes) describes.

## Library

A Library links a shared folder into place instead of copying its files. The source folder sits inside the workspace, e.g. a Git submodule at `vendor/team-rules` that several repos include.

1. Make sure `vendor/team-rules` exists and holds reviewed source files.
2. Preview the attach. This one projects the files into `docs/team` and keeps the links out of Git:

   ```sh
   open-forge library attach team-rules vendor/team-rules --to docs/team \
     --git-ignore true --allow-path docs/team --allow-path .gitignore --dry-run
   ```

3. Run the same command without `--dry-run` to apply it.
4. Check the links with `open-forge library inspect team-rules`.

Each projected file is a relative file symlink, so it always shows the shared folder's current content. The source files stay read-only input. When source files come and go, preview `open-forge library sync team-rules --dry-run`, then apply it. `--git-ignore true` keeps the links out of Git, so each checkout recreates its own. `--git-ignore false` commits them. A file that lands in a primary content location joins an existing route, so its folder and entrypoint must already exist.

[Share rules across repositories](../cli/flows.md#share-rules-across-repositories) walks through attach, sync and detach, and [Workspace libraries](/guides/cli#workspace-libraries) in the CLI reference covers every option.

## Extension

An Extension is a package of files that any project can install and then adapt like its own. The CLI has the first-party catalogue built in, and nothing is fetched from a registry.

To share your own standards across teams or organizations:

1. Create a package scaffold in an existing catalogue folder with `open-forge extension create team-rules --path catalogue --dry-run`, then run it without `--dry-run`.
2. Fill the package's `content/` with reviewed files.
3. Keep the catalogue where your teams can get it, such as a repository they clone next to their projects.
4. In each project, preview `open-forge extension install team-rules --source /path/to/catalogue --dry-run`, then apply it with the same `--source`.

The source catalogue has to be separate from the target workspace. An Open Forge checkout next to your project works. A source catalogue inside that project doesn't. The [Extension guide](/guides/extensions#create-an-extension) covers the package format, and [Extensions](../extensions/index.md) explains every first-party package.

**Next:** [Grow your own framework](grow-your-framework.md).
