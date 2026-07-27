---
open-forge:
  description: Small recursive entrypoints preserve local scope while one recognized entrypoint and explicit root routes remove routing ambiguity
  tags: [Memory, Decision, CurrentTruth, Routing]
---

# Routing Model

Open Forge chose small, recursively linked Markdown entrypoints instead of a flat central registry.

- Direct-child entries keep indexes local, preserve intermediate scope, and prevent every higher route from growing with the full subtree.
- Requiring one recognized entrypoint at each routable folder removes ambiguity about whether a folder is part of the Framework and where its local meaning begins.
- A stable Open Forge-authored filename makes entrypoints predictable. Compatibility aliases remain input-only accommodations so interoperability does not create several competing canonical forms.
- Exposing only explicit root entrypoints keeps root activation deliberate. Loose files beside the loader do not silently become Framework routes.
- Universal navigation and loading rules live once in the loader so category entrypoints can remain small and category-specific.
- Loaded ancestor Axioms remain active for selected descendants so narrower routes add local meaning without copying or silently cancelling broader rules.
- Authored Markdown paths resolve from the containing document because that keeps relationships clickable and portable. Tool route arguments use workspace-relative identity because they have no containing document.

The accepted current result is expressed by the [routing model](../documents/framework/routing/model.md), [scope and inheritance contract](../documents/framework/routing/scope.md), [loading contract](../documents/framework/routing/loading.md), [path contract](../documents/framework/routing/paths.md), and [routed Markdown representation](../documents/framework/markdown/routes.md).

Specialized rationale remains in:

- [Loading reliability](loading-reliability.md)
- [Routing surfaces](routing-surfaces.md)
- [Scope and slugs](scope-and-slugs.md)
- [Workflow shape](workflow-shape.md)
- [Canonical Markdown authoring](canonical-markdown.md)
