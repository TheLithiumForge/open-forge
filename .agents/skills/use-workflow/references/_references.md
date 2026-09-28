---
open-forge:
  description: "Select an installed method by its goal and scope"
  tags: [Extension, Workflow]
---

# Workflow Catalogue

## Axioms

- This catalogue belongs to the Use Workflow Skill. Its recipe conventions do not redefine other native Skills or add a Core primitive.
- A recipe has one non-empty `## Goal`, `## Steps`, and `## Completion`, in that order. Extra sections are optional. An entrypoint may organize recipes without being a recipe itself.
- `Goal` states the outcome the recipe serves and, when that isn't obvious, when it fits. `Steps` states the method, including dependencies and conditional work. `Completion` states what establishes the result or an honest blocked boundary.
- A selected recipe may use several native Skills, tools, or agents. State required relationships directly. Prefer the workspace's configured capabilities over hardcoded providers or models.
- Add, scope, adapt, or remove methods as needed. Keep this catalogue's Entries aligned with the methods that are actually present. Never treat installation as permission to execute a method.

## Entries

- [Implement a change, diagnose a failure, or review a result](development/_development.md) - #Extension #Workflow #Development
- [This workspace's own development, delivery, and deliberation methods](open-forge/_open-forge.md) - #Workflow #OpenForge
- [Coordinate dependent tasks, resumable work, and authorized integration](orchestration/_orchestration.md) - #Extension #Workflow #Orchestration
- [Turn an accepted outcome into ordered work with clear dependencies and evidence](planning/_planning.md) - #Extension #Workflow #Planning
- [Clarify project vision and architecture before maintaining their current explanations](project-documents/_project-documents.md) - #Extension #Workflow #Document
