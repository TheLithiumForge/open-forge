# Documents Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/crystallized/documents/_documents.md`.

The documents memory category `entrypoint` defines how agents discover durable accepted records, or routes to those records, for long-form project knowledge.

## Represents

Documents represent durable accepted records within their stated scope.

They may be files in memory or route files pointing to the destination that owns the full record.

## Contains

The installed file follows the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md): scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Document Contract

Document files hold or route durable current records such as project, product, architecture, design, research, domain, or operating knowledge.

The routed destination owns the detailed truth when the document `entry` points outside itself.

Templates and document-generation behavior belong to the route that owns that behavior.

## Loading Contract

The documents category is relevant when current work needs long-form accepted context or a route to the document that owns it.

The `entrypoint` must route agents to direct document files and child document categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load the smallest document route that can answer the current question.

## Consolidation Contract

Documents must avoid duplicate current truth.

When a document overlaps an existing current document, agents update, split, merge, or link the existing route instead of creating a competing record.

Describing behavior in a document does not activate it. Accepted behavior, reusable form, or generation rules that should guide future work belong in the matching #Core route, including user-created #Core categories and files.

## Scope Contract

Nested document categories may group durable records by any useful positive scope.

Superseded documents must be archived or linked with enough context to understand what replaced them.

## Generated Region

The final generated region uses the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md).

Generated `entries` list direct document files and direct child document categories. The [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md) defines metadata, `entry` representation, entrypoint naming, and marker shape. The routing model governs recursive discovery and generation.

## Used By

Agents use this category when they need durable accepted records or routes to those records.

Any process that writes documents must keep them as accepted records or routes to accepted records.

## Why

Documents exist so long-form current records have a stable memory route.

They keep durable context discoverable without forcing every workspace to use one document taxonomy.

## Alignment Checks

The implementation is aligned when it:

- is named `_documents.md`
- lives in `.agents/memory/crystallized/documents/`
- includes `Record` and `CurrentTruth` in scoped `open-forge:` tags
- defines documents as durable accepted records or routes to those records
- keeps templates and generation behavior out of documents
- avoids duplicate current truth
- puts accepted document material that should guide future work in matching #Core routes
- supports recursive positive scope
- routes only through its final generated region
- keeps compact document, loading, consolidation, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines
