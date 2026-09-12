---
open-forge:
  description: Recorded loader inheritance findings and the proposed treatment of the local execution exception
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Loader inheritance audit

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Baseline: `28cac0fc`

This is a read-only audit of the installable loader, the dogfood loader, their
category entrypoints, and the accepted loading and packaging contracts. It does
not change loader behavior, category files, tags, source parity, or the CLI.

## Direct conclusion

The inheritance model is coherent, but a few category rules restate loader
policy and should be removed rather than converted wholesale to `inherited`.
The installed loader also refers to loading thresholds that are not stated in
the installed payload. That is a real portability ambiguity, with a small
self-contained wording fix. The dogfood mechanical exception is useful, but it
belongs to the repository harness and should be tightened there; it should not
be added to the installable Framework payload by default.

## Findings

### LI-001: Memory repeats inherited authority rules

`src/open-forge/.agents/memory/_memory.md:19` repeats the loader's platform,
runtime, user-direction, and external-source rules from
`src/open-forge/.agents/loader.md:17-19`. The Memory maintenance contract says
that the Memory entrypoint relies on the loader's universal scoping and
management contract instead of restating it.

**Smallest correction:** remove the bullet. The following Memory bullets still
define Memory's own acceptance and record-role behavior.

### LI-002: Memory repeats reserved tag meanings

`src/open-forge/.agents/memory/_memory.md:21` repeats the loader's definitions
of `#Contextual` and `#CurrentTruth` from
`src/open-forge/.agents/loader.md:76-77`. It adds no Memory-specific rule;
`_memory.md:20` and `:22` already explain record roles and acceptance.

**Smallest correction:** remove the bullet. Do not remove the surrounding
Memory-specific acceptance rules.

### LI-003: Directives repeats generic route selection and conflict handling

`src/open-forge/.agents/directives/_directives.md:18` restates the loader's
general route selection rule (`loader.md:43,49-50`), and `:22` restates the
loader's unresolved-conflict rule (`loader.md:27-28`). The Directives-specific
rules at `:15-17` and `:19-21` are narrower and meaningful: they define the
required sibling shape, workspace application, child scope, and additive
authority behavior.

**Smallest correction:** remove the generic selection and conflict bullets.
Retain the narrower child-scope and additive-authority rules. The loader still
requires unresolved conflicts to be reported.

### LI-004: Loader references unavailable loading thresholds

Both `.agents/loader.md:49` and `src/open-forge/.agents/loader.md:49` say to
use `#LoadNow` or `#KeepInMind` only under “their defined thresholds.” The
precise threshold is only in the repository document
`.agents/memory/crystallized/documents/framework/routing/loading.md:30-40`.
That document is not part of the installed payload, while the accepted
packaging decision requires an installed workspace to be complete without
repository governance.

This can make manual users overuse tags and inflate baseline attention, or
underuse them and miss continuity, because the loader does not tell them what
the threshold is.

**Smallest correction:** make the loader sentence self-contained while leaving
the detailed model in the routed loading document. For example:

> Each scoped `entrypoint` chooses the loading its contents justify. Keep entries on demand unless omission costs more than loading. Use `#LoadNow` when omission costs more than its baseline attention cost, and `#KeepInMind` when active continuity is worth refreshing at its scope boundaries.

This preserves the accepted loading model and does not create a new loading
contract.

### LI-005: Dogfood and source Workflows have an authored-contract mismatch

The source Workflows entrypoint
`src/open-forge/.agents/workflows/_workflows.md:17-30` includes route-based
selection, `Goal` confirmation, the “deliberate user method” condition, and
recipe-specific headings. The dogfood entrypoint
`.agents/workflows/_workflows.md:17-29` instead omits route-based selection
and `Goal` confirmation, adds a workflow-change risk-profile rule, uses
different wording for interaction changes, and omits the recipe-heading rule.

The Workflows maintenance contract
`.agents/memory/crystallized/documents/maintenance/payload/agents/workflows.md:12,18-29`
requires the repository entrypoint to dogfood the same authored contract,
with local generated entries as the allowed difference. The packaging decision
also says shared semantics stay aligned unless an intentional local difference
is documented.

**Smallest correction:** align dogfood authored content to the source contract,
or document an accepted intentional local difference before release. This is a
real route/runtime parity issue, separate from language polishing.

## Category classification

The audit found three narrow duplicate clusters above. The remaining category
rules inspected in Directives, Guidance, Maps, Memory lifecycle, Archived,
Crystallized, Decisions, Documents, Emerging, Ideas, Observations, Working,
Patterns, Skills, Templates, and Workflows answer category-specific questions
and should remain. In particular, replacing all category Axioms with an
`inherited` sentinel would erase meaningful behavior such as Directive scope,
Memory acceptance and lifecycle, Workflow optionality and recipe shape, and
the selection rules for Guidance, Patterns, Skills, Templates, and Maps.

Nested dogfood routes that contain only `inherited - No local axioms; loaded
ancestor axioms remain active.` already use the accepted sentinel correctly.
The maintenance contract permits a category root to omit local Axioms, leave
the section empty, or use one sentinel, but it does not require every root to
do so.

## Mechanical-context exception

The exception currently appears in repository `AGENTS.md:12-18`, and related
APM role instructions refer to it. It does not appear in the installable
`src/open-forge/AGENTS.md`, whose managed block is the canonical Framework
handoff. The exception is safe in principle because it requires an explicit
label, exact pre-decided commands, literal evidence parsing, and no semantic
or acceptance decisions; it also keeps system/developer rules, permissions,
user authorization, and platform/runtime safety binding.

The current wording calls the command set “read-only” while also allowing
deterministic build, test, AOT, or tool artifacts. That is technically
ambiguous. If retained in the dogfood harness, the proposed wording is:

> Use this exception only when the user or assigning agent explicitly labels the task `no Open Forge context` (or clearly equivalent wording), supplies the exact commands in advance, and requires only literal evidence/output parsing. The worker may skip `.agents/loader.md` and Open Forge task/scoped materials for that task only. Command execution may create only already-authorized deterministic build, test, AOT, or tool artifacts. It may not select commands, edit source, make unapproved external or destructive changes, or make product, architecture, semantic, integration, acceptance, or correctness decisions. Return only the requested evidence.
>
> Semantic analysis, investigation requiring project meaning, design, implementation, integration, and semantic/code/product/architecture/acceptance/correctness review always load normal Open Forge context.
>
> System/developer rules, platform and runtime safety, permissions, user authorization, sandbox and external-effect boundaries, and the literal assignment remain binding. If the label, commands, or eligibility are ambiguous, load normal context.

This is a host or harness execution policy, not Framework route semantics. Do
not add it to `.agents/loader.md`, where it could be bypassed, or to
`src/open-forge/AGENTS.md` by default. The managed-root pattern requires the
canonical source template to contain exactly its complete managed block, and
the packaging decision keeps harness bridges and repository governance outside
the installed payload. Shipping the exception would therefore require a new,
explicit distribution and authority decision.

## Questions requiring approval

1. Approve removal of the three inherited duplicate clusters (Memory `:19`,
   Memory `:21`, and the two generic Directives bullets)? Recommendation: yes.
2. Align the dogfood Workflows entrypoint to the source entrypoint before
   release, unless the local differences are intentionally accepted and
   documented? Recommendation: align.
3. Keep the improved mechanical exception dogfood-only, outside the managed
   block and installable payload? Recommendation: yes. Adding it to the
   installed payload would need a separate distribution decision.

No runtime, CLI, build, test, or Git mutation was performed.
