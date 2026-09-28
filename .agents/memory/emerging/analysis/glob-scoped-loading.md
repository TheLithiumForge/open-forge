---
open-forge:
  description: Analysis for Task 62 of an applyTo file glob on routed sources, its Markdown-safe syntax in frontmatter and Entries, its loading semantics, CLI support, use cases, and limits
  tags: [Memory, Analysis, Framework, Loading, Frontmatter, Tags, CLI, Contextual, Candidate]
---

# Glob-Scoped Loading Analysis

## Question

Should a routed source be able to say which files it applies to, with a glob,
so it opens when a task works on matching files? If so, what syntax keeps it
fully Markdown-compatible, how does it combine with scopes and the loading
tags, which commands support it, and where does it help?

**Scope:** the authored frontmatter field, its projection into generated
`Entries`, loading semantics, CLI support, use cases, and limits. Root-level
frontmatter is covered where it affects the field. Harness adapters that write
files for other tools are noted, not designed. Owner:
[Task 62](../../working/cli-development/tasks/task62-glob-scoped-loading.md).

## Current Conclusion

A recommendation, not accepted direction.

1. **Add an optional `applyTo` field** to the `open-forge:` frontmatter: a
   YAML list of quoted, workspace-relative globs. `applyTo` is the spelling
   GitHub Copilot and APM already use, so files written for those tools can be
   routed without renaming the key.
2. **Project it into the entry line** as its own segment after the tags, with
   each glob in a code span:

   ```md
   - [C# design and style rules](csharp/_csharp.md) - #Directive #CSharp - applies to `**/*.cs`, `**/*.csproj`
   ```

   Code spans are the one Markdown construct that shows `*`, `**`, and `_`
   literally in every CommonMark renderer, so the line stays readable and
   parseable. A tag-like token such as `#AppliesTo:**/*.cs` is not Markdown-safe.

3. **Make it a trigger and a filter, never a way around scopes.** An entry with
   `applyTo` in a loaded entrypoint must be opened before the agent reads,
   changes, creates, or reviews a matching file. A `LoadNow` entry with
   `applyTo` loads only once the task involves a matching file. A glob never
   activates an unselected ancestor.
4. **Support it in the CLI** through authoring flags, `index`, `doctor`,
   `route inspect`, and a new `--for <path>` selector on `context` and `find`
   that states which glob matched which file.
5. **Get native harness support through small pointer files, later.** Nearly
   every tool reads glob rules only from its own folder. Antigravity is the one
   exception found: it reads glob rules from `.agents/rules/`. A later adapter
   could write one small rule per glob-scoped route into each tool's folder,
   holding the tool's glob key and a line that points to the Open Forge source.
   APM already does this translation for its own instructions.

## Evidence And Reasoning

### What Open Forge has today

| Evidence or source                                                                        | What it establishes                                                                                                                        | Limit or counterevidence                                                                                                                          |
| ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Loader](../../../loader.md#tags-and-loading)                                             | Loading narrows by scope selection and the `LoadNow` and `KeepInMind` tags. Tags act only through loaded parents.                          | Nothing ties a route to the files a task touches. Selection relies on descriptions.                                                               |
| [Canonical syntax](../../crystallized/documents/framework/markdown/syntax.md#frontmatter) | Frontmatter uses the `open-forge:` scope with `description`, optional `responsibility`, and `tags`. Tools fail closed on unclear metadata. | A new key must join this contract and its compatibility boundary.                                                                                 |
| [Route Entries](../../crystallized/documents/framework/markdown/routes.md#route-entries)  | One canonical line: `- [Description](path) - #Tag ...`, one physical line per entry.                                                       | The grammar has no slot after the tags yet.                                                                                                       |
| `SourceGeneratedEntriesParser.cs` in `src/cli/framework/`                                 | The CLI reads entry lines with a string parser: label, destination, then an optional `-` tag suffix.                                       | A new segment needs a grammar change and parser tests.                                                                                            |
| `Directory.Packages.props`                                                                | The CLI references Markdig, YamlDotNet, and System.CommandLine. No glob library.                                                           | Matching needs a small in-house matcher or a new dependency under the [dependency policy](../../crystallized/decisions/cli-dependency-policy.md). |
| `SKILL.md` indexing                                                                       | Open Forge already reads a root-level `description` for native Skill packages.                                                             | That is a native-format exception, not a general rule.                                                                                            |

### Why the entry line needs code spans

Globs contain `*` and often `_`, which Markdown treats as emphasis. In a raw
entry line, `**/*.cs` next to other asterisks can render as bold or italic text
and lose characters. The candidates compare like this:

| Candidate                  | Example                            | Markdown-safe                                                             | Readable                             | Verdict                                                                                                                 |
| -------------------------- | ---------------------------------- | ------------------------------------------------------------------------- | ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------- |
| Tag-like token             | `#AppliesTo:src/**/*.cs`           | No: `*` and `**` can become emphasis, and `:` and `/` break the tag rules | Moderately                           | Reject                                                                                                                  |
| Escaped glob               | `applies to src/\*\*/\*.cs`        | Yes                                                                       | Poor, and easy to get wrong          | Reject                                                                                                                  |
| Code spans after a label   | ``applies to `src/**/*.cs` ``      | Yes: code spans are literal                                               | Good                                 | Recommend                                                                                                               |
| Marker tag plus code spans | ``#AppliesTo `src/**/*.cs` ``      | Yes                                                                       | Good                                 | Keep as the alternative                                                                                                 |
| Link title                 | `[desc](path "applies to src/**")` | Yes                                                                       | Hidden when rendered                 | Reject: invisible to readers of rendered pages                                                                          |
| HTML comment               | `<!-- applies-to: src/** -->`      | Yes                                                                       | Hidden, and agents may skip comments | Reject: [Task 36](../../working/cli-development/tasks/task36-extension-merge-and-guards.md) is retiring comment markers |
| Nested list item           | `  - applies to ...`               | Yes                                                                       | Good                                 | Reject: entries must stay on one physical line                                                                          |

The marker-tag form keeps the glob inside the tag segment, which is close to
what the maintainer described. It also makes `find --tag=AppliesTo` work for
free. Its cost is that a tag would start taking arguments, which the tag
grammar doesn't allow today, and a reserved tag with arguments is a new concept
for every reader. A labeled segment adds a grammar slot but keeps tags bare.
Both are Markdown-safe, so this choice is about grammar clarity, not
compatibility.

### Frontmatter form

```yaml
---
open-forge:
  description: C# design and style rules for source and tests
  tags: [Directive, CSharp]
  applyTo: ["**/*.cs", "**/*.csproj"]
---
```

- **Quote every glob.** In YAML, an unquoted value that starts with `*` is an
  alias reference, so `[**/*.cs]` fails to parse. Canonical output always
  quotes.
- **Use a list.** One glob per item avoids comma-splitting rules. Compatible
  input may also accept a single string or a comma-separated string, as
  Copilot, APM, and Claude Code do.
- **Name it `applyTo`.** Copilot and APM use this spelling, and APM is the tool
  [Task 55](../../archived/cli-development/tasks/task55-alternative-root.md)
  evaluates as an alternative root. The plain-English alternative is
  `applies-to`. The spellings other tools use are compared under
  [harness evidence](#harness-evidence).

### Glob dialect

A small portable subset keeps matching predictable for people, agents, and
the CLI:

- Paths are relative to the workspace root, the folder that contains `.agents/`.
- `/` separates segments on every platform.
- `*` matches within one segment, `**` matches any number of segments, and `?`
  matches one character.
- No braces, character classes, or negation in the first version. `{ts,tsx}`
  becomes two globs. Claude Code, Cline, and APM accept braces, but Copilot's
  documentation shows only `*` and `**`, so the subset is the pattern every
  tool reads the same way.
- Matching is case-sensitive, so a result doesn't depend on the operating system.
- A glob may not be absolute, start with `/`, or contain a `..` segment.

### Semantics

- **Trigger.** When a loaded entrypoint lists an entry with `applyTo`, the
  agent opens that entry before it reads, changes, creates, or reviews a file
  whose path matches. Other tools use the same trigger: Copilot applies a rule
  to a file the agent creates or modifies, Claude Code when it reads one, and
  Windsurf when it reads or edits one. This is more reliable than judging
  relevance from a description.
- **Filter with `LoadNow`.** `LoadNow` plus `applyTo` means: load when the
  parent loads and the task involves a matching file. When the files aren't
  known at startup, it waits until the first matching file.
- **Scopes stay in charge.** A glob acts only through a loaded parent, like the
  loading tags. It never activates an unselected ancestor or scope.
- **Inheritance narrows.** A scope's globs apply to everything beneath it. A
  child may narrow them, never widen them. Without `applyTo`, a route has no
  file condition, exactly as today.
- **Not access control.** A glob says where guidance applies. It doesn't limit
  what an agent may edit.

### CLI support

| Command                                      | Proposed support                                                                                               | Why                                                               |
| -------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `route create`, `route init`, `route update` | Repeatable `--apply-to <glob>`. On update, the list replaces the previous one, like `--tag`.                   | Authoring without hand-editing YAML quoting                       |
| `index`                                      | Projects the field into the entry line                                                                         | Agents see the condition without opening the file                 |
| `doctor`                                     | Flags invalid globs, globs that match no current file, and children that widen a parent's globs                | Catches typos, which fail silently otherwise                      |
| `route inspect <source>`                     | Shows the declared and inherited globs, and how many current files match                                       | Answers "why did or didn't this load?"                            |
| `context --for <path>`                       | Adds the sources whose effective globs match the path. Each added source names the glob and file that matched. | Deterministic matching, and it states where the context came from |
| `find --for <path>`                          | Filters sources by glob match. Combines with `--tag` and `--heading`.                                          | Finds the rules for a file before editing it                      |
| `status`                                     | Counts glob-scoped routes and those matching nothing                                                           | Health at a glance                                                |

`context --for` matters most. It turns glob matching from something an agent
estimates into something the CLI computes. It also lets a harness hook or a CI
job ask what applies to the files in a change.

### Use cases

- **Language rules.** This repository's C# scope, `directives/csharp`, could
  apply to `**/*.cs` and `**/*.csproj`. The TypeScript scope,
  `directives/open-forge/typescript`, could apply to `**/*.ts`. Today both are
  chosen by description.
- **Monorepo areas.** `apps/web/**` for frontend rules and Maps, and
  `services/billing/**` for billing architecture.
- **Tests.** `**/*.test.ts` or `**/tests/**` for a testing Pattern.
- **Generated or vendored code.** `**/generated/**` with a Directive to
  regenerate instead of editing by hand.
- **Database changes.** `**/migrations/**` for migration rules.
- **Memory tied to code.** A Decision or Document about payments that applies
  to `src/payments/**` surfaces whenever payments code changes. That keeps
  reasons next to the code they explain, which helps most in brownfield work.

### Limits

- It helps only when a task has a known set of files. Planning, discussion,
  and research tasks still select by description and scope.
- Without the CLI, the agent matches globs itself, so the dialect has to stay
  simple enough to match by reading.
- Each glob adds tokens to an entry line that may load at startup. A few globs
  per route is the practical limit.
- Libraries attached from another repository hold globs written for that
  repository. They resolve against the consuming workspace's root, so shared
  content should prefer patterns like `**/*.cs` over fixed paths.

## Harness Evidence

Read from each vendor's official documentation on 2026-09-28. Facts marked
inferred were not stated outright.

| Tool                                        | Where rules live                                                                      | Glob key                                        | Value format                                                  | When a glob rule applies                                                                  | `description` use                                 |
| ------------------------------------------- | ------------------------------------------------------------------------------------- | ----------------------------------------------- | ------------------------------------------------------------- | ----------------------------------------------------------------------------------------- | ------------------------------------------------- |
| GitHub Copilot in VS Code                   | `.github/instructions/*.instructions.md`, and `.claude/rules` for Claude-format files | `applyTo`, or `paths` in Claude format          | Glob string                                                   | A file the agent creates or modifies matches                                              | Loads the rule on demand when it matches the task |
| GitHub Copilot on GitHub.com and in the CLI | `.github/instructions/**/*.instructions.md`                                           | `applyTo`, required, plus `excludeAgent`        | Quoted, comma-separated string                                | Copilot works on a matching file                                                          | None documented                                   |
| APM                                         | `.apm/instructions/*.instructions.md`                                                 | `applyTo`, required                             | String, comma-separated string, or YAML list                  | Translated into each tool's format, or compiled into `AGENTS.md` beside the matching code | Required, used in compiled indexes                |
| Cursor                                      | `.cursor/rules/*.mdc`                                                                 | `globs`, plus `alwaysApply`                     | Comma-separated string                                        | A matching file is in context                                                             | The agent decides relevance from it               |
| Claude Code                                 | `.claude/rules/**/*.md`                                                               | `paths`, the only field it reads                | YAML list or comma-separated string, with braces and brackets | Claude reads a matching file                                                              | Ignored                                           |
| Windsurf                                    | `.devin/rules`, falling back to `.windsurf/rules`                                     | `globs`, with `trigger: glob`                   | String                                                        | Cascade reads or edits a matching file                                                    | Shown for `model_decision` rules                  |
| Kiro                                        | `.kiro/steering`                                                                      | `fileMatchPattern`, with `inclusion: fileMatch` | String or list                                                | Work involves a matching file                                                             | Used, with `name`, for `auto` rules               |
| Cline                                       | `.clinerules/`                                                                        | `paths`                                         | YAML list, with braces                                        | The current files match                                                                   | None documented                                   |
| Continue                                    | `.continue/rules`                                                                     | `globs`, plus `regex`                           | String or list                                                | Files in context match                                                                    | The agent can request the rule                    |
| Antigravity                                 | `.agents/rules/*.md`                                                                  | `globs`, with `trigger: glob`                   | Not stated                                                    | Not stated                                                                                | Not stated                                        |
| Gemini CLI, OpenAI Codex, `AGENTS.md`       | Directory files                                                                       | None                                            | None                                                          | Nearest file by directory                                                                 | None                                              |

Sources: [VS Code custom instructions](https://code.visualstudio.com/docs/copilot/customization/custom-instructions),
[GitHub repository instructions](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions),
[APM instructions](https://microsoft.github.io/apm/producer/author-primitives/instructions-and-agents/)
and [targets](https://microsoft.github.io/apm/reference/targets-matrix/),
[Cursor rules](https://cursor.com/docs/context/rules),
[Claude Code memory](https://code.claude.com/docs/en/memory),
[Windsurf rules](https://docs.devin.ai/desktop/cascade/memories),
[Kiro steering](https://kiro.dev/docs/steering/),
[Cline rules](https://docs.cline.bot/features/cline-rules), and
[Continue rules](https://docs.continue.dev/customize/deep-dives/rules). The Antigravity row comes from APM's targets page and was not checked against Antigravity's own documentation.

What the evidence establishes:

- **No shared key exists.** Four spellings are in use: `applyTo`, `paths`,
  `globs`, and `fileMatchPattern`. `applyTo` has the two users closest to Open
  Forge: Copilot and APM.
- **The trigger is the same everywhere:** a rule applies when the agent works
  with a matching file. The proposed semantics match that.
- **The activation modes line up with Open Forge.** Most formats offer four
  modes: always on, by file match, by description, and manual. Open Forge
  already has three of them: `LoadNow` entries, on-demand entries chosen by
  description, and explicit links or requests. A glob adds the missing one.
- **Native support mostly needs files in each tool's own folder.** Antigravity
  reads glob rules from `.agents/rules/`, but every other tool compared reads
  them only from its own folder. VS Code's setting for extra instruction
  folders, `chat.instructionsFilesLocations`, is deprecated. Claude Code reads
  `AGENTS.md` only when there is no `CLAUDE.md`. A field in Open Forge
  frontmatter therefore works through the Open Forge loader and CLI, and
  reaches most harnesses' native matching only through generated pointer
  files.
- **`.agents/skills/` is becoming a shared folder.** APM's targets page lists
  Copilot, Cursor, Codex, Gemini, OpenCode, Windsurf, and Hermes as reading
  Skills from `.agents/skills/<name>/SKILL.md`. That is the folder Open Forge
  already uses, so its Skills, such as `open-forge-cli`, are likely discovered
  natively by those tools without any adapter. This is worth confirming with
  each tool and documenting.
- **Glob dialects differ at the edges.** Claude Code, Cline, and APM accept
  braces. Copilot's pages show only `*` and `**`. None documents negation or
  case sensitivity. The minimal subset is the safe common ground.
- **Not confirmed:** how VS Code accepts several patterns in one `applyTo`, and
  the glob base path for GitHub, Cursor, and Windsurf, which is most likely the
  repository root.

### Pointer files for native support

A later adapter could write, for each glob-scoped route, a small rule into
each harness folder the user enables. For Claude Code:

```md
---
paths: ["**/*.cs", "**/*.csproj"]
---

Read `.agents/directives/csharp/_csharp.md` and follow it before changing C# files.
```

The Open Forge source stays the single definition. The pointer file carries
only the tool's glob key and one instruction, so it can be regenerated by
`index` or a dedicated command and checked by `doctor`. APM already translates
`applyTo` into Copilot, Claude Code, Cursor, Windsurf, and Kiro formats, so a
workspace that uses APM might get this for free, which ties into
[Task 55](../../archived/cli-development/tasks/task55-alternative-root.md).

### Root-level frontmatter

The evidence favors accepting a small set of root-level keys as compatible
input:

- Copilot, APM, Cursor, Claude Code, and Kiro all put `description` and their
  glob key at the root of the frontmatter.
- Open Forge already reads a root-level `description` for native `SKILL.md`
  packages.

A proposed rule: the `open-forge:` block stays canonical, and `index` always
writes it. When a routed file has no `open-forge:` block, readers accept root
`description` and `applyTo` instead. When both are present and disagree,
`doctor` reports the conflict and the scoped values win. `tags` stays scoped
only, because other tools don't use it and a root `tags` key could belong to
another tool.

## Alternatives And Assumptions

- **Scopes only (status quo).** Scopes already narrow by subject. They can't
  react to the files a task touches, and they depend on the agent reading a
  description the same way each time.
- **Harness-native files.** Writing each rule as a Copilot or Cursor rule file
  gives native matching in that tool only, and duplicates content per tool.
- **Assumption:** agents follow a simple "open before touching a matching file"
  rule reliably. The demos in [Task 58](../../working/cli-development/tasks/task58-demo-evals.md)
  could test this by comparing a glob-scoped Directive with a description-only
  one.

## Next Check

**Action:** the maintainer chooses the entry-line form (labeled segment or
marker tag), the key name (`applyTo` or `applies-to`), and whether root-level
`description` and `applyTo` are accepted as input. Then prototype the frontmatter field, the entry
projection, and `context --for` on the C# and TypeScript scopes in this
repository, and run one demo with and without the glob.

**Would change the conclusion:** evidence that agents ignore glob triggers in
practice, or that a dominant harness format makes a shared key much more
valuable than a readable one.

**Acceptance needed:** the maintainer, through a recorded Framework Decision
before any contract or CLI change.
