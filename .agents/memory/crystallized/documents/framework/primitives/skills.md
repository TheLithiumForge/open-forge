---
open-forge:
  description: Current Skill role, native runtime boundary, routed discovery, resource ownership, scope, and composition with other Core primitives
  responsibility: Define how Open Forge makes specialized capabilities discoverable while preserving their runtime-native contracts
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Skill, Capability, Interoperability]
---

# Skills

## Role

A Skill is a bounded specialized capability that helps perform a particular kind of work through instructions, tools, procedures, or supporting resources.

Open Forge makes Skills discoverable through ordinary routing. It does not redefine how the active agent runtime activates, invokes, installs, or executes them.

## Native Runtime Boundary

The selected `SKILL.md` owns the Skill's metadata, instructions, applicability, resource organization, and internal loading behavior. The active runtime owns the mechanics required to use that format.

Open Forge treats the complete Skill package as an interoperable capability. It may expose the Skill through workspace routes and deterministic discovery, but it does not translate the package into a competing Open Forge-specific skill model.

## Scope And Ownership

A Skill describes the positive work for which its capability is useful. Routing may organize Skills by discipline, tool, artifact, project, or another useful scope without changing their native semantics.

Resources referenced by a Skill belong to that Skill unless they identify another authoritative source explicitly. Opening the routed Skill does not make every resource relevant; its own contract determines what should be loaded.

## Composition

A Workflow may invoke a Skill to perform part of its goal. Guidance may help decide whether the Skill fits, Patterns may shape its outputs, and Directives remain binding while it runs.

A Skill can include a procedure, but that does not make it a Workflow. The distinction is the primary role: a Skill supplies a reusable capability, while a Workflow coordinates a repeatable goal and may compose several capabilities.

## Examples And Boundaries

Useful Skills include:

- operating a specialized design or analysis tool
- applying a domain-specific review process
- generating or validating a particular artifact type
- using scripts and references that form one bounded capability

A Markdown recipe that coordinates a project goal is a Workflow. General advice without a bounded executable capability is Guidance. A reusable inspectable output shape is a Pattern.

## Related Current Sources

- [Core primitive model](model.md)
- [Skills entrypoint](../../../../../skills/_skills.md)
- [Skills maintenance contract](../../maintenance/payload/agents/skills.md)
- [Workflows](workflows.md)

## Decisions And Rationale

- [Distinct Core primitive roles](../../../decisions/core-primitives.md)
