---
open-forge:
  description: Apply readable, formatted C# style to source and tests across the workspace
  tags: [LoadNow, Directive, CSharp, Style, Formatting, Readability, Testing]
---

# C# Style

## Instructions

- Honor `.editorconfig` and `dotnet format`. The accepted 200-character line ceiling remains a guideline. Prefer a shorter split when readability improves it.
- When one coherent string contains fixed text and values, prefer an interpolated template over concatenation, composite-format argument trains, or formatting each value separately. For deterministic culture-sensitive text outside a `StringBuilder`, use the exact provider with an interpolated-string-handler API such as `string.Create(CultureInfo.InvariantCulture, $"...")`. Keep escaping, encoding, and conditional inclusion explicit when they are real policy. Do not interpolate a static literal, replace genuinely character-by-character construction, or defeat a structured-logging API that intentionally owns named message-template placeholders.
- When a `StringBuilder` emits one coherent output line with fixed separators and several values, prefer one interpolated template in a single `Append` or `AppendLine` call over a chain of tiny appends. For deterministic culture-sensitive values, use the interpolated-string-handler overload with the exact provider and let it format the values directly instead of calling `ToString` on each value:

  ```csharp
  builder.AppendLine(
      CultureInfo.InvariantCulture,
      $"{layer.PathPosition} {source.Id ?? "none"} {layer.Path} {Layer(layer.Kind)}");
  ```

  Use `Append` instead when the line must not end yet. Precompute a value only when that clarifies real conditional policy; do not fragment a stable line merely to append each token separately.
- When a `StringBuilder` emits one coherent multi-line text block, prefer one raw interpolated string call over many per-line `AppendLine` calls or chains of tiny `.Append(...)` calls:

  ```csharp
  builder.AppendLine($"""
      Open Forge route list
      Workspace: {workspace}
      Result: {status}
      """);
  ```

  Use the same raw multi-line shape when only some lines interpolate values, and use a raw non-interpolated string when none do. Preserve deliberate blank lines, indentation, escaping, and final-newline behavior. Use `Append` instead of `AppendLine` when an extra newline would be unintended.

- Keep loops and conditionals structurally separate when rows or sections repeat or are conditionally omitted. Inside each iteration or selected branch, still emit a coherent fixed row or block with one interpolated template when practical. Build each coherent fixed multi-line block with a raw interpolated string rather than cluttered fragments.
- Group related C# attributes onto one physical line and normally one attribute list, such as `[Fact(...), Trait(...), Trait(...)]`, when the line remains readable and within the accepted limit. Split only when attribute arguments, generated or tool constraints, conditional compilation, or readability require it. Do not change semantic attribute order or meaning merely for style.
- Keep physical folders and namespaces cohesive. Do not let one feature folder become a flat catalogue of many distinct responsibilities. When several related types form a real cluster, place them in a named subfolder and matching namespace, such as `Rendering`, `Topology`, `Parsing`, or `Filesystem`. Keep behavior-owning operation and composition types at the feature root when that makes the entry path clear.
- Do not replace a crowded flat folder with one-file microfolders. Introduce a subfolder when it gives multiple related files one clear responsibility and improves navigation, not merely to reduce a file count.
- Treat records, interfaces, and classes that only carry properties or other state as models even when they are central command contracts. Place them in a `Models/` folder within their nearest owning feature or capability, and match the namespace to that path. Keep behavior-owning implementations, composition, and bindings outside `Models/`.
- When one `Models/` folder grows to roughly five to ten types, group the models further by cohesive topic, such as `Models/Identity/`, `Models/Measurements/`, or `Models/Presentation/`. Do not use one-file topic folders, generic buckets, or a repository-wide model namespace. When a Task moves or materially changes an existing nonconforming model, relocate that model and its consumers/tests in the same coherent phase rather than leaving a forwarding type.
- Within the replacement CLI, place supporting implementation below an explicit `Shared/<Capability>/` child of its narrowest owning CLI, command-family, or command-leaf boundary. Keep only the primary command or component contracts and composition entry at that boundary's root. A leaf-local `Shared` folder states ownership rather than multi-consumer reuse; promotion to a wider `Shared` parent still requires another real consumer with identical meaning. Mirror this path in focused tests, and make namespaces match the physical folder without aliases or forwarding types that hide incorrect placement.
- Style-only work never changes behavior or evidence.
