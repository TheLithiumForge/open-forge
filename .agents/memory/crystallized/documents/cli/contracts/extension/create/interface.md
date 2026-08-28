---
open-forge:
  description: Accepted Interface for creating a local Extension scaffold in a catalogue without installing it
  responsibility: Define create's exact syntax, catalogue destination, wizard and automatic input, workspace no-op, effects, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Create, Interface, Catalogue, Mutation, CurrentTruth]
---

# extension create Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension create`. It owns the exact public syntax, stable-ID and
catalogue destination inputs, wizard/direct behavior, scaffold effects, global
flag applicability, statuses, output, errors, examples, non-goals, and public
conformance. The command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines the technology-neutral
scaffold plan and safe application. The [Extension group entrypoint](../_extension.md)
defines group routing only. No Technical Design exists.

## Purpose And Boundary

`create` gives an author a stable local package boundary. It writes a scaffold
under a catalogue parent and does not install files into a workspace, resolve or
write dependencies, update generated navigation, write the lifecycle document,
or publish Framework or Extension lifecycle state.

`extension create` is the accepted no-workspace mutation exception. The catalogue
destination is the sole operation subject. Because `--workspace` is a no-op for
this operation, create does not acquire `.agents/open-forge.lock` or mutate
workspace state.

The create destination is distinct from package source selection used by other
Extension operations. `--path` names the destination catalogue parent; it is not
an external package or catalogue source.

## Syntax

```text
open-forge extension create [<stable-id>] [--path <catalogue-path>] [--automatic] [--dry-run] [global flags]
```

The human wizard can obtain the stable ID and destination catalogue parent from
the argumentless form. Direct, JSON, and other non-interactive use must provide
both semantic inputs. `--path` is a singleton value and repeated values are
invalid. A stable ID is one exact package identity and repeated positional IDs
are invalid.

The shared flags are:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

`--workspace` is accepted as a global flag but is a no-op for create. The
catalogue destination, not the workspace, is the operation subject, and create
does not acquire `.agents/open-forge.lock`. Other global grammar, repetition,
terminal, and presentation rules remain in [Global CLI
Flags](../../shared/global-flags/interface.md).

Create has no `--source`, `--all`, `--force`, `--prune`, `--yes`, package
selection, dependency installation, or workspace operand.

## Wizard, Direct, And Automatic Behavior

Argumentless human `create` opens a finite wizard for exactly two questions:
stable ID and destination catalogue parent. Explicit operands and `--path`
answer those same questions in one typed request. Conflicting or repeated
explicit inputs are invalid.

JSON and other non-interactive modes never prompt. Missing ID or destination is
`invalid`. `--automatic` suppresses the wizard only after both semantic inputs
are explicit. It selects no package, source, workspace, dependency, or
authority by inference. Repeating it is idempotent.

The human flow validates the ID and destination, presents the scaffold plan, and
uses the explicit create invocation as the operation authority. `--dry-run`
previews the same plan and writes nothing. There is no saved plan or second
confirmation operation.

## Catalogue Destination And Scaffold

`--path` names one exact catalogue parent. The parent is recognized from its
structural catalogue shape; it needs no persistent catalogue marker. The package
destination is `<catalogue>/<id>/`. It may be absent or contain the exact
intended scaffold below. An absent destination is eligible for creation, and an
exact matching scaffold is a verified no-op. Any divergent, partial, additional,
unknown, or colliding occupant blocks. The catalogue parent and destination must
retain their exact physical identities when present and satisfy safe lexical and
physical containment. A source or workspace selection is not used for this
operation.

The exact scaffold writes only:

```text
<catalogue>/<id>/extension.json
<catalogue>/<id>/payload/.agents/
```

It does not install a README, payload content, Framework files, Extension files,
generated `Entries`, a lifecycle section, or a dependency closure.
The package remains a separate authored source location.

## Output And Results

Human output leads with the exact catalogue and package destination, stable ID,
scaffold files, dry-run/application mode, workspace-lifecycle unchanged fact,
verification facts, status, and at most one next action. JSON emits one
complete typed result from the same result as human output.

Illustrative output:

```text
Open Forge extension create development-toolkit
Catalogue: D:/packages/open-forge
Created: D:/packages/open-forge/development-toolkit/extension.json
Created: D:/packages/open-forge/development-toolkit/payload/.agents/
Workspace lifecycle: unchanged
Status: complete
```

The values are illustrative and do not claim implementation evidence.

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. Bounded diagnostics use
stderr. JSON uses one result on stdout for every status.

| Result        | Meaning for `create`                                                                                                                               |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The scaffold was completely applied or previewed, or the exact intended scaffold is already present as a verified no-op.                           |
| `attention`   | No current finite create attention condition is accepted. This status remains in the shared vocabulary but is not reached by the current contract. |
| `incomplete`  | Safe catalogue, path, parser, or filesystem coverage is unavailable. No scaffold write occurs.                                                       |
| `invalid`     | Required ID/path input, value, repetition, operand, or terminal-mode combination is invalid.                                                       |
| `blocked`     | Catalogue shape, package destination, identity, containment, ownership, or exact create-only collision boundary is unsafe or colliding.           |
| `failed`      | Scaffold application or verification fails unexpectedly.                                                                                            |
| `interrupted` | The caller interrupts before completion and no stronger failure remains.                                                                              |

## Errors And Examples

Every error names `extension create`, the stable ID or catalogue path when
known, the cause, and at most one useful next action. A divergent, partial,
additional, unknown, or colliding package destination blocks; create never
overwrites or adopts it. An exact intended scaffold is a verified no-op. A
missing or malformed catalogue parent is invalid, incomplete, or blocked
according to the established fact.

Wizard form:

```text
open-forge extension create
```

Direct scaffold preview:

```text
open-forge extension create development-toolkit --path D:/packages/open-forge --automatic --dry-run
```

The accepted global no-op remains explicit:

```text
open-forge extension create development-toolkit --path D:/packages/open-forge --workspace D:/not-used
```

## Non-Goals And Public Conformance

Create does not use `--workspace` to choose a destination, read a package source,
install or update an Extension, resolve dependencies, mutate a target workspace,
write lifecycle state, project generated navigation, adopt an existing package,
run a formatter, or remove the package source.

Conformance must cover wizard/direct/JSON/automatic omission states, exact ID
and catalogue-parent validation, catalogue destination distinction, workspace
no-op and the absence of `.agents/open-forge.lock` acquisition, scaffold-only
effects, an absent destination, an exact-scaffold no-op, and divergent, partial,
additional, unknown, or colliding occupants blocking. It must also cover exact
catalogue and destination physical identity, expected-state revalidation
immediately before effects, the separate create-only path with no Replace/Delete,
no recovery bundle and no workspace lease, dry-run parity, all seven statuses
and streams, JSON parity, and no workspace lifecycle effect. The shared CLI
Architecture defines the exact JSON result
schema and exit mapping. Gate 5 must prove source-generated serialization,
fixed Markdig where used, real `System.IO`, Native AOT, isolated tests, and
package journeys.
