---
open-forge:
  description: Schema validation must distinguish primitive ownership from topical Workflow and Directive tags in historical or cross-cutting routes
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Routing, Validation, Workflow, Directive]
---

# Primitive Tags Need Ownership Context

## Observation

Treating every file-level #Workflow or #Directive tag as a primitive declaration caused `doctor` to apply recipe and applicability schemas to historical Memory records that merely discuss workflows or directives. The final source audit exposed 49 false errors, then eight more from one archived idea tagged with both #Workflow and #Directive but no explicit #Memory tag.

The opposite failure also matters: a file with a lone #Workflow or #Directive tag under a neutral custom route must not evade validation merely because its folder has no built-in primitive name.

## Current Resolution

- An explicit non-behavior primitive tag such as #Memory, #Pattern, #Guidance, #Skill, or #Workspace owns the file before topical behavior tags.
- A lone #Workflow or #Directive tag declares that behavior primitive, including under a neutral custom route.
- Simultaneous #Workflow and #Directive tags are ambiguous as ownership and defer to the nearest routed `entrypoint`.
- The nearest recognized primitive route remains the fallback for untagged files and organizational category `entrypoints`.

Black-box tests now cover neutral custom primitives, topical tags in a Memory-owned route, workflow-local Pattern routes, and source-workspace doctor validation.

## Candidate Follow-Up

If cross-cutting tags become more common, consider a distinct primitive-kind metadata field or a resolver receipt that separates route ownership from search topics. Until then, tags remain classification and routing signals interpreted with their loaded route context, not standalone authority.
