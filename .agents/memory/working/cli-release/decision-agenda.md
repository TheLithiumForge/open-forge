---
open-forge:
  description: Accepted CLI direction, Gate 3 Architecture decisions, and Gate 5 evidence boundary
  tags: [Memory, Working, CLI, Release, Gate, Decision, Discussion, Contextual]
---

# CLI Decision Agenda

Unchecked Decision items are **discussion only — unaccepted**. Checked items
record maintainer direction already accepted in this program. The unchecked
items under **Gate 5 executable evidence** are proof obligations, not open design
questions. Check a Decision only after the maintainer accepts the choice and any
warranted focused Decision and current source updates are complete.

Gate 3 Architecture and Gate 4 closeout are accepted and current. Gate 4 has no
active pending item. Gate 5 is the active next gate, but it is not accepted or
started; its unchecked decisions and evidence remain unaccepted proof or
implementation obligations.

## Accepted Direction

- [x] **CLI-D001 — Product boundary:** Optional agent-first Framework accelerator
      with a usable human maintenance surface. There is no target command count.
      Every command and flag must justify its value and permanent cost.
- [x] **CLI-D002 — Intended users:** Agents are primary. Humans receive the same
      predictable interface for occasional maintenance and direct use.
- [x] **CLI-D032 — Canonical implementation:** C# on .NET 10 or newer with Native
      AOT. Every dependency, serialization path, and runtime feature must be
      Native AOT and trimming compatible. The accepted concrete Architecture
      decisions are recorded in CLI-D096 through CLI-D103; Gate 5 still owes
      executable proof.
- [x] **CLI-D057 — Thin-wrapper boundary:** Wrappers install or invoke the
      canonical executable and never implement Framework behavior.
- [x] **CLI-D058 — First release channel:** npm is the first thin wrapper. Later
      channels require their own evidence and support decision.
- [x] **CLI-D064 — CLI-v2 knowledge:** Preserve it under
      `.agents/memory/archived/cli-v2/` as raw historical input. Reuse requires
      current evidence and maintainer acceptance.
- [x] **CLI-D066 — Rune boundary:** Rune is entirely outside this CLI effort and
      will be reconsidered only after the CLI is complete.

The root `package.json` remains an ecosystem-neutral orchestration layer. CLI-D103
fixes the launcher and platform package identities; Gate 5 proves their package
placement and journeys.

## Product Decisions

- [x] **CLI-D003 — Core jobs:** The CLI will provide quick workspace status;
      focused route profiling; ordered context reading; exact tag discovery; route, link, metadata,
      overwrite, and generated-navigation checks; safe link repair; route
      initialization, routed-file creation, and bounded route updates; explicit
      move and remove operations; direct Framework management establishment
      through `install` and trusted managed reconciliation through `update`,
      including bounded initial force, current-footprint force, and retired-
      content prune boundaries; the six-leaf Extension family `list`, `inspect`,
      `create`, `install`, `update`, and `remove`; and cleanup of recognized CLI
      recovery and temporary artifacts. Direct Markdown work remains valid.
- [x] **CLI-D004 — Non-goals:** Do not add semantic or fuzzy search, automatic
      relevance or intent inference, receipts or session state, a serializable
      batch-apply interface, shell-profile editing, automatic package-channel
      expansion, or Rune integration. Do not build broad workarounds for
      operating-system, filesystem, runtime, or tool defects unless they affect
      a critical Open Forge guarantee.
- [x] **CLI-D005 — Full retained CLI delivery:** every command retained by an
      accepted product disposition is an implementation target. Do not publish a
      partial `context` plus `find` slice or retain a later release slice. Treat
      the CLI as delivered or released only after every retained accepted command
      is implemented, verified, packaged, and covered by the complete release
      proof. Implementation may still proceed through small coherent, testable
      Tasks in logical dependency order; Task order remains a fluid plan that
      each deep Preflight may move when evidence shows a better dependency order.

      With D018 rejected, the full-delivery target is exactly all retained current contracted commands and excludes completion.

Every operation is stateless and deterministic for the same workspace bytes and
explicit input. A read returns the same semantic result. A write normally
converges on one state, and a repeated successful write reports a verified no-op
when the operation can independently prove that its intended state already
holds. The accepted `route move` exception is honest consumed-source behavior:
repeating a move with its consumed old source returns a non-mutating exact
`source-not-found`/`invalid` result rather than claiming provenance. `route remove`
reports a verified no-op for a repeated request only when complete trusted
ownership, topology, recovery, and reference evidence independently proves the
exact intended absence; otherwise it remains missing, invalid, incomplete, or
blocked as applicable. `cleanup` is a narrow monotonic deletion exception:
each invocation forms a fresh recognized catalogue, and a successful repeat is
a verified no-op when no eligible artifact remains. Cleanup does not create
replacement recovery effects or reverse deletions it already verified; an
unsafe later item remains visible for a fresh invocation.

Implementation Tasks are incremental internal work, not separate publication
targets. The full-delivery sequence and its readiness conditions are recorded in
the accepted program decisions below.

## Command Decisions

- [x] **CLI-D006 — Context command:** use `context [route...]`. With no route, it
      returns the startup-required closure. Explicit routes add their closures.
      `--additions-only` removes sources already present in the startup closure.
      Route operands are separate shell values rather than a comma-separated
      flag. The operation remains stateless and creates no session state. The
      [Context Interface](../../crystallized/documents/cli/contracts/context/interface.md) defines the complete accepted
      interface.
- [x] **CLI-D007 — Orientation:** provide a separate quick `status` operation.
      It compares shipped and current startup context, reports total and
      continuity context size, identifies the largest continuity sources, shows
      root-category customization, and summarizes Extensions, managed files, and
      recognized recovery files. It does not count scopes by guessing their
      meaning, diagnose complete health, or recommend changes. The
      [Status Interface](../../crystallized/documents/cli/contracts/status/interface.md) defines the complete accepted
      interface.
- [x] **CLI-D008 — Discovery:** provide tag-token and structural-heading
      discovery through one flat `find` source inventory. Bare `find` lists the
      inventory. Repeatable `--tag` and `--heading` predicates narrow it;
      `--require=all|any` combines predicates, with omitted `--require` remaining
      flat `all` and explicit `any` available; no grouping or explicit-mode
      requirement is added. Repeatable scalar
      `--include=<source-reference>` and `--exclude=<source-reference>` filters
      select the physical source area: omitted include uses the complete eligible
      `.agents` Markdown universe and omitted exclude subtracts nothing, so
      normal usage is unchanged; includes union, excludes union, and exclusion
      wins overlap. Folder-root references (`Loader`, recognized `entrypoint`, or
      `SKILL.md`) expand to their physically contained eligible Markdown
      subtree, including unrouted Markdown; ordinary references select one
      logical source, with base/overwrite remaining one logical source. This is
      physical area selection, not route, lifecycle, tag, or authority
      inference. Complete coverage is relative to the explicit effective
      universe; no `--limit` or count cap is accepted. `--within` selects
      `document`, `frontmatter`, `body`, or exact sections; `--view` selects
      compact or expanded human results; and `--content` reuses the context
      projection grammar. Complete tag and heading values match while ignoring
      case through ordinal comparison. Do not add general text, partial,
      regular-expression, fuzzy, synonym, semantic, vector, or relevance-ranked
      search. The [Find Interface](../../crystallized/documents/cli/contracts/find/interface.md) defines the
      complete accepted interface.
- [x] **CLI-D009 — Inspection:** use
      `route inspect <source-reference>` for one source's route profile without
      returning authored content. It explains in ordinary language when the
      source is read, measures its own source, context added by selection, and
      descendants read through `#LoadNow`, reports route chain and route
      structure facts, identifies inherited Axioms provenance and the overwrite
      relationship, and never invents a scope count or heaviness score. `context`
      continues to return selected content and explicit link expansion;
      `doctor` provides diagnosis and recommendations. The
      [route inspect Interface](../../crystallized/documents/cli/contracts/route/inspect/interface.md) defines the
      complete accepted interface.
- [x] **CLI-D010 — Validation name:** use `doctor` for complete structural
      validation, findings, and recommendations. `status` remains quick
      orientation and does not absorb diagnosis. The accepted [Doctor Interface](../../crystallized/documents/cli/contracts/doctor/interface.md)
      defines six fixed diagnostic domains, read-only behavior, coverage,
      findings, resolution lanes, and typed next actions. Doctor does not
      repair or prompt.
- [x] **CLI-D011 — Repair boundary:** support a separate `repair` operation
      when one exact, meaning-preserving local-reference correction is known.
      Repair uses one atomic plan, fresh six-domain diagnosis, complete-diagnosis
      write gating, dry-run parity, Git and recovery checks, verification, and
      fresh post-diagnosis. It never authors content, chooses ownership, or
      mutates generated navigation, route intent, recovery, Framework, or
      Extension lifecycle. Guided missing-target relinks use bounded filename,
      title, literal-content, and route-neighborhood candidates, but never a
      semantic or fuzzy automatic choice. The [Repair Interface](../../crystallized/documents/cli/contracts/repair/interface.md)
      defines the exact syntax, automatic and explicit-relink selection,
      wizard, catalogue, and targeted boundary.
- [x] **CLI-D012 — Index maintenance:** retain an explicit generated-navigation
      rebuild operation and run the same rebuild automatically after every CLI
      write that changes routing or indexed metadata. Use `index` as the direct
      command name. It matches the current Loader and generated-index vocabulary,
      applies to first or repeated generation, and avoids implying a broader
      Framework rebuild. The [Index Interface](../../crystallized/documents/cli/contracts/index/interface.md) defines
      the complete accepted interface.
- [x] **CLI-D013 — Route initialization:** use `route init <route-target>` to
      recursively create missing entrypoints with one fixed canonical scaffold.
      New entrypoints use `_{folder-name}.md`, a literal slug title, an honest
      description, the inherited Axioms sentinel, and a valid final generated
      `Entries` region. Missing semantic authoring remains visible through
      `NeedsAuthoring`; optional description, responsibility, and tag flags
      affect only a missing final target. Existing compatibility entrypoints are
      preserved. The command never uses a Template or creates the Loader. The
      [route init Interface](../../crystallized/documents/cli/contracts/route/init/interface.md) defines the complete
      accepted interface.
- [x] **CLI-D014 — Routed-file creation:** use
      `route create <file-target>` to create one ordinary routed Markdown file
      below an existing entrypoint. Destination description and tags are
      explicit; responsibility is optional. `--template` accepts one Template ID
      or exact path and copies only its body. Template metadata and continuing
      ownership do not transfer. The [route create Interface](../../crystallized/documents/cli/contracts/route/create/interface.md)
      defines the complete accepted interface.
- [x] **CLI-D014A — Routed-source update:** use
      `route update <source-reference>` as a field patch for description, tags,
      and optional responsibility. `--responsibility ""` removes that key. A
      selected Template may add its body only when the target is frontmatter
      plus whitespace; any authored body remains byte-for-byte unchanged. The
      command preserves compatibility filenames and plans affected generated
      navigation in the same mutation. The [route update Interface](../../crystallized/documents/cli/contracts/route/update/interface.md)
      defines the complete accepted interface.
- [x] **CLI-D015 — Structural mutation:** retain
      `open-forge route move <source-reference> <destination-target> [--dry-run] [--skip-git-check] [global flags]`
      and
      `open-forge route remove <source-reference> [--dry-run] [--skip-git-check] [global flags]`.
  - **Subjects:** each command accepts one eligible ordinary unmanaged logical
    leaf or category. A category is selected through its exact recognized
    entrypoint and includes the complete physically contained folder tree: its
    root entrypoint, overwrite companion, descendants, routed or unrouted
    Markdown, native or binary resources, ordinary support files, and every
    other regular contained file and directory. Every item must pass containment,
    identity, ownership, lifecycle, collision, and recovery classification. One
    incomplete or unsafe item blocks the whole operation. Generated regions
    remain projected navigation rather than authored authority. A category is one
    atomic plan and recovery/verification boundary, not independently committed
    leaf commands or a generic batch/apply surface.
  - **Ownership:** positive unmanaged proof requires a complete trusted
    lifecycle-ownership inventory from the Framework baseline and every
    applicable Extension receipt or manager claim, establishing that none claims
    any selected source or resource. Missing, malformed, conflicting, stale, or
    incomplete inventory blocks. A missing receipt, path, tag, generated entry,
    matching bytes, or familiar route does not prove unmanaged status.
  - **Destination:** a leaf destination is one exact ordinary routed file target
    under an existing valid route. A category destination is one exact destination
    entrypoint inside a new category folder whose parent is an existing valid
    route; it defines the category root and preserves descendant relative layout.
    Reject self-moves, destinations inside the source, aliases, collisions,
    overwrite conflicts, unsafe containment, and implicit parent initialization.
  - **Move references:** perform one complete physically contained
    supported-workspace-Markdown pass inside and outside `.agents`. Rewrite every
    exact supported resolvable local authored reference whose existing destination
    would no longer resolve to the same intended target after the move, including
    outside-to-moved, moved-to-outside, and needed internal references. Preserve
    labels, fragments, valid encoding, and unrelated bytes. Internal links that
    remain valid need no rewrite, and external URLs remain unchanged. Incomplete
    catalogue/inspection coverage or a potentially applicable unsupported or
    ambiguous reference produces no-write `incomplete`; unsafe identity remains
    `blocked`.
  - **Remove references:** perform the same complete pass. Detach each incoming
    exact supported Markdown link from outside the removed subject to its visible
    label as plain authored text while preserving surrounding prose. Surface every
    detachment in dry-run and final human/JSON results. References inside the
    removed subject disappear with it. Potentially applicable unsupported or
    ambiguous forms and prose-losing transformations produce no-write `blocked`;
    incomplete catalogue coverage produces no-write `incomplete`. Never silently
    leave a supported broken link.
  - **Safety and consent:** project affected parent navigation and the Loader when
    applicable; apply affected-path Git checks, expected-state revalidation,
    backup readiness, all-effects verification, and identity-guarded reverse
    recovery. Create no hidden Index subprocess, saved plan, receipt, tombstone,
    journal, or history. The command path and exact subjects provide consent in
    human, JSON, and non-interactive use. `--dry-run` is the only preview. There is
    no `--force`, `--automatic`, `--yes`, `--apply`, root move/remove, or batch
    operand; `--skip-git-check` bypasses only affected-path cleanliness.
  - **Repeats and authority:** a repeated move using its consumed old source is an
    exact `source-not-found`/`invalid` non-mutating result. A repeated remove is a
    verified no-op only after independent complete proof of intended absence and
    no orphan companion, residual reference, stale generated region, or recovery
    artifact. The [route move Interface](../../crystallized/documents/cli/contracts/route/move/interface.md), [route
    move Behavior](../../crystallized/documents/cli/contracts/route/move/behavior.md), [route remove
    Interface](../../crystallized/documents/cli/contracts/route/remove/interface.md), and [route remove
    Behavior](../../crystallized/documents/cli/contracts/route/remove/behavior.md) define the complete contracts.

- [x] **CLI-D016 — Framework lifecycle:** use the exact direct root forms
      `open-forge install [--force] [--automatic] [--dry-run] [--skip-git-check] [global flags]`
      and `open-forge update [--force] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]`.
      `install` establishes management in a safely absent or eligible initial
      state and verifies an exact trusted managed no-op. Existing managed
      changed, missing, retired, or source-divergent state is blocked and
      directs to root `update`; `install --force` may replace only an eligible
      exact initial occupant and never adopts its old bytes. `update` requires
      trusted existing Framework lifecycle state, applies baseline-unchanged and
      genuinely new safe content normally, uses `--force` only for changed or
      missing current expected content, and uses `--prune` only for eligible
      retired managed content. `--force --prune` composes those two boundaries.
      Automatic mode is interaction policy and never adds selection, replacement,
      restoration, deletion, adoption, ownership, or bypass authority. Both
      operations use one transparent baseline/current/intended plan, current
      Index projection, affected-path Git and adjacent-backup recovery,
      expected-state revalidation, verification, seven statuses, and one typed
      result. There is no `framework` group, root `init`, reinstall/replace/
      restore/recover alias, Framework uninstall/remove leaf, generic apply,
      saved plan, or semver package update. The accepted lifecycle path, schema,
      semantic identity, serialization, filesystem, result, recovery, and
      implementation boundaries are recorded in the accepted Gate 3 Architecture;
      Gate 5 owes executable evidence. The
      [Install Interface](../../crystallized/documents/cli/contracts/install/interface.md) and [Update Interface](../../crystallized/documents/cli/contracts/update/interface.md)
      define the complete public meanings.
- [x] **CLI-D017 — Extension lifecycle:** use exactly the grouped
      `extension` family with leaves `list`, `inspect`, `create`, `install`,
      `update`, and `remove`. `list` and `inspect` are read-only. `create` writes
      only a catalogue scaffold at its distinct `--path` destination and treats
      global `--workspace` as a no-op. Extension install and update use one exact
      embedded or explicit package/catalogue source universe, explicit IDs or
      `--all` where declared, offline transitive dependency closure, a trusted
      Framework anchor and route-host facts for mutation, transparent isolated
      lifecycle facts, semantic fingerprints, generated-navigation projection,
      explicit ownership, and complete recovery. Install establishes absent
      managed IDs or verifies an exact no-op; managed divergence directs to
      update and initial `--force` covers only an eligible exact occupant. Update
      uses `--force` for current expected replacement/restoration and `--prune`
      for eligible retired deletion. Remove needs no package source bytes,
      releases explicit trusted ownership, retains shared files, deletes safe
      unchanged final owners, preserves changed final owners by default, and
      accepts changed-final-owner Delete only through same-request `--prune`.
      Manual, idless, and direct-overlay content remains unmanaged. The accepted
      Gate 3 Architecture fixes the lifecycle path/schema, package identity,
      parser, fingerprint, serialization, filesystem, and recovery boundaries;
      Gate 5 owes executable evidence.
- [x] **CLI-D017A — CLI cleanup:** provide the direct root operation
      `open-forge cleanup [--dry-run] [--skip-git-check] [global flags]`.
      Cleanup has no operands, IDs, paths, selectors, wizard, prompt,
      confirmation, `--automatic`, `--force`, `--yes`, `--apply`, age filter,
      glob, saved plan, or cleanup profile. Bare cleanup discovers and removes
      every currently eligible recognized Open Forge transient or recovery
      artifact in the selected workspace, with the same domain behavior for
      human, JSON, TTY, and non-interactive use; `--dry-run` is the only preview.
      The catalogue includes target-associated adjacent backups, known `.bak`
      compatibility forms when positive identity is provable, operation
      temporary or staging files and directories, and residual recovery
      artifacts from incomplete or completed operations. Positive Open Forge
      provenance, bounded workspace association, and physical containment are
      required observable identity facts. A suffix, age, extension, location,
      proximity, or temporary-looking name is never enough. Unknown,
      user-created, ambiguous, aliased, externally resolving, active, in-use,
      and concurrently changing items remain untouched, as do repository
      `.temp/`, raw evidence or snapshots, source and managed content, lifecycle
      documents and receipts, generated navigation, build outputs, package
      caches, logs that are not positively identified as one of the listed
      cleanup artifact kinds, and arbitrary backups. Explicit cleanup command
      intent supplies consent to discard every currently eligible artifact,
      including backups the user no longer wants after manually preserving or
      migrating desired content; cleanup does not interpret that content or
      migration intent and does not need to prove a backup unnecessary for
      recovery. One complete catalogue and deterministic deletion plan precede
      effects. Identity, containment, inactive state, and expected bytes or
      physical identity are revalidated immediately before each deletion.
      Gitless workspaces are valid; affected-path Git cleanliness remains the
      default when Git can classify a path, and `--skip-git-check` bypasses only
      that check. Cleanup does not create a replacement backup, staging copy,
      receipt, journal, or tombstone merely to delete eligible cleanup artifacts
      and does not reverse verified deletions. Partial failure or interruption
      keeps deleted and remaining artifacts visible and rerun forms a fresh
      catalogue.
      The shared seven statuses and streams apply; planned deletions do not
      create `attention`. When no eligible artifact exists, cleanup returns a
      verified complete no-op without prompting. Cleanup never runs Doctor,
      Repair, Index, lifecycle, package, or Gate 6 cleanup as a hidden operation.
      The accepted Architecture fixes cleanup's storage, schema, process exits,
      recovery, and implementation boundaries; Gate 5 owes executable evidence.
      The
      [cleanup contract set](../../crystallized/documents/cli/contracts/cleanup/_cleanup.md) is the current local
      authority.
- [x] **CLI-D018 — Completion rejected:** reject shell completion from the
      product. There is no `open-forge completion` command, shell value set,
      generated-script output, profile editing, install/remove lifecycle,
      package-manager completion responsibility, completion library dependency,
      custom generator, second runtime, or future completion implementation
      target. Normal CLI usage is unchanged. The rejected script-only candidate
      and its evidence rationale remain only in [Queue 33 contextual history](../../archived/cli-release/review/completion.md).
- [x] **CLI-D019 — Interface style:** keep command paths shallow, flags stable
      and minimally aliased, inputs explicit, authority-bearing flags visible,
      groups limited to coherent families of actual operations, and human and
      structured behavior aligned. `install` and root `update` are separate
      direct Framework operations. `install --force` is only eligible initial
      occupant authority; root/Extension `update --force` and `--prune` widen
      their named current-expected or retired-content boundaries only.
      `extension` is a real six-leaf group. `--automatic` is operation-specific
      interaction policy, never authority. All authority remains separate from
      `--skip-git-check`; no force, prune, adoption, ownership, conflict,
      containment, marker, verification, or recovery bypass is implied. Use the
      ignored `predictable-cli-interface` Extension only as raw guidance. Exact
      flags and aliases remain command-local contract decisions.

## Contract-System Decisions

- [x] **CLI-D069 — Contract roles and authority:** each public operation receives
      one product-facing Command Interface Contract and one
      technology-independent Command Behavior Contract. Interface contracts
      define the complete public surface and observable result. Behavior
      contracts define deterministic semantics, effects, safety, recovery, and
      conformance without selecting implementation technology. Local Technical
      Designs remain subordinate to the accepted Architecture and cannot silently
      change either contract. The consolidated Command Contract Set and detailed
      contracts are now the current Crystallized source set; Gate 5 owes
      implementation proof.
- [x] **CLI-D070 — Contract completeness shape:** keep complete command syntax,
      operands, flags, required and optional values, defaults, incremental
      behavior changes, scenarios, human and structured output, statuses,
      errors, non-goals, and verification in the split command-local contracts.
      The top-level Command Contract Set is a concise Document, not a Pattern.
      Copy-ready Templates remain optional starters and do not become current
      contract authority.
      Every finite flag value and omission state receives explicit coverage and
      a representative result or output. Compatible flags compose without a
      Cartesian example catalogue; only non-additive interactions need their own
      scenario.
- [x] **CLI-D071 — Lossless contract migration:** group each command's Interface,
      Behavior, optional Technical Design, and directly related evidence in its
      own routed scope. Put genuinely shared contracts in the nearest useful
      parent containing all actual consumers. The fact-level ledger preserves
      the `e0f26fe` baseline and records the completed lossless migration. The
      split files are now the current Crystallized contracts, every old mixed
      flat source has been removed, and `contracts/shared/` is permanent. The
      public `index` and `references` files are finalized under
      `contracts/index/` and `contracts/references/`; recognized entrypoints are
      processed once by physical identity. Temporary Find and Index labels are
      removed, and no permanent requirement-ID system is created.
- [x] **CLI-D072 — Remaining accelerator discovery:** after the contract system
      is accepted and applied, investigate ordered context paths, incoming and
      outgoing references, route-neighborhood paths, check versus fix, graph
      views or queries, and other cheap agent-accelerator capabilities before
      accepting or rejecting each command or projection. The capability handoff
      and initial maintainer feedback complete the investigation; individual
      revisions and acceptance remain tracked below.
- [x] **CLI-D073 — Repeated command-specific Boolean flags:** Repeating
      `--dry-run` or `--skip-git-check` is accepted and idempotent. A second or
      later occurrence has no additional effect. This matches repeated Boolean
      global flags and does not multiply authority or bypasses. Value-bearing
      repetition rules are unchanged.
- [x] **CLI-D074 — Index output streams:** `complete`, `attention`, and
      `incomplete` primary human result rendering goes to stdout. `invalid`,
      `blocked`, `failed`, and `interrupted` primary human error rendering goes
      to stderr. Keep each primary human typed result together on its assigned
      stream. JSON remains one complete structured result on stdout for every
      semantic status. Separate bounded diagnostics remain stderr. Do not mix
      ordinary human text into JSON stdout.
- [x] **CLI-D075 — Index dry-run attention result:** A dry run with safely
      established planned changes and a non-blocking finding returns `attention`.
      It still exposes the complete plan and exact bounded diffs and states no
      files changed. Human output says `requires attention`.
- [x] **CLI-D076 — Discovery family:** make Discovery one prominent manually
      organized root-help and documentation section. Do not add a separate
      `discover` command. The accepted Architecture selects
      System.CommandLine 2.0.11 and explicit command-tree construction; Gate 5
      must verify the hierarchy and ordering without reopening this product
      direction. Domain operations retain their distinct route, source,
      reference, and context meanings.
- [x] **CLI-D077 — Route catalogue:** add the read-only `route list` operation
      for authored routed topology by exact root and relative depth. It uses
      compact and expanded human views plus the shared complete structured
      result. Operand-free selection uses every current root exposed by the
      exact Loader, including workspace-defined roots, but never emits the
      Loader as a row. Compact rows retain ID, path, authored description, and
      authored tags; expanded adds structural and provenance facts; JSON retains
      the complete result. It does not include unrouted files, semantic
      relevance or inference, graph queries, a result cap, or a `minimal`,
      `show`, or `display` mode. `CLI-D084` records the exact depth and filter
      boundary.
- [x] **CLI-D078 — Repair name, grammar, catalogue, and boundary:** use the
      canonical `repair` command with this exact public form:
      `open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [--skip-git-check] [global flags]`.
      Repair has no operands, generic proposal or choice references, finding
      dispatch, domain or kind modes, generic batch input, plugin fixer registry,
      `guided` child, `--yes`, `--preview`, `--suggestions`, `--all`, `--force`,
      `--apply`, saved plan, report, session, or receipt. `--automatic`,
      `--dry-run`, and `--skip-git-check` repeat idempotently; `--relink` takes
      exactly three shell values per occurrence, deduplicates identical tuples,
      and rejects contradictory tuples for one occurrence. Human bare `repair`
      opens the wizard; automatic selection includes only current safe-exact
      proposals; explicit relinks supply exact guided or exact intent; JSON and
      other non-interactive requests never prompt. The first catalogue is
      same-target canonical path, case, encoding, and unique fragment repair,
      plus user-selected missing-target relinks from bounded filename, title,
      literal-content, and route-neighborhood evidence. A strict complete
      six-domain diagnosis gate blocks every general Repair write unless all six
      Doctor domains have complete coverage. Repair uses one atomic plan and
      does not mutate external references, authored content or labels, generated
      navigation, route intent, recovery, Framework, or Extension lifecycle.
      The [Repair Interface](../../crystallized/documents/cli/contracts/repair/interface.md) and [Repair Behavior](../../crystallized/documents/cli/contracts/repair/behavior.md)
      define the accepted details. No future lifecycle command syntax is accepted
      by this item.
- [x] **CLI-D079 — Remaining agent accelerators:** add `headings` to the shared
      context content projections instead of adding an outline command. Compact
      human view lists the heading structure; expanded view adds levels, lines,
      source forms, and provenance. Keep provenance in expanded human and
      complete structured results while compact output stays token-friendly.
      Defer read-only batching until the CLI is complete enough to measure real
      compositions. Mutation batching remains outside this idea.
- [x] **CLI-D080 — Unified reference query:** use one read-only `references`
      operation for one selected source. The default result contains separate
      incoming and outgoing sections; `--direction=in|out|both` selects the
      sections, with omission meaning `both`. Do not create incoming and
      outgoing child operations. Compact human view lists references by level;
      expanded view adds directional source-to-target pointers, locations,
      resolution, and provenance. Direct one-hop behavior is the first
      boundary. Each requested direction retains its own coverage and status,
      and the invocation is complete only when every requested direction is
      complete. The current incoming universe, reusable filter authority, and
      unchecked external outgoing-link treatment are recorded in `CLI-D085`.
      A later depth option may produce a levelled tree only after direction,
      cycles, ordering, and completeness are specified.
- [x] **CLI-D081 — No route-neighborhood or graph operation:** use `route list`
      with bounded depth for structural neighborhoods and the reference
      operation for link relationships. Do not add a neighborhood or graph
      command. Reconsider bounded reference depth only if direct references do
      not satisfy repeated workflows.
- [x] **CLI-D082 — Global human result density:** provide global
      `--view=compact|expanded` with `expanded` as the default. Compact output is
      token-friendly but retains identity, order or hierarchy, status,
      completeness, safety, and required next actions. Expanded output adds
      explanations, evidence, provenance, and locations. `--json` returns the
      complete structured result and makes `--view` a no-op. `--verbose` remains
      diagnostic. Every well-formed global flag is a no-op when its meaning does
      not apply to the selected operation. The global contract has no third view
      or fallback contract, including no `minimal`, `show`, or `display` mode.
      Measured evidence may reopen this later.
- [x] **CLI-D083 — Behavior-first CLI evidence:** test semantic behavior,
      observable results, state transitions, safety, and complete journeys more
      heavily than exact prose. Use focused snapshots only when reviewing one
      stable projection is clearer, and use exact byte or text evidence when the
      bytes themselves are contractual, including embedded Framework assets.
      Prove embedded-asset identity once at the focused build or package boundary
      instead of repeating complete embedded prose in unrelated behavior tests.
      Keep an `index/_index.md` fixture that proves body loading, indexing, and
      diagnosis deduplicate one physical entrypoint.
- [x] **CLI-D084 — Route-list defaults and filters:** omitted `--depth` is `1`,
      including each selected root and its direct routed children. With no
      operand, the command selects every current root directly exposed by the
      exact Loader, including workspace-defined roots; the Loader is never a
      row. Explicit `--depth=0`, `--depth=N`, and `--depth=all` select roots
      only, descendants through `N`, and the complete descendant closure. The
      current authored topology and source contracts, not generated `Entries`,
      define membership and order. No own-tag or literal metadata filter is
      accepted in this version. Measured evidence may reopen that filter
      question later.
- [x] **CLI-D085 — Reference-query details:** use exact
      `--direction=in|out|both`; omission is `both`, and direct one-hop behavior
      is the first boundary. Without filters, incoming scans all eligible
      `.agents` Markdown. The shared [Source Universe Filters contract](../../crystallized/documents/cli/contracts/shared/source-universe-filters/interface.md)
      defines reusable operation-specific `--include` and `--exclude` flags,
      not global flags. Find and References explicitly apply them. For
      `references`, includes and excludes affect incoming work only, and either
      is invalid with `--direction=out`; unrelated commands reject them as
      unknown. Outgoing HTTP/HTTPS facts appear as `external-unchecked` with
      `network-not-attempted` and are never fetched. No-fetch alone does not
      weaken complete coverage. The public command is `references`, and its
      accepted files are finalized under `contracts/references/`. Recognized
      entrypoints are processed once by physical identity. The physical-identity
      regression remains Gate 5 evidence, not a current staging path.

- [x] **CLI-D086 — Guided leaves and explicit automatic preview:** apply the
      accepted [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md).
      A group performs no operation or wizard and its bare form shows
      help. A wizard-capable leaf normally exposes its wizard from the simplest
      useful interactive invocation, usually argument-less, unless its primary
      subject cannot be safely and finitely enumerated. Wizard answers and
      explicit inputs populate one typed request; they do not create another
      operation or hidden precedence mode. JSON and other non-interactive modes
      never prompt. An operation-specific `--automatic` is not global and may
      select only documented deterministic automatic inputs. It may execute
      ordinary safe effects already authorized by the explicit operation and
      subjects, including a safe unchanged final-owner deletion when the
      operation defines it; it never selects deletion of changed content on its
      own or adds recommendation, divergence, adoption, ownership, fuzzy-choice,
      safety-bypass, or conflict-bypass authority. `--dry-run` remains the sole
      preview spelling and shares request, facts, planning, and preflight with
      application while writing nothing. Future lifecycle Interface contracts
      must follow this Contract when they declare wizards without inventing their
      exact command names or syntax now, together with their command-local
      Interface and Behavior Contracts.

- [x] **CLI-D087 — Status refinement:** keep `open-forge status [global flags]`
      with no operands or Status-specific flags, the current measurements and
      sections, the seven semantic results, and the Status-versus-Doctor boundary.
      Primary human `complete`, `attention`, and `incomplete` results use stdout;
      primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
      stderr. JSON writes one complete result to stdout for every status, with
      separate bounded diagnostics on stderr; this refinement does not create a
      new shared contract. Expanded human output shows at most three largest
      continuity sources, compact omits them, and JSON carries every
      deterministically ordered contribution. Numeric-numeric differences are
      signed, including zero; unavailable operands produce unavailable derived
      values, and non-applicable comparisons produce not-applicable values. The
      startup percentage follows the accepted zero, unavailable, and positive-
      current/zero-total rules without rendering unavailable or not-applicable as
      zero. An absent lifecycle section shows `0 recorded` Extensions, while a
      trusted empty section shows `0`; both show `none recorded` managed files
      without a status effect alone. Untrusted, incomplete, or blocked lifecycle
      evidence withholds facts it cannot establish, and absence makes no claim
      about unmanaged extension-like files. A safely
      established uninstalled workspace is complete: initial shipped measurement
      remains measured when its payload is available, current startup,
      Difference, startup percentage, continuity, and root-category facts are
      not-applicable, total physical context remains numeric when measurable,
      and applicable unmeasurable facts
      are unavailable and incomplete. Compact uses one operation-level `Next:`
      only when useful and never lists repair or lifecycle proposals. Structured
      schema, numeric exits, token-estimation implementation and extra rounding,
      recovery identity and retention, verbose diagnostic fields and redaction,
      and .NET boundaries are fixed by the accepted Architecture; Gate 5 owes
      executable evidence.
- [x] **CLI-D088 — Context refinement:** preserve the exact public form
      `open-forge context [source-reference...] [--additions-only]
[--content=<part>[,<part>...]] [--follow-links=<positive-depth|all>]
[global flags]`, the current startup and explicit closure semantics,
      source references, paths and headings projections, overwrite layering,
      exact authored bytes, no inference, no sessions, no mutation, and all seven
      semantic results. Repeated `--additions-only` is accepted and idempotent;
      repeated `--follow-links` is invalid even with the same value; repeated
      `--content` is invalid while repeated parts in one value are idempotent;
      shared source-operand and global repetition rules remain in their shared
      contracts. Canonical projection order is independent of flag order:
      operation-level paths first, then sources and physical layers in resolved
      order, then metadata, frontmatter, headings, body, and document-ordered
      requested sections within each layer. A fully resolved zero-source
      additions difference and an empty heading outline are complete. External
      HTTP/HTTPS URLs are unchecked, never fetched or selected, and do not weaken
      completeness; cycles and duplicate links do not change status alone.
      Unsafe or ambiguous source or local-target identity, containment, or
      overwrite identity/boundary is `blocked`; missing local targets, broken
      fragments, invalid link encoding, unreadable required layers, and ambiguous
      sections are `incomplete`; completely inspected safe observations such as
      a proven absent section or exact-target case mismatch are `attention`;
      otherwise the result is `complete`. Multiple ordinary conditions use blocked,
      incomplete, attention, complete precedence; invalid input stops before the
      operation, while failed and interrupted retain their event meanings. Safe
      content and observations remain visible and broken edges are never hidden.
      Primary human complete/attention/incomplete results use stdout, primary
      human invalid/blocked/failed/interrupted results use stderr, and JSON emits
      one structured result to stdout for every status with bounded diagnostics on
      stderr. Interface and Behavior remain current authorities, Technical Design
      remains subordinate, and the accepted Architecture fixes schema, exits,
      parser/library/source-generation, source ranges, filesystem
      identity/containment, graph-builder and source-module boundaries, verbose
      diagnostics/redaction, and other concrete .NET choices. Gate 5 owes their
      executable evidence. The source-stated exact heading-name
      comparison remains accepted. Status remains orientation only; Find
      discovers inventory and predicates; References reports direct edges without
      content; Route Inspect profiles one route without authored content; Doctor
      diagnoses; Context retrieves selected ordered content.
- [x] **CLI-D089 — Route Inspect refinement:** preserve the exact public form
      `open-forge route inspect <source-reference> [global flags]`, exactly one
      subject, no operation-specific flags, current route-profile facts, no
      authored content, no diagnosis, no mutation, and all seven semantic
      statuses. Primary human `complete`, `attention`, and `incomplete` results
      use stdout; primary human `invalid`, `blocked`, `failed`, and `interrupted`
      results use stderr. JSON emits one complete structured result to stdout for
      every status, with bounded diagnostics on stderr and no human text in JSON.
      The finite source-state classification classifies routed entrypoints,
      leaves, native sources, accepted compatibility entrypoints, valid overwrite
      pairs, detached entrypoints, and known supported unrouted sources as
      complete when their applicable facts are complete. A safely resolved
      non-unique automatic ID is the only ordinary `attention` condition;
      exact-path or interactive resolution must make the
      physical source and route safe and complete, and interactive selection
      requires rerunning with the exact path for non-interactive use. Exact-path
      selection preserves that observation without inventing an action. An
      unresolved non-interactive collision is blocked, retains every candidate
      path, and requires rerunning with one listed exact path.
      Unreadable required sources, incomplete route chains, and unmeasurable
      applicable facts are `incomplete`; orphan or ambiguous overwrites,
      ambiguous routes, unsafe identity, and containment failures are `blocked`;
      zero or several operands, Loader, unknown, missing, and unsupported sources
      are `invalid`; failed and interrupted retain event meanings. Empty
      selection additions and empty `#LoadNow` descendant sets are measured zero;
      ordinary leaf descendants and detached or unrouted route-dependent facts
      are not-applicable; applicable unmeasurable facts stay visibly unavailable.
      Replace route-inspect diagnosis-like findings with observations and
      availability conditions. Required next operations are limited to exact-path
      collision recovery, a known direct safe correction or `open-forge doctor`
      for incomplete/structural block, named invalid-input correction, and useful
      failed/interrupted retry guidance; complete results have none. Compact
      human output keeps identity, route state and chain, reading behavior, the
      three route measurements, applicable topology, overwrite state, status,
      completeness, safety, and at most one `Next:` line; inherited-Axiom detail
      and optional explanation remain expanded-only. Doctor owns diagnosis and
      recommendations, and Route Inspect never emits route mutation, health,
      content-placement, or diagnostic recommendations. Interface and Behavior
      remain authoritative, no Technical Design is created, and the accepted
      Architecture fixes exact structured schema, numeric exits,
      parser/filesystem libraries and realization, serialization,
      diagnostics/redaction, and .NET boundaries. Gate 5 owes their executable
      evidence. Shared
      Global Flags, Source References, Status measurements, Context closure,
      Framework meaning, and command boundaries remain linked authorities.

- [x] **CLI-D090 — Uniform route-write closure:** keep `route init`, `route
create`, and `route update` uniform in their existing command meanings and
      boring in their observable shape. Singleton one-value inputs reject
      repetition; explicitly multi-value tags retain each command's local order,
      replacement, duplicate, and deduplication rules; and applicable Boolean
      `--dry-run` and `--skip-git-check` flags repeat idempotently without adding
      authority. All three use the seven semantic statuses with ordinary
      precedence `blocked` > `incomplete` > `attention` > `complete`; invalid
      input stops before operation resolution, while failed and interrupted
      results retain their event meanings. Safe partial coverage is incomplete
      before writes, unsafe or ambiguous boundaries are blocked, and an
      unexpected post-write failure is failed. Human complete/attention/
      incomplete results use stdout; human invalid/blocked/failed/interrupted
      results use stderr; JSON emits one complete typed result on stdout for
      every status and bounded diagnostics use stderr. Dry-run and application
      share status conditions, and planned changes alone do not create
      attention. `route init` reports attention only for a newly planned
      entrypoint whose intended tags contain exact `NeedsAuthoring`; an unchanged
      existing marker does not change a complete no-op. `route create` has no
      current finite attention condition, so its uniform attention status is
      unreachable until one is accepted. `route update` reports attention only
      when explicit Template body content is safely not applied because the
      target contains authored non-whitespace body content, including a
      Template-only byte-level no-op and dry-run; it never suggests overwriting
      that body. The three local Interface and Behavior contracts remain the
      detailed owners of these command-specific conditions and their
      observations.

- [x] **CLI-D091 — Framework and Extension lifecycle closure:** accept the
      unified Queue 29 lifecycle direction and its exact command surfaces. Root
      Framework `install` uses
      `open-forge install [--force] [--automatic] [--dry-run] [--skip-git-check] [global flags]`
      for management establishment and exact trusted managed no-op. Existing
      managed changed, missing, retired, or source-divergent state is `blocked`
      and directs to root `update`; initial `--force` may replace only an exact
      eligible current occupant and records the newly verified source state
      without adopting old bytes. Root `update` uses
      `open-forge update [--force] [--prune] [--automatic] [--dry-run] [--skip-git-check] [global flags]`
      and requires trusted existing Framework lifecycle state. Normal update
      applies baseline-unchanged and genuinely new safe content, preserves
      changed/missing/retired divergence as `attention`, `--force` replaces or
      restores only current expected content, `--prune` deletes only eligible
      retired managed content, and `--force --prune` composes those boundaries.
      Neither operation has a Framework group, root `init`, reinstall/replace/
      restore/recover alias, Framework uninstall/remove leaf, generic apply,
      saved plan, semver source selector, or implicit formatter execution.
      The Extension group uses exactly `list`, `inspect`, `create`, `install`,
      `update`, and `remove`. List and inspect are read-only. Create writes only
      `<catalogue>/<id>/extension.json` and `payload/.agents/` under its distinct
      `--path` catalogue destination; `--workspace` is accepted as a no-op for
      create. Extension install/update use one exact embedded or explicit
      package/catalogue source universe, explicit IDs or declared `--all`, exact
      offline dependency closure, trusted Framework-anchor and route-host facts,
      isolated lifecycle sections, semantic fingerprints, generated projection,
      explicit ownership, and complete recovery. Extension install establishes
      absent managed IDs or verifies an exact no-op; managed divergence directs
      to Extension update and initial `--force` covers only an eligible exact
      occupant. Extension update's `--force` covers changed/missing current
      expected paths and `--prune` covers eligible retired content. Remove needs
      no package source bytes, releases explicit trusted ownership, retains
      shared files, deletes safe unchanged final owners, preserves changed final
      owners by default, and permits changed-final-owner Delete only through
      same-request `--prune`. Manual, idless, and direct-overlay content remains
      unmanaged. All lifecycle operations use one typed result, seven statuses,
      exact affected-path Git policy, adjacent-backup recovery, expected-state
      revalidation, verification, and no hidden journal. The one lifecycle
      document is `.agents/open-forge.lifecycle.json`, schema v1, with isolated
      Framework and Extension sections in a common envelope; co-location does not
      merge their authority. Supported parseable kinds use parser/AST-derived
      format-insensitive semantic fingerprints, while exact bytes remain fresh
      operation-time planning and recovery facts. Unsupported or ambiguous
      equivalence fails closed. D047 is revised by this accepted closure: the
      CLI executes no formatter and persists no formatter receipt; conservative
      detection/advice only may remain informational. Untrusted, malformed, or
      source-unavailable installed facts remain explicit read-only states and do
      not grant mutation trust. Framework and Extension ownership,
      route, containment, generated-marker, shared-owner, dependency, and
      recovery boundaries remain separate. The replacement does not read,
      recognize, migrate, alias, or fall back to old lifecycle or Extension
      files; Gate 5 owes executable proof of these accepted boundaries.
      An exact single-package source may supply only its completely validated
      manifest ID when IDs and `--all` are omitted, including automatic and
      non-interactive requests; automatic mode never chooses among packages or
      broadens selection to `--all`. Dry-run shares application request, facts,
      plan, preflight, and pre-effect planning status, writes nothing, and never
      produces an apply-time `failed` or `interrupted` result. Planning/read
      failures and cancellation before effects retain their own event meaning.
      Old-format lifecycle and Extension files remain ordinary untouched content
      outside replacement authority.

## Program And Process Decisions

- [x] **CLI-D092 — Implementation readiness:** Gate 3 Architecture and Gate 4
      closeout are accepted and current. Gate 4 established the one accepted
      Crystallized source set, finalized the `contracts/index/` and
      `contracts/references/` paths, archived history, reconciled the applicable
      repository guidance and public summaries, and completed review and bounded
      validation. The known legacy Doctor limitation remains preserved Gate 5
      evidence and is not claimed clean. Gate 5 remains unaccepted and unstarted
      pending maintainer review. This item does not claim implementation, Native
      AOT, package, CI, or release evidence.
- [x] **CLI-D093 — Full delivery order:** after implementation readiness, deliver
      the foundation first: C#, .NET 10 or newer, modern `.slnx`, selected and
      proven libraries, Native AOT, xUnit v3 and its accepted test framework,
      real `System.IO` filesystem behavior, and the initial local source layout.
      Then implement every retained command in logical dependency order,
      reanalyzing each command just in time. Create thin package-manager
      packaging with npm as the first wrapper, and finish with CI for build,
      test, and release. Wrappers never implement command behavior, and release
      or publish actions run only from `main`. Gate 5 has no partial publication
      target.
- [x] **CLI-D094 — Branch and release boundary:** begin work from the exact
      `develop` branch, use focused `feature/<task>` branches for accepted Task
      work, and squash-merge accepted work back to `develop`. Release and
      publication work runs only from `main`; no feature or `develop` branch is a
      release source.
- [x] **CLI-D095 — Public publication hygiene and final closeout:** public
      artifacts must not expose AI, provider, model, or runtime-orchestration
      identifiers; internal Task, review, or handoff IDs; hidden system or
      prompt metadata; local user, machine, or path IDs; secrets; or tokens.
      Public product, command, schema, version, and artifact IDs remain valid.
      After the CLI is complete, reconcile public and current documentation,
      update the README and subsequent documents, archive useless history,
      remove competing stale material, validate maps, navigation, and packages,
      and produce final release evidence.

## Record and lifecycle boundary

The top-level [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md)
is the accepted concise `#Evergreen` Document, not a Pattern. It defines the
command-contract roles, topology, and authority boundaries and links to the
detailed split command contracts. It does not create a reusable structural shape
or a second authority source.
The separate [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md)
defines cross-command operation conventions and does not replace the
command-local contracts.
The accepted [CLI Architecture](../../crystallized/documents/cli/architecture.md),
the Command Contract Set, the Shared CLI Operation Contract, and the detailed
contracts are the current Crystallized source set. Working retains only program
records; no moved-file Working authority remains.

Templates remain optional copy-ready starters and have been reconciled with the
accepted source set. Their use does not create contract authority.
No new Decision record is needed for the Queue 33 rejection. This Agenda remains
the current product-disposition source after Gate 2; create a separate Decision
later only when rationale has independent future value. This boundary records
current lifecycle placement without creating a new Decision file.

## Framework Boundary Decisions

- [x] **CLI-D020 — Workspace selection:** use the exact current working directory
      by default or the exact `--workspace <path>` value. Do not search upward,
      infer another root, or use marker-based discovery.
- [x] **CLI-D021 — Source references:** every supported `.agents` source receives
      an automatic path-derived ID. Existing-source operands accept either that
      ID or an exact `.agents/...` path. The prefix distinguishes the forms;
      commands never guess. Exact paths disambiguate ID collisions. Interactive
      use may ask the user to choose, while JSON and non-interactive use block
      with all candidate paths. Results show both ID and path. The
      [CLI Source References Interface](../../crystallized/documents/cli/contracts/shared/source-references/interface.md) contract defines the
      complete accepted behavior.
- [x] **CLI-D022 — Context closure:** support the startup-required closure, a
      continuity set within that closure, and explicit route or scope closures.
      Apply the current Framework loading rules and the complete Context Command
      contract.
- [x] **CLI-D023 — Context state:** keep context operations stateless. Do not add
      receipts, sessions, `--since`, or hidden persistent state. Content hashes
      may be returned as ordinary evidence.
- [x] **CLI-D024 — Authored metadata:** `description` remains required for
      entrypoints and indexed Markdown files. A direct-load file that is never
      indexed may omit Open Forge metadata unless another contract requires it.
      `responsibility` remains optional by design and should be omitted when it
      only repeats the description or file meaning. `route create` requires an
      explicit destination description and tags because Template metadata
      describes the Template and never transfers. `route init` may instead emit
      an honest draft description and `NeedsAuthoring` for missing entrypoints;
      it does not claim the route's eventual meaning. The CLI never invents
      semantic values. The accepted Architecture fixes the YAML parser, semantic
      models, compatibility boundary, unknown-field handling, and canonical
      serialization; Gate 5 owes executable proof.
- [x] **CLI-D025 — Generated Entries:** treat generated `Entries` as derived
      navigation, preserve authored content outside their bounded markers,
      rebuild them explicitly or after relevant writes, and verify the result.
- [x] **CLI-D026 — Overwrites:** treat a base and adjacent overwrite as one
      logical source with ordered layers. Human body output should include a
      clear overwrite separator before the companion body. Structured output
      keeps the layers distinct. Orphans and ambiguous pairs fail visibly.

## Output And Automation Decisions

- [x] **CLI-D027 — Result model:** every operation and important internal stage
      returns a focused typed result. Human output, JSON, dry-run, verification,
      and process completion derive from those results instead of rerunning
      behavior. The accepted Architecture fixes public schema version 1; Gate 5
      owes executable schema and serialization proof.
- [x] **CLI-D028 — Human output:** keep output short, predictable, and readable.
      Errors always name the failed operation, subject, cause, and useful next
      action. Human output renders the semantic status `attention` as
      `requires attention`. Ordinary results state what happened without exposing
      internal stages such as successful preflight. Global `--view` selects
      compact or expanded result density, with expanded as the default. Compact
      output remains token-friendly; expanded output carries ordinary evidence
      and provenance. Provide `--verbose` separately for bounded diagnostic
      detail.
- [x] **CLI-D029 — JSON output:** provide stable structured output from the same
      typed operation result as human output. JSON mode is non-interactive:
      missing semantic input is invalid, while missing authority or unresolved
      choices block. The replacement has no legacy lifecycle compatibility or
      migration path; the accepted Architecture defines the exact document
      boundary and Gate 5 owes executable proof.
- [x] **CLI-D030 — Context projections:** provide CLI-generated metadata, ordered
      paths, parsed heading outlines, authored frontmatter, full bodies, and
      exact named sections through one composable `--content=<parts>` flag. Use
      `metadata` for CLI-generated source metadata without authored payload and
      `frontmatter` for authored YAML. The Context Command defines all parts,
      combinations, defaults, section behavior, and errors. Never make
      incomplete context look complete. Do not add receipt-based suppression or
      semantic ranking.
- [x] **CLI-D030A — Structural headings:** `find --heading` and
      `section:<name>` projections use heading nodes and section boundaries from
      the accepted Markdown parser. Complete visible heading text matches while
      ignoring case. Structural recognition may accept compatibility forms such
      as Setext without making them canonical or semantically active.
- [x] **CLI-D031 — Exit statuses:** use stable semantic categories for clean
      success, completed attention, invalid input, blocked work, failed work,
      and interruption. Structured results keep the value `attention` while
      human output says `requires attention`. The accepted Architecture fixes the
      numeric mapping; Gate 5 owes executable exit proof.

## Accepted Gate 3 Architecture Decisions

The following decisions were accepted by the maintainer with the Gate 3
Architecture. They define design, not executable proof. Gate 5 must verify them
without reopening the accepted choices.

- [x] **CLI-D096 — One executable and physical solution:** use one production
      project, `OpenForge.Cli`, and one replacement executable in
      `OpenForge.slnx`. Target `net10.0` with SDK `10.0.101`,
      `rollForward=latestPatch`, and C# `14.0`. Keep source at
      `src/open-forge-cli/OpenForge.Cli`, mirror production paths in
      `tests/open-forge-cli/OpenForge.Cli.Tests`, and use a separate
      `OpenForge.Cli.SystemTests` project for complete system and end-to-end
      boundaries. Every solution folder must correspond to a physical folder;
      test projects are not shipped executables.
- [x] **CLI-D097 — Explicit composition and earned DI:** make the composition
      root explicitly build the command tree, bind requests, compose capabilities,
      select renderers, and map typed results to exits. Prefer direct construction
      and pure capability-named functions. Add dependency injection only when
      real composition or lifecycle earns it, and require any DI path to be
      source-generated and Native-AOT-safe. Do not use a default general
      container, service locator, reflective dispatch, string registry, universal
      operation engine, or fake filesystem.
- [x] **CLI-D098 — Accepted dependencies and identity:** use
      System.CommandLine 2.0.11 for the explicit parser tree, Markdig 1.3.2
      through one fixed CommonMark pipeline, YamlDotNet 18.1.0 through the
      source-generated semantic path with no reflective fallback, and
      source-generated System.Text.Json metadata with reflection disabled. Use the conservative
      `open-forge-markdown-v1` parser/AST semantic fingerprint, preserve exact
      source bytes for operation-time facts, exclude derived `Entries` interiors,
      and fail closed on unsupported equivalence.
- [x] **CLI-D099 — Lifecycle document and legacy boundary:** use only
      `.agents/open-forge.lifecycle.json`, schema version 1, with one common
      envelope and isolated `framework` and `extensions` sections. Each operation
      changes only its selected section and preserves unrelated bytes and meaning.
      The replacement does not read, recognize, migrate, alias, or fall back to
      old lifecycle or Extension files. Old-format files remain untouched outside
      replacement authority.
- [x] **CLI-D100 — Real filesystem and workspace lock:** use the cross-platform
      .NET BCL first, especially real `System.IO`, with real isolated operating-
      system temporary resources in tests. Operations that mutate the selected
      workspace acquire the actual OS-level exclusive lock at
      `.agents/open-forge.lock`; file existence is not lock ownership. Extension
      create is the sole current no-workspace mutation exception and relies on its
      exact catalogue-destination identity, expected-state, Git, recovery, and
      collision guards. Do not add a virtual or fake filesystem or speculative
      platform adapter.
- [x] **CLI-D101 — Typed results, exits, and recovery provenance:** form one
      concrete typed result and render it without rerunning the operation. Use
      structured schema version 1 with exactly `schemaVersion`, `command`,
      `status`, `workspace`, `result`, and `next`. Fix the seven semantic exits at
      `complete=0`, `failed=1`, `attention=2`, `incomplete=3`, `invalid=4`,
      `blocked=5`, and `interrupted=130`. Bind recovery provenance to workspace,
      operation, target logical and physical identity, artifact kind, expected
      before/after identity, and recovery state.
- [x] **CLI-D102 — xUnit evidence tiers:** use xUnit v3 through Microsoft Testing
      Platform. Every `Fact` and `Theory` has an explicit readable `DisplayName`,
      one durable `Feature` trait, and exactly one evidence trait with one of
      `Evidence=Unit`, `Evidence=Integration`, `Evidence=EndToEnd`, or
      `Evidence=PackageEndToEnd`. Unit tests cover pure functions, Integration
      tests cross real production boundaries, EndToEnd tests execute the built
      Native AOT CLI, and PackageEndToEnd tests execute the npm launcher and
      optional platform package boundary.
- [x] **CLI-D103 — Six-RID package graph and supply chain:** publish exactly
      `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and
      `osx-arm64`. Use `@thelithiumforge/open-forge` as the launcher and
      `@thelithiumforge/open-forge-win32-x64`,
      `@thelithiumforge/open-forge-win32-arm64`,
      `@thelithiumforge/open-forge-linux-x64`,
      `@thelithiumforge/open-forge-linux-arm64`,
      `@thelithiumforge/open-forge-darwin-x64`, and
      `@thelithiumforge/open-forge-darwin-arm64` as the platform packages.
      Packages contain no postinstall, download, compilation, or behavioral
      wrapper. Require support-floor execution, checksums, signatures, SBOM,
      build provenance, OIDC attestation, and main-only publication.

## Accepted safety and operation boundaries

- [x] **CLI-D041 — Validation versus mutation:** checking and inspection are
      read-only. Every write crosses an explicit plan, authority, and consent
      boundary. The explicit `cleanup` command supplies consent for every
      currently eligible cleanup artifact without adding a generic batch surface.
- [x] **CLI-D042 — Dry-run and diff:** every write uses the same planner and
      preflight as application, shows exact intended effects, and writes nothing
      in dry-run. There is no saved plan or generic `apply` command.
- [x] **CLI-D043 — Git policy:** Git remains an affected-path review boundary
      when available. `--skip-git-check` bypasses only cleanliness and never
      grants replacement, deletion, ownership, identity, containment,
      verification, or recovery authority.
- [x] **CLI-D044 — Verification and recovery:** revalidate expected state,
      apply the complete plan, verify every effect and the whole operation, and
      reverse handled partial writes only while identity guards match. Cleanup is
      the narrow monotonic deletion exception.
- [x] **CLI-D046 — Recovery artifacts:** retain structured provenance and required
      adjacent recovery artifacts until complete verification. Do not create a
      persistent transaction journal or replay a saved plan.
- [x] **CLI-D047 — Formatting boundary:** the CLI does not execute a formatter or
      persist formatter state. Supported parseable content uses the accepted
      semantic fingerprint; exact bytes remain operation-time facts; unsupported,
      binary, and unparseable content fails closed.
- [x] **CLI-D048 — User content:** preserve unowned files, local changes,
      intentional removals, authored content outside generated regions, and
      overwrite companions. No force, prune, or skip-Git flag grants arbitrary
      deletion or adoption.
- [x] **CLI-D050 — Evidence tiers:** test typed stages directly, cross real
      boundaries in focused integration tests, and keep built Native AOT and
      package journeys for complete process evidence.

## Gate 5 executable evidence, not yet accepted

The following remain unperformed proof obligations, not unresolved Architecture
questions:

- [ ] Prove the pinned SDK, `.slnx`, central dependency versions, committed lock
      files, warning-free managed build, trimming, and warning-free Native AOT
      publish.
- [ ] Prove real filesystem identity, containment, UTF-8 ranges, lifecycle
      section preservation, `.agents/open-forge.lock`, recovery provenance, and
      the retained physical-identity regressions.
- [ ] Run isolated Unit, Integration, EndToEnd, and PackageEndToEnd evidence with
      selectable `Feature` and `Evidence` traits and explicit display names.
- [ ] Verify every retained command and the complete non-shipping delivery
      boundary. Completion is excluded; no partial publication is accepted.
- [ ] Execute the six RIDs at their support floors and prove the package graph,
      checksums, signatures, SBOM, provenance, OIDC attestation, and main-only
      publication journeys.

## Explicit deferrals

- [x] **CLI-D067 — Verbose diagnostics:** provide `--verbose`. Ordinary errors
      remain complete and human readable. Exact diagnostic content and redaction
      are Gate 5 evidence details.
- [x] **CLI-D068 — Additional channels:** keep package channels beyond the first
      npm wrapper outside this CLI program. Reconsider them only through a new
      user need and support decision.
- [x] **CLI-D065 — MVP retirement:** keep the frozen executable and its useful
      history until post-implementation Gate 6 closeout establishes the required
      retirement evidence.
- Package-manager update UX, prerelease behavior, wrapper mismatch handling,
  binary coexistence, and cutover communication remain outside the accepted
  first-release implementation boundary. They require release evidence or a
  new support decision; they do not reopen Gate 3 Architecture.
