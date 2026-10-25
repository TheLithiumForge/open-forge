---
open-forge:
  description: Complete metadata and missing route chains, and repair proven manual renames without losing authored content
  responsibility: Define the shared realization and safety boundaries for Task 79 maintenance
  tags: [Memory, Crystallized, Document, CLI, TechnicalDesign, Index, Repair, Doctor, Frontmatter, Routing, Evergreen]
---

# Forgiving Maintenance Technical Design

## Selected Layout

Doctor, Repair and Index use the observed context folder and fixed native Skills location for classification, route ancestry, metadata completion and navigation. Repair carries Doctor’s layout observation into its plan and revalidates it under the lease. A lone missing loader remains restorable drift, while a recorded removal remains intentional. Layout mismatch is not repaired by guessing another root. See [Workspace Layout](workspace-layout.md#observation-and-revalidation).

The shared maintenance planner also accepts caller-selected registered outside boundaries. Its roots are selection facts, distinct from structural registration and Loader listing. Command clients retain their selection, findings, application and verification responsibilities.

Wording examples below use the default `.agents` layout. Custom-root guidance receives projected paths.

## Boundary

After manual edits, Index completes missing metadata and navigation. Repair fixes links when evidence identifies one renamed or moved file. Doctor gives the next action that can actually resolve the remaining problem.

The maintainer's [Task 79 decisions D1 to D5](../../../../archived/cli-development/tasks/task79-forgiving-maintenance.md#accepted-decisions) define the accepted outcome. This design defines the current realization. It is not an implementation receipt. The [Index](../contracts/index-candidate/interface.md), [Doctor](../contracts/doctor/interface.md) and [Repair](../contracts/repair/interface.md) contracts define public behavior. The [CLI Architecture](../architecture.md), [Generated Navigation](generated-navigation.md) and [Mutation and Recovery](mutation-and-recovery.md) remain the implementation boundaries.

## Shared Capabilities

Generated navigation and local-reference maintenance share one path-segment
encoder. It preserves `/` and percent-encodes every non-unreserved UTF-8 byte
with uppercase hexadecimal digits. Canonicalization compares authored bytes
with that encoded spelling and the accepted minimally escaped literal spelling,
as defined by the Source References contract. Resolution and source identity use
the decoded path. Literal serialization delegates character-reference recognition
to the Markdown capability's pinned Markdig scanner and entity lookup. It escapes
recognized opening ampersands after reserved path characters, without reparsing
documents or introducing another entity grammar. Ordinary ampersands stay literal.
Navigation checks raw delimiters before decoding, so encoded
filename punctuation remains a path component. Fragment corrections preserve the encoded path. An unresolved fragment
prevents path-only automatic proposals. Protected local-reference findings retain
their original finding identity and use manual ownership guidance without an
exact proposal or an unsupported Update recommendation.
Doctor excludes generated Entries and images from its same-target Repair offers,
matching Repair's existing authored-reference boundary. Index owns Entries
spelling through the generated navigation projection.
The shared reference reader projects an angle-enclosed parser span onto the
literal URL bytes, preserving its delimiters. It exposes no editable destination
when decoded entities or escapes differ from the source span. Doctor and Repair
therefore share the same exact-byte eligibility.
Without that span, a noncanonical heading remains a manual fragment finding
rather than an automatic proposal or a clean diagnosis.
The fragment descriptor is suppressed only when a fragment canonicalization will
emit its replacement finding. Images therefore retain manual fragment findings
even with an editable destination span.

Shared destination resolution retains a folder fact from fresh contained
directory attributes, or from an explicit trailing slash on a missing target.
Folders without fragments complete before target byte acquisition. Existing
folder fragments produce a malformed destination with a precise manual message.
Missing folders are excluded from file-candidate and rename proposal formation.
Doctor and Repair carry that typed folder fact into their manual findings.
Their folder messages state only the fact. Each finding's sole action names the
source and location, asking for a manual link edit or removal of the heading
fragment as defined by the Doctor and Repair interface contracts.
Category Move reference maps and Remove target sets include inventoried directory
coordinates as well as files, while file effect formation still excludes
directories. Rewrites preserve the authored trailing-slash choice. Verification
freshly resolves the resulting folder target through the ordinary resolver.

Index and Repair use `Commands/Shared/RoutedMaintenance` for eligible metadata, missing chains and affected Entries. Route Init and Route Create share `Commands/Shared/RouteScaffolding` for entrypoint composition and intended membership. Init retains its explicit draft defaults and restoration rules.

The Framework metadata completion API inserts absent fields using parsed source spans. Sources supplies read-only rename observations. Root composition runs bounded Git requests without staging, writing or network access. Doctor and Repair use the same current Entries records, protected-source policy and candidate evidence. Generated Navigation projects intended membership without applying effects. Existing expected-state, no-follow and recovery primitives guard every write.

## Metadata Completion in Index

### Eligible files

Complete readable ordinary routed Markdown and user-owned recognized entrypoints below existing Loader-recognized root routes. Include files reached through the prospective completed chain. Apply existing explicit-source selection boundaries. A leaf selection includes the missing chain to that leaf and the exposing parent, without selecting unrelated roots. Unregistered detached maintenance retains its existing behavior and receives no automatic chain or metadata completion.

For outside completion, the caller supplies selected anchors in `RootPaths`, exact canonical closure coordinates in `SelectedSourcePaths`, and the retained observation in `Policy.LayoutRead`. Its layout instance must match the catalogue. A matching available outside observation and bounded physical target proof precede snapshot acquisition. An included row or a merged physical representative supplies no admission. Ownership, removals, private routes, Library projections and ordinary-leaf checks remain effective. A null selected-path array preserves existing primary callers.

Index and Repair keep readonly metadata acquisition and bounded Entries maintenance separate from completion eligibility. Their command plans retain exact read expectations for admitted outside inputs and verify them under the workspace lease. Reading protected metadata does not permit repairing its authored links or changing Entries owned by a Library.

| Excluded source                                                                             | Reason                                                                                                                     |
| ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Loader, root host files, settings, locks and non-Markdown resources                         | Their formats and lifecycle belong to other operations                                                                     |
| Overwrite companions                                                                        | They share their base route and never supply independent indexed metadata                                                  |
| Whole-file managed Framework or Extension content                                           | Index does not change managed authored content. The managing Update operation owns it                                      |
| Generated files or managed authored regions                                                 | Completion must not manufacture user metadata in a manager's output                                                        |
| Excluded files, directories, categories or private contents in `framework.gitIgnoredRoutes` | Removal and sharing intent remain effective. The shared entrypoint may still receive its ordinary generated Entries update |
| Filesystem links, reparse leaves, unsafe aliases and Library projections                    | Ordinary writes may not follow a separately owned leaf                                                                     |

Use existing settings exclusion and route-sharing evaluators. Do not add a global Gitignore interpretation to Index. The existing recorded private-route policy excludes those contents even without Git. A generally Git-ignored file outside that policy retains today's eligibility. Rename evidence never reads ignored private candidates. No-Git operation keeps ordinary Index available.

### Derivation

Add only absent `description` and `tags`. An empty, null, duplicate, wrong-type, conflicting or syntactically invalid supplied value is malformed, not missing. Never add `responsibility`, `applyTo`, acceptance tags or inferred business topics. The only derived loading tag is `LoadNow` for ordinary Directive files, required by the existing Directive category rule.

For description, take the first non-empty root-level heading in document order from the existing Markdown parser, at any heading level, using its established visible text. Ignore headings in code, quotes and lists. Collapse heading whitespace to one space. If visible text cannot be established, use the file stem. Remove `.md`, split hyphens, underscores, spaces and camel-case boundaries, lowercase words invariantly and uppercase the first letter. Thus `deploy-checklist.md` becomes `Deploy checklist`, and `moneyRounding.md` becomes `Money rounding`. If the resulting stem has no letters or digits, use `Note`. Preserve a supplied description exactly.

Category follows recognized route roles, not an arbitrary folder with a familiar name. Inserted scope folders inherit the nearest established category. Memory state recognition follows the existing route model even when scopes intervene. Use these exact ordered defaults:

| Category                                             | Tags                                         |
| ---------------------------------------------------- | -------------------------------------------- |
| Directives, ordinary files                           | `[LoadNow, Directive]`                       |
| Guidance                                             | `[Guidance]`                                 |
| Maps                                                 | `[Map]`                                      |
| Patterns                                             | `[Pattern]`                                  |
| Templates                                            | `[Template]`                                 |
| Ordinary routed Markdown in Skills, outside packages | `[Skill]`                                    |
| Extensions                                           | `[Extension]`                                |
| Memory, no recognized state                          | `[Memory]`                                   |
| Memory Working                                       | `[Memory, Working, Contextual]`              |
| Memory Emerging                                      | `[Memory, Emerging, Contextual]`             |
| Memory Crystallized                                  | `[Memory, Crystallized]`                     |
| Memory Archived                                      | `[Memory, Archived, Contextual, Historical]` |
| Other recognized custom root or category             | `[Workspace]`                                |

These tags aid selection. They do not establish acceptance or add loading authority. A maintainer may refine prefilled values later. An existing valid tag list stays byte-exact, including its order.

Outside content always uses the generic `[Workspace]` defaults. Registered directories named `directives`, `memory` or `skills`, including their nested scope and state folders, acquire no primary category semantics. An outside `SKILL.md` is ordinary Markdown.

### Form and preservation

Read both supported forms through the existing metadata reader. Write absent fields in the form selected by `.agents/open-forge.json`, defaulting to scoped when the setting is absent. A partial header already in the selected form is completed in its existing mapping. Do not convert the file or add a second declaration of an existing field. Existing scoped precedence and dual-location `applyTo` validation remain unchanged. A partial header in the other form is completed in its existing declaration form, because inserting the missing fields in the workspace form would not be read together with the supplied ones and moving supplied fields would break byte preservation. All supplied bytes stay exact. New headers and new entrypoints still use the workspace form. No reader merge or form conversion is added. (Overseer decision, 2026-10-13.)

For a plain note, insert a leading block after any UTF-8 BOM, followed by one blank line, then the original bytes. Use its first LF or CRLF convention, defaulting to LF. For a partial header, use parser-owned YAML spans to insert fields without rewriting existing keys, values, comments, indentation, unrelated fields or body bytes. Flow mappings use parser-provided insertion boundaries and retain existing bytes. Validate the resulting metadata and all untouched spans before admitting an effect. An unsafe insertion boundary uses existing `index.metadata-unsafe`, not a guessed rewrite.

```yaml
---
open-forge:
  description: Deploy checklist
  tags: [Guidance]
---
```

In root form, emit the same two fields without the `open-forge` wrapper. `--dry-run` shows inserted fields and the Entries changes, then `No files were changed.` Applied per-file text is `<path>  metadata completed, your content was kept`. Preview text is `<path>  metadata would be completed, your content would be kept`.

### Malformed input

A readable malformed ordinary frontmatter block stays byte-exact. Emit exactly one `index.metadata-skipped` finding per physical source:

```text
<path> has invalid frontmatter. It was left unchanged. Other files were indexed.
```

Preview substitutes `Other files would be indexed.` Include the parser cause at standard detail without another finding. Exclude that source's row from the expected generated projection until its metadata is valid. Do not retain stale Entries as authoritative metadata or remove the file. Valid siblings in the same parent still get their rows. A malformed entrypoint keeps its own generated region untouched, and dependent projection through it remains unavailable. Independent branches continue. Count skipped sources once and list dependent limitations separately.

This narrow exclusion makes Index `incomplete`, exit 3, even if no independent write is needed. It replaces the old malformed-only block and the requirement for an independent changed region before skipping. Native Skill validation, unreadable inputs, unsafe identities and ambiguous generated boundaries retain their strict existing limits. Next is `Fix the frontmatter in <path>, then run open-forge index.` Doctor uses the same source and precise action.

## Entrypoint Chain Completion in Index

Find gaps only below an existing recognized root route in the selected closure. Each qualifying folder lies on a path to at least one eligible routed Markdown source. Empty folders and folders with only resources receive no entrypoint. Never create a root, Loader, removed category, excluded path or entrypoint inside a native Skill package. A missing recorded shared entrypoint remains a restoration boundary rather than permission to recreate it.

A selected outside boundary can instead restore its missing canonical anchor from the registered descriptor when its ordinary directory is available. Its chain stops at that directory and never creates an ancestor above it. Existing compatibility entrypoints and ambiguity checks remain effective. Outside gaps retain full literal source IDs, including catalogue filenames. `WorkspaceRoutePath` owns canonical path construction. Proposed outside entrypoints receive protection and physical containment checks before composition, and formation never adds an outside Loader row.

Preserve one existing canonical or compatibility entrypoint. Two recognized entrypoints or an occupied proposed identity block the selected plan. Derive the missing canonical path as `_{folder-name}.md`. Use the folder's sentence-case name as title and `Notes in <folder-id>` as description. Tags use the table above, except that a new Directives entrypoint uses `[Directive]` without `LoadNow`. A child scope remains on demand under the existing Directive rules. Omit responsibility and applicability. Reuse Route Init's scaffold composer with these explicit metadata facts:

```md
---
open-forge:
  description: Notes in memory/crystallized/platform/payments
  tags: [Memory, Crystallized]
---

# Payments

Notes in memory/crystallized/platform/payments.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Notes in memory/crystallized/platform/payments/stripe](stripe/_stripe.md) - #Memory #Crystallized
```

Root-form emission removes only the wrapper. The created file already contains its final projected Entries. Do not create an empty scaffold and then replace it. Index uses no `NeedsAuthoring` draft tag or advisory authoring ritual. Explicit Route Init keeps its current draft defaults and explicit restoration authority.

Plan metadata first, form intended source membership, then project direct-child navigation. Application orders entrypoint creates by depth from outer to inner, ties ordinally by canonical path, then ordinary metadata replacements and existing generated-region updates in ordinal path order. Coalesce metadata and Entries edits to one existing file into one intended whole-file replacement. Create directories only if actually missing, using existing lease-bound directory mechanics.

For a renamed folder with compatibility `index.md`, preserve that entrypoint. When a folder renamed from `billing` to `invoices` still contains its old canonical `_billing.md` and has no `_invoices.md`, and rename evidence proves that `_billing.md` is the old `billing/_billing.md` (committed content identity or Git removal with at least 90% unchanged content), Index renames it to `_invoices.md` and refreshes its Entries. Its authored description, Axioms and body stay exact, so the folder keeps the rules it had before the rename. The rename is one Create of the new canonical file and one Delete of the old file in the same plan, under the existing expected-state and recovery rules, and links to the old entrypoint are repaired as a proven move. Without that proof, Index creates `_invoices.md` as a new scaffold and leaves `_billing.md` as an ordinary file. Doctor then asks the user to copy any wanted rules into the new entrypoint before deleting the old file and running Index. Before the new file exists, Doctor offers renaming the old file to the canonical name or deleting it.

Exact headline when completion occurs: `Created <K> entrypoints and completed metadata in <N> files.` Preview: `Would create <K> entrypoints and complete metadata in <N> files.` Name only the non-zero parts, with singular forms for one, list every created path as `<path>  entrypoint created` or `<path>  entrypoint would be created`, and retain the existing per-region Entries rows. If only navigation changes, keep the existing headline. A repeat with no remaining limitation uses the existing current/no-op headline.

A proven canonical navigation rename contributes to `navigationFilesRenamed`, separately from `entrypointsCreated`. Its headline is `Renamed <R> navigation files.` or `Would rename <R> navigation files.`, with singular forms for one. Join this part with any non-zero creation and metadata-completion parts. Show its single rename row instead of a creation row, preserving the authored rules.

## Rename and Move Evidence

### Observation model

An immutable observation identifies the missing canonical old path, candidate canonical new path, evidence basis, source snapshot or committed blob identity, content hashes, similarity where applicable, old metadata signature, current candidate expectation and missing-old-path expectation. Candidates must be ordinary readable contained Markdown outside managed content, packages, overwrites, excluded paths and private records. Deduplicate proven physical aliases before cardinality. A candidate may sit behind a gap that Index can safely complete.

Read Git facts without mutation or network access through `GitProcessRunner`. Probe the selected workspace, resolve its contained repository prefix, read HEAD identity, tracked paths and HEAD blobs, and compare current candidates including untracked destinations. Use NUL-delimited path output and argument lists. Do not refresh the index, stage, commit, fetch, persist a rename cache or select a Git root as the workspace. Retain bounded invocation-only bytes and observations. Each Git invocation has its own five-second deadline. After the linear content read, evidence evaluation has its own five-second deadline. Cancellation kills and drains the child process. A timeout admits no proof, is reported and retries the complete request, while other unavailable optional Git evidence leaves metadata proof and ordinary maintenance available.

Normalize CRLF to LF for content identity and similarity. Keep BOMs and final unterminated lines significant. Retain original bytes and hashes for snapshots and expected-state checks. A current file whose normalized content equals its own tracked HEAD content existed before the rename. Exclude it from committed-content and near-identical proof, including strong-candidate cardinality. Keep it in the complete metadata uniqueness universe and retain its old identity for contradiction checks. Changed tracked files and untracked files remain content candidates.

### Strong evidence

There are three admitted bases:

1. **Committed content identity.** The old path is missing now, was tracked in HEAD, and exactly one admitted successor candidate has the same complete normalized SHA-256 content as that HEAD blob. Non-empty authored content is required. Identical boilerplate or empty files whose authored bodies contain no non-whitespace content are not identity proof.
2. **Git removal with near-identical content.** Git establishes that the old tracked HEAD path is now missing. Compare its HEAD blob directly with every plausible successor, including untracked destinations. Tokenize strict UTF-8 into complete lines, normalizing CRLF to LF and retaining the final unterminated line. Compute the maximum byte weight of an order-preserving common subsequence of identical normalized lines. The score is `floor(100 * 2 * commonBytes / (oldBytes + newBytes))`, using normalized byte lengths. Require at least 90, non-empty authored content and at least one identical non-whitespace authored line. A score of 100 still requires normalized SHA-256 equality before saying `same content`. Compare hashes first. A multiset count of common lines provides an upper bound that can reject pairs below 90 before dynamic programming. Intern lines into integer IDs for the exact weighted subsequence comparison. The optional scored comparison is bounded to 1 MiB of original bytes and 2,000 lines per side. Beyond that boundary, retain exact hashing and metadata evidence but do not guess a score. This pure comparison uses invocation-only bytes, adds no temporary files or Git objects and does not stage a destination. A Git-reported rename may narrow candidate discovery, but its score alone does not authorize a repair.
3. **Recorded metadata identity.** A valid stale Entries row, or old metadata read from HEAD, binds the missing path to a signature of description, ordered tags and normalized declared `applyTo` set. The candidate has exactly that signature. The signature must be unique across the complete admitted current candidate universe and the available old records. The candidate must not already have a different old identity in those records. Description must differ from the old filename-derived label and from a title-only label, and tags must be non-empty. This keeps title or filename guesses alone out of the strong lane. A generated record is evidence of former identity only, never the source of new metadata or current topology.

Weak candidate discovery reuses existing filename, title, independent literal-content and nearby-route facts. Nearby-route evidence alone does not nominate a replacement. The broken occurrence itself and its containing source are not weak successor evidence. Nearby relationships may explain an otherwise admitted candidate. This avoids sending a deleted-note link into a list of unrelated parent or source guesses. Strong source-move identity is a separate admitted observation.

All strong bases are combined before selection. Automatic repair requires exactly one candidate path with strong evidence and no contradictory mapping. Two strong candidates remain `guided-choice`, even if one has a higher score or stronger-looking explanation. Several weak candidates do not defeat one uniquely strong candidate. Several missing old paths competing for one candidate remain guided unless they are independently verified occurrences of the same old identity. Never infer a whole folder mapping from one child. A current generated row that names the successor is not a prior identity, so running Index before Repair keeps a Git-backed proof. Only a committed identity at the successor path contradicts committed-content or near-identical proof. A metadata-only proof still treats any other record at the successor path as a contradiction.

### Variants

| Situation                                                                     | Behavior                                                                                                                                                                                                                                                                                                                                  |
| ----------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| No Git, or Git unavailable, unborn or outside the selected workspace boundary | Use valid stale metadata evidence. Otherwise keep guided candidates or a precise manual edit. Ordinary Index and diagnosis still work. Unavailable optional Git evidence does not itself make Doctor incomplete                                                                                                                           |
| Git timed out                                                                 | Say `Git did not answer in time, so renamed files could not be proven. Run the command again.` Do not advise manual edits for links that may be renames. Next repeats the same request.                                                                                                                                                   |
| Index removed the stale row first                                             | Read old HEAD content or metadata. Without Git or an available old record, identity cannot be proven. Print the choice or manual action instead of inventing history                                                                                                                                                                      |
| File edited during its rename or move                                         | Admit a unique verified score of at least 90 or unique recorded metadata identity. Lower similarity alone remains guided                                                                                                                                                                                                                  |
| Across folders or roots                                                       | Compare the same complete admitted universe within the selected workspace. Compute each new relative destination from its actual containing source                                                                                                                                                                                        |
| Whole folder renamed or moved                                                 | Prove each missing target independently. Preserve compatibility entrypoints, complete canonical gaps and repair outgoing links in moved files as well as incoming links. A link broken by moving its source uses that source's proven old directory to resolve the former target, then validates the current target or its proven mapping |
| Deleted file with no proven successor                                         | Remove stale generated Entries through Index. Never recreate the note, pick a nearby file, or remove an authored link automatically                                                                                                                                                                                                       |
| Fragment no longer resolves after an edited rename                            | Preserve it only if verified at the candidate. Otherwise remain guided or use the existing unique fragment correction rule                                                                                                                                                                                                                |
| Candidate changed or vanished after preview                                   | Replan from fresh facts. Expected-state and evidence checks prevent applying the stale mapping                                                                                                                                                                                                                                            |

## Repair and Doctor

Link scope is owned by Framework Settings and uses `WorkspaceAllowList` path
comparison over retained source and resolved target paths. The operational
reference reader filters before candidate and graph formation and retains an
ignored count. It keeps parsed documents for rename identity, including ignored
sources. References does not consume this filter. Context and route scanners
use the same policy. No exclusion causes a filesystem walk. Doctor uses the
retained catalogue for known paths and point observations for remaining listed
paths. Route settings changes join the existing expected-state and recovery
plan, and fresh under-lease planning includes their original observation.
Settings remapping transforms each original entry once, selecting an exact
root-layer destination before a category folder-prefix destination. Leaf
base/overwrite destinations come from the resolved subject layers. The
operational reference reader retains an unrecognized settings version from its
existing settings observation for Doctor's informational projection, without
another settings read or filesystem walk.

Repair's automatic catalogue adds proven missing-target relinks and proven source-move corrections to existing exact path, case, encoding, fragment and Library residual corrections. A proven rename needs no target-choice prompt. Interactive Repair still shows the complete plan and asks its existing final confirmation. `--automatic` retains its no-prompt application behavior. No new flag is added.

A minimal proof row says `billing/rounding.md was renamed to billing/money-rounding.md, same content`. Cross-folder text uses `was moved to`. For near-identical content use `content is <S>% unchanged`. Metadata proof uses `recorded description and tags match`. Relative paths in these examples share a displayed route base. Full evidence carries canonical workspace paths.

Repair also refreshes all generated Entries and completes eligible metadata and gaps in the affected old and new route closures. Include the exposing parents and any missing ancestor chain. This is the shared Index maintenance planner consumed inside one Repair plan, not a second command invocation or a loop until clean. It prevents a successful relink from leaving doctor reporting stale navigation or an unreachable moved target. It does not expand Repair to unrelated maintenance elsewhere in the workspace.

Defer the old destination's generated Entries closure while any unselected broken authored occurrence still points at that missing path. Its stale row may be the only identity evidence for the remaining choices. The last relink refreshes that closure. Fresh authored reference observations recompute the deferral under the lease, without persisting evidence. Listed candidates expose their complete relink commands in the existing structured finding `actions`, with the request's explicit workspace. Human text still prints each command once beneath its candidate label.

Assemble relinks against original source spans, then compose completion and navigation on those intended bytes. One physical file receives one final file effect and one recovery entry. All selected relinks and affected navigation share preflight, confirmation, lease, recovery and verification. A required affected navigation safety failure blocks the combined plan before writing. Readable malformed metadata uses Index's explicit skip rule only when independent selected effects do not depend on it, and remains visible as incomplete work.

Doctor remains read-only. It consumes shared eligibility and rename facts, without calling the mutating planner or creating persistent evidence. Each finding's explanation says what is wrong, and its action line says what to do. No sentence appears in both. Print these exact texts. `<cause>` is the validator's own sentence, such as `The frontmatter block is not closed.`

| Condition                                   | Explanation                                                                                                               | Action line and resolution                                                                                                           |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| Eligible missing fields                     | `<path> needs a description or tags. Index can fill the missing fields and keep your content.`                            | `targeted-operation`, `open-forge index`                                                                                             |
| User-owned `SKILL.md` missing native fields | `<path> needs its Skill name or description. Index can fill them from the folder name and heading and keep your content.` | `targeted-operation`, `open-forge index`                                                                                             |
| File outside every routed folder            | `<path> is outside the folders Open Forge routes.`                                                                        | `manual-decision`, `Move <path> into a category such as .agents/guidance/, or add its folder to .agents/loader.md.`                  |
| Eligible chain gap                          | `<path> is inside folders without navigation files. Index can create the missing chain.`                                  | `targeted-operation`, `open-forge index`                                                                                             |
| Proven rename or move                       | The proof row above, then `Repair can update the links and affected Entries.`                                             | `safe-exact`, `open-forge repair --automatic`                                                                                        |
| Several strong candidates                   | `<old-path> is missing. More than one file looks like its new location.`                                                  | `guided-choice`, `open-forge repair`                                                                                                 |
| Only weak candidates                        | `<old-path> is missing. Its new location could not be proven.`                                                            | `guided-choice`, `open-forge repair`                                                                                                 |
| No candidate, including deletion            | `<old-path> is missing and no replacement was found.`                                                                     | `manual-decision`, `Update or remove this link in <source-path> at line <line>, column <column>.`                                    |
| Malformed ordinary frontmatter              | `<path> has invalid frontmatter. <cause>`                                                                                 | `manual-decision`, `Fix the frontmatter in <path>, then run open-forge index.`                                                       |
| Old navigation file left behind             | `<path> looks like this folder's old navigation file.`                                                                    | `manual-decision`, `Rename <path> to <canonical-name> to keep its rules, or delete it, then run open-forge index.`                   |
| Old navigation file beside a new one        | `<path> looks like this folder's old navigation file.`                                                                    | `manual-decision`, `Copy any rules you want to keep from <path> into <canonical-path>, then delete <path> and run open-forge index.` |

A link inside a generated Entries region whose target is missing is reported once, as the stale entry Index removes. It is never an authored link to edit.

Retain the existing managed Update action, native Skill authoring action and excluded-route restoration boundary. An incomplete Doctor result starts with `Doctor could not finish <K> checks.`, using the singular for one or `Doctor could not finish.` when the count is unavailable. It never says `No problems found.` Never recommend Index for an unknown root, intentional removal, duplicate entrypoints, unsafe leaf or unavailable required input that Index cannot resolve.

Choose one result-level next action deterministically: existing safety or recovery boundary first, then safe Repair, Repair's listed copyable choice, Index for remaining eligible completion, guided Repair, exact manual edit. When Repair cannot ask and lists relink commands, Next is `Choose a target above, then run its command.`, even if Index work remains. The chosen explicit relink completes eligible metadata, chains and generated Entries in the affected old and new route closures in the same plan. Its fresh Next uses the normal order, with Index for unrelated work or Doctor when nothing remains. Doctor prints no Next when nothing remains, because it never points to itself. Commands preserve explicit workspace coordinates and the flags needed by the next action. Do not carry `--automatic` into a guided retry or carry `--dry-run` into an application action. Preview of safe repairs points to `open-forge repair --automatic`. Guided Repair without listed commands points to `open-forge repair`.

After an applied operation, use fresh observations to select Next. If nothing remains, print `Next: open-forge doctor`. Following printed actions in order must reach Doctor with complete coverage and no warning or error findings, assuming required reads become available and the user makes the named choices or edits. Informational ownership or recovery observations are separate from health. Deleted references and malformed authoring require explicit human edits. No command promises to decide their meaning. Tests must exercise the entire continuation, including those edits.

Repair applies authored-link changes only to admitted user-owned Markdown sources and targets. Managed authored content, overwrites, excluded or private contents, and actual unsafe link or Library projection leaves remain protected during diagnosis, selection, planning and revalidation. Repair's managed findings point to their managing Update. Doctor retains each protected local-reference finding's identity and names its owner for manual correction. Ownership alone does not establish that Update repairs a link. Lifecycle restoration findings remain independent. Library residual corrections retain their separate recovery authority.

When Repair cannot ask, list each candidate once under its own link, with the strongest proof or readable reason and an indented, unwrapped relink command. Keep explicit workspace coordinates in every command, including sentence-embedded instructions. A partial application names verified effects before remaining choices or edits. An incomplete preview with independent safe work points to applying that plan before a malformed-file edit. A renamed navigation file is one row, and a newly created entrypoint has no duplicate Entries row.

## Safety and Wire Inventory

Every write uses existing plan and mutation primitives. Capture settings, ownership, old absence, candidate bytes, source occurrences, generated boundaries and Git HEAD expectations during planning. Revalidate under the existing workspace lease and immediately before the effect. Recompute strong candidate cardinality under the lease. Git evidence can be unavailable without blocking unrelated maintenance, but loss of evidence selected as proof invalidates that relink. No evidence reader grants filesystem or lifecycle authority.

All existing-file replacements use one immutable verified recovery bundle for the complete operation before its first covered effect. Ordinary entrypoint Create and directory Create retain existing preparation-free semantics. No-op and preview create no lock, bundle, staging file or history. Recovery failure, interruption, retained verified effects and cleanup follow existing monotonic rules. No automatic rollback is added. Prior bytes include uncommitted authored content, not merely Git HEAD. Successful whole-operation verification disposes of only the command's own bundle under the current shared rules.

The schema-3 envelope, exits, streams and existing members remain. Add fields only. Existing `file`, `section`, `link` and `directory` effect kinds suffice. Native changes remain `Create`, `Replace`, `ReplaceGeneratedRegion` and directory creation. No rename, metadata or repair effect kind is added. Ordinary link receipts retain `rewritten` and Library recovery keeps `restored`, file receipts use `created` or `rewritten`, and Entries receipts use `section`/`rewritten`.

| Finding code                                                                                                                                                         | Change                                                                                                                                                     | Severity and result                                 |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| `index.metadata-optional`                                                                                                                                            | No longer emitted for eligible completion. Retained only for excluded ordinary missing metadata that is still projectable                                  | Existing warning and exit 2                         |
| `index.metadata-skipped`                                                                                                                                             | One finding for any readable malformed ordinary source, including malformed-only invocation and same-parent independent work, with the exact wording above | Error, `incomplete`, exit 3                         |
| `index.metadata-unsafe`, `index.metadata-incomplete`, `index.target-changed`, `index.target-changed-during-apply`, `index.target-unsafe`, `index.topology-ambiguous` | Existing safety codes cover added targets. No wording or value additions                                                                                   | Existing status mappings                            |
| `route.metadata-required-missing`                                                                                                                                    | Keep code. Eligible files now use completion text and targeted Index action                                                                                | Warning, Doctor exit 2                              |
| `route.unreachable`                                                                                                                                                  | Keep code. Eligible gaps now use chain-completion text and targeted Index action                                                                           | Warning, Doctor exit 2                              |
| `workspace.frontmatter-malformed`                                                                                                                                    | Keep code. Use precise edit-and-index text, one finding per malformed source                                                                               | Existing error severity, completed diagnosis exit 2 |
| `reference.target-renamed`                                                                                                                                           | New. Replaces target-missing for one proven occurrence and carries exact proposal                                                                          | Warning, `safe-exact`, Doctor exit 2                |
| `reference.target-missing`                                                                                                                                           | Keep code. Filter neighborhood-only and self-occurrence guesses, then distinguish several strong, weak-only and no-candidate cases using the text above    | Existing warning, guided or manual lane             |
| `repair.rename-proven`                                                                                                                                               | New. One informational proof per old/new mapping in preview and verified application                                                                       | Info, does not raise status                         |
| `repair.guided-finding-remaining`                                                                                                                                    | Keep code and counts. Listed copyable choices point to choosing a target before Index                                                                      | Existing warning, exit 2                            |

New Index data arrays at every detail are `metadata` with `{path, fields, description, tags, outcome}` and `entrypoints` with `{path, outcome}`. `fields` contains only `description` and/or `tags` in that order. `description` and `tags` report only inserted values, otherwise null. Existing Entries `changes` remains Entries-only. Existing `filesChecked`, `filesUpdated` and `filesCurrent` continue to describe inspected existing generated regions, without counting ordinary metadata files or newly created entrypoints. Canonical destinations retain the existing `entrypoints` array. Add counts `metadataCompleted`, `entrypointsCreated`, `filesSkipped` and, after those existing additions, `navigationFilesRenamed`. `entrypointsCreated` counts new scaffolds and excludes canonical renames. `navigationFilesRenamed` counts proven canonical renames. File effects retain the actual Create and Delete pair. Applied counts count verified effects, preview counts count planned effects and are identified by the existing mode. No skipped source counts as completed.

Repair adds `renames` with `{oldPath, newPath, evidence, similarity}` and `navigation` with `{path, before, after}` at every detail. `evidence` has exactly `git-rename`, `committed-content` or `recorded-metadata`. `similarity` is an integer 90 to 100 for Git similarity, otherwise null. Full and debug detail add `proof` to each rename row with `{head, oldBlob, oldHash, newHash, metadata}`. The first three are nullable lowercase object/hash strings when available, `newHash` is the current lowercase SHA-256, and `metadata` is null or `{description, tags, applyTo}` with the exact recorded description, ordered tags and normalized pattern list. Minimal and standard omit `proof`. Other unavailable optional Git stays an internal observation and adds no wire enum or warning by itself. A timeout is a reported limitation. Doctor marks reference coverage incomplete, while Repair preserves unresolved rename proposals and retries. Add `entriesUpdated` for verified affected regions, or planned regions in preview. Existing repair counts continue to count link occurrences, not entrypoint or metadata writes. Add the same `metadata` and `entrypoints` arrays and completion counts when the affected closure requires them.

Outcome values reuse `planned`, `done`, `not-started` and `unknown`. Internal action projections add `metadata-completed`, `entrypoint-created` and `entries-updated`, without replacing existing values. This table and the finding table are the exhaustive Task 79 additions. No persisted schema, receipt attribution vocabulary, command name, flag, exit, status, severity or resolution lane is added.

## Placement and Evidence

Promote neutral entrypoint composition and intended-chain planning from Route Init to `Commands/Shared/RouteScaffolding`, outside the Route command, so Route Init, Route Create and the shared maintenance planner consume it without importing another command's private Shared namespace. Keep Init's draft and Framework-restoration policies in Init. Put Index and Repair's completion orchestration under `Commands/Shared/RoutedMaintenance`, with direct immutable requests and facts. Put missing-field byte insertion mechanics under `Framework/Documents/Metadata`, using existing YAML and Markdown parsers. Native Skill completion retains its local field policy.

Put neutral rename observations beside `Framework/Sources/Operational` reference facts. Root composition owns Git process execution and injects observations. Doctor and Repair consume the same evidence interpretation. No runtime registry, generic mutation engine, new dependency or persistent identity database is needed.

The [Task test and slice plan](../../../../archived/cli-development/tasks/task79-forgiving-maintenance.md#ds1-end-to-end-test-plan) defines owned production paths, cheap evidence and complete published-process journeys. It includes changed F03, F05 and F08 and new F29 to F31, independently checked bytes, expected-state drift, recovery, additive JSON and continuation to clean Doctor.
