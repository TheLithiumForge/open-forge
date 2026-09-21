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

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact shared JSON result schema and numeric exit mapping. The accepted [CLI
Architecture](../../architecture.md) defines parser roles, filesystem identity,
hashing and concurrency constraints, resource boundaries, and C# and .NET Native
AOT structure. Gate 5 must prove source-generated
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

| Order | Domain                                                   | Boundary                                                                                                                                                                                                                         |
| ----- | -------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1     | `workspace and entry`                                    | Establish the selected workspace, `.agents` boundary, Loader, entrypoints, source identity, parsing, reachable roots, and the Workspace Library record, every registered source-root inventory, and registered-projection facts. |
| 2     | `recovery and residual state`                            | Report exact named external final bundles and incomplete drafts, with one semantic final-ZIP integrity check and Cleanup guidance.                                                                                               |
| 3     | `routes, metadata, overwrites, and generated navigation` | Compare authored topology and metadata with derived route relationships and generated `Entries`.                                                                                                                                 |
| 4     | `local references`                                       | Inspect supported authored local references, target and fragment resolution, containment, and bounded repair evidence.                                                                                                           |
| 5     | `Framework lifecycle`                                    | Diagnose the installed or absent Framework payload, Framework ownership claims, managed files and regions, trust, ownership boundaries, and recovery evidence.                                                                   |
| 6     | `Extension lifecycle`                                    | Diagnose the `extensions` claims of the ownership lock, manifests, managed files, dependencies, source availability, catalogues, ownership, and registration evidence.                                                           |

All six domain groups remain in the result. A dependent domain reports
`incomplete` or `blocked` coverage when an earlier boundary prevents trustworthy
work; it does not disappear.

## Ownership And Current Target Boundary

Ownership is read from `.agents/open-forge.lock.json` through the shared forgiving
reader. Framework, Extension, and Library claims remain separate. Neither the
old lifecycle document nor the old Library record supplies ownership facts.
An absent, unreadable, nonordinary, malformed, or uninterpretable lock supplies
no usable claims and produces an informational ownership observation, without
blocking the command or reconstructing ownership from files. Read-only commands
never create or repair the lock. Actual source, target, route, and recovery
boundaries still determine their own coverage and findings.

Target comparison uses actual disk content against current intended content:
the running embedded Framework payload, or the currently read exact Extension
source recorded by its owner. A recorded version or stored content hash does not
gate comparison. Missing targets remain `missing`; unavailable reads or intended
sources remain `unavailable`; unsafe physical or Markdown boundaries remain
`blocked`. Comparable content is `current` when equal and `changed` otherwise.
The existing immutable `open-forge-markdown-v1` policy normalizes line endings
and eligible generated content only; authored whitespace and final-newline
choices remain significant. Non-Markdown Extension payloads use exact bytes.
Comparison evidence is computed during the invocation and stores no baseline.

Root managed hosts compare only their `open-forge` region. Scoped Framework
entrypoints use the existing canonical payload alignment; ambiguous or missing
alignment makes intended comparison unavailable. Generated Entries compare with
the current authored route projection, without consulting stored fingerprints.

The existing lifecycle vocabulary remains a presentation of observed coverage.
`absent` still requires independent complete footprint and recovery inspection;
no lock or matching file alone proves absence or ownership. Readable claims may
remain reportable while their source is unavailable. An unavailable lock yields
an observation, not an installation error or a trusted empty inventory.

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
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
--help
--version
```

The shared contract defines their value grammar, defaults, repetition,
composition, terminal behavior, and errors. Doctor does not redefine them or
make `--automatic`, `--dry-run`, or `--relink` global. Those spellings are not
accepted by Doctor.

An omitted global flag uses the shared default. Repeated Boolean global flags
retain the shared idempotent behavior. Repeating `--workspace` or `--detail`
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
The Workspace Library kinds remain in `workspace and entry`; Doctor retains six
domains. Ownership observations replace legacy document-admission findings.
Unused legacy identifiers may remain internal during the migration and do not
establish an emission path or a current document gate.

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

### Workspace Libraries

The following finite subcatalogue belongs to the existing `workspace and entry`
domain. It diagnoses only typed Workspace Library evidence and does not add a
seventh Doctor domain:

| Kind                                  | Detectable condition                                                                                                                                                                                                                          | Resolution or next action                                                                              |
| ------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `library.ownership-observation`       | Ownership claims are absent, unavailable, or uninterpretable; no ownership is inferred.                                                                                                                                                       | `informational`; do not gate diagnosis or write state.                                                 |
| `library.source-root-invalid`         | A typed `sourceRoot` is malformed, not contained, or not an ordinary directory.                                                                                                                                                               | `blocked-repair`; correct the exact source-root boundary without creating or adopting it.              |
| `library.source-root-aliased`         | A source root or its ancestry physically aliases another identity or cannot be assigned one safe physical identity.                                                                                                                           | `blocked-repair`; resolve the physical identity ambiguity.                                             |
| `library.inventory-incomplete`        | Complete eligible inventory cannot be established for one or more Library source roots named by a readable ownership catalogue; Library coverage is `incomplete` and no source addition or retirement is inferred.                            | `informational`; report incomplete coverage and do not narrow the inventory silently.                  |
| `library.projection-missing`          | A typed registered destination has no current directory entry.                                                                                                                                                                                | `manual-decision`; report projection drift and leave link creation to the accepted Library operation.  |
| `library.projection-dangling`         | The expected relative link is present, but its source target is unavailable; Doctor does not follow it to read source bytes.                                                                                                                  | `blocked-repair`; preserve the link and resolve the source boundary explicitly.                        |
| `library.projection-retargeted`       | A registered destination is a relative link whose raw target differs from the exact recorded target.                                                                                                                                          | `blocked-repair`; preserve the occupant and do not retarget it automatically.                          |
| `library.path-collision`              | A Library mapping collides with another mapping, consumer control, lifecycle/Framework/Extension path, or another manager's physical identity.                                                                                                | `manual-decision`; ownership and authored intent must be resolved explicitly.                          |
| `library.link-capability-unsupported` | A typed or otherwise proven capability fact says the required relative file-link projection is unsupported; Doctor does not probe link capability.                                                                                            | `blocked-repair`; use an environment that proves the accepted capability or make an authored decision. |
| `library.extension-collision`         | An exact Library projection claim or real link occupant conflicts with an Extension target or ownership claim.                                                                                                                                | `manual-decision`; keep Library and Extension authorities separate and choose no winner automatically. |
| `library.recovery-safe-exact`         | A semantically verified current-v1 residual is Library-attributed to the selected workspace and proves one exact typed ordinary-record, ordinary-file, or relative-file-link recovery effect with safe no-follow identity and no third state. | `safe-exact`; Repair may admit the exact effect through its existing automatic or guided selection.    |

Library findings carry typed Library ID, source-root, mapping, projection, or
residual subjects and provenance. They never invoke Library, mutate, adopt,
delete recovery, probe link capability, or infer identity from a filename or
path. Unavailable Library ownership produces an informational ownership observation,
complete observation coverage, and no inferred registrations or mappings. For a readable ownership catalogue, Doctor attempts a complete
eligible inventory for every named source root. Those registered roots are the
complete declared Library coverage. `library.projection-missing` is safe drift
only when every registered-root inventory and mapping fact is complete; an
incomplete inventory emits `library.inventory-incomplete` and remains
incomplete, with no safe prefix treated as complete. Doctor never enumerates an
unregistered source root, and unsafe ambiguity remains blocked.

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
for each exact named final ZIP: source-generated current schema-v1 manifest
decoding, required immutable typed attribution validation, exact ordered entry
names and counts, declared lengths and hashes, and exact payload bytes. A valid
current-v1 final is `Verified`; a schema-1 final missing or carrying invalid
attribution is `Malformed`/unattributed, an unknown schema version is
`Unsupported`, and an unreadable final is `Unavailable`. Finals with missing or
invalid attribution remain preserved; Doctor never migrates,
rewrites, repairs, deletes, adopts, or infers them. An exact named draft is
always exact-name, path-only `Incomplete` and never preparation; Doctor does not
inspect or use draft bytes for attribution.
Doctor never creates, renames, deletes, extracts, restores, rolls back, or
rebinds a recovery item.

The exact schema-v1 attribution vocabulary, valid producer/operation/subject
combinations, and required non-null workspace identity are defined by the
[Mutation And Recovery Technical Design](../../technical-designs/mutation-and-recovery.md#schema-v1-attribution-vocabulary).
Doctor accepts no unknown value or fallback attribution.

Doctor reports only the item's exact path, kind, integrity condition, and the
separate Cleanup action. It does not inspect live targets, classify target
state, or infer activity. Payload validation uses fixed bounded buffers and never
extracts, discloses, renders, logs, returns, retains, or materializes payload
bytes. Cleanup owns deletion only after it acquires the same-workspace lease,
re-enumerates the selected bucket, and repeats final ordinary path/kind and
semantic validation.

Recovery attribution, when available, remains a neutral producer fact beside
the recovery models. Lifecycle diagnosis consumes it only after verifying the
current-v1 final's immutable typed attribution to a Framework producer,
operation, and typed subject `{kind, identity}` for the selected workspace, plus
the exact neutral recovery-state comparison defined below. A generic
mixed-current-state observation alone is insufficient. It is not a Doctor enum,
a presentation dependency, or a generic bag.

| Kind                              | Detectable condition                                                                                                                                                                    | Resolution or next action                                                                                                        |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `recovery.bundle-recognized`      | An exact named current-v1 final ZIP has valid immutable typed attribution and is semantically verified under the selected workspace bucket.                                             | `informational`; report its path and `Verified` integrity, then offer the separate [cleanup operation](../cleanup/interface.md). |
| `recovery.draft-recognized`       | An exact named draft is present under the selected workspace bucket.                                                                                                                    | `informational`; report its path as `Incomplete`; it is never a recovery preparation.                                            |
| `recovery.bundle-collision`       | An exact deterministic final name contains malformed, unsupported, or unreadable content.                                                                                               | `blocked-repair`; preserve it and report the exact integrity condition.                                                          |
| `recovery.provenance-unavailable` | A final ZIP cannot provide complete current-v1 semantic schema, valid immutable typed attribution, exact ordered entries, prior payload, intended fingerprint, or operation provenance. | `blocked-repair`; preserve it; Cleanup cannot delete it without semantic validation and final under-lease revalidation.          |

### Routes, Metadata, Overwrites, And Generated Navigation

| Kind                                | Detectable condition                                                                                                                                   | Resolution or next action                                                                                                                      |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `route.entrypoint-missing`          | A routed folder lacks its one recognized entrypoint.                                                                                                   | `manual-decision`; route authoring is not general repair.                                                                                      |
| `route.entrypoint-duplicate`        | A folder has duplicate recognized entrypoints.                                                                                                         | `blocked-repair`; resolve the route identity manually.                                                                                         |
| `route.escape`                      | A route destination leaves the containing workspace or route boundary.                                                                                 | `blocked-repair`; no route escape is repaired automatically.                                                                                   |
| `route.unreachable`                 | A route is not reachable from its established root or exposing parent.                                                                                 | `manual-decision`; do not invent a parent.                                                                                                     |
| `route.detached`                    | A complete-looking route tree is detached from the Loader or selected root.                                                                            | `manual-decision`; detached content is not adopted.                                                                                            |
| `route.metadata-required-missing`   | Required route metadata is absent.                                                                                                                     | `manual-decision`; authored meaning is not invented.                                                                                           |
| `route.title-invalid`               | A required route title is missing or structurally invalid.                                                                                             | `manual-decision`; do not derive authored meaning from a filename.                                                                             |
| `route.axioms-invalid`              | Required `Axioms` structure or inherited sentinel is missing or malformed; route coverage is `blocked` when active rules cannot be established safely. | `manual-decision`; do not rewrite active rules automatically.                                                                                  |
| `route.generated-region-stale`      | A valid generated region does not match current route facts.                                                                                           | `targeted-operation`; use accepted `index` behavior, not general Repair.                                                                       |
| `route.generated-region-missing`    | A required generated region is absent; route coverage is `blocked` when its boundary cannot be established safely.                                     | `targeted-operation`; use accepted `index` behavior only after the route boundary is valid; generated-region authoring remains outside Repair. |
| `route.generated-region-malformed`  | A generated region cannot be parsed as one valid bounded region.                                                                                       | `blocked-repair`; do not repair Entries headings through general Repair.                                                                       |
| `route.generated-region-misplaced`  | A generated region is not in its accepted location.                                                                                                    | `manual-decision`; no authored-file rewrite is inferred.                                                                                       |
| `route.generated-region-duplicate`  | More than one generated region claims one entrypoint.                                                                                                  | `blocked-repair`; resolve the boundary manually.                                                                                               |
| `route.generated-entry-missing`     | A required generated entry is absent.                                                                                                                  | `targeted-operation`; use `index` after the route boundary is valid.                                                                           |
| `route.generated-entry-extra`       | A generated entry has no current routed source.                                                                                                        | `targeted-operation`; use `index` after the route boundary is valid.                                                                           |
| `route.generated-entry-order`       | Generated entries are present but not in canonical order.                                                                                              | `targeted-operation`; use `index`.                                                                                                             |
| `route.generated-entry-path`        | A generated destination is not the canonical direct-child path.                                                                                        | `targeted-operation`; use `index`, not a relink.                                                                                               |
| `route.generated-entry-description` | A generated description does not match current authored metadata.                                                                                      | `targeted-operation`; use `index` after metadata is valid.                                                                                     |
| `route.generated-entry-tags`        | Generated tags do not match current authored metadata.                                                                                                 | `targeted-operation`; use `index` after metadata is valid.                                                                                     |
| `route.overwrite-orphan`            | An overwrite companion has no valid base source; route coverage is `blocked` when the relationship is unsafe to inspect.                               | `manual-decision`; never index or adopt the orphan.                                                                                            |
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

Candidate bases are exact producer-observed facts over the bounded contained
source universe. Doctor retains every applicable base for each candidate. It
does not rank bases or candidates, select a winner, fall back to another basis,
or infer a basis through semantic, fuzzy, synonym, network, or broad text
search. The bases have these conservative meanings:

- `filename` is an exact canonical leaf filename.
- `title` is an exact authored link label or title matched to the parsed primary
  title.
- `literal-content` is an exact bounded link-label or title occurrence supplied
  by retained parsed facts; it is not a broad body search.
- `route-neighborhood` is an exact established route parent, child, or sibling
  relation.

When current typed facts cannot establish a basis, that basis is absent and
Doctor does not infer it.

### Framework Lifecycle

| Kind                                       | Detectable condition                                                                                                                                                                                                                                                                                                | Resolution or next action                                                                               |
| ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `framework.install-absent`                 | No Framework installation is present, and absence is safely established.                                                                                                                                                                                                                                            | `informational` with `open-forge install` as a typed next action; Doctor and Repair do not mutate it.   |
| `framework.ownership-observation`          | Ownership claims are absent, unavailable, or uninterpretable; no ownership is inferred.                                                                                                                                                                                                                             | `informational`; do not gate diagnosis or write state.                                                  |
| `framework.managed-missing`                | A trusted Framework lifecycle section names a managed file or region that is missing.                                                                                                                                                                                                                               | `targeted-operation`; use `open-forge update`; do not restore it through Doctor.                        |
| `framework.managed-changed`                | A trusted Framework managed file or region differs from its current intended payload.                                                                                                                                                                                                                               | `targeted-operation`; use `open-forge update`; do not replace it automatically.                         |
| `framework.lifecycle-evidence-unavailable` | Framework lifecycle evidence or required embedded source facts are unavailable; affected Framework coverage is `incomplete`.                                                                                                                                                                                        | `blocked-repair`; do not infer installation, ownership, or an update source.                            |
| `framework.bridge-boundary`                | A provider bridge or root-region boundary is missing, changed, or ambiguous.                                                                                                                                                                                                                                        | `blocked-repair`; future lifecycle contracts own exact mutation.                                        |
| `framework.root-region-boundary`           | A managed root region cannot be delimited safely.                                                                                                                                                                                                                                                                   | `blocked-repair`; do not replace or adopt the region.                                                   |
| `framework.ownership-conflict`             | Managed, user, and Extension claims overlap incompatibly.                                                                                                                                                                                                                                                           | `manual-decision`; ownership is not inferred from severity.                                             |
| `framework.partial-lifecycle`              | Within one exact trusted declared managed subject or set, at least one expected member is current and at least one other expected member is non-current.                                                                                                                                                            | `blocked-repair`; preserve the partial state until a typed recovery action is available.                |
| `framework.partial-recovery`               | For one semantically verified same-workspace Framework-attributed final, a neutral producer compares every ordered existing-target entry with its exact prior and intended states; every entry is safely observable, at least one matches prior, at least one other matches intended, and none is third or unknown. | `blocked-repair`; preserve recovery evidence.                                                           |
| `framework.distributed-payload-defect`     | The distributed Framework payload is missing or internally inconsistent.                                                                                                                                                                                                                                            | `manual-decision`; report the defect or a typed distribution action; Repair does not alter the payload. |

`framework.partial-lifecycle` is a finite mixed-current-state observation within
one exact trusted declared managed subject or set. It requires at least one
expected member to be current and at least one other expected member to be
non-current. It reports the observed mixed state only. It never reports
operation history, transition intent, or recovery attribution. The more
specific `framework.managed-missing` and `framework.managed-changed` findings
remain alongside it whenever their facts apply.

`framework.partial-recovery` requires one semantically verified current-v1 final
whose immutable typed attribution identifies the finite Framework producer, its
exact operation, and a `workspace` subject whose identity matches the selected
workspace key. A neutral producer then compares the current ordinary target
state for every ordered existing-target entry (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) with its recorded exact prior and
intended states. A prior match means a safely observable ordinary file contained
by the workspace has the exact recorded prior length and lowercase SHA-256. An
intended match means the same exact ordinary-file comparison against the
recorded intended state, or safely proven absence when the intended state is
absence. Absence is not unavailable. Doctor emits the finding only when every
compared entry is safely observable, at least one entry matches prior, at least
one other matches intended, and no compared entry is third or unknown. Each
verified Framework-attributed final is evaluated independently in deterministic
catalogue order; entries from separate bundles are never ranked, selected as a
winner, or combined. All-intended is a no-finding state compatible with a
completed historical operation; all-prior is a no-partial-finding state
compatible with an unapplied or fully restored operation. Unavailable, unsafe,
non-ordinary, third, unknown, or mismatched state produces incomplete or blocked
coverage or another applicable exact finding, never partial recovery. The finding describes
mixed current state relative to recovery evidence and never claims that recovery
occurred. Doctor never guesses attribution or state from a GUID, path, filename,
command text, ordered entry, or untrusted bytes. The neutral comparison may
expose only finite states or bounded evidence; no payload bytes enter Doctor
output. Attribution remains a verified neutral producer fact beside recovery
models, not a Doctor enum or presentation dependency.

All Framework lifecycle findings remain diagnosis, targeted, manual, or future
lifecycle actions. General Repair does not mutate Framework files.

### Extension Lifecycle

| Kind                                | Detectable condition                                                                                                                                              | Resolution or next action                                                                                        |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `extension.ownership-observation`   | Ownership claims are absent, unavailable, or uninterpretable; no ownership is inferred.                                                                           | `informational`; do not gate diagnosis or write state.                                                           |
| `extension.manifest-missing`        | An expected Extension manifest is absent.                                                                                                                         | `manual-decision`; decide whether to author or restore it through an accepted lifecycle action.                  |
| `extension.manifest-malformed`      | An Extension manifest cannot be trusted.                                                                                                                          | `blocked-repair`; do not infer dependencies or ownership.                                                        |
| `extension.duplicate-id`            | The inspected Extension source has an ambiguous or duplicate package identity.                                                                                    | `blocked-repair`; no identity winner is chosen.                                                                  |
| `extension.unknown-id`              | Lifecycle evidence names an unknown Extension ID.                                                                                                                 | `manual-decision`; catalogue or lifecycle authority is unresolved.                                               |
| `extension.version-invalid`         | An Extension version is missing, malformed, or incompatible with its evidence.                                                                                    | `manual-decision`; do not choose a version.                                                                      |
| `extension.managed-missing`         | Trusted lifecycle facts name a managed Extension file that is missing.                                                                                            | `targeted-operation`; use `open-forge extension update`; no restoration through Doctor.                          |
| `extension.managed-changed`         | A trusted managed Extension file differs from recorded evidence.                                                                                                  | `targeted-operation`; use `open-forge extension update`; no automatic replacement.                               |
| `extension.dependency-missing`      | A declared Extension dependency is unavailable.                                                                                                                   | `manual-decision`; decide the dependency action or use a future lifecycle action.                                |
| `extension.dependency-cycle`        | Extension dependencies contain a cycle.                                                                                                                           | `blocked-repair`; dependency order is not guessed.                                                               |
| `extension.dependency-incompatible` | Dependency versions or capabilities cannot satisfy the declared relation.                                                                                         | `manual-decision`; no version is selected automatically.                                                         |
| `extension.source-unavailable`      | The Extension source or catalogue needed for source-dependent diagnosis is unavailable; source-dependent coverage is `incomplete`.                                | `informational`; retain independently readable installed IDs and ownership facts and do not substitute a source. |
| `extension.catalogue-unavailable`   | The declared catalogue cannot be inspected; Extension coverage is `incomplete` or `blocked` according to the boundary.                                            | `blocked-repair`; no catalogue fallback is inferred.                                                             |
| `extension.partial-lifecycle`       | Within one exact trusted declared managed subject or set, at least one expected member is current and at least one other expected member is non-current.          | `blocked-repair`; preserve the partial state until a typed recovery action is available.                         |
| `extension.ownership-collision`     | User, Framework, or Extension ownership claims conflict.                                                                                                          | `manual-decision`; ownership is not inferred.                                                                    |
| `extension.bridge-registration`     | One exact lifecycle-owned routed Extension payload target has a missing, unreadable, or inconsistent ordinary generated-navigation parent `Entries` registration. | `manual-decision`; report the evidence or use a typed future lifecycle action.                                   |

`extension.partial-lifecycle` uses the same finite mixed-current-state rule as
Framework lifecycle: within one exact trusted declared managed subject or set,
at least one expected member is current and at least one other is non-current.
It reports observed mixed state only, never operation history, transition intent,
or recovery attribution. The more specific `extension.managed-missing` and
`extension.managed-changed` findings remain whenever their facts apply.

`extension.bridge-registration` is a set-valued producer horizon. It forms one
typed observation for each exact lifecycle-owned routed Extension payload
target whose exact reviewed source facts form exactly one ordinary
generated-navigation parent `Entries` registration. Lifecycle supplies the
target identity and owners. Exact reviewed source bytes and metadata establish
the routed role. Neutral generated-navigation formation and projection supply
the exact parent host and expected entry, and generated-entry comparison
supplies the current, missing, unreadable, or inconsistent observation. The
singular target wording applies to each observation, not to one global target.
Content is inspected only when readable.

An unavailable source makes the source-dependent observation `incomplete`; an
ambiguous mapping is `blocked`. Neither case selects or reconstructs a role.
This observation adds no manifest or lifecycle field or schema change. It never
uses a compatibility path, provider bridge, symbolic link, registry, dependency
injection, fuzzy path, or content inference.

`extension.bridge-registration` remains backed by the accepted set-valued
contributor observation. No finding is synthesized without its required current
facts.

Neither legacy `open-forge.extensions.json`, package-source manifests, broad
`.agents` recursion, payload/path/byte resemblance, nor Framework bridges may
substitute for producer-owned facts. Static CLI composition is wiring only and
cannot manufacture observations; dependency injection and a runtime registry
are not observation substitutes.

All Extension lifecycle findings remain diagnosis, typed next actions, manual
decisions, or blocked boundaries. Doctor does not run `extension install`,
`extension update`, `extension remove`, or `extension create`, and general Repair
does not mutate Extension files, lifecycle sections, manifests,
dependencies, catalogues, or registrations.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

Doctor text has an opt-in minimal-warning presentation. Without an explicit
severity filter, minimal text renders known error and warning findings with
their exact source/path/location, cause or message, and first action; info
findings remain a Full-only text detail. An explicit `--detail-filter` is
authoritative and selects only its requested severity rows. This presentation
rule does not change the typed result, status, streams, structured schema, or
the JSON detail ladder.

### Statuses and headlines

| Status                  | When                                                | Headline                                                                                        | Exit | Stream |
| ----------------------- | --------------------------------------------------- | ----------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | no error, no warning, complete coverage             | `No problems found.`                                                                            |    0 | stdout |
| completed               | Info findings only                                  | `No problems found. <N> info findings were recorded.`                                           |    0 | stdout |
| completed-with-warnings | warnings, no errors                                 | `No errors. <W> warnings and <I> info findings were recorded.` (omit the info clause when zero) |    2 | stdout |
| completed-with-warnings | errors present                                      | `<E> errors, <W> warnings and <I> info findings.` (singular forms when 1; omit zero parts)      |    2 | stdout |
| incomplete              | a category could not finish                         | previous sentence + ` <K> checks could not finish.`                                             |    3 | stdout |
| invalid-input           | operand or unknown flag                             | family `invalid-input`: `Cannot run doctor: <problem>.`                                         |    4 | stderr |
| blocked                 | workspace missing, not a directory, unsafe boundary | `Cannot check this workspace: <reason>.`                                                        |    5 | stderr |
| failed                  | unexpected error                                    | `Doctor stopped because of an unexpected error: <reason>.`                                      |    1 | stderr |
| cancelled               | Ctrl+C                                              | `Doctor was cancelled.`                                                                         |  130 | stderr |

Doctor retains `completed-with-warnings` for warning-only and error-containing
diagnoses; exposing warning rows at minimal text does not change that status,
its exit, or its stream. Errors do not make diagnosis fail.

### Text by level

`minimal`, healthy:

```text
No problems found.
  6 checks complete. 21 links and 20 routes checked.
```

`minimal`, warnings only:

Minimal text renders the known warning findings immediately with their exact
source/path/location, cause or message, and first action. Info findings remain
counted but omitted; no extra run is required to see warnings.

`minimal`, errors:

Minimal text renders all known error and warning findings with their exact
source/path/location, cause or message, and first action. Info findings remain
counted but omitted; any hint for hidden findings points to `--detail full`.
The typed next action remains governed by the Next rules below.

`minimal`, incomplete:

Minimal text renders known warning findings with their exact
source/path/location, cause or message, and first action while retaining the
coverage sentence for checks that could not finish. Info findings remain
omitted; any hint for hidden findings points to `--detail full`.

`standard`, errors and warnings (categories appear only when they have listed
findings; Info stays counted):

```text
1 error, 2 warnings and 3 info findings.
Workspace: D:/work/myrepo

Routes and Entries
  Error    .agents/skills/pdf/SKILL.md:1:1   Frontmatter is invalid
           The frontmatter block is not closed. Edit the file by hand.

Links
  Warning  .agents/loader.md:105:3           Broken link
           The linked file was not found: patterns/_patterns.md
           Possible target (not chosen): .agents/patterns/_patterns.md
           Choose it: open-forge repair
  Warning  .agents/maps/_maps.md:32:3        Broken link
           The linked file was not found: nowhere/_nope.md
           No possible target was found. Fix the link by hand.

  21 links valid, 3 external links not checked, 20 routes checked.
Next: open-forge repair --dry-run  (preview the 1 repair that is safe to apply)
```

`full` adds Info rows, the code after each title, `why this was suggested`
under each possible target, `Read from: <source>` per finding, the lane
sentence (`can be fixed automatically`, `needs a choice`) after the message,
and the lane counts sentence: `1 can be fixed automatically, 2 need a choice,
1 must be fixed by hand.`

`debug` adds each category's boundary and coverage on stderr.

The hint line appears only when the level hides at least one finding. Because
Doctor text opts into minimal warnings, minimal text names `--detail full`
only when Info findings are hidden; standard likewise names `--detail full`
when only Info is hidden. An explicit severity filter controls its requested
rows and does not receive automatic warning inclusion.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-completed). [Matching reviewed capture](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Rendering/__snapshots__/Diagnosis/healthy.minimal/healthy.minimal.txt).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-completed-with-warnings). [Matching reviewed capture](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Rendering/__snapshots__/Diagnosis/warnings-only.minimal/warnings-only.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-incomplete). [Matching reviewed capture](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Rendering/__snapshots__/Diagnosis/incomplete.minimal/incomplete.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-invalid-input). [Matching reviewed capture](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Rendering/__snapshots__/Diagnosis/invalid-input.full/invalid-input.full.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-blocked). [Matching reviewed capture](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/Shared/Rendering/__snapshots__/Diagnosis/blocked-workspace.minimal/blocked-workspace.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#doctor-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | Findings                                  | `data`                                                                                            |
| -------- | ----------------------------------------- | ------------------------------------------------------------------------------------------------- |
| minimal  | errors                                    | `{}`                                                                                              |
| standard | + warnings; `category`, `resolution`      | `{ categories: [ { name, coverage, counts: { errors, warnings, infos }, limitations: [...] } ] }` |
| full     | + infos; candidates, evidence, provenance | + `lanes: { safeExact, guidedChoice, targetedOperation, manualDecision, blockedRepair }`          |

The JSON ladder remains unchanged: minimal JSON contains errors only,
standard adds warnings, and full adds infos and evidence. The minimal-warning
opt-in is text-only.

Counts at every level: `checks`, `checksComplete`, `errors`, `warnings`,
`infos`, `linksChecked`, `linksValid`, `externalLinksNotChecked`,
`imageLinks`, `routesChecked`, `frameworkFiles`, `extensionsInstalled`,
`librariesRegistered`.

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Next rules

One overall `Next:`, chosen in this order: any safe-exact finding ->
`open-forge repair --dry-run` (reason: preview the N repairs that are safe to
apply); any targeted-operation finding -> that command, first by category
order; any guided-choice finding -> `open-forge repair`; any
manual-decision error -> a sentence naming the first file; blocked ->
none; healthy -> none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

Severities are the contract's. Lanes decide the per-finding action phrase.
Family rows use the shared sentence with the subject filled in. Titles are the
current catalogue titles.

### Workspace

| Kind                                    | Severity | Lane            | Title                                 | Message                                                                                  | Action                                                                          |
| --------------------------------------- | -------- | --------------- | ------------------------------------- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| workspace.unavailable                   | error    | blocked-repair  | Workspace cannot be read              | `<path> does not exist or cannot be read.`                                               | none                                                                            |
| workspace.not-directory                 | error    | blocked-repair  | Workspace is not a directory          | `<path> is a file, not a directory.`                                                     | none                                                                            |
| workspace.agents-missing                | info     | informational   | The .agents folder is missing         | `<path> has no .agents folder. Open Forge is not installed here.`                        | `open-forge install --dry-run`                                                  |
| workspace.agents-inaccessible           | error    | blocked-repair  | The .agents folder cannot be read     | `.agents exists but cannot be read: <reason>.`                                           | none                                                                            |
| workspace.loader-missing                | error    | blocked-repair  | Loader is missing                     | `.agents/loader.md is missing.`                                                          | `open-forge update` when the record exists, else `open-forge install --dry-run` |
| workspace.loader-unreadable             | error    | blocked-repair  | Loader cannot be read                 | `.agents/loader.md cannot be read: <reason>.`                                            | none                                                                            |
| workspace.loader-malformed              | error    | blocked-repair  | Loader has invalid content            | `.agents/loader.md could not be understood: <reason>.`                                   | `open-forge update` (restore the shipped Loader)                                |
| workspace.entry-missing                 | warning  | manual-decision | Entrypoint is missing                 | `<folder> is routed but has no entrypoint file.`                                         | `open-forge route init <id>`                                                    |
| workspace.entry-ambiguous               | error    | blocked-repair  | Several entrypoints match             | `<folder> has more than one entrypoint file: <names>. Keep one.`                         | edit by hand                                                                    |
| workspace.entry-compatibility-collision | error    | blocked-repair  | Entrypoint names conflict             | `<folder> has both <_name.md> and <index.md>. Keep one.`                                 | edit by hand                                                                    |
| workspace.source-id-collision           | warning  | manual-decision | Source IDs conflict                   | `The ID <id> is derived by more than one file: <paths>. Use exact paths, or rename one.` | edit by hand                                                                    |
| workspace.path-invalid                  | error    | blocked-repair  | Path is invalid                       | `<path> is not a valid path for a <kind>.`                                               | edit by hand                                                                    |
| workspace.path-containment              | error    | blocked-repair  | Path is outside the workspace         | `<path> points outside the workspace.`                                                   | edit by hand                                                                    |
| workspace.physical-alias                | error    | blocked-repair  | Path identity is ambiguous            | `<path> and <other> resolve to the same file, so its identity is ambiguous.`             | edit by hand                                                                    |
| workspace.frontmatter-malformed         | warning  | manual-decision | Frontmatter is invalid                | `<what is wrong, from the parser, in plain words>` at `<path>:line:col`                  | edit by hand                                                                    |
| workspace.frontmatter-duplicate         | warning  | manual-decision | Frontmatter contains duplicate fields | `The key <key> appears more than once.`                                                  | edit by hand                                                                    |
| workspace.parse-incomplete              | info     | informational   | Source could not be read completely   | `<path> could not be parsed completely: <reason>. Its checks are incomplete.`            | none                                                                            |
| workspace.unsupported-source            | info     | informational   | Source type is unsupported            | `<path> is not a kind of file Open Forge checks.`                                        | none                                                                            |
| workspace.root-missing                  | warning  | manual-decision | Root route is missing                 | `The Loader lists <route> but <path> does not exist.`                                    | `open-forge route init <id>` or remove the entry                                |
| workspace.root-unreachable              | warning  | manual-decision | Root route cannot be reached          | `<route> is listed but cannot be reached from the Loader: <reason>.`                     | edit by hand                                                                    |
| workspace.detached                      | info     | informational   | Source is outside the loaded routes   | `<path> is not reachable from any route, so agents never load it.`                       | `open-forge index` when its parent is routed                                    |

### Recovery data

| Kind                            | Severity | Lane           | Title                                 | Message                                                                            | Action                         |
| ------------------------------- | -------- | -------------- | ------------------------------------- | ---------------------------------------------------------------------------------- | ------------------------------ |
| recovery.bundle-recognized      | info     | informational  | Recovery bundle is kept               | `A recovery bundle from an earlier command is kept at <path>.`                     | `open-forge cleanup`           |
| recovery.draft-recognized       | warning  | informational  | Incomplete recovery draft found       | `An unfinished recovery draft is at <path>. A command did not finish.`             | `open-forge cleanup --dry-run` |
| recovery.bundle-collision       | error    | blocked-repair | Recovery bundle is damaged            | `The recovery bundle at <path> is damaged: <reason>. It was left in place.`        | `open-forge cleanup --dry-run` |
| recovery.provenance-unavailable | error    | blocked-repair | Recovery origin could not be verified | `The recovery bundle at <path> cannot be verified, so cleanup will not delete it.` | none                           |

### Routes and Entries

| Kind                              | Severity | Lane               | Title                               | Message                                                                                             | Action                                             |
| --------------------------------- | -------- | ------------------ | ----------------------------------- | --------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| route.entrypoint-missing          | warning  | manual-decision    | Route entrypoint is missing         | `<folder> is routed but has no entrypoint file.`                                                    | `open-forge route init <id>`                       |
| route.entrypoint-duplicate        | error    | blocked-repair     | Route has several entrypoints       | `<folder> has more than one entrypoint: <names>.`                                                   | edit by hand                                       |
| route.escape                      | error    | blocked-repair     | Route leaves its allowed boundary   | `The entry <text> in <path>:l:c points outside <folder>.`                                           | edit by hand                                       |
| route.unreachable                 | warning  | manual-decision    | Route cannot be reached             | `<path> is routed but no parent lists it.`                                                          | `open-forge index`                                 |
| route.detached                    | warning  | manual-decision    | Source is outside the loaded routes | `<folder> looks like a route but no Loader entry or parent reaches it.`                             | edit by hand                                       |
| route.metadata-required-missing   | warning  | manual-decision    | Required route metadata is missing  | `<path> has no <description \| tags> in its frontmatter.`                                           | `open-forge route update <id> --description "..."` |
| route.title-invalid               | warning  | manual-decision    | Route title is invalid              | `<path> has no level-1 heading.`                                                                    | edit by hand                                       |
| route.axioms-invalid              | info     | manual-decision    | Route Axioms are invalid            | `<path> has no Axioms section, or its Axioms section is malformed.`                                 | edit by hand                                       |
| route.generated-region-stale      | warning  | targeted-operation | Entries section is stale            | `The Entries section of <path> does not match its routed files.`                                    | targeted Index advice below                        |
| route.generated-region-missing    | warning  | targeted-operation | Entries section is missing          | `<path> has no Entries section.`                                                                    | targeted Index advice below                        |
| route.generated-region-malformed  | error    | blocked-repair     | Entries section is malformed        | `The Entries section of <path> could not be read as a list.`                                        | edit by hand                                       |
| route.generated-region-misplaced  | warning  | manual-decision    | Entries section is not last         | `The Entries section of <path> is followed by another section.`                                     | edit by hand                                       |
| route.generated-region-duplicate  | error    | blocked-repair     | More than one Entries section       | `<path> has more than one Entries section.`                                                         | edit by hand                                       |
| route.generated-entry-missing     | warning  | targeted-operation | Entry is missing                    | `<path> does not list <child>.`                                                                     | targeted Index advice below                        |
| route.generated-entry-extra       | warning  | targeted-operation | Entry has no file                   | `<path> lists <child>, which does not exist.`                                                       | targeted Index advice below                        |
| route.generated-entry-order       | warning  | targeted-operation | Entries are out of order            | `The entries in <path> are not in the expected order.`                                              | targeted Index advice below                        |
| route.generated-entry-path        | warning  | targeted-operation | Entry path is wrong                 | `The entry for <child> in <path> points to <wrong path>.`                                           | targeted Index advice below                        |
| route.generated-entry-description | warning  | targeted-operation | Entry description is stale          | `The entry for <child> in <path> has an old description.`                                           | targeted Index advice below                        |
| route.generated-entry-tags        | warning  | targeted-operation | Entry tags are stale                | `The entry for <child> in <path> has old tags.`                                                     | targeted Index advice below                        |
| route.overwrite-orphan            | warning  | manual-decision    | Overwrite has no base file          | `<name>.overwrite.md has no <name>.md beside it.`                                                   | edit by hand                                       |
| route.overwrite-independent-index | warning  | targeted-operation | Overwrite is listed independently   | `<path> lists <name>.overwrite.md as its own entry. Overwrite files are read with their base file.` | targeted Index advice below                        |
| route.compatibility-conflict      | error    | blocked-repair     | Route names conflict                | `<folder> can be reached by two route names: <names>.`                                              | edit by hand                                       |

### Links

| Kind                               | Severity | Lane            | Title                                    | Message                                                                                            | Action                                                      |
| ---------------------------------- | -------- | --------------- | ---------------------------------------- | -------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| reference.target-missing           | warning  | guided-choice   | Broken link                              | `The linked file was not found: <destination>.` then candidates or `No possible target was found.` | `open-forge repair` when candidates exist; else fix by hand |
| reference.fragment-missing         | warning  | guided-choice   | Linked heading was not found             | `<file> has no heading <#fragment>.` then candidates                                               | `open-forge repair` or fix by hand                          |
| reference.fragment-unverified      | warning  | blocked-repair  | Linked heading could not be checked      | `<file> could not be parsed, so <#fragment> was not checked.`                                      | `open-forge doctor` after fixing the file                   |
| reference.destination-malformed    | warning  | manual-decision | Link destination is invalid              | `<destination> is not a link Open Forge can check.`                                                | fix by hand                                                 |
| reference.destination-absolute     | warning  | manual-decision | Absolute local link is unsupported       | `<destination> is an absolute path. Use a relative path.`                                          | fix by hand                                                 |
| reference.destination-query        | warning  | manual-decision | Local link query is unsupported          | `<destination> has a query string, which local links do not support.`                              | fix by hand                                                 |
| reference.destination-encoding     | error    | blocked-repair  | Link encoding is unsupported             | `<destination> uses an encoding that cannot be resolved safely.`                                   | fix by hand                                                 |
| reference.target-outside-workspace | error    | blocked-repair  | Link leaves the workspace                | `<destination> points outside the workspace.`                                                      | fix by hand                                                 |
| reference.target-physical-escape   | error    | blocked-repair  | Link resolves outside the workspace      | `<destination> resolves outside the workspace through a link.`                                     | fix by hand                                                 |
| reference.target-alias             | error    | blocked-repair  | Link target identity is ambiguous        | `<destination> resolves to more than one file.`                                                    | fix by hand                                                 |
| reference.target-unreadable        | warning  | blocked-repair  | Link target cannot be read               | `<file> exists but cannot be read: <reason>.`                                                      | none                                                        |
| reference.target-unsupported       | info     | informational   | Link target type is unsupported          | `<destination> is a kind of file Open Forge does not check.`                                       | none                                                        |
| reference.same-target-path         | info     | safe-exact      | Equivalent link path is available        | `<destination> works but is not the canonical spelling: <canonical>.`                              | `open-forge repair --automatic`                             |
| reference.same-target-case         | info     | safe-exact      | Equivalent link letter case is available | `<destination> differs from the file's name only by letter case: <canonical>.`                     | `open-forge repair --automatic`                             |
| reference.same-target-encoding     | info     | safe-exact      | Equivalent link encoding is available    | `<destination> uses a different encoding than the canonical <canonical>.`                          | `open-forge repair --automatic`                             |
| reference.same-target-fragment     | info     | safe-exact      | Equivalent heading link is available     | `<#fragment> matches the heading <#canonical> apart from spelling.`                                | `open-forge repair --automatic`                             |

Counts on this category: `linksChecked`, `linksValid`,
`externalLinksNotChecked`, `imageLinks`.

### Framework files

| Kind                                     | Severity | Lane               | Title                                        | Message                                                                                                | Action                             |
| ---------------------------------------- | -------- | ------------------ | -------------------------------------------- | ------------------------------------------------------------------------------------------------------ | ---------------------------------- |
| framework.install-absent                 | info     | informational      | Framework is not installed                   | `Open Forge is not installed in this workspace.`                                                       | `open-forge install --dry-run`     |
| framework.ownership-observation          | info     | informational      | No ownership record                          | family `ownership-observation` (`Framework files`)                                                     | none                               |
| framework.managed-missing                | warning  | targeted-operation | Framework file is missing                    | `<path> is missing. It was installed by the Framework.`                                                | `open-forge update`                |
| framework.managed-changed                | warning  | targeted-operation | Framework file changed                       | `<path> changed since it was installed.`                                                               | `open-forge update`                |
| framework.lifecycle-evidence-unavailable | warning  | blocked-repair     | Ownership record cannot be read              | `.agents/open-forge.lock.json could not be read: <reason>.`                                            | none                               |
| framework.bridge-boundary                | error    | blocked-repair     | Open Forge section in AGENTS.md needs review | `The Open Forge section in <AGENTS.md \                                                                | CLAUDE.md> is missing or changed.` |
| framework.root-region-boundary           | error    | blocked-repair     | Open Forge section boundary is unclear       | `The Open Forge section in <file> has no clear start or end.`                                          | edit by hand                       |
| framework.ownership-conflict             | warning  | manual-decision    | Framework file ownership conflicts           | `<path> is claimed by the Framework and by <other>.`                                                   | fix by hand                        |
| framework.partial-lifecycle              | error    | blocked-repair     | Framework update did not finish              | `Some Framework files are current and others are not, so an update did not finish.`                    | `open-forge update`                |
| framework.partial-recovery               | error    | blocked-repair     | Framework recovery is incomplete             | `The recovery bundle at <path> was partly applied: some files match the old content and some the new.` | `open-forge doctor --detail full`  |
| framework.distributed-payload-defect     | error    | manual-decision    | Bundled Framework is invalid                 | family `payload-invalid`                                                                               | reinstall the CLI                  |

### Extensions

| Kind                              | Severity | Lane               | Title                                        | Message                                                                                                           | Action                                      |
| --------------------------------- | -------- | ------------------ | -------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| extension.ownership-observation   | info     | informational      | No ownership record                          | family `ownership-observation` (`installed Extensions`)                                                           | none                                        |
| extension.manifest-missing        | warning  | manual-decision    | Extension manifest is missing                | `The package at <path> has no extension.json.`                                                                    | fix by hand                                 |
| extension.manifest-malformed      | error    | blocked-repair     | Extension manifest is invalid                | `<path>/extension.json could not be read: <reason>. Expected keys: id, name, description, version, dependencies.` | fix by hand                                 |
| extension.duplicate-id            | error    | blocked-repair     | Extension ID is duplicated                   | `Two packages in <source> have the ID <id>.`                                                                      | fix by hand                                 |
| extension.unknown-id              | warning  | manual-decision    | Extension ID is unknown                      | `The ownership record names <id>, which is not in the bundled Extensions or the recorded source.`                 | `open-forge extension list`                 |
| extension.version-invalid         | warning  | manual-decision    | Extension version is invalid                 | `<id> has an invalid version: <value>.`                                                                           | fix by hand                                 |
| extension.managed-missing         | warning  | targeted-operation | Extension file is missing                    | `<path> is missing. It was installed by <id>.`                                                                    | `open-forge extension update <id>`          |
| extension.managed-changed         | warning  | targeted-operation | Extension file changed                       | `<path> changed since it was installed by <id>.`                                                                  | `open-forge extension update <id>`          |
| extension.dependency-missing      | warning  | manual-decision    | Required Extension dependency is missing     | `<id> requires <dependency>, which is not installed.`                                                             | `open-forge extension install <dependency>` |
| extension.dependency-cycle        | error    | blocked-repair     | Extension dependencies form a cycle          | `<a> requires <b>, which requires <a>.`                                                                           | fix by hand                                 |
| extension.dependency-incompatible | warning  | manual-decision    | Extension dependency version is incompatible | `<id> requires <dependency> <range>, but <version> is installed.`                                                 | fix by hand                                 |
| extension.source-unavailable      | info     | informational      | Extension source cannot be read              | `The source of <id>, <path>, cannot be read, so its files were not compared.`                                     | none                                        |
| extension.catalogue-unavailable   | error    | blocked-repair     | Package folder cannot be read                | `The package folder <path> cannot be read.`                                                                       | none                                        |
| extension.partial-lifecycle       | error    | blocked-repair     | Extension update did not finish              | `Some files of <id> are current and others are not.`                                                              | `open-forge extension update <id>`          |
| extension.ownership-collision     | warning  | manual-decision    | Extension file ownership conflicts           | `<path> is claimed by <id> and by <other>.`                                                                       | fix by hand                                 |
| extension.bridge-registration     | warning  | manual-decision    | Extension entry is missing from its parent   | `<parent> does not list <path>, which <id> installed.`                                                            | `open-forge index`                          |

### Libraries (reported under Workspace)

| Kind                                | Severity | Lane            | Title                                     | Message                                                                    | Action                            |
| ----------------------------------- | -------- | --------------- | ----------------------------------------- | -------------------------------------------------------------------------- | --------------------------------- |
| library.ownership-observation       | info     | informational   | No ownership record                       | family `ownership-observation` (`Libraries`)                               | none                              |
| library.source-root-invalid         | error    | blocked-repair  | Library source folder is invalid          | `The source folder of <id>, <path>, is not a folder inside the workspace.` | `open-forge library inspect <id>` |
| library.source-root-aliased         | error    | blocked-repair  | Library source root has an ambiguous path | `The source folder of <id>, <path>, resolves to an ambiguous location.`    | none                              |
| library.inventory-incomplete        | warning  | informational   | Library source scan is incomplete         | `The source folder of <id> could not be scanned completely: <reason>.`     | `open-forge library inspect <id>` |
| library.projection-missing          | warning  | manual-decision | Registered Library link is missing        | `<path>, a link of <id>, is missing.`                                      | `open-forge library sync <id>`    |
| library.projection-dangling         | error    | blocked-repair  | Library link target is missing            | `<path> links to <target>, which does not exist.`                          | `open-forge library inspect <id>` |
| library.projection-retargeted       | error    | blocked-repair  | Library link target changed               | `<path> no longer links to <expected>; it links to <actual>.`              | `open-forge library inspect <id>` |
| library.path-collision              | warning  | manual-decision | Library destination is occupied           | `<path> is used by <id> and by <other>.`                                   | fix by hand                       |
| library.link-capability-unsupported | error    | blocked-repair  | Required Library links are unsupported    | `This system cannot create the file links the <id> Library needs.`         | none                              |
| library.extension-collision         | warning  | manual-decision | Library and Extension paths conflict      | `<path> is claimed by the <id> Library and the <id> Extension.`            | fix by hand                       |
| library.recovery-safe-exact         | info     | safe-exact      | Verified Library recovery is available    | `A verified recovery step for <id> can restore <path>.`                    | `open-forge repair --automatic`   |

## Scenarios

### Catalogue situations

Fixtures only: `healthy`, `info-only` (no ownership record),
`warnings-only` (two broken links, one with candidates), `error-and-warnings`
(malformed frontmatter plus links), `incomplete` (Extension source
unreadable), `blocked-workspace`, `invalid-input`, `changed-extension-file`,
`stale-entries`, `library-drift`, `recovery-bundle`. Each at all four levels,
text and JSON.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

Current native output uses `To list the warnings: open-forge doctor --detail standard` and `To list the info findings`; the catalogue's shorter hint is different. At standard, full and debug, the native renderer also doubles `Fix it by hand.` where the catalogue says the manual-action sentence is emitted once. The `error-and-warnings` situation currently has no error finding and reports `No errors. 3 warnings and 3 info findings were recorded.` These wording, rendering, and situation-definition questions remain unresolved. **Maintainer decision/clarification remains open.**

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
- Invoke Workspace Library operations, enumerate an unregistered source tree,
  adopt or create a projection, probe link capability, or delete or restore a
  recovery residual.
- Report a health score, percentage, or fabricated complete coverage.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact JSON schema and numeric exits. The accepted [CLI
Architecture](../../architecture.md) defines parser roles, filesystem alias and
physical-identity structure, hashing and concurrency constraints, resource
limits, and lifecycle mutation boundaries. Gate 5 must prove source-generated YamlDotNet and STJ serialization,
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
- Fresh Extension installation and successful Index generation do not produce
  Framework drift solely because the recorded generated fingerprint predates
  current Entries. Real authored drift, stale navigation, ambiguous Entries headings,
  missing targets and unavailable source evidence remain diagnosed.
- A fresh Framework installation with a trusted empty Extension section has
  complete coverage and no false error. Installed embedded packages retain their
  recorded source identity without being treated as filesystem paths.
- Ordinary Markdown leaves do not require generated regions; missing or malformed
  regions on the Loader and recognized entrypoints remain diagnosable.
- Independent severity and resolution, stable kinds, typed subjects, evidence,
  provenance, candidates, proposals, and typed next actions.
- Every workspace and entry kind, including Loader, entrypoint, compatibility,
  identity, path, metadata, parsing, root, and detached boundaries.
- The finite Workspace Library subcatalogue: safely absent records with zero
  Libraries and complete coverage without a finding; malformed or unavailable
  records,
  invalid or aliased source roots and `.agents` boundaries, incomplete source
  inventory, missing/dangling/retargeted projections, path collisions,
  unsupported proven link capability, Library/Extension collisions, and the
  safe-exact typed residual lane.
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
- Minimal, standard, JSON, and debug projections from one typed result, with no
  health score or percentage and no prompts.
- `completed`, `completed-with-warnings`, `incomplete`, `invalid-input`, `blocked`, `failed`, and
  `cancelled` meanings, including informational findings that do not create
  `completed-with-warnings`.
- Read-only, stateless, repeatable behavior with no plan, recovery-bundle,
  temporary, lifecycle, or public-command effect.
- The existing three public Doctor EndToEnd journeys remain unchanged; Library
  findings are covered within the existing six-domain result.

## Related Current Sources

- [Doctor Command Contract Set](_doctor.md)
- [Doctor Behavior Contract](behavior.md)
- [Repair Interface Contract](../repair/interface.md)
- [Repair Behavior Contract](../repair/behavior.md)
- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`doctor.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Doctor/DoctorText.cs).

<!-- @OpenForgeTextRef doctor.help.syntax -->

## Targeted generated-navigation advice

A missing generated entry names its expected destination, never the diagnostic
sentinel `absent`. The actual/expected evidence still records absence faithfully.
Generated-region, generated-entry and independent-overwrite findings identify
the catalogue containing the incorrect Entries and target that catalogue with
`open-forge index <path>`. They do not append a contradictory default Index
suggestion. This works for an explicitly selectable detached Skill catalogue
without making that catalogue part of default rooted traversal.

For a workspace-relative path using only ASCII letters, digits, slash, period,
underscore, hyphen and ordinary spaces, the advice is a copyable command. Quote
the entire argument with double quotes when it contains spaces. For a path
beginning with hyphen or containing another character, show this sentence:
`Run open-forge index with <path> as its source argument, using your shell's quoting rules.`
Keep the exact path. This fallback does not reject, rename or stop supporting it.

The command explanation is `Refresh the generated Entries in this catalogue.`
The report's Next action prefers a copyable command and otherwise retains the
explicit sentence instruction. Finding kinds, resolution lanes, status, exits,
JSON shape and read-only effects are unchanged. Other findings retain their
existing fallback behavior.
