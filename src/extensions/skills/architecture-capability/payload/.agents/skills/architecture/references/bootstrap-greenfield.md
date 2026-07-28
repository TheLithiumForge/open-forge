# Bootstrap A Greenfield Architecture

Use this when there is no implemented system to map, or when an early project has structure only by accident.

1. State that the current architecture is absent or unstructured. Do not invent a current-state map.
2. Identify the first vertical slice, likely second slice, users, data, external integrations, runtime, deployment, and team capability.
3. Establish the minimum boundaries, ownership, dependency direction, failure containment, configuration, observability, security, and verification seams needed for those slices.
4. Prefer one simple deployable shape until evidence justifies distribution. Record scale or complexity triggers that would cause reconsideration.
5. Define a recognizable folder, component, interface, and test shape that later work can repeat.
6. Identify the mandatory invariants, contextual tradeoffs, important locations, and accepted rationale that deserve routed artifacts.
7. Verify that a cold implementation session can find the direction and build the first slice without guessing material structure.

The result is a minimal structural runway, not speculative enterprise architecture.
