---
open-forge:
  description: Small recursive `entrypoints` preserve local scope while one recognized `entrypoint` and explicit `root routes` remove routing ambiguity
  tags: [Memory, Decision, CurrentTruth, Routing]
---

# Routing Model

## Context

Open Forge needed navigation that could expand across many projects and disciplines without loading a flat global catalogue or requiring a proprietary registry. Scope had to remain visible and local as routes became deeper.

## Decision

Open Forge uses small recursively linked Markdown `entrypoints` with direct-child `entries`.

Every routable folder has one recognized `entrypoint`. Explicit loader `entries` select `root routes`, and each selected `entrypoint` exposes only its direct children. Loaded ancestor `Axioms` remain active for descendants so narrower `routes` add local meaning without copying or silently cancelling broader rules. `inherited` is the only explicit sentinel for declaring no local `Axioms`; `none` is not accepted because it can imply that ancestor `Axioms` no longer apply.

Open Forge authors one predictable `entrypoint` filename. Compatibility aliases are input-only. Authored Markdown links resolve from the containing file, while CLI `route` arguments use workspace-relative identities because they have no containing document.

## Rationale

Direct-child navigation keeps indexes proportional to local branching, preserves intermediate scope, and makes unrelated subtrees nearly free until selected.

One `entrypoint` per folder removes ambiguity about whether a folder participates in routing and where its local contract begins. Explicit roots prevent loose files from silently becoming `routes`. Relative Markdown links remain clickable, portable, and inspectable without a tool.

## Alternatives And Tradeoffs

- A flat central registry would simplify global enumeration but grow with the whole workspace and erase intermediate scope
- Flattening nested `entries` into higher `routes` would create the same scaling and locality problem
- Treating loose files as implicit `routes` would make activation difficult to inspect
- Several canonical `entrypoint` names would make authored structure nondeterministic
- A required `local.md` route would hard-code one organization model
- Per-file companion metadata would multiply hidden routing surfaces; user-owned overwrites instead remain visible companions to an existing base

Recursive routing adds `entrypoint` files and requires each `route` chain to remain structurally valid. Generic routing can navigate every valid tree, while lifecycle tools also need an explicit human-readable way to distinguish manager-recognized `route` segments from ordinary scope `slugs`.

## Consequences

- Context cost grows primarily with selected `route` depth, branching, and relationships rather than total workspace size
- Universal navigation and loading semantics stay in the loader
- Scope can appear recursively at every position permitted by the containing `root route`
- A child does not become a root or inherit another root's behavior because it uses a familiar name or tags
- All valid `routes` remain generically navigable without acquiring additional behavior from familiar names
- Tools may index and validate the plain-file graph but do not privately define it
- Runtime-owned `SKILL.md` files keep native resource navigation rather than receiving generated Open Forge Entries

## Authoritative Sources

- [Current routing model](../documents/framework/routing/model.md)
- [Scope and inheritance contract](../documents/framework/routing/scope.md)
- [Loading contract](../documents/framework/routing/loading.md)
- [Path contract](../documents/framework/routing/paths.md)
- [Routed Markdown representation](../documents/framework/markdown/routes.md)
- [Open Forge loader](../../../loader.md)

## Decision Relationships

- [Loading reliability](loading-reliability.md)
- [Routing surfaces](routing-surfaces.md)
- [Scope and slugs](scope-and-slugs.md)
- [Canonical Markdown authoring](canonical-markdown.md)
- [Tag semantics](tags.md)
