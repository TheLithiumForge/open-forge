---
open-forge:
  description: "Historical record: Proposed applyTo loading rules, cross-file behavior, frontmatter forms, Entries alternatives, and CLI filtering for Task 62"
  tags: [Memory, Analysis, Framework, Loading, Frontmatter, CLI, Contextual, Archived, Historical]
---

# Glob-Scoped Loading Analysis

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/glob-scoped-loading.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Status and question

Revised on 2026-09-29 from the maintainer's clarification.
[Task 62](../../../working/cli-development/tasks/task62-glob-scoped-loading.md)
records accepted direction and the delivery plan. This analysis proposes the
remaining semantics. It is not implementation authority or an active Loader rule.

The accepted requirements are optional `applyTo` at either frontmatter location
and path-based context filtering. Alternative roots and APM integration belong
to the separate [Task 55](../tasks/task55-alternative-root-decision.md).
The question here is how to make file conditions predictable while allowing
necessary related work outside the initially matched paths.

## Recommendation

Use `applyTo` to decide when context applies. It does not grant or restrict
permission to edit files. When related work reaches another file, add that path
to the task's working set and load its applicable context before working on it.
Keep the original rules scoped to their original files.

For example, C# rules matching `**/*.cs` load for a change to `src/Order.cs`.
If that change requires a TypeScript caller, add `web/order.ts` and load its
context. Both rule sets can be present without applying C# style to TypeScript.
Normal task authorization still determines whether the related change is in scope.

This follows the existing [loading invariant](../../../crystallized/documents/framework/routing/loading.md):
reading context does not create authority. It also preserves the existing
[scope model](../../../crystallized/documents/framework/routing/scope.md), where
selected branches retain separate inherited rules.

## Proposed Loader wording

The following is draft wording for discussion, not an instruction activated by
reading this analysis:

> `applyTo` is an optional list of workspace-relative file patterns. It limits
> where the source's context applies, not which files you may edit.
>
> Before working on a file, read matching entries exposed by loaded parents.
> Select relevant scopes as usual. A pattern does not activate a hidden ancestor.
>
> Patterns in one list are alternatives. A file must also match each condition
> inherited from its route ancestors. Without a local condition, inherit the
> parent's condition.
>
> Apply `LoadNow` and `KeepInMind` only when the condition matches a file involved
> in the task. If the files are not yet known, defer conditioned loading until
> they are known. Refresh matching `KeepInMind` context at the usual checkpoints.
>
> When related work reaches another file, include that file and load its context
> before working on it. A reference may be followed even when its destination
> does not match the referring source's condition. Reading a reference does not
> widen that condition or activate unrelated rules.

A short companion example should distinguish the files being worked on from
the Markdown files being read to obtain their context. Reading a C# Directive
does not itself turn every C# task into a task to edit Markdown.

## Working files and matching

A working file is a file the task needs to read as a subject of investigation,
create, change, delete, rename, or review. Listing paths or reading route
metadata to select context does not automatically add those context files.

Track paths for the current task, including related files discovered during
work. A path need not exist yet. For rename work, evaluate both the old and
new paths. Retain involved paths through validation and closeout so applicable
`KeepInMind` material is not dropped merely because an edit is finished.
An explicit task-scope change can remove paths. Starting a different task
establishes a new set.

The CLI is stateless: callers supply the complete current set through repeated
`--for` options. It does not infer that set from Git, inspect language dependency
graphs, watch files, or remember paths from an earlier invocation.

### Combining conditions

Use OR within one source's glob list and AND across conditioned ancestors.
Evaluate the complete chain against the same file, then include the source if
any working file passes that chain.

For a parent `src/**` and child `**/*.cs`, the effective condition is
C# files below `src/`. The child does not need to repeat the parent pattern.
A working set containing `src/readme.md` and `tests/Order.cs` must not activate
that child: neither individual file passes both conditions.

This replaces the earlier proposal that Doctor must prove a child's globs are
a subset of its parent's. Intersection gives the narrowing behavior directly,
without a general glob-containment solver.

An absent local field inherits the parent condition. An unconditioned route
chain keeps today's behavior. A matching entrypoint is opened before its
children, whose normal loading rules then apply. Overwrite companions share the
base source's effective condition and remain adjacent to it. They are not
independently matched or used to cancel inherited conditions.

### Tags and unknown files

| Source                    | Matching working file                    | Known files, none matching                 | Working files unknown        |
| ------------------------- | ---------------------------------------- | ------------------------------------------ | ---------------------------- |
| No effective condition    | Existing tag or on-demand behavior       | Existing behavior                          | Existing behavior            |
| Condition, no loading tag | Open when exposed by a loaded parent     | No automatic load                          | Defer automatic load         |
| Condition with LoadNow    | Open when exposed                        | No automatic load                          | Defer automatic load         |
| Condition with KeepInMind | Open, then refresh at normal checkpoints | No automatic load or refresh for that task | Defer until a match is known |

`LoadNow` is redundant for the first matching read of a conditioned source.
Keeping it valid avoids inventing a tag incompatibility. `KeepInMind` adds its
existing refresh obligation. A source with both tags follows that same rule.

Unknown is not the same as a proven mismatch. Without `--for`, Context should
preserve unconditioned behavior and visibly report encountered conditioned
sources as pending file selection. It must not claim to have returned every
required rule for an unspecified file set.

For planning or research with no file set, ordinary relevance selection still
works. Explicitly reading a conditioned source makes its information available
without pretending a file match exists.

## References and necessary side effects

References retain their current meaning. They can identify a dependency,
explanation, authority, or another source to inspect. A link alone does not
extend either source's scope and does not add every linked file to the work.

If implementation requires changing a linked file or a caller outside the
current globs, add that file to the working set and load the rules for it.
Do the same when editing the `applyTo` source itself. Its glob describes the
files its content governs, not whether that Markdown file may be maintained.

An explicit instruction to consult another source remains followable.
If a required binding instruction and a declared condition disagree materially,
report that actual conflict. Do not manufacture an exception that silently
widens the glob. Related inspection and ordinary in-scope edits need no special
permission merely because they cross a glob boundary.

Propose that explicit Context source operands and `--follow-links` can still
retrieve nonmatching sources, with the reason labeled as explicit selection or
reference inspection. Include their necessary route context, but distinguish
inspection from activation. Such retrieval must not activate nonmatching
automatic children. Without `--for`, existing behavior on unconditioned
sources remains unchanged.

## Frontmatter

Both accepted locations have identical meaning:

```yaml
---
applyTo: ["**/*.cs", "**/*.csproj"]
open-forge:
  description: C# design rules
  tags: [Directive, CSharp]
---
```

```yaml
---
open-forge:
  description: C# design rules
  tags: [Directive, CSharp]
  applyTo: ["**/*.cs", "**/*.csproj"]
---
```

Recommend the following remaining choices:

- Use a quoted YAML string for one pattern or a list of quoted strings. Emit a
  list when creating new fields, preferably under `open-forge:`.
- Read root `applyTo` even when the scoped block exists without that field.
  Do not require the entire scoped block to be absent.
- Preserve the authored location during unrelated updates. Index projects
  Entries and does not migrate or rewrite frontmatter.
- If both fields contain the same normalized set, treat them as one condition.
  If they differ, report ambiguous metadata. Do not silently prefer one or
  union them. Context reports incomplete selection and mutations needing that
  metadata stop through their existing error boundary.
- Missing means no added condition. Empty lists, empty strings, nulls, and
  non-string items are invalid rather than an accidental match-everything form.
- Keep other metadata rules unchanged. Accepting root `applyTo` does not
  implicitly accept root `description`, `tags`, or `responsibility`.
- Initially use lists for multiple patterns. Do not silently split strings at
  commas, which can be literal filename characters. Any foreign-format import
  normalization belongs to a separately specified adapter.

Recommend workspace-relative patterns with `/` separators, `*` within a segment,
`**` across zero or more segments, and `?` for one non-separator character.
Use case-sensitive matching on every platform. Reject absolute patterns,
parent traversal, and unsupported pattern syntax. Initially defer braces,
character classes, negation, and escaping syntax. New file paths must be
matchable without disk existence checks. Resolve concrete CLI path operands
consistently against the selected workspace and reject outside-workspace paths.

The exact grammar and supported matcher must be checked before implementation.
Prefer a supported existing library surface if it fits the accepted contract.
Do not assume a custom glob engine is required.

## Entries display

All candidates keep one physical line and preserve the descriptive link.
Examples below are proposed syntax, not current Entries grammar.

**Recommended: labeled segment after tags.**

```md
- [C# rules](csharp/_csharp.md) - #Directive #KeepInMind - applies to `**/*.cs`, `**/*.csproj`
```

**Alternative: labeled segment before tags.**

```md
- [C# rules](csharp/_csharp.md) - applies to `**/*.cs`, `**/*.csproj` - #Directive #KeepInMind
```

**Alternative: parentheses after tags.**

```md
- [C# rules](csharp/_csharp.md) - #Directive #KeepInMind (applies to `**/*.cs`, `**/*.csproj`)
```

The first keeps today's link-and-tags prefix and gives the condition a distinct
slot. The second puts applicability earlier but separates tags from their
existing position. Parentheses render cleanly but make the boundary between
tags and the condition less explicit. Bold labels would add visual weight
without adding meaning. A `#AppliesTo` marker with arguments would introduce
a new kind of tag, which is unnecessary for a first version.

Keep patterns inside code spans in all variants. Avoid hidden comments or link
titles: the condition should remain visible in rendered Markdown. Choose one
canonical shape rather than making all alternatives valid input immediately.

Project the source's declared patterns, not a synthesized flattened inherited
expression. The route chain supplies inheritance, while Route Inspect explains
the effective condition. Unconditioned source lines keep their current shape.

## CLI boundaries

The installed `context --help` currently exposes explicit sources, content
selection, additions-only, and link following. It has no `--for` option.
The following are proposals, not commands available today.

| Command                    | Proposed responsibility                                                                                                                        |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Context                    | Repeatable `--for <path>`. Return startup and selected-route context, include exposed matching sources, explain matches and pending conditions |
| Find                       | Repeatable `--for <path>` as a discovery filter over its existing source universe and include/exclude boundaries                               |
| Route Inspect              | Optional repeated `--for <path>` to explain declared/inherited conditions and why each supplied path matches or does not                       |
| Route Create, Init, Update | Author the optional field through repeated `--apply-to`; define a separate explicit clear operation for Update                                 |
| Index                      | Project the agreed condition syntax into all relevant Entries, independent of any current working set                                          |
| Doctor                     | Validate metadata and syntax across its normal scope; do not suppress problems in currently nonmatching routes                                 |
| Status                     | No required first-version change                                                                                                               |

Context stays top-down. A matching visible entry may select its own route,
but a glob buried below an unselected ancestor does not globally activate
that ancestor. An explicit source operand may establish the required chain.
Find may discover candidates across its configured universe without activating
them. The caller then selects the relevant route for Context.

This makes the boundary honest: `context --for` pre-filters within startup and
selected routes. It does not guarantee discovery of every hidden rule or code
dependency in the repository. Global automatic selection would be a separate
loading decision and would change the earlier no-hidden-ancestor proposal.

Recommend that Find treats sources without any effective condition as
file-compatible, then applies existing tag/heading predicates. Inherited
conditions still count when the leaf has no local field. Find remains an
inventory, not an ordered context pack. `--for` is an additional filter,
independent of `--require all|any` for tags and headings.

Each selected Context source should explain its inclusion reason and, when
applicable, the matching path and declared/inherited conditions. Keep detailed
nonmatch traces in inspection or fuller output. Do not build a second persisted
context index or automatically calculate callers and references.

Do not treat a valid glob matching no current disk file as an error: it may
describe files a task will create. Counts may be informational. Update clearing,
`--additions-only` with `--for`, content projection, and explicit/link inclusion
must receive concrete command-contract examples before coding.

## Behavior examples to freeze

| Situation                                               | Expected proposed behavior                                               |
| ------------------------------------------------------- | ------------------------------------------------------------------------ |
| No applyTo anywhere in selected routes                  | Existing context and generated Entries behavior remains intact           |
| Root applyTo beside scoped description/tags             | Read the condition normally                                              |
| Equivalent fields at both locations                     | One condition, no duplicate loading                                      |
| Conflicting fields                                      | Diagnose ambiguity; do not report complete filtered context              |
| Matching entry visible under loaded parent              | Open it before working on the matched file                               |
| Matching leaf below hidden unselected parent            | Remain unselected until the relevant chain is selected                   |
| Parent src/**, child **/*.cs, file src/Order.cs         | Load child after parent                                                  |
| Same conditions, files src/readme.md and tests/Order.cs | Neither file satisfies the whole chain; do not load child                |
| C# change requires a TypeScript caller                  | Add caller, select its context, retain C# rules for C# only              |
| Matched context links to a nonmatching reference        | Allow inspection; link does not widen applicability                      |
| Editing the Markdown file declaring applyTo             | Load rules for that Markdown path; its own glob does not prevent editing |
| New or renamed file                                     | Match planned path, and both old/new paths for a rename                  |
| No file paths known                                     | Load unconditioned baseline; disclose deferred conditions                |
| Matching KeepInMind source at closeout                  | Refresh while its involved paths remain in task scope                    |
| Base with overwrite companion                           | Load companion immediately after eligible base under the same condition  |
| Index run during a C# task                              | Retain all Entries, including non-C# sources                             |
| Doctor run during a C# task                             | Do not hide invalid TypeScript metadata                                  |

## Evidence and limits

This revision uses the current Loader, Framework syntax and loading documents,
the archived Task 55 record, and installed Context/Find command help. It is
design analysis, not a code audit or tested prototype. No new vendor research
was needed to define these repository-local recommendations.

The earlier analysis compared vendor formats on 2026-09-28. That evidence is
retained below for provenance only. Its external claims have not been refreshed
in this pass. They do not establish native support for arbitrary Open Forge
files, and adapter/root recommendations there belong to Task 55. The proposals
above supersede the earlier field precedence, comma splitting, glob subset
checking, and unconditional no-current-match diagnostics.

## Next decision

Discuss the Loader rules and choose the Entries form. Then freeze the metadata
and command examples before any implementation. The task's staged plan includes
documentation at the beginning, during changes, and at final acceptance.

## Historical vendor evidence

The following is the comparison recorded on 2026-09-28, retained unchanged.
Only that comparison is retained here, not the earlier adapter design or
root-level metadata recommendations.

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
