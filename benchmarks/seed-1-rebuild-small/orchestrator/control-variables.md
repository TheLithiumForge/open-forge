---
open-forge:
  description: Candidate observation about keeping dogfood comparisons stable
  tags: [Extension, Memory, Observation, Dogfood, Candidate, Contextual]
---

# Dogfood Control Variables

## Observation

When comparing OpenForge iterations, keep the seed project and orchestrator prompt stable. Otherwise it is hard to tell whether differences came from OpenForge, the seed, the prompt, the model, or the environment.

## Candidate Rule

For iteration comparisons, change one major variable at a time and record it in the final report.
