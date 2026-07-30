---
open-forge:
  description: General convention that resource routes expose a full, symmetric CRUD surface
  tags: [Pattern, API, Convention, REST]
---

# REST Full-CRUD Symmetry

Expose a full, symmetric CRUD surface for each resource unless accepted
resource-specific information establishes a deliberate exception.

## Shape

Every resource route this project exposes provides the full CRUD surface for client
predictability: `GET` (read), `POST` (create), `PUT`/`PATCH` (update), and `DELETE`
(remove). New resource work follows this shape by default. When accepted information
for a particular resource excludes one of these operations, state that exception and
its reason explicitly rather than silently departing from the pattern.

## Review Checks

- The pattern is applied as a default shape while preserving deliberate,
  resource-specific exceptions established by accepted project information.
- A resource missing one of the four verbs states why, rather than silently omitting it.
