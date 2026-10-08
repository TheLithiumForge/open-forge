---
open-forge:
  description: Current install rules for setup selection, additive restoration, bounded frontmatter conversion, management establishment and recovery
  responsibility: Define how install resolves selections and Framework facts, plans one safe establishment or configuration, and forms its result
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Install, Framework, Behavior, Determinism, Lifecycle, Safety, Recovery, CurrentTruth]
---

# Install Behavior Contract

## Ownership Receipt Formation

Form whole-file and region receipts according to what the operation manages.
The `open-forge` blocks in root `AGENTS.md` and `CLAUDE.md` produce region
receipts even when creating a previously missing host requires a physical file
creation. Generated Entries produce `entries` region receipts. Preserve existing
verified ownership when a planned no-op leaves its bytes unchanged. Publish the
ownership lock after the operation's target effects verify; a region-only edit
never establishes whole-file ownership of its authored host. Skip an unavailable
lock write without blocking ordinary Install. Configuration verifies its
route-sharing registration before applying Gitignore.

The named installed content-file population is separate from the generated
`.agents/open-forge.lock.json` ownership control file and from the managed
host regions. `filesCreated` counts only the named installed content files.
`directoriesCreated` counts only directories strictly below `.agents`;
creating the `.agents` container is still the first ordinary directory effect
when needed, but it is excluded from that count.

## Status And Boundary

This is the accepted current Crystallized Behavior Contract for the root
`open-forge install` operation. The CLI is available as a public beta. [CLI Distribution](../../distribution.md)
records qualified platforms and published versions. [CLI Development](../../../../../working/cli-development/_cli-development.md)
records current implementation and release work. It defines technology-neutral request
resolution, exact workspace and payload facts, management classification,
semantic identity, intended state, generated projection, complete planning,
preflight, dry-run and application, verification, lifecycle publication,
recovery, result formation, and conformance.

The [Interface Contract](interface.md) defines public syntax, states, output,
status vocabulary, errors, examples, and non-goals. Shared Global Flags and the
Framework, routing, maintenance, and Index contracts define their respective
meanings. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the shared JSON
result schema and exit mapping. The [Ownership And Source Alignment Technical
Design](../../technical-designs/lifecycle-provenance.md) defines ownership serialization and current source alignment, while the [CLI Architecture](../../architecture.md)
defines cross-cutting implementation structure. This file does not duplicate
those mechanics or claim their Gate 5 proof.

## Operation Flow And Invariants

Install follows one complete typed flow:

```text
validated command input
  -> exact workspace and embedded Framework source
  -> recognized footprint and ownership receipts
  -> current/intended comparison
  -> authoritative generated-navigation projection
  -> complete ordered establishment plan
  -> preflight
  -> dry-run or application
  -> expected-state revalidation
  -> per-effect and whole-operation verification
  -> lifecycle publication and recovery-disposition reporting
  -> one typed result
  -> human or structured rendering
```

No effect begins until the complete footprint, current facts, management state,
intended state, generated projection, expected bytes, recovery-bundle identity
and preparation readiness, verification conditions, and preservation conditions
are known. One
blocked, incomplete, ambiguous, or unsafe selected effect blocks the whole
plan. Install never applies a safe subset around a blocked target.

For unchanged payload, workspace bytes, explicit input, and relevant external
facts, the operation resolves the same classification, plan, status, and result.
Filesystem enumeration order, matching bytes, route names, tags, provider
resemblance, and current generated lines are never hidden selection inputs.

## Request Resolution

The resolver:

1. Parses direct root `install` and rejects operands, a Framework group, root
   aliases, `--prune`, replacement/reinstall forms, and other unaccepted flags.
2. Resolves shared terminal `--help` and `--version` before workspace or domain
   work. Command-specific input combined with a terminal mode is invalid.
3. Collapses repeated `--configure`, `--force`, `--automatic`, and `--dry-run` presence to one
   Boolean each. No occurrence wins by order.
4. Resolves the exact current directory or exact `--workspace` value through the
   shared contract. It does not discover another root.
5. Resolves the preset, frontmatter, and Custom row values under the Interface's
   repetition
   and composition rules before setup selection.
6. Preserves independent dimensions: automatic does not set force, and dry-run
   does not remove authority from the plan it previews.

A human application that would write may continue without `--automatic` only
when standard input and the prompt stream on standard error are both terminal-
capable. After the complete plan and preflight succeed, it asks exactly once
before acquiring the workspace lease or beginning an effect. Confirmation
continues the already formed plan. Refusal, end of input, or caller cancellation
returns `cancelled` and writes nothing.

Setup selection precedes planning as defined under Configuration Planning.
Dry-run, ordinary verified no-op, `--automatic`, JSON, and non-prompt-capable requests
never prompt. A non-prompt-capable human application that would write is
`invalid-input` unless `--automatic` is explicit and directs the caller to rerun that
same command with `--automatic`. Automatic never supplies force or bypasses a
safety boundary. Decorative prompt wording is not contract meaning.

## Configuration Planning

Resolve setup selection through the existing Shell prompt contracts before
planning or exact-no-op admission. Selection observes concrete settings,
ordinary route presence and the Install-owned Git-ignore section. It does not
write files. Preserve one resolved immutable selection in the request and plan.
Use the Interface's deterministic policy when interaction is unavailable.

Ordinary first setup selection retains the existing safe initial-establishment
and bounded adoption rules below, using the prospective selected settings.
It also retains their ownership publication for safely installed replacements
and preserved canonical bases. Settings and ignore files remain authored inputs.
Loader absence alone does not establish initial-adoption eligibility when a
Framework receipt already identifies an established installation. Explicit
`--configure` uses additive preservation with the narrow frontmatter conversion
exception below, rather than initial replacement or adoption authority.

After preset resolution, first interactive Install asks
`How should Open Forge write file metadata?` with root preselected. Choices are
`Root keys` with `description: and tags: at the top of the frontmatter`, and
`Scoped under open-forge:` with `open-forge: holds description: and tags:`.
An explicit `--frontmatter` skips the question. A fresh unattended Install uses
the flag, then an explicit preference already in settings, otherwise root.
Every fresh Install writes the resolved key. Ordinary repeated Install and
Update never ask and never write it. On an installed workspace, explicit
`--frontmatter` requires `--configure`. Form-only configuration is valid without
a preset and keeps route choices unchanged.

Plan authored settings, selected payload, required ancestors, affected generated
navigation and the bounded Git-ignore section together. Use the existing
settings codec so unrelated and unknown JSON members survive. Do not persist a
preset identifier. Keep original settings and ignore snapshots as preconditions,
while prospective topology uses the planned settings. Add retains narrower
omissions. A broad Memory exclusion may be replaced by the complete explicit
state selection, preserving the states the user chose to omit.

Route configuration has a distinct additive admission before ordinary managed
divergence. It creates missing eligible selected defaults and retains occupied
authored files, overwrite companions and compatible route hosts. Existing
unselected managed targets retain their ordinary identity checks. It never
replaces an edited managed payload as an implicit Update or interprets matching
bytes as proof of ownership. Selected missing scaffolding may be recreated after
checkout with or without an ownership file when the existing loader and host
boundaries are safely recognized. Competing manager claims, ambiguous topology,
unsafe paths or invalid document boundaries still block the complete plan.

As a narrow exception to Configure's additive-only rule, Configure compares
each owned eligible delivered target with the payload rendered in the selected
form and in the other form. A match with the selected form needs no effect. A
match with the other form becomes one whole-file replacement in the selected
form with projected `Entries`.

Targets matching neither form, including edited files and Extension files whose
source is unavailable, are kept unchanged and reported. Excluded, user-authored,
Library, and overwrite files are never touched. Retention under this rule does
not block the form change. The settings write and conversions form one reviewed
plan with ordinary recovery and the existing safety and permission checks.

Remove records the selected omission and releases only Framework receipts for
the supplied defaults in that selected canonical subtree. It does not delete
files, remove user-owned records, release Extension/Library ownership, or release
unrelated scoped Framework claims. Publish one final Framework ownership
projection after verification instead of combining release/publication effects.

Git-ignore selection uses one marked Install-owned section containing anchored
contents patterns (`/<directory>/*`) followed by an exception for the actual
direct entrypoint (`!/<entrypoint>`). Reuse a unique compatible host. Existing
whole-directory patterns remain recognized and upgrade during configuration.
Preserve all bytes outside that section. Add and Remove
clear only the selected route's Install-owned pattern, not unrelated user rules.
Malformed, duplicated or ambiguous section boundaries block safely. Changes
participate in ordinary file expectations, preflight, complete recovery,
lease-bound revalidation and verified application. The whole ignore file never
becomes Framework-owned. Invoke no Git subprocess and untrack no existing file.

Create or restore selected routes and settings, then verify them. Publish and
verify `framework.gitIgnoredRoutes` in `.agents/open-forge.lock.json` before
applying Gitignore. Failed registration leaves Gitignore unapplied and reports
the verified earlier effects. Configuration requires trustworthy existing lock
facts. An absent lock can be rebuilt. Ordinary installation retains best-effort
ownership publication after a valid or absent read; an invalid or unreadable
existing lock blocks all Install projections before effects, including force.

Install's shared generated navigation uses the recorded or prospective sharing
policy. It retains the shared entrypoint and parent link while omitting private
children, nested entrypoints and overwrite metadata. Local discovery remains
complete. [Index](../index-candidate/behavior.md#route-sharing) defines the matching
indexing boundary.

Settings and ignore changes participate in no-op detection, reported effects,
counts, recovery and final verification. A stale settings, ignore, payload,
projection or ownership snapshot stops the plan before any affected write.
Confirmation continues exactly that complete plan. Repeating the same concrete
configuration produces no effects. Ordinary repeat Install retains its prior
exact verification and divergence behavior.

## Exact Workspace And Payload

Workspace resolution establishes the selected directory, lexical containment,
physical identity, and access required by this operation. Missing, unavailable,
non-directory, escaping, aliased, or otherwise unsafe boundaries return
`blocked` before lifecycle work. The resolver never substitutes a Git root,
package root, marker location, nested `.agents`, or nearby source tree.

For ordinary Install, the source resolver admits only the embedded current
Framework payload. It
validates current destination identity, supported file kinds, source identity,
and containment. Safely unavailable payload coverage is `incomplete`; malformed,
ambiguous, or unsafe source identity is `blocked`. Force cannot make an
unavailable or unsafe source usable.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof establishes distributed source
identity only; it is not workspace or runtime implementation evidence.

Resolve that inventory through the CLI Architecture's neutral Framework
distribution capability over ordinary .NET embedded resources. Runtime never
reads the repository source tree.

The closed current-fact universe includes:

- named installed content destinations below `.agents`;
- the generated `.agents/open-forge.lock.json` ownership control file as a
  separate state-file population;
- the canonical `AGENTS.md` managed region;
- the supported Claude `CLAUDE.md` bridge region;
- current authored topology and metadata needed for affected generated regions;
  and
- recognized recovery-bundle provenance, kept outside the ownership lock.

Ordinary Install excludes arbitrary providers, package sources, Extension
payloads, overwrite
companions as Framework targets, retired-only paths, files outside the closed
Framework footprint, and the repository `.temp/` directory.

Explicit configuration also includes the authored settings exclusions and the
bounded owned Git-ignore section as separate effects. The ordinary selected
fact universe is the closed base Install subset. Ownership may
also contain scoped paths and regions from Route Init. Preserve those receipts
and current content without selecting them as root effects or checking them
against stored integrity facts. Configure's frontmatter conversion also observes
owned eligible Framework and Extension delivered targets, including scoped
copies, and their available source bytes. This grants only the conversion effects
defined above, with shared rendering eligibility and existing permissions.

## Ownership Observation And Publication

The generated `.agents/open-forge.lock.json` is the only ownership state file.
The authored `.agents/open-forge.json` supplies settings.
Read its ownership receipts with the forgiving workspace reader. An absent
ownership file is known empty. Invalid or unreadable existing ownership cannot
establish the route-sharing policy and blocks Install before effects, including
force. This read prerequisite does not require ordinary ownership publication
to succeed. Unavailable facts also do not authorize adoption when a competing
claim cannot be ruled out.
Do not read, migrate, delete, or honour leftover records from earlier formats.
A matching file does not establish an ownership receipt.

Root Install selects only its current embedded destinations and supported managed
blocks. Existing Framework receipts outside that subset are preserved without
reading their content or making their absence a root Install failure. Source
release metadata in a receipt does not participate in currentness.

Plan one best-effort lock publication after target verification. Preserve the
other ownership sections and unaffected Framework claims. An identical intended
receipt plans no write. If the ownership writer cannot form a safe change, skip
that change and continue; the existing public publication outcome is
`not-requested`. When publication is planned, its exact prior state participates
in preflight, revalidation, recovery preparation and verified application.
The public effect list contains that one state-file effect. It replaces the
former state-file subject rather than adding another outcome.

A known Extension claim at a selected Framework path remains an ownership
conflict, including portable case aliases and claims on a region's host.
Force cannot overwrite that destination.

## Initial Workspace Adoption

During safe initial establishment, ordinary Install automatically completes
missing compatible required metadata and route entrypoints in the selected
standard route subtree when they are needed for routability. It does not require
a new flag or `--force`. A trusted exact managed state stays a no-op and is
classified `TrustedExact`. When a previously verified managed authored base
needs only scoped workspace adoption or the generated navigation required by
that adoption, Install proceeds without `--force` and is classified internally
as `ManagedAdoption` and publicly as `managed-adoption`. It is not reported as
an exact no-op. Missing, replaced, or divergent authored managed payload remains
managed divergence and directs the caller to `open-forge update`.

For managed-base admission, independently verify each selected non-user payload
target's authored source fingerprint and required managed-block contents using
the existing readers. Exclude `UserOwnedPaths` from this base-admission check,
but include every intended target in final exact applied verification. For a
managed generated target projected from scoped adoption, its generated `Entries`
fingerprint may differ only when its authored fingerprint still matches and the
plan records the corresponding navigation migration. Retain the existing
bounded generated-span safety check before any write. Only the verified managed
target set, planned `UserOwnedPaths`, and explicitly preserved unowned category
entrypoints receive the occupancy exception. Other occupied payload files keep
their existing `--force` boundary.

Ownership observation precedes candidate formation. An absent ownership file is
known empty. A known Framework, Extension, or Library claim is checked under
its existing lifecycle rules. An invalid or unreadable existing lock blocks all
Install effects because its route-sharing policy is unavailable. Recognized data
in an unknown schema retains the forgiving reader behavior; schema version alone
does not block. Unavailable claims cannot authorize migration, and ordinary
best-effort ownership publication remains a separate write boundary.

The candidate set is limited to existing Markdown sources and missing
entrypoints in the selected standard route subtree that are required for
routability. During first establishment, it reuses a unique recognized
entrypoint in place unless that unowned file occupies the exact shipped category entrypoint path. At that exact
path, Install preserves its authored content in the adjacent `.overwrite.md`
companion and installs the Framework base without requiring `--force`.
Recognized generated navigation and its heading are omitted from the preserved
content; ambiguous sections remain intact rather than being discarded. An
existing companion is retained after the preserved content so its precedence
survives. The companion remains user owned, while the installed base receives
Framework ownership. Both effects use exact observations and the existing
recovery and verification pipeline. A repeated Install does not migrate the
content again. Excluded or unsafe preservation destinations block preflight.
It creates only the
missing canonical `_{folder-name}.md` route source required by the selected
topology, or adds a missing `## Entries` section where required. It honors
`removedFiles` and removed defaults. Ordinary catalogue observation may read
other safe sources under its existing selection rules, but unrelated content is
not adopted or normalized. Outside junction targets are never followed.

The prospective metadata edit adds only missing compatible required fields.
It preserves existing valid fields, unknown YAML members, body bytes, encoding,
newline style, overwrite companions, binary content, and user ownership. It
does not fill optional fields that remain allowed to be absent. Malformed,
ambiguous, conflicting, or unsafe inputs block the complete preflight with a
finding tied to the affected path or field.

For native `SKILL.md` files, use native semantics. A missing required
`name` is derived from the Skill directory basename. A missing `description`
uses an existing usable description, then an existing usable title, then the
first top-level H1 heading, then the workspace-relative path. Install does not
add an `open-forge:` wrapper or `Entries` to a native Skill. It leaves
complete native fields, including optional `license`, untouched. This also
applies to an unowned native Skill at a bundled `SKILL.md` destination: preserve
that Skill instead of installing the bundled file or requiring `--force`. Resource
catalogues under `references` remain within the accepted Task 47 selection
boundary.

Other required route descriptions use an existing usable description, then an
existing usable title, then the first top-level H1 heading, then the
workspace-relative path. A required tag is added only if the new completion
actually needs ordinary classification. The only permitted synthesized search
tag is `Workspace`. Do not infer loading, behavior, authority, or state tags.

The final prospective document must have one unique safe `Entries` boundary.
Authorized adoption may first append a missing section; duplicate or ambiguous
boundaries still block. Install forms prospective source bytes and topology
before its existing generated-navigation projection. The shared Index parser,
selector, and projector do not change. Payload-owned targets remain separate
from user-adoption targets. Migrated sources and new local resource entrypoints
do not receive whole-file Framework ownership. A verified generated
`Entries` region may retain its separate region receipt.

The metadata and entrypoint effects join the same complete Install plan. An
existing source edit uses exact prior bytes, revalidation, the existing recovery
bundle, application, and post-verification. A created entrypoint uses the
existing create effect. Install does not add automatic rollback or change
recovery retention.

On a managed root, a generated Entries host updated to expose a newly observed
selected-route source records a `navigation-updated` migration at that host,
even when the native source itself needed no metadata edit. Preserve the host's
existing ownership in its current region or whole-file form. `UserOwnedPaths`
describes ownership classification; it is not a synonym for migration rows.

## Semantic Fingerprints And Current Bytes

For supported parseable Markdown and frontmatter kinds, the operation uses the
`open-forge-markdown-v1` conservative parser/AST-derived, syntax-aware semantic
fingerprint. It:

- preserves Unicode and semantic text without blanket ASCII conversion,
  Unicode loss, case folding, or unsupported normalization;
- preserves headings, tags, links and destinations, managed-host boundary
  meaning, inline text,
  code-block content, and semantically significant whitespace;
- normalizes line endings and only parser-proven formatting trivia;
- excludes derived generated `Entries` interiors from authored identity while
  retaining the Entries heading and outside bytes; and
- fails closed when syntax or equivalence is unsupported or ambiguous.

Unsupported, binary, and unparseable kinds use exact-byte comparison. No
fingerprint, comparison policy, or workspace binding is persisted in ownership.

Every invocation captures current exact bytes freshly for the plan, bounded diff,
expected-state revalidation, bundle payload, write verification, and recovery.
If
current and intended semantic fingerprints are equal while exact
bytes differ only in parser-proven formatting trivia, the operation reports a
formatting-only observation and does not treat it as divergence or rewrite it
under install.

The accepted conservative formatter direction permits advice only. Install does
not execute a formatter or persist formatter state. Advice never grants authority
or changes the plan.

## Management Classification

The classifier evaluates every recognized target and managed region together:

### Safely absent

Normal planning may establish management only when complete inspection proves
all four facts:

1. No Framework receipt selects a target in the closed base Install subset.
2. No exact current payload destination is occupied.
3. No recognized canonical or exact legacy `AGENTS.md` or supported `CLAUDE.md`
   managed host exists.
4. No current unresolved or uninspectable recovery evidence blocks Install.

Recheck each verified recovery final against its recorded prior and intended
file identities. A complete prior state is restored. A complete intended state
is applied. A safely observed different identity supersedes the old recovery
snapshot. These finals remain available as history and do not block Install.
A mixture of exact prior and intended states still represents partial recovery
and blocks. Incomplete drafts, invalid finals, unavailable observations and
unsafe target objects also block. Apply this check during planning and again
under the workspace lease. Do not use age, timestamps or a Git branch name to
decide relevance. Install does not delete or replay historical bundles.

Existing hosts with no managed-host boundary candidate and unrelated user-owned
`.agents` content do not alone defeat this state. An unavailable absence fact
is `incomplete`; partial, malformed, ambiguous, colliding, or occupied evidence
is `blocked` when unsafe. Neither writes.

### Trusted exact managed state

A Framework receipt selects the base footprint, and its current source content
matches the running payload rendered in the workspace's frontmatter form under
the existing operation-time comparison policy. Source inventory hashes retain
repository-byte identity. Current generated navigation also matches the intended
projection. Install forms
a verified no-op and preserves unrelated scoped ownership and content. It does not invent a write to normalize timestamps, formatting,
provenance, or unrelated bytes. `--force` and `--automatic` do not change the
no-op.

### Bounded adoption from a verified managed base

When a previously managed workspace gains a selected compatible source,
ordinary Install may complete only the scoped adoption and generated navigation
needed for that source. First verify each selected non-user payload target's
authored source fingerprint and required managed-block contents with the
existing readers. A missing, replaced, or divergent authored managed target
does not qualify. For a generated target, permit a different `Entries`
fingerprint only when its authored fingerprint still matches and the intended
projection carries `navigation-updated` for that path. Keep the existing
bounded generated-span safety check before writing.

If this verified base has migration effects, classify the plan as
`ManagedAdoption` internally and set schema-3 `data.classification` to
`managed-adoption`; do not short circuit it as `TrustedExact`. Exempt only the
verified managed target set and planned `UserOwnedPaths` from the initial
occupancy/force boundary. Continue
exact final-byte verification for every user-owned target, ownership snapshot
revalidation, lease-time rebuild, and all existing application checks. A
generated managed `Entries` host may have a `navigation-updated` migration row
while retaining its prior ownership. `UserOwnedPaths` is an ownership
classification, not a list of every migration path. If there are no effects or
migrations, retain `TrustedExact` and the existing no-op result.

### Managed divergence

A selected authored managed target is divergent when its content differs from
the running payload or a selected expected path is missing. A generated region
that differs from the intended projection is also divergent unless its authored
host verifies and the difference is exactly the bounded navigation migration
for the scoped adoption above. A stale source version or an unselected scoped
claim does not establish divergence.
Outside Configure's bounded form conversion, Install does not reconcile these
states. It forms no mutation plan,
returns `blocked`, preserves current bytes, and directs the caller to
`open-forge update`. The same result applies when `--force` is present. Initial
force is not managed-update authority.

### Eligible initial occupant

An exact current payload destination or supported managed block may be an
eligible initial occupant only when no known ownership receipt or competing
manager claims it and all route, source, physical-identity, containment,
managed-host boundary, bundle identity and recovery-bundle facts are safe. A
known user-owned or Extension-owned path, route collision, ambiguous managed
block, unknown path, or unsafe boundary is not eligible.

Normal install blocks an eligible occupant without writing. Explicit force may
replace only the exact recognized occupant, never its surrounding host bytes,
and establishes management from the new current source after complete
verification. Previous occupant bytes are retained through recovery when a replacement is
planned. Matching pre-existing files do not become whole-file ownership merely
because a no-op verifies their bytes.

## Intended State And Generated Projection

For a safe establishment or eligible force request, the planner first forms one
hypothetical post-install workspace from current authored content, permitted
payload effects, and eligible prospective metadata and route completions. It
preserves user routes, Memory, overwrite companions, intentionally absent
defaults, Extension content, and all bytes outside exact planned target or
generated-region spans.

After adoption candidates have their prospective bytes and source facts, the
generated-navigation projector uses the current Index rules to derive every
affected `Entries` body from that hypothetical authored topology and metadata.
It does not use current generated lines as topology or metadata and does not
copy generated interiors from the embedded payload. A lifecycle plan cannot
invoke a hidden `index` operation or change shared Index acceptance.

For an existing section, only the body of one unique top-level `## Entries`
section may change. A missing section may be added only by a planned
`entries-section-added` action on an eligible route source with an unambiguous
insertion point. A duplicate or ambiguous heading boundary blocks before any
write. Existing headings and all bytes outside a changed body remain unchanged.
Retired guard comments inside a rewritten body are removed by the shared
generated-navigation projection.

Root and provider resolution admits only an absent host where creation is
supported, an existing host with no boundary candidate for bounded append, or
one complete canonical heading and footer pair or exact legacy comment pair for
bounded replacement. It recognizes boundaries from
Markdig parsed root-level blocks and typed source spans. Fenced code blocks,
quoted blocks, nested lists, and inline examples do not delimit a host. Missing,
reversed, duplicate, mixed, or otherwise ambiguous boundaries block before any
write. A heading without its named footer never captures the user suffix or the
end of the file. Install emits the canonical heading and footer form for new or
eligible replacement hosts. A managed legacy host is managed divergence even
when its instruction or import body matches, so it remains blocked and directs
the caller to Update. Install never replaces host bytes outside the managed
block.

## Complete Plan And Preflight

The one ordered plan records, for every effect:

- exact logical and physical target identity and containment;
- creation or eligible initial replacement kind;
- complete expected current and intended bytes or bounded interiors;
- current expected-state and revalidation conditions;
- generated projection and lifecycle-section effects;
- recovery-bundle readiness, identity, and collision facts;
- per-effect and whole-operation verification;
- exact bundle identity, provenance, success-removal, and residual-reporting
  facts.

Preflight validates all source, target, route, ownership, containment,
managed-host boundary,
cross-section, expected-state, recovery-bundle, verification, and preservation
facts. Every planned existing-target effect (`Replace` or
`ReplaceGeneratedRegion`) must be covered by one
verified bundle preparation; a `Create` or semantic/byte no-op has none. A
verified no-op has no mutation path and needs no bundle.

Before the first workspace effect, the implementation obtains the actual OS lock
for the persistent reusable zero-byte external path defined by the [Mutation And
Recovery Technical Design](../../technical-designs/mutation-and-recovery.md). The operation holds one read/write `FileShare.None` handle and
never writes metadata, truncates, or deletes the lock file. An active handle
blocks the plan; lock state is not lifecycle authority, history, or recovery
evidence.

`--automatic` admits only deterministic safe effects already selected by
ordinary Install, including eligible bounded metadata and route completion. It
cannot admit an eligible initial payload occupant without explicit
`--force`. It never admits divergence, deletion, ownership, or managed-host
boundary repair.

## Dry-Run Parity

Dry-run uses the same normalized request, fresh current facts, ownership
receipts, classification, intended state, prospective adoption bytes, generated
projection, complete plan, and preflight as application. It reports all safe
creations, eligible force effects, planned migrations, bounded generated and
managed-region changes, lifecycle publication that would occur after
verification, preserved content, and recovery readiness.

At minimal detail, the preview identifies the planned
`.agents/open-forge.lock.json` state-file effect when ownership publication
would occur, separately from named installed content files, child directories,
and managed host regions. It does not fold the lock path or host regions into
the installed content-file population.

It stops before directory, file, lifecycle, recovery-bundle, temporary, formatter,
or other persistent effects. It does not claim application, verification,
lifecycle publication, or bundle-handling success. It forms the same pre-effect status
as the corresponding application request. Because dry-run performs no effects,
it never produces an apply-time `failed` or `cancelled` result. A planning or
read failure and caller cancellation before effects retain their own event
meaning.

## Application, Verification, And Recovery

When application is selected:

1. If this is a prompt-capable human application that would write, ask the one
   confirmation after complete preflight. A refusal, end of input, or caller
   cancellation stops with no effects.
2. Acquire the persistent external workspace lease. The zero-byte ordinary lock
   lives under `LocalApplicationData/OpenForge/locks/v1`, with a display-only
   friendly workspace prefix and the authoritative full SHA-256 key of the
   normalized physical workspace path. Cancellation before acquisition creates
   no workspace effect.
3. Revalidate the complete plan and all volatile source, target, ownership,
   containment, managed-host boundary, section, and expected-state facts.
4. Prepare and verify the one complete external recovery bundle when the plan
   contains an existing-target effect. Complete preparation before any workspace
   effect.
5. Apply every explicitly planned missing directory parent-first through the
   shared directory capability. Missing `.agents` is the first ordinary
   directory-create effect; its later planned directories are descendants.
   Immediately revalidate each missing target and its exact contained physical
   parent, call ordinary `Directory.CreateDirectory`, then verify the exact
   resulting contained ordinary directory.
6. Revalidate each file or bounded-region target immediately before its effect.
7. Apply complete planned file or bounded-region bytes through the accepted safe
   replacement property. Do not edit in place or weaken the property after a
   check fails.
8. Verify each payload, root/provider, generated-region, and lifecycle effect.
9. Rebuild and verify the complete recognized Framework result and preservation
   boundaries as one operation.
10. Publish the planned Framework ownership update after target verification,
    preserving the other sections and unaffected existing ownership. An
    unchanged receipt writes nothing. Exact prior lock bytes participate in the
    same recovery bundle when replacement is planned. Verify the state-file
    effect and report its actual outcome.
    During configuration, verify the route-sharing registration before applying
    the deferred Gitignore effect, then verify the whole result.
11. After final verification, delete only the positively recognized bundle
    created for this operation. `Deleted`/`Removed` permits normal completion.
    `Failed`/positively observed `Retained` keeps target effects successful and
    produces `completed-with-warnings`, the exact residual path,
    and cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the
    deletion result provides one.

Install has no target deletion effect. Before any existing byte or bounded region
is replaced, orchestration selects only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is
pre-effect `incomplete`. For an operation with one or more existing replacement
targets, it creates one immutable ZIP bundle outside the
workspace for the complete operation. Its deterministic external directory key
and final name use the normalized physical workspace path and operation ID. An
operation containing only creates or no-ops creates no bundle. A source-generated schema-v1
`manifest.json` and streamed ordinal payload entries record operation and
normalized physical-workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
The draft uses `CreateNew` under its exact name, is closed and reopened for
semantic manifest, exact ordered entry, length, hash, and payload-byte
validation, moved within the same directory to the deterministic final name,
and reopened and verified again. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. Every planned
existing-target effect must match the preparation; Create and no-op effects create
no bundle. All preparation is complete before the first target effect.

Directory creation remains a distinct effect from file Create/Replace and has no
recovery entry. Missing `.agents` is the first ordinary lease-bound directory
effect. A directory created by this operation is retained and reported as
residual state after later failure or cancellation. Install never rolls it back,
compensates for it, or removes it. The shared capability uses ordinary BCL
filesystem behavior; it adds no P/Invoke, recovery protocol, or hostile same-user
creator-identity guarantee. Lock and recovery data use separate
application-owned versioned subtrees under `LocalApplicationData`.

Before post-verification deletion begins, an application, verification,
lifecycle-publication, or cancellation outcome stops new effects and reports
the actual residual draft or final path; a valid final remains when preparation
completed. The foundation never restores a target automatically or derives
current target state from recovery provenance. A closed final ZIP may remain
after abrupt process termination, without an executable crash or power-loss
guarantee. An unexpected concurrent edit is preserved and reported as residual
state. An unsafe residual is `failed`; caller cancellation is `cancelled` only
when no stronger failure remains. Cleanup owns exact named final and draft
deletion under its separate lease-bound contract. A later
invocation forms a fresh plan and never replays a saved plan, receipt, journal,
history, or progress record.

## Result Formation And Streams

The operation forms one typed result after invalid input, classification,
preflight, dry-run, verified application, interruption, or recovery. Human and
JSON renderers consume that result and do not rerun lifecycle work.

The typed result forms exactly the ordered command-local result graph frozen by
the Interface: mode, force, automatic, atomic nullable embedded-source identity,
atomic nullable destination classification, atomic nullable managed-footprint
counts, exact ordered effects, the internal migration collection, lifecycle,
recovery, verification, and ordered findings. Managed-footprint counts distinguish
named installed content files from child directories below `.agents`; the
`.agents` container effect is
not included in the directory count, and the generated lock file and host
regions remain separate populations. All required top-level properties are
present for every status and collections are non-null. Public schema-3
`data.migrations` is omitted when the internal collection is empty and appears
at every detail level when rows exist. The shared envelope's command, status,
workspace, and next action are not duplicated. Effect residual state remains the typed value `none`,
`retained`, or `unknown`, so dry-run, retained state, partial failure, and
uncertain completion cannot be collapsed into a Boolean.

The result retains exact workspace and selection method, normalized flags,
source identity, recognized payload footprint, separate user-adoption targets,
ownership and management classification, semantic current and intended facts,
generated projection, effects, migrations, preserved content, lifecycle
publication, recovery-bundle facts, verification, residuals, and at most one
next action.

Use the Interface status meanings and ordinary precedence `blocked` >
`incomplete` > `completed-with-warnings` > `completed`. `completed` includes safe application,
eligible force, dry-run, and exact no-op. Ordinary managed divergence is
`blocked`, not `completed-with-warnings`, because install does not own update authority.
Planned effects, migration rows, format-only facts, force presence, automatic
mode, and managed divergence do not form `completed-with-warnings`. Managed
divergence directs the caller to `update`. Post-verification recovery deletion `Failed` with positively observed
disposition `Retained` is the only current install `completed-with-warnings` condition.

Primary human `completed`, `completed-with-warnings`, and `incomplete` results go to stdout.
Primary human `invalid-input`, `blocked`, `failed`, and `cancelled` results go to
stderr. JSON emits one complete result to stdout for every semantic status, and
bounded diagnostics use stderr. No status claims a runtime implementation or
shipping evidence.

## Behavioral Conformance

The pinned Skill Creator process acceptance uses the public read-only fixture
from `anthropics/skills` commit
`8a1541c4a3ffa5a20a5a91de0dcf3f0bab1d1ef4`, subtree `skills/skill-creator`,
tree `d482eba557f7b2035c8a83589809628b96f6f40e`. Its verified manifest SHA256
is `DEE669A0BE312D756B0F0E7EC00294F4D7FB71557C99B1EDA93F53086333C064`; it
contains 18 regular blobs totaling 224,992 bytes. The upstream root `LICENSE`
is absent at this commit. Preserve its provenance evidence and do not require
or substitute one. The actual subtree `LICENSE.txt` is retained (SHA256
`BC6B3AF2F331CBC7FB0DA1344EFB2CBE5877A31498B4D70DBC7000F3405A1362`). The
original native `SKILL.md` has `name` and `description`, no `license`, and
SHA256 `DCD4803E61E913E6FC27294184CD3A71F09F5E924FF20C8A9A20173E7B3C2BCF`.
The first ordinal Markdown reference is
`.agents/skills/skill-creator/references/schemas.md` with original SHA256
`8E8876180A8989B406A4D3EDDDf875B04CDFD5805CC8616686D552B11CE4455F`. Use the
separate ready cache/manifest, never execute downloaded instructions or
scripts, and do not edit the immutable cache. The allowed public read-only
sources are `api.github.com` and `raw.githubusercontent.com`.

The following exact argv are confirmed by installed CLI help. The process
harness sets cwd to the isolated workspace and uses an artifact-owned
`OPENFORGE_DATA_HOME`:

```text
install --automatic --dry-run --format json --detail full
install --automatic --format json --detail full
install
index --dry-run --format json --detail full
index --format json --detail full
context .agents/skills/skill-creator/SKILL.md --content frontmatter,body --format json --detail full
route list .agents/skills/skill-creator/SKILL.md --depth=all --format json --detail full
route inspect .agents/skills/skill-creator/SKILL.md --format json --detail full
context .agents/skills/skill-creator/references/schemas.md --content body --format json --detail full
```

Index applies when `--dry-run` is omitted and has no `--automatic` option. A
`references .agents/skills/skill-creator/SKILL.md --direction both --format
json --detail full` call may supplement authored-link inspection; it does not
replace the explicit reference-body Context command. Inspect only actual
Markdown links and report unresolved upstream links without altering valid
third-party content.

**A, brownfield:** use the intact 18-file subtree in a pre-existing workspace
with no Open Forge entrypoints. Automatic JSON dry-run must write nothing;
plain interactive Install runs through the Windows TTY harness with cwd set to
the workspace and input `y\r`. Verify exit, text `Migrated` paths, source
preservation, native semantics, and additive catalogue files. Read Skill
Context, route list/inspect, and the explicit `schemas.md` body. Automatic JSON
repeat must have zero effects and absent-or-empty `data.migrations`; default
Index dry-run must require no additional catalogue repair. A separate intact
copy runs the automatic JSON dry-run/apply pair to prove planned and applied
schema-3 migration facts. Hash all upstream files after Install and Index and
list additive catalogues separately.

Use separate explicit test mutations of the native Skill frontmatter only:
`missingFM` removes the original frontmatter while retaining the original body;
`mutated-pinned-skill-partial-name-license` retains the original `name`, adds
`license: Task70-fixture-sentinel`, and omits `description`. The sentinel is a
preservation probe, not upstream license metadata. Every other fixture byte,
including `LICENSE.txt`, stays unchanged. The earlier minimal MIT-license
regression remains separately identified.

**B, managed root:** in a separate workspace, perform a normal fresh Framework
Install with plain interactive `install` through the Windows TTY harness and
input `y\r`, then copy the intact subtree under
`.agents/skills/skill-creator`. Before adoption, capture these commands in
order, with streams, exits, and per-file source hashes:

```text
index --dry-run --format json --detail full
index --format json --detail full
context .agents/skills/skill-creator/SKILL.md --content frontmatter,body --format json --detail full
route inspect .agents/skills/skill-creator/SKILL.md --format json --detail full
route list .agents/skills/skill-creator/SKILL.md --depth=all --format json --detail full
context .agents/skills/skill-creator/references/schemas.md --content body --format json --detail full
```

Promptly report any failed or incomplete pre-Install result with exact argv,
exit, and findings. Do not waive it, add metadata or entrypoints manually, or
change Index. Continue evidence-only adoption when safe. Run automatic JSON
Install dry-run and plain interactive Install with `y\r` through the Windows
TTY harness. Plain Install must adopt through the verified managed-base path
without force. After Install, rerun Skill Context, route inspect/list, and the
explicit `schemas.md` body read. Verify source preservation and catalogues.
Automatic JSON repeat must have zero effects and no migration rows; default
Index dry-run must show zero additional catalogue repair. Capture cwd, argv,
TTY mode, input, raw streams, numeric exits, before/after hashes, and candidate
runtime/source hashes. Candidate provenance includes the selected executable,
every published `OpenForge.Cli*.dll`, runtime configuration and dependency
files, version marker, test assembly, and full candidate source inventory. At
least one plain Install uses a real TTY.

Before QA, the Task70 owner freezes both scenarios, fixture provenance, and
these exact command forms in the task acceptance record. Do not rerun the
historical public beta 4 baseline in this packet. The historical
`r_b714963066ce` ambiguity remains unresolved.

A conforming implementation must demonstrate:

- exact request normalization, terminal handling, Boolean repetition, and workspace
  selection without discovery;
- closed embedded-payload footprint and rejection of arbitrary providers,
  operands, route coincidence, tags, and matching-byte ownership inference;
- source/payload set and byte parity plus published Native AOT resource access
  away from the checkout;
- all four safe-absence facts, trusted exact no-op, verified managed-base
  adoption with `ManagedAdoption` / `managed-adoption`, managed authored
  divergence block with `update` next action, eligible initial occupant, and
  force-only initial replacement;
- forgiving ownership observation and isolation, including absent and unreadable
  states, source-unavailable facts, and unsupported or ambiguous schema handling;
- bounded initial workspace adoption, with ownership observed before candidate
  formation, required fields only, exact description and native name derivation,
  no invented reserved tags, no outside-junction traversal, and precise
  malformed, ambiguous, conflicting, and colliding input blocks;
- nullable operation-time `sourceAssetPath` on selected effects, separate
  whole-file and region receipts, and preservation of unselected scoped claims;
- syntax-aware semantic fingerprints, exact operation-time bytes, format-only
  observations, generated-interior exclusion, and fail-closed equivalence;
- one intended topology and current Index projection;
- prospective adoption bytes formed before that projection, with payload-owned
  targets separate from user-adoption targets and no shared Index or parser
  relaxation;
- managed-base verification that separates authored payload identity from
  scoped generated navigation, preserves existing ownership, and keeps the
  force exception limited to verified managed targets and `UserOwnedPaths`;
- bounded generated sections and canonical or legacy root/provider boundaries;
  no hidden subprocess;
- canonical `# Open Forge` and standalone footer output for both hosts, exact
  legacy boundary input, source/dogfood parity, unchanged bytes outside the
  managed span, and blocked incomplete, reversed, duplicate, mixed, or
  ambiguous boundaries;
- complete preflight, external schema-v1 recovery-bundle preparation and verification,
  exact prior-byte preservation, expected-state revalidation, per-effect and
  whole-operation verification, typed post-verification deletion
  state/disposition facts, residual reporting, and fresh rerun;
- the exact one-prompt matrix, no-write refusal/end-of-input/cancellation,
  direct automatic rerun guidance for non-prompt-capable human writes, and no
  prompt in dry-run, no-op, automatic, JSON, or redirected modes;
- separate parent-first directory effects under the held workspace lease, with
  immediate missing-target and physical-parent revalidation, ordinary BCL
  creation, post-verification, retained residuals, and no rollback,
  compensation, removal, or recovery entry;
- the exact visible missing-`.agents` first ordinary directory effect after
  external lease acquisition, including planning/reporting, immediate
  revalidation and verification, pre-effect cancellation/contention, and
  retained residuals after later failure;
- dry-run/application parity with no persistent dry-run effects;
- pinned real Skill Creator brownfield and already-managed-root process
  acceptance, including the pre-Install default Index result, plain TTY
  confirmation, automatic JSON dry-run/apply, Context and route/reference
  inspection, repeated zero-effect Install, and zero-repair default Index dry-run;
- schema-3 migration rows with exact actions, fields, derivation sources,
  planned/applied outcomes, omitted public no-op migrations, and verified-only applied rows;
- seven statuses, including `Failed`/positively observed `Retained` recovery
  `completed-with-warnings` and `Failed`/`Unknown` recovery `failed`, streams, one typed result,
  JSON stdout, bounded diagnostics, and one next action;
- no formatter execution or persisted formatter state;
- Gate 5 proof of source-generated serialization, fixed Markdig where used, real
  `System.IO`, Native AOT, OS locking, isolated tests, and package journeys;
- no runtime implementation or shipping claim.

## Deliberately Removed Framework Destinations

Apply `removedCategories`, `removedFiles`, and removed defaults from authored
settings. A category such as `skills` excludes embedded targets beneath
`.agents/skills/`. Each `removedFiles` entry is one exact canonical
workspace-relative file destination; it has no glob or recursive-directory
meaning.
When that exact file is a native `SKILL.md` host, Framework payload selection also
omits its packaged resources below the same Skill directory. Those resources
depend on the host. This does not broaden generic file exclusions or delete
existing user files.
The concrete destination is
excluded from whole-file and generated-region planning, including root managed
hosts. Without explicit configuration, do not recreate excluded files, change
settings, or infer new ownership for them. Existing user content and prior
receipts outside actual selected effects
remain preserved. If an excluded missing entrypoint makes another selected route
unreachable, report a structural blocker instead of recreating it. Required loader
and root host anchors are otherwise not categories.
