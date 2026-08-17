---
open-forge:
  description: Accepted read-only Interface for listing installed and available Extension packages from one exact source universe
  responsibility: Define extension list syntax, Installed and Available sections, trust states, source handling, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, List, Interface, ReadOnly, Source, Lifecycle, CurrentTruth]
---

# extension list Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension list`. It owns the public syntax, source selection,
section filters, installed lifecycle facts, available package facts, output,
semantic results, errors, examples, non-goals, and public conformance. The
command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines deterministic,
technology-neutral read-only behavior. The [Extension group entrypoint](../_extension.md)
defines only routing and help. Shared Global Flags define workspace and
presentation flags once.

## Purpose And Boundary

`list` answers which Extension packages are installed and which are available.
It keeps those facts in separate sections. Availability never implies
installation, a matching ID never proves ownership, and an installed fact does
not disappear because its package source is unavailable.

`list` is stateless, read-only, deterministic, and non-shipping. It does not
inspect package contents beyond the selected source facts needed for the list,
does not form a mutation plan, and does not replace `status` or `doctor`.

## Syntax

```text
open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]
```

`list` has no operands, `--all`, `--automatic`, `--dry-run`, `--force`,
`--prune`, wizard, or mutation flag. The shared flags are:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

All shared grammar, defaults, repetition, composition, terminal, and error rules
remain in [Global CLI Flags](../../shared/global-flags/interface.md). A
well-formed global flag with no applicable behavior is a shared no-op.

## Source And Workspace

The workspace is the exact current directory unless `--workspace` selects one
exact directory. Installed facts come from the selected workspace's recognized
lifecycle state in `.agents/open-forge.lifecycle.json`, schema v1. The operation
does not discover another root. A missing document or section is not, by itself,
proof of an empty installed set; complete inspection is required before
reporting safe absence.

`--source` is one exact external read location. Structural package and catalogue
facts distinguish one package directory from one catalogue directory. The CLI
does not use ambient search, a network, registry, cache, glob, fuzzy matching,
resemblance, or a fallback source. The source is read-only and must be lexically
and physically disjoint from the target workspace. Source overlap, aliases, and
containment in either direction are blocked.

When `--source` is omitted, available facts come from the embedded catalogue.
Installed facts still come from the exact selected workspace and do not require
the selected package source to be available.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Section Filters

| Input         | Meaning                                   | Omission and composition                                                                                                                                                                                                                                               |
| ------------- | ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--installed` | Render only the Installed section         | Omitted means include Installed unless `--available` is the only filter. Repeating is invalid under singleton value-free selection rules unless the command's Boolean repetition rule accepts idempotence; the command accepts repeated Boolean presence idempotently. |
| `--available` | Render only the Available section         | Omitted means include Available unless `--installed` is the only filter. It composes with `--installed` back to both sections.                                                                                                                                         |
| `--source`    | Select one exact available-package source | Omitted selects the embedded catalogue. It is a singleton and repetition is invalid.                                                                                                                                                                                   |

With neither section flag, both sections are rendered. `--installed` alone
renders Installed. `--available` alone renders Available. Both flags together
render both. There is no last-wins rule.

## Lifecycle Trust States

Installed rows retain the state of the recognized lifecycle evidence:

- `trusted` means a valid current Extension section establishes exact workspace,
  package, dependency, path, owner, fingerprint, and coverage facts.
- `untrusted` or `incomplete` means facts are readable in part but cannot support
  current trust. Safe partial facts remain visible.
- `blocked` means ambiguity, collision, malformed identity, or unsafe ownership
  prevents a safe classification.
- `absent` means complete inspection found no expected managed claim. It does not
  claim that idless, manually copied, or direct-overlay content is absent.

An unavailable package source does not erase an installed row. It
prevents comparison fields that require current source bytes and produces the
applicable availability condition.

## Output

The expanded human result leads with the exact workspace and source. It renders
separate `Installed` and `Available` sections, stable IDs, descriptive versions
when present, package or dependency counts, managed-path and trust facts, source
availability, status, and at most one useful `Next:` action. Compact output keeps
section identity, IDs, key state, counts, and status. JSON emits one complete
typed result on stdout for every semantic status; `--view` is a no-op in JSON.

An illustrative result is:

```text
Open Forge extension list
Workspace: D:/work/example
Source: embedded catalogue
Installed
- development-toolkit 0.1.0; trusted; 21 managed paths
Available
- development-toolkit 0.1.0; 1 package; 0 dependencies
Status: complete
```

Values are illustrative and do not claim current inventory or implementation.

Primary human `complete`, `attention`, and `incomplete` results go to stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results go to
stderr. Bounded diagnostics go to stderr. Human output may say `requires
attention`; JSON retains `attention`.

## Semantic Results

| Result        | Meaning for `list`                                                                                                         |
| ------------- | -------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Requested Installed and/or Available sections have complete safe coverage.                                                 |
| `attention`   | Facts are complete but a finite source-unavailable, changed, missing, or equivalent lifecycle observation remains visible. |
| `incomplete`  | Safe installed or available facts remain, but required lifecycle, source, parser, or coverage facts are unavailable.       |
| `invalid`     | Syntax, operand, flag value, singleton repetition, or terminal-mode input is invalid.                                      |
| `blocked`     | Workspace, source disjointness, identity, containment, or ownership ambiguity prevents safe inspection.                    |
| `failed`      | An unexpected inspection or result-formation failure occurs.                                                               |
| `interrupted` | The caller interrupts before the read-only result completes.                                                               |

Source unavailability may be `incomplete` when requested available or comparison
facts cannot be formed. Installed facts that remain independently trustworthy
are still reported. Planned changes never exist because list is read-only.

## Errors And Next Actions

Every error names `extension list`, the workspace, source, section, or ID when
known, the cause, and at most one useful next action. Multiple source locations,
source overlap, malformed package/catalogue structure, unsafe physical identity,
and unsupported source classification are invalid or blocked as applicable; no
fallback is inferred. A missing source is not silently replaced by the embedded
catalogue when `--source` was explicit.

## Examples

Show both sections from the embedded catalogue:

```text
open-forge extension list
```

Show installed facts even when the package source is unavailable:

```text
open-forge extension list --installed --json
```

Show available packages from one exact external catalogue:

```text
open-forge extension list --available --source D:/packages/open-forge
```

`list` never prompts, installs, updates, removes, creates, adopts, or rewrites
an Extension or lifecycle document.

## Non-Goals And Public Conformance

`list` does not select package IDs for a lifecycle operation, infer ownership
from path or fingerprint, resolve dependencies for installation, mutate a
source or workspace, repair lifecycle evidence, run `status` or `doctor`, or
execute an index operation.

Conformance must cover exact workspace and source selection, embedded versus
explicit source, package/catalogue distinction, source disjointness, both
section filters and their composition, trusted/untrusted/incomplete/blocked/
absent states, source-unavailable installed facts, deterministic ordering, all
seven statuses, human/JSON parity and streams, no prompts, and no persistent
effect. The shared CLI Architecture defines the exact JSON result schema and exit
mapping. Gate 5 must prove source-generated serialization, fixed Markdig where
used, real `System.IO`, Native AOT, isolated tests, and package journeys.
