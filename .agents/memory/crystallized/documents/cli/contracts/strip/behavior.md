---
open-forge:
  description: Plan, revalidate, apply and verify one deterministic strip plan with recovery data for interrupted writes
  responsibility: Define how strip and uninstall form, check and apply their shared plan
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Behavior, CurrentTruth]
---

# strip Behavior Contract

The [Interface Contract](interface.md) defines the parts, the document set and flatten. The [mutation and recovery design](../../technical-designs/mutation-and-recovery.md) defines the shared lock and recovery primitives. [`uninstall`](../uninstall/behavior.md) adds its removals to the same plan.

## Plan Before Effects

Read the workspace layout once. Invalid or unavailable settings stop the run as `strip.workspace-unavailable`. So does an unreadable ownership lock, because strip then cannot tell which files Open Forge manages. Enumerate the documents below the inventory roots without following links, and read each one as strict UTF-8. Parse each document through the shared Markdown and YAML readers. Strip adds no second parser. It uses the parser's frontmatter and link spans and edits only those spans, or the comment that holds a comment link.

Form every edit for every document before the first effect. Edits never overlap, and an edit inside a removed span is dropped. A document that cannot be edited safely blocks the whole plan before any write. Resolve link destinations through the shared local-destination rules, against each document's current location, then compute flattened spellings from the new locations.

## Determinism

The same workspace bytes and options give the same plan and the same bytes. Documents are visited in ordinal path order. Parts apply in the fixed order `frontmatter` or `metadata`, then `entrypoints`, then `links`, then flatten relinks. Plan entries are ordered by path, with folder removals last and deepest first. No time, randomness, environment or file-system enumeration order reaches the plan or the written bytes.

## One Complete Change

Applying acquires the workspace lease, revalidates the layout and forms the plan again. When the fresh plan differs from the reviewed plan in any path, action or hash, the run stops as `strip.target-changed` before any write. A file that changed after planning therefore stops the run.

Prepare one recovery bundle with every planned file's exact prior bytes, or its proven absence, before the first effect. Apply moves' new files first, then rewrites, then deletions, then empty folder removals. Check each file's expected bytes right before its effect, and verify its result after. A mismatch before the first effect is `strip.target-changed` and nothing changes. A mismatch after effects started is `strip.target-changed-during-apply`, with the `doctor` next action. Stop at the first failure or interruption and report how many changes were made, `Stopped after <n> of <m> changes.`, like other writing commands. The recovery bundle stays, holding every planned file's prior bytes, and no automatic restoration runs. After every effect verifies, delete the recovery bundle. A bundle that cannot be deleted gives `strip.recovery-artifact-retained` and the `cleanup` next action.

A preview, a refusal and a verified no-op take no lock and create no runtime state.

## Required Evidence

Cover each part alone and combined, root and `open-forge:` frontmatter, foreign keys, CRLF bodies, a frontmatter block without other keys, whole frontmatter removal with and without `metadata`, entrypoints with and without user text, managed and unowned, inline and reference links, links in `Entries` lists, fragment-only, folder, image, external and code-span links, comment links alone on their line and inline, flatten moves, relinks and collisions. Include the eval harness's cases as Integration fixtures that compare exact bytes, copied into the test project with a note that records their origin. Cover a flatten folder that holds only a managed entrypoint above user documents, which strip refuses, the same folder with `entrypoints`, which strip flattens, and a user route with an unowned scaffold entrypoint, which strip flattens. Verify dry-run parity, no writes on preview or refusal, changed files under the lease, interruption and repeated runs that reach a no-op.
