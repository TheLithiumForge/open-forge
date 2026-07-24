---
open-forge:
  description: Questions about ownership classification and the long-term role of decisions surfaced by the maintenance migration
  tags: [Memory, Idea, Contextual, Candidate, Framework, Maintenance, Migration, Ownership]
---

# Maintenance Migration Considerations

The framework should make it natural to recognize whether a change creates or updates a directive, pattern, decision, or current document without needing another directive that tells agents to classify it.

During the file-by-file migration, observe:

- where the owning primitive is immediately clear from the route and existing definitions;
- where more than one owner appears plausible;
- whether ambiguity comes from primitive definitions, route structure, document shape, or missing links;
- which repeated ambiguities justify a framework design change after the migration provides evidence.

Keep this contextual until the migration shows whether the design itself needs refinement.

## Decision Role

The migration should test whether Open Forge uses decisions too broadly.

The intended model may be a catalogue of discrete accepted choices and useful rationale whose combined current outcome is expressed coherently by #Evergreen documents. The current use of decisions as durable rationale for source and packaging choices is also useful.

Observe:

- whether a decision records a real choice and rationale or merely repeats current behavior;
- whether #Evergreen documents integrate the current outcome without requiring readers to assemble it from many decisions;
- when a revised, superseded, or low-value decision should be consolidated or archived;
- whether the decisions catalogue remains navigable and proportionate to the choices it preserves.

Keep the model open during this migration and refine it from repeated evidence rather than reorganizing decisions immediately.
