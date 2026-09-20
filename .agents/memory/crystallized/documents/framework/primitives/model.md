---
open-forge:
  description: Current Core primitive roles, selection questions, authority boundaries, relationships, recursive scope, and admission criteria
  responsibility: Define how Core primitives differ, compose, and evolve without becoming one generic knowledge bucket or a universal methodology
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Primitive, Authority, Scope]
---

# Core Primitive Model

## Scope

Core primitives are the reusable roles within the Framework that shape or perform work. They separate materially different questions so a `route` communicates how its contents should be selected, interpreted, applied, and maintained.

Core currently defines seven primitives: Directives, Guidance, Patterns, Skills, Templates, Workflows, and Map. The loader, routing, loading, and inherited `Axioms` are Core mechanics that make these roles work, not additional primitives. Direct Directive leaves use `Instructions`; `Axioms` remain the inherited mechanics of the loader and entrypoints.

The primitive type does not decide whether a file is currently relevant. The [routing model](../routing/model.md) selects visible context, and the [authority model](../architecture.md#authority) resolves its force within the current scope.

## Roles

| Primitive | Primary question                                            | Meaning when selected                                  |
| --------- | ----------------------------------------------------------- | ------------------------------------------------------ |
| Directive | What behavior is mandatory in this scope?                   | Binding instruction                                    |
| Guidance  | How should this recurring choice or scenario be approached? | Adaptable contextual judgment                          |
| Pattern   | What reusable inspectable shape is the established default? | Continuing structural reference                        |
| Skill     | What specialized capability can perform this work?          | Runtime-native capability and its resources            |
| Template  | What copy-ready source can start this artifact?             | Starting content whose authority ends at instantiation |
| Workflow  | How should this defined goal be pursued and completed?      | Selected goal-oriented recipe                          |
| Map       | Where does relevant local or external truth live?           | Coarse navigation whose destination retains authority  |

Each primitive has a focused current document for its deeper meaning and boundaries:

- [Directives](directives.md)
- [Guidance](guidance.md)
- [Patterns](patterns.md)
- [Skills](skills.md)
- [Templates](templates.md)
- [Workflows](workflows.md)
- [Map](map.md)

Installed `entrypoints` contain the compact complete runtime contracts users receive. Primitive documents explain the design beyond immediate execution, while Maintenance documents own canonical sources, synchronization obligations, and verification.

## Selection And Authority

Routing, loading, type, and authority are separate:

- A `route` and its `description` make potentially relevant material discoverable
- Loading makes selected material visible
- A primitive type states how the material should be used
- Scope and declared authority determine where its meaning applies
- Tags compress selection and classification signals without creating authority

Guidance remains advisory. A Pattern is the established reusable shape in its selected scope, but a different shape may be chosen deliberately; a mandatory shape also needs a Directive or another binding `axiom`. A Skill follows its native `SKILL.md` contract and active runtime. A Map `route` points to detailed truth without replacing it.

## Composition

Primitive relationships are compositional rather than one authority ladder.

A Workflow may invoke Skills, consult Guidance, apply Patterns, instantiate Templates, and follow Map `routes` while obeying every Directive active in its scope. Any primitive may link to another when the relationship helps selection or execution, but the link does not merge their semantics.

If one artifact needs several roles, each independently meaningful role uses its matching authoritative source. For example, a Pattern can explain a reusable document shape, a Template can provide copy-ready starting content for it, and a Directive can require the shape in a particular scope.

## Recursive Scope

Core primitives use the [universal scope contract](../routing/scope.md). Routed scopes inherit the active primitive role from their loaded ancestor, so they do not reinitialize it.

For non-directive primitives, a narrower selected `route` is preferred when it safely specializes broader material. Directives are additive: a narrower selected Directive adds binding constraints without silently overriding active ancestor Directives.

Core `root routes` compose through explicit relationships. A Workflow invokes Skills, consults Guidance, applies Patterns, instantiates Templates, and follows Map `routes` through Steps or ordinary links instead of physically containing those `root routes`.

A routed folder may use any name, but a familiar primitive `slug` beneath another root does not acquire that primitive's contract automatically. The [scope contract](../routing/scope.md#universal-scope) defines the boundary.

## Boundary With Memory And Current Documents

Primitives shape or perform work. [Memory](../../../../_memory.md) preserves useful state across time. A decision preserves why an important choice was accepted, while a current document integrates what is accepted now.

Recorded text does not become active behavior merely because it describes a Directive, Pattern, Workflow, or another primitive. When durable behavior is intended, the accepted material is promoted or linked to the matching Core `route`.

## Admission And Customization

A new shared primitive is justified only when it:

1. Answers a primary question that existing primitives do not answer clearly
2. Has distinct selection, authority, application, or lifecycle semantics
3. Provides likely reusable value across workspaces
4. Justifies another `route`, explanation, validation, and maintenance surface

Specialized local content, routed scopes, and Extensions do not require expanding the shared primitive vocabulary.

The contract of a primitive does not also become a Pattern merely because it has structure. A Pattern is justified only when an inspectable shape remains useful independently of the contract that defines a primitive, `route`, or state.

Core does not ship a separate Rules primitive. `Axioms` declared by the loader or an `entrypoint` express universal or inherited Framework mechanics, while Directives express independently routed mandatory behavior. A third binding category would duplicate those roles.

The standard `routes` are useful defaults, not an untouchable taxonomy. Users may remove unused categories, replace their contents, or grow recursively specialized structures without invalidating the remaining Framework.

## Installed Sources

- [Directives](../../../../../directives/_directives.md)
- [Guidance](../../../../../guidance/_guidance.md)
- [Patterns](../../../../../patterns/_patterns.md)
- [Skills](../../../../../skills/_skills.md)
- [Templates](../../../../../templates/_templates.md)
- [Workflows](../../../../../skills/use-workflow/references/open-forge/_open-forge.md)
- [Maps](../../../../../maps/_maps.md)

## Decisions And Rationale

- [Distinct Core primitive roles](../../../decisions/framework/core-primitives.md)
- [Map primitive naming](../../../decisions/framework/map-primitive.md)
- [Templates as a Core primitive](../../../decisions/framework/template-primitive.md)
- [Workflow shape](../../../decisions/framework/workflow-shape.md)
- [Routing surfaces](../../../decisions/framework/routing-surfaces.md)
- [Loading reliability](../../../decisions/framework/loading-reliability.md)
