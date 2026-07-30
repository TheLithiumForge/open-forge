---
open-forge:
  description: Historical maintenance-migration questions that clarified knowledge roles, Decision boundaries, and a remaining lifecycle-metadata idea
  tags: [Memory, Archived, Idea, Contextual, Historical, Framework, Maintenance, Migration, Decision, KnowledgeRole]
---

# Maintenance Migration Considerations

## Resolution

The migration confirmed that `route` semantics, current documents, and repository-only helpers can make knowledge placement understandable without another binding classification system.

The accepted relationship among current documents, Decisions, and Archives now lives in [Accepted State and Synchronization](../../crystallized/documents/framework/truth.md#current-views-decisions-and-history). The [Knowledge Role Helper](../../crystallized/documents/maintenance/helpers/knowledge-roles.md) supports repository maintenance without becoming user-facing vocabulary.

The remaining open question about approval and replacement metadata moved to [Decision Lifecycle Metadata](../../emerging/ideas/decision-lifecycle-metadata.md). The exploration below is retained as historical migration context.

The framework should make it natural to recognize whether a change creates or updates a directive, pattern, decision, or current document without needing another directive that tells agents to classify it.

During the file-by-file migration, observe:

- where the authoritative primitive is immediately clear from the route and existing definitions;
- where more than one authoritative source appears plausible;
- whether ambiguity comes from primitive definitions, route structure, document shape, or missing links;
- which repeated ambiguities justify a framework design change after the migration provides evidence.

Keep this contextual until the migration shows whether the design itself needs refinement.

## Decision Role

The migration should test whether Open Forge uses decisions too broadly.

The intended model may be a catalogue of discrete accepted choices and useful rationale whose combined current outcome is expressed coherently by #Evergreen documents. The current use of decisions as durable rationale for source and packaging choices is also useful.

Candidate boundary from the vision and architecture discussion:

- An emerging idea or analysis owns choices still under consideration; a decision is not a backlog of unresolved options.
- A crystallized decision preserves a discrete accepted choice and the useful reason it was made.
- The affected document, directive, pattern, route, code, or external system owns the coherent current outcome.
- A decision links to those authoritative sources instead of restating their complete current contents.
- When a decision no longer supports current truth, preserve it under the matching archived scope if its history remains useful; otherwise consolidate or prune it.
- Decisions and archives may be initialized below a narrower scope when that ownership makes selection, history, and inheritance clearer. The state and scope axes remain distinct.

Observe:

- whether a decision records a real choice and rationale or merely repeats current behavior;
- whether #Evergreen documents integrate the current outcome without requiring readers to assemble it from many decisions;
- when a revised, replaced, or low-value decision should be consolidated or archived;
- whether the decisions catalogue remains navigable and proportionate to the choices it preserves.

Candidate lifecycle metadata for decisions includes an approval date and time, plus a replacement date, time, and link when applicable. Before adding it to the decision template, determine:

- whether lifecycle metadata belongs in frontmatter or the document body;
- which timestamp precision and timezone contract remains useful and deterministic;
- whether replacement always moves a decision to Archived or may leave it current within a narrower surviving scope;
- whether archival should be automatic, proposed, or explicitly selected;
- how partial replacement, consolidation, and several replacement decisions are represented.

This model remained open during migration and was refined through the accepted current sources linked above.
