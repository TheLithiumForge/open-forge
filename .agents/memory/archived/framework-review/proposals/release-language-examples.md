---
open-forge:
  description: Larger before-and-after passages used to agree on the Framework writing standard
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# Release Language Examples

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Drafts for discussion. These samples do not apply changes to the source files. Each Before block is an exact current source excerpt; each Proposed block explores the requested voice while preserving its meaning. Metadata and surrounding sections are outside the excerpts.

## 1. Memory: purpose, authority, and classification

Source: [src/open-forge/.agents/memory/_memory.md](../../../../../src/open-forge/.agents/memory/_memory.md) (recorded line 7). Opening and complete Authority And Classification subsection. Lines 7-18.

- Source file SHA-256: `f8957347ce3218548abf28573d1f8137b026820d8ba0f2933beed87765aab23c`
- Excerpt SHA-256: `852735ba15bcb072e4c36555c2419b6bf1148a45881a8fff88f0fd21866d5186`

### Before

```markdown
# Memory

Memory is self-growing Markdown state for active work, coordination, accepted knowledge, candidates, and history. It can grow through useful records and routed scopes without a fixed structural ceiling, while unrelated branches stay outside active context. Growth is deliberate rather than automatic.

## Axioms

### Authority And Classification

- User direction, runtime safety, and platform constraints bound Memory use. Follow declared external sources for the facts assigned to them.
- Each Memory record answers its own question within its scope. Accepted records can define current knowledge or decisions. Recording another category's content does not give the record that category's role.
- Use #Contextual and #CurrentTruth to distinguish useful context from accepted current state.
- Treat clear user direction as accepted within its scope. Keep tentative, exploratory, inferred, or meaningfully unclear conclusions in a #Contextual Memory route until accepted.
```

### Proposed

```markdown
# Memory

Memory holds Markdown records for active work, coordination, accepted knowledge, candidates, and history.

It can grow through useful records and routed scopes, with no fixed structural limit. Unrelated branches stay outside the active context. This growth is deliberate and does not happen automatically.

## Axioms

### Authority And Classification

- Follow user direction and respect runtime safety and platform constraints when using Memory. Follow declared external sources for the facts assigned to them.
- Each Memory record answers its own question within its scope. If accepted, it can define current knowledge or decisions. Recording content from another category does not give the record that category's role.
- Use #Contextual for useful context and #CurrentTruth for accepted current state.
- Treat clear user direction as accepted within its scope. If a conclusion is tentative, exploratory, inferred, or unclear in a way that matters, keep it in a #Contextual Memory route until it is accepted.
```

Meaning check: Keeps deliberate self-growth, unrelated branches outside active context, all limits on Memory use, assigned external facts, scoped acceptance, and the distinction between recorded content and category roles. The two status tags retain their meanings. Tentative, exploratory, inferred, and meaningfully unclear conclusions still remain contextual until accepted.

## 2. Templates: choosing and using a starting file

Source: [src/open-forge/.agents/templates/_templates.md](../../../../../src/open-forge/.agents/templates/_templates.md) (recorded line 7). Opening and complete Selection And Use subsection. Lines 7-19.

- Source file SHA-256: `f65b270a87eff9a47ac11a3fcff915e90abc6197a8a3e0ba5f35733f7f34e920`
- Excerpt SHA-256: `32e453c7588b8e8eede4499e072eec9829f69eae62b3d51881e5c5b9e49d5382`

### Before

```markdown
# Templates

Templates are reusable starting files. Copy and adapt them, then maintain the result independently.

## Axioms

### Selection And Use

- Check `Entries` when a new artifact would benefit from copy-ready starting content.
- Choose the most relevant Template, then copy and adapt only what the destination needs.
- Replace metadata and placeholders so they describe the destination's ownership, scope, state, authority, and relationships.
- The new artifact becomes independent. Later Template changes do not update it.
- Link a matching #Core route when the new artifact needs continuing guidance or requirements. The Template provides only starting content.
```

### Proposed

```markdown
# Templates

Templates are reusable starting files. Copy and adapt a Template, then maintain the result independently.

## Axioms

### Selection And Use

- Check `Entries` when copy-ready starting content would help with a new artifact.
- Choose the most relevant Template. Copy and adapt only what the destination needs.
- Replace the metadata and placeholders so they describe the destination's ownership, scope, state, authority, and relationships.
- The new artifact is independent of the Template. Later changes to the Template do not update it.
- If the new artifact needs continuing guidance or requirements, link a matching #Core route. The Template provides only starting content.
```

Meaning check: Keeps conditional Template selection, the most relevant choice, copying only what the destination needs, and all five metadata dimensions. The result remains independently maintained and receives no later Template updates. Linking a matching #Core route is still conditional on needing continuing guidance or requirements.

## 3. Managed Delivery: execution, discoveries, and resumption

Source: [src/extensions/orchestration/content/.agents/workflows/managed-delivery.md](../../../../../src/extensions/orchestration/content/.agents/workflows/managed-delivery.md) (recorded line 21). Steps 5-7 as one contiguous operational excerpt. The surrounding Workflow remains unchanged. Lines 21-23.

- Source file SHA-256: `e29af80ab18da51aa6b772c932bd8ea831b29c78c450e7e6885c0cd557683030`
- Excerpt SHA-256: `ca974ce4cddda3f798731cd23ec653e4c1195994bed16d1831115a3fd295ae7e`

### Before

```markdown
5. Choose sequential or parallel execution from the dependencies, authorization, and available capabilities. Parallel mutation needs distinct responsibilities and isolated worktrees or equivalent accepted isolation. Record each task's exact starting revision and verify its working location before editing. When isolation or delegation is unavailable, proceed sequentially.
6. Follow [Development](development.md) within each task. Keep related implementation and repair together. Return discoveries that change shared meaning before altering another task's assumptions, and revise only affected plans.
7. At useful checkpoints or interruptions, preserve changed and untracked work, evidence, unresolved decisions, and the next action. Before resuming, verify the recorded baseline and actual working state. Resolve discrepancies before dependent edits. Retire temporary resources only after their work is safely retained and cleanup is authorized.
```

### Proposed

```markdown
5. Choose sequential or parallel execution based on the dependencies, authorization, and available capabilities. Parallel changes require distinct responsibilities and isolated worktrees or equivalent accepted isolation.

   For each task, record its exact starting revision and verify its working location before editing. If isolation or delegation is unavailable, proceed sequentially.

6. Follow [Development](development.md) within each task. Keep related implementation and repair together. Return discoveries that change shared meaning before changing another task's assumptions. Revise only the affected plans.

7. At useful checkpoints or interruptions, preserve changed and untracked work, evidence, unresolved decisions, and the next action.

   Before resuming, verify the recorded baseline and the actual working state. Resolve discrepancies before making dependent edits. Retire temporary resources only after their work has been safely retained and cleanup is authorized.
```

Meaning check: Keeps the execution-choice factors and accepted isolation alternatives. Revision and location checks still apply to every task; unavailable isolation or delegation still requires sequential work. The Development link, related repair, shared-discovery return, affected-plan limit, interruption state, discrepancy-before-dependent-edit boundary, and both cleanup conditions remain intact. Paragraph breaks add no steps or gates.

No unresolved meaning question was identified in these three samples. Their wording remains open for user review.
