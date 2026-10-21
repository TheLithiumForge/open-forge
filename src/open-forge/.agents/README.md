# Open Forge in this project

**You may delete this page.** It is a short guide for the people who run this project, and no agent or command depends on it. Agents follow [loader.md](loader.md), and they read [authoring.md](authoring.md) before they write workspace files. To keep this page deleted through `open-forge update`, add `.agents/README.md` to `removedFiles` in `open-forge.json`.

Open Forge tells agents where your rules and knowledge live. You decide what they use with the list under `## Entries` in the loader. Every file here is yours, and everything works without the CLI. The CLI's settings and ownership files support maintenance and give agents no authority.

## Use a document you already have

Your rules may already live somewhere, such as a contribution guide. Include it where it is:

```sh
open-forge route include CONTRIBUTING.md --description "Contribution rules to follow" --tag LoadNow
```

That adds this line to the loader, which you can also write by hand:

```markdown
- [Contribution rules to follow](../CONTRIBUTING.md) - #LoadNow
```

`#LoadNow` means agents read it before every task. Leave the tag off when agents should open the file only when its description fits the task. To limit a file to some work, add a pattern: `--apply-to "**/*.cs"` writes `- applies to` and the pattern at the end of the line.

Any text file works, including rules you wrote for other tools, such as `.cursor/rules/testing.mdc`. Open Forge ignores another tool's frontmatter and activation settings, so the line's tags and conditions decide when agents read the file. Include files one by one rather than whole folders. A Markdown file with its own `## Entries` list, such as `docs/team/_team.md`, brings its listed files along.

Including a file asks agents to read it. It doesn't make its claims accepted and doesn't let agents edit it. `open-forge route exclude CONTRIBUTING.md` removes the line and leaves the file alone.

## Write your first rule

The rules that matter most are the ones only your project knows. Write them down as Directives, one subject per file:

```sh
open-forge route create directives/exports --description "How data exports are formatted" --tag LoadNow --tag Directive
```

Then write the rule under `## Instructions`. [authoring.md](authoring.md) shows the shape. Start with one rule that agents keep getting wrong, and add more as you notice them. This is your framework: rename, restructure and replace anything here to fit how your team works.

## Add only what helps

Each folder holds one kind of content, and you add only the ones you want:

- **Directives** hold rules agents must follow.
- **Memory** holds notes that help work continue. Its states hold accepted knowledge (Crystallized), work in progress (Working), findings not accepted yet (Emerging) and history (Archived).
- **Guidance** offers advice for recurring choices.
- **Patterns** show reusable shapes for code and documents.
- **Templates** provide starting files to copy.
- **Maps** point to important sources.
- **Skills** expose capabilities through native `SKILL.md` packages.

Each Memory state needs Memory, but the states don't need each other. The saving policy needs Working Memory. Everything else needs only the loader, unless its own content says otherwise.

You don't have to adopt any of these. Existing ADRs and docs can stay where they are and be included. Named record kinds and workflows come from optional Extensions.

`open-forge install --configure` adds or leaves out these parts later, and `open-forge route init <category> --framework` restores one.

## Turn something off

Remove its line from the loader, or run `open-forge route exclude` with its catalogue, such as `.agents/patterns/_patterns.md`. The whole branch switches off and its files stay as notes. Remove or replace any part that depends on it first. Your accepted requirements, and instructions your agent tool loads on its own, still apply.

Inside a catalogue, the list follows the files on disk. To keep a file as a note there, move it out of the routed folder and remove its line. To get rid of it, delete the file and its line. When the CLI manages updates, record the removal too, so updates don't bring it back. Custom's `remove` choice only stops Open Forge managing the defaults. It keeps existing files and routes, and it doesn't switch them off.

To stop automatic progress saving, follow the note at the top of `memory/saving.md`. Emerging's capture of new findings is a separate choice, and it stays active while Emerging is listed with #KeepInMind.

## Reuse your knowledge elsewhere

When your rules prove useful, share them:

- **Include** uses a document already in this project, where it lives.
- A **Library** projects a folder registered in this workspace into another place through links.
- An **Extension** packages files you can install in any project.

## Find the details

- [loader.md](loader.md) says what agents read and how they use it.
- [authoring.md](authoring.md) describes the file shapes agents and the CLI read.
- `open-forge help concepts` explains every concept in one place.
- `open-forge <command> --help` shows a command's options.
- The website has the full guide: <https://thelithiumforge.github.io/open-forge/>.
