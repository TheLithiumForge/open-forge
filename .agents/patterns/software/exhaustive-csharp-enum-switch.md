---
open-forge:
  description: Map every named C# enum member explicitly and reject unnamed runtime values
  tags: [Pattern, Software, CSharp, Enum, Switch, Exhaustiveness, Testing]
---

# Exhaustive C# Enum Switch

## Shape

Schematic mapping member. The containing type and enum declaration are omitted. Names, values and member order are illustrative. Every named mapping and the rejecting discard arm are required.

```csharp
internal static string ReadMachineName(OperationState state)
    => state switch
    {
        OperationState.Pending => "pending",
        OperationState.Running => "running",
        OperationState.Complete => "complete",
        _ => throw new ArgumentOutOfRangeException(
            nameof(state),
            state,
            "The operation state is not defined."),
    };
```

Schematic xUnit members, with containing test class, imports and production declarations omitted:

```csharp
[Theory]
[InlineData(OperationState.Pending, "pending")]
[InlineData(OperationState.Running, "running")]
[InlineData(OperationState.Complete, "complete")]
public void NamedStatesHaveTheirExactMachineNames(
    OperationState state,
    string expected)
{
    Assert.Equal(expected, OperationDefinitions.ReadMachineName(state));
}

[Fact]
public void UndefinedRuntimeStateIsRejected()
{
    var undefined = (OperationState)int.MaxValue;

    Assert.Throws<ArgumentOutOfRangeException>(
        () => OperationDefinitions.ReadMachineName(undefined));
}
```

| Part                                                             | Status                                            |
| ---------------------------------------------------------------- | ------------------------------------------------- |
| Explicit arm for every supported named member                    | Required                                          |
| Discard throwing `ArgumentOutOfRangeException`                   | Required for unnamed numeric values               |
| Semantic assertion for every named mapping and one unnamed value | Required                                          |
| Named-case coverage against `Enum.GetValues<TEnum>()`            | Required for a closed public or persisted mapping |
| Shared arm for several named members                             | Optional only for identical meaning               |

## Applicability And Sources

For finite enum-to-value or enum-to-behavior mappings. A C# enum includes unnamed numeric values, so the discard arm does not prove named-member completeness at compile time.

[C# Callable Design](../../directives/csharp/design.md#enum-mapping-evidence) binds mapping placement, completeness evidence and the native-language boundary.
