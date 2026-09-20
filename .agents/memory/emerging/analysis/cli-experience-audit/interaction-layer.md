---
open-forge:
  description: The wizard does not exist, confirmation is asked before the plan is shown, and error paths point at help instead of answers
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Interaction, Wizard, Help, Errors]
---

# Interaction Layer

## Conclusion

There is no wizard anywhere in the CLI. What is called a wizard is a
`ReadLine` loop that asks you to type words like `skip`, `back`, or an exact
package ID. No command reads a keystroke, draws a selection list, or supports
arrow keys or space-to-select. The word should either be removed or implemented.

Separately, `install` asks for confirmation of a plan it has not yet shown, and
every invalid-input path routes the user to `--help`, which cannot answer the
question that was asked.

## There is no wizard

The complete interaction primitive is 40 lines:

```csharp
// src/cli/core/OpenForge.Cli.Core/Shell/Interaction/CliInteractiveSession.cs
await _promptOutput.WriteAsync(prompt.AsMemory(), cancellationToken);
await _promptOutput.FlushAsync(cancellationToken);
var answer = await _standardInput.ReadLineAsync(cancellationToken);
return new CliInteractiveResponse { Answer = answer };
```

`Console.ReadKey` and `ConsoleKey` appear nowhere in non-test source. The only
ANSI escapes in the codebase are five foreground colours in `CliHumanStyle.cs`.
There is no cursor movement, no line redraw, no alternate screen, no raw mode.

`RepairWizard.SelectAsync` is a `while` loop over proposals, and the accepted
answers are the literal strings `cancel`, `back`, `skip`, `select`, an empty
line, or an ordinal number. That is a 1980s prompt sequence. Calling it a wizard
sets an expectation the code cannot meet, which is exactly why it reads as
broken.

`extension install` is worse, because its selection cannot express the thing
users want:

```csharp
// ExtensionInstallSelectionResolver.cs:113
var prompt = $"Available Extension packages:{NewLine}{ids}{NewLine}Select one exact stable ID or all: ";
...
while (true)
{
    var response = await _interactiveSession.AskAsync(prompt, ct);
    if (string.Equals(response.Answer, "all", StringComparison.Ordinal)) { ... }
    if (ids.Contains(response.Answer, StringComparer.Ordinal)) { ... }
    // no else — silently re-prompts forever
}
```

Three defects in nine lines:

- **One or all, nothing between.** You cannot select two of six packages. The
  one interaction where multi-select is the entire point does not support it.
- **You must retype an exact ID.** Ordinal-sensitive, case-sensitive, no
  completion, no numbering.
- **A typo loops forever with no message.** The `while (true)` has no `else`
  branch, so a wrong answer redraws the identical prompt with no explanation.
  The user cannot tell whether the CLI is hung.

### Proposed result

Either implement a real selector or rename the concept. If implementing:

```
Select Extension packages                          space to toggle · enter to confirm

  [x] development           Debugging and review workflows
  [ ] development-toolkit   Documents, memory starters, planning  (5 packages)
  [x] memory-starters       Starter memory documents
  [ ] ...

  2 selected · 6 files · esc to cancel
```

That needs `Console.ReadKey(intercept: true)`, cursor-up redraw, and a
non-interactive fallback (which already exists as explicit IDs and `--all`).
It is contained work; the selection model behind it already exists.

If not implementing now, rename `interactive-wizard` to `interactive-prompt`
everywhere it appears in output and help, and fix the loop to say
`Not a listed package ID. Type one of the IDs above, "all", or "cancel".`

## Confirmation is asked before the plan is shown

```csharp
// InstallOperation.cs:20
private const string ConfirmationPrompt = "Apply this Install plan? [y/N] ";
```

The prompt is issued inside the operation. The result — including the entire
`Effects:` list that constitutes the plan — is rendered by the presenter only
after the operation returns. So the observable sequence is:

1. Nothing.
2. `Apply this Install plan? [y/N]`

There is no plan on screen when the question is asked. This is a strict safety
regression over `--dry-run`, which shows the plan and asks nothing.

### Proposed result

Render the plan, then ask, and make the question specific about what changes:

```
Install the Open Forge Framework into D:\work\myrepo

  21 files, 20 directories, 2 managed regions in existing files
  AGENTS.md and CLAUDE.md gain an Open Forge section; existing content is kept.

  Nothing else is modified. Run with --dry-run to see every path.

Proceed? [y/N]
```

## Errors point at help instead of at answers

Every invalid-input path ends the same way, and `--help` cannot answer any of
these questions.

| Input                               | Output                                                                                                          | What the user needed                               |
| ----------------------------------- | --------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| `open-forge statuss`                | `Unrecognized command or argument 'statuss'.` and nothing else                                                  | `Did you mean "status"?`                           |
| `open-forge status --deep`          | `Unrecognized command or argument '--deep'.`                                                                    | the nearest valid flag                             |
| `open-forge route list memries`     | `Selection: source ID memries -> none at none` then `Next: open-forge route list --help`                        | the list of valid IDs, or `Did you mean "memory"?` |
| `open-forge route create x`         | `requires one nonblank --description value`, then on retry `requires one or more unique canonical --tag values` | both, at once                                      |
| `open-forge status --view Expanded` | `Argument 'Expanded' not recognized.` with a raw tab, no header                                                 | help should not have printed `[default: Expanded]` |

Three specific problems.

**`Next:` is wrong for identity errors.** `--help` lists options; it cannot
enumerate the workspace's source IDs. The useful next command is
`open-forge route list`, or better, an inline suggestion. Levenshtein distance 1
covers `memries` → `memory` and `statuss` → `status`.

**Validation is sequential.** `route create` knew both `--description` and
`--tag` were missing on the first run and reported one. Report all invalid
inputs in one pass.

**A `Next:` command that does not work.** `route init` emits
`Next: open-forge route update`, which is missing its required
`<source-reference>` operand. Copy-pasting it fails.

### Proposed result

```
$ open-forge route list memries
No source with ID "memries".

  Did you mean:  memory
                 memory/emerging

  See all 20 IDs:  open-forge route list --depth=all
```

```
$ open-forge route create memory/emerging/ideas/tiers
Cannot create the routed file — two required values are missing:

  --description   one sentence describing what the file is for
  --tag           one or more tags, e.g. --tag Idea

  open-forge route create memory/emerging/ideas/tiers \
    --description "..." --tag Idea
```

## Help structure

`index --help` is 65 lines. Its single command-specific option is buried among
six global ones with no separation:

```
Options:
  --dry-run           Preview all generated Entries changes without writing files.
  --workspace <path>  Select the workspace explicitly.
  --json              Write JSON; --view selects compact or expanded detail.
  --view <view>       Result detail: compact or expanded. [default: Expanded]
  --verbose           Write diagnostic details to stderr.
  --help              Show help and exit.
  --version           Show the executable version and exit.
```

The globals are then re-listed further down in a prose section called
_Inherited global options_, so they appear twice.

Below that, every single command repeats the same 20-line block describing the
stream policy and the seven exit codes. That content is workspace-global and
identical everywhere.

Other defects in help output:

- `[default: Expanded]` names a value the CLI rejects; only `expanded` is
  accepted.
- _"The equals form is required for `--depth`"_ is false; `--depth 2` works.
- `route --help` wraps a required operand into the description column, so
  `move` appears to take one argument:
  ```
  move <source-reference>   Move one routed source or category while
  <destination-target>        preserving its route meaning.
  ```
- `--verbose` is documented on every command and produces zero bytes on
  `status`, `doctor`, and `library list`.

### Proposed result

```
Usage:
  open-forge index [<source-reference>...] [options]

Arguments:
  <source-reference>  Source IDs or .agents/... paths. Default: every route.

Options:
  --dry-run           Show what would change without writing.

Global options:
  --workspace, --view, --json, --verbose, --help, --version
  See: open-forge help global

Examples:
  open-forge index
  open-forge index memory --dry-run
```

Move the exit-code table and stream policy to `open-forge help exit-codes`,
referenced once from root help. Target ~20 lines per command.

## Colour carries no hierarchy

`CliHumanStyle` colours exactly two things: the `Status:` label and the severity
word on a finding. In a 194-line `doctor` output that is a handful of tokens.
Everything else — paths, headings, counts, evidence, secondary fields — renders
at identical weight, so colour cannot separate signal from noise.

`NO_COLOR` is honoured, which is correct.

### Proposed result

Use dim for secondary text (`Read from:`, `Observed:`, paths in an already-named
context), bold for section headings and file paths that are the subject of a
finding, and keep the existing semantic colours. That alone would make the
current output substantially more scannable without removing a line — though
removing lines remains the better fix.

## Exit codes

The seven-value scheme is a good design, applied inconsistently:

- `route init` exits **2** on its normal success path, because the stub it was
  asked to create needs authoring. That is the purpose of the command. It fails
  under `set -e`.
- Bare `open-forge` with no command exits **0**. Conventionally a usage error.
- `open-forge status` in a non-workspace exits **0** while reporting
  `Open Forge is not installed.` A script cannot distinguish that from success.
- `context` exits **2** on a pristine install, because the shipped `AGENTS.md`
  and `loader.md` carry no frontmatter.

### Proposed result

Reserve non-zero for states the caller must handle. Creating a stub that needs
authoring is `complete` with an advisory line. `status` in a non-workspace is
either `complete` with `installed: false`, or a distinct code — but not `0`
meaning two different things.

## Two more found while filing this audit

**`index <folder-path>` is rejected with a message about "logical sources".**

```
$ open-forge index .agents/memory/emerging/analysis/cli-experience-audit
Status: invalid                                                     exit 4
INVALID: The exact path is not an admitted logical source. [index.invalid-source]
```

The path exists and is a routed scope. It was rejected because it is a
directory rather than the entrypoint file inside it. Nothing in the message says
so, and `Selection: explicit-sources / not-established / 0 sources` does not
help. The same command with the source ID works.

Either accept a directory and resolve it to its entrypoint, or say:

```
.agents/memory/emerging/analysis/cli-experience-audit is a folder.
Use its entrypoint or its ID:
  open-forge index memory/emerging/analysis/cli-experience-audit
```

**`Regions: 137; updates: 16; already current: 105; verified: 105`.**
137 regions, 121 accounted for. The summary line does not balance, in either the
blocked or the successful case (`Regions: 20; updates: 0; already current: 19;
verified: 19` on a clean workspace). A count line that does not add up is worse
than no count line.
