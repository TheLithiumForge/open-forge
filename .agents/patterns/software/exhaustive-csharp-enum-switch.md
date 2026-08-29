---
open-forge:
  description: Map every named C# enum member explicitly and reject unnamed runtime values
  tags: [Pattern, Software, CSharp, Enum, Switch, Exhaustiveness, Testing]
---

# Exhaustive C# Enum Switch

## Shape

Use one switch expression that names every supported enum member and rejects the
remaining runtime value space explicitly:

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

The discard arm is required because a C# enum can contain an unnamed value of
its underlying numeric type. Listing every currently named member does not close
that runtime value space and does not by itself provide compiler-enforced
exhaustiveness.

Keep the switch at the nearest scope that owns the mapping. Group multiple named
members in one arm only when they intentionally share exactly the same meaning.

## Evidence

Test the semantic result for every named member and the rejection of an unnamed
runtime value:

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

When the mapping is a closed public or persisted contract, add one focused
coverage assertion that compares the explicitly specified named cases with
`Enum.GetValues<TEnum>()`. Its purpose is to make a newly authored member require
an intentional mapping decision; do not use broad inventory or hash snapshots as
a substitute for semantic assertions.

## Boundaries

- Use this Pattern for finite enum-to-value or enum-to-behavior mappings.
- Do not claim that the compiler proves named-member completeness when the
  discard arm handles the open numeric value space.
- Do not suppress the compiler warning or introduce reflection-based dispatch,
  a source generator, analyzer, or discriminated-union framework solely to make
  this small boundary appear closed.
- Use a stronger closed type only when the domain independently needs one, not
  merely to replace this direct native C# shape.
