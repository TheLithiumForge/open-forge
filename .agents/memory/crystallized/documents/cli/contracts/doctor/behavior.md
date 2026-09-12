---
open-forge:
  description: Current technology-neutral six-domain diagnosis, findings, coverage, and conformance for `doctor`
  responsibility: Define how Doctor resolves facts, preserves unsafe boundaries, forms one read-only result, and reports conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Doctor, Behavior, Diagnosis, Determinism, Safety, CurrentTruth]
---

# Doctor Behavior Contract

## Status And Boundary

This file is the accepted current Crystallized Behavior Contract for
`open-forge doctor`. It defines deterministic request resolution, six-domain
diagnosis, coverage, finding and candidate formation, result formation,
read-only safety, and conformance without choosing implementation technology.
The command does not ship yet.

The [Interface Contract](interface.md) owns the complete public grammar,
catalogue, observable projections, semantic result names, errors, examples, and
public verification. This Behavior Contract does not add flags, operands,
aliases, JSON field names, numeric exits, Library command syntax, parsers,
storage, or lifecycle mutation. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the exact shared
JSON result schema and exit mapping, while the accepted [CLI
Architecture](../../architecture.md) defines the implementation boundary. This behavior does not
duplicate those mechanics or claim their Gate 5 proof.

The operation consumes Framework and Markdown facts without replacing the
authoritative Framework sources that define their meaning. A link to a related
source preserves its authority boundary; it does not create a second diagnosis
rule.

## Operation Flow And Invariants

Doctor follows one complete read-only flow:

```text
validated command input
  -> exact workspace boundary
  -> fresh six-domain fact inspection, including the Workspace Library subcatalogue
  -> per-domain coverage, limitations, counts, findings, and actions
  -> aggregate semantic result
  -> one typed result
  -> human or structured rendering
```

The operation stops after forming the typed result. It never creates an empty
mutation plan or enters a mutation lifecycle. It never invokes `repair`, `index`,
a route operation, a cleanup operation, or a lifecycle operation.

For unchanged CLI payload, workspace bytes, and explicit input, Doctor resolves
the same workspace, runs the same domain checks, preserves the same evidence,
orders the same findings, and forms the same semantic result. It stores no
session, report, graph, proposal authority, cache, or historical baseline.

Doctor reads `.agents/open-forge.lifecycle.json`, schema v1, when lifecycle facts
are in scope. The common envelope and isolated `framework` and `extensions`
sections remain separate. The document stores no plan, runtime history, journal,
recovery evidence, or session. Files outside this exact path are ordinary
workspace content, not lifecycle input. The CLI distribution embeds Framework and
first-party Extension assets with deterministic inventory and hash proof; that
proof is distributed-source identity, not workspace or runtime implementation
evidence.

The exact `.agents/open-forge.libraries.json` record is a separate consumer-local
authority. Doctor validates its typed record, every registered source-root
inventory, registered projections, and typed residual evidence under the
`workspace and entry` domain. It never invokes a Library command, probes link
capability, mutates, adopts, or deletes recovery.

## Request Resolution

Request resolution applies the shared Global CLI Flags contract and the exact
Doctor syntax:

1. Resolve terminal `--help` or `--version` before workspace selection or domain
   inspection.
2. Reject every positional operand and every operation-specific flag because
   Doctor has none.
3. Reject unknown flags and malformed or conflicting shared values under the
   shared contract.
4. Resolve the exact current directory or exact `--workspace` value without
   parent, Git-root, package-root, marker, route, or file discovery.
5. Produce one complete diagnostic request. No request member supplies mutation
   authority.

Shared Boolean repetition is collapsed according to the shared contract.
Doctor-specific spellings such as `--automatic`, `--dry-run`, and `--relink`
are rejected at request resolution. A well-formed global presentation flag does
not change facts, selection, coverage, finding order, or semantic status.

`--help` and `--version` form terminal informational results without resolving a
workspace or running a domain. They remain mutually exclusive. A domain operand
or Doctor-specific flag combined with either terminal mode remains invalid.

## Workspace And Diagnostic Boundary

The workspace stage establishes the exact selected directory and its safe lexical
and physical boundary before it admits workspace facts. It does not substitute a
nearby directory, Git root, Loader, or package root.

If the selected value is missing, unavailable, or not a directory, the aggregate
result is `blocked`. The result still retains the fixed domain identities and
reports dependent domains as blocked when their boundaries cannot be established.
It never fabricates an empty workspace, installed Framework, or complete route
universe.

For a valid directory, the stage records whether `.agents`, the Loader, and
other required entry evidence are present and readable. A safely absent
installation is a fact. The Framework lifecycle domain may report an absent
installation informationally, while missing evidence needed for another domain
remains incomplete or blocked.

The admissible inspection universe is the declared workspace, its supported
Open Forge sources, recognized lifecycle and recovery evidence, and contained
local-reference targets. Ordinary links do not expand the workspace boundary.
An outside target is never admitted merely because it is named by a source.

For the local-reference domain, the resolver admits a supported Markdown source
and a local target only when both are lexically and physically contained by the
selected workspace and fall within the declared contained local-reference
boundary. The normal source universe is below `.agents`; supported Markdown
sources and contained targets outside `.agents` remain eligible only within that
same boundary. External URLs, absolute destinations, workspace escapes, physical
aliases, unreadable targets, and unsupported target kinds do not expand it.

## Fixed Domain Scheduling

The scheduler runs these domain stages in this fixed order:

1. Workspace and entry.
2. Recovery and residual state.
3. Routes, metadata, overwrites, and generated navigation.
4. Local references.
5. Framework lifecycle.
6. Extension lifecycle.

Every stage produces a domain report, even when a prerequisite is unavailable.
The scheduler passes safe established facts forward, but it does not use a
broader scan to hide a missing prerequisite. A dependent domain explicitly marks
its own coverage as `incomplete` or `blocked`, records the limitation, and
retains any safe findings it could establish.

The domain order is part of the typed result and human and structured rendering.
Within a domain, findings are ordered by stable kind, typed subject, and
canonical location using deterministic ordinal comparison. Discovery timing,
filesystem enumeration order, modification time, severity, recommendation, and
human message do not select the order or an effect.

## Coverage And Aggregate Status

Each domain stage records its declared boundary before checking it. The stage
forms `complete` coverage only after every check in that boundary ran or was
resolved as a safe absence. A complete domain can contain warnings, errors,
safe-exact proposals, guided candidates, or informational facts.

The stage forms `incomplete` coverage when safe facts remain but an applicable
part of the boundary cannot be inspected or trusted. It forms `blocked` coverage
when a required identity, containment, authority, or other safety boundary cannot
be established. It never converts an unavailable value into zero or an unknown
artifact into a known one.

Aggregate result formation preserves the public distinctions in the Interface:

- Invalid request input forms `invalid` before domain work.
- A required unsafe workspace or domain boundary forms `blocked`.
- A trustworthy but partial required inspection forms `incomplete` when no
  stronger blocked condition applies.
- An unexpected execution failure forms `failed`.
- Caller cancellation or interruption forms `interrupted` according to the
  accepted interruption boundary.
- When all six domains have complete coverage, actionable warning or error
  findings form `attention`.
- When all six domains have complete coverage and no actionable warning or error
  remains, the result is `complete`.

The Workspace Library subcatalogue contributes to the existing
`workspace and entry` coverage. A safely proven absent Library record means
zero Libraries and complete Library coverage with no Library finding; it grants
no ownership and infers no mapping. For a readable strict record, Doctor
attempts a complete eligible inventory for every named source root. Those
registered roots are the complete declared Library coverage. Complete safely
observed missing or changed registered projections form `attention`; an
unavailable or incomplete registered-root inventory emits
`library.inventory-incomplete` and forms `incomplete` coverage; malformed,
aliased, colliding, or otherwise unsafe identity forms `blocked`. Doctor never
treats a safe prefix as complete and never enumerates an unregistered source
root. It does not authorize adoption or mutation.

Informational findings alone do not form `attention`. Severity remains separate
from resolution, and neither severity nor resolution changes coverage.

## Finding And Proposal Formation

The finding stage forms one typed finding from current facts. Each finding keeps
its domain-qualified stable kind, independent severity, typed subject, observed
evidence, provenance, resolution lane, applicable candidates or exact proposal,
and typed next actions.

The stage does not use a message, display code, severity, list position, or
recommendation as a dispatch key. A proposal is data attached to current facts,
not an executable report or persistent authority. A candidate is evidence for a
user choice, not a default.

An exact proposal is admitted only when current facts prove one
meaning-preserving effect and its expected state, intended state, affected
boundary, verification condition, and recovery requirement are available. The
typed proposal retains each of those five facts, including the affected
boundary, without converting any of them into an executable effect. Doctor does
not apply it. A guided candidate is retained only when it comes from the finite
filename, title, literal-content, or route-neighborhood evidence named by the
Interface. Doctor does not perform semantic, fuzzy, synonym, relevance, or
network selection.

The resolution lanes are formed as follows:

- `safe-exact` records one proof-backed effect that a later Repair request may
  admit to its automatic catalogue.
- `guided-choice` records bounded candidates and leaves selection to a wizard or
  explicit user input.
- `targeted-operation` records a typed action whose accepted command owns the
  known intent. Doctor reports it and never calls it.
- `manual-decision` records an authored, ownership, lifecycle, or other human
  decision boundary.
- `blocked-repair` records unsafe, ambiguous, stale, unsupported, or
  unrecoverable evidence.
- `informational` records useful evidence without an action.

## Domain 1: Workspace And Entry

The first stage establishes the selected workspace, `.agents` boundary, Loader,
recognized entrypoints, source identity, parse coverage, root reachability, and
detached facts. It derives source IDs from current paths for evidence only. It
does not make an ID authoritative, routed, managed, or safe to mutate.

### Workspace Library subcatalogue

Within the same workspace-and-entry stage, the operation reads the exact
consumer-owned `.agents/open-forge.libraries.json` record, schema v1, and keeps
it separate from Framework and Extension lifecycle authority. A safely proven
absent record is valid zero-Library evidence: Library coverage is complete,
Doctor emits no Library finding, and no ownership or mapping is inferred. For a
present strict record, it validates the typed record discriminator, stable IDs,
normalized workspace-relative `sourceRoot` values, and exact `paths` mappings.
An unreadable or otherwise unavailable present record emits the existing
`library.record-unavailable` finding and makes Library coverage incomplete; it
is not treated as an empty record.

For each readable strict record, establish canonical lexical and physical
containment and an ordinary source root with safe real ancestry,
then attempt a complete eligible inventory for every named source root. Those
registered source roots are the complete declared Library coverage. If any
inventory or required source fact is unavailable, Doctor emits
`library.inventory-incomplete`, marks Library coverage incomplete, and never
silently narrows the inventory, treats a safe prefix as complete, or infers
source additions or retirements. Doctor never enumerates an unregistered source
root.

For each registered mapping, observe the destination entry with no-follow
logical parent/leaf/raw-target identity. Report current exact relative-file
links, missing destinations, dangling expected links, retargeted links, path or
ownership collisions, and unavailable or unsafe facts. Unsupported link
capability is reported only from an already proven typed fact; Doctor does not
probe or create a link. Destination-derived automatic source IDs remain
separate from the Library management ID. External destinations retain explicit
paths and null automatic source IDs under the existing source-reference grammar.

`library.recovery-safe-exact` is emitted only when a semantically verified
current-v1 residual carries trusted Library attribution for the selected
workspace and an exact typed ordinary-record, ordinary-file, or
relative-file-link effect with safe no-follow identity and no third state.
Doctor reports this safe-exact evidence for Repair selection; it never applies
the effect, restores or deletes a target, follows a source target, or infers
intent from a filename or path. Library/Extension collision remains a separate
blocked/manual ownership fact.

The stage realizes the complete workspace catalogue:

- `workspace.unavailable` and `workspace.not-directory` preserve a blocked
  selected-workspace boundary.
- `workspace.agents-missing` records safe absence or the dependent incomplete or
  blocked boundary. It does not create `.agents`.
- `workspace.agents-inaccessible` preserves the blocked `.agents` boundary.
- `workspace.loader-missing`, `workspace.loader-unreadable`, and
  `workspace.loader-malformed` preserve the exact Loader failure and do not
  adopt another Loader.
- `workspace.entry-missing`, `workspace.entry-ambiguous`, and
  `workspace.entry-compatibility-collision` preserve missing or colliding
  recognized entrypoint evidence. No filename wins by compatibility preference.
- `workspace.source-id-collision` retains every current candidate path. It does
  not choose by path order, file kind, depth, generated order, or likely intent.
- `workspace.path-invalid`, `workspace.path-containment`, and
  `workspace.physical-alias` block or limit the affected identity rather than
  admitting an unsafe path or alias.
- `workspace.frontmatter-malformed` and `workspace.frontmatter-duplicate`
  preserve authored metadata ambiguity and do not invent a value.
- `workspace.parse-incomplete` retains safe readable facts and marks the
  affected coverage incomplete. It does not make unsupported parsing look
  complete.
- `workspace.unsupported-source` records an excluded source kind without
  reinterpreting it as a supported source.
- `workspace.root-missing` and `workspace.root-unreachable` preserve the exact
  root boundary and prevent a fabricated root or route closure.
- `workspace.detached` reports an inspectable detached source or tree without
  adopting it into the Framework.

The workspace stage distinguishes a valid absent installation from an inaccessible
or malformed required source. It does not make lifecycle absence a route or
entry repair proposal.

## Domain 2: Recovery And Residual State

The recovery stage resolves only the current user's external
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1` root. This
observer-only lookup never creates the OS application-data root or the Open Forge
subtree. An absent root or store produces zero recovery items. An unreadable
selected workspace bucket produces unavailable or incomplete recovery coverage;
it is never absence. The stage does not recursively inspect the workspace or
arbitrary private artifacts.

The stage enumerates exact deterministic final and draft names directly under
the selected workspace bucket. It performs at most one semantic integrity check
per exact named final ZIP: source-generated current schema-v1 manifest decoding,
required immutable typed attribution validation, exact ordered entry names and
counts, declared lengths and hashes, and exact payload bytes. A valid current-v1
final is `Verified`; a schema-1 final missing or carrying invalid attribution is
`Malformed`/unattributed, an unknown schema version is `Unsupported`, and an
unreadable final is `Unavailable`. Finals without valid attribution remain
preserved and are never migrated, rewritten, repaired, deleted, adopted, or
inferred. An exact named draft is exact-name, path-only `Incomplete` and never
preparation; Doctor does not inspect or use draft bytes for attribution. Payload
validation uses fixed bounded buffers and never extracts, discloses, renders,
logs, returns, retains, or materializes payload bytes.

The exact schema-v1 attribution vocabulary, valid producer/operation/subject
combinations, and required non-null workspace identity are defined by the
[Mutation And Recovery Technical Design](../../technical-designs/mutation-and-recovery.md#schema-v1-attribution-vocabulary).
Doctor accepts no unknown value or fallback attribution.

The stage reports the exact path, kind, integrity condition, and separate Cleanup
guidance. It does not acquire `WorkspaceLockLease`, inspect live targets,
classify target state, infer activity, or auto-bind another workspace bucket.
The recovery catalogue contains only `recovery.bundle-recognized`,
`recovery.draft-recognized`, `recovery.bundle-collision`, and
`recovery.provenance-unavailable` under the Interface definitions.

Recovery attribution, when available, remains a neutral producer fact beside
the recovery models. Lifecycle diagnosis consumes it only after verifying the
current-v1 final's immutable typed attribution to a Framework producer,
operation, and typed subject `{kind, identity}` for the selected workspace, plus
the exact neutral recovery-state comparison defined below. A generic
mixed-current-state observation alone is insufficient. It is not a Doctor enum,
a presentation dependency, or a generic bag.

Recovery findings remain diagnosis, manual, blocked, or typed actions for the
separate [cleanup operation](../cleanup/interface.md). The general Repair
catalogue contains no recovery-bundle deletion or restoration, and Doctor never runs
cleanup.

## Domain 3: Routes, Metadata, Overwrites, And Generated Navigation

The route stage derives current authored topology and metadata, resolves base and
overwrite relationships as one logical source, and compares the expected
generated navigation with the current bounded generated region. Authored
filesystem and metadata facts are authoritative for the comparison. Current
generated lines do not define topology or provide metadata fallback.

The stage realizes the route catalogue:

- `route.entrypoint-missing` and `route.entrypoint-duplicate` preserve
  route-shape gaps without authoring a route or child.
- `route.escape`, `route.unreachable`, and `route.detached`
  block or limit unsafe topology and never invent a parent, root, or route
  membership.
- `route.metadata-required-missing`, `route.title-invalid`, and
  `route.axioms-invalid` retain authored metadata and rule failures without
  inventing descriptions, titles, tags, or active rules.
- `route.generated-region-stale` compares a valid region with its current
  expected projection and forms a `targeted-operation` action for `index`.
- `route.generated-region-missing`, `route.generated-region-malformed`,
  `route.generated-region-misplaced`, and `route.generated-region-duplicate`
  preserve the generated ownership boundary. Missing or malformed markers are
  not general Repair proposals.
- `route.generated-entry-missing`, `route.generated-entry-extra`,
  `route.generated-entry-order`, `route.generated-entry-path`,
  `route.generated-entry-description`, and `route.generated-entry-tags`
  compare each generated entry's presence, membership, ordering, destination,
  description, and tags. They form a typed `index` action only when the route
  boundary and authored metadata are valid.
- `route.overwrite-orphan` and `route.overwrite-independent-index` preserve the
  base and overwrite identity boundary. The overwrite is not treated as an
  independent source or generated child.
- `route.compatibility-conflict` retains every conflicting route form and does
  not choose a canonical winner.

Generated-region comparison applies to the Loader and recognized entrypoints.
Ordinary Markdown leaves supply child metadata and reference facts; they do not
require generated regions. Unavailable or malformed eligible hosts still retain
their diagnostic boundary.

Generated drift belongs to `index`. Route authoring, topology, metadata, and
overwrite intent belong to accepted route operations or manual decisions. Doctor
never invokes the targeted operation and Repair never absorbs these findings.

## Domain 4: Local References

The local-reference stage inspects authored local reference occurrences within
the accepted contained workspace boundary. It records source path and location,
raw destination, target path or identity when available, fragment, target kind,
physical containment, parsing status, and provenance. It does not fetch external
URLs, follow a target into another scan, or turn a link edge into route or
authority meaning.

The stage realizes the complete reference catalogue:

- `reference.target-valid` records a complete local resolution as information.
- `reference.target-missing` retains the raw occurrence and forms guided
  evidence only when bounded candidates exist. It never creates a target.
- `reference.fragment-missing` distinguishes a missing fragment from a missing
  target. `reference.fragment-unverified` preserves incomplete target parsing or
  coverage instead of guessing.
- `reference.destination-malformed`, `reference.destination-absolute`, and
  `reference.destination-query` preserve authored forms outside the automatic
  local repair grammar. No component is discarded to make a repair fit.
- `reference.destination-encoding` admits a `safe-exact` proposal only when the
  same target and exact canonical authored bytes are proven.
- `reference.target-outside-workspace`, `reference.target-physical-escape`,
  and `reference.target-alias` block unsafe or ambiguous target identity.
- `reference.target-unreadable` and `reference.target-unsupported` preserve the
  affected incomplete or excluded boundary without pretending it was checked.
- `reference.image` records an image or supported non-navigation local resource
  occurrence as information when its bounded facts are available. It does not
  infer a semantic image repair.
- `reference.external-unchecked` records external HTTP or HTTPS facts without a
  network attempt. No-fetch alone does not make complete local coverage
  incomplete or attentive.
- `reference.cycle` and `reference.repeat` record bounded cycle or repetition
  evidence as information. They do not create a traversal or repair choice.
- `reference.same-target-path`, `reference.same-target-case`, and
  `reference.same-target-encoding` form safe-exact proposals only when the
  authored destination and canonical target are proven to be the same target.
- `reference.same-target-fragment` forms a safe-exact proposal only when one
  canonical fragment correction is proven.
- `reference.candidate-filename`, `reference.candidate-title`,
  `reference.candidate-literal-content`, and
  `reference.candidate-route-neighborhood` add explicit evidence to a guided
  candidate. They never rank a candidate into authority or silently collapse
  several evidence bases into semantic certainty.
- `reference.candidates-none`, `reference.candidates-one`, and
  `reference.candidates-several` record the finite candidate cardinality. One
  candidate remains a user choice, and several candidates remain all visible;
  neither state creates an automatic selection.

Candidate collection uses only the named filename, authored title, literal
content, and route-neighborhood evidence. It does not use fuzzy, semantic,
synonym, relevance, vector, or network matching. The same contained local
reference boundary is the only boundary available to an explicit Repair relink.

Each candidate basis is an exact producer-observed fact over the bounded
contained source universe. Doctor retains every applicable basis for each
candidate. It does not rank bases or candidates, select a winner, fall back to
another basis, or infer a basis through semantic, fuzzy, synonym, network, or
broad text search. The bases are defined conservatively:

- `filename` is an exact canonical leaf filename.
- `title` is an exact authored link label or title matched to the parsed primary
  title.
- `literal-content` is an exact bounded link-label or title occurrence supplied
  by retained parsed facts; it is not a broad body search.
- `route-neighborhood` is an exact established route parent, child, or sibling
  relation.

When current typed facts cannot establish a basis, the basis is absent and the
stage does not infer it.

## Domain 5: Framework Lifecycle

The Framework lifecycle stage diagnoses the isolated `framework` section of
`.agents/open-forge.lifecycle.json`, schema v1, without taking lifecycle
authority. It distinguishes
safe absence, trusted management, untrusted evidence, incomplete coverage, and
blocked ambiguity. It compares managed paths only when the Framework section is
trusted and retains bridge, root-region, ownership, generated-navigation,
cross-section, and distributed-payload boundaries.

The section is `absent` only after complete inspection proves no expected managed
state, managed boundary, or recovery residual. It is `trusted` only when exact
workspace and target identities, schema v1 and `open-forge-markdown-v1`,
internal consistency, and complete verifiable coverage hold. A path, matching
bytes, matching fingerprint, or force flag never promotes it. If the Extension
section or common envelope is malformed, the Framework stage retains any safe
independent facts but reports the preservation or coverage limitation.

For a trusted Framework section with available source evidence, a valid derived
Entries target is current when the safely observed generated region matches the
current authored topology at that exact path. A different recorded generated
fingerprint alone is not drift after Extension installation or indexing. This
read-only comparison preserves the recorded fingerprint and does not rebaseline
lifecycle state or authorize mutation. Authored targets, stale or malformed
navigation, missing targets and unavailable or blocked evidence retain their
separate checks.

The mixed managed-set observation uses these same currentness facts.

The stage realizes every Framework kind:

- `framework.install-absent` records safely established absence without creating
  an install effect in Doctor or general Repair; it may identify
  `open-forge install` as a typed next action.
- `framework.install-incomplete` preserves partial installation evidence and
  marks coverage incomplete or blocked as required.
- `framework.managed-missing` and `framework.managed-changed` compare current
  managed paths and semantic fingerprints with trusted lifecycle evidence but
  never replace user content; they may identify `open-forge update` as a typed
  next action.
- `framework.lifecycle-evidence-unavailable` and
  `framework.lifecycle-evidence-malformed` prevent inferred installation,
  ownership, or managed-file counts.
- `framework.lifecycle-untrusted` and `framework.lifecycle-section-missing`
  retain readable but untrusted or missing-section facts and never treat them as
  an empty trusted baseline.
- `framework.bridge-boundary` and `framework.root-region-boundary` preserve
  unsafe bridge or root-region boundaries without selecting a replacement.
- `framework.ownership-conflict` keeps managed, user, and Extension claims
  separate and requires a manual decision.
- `framework.partial-lifecycle` is formed only from a finite mixed-current-state
  observation within one exact trusted declared managed subject or set. At least
  one expected member must be current and at least one other expected member
  must be non-current. The finding reports observed mixed state only. It never
  reports operation history, transition intent, or recovery attribution. The
  more specific `framework.managed-missing` and `framework.managed-changed`
  findings remain alongside it whenever their facts apply.
- `framework.partial-recovery` is formed only for one semantically verified
  current-v1 final whose immutable typed attribution identifies the finite
  Framework producer, its exact operation, and a `workspace` subject whose
  identity matches the selected workspace key. A neutral producer compares the
  current ordinary target state for every ordered existing-target entry
  (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) with its recorded exact
  prior and intended states. A prior match means a safely observable ordinary
  file contained by the workspace has the exact recorded prior length and
  lowercase SHA-256. An intended match means the same exact ordinary-file
  comparison against the recorded intended state, or safely proven absence when
  the intended state is absence. Absence is not unavailable. The finding is
  formed only when at least one entry matches prior, at least one other matches
  intended, every compared entry is safely observable, and no compared entry is
  third or unknown. Each verified Framework-attributed final is evaluated
  independently in deterministic catalogue order; entries from separate bundles
  are never ranked, selected as a winner, or combined. All-intended is a
  no-finding state compatible with a completed historical operation; all-prior is
  a no-partial-finding state compatible with an unapplied or fully restored
  operation. Unavailable, unsafe, non-ordinary, third, unknown, or mismatched
  state produces incomplete or blocked coverage or another exact finding, never
  partial recovery. The finding describes mixed current state relative to recovery
  evidence and never claims that recovery occurred. Doctor never guesses
  attribution or state from a GUID, path, filename, command text, ordered entry,
  or untrusted bytes. The producer may expose only finite comparison states or
  bounded evidence; no payload bytes enter Doctor output. The lifecycle document
  itself stores no recovery evidence or operation history.
- `framework.distributed-payload-defect` reports a defect in the distributed
  payload as diagnosis or a future distribution action. It does not mutate the
  installed workspace or payload.

The accepted Framework lifecycle contracts own mutation. Doctor may report
`open-forge install` or `open-forge update` as typed next actions, but it never
invokes them, creates their authority, or mutates Framework state.

## Domain 6: Extension Lifecycle

The Extension lifecycle stage diagnoses the isolated `extensions` section of
`.agents/open-forge.lifecycle.json`, schema v1, plus manifest, dependency,
source, ownership, bridge, and registration evidence without installing,
updating, removing, creating, adopting, or registering an Extension.

A complete present empty Extension section is trusted and requires no warning.
The recorded source identity `embedded catalogue` refers to the embedded package
catalogue; recorded explicit filesystem paths retain their exact source and
safety checks. Reading a complete exact source does not require a separate
workspace manifest scan or an unconditional coverage limitation. Actual source
and bridge unavailability remains incomplete or blocked.

The stage preserves installed IDs, ownership, and recorded paths when package
source bytes are unavailable. It marks source-dependent comparison incomplete and
never substitutes another source. A trusted current section requires exact
workspace/package/path/owner/dependency identities, the `open-forge-markdown-v1`
semantic baseline policy, reciprocal facts, and complete verifiable coverage. An
absent document or section is not reconstructed from paths, bytes, or manifests.

The unreleased schema-v1 catalogue contains exactly 120 kinds: 33
workspace-and-entry (including the Workspace Library subcatalogue), 4 recovery,
22 route, 28 local-reference, 14 Framework, and 19 Extension kinds. Doctor
retains six result domains.
The accepted Task 16 implementation, as extended by Task 17, realizes
producer-backed findings for the complete catalogue. Task 17 closed the
accepted set-valued bridge-registration observation by extending the typed
contributor views and Doctor. The current Extension domain has no remaining
observation horizon, and the final pre-release completeness gate has an honest
emission path for all 120 kinds.

- `extension.lifecycle-document-missing` distinguishes a missing document from a
  safely established absence; the missing document alone does not prove an empty
  installed set.
- `extension.lifecycle-document-invalid`, `extension.manifest-missing`, and
  `extension.manifest-malformed` preserve unsupported, malformed, or untrusted
  lifecycle evidence and do not fabricate an Extension record.
- `extension.lifecycle-untrusted` and `extension.lifecycle-section-missing`
  preserve the section's explicit state and prevent trust promotion.
- `extension.duplicate-id`, `extension.unknown-id`, and
  `extension.version-invalid` retain identity and version ambiguity without
  choosing a record or version.
- `extension.managed-missing` and `extension.managed-changed` report trusted
  managed-file differences without replacement authority.
- `extension.dependency-missing`, `extension.dependency-cycle`, and
  `extension.dependency-incompatible` preserve dependency failures without
  selecting a package, order, or version.
- `extension.source-unavailable` and `extension.catalogue-unavailable` mark
  source-dependent coverage incomplete rather than substituting a source; safe
  installed facts remain visible.
- `extension.partial-lifecycle` is formed only from a finite mixed-current-state
  observation within one exact trusted declared managed subject or set. At least
  one expected member must be current and at least one other expected member
  must be non-current. The finding reports observed mixed state only. It never
  reports operation history, transition intent, or recovery attribution. The
  more specific `extension.managed-missing` and `extension.managed-changed`
  findings remain whenever their facts apply. Recovery evidence remains outside
  the lifecycle document.
- `extension.ownership-collision` keeps user, Framework, and Extension claims
  separate and requires a manual decision.
- `extension.bridge-registration` is a set-valued producer horizon. It forms one
  typed observation for each exact lifecycle-owned routed Extension payload
  target whose exact reviewed source facts form exactly one ordinary
  generated-navigation parent `Entries` registration. Lifecycle supplies the
  target identity and owners; exact reviewed source bytes and metadata establish
  the routed role; neutral generated-navigation formation and projection supply
  the exact parent host and expected entry; and generated-entry comparison
  supplies the current, missing, unreadable, or inconsistent observation. The
  singular target wording applies per observation. Content is inspected only
  when readable. Source-unavailable coverage is `incomplete`, and ambiguous
  mapping is `blocked`, without inference.
  Task 16 emits `extension.bridge-registration` from the producer-owned facts
  accepted by Task 17. No Extension finding is synthesized without its required
  producer-owned facts, and the final pre-release completeness gate requires an
  honest emission path for all 120 kinds.

Neither legacy `open-forge.extensions.json`, package-source manifests, broad
`.agents` recursion, payload/path/byte resemblance, nor Framework bridges may
substitute for producer facts. Static CLI composition is wiring only and cannot
manufacture observations; dependency injection and a runtime registry are not
observation substitutes.

The accepted bridge-registration observation changes no Extension manifest or
lifecycle field or schema. It adds no compatibility path, provider bridge,
symbolic link, registry, dependency injection, fuzzy path, or content inference.

The accepted Extension lifecycle contracts own exact mutation syntax and effects.
Doctor may report their typed next actions, but does not invoke them or grant
their authority. Repair does not mutate this domain.

## Result Formation And Presentation

After all six stages finish or record their boundaries, Doctor forms one typed
result containing the workspace and selection method, explicit read-only mode,
ordered domain reports, lifecycle section trust and source-availability states,
coverage and limitations, counts, findings, candidates and proposals, typed next
actions, and aggregate semantic status. Human and JSON
renderers consume that result without rerunning diagnosis.

The existing `workspace-entry` report carries the finite Workspace Library
subcatalogue: typed record/source-root/complete registered-root inventory and
registered-link facts, Library subjects, destination-derived source IDs kept
separate from Library IDs, and any `library.recovery-safe-exact` evidence. The
six-domain result shape and the existing three public Doctor EndToEnd journeys
remain unchanged.

The typed diagnosis is intrinsically read-only: `ReadOnly` is always `true` and
`ChangesMade` is always `false`; callers cannot supply alternative values. The
JSON projection emits those same required boolean values.

Aggregate and per-domain resolution and severity counts retain one typed
availability coordinate for every lane. The coordinate uses the shared
`OperationalValueState` values `available`, `unavailable`, and
`not-applicable`, with a nonnegative value only for `available`; unavailable or
not-applicable counts remain `null` rather than being converted to zero.

The JSON projection uses the exact command-local graph in the Interface Contract.
It preserves the six-domain array, domain and finding order, independent
resolution and severity counts, typed subject, evidence and provenance members,
candidate or proposal nullability, and typed action arrays. It does not duplicate
the shared envelope coordinates inside `result`. Every command-local object and
array is present; nullable members remain present and are `null` only for an
inapplicable or unavailable typed fact.

Compact rendering is a projection only. It retains the identity, status,
coverage, resolution lane, typed subject, candidate count when applicable, and
next action for each finding. Expanded rendering adds evidence, provenance,
locations, and candidate basis. `--verbose` adds bounded diagnostics as a
separate dimension. `--json` emits the complete typed result and never prompts.

No renderer creates a health score, percentage, recommendation authority, or
new finding. `--view` cannot change domain selection, coverage, finding order,
or status.

Primary human `complete`, `attention`, and `incomplete` results are kept
together on stdout. Primary human `invalid`, `blocked`, `failed`, and
`interrupted` results are kept together on stderr. JSON emits one complete result
to stdout for every semantic status; separate bounded diagnostics use stderr.

## Read-Only Safety

Doctor performs no persistent effect. It does not write authored files,
generated navigation, lifecycle sections, lifecycle records, recovery bundles,
temporary files, reports, sessions, or caches. It does not remove or restore
recovery bundles or drafts, invoke a public command, or acquire mutation authority
through an interactive or structured renderer.

Bounded payload streaming serves only semantic length/hash validation. Doctor never
extracts, discloses, retains, or materializes payload bytes and never turns the
persistent external lock file into activity evidence.

Library checks are equally read-only and no-follow. They never invoke a Library
or Extension operation, create or remove a projection, read source-target bytes,
enumerate an unregistered source root, adopt an unregistered link, probe link
capability, or delete or restore a Library residual.

All facts and proposals are derived per invocation. A repeated unchanged
invocation returns the same semantic result and does not create a synthetic
no-op. An incomplete or blocked fact remains visible and cannot be replaced by a
broader scan, a fallback target, a severity rule, or a recommendation.

## Behavioral Conformance

The mandatory public evidence boundary is the [Doctor Public Verification](interface.md#public-verification)
section. A conforming implementation must additionally prove:

- One request resolver for exact workspace selection, shared global flags,
  terminal modes, and rejection of all Doctor-specific mutation or selection
  inputs.
- Fresh six-domain diagnosis in the fixed order, with dependent domains retained
  as explicit incomplete or blocked reports.
- Deterministic domain, kind, subject, and location ordering independent of
  filesystem enumeration and messages.
- Independent coverage, severity, resolution, candidate, proposal, provenance,
  and next-action facts.
- Workspace identity, Loader, entrypoint, path, source-ID, metadata, parsing,
  root, and detached boundaries from the complete catalogue.
- Verified final bundles, incomplete drafts, final-name collisions, and
  unavailable provenance without Cleanup effects, live-target inspection, or
  activity inference.
- Representative payload entries with exact declared lengths
  and hashes, bounded buffers and memory independent of entry size, and no
  extraction, disclosure, retention, or materialization.
- Authored route topology, generated navigation, metadata, overwrite, and
  compatibility evidence without route or index mutation.
- Local-reference target, fragment, path, encoding, containment, alias, image,
  external, cycle, repeat, exact same-target, candidate-basis, and candidate
  cardinality behavior without network or fuzzy selection.
- Framework and Extension lifecycle evidence without general repair authority.
- Workspace Library record, every readable registered source-root inventory,
  registered-projection, collision, and typed safe-exact residual evidence
  within the existing workspace-and-entry domain, with no unregistered-source
  enumeration, Library invocation, capability probe, adoption, or mutation.
- Human compact and expanded, JSON, and verbose projections from one typed
  result, including the accepted stdout and stderr policy.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  formation, with informational facts not producing attention by themselves.
- Repeatability, no prompt behavior, no persistent state, no plan, and no public
  command invocation.
- Preservation of the existing three public Doctor EndToEnd journeys and six
  result domains while adding the finite Library subcatalogue.

Direct tests should prove request, boundary, domain, finding, ordering, and
semantic-result behavior. Focused integration tests should use real temporary
workspaces with malformed, ambiguous, detached, recovery, link, lifecycle, and
ownership evidence. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the structured
schema and numeric exits. The accepted [CLI
Architecture](../../architecture.md) defines the parser, filesystem abstraction,
hashing constraints, and AOT process boundary. Gate 5 must prove source-generated YamlDotNet and STJ
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys.

## Related Current Sources

- [Doctor Interface Contract](interface.md)
- [Doctor Command Contract Set](_doctor.md)
- [Repair Behavior Contract](../repair/behavior.md)
- [Repair Interface Contract](../repair/interface.md)
- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
