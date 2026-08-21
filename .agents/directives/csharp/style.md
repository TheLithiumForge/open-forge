---
open-forge:
  description: Apply readable, formatted C# style to source and tests across the workspace
  tags: [LoadNow, Directive, CSharp, Style, Formatting, Readability, Testing]
---

# C# Style

## Instructions

- Honor `.editorconfig` and `dotnet format`. The accepted 200-character line ceiling remains a guideline. Prefer a shorter split when readability improves it.
- When a `StringBuilder` emits one coherent multi-line text block, prefer one raw interpolated string call over many per-line `AppendLine` calls or chains of tiny `.Append(...)` calls:

  ```csharp
  builder.AppendLine($"""
      Open Forge route list
      Workspace: {workspace}
      Result: {status}
      """);
  ```

  Use the same raw multi-line shape when only some lines interpolate values, and use a raw non-interpolated string when none do. Preserve deliberate blank lines, indentation, escaping, and final-newline behavior. Use `Append` instead of `AppendLine` when an extra newline would be unintended.

- Keep loops and conditionals as separate appends when rows or sections repeat or are conditionally omitted. Build each coherent fixed block with a raw interpolated string rather than cluttered fragments.
- Group related C# attributes onto one physical line and normally one attribute list, such as `[Fact(...), Trait(...), Trait(...)]`, when the line remains readable and within the accepted limit. Split only when attribute arguments, generated or tool constraints, conditional compilation, or readability require it. Do not change semantic attribute order or meaning merely for style.
- Keep physical folders and namespaces cohesive. Do not let one feature folder become a flat catalogue of many distinct responsibilities. When several related types form a real cluster, place them in a named subfolder and matching namespace, such as `Rendering`, `Topology`, `Parsing`, or `Filesystem`. Keep the feature's central request, result, operation, or composition types at the feature root when that makes the entry path clear.
- Do not replace a crowded flat folder with one-file microfolders. Introduce a subfolder when it gives multiple related files one clear responsibility and improves navigation, not merely to reduce a file count.
- Within the replacement CLI, place supporting implementation below an explicit `Shared/<Capability>/` child of its narrowest owning CLI, command-family, or command-leaf boundary. Keep only the primary command or component contracts and composition entry at that boundary's root. A leaf-local `Shared` folder states ownership rather than multi-consumer reuse; promotion to a wider `Shared` parent still requires another real consumer with identical meaning. Mirror this path in focused tests, and make namespaces match the physical folder without aliases or forwarding types that hide incorrect placement.
- Style-only work never changes behavior or evidence.
