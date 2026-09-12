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
and deterministic manifest under a catalogue parent. It may record explicit
dependency IDs but does not resolve their availability or install them. It does
not install files into a workspace, update generated navigation, write the
lifecycle document, or publish Framework or Extension lifecycle state.

`extension create` is the accepted no-workspace mutation exception. The catalogue
destination is the sole operation subject. Because `--workspace` is a no-op for
this operation, create does not acquire the external workspace mutation lock or
mutate workspace state.

The create destination is distinct from package source selection used by other
Extension operations. `--path` names the destination catalogue parent; it is not
an external package or catalogue source.

## Syntax

```text
open-forge extension create [<stable-id>]
  [--path <catalogue-path>]
  [--name <text>]
  [--description <text>]
  [--package-version <text>]
  [--dependency <stable-id>]...
  [--automatic]
  [--dry-run]
  [global flags]
```

The command-local human wizard can obtain whichever required facts are missing:
the stable ID and destination catalogue parent. A prompt-capable request may
supply neither, either, or both explicitly; the wizard asks only for missing
facts. JSON, `--automatic`, and non-prompt-capable use must provide both. `--path`
is a singleton value and repeated values are invalid. A stable ID is one exact
package identity and repeated positional IDs are invalid.

`--name`, `--description`, and `--package-version` are singleton nonblank
manifest overrides. `--package-version` remains distinct from the global
terminal `--version`. Accepted override text is preserved exactly after nonblank
validation; the descriptive package version does not gain a SemVer parser or
compatibility policy. `--dependency` is repeatable. Each value must be a valid
stable ID, must not equal the new package ID, and must not repeat. Accepted
dependencies serialize in ordinal stable-ID order, independent of option order.

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
does not acquire the external workspace mutation lock. Other global grammar, repetition,
terminal, and presentation rules remain in [Global CLI
Flags](../../shared/global-flags/interface.md).

Create has no `--source`, `--all`, `--force`, `--prune`, `--yes`, package
selection, dependency installation, or workspace operand.

## Wizard, Direct, And Automatic Behavior

A prompt-capable human `create` asks for each missing required fact: stable ID
and destination catalogue parent. The argumentless form asks for both, a partial
explicit request asks only for the missing fact, and a complete explicit request
asks none. Each question gives clear local guidance. A blank or invalid answer
may be explained and asked again while input remains available; the command has
no arbitrary attempt limit or shared retry abstraction. End of input leaves a
required fact missing and returns `invalid` without writes. Caller cancellation
returns `interrupted` without writes. Explicit operands and `--path` answer the
same questions in one typed request. Conflicting or repeated explicit inputs are
invalid.

After those two required inputs resolve, omitted manifest fields use these exact
deterministic defaults:

| Field          | Default                                                                                                  |
| -------------- | -------------------------------------------------------------------------------------------------------- |
| `id`           | The exact resolved stable ID.                                                                            |
| `name`         | Split the ID on `-`, uppercase the first ASCII letter of each segment, and join segments with one space. |
| `description`  | `Open Forge Extension package <stable-id>.`                                                              |
| `version`      | `0.1.0`                                                                                                  |
| `dependencies` | An empty array.                                                                                          |

Explicit manifest options replace only their corresponding defaults. Optional
manifest metadata never adds a wizard question. Human planning shows the
resolved manifest before application.

JSON and other non-interactive modes never prompt. Missing ID or destination is
`invalid`. `--automatic` suppresses the wizard only after both semantic inputs
are explicit. It selects no package, source, workspace, dependency, or
authority by inference. Repeating it is idempotent.

The human flow validates the ID and destination, presents the scaffold plan, and
uses the explicit create invocation as the operation authority. `--dry-run`
previews the same plan and writes nothing. There is no saved plan or second
confirmation operation.

## Catalogue Destination And Scaffold

`--path` names one exact catalogue parent. Any existing safely resolved ordinary
directory is eligible, including an empty directory; it needs no catalogue
marker. Create never creates the parent. Unrelated sibling files or package
directories neither validate nor invalidate it and are not inspected. Create
inspects only the exact package destination `<catalogue>/<id>/`. It may be absent or contain the exact
intended scaffold below. An absent destination is eligible for creation, and an
exact matching scaffold is a verified no-op. Any divergent, partial, additional,
unknown, or colliding occupant blocks. The catalogue parent and destination must
retain their exact physical identities when present and satisfy safe lexical and
physical containment. A source or workspace selection is not used for this
operation.

The exact scaffold writes only:

```text
<catalogue>/<id>/extension.json
<catalogue>/<id>/content/.agents/
```

It does not install a README, payload content, Framework files, Extension files,
generated `Entries`, a lifecycle section, or a dependency closure.
The package remains a separate authored source location.

`extension.json` contains exactly `id`, `name`, `description`, `version`, and
`dependencies` in that order. All five are present. The command validates the
accepted manifest shape but performs no source lookup or dependency closure.

## Output And Results

Both human views begin with the creation outcome or preview and status, then
exact catalogue/package destination, stable ID, resolved manifest metadata and
dependency IDs. Create has no selected workspace: its null workspace does not
mean that a workspace lookup failed. State that workspace installation/lifecycle
was unchanged.

Both views retain every planned and applied scaffold path. Group equal effect
identities and distinguish an intended file from an applied file, especially
when creation is blocked, fails or is interrupted. Expanded explains the effect
kind and concrete verification states/cause. Compact uses shorter rows. Findings
retain status, code, exact subject and cause. Show the actual Next command once
when supplied; expanded may add its reason. Paths are not truncated. JSON
emits the complete typed result from the same result as human output.

The command-local JSON `result` uses camel-case properties in exactly this order:

1. `catalogue`;
2. `destination`;
3. `id`;
4. `manifest`, whose members are `name`, `description`, `version`, and
   `dependencies` in that order;
5. `mode`;
6. `intendedEffects`;
7. `appliedEffects`;
8. `verification`; and
9. `workspaceLifecycleChanged`, always `false`.

The shared envelope already owns command, status, workspace, and next-action
coordinates; none is duplicated inside this result.

An applied-result excerpt is:

```text
Extension creation completed.
Status: complete
Package: review-tools
Catalogue: /packages/open-forge
Destination: /packages/open-forge/review-tools
Mode: apply
Scaffold: 2 intended; 2 applied
  /packages/open-forge/review-tools/extension.json: applied
  /packages/open-forge/review-tools/content/.agents: applied
Verification: catalogue verified; destination verified; manifest verified; content verified
Workspace installation: unchanged
```

The full output also includes manifest metadata. Expanded adds effect kinds and
verification explanations. A verified no-op reports zero applied effects even
when the intended scaffold is already present. Values are illustrative.

Primary human complete/attention/incomplete results go to stdout. Primary human
invalid/blocked/failed/interrupted results go to stderr. Bounded diagnostics use
stderr. JSON uses one result on stdout for every status.

| Result        | Meaning for `create`                                                                                                                               |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The scaffold was completely applied or previewed, or the exact intended scaffold is already present as a verified no-op.                           |
| `attention`   | No current finite create attention condition is accepted. This status remains in the shared vocabulary but is not reached by the current contract. |
| `incomplete`  | Safe catalogue, path, parser, or filesystem coverage is unavailable. No scaffold write occurs.                                                     |
| `invalid`     | Required ID/path input, value, repetition, operand, or terminal-mode combination is invalid.                                                       |
| `blocked`     | Catalogue shape, package destination, identity, containment, ownership, or exact create-only collision boundary is unsafe or colliding.            |
| `failed`      | Scaffold application or verification fails unexpectedly.                                                                                           |
| `interrupted` | The caller interrupts before completion and no stronger failure remains.                                                                           |

## Errors And Examples

Every error names `extension create`, the stable ID or catalogue path when
known, the cause, and at most one useful next action. A divergent, partial,
additional, unknown, or colliding package destination blocks; create never
overwrites or adopts it. An exact intended scaffold is a verified no-op. A
missing or non-directory catalogue parent is invalid. Safely unavailable parent
coverage is incomplete; unsafe or ambiguous identity is blocked.

Wizard form:

```text
open-forge extension create
```

Direct scaffold preview:

```text
open-forge extension create development-toolkit --path D:/packages/open-forge --automatic --dry-run
```

Direct manifest overrides:

```text
open-forge extension create development-toolkit --path D:/packages/open-forge --name "Development Toolkit" --description "Adds development workflows" --package-version 0.2.0 --dependency shared-prompts
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

Conformance must cover zero, one, and all currently missing required human facts;
command-local correction of blank/invalid input without an attempt limit;
no-write invalid end of input and interrupted cancellation; direct, JSON,
automatic, and redirected omission states; exact ID
and catalogue-parent validation, empty and populated marker-free parents,
unrelated sibling preservation, exact-destination-only inspection, refusal to
create a missing parent, catalogue destination distinction, workspace
no-op and the absence of workspace-lock acquisition, scaffold-only
effects, an absent destination, an exact-scaffold no-op, and divergent, partial,
additional, unknown, or colliding occupants blocking. It must also cover exact
catalogue and destination physical identity, expected-state revalidation
immediately before effects, the separate create-only path with no Replace/Delete,
no recovery bundle and no workspace lease, dry-run parity, all five deterministic
manifest defaults, every singleton override and native option-value form,
repeatable dependency ordering, duplicate/self/invalid dependency rejection,
exact manifest property order, no dependency availability resolution, no
optional-metadata wizard questions, exact command-local JSON result/property order without
shared-envelope duplication, all seven statuses
and streams, JSON parity, and no workspace lifecycle effect. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated serialization,
fixed Markdig where used, real `System.IO`, Native AOT, isolated tests, and
package journeys.
