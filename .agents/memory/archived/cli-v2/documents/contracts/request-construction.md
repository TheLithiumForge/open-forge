---
open-forge:
  description: "Historical CLI-v2 source: One typed request-construction path for explicit arguments, guided choices, defaults, confirmation, preview, and structured automation"
  responsibility: Define how parsed CLI input becomes complete operation intent without leaking raw flags, terminal state, or presentation into handlers and planners
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Request Construction Contract

## Scope

The [CLI interface](../interface.md) owns public arguments, flags, guided
behavior, and result semantics. Commander owns command syntax, presence, coercion,
and value validation. This contract owns the transition from that validated
parser output to one complete semantic operation request.

The [mutation execution contract](mutation-execution.md) owns planning,
preflight, application, verification, and recovery after request construction.
Production TypeScript owns exact declarations, discriminants, and import paths.

## Guarantees

### One Direction

Every selected leaf follows one request path:

```text
Commander-validated command values
  + immutable execution context
  + named interaction and authorization policy
  -> command-local request resolver
  -> ready | invalid | blocked | cancelled
  -> complete typed operation request
  -> named handler
```

Exact state values live in one enum or readonly const object beside their
source contract. Coordinators exhaustively branch on them through direct
references. Raw strings never select a resolver, prompt, handler, or policy.

The resolver is read-only. It may inspect mechanically knowable facts needed
to show real choices, but it never writes, builds an alternate mutation plan,
or silently selects intent from filesystem coincidence.

### Boundary Separation

Keep five concerns distinct:

| Concern                          | Contains                                                                                                                                                                                                                                                         | Excludes                                                                                      |
| -------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| Parsed command values            | Commander-validated positionals and command options                                                                                                                                                                                                              | Raw `argv`, unvalidated strings                                                               |
| Semantic request                 | Complete selected operation, subjects, targets, and authored values                                                                                                                                                                                              | Presentation, terminal state, raw global flags, preview, confirmation policy                  |
| Execution context                | Resolved logical workspace when applicable, cancellation, immutable runtime facts                                                                                                                                                                                | Mutable global state, service locator                                                         |
| Application policy               | Preview or apply                                                                                                                                                                                                                                                 | Another operation or different semantic target                                                |
| Interaction and authority policy | Whether prompts are available, documented defaults may be accepted, an exact inspected external source is approved, eligible collisions are authorized, the Git check is explicitly bypassed, and a formatter invocation classified `RUN` is explicitly approved | Inferred semantic choices, hidden ownership transfer, or authority granted by unrelated flags |

Complete does not mean every field is non-optional. A genuinely optional value,
such as `responsibility`, may remain absent. It means every choice required to
identify and authorize the selected operation has been resolved explicitly or
through one documented accepted default.

### Global And Shared Flags

Flags map once at the command boundary:

| Flag               | Typed meaning after parsing                                                                                    |
| ------------------ | -------------------------------------------------------------------------------------------------------------- |
| `--workspace`      | Resolved immutable workspace execution context for workspace-aware operations                                  |
| `--json`           | Structured presentation and forced non-interactive interaction availability                                    |
| `--yes`            | Accept documented safe defaults and ordinary confirmations                                                     |
| `--dry-run`        | Preview application policy over the same request and planner                                                   |
| `--overwrite`      | Authorize the complete explicitly reported eligible collision set in the selected lifecycle plan               |
| `--skip-git-check` | Bypass only the relevant-path Git cleanliness prerequisite and activate applicable Gitless backup requirements |

Global registration and semantic applicability are separate. After selecting
a leaf, the parser validates supplied global flags against the CLI Interface's
typed operation-applicability map. An inapplicable flag returns `cli.parse`
with `invalid` status before execution-context resolution or request
construction. Applicability is stable for the operation and never inferred
from workspace state, interaction availability, or whether one invocation
happens to require confirmation.

None of these raw flag names enters a semantic request. `--json` never reaches
the handler. The executable boundary derives interaction availability once and
passes a named policy; handlers do not read terminal or process globals.

Status-local `--redact` follows the same boundary rule as `--json`. It selects
the exhaustive status exposure projection after the typed result and before
display selection. It never enters the semantic request, handler, inspection,
suggestion mapping, or semantic-status calculation.

`--yes` and `--overwrite` remain separate authority:

- `--yes` may accept a documented baseline, a safely detected interactive
  default, or ordinary plan confirmation.
- `--yes` cannot choose an Extension, Template, route, source, or another
  material subject when no documented default exists.
- `--yes` cannot authorize an ownership collision.
- `--yes` cannot execute an arbitrary formatter command read from the
  Git-tracked workspace.
- `--yes` cannot accept a source-review finding or weaken a blocked source.
- `--overwrite` cannot choose the package, item, or target set and cannot make
  an ineligible target eligible.
- `--skip-git-check` cannot authorize replacement, deletion, ownership
  transfer, formatter execution, or another semantic choice.

### Guided Resolution

A wizard is a request presentation adapter, not another operation.

For an operation whose contract deliberately makes subject selection optional:

- Omitted subjects in an interactive invocation open the applicable selection
  wizard.
- Explicit space-separated subjects bypass selection and remain visible in the
  completed request.
- Omitted subjects in a non-interactive or structured invocation are invalid
  unless the operation documents one deterministic default.
- A source, filter, or path flag narrows the candidate set; it does not select
  a subject.
- Explicit selection does not bypass later confirmation or authority.

When those subjects resolve an external source closure, source inspection and
its dedicated decision follow selection. Source-review authority identifies
the exact inspected source fingerprints and expires when they change. It does
not grant collision, deletion, executable-configuration, formatter, or Git
authority. The [external source review contract](source-review.md) owns the
finding and interaction semantics.

It may:

1. Inspect mechanically knowable facts.
2. Present exact valid candidates and a documented recommendation.
3. Ask only for missing choices that materially affect intent.
4. Return the same complete request automation would construct.

It may not:

- Mutate the workspace.
- Infer a choice merely because one candidate exists.
- Create a private request or plan shape.
- Catch a domain failure and replace it with different behavior.
- Continue after cancellation.

If a user changes a semantic choice after seeing later evidence, discard the
old request and any plan derived from it. Resolve a new complete request and
plan again; never patch a preflighted plan in place.

### Plan Confirmation

Plan confirmation occurs after complete request construction and complete
preflight:

```text
complete request
  -> inspect, plan, preflight
  -> preview result
     or
  -> show safe plan projection
  -> accepted | cancelled | blocked
  -> apply unchanged preflighted plan
```

The confirmation presenter receives only the safe public plan projection, not
private bytes, physical paths, or rollback material. It returns a named outcome
and performs no application behavior.

Preview never prompts for application confirmation. Interactive application
asks only when the operation contract warrants it. Non-interactive application
must already carry the accepted documented default or explicit authority. A
missing confirmation or collision authority blocks before writes.

Confirmation cannot change effects. Any changed choice requires fresh request
resolution, planning, and preflight.

### Representative Requests

#### Extension Add

These invocations resolve to the same semantic selection:

```text
open-forge extension add
  -> interactive choice: Open Forge / development-toolkit

open-forge extension add development-toolkit
  -> explicit automation input

request
  source kind: open-forge
  extension ids: [development-toolkit]
```

An explicit on-disk source retains the same managed identity:

```text
open-forge extension add team-toolkit shared-toolkit \
  --path ./extensions

request
  source kind: external
  extension ids: [team-toolkit, shared-toolkit]
  invocation-only catalogue path: <resolved filesystem path>
```

The input path may identify a source directory inside or outside the selected
workspace. A manifest at its root makes it a direct one-package catalogue.
Without a root manifest, only immediate child directories with manifests form
the catalogue. The path resolves exactly for this invocation, receives
source-boundary and manifest validation, and never enters committed lifecycle
state. Manifest ids establish managed identity. `--path` selects the catalogue;
it does not select, infer, or rename an Extension.

One catalogue may serve several selected ids:

```text
extension update open-forge-one open-forge-two
  -> selected ids from the embedded Open Forge catalogue

extension update external-one external-two --path ./extensions
  -> selected ids from one exact external catalogue
```

IDs remain separate shell tokens. `one,two` is one invalid literal id with help
to use spaces; it is never parsed as a private comma-separated language.
Several external sources are not encoded through positional ordering or
repeated path flags. Use another invocation for another catalogue.

These do not resolve it:

```text
open-forge extension add --json
  -> invalid: Extension ids are missing and prompts are forbidden

open-forge extension add --yes
  -> still requires Extension ids; --yes does not choose them
```

`--dry-run` and `--overwrite` do not change that request. The first selects
preview. The second authorizes eligible collision resolution after planning.

#### Managed Reconciliation

Guided lifecycle resolution may add literal per-subject decisions to the
complete request:

```text
Decision       Subject
RESTORE        patterns
KEEP REMOVED   workflows
DELETE         retired.md
KEEP           customized.md
```

Selecting a row toggles only its explicit alternatives. Changing a decision
discards any earlier plan. Non-interactive execution may accept only documented
unambiguous defaults; unresolved changed, unknown, destructive, or ownership
decisions block rather than being inferred from `--yes`.

#### Formatter Execution

Formatter selection and resolved-invocation trust remain separate. A `SAFE`
invocation may use ordinary confirmation. A `RUN` invocation caused by
executable configuration, plugins, analyzers, project code, or custom argv
requires an explicit interactive `RUN` decision. Generic `--yes` never supplies
that authority. `BLOCK` never enters a completed executable request. The
completed request contains the selected formatter policy and trust evidence,
not terminal prompt state or a raw shell command.

#### Install

These resolve to the same whole-Framework request:

```text
open-forge install
  -> interactive complete-payload review and acceptance

open-forge install --yes
  -> deterministic complete embedded payload

open-forge install --restore patterns workflows
  -> complete embedded payload with those exact persisted exclusions re-enabled

request
  Framework source: running CLI payload
  Framework selection: complete payload minus retained persisted exclusions
```

Install accepts no item arguments. `--yes` supplies ordinary confirmation, not
a second payload selection. Every `--restore` value must exactly match a
persisted exclusion; unknown routes, non-excluded routes, and descendants hidden
only by a broader exclusion are invalid. The flag changes only those exclusion
decisions and does not grant overwrite, confirmation, formatter, Git, or
recovery authority. Keep-removed and retired-file decisions remain separate
typed policy around the same whole-Framework request.

#### Completion Install

```text
open-forge completion install bash zsh
  -> explicit two-target request

open-forge completion install
  -> interactive multi-select with every safely detected target preselected

open-forge completion install --all --json
  -> complete request for every safely discoverable target

open-forge completion install zsh \
  --profile /home/me/dotfiles/.zshrc
  -> one explicit shell and one exact custom activation profile

open-forge completion install --json
  -> invalid: shell selection is missing
```

Non-interactive execution does not infer a shell merely because the host can
observe one. It requires explicit space-separated shell identifiers or
`--all`. `--profile` is valid only with one explicit Bash, Zsh, or PowerShell
identifier. It is invalid with omitted or multiple shells, `--all`, or Fish.

Guided preselection does not authorize the plan. Divergent owned content still
requires its dedicated interactive decision, and `--yes` cannot supply it.
Direct installation may return a visible next-command reminder after its
own result. It never creates a Completion request or effects inside the
Framework operation.

### Result Boundary

- Parser syntax, coercion, enum, or arity failure returns an invalid result at
  the parser boundary.
- Missing semantic input without a safe accepted default returns invalid and
  shows accepted invocation forms.
- Unavailable workspace state or missing required authority for an otherwise
  complete intent returns blocked.
- User cancellation returns the selected operation's cancelled result.
- Request resolution never returns a partial request for downstream code to
  guess from.

## Boundaries

Request construction selects semantic intent only. Raw flags, terminal state,
presentation, workspace resolution, preview policy, confirmation, collision
authority, and cancellation remain typed context or policy around the request.
Selection never grants safety authority, and `--yes` never invents a material
choice without a documented accepted default.

## Verification

- A guided command keeps its request resolver beside the command.
- Automation and wizard tests compare the complete resolved request, not prompt
  implementation details.
- Handler tests start from complete typed requests and explicit execution
  context and policy.
- Request-resolution tests cover explicit input, documented defaults, missing
  input, non-interactive mode, cancellation, and forbidden authority transfer.
- Plan-confirmation tests prove preview does not prompt and changed choices
  require replanning.
- No handler reads `process.argv`, `process.stdin`, TTY state, environment
  variables, or raw global flags.

## Related Current Sources

- [Operation prerequisites](operation-prerequisites.md)
- [Mutation execution](mutation-execution.md)
- [CLI result and display boundary](../interface.md#results-and-presentation)
