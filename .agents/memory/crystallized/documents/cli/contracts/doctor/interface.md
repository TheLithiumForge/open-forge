---
open-forge:
  description: Current accepted read-only interface for complete workspace diagnosis and repair next actions
  responsibility: Define the Doctor public grammar, finite finding catalogue, coverage, results, errors, and examples
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Doctor, Interface, Diagnosis, Findings, Safety, CurrentTruth]
---

# Doctor Interface Contract

## Status And Authority

This file is the accepted current Crystallized authority for the public `doctor`
Interface Contract. The command does not ship yet. This contract owns the exact
syntax, accepted global flags, fixed diagnostic domains, finite detectable
catalogue, coverage and status meanings, output projections, errors, examples,
non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines the deterministic,
technology-neutral operation behind this surface. The [Global CLI Flags](../shared/global-flags/interface.md)
contract defines the six global flags once. The [Source References](../shared/source-references/interface.md)
contract remains the authority for source identity wherever a diagnostic finding
reports a source.

The accepted CLI Architecture defines the exact shared JSON result schema and
numeric exit mapping, parser and library choices, filesystem identity details,
token and hashing choices, concurrency, numeric resource limits, C# and .NET
Native AOT source boundaries, and packaging. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. No Technical Design
file exists for Doctor, and this contract does not duplicate those mechanics.

## Purpose And Boundary

`doctor` diagnoses the complete known structural and lifecycle surface of one
exact workspace without changing it. It reports what was checked, what could not
be checked safely, what was found, and which kind of next action is appropriate.

Doctor is one read-only operation. It is stateless and deterministic for the
same CLI payload, workspace bytes, and explicit input. It does not prompt. It
does not repair, plan, index, clean up, install, upgrade, remove, adopt, or
otherwise change anything.

The operation always uses these six diagnostic domains, in this order:

| Order | Domain                                                   | Boundary                                                                                                                                                                                   |
| ----- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1     | `workspace and entry`                                    | Establish the selected workspace, `.agents` boundary, Loader, entrypoints, source identity, parsing, and reachable roots.                                                                  |
| 2     | `recovery and residual state`                            | Report exact named external final bundles and incomplete drafts, with one semantic final-ZIP integrity check and Cleanup guidance.                                                          |
| 3     | `routes, metadata, overwrites, and generated navigation` | Compare authored topology and metadata with derived route relationships and generated `Entries`.                                                                                           |
| 4     | `local references`                                       | Inspect supported authored local references, target and fragment resolution, containment, and bounded repair evidence.                                                                     |
| 5     | `Framework lifecycle`                                    | Diagnose the installed or absent Framework payload, isolated Framework lifecycle section, managed files and regions, trust, ownership boundaries, and recovery evidence.                   |
| 6     | `Extension lifecycle`                                    | Diagnose the isolated `extensions` section of the exact lifecycle document, manifests, managed files, dependencies, source availability, catalogues, ownership, and registration evidence. |

All six domain groups remain in the result. A dependent domain reports
`incomplete` or `blocked` coverage when an earlier boundary prevents trustworthy
work; it does not disappear.

## Unified Lifecycle Document Boundary

Doctor reads `.agents/open-forge.lifecycle.json`, schema v1, as one physical
common envelope with separate logical `framework` and `extensions` sections. The
sections remain isolated authorities: Framework source, target, and managed
region facts are not Extension package, dependency, or owner facts. Doctor
reports each section's state without mutating, rebaselining, repairing, or
publishing it. The document stores no plan, runtime history, journal, recovery
evidence, or session. Files outside this exact path are ordinary workspace
content, not lifecycle input.

Each section may be `absent`, `trusted`, `untrusted`, `incomplete`, or
`blocked`:

- `absent` requires complete inspection proving that no expected managed state,
  managed boundary, or recovery residual exists.
- `trusted` requires schema v1 and `open-forge-markdown-v1`, exact workspace
  and managed identities, intact reciprocal facts, and complete verifiable
  coverage.
- `untrusted` identifies readable facts whose provenance, integrity,
  compatibility, identity, or coverage cannot establish current trust.
- `incomplete` identifies safe unavailable lifecycle or source coverage.
- `blocked` identifies malformed, ambiguous, colliding, or unsafe lifecycle
  identity.

A path, matching bytes, matching fingerprint, source, or recommendation never
promotes a section to trusted. An absent document or section is not, by itself,
proof of an unmanaged or empty workspace; complete inspection is required for
safe absence. Installed Extension IDs, ownership, and recorded paths remain
reportable when package source bytes are unavailable. Source unavailability
prevents source-dependent comparison or mutation planning but does not erase
read-only installed facts. Unsupported or ambiguous schema facts are
`incomplete` when safely unavailable and `blocked` when unsafe.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Syntax

The complete public command form is:

```text
open-forge doctor [global flags]
```

`doctor` has no positional operands and no doctor-specific flags. It accepts no
aliases, domain filters, caps, include or exclude selectors, repair or fix flag,
wizard flag, or prompt request. The only applicable flags are the six shared
global flags:

```text
--workspace <path>
--json
--view=compact|expanded
--verbose
--help
--version
```

The shared contract defines their value grammar, defaults, repetition,
composition, terminal behavior, and errors. Doctor does not redefine them or
make `--automatic`, `--dry-run`, or `--relink` global. Those spellings are not
accepted by Doctor.

An omitted global flag uses the shared default. Repeated Boolean global flags
retain the shared idempotent behavior. Repeating `--workspace` or `--view`
retains the shared invalid-repetition rule. `--help` and `--version` are
terminal informational modes, are mutually exclusive, and stop before workspace
resolution or diagnosis. A domain operand or doctor-specific flag remains
invalid in either terminal mode. Other well-formed global flags may be no-ops in
that terminal mode as defined by the shared contract.

## Workspace Selection

Doctor uses the exact process current working directory when `--workspace` is
omitted, or the exact `--workspace <path>` value when supplied. A relative value
is resolved from the process current working directory. Doctor does not search
upward, choose a Git root, infer a package root, or select a nearby `.agents`
directory.

A missing, unavailable, or non-directory selected workspace is a blocked
inspection boundary. When the workspace boundary can be reported safely, the
result retains the six domain identities and marks dependent domain coverage as
blocked rather than fabricating facts. A valid directory without an installed
Framework remains a diagnosis subject. Safe absence is reported as evidence; it
is not silently converted to a healthy installation.

## Coverage, Severity, And Resolution

Each domain report exposes its identity, inspected boundary, coverage,
limitations, counts, findings, and next actions. Domain coverage has three
values:

| Coverage     | Meaning                                                                                                             |
| ------------ | ------------------------------------------------------------------------------------------------------------------- |
| `complete`   | The domain ran every check in its declared boundary. Complete does not mean healthy and does not suppress findings. |
| `incomplete` | Safe facts were obtained, but a trustworthy part of the declared boundary could not be inspected or resolved.       |
| `blocked`    | An unsafe or ambiguous required boundary prevented the domain from proceeding safely.                               |

Finding severity and resolution are independent dimensions. Severity is one of
`information`, `warning`, or `error`. Severity never selects a repair.

Every finding uses one of these resolution lanes:

| Resolution           | Meaning                                                                                                                                                                         |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `safe-exact`         | Doctor has enough current facts to describe one exact, meaning-preserving effect. Repair may admit it to its automatic catalogue.                                               |
| `guided-choice`      | A user must choose among finite bounded candidates or supply equivalent exact intent. No candidate is selected automatically.                                                   |
| `targeted-operation` | A distinct accepted operation owns the known intent, such as generated-navigation indexing or an accepted route operation. Doctor reports the typed action and does not run it. |
| `manual-decision`    | The result depends on authored meaning, ownership, lifecycle intent, or another decision outside general repair authority.                                                      |
| `blocked-repair`     | The boundary is unsafe, ambiguous, stale, unsupported, or lacks authority or recovery. No repair effect is proposed.                                                            |
| `informational`      | The fact is useful context and has no repair action.                                                                                                                            |

Every finite-catalogue row below uses exactly one of these six lanes. Domain
coverage remains a separate `complete`, `incomplete`, or `blocked` fact and is
stated in the condition or explanation when a finding limits coverage.

Recommendations can be emphasized in human output, but recommendation text,
severity, finding order, and messages never dispatch behavior. A finding can be
informational even when its domain has warning or error findings, and a complete
domain can contain any severity.

## Finding Contract

Each finding reports all of the following concepts:

- A domain-qualified stable kind from the finite catalogue below.
- Severity independent of coverage, semantic status, and resolution.
- A typed subject, such as the workspace, a path, route, generated region,
  source occurrence, target, recovery bundle or draft, managed file, Extension ID, or
  dependency.
- Typed observed evidence, including the relevant authored value, current
  identity, location, expected relationship, or unavailable fact.
- Provenance identifying the domain boundary and source of the observation.
- Its resolution lane.
- Candidates or an exact proposal when that lane applies. Candidate evidence is
  explicit and bounded; a recommendation is never authority.
- Typed next actions. Actions may point to an accepted operation, a future
  operation without inventing its command name, a general Repair mode, or a
  manual decision.

Messages and display codes are explanatory only. They are not selectors and
cannot dispatch an effect. Finding order is deterministic domain order followed
by stable kind, typed subject, and location order. A human view may change
framing, but not finding identity, order, evidence, or resolution.

## Local-Reference Boundary

The local-reference domain admits supported Markdown source files and local
target paths that are both lexically and physically contained by the selected
workspace. The normal Open Forge source universe is below `.agents`. A source or
target outside `.agents` is admitted only when it is a supported Markdown or
contained local target within this same declared boundary. The boundary does not
expand through a link, a Git root, a package root, or an external URL.

An admitted target may carry one Markdown fragment for fragment diagnosis and
Repair relink intent. Absolute destinations, external URLs, query-only changes,
workspace escapes, physical aliases, unreadable targets, and unsupported target
kinds remain findings or coverage boundaries rather than entering the Repair
relink boundary. If lexical and physical containment cannot both be established,
the occurrence is blocked.

## Finite Diagnostic Catalogue

The following catalogue is the complete first-release set of detectable finding
kinds. A domain may also report a limitation or coverage boundary when the
declared check cannot be trusted. A fact that is valid and needs no action is
represented as an informational finding where that distinction helps the user.

### Workspace And Entry

| Kind                                      | Detectable condition                                                                                                                                                            | Resolution or next action                                                                                   |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `workspace.unavailable`                   | The selected workspace cannot be accessed.                                                                                                                                      | `blocked-repair`; correct the exact workspace or its access.                                                |
| `workspace.not-directory`                 | The selected workspace value resolves to a non-directory.                                                                                                                       | `blocked-repair`; select an exact directory.                                                                |
| `workspace.agents-missing`                | The `.agents` boundary is missing. Safe absence may be reported, while dependent domain coverage is `incomplete` or `blocked` when its checks require the boundary.             | `informational`; report the boundary and do not create it.                                                  |
| `workspace.agents-inaccessible`           | `.agents` exists but cannot be inspected safely.                                                                                                                                | `blocked-repair`; restore access or use a valid workspace.                                                  |
| `workspace.loader-missing`                | The required Loader is absent.                                                                                                                                                  | `blocked-repair`; restore the required Loader; do not adopt another Loader.                                 |
| `workspace.loader-unreadable`             | The Loader exists but cannot be read.                                                                                                                                           | `blocked-repair`; restore readable evidence.                                                                |
| `workspace.loader-malformed`              | Loader structure cannot establish its required meaning.                                                                                                                         | `blocked-repair`; correct the Loader manually.                                                              |
| `workspace.entry-missing`                 | A required recognized workspace or route entrypoint is absent.                                                                                                                  | `manual-decision`; decide whether to author it through an accepted route action; Doctor does not author it. |
| `workspace.entry-ambiguous`               | More than one recognized entrypoint claims one folder.                                                                                                                          | `blocked-repair`; resolve the compatibility or physical ambiguity.                                          |
| `workspace.entry-compatibility-collision` | Canonical and compatibility entrypoint forms collide or cannot be assigned one identity.                                                                                        | `blocked-repair`; preserve one unambiguous route identity.                                                  |
| `workspace.source-id-collision`           | Several current sources derive the same automatic source ID.                                                                                                                    | `manual-decision`; use exact paths for inspection and resolve the identity collision explicitly.            |
| `workspace.path-invalid`                  | A discovered or referenced path is malformed for its declared source kind.                                                                                                      | `blocked-repair`; correct the path or retain the finding as manual evidence.                                |
| `workspace.path-containment`              | A path leaves the selected workspace or cannot prove containment.                                                                                                               | `blocked-repair`; no outside path is selected.                                                              |
| `workspace.physical-alias`                | Distinct path spellings or physical identities alias one another in a way that makes source identity unsafe.                                                                    | `blocked-repair`; resolve the physical identity ambiguity.                                                  |
| `workspace.frontmatter-malformed`         | Frontmatter cannot be parsed for the required source; affected source coverage is `incomplete` when safe facts remain and `blocked` when a safe boundary cannot be established. | `manual-decision`; correct authored metadata without inventing a value.                                     |
| `workspace.frontmatter-duplicate`         | A frontmatter key or required metadata occurrence is duplicated ambiguously.                                                                                                    | `manual-decision`; authored metadata must be corrected.                                                     |
| `workspace.parse-incomplete`              | Supported Markdown or another declared source cannot be parsed completely; affected coverage is `incomplete`.                                                                   | `informational`; retain safe facts and report the unreadable boundary.                                      |
| `workspace.unsupported-source`            | A discovered source kind is outside the supported diagnostic boundary; affected coverage is `incomplete` or `blocked` when the source cannot be excluded safely.                | `informational`; exclude it without reinterpretation.                                                       |
| `workspace.root-missing`                  | A Loader-declared root cannot be found; dependent route coverage is `incomplete` or `blocked` when the root is required.                                                        | `manual-decision`; decide whether to restore or revise the declared root; do not invent one.                |
| `workspace.root-unreachable`              | A declared root or descendant cannot be reached through established routing facts; dependent route coverage is `incomplete` or `blocked` when reachability is required.         | `manual-decision`; report the detached evidence and do not invent reachability.                             |
| `workspace.detached`                      | A source or route tree exists outside the established reachable topology; coverage is `blocked` when the detached boundary cannot be inspected safely.                          | `informational`; report the detached evidence and do not adopt it.                                          |

### Recovery And Residual State

Doctor inspects the current user's external recovery root at
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1`. This
observer-only lookup never creates the OS application-data root or the Open Forge
subtree. If the application-data root or recovery store is absent, Doctor reports
zero recovery bundles or drafts. If the selected workspace bucket cannot be
read, Doctor reports recovery coverage as unavailable or incomplete; it does not
treat access failure as absence or search the workspace recursively.

Doctor enumerates only exact deterministic final and draft names directly under
the selected workspace bucket. It performs at most one semantic integrity check
for each exact named final ZIP: source-generated schema-v1 manifest decoding,
exact ordered entry names and counts, declared lengths and hashes, and exact
payload bytes. A valid final is `Verified`; an invalid or unreadable final is
`Malformed`, `Unsupported`, or `Unavailable`. An exact named draft is always
`Incomplete` and never preparation. Doctor never creates, renames, deletes,
extracts, restores, rolls back, or rebinds a recovery item.

Doctor reports only the item's exact path, kind, integrity condition, and the
separate Cleanup action. It does not inspect live targets, classify target
state, or infer activity. Payload validation uses fixed bounded buffers and never
extracts, discloses, renders, logs, returns, retains, or materializes payload
bytes. Cleanup owns deletion only after it acquires the same-workspace lease,
re-enumerates the selected bucket, and repeats final ordinary path/kind and
semantic validation.

| Kind                              | Detectable condition                                                                                                                     | Resolution or next action                                                                                                                               |
| --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `recovery.bundle-recognized`      | An exact named final ZIP is semantically verified under the selected workspace bucket.                                                   | `informational`; report its path and `Verified` integrity, then offer the separate [cleanup operation](../cleanup/interface.md).                           |
| `recovery.draft-recognized`       | An exact named draft is present under the selected workspace bucket.                                                                      | `informational`; report its path as `Incomplete`; it is never a recovery preparation.                                                                    |
| `recovery.bundle-collision`       | An exact deterministic final name contains malformed, unsupported, or unreadable content.                                                | `blocked-repair`; preserve it and report the exact integrity condition.                                                                                   |
| `recovery.provenance-unavailable` | A final ZIP cannot provide complete semantic schema, exact ordered entries, prior payload, intended fingerprint, or operation provenance. | `blocked-repair`; preserve it; Cleanup cannot delete it without semantic validation and final under-lease revalidation.                                  |

### Routes, Metadata, Overwrites, And Generated Navigation

| Kind                                | Detectable condition                                                                                                                                   | Resolution or next action                                                                                                                      |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `route.entrypoint-missing`          | A routed folder lacks its one recognized entrypoint.                                                                                                   | `manual-decision`; route authoring is not general repair.                                                                                      |
| `route.entrypoint-duplicate`        | A folder has duplicate recognized entrypoints.                                                                                                         | `blocked-repair`; resolve the route identity manually.                                                                                         |
| `route.child-missing`               | Authored topology refers to a routed child that is absent.                                                                                             | `manual-decision`; do not create the child from diagnosis.                                                                                     |
| `route.escape`                      | A route destination leaves the containing workspace or route boundary.                                                                                 | `blocked-repair`; no route escape is repaired automatically.                                                                                   |
| `route.cycle`                       | Route relationships contain a cycle where an acyclic route boundary is required.                                                                       | `blocked-repair`; resolve topology manually.                                                                                                   |
| `route.unreachable`                 | A route is not reachable from its established root or exposing parent.                                                                                 | `manual-decision`; do not invent a parent.                                                                                                     |
| `route.detached`                    | A complete-looking route tree is detached from the Loader or selected root.                                                                            | `manual-decision`; detached content is not adopted.                                                                                            |
| `route.metadata-required-missing`   | Required route metadata is absent.                                                                                                                     | `manual-decision`; authored meaning is not invented.                                                                                           |
| `route.title-invalid`               | A required route title is missing or structurally invalid.                                                                                             | `manual-decision`; do not derive authored meaning from a filename.                                                                             |
| `route.axioms-invalid`              | Required `Axioms` structure or inherited sentinel is missing or malformed; route coverage is `blocked` when active rules cannot be established safely. | `manual-decision`; do not rewrite active rules automatically.                                                                                  |
| `route.generated-region-stale`      | A valid generated region does not match current route facts.                                                                                           | `targeted-operation`; use accepted `index` behavior, not general Repair.                                                                       |
| `route.generated-region-missing`    | A required generated region is absent; route coverage is `blocked` when its boundary cannot be established safely.                                     | `targeted-operation`; use accepted `index` behavior only after the route boundary is valid; generated-region authoring remains outside Repair. |
| `route.generated-region-malformed`  | A generated region cannot be parsed as one valid bounded region.                                                                                       | `blocked-repair`; do not repair markers through general Repair.                                                                                |
| `route.generated-region-misplaced`  | A generated region is not in its accepted location.                                                                                                    | `manual-decision`; no authored-file rewrite is inferred.                                                                                       |
| `route.generated-region-duplicate`  | More than one generated region claims one entrypoint.                                                                                                  | `blocked-repair`; resolve the boundary manually.                                                                                               |
| `route.generated-entry-missing`     | A required generated entry is absent.                                                                                                                  | `targeted-operation`; use `index` after the route boundary is valid.                                                                           |
| `route.generated-entry-extra`       | A generated entry has no current routed source.                                                                                                        | `targeted-operation`; use `index` after the route boundary is valid.                                                                           |
| `route.generated-entry-order`       | Generated entries are present but not in canonical order.                                                                                              | `targeted-operation`; use `index`.                                                                                                             |
| `route.generated-entry-path`        | A generated destination is not the canonical direct-child path.                                                                                        | `targeted-operation`; use `index`, not a relink.                                                                                               |
| `route.generated-entry-description` | A generated description does not match current authored metadata.                                                                                      | `targeted-operation`; use `index` after metadata is valid.                                                                                     |
| `route.generated-entry-tags`        | Generated tags do not match current authored metadata.                                                                                                 | `targeted-operation`; use `index` after metadata is valid.                                                                                     |
| `route.overwrite-orphan`            | An overwrite companion has no valid base source; route coverage is `blocked` when the relationship is unsafe to inspect.                               | `manual-decision`; never index or adopt the orphan.                                                                                            |
| `route.overwrite-ambiguous`         | Base and overwrite relationships cannot establish one logical source.                                                                                  | `blocked-repair`; preserve both paths and resolve identity manually.                                                                           |
| `route.overwrite-independent-index` | An overwrite appears as an independent generated or route entry.                                                                                       | `targeted-operation`; use accepted `index` behavior; overwrite content is not independently indexed.                                           |
| `route.compatibility-conflict`      | Compatibility route forms cannot be reconciled to one accepted route identity.                                                                         | `blocked-repair`; no compatibility winner is selected.                                                                                         |

### Local References

| Kind                                     | Detectable condition                                                                                                                                                                         | Resolution or next action                                                                                          |
| ---------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `reference.target-valid`                 | A local target resolves within the accepted contained boundary.                                                                                                                              | `informational`; no repair is needed.                                                                              |
| `reference.target-missing`               | A local destination has no current target. Bounded candidate evidence, when available, is reported separately.                                                                               | `guided-choice`; review bounded candidates or supply equivalent exact intent; never select a target automatically. |
| `reference.fragment-missing`             | A target exists but the authored fragment has no matching heading or anchor. Bounded fragment candidates, when available, are reported separately.                                           | `guided-choice`; review the bounded evidence or supply equivalent exact intent.                                    |
| `reference.fragment-unverified`          | A fragment cannot be verified because target parsing or coverage is incomplete; the affected reference coverage is `incomplete` or `blocked`.                                                | `blocked-repair`; retain the occurrence and never claim a safe correction.                                         |
| `reference.destination-malformed`        | The authored destination cannot be parsed as a supported local reference.                                                                                                                    | `manual-decision`; authored text is not rewritten.                                                                 |
| `reference.destination-absolute`         | The destination is an absolute local or filesystem form outside the accepted local grammar.                                                                                                  | `manual-decision`; no external or absolute repair.                                                                 |
| `reference.destination-query`            | The destination contains a query component outside the accepted local repair grammar.                                                                                                        | `manual-decision`; no query is discarded automatically.                                                            |
| `reference.destination-encoding`         | Encoding prevents safe target identity or differs from the canonical authored representation. The affected reference remains unresolved until exact identity and resulting bytes are proven. | `blocked-repair`; preserve the occurrence until its exact identity is established.                                 |
| `reference.target-outside-workspace`     | The resolved target leaves the selected workspace.                                                                                                                                           | `blocked-repair`; no outside target is selected.                                                                   |
| `reference.target-physical-escape`       | Lexical containment does not match physical containment.                                                                                                                                     | `blocked-repair`; preserve the occurrence and target evidence.                                                     |
| `reference.target-alias`                 | Several path forms or physical aliases identify the target ambiguously.                                                                                                                      | `blocked-repair`; do not choose a path spelling.                                                                   |
| `reference.target-unreadable`            | The target cannot be read for the required resolution check; affected reference coverage is `incomplete` when safely unavailable and `blocked` when the boundary is unsafe.                  | `blocked-repair`; retain the occurrence.                                                                           |
| `reference.target-unsupported`           | The target kind is outside the supported local-reference boundary; affected reference coverage is `incomplete` when it cannot be excluded safely.                                            | `informational`; exclude it without reinterpretation and do not repair it.                                         |
| `reference.image`                        | The occurrence is an image or other supported non-navigation local resource reference.                                                                                                       | `informational` when its bounded facts are available; no general semantic repair.                                  |
| `reference.external-unchecked`           | The occurrence names an external URL that was not fetched.                                                                                                                                   | `informational`; no-fetch does not change the declared local coverage.                                             |
| `reference.cycle`                        | Local references repeat a source or form a cycle during bounded evidence collection.                                                                                                         | `informational`; cycles do not select a repair.                                                                    |
| `reference.repeat`                       | The same authored target or occurrence is repeated.                                                                                                                                          | `informational`; repetition is evidence, not a repair instruction.                                                 |
| `reference.same-target-path`             | The authored path spelling differs from the canonical path but resolves to the same target.                                                                                                  | `safe-exact`; Repair may canonicalize the destination.                                                             |
| `reference.same-target-case`             | Case differs from the canonical target spelling while target identity is exact.                                                                                                              | `safe-exact`; Repair may canonicalize the exact target spelling.                                                   |
| `reference.same-target-encoding`         | Percent or path encoding differs from the canonical same-target representation, and exact identity and resulting bytes are established.                                                      | `safe-exact`; Repair may canonicalize the exact encoding.                                                          |
| `reference.same-target-fragment`         | A unique canonical fragment correction identifies the same target and intended fragment.                                                                                                     | `safe-exact`; Repair may apply the unique correction.                                                              |
| `reference.candidate-filename`           | A missing target has a filename-based candidate.                                                                                                                                             | `guided-choice`; display evidence, never choose automatically.                                                     |
| `reference.candidate-title`              | A missing target has a title-based candidate.                                                                                                                                                | `guided-choice`; display the authored title evidence.                                                              |
| `reference.candidate-literal-content`    | A missing target has a literal-content candidate.                                                                                                                                            | `guided-choice`; literal evidence supplies intent for review, not proof.                                           |
| `reference.candidate-route-neighborhood` | A missing target has a bounded route-neighborhood candidate.                                                                                                                                 | `guided-choice`; structural proximity is recommendation evidence only.                                             |
| `reference.candidates-none`              | No bounded filename, title, literal-content, or route-neighborhood candidate exists.                                                                                                         | `manual-decision`; no target is invented.                                                                          |
| `reference.candidates-one`               | Exactly one bounded candidate is available.                                                                                                                                                  | `guided-choice`; one candidate is still not auto-selected.                                                         |
| `reference.candidates-several`           | Several bounded candidates are available.                                                                                                                                                    | `guided-choice`; show all applicable candidates and their evidence.                                                |

Candidate evidence is finite and explicit. Doctor does not perform semantic,
fuzzy, relevance-ranked, synonym, network, or broad text search. A candidate
recommendation may be highlighted for review but never supplies automatic
authority.

### Framework Lifecycle

| Kind                                       | Detectable condition                                                                                                                                                                                   | Resolution or next action                                                                                |
| ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------- |
| `framework.install-absent`                 | No Framework installation is present, and absence is safely established.                                                                                                                               | `informational` with `open-forge install` as a typed next action; Doctor and Repair do not mutate it.    |
| `framework.install-incomplete`             | Installation evidence is present but incomplete or cannot establish a safe state; Framework coverage is `incomplete` or `blocked` as applicable.                                                       | `blocked-repair`; use the accepted lifecycle contract or a manual action after the boundary is resolved. |
| `framework.managed-missing`                | A trusted Framework lifecycle section names a managed file or region that is missing.                                                                                                                  | `targeted-operation`; use `open-forge update`; do not restore it through Doctor.                         |
| `framework.managed-changed`                | A trusted Framework managed file or region differs from its semantic baseline.                                                                                                                         | `targeted-operation`; use `open-forge update`; do not replace it automatically.                          |
| `framework.lifecycle-evidence-unavailable` | Framework lifecycle evidence or required embedded source facts are unavailable; affected Framework coverage is `incomplete`.                                                                           | `blocked-repair`; do not infer installation, ownership, or an update source.                             |
| `framework.lifecycle-evidence-malformed`   | Framework lifecycle evidence cannot be trusted or safely preserved.                                                                                                                                    | `blocked-repair`; preserve the ordinary evidence and do not guess a section or baseline state.           |
| `framework.lifecycle-untrusted`            | Framework facts are readable but provenance, integrity, compatibility, identity, or coverage does not establish `trusted`; coverage is `incomplete` when safely unavailable and `blocked` when unsafe. | `blocked-repair`; force is not inferred.                                                                 |
| `framework.lifecycle-section-missing`      | A Framework section is expected but absent from the lifecycle document; affected Framework coverage is `incomplete` or `blocked`.                                                                      | `blocked-repair`; never treat the section as empty.                                                      |
| `framework.bridge-boundary`                | A provider bridge or root-region boundary is missing, changed, or ambiguous.                                                                                                                           | `blocked-repair`; future lifecycle contracts own exact mutation.                                         |
| `framework.root-region-boundary`           | A managed root region cannot be delimited safely.                                                                                                                                                      | `blocked-repair`; do not replace or adopt the region.                                                    |
| `framework.ownership-conflict`             | Managed, user, and Extension claims overlap incompatibly.                                                                                                                                              | `manual-decision`; ownership is not inferred from severity.                                              |
| `framework.partial-lifecycle`              | Current Framework targets or regions show a partial lifecycle transition.                                                                                                                              | `blocked-repair`; preserve the partial state until a typed recovery action is available.                 |
| `framework.partial-recovery`               | Framework recovery evidence is incomplete or mixed.                                                                                                                                                    | `blocked-repair`; preserve recovery evidence.                                                            |
| `framework.distributed-payload-defect`     | The distributed Framework payload is missing or internally inconsistent.                                                                                                                               | `manual-decision`; report the defect or a typed distribution action; Repair does not alter the payload.  |

All Framework lifecycle findings remain diagnosis, targeted, manual, or future
lifecycle actions. General Repair does not mutate Framework files.

### Extension Lifecycle

| Kind                                   | Detectable condition                                                                                                                                                                                                                              | Resolution or next action                                                                                        |
| -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `extension.lifecycle-document-missing` | The exact new-CLI lifecycle document is absent. Complete inspection may still establish safe absence, but the missing document alone does not prove an empty installed set; Extension coverage is `incomplete` until that inspection is complete. | `informational`; do not infer managed state.                                                                     |
| `extension.lifecycle-document-invalid` | The lifecycle document cannot be parsed as supported schema v1, or its section shape is unsupported or ambiguous; affected Extension coverage is `incomplete` when safely unavailable and `blocked` when unsafe.                                  | `blocked-repair`; retain the ordinary file and do not invent lifecycle facts.                                    |
| `extension.lifecycle-untrusted`        | Extension lifecycle facts are readable but cannot establish trusted package, dependency, path, owner, or fingerprint coverage; coverage is `incomplete` when safely unavailable and `blocked` when unsafe.                                        | `blocked-repair`; no lifecycle mutation through Doctor or Repair.                                                |
| `extension.lifecycle-section-missing`  | An Extension section is expected but absent from the unified lifecycle document; affected Extension coverage is `incomplete` or `blocked`.                                                                                                        | `blocked-repair`; never reconstruct it from paths, bytes, or source.                                             |
| `extension.manifest-missing`           | An expected Extension manifest is absent.                                                                                                                                                                                                         | `manual-decision`; decide whether to author or restore it through an accepted lifecycle action.                  |
| `extension.manifest-malformed`         | An Extension manifest cannot be trusted.                                                                                                                                                                                                          | `blocked-repair`; do not infer dependencies or ownership.                                                        |
| `extension.duplicate-id`               | More than one lifecycle record or manifest claims one Extension ID.                                                                                                                                                                               | `blocked-repair`; no identity winner is chosen.                                                                  |
| `extension.unknown-id`                 | Lifecycle evidence names an unknown Extension ID.                                                                                                                                                                                                 | `manual-decision`; catalogue or lifecycle authority is unresolved.                                               |
| `extension.version-invalid`            | An Extension version is missing, malformed, or incompatible with its evidence.                                                                                                                                                                    | `manual-decision`; do not choose a version.                                                                      |
| `extension.managed-missing`            | Trusted lifecycle facts name a managed Extension file that is missing.                                                                                                                                                                            | `targeted-operation`; use `open-forge extension update`; no restoration through Doctor.                          |
| `extension.managed-changed`            | A trusted managed Extension file differs from recorded evidence.                                                                                                                                                                                  | `targeted-operation`; use `open-forge extension update`; no automatic replacement.                               |
| `extension.dependency-missing`         | A declared Extension dependency is unavailable.                                                                                                                                                                                                   | `manual-decision`; decide the dependency action or use a future lifecycle action.                                |
| `extension.dependency-cycle`           | Extension dependencies contain a cycle.                                                                                                                                                                                                           | `blocked-repair`; dependency order is not guessed.                                                               |
| `extension.dependency-incompatible`    | Dependency versions or capabilities cannot satisfy the declared relation.                                                                                                                                                                         | `manual-decision`; no version is selected automatically.                                                         |
| `extension.source-unavailable`         | The Extension source or catalogue needed for source-dependent diagnosis is unavailable; source-dependent coverage is `incomplete`.                                                                                                                | `informational`; retain independently readable installed IDs and ownership facts and do not substitute a source. |
| `extension.catalogue-unavailable`      | The declared catalogue cannot be inspected; Extension coverage is `incomplete` or `blocked` according to the boundary.                                                                                                                            | `blocked-repair`; no catalogue fallback is inferred.                                                             |
| `extension.partial-lifecycle`          | Extension targets or lifecycle facts show a partial transition.                                                                                                                                                                                   | `blocked-repair`; preserve the partial state until a typed recovery action is available.                         |
| `extension.ownership-collision`        | User, Framework, or Extension ownership claims conflict.                                                                                                                                                                                          | `manual-decision`; ownership is not inferred.                                                                    |
| `extension.bridge-registration`        | Extension bridge or registration evidence is missing or inconsistent.                                                                                                                                                                             | `manual-decision`; report the evidence or use a typed future lifecycle action.                                   |
| `extension.unmanaged-like-content`     | Unmanaged content resembles an Extension without lifecycle evidence.                                                                                                                                                                              | `informational`; do not adopt, register, or remove it.                                                           |

All Extension lifecycle findings remain diagnosis, typed next actions, manual
decisions, or blocked boundaries. Doctor does not run `extension install`,
`extension update`, `extension remove`, or `extension create`, and general Repair
does not mutate Extension files, lifecycle sections, manifests,
dependencies, catalogues, or registrations.

## Output

Doctor output has one hierarchy:

1. Overall semantic status.
2. Exact workspace identity and selection method.
3. An explicit read-only statement that no changes were made.
4. Overall and per-domain coverage, including limitations.
5. Counts by resolution lane and finding severity where counts are available.
6. Immediate typed actions, including safe Repair preview or a targeted or
   manual next action.
7. The six deterministic domain groups, lifecycle section trust states, and
   their findings.

The default human view is `expanded`, as defined by the shared global contract.
Compact output retains workspace identity, status, coverage, resolution lane,
typed subject, candidate count when applicable, and the required next action for
each finding. Expanded output adds evidence, provenance, locations, and the
basis for each candidate. Neither view emits a health score or percentage.

An illustrative expanded result is:

```text
Open Forge doctor
Workspace: D:/work/example
Selected by: current directory
Mode: read-only; no files changed
Status: requires attention
Coverage: complete; 6 domains complete

Resolution counts
  safe-exact: 2
  guided-choice: 1
  targeted-operation: 1
  manual-decision: 1
  blocked-repair: 0
  informational: 4

Immediate actions
  Preview safe exact repairs: open-forge repair --automatic --dry-run
  Review the guided local-reference candidates before selecting a target.

1. Workspace and entry
   Coverage: complete
   Findings: none

2. Recovery and residual state
   Coverage: complete
   Findings: none

3. Routes, metadata, overwrites, and generated navigation
   Coverage: complete
   Finding: route.generated-entry-order
     Resolution: targeted-operation
     Next: open-forge index --dry-run

4. Local references
   Coverage: complete
   Finding: reference.target-missing
     Resolution: guided-choice; candidates: 1
     Next: review the candidate and use an explicit relink or the Repair wizard.
```

The values are illustrative. Human output may say `requires attention` for the
typed status `attention`. `--verbose` is separate from expanded view and adds
bounded diagnostic detail without changing facts, order, coverage, findings, or
status.

`--json` emits one complete structured result derived from the same typed result
as human output. JSON is non-interactive and never prompts. It includes the
complete domain groups, coverage and limitations, counts, findings, typed
subjects and evidence, provenance, resolution lanes, candidates or proposals,
next actions, status, and post-condition facts that the contract exposes. Exact
field names, schema compatibility, and exit mapping follow the shared CLI
Architecture.

## Semantic Results

| Result        | Meaning                                                                                                                                         |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | All six domains have complete coverage and no warning or error finding requires attention. Informational facts alone do not change this result. |
| `attention`   | All six domains have complete coverage, but one or more actionable warning or error findings remain.                                            |
| `incomplete`  | Safe facts are available, but one or more required domains has trustworthy partial coverage.                                                    |
| `invalid`     | The command input does not follow the exact Doctor grammar or shared global-flag contract.                                                      |
| `blocked`     | A required workspace, identity, containment, parsing, or other safety boundary cannot be established.                                           |
| `failed`      | An unexpected failure prevents normal diagnosis or result formation.                                                                            |
| `interrupted` | The caller cancels or interrupts diagnosis before completion, with the accepted interruption meaning.                                           |

Coverage and health are separate. A domain can be `complete` with findings. A
workspace can be safely absent from a lifecycle boundary and still have complete
diagnostic coverage. Informational findings alone do not produce `attention`.
Severity does not override coverage and does not select a repair.

## Errors And Omission States

Doctor has no domain-specific omission state because it has no domain selector.
Omitting all operands and doctor-specific flags is the required valid form. The
following states are finite:

- An unexpected operand or doctor-specific flag is `invalid`.
- An unknown flag, malformed global value, missing global value, or invalid
  shared repetition is `invalid` under the shared contract.
- `--help` or `--version` alone, or with other compatible global flags, is a
  terminal informational request. It does not diagnose.
- Supplying both `--help` and `--version` is `invalid`.
- A missing, unavailable, or non-directory selected workspace is `blocked`.
- An unsafe path, physical alias, ambiguous identity, or required unreadable
  boundary is `blocked`.
- A trustworthy partial inspection is `incomplete`, not a guessed complete
  result.

Every ordinary error names the `doctor` operation, affected workspace or typed
subject when known, direct cause, and useful next action. Human primary results
for `complete`, `attention`, and `incomplete` use stdout. Primary human errors
for `invalid`, `blocked`, `failed`, and `interrupted` use stderr, in line with
the accepted Index output policy. JSON always emits one complete structured
result to stdout for every semantic status; separate bounded diagnostics use
stderr.

## Examples

Run complete diagnosis for the exact current workspace:

```text
open-forge doctor
```

Select another exact workspace and a compact human projection:

```text
open-forge doctor --workspace ../another-workspace --view=compact
```

Request the complete non-interactive result:

```text
open-forge doctor --json
```

Add bounded diagnostics without changing diagnosis:

```text
open-forge doctor --verbose
```

Doctor never prompts and never applies the suggested action in any of these
forms. A safe exact finding may show the explicit next preview
`open-forge repair --automatic --dry-run`. A generated-navigation finding may
show the accepted `open-forge index --dry-run` action. Framework and Extension
lifecycle findings may show the accepted `install`, `update`, or Extension
operation as a typed next action, but Doctor never invokes it or grants its
mutation authority.

## Non-Goals And Architecture Boundary

Doctor does not:

- Mutate files, generated navigation, lifecycle state, recovery bundles or
  drafts, or temporary files.
- Prompt, choose a candidate, accept a recommendation, or turn severity into
  repair authority.
- Build a plan, save a plan or report, create a session, or invoke a
  public command.
- Accept operands, domain filters, include or exclude selectors, caps, repair or
  fix flags, `--automatic`, `--dry-run`, `--relink`, or aliases.
- Perform semantic, fuzzy, relevance-ranked, synonym, vector, or network search.
- Fetch or validate external URLs. External no-fetch facts remain informational
  when the declared local coverage is complete.
- Repair authored prose, labels, route topology, metadata, overwrites, recovery,
  Framework lifecycle, or Extension lifecycle.
- Report a health score, percentage, or fabricated complete coverage.

The accepted CLI Architecture defines the exact JSON schema, numeric exits,
parser and library, filesystem alias and physical-identity mechanics, token and
hashing rules, concurrency, numeric resource limits, and lifecycle mutation
boundaries. Gate 5 must prove source-generated YamlDotNet and STJ serialization,
fixed Markdig where used, real `System.IO`, Native AOT, OS locking, isolated
tests, and package journeys. Doctor remains non-shipping and does not claim that
proof.

## Public Verification

Conformance evidence must cover:

- The exact operand-free grammar, rejection of doctor-specific flags and aliases,
  all six shared global flags, terminal help and version, and shared repetition
  and composition rules.
- Exact current-directory and `--workspace` selection without parent, Git-root,
  package-root, or marker discovery.
- The six fixed domains in order, with every domain retained when a dependency is
  incomplete or blocked.
- Complete, incomplete, and blocked coverage independently of finding health.
- Independent severity and resolution, stable kinds, typed subjects, evidence,
  provenance, candidates, proposals, and typed next actions.
- Every workspace and entry kind, including Loader, entrypoint, compatibility,
  identity, path, metadata, parsing, root, and detached boundaries.
- The four recovery kinds for verified finals, incomplete drafts, final-name
  collisions, and unavailable provenance, with exact paths and no live-target or
  activity inference.
- Semantic representative payload validation with exact declared
  lengths and hashes, bounded buffers and memory independent of entry size, and
  no extraction, disclosure, retention, or materialization.
- Every route, metadata, overwrite, generated-region, generated-entry, and
  compatibility kind.
- Every local-reference kind, including valid and missing targets, fragments,
  malformed and unsafe destinations, images, external unchecked facts, cycles,
  repeats, exact same-target corrections, bounded candidate evidence, and zero,
  one, or several candidates.
- Every Framework and Extension lifecycle kind without allowing general Repair
  to mutate those domains, including unified-document section isolation,
  absent/trusted/untrusted/incomplete/blocked states, ownership facts,
  and source-unavailable installed facts.
- Compact, expanded, JSON, and verbose projections from one typed result, with no
  health score or percentage and no prompts.
- `complete`, `attention`, `incomplete`, `invalid`, `blocked`, `failed`, and
  `interrupted` meanings, including informational findings that do not create
  `attention`.
- Read-only, stateless, repeatable behavior with no plan, recovery-bundle,
  temporary, lifecycle, or public-command effect.

## Related Current Sources

- [Doctor Command Contract Set](_doctor.md)
- [Doctor Behavior Contract](behavior.md)
- [Repair Interface Contract](../repair/interface.md)
- [Repair Behavior Contract](../repair/behavior.md)
- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
