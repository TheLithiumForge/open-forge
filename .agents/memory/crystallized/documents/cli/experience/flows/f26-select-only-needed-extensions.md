---
open-forge:
  description: Install optional collaboration and planning content without unrelated packages
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F26: Select only needed extensions

**Selection:** Recommended addition for maintainer validation.

## Who And Goal

A maintainer wants collaboration advice first, then optional planning templates, without installing an entire development toolkit.

## Starting Point

A fresh Core installation from the current build, with no Extensions.

## Flow

1. Install `collaboration --automatic` through the Extension command and inspect its installed files.
2. Read `.agents/guidance/adaptive-collaboration.md` through context.
3. Install `planning --automatic` and list installed Extensions.
4. Read the User Flow and Scenario Templates through their explicit installed paths.
5. Remove Collaboration automatically and run Doctor.

## Expected Result

Collaboration initially adds only its three runtime files. Planning adds its own content and the shared Workflow Support dependency, with no Project Documents, Development, or Orchestration package. Removing Collaboration keeps Planning and all user-created content. The remaining workspace has valid navigation.

## Verification

Compare installed IDs, actual file presence and ownership, link closure, and independently preserved content. A template's text being returned is distinct from its recipe being executed. Existing installations with a formerly Core-owned guidance file need a separately qualified migration flow; this fresh-install flow does not prove that transition.
